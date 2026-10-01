using System.Net;
using System.Net.Sockets;

namespace BMW.Rheingold.xVM
{
    public class SocketState
    {
        private readonly byte[] buffer;

        private readonly Socket workSocket;

        private EndPoint endPoint;

        private readonly string name;

        public EndPoint EndPoint
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

        public Socket WorkSocket => workSocket;

        public string Name => name;

        public byte[] Buffer => buffer;

        public SocketState(Socket workSocket, byte[] buffer, string name)
        {
            this.workSocket = workSocket;
            this.buffer = buffer;
            this.name = name;
        }
    }

}
