#if !Android
#define USE_SERIAL_PORT
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace EdiabasLib
{
    public class EdInterfaceObd : EdInterfaceBase
    {
        public enum SerialParity
        {
            None = 0,
            Odd = 1,
            Even = 2,
            Mark = 3,
            Space = 4,
        }

        protected enum CommThreadCommands
        {
            Idle,               // do nothing
            SingleTransmit,     // single data transmission
            IdleTransmit,       // idle data transmission
            Exit,               // exit thread
        }

        public enum InterfaceErrorResult
        {
            NoError = 0,
            ConfigError,
            DeviceTypeError,
        }

        public delegate bool InterfaceConnectDelegate(string port, object parameter);
        public delegate bool InterfaceDisconnectDelegate();
        public delegate InterfaceErrorResult InterfaceSetConfigDelegate(int baudRate, int dataBits, SerialParity parity, bool allowBitBang);
        public delegate bool InterfaceSetDtrDelegate(bool dtr);
        public delegate bool InterfaceSetRtsDelegate(bool rts);
        public delegate bool InterfaceGetDsrDelegate(out bool dsr);
        public delegate bool InterfaceSetBreakDelegate(bool enable);
        public delegate bool InterfaceSetInterByteTimeDelegate(int time);
        public delegate bool InterfacePurgeInBufferDelegate();
        public delegate bool InterfaceAdapterEchoDelegate();
        public delegate bool InterfaceSendDataDelegate(byte[] sendData, int length, bool setDtr, double dtrTimeCorr);
        public delegate bool InterfaceReceiveDataDelegate(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog);
        public delegate bool InterfaceSendPulseDelegate(UInt64 dataBits, int length, int pulseWidth, bool setDtr);
        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        protected delegate EdiabasNet.ErrorCodes IdleDelegate();
        protected delegate EdiabasNet.ErrorCodes FinishDelegate();

        private bool _disposed;
        protected const int TransBufferSize = 1024; // transmit buffer size
        protected static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly byte[] ByteArray0 = new byte[0];
        protected static readonly long TickResolMs = Stopwatch.Frequency / 1000;
#if USE_SERIAL_PORT
        protected static System.IO.Ports.SerialPort SerialPort;
        protected static AutoResetEvent CommReceiveEvent;
#endif
        protected static AutoResetEvent CommThreadReqEvent;
        protected static AutoResetEvent CommThreadResEvent;
        protected static Thread CommThread;
        protected static object CommThreadLock;
        protected static volatile CommThreadCommands CommThreadCommand;
        protected static volatile uint CommThreadReqCount;
        protected static volatile uint CommThreadResCount;

        protected string ComPortProtected = string.Empty;
        protected double DtrTimeCorrCom = 0.3;
        protected double DtrTimeCorrFtdi = 0.3;
        protected int AddRecTimeout = 20;
        protected bool EnableFtdiBitBang;
        protected bool ConnectedProtected;
        protected const int EchoTimeout = 100;
        protected bool UseExtInterfaceFunc;
        protected InterfaceConnectDelegate InterfaceConnectFuncProtected;
        protected InterfaceConnectDelegate InterfaceConnectFuncInt;
        protected InterfaceDisconnectDelegate InterfaceDisconnectFuncProtected;
        protected InterfaceDisconnectDelegate InterfaceDisconnectFuncInt;
        protected InterfaceSetConfigDelegate InterfaceSetConfigFuncProtected;
        protected InterfaceSetConfigDelegate InterfaceSetConfigFuncInt;
        protected InterfaceSetDtrDelegate InterfaceSetDtrFuncProtected;
        protected InterfaceSetDtrDelegate InterfaceSetDtrFuncInt;
        protected InterfaceSetRtsDelegate InterfaceSetRtsFuncProtected;
        protected InterfaceSetRtsDelegate InterfaceSetRtsFuncInt;
        protected InterfaceGetDsrDelegate InterfaceGetDsrFuncProtected;
        protected InterfaceGetDsrDelegate InterfaceGetDsrFuncInt;
        protected InterfaceSetBreakDelegate InterfaceSetBreakFuncProtected;
        protected InterfaceSetBreakDelegate InterfaceSetBreakFuncInt;
        protected InterfaceSetInterByteTimeDelegate InterfaceSetInterByteTimeFuncProtected;
        protected InterfaceSetInterByteTimeDelegate InterfaceSetInterByteTimeFuncInt;
        protected InterfacePurgeInBufferDelegate InterfacePurgeInBufferFuncProtected;
        protected InterfacePurgeInBufferDelegate InterfacePurgeInBufferFuncInt;
        protected InterfaceAdapterEchoDelegate InterfaceAdapterEchoFuncProtected;
        protected InterfaceAdapterEchoDelegate InterfaceAdapterEchoFuncInt;
        protected InterfaceSendDataDelegate InterfaceSendDataFuncProtected;
        protected InterfaceSendDataDelegate InterfaceSendDataFuncInt;
        protected InterfaceReceiveDataDelegate InterfaceReceiveDataFuncProtected;
        protected InterfaceReceiveDataDelegate InterfaceReceiveDataFuncInt;
        protected InterfaceSendPulseDelegate InterfaceSendPulseFuncProtected;
        protected InterfaceSendPulseDelegate InterfaceSendPulseFuncInt;
        protected Stopwatch StopWatch = new Stopwatch();
        protected byte[] KeyBytesProtected = ByteArray0;
        protected byte[] StateProtected = new byte[2];
        protected byte[] SendBuffer = new byte[TransBufferSize];
        protected byte[] SendBufferThread = new byte[TransBufferSize];
        protected byte[] SendBufferInternal = new byte[1];
        protected volatile int SendBufferLength;
        protected byte[] RecBuffer = new byte[TransBufferSize];
        protected byte[] RecBufferThread = new byte[TransBufferSize];
        protected volatile int RecBufferLength;
        protected volatile EdiabasNet.ErrorCodes RecErrorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        protected byte[] Iso9141Buffer = new byte[256];
        protected byte[] Iso9141BlockBuffer = new byte[1];
        protected Dictionary<byte, int> NrDict = new Dictionary<byte, int>();
        protected bool EcuConnected;
        protected long LastCommTick;
        protected long LastResponseTick;
        protected int CurrentBaudRate;
        protected SerialParity CurrentParity;
        protected int CurrentDataBits;
        protected byte BlockCounter;
        protected byte LastIso9141Cmd;

        protected TransmitDelegate ParTransmitFunc;
        protected IdleDelegate ParIdleFunc;
        protected FinishDelegate ParFinishFunc;
        protected int ParTimeoutStd;
        protected int ParTimeoutTelEnd;
        protected int ParInterbyteTime;
        protected int ParRegenTime;
        protected int ParTimeoutNr;
        protected int ParRetryNr;
        protected byte ParWakeAddress;
        protected int ParTesterPresentTime;
        protected int ParTesterPresentTelLen;
        protected byte[] ParTesterPresentTel = new byte[TransBufferSize];
        protected int ParStartCommTelLen;
        protected byte[] ParStartCommTel = new byte[TransBufferSize];
        protected bool ParChecksumByUser;
        protected bool ParChecksumNoCheck;
        protected bool ParSendSetDtr;
        protected bool ParAllowBitBang;
        protected bool ParHasKeyBytes;
        protected bool ParSupportFrequent;

        public override EdiabasNet Ediabas
        {
            get
            {
                return base.Ediabas;
            }
            set
            {
                base.Ediabas = value;

                string prop = EdiabasProtected.GetConfigProperty("ObdComPort");
                if (prop != null)
                {
                    ComPortProtected = prop;
                }

                prop = EdiabasProtected.GetConfigProperty("ObdDtrTimeCorrCom");
                if (prop != null)
                {
                    DtrTimeCorrCom = EdiabasNet.StringToFloat(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("ObdDtrTimeCorrFtdi");
                if (prop != null)
                {
                    DtrTimeCorrFtdi = EdiabasNet.StringToFloat(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("ObdAddRecTimeout");
                if (prop != null)
                {
                    AddRecTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("ObdFtdiBitBang");
                if (prop != null)
                {
                    EnableFtdiBitBang = EdiabasNet.StringToValue(prop) != 0;
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

                CommParameterProtected = value;
                CommAnswerLenProtected[0] = 0;
                CommAnswerLenProtected[1] = 0;

                ParTransmitFunc = null;
                ParIdleFunc = null;
                ParFinishFunc = null;
                ParTimeoutStd = 0;
                ParTimeoutTelEnd = 0;
                ParInterbyteTime = 0;
                ParRegenTime = 0;
                ParTimeoutNr = 0;
                ParRetryNr = 0;
                ParWakeAddress = 0;
                ParTesterPresentTime = 0;
                ParTesterPresentTelLen = 0;
                ParStartCommTelLen = 0;
                ParChecksumByUser = false;
                ParChecksumNoCheck = false;
                ParSendSetDtr = false;
                ParAllowBitBang = false;
                ParHasKeyBytes = false;
                ParSupportFrequent = false;
                KeyBytesProtected = ByteArray0;
                NrDict.Clear();
                EcuConnected = false;
                // don't init lastCommTick here
                LastResponseTick = DateTime.MinValue.Ticks;
                BlockCounter = 0;
                LastIso9141Cmd = 0x00;

                if (CommParameterProtected == null)
                {   // clear parameter
                    return;
                }
                if (CommParameterProtected.Length < 1)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    return;
                }

                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, CommParameterProtected, 0, CommParameterProtected.Length,
                    string.Format(Culture, "{0} CommParameter Port={1}, CorrCom={2}, CorrFtdi={3}, RecTimeout={4}, BitBang={5}",
                            InterfaceName, ComPortProtected, DtrTimeCorrCom, DtrTimeCorrFtdi, AddRecTimeout, EnableFtdiBitBang));

                int baudRate;
                int dataBits = 8;
                SerialParity parity;
                bool stateDtr = false;
                bool stateRts = false;
                uint concept = CommParameterProtected[0];
                switch (concept)
                {
                    case 0x0001:    // Concept 1
                        if (HasAdapterEcho)
                        {   // only with ADS adapter
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                            return;
                        }
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 10)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[9]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        CommAnswerLenProtected[0] = -2;
                        CommAnswerLenProtected[1] = 0;
                        baudRate = (int)CommParameterProtected[1];
                        parity = SerialParity.Even;
                        ParTransmitFunc = TransDs2;
                        ParTimeoutStd = (int)CommParameterProtected[5];
                        ParRegenTime = (int)CommParameterProtected[6];
                        ParTimeoutTelEnd = (int)CommParameterProtected[7];
                        ParSendSetDtr = false;
                        ParAllowBitBang = false;
                        break;

                    case 0x0002:    // Concept 2 ISO 9141
                        if (HasAdapterEcho)
                        {   // only with ADS adapter
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                            return;
                        }
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 10)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[9]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        CommAnswerLenProtected[0] = 1;
                        CommAnswerLenProtected[1] = 0;
                        baudRate = 9600;
                        parity = SerialParity.None;
                        ParTransmitFunc = TransIso9141;
                        ParIdleFunc = IdleIso9141;
                        ParWakeAddress = (byte)CommParameterProtected[2];
                        ParTimeoutStd = (int)CommParameterProtected[5];
                        ParRegenTime = (int)CommParameterProtected[6];
                        ParTimeoutTelEnd = (int)CommParameterProtected[7];
                        ParSendSetDtr = true;
                        ParAllowBitBang = EnableFtdiBitBang;
                        ParHasKeyBytes = true;
                        break;

                    case 0x0003:    // Concept 3
                        if (HasAdapterEcho)
                        {   // only with ADS adapter
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                            return;
                        }
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 10)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[9]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        CommAnswerLenProtected[0] = 52;
                        CommAnswerLenProtected[1] = 0;
                        baudRate = 9600;
                        parity = SerialParity.None;
                        ParTransmitFunc = TransConcept3;
                        ParIdleFunc = IdleConcept3;
                        ParFinishFunc = FinishConcept3;
                        ParWakeAddress = (byte)CommParameterProtected[2];
                        ParTimeoutStd = (int)CommParameterProtected[5];
                        ParRegenTime = (int)CommParameterProtected[6];
                        ParTimeoutTelEnd = (int)CommParameterProtected[7];
                        ParSendSetDtr = true;
                        ParAllowBitBang = EnableFtdiBitBang;
                        ParHasKeyBytes = true;
                        ParSupportFrequent = true;
                        break;

                    case 0x0005:    // DS1
                    case 0x0006:    // DS2
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 10)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[9]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        CommAnswerLenProtected[0] = -1;
                        CommAnswerLenProtected[1] = 0;
                        baudRate = (int)CommParameterProtected[1];
                        parity = SerialParity.Even;
                        ParTransmitFunc = TransDs2;
                        ParTimeoutStd = (int)CommParameterProtected[5];
                        ParRegenTime = (int)CommParameterProtected[6];
                        ParTimeoutTelEnd = (int)CommParameterProtected[7];
                        ParInterbyteTime = (int)CommParameterProtected[8];
                        ParSendSetDtr = false;
                        if (concept == 6)
                        {   // DS2 uses DTR
                            ParSendSetDtr = !HasAdapterEcho;
                        }
                        ParAllowBitBang = EnableFtdiBitBang && ParSendSetDtr;
                        break;

                    case 0x010C:    // KWP2000 BMW
                        if (CommParameterProtected.Length < 33)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 34)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[33]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        baudRate = (int)CommParameterProtected[1];
                        parity = SerialParity.None;
                        ParTransmitFunc = TransKwp2000Bmw;
                        ParIdleFunc = IdleKwp2000Bmw;
                        ParTimeoutStd = (int)CommParameterProtected[2];
                        ParRegenTime = (int)CommParameterProtected[3];
                        ParTimeoutTelEnd = (int)CommParameterProtected[4];
                        ParInterbyteTime = (int)CommParameterProtected[5];
                        ParTimeoutNr = (int)CommParameterProtected[7];
                        ParRetryNr = (int)CommParameterProtected[6];
                        ParTesterPresentTime = (int)CommParameterProtected[8];
                        ParTesterPresentTelLen = (int)CommParameterProtected[9];
                        if (ParTesterPresentTelLen > 11)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (ParTesterPresentTelLen > 0)
                        {
                            for (int i = 0; i < ParTesterPresentTelLen; i++)
                            {
                                ParTesterPresentTel[i] = (byte)CommParameterProtected[10 + i];
                            }
                        }
                        ParStartCommTelLen = (int)CommParameterProtected[21];
                        if (ParStartCommTelLen > 11)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (ParStartCommTelLen > 0)
                        {
                            for (int i = 0; i < ParStartCommTelLen; i++)
                            {
                                ParStartCommTel[i] = (byte)CommParameterProtected[22 + i];
                            }
                        }
                        ParSendSetDtr = !HasAdapterEcho;
                        ParAllowBitBang = EnableFtdiBitBang && ParSendSetDtr;
                        break;

                    case 0x010D:    // KWP2000*
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 22)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[21]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        baudRate = (int)CommParameterProtected[1];
                        parity = SerialParity.Even;
                        ParTransmitFunc = TransKwp2000S;
                        ParTimeoutStd = (int)CommParameterProtected[2];
                        ParRegenTime = (int)CommParameterProtected[3];
                        ParTimeoutTelEnd = (int)CommParameterProtected[4];
                        ParInterbyteTime = (int)CommParameterProtected[5];
                        ParTimeoutNr = (int)CommParameterProtected[7];
                        ParRetryNr = (int)CommParameterProtected[6];
                        ParSendSetDtr = !HasAdapterEcho;
                        ParAllowBitBang = EnableFtdiBitBang && ParSendSetDtr;
                        break;

                    case 0x010F:    // BMW-FAST
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        if (CommParameterProtected.Length >= 8)
                        {
                            if (!EvalChecksumPar(CommParameterProtected[7]))
                            {
                                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                return;
                            }
                        }
                        baudRate = (int)CommParameterProtected[1];
                        parity = SerialParity.None;
                        stateDtr = HasAdapterEcho;
                        ParTransmitFunc = TransBmwFast;
                        ParTimeoutStd = (int)CommParameterProtected[2];
                        ParRegenTime = (int)CommParameterProtected[3];
                        ParTimeoutTelEnd = (int)CommParameterProtected[4];
                        ParTimeoutNr = (int)CommParameterProtected[6];
                        ParRetryNr = (int)CommParameterProtected[5];
                        ParSendSetDtr = !HasAdapterEcho;
                        ParAllowBitBang = false;
                        break;

                    case 0x0110:    // D-CAN
                        if (CommParameterProtected.Length < 30)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        baudRate = 115200;
                        parity = SerialParity.None;
                        stateDtr = HasAdapterEcho;
                        ParTransmitFunc = TransBmwFast;
                        ParTimeoutStd = (int)CommParameterProtected[7];
                        ParTimeoutTelEnd = 10;
                        ParRegenTime = (int)CommParameterProtected[8];
                        ParTimeoutNr = (int)CommParameterProtected[9];
                        ParRetryNr = (int)CommParameterProtected[10];
                        ParSendSetDtr = !HasAdapterEcho;
                        ParAllowBitBang = false;
                        break;

                    default:
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
                        return;
                }

                StartCommThread();

                if (UseExtInterfaceFunc)
                {
                    InterfaceErrorResult ftdiResult = InterfaceSetConfigFuncUse(baudRate, dataBits, parity, ParAllowBitBang);
                    switch (ftdiResult)
                    {
                        case InterfaceErrorResult.NoError:
                            break;

                        case InterfaceErrorResult.DeviceTypeError:
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Bit bang mode is only working with FT232H devices!");
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0063);
                            return;

                        default:
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                    }
                    if (!InterfaceSetDtrFuncUse(stateDtr))
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return;
                    }
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!InterfaceSetRtsFuncUse(stateRts))
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                        return;
                    }
                }
                else
                {
#if USE_SERIAL_PORT
                    try
                    {
                        if (SerialPort.BaudRate != baudRate)
                        {
                            SerialPort.BaudRate = baudRate;
                        }
                        if (SerialPort.DataBits != dataBits)
                        {
                            SerialPort.DataBits = dataBits;
                        }

                        System.IO.Ports.Parity newParity = System.IO.Ports.Parity.None;
                        switch (parity)
                        {
                            case SerialParity.None:
                                newParity = System.IO.Ports.Parity.None;
                                break;

                            case SerialParity.Odd:
                                newParity = System.IO.Ports.Parity.Odd;
                                break;

                            case SerialParity.Even:
                                newParity = System.IO.Ports.Parity.Even;
                                break;

                            case SerialParity.Mark:
                                newParity = System.IO.Ports.Parity.Mark;
                                break;

                            case SerialParity.Space:
                                newParity = System.IO.Ports.Parity.Space;
                                break;
                        }
                        if (SerialPort.Parity != newParity)
                        {
                            SerialPort.Parity = newParity;
                        }
                        if (SerialPort.DtrEnable != stateDtr)
                        {
                            SerialPort.DtrEnable = stateDtr;
                        }
                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                        if (SerialPort.RtsEnable != stateRts)
                        {
                            SerialPort.RtsEnable = stateRts;
                        }
                        // ReSharper restore ConditionIsAlwaysTrueOrFalse
                    }
                    catch (Exception)
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    }
#else
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
#endif
                }
                CurrentBaudRate = baudRate;
                CurrentParity = parity;
                CurrentDataBits = dataBits;
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
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return null;
                }
                if (CommParameterProtected != null && ParHasKeyBytes)
                {
                    if (EcuConnected)
                    {
                        return KeyBytesProtected;
                    }
                    // start transmission
                    if (CommThread == null)
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0030);
                        return ByteArray0;
                    }
                    lock (CommThreadLock)
                    {
                        CommThreadReqCount++;
                        CommThreadCommand = CommThreadCommands.SingleTransmit;
                    }
                    CommThreadReqEvent.Set();

                    for (; ; )
                    {
                        lock (CommThreadLock)
                        {
                            if (CommThreadResCount == CommThreadReqCount)
                            {
                                break;
                            }
                        }
                        CommThreadResEvent.WaitOne(10, false);
                    }
                    if (RecErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        EdiabasProtected.SetError(RecErrorCode);
                        return ByteArray0;
                    }
                    return KeyBytesProtected;
                }
                return ByteArray0;
            }
        }

        public override byte[] State
        {
            get
            {
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return null;
                }
                StateProtected[0] = 0x00;
                StateProtected[1] = (byte)(GetDsrState() ? 0x00 : 0x30);
                return StateProtected;
            }
        }

        public override Int64 BatteryVoltage
        {
            get
            {
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                return GetDsrState() ? 12000 : 0;
            }
        }

        public override Int64 IgnitionVoltage
        {
            get
            {
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                return GetDsrState() ? 12000 : 0;
            }
        }

        public override bool Connected
        {
            get
            {
                if (UseExtInterfaceFunc)
                {
                    return ConnectedProtected;
                }
#if USE_SERIAL_PORT
                return SerialPort.IsOpen;
#else
                return false;
#endif
            }
        }

        static EdInterfaceObd()
        {
#if WindowsCE || Android
            InterfaceMutex = new Mutex(false);
#else
            InterfaceMutex = new Mutex(false, "EdiabasLib_InterfaceObd");
#endif
#if USE_SERIAL_PORT
            SerialPort = new System.IO.Ports.SerialPort();
            SerialPort.DataReceived += SerialDataReceived;
            CommReceiveEvent = new AutoResetEvent(false);
#endif
            CommThreadReqEvent = new AutoResetEvent(false);
            CommThreadResEvent = new AutoResetEvent(false);
            CommThread = null;
            CommThreadLock = new Object();
            CommThreadCommand = CommThreadCommands.Idle;
            CommThreadReqCount = 0;
            CommThreadResCount = 0;
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
            return IsValidInterfaceNameStatic(name);
        }

        public static bool IsValidInterfaceNameStatic(string name)
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
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }

            if (ComPortProtected.ToUpper(Culture).StartsWith(EdFtdiInterface.PortId))
            {   // automtatic hook of FTDI functions
                EdFtdiInterface.Ediabas = Ediabas;
                InterfaceConnectFuncInt = EdFtdiInterface.InterfaceConnect;
                InterfaceDisconnectFuncInt = EdFtdiInterface.InterfaceDisconnect;
                InterfaceSetConfigFuncInt = EdFtdiInterface.InterfaceSetConfig;
                InterfaceSetDtrFuncInt = EdFtdiInterface.InterfaceSetDtr;
                InterfaceSetRtsFuncInt = EdFtdiInterface.InterfaceSetRts;
                InterfaceGetDsrFuncInt = EdFtdiInterface.InterfaceGetDsr;
                InterfaceSetBreakFuncInt = EdFtdiInterface.InterfaceSetBreak;
                InterfaceSetInterByteTimeFuncInt = null;
                InterfacePurgeInBufferFuncInt = EdFtdiInterface.InterfacePurgeInBuffer;
                InterfaceAdapterEchoFuncInt = null;
                InterfaceSendDataFuncInt = EdFtdiInterface.InterfaceSendData;
                InterfaceReceiveDataFuncInt = EdFtdiInterface.InterfaceReceiveData;
                InterfaceSendPulseFuncInt = null;
            }
