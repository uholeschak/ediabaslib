using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace EdiabasLib
{
    public class EdCommObd : EdCommBase
    {
        public delegate bool InterfaceConnectDelegate();
        public delegate bool InterfaceDisconnectDelegate();
        public delegate bool InterfaceSetConfigDelegate(int baudRate, Parity parity);
        public delegate bool SendDataDelegate(byte[] sendData, int length);
        public delegate bool ReceiveDataDelegate(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, bool logResponse);
        private delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, int timeoutStd, int timeoutTelEnd, int timeoutNR, int retryNR);

        private bool disposed = false;
        private SerialPort serialPort = new SerialPort();

        private string comPort = string.Empty;
        private bool connected = false;
        private const int echoTimeout = 100;
        private InterfaceConnectDelegate interfaceConnectFunc = null;
        private InterfaceDisconnectDelegate interfaceDisconnectFunc = null;
        private InterfaceSetConfigDelegate interfaceSetConfigFunc = null;
        private SendDataDelegate sendDataFunc = null;
        private ReceiveDataDelegate receiveDataFunc = null;
        private Stopwatch stopWatch = new Stopwatch();
        private byte[] keyBytes = new byte[0];
        private byte[] state = new byte[2];
        private byte[] sendBuffer = new byte[260];
        private byte[] recBuffer = new byte[260];

        public override string InterfaceType
        {
            get
            {
                return "OBD";
            }
        }

        public override UInt32 InterfaceVersion
        {
            get
            {
                return 209;
            }
        }

        public override byte[] KeyBytes
        {
            get
            {
                return keyBytes;
            }
        }

        public override byte[] State
        {
            get
            {
                return state;
            }
        }

        public override UInt32 BatteryVoltage
        {
            get
            {
                return 12000;
            }
        }

        public override UInt32 IgnitionVoltage
        {
            get
            {
                return 12000;
            }
        }

        public override bool Connected
        {
            get
            {
                return connected;
            }
        }

        public EdCommObd(EdiabasNet ediabas) : base(ediabas)
        {
            string prop = ediabas.GetConfigProperty("ObdComPort");
            if (prop != null)
            {
                comPort = prop;
            }
        }

        ~EdCommObd()
        {
            Dispose(false);
        }

        public override bool IsValidInterfaceName(string name)
        {
            if (string.Compare(name, "STD:OBD", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (string.Compare(name, "STD:OMITEC", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        public override bool InterfaceConnect()
        {
            if (!base.InterfaceConnect())
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }
            if (interfaceConnectFunc != null)
            {
                connected = interfaceConnectFunc();
                if (!connected)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                }
                return connected;
            }

            if (comPort.Length == 0)
            {
                return false;
            }
            if (serialPort.IsOpen)
            {
                return true;
            }
            try
            {
                serialPort.PortName = comPort;
                serialPort.BaudRate = 115200;
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;
                serialPort.ReadTimeout = 1;
                serialPort.Open();

                connected = true;
            }
            catch (Exception)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }
            return true;
        }

        public override bool InterfaceDisconnect()
        {
            base.InterfaceDisconnect();
            connected = false;
            if (interfaceDisconnectFunc != null)
            {
                return interfaceDisconnectFunc();
            }

            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            return true;
        }

        public override bool TransmitData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = null;
            if (sendData.Length > sendBuffer.Length)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0031);
                return false;
            }
            sendData.CopyTo(sendBuffer, 0);
            int receiveLength;
            if (!OBDTrans(sendBuffer, sendData.Length, ref recBuffer, out receiveLength))
            {
                return false;
            }
            receiveData = new byte[receiveLength];
            Array.Copy(recBuffer, receiveData, receiveLength);
            return true;
        }

        public string ComPort
        {
            get
            {
                return comPort;
            }
            set
            {
                comPort = value;
            }
        }

        public InterfaceConnectDelegate InterfaceConnectFunc
        {
            get
            {
                return interfaceConnectFunc;
            }
            set
            {
                interfaceConnectFunc = value;
            }
        }

        public InterfaceDisconnectDelegate InterfaceDisconnectFunc
        {
            get
            {
                return interfaceDisconnectFunc;
            }
            set
            {
                interfaceDisconnectFunc = value;
            }
        }

        public InterfaceSetConfigDelegate InterfaceSetConfigFunc
        {
            get
            {
                return interfaceSetConfigFunc;
            }
            set
            {
                interfaceSetConfigFunc = value;
            }
        }

        public SendDataDelegate SendDataFunc
        {
            get
            {
                return sendDataFunc;
            }
            set
            {
                sendDataFunc = value;
            }
        }

        public ReceiveDataDelegate ReceiveDataFunc
        {
            get
            {
                return receiveDataFunc;
            }
            set
            {
                receiveDataFunc = value;
            }
        }

        private bool SendData(byte[] sendData, int length)
        {
            if (sendDataFunc != null)
            {
                return sendDataFunc(sendData, length);
            }
            serialPort.DiscardInBuffer();
            serialPort.Write(sendData, 0, length);
            while (serialPort.BytesToWrite > 0)
            {
                Thread.Sleep(10);
            }
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, bool logResponse)
        {
            if (receiveDataFunc != null)
            {
                return receiveDataFunc(receiveData, offset, length, timeout, timeoutTelEnd, logResponse);
            }
            try
            {
                // wait for first byte
                int lastBytesToRead = 0;
                stopWatch.Reset();
                stopWatch.Start();
                for (; ; )
                {
                    lastBytesToRead = serialPort.BytesToRead;
                    if (lastBytesToRead > 0)
                    {
                        break;
                    }
                    if (stopWatch.ElapsedMilliseconds > timeout)
                    {
                        stopWatch.Stop();
                        return false;
                    }
                    Thread.Sleep(10);
                }

                int recLen = 0;
                stopWatch.Reset();
                stopWatch.Start();
                for (; ; )
                {
                    int bytesToRead = serialPort.BytesToRead;
                    if (bytesToRead >= length)
                    {
                        recLen += serialPort.Read(receiveData, offset + recLen, length - recLen);
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if (lastBytesToRead != bytesToRead)
                    {   // bytes received
                        stopWatch.Reset();
                        stopWatch.Start();
                        lastBytesToRead = bytesToRead;
                    }
                    else
                    {
                        if (stopWatch.ElapsedMilliseconds > timeoutTelEnd)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }
                stopWatch.Stop();
                if (logResponse)
                {
                    ediabas.LogData(receiveData, offset, recLen, "Rec ");
                }
                if (recLen < length)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd)
        {
            return ReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, false);
        }

        private bool OBDTrans(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            if (interfaceConnectFunc == null)
            {
                if (!serialPort.IsOpen)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0019);
                    return false;
                }
            }

            if ((commParameter == null) || (commParameter.Length < 1))
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                return false;
            }

            TransmitDelegate transmitFunc;
            int baudRate;
            Parity parity;
            int timeoutStd;
            int timeoutTelEnd;
            int timeoutNR;
            int retryNR;
            switch (commParameter[0])
            {
                case 0x010D:    // KWP2000*
                    if (commParameter.Length < 7)
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return false;
                    }
                    if (commParameter.Length >= 34 && commParameter[33] != 1)
                    {   // not checksum calculated by interface
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return false;
                    }
                    transmitFunc = TransKwp2000S;
                    baudRate = (int)commParameter[1];
                    parity = Parity.Even;
                    timeoutStd = (int)commParameter[2];
                    timeoutTelEnd = (int)commParameter[4];
                    timeoutNR = (int)commParameter[7];
                    retryNR = (int)commParameter[6];
                    break;

                case 0x010F:    // BMW-FAST
                    if (commParameter.Length < 7)
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return false;
                    }
                    if (commParameter[1] != 115200)
                    {   // not BMW-FAST baud rate
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return false;
                    }
                    if (commParameter.Length >= 8 && commParameter[7] != 1)
                    {   // not checksum calculated by interface
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return false;
                    }
                    transmitFunc = TransBmwFast;
                    parity = Parity.None;
                    baudRate = (int)commParameter[1];
                    timeoutStd = (int)commParameter[2];
                    timeoutTelEnd = (int)commParameter[4];
                    timeoutNR = (int)commParameter[6];
                    retryNR = (int)commParameter[5];
                    break;

                case 0x0110:    // D-CAN
                    if (commParameter.Length < 30)
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return false;
                    }
                    transmitFunc = TransBmwFast;
                    parity = Parity.None;
                    baudRate = 115200;
                    timeoutStd = (int)commParameter[7];
                    timeoutTelEnd = 10;
                    timeoutNR = (int)commParameter[9];
                    retryNR = (int)commParameter[10];
                    break;

                default:
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    return false;
            }

            if (interfaceSetConfigFunc != null)
            {
                if (!interfaceSetConfigFunc(baudRate, parity))
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    return false;
                }
            }
            else
            {
                if (serialPort.BaudRate != baudRate)
                {
                    serialPort.BaudRate = baudRate;
                }
                if (serialPort.Parity != parity)
                {
                    serialPort.Parity = parity;
                }
            }

            EdiabasNet.ErrorCodes errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            for (int i = 0; i < commRepeats + 1; i++)
            {
                errorCode = transmitFunc(sendData, sendDataLength, ref receiveData, out receiveLength, timeoutStd, timeoutTelEnd, timeoutNR, retryNR);
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    return true;
                }
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
                {   // interface error
                    break;
                }
            }
            ediabas.SetError(errorCode);
            return false;
        }

        private EdiabasNet.ErrorCodes TransBmwFast(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, int timeoutStd, int timeoutTelEnd, int timeoutNR, int retryNR)
        {
            receiveLength = 0;
            bool broadcast = false;
            if (sendData[1] == 0xEF)
            {
                broadcast = true;
            }
            if (sendDataLength == 0)
            {
                broadcast = true;
            }

            if (sendDataLength > 0)
            {
                int sendLength = TelLengthBmwFast(sendData);
                sendData[sendLength] = CalcChecksumBmwFast(sendData, sendLength);
                sendLength++;
                ediabas.LogData(sendData, 0, sendLength, "Send");
                if (!SendData(sendData, sendLength))
                {
                    ediabas.LogString("*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                // remove remote echo
                if (!ReceiveData(receiveData, 0, sendLength, echoTimeout, timeoutTelEnd))
                {
                    ediabas.LogString("*** No echo received");
                    ReceiveData(receiveData, 0, receiveData.Length, echoTimeout, timeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                ediabas.LogData(receiveData, 0, sendLength, "Echo");
                for (int i = 0; i < sendLength; i++)
                {
                    if (receiveData[i] != sendData[i])
                    {
                        ediabas.LogString("*** Echo incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeoutStd, timeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                }
            }

            int timeout = timeoutStd;
            for (int retry = 0; retry <= retryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout, timeoutTelEnd))
                {
                    ediabas.LogString("*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if ((receiveData[0] & 0xC0) != 0x80)
                {
                    ediabas.LogData(receiveData, 0, 4, "Head");
                    ediabas.LogString("*** Invalid header");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, timeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthBmwFast(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, timeoutTelEnd, timeoutTelEnd))
                {
                    ediabas.LogString("*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                ediabas.LogData(receiveData, 0, recLength + 1, "Resp");
                if (CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                {
                    ediabas.LogString("*** Checksum incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, timeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (!broadcast)
                {
                    if ((receiveData[1] != sendData[2]) ||
                        (receiveData[2] != sendData[1]))
                    {
                        ediabas.LogString("*** Address incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeout, timeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }

                int dataLen = receiveData[0] & 0x3F;
                int dataStart = 3;
                if (dataLen == 0)
                {   // with length byte
                    dataLen = receiveData[3];
                    dataStart++;
                }
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    ediabas.LogString("*** NR 0x78");
                    timeout = timeoutNR;
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
        private static int TelLengthBmwFast(byte[] dataBuffer)
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

        private static byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        private EdiabasNet.ErrorCodes TransKwp2000S(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, int timeoutStd, int timeoutTelEnd, int timeoutNR, int retryNR)
        {
            receiveLength = 0;
            bool broadcast = false;
            if (sendData[1] == 0xEF)
            {
                broadcast = true;
            }
            if (sendDataLength == 0)
            {
                broadcast = true;
            }

            if (sendDataLength > 0)
            {
                int sendLength = TelLengthKwp2000S(sendData);
                sendData[sendLength] = CalcChecksumKWP2000S(sendData, sendLength);
                sendLength++;
                ediabas.LogData(sendData, 0, sendLength, "Send");
                if (!SendData(sendData, sendLength))
                {
                    ediabas.LogString("*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                // remove remote echo
                if (!ReceiveData(receiveData, 0, sendLength, echoTimeout, timeoutTelEnd))
                {
                    ediabas.LogString("*** No echo received");
                    ReceiveData(receiveData, 0, receiveData.Length, echoTimeout, timeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                ediabas.LogData(receiveData, 0, sendLength, "Echo");
                for (int i = 0; i < sendLength; i++)
                {
                    if (receiveData[i] != sendData[i])
                    {
                        ediabas.LogString("*** Echo incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeoutStd, timeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                }
            }

            int timeout = timeoutStd;
            for (int retry = 0; retry <= retryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout, timeoutTelEnd))
                {
                    ediabas.LogString("*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthKwp2000S(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, timeoutTelEnd, timeoutTelEnd))
                {
                    ediabas.LogString("*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                ediabas.LogData(receiveData, 0, recLength + 1, "Resp");
                if (CalcChecksumKWP2000S(receiveData, recLength) != receiveData[recLength])
                {
                    ediabas.LogString("*** Checksum incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, timeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (!broadcast)
                {
                    if ((receiveData[1] != sendData[2]) ||
                        (receiveData[2] != sendData[1]))
                    {
                        ediabas.LogString("*** Address incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeout, timeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }

                int dataLen = receiveData[3];
                int dataStart = 4;
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    ediabas.LogString("*** NR 0x78");
                    timeout = timeoutNR;
                }
                else
                {
                    break;
                }
            }
            receiveLength = TelLengthKwp2000S(receiveData) + 1;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length without checksum
        private static int TelLengthKwp2000S(byte[] dataBuffer)
        {
            int telLength = dataBuffer[3] + 4;
            return telLength;
        }

        private static byte CalcChecksumKWP2000S(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum ^= data[i];
            }
            return sum;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    serialPort.Dispose();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
