using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;


using LibLanScanner;
using LibInterface;
using LibArpPoisoning;



//this is just a simple form to test it

namespace SimpleGUI
{
    public partial class Form1 : Form
    {

        Device[] devices;
        LanScanner scanner;
        ArpPoisoning poisoning;


        public Form1()
        {
            InitializeComponent();


        }

        private void btn_devices_Click(object sender, EventArgs e)
        {

            progrBar_loading.Visible = true;

   
            devices = InterfaceInformation.GetDevices().ToArray();

            progrBar_loading.Visible = false;


            for (int i=0;i<devices.Length;i++)
            {
                ListViewItem item = new ListViewItem(
                    new string[] {
                        devices[i].DeviceName,
                        devices[i].ip.ToString(),
                        devices[i].MacAddress,
                        devices[i].gatewayAddress.ToString()
                    });
                list_devices.Items.Add(item);
            }
        }

        private void btn_scan_Click(object sender, EventArgs e)
        {
            if (list_devices.FocusedItem == null)
            {
                MessageBox.Show("Choice an interface");
                return;
            }




            if (scanner != null)
            {
                scanner.StopScanning();
                scanner = null;
                btn_scan.Text = "Start Scanning";
                progrBar_loading.Visible = false;
                return;
            }
                

            scanner = new LanScanner(devices[list_devices.FocusedItem.Index]);
            list_hosts.Items.Clear();
            btn_devices.Enabled = false;

            btn_scan.Text = "Stop Scanning";
            progrBar_loading.Visible = true;

            scanner.StartScanning();
            scanner.OnCaptureHost += Scanner_OnCaptureHost;



        }

        private void Scanner_OnCaptureHost(LanScanner.NetworkHost host)
        {
        
            Invoke(new Action(() =>
            {
                foreach (ListViewItem x in list_hosts.Items)
                {
                    if (x.SubItems[0].Text == host.IP && x.SubItems[1].Text == host.MAC)
                        return;
                }

                ListViewItem item = new ListViewItem(new string[] { host.IP, host.MAC });
                list_hosts.Items.Add(item);
            }));

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btn_block_Click(object sender, EventArgs e)
        {
            if(btn_block.Text == "Start Arp Poisoning")
            {
                if (list_hosts.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select the targets");
                    return;
                }


                poisoning = new ArpPoisoning(devices[list_devices.FocusedItem.Index]);

             
                for (int i = 0; i < list_hosts.Items.Count; i++)
                {
                    if (list_hosts.Items[i].Checked)
                    {
                        poisoning.AddTarget(
                           IPAddress.Parse(list_hosts.Items[i].SubItems[0].Text),
                            list_hosts.Items[i].SubItems[1].Text,
                            IPAddress.Parse(txt_gatewayip.Text),
                            txt_gatewaymac.Text);
                    }
                }
                poisoning.StartPoisoning();

                btn_block.Text = "Stop Arp Poisoning";
            }
            else
            {
                if (poisoning != null)
                {
                    poisoning.StopPoisoning();
                    poisoning.ClearTargets();
                }

                btn_block.Text = "Start Arp Poisoning";
            }



        }
    }
}
