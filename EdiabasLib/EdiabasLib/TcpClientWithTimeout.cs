﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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
        public delegate bool ConnectAbortDelegate();

        private readonly IPAddress _host;
        private readonly int _port;
        private readonly int _timeoutMilliseconds;
        private readonly bool? _noDelay;
        private readonly int? _sendBufferSize;
        private TcpClient _connection;
        public static readonly long TickResolMs = System.Diagnostics.Stopwatch.Frequency / 1000;

#if ANDROID
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

        public TcpClient Connect(System.Threading.ManualResetEvent resetEvent = null)
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

            using (System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource())
            {
                System.Threading.AutoResetEvent threadFinishEvent = null;
                System.Threading.Thread abortThread = null;
                if (resetEvent != null)
                {
                    threadFinishEvent = new System.Threading.AutoResetEvent(false);
                    System.Threading.WaitHandle[] waitHandles = { threadFinishEvent, resetEvent };
                    abortThread = new System.Threading.Thread(() =>
                    {
                        if (System.Threading.WaitHandle.WaitAny(waitHandles, _timeoutMilliseconds) == 1)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            cts.Cancel();
                        }
                    });
                    abortThread.Start();
                }

                try
                {
                    System.Threading.Tasks.Task connectTask = _connection.ConnectAsync(_host, _port);
                    try
                    {
                        if (!connectTask.Wait(_timeoutMilliseconds, cts.Token))
                        {
                            throw new TimeoutException("Connect timeout");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw new TimeoutException("Cancelled");
                    }
                }
                finally
                {
                    if (abortThread != null)
                    {
                        threadFinishEvent.Set();
                        abortThread.Join();
                    }

                    threadFinishEvent?.Dispose();
                }
            }

            return _connection;
        }

        public static void ExecuteNetworkCommand(ExecuteNetworkDelegate command, IPAddress ipAddr, object networkDataObject)
        {
#if ANDROID
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
                                        System.Collections.Generic.IList<Java.Net.InterfaceAddress> interfaceAdresses = networkInterface.InterfaceAddresses;
                                        if (interfaceAdresses == null)
                                        {
                                            continue;
                                        }

                                        foreach (Java.Net.InterfaceAddress interfaceAddress in interfaceAdresses)
                                        {
                                            if (interfaceAddress.Address is Java.Net.Inet4Address interface4Addr)
                                            {
                                                if (IsIpMatchingSubnet(inet4Addr, interface4Addr, interfaceAddress.NetworkPrefixLength))
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
#pragma warning disable CA1422
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
#pragma warning restore CA1422
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

#if ANDROID
        public static string ConvertIpAddress(int ipAddress)
        {
            int ipValue = ipAddress;
            Java.Nio.ByteOrder nativeOrder = Java.Nio.ByteOrder.NativeOrder();
            if (nativeOrder != null && nativeOrder.Equals(Java.Nio.ByteOrder.LittleEndian))
            {
                ipValue = Java.Lang.Integer.ReverseBytes(ipValue);
            }

            byte[] ipByteArray = Java.Math.BigInteger.ValueOf(ipValue).ToByteArray();
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

        public static bool IsIpMatchingSubnet(Java.Net.Inet4Address ipAddr, Java.Net.Inet4Address networkAddr, int prefixLen)
        {
            try
            {
                byte[] ipAddrBytes = ipAddr.GetAddress();
                byte[] networkBytes = networkAddr.GetAddress();

                if (ipAddrBytes != null && networkBytes != null)
                {
                    if (ipAddrBytes.Length == networkBytes.Length)
                    {
                        for (int bit = prefixLen; bit < networkBytes.Length * 8; bit++)
                        {
                            int index = bit >> 3;
                            byte mask = (byte)(0x80 >> (bit & 0x07));
                            ipAddrBytes[index] |= mask;
                            networkBytes[index] |= mask;
                        }
                    }

                    if (ipAddrBytes.SequenceEqual(networkBytes))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
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
