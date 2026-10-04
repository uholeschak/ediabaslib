using PsdzClient.Core;
using System.Net;
using System.Net.Sockets;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.xVM
{
    public class UdpState
    {
        private VCIDevice dev;

        private UdpClient udpClient;

        private IPEndPoint endPoint;

        public VCIDevice Device
        {
            get
            {
                return dev;
            }
            set
            {
                dev = value;
            }
        }

        public IPEndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                endPoint = value;
            }
        }

        public UdpClient UdpClient
        {
            get
            {
                return udpClient;
            }
            set
            {
                udpClient = value;
            }
        }

        public UdpState()
            : this(null)
        {
        }

        public UdpState(UdpClient client)
        {
            udpClient = client;
        }

        public UdpState(UdpClient client, IPEndPoint endPoint)
            : this(client)
        {
            this.endPoint = endPoint;
        }
    }
}
