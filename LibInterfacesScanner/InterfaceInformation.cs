using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets.Ethernet;

namespace LibInterface
{

    public class Device
    {
        public PacketDevice NetworkDevice { get; set; }
        public string DeviceName { get; set; }
        public string MacAddress { get; set; }
        public IPAddress ip { get; set; }
        public IPAddress subnetMask { get; set; }
        public IPAddress networkAddress { get; set; }
        public IPAddress gatewayAddress { get; set; }
        public IPAddress broadcasatAddress { get; set; }
    }

    public static class InterfaceInformation
    {
        public static List<Device> GetDevices()
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            List<Device> devices = new List<Device>();


            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                DeviceAddress ip = null;


                foreach (DeviceAddress dev in device.Addresses)
                {
                    if (dev.Address.Family == SocketAddressFamily.Internet)
                        ip = dev;
                }

                if (ip == null)
                    continue;


                    IPAddress address = IPAddress.Parse(((IpV4SocketAddress)ip.Address).Address.ToString()); 
                    IPAddress mask = GetSubnetMask(IPAddress.Parse(((IpV4SocketAddress)ip.Address).Address.ToString()));

                    if (mask == null)
                        continue;

                    IPAddress networkAddress = GetNetworkAddress(address, mask);
                    IPAddress gatewayAddress = GetDefaultGateway(address);
                    IPAddress broadcastAddress = GetBroadcastAddress(address, mask);
                    string DeviceName = DeviceDescription(address);

                    if (DeviceName == null)
                       continue;

                    string macaddress = GetMacAddress(DeviceName);

                                    
            


                    Device record = new Device
                    {
                        NetworkDevice = device,
                        DeviceName = DeviceDescription(address),
                        MacAddress = macaddress,
                        ip = address,
                        subnetMask = mask,
                        networkAddress = networkAddress,
                        gatewayAddress = gatewayAddress,
                        broadcasatAddress = broadcastAddress
                    };

                    devices.Add(record);

            }
            return devices;
        }
        public static string DeviceDescription(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return adapter.Description;
                        }
                    }
                }
            }
            return null;
        }
        public static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {

                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            return null;

        }
        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAddressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAddressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAddressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }
        public static IPAddress GetDefaultGateway(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            var gateways = adapter.GetIPProperties().GatewayAddresses;
                            if (!gateways.Any())
                                continue;

                            var gateway = gateways.FirstOrDefault(gate => gate.Address.AddressFamily == AddressFamily.InterNetwork);

                            if (gateway == null)
                                continue;

                            return gateway.Address;
                        }
                    }
                }
            }
            return null;
        }
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAddressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAddressBytes.Length];


            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAddressBytes[i] | (~subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }
        public static string GetMacAddress(string DeviceDescription)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (adapter.Description == DeviceDescription)
                        {
                            return adapter.GetPhysicalAddress().ToString()  ;
                        }
                    }
                }
            }
            return null;
        }
    }

}
