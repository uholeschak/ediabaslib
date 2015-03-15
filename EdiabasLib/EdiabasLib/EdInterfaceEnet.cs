using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace EdiabasLib
{
    public class EdInterfaceEnet : EdInterfaceBase
    {
        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);

        private bool disposed = false;
        protected const int transBufferSize = 512; // transmit buffer size
        protected const string autoIp = "auto";
        protected static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly byte[] byteArray0 = new byte[0];
        protected static readonly byte[] udpIdentReq = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x11};
        protected static readonly long tickResolMs = Stopwatch.Frequency / 1000;

        private static TcpClient tcpClient = null;
        private static NetworkStream tcpStream = null;
        private UdpClient udpClient = null;
        private volatile IPEndPoint udpRecEndPoint = null;
        private AutoResetEvent udpEvent = new AutoResetEvent(false);

        protected string remoteHost = autoIp;
        protected int testerAddress = 0xF4;
        protected int controlPort = 6811;
        protected int diagnosticPort = 6801;

        protected byte[] recBuffer = new byte[transBufferSize];
        protected byte[] dataBuffer = new byte[transBufferSize];
        protected byte[] ackBuffer = new byte[transBufferSize];

        protected TransmitDelegate parTransmitFunc;
        protected int parTimeoutStd = 0;
        protected int parTimeoutTelEnd = 0;
        protected int parInterbyteTime = 0;
        protected int parRegenTime = 0;
        protected int parTimeoutNR = 0;
        protected int parRetryNR = 0;

        public override EdiabasNet Ediabas
        {
            get
            {
                return base.Ediabas;
            }
            set
            {
                base.Ediabas = value;

                string prop;
                prop = ediabas.GetConfigProperty("EnetRemoteHost");
                if (prop != null)
                {
                    this.remoteHost = prop;
                }

                prop = ediabas.GetConfigProperty("EnetTesterAddress");
                if (prop != null)
                {
                    this.testerAddress = (int)EdiabasNet.StringToValue(prop);
                }

                prop = ediabas.GetConfigProperty("EnetControlPort");
                if (prop != null)
                {
                    this.controlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = ediabas.GetConfigProperty("EnetDiagnosticPort");
                if (prop != null)
                {
                    this.diagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

            }
        }

        public override UInt32[] CommParameter
        {
            get
            {
                return base.CommParameter;
            }
            set
            {
                commParameter = value;
                commAnswerLen[0] = 0;
                commAnswerLen[1] = 0;

                this.parTransmitFunc = null;
                this.parTimeoutStd = 0;
                this.parTimeoutTelEnd = 0;
                this.parInterbyteTime = 0;
                this.parRegenTime = 0;
                this.parTimeoutNR = 0;
                this.parRetryNR = 0;

                if (commParameter == null)
                {   // clear parameter
                    return;
                }
                if (commParameter.Length < 1)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    return;
                }

                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, commParameter, 0, commParameter.Length,
                    string.Format(culture, "{0} CommParameter Host={1}, Tester={2}, ControlPort={3}, DiagPort={4}",
                            InterfaceName, this.remoteHost, this.testerAddress, this.controlPort, this.diagnosticPort));

                uint concept = commParameter[0];
                switch (concept)
                {
                    case 0x010F:    // BMW-FAST
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        this.parTransmitFunc = TransBmwFast;
                        this.parTimeoutStd = (int)commParameter[2];
                        this.parRegenTime = (int)commParameter[3];
                        this.parTimeoutTelEnd = (int)commParameter[4];
                        this.parTimeoutNR = (int)commParameter[6];
                        this.parRetryNR = (int)commParameter[5];
                        break;

                    case 0x0110:    // D-CAN
                        if (commParameter.Length < 30)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        this.parTransmitFunc = TransBmwFast;
                        this.parTimeoutStd = (int)commParameter[7];
                        this.parTimeoutTelEnd = 10;
                        this.parRegenTime = (int)commParameter[8];
                        this.parTimeoutNR = (int)commParameter[9];
                        this.parRetryNR = (int)commParameter[10];
                        break;

                    default:
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
                        return;
                }
            }
        }

        public override string InterfaceType
        {
            get
            {
                return "ENET";
            }
        }

        public override UInt32 InterfaceVersion
        {
            get
            {
                return 1795;
            }
        }

        public override string InterfaceName
        {
            get
            {
                return "ENET";
            }
        }

        public override byte[] KeyBytes
        {
            get
            {
                return byteArray0;
            }
        }

        public override byte[] State
        {
            get
            {
                return byteArray0;
            }
        }

        public override Int64 BatteryVoltage
        {
            get
            {
                if (!Connected)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                return 12000;
            }
        }

        public override Int64 IgnitionVoltage
        {
            get
            {
                if (!Connected)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                return 12000;
            }
        }

        public override bool Connected
        {
            get
            {
                return (tcpClient != null) && (tcpStream != null);
            }
        }

        static EdInterfaceEnet()
        {
#if WindowsCE
            interfaceMutex = new Mutex(false);
#else
            interfaceMutex = new Mutex(false, "EdiabasLib_InterfaceEnet");
#endif
        }

        public EdInterfaceEnet()
        {
        }

        ~EdInterfaceEnet()
        {
            Dispose(false);
        }

        public override bool IsValidInterfaceName(string name)
        {
            if (string.Compare(name, "ENET", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        public override bool InterfaceConnect()
        {
            if (tcpClient != null)
            {
                return true;
            }
            try
            {
                IPAddress hostIp = null;
                if (remoteHost.StartsWith(autoIp, StringComparison.OrdinalIgnoreCase))
                {
                    IPEndPoint ipUdp = new IPEndPoint(IPAddress.Any, 0);
                    udpClient = new UdpClient(ipUdp);
                    udpRecEndPoint = null;
                    StartUdpListen();

                    bool broadcastSend = false;
                    string adapterName = string.Empty;
                    string configData = remoteHost.Remove(0, autoIp.Length);
                    if ((configData.Length > 0) && (configData[0] == ':'))
                    {
                        if (configData.StartsWith(":all", StringComparison.OrdinalIgnoreCase))
                        {   // search all adapters
                            adapterName = string.Empty;
                        }
                        else
                        {
                            adapterName = configData.Remove(0, 1);
                        }

                        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                        foreach (NetworkInterface adapter in adapters)
                        {
                            if (adapter.OperationalStatus == OperationalStatus.Up)
                            {
                                IPInterfaceProperties properties = adapter.GetIPProperties();
                                if (properties.UnicastAddresses != null)
                                {
                                    foreach (UnicastIPAddressInformation ipAddressInfo in properties.UnicastAddresses)
                                    {
                                        if (ipAddressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                                        {
                                            if ((adapterName.Length == 0) || (adapter.Name.StartsWith(adapterName, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                try
                                                {
                                                    byte[] ipBytes = ipAddressInfo.Address.GetAddressBytes();
                                                    byte[] maskBytes = ipAddressInfo.IPv4Mask.GetAddressBytes();
                                                    for (int i = 0; i < ipBytes.Length; i++)
                                                    {
                                                        ipBytes[i] |= (byte)(~maskBytes[i]);
                                                    }
                                                    IPAddress broadcastAddress = new IPAddress(ipBytes);
                                                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, string.Format("Sending: '{0}': Ip={1} Mask={2} Broadcast={3}",
                                                        adapter.Name, ipAddressInfo.Address.ToString(), ipAddressInfo.IPv4Mask.ToString(), broadcastAddress.ToString()));
                                                    IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, controlPort);
                                                    udpClient.Send(udpIdentReq, udpIdentReq.Length, ipUdpIdent);
                                                    broadcastSend = true;
                                                }
                                                catch (Exception)
                                                {
                                                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Broadcast failed");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Parse("169.254.255.255"), controlPort);
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, string.Format("Sending to: {0}", ipUdpIdent.Address.ToString()));
                            udpClient.Send(udpIdentReq, udpIdentReq.Length, ipUdpIdent);
                            broadcastSend = true;
                        }
                        catch (Exception)
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Broadcast failed");
                        }
                    }
                    if (!broadcastSend)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "No broadcast send");
                        InterfaceDisconnect();
                        return false;
                    }

                    if (!udpEvent.WaitOne(1000) || (udpRecEndPoint == null))
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "No answer received");
                        InterfaceDisconnect();
                        return false;
                    }
                    hostIp = udpRecEndPoint.Address;
                    udpClient.Close();
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, string.Format("Received: IP={0}", hostIp.ToString()));
                }
                else
                {
                    hostIp = IPAddress.Parse(this.remoteHost);
                }

                tcpClient = new TcpClient();
                IPEndPoint ipTcp = new IPEndPoint(hostIp, this.diagnosticPort);
                tcpClient.Connect(ipTcp);
                tcpStream = tcpClient.GetStream();
            }
            catch (Exception)
            {
                InterfaceDisconnect();
                return false;
            }
            return true;
        }

        public override bool InterfaceDisconnect()
        {
            bool result = true;

            try
            {
                if (tcpStream != null)
                {
                    tcpStream.Close();
                    tcpStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (udpClient != null)
                {
                    udpClient.Close();
                    udpClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            udpRecEndPoint = null;
            return result;
        }

        public override bool InterfaceReset()
        {
            CommParameter = null;
            return true;
        }

        public override bool TransmitData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = null;
            if (commParameter == null)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            int recLength;
            EdiabasNet.ErrorCodes errorCode = OBDTrans(sendData, sendData.Length, ref this.recBuffer, out recLength);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                ediabas.SetError(errorCode);
                return false;
            }
            receiveData = new byte[recLength];
            Array.Copy(this.recBuffer, receiveData, recLength);
            return true;
        }

        public override bool ReceiveFrequent(out byte[] receiveData)
        {
            receiveData = null;
            if (commParameter == null)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            receiveData = byteArray0;
            return true;
        }

        public override bool StopFrequent()
        {
            return true;
        }

        public string RemoteHost
        {
            get
            {
                return remoteHost;
            }
            set
            {
                remoteHost = value;
            }
        }

        protected void StartUdpListen()
        {
            if (udpClient == null)
            {
                return;
            }
            udpClient.BeginReceive(UdpReceiver, new Object());
        }

        protected void UdpReceiver(IAsyncResult ar)
        {
            try
            {
                UdpClient udpClientLocal = udpClient;
                if (udpClientLocal == null)
                {
                    return;
                }
                IPEndPoint ipUdp = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udpClientLocal.EndReceive(ar, ref ipUdp);
                if ((udpRecEndPoint == null) &&
                    (bytes != null) && (bytes.Length == (6 + 50)) &&
                    (bytes[0] == 0x00) &&
                    (bytes[1] == 0x00) &&
                    (bytes[2] == 0x00) &&
                    (bytes[3] == 50) &&
                    (bytes[4] == 0x00) &&
                    (bytes[5] == 0x04) &&   // anouncement
                    (bytes[6] == 'D') &&
                    (bytes[7] == 'I') &&
                    (bytes[8] == 'A') &&
                    (bytes[9] == 'G') &&
                    (bytes[10] == 'A') &&
                    (bytes[11] == 'D') &&
                    (bytes[12] == 'R') &&
                    (bytes[13] == '1') &&
                    (bytes[14] == '0'))
                {
                    udpRecEndPoint = ipUdp;
                    udpEvent.Set();
                }
                else
                {
                    StartUdpListen();
                }
            }
            catch (Exception)
            {
            }
        }

        protected bool SendData(byte[] sendData, int length, bool enableLogging)
        {
            if (tcpStream == null)
            {
                return false;
            }
            try
            {
                tcpStream.Flush();
                while (tcpStream.DataAvailable)
                {
                    tcpStream.ReadByte();
                }

                byte targetAddr = sendData[1];
                byte sourceAddr = sendData[2];
                if (sourceAddr == 0xF1) sourceAddr = (byte)this.testerAddress;
                int dataOffset = 3;
                int dataLength = sendData[0] & 0x3F;
                if (dataLength == 0)
                {   // with length byte
                    dataLength = sendData[3];
                    dataOffset = 4;
                }
                int payloadLength = dataLength + 2;
                dataBuffer[0] = (byte)((payloadLength >> 24) & 0xFF);
                dataBuffer[1] = (byte)((payloadLength >> 16) & 0xFF);
                dataBuffer[2] = (byte)((payloadLength >> 8) & 0xFF);
                dataBuffer[3] = (byte)(payloadLength & 0xFF);
                dataBuffer[4] = 0x00;   // Payoad type: Diag message
                dataBuffer[5] = 0x01;
                dataBuffer[6] = sourceAddr;
                dataBuffer[7] = targetAddr;
                Array.Copy(sendData, dataOffset, dataBuffer, 8, dataLength);
                int sendLength = dataLength + 8;
                tcpStream.Write(dataBuffer, 0, sendLength);

                // wait for ack
                int recLen = ReceiveTelegram(ackBuffer, 5000);
                if (recLen < 0)
                {
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No ack received");
                    return false;
                }
                if ((recLen != sendLength) || (ackBuffer[5] != 0x02))
                {
                    if (enableLogging) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, recBuffer, 0, recLen, "*** Ack frame invalid");
                    return false;
                }
                ackBuffer[4] = dataBuffer[4];
                ackBuffer[5] = dataBuffer[5];
                for (int i = 0; i < recLen; i++)
                {
                    if (ackBuffer[i] != dataBuffer[i])
                    {
                        if (enableLogging) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, recBuffer, 0, recLen, "*** Ack data invalid");
                        return false;
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool ReceiveData(byte[] receiveData, int timeout)
        {
            if (tcpStream == null)
            {
                return false;
            }
            try
            {
                int recLen = ReceiveTelegram(dataBuffer, timeout);
                if (recLen < 0)
                {
                    return false;
                }
                int dataLen = (((int)dataBuffer[0] << 24) | ((int)dataBuffer[1] << 16) | ((int)dataBuffer[2] << 8) | dataBuffer[3]) - 2;
                if ((dataLen < 1) || ((dataLen + 8) > recLen))
                {
                    return false;
                }
                // create BMW-FAST telegram
                byte sourceAddr = dataBuffer[6];
                byte targetAddr = 0xF1;
                int len;
                if (dataLen > 0x3F)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = (byte)dataLen;
                    Array.Copy(dataBuffer, 8, receiveData, 4, dataLen);
                    len = dataLen + 4;
                }
                else
                {
                    receiveData[0] = (byte)(0x80 | dataLen);
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    Array.Copy(dataBuffer, 8, receiveData, 3, dataLen);
                    len = dataLen + 3;
                }
                receiveData[len] = CalcChecksumBmwFast(receiveData, len);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected int ReceiveTelegram(byte[] receiveData, int timeout)
        {
            if (tcpStream == null)
            {
                return -1;
            }
            int recLen = -1;
            try
            {
                long startTime = Stopwatch.GetTimestamp();
                for (; ; )
                {
                    if (tcpStream.DataAvailable)
                    {
                        int bytesRead = tcpStream.Read(receiveData, 0, 6);
                        recLen = bytesRead;
                        break;
                    }
                    if ((Stopwatch.GetTimestamp() - startTime) > timeout * tickResolMs)
                    {
                        return -1;
                    }
                    Thread.Sleep(10);
                }
                int telLen = (((int)receiveData[0] << 24) | ((int)receiveData[1] << 16) | ((int)receiveData[2] << 8) | receiveData[3]) + 6;
                if (telLen > transBufferSize)
                {
                    return -1;
                }
                for (; ; )
                {
                    if (recLen >= telLen)
                    {
                        break;
                    }
                    if (tcpStream.DataAvailable)
                    {
                        int bytesRead = tcpStream.Read(receiveData, recLen, telLen - recLen);
                        recLen += bytesRead;
                    }
                    if (recLen >= telLen)
                    {
                        break;
                    }
                    if ((Stopwatch.GetTimestamp() - startTime) > timeout * tickResolMs)
                    {
                        return -1;
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return recLen;
        }

        protected EdiabasNet.ErrorCodes OBDTrans(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            if (tcpStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            UInt32 retries = commRepeats;
            string retryComm = ediabas.GetConfigProperty("RetryComm");
            if (retryComm != null)
            {
                if (EdiabasNet.StringToValue(retryComm) == 0)
                {
                    retries = 0;
                }
            }
            for (int i = 0; i < retries + 1; i++)
            {
                errorCode = this.parTransmitFunc(sendData, sendDataLength, ref receiveData, out receiveLength);
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    return errorCode;
                }
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
                {   // interface error
                    break;
                }
            }
            return errorCode;
        }

        private EdiabasNet.ErrorCodes TransBmwFast(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            return TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, true);
        }

        private EdiabasNet.ErrorCodes TransKwp2000(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, bool enableLogging)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                int sendLength = TelLengthBmwFast(sendData);
                if (enableLogging) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, sendLength, "Send");

                if (!SendData(sendData, sendLength, enableLogging))
                {
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
            }

            int timeout = this.parTimeoutStd;
            for (int retry = 0; retry <= this.parRetryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, timeout))
                {
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No data received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthBmwFast(receiveData);
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, recLength + 1, "Resp");

                int dataLen = receiveData[0] & 0x3F;
                int dataStart = 3;
                if (dataLen == 0)
                {   // with length byte
                    dataLen = receiveData[3];
                    dataStart++;
                }
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** NR 0x78");
                    timeout = this.parTimeoutNR;
                }
                else
                {
                    break;
                }
            }

            receiveLength = TelLengthBmwFast(receiveData) + 1;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length without checksum
        private int TelLengthBmwFast(byte[] dataBuffer)
        {
            int telLength = dataBuffer[0] & 0x3F;
            if (telLength == 0)
            {   // with length byte
                telLength = dataBuffer[3] + 4;
            }
            else
            {
                telLength += 3;
            }
            return telLength;
        }

        private byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                InterfaceDisconnect();
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }
                InterfaceUnlock();

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
