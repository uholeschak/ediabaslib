using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EdiabasLib
{
    public class EdElmWifiInterface
    {
        public const string PortId = "ELM327WIFI";
        public static string ElmIp = "192.168.0.10";
        public static int ElmPort = 35000;
        protected static TcpClient TcpElmClient;
        protected static NetworkStream TcpElmStream;
        protected static int ConnectTimeout = 5000;
        protected static string ConnectPort;
        private static EdElmInterface _edElmInterface;

        static EdElmWifiInterface()
        {
        }

        public static EdiabasNet Ediabas { get; set; }

        public static bool InterfaceConnect(string port, object parameter)
        {
            if (TcpElmClient != null)
            {
                return true;
            }
            try
            {
                ConnectPort = port;
                TcpElmClient = new TcpClientWithTimeout(IPAddress.Parse(ElmIp), ElmPort, ConnectTimeout).Connect();
                TcpElmStream = TcpElmClient.GetStream();
                _edElmInterface = new EdElmInterface(Ediabas, TcpElmStream, TcpElmStream);
                if (!_edElmInterface.Elm327Init())
                {
                    InterfaceDisconnect();
                    return false;
                }
            }
            catch (Exception)
            {
                InterfaceDisconnect();
                return false;
            }
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            bool result = true;
            if (_edElmInterface != null)
            {
                _edElmInterface.Dispose();
                _edElmInterface = null;
            }

            try
            {
                if (TcpElmStream != null)
                {
                    TcpElmStream.Close();
                    TcpElmStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (TcpElmClient != null)
                {
                    TcpElmClient.Close();
                    TcpElmClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (TcpElmStream == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            if ((baudRate != 115200) || (dataBits != 8) || (parity != EdInterfaceObd.SerialParity.None))
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            return EdInterfaceObd.InterfaceErrorResult.NoError;
        }

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (TcpElmStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetRts(bool rts)
        {
            if (TcpElmStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = true;
            if (TcpElmStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            return false;
        }

        public static bool InterfaceSetInterByteTime(int time)
        {
            return true;
        }

        public static bool InterfacePurgeInBuffer()
        {
            if (TcpElmStream == null)
            {
                return false;
            }
            if (_edElmInterface == null)
            {
                return false;
            }
            return _edElmInterface.InterfacePurgeInBuffer();
        }

        public static bool InterfaceAdapterEcho()
        {
            return false;
        }

        public static bool InterfaceHasPreciseTimeout()
        {
            return false;
        }

        public static bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            if (TcpElmStream == null)
            {
                return false;
            }
            if (_edElmInterface == null)
            {
                return false;
            }
            if (_edElmInterface.StreamFailure)
            {
                Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect();
                if (!InterfaceConnect(ConnectPort, null))
                {
                    _edElmInterface.StreamFailure = true;
                    return false;
                }
            }
            if (!_edElmInterface.InterfaceSendData(sendData, length, setDtr, dtrTimeCorr))
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            if (TcpElmStream == null)
            {
                return false;
            }
            if (_edElmInterface == null)
            {
                return false;
            }
            return _edElmInterface.InterfaceReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, ediabasLog);
        }

        public static bool InterfaceSendPulse(UInt64 dataBits, int length, int pulseWidth, bool setDtr)
        {
            return false;
        }

        /// <summary>
        /// TcpClientWithTimeout is used to open a TcpClient connection, with a
        /// user definable connection timeout in milliseconds (1000=1second)
        /// Use it like this:
        /// TcpClient connection = new TcpClientWithTimeout(host,80,1000).Connect();
        /// </summary>
        private class TcpClientWithTimeout
        {
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
        }
    }
}