#if Android
            else if (ComPortProtected.ToUpper(Culture).StartsWith(EdBluetoothInterface.PortId))
            {   // automtatic hook of bluetooth functions
                EdBluetoothInterface.Ediabas = Ediabas;
                InterfaceConnectFuncInt = EdBluetoothInterface.InterfaceConnect;
                InterfaceDisconnectFuncInt = EdBluetoothInterface.InterfaceDisconnect;
                InterfaceSetConfigFuncInt = EdBluetoothInterface.InterfaceSetConfig;
                InterfaceSetDtrFuncInt = EdBluetoothInterface.InterfaceSetDtr;
                InterfaceSetRtsFuncInt = EdBluetoothInterface.InterfaceSetRts;
                InterfaceGetDsrFuncInt = EdBluetoothInterface.InterfaceGetDsr;
                InterfaceSetBreakFuncInt = EdBluetoothInterface.InterfaceSetBreak;
                InterfaceSetInterByteTimeFuncInt = EdBluetoothInterface.InterfaceSetInterByteTime;
                InterfacePurgeInBufferFuncInt = EdBluetoothInterface.InterfacePurgeInBuffer;
                InterfaceAdapterEchoFuncInt = EdBluetoothInterface.InterfaceAdapterEcho;
                InterfaceSendDataFuncInt = EdBluetoothInterface.InterfaceSendData;
                InterfaceReceiveDataFuncInt = EdBluetoothInterface.InterfaceReceiveData;
                InterfaceSendPulseFuncInt = EdBluetoothInterface.InterfaceSendPulse;
            }
