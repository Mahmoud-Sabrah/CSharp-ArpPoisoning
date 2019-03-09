using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibArpPoisoning
{
    public class ArpRecord 
    {

        public IPAddress SenderIP { get; set; }


        public string SenderMAC { get; set; }


        public IPAddress TargetIP { get; set; }


        public string TargetMAC { get; set; }


        public IPAddress GatewayIP { get; set; }


        public string GatewayMAC { get; set; }

 


        //Packets to send
        internal List<Packet> pcks = new List<Packet>();

        public ArpRecord(IPAddress SenderIP, string SenderMAC, IPAddress TargetIP, string TargetMAC, IPAddress RouterIP, string RouterMAC)
        {
            this.SenderIP = SenderIP;
            this.SenderMAC = SenderMAC;
            this.TargetIP = TargetIP;
            this.TargetMAC = TargetMAC;
            this.GatewayIP = RouterIP;
            this.GatewayMAC = RouterMAC;
        }

  
    }
}
