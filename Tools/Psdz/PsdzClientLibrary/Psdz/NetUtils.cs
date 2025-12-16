using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BMW.Rheingold.Psdz
{
    public static class NetUtils
    {
        public static int GetFirstFreePort(int minPort, int maxPort, int? fallbackPort = null)
        {
            Log.Info(Log.CurrentMethod(), "called. Params: [minPort: {0}, maxPort: {1}, fallbackPort: {2}]", minPort, maxPort, fallbackPort);
            if (minPort > maxPort)
            {
                throw new ArgumentException("Param 'minPort' must be less or equal than 'maxPort'!");
            }
            if (fallbackPort.HasValue && fallbackPort <= 0)
            {
                throw new ArgumentException("Param 'fallbackPort' must be positive.");
            }
            IPEndPoint[] activeTcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            List<int> list = Enumerable.Range(minPort, maxPort - minPort + 1).ToList();
            if (fallbackPort.HasValue)
            {
                list.Add(fallbackPort.GetValueOrDefault());
            }
            foreach (int currPort in list)
            {
                if (!activeTcpListeners.Any((IPEndPoint e) => e.Port == currPort))
                {
                    Log.Info(Log.CurrentMethod(), "First free and available port found: {0}", currPort);
                    return currPort;
                }
            }
            Log.Warning(Log.CurrentMethod(), "No free port available!");
            return -1;
        }

        public static IEnumerable<IPAddress> Convert(IEnumerable<string> ipAddresses)
        {
            List<IPAddress> list = new List<IPAddress>();
            if (ipAddresses == null)
            {
                return list;
            }
            foreach (string ipAddress in ipAddresses)
            {
                IPAddress item = Convert(ipAddress);
                if (ipAddress != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static IPAddress Convert(string ipAddress)
        {
            try
            {
                return IPAddress.Parse(ipAddress.Trim());
            }
            catch (Exception)
            {
                Log.Error("NetUtils.Convert()", "Ip address '{0}' could not be parsed.", ipAddress);
            }
            return null;
        }

        public static string GetWlanSignalStrength()
        {
            try
            {
                string connectedSignalStrength = NativeWifi.GetConnectedSignalStrength();
                return (connectedSignalStrength == null) ? "No WLAN connection" : connectedSignalStrength;
            }
            catch (Exception)
            {
                Log.Error("NetUtils.GetWlanSignalStrength()", "WLAN signal strength could not be read");
            }
            return "WLAN signal strength could not be read";
        }

        private static IEnumerable<IPAddress> GetAddressesByName(string hostnameOrAddress)
        {
            IPAddress[] addressList = Dns.GetHostEntry(hostnameOrAddress).AddressList;
            IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            ISet<IPAddress> set = new HashSet<IPAddress>(addressList);
            HashSet<IPAddress> hashSet = new HashSet<IPAddress>(addressList);
            ((ISet<IPAddress>)hashSet).IntersectWith((IEnumerable<IPAddress>)hostAddresses);
            if (((ICollection<IPAddress>)hashSet).Count > 0)
            {
                set.Add(IPAddress.Loopback);
                set.Add(IPAddress.IPv6Loopback);
            }
            return set;
        }

        private static bool TryGetLocalIpAddress(out IPAddress address)
        {
            address = null;
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP))
                {
                    socket.Connect("1.1.1.1", 65530);
                    if (socket.LocalEndPoint is IPEndPoint iPEndPoint)
                    {
                        address = iPEndPoint.Address;
                        return true;
                    }
                }
            }
            catch (SocketException)
            {
                address = null;
            }
            return false;
        }

        private static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] addressBytes = address.GetAddressBytes();
            byte[] addressBytes2 = subnetMask.GetAddressBytes();
            if (addressBytes.Length != addressBytes2.Length)
            {
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
            }
            byte[] array = new byte[addressBytes.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (byte)(addressBytes[i] & addressBytes2[i]);
            }
            return new IPAddress(array);
        }

        private static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress networkAddress = address.GetNetworkAddress(subnetMask);
            IPAddress networkAddress2 = address2.GetNetworkAddress(subnetMask);
            return networkAddress.Equals(networkAddress2);
        }

        public static bool IsInSameSubnet(string theAddress, string networkAddress, string subnetMask)
        {
            try
            {
                IPAddress address = IPAddress.Parse(theAddress);
                IPAddress address2 = IPAddress.Parse(networkAddress);
                IPAddress subnetMask2 = IPAddress.Parse(subnetMask);
                return address.IsInSameSubnet(address2, subnetMask2);
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
                return false;
            }
        }

        public static void RenewIpConfig()
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "ipconfig",
                Arguments = "/renew"
            };
            process.Start();
        }
    }
}