#endif
            else
            {
                InterfaceConnectFuncInt = null;
                InterfaceDisconnectFuncInt = null;
                InterfaceSetConfigFuncInt = null;
                InterfaceSetDtrFuncInt = null;
                InterfaceSetRtsFuncInt = null;
                InterfaceGetDsrFuncInt = null;
                InterfaceSetBreakFuncInt = null;
                InterfaceSetInterByteTimeFuncInt = null;
                InterfacePurgeInBufferFuncInt = null;
                InterfaceAdapterEchoFuncInt = null;
                InterfaceSendDataFuncInt = null;
                InterfaceReceiveDataFuncInt = null;
                InterfaceSendPulseFuncInt = null;
            }
            UpdateUseExtInterfaceFunc();

            CurrentBaudRate = 9600;
            CurrentParity = SerialParity.None;
            CurrentDataBits = 8;

            if (UseExtInterfaceFunc)
            {
                ConnectedProtected = InterfaceConnectFuncUse(ComPortProtected, ConnectParameterProtected);
                if (!ConnectedProtected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                }
                return ConnectedProtected;
            }

#if USE_SERIAL_PORT
            if (ComPortProtected.Length == 0)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }
            if (SerialPort.IsOpen)
            {
                return true;
            }
            try
            {
                SerialPort.PortName = ComPortProtected;
                SerialPort.BaudRate = 9600;
                SerialPort.DataBits = 8;
                SerialPort.Parity = System.IO.Ports.Parity.None;
                SerialPort.StopBits = System.IO.Ports.StopBits.One;
                SerialPort.Handshake = System.IO.Ports.Handshake.None;
                SerialPort.DtrEnable = false;
                SerialPort.RtsEnable = false;
                SerialPort.ReadTimeout = 1;
                SerialPort.Open();

                LastCommTick = DateTime.MinValue.Ticks;
            }
            catch (Exception)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }
            return true;
