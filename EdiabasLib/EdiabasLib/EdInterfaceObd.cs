using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace EdiabasLib
{
    public class EdInterfaceObd : EdInterfaceBase
    {
        public delegate bool InterfaceConnectDelegate(string port);
        public delegate bool InterfaceDisconnectDelegate();
        public delegate EdFtdiInterface.ErrorResult InterfaceSetConfigDelegate(int baudRate, int dataBits, Parity parity, bool allowBitBang);
        public delegate bool InterfaceSetDtrDelegate(bool dtr);
        public delegate bool InterfaceSetRtsDelegate(bool rts);
        public delegate bool InterfaceGetDsrDelegate(out bool dsr);
        public delegate bool InterfaceSetBreakDelegate(bool enable);
        public delegate bool InterfacePurgeInBufferDelegate();
        public delegate bool InterfaceSendDataDelegate(byte[] sendData, int length, bool setDtr, double dtrTimeCorr);
        public delegate bool InterfaceReceiveDataDelegate(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog);
        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        protected delegate EdiabasNet.ErrorCodes IdleDelegate();
        protected delegate EdiabasNet.ErrorCodes FinishDelegate();

        protected enum CommThreadCommands
        {
            Idle,               // do nothing
            SingleTransmit,     // single data transmission
            IdleTransmit,       // idle data transmission
            Exit,               // exit thread
        }

        private bool disposed = false;
        protected const int transBufferSize = 1024; // transmit buffer size
        protected static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly byte[] byteArray0 = new byte[0];
        protected static readonly long tickResolMs = Stopwatch.Frequency / 1000;
        protected static SerialPort serialPort;
        protected static AutoResetEvent commReceiveEvent;
        protected static AutoResetEvent commThreadReqEvent;
        protected static AutoResetEvent commThreadResEvent;
        protected static Thread commThread;
        protected static object commThreadLock;
        protected static volatile CommThreadCommands commThreadCommand;
        protected static volatile uint commThreadReqCount;
        protected static volatile uint commThreadResCount;

        protected string comPort = string.Empty;
        protected double dtrTimeCorrCom = 0.3;
        protected double dtrTimeCorrFtdi = 0.3;
        protected int addRecTimeout = 20;
        protected bool enableFtdiBitBang = false;
        protected bool connected = false;
        protected const int echoTimeout = 100;
        protected bool useExtInterfaceFunc = false;
        protected InterfaceConnectDelegate interfaceConnectFunc = null;
        protected InterfaceConnectDelegate interfaceConnectFuncInt = null;
        protected InterfaceDisconnectDelegate interfaceDisconnectFunc = null;
        protected InterfaceDisconnectDelegate interfaceDisconnectFuncInt = null;
        protected InterfaceSetConfigDelegate interfaceSetConfigFunc = null;
        protected InterfaceSetConfigDelegate interfaceSetConfigFuncInt = null;
        protected InterfaceSetDtrDelegate interfaceSetDtrFunc = null;
        protected InterfaceSetDtrDelegate interfaceSetDtrFuncInt = null;
        protected InterfaceSetRtsDelegate interfaceSetRtsFunc = null;
        protected InterfaceSetRtsDelegate interfaceSetRtsFuncInt = null;
        protected InterfaceGetDsrDelegate interfaceGetDsrFunc = null;
        protected InterfaceGetDsrDelegate interfaceGetDsrFuncInt = null;
        protected InterfaceSetBreakDelegate interfaceSetBreakFunc = null;
        protected InterfaceSetBreakDelegate interfaceSetBreakFuncInt = null;
        protected InterfacePurgeInBufferDelegate interfacePurgeInBufferFunc = null;
        protected InterfacePurgeInBufferDelegate interfacePurgeInBufferFuncInt = null;
        protected InterfaceSendDataDelegate interfaceSendDataFunc = null;
        protected InterfaceSendDataDelegate interfaceSendDataFuncInt = null;
        protected InterfaceReceiveDataDelegate interfaceReceiveDataFunc = null;
        protected InterfaceReceiveDataDelegate interfaceReceiveDataFuncInt = null;
        protected Stopwatch stopWatch = new Stopwatch();
        protected byte[] keyBytes = byteArray0;
        protected byte[] state = new byte[2];
        protected byte[] sendBuffer = new byte[transBufferSize];
        protected byte[] sendBufferThread = new byte[transBufferSize];
        protected byte[] sendBufferInternal = new byte[1];
        protected volatile int sendBufferLength = 0;
        protected byte[] recBuffer = new byte[transBufferSize];
        protected byte[] recBufferThread = new byte[transBufferSize];
        protected volatile int recBufferLength = 0;
        protected volatile EdiabasNet.ErrorCodes recErrorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        protected byte[] iso9141Buffer = new byte[256];
        protected byte[] iso9141BlockBuffer = new byte[1];
        protected bool ecuConnected;
        protected long lastCommTick;
        protected long lastResponseTick;
        protected byte blockCounter;
        protected byte lastIso9141Cmd;

        protected TransmitDelegate parTransmitFunc;
        protected IdleDelegate parIdleFunc;
        protected FinishDelegate parFinishFunc;
        protected int parTimeoutStd = 0;
        protected int parTimeoutTelEnd = 0;
        protected int parInterbyteTime = 0;
        protected int parRegenTime = 0;
        protected int parTimeoutNR = 0;
        protected int parRetryNR = 0;
        protected byte parWakeAddress = 0;
        protected int parTesterPresentTime = 0;
        protected int parTesterPresentTelLen = 0;
        protected byte[] parTesterPresentTel = new byte[transBufferSize];
        protected int parStartCommTelLen = 0;
        protected byte[] parStartCommTel = new byte[transBufferSize];
        protected bool parChecksumByUser = false;
        protected bool parChecksumNoCheck = false;
        protected bool parSendSetDtr = false;
        protected bool parAllowBitBang = false;
        protected bool parHasKeyBytes = false;
        protected bool parSupportFrequent = false;

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
                prop = ediabas.GetConfigProperty("ObdComPort");
                if (prop != null)
                {
                    this.comPort = prop;
                }

                prop = ediabas.GetConfigProperty("ObdDtrTimeCorrCom");
                if (prop != null)
                {
                    this.dtrTimeCorrCom = EdiabasNet.StringToFloat(prop);
                }

                prop = ediabas.GetConfigProperty("ObdDtrTimeCorrFtdi");
                if (prop != null)
                {
                    this.dtrTimeCorrFtdi = EdiabasNet.StringToFloat(prop);
                }

                prop = ediabas.GetConfigProperty("ObdAddRecTimeout");
                if (prop != null)
                {
                    this.addRecTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = ediabas.GetConfigProperty("ObdFtdiBitBang");
                if (prop != null)
                {
                    this.enableFtdiBitBang = EdiabasNet.StringToValue(prop) != 0;
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
                StopCommThread();

                commParameter = value;
                commAnswerLen[0] = 0;
                commAnswerLen[1] = 0;

                this.parTransmitFunc = null;
                this.parIdleFunc = null;
                this.parFinishFunc = null;
                this.parTimeoutStd = 0;
                this.parTimeoutTelEnd = 0;
                this.parInterbyteTime = 0;
                this.parRegenTime = 0;
                this.parTimeoutNR = 0;
                this.parRetryNR = 0;
                this.parWakeAddress = 0;
                this.parTesterPresentTime = 0;
                this.parTesterPresentTelLen = 0;
                this.parStartCommTelLen = 0;
                this.parChecksumByUser = false;
                this.parChecksumNoCheck = false;
                this.parSendSetDtr = false;
                this.parAllowBitBang = false;
                this.parHasKeyBytes = false;
                this.parSupportFrequent = false;
                this.keyBytes = byteArray0;
                this.ecuConnected = false;
                // don't init lastCommTick here
                this.lastResponseTick = DateTime.MinValue.Ticks;
                this.blockCounter = 0;
                this.lastIso9141Cmd = 0x00;

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
                    string.Format(culture, "{0} CommParameter Port={1}, CorrCom={2}, CorrFtdi={3}, RecTimeout={4}, BitBang={5}",
                            InterfaceName, this.comPort, this.dtrTimeCorrCom, this.dtrTimeCorrFtdi, this.addRecTimeout, this.enableFtdiBitBang));

                int baudRate;
                int dataBits = 8;
                Parity parity;
                bool stateDtr = false;
                bool stateRts = false;
                uint concept = commParameter[0];
                switch (concept)
                {
                    case 0x0001:    // Concept 1
                        if (adapterEcho)
                        {   // only with ADS adapter
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                            return;
                        }
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 10)
                        {
                            if (!EvalChecksumPar(commParameter[9]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        commAnswerLen[0] = -2;
                        commAnswerLen[1] = 0;
                        baudRate = (int)commParameter[1];
                        dataBits = 8;
                        parity = Parity.Even;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransDS2;
                        this.parTimeoutStd = (int)commParameter[5];
                        this.parRegenTime = (int)commParameter[6];
                        this.parTimeoutTelEnd = (int)commParameter[7];
                        this.parSendSetDtr = false;
                        this.parAllowBitBang = false;
                        break;

                    case 0x0002:    // Concept 2 ISO 9141
                        if (adapterEcho)
                        {   // only with ADS adapter
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                            return;
                        }
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 10)
                        {
                            if (!EvalChecksumPar(commParameter[9]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        commAnswerLen[0] = 1;
                        commAnswerLen[1] = 0;
                        baudRate = 9600;
                        dataBits = 8;
                        parity = Parity.None;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransIso9141;
                        this.parIdleFunc = IdleIso9141;
                        this.parWakeAddress = (byte)commParameter[2];
                        this.parTimeoutStd = (int)commParameter[5];
                        this.parRegenTime = (int)commParameter[6];
                        this.parTimeoutTelEnd = (int)commParameter[7];
                        this.parSendSetDtr = true;
                        this.parAllowBitBang = this.enableFtdiBitBang;
                        this.parHasKeyBytes = true;
                        break;

                    case 0x0003:    // Concept 3
                        if (adapterEcho)
                        {   // only with ADS adapter
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                            return;
                        }
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 10)
                        {
                            if (!EvalChecksumPar(commParameter[9]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        commAnswerLen[0] = 52;
                        commAnswerLen[1] = 0;
                        baudRate = 9600;
                        dataBits = 8;
                        parity = Parity.None;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransConcept3;
                        this.parIdleFunc = IdleConcept3;
                        this.parFinishFunc = FinishConcept3;
                        this.parWakeAddress = (byte)commParameter[2];
                        this.parTimeoutStd = (int)commParameter[5];
                        this.parRegenTime = (int)commParameter[6];
                        this.parTimeoutTelEnd = (int)commParameter[7];
                        this.parSendSetDtr = true;
                        this.parAllowBitBang = false;
                        this.parHasKeyBytes = true;
                        this.parSupportFrequent = true;
                        break;

                    case 0x0005:    // DS1
                    case 0x0006:    // DS2
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 10)
                        {
                            if (!EvalChecksumPar(commParameter[9]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        commAnswerLen[0] = -1;
                        commAnswerLen[1] = 0;
                        baudRate = (int)commParameter[1];
                        dataBits = 8;
                        parity = Parity.Even;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransDS2;
                        this.parTimeoutStd = (int)commParameter[5];
                        this.parRegenTime = (int)commParameter[6];
                        this.parTimeoutTelEnd = (int)commParameter[7];
                        this.parInterbyteTime = (int)commParameter[8];
                        this.parSendSetDtr = false;
                        if (concept == 6)
                        {   // DS2 uses DTR
                            this.parSendSetDtr = !adapterEcho;
                        }
                        this.parAllowBitBang = false;
                        break;

                    case 0x010C:    // KWP2000 BMW
                        if (commParameter.Length < 33)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 34)
                        {
                            if (!EvalChecksumPar(commParameter[33]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        baudRate = (int)commParameter[1];
                        dataBits = 8;
                        parity = Parity.None;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransKwp2000Bmw;
                        this.parIdleFunc = IdleKwp2000Bmw;
                        this.parTimeoutStd = (int)commParameter[2];
                        this.parRegenTime = (int)commParameter[3];
                        this.parTimeoutTelEnd = (int)commParameter[4];
                        this.parInterbyteTime = (int)commParameter[5];
                        this.parTimeoutNR = (int)commParameter[7];
                        this.parRetryNR = (int)commParameter[6];
                        this.parTesterPresentTime = (int)commParameter[8];
                        this.parTesterPresentTelLen = (int)commParameter[9];
                        if (this.parTesterPresentTelLen > 11)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (this.parTesterPresentTelLen > 0)
                        {
                            for (int i = 0; i < this.parTesterPresentTelLen; i++)
                            {
                                this.parTesterPresentTel[i] = (byte)commParameter[10 + i];
                            }
                        }
                        this.parStartCommTelLen = (int)commParameter[21];
                        if (this.parStartCommTelLen > 11)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (this.parStartCommTelLen > 0)
                        {
                            for (int i = 0; i < this.parStartCommTelLen; i++)
                            {
                                this.parStartCommTel[i] = (byte)commParameter[22 + i];
                            }
                        }
                        this.parSendSetDtr = !adapterEcho;
                        this.parAllowBitBang = false;
                        break;

                    case 0x010D:    // KWP2000*
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 22)
                        {
                            if (!EvalChecksumPar(commParameter[21]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        baudRate = (int)commParameter[1];
                        dataBits = 8;
                        parity = Parity.Even;
                        stateDtr = false;
                        stateRts = false;
                        this.parTransmitFunc = TransKwp2000S;
                        this.parTimeoutStd = (int)commParameter[2];
                        this.parRegenTime = (int)commParameter[3];
                        this.parTimeoutTelEnd = (int)commParameter[4];
                        this.parInterbyteTime = (int)commParameter[5];
                        this.parTimeoutNR = (int)commParameter[7];
                        this.parRetryNR = (int)commParameter[6];
                        this.parSendSetDtr = !adapterEcho;
                        this.parAllowBitBang = false;
                        break;

                    case 0x010F:    // BMW-FAST
                        if (commParameter.Length < 7)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (commParameter.Length >= 8)
                        {
                            if (!EvalChecksumPar(commParameter[7]))
                            {
                                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        baudRate = (int)commParameter[1];
                        dataBits = 8;
                        parity = Parity.None;
                        stateDtr = adapterEcho;
                        stateRts = false;
                        this.parTransmitFunc = TransBmwFast;
                        this.parTimeoutStd = (int)commParameter[2];
                        this.parRegenTime = (int)commParameter[3];
                        this.parTimeoutTelEnd = (int)commParameter[4];
                        this.parTimeoutNR = (int)commParameter[6];
                        this.parRetryNR = (int)commParameter[5];
                        this.parSendSetDtr = !adapterEcho;
                        this.parAllowBitBang = false;
                        break;

                    case 0x0110:    // D-CAN
                        if (commParameter.Length < 30)
                        {
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        baudRate = 115200;
                        dataBits = 8;
                        parity = Parity.None;
                        stateDtr = adapterEcho;
                        stateRts = false;
                        this.parTransmitFunc = TransBmwFast;
                        this.parTimeoutStd = (int)commParameter[7];
                        this.parTimeoutTelEnd = 10;
                        this.parRegenTime = (int)commParameter[8];
                        this.parTimeoutNR = (int)commParameter[9];
                        this.parRetryNR = (int)commParameter[10];
                        this.parSendSetDtr = !adapterEcho;
                        this.parAllowBitBang = false;
                        break;

                    default:
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
                        return;
                }

                StartCommThread();

                if (this.useExtInterfaceFunc)
                {
                    EdFtdiInterface.ErrorResult ftdiResult = InterfaceSetConfigFuncUse(baudRate, dataBits, parity, this.parAllowBitBang);
                    switch (ftdiResult)
                    {
                        case EdFtdiInterface.ErrorResult.NO_ERROR:
                            break;

                        case EdFtdiInterface.ErrorResult.USB_LOC_ERROR:
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Invalid USB bus configuration! Only one device per USB root hub allowed!");
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0063);
                            return;

                        default:
                            ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                    }
                    if (!InterfaceSetDtrFuncUse(stateDtr))
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return;
                    }
                    if (!InterfaceSetRtsFuncUse(stateRts))
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return;
                    }
                }
                else
                {
                    if (serialPort.BaudRate != baudRate)
                    {
                        serialPort.BaudRate = baudRate;
                    }
                    if (serialPort.DataBits != dataBits)
                    {
                        serialPort.DataBits = dataBits;
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

        public override string InterfaceName
        {
            get
            {
                return "STD:OBD";
            }
        }

        public override byte[] KeyBytes
        {
            get
            {
                if (this.commParameter != null && this.parHasKeyBytes)
                {
                    if (this.ecuConnected)
                    {
                        return keyBytes;
                    }
                    // start transmission
                    if (commThread == null)
                    {
                        ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0030);
                        return byteArray0;
                    }
                    lock (commThreadLock)
                    {
                        commThreadReqCount++;
                        commThreadCommand = CommThreadCommands.SingleTransmit;
                    }
                    commThreadReqEvent.Set();

                    for (; ; )
                    {
                        lock (commThreadLock)
                        {
                            if (commThreadResCount == commThreadReqCount)
                            {
                                break;
                            }
                        }
                        commThreadResEvent.WaitOne(10, false);
                    }
                    if (this.recErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        ediabas.SetError(this.recErrorCode);
                        return byteArray0;
                    }
                    return keyBytes;
                }
                return byteArray0;
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

        public override bool Connected
        {
            get
            {
                if (this.useExtInterfaceFunc)
                {
                    return connected;
                }
                return serialPort.IsOpen;
            }
        }

        static EdInterfaceObd()
        {
#if WindowsCE
            interfaceMutex = new Mutex(false);
#else
            interfaceMutex = new Mutex(false, "EdiabasLib_InterfaceObd");
#endif
            serialPort = new SerialPort();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
            commReceiveEvent = new AutoResetEvent(false);
            commThreadReqEvent = new AutoResetEvent(false);
            commThreadResEvent = new AutoResetEvent(false);
            commThread = null;
            commThreadLock = new Object();
            commThreadCommand = CommThreadCommands.Idle;
            commThreadReqCount = 0;
            commThreadResCount = 0;
        }

        public EdInterfaceObd()
        {
            UpdateUseExtInterfaceFunc();
        }

        ~EdInterfaceObd()
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

            if (this.comPort.ToUpper(culture).StartsWith(EdFtdiInterface.PortID))
            {   // automtatic hook of FTDI functions
                interfaceConnectFuncInt = EdFtdiInterface.InterfaceConnect;
                interfaceDisconnectFuncInt = EdFtdiInterface.InterfaceDisconnect;
                interfaceSetConfigFuncInt = EdFtdiInterface.InterfaceSetConfig;
                interfaceSetDtrFuncInt = EdFtdiInterface.InterfaceSetDtr;
                interfaceSetRtsFuncInt = EdFtdiInterface.InterfaceSetRts;
                interfaceGetDsrFuncInt = EdFtdiInterface.InterfaceGetDsr;
                interfaceSetBreakFuncInt = EdFtdiInterface.InterfaceSetBreak;
                interfacePurgeInBufferFuncInt = EdFtdiInterface.InterfacePurgeInBuffer;
                interfaceSendDataFuncInt = EdFtdiInterface.InterfaceSendData;
                interfaceReceiveDataFuncInt = EdFtdiInterface.InterfaceReceiveData;
            }
            else
            {
                interfaceConnectFuncInt = null;
                interfaceDisconnectFuncInt = null;
                interfaceSetConfigFuncInt = null;
                interfaceSetDtrFuncInt = null;
                interfaceSetRtsFuncInt = null;
                interfaceGetDsrFuncInt = null;
                interfaceSetBreakFuncInt = null;
                interfacePurgeInBufferFuncInt = null;
                interfaceSendDataFuncInt = null;
                interfaceReceiveDataFuncInt = null;
            }
            UpdateUseExtInterfaceFunc();

            if (this.useExtInterfaceFunc)
            {
                connected = InterfaceConnectFuncUse(this.comPort);
                if (!connected)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                }
                return connected;
            }

            if (this.comPort.Length == 0)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }
            if (serialPort.IsOpen)
            {
                return true;
            }
            try
            {
                serialPort.PortName = this.comPort;
                serialPort.BaudRate = 9600;
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = false;
                serialPort.ReadTimeout = 1;
                serialPort.Open();

                this.lastCommTick = DateTime.MinValue.Ticks;
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
            StopCommThread();
            base.InterfaceDisconnect();
            connected = false;
            if (this.useExtInterfaceFunc)
            {
                return InterfaceDisconnectFuncUse();
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
            if (commParameter == null)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (sendData.Length > this.sendBuffer.Length)
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0031);
                return false;
            }
#if false
            sendData.CopyTo(this.sendBuffer, 0);
            this.sendBufferLength = sendData.Length;
            int recLength;
            this.recErrorCode = OBDTrans(this.sendBuffer, sendData.Length, ref this.recBuffer, out recLength);
            this.recBufferLength = recLength;
#else
            StartCommThread();
            lock (commThreadLock)
            {
                sendData.CopyTo(this.sendBuffer, 0);
                this.sendBufferLength = sendData.Length;
                commThreadReqCount++;
                commThreadCommand = CommThreadCommands.SingleTransmit;
            }
            commThreadReqEvent.Set();

            for (;;)
            {
                lock (commThreadLock)
                {
                    if (commThreadResCount == commThreadReqCount)
                    {
                        break;
                    }
                }
                commThreadResEvent.WaitOne(10, false);
            }
#endif
            if (this.recErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                ediabas.SetError(this.recErrorCode);
                return false;
            }
            lock (commThreadLock)
            {
                receiveData = new byte[this.recBufferLength];
                Array.Copy(this.recBuffer, receiveData, this.recBufferLength);
            }
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
            if (!this.parSupportFrequent)
            {
                receiveData = byteArray0;
                return true;
            }
            StartCommThread();
            lock (commThreadLock)
            {
                this.sendBufferLength = 0;
                commThreadReqCount++;
                commThreadCommand = CommThreadCommands.SingleTransmit;
            }
            commThreadReqEvent.Set();

            for (; ; )
            {
                lock (commThreadLock)
                {
                    if (commThreadResCount == commThreadReqCount)
                    {
                        break;
                    }
                }
                commThreadResEvent.WaitOne(10, false);
            }

            if (this.recErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                ediabas.SetError(this.recErrorCode);
                return false;
            }
            lock (commThreadLock)
            {
                receiveData = new byte[this.recBufferLength];
                Array.Copy(this.recBuffer, receiveData, this.recBufferLength);
            }
            return true;
        }

        public override bool StopFrequent()
        {
            StopCommThread();
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
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceConnectDelegate InterfaceConnectFuncUse
        {
            get
            {
                return (interfaceConnectFunc != null) ? interfaceConnectFunc : interfaceConnectFuncInt;
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
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceDisconnectDelegate InterfaceDisconnectFuncUse
        {
            get
            {
                return (interfaceDisconnectFunc != null) ? interfaceDisconnectFunc : interfaceDisconnectFuncInt;
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
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetConfigDelegate InterfaceSetConfigFuncUse
        {
            get
            {
                return (interfaceSetConfigFunc != null) ? interfaceSetConfigFunc : interfaceSetConfigFuncInt;
            }
        }

        public InterfaceSetDtrDelegate InterfaceSetDtrFunc
        {
            get
            {
                return interfaceSetDtrFunc;
            }
            set
            {
                interfaceSetDtrFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetDtrDelegate InterfaceSetDtrFuncUse
        {
            get
            {
                return (interfaceSetDtrFunc != null) ? interfaceSetDtrFunc : interfaceSetDtrFuncInt;
            }
        }

        public InterfaceSetRtsDelegate InterfaceSetRtsFunc
        {
            get
            {
                return interfaceSetRtsFunc;
            }
            set
            {
                interfaceSetRtsFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetRtsDelegate InterfaceSetRtsFuncUse
        {
            get
            {
                return (interfaceSetRtsFunc != null) ? interfaceSetRtsFunc : interfaceSetRtsFuncInt;
            }
        }

        public InterfaceGetDsrDelegate InterfaceGetDsrFunc
        {
            get
            {
                return interfaceGetDsrFunc;
            }
            set
            {
                interfaceGetDsrFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceGetDsrDelegate InterfaceGetDsrFuncUse
        {
            get
            {
                return (interfaceGetDsrFunc != null) ? interfaceGetDsrFunc : interfaceGetDsrFuncInt;
            }
        }

        public InterfaceSetBreakDelegate InterfaceSetBreakFunc
        {
            get
            {
                return interfaceSetBreakFunc;
            }
            set
            {
                interfaceSetBreakFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetBreakDelegate InterfaceSetBreakFuncUse
        {
            get
            {
                return (interfaceSetBreakFunc != null) ? interfaceSetBreakFunc : interfaceSetBreakFuncInt;
            }
        }

        public InterfacePurgeInBufferDelegate InterfacePurgeInBufferFunc
        {
            get
            {
                return interfacePurgeInBufferFunc;
            }
            set
            {
                interfacePurgeInBufferFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfacePurgeInBufferDelegate InterfacePurgeInBufferFuncUse
        {
            get
            {
                return (interfacePurgeInBufferFunc != null) ? interfacePurgeInBufferFunc : interfacePurgeInBufferFuncInt;
            }
        }

        public InterfaceSendDataDelegate InterfaceSendDataFunc
        {
            get
            {
                return interfaceSendDataFunc;
            }
            set
            {
                interfaceSendDataFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSendDataDelegate InterfaceSendDataFuncUse
        {
            get
            {
                return (interfaceSendDataFunc != null) ? interfaceSendDataFunc : interfaceSendDataFuncInt;
            }
        }

        public InterfaceReceiveDataDelegate InterfaceReceiveDataFunc
        {
            get
            {
                return interfaceReceiveDataFunc;
            }
            set
            {
                interfaceReceiveDataFunc = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceReceiveDataDelegate InterfaceReceiveDataFuncUse
        {
            get
            {
                return (interfaceReceiveDataFunc != null) ? interfaceReceiveDataFunc : interfaceReceiveDataFuncInt;
            }
        }

        protected virtual bool adapterEcho
        {
            get
            {
                return true;
            }
        }

        protected void UpdateUseExtInterfaceFunc()
        {
            this.useExtInterfaceFunc =
                InterfaceConnectFuncUse != null &&
                InterfaceDisconnectFuncUse != null &&
                InterfaceSetConfigFuncUse != null &&
                InterfaceSetDtrFuncUse != null &&
                InterfaceSetRtsFuncUse != null &&
                InterfaceGetDsrFuncUse != null &&
                InterfaceSetBreakFuncUse != null &&
                InterfacePurgeInBufferFuncUse != null &&
                InterfaceSendDataFuncUse != null &&
                InterfaceReceiveDataFuncUse != null;
        }

        protected bool getDsrState()
        {
            if (!this.useExtInterfaceFunc)
            {
                if (!serialPort.IsOpen)
                {
                    ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0019);
                    return false;
                }
                return serialPort.DsrHolding;
            }

            bool dsrState = false;
            if (!InterfaceGetDsrFuncUse(out dsrState))
            {
                ediabas.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0019);
                return false;
            }
            return dsrState;
        }

        private static void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            commReceiveEvent.Set();
        }

        private bool EvalChecksumPar(uint value)
        {
            this.parChecksumByUser = (value & 0x01) == 0;
            this.parChecksumNoCheck = (value & 0x02) != 0;
            return true;
        }

        private bool StartCommThread()
        {
            if (commThread != null)
            {
                return true;
            }
            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Start comm thread");
            try
            {
                commThreadCommand = CommThreadCommands.Idle;
                commThreadReqCount = 0;
                commThreadResCount = 0;
                commThread = new Thread(CommThreadFunc);
                commThread.Priority = ThreadPriority.Highest;
                commThread.Start();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void StopCommThread()
        {
            if (ediabas != null)
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Stop comm thread");
            }
            if (commThread != null)
            {
                lock (commThreadLock)
                {
                    commThreadReqCount++;
                    commThreadCommand = CommThreadCommands.Exit;
                }
                commThreadReqEvent.Set();
                commThread.Join();
                commThread = null;
            }
        }

        private void CommThreadFunc()
        {
            long lastReqCount = -1;
            bool bExitThread = false;
            for (; ; )
            {
                commThreadReqEvent.WaitOne(10, false);

                uint reqCount;
                CommThreadCommands command;
                int sendLength = 0;
                bool newRequest = false;
                lock (commThreadLock)
                {
                    reqCount = commThreadReqCount;
                    command = commThreadCommand;
                    if (lastReqCount != reqCount)
                    {
                        newRequest = true;
                        Array.Copy(this.sendBuffer, this.sendBufferThread, this.sendBufferLength);
                        sendLength = this.sendBufferLength;
                    }
                }

                switch (command)
                {
                    case CommThreadCommands.Idle:
                        break;

                    case CommThreadCommands.IdleTransmit:
                        {
                            EdiabasNet.ErrorCodes errorCode = OBDIdleTrans();
                            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                            {
                                lock (commThreadLock)
                                {
                                    if (commThreadCommand == CommThreadCommands.IdleTransmit)
                                    {
                                        commThreadCommand = CommThreadCommands.Idle;
                                    }
                                }
                            }
                            break;
                        }
                }

                if (!newRequest)
                {
                    continue;
                }
                lastReqCount = reqCount;

                int recLength = -1;
                switch (command)
                {
                    case CommThreadCommands.SingleTransmit:
                        {
                            this.recErrorCode = OBDTrans(this.sendBufferThread, sendLength, ref this.recBufferThread, out recLength);
                            command = CommThreadCommands.Idle;
                            if (this.parIdleFunc != null)
                            {
                                command = CommThreadCommands.IdleTransmit;
                            }
                            break;
                        }

                    case CommThreadCommands.Exit:
                        OBDFinishTrans();
                        command = CommThreadCommands.Idle;
                        bExitThread = true;
                        break;
                }
                lock (commThreadLock)
                {
                    if (recLength > 0)
                    {
                        Array.Copy(this.recBufferThread, this.recBuffer, recLength);
                        this.recBufferLength = recLength;
                    }
                    commThreadCommand = command;
                    commThreadResCount = reqCount;
                }
                commThreadResEvent.Set();
                if (bExitThread)
                {
                    break;
                }
            }
        }

        protected bool SendData(byte[] sendData, int length, bool setDtr, int interbyteTime)
        {
            if (length <= 0)
            {
                return true;
            }
            try
            {
                if (interbyteTime > 0)
                {
                    int bitCount = (serialPort.Parity == Parity.None) ? (serialPort.DataBits + 2) : (serialPort.DataBits + 3);
                    double byteTime = 1.0d / serialPort.BaudRate * 1000 * bitCount;
                    long interbyteTicks = (long)((interbyteTime + byteTime) * tickResolMs);

                    if (this.useExtInterfaceFunc)
                    {
                        if (!InterfacePurgeInBufferFuncUse())
                        {
                            return false;
                        }
                    }
                    else
                    {
                        serialPort.DiscardInBuffer();
                    }

                    if (setDtr)
                    {
                        if (!SetDtrSignal(true))
                        {
                            return false;
                        }
                    }
                    for (int i = 0; i < length; i++)
                    {
                        this.sendBufferInternal[0] = sendData[i];
                        bool dtrHandling = ((i + 1) < length) ? false : setDtr;
                        long startTime = Stopwatch.GetTimestamp();
                        if (!SendData(this.sendBufferInternal, 1, dtrHandling, true))
                        {
                            return false;
                        }
                        while ((Stopwatch.GetTimestamp() - startTime) < interbyteTicks)
                        {
                        }
                    }
                }
                else
                {
                    if (!SendData(sendData, length, setDtr))
                    {
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

        protected bool SendData(byte[] sendData, int length, bool setDtr)
        {
            return SendData(sendData, length, setDtr, false);
        }

        protected bool SendData(byte[] sendData, int length, bool setDtr, bool keepInBuffer)
        {
            if (length <= 0)
            {
                return true;
            }
            if (this.useExtInterfaceFunc)
            {
                if (!keepInBuffer)
                {
                    if (!InterfacePurgeInBufferFuncUse())
                    {
                        return false;
                    }
                }
                return InterfaceSendDataFuncUse(sendData, length, setDtr, this.dtrTimeCorrFtdi);
            }
            try
            {
                int bitCount = (serialPort.Parity == Parity.None) ? (serialPort.DataBits + 2) : (serialPort.DataBits + 3);
                double byteTime = 1.0d / serialPort.BaudRate * 1000 * bitCount;
                if (setDtr)
                {
                    long waitTime = (long)((this.dtrTimeCorrCom + byteTime * length) * tickResolMs);
                    if (!keepInBuffer)
                    {
                        serialPort.DiscardInBuffer();
                    }
                    serialPort.DtrEnable = true;
                    long startTime = Stopwatch.GetTimestamp();
                    serialPort.Write(sendData, 0, length);
                    while ((Stopwatch.GetTimestamp() - startTime) < waitTime)
                    {
                    }
                    serialPort.DtrEnable = false;
                }
                else
                {
                    long waitTime = (long)(byteTime * length);
                    if (!keepInBuffer)
                    {
                        serialPort.DiscardInBuffer();
                    }
                    serialPort.Write(sendData, 0, length);
                    if (waitTime > 10)
                    {
                        Thread.Sleep((int)waitTime);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, bool logResponse)
        {
            if (length <= 0)
            {
                return true;
            }
            // add extra delay for internal signal transitions
            timeout += this.addRecTimeout;
            timeoutTelEnd += this.addRecTimeout;
            if (this.useExtInterfaceFunc)
            {
                return InterfaceReceiveDataFuncUse(receiveData, offset, length, timeout, timeoutTelEnd, logResponse ? ediabas : null);
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
                    commReceiveEvent.WaitOne(1, false);
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
                    commReceiveEvent.WaitOne(1, false);
                }
                stopWatch.Stop();
                if (logResponse)
                {
                    ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, offset, recLen, "Rec ");
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

        protected bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd)
        {
            return ReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, false);
        }

        protected bool SetDtrSignal(bool value)
        {
            try
            {
                if (this.useExtInterfaceFunc)
                {
                    if (!InterfaceSetDtrFuncUse(value))
                    {
                        return false;
                    }
                }
                else
                {
                    serialPort.DtrEnable = value;
                }
            }
            catch (Exception)
            {
            }
            return true;
        }

        protected bool SendWakeFastInit(bool setDtr)
        {
            if (this.useExtInterfaceFunc)
            {
                if (setDtr) InterfaceSetDtrFuncUse(true);
                long startTime = Stopwatch.GetTimestamp();
                InterfaceSetBreakFuncUse(true);
                while ((Stopwatch.GetTimestamp() - startTime) < 25 * tickResolMs)
                {
                }
                InterfaceSetBreakFuncUse(false);
                while ((Stopwatch.GetTimestamp() - startTime) < 50 * tickResolMs)
                {
                }
                if (setDtr) InterfaceSetDtrFuncUse(false);
            }
            else
            {
                try
                {
                    if (setDtr) serialPort.DtrEnable = true;
                    long startTime = Stopwatch.GetTimestamp();
                    serialPort.BreakState = true;
                    while ((Stopwatch.GetTimestamp() - startTime) < 25 * tickResolMs)
                    {
                    }
                    serialPort.BreakState = false;
                    while ((Stopwatch.GetTimestamp() - startTime) < 50 * tickResolMs)
                    {
                    }
                    if (setDtr) serialPort.DtrEnable = false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        protected bool SendWakeAddress5Baud(byte value)
        {
            if (this.useExtInterfaceFunc)
            {
                InterfacePurgeInBufferFuncUse();
                long startTime = Stopwatch.GetTimestamp();
                InterfaceSetBreakFuncUse(true);    // start bit
                Thread.Sleep(180);
                while ((Stopwatch.GetTimestamp() - startTime) < 200 * tickResolMs)
                {
                }
                for (int i = 0; i < 8; i++)
                {
                    InterfaceSetBreakFuncUse((value & (1 << i)) == 0);
                    Thread.Sleep(180);
                    while ((Stopwatch.GetTimestamp() - startTime) < 200 * (i + 2) * tickResolMs)
                    {
                    }
                }
                InterfaceSetBreakFuncUse(false);   // stop bit
                Thread.Sleep(200);
            }
            else
            {
                try
                {
                    serialPort.DiscardInBuffer();
                    long startTime = Stopwatch.GetTimestamp();
                    serialPort.BreakState = true;  // start bit
                    Thread.Sleep(180);
                    while ((Stopwatch.GetTimestamp() - startTime) < 200 * tickResolMs)
                    {
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        serialPort.BreakState = (value & (1 << i)) == 0;
                        Thread.Sleep(180);
                        while ((Stopwatch.GetTimestamp() - startTime) < 200 * (i + 2) * tickResolMs)
                        {
                        }
                    }
                    serialPort.BreakState = false; // stop bit
                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        protected EdiabasNet.ErrorCodes OBDTrans(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            if (!this.useExtInterfaceFunc)
            {
                if (!serialPort.IsOpen)
                {
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
                }
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

        protected EdiabasNet.ErrorCodes OBDIdleTrans()
        {
            if (this.parIdleFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;
            }

            return this.parIdleFunc();
        }

        protected EdiabasNet.ErrorCodes OBDFinishTrans()
        {
            if (this.parFinishFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;
            }

            return this.parFinishFunc();
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
                if (!this.parChecksumByUser)
                {
                    sendData[sendLength] = CalcChecksumBmwFast(sendData, sendLength);
                }
                sendLength++;
                if (enableLogging) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, sendLength, "Send");

                while ((Stopwatch.GetTimestamp() - this.lastResponseTick) < this.parRegenTime * tickResolMs)
                {
                    Thread.Sleep(1);
                }
                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!SendData(sendData, sendLength, this.parSendSetDtr, this.parInterbyteTime))
                {
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (adapterEcho)
                {
                    // remove remote echo
                    if (!ReceiveData(receiveData, 0, sendLength, echoTimeout, this.parTimeoutTelEnd))
                    {
                        if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No echo received");
                        ReceiveData(receiveData, 0, receiveData.Length, echoTimeout, this.parTimeoutTelEnd, enableLogging);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    if (enableLogging) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, sendLength, "Echo");
                    for (int i = 0; i < sendLength; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Echo incorrect");
                            ReceiveData(receiveData, 0, receiveData.Length, this.parTimeoutStd, this.parTimeoutTelEnd, enableLogging);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                        }
                    }
                }
            }

            int timeout = this.parTimeoutStd;
            for (int retry = 0; retry <= this.parRetryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout, this.parTimeoutTelEnd))
                {
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if ((receiveData[0] & 0xC0) != 0x80)
                {
                    if (enableLogging) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, 4, "Head");
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Invalid header");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, this.parTimeoutTelEnd, enableLogging);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthBmwFast(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, this.parTimeoutTelEnd, this.parTimeoutTelEnd))
                {
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, recLength + 1, "Resp");
                if (!this.parChecksumNoCheck)
                {
                    if (CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                    {
                        if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Checksum incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeout, this.parTimeoutTelEnd, enableLogging);
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
                    if (enableLogging) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** NR 0x78");
                    timeout = this.parTimeoutNR;
                }
                else
                {
                    break;
                }
            }

            this.lastResponseTick = Stopwatch.GetTimestamp();
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

        private EdiabasNet.ErrorCodes TransKwp2000Bmw(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            EdiabasNet.ErrorCodes errorCode;
            if (!this.ecuConnected)
            {
                while ((Stopwatch.GetTimestamp() - this.lastCommTick) < this.parTimeoutStd * tickResolMs)
                {
                    Thread.Sleep(10);
                }
                if (!SendWakeFastInit(this.parSendSetDtr))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending wake fast init failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                this.lastCommTick = Stopwatch.GetTimestamp();
                if (this.parStartCommTelLen > 0)
                {
                    errorCode = TransKwp2000(this.parStartCommTel, this.parStartCommTelLen, ref receiveData, out receiveLength, true);
                    if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending start communication failed");
                        return errorCode;
                    }
                }
                this.lastCommTick = Stopwatch.GetTimestamp();
                this.ecuConnected = true;
            }
            errorCode = TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, true);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                this.ecuConnected = false;
                return errorCode;
            }

            this.lastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleKwp2000Bmw()
        {
            if (!this.ecuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            while ((Stopwatch.GetTimestamp() - this.lastCommTick) < this.parTesterPresentTime * tickResolMs)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            if (this.parTesterPresentTelLen > 0)
            {
                int receiveLength;
                EdiabasNet.ErrorCodes errorCode = TransKwp2000(this.parTesterPresentTel, this.parTesterPresentTelLen, ref this.iso9141Buffer, out receiveLength, false);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    this.ecuConnected = false;
                    return errorCode;
                }
            }

            this.lastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes TransKwp2000S(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                int sendLength = TelLengthKwp2000S(sendData);
                if (!this.parChecksumByUser)
                {
                    sendData[sendLength] = CalcChecksumXor(sendData, sendLength);
                }
                sendLength++;
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, sendLength, "Send");

                while ((Stopwatch.GetTimestamp() - this.lastResponseTick) < this.parRegenTime * tickResolMs)
                {
                    Thread.Sleep(1);
                }
                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!SendData(sendData, sendLength, this.parSendSetDtr, this.parInterbyteTime))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (adapterEcho)
                {
                    // remove remote echo
                    if (!ReceiveData(receiveData, 0, sendLength, echoTimeout, this.parTimeoutTelEnd))
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No echo received");
                        ReceiveData(receiveData, 0, receiveData.Length, echoTimeout, this.parTimeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, sendLength, "Echo");
                    for (int i = 0; i < sendLength; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Echo incorrect");
                            ReceiveData(receiveData, 0, receiveData.Length, this.parTimeoutStd, this.parTimeoutTelEnd, true);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                        }
                    }
                }
            }

            int timeout = this.parTimeoutStd;
            for (int retry = 0; retry <= this.parRetryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout, this.parTimeoutTelEnd))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthKwp2000S(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, this.parTimeoutTelEnd, this.parTimeoutTelEnd))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, recLength + 1, "Resp");
                if (!this.parChecksumNoCheck)
                {
                    if (CalcChecksumXor(receiveData, recLength) != receiveData[recLength])
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Checksum incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeout, this.parTimeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }

                int dataLen = receiveData[3];
                int dataStart = 4;
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** NR 0x78");
                    timeout = this.parTimeoutNR;
                }
                else
                {
                    break;
                }
            }

            this.lastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = TelLengthKwp2000S(receiveData) + 1;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length without checksum
        private int TelLengthKwp2000S(byte[] dataBuffer)
        {
            int telLength = dataBuffer[3] + 4;
            return telLength;
        }

        private EdiabasNet.ErrorCodes TransDS2(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                int sendLength = sendDataLength;
                if (!this.parChecksumByUser)
                {
                    sendData[sendLength] = CalcChecksumXor(sendData, sendLength);
                    sendLength++;
                }
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, sendLength, "Send");

                while ((Stopwatch.GetTimestamp() - this.lastResponseTick) < this.parRegenTime * tickResolMs)
                {
                    Thread.Sleep(1);
                }
                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!SendData(sendData, sendLength, this.parSendSetDtr, this.parInterbyteTime))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (adapterEcho)
                {
                    // remove remote echo
                    if (!ReceiveData(receiveData, 0, sendLength, echoTimeout, this.parTimeoutTelEnd))
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No echo received");
                        ReceiveData(receiveData, 0, receiveData.Length, echoTimeout, this.parTimeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, sendLength, "Echo");
                    for (int i = 0; i < sendLength; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Echo incorrect");
                            ReceiveData(receiveData, 0, receiveData.Length, this.parTimeoutStd, this.parTimeoutTelEnd, true);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                        }
                    }
                }
            }

            // header byte
            int headerLen = commAnswerLen[0];
            if (headerLen < 0)
            {
                headerLen = (-headerLen) + 1;
            }
            if (headerLen == 0)
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Header lenght zero");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }
            if (!ReceiveData(receiveData, 0, headerLen, this.parTimeoutStd, this.parTimeoutTelEnd))
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No header received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            int recLength = TelLengthDS2(receiveData);
            if (recLength == 0)
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Receive lenght zero");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!ReceiveData(receiveData, headerLen, recLength - headerLen, this.parTimeoutTelEnd, this.parTimeoutTelEnd))
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No tail received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, recLength, "Resp");
            if (!this.parChecksumNoCheck)
            {
                if (CalcChecksumXor(receiveData, recLength - 1) != receiveData[recLength - 1])
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Checksum incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, this.parTimeoutStd, this.parTimeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }

            this.lastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = recLength;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length with checksum
        private int TelLengthDS2(byte[] dataBuffer)
        {
            int telLength = commAnswerLen[0];   // >0 fix length
            if (telLength < 0)
            {   // offset in buffer
                int offset = (-telLength);
                if (dataBuffer.Length < offset)
                {
                    return 0;
                }
                telLength = dataBuffer[offset] + commAnswerLen[1];  // + answer offset
            }
            return telLength;
        }

        private byte CalcChecksumXor(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum ^= data[i];
            }
            return sum;
        }

        private EdiabasNet.ErrorCodes TransIso9141(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            EdiabasNet.ErrorCodes errorCode;
            List<byte> keyBytesList = null;

            if (sendDataLength > iso9141Buffer.Length)
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Invalid send data length");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }

            if (!this.ecuConnected)
            {
                keyBytes = byteArray0;
                keyBytesList = new List<byte>();
                while ((Stopwatch.GetTimestamp() - this.lastCommTick) < 2600 * tickResolMs)
                {
                    Thread.Sleep(10);
                }

                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Establish connection");
                if (this.useExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(9600, 8, Parity.None, this.parAllowBitBang) != EdFtdiInterface.ErrorResult.NO_ERROR)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }
                else
                {
                    try
                    {
                        serialPort.BaudRate = 9600;
                    }
                    catch (Exception)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }

                if (!SetDtrSignal(false))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set DTR failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }

                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!SendWakeAddress5Baud(this.parWakeAddress))
                {
                    this.lastCommTick = Stopwatch.GetTimestamp();
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(iso9141Buffer, 0, 1, this.parTimeoutStd, this.parTimeoutStd))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No wake response");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Baud rate byte: {0:X02}", iso9141Buffer[0]);
                if (iso9141Buffer[0] == 0x55)
                {
                    ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Baud rate 9.6k detected");
                }
                else
                {   // baud rate different
                    if ((iso9141Buffer[0] & 0x87) != 0x85)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Invalid baud rate");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Baud rate 10.4k detected");
                    if (this.useExtInterfaceFunc)
                    {
                        if (InterfaceSetConfigFuncUse(10400, 8, Parity.None, this.parAllowBitBang) != EdFtdiInterface.ErrorResult.NO_ERROR)
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
                    }
                    else
                    {
                        try
                        {
                            serialPort.BaudRate = 10400;
                        }
                        catch (Exception)
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
                    }
                }

                this.ecuConnected = true;
                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(iso9141Buffer, 0, 2, 500, 500))
                {
                    this.ecuConnected = false;
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No key bytes received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                keyBytesList.Add((byte)(iso9141Buffer[0] & 0x7F));
                keyBytesList.Add((byte)(iso9141Buffer[1] & 0x7F));
                keyBytesList.Add(0x09);
                keyBytesList.Add(0x03);

                this.lastCommTick = Stopwatch.GetTimestamp();
                ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Key bytes: {0:X02} {1:X02}", iso9141Buffer[0], iso9141Buffer[1]);
                iso9141Buffer[0] = (byte)(~iso9141Buffer[1]);
                Thread.Sleep(10);
                if (!SendData(iso9141Buffer, 1, this.parSendSetDtr))
                {
                    this.ecuConnected = false;
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending key byte response failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                this.blockCounter = 1;

                Thread.Sleep(10);
                errorCode = ReceiveIso9141Block(iso9141Buffer, true);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    this.ecuConnected = false;
                    return errorCode;
                }
                this.lastIso9141Cmd = iso9141Buffer[2];
                this.blockCounter++;
                if (this.lastIso9141Cmd != 0x09)
                {
                    // store key bytes
                    int dataLen = iso9141Buffer[0] + 1;
                    for (int i = 0; i < dataLen; i++)
                    {
                        keyBytesList.Add(iso9141Buffer[i]);
                    }
                }
            }

            ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, sendDataLength, "Request");
            int recLength = 0;
            int recBlocks = 0;
            int maxRecBlocks = commAnswerLen[0];

            int waitToSendCount = 0;
            bool waitToSend = true;
            bool transmitDone = false;
            for (; ; )
            {
                bool sendDataValid = false;
                if (this.lastIso9141Cmd == 0x09)
                {   // ack
                    if (waitToSend)
                    {
                        waitToSend = false;
                        if (sendDataLength > 0)
                        {
                            Array.Copy(sendData, iso9141Buffer, sendDataLength);
                            sendDataValid = true;
                        }
                        else
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Receive ID finished");
                            transmitDone = true;
                        }
                    }
                    else
                    {
                        if (recBlocks > 0)
                        {
                            // at least one block received
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Transmission finished");
                            transmitDone = true;
                        }
                    }
                }

                if (waitToSend)
                {
                    waitToSendCount++;
                    if (waitToSendCount > 1000)
                    {
                        this.ecuConnected = false;
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Wait for first ACK failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }
                if (sendDataValid)
                {
#if false
                    // EDIABAS seems not to respect the regeneration time
                    while ((Stopwatch.GetTimestamp() - this.lastResponseTick) < this.parRegenTime * tickResolMs)
                    {
                        Thread.Sleep(10);
                    }
#endif
                }
                else
                {
                    iso9141Buffer[0] = 0x03;    // block length
                    iso9141Buffer[2] = 0x09;    // ACK
                }

                Thread.Sleep(50);

                this.lastCommTick = Stopwatch.GetTimestamp();
                iso9141Buffer[1] = this.blockCounter++;
                errorCode = SendIso9141Block(iso9141Buffer, true);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    this.ecuConnected = false;
                    return errorCode;
                }

                this.lastCommTick = Stopwatch.GetTimestamp();
                errorCode = ReceiveIso9141Block(iso9141Buffer, true);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    this.ecuConnected = false;
                    return errorCode;
                }
                this.blockCounter++;
                this.lastIso9141Cmd = iso9141Buffer[2];

                if (!waitToSend)
                {   // store received data
                    if ((recBlocks == 0) || (this.lastIso9141Cmd != 0x09))
                    {
                        int blockLen = iso9141Buffer[0];
                        if (recLength + blockLen > receiveData.Length)
                        {
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Receive buffer overflow, ignore data");
                            transmitDone = true;
                        }
                        Array.Copy(iso9141Buffer, 0, receiveData, recLength, blockLen);
                        recLength += blockLen;
                        recBlocks++;
                        if (recBlocks >= maxRecBlocks)
                        {   // all blocks received
                            ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "All blocks received");
                            transmitDone = true;
                        }
                    }
                }
                else
                {
                    if ((keyBytesList != null) && (this.lastIso9141Cmd != 0x09))
                    {   // store key bytes
                        int dataLen = iso9141Buffer[0] + 1;
                        for (int i = 0; i < dataLen; i++)
                        {
                            keyBytesList.Add(iso9141Buffer[i]);
                        }
                    }
                }
                if (transmitDone)
                {
                    break;
                }
            }

            if (keyBytesList != null)
            {
                this.keyBytes = keyBytesList.ToArray();
                ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, this.keyBytes, 0, this.keyBytes.Length, "ID bytes");
            }

            this.lastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = recLength;
            ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, 0, receiveLength, "Answer");
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleIso9141()
        {
            if (!this.ecuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            while ((Stopwatch.GetTimestamp() - this.lastCommTick) < 500 * tickResolMs)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            EdiabasNet.ErrorCodes errorCode;

            iso9141Buffer[0] = 0x03;    // block length
            iso9141Buffer[2] = 0x09;    // ACK

            this.lastCommTick = Stopwatch.GetTimestamp();
            iso9141Buffer[1] = this.blockCounter++;
            errorCode = SendIso9141Block(iso9141Buffer, false);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                this.ecuConnected = false;
                return errorCode;
            }

            this.lastCommTick = Stopwatch.GetTimestamp();
            errorCode = ReceiveIso9141Block(iso9141Buffer, false);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                this.ecuConnected = false;
                return errorCode;
            }
            this.blockCounter++;
            this.lastIso9141Cmd = iso9141Buffer[2];

            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes SendIso9141Block(byte[] sendData, bool enableLog)
        {
            int blockLen = sendData[0];
            if (enableLog) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, sendData, 0, blockLen, "Send");
            for (int i = 0; i < blockLen; i++)
            {
                iso9141BlockBuffer[0] = sendData[i];
                if (!SendData(iso9141BlockBuffer, 1, this.parSendSetDtr))
                {
                    if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (!ReceiveData(iso9141BlockBuffer, 0, 1, this.parTimeoutTelEnd, this.parTimeoutTelEnd))
                {
                    if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No data ack received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (enableLog) ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.INFO, "(A): {0:X02}", (byte)(~iso9141BlockBuffer[0]));
                if ((byte)(~iso9141BlockBuffer[0]) != sendData[i])
                {
                    if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Response invalid");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            iso9141BlockBuffer[0] = 0x03;   // block end
            if (!SendData(iso9141BlockBuffer, 1, this.parSendSetDtr))
            {
                if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes ReceiveIso9141Block(byte[] recData, bool enableLog)
        {
            // block length
            if (!ReceiveData(recData, 0, 1, this.parTimeoutTelEnd, this.parTimeoutTelEnd))
            {
                if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No block length received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (enableLog) ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.INFO, "(R): {0:X02}", recData[0]);

            int blockLen = recData[0];
            for (int i = 0; i < blockLen; i++)
            {
                iso9141BlockBuffer[0] = (byte)(~recData[i]);
                if (!SendData(iso9141BlockBuffer, 1, this.parSendSetDtr))
                {
                    if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (!ReceiveData(recData, i + 1, 1, this.parTimeoutTelEnd, this.parTimeoutTelEnd))
                {
                    if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No block data received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (enableLog) ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.INFO, "(R): {0:X02}", recData[i + 1]);
            }
            if (enableLog) ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, recData, 0, blockLen, "Resp");
            if (recData[blockLen] != 0x03)
            {
                if (enableLog) ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Block end invalid");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes TransConcept3(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            List<byte> keyBytesList = null;

            if (!this.ecuConnected)
            {
                keyBytes = byteArray0;
                keyBytesList = new List<byte>();
                while ((Stopwatch.GetTimestamp() - this.lastCommTick) < 2600 * tickResolMs)
                {
                    Thread.Sleep(10);
                }

                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "Establish connection");
                if (this.useExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(9600, 8, Parity.None, this.parAllowBitBang) != EdFtdiInterface.ErrorResult.NO_ERROR)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }
                else
                {
                    try
                    {
                        serialPort.BaudRate = 9600;
                        serialPort.Parity = Parity.None;
                    }
                    catch (Exception)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }

                if (!SetDtrSignal(false))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set DTR failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }

                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!SendWakeAddress5Baud(this.parWakeAddress))
                {
                    this.lastCommTick = Stopwatch.GetTimestamp();
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(iso9141Buffer, 0, 1, this.parTimeoutStd, this.parTimeoutStd))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No wake response");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Baud rate byte: {0:X02}", iso9141Buffer[0]);
                if (iso9141Buffer[0] == 0x55)
                {
                    ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Baud rate 9.6k detected");
                }
                else
                {   // baud rate different
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Invalid baud rate");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                this.ecuConnected = true;
                this.lastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(iso9141Buffer, 0, 3, 200, 200))
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No key bytes received");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                keyBytesList.Add((byte)(iso9141Buffer[0] & 0x7F));
                keyBytesList.Add((byte)(iso9141Buffer[1] & 0x7F));
                keyBytesList.Add((byte)(iso9141Buffer[2] & 0x7F));
                keyBytesList.Add(0x09);
                keyBytesList.Add(0x03);

                this.lastCommTick = Stopwatch.GetTimestamp();
                ediabas.LogFormat(EdiabasNet.ED_LOG_LEVEL.IFH, "Key bytes: {0:X02} {1:X02} {2:X02}", iso9141Buffer[0], iso9141Buffer[1], iso9141Buffer[2]);
                if (this.useExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(9600, 8, Parity.Even, this.parAllowBitBang) != EdFtdiInterface.ErrorResult.NO_ERROR)
                    {
                        ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                        FinishConcept3();
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }
                else
                {
                    serialPort.Parity = Parity.Even;
                }
            }
            // receive a data block
            if (!ReceiveData(iso9141Buffer, 0, 1, this.parTimeoutStd, this.parTimeoutStd))
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** No header byte");
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            int recLength = 1;
            for (; ; )
            {
                if (recLength >= iso9141Buffer.Length)
                {   // buffer overflow
                    break;
                }
                if (!ReceiveData(iso9141Buffer, recLength, 1, 20, 20))
                {   // last byte receive
                    break;
                }
                recLength++;
            }
            ediabas.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, iso9141Buffer, 0, recLength, "Rec");
            if (commAnswerLen[0] > 0 && recLength != commAnswerLen[0])
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Invalid response length");
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!this.parChecksumNoCheck)
            {
                if (CalcChecksumXor(iso9141Buffer, recLength - 1) != iso9141Buffer[recLength - 1])
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Checksum incorrect");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            Array.Copy(iso9141Buffer, receiveData, recLength);
            receiveLength = recLength;

            if (keyBytesList != null)
            {
                for (int i = 0; i < recLength; i++)
                {
                    keyBytesList.Add(iso9141Buffer[i]);
                }
                keyBytesList.Add(0x03);
                this.keyBytes = keyBytesList.ToArray();
            }

            this.lastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleConcept3()
        {
            if (!this.ecuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            // receive a data block
            if (!ReceiveData(iso9141Buffer, 0, 1, this.parTimeoutStd, this.parTimeoutStd))
            {
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            int recLength = 1;
            for (; ; )
            {
                if (recLength >= iso9141Buffer.Length)
                {   // buffer overflow
                    break;
                }
                if (!ReceiveData(iso9141Buffer, recLength, 1, 20, 20))
                {   // last byte receive
                    break;
                }
                recLength++;
            }
            if (commAnswerLen[0] > 0 && recLength != commAnswerLen[0])
            {
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!this.parChecksumNoCheck)
            {
                if (CalcChecksumXor(iso9141Buffer, recLength - 1) != iso9141Buffer[recLength - 1])
                {
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            this.lastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes FinishConcept3()
        {
            this.ecuConnected = false;
            if (this.useExtInterfaceFunc)
            {
                if (InterfaceSetConfigFuncUse(10400, 8, Parity.None, this.parAllowBitBang) != EdFtdiInterface.ErrorResult.NO_ERROR)
                {
                    ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Set baud rate failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }
            }
            else
            {
                serialPort.BaudRate = 10400;
                serialPort.Parity = Parity.None;
            }

            Thread.Sleep(10);
            this.lastCommTick = Stopwatch.GetTimestamp();
            iso9141Buffer[0] = 0xFF;
            if (!SendData(iso9141Buffer, 1, false))
            {
                ediabas.LogString(EdiabasNet.ED_LOG_LEVEL.IFH, "*** Sending stop byte failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
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
                    serialPort.Dispose();
                }
                InterfaceUnlock();

                // Note disposing has been done.
                disposed = true;
            }
        }
    }
}
