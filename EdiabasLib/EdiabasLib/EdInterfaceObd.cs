#if !Android
#define USE_SERIAL_PORT
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseNullPropagation
// ReSharper disable IntroduceOptionalParameters.Local
// ReSharper disable NotAccessedVariable
// ReSharper disable InlineOutVariableDeclaration

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

        public enum Protocol
        {
            Uart,
            Kwp,
            Tp20,
            IsoTp,
        }

        [Flags]
        public enum CanFlags
        {
            Empty = 0x00,
            BusCheck = 0x01,
            Disconnect = 0x02,
        }

        protected enum CanStatus
        {
            Undefined,
            CanOk,
            CanError
        }

        protected enum KwpModes
        {
            Undefined,
            Kwp2000,
            Kwp1281,
        }

        protected enum CommThreadCommands
        {
            Idle,               // do nothing
            SingleTransmit,     // single data transmission
            FrequentMode,       // frequent mode setting
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
        public delegate InterfaceErrorResult InterfaceSetConfigDelegate(Protocol protocol, int baudRate, int dataBits, SerialParity parity, bool allowBitBang);
        public delegate bool InterfaceSetDtrDelegate(bool dtr);
        public delegate bool InterfaceSetRtsDelegate(bool rts);
        public delegate bool InterfaceGetDsrDelegate(out bool dsr);
        public delegate bool InterfaceSetBreakDelegate(bool enable);
        public delegate bool InterfaceSetInterByteTimeDelegate(int time);
        public delegate bool InterfaceSetCanIdsDelegate(int canTxId, int canRxId, CanFlags canFlags);
        public delegate bool InterfacePurgeInBufferDelegate();
        public delegate bool InterfaceAdapterEchoDelegate();
        public delegate bool InterfaceHasPreciseTimeoutDelegate();
        public delegate bool InterfaceHasAutoBaudRateDelegate();
        public delegate bool InterfaceHasAutoKwp1281Delegate();
        public delegate bool InterfaceHasIgnitionStatusDelegate();
        public delegate bool InterfaceSendDataDelegate(byte[] sendData, int length, bool setDtr, double dtrTimeCorr);
        public delegate bool InterfaceReceiveDataDelegate(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog);
        public delegate bool InterfaceSendPulseDelegate(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay);
        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        protected delegate EdiabasNet.ErrorCodes IdleDelegate();
        protected delegate EdiabasNet.ErrorCodes FrequentDelegate();
        protected delegate EdiabasNet.ErrorCodes FinishDelegate();

        private bool _disposed;
        protected const int TransBufferSize = 0x800; // transmit buffer size
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
        protected int UdsDtcStatusOverrideProtected = -1;
        protected double DtrTimeCorrCom = 0.3;
        protected double DtrTimeCorrFtdi = 0.3;
        protected int AddRecTimeout = 20;
        protected bool EnableFtdiBitBang;
        protected bool ConnectedProtected;
        protected const int EchoTimeout = 100;
        protected const int Kwp1281ByteTimeout = 55;
        protected const int Kwp1281StatusTimeout = 1000;
        protected const int Kwp1281ErrorDelay = 150;
        protected const int Kwp1281ErrorRetries = 3;
        protected const int Kwp1281InitDelay = 2600;
        protected const byte Kwp1281EndOutput = 0x06;
        protected const byte Kwp1281Ack = 0x09;
        protected const byte Kwp1281Nack = 0x0A;
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
        protected InterfaceSetCanIdsDelegate InterfaceSetCanIdsFuncProtected;
        protected InterfaceSetCanIdsDelegate InterfaceSetCanIdsFuncInt;
        protected InterfacePurgeInBufferDelegate InterfacePurgeInBufferFuncProtected;
        protected InterfacePurgeInBufferDelegate InterfacePurgeInBufferFuncInt;
        protected InterfaceAdapterEchoDelegate InterfaceAdapterEchoFuncProtected;
        protected InterfaceAdapterEchoDelegate InterfaceAdapterEchoFuncInt;
        protected InterfaceHasPreciseTimeoutDelegate InterfaceHasPreciseTimeoutFuncProtected;
        protected InterfaceHasPreciseTimeoutDelegate InterfaceHasPreciseTimeoutFuncInt;
        protected InterfaceHasAutoBaudRateDelegate InterfaceHasAutoBaudRateFuncProtected;
        protected InterfaceHasAutoBaudRateDelegate InterfaceHasAutoBaudRateFuncInt;
        protected InterfaceHasAutoKwp1281Delegate InterfaceHasAutoKwp1281FuncProtected;
        protected InterfaceHasAutoKwp1281Delegate InterfaceHasAutoKwp1281FuncInt;
        protected InterfaceHasIgnitionStatusDelegate InterfaceHasIgnitionStatusFuncProtected;
        protected InterfaceHasIgnitionStatusDelegate InterfaceHasIgnitionStatusFuncInt;
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
        protected volatile int SendBufferLength;
        protected byte[] SendBufferThread = new byte[TransBufferSize];
        protected byte[] SendBufferFrequent = new byte[TransBufferSize];
        protected volatile int SendBufferFrequentLength;
        protected byte[] SendBufferInternal = new byte[1];
        protected byte[] RecBuffer = new byte[TransBufferSize];
        protected volatile int RecBufferLength;
        protected byte[] RecBufferThread = new byte[TransBufferSize];
        protected byte[] RecBufferFrequent = new byte[TransBufferSize];
        protected int RecBufferFrequentLength;
        protected volatile EdiabasNet.ErrorCodes RecErrorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        protected byte[] Kwp1281Buffer = new byte[256];
        protected byte[] Kwp1281BlockBuffer = new byte[256];
        protected Dictionary<byte, int> Nr78Dict = new Dictionary<byte, int>();
        protected bool EcuConnected;
        protected KwpModes KwpMode;
        protected long LastCommTick;
        protected long LastResponseTick;
        protected bool Kwp1281SendNack;
        protected int CurrentBaudRate;
        protected SerialParity CurrentParity;
        protected int CurrentDataBits;
        protected CanStatus CurrentCanStatus;
        protected byte BlockCounter;
        protected byte LastKwp1281Cmd;

        protected TransmitDelegate ParTransmitFunc;
        protected IdleDelegate ParIdleFunc;
        protected FrequentDelegate ParFrequentFunc;
        protected FinishDelegate ParFinishFunc;
        protected int ParTimeoutStd;
        protected int ParTimeoutTelEnd;
        protected int ParInterbyteTime;
        protected int ParRegenTime;
        protected int ParRequestTimeNr21;
        protected int ParRequestTimeNr23;
        protected int ParRetryNr21;
        protected int ParRetryNr23;
        protected int ParTimeoutNr78;
        protected int ParRetryNr78;
        protected byte ParWakeAddress;
        protected byte ParEdicWakeAddress;
        protected byte ParEdicTesterAddress;
        protected byte ParEdicEcuAddress;
        protected ushort ParEdicTesterCanId;
        protected ushort ParEdicEcuCanId;
        protected int ParEdicW1;
        protected int ParEdicW2;
        protected int ParEdicW3;
        protected int ParEdicW4A;
        protected int ParEdicW4;
        protected int ParEdicW5;
        protected int ParEdicP1;
        protected int ParEdicP2;
        protected int ParEdicP3;
        protected int ParEdicP4;
        protected int ParEdicTesterPresentTime;
        protected int ParEdicTesterPresentTelLen;
        protected byte[] ParEdicTesterPresentTel = new byte[TransBufferSize];
        protected int ParEdicAddRetries;
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

                prop = EdiabasProtected.GetConfigProperty("ObdUdsDtcStatusOverride");
                if (prop != null)
                {
                    UdsDtcStatusOverride = (int)EdiabasNet.StringToValue(prop);
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
                bool edicPar = (CommParameterProtected != null) && (CommParameterProtected.Length > 0) &&
                              (CommParameterProtected[0] == 0x0000);
                CommAnswerLenProtected[0] = 0;
                CommAnswerLenProtected[1] = 0;

                ParTransmitFunc = null;
                ParIdleFunc = null;
                ParFrequentFunc = null;
                ParFinishFunc = null;
                if (!edicPar)
                {
                    ParTimeoutStd = 0;
                    ParTimeoutTelEnd = 0;
                    ParInterbyteTime = 0;
                    ParRegenTime = 0;
                    ParRequestTimeNr21 = 0;
                    ParRequestTimeNr23 = 0;
                    ParRetryNr21 = 0;
                    ParRetryNr23 = 0;
                    ParTimeoutNr78 = 0;
                    ParRetryNr78 = 0;
                }
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
                SendBufferFrequentLength = 0;
                RecBufferFrequentLength = 0;
                Nr78Dict.Clear();
                EcuConnected = false;
                KwpMode = KwpModes.Undefined;
                // don't init lastCommTick here
                LastResponseTick = DateTime.MinValue.Ticks;
                Kwp1281SendNack = false;
                BlockCounter = 0;
                LastKwp1281Cmd = 0x00;

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
                    case 0x0000:    // Raw (EDIC)
                        if (!EdicSimulation)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
                            return;
                        }
                        if (CommParameterProtected.Length < 5)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        switch (CommParameterProtected[4])
                        {
                            case 0x81:      // KWP2000
                                ParTransmitFunc = TransKwp2000;
                                ParIdleFunc = IdleKwp2000;
                                ParFrequentFunc = FrequentKwp2000;
                                ParFinishFunc = FinishKwp2000;
                                break;

                            case 0xA5:      // TP2.0
                                ParTransmitFunc = TransTp20;
                                ParFinishFunc = FinishTp20;
                                if (!UseExtInterfaceFunc || (InterfaceSetConfigFuncUse(Protocol.Tp20, 500000, 8, SerialParity.None, false) != InterfaceErrorResult.NoError))
                                {
                                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set TP2.0 protocol failed");
                                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                }
                                break;

                            case 0xAA:      // ISO-TP
                                ParTransmitFunc = TransIsoTp;
                                ParIdleFunc = IdleIsoTp;
                                if (!UseExtInterfaceFunc || (InterfaceSetConfigFuncUse(Protocol.IsoTp, 500000, 8, SerialParity.None, false) != InterfaceErrorResult.NoError))
                                {
                                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set ISO-TP protocol failed");
                                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                                }
                                break;

                            default:
                                ParTransmitFunc = TransUnsupported;
                                break;
                        }
                        ParSendSetDtr = true;
                        ParAllowBitBang = false;
                        ParHasKeyBytes = true;
                        ParSupportFrequent = true;
                        return;

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

                    case 0x0002:    // Concept 2 ISO 9141 (KWP1281)
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
                        ParTransmitFunc = TransKwp1281;
                        ParIdleFunc = IdleKwp1281;
                        ParFinishFunc = FinishKwp1281;
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
                        ParTimeoutNr78 = (int)CommParameterProtected[7];
                        ParRetryNr78 = (int)CommParameterProtected[6];
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
                        ParTimeoutNr78 = (int)CommParameterProtected[7];
                        ParRetryNr78 = (int)CommParameterProtected[6];
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
                        ParTimeoutNr78 = (int)CommParameterProtected[6];
                        ParRetryNr78 = (int)CommParameterProtected[5];
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
                        ParTimeoutNr78 = (int)CommParameterProtected[9];
                        ParRetryNr78 = (int)CommParameterProtected[10];
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
                    InterfaceErrorResult ftdiResult = InterfaceSetConfigFuncUse(Protocol.Uart, baudRate, dataBits, parity, ParAllowBitBang);
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
                    InterfaceSetInterByteTimeDelegate setInterByteTimeFunc = InterfaceSetInterByteTimeFuncUse;
                    if (setInterByteTimeFunc != null)
                    {
                        if (!setInterByteTimeFunc(0))
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
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
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read key bytes");
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return null;
                }
                StartCommThread();
                if (CommParameterProtected != null && ParHasKeyBytes)
                {
                    if (EcuConnected)
                    {
                        EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, KeyBytesProtected, 0, KeyBytesProtected.Length, "KeyBytes");
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
                    EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, KeyBytesProtected, 0, KeyBytesProtected.Length, "KeyBytes");
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
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read battery voltage");
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                Int64 voltage = GetDsrState() ? 12000 : 0;
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Battery voltage: {0}", voltage);
                return voltage;
            }
        }

        public override Int64 IgnitionVoltage
        {
            get
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read ignition voltage");
                Int64 voltage;
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                if (!HasIgnitionStatus || EdicSimulation || (ParTransmitFunc != null && ParTransmitFunc != TransBmwFast))
                {
                    voltage = GetDsrState() ? 12000 : 0;
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ignition voltage from DSR: {0}", voltage);
                    return voltage;
                }

                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read ignition status from interface");
                if (ParTransmitFunc != TransBmwFast)
                {
                    ParTransmitFunc = TransBmwFast;
                    if (UseExtInterfaceFunc)
                    {
                        if (InterfaceSetConfigFuncUse(Protocol.Uart, 115200, 8, SerialParity.None, false) != InterfaceErrorResult.NoError)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Set interface func failed");
                            return 0;
                        }
                    }
                }

                byte[] sendData = { 0x82, 0xF1, 0xF1, 0xFA, 0xFA };
                StartCommThread();
                lock (CommThreadLock)
                {
                    sendData.CopyTo(SendBuffer, 0);
                    SendBufferLength = sendData.Length;
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
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read ignition status failed, assume active");
                    return 12000;
                }
                byte[] receiveData;
                lock (CommThreadLock)
                {
                    receiveData = new byte[RecBufferLength];
                    Array.Copy(RecBuffer, receiveData, RecBufferLength);
                }
                bool ignitionOn = false;
                if (receiveData.Length >= 7 && receiveData[3] == 0xFA)
                {
                    if ((receiveData[4] & 0x03) == 0x03)
                    {   // CAN enabled and status present
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Status present");
                        if ((receiveData[4] & 0x02) != 0x00)
                        {   // status valid
                            ignitionOn = (receiveData[5] & 0x0C) == 0x04;
                        }
                    }
                    else
                    {   // K-LINE
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Status not present");
                        ignitionOn = true;
                    }
                }
                voltage = ignitionOn ? 12000 : 0;
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ignition voltage: {0}", voltage);

                return voltage;
            }
        }

        public override Int64 GetPort(UInt32 index)
        {
            return 0;
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
                InterfaceSetCanIdsFuncInt = null;
                InterfacePurgeInBufferFuncInt = EdFtdiInterface.InterfacePurgeInBuffer;
                InterfaceAdapterEchoFuncInt = null;
                InterfaceHasPreciseTimeoutFuncInt = null;
                InterfaceHasAutoBaudRateFuncInt = null;
                InterfaceHasAutoKwp1281FuncInt = null;
                InterfaceHasIgnitionStatusFuncInt = null;
                InterfaceSendDataFuncInt = EdFtdiInterface.InterfaceSendData;
                InterfaceReceiveDataFuncInt = EdFtdiInterface.InterfaceReceiveData;
                InterfaceSendPulseFuncInt = null;
            }
