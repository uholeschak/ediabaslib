using System;
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
        private TcpClient _connection;
        private bool _connected;
        private Exception _exception;

        public TcpClientWithTimeout(IPAddress host, int port, int timeoutMilliseconds)
        {
            _host = host;
            _port = port;
            _timeoutMilliseconds = timeoutMilliseconds;
        }

        public TcpClient Connect()
        {
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
        }

        private void BeginConnect()
        {
            try
            {
                _connection = new TcpClient();
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

        public static void ExecuteNetworkCommand(ExecuteNetworkDelegate command, object connManager)
        {
#if Android
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
            {
                command();
                return;
            }
            Android.Net.Network bindNetwork = null;
            Android.Net.ConnectivityManager connectivityManager = connManager as Android.Net.ConnectivityManager;
            // ReSharper disable once UseNullPropagation
            if (connectivityManager != null)
            {
                Android.Net.Network[] networks = connectivityManager.GetAllNetworks();
                if (networks != null)
                {
                    foreach (Android.Net.Network network in networks)
                    {
                        Android.Net.NetworkInfo networkInfo = connectivityManager.GetNetworkInfo(network);
                        Android.Net.NetworkCapabilities networkCapabilities = connectivityManager.GetNetworkCapabilities(network);
                        // HasTransport support started also with Lollipop
                        if (networkInfo != null && networkInfo.IsConnected &&
                            networkCapabilities != null && networkCapabilities.HasTransport(Android.Net.TransportType.Wifi))
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

#if Android
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
    }
}
