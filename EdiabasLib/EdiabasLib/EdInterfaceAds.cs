using System;
using System.IO.Ports;

namespace EdiabasLib
{
    public class EdInterfaceAds : EdInterfaceObd
    {
        public override EdiabasNet Ediabas
        {
            get
            {
                return base.Ediabas;
            }
            set
            {
                base.Ediabas = value;

                string prop = ediabas.GetConfigProperty("AdsComPort");
                if (prop != null)
                {
                    comPort = prop;
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

                this.parTransmitFunc = null;
                this.parTimeoutStd = 0;
                this.parTimeoutTelEnd = 0;
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

                int baudRate;
                Parity parity;
                bool stateDtr = false;
                bool stateRts = false;
                switch (commParameter[0])
                {
                    case 0x0001:    // Concept 1 (requires ADS adapter!)
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 10 && commParameter[33] != 1)
                        {   // not checksum calculated by interface
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        commAnswerLen = new short[] { -2, 0 };
                        baudRate = (int)commParameter[1];
                        parity = Parity.Even;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransC1;
                        this.parTimeoutStd = (int)commParameter[5];
                        this.parTimeoutTelEnd = (int)commParameter[7];
                        break;

                    default:
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
                        return;
                }

                if (interfaceSetConfigFunc != null)
                {
                    if (!interfaceSetConfigFunc(baudRate, parity))
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return;
                    }
                    if (interfaceSetDtrFunc != null)
                    {
                        if (!interfaceSetDtrFunc(stateDtr))
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                    }
                    if (interfaceSetRtsFunc != null)
                    {
                        if (!interfaceSetRtsFunc(stateRts))
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
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
                    if (serialPort.DtrEnable != stateDtr)
                    {
                        serialPort.DtrEnable = stateDtr;
                    }
                    if (serialPort.RtsEnable != stateRts)
                    {
                        serialPort.RtsEnable = stateRts;
                    }
                }
            }
        }

        public override string InterfaceType
        {
            get
            {
                return "ADS";
            }
        }

        public override UInt32 InterfaceVersion
        {
            get
            {
                return 209;
            }
        }

        public override string InterfaceName
        {
            get
            {
                return "ADS";
            }
        }

        public override byte[] State
        {
            get
            {
                state[0] = 0x00;
                state[1] = (byte)(getDsrState() ? 0x00 : 0x30);
                return state;
            }
        }

        public override UInt32 BatteryVoltage
        {
            get
            {
                return (UInt32)(getDsrState() ? 12000 : 0);
            }
        }

        public override UInt32 IgnitionVoltage
        {
            get
            {
                return (UInt32)(getDsrState() ? 12000 : 0);
            }
        }

        public override bool IsValidInterfaceName(string name)
        {
            if (string.Compare(name, "ADS", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        public EdInterfaceAds()
        {
        }

        private EdiabasNet.ErrorCodes TransC1(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, int timeoutStd, int timeoutTelEnd, int timeoutNR, int retryNR)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                int sendLength = sendDataLength;
                sendData[sendLength] = CalcChecksumC1(sendData, sendLength);
                sendLength++;
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, sendLength, "Send");
                if (!SendData(sendData, sendLength))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                // no remote echo
            }

            // header byte
            int headerLen = 0;
            if (commAnswerLen != null && commAnswerLen.Length >= 2)
            {
                headerLen = commAnswerLen[0];
                if (headerLen < 0)
                {
                    headerLen = (-headerLen) + 1;
                }
            }
            if (headerLen == 0)
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Header lenght zero");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }
            if (!ReceiveData(receiveData, 0, headerLen, timeoutStd, timeoutTelEnd))
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No header received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            int recLength = TelLengthC1(receiveData);
            if (recLength == 0)
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Receive lenght zero");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!ReceiveData(receiveData, headerLen, recLength - headerLen, timeoutTelEnd, timeoutTelEnd))
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No tail received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, recLength, "Resp");
            if (CalcChecksumC1(receiveData, recLength - 1) != receiveData[recLength - 1])
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Checksum incorrect");
                ReceiveData(receiveData, 0, receiveData.Length, timeoutStd, timeoutTelEnd, true);
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            receiveLength = recLength;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length with checksum
        private int TelLengthC1(byte[] dataBuffer)
        {
            int telLength = 0;
            if (commAnswerLen != null && commAnswerLen.Length >= 2)
            {
                telLength = commAnswerLen[0];   // >0 fix length
                if (telLength < 0)
                {   // offset in buffer
                    int offset = (-telLength);
                    if (dataBuffer.Length < offset)
                    {
                        return 0;
                    }
                    telLength = dataBuffer[offset] + commAnswerLen[1];  // + answer offset
                }
            }
            return telLength;
        }

        private byte CalcChecksumC1(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum ^= data[i];
            }
            return sum;
        }
    }
}