#if !WindowsCE
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
                InterfaceSetCanIdsFuncInt = EdBluetoothInterface.InterfaceSetCanIds;
                InterfacePurgeInBufferFuncInt = EdBluetoothInterface.InterfacePurgeInBuffer;
                InterfaceAdapterEchoFuncInt = EdBluetoothInterface.InterfaceAdapterEcho;
                InterfaceHasPreciseTimeoutFuncInt = EdBluetoothInterface.InterfaceHasPreciseTimeout;
                InterfaceHasAutoBaudRateFuncInt = EdBluetoothInterface.InterfaceHasAutoBaudRate;
                InterfaceHasAutoKwp1281FuncInt = EdBluetoothInterface.InterfaceHasAutoKwp1281;
                InterfaceHasIgnitionStatusFuncInt = EdBluetoothInterface.InterfaceHasIgnitionStatus;
                InterfaceSendDataFuncInt = EdBluetoothInterface.InterfaceSendData;
                InterfaceReceiveDataFuncInt = EdBluetoothInterface.InterfaceReceiveData;
                InterfaceSendPulseFuncInt = EdBluetoothInterface.InterfaceSendPulse;
            }
            else if (ComPortProtected.ToUpper(Culture).StartsWith(EdElmWifiInterface.PortId))
            {   // automtatic hook of elm wifi functions
                EdElmWifiInterface.Ediabas = Ediabas;
                InterfaceConnectFuncInt = EdElmWifiInterface.InterfaceConnect;
                InterfaceDisconnectFuncInt = EdElmWifiInterface.InterfaceDisconnect;
                InterfaceSetConfigFuncInt = EdElmWifiInterface.InterfaceSetConfig;
                InterfaceSetDtrFuncInt = EdElmWifiInterface.InterfaceSetDtr;
                InterfaceSetRtsFuncInt = EdElmWifiInterface.InterfaceSetRts;
                InterfaceGetDsrFuncInt = EdElmWifiInterface.InterfaceGetDsr;
                InterfaceSetBreakFuncInt = EdElmWifiInterface.InterfaceSetBreak;
                InterfaceSetInterByteTimeFuncInt = EdElmWifiInterface.InterfaceSetInterByteTime;
                InterfaceSetCanIdsFuncInt = EdElmWifiInterface.InterfaceSetCanIds;
                InterfacePurgeInBufferFuncInt = EdElmWifiInterface.InterfacePurgeInBuffer;
                InterfaceAdapterEchoFuncInt = EdElmWifiInterface.InterfaceAdapterEcho;
                InterfaceHasPreciseTimeoutFuncInt = EdElmWifiInterface.InterfaceHasPreciseTimeout;
                InterfaceHasAutoBaudRateFuncInt = EdElmWifiInterface.InterfaceHasAutoBaudRate;
                InterfaceHasAutoKwp1281FuncInt = EdElmWifiInterface.InterfaceHasAutoKwp1281;
                InterfaceHasIgnitionStatusFuncInt = EdElmWifiInterface.InterfaceHasIgnitionStatus;
                InterfaceSendDataFuncInt = EdElmWifiInterface.InterfaceSendData;
                InterfaceReceiveDataFuncInt = EdElmWifiInterface.InterfaceReceiveData;
                InterfaceSendPulseFuncInt = EdElmWifiInterface.InterfaceSendPulse;
            }
            else if (ComPortProtected.ToUpper(Culture).StartsWith(EdCustomWiFiInterface.PortId))
            {   // automtatic hook of custom wifi adapter functions
                EdCustomWiFiInterface.Ediabas = Ediabas;
                InterfaceConnectFuncInt = EdCustomWiFiInterface.InterfaceConnect;
                InterfaceDisconnectFuncInt = EdCustomWiFiInterface.InterfaceDisconnect;
                InterfaceSetConfigFuncInt = EdCustomWiFiInterface.InterfaceSetConfig;
                InterfaceSetDtrFuncInt = EdCustomWiFiInterface.InterfaceSetDtr;
                InterfaceSetRtsFuncInt = EdCustomWiFiInterface.InterfaceSetRts;
                InterfaceGetDsrFuncInt = EdCustomWiFiInterface.InterfaceGetDsr;
                InterfaceSetBreakFuncInt = EdCustomWiFiInterface.InterfaceSetBreak;
                InterfaceSetInterByteTimeFuncInt = EdCustomWiFiInterface.InterfaceSetInterByteTime;
                InterfaceSetCanIdsFuncInt = EdCustomWiFiInterface.InterfaceSetCanIds;
                InterfacePurgeInBufferFuncInt = EdCustomWiFiInterface.InterfacePurgeInBuffer;
                InterfaceAdapterEchoFuncInt = EdCustomWiFiInterface.InterfaceAdapterEcho;
                InterfaceHasPreciseTimeoutFuncInt = EdCustomWiFiInterface.InterfaceHasPreciseTimeout;
                InterfaceHasAutoBaudRateFuncInt = EdCustomWiFiInterface.InterfaceHasAutoBaudRate;
                InterfaceHasAutoKwp1281FuncInt = EdCustomWiFiInterface.InterfaceHasAutoKwp1281;
                InterfaceHasIgnitionStatusFuncInt = EdCustomWiFiInterface.InterfaceHasIgnitionStatus;
                InterfaceSendDataFuncInt = EdCustomWiFiInterface.InterfaceSendData;
                InterfaceReceiveDataFuncInt = EdCustomWiFiInterface.InterfaceReceiveData;
                InterfaceSendPulseFuncInt = EdCustomWiFiInterface.InterfaceSendPulse;
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
                InterfaceSetCanIdsFuncInt = null;
                InterfacePurgeInBufferFuncInt = null;
                InterfaceAdapterEchoFuncInt = null;
                InterfaceHasPreciseTimeoutFuncInt = null;
                InterfaceHasAutoBaudRateFuncInt = null;
                InterfaceHasAutoKwp1281FuncInt = null;
                InterfaceHasIgnitionStatusFuncInt = null;
                InterfaceSendDataFuncInt = null;
                InterfaceReceiveDataFuncInt = null;
                InterfaceSendPulseFuncInt = null;
            }
            UpdateUseExtInterfaceFunc();

            CurrentBaudRate = 9600;
            CurrentParity = SerialParity.None;
            CurrentDataBits = 8;
            CurrentCanStatus = CanStatus.Undefined;

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
            try
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
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
            if (EdicSimulation)
            {
                if (SendBufferFrequentLength != 0)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Frequent mode active");
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                    return false;
                }
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendData.Length, "Send EDIC");
                if (CommAnswerLenProtected[1] != 0x0000)
                {   // command
                    receiveData = ByteArray0;
                    switch (CommAnswerLenProtected[1])
                    {
                        case 0x0001:    // parameter set 1
                            if (CommParameterProtected != null && CommParameterProtected.Length >= 89 && CommParameterProtected[4] == 0x81)
                            {
                                ParEdicTesterAddress = (byte)CommParameterProtected[88];
                                ParEdicEcuAddress = (byte)CommParameterProtected[5];
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC Tester: {0:X02}, Ecu: {1:X02}", ParEdicTesterAddress, ParEdicEcuAddress);

                                ParEdicW1 = (int)(CommParameterProtected[23] + (CommParameterProtected[24] << 8));
                                ParEdicW2 = (int)(CommParameterProtected[25] + (CommParameterProtected[26] << 8));
                                ParEdicW3 = (int)(CommParameterProtected[27] + (CommParameterProtected[28] << 8));
                                ParEdicW4A = (int)(CommParameterProtected[29] + (CommParameterProtected[30] << 8));
                                ParEdicW4 = (int)(CommParameterProtected[31] + (CommParameterProtected[32] << 8));
                                ParEdicW5 = (int)(CommParameterProtected[33] + (CommParameterProtected[34] << 8));
                                ParEdicP1 = (int)(CommParameterProtected[35] + (CommParameterProtected[36] << 8));
                                ParEdicP2 = (int)(CommParameterProtected[37] + (CommParameterProtected[38] << 8));
                                ParEdicP3 = (int)(CommParameterProtected[39] + (CommParameterProtected[40] << 8));
                                ParEdicP4 = (int)(CommParameterProtected[41] + (CommParameterProtected[42] << 8));
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC W1-W5: {0} {1} {2} {3} {4} {5}",
                                    ParEdicW1, ParEdicW2, ParEdicW3, ParEdicW4A, ParEdicW4, ParEdicW5);
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC P1-P4: {0} {1} {2} {3}",
                                    ParEdicP1, ParEdicP2, ParEdicP3, ParEdicP4);
                                ParEdicTesterPresentTime = (int) (CommParameterProtected[60] + (CommParameterProtected[61] << 8));
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC Tester present time: {0}", ParEdicTesterPresentTime);
                                ParEdicTesterPresentTelLen = (byte) CommParameterProtected[47];
                                if (ParTesterPresentTelLen > 11)
                                {
                                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0010);
                                    return false;
                                }
                                for (int i = 0; i < ParEdicTesterPresentTelLen; i++)
                                {
                                    ParEdicTesterPresentTel[i] = (byte) CommParameterProtected[i + 49];
                                }
                                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, ParEdicTesterPresentTel, 0, ParEdicTesterPresentTelLen, "EDIC tester present");
                                ParEdicAddRetries = 3;
                                // copy Px values to standard timeouts
                                ParTimeoutStd = ParEdicP2;
                                ParTimeoutTelEnd = ParEdicP1;
                                ParInterbyteTime = ParEdicP4;
                                ParRegenTime = ParEdicP3;
                                ParRequestTimeNr21 = 500;
                                ParRequestTimeNr23 = 500;
                                ParRetryNr21 = 240;     // 2 min
                                ParRetryNr23 = 240;     // 2 min
                                ParTimeoutNr78 = 5000;
                                ParRetryNr78 = 50;      // VAG is only using interface deadlock timeout
                            }
                            return true;

                        case 0x0002:    // parameter set 2
                            if (CommParameterProtected != null && CommParameterProtected.Length >= 6 && CommParameterProtected[4] == 0x81)
                            {
                                byte wakeAddress = (byte) (CommParameterProtected[5] & 0x7F);
                                bool oddParity = true;
                                for (int i = 0; i < 7; i++)
                                {
                                    oddParity ^= (wakeAddress & (1 << i)) != 0;
                                }
                                if (oddParity)
                                {
                                    wakeAddress |= 0x80;
                                }
                                ParEdicWakeAddress = wakeAddress;
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC Wake: {0:X02}", ParEdicWakeAddress);
                            }
                            return true;

                        case 0x0004:    // parameter set 4
                            if (CommParameterProtected != null && CommParameterProtected.Length >= 74 && CommParameterProtected[4] == 0xA5)
                            {
                                ParEdicWakeAddress = (byte)CommParameterProtected[5];
                                ParEdicTesterAddress = (byte) CommParameterProtected[70];
                                ParEdicEcuAddress = (byte) CommParameterProtected[71];
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC CAN: {0:X02}, Tester: {1:X02}, Ecu: {2:X02}", ParEdicWakeAddress, ParEdicTesterAddress, ParEdicEcuAddress);

                                ParEdicAddRetries = 3;
                                // set standard timeouts
                                ParTimeoutStd = 600;   // max timeout, use ParTimeoutNr78
                                ParTimeoutTelEnd = 200;
                                ParInterbyteTime = 0;
                                ParRegenTime = 0;
                                ParRequestTimeNr21 = 1;
                                ParRequestTimeNr23 = 1;
                                ParRetryNr21 = 240;     // 2 min
                                ParRetryNr23 = 240;     // 2 min
                                ParTimeoutNr78 = 2000;
                                ParRetryNr78 = 50;      // VAG is only using interface deadlock timeout

                                KeyBytesProtected = new byte[] { 0xDA, 0x8F, ParEdicWakeAddress, 0x54, 0x50 };
                            }
                            return true;

                        case 0x0091:    // parameter set 0x91
                            if (CommParameterProtected != null && CommParameterProtected.Length >= 86 && CommParameterProtected[4] == 0xAA && sendData.Length >= 15 && sendData[1] == 0xAA)
                            {
                                ParEdicTesterCanId = (ushort)(sendData[15] << 8 | sendData[14]);
                                ParEdicEcuCanId = (ushort)(sendData[11] << 8 | sendData[10]);

                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC ISO-TP: Tester: {0:X03}, Ecu: {1:X03}", ParEdicTesterCanId, ParEdicEcuCanId);

                                ParEdicTesterPresentTime = (int)(CommParameterProtected[72] + (CommParameterProtected[73] << 8));
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC Tester present time: {0}", ParEdicTesterPresentTime);
                                ParEdicTesterPresentTelLen = (byte)CommParameterProtected[76];
                                if (ParTesterPresentTelLen > 10)
                                {
                                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0010);
                                    return false;
                                }
                                for (int i = 0; i < ParEdicTesterPresentTelLen; i++)
                                {
                                    ParEdicTesterPresentTel[i] = (byte)CommParameterProtected[i + 77];
                                }
                                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, ParEdicTesterPresentTel, 0, ParEdicTesterPresentTelLen, "EDIC tester present");

                                ParEdicAddRetries = 3;
                                // set standard timeouts
                                ParTimeoutStd = (int)(CommParameterProtected[48] + (CommParameterProtected[49] << 8) + (CommParameterProtected[50] << 16) + (CommParameterProtected[51] << 24));
                                ParTimeoutNr78 = (int)(CommParameterProtected[56] + (CommParameterProtected[57] << 8) + (CommParameterProtected[58] << 16) + (CommParameterProtected[59] << 24));
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EDIC UDS P2={0}, P2Ext={1}", ParTimeoutStd, ParTimeoutNr78);
                                ParRetryNr78 = 50;  // VAG has no limit
                                ParTimeoutTelEnd = 200;
                                ParInterbyteTime = 0;
                                ParRegenTime = 0;
                            }
                            return true;

                        case 0x0010:    // start communication
                            break;

                        case 0x0011:    // stop communication
                            return true;

                        default:
                            return true;
                    }
                }
            }
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
            EdiabasNet.ErrorCodes cachedErrorCode;
            byte[] cachedResponse;
            if (ReadCachedTransmission(sendData, out cachedResponse, out cachedErrorCode))
            {
                if (cachedErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected.SetError(cachedErrorCode);
                    return false;
                }
                receiveData = cachedResponse;
                return true;
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
                CacheTransmission(sendData, null, RecErrorCode);
                EdiabasProtected.SetError(RecErrorCode);
                return false;
            }
            lock (CommThreadLock)
            {
                receiveData = new byte[RecBufferLength];
                Array.Copy(RecBuffer, receiveData, RecBufferLength);
            }
            CacheTransmission(sendData, receiveData, EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE);
            return true;
        }

        public override bool TransmitFrequent(byte[] sendData)
        {
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendData.Length, "Send Frequent");
            if (!EdicSimulation)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (CommParameterProtected == null)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (!ParSupportFrequent)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Frequent not supported");
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (sendData.Length == 0)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "No frequent send data");
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            if (SendBufferFrequentLength != 0)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Frequent mode active");
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            StartCommThread();
            lock (CommThreadLock)
            {
                sendData.CopyTo(SendBuffer, 0);
                SendBufferLength = sendData.Length;
                CommThreadReqCount++;
                CommThreadCommand = CommThreadCommands.FrequentMode;
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
            if (RecErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected.SetError(RecErrorCode);
                return false;
            }
            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Frequent mode started");
            return true;
        }

        public override bool ReceiveFrequent(out byte[] receiveData)
        {
            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "ReceiveFrequent");
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
            if (EdicSimulation && SendBufferFrequentLength == 0)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Frequent mode not active");
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
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, receiveData.Length, "Frequent");
            return true;
        }

        public override bool StopFrequent()
        {
            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "StopFrequent");
            if (EdicSimulation)
            {
                StartCommThread();
                lock (CommThreadLock)
                {
                    SendBufferLength = 0;
                    CommThreadReqCount++;
                    CommThreadCommand = CommThreadCommands.FrequentMode;
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
                if (RecErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected.SetError(RecErrorCode);
                    return false;
                }
            }
            else
            {
                StopCommThread();
            }
            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Frequent mode stopped");
            return true;
        }

        public override bool RawData(byte[] sendData, out byte[] receiveData)
        {
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendData.Length, "Send Raw");
            receiveData = ByteArray0;
            if (EdicSimulation)
            {
                // bit0 = 0 = HISTORY KL30 ON, bit4 = 0 = KL30 ON, bit5 = 0 = KL15 ON, bit6 = 0 = KL15 DISCONNECTED
                byte edicStatus = 0x00;
                if (sendData.Length == 2 && sendData[0] == 0xF1 && sendData[1] == 0x07)
                {
                    receiveData = new byte[] { 0x00, 0x00, 0xFF };
                }
                else if (sendData.Length == 2 && sendData[0] == 0x2E && sendData[1] == 0x00)
                {
                    receiveData = new byte[] { 0x00, edicStatus, (byte) (EcuConnected ? 0x02 : 0x00), (byte)((EcuConnected ? 0x80 : 0x00) | 0x01), 0x00, 0x00 };
                }
                else if (sendData.Length == 3 && sendData[0] == 0x24 && sendData[1] == 0x00 && sendData[2] == 0x80)
                {
                    receiveData = new byte[] { 0x0B, edicStatus };
                }
            }
            else
            {
                if (sendData.Length == 2 && sendData[0] == 0xF1 && sendData[1] == 0x07)
                {
                    receiveData = new byte[] { 0x05, 0x03, 0x00 };
                }
            }
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, receiveData.Length, "Resp Raw");
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

        public int UdsDtcStatusOverride
        {
            get
            {
                return UdsDtcStatusOverrideProtected;
            }
            set
            {
                UdsDtcStatusOverrideProtected = value;
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

        public InterfaceSetCanIdsDelegate InterfaceSetCanIdsFunc
        {
            get
            {
                return InterfaceSetCanIdsFuncProtected;
            }
            set
            {
                InterfaceSetCanIdsFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceSetCanIdsDelegate InterfaceSetCanIdsFuncUse
        {
            get
            {
                return InterfaceSetCanIdsFuncProtected ?? InterfaceSetCanIdsFuncInt;
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

        public InterfaceHasPreciseTimeoutDelegate InterfaceHasPreciseTimeoutFunc
        {
            get
            {
                return InterfaceHasPreciseTimeoutFuncProtected;
            }
            set
            {
                InterfaceHasPreciseTimeoutFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceHasPreciseTimeoutDelegate InterfaceHasPreciseTimeoutFuncUse
        {
            get
            {
                return InterfaceHasPreciseTimeoutFuncProtected ?? InterfaceHasPreciseTimeoutFuncInt;
            }
        }

        public InterfaceHasAutoBaudRateDelegate InterfaceHasAutoBaudRateFunc
        {
            get
            {
                return InterfaceHasAutoBaudRateFuncProtected;
            }
            set
            {
                InterfaceHasAutoBaudRateFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceHasAutoBaudRateDelegate InterfaceHasAutoBaudRateFuncUse
        {
            get
            {
                return InterfaceHasAutoBaudRateFuncProtected ?? InterfaceHasAutoBaudRateFuncInt;
            }
        }

        public InterfaceHasAutoKwp1281Delegate InterfaceHasAutoKwp1281Func
        {
            get
            {
                return InterfaceHasAutoKwp1281FuncProtected;
            }
            set
            {
                InterfaceHasAutoKwp1281FuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceHasAutoKwp1281Delegate InterfaceHasAutoKwp1281FuncUse
        {
            get
            {
                return InterfaceHasAutoKwp1281FuncProtected ?? InterfaceHasAutoKwp1281FuncInt;
            }
        }

        public InterfaceHasIgnitionStatusDelegate InterfaceHasIgnitionStatusFunc
        {
            get
            {
                return InterfaceHasIgnitionStatusFuncProtected;
            }
            set
            {
                InterfaceHasIgnitionStatusFuncProtected = value;
                UpdateUseExtInterfaceFunc();
            }
        }

        protected InterfaceHasIgnitionStatusDelegate InterfaceHasIgnitionStatusFuncUse
        {
            get
            {
                return InterfaceHasIgnitionStatusFuncProtected ?? InterfaceHasIgnitionStatusFuncInt;
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

        protected virtual bool EdicSimulation
        {
            get
            {
                return false;
            }
        }

        protected bool HasPreciceTimeout
        {
            get
            {
                InterfaceHasPreciseTimeoutDelegate preciceTimeoutFunc = InterfaceHasPreciseTimeoutFuncUse;
                if (preciceTimeoutFunc != null)
                {
                    return preciceTimeoutFunc();
                }
                return true;
            }
        }

        protected bool HasAutoBaudRate
        {
            get
            {
                InterfaceHasAutoBaudRateDelegate autoBaudRateFunc = InterfaceHasAutoBaudRateFuncUse;
                if (autoBaudRateFunc != null)
                {
                    return autoBaudRateFunc();
                }
                return true;
            }
        }

        protected bool HasAutoKwp1281
        {
            get
            {
                InterfaceHasAutoKwp1281Delegate autoKwp1281Func = InterfaceHasAutoKwp1281FuncUse;
                if (autoKwp1281Func != null)
                {
                    return autoKwp1281Func();
                }
                return true;
            }
        }

        protected bool HasIgnitionStatus
        {
            get
            {
                InterfaceHasIgnitionStatusDelegate hasIgnitionStatus = InterfaceHasIgnitionStatusFuncUse;
                if (hasIgnitionStatus != null)
                {
                    return hasIgnitionStatus();
                }
                return false;
            }
        }

        protected void UpdateUseExtInterfaceFunc()
        {
            // these funtions are optional:
            // InterfaceSetInterByteTimeFuncUse, InterfaceSetCanIdsFuncUse,
            // InterfaceAdapterEchoFuncUse,
            // InterfaceHasPreciseTimeoutFuncUse, InterfaceHasAutoBaudRateFuncUse,
            // InterfaceHasAutoKwp1281FuncUse, InterfaceHasIgnitionStatusFuncUse,
            // InterfaceSendPulseFuncUse
            UseExtInterfaceFunc =
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
                            EdiabasNet.ErrorCodes errorCode = (SendBufferFrequentLength == 0) ? ObdIdleTrans() : ObdFrequentTrans();
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
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (SendBufferFrequentLength != 0)
                            {
                                if (RecBufferFrequentLength == 0)
                                {
                                    EdiabasNet.ErrorCodes errorCode = ObdFrequentTrans();
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
                                }
                                RecErrorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                                if (EcuConnected && RecBufferFrequentLength != 0)
                                {
                                    Array.Copy(RecBufferFrequent, RecBufferThread, RecBufferFrequentLength);
                                    recLength = RecBufferFrequentLength;
                                    RecBufferFrequentLength = 0;
                                }
                                else
                                {
                                    recLength = 0;
                                }
                            }
                            else
                            {
                                RecErrorCode = ObdTrans(SendBufferThread, sendLength, ref RecBufferThread, out recLength);
                            }
                            command = CommThreadCommands.Idle;
                            if ((SendBufferFrequentLength != 0) || (ParIdleFunc != null))
                            {
                                command = CommThreadCommands.IdleTransmit;
                            }
                            break;
                        }

                    case CommThreadCommands.FrequentMode:
                        {
                            RecErrorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                            if (sendLength == 0)
                            {   // stop frequent
                                SendBufferFrequentLength = 0;
                                RecBufferFrequentLength = 0;
                            }
                            else
                            {
                                if (ParFrequentFunc == null)
                                {
                                    RecErrorCode = EdiabasNet.ErrorCodes.EDIABAS_IFH_0006;
                                }
                                else
                                {
                                    Array.Copy(SendBufferThread, SendBufferFrequent, sendLength);
                                    SendBufferFrequentLength = sendLength;
                                    RecBufferFrequentLength = 0;
                                }
                            }
                            command = CommThreadCommands.Idle;
                            if ((SendBufferFrequentLength != 0) || (ParIdleFunc != null))
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
                    if (!setInterByteTimeFunc(interbyteTime))
                    {
                        return false;
                    }
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
                InterfacePurgeInBufferFuncUse();
                if (InterfaceSendPulseFuncUse != null)
                {
                    if (!InterfaceSendPulseFuncUse(0x02, 2, 25, setDtr, false, 0))
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

        protected bool SendWakeAddress5Baud(byte value, int autoKeyByteDelay)
        {
            return SendWakeAddress5Baud(value, false, false, autoKeyByteDelay);
        }

        protected bool SendWakeAddress5Baud(byte value, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            if (UseExtInterfaceFunc)
            {
                InterfacePurgeInBufferFuncUse();
                if (InterfaceSendPulseFuncUse != null)
                {
                    if (!InterfaceSendPulseFuncUse((UInt64) ((value << 1) | 0x0200), 10, 200, setDtr, bothLines, autoKeyByteDelay))
                    {
                        return false;
                    }
                }
                else
                {
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
            if (ParTransmitFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0006;
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
            if (EdicSimulation)
            {
                retries += (UInt32)ParEdicAddRetries;
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
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0011)
                {   // unknown interface
                    break;
                }
            }
            if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0011)
            {   // hide interface error
                errorCode = EdiabasNet.ErrorCodes.EDIABAS_IFH_0010;
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

        protected EdiabasNet.ErrorCodes ObdFrequentTrans()
        {
            if (ParFrequentFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;
            }

            return ParFrequentFunc();
        }

        protected EdiabasNet.ErrorCodes ObdFinishTrans()
        {
            if (ParFinishFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;
            }

            return ParFinishFunc();
        }

        private void Nr78DictAdd(byte deviceAddr, bool enableLogging)
        {
            int retries;
            if (Nr78Dict.TryGetValue(deviceAddr, out retries))
            {
                Nr78Dict.Remove(deviceAddr);
                retries++;
                if (retries <= ParRetryNr78)
                {
                    if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) count={1}", deviceAddr, retries);
                    Nr78Dict.Add(deviceAddr, retries);
                }
                else
                {
                    if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NR78({0:X02}) exceeded", deviceAddr);
                }
            }
            else
            {
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) added", deviceAddr);
                Nr78Dict.Add(deviceAddr, 0);
            }
        }

        private void Nr78DictRemove(byte deviceAddr, bool enableLogging)
        {
            if (Nr78Dict.ContainsKey(deviceAddr))
            {
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) removed", deviceAddr);
                Nr78Dict.Remove(deviceAddr);
            }
        }

        private EdiabasNet.ErrorCodes TransUnsupported(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Interface unsupported");
            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0011;
        }

        private EdiabasNet.ErrorCodes TransBmwFast(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            return TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, true);
        }

        private EdiabasNet.ErrorCodes TransKwp2000(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            List<byte> keyBytesList = null;
            receiveLength = 0;

            if (!EcuConnected)
            {
                if (CurrentCanStatus == CanStatus.CanOk)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** KWP2000 aborted because of CAN support");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                KwpMode = KwpModes.Undefined;
                Kwp1281SendNack = false;
                KeyBytesProtected = ByteArray0;
                keyBytesList = new List<byte>();
                long delayTime = ParEdicW1 + 1000;
                while ((Stopwatch.GetTimestamp() - LastCommTick) < delayTime * TickResolMs)
                {
                    Thread.Sleep(10);
                }

                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Establish connection");
                if (UseExtInterfaceFunc)
                {
                    if (HasAutoBaudRate) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Auto baud rate");
                    if (InterfaceSetConfigFuncUse(Protocol.Kwp, HasAutoBaudRate ? BaudAuto : 9600, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                    InterfaceSetInterByteTimeDelegate setInterByteTimeFunc = InterfaceSetInterByteTimeFuncUse;
                    if (setInterByteTimeFunc != null)
                    {
                        if (!setInterByteTimeFunc(2))
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set interbyte time failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
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

                if (!SetDtrSignal(ParSendSetDtr))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set DTR failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!SendWakeAddress5Baud(ParEdicWakeAddress, ParSendSetDtr, true, ParEdicW4A))
                {
                    LastCommTick = Stopwatch.GetTimestamp();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (HasAutoBaudRate)
                {
                    if (!ReceiveData(Kwp1281Buffer, 0, 2, ParEdicW1, ParEdicW1))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    int baudRate = ((Kwp1281Buffer[0] << 8) + Kwp1281Buffer[1]) << 1;
                    if (baudRate == 0)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid baud rate");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate {0} detected", baudRate);

                    CurrentBaudRate = baudRate;
                    CurrentParity = SerialParity.None;
                    if (UseExtInterfaceFunc)
                    {
                        if (InterfaceSetConfigFuncUse(Protocol.Kwp, CurrentBaudRate, 8, CurrentParity, ParAllowBitBang) != InterfaceErrorResult.NoError)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
                    }
                }
                else
                {
                    if (!ReceiveData(Kwp1281Buffer, 0, 1, ParEdicW1, ParEdicW1))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate byte: {0:X02}", Kwp1281Buffer[0]);
                    if (Kwp1281Buffer[0] == 0x55)
                    {
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 9.6k detected");
                    }
                    else
                    {
                        // baud rate different
                        if ((Kwp1281Buffer[0] & 0x87) != 0x85)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid baud rate");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                        }
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 10.4k detected");
                        if (UseExtInterfaceFunc)
                        {
                            if (InterfaceSetConfigFuncUse(Protocol.Kwp, 10400, 8, SerialParity.None, ParAllowBitBang) !=
                                InterfaceErrorResult.NoError)
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
                    }
                    CurrentBaudRate = 10400;
                    CurrentParity = SerialParity.None;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Kwp1281Buffer, 0, 2, ParEdicW2, ParEdicW3))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No key bytes received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Key bytes: {0:X02} {1:X02}", Kwp1281Buffer[0], Kwp1281Buffer[1]);
                switch (Kwp1281Buffer[1])
                {
                    case 0x8F:
                        KwpMode = KwpModes.Kwp2000;
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "KWP2000 protocol");
                        break;

                    default:
                        KwpMode = KwpModes.Kwp1281;
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "KWP1281 protocol");
                        break;
                }

                keyBytesList.Add(Kwp1281Buffer[0]);
                keyBytesList.Add(Kwp1281Buffer[1]);
                keyBytesList.Add((byte)((KwpMode == KwpModes.Kwp2000) ? (~ParEdicWakeAddress) : 0x00));
                keyBytesList.Add((byte)CurrentBaudRate);
                keyBytesList.Add((byte)(CurrentBaudRate >> 8));

                Thread.Sleep(ParEdicW4A);
                if (!HasAutoBaudRate)
                {
                    Kwp1281Buffer[0] = (byte) (~Kwp1281Buffer[1]);
                    if (!SendData(Kwp1281Buffer, 1, ParSendSetDtr))
                    {
                        EcuConnected = false;
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending key byte response failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                }
                LastCommTick = Stopwatch.GetTimestamp();

                EdiabasNet.ErrorCodes errorCode = EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;   // concept not implemented
                switch (KwpMode)
                {
                    case KwpModes.Kwp2000:
                        errorCode = InitKwp2000(ref keyBytesList);
                        break;

                    case KwpModes.Kwp1281:
                        errorCode = InitKwp1281(ref keyBytesList);
                        break;
                }
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    return errorCode;
                }
            }
            switch (KwpMode)
            {
                case KwpModes.Kwp2000:
                    if (sendDataLength > 0)
                    {
                        // create full telegram
                        byte[] sendDataBuffer;
                        if (sendDataLength > 0x3F)
                        {
                            sendDataBuffer = new byte[sendDataLength + 5]; // +1 checksum
                            sendDataBuffer[0] = 0x80;
                            sendDataBuffer[1] = ParEdicEcuAddress;
                            sendDataBuffer[2] = ParEdicTesterAddress;
                            sendDataBuffer[3] = (byte)sendDataLength;
                            Array.Copy(sendData, 0, sendDataBuffer, 4, sendDataLength);
                        }
                        else
                        {
                            sendDataBuffer = new byte[sendDataLength + 4]; // +1 checksum
                            sendDataBuffer[0] = (byte)(0x80 | sendDataLength);
                            sendDataBuffer[1] = ParEdicEcuAddress;
                            sendDataBuffer[2] = ParEdicTesterAddress;
                            Array.Copy(sendData, 0, sendDataBuffer, 3, sendDataLength);
                        }
                        return ProcessKwp2000(sendDataBuffer, sendDataBuffer.Length, ref receiveData, out receiveLength);
                    }
                    return ProcessKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength);

                case KwpModes.Kwp1281:
                    return ProcessKwp1281(sendData, sendDataLength, ref receiveData, out receiveLength, ref keyBytesList, true);
            }
            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;   // concept not implemented
        }

        private EdiabasNet.ErrorCodes InitKwp2000(ref List<byte> keyBytesList)
        {
            if (!ReceiveData(Kwp1281Buffer, 0, 1, ParEdicW4, ParEdicW4))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake address received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Wake address byte: {0:X02}", Kwp1281Buffer[0]);
            if (ParEdicWakeAddress != (byte)(~Kwp1281Buffer[0]))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid wake address received");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            EcuConnected = true;

            KeyBytesProtected = keyBytesList.ToArray();
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, KeyBytesProtected, 0, KeyBytesProtected.Length, "ID bytes");
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes ProcessKwp2000(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;

            if (sendDataLength == 0)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }
            EdiabasNet.ErrorCodes errorCode = TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, true);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                return errorCode;
            }
            for (;;)
            {
                byte[] tempRecBuffer = new byte[receiveData.Length];
                int tempRecLen;
                errorCode = TransKwp2000(null, 0, ref tempRecBuffer, out tempRecLen, true);
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
                {
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                }
                if (receiveLength + tempRecLen <= receiveData.Length)
                {
                    Array.Copy(tempRecBuffer, 0, receiveData, receiveLength, tempRecLen);
                    receiveLength += tempRecLen;
                }
            }
        }

        private EdiabasNet.ErrorCodes TransKwp2000(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, bool enableLogging)
        {
            int nrSendCount = 0;
            restart:
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                Nr78Dict.Clear();
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
                int timeout = (Nr78Dict.Count > 0) ? ParTimeoutNr78 : ParTimeoutStd;
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
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength + 1, "Resp");
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
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F))
                {
                    int nrRequestTime = 0;
                    int nrRetries = 0;
                    if (receiveData[dataStart + 2] == 0x21)
                    {
                        nrRequestTime = ParRequestTimeNr21;
                        nrRetries = ParRetryNr21;
                    }
                    if (receiveData[dataStart + 2] == 0x23)
                    {
                        nrRequestTime = ParRequestTimeNr23;
                        nrRetries = ParRetryNr23;
                    }
                    if (nrRequestTime > 0 && nrRetries > 0)
                    {
                        if (nrSendCount >= nrRetries)
                        {
                            if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NR21/23 exceeded");
                            break;
                        }
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "NR21/23 request");
                        Thread.Sleep(nrRequestTime);
                        nrSendCount++;
                        goto restart;
                    }
                }

                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    Nr78DictAdd(receiveData[2], enableLogging);
                }
                else
                {
                    Nr78DictRemove(receiveData[2], enableLogging);
                    break;
                }
                if (Nr78Dict.Count == 0)
                {
                    break;
                }
            }

            LastResponseTick = Stopwatch.GetTimestamp();
            receiveLength = TelLengthBmwFast(receiveData) + 1;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes IdleKwp2000()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            switch (KwpMode)
            {
                case KwpModes.Kwp2000:
                    while ((Stopwatch.GetTimestamp() - LastCommTick) < ParEdicTesterPresentTime * TickResolMs)
                    {
                        return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                    }

                    if (ParEdicTesterPresentTelLen > 0)
                    {
                        int receiveLength;
                        EdiabasNet.ErrorCodes errorCode = TransKwp2000(ParEdicTesterPresentTel, ParEdicTesterPresentTelLen, ref Kwp1281Buffer, out receiveLength, false);
                        if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                        {
                            EcuConnected = false;
                            return errorCode;
                        }
                    }

                    LastCommTick = Stopwatch.GetTimestamp();
                    return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;

                case KwpModes.Kwp1281:
                    return IdleKwp1281();
            }
            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;   // concept not implemented
        }

        private EdiabasNet.ErrorCodes FrequentKwp2000()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (SendBufferFrequentLength == 0)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            switch (KwpMode)
            {
                case KwpModes.Kwp2000:
                    {
                        EdiabasNet.ErrorCodes errorCode = TransKwp2000(SendBufferFrequent, SendBufferFrequentLength, ref RecBufferFrequent, out RecBufferFrequentLength, false);
                        if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                        {
                            EcuConnected = false;
                            return errorCode;
                        }
                        return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                    }

                case KwpModes.Kwp1281:
                    {
                        List<byte> keyBytesList = null;
                        EdiabasNet.ErrorCodes errorCode = ProcessKwp1281(SendBufferFrequent, SendBufferFrequentLength, ref RecBufferFrequent, out RecBufferFrequentLength, ref keyBytesList, false);
                        if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                        {
                            EcuConnected = false;
                            return errorCode;
                        }
                        return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                    }
            }
            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;   // concept not implemented
        }

        private EdiabasNet.ErrorCodes FinishKwp2000()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            switch (KwpMode)
            {
                case KwpModes.Kwp2000:
                {
                    int receiveLength;
                    byte[] finishTel = {0x81, ParEdicEcuAddress, ParEdicTesterAddress, 0x82, 0x00};
                    EdiabasNet.ErrorCodes errorCode = TransKwp2000(finishTel, finishTel.Length - 1, ref Kwp1281Buffer, out receiveLength, false);
                    EcuConnected = false;
                    return errorCode;
                }

                case KwpModes.Kwp1281:
                    return FinishKwp1281();
            }
            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0014;   // concept not implemented
        }

        private EdiabasNet.ErrorCodes TransTp20(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            byte[] sendDataBuffer;
            if (sendDataLength == 0)
            {
                // connect check command
                sendDataBuffer = new byte[] {0x01, ParEdicWakeAddress, ParEdicTesterAddress, 0x00, 0x00 };
            }
            else
            {
                // create full telegram
                if (sendDataLength > 0x3F)
                {
                    sendDataBuffer = new byte[sendDataLength + 5]; // +1 checksum
                    sendDataBuffer[0] = 0x80;
                    sendDataBuffer[1] = ParEdicEcuAddress;
                    sendDataBuffer[2] = ParEdicTesterAddress;
                    sendDataBuffer[3] = (byte)sendDataLength;
                    Array.Copy(sendData, 0, sendDataBuffer, 4, sendDataLength);
                }
                else
                {
                    sendDataBuffer = new byte[sendDataLength + 4]; // +1 checksum
                    sendDataBuffer[0] = (byte)(0x80 | sendDataLength);
                    sendDataBuffer[1] = ParEdicEcuAddress;
                    sendDataBuffer[2] = ParEdicTesterAddress;
                    Array.Copy(sendData, 0, sendDataBuffer, 3, sendDataLength);
                }
            }
            int sendLen = sendDataBuffer.Length - 1;   // for checksum

            receiveLength = 0;
            for (;;)
            {
                byte[] tempRecBuffer = new byte[receiveData.Length];
                int tempRecLen;
                EdiabasNet.ErrorCodes errorCode = TransTp20(sendDataBuffer, sendLen, ref tempRecBuffer, out tempRecLen, true);
                if ((errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE) || (tempRecLen == 0))
                {
                    return errorCode;
                }
                sendLen = 0;
                if ((receiveLength + tempRecLen) <= receiveData.Length)
                {
                    Array.Copy(tempRecBuffer, 0, receiveData, receiveLength, tempRecLen);
                    receiveLength += tempRecLen;
                }
            }
        }

        private EdiabasNet.ErrorCodes TransTp20(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, bool enableLogging)
        {
            if (sendDataLength >= 3)
            {
                // replace ecu address with the CAN address
                sendData[1] = ParEdicWakeAddress;
            }
            CanFlags canFlags = CanFlags.Empty;
            if (sendDataLength == 4 && sendData[0] == 0x01)
            {
                switch (sendData[3])
                {
                    case 0x00:  // connect check
                        canFlags = CanFlags.BusCheck;
                        break;

                    default:  // disconnect
                        canFlags = CanFlags.Disconnect;
                        break;
                }
                sendData[0] = 0x81;
            }

            if (!InterfaceSetCanIdsFuncUse(-1, -1, canFlags))
            {
                if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Setting CAN IDs failed");
                receiveLength = 0;
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }

            EdiabasNet.ErrorCodes errorCode = TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, enableLogging);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                return errorCode;
            }

            if (receiveLength == 6 && receiveData[1] == 0xF1 && receiveData[2] == 0xF1 && receiveData[3] == 0x7F)
            {
                receiveLength = 0;
                // adapter status telegram
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Adapter status: {0:X02}", receiveData[4]);
                errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                switch (receiveData[4])
                {
                    case 0x00:  // connected
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Connect OK");
                        CurrentCanStatus = CanStatus.CanOk;
                        break;

                    case 0x01:  // disconnected
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Disconnect OK");
                        if (CurrentCanStatus != CanStatus.CanError)
                        {
                            CurrentCanStatus = CanStatus.CanOk;
                        }
                        break;

                    case 0x02:  // receive complete
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Receive complete");
                        break;

                    case 0x03:  // CAN error
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** CAN error");
                        CurrentCanStatus = CanStatus.CanError;
                        errorCode = EdiabasNet.ErrorCodes.EDIABAS_IFH_0011;
                        break;
                }
                return errorCode;
            }
            if (receiveLength >= 4)
            {
                // restore address in response
                receiveData[1] = ParEdicTesterAddress;
                receiveData[2] = ParEdicEcuAddress;
                receiveData[receiveLength - 1] = CalcChecksumBmwFast(receiveData, receiveLength - 1);
            }
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes FinishTp20()
        {
            // disconnect command
            int receiveLength;
            byte[] finishTel = { 0x01, ParEdicWakeAddress, ParEdicTesterAddress, 0x01, 0x00 };
            return TransTp20(finishTel, finishTel.Length - 1, ref Kwp1281Buffer, out receiveLength, true);
        }

        private EdiabasNet.ErrorCodes TransIsoTp(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            return TransIsoTp(sendData, sendDataLength, ref receiveData, out receiveLength, true);
        }

        private EdiabasNet.ErrorCodes TransIsoTp(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, bool enableLogging)
        {
            byte[] sendDataBuffer = sendData;
            int sendLen = sendDataLength;
            receiveLength = 0;
            CanFlags canFlags = CanFlags.Empty;
            if ((sendDataBuffer == null) || (sendLen == 0))
            {
                // connect check command
                if (ParEdicTesterPresentTelLen <= 0)
                {   // no check telegram present
                    return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                }
                canFlags = CanFlags.BusCheck;
                sendDataBuffer = ParEdicTesterPresentTel;
                sendLen = ParEdicTesterPresentTelLen;
            }
            if (UdsDtcStatusOverride >= 0 && 
                sendLen == 3 && sendDataBuffer[0] == 0x19 && sendDataBuffer[1] == 0x02 && sendDataBuffer[2] == 0x0C)
            {
                // request error memory pendingDTC and confirmedDTC
                sendDataBuffer[2] = (byte) UdsDtcStatusOverride;
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Overriding UDS DTC status with {0:X02}", (byte) UdsDtcStatusOverride);
            }

            if (!InterfaceSetCanIdsFuncUse(ParEdicEcuCanId, ParEdicTesterCanId, canFlags))
            {
                if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Setting CAN IDs failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }
            if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendDataBuffer, 0, sendLen, "Send");
            LastCommTick = Stopwatch.GetTimestamp();
            if (!SendData(sendDataBuffer, sendLen, false, 0))
            {
                if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
            }
            int timeout = ParTimeoutStd;
            for (int retry = 0; retry < ParRetryNr78; retry++)
            {
                if (!ReceiveData(receiveData, 0, 3, timeout, ParTimeoutTelEnd))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No header received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                int dataLength = (receiveData[1] << 8) | receiveData[2];
                byte[] tempBuffer = new byte[dataLength + 4];
                Array.Copy(receiveData, tempBuffer, 3);
                if (!ReceiveData(tempBuffer, 3, dataLength + 1, ParTimeoutTelEnd, ParTimeoutTelEnd))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No tail received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                LastCommTick = Stopwatch.GetTimestamp();
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, tempBuffer, 0, dataLength + 4, "Resp");
                if (CalcChecksumBmwFast(tempBuffer, dataLength + 3) != tempBuffer[dataLength + 3])
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Checksum incorrect");
                    ReceiveData(receiveData, 0, receiveData.Length, ParTimeoutStd, ParTimeoutTelEnd);
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                switch (tempBuffer[0])
                {
                    case 0x01:  // data telegram
                        break;

                    case 0x02:  // status telegram
                        if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Adapter status: {0:X02}", tempBuffer[3]);
                        switch (tempBuffer[3])
                        {
                            case 0x00:  // bus ok
                                if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "CAN bus OK");
                                CurrentCanStatus = CanStatus.CanOk;
                                break;

                            case 0x01:  // CAN error
                                if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** CAN error");
                                CurrentCanStatus = CanStatus.CanError;
                                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0011;
                        }
                        return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;

                    default:
                        if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Unknown telegram type: {0:X02}", tempBuffer[0]);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                if (dataLength >= 3 && tempBuffer[3] == 0x7F && tempBuffer[4] == sendDataBuffer[0] && tempBuffer[5] == 0x78)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "NR78");
                    timeout = ParTimeoutNr78;
                    continue;
                }

                if (sendDataLength >= 2 && sendDataBuffer[0] == 0x10)
                {   // session control
                    if (dataLength >= 6 && tempBuffer[3] == 0x50)
                    {   // positive response
                        int timeoutP2 = (tempBuffer[5] << 8) + tempBuffer[6];
                        int timeoutP2Ext = ((tempBuffer[7] << 8) + tempBuffer[8]) * 10;
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Info: UDS P2={0}, P2Ext={1}", timeoutP2, timeoutP2Ext);
                    }
                }

                if (dataLength > receiveData.Length)
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive buffer too small");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                int dataOffset = 18;
                Array.Clear(receiveData, 0, dataOffset);
                receiveData[0] = (byte)ParEdicTesterCanId;
                receiveData[1] = (byte)(ParEdicTesterCanId >> 8);
                receiveData[15] = 0x01;
                receiveData[16] = (byte)dataLength;
                receiveData[17] = (byte)(dataLength >> 8);
                Array.Copy(tempBuffer, 3, receiveData, dataOffset, dataLength);
                receiveLength = dataLength + dataOffset;
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }
            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
        }

        private EdiabasNet.ErrorCodes IdleIsoTp()
        {
            int receiveLength;

            while ((Stopwatch.GetTimestamp() - LastCommTick) < ParEdicTesterPresentTime * TickResolMs)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            EdiabasNet.ErrorCodes errorCode = TransIsoTp(null, 0, ref Kwp1281Buffer, out receiveLength, false);
            LastCommTick = Stopwatch.GetTimestamp();
            return errorCode;
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
                if (ParStartCommTelLen > 0)
                {
                    if (!SendWakeFastInit(ParSendSetDtr))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake fast init failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    LastCommTick = Stopwatch.GetTimestamp();
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
                EdiabasNet.ErrorCodes errorCode = TransKwp2000(ParTesterPresentTel, ParTesterPresentTelLen, ref Kwp1281Buffer, out receiveLength, false);
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
                Nr78Dict.Clear();
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
                int timeout = (Nr78Dict.Count > 0) ? ParTimeoutNr78 : ParTimeoutStd;
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
                    Nr78DictAdd(receiveData[2], true);
                }
                else
                {
                    Nr78DictRemove(receiveData[2], true);
                    break;
                }
                if (Nr78Dict.Count == 0)
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

        private EdiabasNet.ErrorCodes TransKwp1281(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            List<byte> keyBytesList = null;

            if (sendDataLength > Kwp1281Buffer.Length)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid send data length");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }

            if (!EcuConnected)
            {
                Kwp1281SendNack = false;
                KeyBytesProtected = ByteArray0;
                keyBytesList = new List<byte>();
                while ((Stopwatch.GetTimestamp() - LastCommTick) < Kwp1281InitDelay * TickResolMs)
                {
                    Thread.Sleep(10);
                }

                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Establish connection");
                if (UseExtInterfaceFunc)
                {
                    if (HasAutoBaudRate) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Auto baud rate");
                    if (InterfaceSetConfigFuncUse(Protocol.Kwp, HasAutoBaudRate ? BaudAuto : 9600, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                    InterfaceSetInterByteTimeDelegate setInterByteTimeFunc = InterfaceSetInterByteTimeFuncUse;
                    if (setInterByteTimeFunc != null)
                    {
                        if (!setInterByteTimeFunc(0))
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set interbyte time failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
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
                if (!SendWakeAddress5Baud(ParWakeAddress, 10))
                {
                    LastCommTick = Stopwatch.GetTimestamp();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (HasAutoBaudRate)
                {
                    if (!ReceiveData(Kwp1281Buffer, 0, 2, ParTimeoutStd, ParTimeoutStd))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    int baudRate = ((Kwp1281Buffer[0] << 8) + Kwp1281Buffer[1]) << 1;
                    if (baudRate == 0)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid baud rate");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate {0} detected", baudRate);

                    CurrentBaudRate = baudRate;
                    CurrentParity = SerialParity.None;
                    if (UseExtInterfaceFunc)
                    {
                        if (InterfaceSetConfigFuncUse(Protocol.Kwp, CurrentBaudRate, 8, CurrentParity, ParAllowBitBang) != InterfaceErrorResult.NoError)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
                    }
                }
                else
                {
                    if (!ReceiveData(Kwp1281Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate byte: {0:X02}", Kwp1281Buffer[0]);
                    if (Kwp1281Buffer[0] == 0x55)
                    {
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 9.6k detected");
                    }
                    else
                    {
                        // baud rate different
                        if ((Kwp1281Buffer[0] & 0x87) != 0x85)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid baud rate");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                        }
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate 10.4k detected");
                        if (UseExtInterfaceFunc)
                        {
                            if (InterfaceSetConfigFuncUse(Protocol.Kwp, 10400, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
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
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Kwp1281Buffer, 0, 2, 500, 500))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No key bytes received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                keyBytesList.Add((byte)(Kwp1281Buffer[0] & 0x7F));
                keyBytesList.Add((byte)(Kwp1281Buffer[1] & 0x7F));
                keyBytesList.Add(0x09);
                keyBytesList.Add(0x03);

                LastCommTick = Stopwatch.GetTimestamp();
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Key bytes: {0:X02} {1:X02}", Kwp1281Buffer[0], Kwp1281Buffer[1]);

                Thread.Sleep(40);
                if (!HasAutoBaudRate)
                {
                    Kwp1281Buffer[0] = (byte) (~Kwp1281Buffer[1]);
                    if (!SendData(Kwp1281Buffer, 1, ParSendSetDtr))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending key byte response failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                }
                LastCommTick = Stopwatch.GetTimestamp();

                EdiabasNet.ErrorCodes errorCode = InitKwp1281(ref keyBytesList);
                LastCommTick = Stopwatch.GetTimestamp();
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    return errorCode;
                }
            }

            return ProcessKwp1281(sendData, sendDataLength, ref receiveData, out receiveLength, ref keyBytesList, false);
        }

        private EdiabasNet.ErrorCodes IdleKwp1281()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            while ((Stopwatch.GetTimestamp() - LastCommTick) < 500 * TickResolMs)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            Kwp1281Buffer[0] = 0x03;    // block length
            Kwp1281Buffer[2] = Kwp1281Ack;

            LastCommTick = Stopwatch.GetTimestamp();
            Kwp1281Buffer[1] = BlockCounter++;
            EdiabasNet.ErrorCodes errorCode = SendKwp1281Block(Kwp1281Buffer, false);
            LastCommTick = Stopwatch.GetTimestamp();
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EcuConnected = false;
                return errorCode;
            }

            errorCode = ReceiveKwp1281Block(Kwp1281Buffer, false);
            LastCommTick = Stopwatch.GetTimestamp();
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EcuConnected = false;
                return errorCode;
            }
            BlockCounter++;
            LastKwp1281Cmd = Kwp1281Buffer[2];

            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes FinishKwp1281()
        {
            if (!EcuConnected)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }

            int receiveLength;
            byte[] finishTel = { 0x03, 0x00, Kwp1281EndOutput };    // end output
            byte[] receiveData = new byte[256];
            List<byte> keyBytesList = null;
            EdiabasNet.ErrorCodes errorCode = ProcessKwp1281(finishTel, finishTel.Length, ref receiveData, out receiveLength, ref keyBytesList, true);
            EcuConnected = false;
            return errorCode;
        }

        private EdiabasNet.ErrorCodes InitKwp1281(ref List<byte> keyBytesList)
        {
            BlockCounter = 1;

            EdiabasNet.ErrorCodes errorCode = ReceiveKwp1281Block(Kwp1281Buffer, true, 50);
            LastCommTick = Stopwatch.GetTimestamp();
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                return errorCode;
            }
            LastKwp1281Cmd = Kwp1281Buffer[2];
            BlockCounter++;
            if (EdicSimulation || (LastKwp1281Cmd != Kwp1281Ack))
            {
                // store key bytes
                int dataLen = Kwp1281Buffer[0] + 1;
                for (int i = 0; i < dataLen; i++)
                {
                    keyBytesList.Add(Kwp1281Buffer[i]);
                }
            }
            EcuConnected = true;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        private EdiabasNet.ErrorCodes ProcessKwp1281(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, ref List<byte> keyBytesList, bool appendAck)
        {
            receiveLength = 0;

            if (sendDataLength > Kwp1281Buffer.Length)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid send data length");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
            }

            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendDataLength, "Request");
            int recLength = 0;
            int recBlocks = 0;
            int maxRecBlocks = EdicSimulation ? int.MaxValue : CommAnswerLenProtected[0];

            Kwp1281SendNack = false;
            int waitToSendCount = 0;
            bool waitToSend = true;
            bool transmitDone = false;
            bool ackStored = false;
            for (;;)
            {
                bool sendDataValid = false;
                if (Kwp1281SendNack)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Send NACK");
                    Kwp1281Buffer[0] = 0x03; // block length
                    Kwp1281Buffer[2] = Kwp1281Nack;
                }
                else
                {
                    if (LastKwp1281Cmd == Kwp1281Ack)
                    {   // ack
                        if (waitToSend)
                        {
                            waitToSend = false;
                            if (sendDataLength > 0)
                            {
                                Array.Copy(sendData, Kwp1281Buffer, sendDataLength);
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
                        Kwp1281Buffer[0] = 0x03;    // block length
                        Kwp1281Buffer[2] = Kwp1281Ack;
                    }
                }

                Thread.Sleep(50);
                LastCommTick = Stopwatch.GetTimestamp();

                Kwp1281Buffer[1] = BlockCounter++;
                EdiabasNet.ErrorCodes errorCode = SendKwp1281Block(Kwp1281Buffer, true);
                LastCommTick = Stopwatch.GetTimestamp();
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EcuConnected = false;
                    return errorCode;
                }

                if (sendDataValid && sendDataLength == 3 && sendData[2] == Kwp1281EndOutput)
                {   // end output command
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Disconnect");
                    EcuConnected = false;
                    return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                }

                errorCode = ReceiveKwp1281Block(Kwp1281Buffer, true);
                LastCommTick = Stopwatch.GetTimestamp();
                if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EcuConnected = false;
                    return errorCode;
                }
                BlockCounter++;
                LastKwp1281Cmd = Kwp1281Buffer[2];
                if (Kwp1281SendNack)
                {
                    continue;
                }

                if (!waitToSend)
                {   // store received data
                    if ((recBlocks == 0) || (LastKwp1281Cmd != Kwp1281Ack) || (!ackStored && appendAck))
                    {
                        int blockLen = Kwp1281Buffer[0];
                        if (recLength + blockLen > receiveData.Length)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive buffer overflow, ignore data");
                            transmitDone = true;
                        }
                        else
                        {
                            Array.Copy(Kwp1281Buffer, 0, receiveData, recLength, blockLen);
                            recLength += blockLen;
                            recBlocks++;
                            if (LastKwp1281Cmd == Kwp1281Ack)
                            {
                                ackStored = true;
                            }
                            if (recBlocks >= maxRecBlocks)
                            {
                                // all blocks received
                                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "All blocks received");
                                transmitDone = true;
                            }
                        }
                    }
                }
                else
                {
                    if ((keyBytesList != null) && (EdicSimulation || (LastKwp1281Cmd != Kwp1281Ack)))
                    {   // store key bytes
                        int dataLen = Kwp1281Buffer[0] + 1;
                        for (int i = 0; i < dataLen; i++)
                        {
                            keyBytesList.Add(Kwp1281Buffer[i]);
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

        private EdiabasNet.ErrorCodes SendKwp1281Block(byte[] sendData, bool enableLog)
        {
            bool autoKwp = HasAutoKwp1281;
            int retries = 0;
            int blockLen = sendData[0];
            if (enableLog) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, blockLen, "Send");
            if (autoKwp)
            {
                for (;;)
                {
                    Array.Copy(sendData, Kwp1281BlockBuffer, blockLen);
                    Kwp1281BlockBuffer[blockLen] = 0x03;    // block end
                    if (!SendData(Kwp1281BlockBuffer, blockLen + 1, ParSendSetDtr))
                    {
                        if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    bool restart = false;
                    for (int i = 0; i < blockLen; i++)
                    {
                        if (!ReceiveData(Kwp1281BlockBuffer, 0, 1, Kwp1281StatusTimeout, Kwp1281StatusTimeout))
                        {
                            if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No data ack received: {0}", i);
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                        }
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(A): {0:X02}", (byte)(~Kwp1281BlockBuffer[0]));
                        if ((byte)(~Kwp1281BlockBuffer[0]) != sendData[i])
                        {
                            if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Response invalid: {0:X02} {1:X02}", (byte)(~Kwp1281BlockBuffer[0]), sendData[i]);
                            retries++;
                            if (retries > Kwp1281ErrorRetries)
                            {
                                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                            }
                            if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Retry: {0}", retries);
                            Thread.Sleep(Kwp1281ErrorDelay);
                            restart = true;
                            break;
                        }
                    }
                    if (restart)
                    {
                        continue;
                    }
                    return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                }
            }
            for (;;)
            {
                bool restart = false;
                for (int i = 0; i < blockLen; i++)
                {
                    Kwp1281BlockBuffer[0] = sendData[i];
                    if (!SendData(Kwp1281BlockBuffer, 1, ParSendSetDtr))
                    {
                        if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    if (!ReceiveData(Kwp1281BlockBuffer, 0, 1, Kwp1281ByteTimeout, Kwp1281ByteTimeout))
                    {
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No data ack received: {0}", i);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(A): {0:X02}", (byte)(~Kwp1281BlockBuffer[0]));
                    if ((byte)(~Kwp1281BlockBuffer[0]) != sendData[i])
                    {
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Response invalid: {0:X02} {1:X02}", (byte)(~Kwp1281BlockBuffer[0]), sendData[i]);
                        retries++;
                        if (retries > Kwp1281ErrorRetries)
                        {
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                        }
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Retry: {0}", retries);
                        Thread.Sleep(Kwp1281ErrorDelay);
                        restart = true;
                        break;
                    }
                }
                if (restart)
                {
                    continue;
                }
                Kwp1281BlockBuffer[0] = 0x03;   // block end
                if (!SendData(Kwp1281BlockBuffer, 1, ParSendSetDtr))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }
        }

        private EdiabasNet.ErrorCodes ReceiveKwp1281Block(byte[] recData, bool enableLog)
        {
            return ReceiveKwp1281Block(recData, enableLog, 0);
        }

        private EdiabasNet.ErrorCodes ReceiveKwp1281Block(byte[] recData, bool enableLog, int addStartTimeout)
        {
            Kwp1281SendNack = false;
            bool autoKwp = HasAutoKwp1281;

            if (autoKwp)
            {
                byte[] statusBuffer = new byte[2];
                // block length
                if (!ReceiveData(statusBuffer, 0, 2, Kwp1281StatusTimeout + addStartTimeout, Kwp1281StatusTimeout + addStartTimeout))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No block length received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(R): {0:X02} {1:X02}", statusBuffer[0], statusBuffer[1]);
                recData[0] = statusBuffer[0];

                int blockLen = recData[0];
                for (int i = 0; i < blockLen; i++)
                {
                    if (!ReceiveData(statusBuffer, 0, 2, Kwp1281StatusTimeout, Kwp1281StatusTimeout))
                    {
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No status data received: {0}", i);
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                    int delayTime = (statusBuffer[1] & 0x7F)*10;
                    if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(R): {0:X02} {1:X02} t={2}ms", statusBuffer[0], statusBuffer[1], delayTime);
                    if ((statusBuffer[1] & 0x80) != 0x00)
                    {
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Restart block at {0} t={1}ms", i, delayTime);
                        recData[0] = statusBuffer[0];
                        blockLen = recData[0];
                        i = -1; // will be incremented next to 0
                        continue;
                    }
                    recData[i + 1] = statusBuffer[0];
                }
                if (enableLog) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, recData, 0, blockLen + 1, "Resp");
                if (recData[blockLen] != 0x03)
                {
                    if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Block end invalid: {0:X02}", recData[blockLen]);
                    Kwp1281SendNack = true;
                }
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }

            for (;;)
            {
                // block length
                if (!ReceiveData(recData, 0, 1, Kwp1281ByteTimeout + addStartTimeout, Kwp1281ByteTimeout + addStartTimeout))
                {
                    if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No block length received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(R): {0:X02}", recData[0]);

                bool restart = false;
                int blockLen = recData[0];
                for (int i = 0; i < blockLen; i++)
                {
                    Kwp1281BlockBuffer[0] = (byte)(~recData[i]);
                    if (!SendData(Kwp1281BlockBuffer, 1, ParSendSetDtr))
                    {
                        if (enableLog) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                    if (!ReceiveData(recData, i + 1, 1, Kwp1281ByteTimeout, Kwp1281ByteTimeout))
                    {
                        if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No block data received: {0}", i);
                        restart = true;
                        break;
                    }
                    if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Info, "(R): {0:X02}", recData[i + 1]);
                }
                if (restart)
                {
                    continue;
                }
                if (enableLog) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, recData, 0, blockLen + 1, "Resp");
                if (recData[blockLen] != 0x03)
                {
                    if (enableLog) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Block end invalid: {0:X02}", recData[blockLen]);
                    Kwp1281SendNack = true;
                }
                return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            }
        }

        private EdiabasNet.ErrorCodes TransConcept3(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            List<byte> keyBytesList = null;

            if (!EcuConnected)
            {
                KeyBytesProtected = ByteArray0;
                keyBytesList = new List<byte>();
                while ((Stopwatch.GetTimestamp() - LastCommTick) < Kwp1281InitDelay * TickResolMs)
                {
                    Thread.Sleep(10);
                }

                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Establish connection");
                if (UseExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(Protocol.Uart, 9600, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set baud rate failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                    }
                    InterfaceSetInterByteTimeDelegate setInterByteTimeFunc = InterfaceSetInterByteTimeFuncUse;
                    if (setInterByteTimeFunc != null)
                    {
                        if (!setInterByteTimeFunc(0))
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Set interbyte time failed");
                            return EdiabasNet.ErrorCodes.EDIABAS_IFH_0041;
                        }
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
                if (!SendWakeAddress5Baud(ParWakeAddress, 10))
                {
                    LastCommTick = Stopwatch.GetTimestamp();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending wake address failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }

                LastCommTick = Stopwatch.GetTimestamp();
                if (!ReceiveData(Kwp1281Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No wake response");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Baud rate byte: {0:X02}", Kwp1281Buffer[0]);
                if (Kwp1281Buffer[0] == 0x55)
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
                if (!ReceiveData(Kwp1281Buffer, 0, 3, 200, 200))
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No key bytes received");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
                keyBytesList.Add((byte)(Kwp1281Buffer[0] & 0x7F));
                keyBytesList.Add((byte)(Kwp1281Buffer[1] & 0x7F));
                keyBytesList.Add((byte)(Kwp1281Buffer[2] & 0x7F));
                keyBytesList.Add(0x09);
                keyBytesList.Add(0x03);

                LastCommTick = Stopwatch.GetTimestamp();
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Key bytes: {0:X02} {1:X02} {2:X02}", Kwp1281Buffer[0], Kwp1281Buffer[1], Kwp1281Buffer[2]);
                if (UseExtInterfaceFunc)
                {
                    if (InterfaceSetConfigFuncUse(Protocol.Uart, 9600, 8, SerialParity.Even, ParAllowBitBang) != InterfaceErrorResult.NoError)
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
            if (!ReceiveData(Kwp1281Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No header byte");
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            int recLength = 1;
            for (; ; )
            {
                if (recLength >= Kwp1281Buffer.Length)
                {   // buffer overflow
                    break;
                }
                if (!HasPreciceTimeout)
                {   // bugfix if we can't detected telegram timeout
                    if (CommAnswerLenProtected[0] > 0 && recLength >= CommAnswerLenProtected[0])
                    {
                        break;
                    }
                }
                if (!ReceiveData(Kwp1281Buffer, recLength, 1, 20, 20))
                {   // last byte receive
                    break;
                }
                recLength++;
            }
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, Kwp1281Buffer, 0, recLength, "Rec");
            if (CommAnswerLenProtected[0] > 0 && recLength != CommAnswerLenProtected[0])
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Invalid response length");
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            if (!ParChecksumNoCheck)
            {
                if (CalcChecksumXor(Kwp1281Buffer, recLength - 1) != Kwp1281Buffer[recLength - 1])
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Checksum incorrect");
                    FinishConcept3();
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }
            }
            Array.Copy(Kwp1281Buffer, receiveData, recLength);
            receiveLength = recLength;

            if (keyBytesList != null)
            {
                for (int i = 0; i < recLength; i++)
                {
                    keyBytesList.Add(Kwp1281Buffer[i]);
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
            if (!ReceiveData(Kwp1281Buffer, 0, 1, ParTimeoutStd, ParTimeoutStd))
            {
                FinishConcept3();
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            }
            int recLength = 1;
            for (; ; )
            {
                if (recLength >= Kwp1281Buffer.Length)
                {   // buffer overflow
                    break;
                }
                if (!HasPreciceTimeout)
                {   // bugfix if we can't detected telegram timeout
                    if (CommAnswerLenProtected[0] > 0 && recLength >= CommAnswerLenProtected[0])
                    {
                        break;
                    }
                }
                if (!ReceiveData(Kwp1281Buffer, recLength, 1, 20, 20))
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
                if (CalcChecksumXor(Kwp1281Buffer, recLength - 1) != Kwp1281Buffer[recLength - 1])
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
                if (InterfaceSetConfigFuncUse(Protocol.Uart, 10400, 8, SerialParity.None, ParAllowBitBang) != InterfaceErrorResult.NoError)
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
            Kwp1281Buffer[0] = 0xFF;
            if (!SendData(Kwp1281Buffer, 1, false))
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
