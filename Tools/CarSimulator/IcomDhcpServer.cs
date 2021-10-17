using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Singulink.Net.Dhcp;

namespace CarSimulator
{
    public class IcomDhcpServer : DhcpServer
    {
        // Your implementation of mappings between IP addresses and MAC addresses - could be
        // an in-memory dictionary, database lookup, or some other custom mechanism of 
        // mapping values:
        private readonly Dictionary<PhysicalAddress, IPAddress> _clientMap;
        private readonly List<IPAddress> _availableIpAddresses;

        private readonly object _syncRoot = new object();

        public event Action<PhysicalAddress> DiscoverReceived = delegate { };
        public event Action Disconnected = delegate { };

        public IcomDhcpServer(IPAddress listeningAddress, IPAddress subnetMask) : base(listeningAddress, subnetMask)
        {
            _clientMap = new Dictionary<PhysicalAddress, IPAddress>();
            _availableIpAddresses = new List<IPAddress>();
            for (int i = 2; i < 200; i++)
            {
                IPAddress ipAddress = IPAddress.Parse(string.Format(CultureInfo.InvariantCulture, "192.168.11.{0}", i));
                _availableIpAddresses.Add(ipAddress);
            }
        }

        public override void Start()
        {
            Debug.WriteLine("Starting server...");
            base.Start();
            Debug.WriteLine("Started.");
        }

        public override void Stop()
        {
            Debug.WriteLine("Stopping server...");
            base.Stop();
            Debug.WriteLine("Stopped.");
        }

        public PhysicalAddress GetMacAddress(IPAddress ip)
        {
            lock (_syncRoot)
            {
                foreach (KeyValuePair<PhysicalAddress, IPAddress> pair in _clientMap)
                {
                    if (pair.Value.Equals(ip))
                    {
                        return pair.Key;
                    }
                }
            }

            return null;
        }

        protected override DhcpDiscoverResult OnDiscoverReceived(DhcpMessage message)
        {
            Debug.WriteLine(message.ToString());

            IPAddress ip;
            PhysicalAddress mac = message.ClientMacAddress;

            lock (_syncRoot)
            {
                if (!_clientMap.TryGetValue(mac, out ip))
                {
                    ip = GetNextAvailableIPAddress();
                    if (ip == null)
                    {
                        Debug.WriteLine("No more IP addresses available.");
                        return null;
                    }

                    _clientMap[mac] = ip;
                }
            }

            DiscoverReceived.Invoke(mac);
            return DhcpDiscoverResult.CreateOffer(message, ip, uint.MaxValue);
        }

        protected override DhcpRequestResult OnRequestReceived(DhcpMessage message)
        {
            Debug.WriteLine(message);

            var ip = message.Options.RequestedIPAddress;
            var mac = message.ClientMacAddress;

            if (ip == null)
                return DhcpRequestResult.CreateNoAcknowledgement(message, "No requested IP address provided");

            bool found = false;
            lock (_syncRoot)
            {
                if (_clientMap.TryGetValue(mac, out IPAddress ipAddress))
                {
                    if (ip.Equals(ipAddress))
                    {
                        found = true;
                    }
                }
            }

            if (!found)
            {
                return DhcpRequestResult.CreateNoAcknowledgement(message, "No matching offer found.");
            }

            return DhcpRequestResult.CreateAcknowledgement(message, ip, uint.MaxValue);
        }

        protected override void OnReleaseReceived(DhcpMessage message)
        {
            Debug.WriteLine(message);
        }

        protected override void OnDeclineReceived(DhcpMessage message)
        {
            Debug.WriteLine(message);

            var ip = message.Options.RequestedIPAddress;
            var mac = message.ClientMacAddress;

            if (ip != null)
            {
                Debug.WriteLine("Purge requested for client record: {0} / {1}.", mac, ip);

                lock (_syncRoot)
                {
                    if (_clientMap.TryGetValue(mac, out IPAddress ipAddress))
                    {
                        if (ip.Equals(ipAddress))
                        {
                            _clientMap.Remove(mac);
                            Debug.WriteLine("Purged client record: {0} / {1}.", mac, ip);
                        }
                    }
                }
            }
        }

        protected override void OnInformReceived(DhcpMessage message)
        {
            Debug.WriteLine(message);
        }

        protected override void OnResponseSent(DhcpMessage message)
        {
            Debug.WriteLine(message);
        }

        protected override void OnMessageError(Exception ex)
        {
            Debug.WriteLine("Bad message received.", ex.Message);
        }

        protected override void OnSocketError(SocketException ex)
        {
            Debug.WriteLine("Socket error {0}", ex.Message);
            Disconnected.Invoke();
        }

        private IPAddress GetNextAvailableIPAddress()
        {
            foreach (IPAddress ipAddress in _availableIpAddresses)
            {
                bool found = false;
                foreach (KeyValuePair<PhysicalAddress, IPAddress> pair in _clientMap)
                {
                    if (pair.Value.Equals(ipAddress))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return ipAddress;
                }
            }

            return null;
        }
    }
}