#else
            return false;
#endif
        }

        public override bool InterfaceDisconnect()
        {
            StopCommThread();
            base.InterfaceDisconnect();
            ConnectedProtected = false;
            if (UseExtInterfaceFunc)
            {
                return InterfaceDisconnectFuncUse();
            }

#if USE_SERIAL_PORT
            if (SerialPort.IsOpen)
            {
                SerialPort.Close();
            }
            return true;
#else
            return false;
#endif
        }

        public override bool InterfaceReset()
        {
            if (!Connected)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
            }
            CommParameter = null;
            return true;
        }

        public override bool TransmitData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = null;
            if (CommParameterProtected == null)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (sendData.Length > SendBuffer.Length)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0031);
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
            lock (CommThreadLock)
            {
                sendData.CopyTo(SendBuffer, 0);
                SendBufferLength = sendData.Length;
                CommThreadReqCount++;
                CommThreadCommand = CommThreadCommands.SingleTransmit;
            }
            CommThreadReqEvent.Set();

            for (;;)
            {
                lock (CommThreadLock)
                {
                    if (CommThreadResCount == CommThreadReqCount)
                    {
                        break;
                    }
                }
                CommThreadResEvent.WaitOne(10, false);
            }
#endif
            if (RecErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected.SetError(RecErrorCode);
                return false;
            }
            lock (CommThreadLock)
            {
                receiveData = new byte[RecBufferLength];
                Array.Copy(RecBuffer, receiveData, RecBufferLength);
            }
            return true;
        }

        public override bool ReceiveFrequent(out byte[] receiveData)
        {
            receiveData = null;
            if (CommParameterProtected == null)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (!ParSupportFrequent)
            {
                receiveData = ByteArray0;
                return true;
            }
            StartCommThread();
            lock (CommThreadLock)
            {
                SendBufferLength = 0;
                CommThreadReqCount++;
                CommThreadCommand = CommThreadCommands.SingleTransmit;
            }
            CommThreadReqEvent.Set();

            for (; ; )
            {
                lock (CommThreadLock)
                {
                    if (CommThreadResCount == CommThreadReqCount)
                    {
                        break;
                    }
                }
                CommThreadResEvent.WaitOne(10, false);
            }

            if (RecErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected.SetError(RecErrorCode);
                return false;
            }
            lock (CommThreadLock)
            {
                receiveData = new byte[RecBufferLength];
                Array.Copy(RecBuffer, receiveData, RecBufferLength);
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
                return ComPortProtected;
            }
            set
            {
                ComPortProtected = value;
            }
        }

        public InterfaceConnectDelegate InterfaceConnectFunc
        {
            get
            {
                return InterfaceConnectFuncProtected;
            }
            set
            {
                InterfaceConnectFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceConnectDelegate InterfaceConnectFuncUse
        {
            get
            {
                return InterfaceConnectFuncProtected ?? InterfaceConnectFuncInt;
            }
        }

        public InterfaceDisconnectDelegate InterfaceDisconnectFunc
        {
            get
            {
                return InterfaceDisconnectFuncProtected;
            }
            set
            {
                InterfaceDisconnectFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceDisconnectDelegate InterfaceDisconnectFuncUse
        {
            get
            {
                return InterfaceDisconnectFuncProtected ?? InterfaceDisconnectFuncInt;
            }
        }

        public InterfaceSetConfigDelegate InterfaceSetConfigFunc
        {
            get
            {
                return InterfaceSetConfigFuncProtected;
            }
            set
            {
                InterfaceSetConfigFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetConfigDelegate InterfaceSetConfigFuncUse
        {
            get
            {
                return InterfaceSetConfigFuncProtected ?? InterfaceSetConfigFuncInt;
            }
        }

        public InterfaceSetDtrDelegate InterfaceSetDtrFunc
        {
            get
            {
                return InterfaceSetDtrFuncProtected;
            }
            set
            {
                InterfaceSetDtrFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetDtrDelegate InterfaceSetDtrFuncUse
        {
            get
            {
                return InterfaceSetDtrFuncProtected ?? InterfaceSetDtrFuncInt;
            }
        }

        public InterfaceSetRtsDelegate InterfaceSetRtsFunc
        {
            get
            {
                return InterfaceSetRtsFuncProtected;
            }
            set
            {
                InterfaceSetRtsFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetRtsDelegate InterfaceSetRtsFuncUse
        {
            get
            {
                return InterfaceSetRtsFuncProtected ?? InterfaceSetRtsFuncInt;
            }
        }

        public InterfaceGetDsrDelegate InterfaceGetDsrFunc
        {
            get
            {
                return InterfaceGetDsrFuncProtected;
            }
            set
            {
                InterfaceGetDsrFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceGetDsrDelegate InterfaceGetDsrFuncUse
        {
            get
            {
                return InterfaceGetDsrFuncProtected ?? InterfaceGetDsrFuncInt;
            }
        }

        public InterfaceSetBreakDelegate InterfaceSetBreakFunc
        {
            get
            {
                return InterfaceSetBreakFuncProtected;
            }
            set
            {
                InterfaceSetBreakFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetBreakDelegate InterfaceSetBreakFuncUse
        {
            get
            {
                return InterfaceSetBreakFuncProtected ?? InterfaceSetBreakFuncInt;
            }
        }

        public InterfaceSetInterByteTimeDelegate InterfaceSetInterByteTimeFunc
        {
            get
            {
                return InterfaceSetInterByteTimeFuncProtected;
            }
            set
            {
                InterfaceSetInterByteTimeFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetInterByteTimeDelegate InterfaceSetInterByteTimeFuncUse
        {
            get
            {
                return InterfaceSetInterByteTimeFuncProtected ?? InterfaceSetInterByteTimeFuncInt;
            }
        }

        public InterfacePurgeInBufferDelegate InterfacePurgeInBufferFunc
        {
            get
            {
                return InterfacePurgeInBufferFuncProtected;
            }
            set
            {
                InterfacePurgeInBufferFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfacePurgeInBufferDelegate InterfacePurgeInBufferFuncUse
        {
            get
            {
                return InterfacePurgeInBufferFuncProtected ?? InterfacePurgeInBufferFuncInt;
            }
        }

        public InterfaceAdapterEchoDelegate InterfaceAdapterEchoFunc
        {
            get
            {
                return InterfaceAdapterEchoFuncProtected;
            }
            set
            {
                InterfaceAdapterEchoFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceAdapterEchoDelegate InterfaceAdapterEchoFuncUse
        {
            get
            {
                return InterfaceAdapterEchoFuncProtected ?? InterfaceAdapterEchoFuncInt;
            }
        }

        public InterfaceSendDataDelegate InterfaceSendDataFunc
        {
            get
            {
                return InterfaceSendDataFuncProtected;
            }
            set
            {
                InterfaceSendDataFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSendDataDelegate InterfaceSendDataFuncUse
        {
            get
            {
                return InterfaceSendDataFuncProtected ?? InterfaceSendDataFuncInt;
            }
        }

        public InterfaceReceiveDataDelegate InterfaceReceiveDataFunc
        {
            get
            {
                return InterfaceReceiveDataFuncProtected;
            }
            set
            {
                InterfaceReceiveDataFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceReceiveDataDelegate InterfaceReceiveDataFuncUse
        {
            get
            {
                return InterfaceReceiveDataFuncProtected ?? InterfaceReceiveDataFuncInt;
            }
        }

        public InterfaceSendPulseDelegate InterfaceSendPulseFunc
        {
            get
            {
                return InterfaceSendPulseFuncProtected;
            }
            set
            {
                InterfaceSendPulseFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSendPulseDelegate InterfaceSendPulseFuncUse
        {
            get
            {
                return InterfaceSendPulseFuncProtected ?? InterfaceSendPulseFuncInt;
            }
        }

        protected bool HasAdapterEcho
        {
            get
            {
                InterfaceAdapterEchoDelegate adapterEchoFunc = InterfaceAdapterEchoFuncUse;
                if (adapterEchoFunc != null)
                {
                    return adapterEchoFunc();
                }
                return AdapterEcho;
            }
        }

        protected virtual bool AdapterEcho
        {
            get
            {
                return true;
            }
        }

        protected void UpdateUseExtInterfaceFunc()
        {
            UseExtInterfaceFunc =
                InterfaceConnectFuncUse != null &&
                InterfaceDisconnectFuncUse != null &&
                InterfaceSetConfigFuncUse != null &&
                InterfaceSetDtrFuncUse != null &&
                InterfaceSetRtsFuncUse != null &&
                InterfaceGetDsrFuncUse != null &&
                InterfaceSetBreakFuncUse != null &&
                InterfaceSetInterByteTimeFuncUse != null &&
                InterfacePurgeInBufferFuncUse != null &&
                InterfaceAdapterEchoFuncUse != null &&
                InterfaceSendDataFuncUse != null &&
                InterfaceReceiveDataFuncUse != null &&
                InterfaceSendPulseFuncUse != null;
        }

        protected bool GetDsrState()
        {
            if (!UseExtInterfaceFunc)
            {
#if USE_SERIAL_PORT
                try
                {
                    if (!SerialPort.IsOpen)
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0019);
                        return false;
                    }
                    return SerialPort.DsrHolding;
                }
                catch (Exception)
                {
                    return false;
                }
#else
                return false;
#endif
            }

            bool dsrState;
            if (!InterfaceGetDsrFuncUse(out dsrState))
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0019);
                return false;
            }
            return dsrState;
        }

#if USE_SERIAL_PORT
        private static void SerialDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            CommReceiveEvent.Set();
        }
#endif

        private bool EvalChecksumPar(uint value)
        {
            ParChecksumByUser = (value & 0x01) == 0;
            ParChecksumNoCheck = (value & 0x02) != 0;
            return true;
        }

        private void StartCommThread()
        {
            if (CommThread != null)
            {
                return;
            }
            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Start comm thread");
            try
            {
                CommThreadCommand = CommThreadCommands.Idle;
                CommThreadReqCount = 0;
                CommThreadResCount = 0;
                CommThread = new Thread(CommThreadFunc)
                {
                    Priority = ThreadPriority.Highest
                };
                CommThread.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void StopCommThread()
        {
            if (EdiabasProtected != null)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Stop comm thread");
            }
            if (CommThread != null)
            {
                lock (CommThreadLock)
                {
                    CommThreadReqCount++;
                    CommThreadCommand = CommThreadCommands.Exit;
                }
                CommThreadReqEvent.Set();
                CommThread.Join();
                CommThread = null;
            }
        }

        private void CommThreadFunc()
        {
            long lastReqCount = -1;
            bool bExitThread = false;
            for (; ; )
            {
                CommThreadReqEvent.WaitOne(10, false);

                uint reqCount;
                CommThreadCommands command;
                int sendLength = 0;
                bool newRequest = false;
                lock (CommThreadLock)
                {
                    reqCount = CommThreadReqCount;
                    command = CommThreadCommand;
                    if (lastReqCount != reqCount)
                    {
                        newRequest = true;
                        Array.Copy(SendBuffer, SendBufferThread, SendBufferLength);
                        sendLength = SendBufferLength;
                    }
                }

                switch (command)
                {
                    case CommThreadCommands.Idle:
                        break;

                    case CommThreadCommands.IdleTransmit:
                        {
                            EdiabasNet.ErrorCodes errorCode = ObdIdleTrans();
                            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                            {
                                lock (CommThreadLock)
                                {
                                    if (CommThreadCommand == CommThreadCommands.IdleTransmit)
                                    {
                                        CommThreadCommand = CommThreadCommands.Idle;
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
                            RecErrorCode = ObdTrans(SendBufferThread, sendLength, ref RecBufferThread, out recLength);
                            command = CommThreadCommands.Idle;
                            if (ParIdleFunc != null)
                            {
                                command = CommThreadCommands.IdleTransmit;
                            }
                            break;
                        }

                    case CommThreadCommands.Exit:
                        ObdFinishTrans();
                        command = CommThreadCommands.Idle;
                        bExitThread = true;
                        break;
                }
                lock (CommThreadLock)
                {
                    if (recLength > 0)
                    {
                        Array.Copy(RecBufferThread, RecBuffer, recLength);
                        RecBufferLength = recLength;
                    }
                    CommThreadCommand = command;
                    CommThreadResCount = reqCount;
                }
                CommThreadResEvent.Set();
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
                InterfaceSetInterByteTimeDelegate setInterByteTimeFunc = InterfaceSetInterByteTimeFuncUse;
                if (setInterByteTimeFunc != null)
                {
                    setInterByteTimeFunc(interbyteTime);
                }
                if (interbyteTime > 0 && setInterByteTimeFunc == null)
                {
                    int bitCount = (CurrentParity == SerialParity.None) ? (CurrentDataBits + 2) : (CurrentDataBits + 3);
                    double byteTime = 1.0d / CurrentBaudRate * 1000 * bitCount;
                    long interbyteTicks = (long)((interbyteTime + byteTime) * TickResolMs);

                    if (UseExtInterfaceFunc)
                    {
                        if (!InterfacePurgeInBufferFuncUse())
                        {
                            return false;
                        }
                    }
                    else
                    {
#if USE_SERIAL_PORT
                        SerialPort.DiscardInBuffer();
#else
                        return false;
#endif
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
                        SendBufferInternal[0] = sendData[i];
                        bool dtrHandling = ((i + 1) >= length) && setDtr;
                        long startTime = Stopwatch.GetTimestamp();
                        if (!SendData(SendBufferInternal, 1, dtrHandling, true))
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
            if (UseExtInterfaceFunc)
            {
                if (!keepInBuffer)
                {
                    if (!InterfacePurgeInBufferFuncUse())
                    {
                        return false;
                    }
                }
                return InterfaceSendDataFuncUse(sendData, length, setDtr, DtrTimeCorrFtdi);
            }
#if USE_SERIAL_PORT
            try
            {
                int bitCount = (CurrentParity == SerialParity.None) ? (CurrentDataBits + 2) : (CurrentDataBits + 3);
                double byteTime = 1.0d / CurrentBaudRate * 1000 * bitCount;
                if (setDtr)
                {
                    long waitTime = (long)((DtrTimeCorrCom + byteTime * length) * TickResolMs);
                    if (!keepInBuffer)
                    {
                        SerialPort.DiscardInBuffer();
                    }
                    SerialPort.DtrEnable = true;
                    long startTime = Stopwatch.GetTimestamp();
                    SerialPort.Write(sendData, 0, length);
                    while ((Stopwatch.GetTimestamp() - startTime) < waitTime)
                    {
                    }
                    SerialPort.DtrEnable = false;
                }
                else
                {
                    long waitTime = (long)(byteTime * length);
                    if (!keepInBuffer)
                    {
                        SerialPort.DiscardInBuffer();
                    }
                    SerialPort.Write(sendData, 0, length);
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
#else
            return false;
#endif
        }

        protected bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, bool logResponse)
        {
            if (length <= 0)
            {
                return true;
            }
            // add extra delay for internal signal transitions
            timeout += AddRecTimeout;
            timeoutTelEnd += AddRecTimeout;
            if (UseExtInterfaceFunc)
            {
                return InterfaceReceiveDataFuncUse(receiveData, offset, length, timeout, timeoutTelEnd, logResponse ? EdiabasProtected : null);
            }
#if USE_SERIAL_PORT
            try
            {
                // wait for first byte
                int lastBytesToRead;
                StopWatch.Reset();
                StopWatch.Start();
                for (; ; )
                {
                    lastBytesToRead = SerialPort.BytesToRead;
                    if (lastBytesToRead > 0)
                    {
                        break;
                    }
                    if (StopWatch.ElapsedMilliseconds > timeout)
                    {
                        StopWatch.Stop();
                        return false;
                    }
                    CommReceiveEvent.WaitOne(1, false);
                }

                int recLen = 0;
                StopWatch.Reset();
                StopWatch.Start();
                for (; ; )
                {
                    int bytesToRead = SerialPort.BytesToRead;
                    if (bytesToRead >= length)
                    {
                        recLen += SerialPort.Read(receiveData, offset + recLen, length - recLen);
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if (lastBytesToRead != bytesToRead)
                    {   // bytes received
                        StopWatch.Reset();
                        StopWatch.Start();
                        lastBytesToRead = bytesToRead;
                    }
                    else
                    {
                        if (StopWatch.ElapsedMilliseconds > timeoutTelEnd)
                        {
                            break;
                        }
                    }
                    CommReceiveEvent.WaitOne(1, false);
                }
                StopWatch.Stop();
                if (logResponse)
                {
                    EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
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
#else
            return false;
#endif
        }

        protected bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd)
        {
            return ReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, false);
        }

        protected bool SetDtrSignal(bool value)
        {
            try
            {
                if (UseExtInterfaceFunc)
                {
                    if (!InterfaceSetDtrFuncUse(value))
                    {
                        return false;
                    }
                }
                else
                {
#if USE_SERIAL_PORT
                    SerialPort.DtrEnable = value;
#else
                    return false;
#endif
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return true;
        }

        protected bool SendWakeFastInit(bool setDtr)
        {
            if (UseExtInterfaceFunc)
            {
                if (InterfaceSendPulseFuncUse != null)
                {
                    if (!InterfaceSendPulseFuncUse(0x02, 2, 25, setDtr))
                    {
                        return false;
                    }
                }
                else
                {
                    if (setDtr) InterfaceSetDtrFuncUse(true);
                    long startTime = Stopwatch.GetTimestamp();
                    InterfaceSetBreakFuncUse(true);
                    while ((Stopwatch.GetTimestamp() - startTime) < 25 * TickResolMs)
                    {
                    }
                    InterfaceSetBreakFuncUse(false);
                    while ((Stopwatch.GetTimestamp() - startTime) < 50 * TickResolMs)
                    {
                    }
                    if (setDtr) InterfaceSetDtrFuncUse(false);
                }
            }
            else
            {
#if USE_SERIAL_PORT
                try
                {
                    if (setDtr) SerialPort.DtrEnable = true;
                    long startTime = Stopwatch.GetTimestamp();
                    SerialPort.BreakState = true;
                    while ((Stopwatch.GetTimestamp() - startTime) < 25 * TickResolMs)
                    {
                    }
                    SerialPort.BreakState = false;
                    while ((Stopwatch.GetTimestamp() - startTime) < 50 * TickResolMs)
                    {
                    }
                    if (setDtr) SerialPort.DtrEnable = false;
                }
                catch (Exception)
                {
                    return false;
                }
#else
                return false;
#endif
            }
            return true;
        }

        protected bool SendWakeAddress5Baud(byte value)
        {
            if (UseExtInterfaceFunc)
            {
                if (InterfaceSendPulseFuncUse != null)
                {
                    if (!InterfaceSendPulseFuncUse((UInt64) (((~value & 0xFF) << 1) | 0x0200), 10, 200, false))
                    {
                        return false;
                    }
                }
                else
                {
                    InterfacePurgeInBufferFuncUse();
                    long startTime = Stopwatch.GetTimestamp();
                    InterfaceSetBreakFuncUse(true);    // start bit
                    Thread.Sleep(180);
                    while ((Stopwatch.GetTimestamp() - startTime) < 200 * TickResolMs)
                    {
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        InterfaceSetBreakFuncUse((value & (1 << i)) == 0);
                        Thread.Sleep(180);
                        while ((Stopwatch.GetTimestamp() - startTime) < 200 * (i + 2) * TickResolMs)
                        {
                        }
                    }
                    InterfaceSetBreakFuncUse(false);   // stop bit
                    Thread.Sleep(200);
                }
            }
            else
            {
#if USE_SERIAL_PORT
                try
                {
                    SerialPort.DiscardInBuffer();
                    long startTime = Stopwatch.GetTimestamp();
                    SerialPort.BreakState = true;  // start bit
                    Thread.Sleep(180);
                    while ((Stopwatch.GetTimestamp() - startTime) < 200 * TickResolMs)
                    {
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        SerialPort.BreakState = (value & (1 << i)) == 0;
                        Thread.Sleep(180);
                        while ((Stopwatch.GetTimestamp() - startTime) < 200 * (i + 2) * TickResolMs)
                        {
                        }
                    }
                    SerialPort.BreakState = false; // stop bit
                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    return false;
                }
#else
                return false;
#endif
            }
            return true;
        }

        protected EdiabasNet.ErrorCodes ObdTrans(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            if (!UseExtInterfaceFunc)
            {
#if USE_SERIAL_PORT
                if (!SerialPort.IsOpen)
                {
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
                }
#else
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
#endif
            }

            EdiabasNet.ErrorCodes errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            UInt32 retries = CommRepeatsProtected;
            string retryComm = EdiabasProtected.GetConfigProperty("RetryComm");
            if (retryComm != null)
            {
                if (EdiabasNet.StringToValue(retryComm) == 0)
                {
                    retries = 0;
                }
            }
            for (int i = 0; i < retries + 1; i++)
            {
                errorCode = ParTransmitFunc(sendData, sendDataLength, ref receiveData, out receiveLength);
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

        protected EdiabasNet.ErrorCodes ObdIdleTrans()
        {
            if (ParIdleFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;
            }

            return ParIdleFunc();
        }

        protected EdiabasNet.ErrorCodes ObdFinishTrans()
        {
            if (ParFinishFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;
            }

            return ParFinishFunc();
        }

        private void NrDictAdd(byte deviceAddr, bool enableLogging)
        {
            int retries;
            if (NrDict.TryGetValue(deviceAddr, out retries))
            {
                NrDict.Remove(deviceAddr);
                retries++;
                if (retries <= ParRetryNr)
                {
                    if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR({0:X02}) count={1}", deviceAddr, retries);
                    NrDict.Add(deviceAddr, retries);
                }
                else
                {
                    if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NR({0:X02}) exceeded", deviceAddr);
                }
            }
            else
            {
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR({0:X02}) added", deviceAddr);
                NrDict.Add(deviceAddr, 0);
            }
        }

        private void NrDictRemove(byte deviceAddr, bool enableLogging)
        {
            if (NrDict.ContainsKey(deviceAddr))
            {
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR({0:X02}) removed", deviceAddr);
                NrDict.Remove(deviceAddr);
            }
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
                NrDict.Clear();
                int sendLength = TelLengthBmwFast(sendData);
                if (!ParChecksumByUser)
                {
                    sendData[sendLength] = CalcChecksumBmwFast(sendData, sendLength);
                }
                sendLength++;
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendLength, "Send");

                while ((Stopwatch.GetTimestamp() - LastResponseTick) < ParRegenTime * TickResolMs)
                {
                    Thread.Sleep(1);
                }
                LastCommTick = Stopwatch.GetTimestamp();
                if (!SendData(sendData, sendLength, ParSendSetDtr, ParInterbyteTime))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (HasAdapterEcho)
                {
                    // remove remote echo
                    if (!ReceiveData(receiveData, 0, sendLength, EchoTimeout, ParTimeoutTelEnd))
                    {
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No echo received");
                        ReceiveData(receiveData, 0, receiveData.Length, EchoTimeout, ParTimeoutTelEnd, enableLogging);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, sendLength, "Echo");
                    for (int i = 0; i < sendLength; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Echo incorrect");
                            ReceiveData(receiveData, 0, receiveData.Length, ParTimeoutStd, ParTimeoutTelEnd, enableLogging);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                        }
                    }
                }
            }

            for (; ; )
            {
                int timeout = (NrDict.Count > 0) ? ParTimeoutNr : ParTimeoutStd;
                //if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Timeout: {0}", timeout);
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout, ParTimeoutTelEnd))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if ((receiveData[0] & 0xC0) != 0x80)
                {
                    if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, 4, "Head");
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid header");
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, ParTimeoutTelEnd, enableLogging);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthBmwFast(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, ParTimeoutTelEnd, ParTimeoutTelEnd))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength + 1, "Resp");
                if (!ParChecksumNoCheck)
                {
                    if (CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                    {
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Checksum incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeout, ParTimeoutTelEnd, enableLogging);
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
                    NrDictAdd(receiveData[2], enableLogging);
                }
                else
                {
                    NrDictRemove(receiveData[2], enableLogging);
                    break;
                }
                if (NrDict.Count == 0)
                {
                    break;
                }
            }

            LastResponseTick = Stopwatch.GetTimestamp();
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
            if (!EcuConnected)
            {
                while ((Stopwatch.GetTimestamp() - LastCommTick) < ParTimeoutStd * TickResolMs)
                {
                    Thread.Sleep(10);
                }
                if (!SendWakeFastInit(ParSendSetDtr))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake fast init failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                LastCommTick = Stopwatch.GetTimestamp();
                if (ParStartCommTelLen > 0)
                {
                    errorCode = TransKwp2000(ParStartCommTel, ParStartCommTelLen, ref receiveData, out receiveLength, true);
                    if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending start communication failed");
                        return errorCode;
                    }
                }
                LastCommTick = Stopwatch.GetTimestamp();
                EcuConnected = true;
            }
            errorCode = TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, true);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EcuConnected = false;
                return errorCode;
            }

            LastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleKwp2000Bmw()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            while ((Stopwatch.GetTimestamp() - LastCommTick) < ParTesterPresentTime * TickResolMs)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            if (ParTesterPresentTelLen > 0)
            {
                int receiveLength;
                EdiabasNet.ErrorCodes errorCode = TransKwp2000(ParTesterPresentTel, ParTesterPresentTelLen, ref Iso9141Buffer, out receiveLength, false);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EcuConnected = false;
                    return errorCode;
                }
            }

            LastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes TransKwp2000S(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                NrDict.Clear();
                int sendLength = TelLengthKwp2000S(sendData);
                if (!ParChecksumByUser)
                {
                    sendData[sendLength] = CalcChecksumXor(sendData, sendLength);
                }
                sendLength++;
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendLength, "Send");

                while ((Stopwatch.GetTimestamp() - LastResponseTick) < ParRegenTime * TickResolMs)
                {
                    Thread.Sleep(1);
                }
                LastCommTick = Stopwatch.GetTimestamp();
                if (!SendData(sendData, sendLength, ParSendSetDtr, ParInterbyteTime))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (HasAdapterEcho)
                {
                    // remove remote echo
                    if (!ReceiveData(receiveData, 0, sendLength, EchoTimeout, ParTimeoutTelEnd))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No echo received");
                        ReceiveData(receiveData, 0, receiveData.Length, EchoTimeout, ParTimeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, sendLength, "Echo");
                    for (int i = 0; i < sendLength; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Echo incorrect");
                            ReceiveData(receiveData, 0, receiveData.Length, ParTimeoutStd, ParTimeoutTelEnd, true);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                        }
                    }
                }
            }

            for (; ; )
            {
                int timeout = (NrDict.Count > 0) ? ParTimeoutNr : ParTimeoutStd;
                //EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Timeout: {0}", timeout);
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout, ParTimeoutTelEnd))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthKwp2000S(receiveData);
                if (!ReceiveData(receiveData, 4, recLength - 3, ParTimeoutTelEnd, ParTimeoutTelEnd))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength + 1, "Resp");
                if (!ParChecksumNoCheck)
                {
                    if (CalcChecksumXor(receiveData, recLength) != receiveData[recLength])
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Checksum incorrect");
                        ReceiveData(receiveData, 0, receiveData.Length, timeout, ParTimeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }

                int dataLen = receiveData[3];
                int dataStart = 4;
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    NrDictAdd(receiveData[2], true);
                }
                else
                {
                    NrDictRemove(receiveData[2], true);
                    break;
                }
                if (NrDict.Count == 0)
                {
                    break;
                }
            }

            LastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = TelLengthKwp2000S(receiveData) + 1;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length without checksum
        private int TelLengthKwp2000S(byte[] dataBuffer)
        {
            int telLength = dataBuffer[3] + 4;
            return telLength;
        }

        private EdiabasNet.ErrorCodes TransDs2(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                int sendLength = sendDataLength;
                if (!ParChecksumByUser)
                {
                    sendData[sendLength] = CalcChecksumXor(sendData, sendLength);
                    sendLength++;
                }
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendLength, "Send");

                while ((Stopwatch.GetTimestamp() - LastResponseTick) < ParRegenTime * TickResolMs)
                {
                    Thread.Sleep(1);
                }
                LastCommTick = Stopwatch.GetTimestamp();
                if (!SendData(sendData, sendLength, ParSendSetDtr, ParInterbyteTime))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (HasAdapterEcho)
                {
                    // remove remote echo
                    if (!ReceiveData(receiveData, 0, sendLength, EchoTimeout, ParTimeoutTelEnd))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No echo received");
                        ReceiveData(receiveData, 0, receiveData.Length, EchoTimeout, ParTimeoutTelEnd, true);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, sendLength, "Echo");
                    for (int i = 0; i < sendLength; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Echo incorrect");
                            ReceiveData(receiveData, 0, receiveData.Length, ParTimeoutStd, ParTimeoutTelEnd, true);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                        }
                    }
                }
            }

            // header byte
            int headerLen = CommAnswerLenProtected[0];
            if (headerLen < 0)
            {
                headerLen = (-headerLen) + 1;
            }
            if (headerLen == 0)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Header lenght zero");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }
            if (!ReceiveData(receiveData, 0, headerLen, ParTimeoutStd, ParTimeoutTelEnd))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No header received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            int recLength = TelLengthDs2(receiveData);
            if (recLength == 0)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive lenght zero");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!ReceiveData(receiveData, headerLen, recLength - headerLen, ParTimeoutTelEnd, ParTimeoutTelEnd))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No tail received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength, "Resp");
            if (!ParChecksumNoCheck)
            {
                if (CalcChecksumXor(receiveData, recLength - 1) != receiveData[recLength - 1])
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Checksum incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, ParTimeoutStd, ParTimeoutTelEnd, true);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }

            LastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = recLength;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length with checksum
        private int TelLengthDs2(byte[] dataBuffer)
        {
            int telLength = CommAnswerLenProtected[0];   // >0 fix length
            if (telLength < 0)
            {   // offset in buffer
                int offset = (-telLength);
                if (dataBuffer.Length < offset)
                {
                    return 0;
                }
                telLength = dataBuffer[offset] + CommAnswerLenProtected[1];  // + answer offset
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

            if (sendDataLength > Iso9141Buffer.Length)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid send data length");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }

            if (!EcuConnected)
            {
                KeyBytesProtected = ByteArray0;
                keyBytesList = new List<byte>();
                while ((Stopwatch.GetTimestamp() - LastCommTick) < 2600 * TickResolMs)
                {
                    Thread.Sleep(10);
                }

                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Establish connection");
                if (UseExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(9600, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }
                else
                {
#if USE_SERIAL_PORT
                    try
                    {
                        SerialPort.BaudRate = 9600;
                        SerialPort.Parity = System.IO.Ports.Parity.None;
                    }
                    catch (Exception)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
#else
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
#endif
                }
                CurrentBaudRate = 9600;
                CurrentParity = SerialParity.None;

                if (!SetDtrSignal(false))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set DTR failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!SendWakeAddress5Baud(ParWakeAddress))
                {
                    LastCommTick = Stopwatch.GetTimestamp();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Iso9141Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate byte: {0:X02}", Iso9141Buffer[0]);
                if (Iso9141Buffer[0] == 0x55)
                {
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 9.6k detected");
                }
                else
                {   // baud rate different
                    if ((Iso9141Buffer[0] & 0x87) != 0x85)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid baud rate");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 10.4k detected");
                    if (UseExtInterfaceFunc)
                    {
                        if (InterfaceSetConfigFuncUse(10400, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
                    }
                    else
                    {
#if USE_SERIAL_PORT
                        try
                        {
                            SerialPort.BaudRate = 10400;
                            SerialPort.Parity = System.IO.Ports.Parity.None;
                        }
                        catch (Exception)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
#else
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
#endif
                    }
                    CurrentBaudRate = 10400;
                    CurrentParity = SerialParity.None;
                }

                EcuConnected = true;
                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Iso9141Buffer, 0, 2, 500, 500))
                {
                    EcuConnected = false;
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No key bytes received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                keyBytesList.Add((byte)(Iso9141Buffer[0] & 0x7F));
                keyBytesList.Add((byte)(Iso9141Buffer[1] & 0x7F));
                keyBytesList.Add(0x09);
                keyBytesList.Add(0x03);

                LastCommTick = Stopwatch.GetTimestamp();
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Key bytes: {0:X02} {1:X02}", Iso9141Buffer[0], Iso9141Buffer[1]);
                Iso9141Buffer[0] = (byte)(~Iso9141Buffer[1]);
                Thread.Sleep(10);
                if (!SendData(Iso9141Buffer, 1, ParSendSetDtr))
                {
                    EcuConnected = false;
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending key byte response failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                BlockCounter = 1;

                Thread.Sleep(10);
                errorCode = ReceiveIso9141Block(Iso9141Buffer, true);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EcuConnected = false;
                    return errorCode;
                }
                LastIso9141Cmd = Iso9141Buffer[2];
                BlockCounter++;
                if (LastIso9141Cmd != 0x09)
                {
                    // store key bytes
                    int dataLen = Iso9141Buffer[0] + 1;
                    for (int i = 0; i < dataLen; i++)
                    {
                        keyBytesList.Add(Iso9141Buffer[i]);
                    }
                }
            }

            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendDataLength, "Request");
            int recLength = 0;
            int recBlocks = 0;
            int maxRecBlocks = CommAnswerLenProtected[0];

            int waitToSendCount = 0;
            bool waitToSend = true;
            bool transmitDone = false;
            for (; ; )
            {
                bool sendDataValid = false;
                if (LastIso9141Cmd == 0x09)
                {   // ack
                    if (waitToSend)
                    {
                        waitToSend = false;
                        if (sendDataLength > 0)
                        {
                            Array.Copy(sendData, Iso9141Buffer, sendDataLength);
                            sendDataValid = true;
                        }
                        else
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Receive ID finished");
                            transmitDone = true;
                        }
                    }
                    else
                    {
                        if (recBlocks > 0)
                        {
                            // at least one block received
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Transmission finished");
                            transmitDone = true;
                        }
                    }
                }

                if (waitToSend)
                {
                    waitToSendCount++;
                    if (waitToSendCount > 1000)
                    {
                        EcuConnected = false;
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Wait for first ACK failed");
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
                    Iso9141Buffer[0] = 0x03;    // block length
                    Iso9141Buffer[2] = 0x09;    // ACK
                }

                Thread.Sleep(50);

                LastCommTick = Stopwatch.GetTimestamp();
                Iso9141Buffer[1] = BlockCounter++;
                errorCode = SendIso9141Block(Iso9141Buffer, true);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EcuConnected = false;
                    return errorCode;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                errorCode = ReceiveIso9141Block(Iso9141Buffer, true);
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EcuConnected = false;
                    return errorCode;
                }
                BlockCounter++;
                LastIso9141Cmd = Iso9141Buffer[2];

                if (!waitToSend)
                {   // store received data
                    if ((recBlocks == 0) || (LastIso9141Cmd != 0x09))
                    {
                        int blockLen = Iso9141Buffer[0];
                        if (recLength + blockLen > receiveData.Length)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive buffer overflow, ignore data");
                            transmitDone = true;
                        }
                        Array.Copy(Iso9141Buffer, 0, receiveData, recLength, blockLen);
                        recLength += blockLen;
                        recBlocks++;
                        if (recBlocks >= maxRecBlocks)
                        {   // all blocks received
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "All blocks received");
                            transmitDone = true;
                        }
                    }
                }
                else
                {
                    if ((keyBytesList != null) && (LastIso9141Cmd != 0x09))
                    {   // store key bytes
                        int dataLen = Iso9141Buffer[0] + 1;
                        for (int i = 0; i < dataLen; i++)
                        {
                            keyBytesList.Add(Iso9141Buffer[i]);
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
                KeyBytesProtected = keyBytesList.ToArray();
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, KeyBytesProtected, 0, KeyBytesProtected.Length, "ID bytes");
            }

            LastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = recLength;
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, receiveLength, "Answer");
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleIso9141()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            while ((Stopwatch.GetTimestamp() - LastCommTick) < 500 * TickResolMs)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            Iso9141Buffer[0] = 0x03;    // block length
            Iso9141Buffer[2] = 0x09;    // ACK

            LastCommTick = Stopwatch.GetTimestamp();
            Iso9141Buffer[1] = BlockCounter++;
            EdiabasNet.ErrorCodes errorCode = SendIso9141Block(Iso9141Buffer, false);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EcuConnected = false;
                return errorCode;
            }

            LastCommTick = Stopwatch.GetTimestamp();
            errorCode = ReceiveIso9141Block(Iso9141Buffer, false);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EcuConnected = false;
                return errorCode;
            }
            BlockCounter++;
            LastIso9141Cmd = Iso9141Buffer[2];

            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes SendIso9141Block(byte[] sendData, bool enableLog)
        {
            int blockLen = sendData[0];
            if (enableLog) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, blockLen, "Send");
            for (int i = 0; i < blockLen; i++)
            {
                Iso9141BlockBuffer[0] = sendData[i];
                if (!SendData(Iso9141BlockBuffer, 1, ParSendSetDtr))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (!ReceiveData(Iso9141BlockBuffer, 0, 1, ParTimeoutTelEnd, ParTimeoutTelEnd))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No data ack received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(A): {0:X02}", (byte)(~Iso9141BlockBuffer[0]));
                if ((byte)(~Iso9141BlockBuffer[0]) != sendData[i])
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Response invalid");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            Iso9141BlockBuffer[0] = 0x03;   // block end
            if (!SendData(Iso9141BlockBuffer, 1, ParSendSetDtr))
            {
                if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes ReceiveIso9141Block(byte[] recData, bool enableLog)
        {
            // block length
            if (!ReceiveData(recData, 0, 1, ParTimeoutTelEnd, ParTimeoutTelEnd))
            {
                if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No block length received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(R): {0:X02}", recData[0]);

            int blockLen = recData[0];
            for (int i = 0; i < blockLen; i++)
            {
                Iso9141BlockBuffer[0] = (byte)(~recData[i]);
                if (!SendData(Iso9141BlockBuffer, 1, ParSendSetDtr))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (!ReceiveData(recData, i + 1, 1, ParTimeoutTelEnd, ParTimeoutTelEnd))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No block data received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(R): {0:X02}", recData[i + 1]);
            }
            if (enableLog) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, recData, 0, blockLen, "Resp");
            if (recData[blockLen] != 0x03)
            {
                if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Block end invalid");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes TransConcept3(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            List<byte> keyBytesList = null;

            if (!EcuConnected)
            {
                KeyBytesProtected = ByteArray0;
                keyBytesList = new List<byte>();
                while ((Stopwatch.GetTimestamp() - LastCommTick) < 2600 * TickResolMs)
                {
                    Thread.Sleep(10);
                }

                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Establish connection");
                if (UseExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(9600, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }
                else
                {
#if USE_SERIAL_PORT
                    try
                    {
                        SerialPort.BaudRate = 9600;
                        SerialPort.Parity = System.IO.Ports.Parity.None;
                    }
                    catch (Exception)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
#else
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
#endif
                }
                CurrentBaudRate = 9600;
                CurrentParity = SerialParity.None;

                if (!SetDtrSignal(false))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set DTR failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!SendWakeAddress5Baud(ParWakeAddress))
                {
                    LastCommTick = Stopwatch.GetTimestamp();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Iso9141Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate byte: {0:X02}", Iso9141Buffer[0]);
                if (Iso9141Buffer[0] == 0x55)
                {
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 9.6k detected");
                }
                else
                {   // baud rate different
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid baud rate");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                EcuConnected = true;
                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Iso9141Buffer, 0, 3, 200, 200))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No key bytes received");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                keyBytesList.Add((byte)(Iso9141Buffer[0] & 0x7F));
                keyBytesList.Add((byte)(Iso9141Buffer[1] & 0x7F));
                keyBytesList.Add((byte)(Iso9141Buffer[2] & 0x7F));
                keyBytesList.Add(0x09);
                keyBytesList.Add(0x03);

                LastCommTick = Stopwatch.GetTimestamp();
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Key bytes: {0:X02} {1:X02} {2:X02}", Iso9141Buffer[0], Iso9141Buffer[1], Iso9141Buffer[2]);
                if (UseExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(9600, 8, SerialParity.Even, ParAllowBitBang) != InterfaceErrorResult.NoError)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        FinishConcept3();
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                }
                else
                {
#if USE_SERIAL_PORT
                    try
                    {
                        SerialPort.Parity = System.IO.Ports.Parity.Even;
                    }
                    catch (Exception)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set parity failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
#else
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
#endif
                }
            }
            // receive a data block
            if (!ReceiveData(Iso9141Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No header byte");
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            int recLength = 1;
            for (; ; )
            {
                if (recLength >= Iso9141Buffer.Length)
                {   // buffer overflow
                    break;
                }
                if (!ReceiveData(Iso9141Buffer, recLength, 1, 20, 20))
                {   // last byte receive
                    break;
                }
                recLength++;
            }
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, Iso9141Buffer, 0, recLength, "Rec");
            if (CommAnswerLenProtected[0] > 0 && recLength != CommAnswerLenProtected[0])
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid response length");
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!ParChecksumNoCheck)
            {
                if (CalcChecksumXor(Iso9141Buffer, recLength - 1) != Iso9141Buffer[recLength - 1])
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Checksum incorrect");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            Array.Copy(Iso9141Buffer, receiveData, recLength);
            receiveLength = recLength;

            if (keyBytesList != null)
            {
                for (int i = 0; i < recLength; i++)
                {
                    keyBytesList.Add(Iso9141Buffer[i]);
                }
                keyBytesList.Add(0x03);
                KeyBytesProtected = keyBytesList.ToArray();
            }

            LastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleConcept3()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            // receive a data block
            if (!ReceiveData(Iso9141Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
            {
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            int recLength = 1;
            for (; ; )
            {
                if (recLength >= Iso9141Buffer.Length)
                {   // buffer overflow
                    break;
                }
                if (!ReceiveData(Iso9141Buffer, recLength, 1, 20, 20))
                {   // last byte receive
                    break;
                }
                recLength++;
            }
            if (CommAnswerLenProtected[0] > 0 && recLength != CommAnswerLenProtected[0])
            {
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!ParChecksumNoCheck)
            {
                if (CalcChecksumXor(Iso9141Buffer, recLength - 1) != Iso9141Buffer[recLength - 1])
                {
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            LastCommTick = Stopwatch.GetTimestamp();
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes FinishConcept3()
        {
            EcuConnected = false;
            if (UseExtInterfaceFunc)
            {
                if (InterfaceSetConfigFuncUse(10400, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }
            }
            else
            {
#if USE_SERIAL_PORT
                try
                {
                    SerialPort.BaudRate = 10400;
                    SerialPort.Parity = System.IO.Ports.Parity.None;
                }
                catch (Exception)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }
#else
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
#endif
            }
            CurrentBaudRate = 10400;
            CurrentParity = SerialParity.None;

            Thread.Sleep(10);
            LastCommTick = Stopwatch.GetTimestamp();
            Iso9141Buffer[0] = 0xFF;
            if (!SendData(Iso9141Buffer, 1, false))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending stop byte failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                InterfaceDisconnect();
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
#if USE_SERIAL_PORT
                    SerialPort.Dispose();
#endif
                }
                InterfaceUnlock();

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
