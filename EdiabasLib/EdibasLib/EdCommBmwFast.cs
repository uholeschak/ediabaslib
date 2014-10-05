using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace EdiabasLib
{
    public class EdCommBmwFast : EdCommBase
    {
        public delegate bool InterfaceConnectDelegate();
        public delegate bool InterfaceDisconnectDelegate();
        public delegate bool TransmitDataDelegate(byte[] sendData, ref byte[] receiveData, int timeoutStd, int timeoutNR, int retryNR);

        private bool disposed = false;
        private SerialPort serialPort = new SerialPort();

        private string comPort = string.Empty;
        private const int echoTimeout = 100;
        private InterfaceConnectDelegate interfaceConnectFunc = null;
        private InterfaceDisconnectDelegate interfaceDisconnectFunc = null;
        private TransmitDataDelegate transmitDataFunc = null;
        private Stopwatch stopWatch = new Stopwatch();
        private byte[] sendBuffer = new byte[256];
        private byte[] recBuffer = new byte[256];

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

        public override bool Connected
        {
            get
            {
                return serialPort.IsOpen;
            }
        }

        public EdCommBmwFast(Ediabas ediabas) : base(ediabas)
        {
        }

        ~EdCommBmwFast()
        {
            Dispose(false);
        }

        public override bool InterfaceConnect()
        {
            if (interfaceConnectFunc != null)
            {
                return interfaceConnectFunc();
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
            }
            catch (Exception ex)
            {
                ediabas.LogString("Serial port exception: " + Ediabas.GetExceptionText(ex));
                return false;
            }
            return true;
        }

        public override bool InterfaceDisconnect()
        {
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
                return false;
            }
            sendData.CopyTo(sendBuffer, 0);
            if (!OBDTrans(sendBuffer, ref recBuffer))
            {
                return false;
            }
            int recLength = TelLength(recBuffer) + 1;
            receiveData = new byte[recLength];
            Array.Copy(recBuffer, receiveData, recLength);
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

        public TransmitDataDelegate TransmitDataFunc
        {
            get
            {
                return transmitDataFunc;
            }
            set
            {
                transmitDataFunc = value;
            }
        }

        private bool SendData(byte[] sendData, int length)
        {
            serialPort.DiscardInBuffer();
            serialPort.Write(sendData, 0, length);
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, bool logResponse)
        {
            try
            {
                int recLen = 0;
                stopWatch.Reset();
                stopWatch.Start();
                for (; ; )
                {
                    if (serialPort.BytesToRead >= length)
                    {
                        recLen = serialPort.Read(receiveData, offset + recLen, length - recLen);
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if (stopWatch.ElapsedMilliseconds > timeout)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                stopWatch.Stop();
                if (logResponse)
                {
                    ediabas.LogData(receiveData, recLen, "Rec ");
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

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout)
        {
            return ReceiveData(receiveData, offset, length, timeout, false);
        }

        private bool OBDTrans(byte[] sendData, ref byte[] receiveData)
        {
            if (interfaceConnectFunc == null)
            {
                if (!serialPort.IsOpen)
                {
                    ediabas.SetError(Ediabas.ErrorCodes.EDIABAS_IFH_0019);
                }
            }

            if (ediabas.CommParameter.Length < 7)
            {
                return false;
            }
            if (ediabas.CommParameter[0] != 0x010F)
            {   // not BMW-FAST
                return false;
            }
            if (ediabas.CommParameter[1] != 115200)
            {   // not BMW-FAST baud rate
                return false;
            }
            if (ediabas.CommParameter.Length >= 8 && ediabas.CommParameter[7] != 1)
            {   // not checksum calculated by interface
                return false;
            }
            for (int i = 0; i < ediabas.CommRepeats + 1; i++)
            {
                if (OBDTrans(sendData, ref receiveData, (int)ediabas.CommParameter[2], (int)ediabas.CommParameter[6], (int)ediabas.CommParameter[5]))
                {
                    return true;
                }
            }
            return false;
        }

        private bool OBDTrans(byte[] sendData, ref byte[] receiveData, int timeoutStd, int timeoutNR, int retryNR)
        {
            if (transmitDataFunc != null)
            {
                return transmitDataFunc(sendData, ref receiveData, timeoutStd, timeoutNR, retryNR);
            }

            int sendLength = TelLength(sendData);
            sendData[sendLength] = CalcChecksum(sendData, sendLength);
            sendLength++;
            ediabas.LogData(sendData, sendLength, "Send");
            if (!SendData(sendData, sendLength))
            {
                ediabas.LogString("*** Sending failed");
                return false;
            }
            // remove remote echo
            if (!ReceiveData(receiveData, 0, sendLength, echoTimeout))
            {
                ediabas.LogString("*** No echo received");
                ReceiveData(receiveData, 0, receiveData.Length, echoTimeout, true);
                return false;
            }
            ediabas.LogData(receiveData, sendLength, "Echo");
            for (int i = 0; i < sendLength; i++)
            {
                if (receiveData[i] != sendData[i])
                {
                    ediabas.LogString("*** Echo incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, timeoutStd, true);
                    return false;
                }
            }

            int timeout = timeoutStd;
            for (int retry = 0; retry <= retryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout))
                {
                    ediabas.LogString("*** No header received");
                    return false;
                }
                if ((receiveData[0] & 0xC0) != 0x80)
                {
                    ediabas.LogData(receiveData, 4, "Head");
                    ediabas.LogString("*** Invalid header");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, true);
                    return false;
                }

                int recLength = TelLength(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, timeout))
                {
                    ediabas.LogString("*** No tail received");
                    return false;
                }
                ediabas.LogData(receiveData, recLength + 1, "Resp");
                if (CalcChecksum(receiveData, recLength) != receiveData[recLength])
                {
                    ediabas.LogString("*** Checksum incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, true);
                    return false;
                }
                if ((receiveData[1] != sendData[2]) ||
                    (receiveData[2] != sendData[1]))
                {
                    ediabas.LogString("*** Address incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, true);
                    return false;
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
            return true;
        }

        // telegram length without checksum
        private static int TelLength(byte[] dataBuffer)
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

        private static byte CalcChecksum(byte[] data, int length)
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
