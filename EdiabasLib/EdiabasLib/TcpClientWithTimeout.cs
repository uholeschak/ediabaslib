using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EdiabasLib
{
    /// <summary>
    /// TcpClientWithTimeout is used to open a TcpClient connection, with a
    /// user definable connection timeout in milliseconds (1000=1second)
    /// Use it like this:
    /// TcpClient connection = new TcpClientWithTimeout(host,80,1000).Connect();
    /// </summary>
    public class TcpClientWithTimeout
    {
        public delegate void ExecuteNetworkDelegate();

        private readonly IPAddress _host;
        private readonly int _port;
        private readonly int _timeoutMilliseconds;
        private readonly bool? _noDelay;
        private readonly int? _sendBufferSize;
        private TcpClient _connection;
#if WindowsCE
        private bool _connected;
        private Exception _exception;
#endif

#if __ANDROID__
        public class NetworkData
        {
            public NetworkData(Android.Net.ConnectivityManager connectivityManager)
            {
                ConnectivityManager = connectivityManager;
                LockObject = new object();
                ActiveCellularNetworks = new System.Collections.Generic.List<Android.Net.Network>();
                ActiveWifiNetworks = new System.Collections.Generic.List<Android.Net.Network>();
                ActiveEthernetNetworks = new System.Collections.Generic.List<Android.Net.Network>();
            }

            public Android.Net.ConnectivityManager ConnectivityManager { get; }
            public object LockObject { get; }
            public System.Collections.Generic.List<Android.Net.Network> ActiveCellularNetworks { get; }
            public System.Collections.Generic.List<Android.Net.Network> ActiveWifiNetworks { get; }
            public System.Collections.Generic.List<Android.Net.Network> ActiveEthernetNetworks { get; }
        }
#endif
        public TcpClientWithTimeout(IPAddress host, int port, int timeoutMilliseconds) :
            this(host, port, timeoutMilliseconds, null, null)
        {
        }

        public TcpClientWithTimeout(IPAddress host, int port, int timeoutMilliseconds, bool? noDelay) :
            this(host, port, timeoutMilliseconds, noDelay, null)
        {
        }

        public TcpClientWithTimeout(IPAddress host, int port, int timeoutMilliseconds, bool? noDelay, int? sendBufferSize)
        {
            _host = host;
            _port = port;
            _timeoutMilliseconds = timeoutMilliseconds;
            _noDelay = noDelay;
            _sendBufferSize = sendBufferSize;
        }

        public TcpClient Connect()
        {
            _connection = new TcpClient();
            if (_noDelay.HasValue)
            {
                _connection.NoDelay = _noDelay.Value;
            }
            if (_sendBufferSize.HasValue)
            {
                _connection.SendBufferSize = _sendBufferSize.Value;
            }

#if !WindowsCE
            System.Threading.Tasks.Task connectTask = _connection.ConnectAsync(_host, _port);
            if (!connectTask.Wait(_timeoutMilliseconds))
            {
                throw new TimeoutException("Connect timeout");
            }

            return _connection;
#else
            // kick off the thread that tries to connect
            _connected = false;
            _exception = null;
            Thread thread = new Thread(BeginConnect)
            {
                IsBackground = true
            };
            // So that a failed connection attempt
            // wont prevent the process from terminating while it does the long timeout
            thread.Start();

            // wait for either the timeout or the thread to finish
            thread.Join(_timeoutMilliseconds);

            if (_connected)
            {
                // it succeeded, so return the connection
                thread.Abort();
                return _connection;
            }
            if (_exception != null)
            {
                // it crashed, so return the exception to the caller
                thread.Abort();
                throw _exception;
            }
            else
            {
                // if it gets here, it timed out, so abort the thread and throw an exception
                thread.Abort();
                throw new TimeoutException("Connect timeout");
            }
#endif
        }

#if WindowsCE
        private void BeginConnect()
        {
            try
            {
                IPEndPoint ipTcp = new IPEndPoint(_host, _port);
                _connection.Connect(ipTcp);
                // record that it succeeded, for the main thread to return to the caller
                _connected = true;
            }
            catch (Exception ex)
            {
                // record the exception for the main thread to re-throw back to the calling code
                _exception = ex;
            }
        }
#endif

        public static void ExecuteNetworkCommand(ExecuteNetworkDelegate command, IPAddress ipAddr, object networkDataObject)
        {
#if __ANDROID__
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
            {
                command();
                return;
            }
            Android.Net.Network bindNetwork = null;
            NetworkData networkData = networkDataObject as NetworkData;
            Android.Net.ConnectivityManager connectivityManager = networkData?.ConnectivityManager;
            Java.Net.InetAddress inetAddr = Java.Net.InetAddress.GetByName(ipAddr.ToString());
            // ReSharper disable once UseNullPropagation
            if (connectivityManager != null && inetAddr is Java.Net.Inet4Address inet4Addr)
            {
                System.Collections.Generic.List<Android.Net.Network> networkList = new System.Collections.Generic.List<Android.Net.Network>();
                lock (networkData.LockObject)
                {
                    networkList.AddRange(networkData.ActiveWifiNetworks);
                    networkList.AddRange(networkData.ActiveEthernetNetworks);
                }

                foreach (Android.Net.Network network in networkList)
                {
                    Android.Net.NetworkCapabilities networkCapabilities = connectivityManager.GetNetworkCapabilities(network);
                    // HasTransport support started also with Lollipop
                    if (networkCapabilities != null)
                    {
                        bool linkValid = false;
                        Android.Net.LinkProperties linkProperties = connectivityManager.GetLinkProperties(network);
                        if (linkProperties != null)
                        {
                            foreach (Android.Net.LinkAddress linkAddress in linkProperties.LinkAddresses)
                            {
                                if (linkAddress.Address is Java.Net.Inet4Address linkInet4Address)
                                {
                                    Java.Net.NetworkInterface networkInterface = Java.Net.NetworkInterface.GetByInetAddress(linkInet4Address);
                                    if (networkInterface != null && networkInterface.IsUp &&
                                        (linkInet4Address.IsSiteLocalAddress || linkInet4Address.IsLinkLocalAddress))
                                    {
                                        foreach (Java.Net.InterfaceAddress interfaceAddress in networkInterface.InterfaceAddresses)
                                        {
                                            if (interfaceAddress.Address is Java.Net.Inet4Address)
                                            {
                                                byte[] linkAddrBytes = interfaceAddress.Address.GetAddress();
                                                byte[] inet4AddrBytes = inet4Addr.GetAddress();
                                                if (linkAddrBytes.Length == inet4AddrBytes.Length)
                                                {
                                                    for (int bit = interfaceAddress.NetworkPrefixLength; bit < linkAddrBytes.Length * 8; bit++)
                                                    {
                                                        int index = bit >> 3;
                                                        byte mask = (byte)(0x80 >> (bit & 0x07));
                                                        linkAddrBytes[index] |= mask;
                                                        inet4AddrBytes[index] |= mask;
                                                    }
                                                }

                                                if (linkAddrBytes.SequenceEqual(inet4AddrBytes))
                                                {
                                                    linkValid = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (linkValid)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (linkValid)
                        {
                            bindNetwork = network;
                            break;
                        }
                    }
                }
            }

            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
            {
#pragma warning disable 618
                Android.Net.Network defaultNetwork = Android.Net.ConnectivityManager.ProcessDefaultNetwork;
                try
                {
                    Android.Net.ConnectivityManager.SetProcessDefaultNetwork(bindNetwork);
                    command();
                }
                finally
                {
                    Android.Net.ConnectivityManager.SetProcessDefaultNetwork(defaultNetwork);
                }
#pragma warning restore 618
                return;
            }
            Android.Net.Network boundNetwork = connectivityManager?.BoundNetworkForProcess;
            try
            {
                connectivityManager?.BindProcessToNetwork(bindNetwork);
                command();
            }
            finally
            {
                connectivityManager?.BindProcessToNetwork(boundNetwork);
            }
#else
            command();
#endif
        }

#if __ANDROID__
        public static string ConvertIpAddress(int ipAddress)
        {
            if (Java.Nio.ByteOrder.NativeOrder().Equals(Java.Nio.ByteOrder.LittleEndian))
            {
                ipAddress = Java.Lang.Integer.ReverseBytes(ipAddress);
            }
            byte[] ipByteArray = Java.Math.BigInteger.ValueOf(ipAddress).ToByteArray();
            try
            {
                Java.Net.InetAddress inetAddress = Java.Net.InetAddress.GetByAddress(ipByteArray);
                return ConvertIpAddress(inetAddress);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ConvertIpAddress(Java.Net.InetAddress inetAddress)
        {
            if (inetAddress == null)
            {
                return null;
            }

            try
            {
                string address = inetAddress.HostAddress;
                if (address == null)
                {
                    string text = inetAddress.ToString();
                    System.Text.RegularExpressions.Match match =
                        System.Text.RegularExpressions.Regex.Match(text, @"\d+\.\d+\.\d+\.\d+$", System.Text.RegularExpressions.RegexOptions.Singleline);
                    if (match.Success)
                    {
                        address = match.Value;
                    }
                }
                return address;
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif

        public static IPAddress PrefixLenToMask(int prefixLen)
        {
            try
            {
                int shift = 32 - prefixLen;
                long mask = 0;
                if (shift >= 0 && shift < 32)
                {
                    mask = (0xFFFFFFFF << shift) >> shift;
                }

                return new IPAddress(mask);
            }
            catch (Exception)
            {
                return IPAddress.Any;
            }
        }
    }
}
