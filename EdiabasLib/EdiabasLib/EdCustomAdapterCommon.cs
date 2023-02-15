using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdCustomAdapterCommon
    {
        // flags
        // ReSharper disable InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public const byte KLINEF1_PARITY_MASK = 0x7;
        public const byte KLINEF1_PARITY_NONE = 0x0;
        public const byte KLINEF1_PARITY_EVEN = 0x1;
        public const byte KLINEF1_PARITY_ODD = 0x2;
        public const byte KLINEF1_PARITY_MARK = 0x3;
        public const byte KLINEF1_PARITY_SPACE = 0x4;
        public const byte KLINEF1_USE_LLINE = 0x08;
        public const byte KLINEF1_SEND_PULSE = 0x10;
        public const byte KLINEF1_NO_ECHO = 0x20;
        public const byte KLINEF1_FAST_INIT = 0x40;
        public const byte KLINEF1_USE_KLINE = 0x80;

        public const byte KLINEF2_KWP1281_DETECT = 0x01;

        public const byte CANF_NO_ECHO = 0x01;
        public const byte CANF_CAN_ERROR = 0x02;
        public const byte CANF_CONNECT_CHECK = 0x04;
        public const byte CANF_DISCONNECT = 0x08;

        // CAN protocols
        // ReSharper disable once UnusedMember.Global
        public const byte CAN_PROT_BMW = 0x00;
        public const byte CAN_PROT_TP20 = 0x01;
        public const byte CAN_PROT_ISOTP = 0x02;

        public const byte KWP1281_TIMEOUT = 60;
        // ReSharper restore InconsistentNaming

        public const byte EscapeConfRead = 0x01;
        public const byte EscapeConfWrite = 0x02;
        public const byte EscapeCodeDefault = 0xFF;
        public const byte EscapeMaskDefault = 0x80;
        public const byte EscapeXor = 0x55;

        public static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        public static readonly double AdapterVoltageScale = 0.1;
        public static readonly long AdapterVoltageTimeout = 10000;
        public delegate void SendDataDelegate(byte[] buffer, int length);
        public delegate bool ReceiveDataDelegate(byte[] buffer, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog);
        public delegate void DiscardInBufferDelegate();
        public delegate List<byte> ReadInBufferDelegate();

        private readonly int _readTimeoutOffsetLong;
        private readonly int _readTimeoutOffsetShort;
        private readonly int _echoTimeout;
        private readonly bool _echoFailureReconnect;
        private readonly SendDataDelegate _sendDataFunc;
        private readonly ReceiveDataDelegate _receiveDataFunc;
        private readonly DiscardInBufferDelegate _discardInBufferFunc;
        private readonly ReadInBufferDelegate _readInBufferFunc;
        private long _lastVoltageUpdateTime;
        private int _adapterVoltage;

        public EdiabasNet Ediabas { get; set; }

        public bool RawMode { get; set; }

        public EdInterfaceObd.Protocol CurrentProtocol { get; set; }

        public EdInterfaceObd.Protocol ActiveProtocol { get; set; }

        public int CurrentBaudRate { get; set; }

        public int ActiveBaudRate { get; set; }

        public int CurrentWordLength { get; set; }

        public int ActiveWordLength { get; set; }

        public EdInterfaceObd.SerialParity CurrentParity { get; set; }

        public EdInterfaceObd.SerialParity ActiveParity { get; set; }

        public int InterByteTime { get; set; }

        public bool FastInit { get; set; }

        public int CanTxId { get; set; }

        public int CanRxId { get; set; }

        public EdInterfaceObd.CanFlags CanFlags { get; set; }

        public bool ConvertBaudResponse { get; set; }

        public bool AutoKeyByteResponse { get; set; }

        public int IgnitionStatus { get; set; }

        public bool EscapeModeRead { get; set; }

        public bool EscapeModeWrite { get; set; }

        public int AdapterType { get; set; }

        public int AdapterVersion { get; set; }

        public byte[] AdapterSerial { get; set; }

        public int AdapterVoltage
        {
            get
            {
                UpdateAdapterVoltage = true;
                return _adapterVoltage;
            }
            set => _adapterVoltage = value;
        }

        public bool UpdateAdapterVoltage { get; set; }

        public long LastCommTick { get; set; }

        public bool ReconnectRequired { get; set; }

        public int MaxBaudRate
        {
            get
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                return 25000;
            }
        }

        public int MinBaudRate
        {
            get
            {
                if (AdapterType < 0x0002 || AdapterVersion < 0x000D)
                {
                    return 4000;
                }

                return 980;
            }
        }

        public static List<byte[]> AdapterBlackList { get; set; }

        public EdCustomAdapterCommon(SendDataDelegate sendDataFunc, ReceiveDataDelegate receiveDataFunc,
            DiscardInBufferDelegate discardInBufferFunc, ReadInBufferDelegate readInBufferFunc,
            int readTimeoutOffsetLong, int readTimeoutOffsetShort, int echoTimeout, bool echoFailureReconnect)
        {
            _readTimeoutOffsetLong = readTimeoutOffsetLong;
            _readTimeoutOffsetShort = readTimeoutOffsetShort;
            _echoTimeout = echoTimeout;
            _echoFailureReconnect = echoFailureReconnect;
            _sendDataFunc = sendDataFunc;
            _receiveDataFunc = receiveDataFunc;
            _discardInBufferFunc = discardInBufferFunc;
            _readInBufferFunc = readInBufferFunc;
            _lastVoltageUpdateTime = Stopwatch.GetTimestamp();

            RawMode = false;
            CurrentProtocol = EdInterfaceObd.Protocol.Uart;
            ActiveProtocol = EdInterfaceObd.Protocol.Uart;
            CurrentBaudRate = 0;
            ActiveBaudRate = -1;
            CurrentWordLength = 0;
            ActiveWordLength = -1;
            CurrentParity = EdInterfaceObd.SerialParity.None;
            ActiveParity = EdInterfaceObd.SerialParity.None;
            InterByteTime = 0;
            CanTxId = -1;
            CanRxId = -1;
            CanFlags = EdInterfaceObd.CanFlags.Empty;
            IgnitionStatus = -1;
            EscapeModeRead = false;
            EscapeModeWrite = false;
            AdapterType = -1;
            AdapterVersion = -1;
            AdapterSerial = null;
            AdapterVoltage = -1;
            UpdateAdapterVoltage = false;
        }

        public void Init()
        {
            RawMode = false;
            FastInit = false;
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;
            IgnitionStatus = -1;
            EscapeModeRead = false;
            EscapeModeWrite = false;
            AdapterType = -1;
            AdapterVersion = -1;
            AdapterSerial = null;
            AdapterVoltage = -1;
            UpdateAdapterVoltage = false;
            ReconnectRequired = false;
            LastCommTick = DateTime.MinValue.Ticks;
        }

        public byte[] CreateAdapterTelegram(byte[] sendData, int length, bool setDtr)
        {
            ConvertBaudResponse = false;
            if ((AdapterType < 0x0002) || (AdapterVersion < 0x0003))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateAdapterTelegram, invalid adapter: {0} {1}", AdapterType, AdapterVersion);
                }
                return null;
            }
            if ((CurrentBaudRate != 115200) &&
                ((CurrentBaudRate < MinBaudRate) || (CurrentBaudRate > MaxBaudRate)))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateAdapterTelegram, invalid baud rate: {0}", CurrentBaudRate);
                }
                return null;
            }
            if ((InterByteTime < 0) || (InterByteTime > 255))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateAdapterTelegram, invalid inter byte time: {0}", InterByteTime);
                }
                return null;
            }

            byte telType = (byte)((AdapterVersion < 0x0008) ? 0x00 : 0x02);
            byte[] resultArray = new byte[length + ((telType == 0x00) ? 9 : 11)];
            resultArray[0] = 0x00;      // header
            resultArray[1] = telType;   // telegram type

            uint baudHalf;
            byte flags1 = KLINEF1_NO_ECHO;
            if (CurrentBaudRate == 115200)
            {
                baudHalf = 0;
            }
            else
            {
                baudHalf = (uint) (CurrentBaudRate >> 1);
                if (!setDtr)
                {
                    flags1 |= KLINEF1_USE_LLINE;
                }
                flags1 |= CalcParityFlags();
                if (FastInit)
                {
                    flags1 |= KLINEF1_FAST_INIT;
                }
            }

            int interByteTime = InterByteTime;
            byte flags2 = 0x00;
            if (CurrentProtocol == EdInterfaceObd.Protocol.Kwp)
            {
                flags2 |= KLINEF2_KWP1281_DETECT;
                if (interByteTime > 0 && AdapterType >= 0x0002 && AdapterVersion < 0x000B)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Setting interbyte time {0} to 0 for adapter version: {1}", interByteTime, AdapterVersion);
                    interByteTime = 0;
                }
            }

            resultArray[2] = (byte)(baudHalf >> 8);     // baud rate / 2 high
            resultArray[3] = (byte)baudHalf;            // baud rate / 2 low
            resultArray[4] = flags1;                    // flags 1
            if (telType == 0x00)
            {
                resultArray[5] = (byte)interByteTime;   // interbyte time
                resultArray[6] = (byte)(length >> 8);   // telegram length high
                resultArray[7] = (byte)length;          // telegram length low
                Array.Copy(sendData, 0, resultArray, 8, length);
                resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            }
            else
            {
                resultArray[5] = flags2;                // flags 2
                resultArray[6] = (byte)interByteTime;   // interbyte time
                resultArray[7] = KWP1281_TIMEOUT;       // KWP1281 timeout
                resultArray[8] = (byte)(length >> 8);   // telegram length high
                resultArray[9] = (byte)length;          // telegram length low
                Array.Copy(sendData, 0, resultArray, 10, length);
                resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            }
            return resultArray;
        }

        public byte[] CreatePulseTelegram(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;
            if ((AdapterType < 0x0002) || (AdapterVersion < 0x0007))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreatePulseTelegram, invalid adapter: {0} {1}", AdapterType, AdapterVersion);
                }
                return null;
            }
            if ((CurrentBaudRate != EdInterfaceBase.BaudAuto) && ((CurrentBaudRate < MinBaudRate) || (CurrentBaudRate > MaxBaudRate)))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreatePulseTelegram, invalid baud rate: {0}", CurrentBaudRate);
                }
                return null;
            }
            if ((length < 0) || (length > 64))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreatePulseTelegram, invalid length: {0}", length);
                }
                return null;
            }
            if ((pulseWidth < 0) || (pulseWidth > 255))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreatePulseTelegram, invalid pulse width: {0}", pulseWidth);
                }
                return null;
            }
            ConvertBaudResponse = (AdapterVersion < 0x0008) && (CurrentBaudRate == EdInterfaceBase.BaudAuto);

            byte telType = (byte)((AdapterVersion < 0x0008) ? 0x00 : 0x02);
            int dataBytes = (length + 7) >> 3;
            byte[] resultArray = new byte[dataBytes + 2 + 1 + ((telType == 0x00) ? 9 : 11)];
            resultArray[0] = 0x00;      // header
            resultArray[1] = telType;   // telegram type

            uint baudHalf = (uint)(CurrentBaudRate >> 1);
            byte flags1 = KLINEF1_SEND_PULSE | KLINEF1_NO_ECHO;
            if (bothLines)
            {
                flags1 |= KLINEF1_USE_LLINE | KLINEF1_USE_KLINE;
            }
            else if (!setDtr)
            {
                flags1 |= KLINEF1_USE_LLINE;
            }
            flags1 |= CalcParityFlags();

            byte flags2 = 0x00;
            if (CurrentProtocol == EdInterfaceObd.Protocol.Kwp)
            {
                flags2 |= KLINEF2_KWP1281_DETECT;
            }

            resultArray[2] = (byte)(baudHalf >> 8);     // baud rate / 2 high
            resultArray[3] = (byte)baudHalf;            // baud rate / 2 low
            resultArray[4] = flags1;                    // flags 1
            if (telType == 0x00)
            {
                resultArray[5] = (byte) InterByteTime; // interbyte time
                resultArray[6] = 0x00; // telegram length high
                resultArray[7] = (byte) (dataBytes + 2 + 1); // telegram length low
                resultArray[8] = (byte) pulseWidth;
                resultArray[9] = (byte) length;
                for (int i = 0; i < dataBytes; i++)
                {
                    resultArray[10 + i] = (byte) (dataBits >> (i << 3));
                }
            }
            else
            {
                resultArray[5] = flags2;                // flags 2
                resultArray[6] = (byte)InterByteTime;   // interbyte time
                resultArray[7] = KWP1281_TIMEOUT;       // KWP1281 timeout
                resultArray[8] = 0x00;                  // telegram length high
                resultArray[9] = (byte)(dataBytes + 2 + 1); // telegram length low
                resultArray[10] = (byte)pulseWidth;
                resultArray[11] = (byte)length;
                for (int i = 0; i < dataBytes; i++)
                {
                    resultArray[12 + i] = (byte)(dataBits >> (i << 3));
                }
            }
            resultArray[resultArray.Length - 2] = (byte)autoKeyByteDelay;   // W4 auto key byte response delay [ms], 0 = off
            resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            return resultArray;
        }

        public byte[] CreateCanTelegram(byte[] sendData, int length)
        {
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;
            if ((AdapterType < 0x0002) || (AdapterVersion < 0x0008))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateCanTelegram, invalid adapter: {0} {1}", AdapterType, AdapterVersion);
                }
                return null;
            }
            byte protocol;
            switch (CurrentProtocol)
            {
                case EdInterfaceObd.Protocol.Tp20:
                    protocol = CAN_PROT_TP20;
                    break;

                case EdInterfaceObd.Protocol.IsoTp:
                    if (AdapterVersion < 0x0009)
                    {
                        if (Ediabas != null)
                        {
                            Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ISO-TP not supported by adapter");
                        }
                        return null;
                    }
                    if (CanTxId < 0 || CanRxId < 0)
                    {
                        if (Ediabas != null)
                        {
                            Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No CAN IDs present for ISO-TP");
                        }
                        return null;
                    }
                    protocol = CAN_PROT_ISOTP;
                    break;

                default:
                    if (Ediabas != null)
                    {
                        Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateCanTelegram, invalid protocol: {0}", CurrentProtocol);
                    }
                    return null;
            }
            if ((CurrentBaudRate != 500000) && (CurrentBaudRate != 100000))
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateCanTelegram, invalid baud rate: {0}", CurrentBaudRate);
                }
                return null;
            }

            byte telType = (byte)((AdapterVersion < 0x0009) ? 0x01 : 0x03);
            byte[] resultArray = new byte[length + ((telType == 0x01) ? 11 : 14)];
            resultArray[0] = 0x00;      // header
            resultArray[1] = telType;   // telegram type

            byte flags = CANF_NO_ECHO | CANF_CAN_ERROR;
            if ((CanFlags & EdInterfaceObd.CanFlags.BusCheck) != 0x00)
            {
                flags |= CANF_CONNECT_CHECK;
            }
            if ((CanFlags & EdInterfaceObd.CanFlags.Disconnect) != 0x00)
            {
                flags |= CANF_DISCONNECT;
            }

            resultArray[2] = protocol;              // protocol
            resultArray[3] = (byte)((CurrentBaudRate == 500000) ? 0x01 : 0x09);     // baud rate
            resultArray[4] = flags;                 // flags
            if (protocol == CAN_PROT_TP20)
            {
                resultArray[5] = 0x0F;                  // block size
                resultArray[6] = 0x0A;                  // packet interval (1ms)
                resultArray[7] = 1000 / 10;             // idle time (10ms)
            }
            else
            {
                resultArray[5] = 0x00;                  // block size (off)
                resultArray[6] = 0x00;                  // separation time (off)
                resultArray[7] = (byte)(CanTxId >> 8);  // CAN TX ID high
                resultArray[8] = (byte)CanTxId;         // CAN TX ID low
                resultArray[9] = (byte)(CanRxId >> 8);  // CAN RX ID high
                resultArray[10] = (byte)CanRxId;        // CAN RX ID low
            }
            if (telType == 0x01)
            {
                resultArray[8] = (byte)(length >> 8);   // telegram length high
                resultArray[9] = (byte)length;          // telegram length low
                Array.Copy(sendData, 0, resultArray, 10, length);
                resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            }
            else
            {
                resultArray[11] = (byte)(length >> 8);  // telegram length high
                resultArray[12] = (byte)length;         // telegram length low
                Array.Copy(sendData, 0, resultArray, 13, length);
                resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            }
            return resultArray;
        }

        public bool IsFastInit(UInt64 dataBits, int length, int pulseWidth)
        {
            return (dataBits == 0x02) && (length == 2) && (pulseWidth == 25);
        }

        public static void ConvertStdBaudResponse(byte[] receiveData, int offset)
        {
            int baudRate = 0;
            if (receiveData[offset] == 0x55)
            {
                baudRate = 9600;
            }
            else if ((receiveData[offset] & 0x87) == 0x85)
            {
                baudRate = 10400;
            }
            baudRate /= 2;
            receiveData[offset] = (byte)(baudRate >> 8);
            receiveData[offset + 1] = (byte)baudRate;
        }

        public static byte CalcChecksumBmwFast(byte[] data, int offset, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i + offset];
            }
            return sum;
        }

        public static bool IsAdapterBlacklisted(byte[] adapterSerial)
        {
            if (adapterSerial == null || AdapterBlackList == null)
            {
                return false;
            }

            foreach (byte[] serial in AdapterBlackList)
            {
                if (adapterSerial.SequenceEqual(serial))
                {
                    return true;
                }
            }
            return false;
        }

        public byte CalcParityFlags()
        {
            byte flags = 0x00;
            switch (CurrentParity)
            {
                case EdInterfaceObd.SerialParity.None:
                    flags |= KLINEF1_PARITY_NONE;
                    break;

                case EdInterfaceObd.SerialParity.Odd:
                    flags |= KLINEF1_PARITY_ODD;
                    break;

                case EdInterfaceObd.SerialParity.Even:
                    flags |= KLINEF1_PARITY_EVEN;
                    break;

                case EdInterfaceObd.SerialParity.Mark:
                    flags |= KLINEF1_PARITY_MARK;
                    break;

                case EdInterfaceObd.SerialParity.Space:
                    flags |= KLINEF1_PARITY_SPACE;
                    break;
            }
            return flags;
        }

        public void UpdateActiveSettings()
        {
            ActiveProtocol = CurrentProtocol;
            ActiveBaudRate = CurrentBaudRate;
            ActiveWordLength = CurrentWordLength;
            ActiveParity = CurrentParity;
        }

        public bool SettingsUpdateRequired()
        {
            switch (CurrentProtocol)
            {
                case EdInterfaceObd.Protocol.Tp20:
                case EdInterfaceObd.Protocol.IsoTp:
                    return false;
            }
            if (CurrentBaudRate == 115200)
            {
                return false;
            }
            if (ActiveBaudRate == EdInterfaceBase.BaudAuto)
            {
                return false;
            }
            if (CurrentBaudRate == ActiveBaudRate &&
                CurrentWordLength == ActiveWordLength &&
                CurrentParity == ActiveParity)
            {
                return false;
            }
            return true;
        }

        public bool UpdateAdapterInfo(bool forceUpdate = false)
        {
            bool voltageUpdate = false;
            if (!forceUpdate && AdapterType >= 0)
            {
                if (UpdateAdapterVoltage && Stopwatch.GetTimestamp() - _lastVoltageUpdateTime > AdapterVoltageTimeout * TickResolMs)
                {
                    voltageUpdate = true;
                }

                if (!voltageUpdate)
                {
                    // only read once
                    return true;
                }
            }

            if (Ediabas != null)
            {
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateAdapterInfo: ForceUpdate={0}, VoltageUpdate={1}", forceUpdate, voltageUpdate);
            }

            if (!voltageUpdate)
            {
                IgnitionStatus = -1;
                AdapterType = -1;
                AdapterSerial = null;
            }
            AdapterVoltage = -1;

            try
            {
                for (int telType = 0; telType < 5; telType++)
                {
                    if (voltageUpdate && telType != 4)
                    {
                        continue;
                    }

                    int respLen = 0;
                    byte[] testTel = null;
                    switch (telType)
                    {
                        case 0:
                            // read ignition
                            respLen = 6;
                            testTel = new byte[] { 0x82, 0xF1, 0xF1, 0xFE, 0xFE, 0x00 };
                            break;

                        case 1:
                        {
                            // escape mode
                            respLen = 8;
                            int modeValue = 0x00;
                            if (EscapeModeRead)
                            {
                                modeValue |= EscapeConfRead;
                            }

                            if (EscapeModeWrite)
                            {
                                modeValue |= EscapeConfWrite;
                            }

                            testTel = new byte[] { 0x84, 0xF1, 0xF1, 0x06, (byte)(modeValue ^ EscapeXor), EscapeCodeDefault ^ EscapeXor, EscapeMaskDefault ^ EscapeXor, 0x00 };
                            break;
                        }

                        case 2:
                            // read firmware version
                            respLen = 9;
                            testTel = new byte []{ 0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x00 };
                            break;

                        case 3:
                            if (AdapterType < 0x0002)
                            {   // no id support
                                break;
                            }
                            respLen = 13;
                            testTel = new byte[] { 0x82, 0xF1, 0xF1, 0xFB, 0xFB, 0x00 };
                            break;

                        case 4:
                            if (AdapterType < 0x0002)
                            {   // no voltage support
                                break;
                            }
                            respLen = 6;
                            testTel = new byte[] { 0x82, 0xF1, 0xF1, 0xFC, 0xFC, 0x00 };
                            break;
                    }

                    if (testTel == null)
                    {
                        break;
                    }

                    testTel[testTel.Length - 1] = CalcChecksumBmwFast(testTel, 0, testTel.Length - 1);
                    if (Ediabas != null)
                    {
                        Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, testTel, 0, testTel.Length, "AdSend");
                    }

                    _discardInBufferFunc();
                    _sendDataFunc(testTel, testTel.Length);
                    LastCommTick = Stopwatch.GetTimestamp();

                    List<byte> responseList = new List<byte>();
                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        List<byte> newList = _readInBufferFunc();
                        if (newList.Count > 0)
                        {
                            responseList.AddRange(newList);
                            startTime = Stopwatch.GetTimestamp();
                        }
                        if (responseList.Count >= testTel.Length + respLen)
                        {
                            if (Ediabas != null)
                            {
                                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, responseList.ToArray(), 0, responseList.Count, "AdResp");
                            }

                            bool validEcho = !testTel.Where((t, i) => responseList[i] != t).Any();
                            if (!validEcho)
                            {
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateAdapterInfo Type={0}: Echo invalid", telType);
                                }
                                return false;
                            }
                            if (CalcChecksumBmwFast(responseList.ToArray(), testTel.Length, respLen - 1) !=
                                responseList[testTel.Length + respLen - 1])
                            {
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateAdapterInfo Type={0}: Checksum invalid", telType);
                                }
                                return false;
                            }
                            switch (telType)
                            {
                                case 0:
                                    IgnitionStatus = responseList[testTel.Length + 4];
                                    break;

                                case 1:
                                {
                                    int modeValue = (responseList[testTel.Length + 4] ^ EscapeXor);
                                    EscapeModeRead = (modeValue & EscapeConfRead) != 0x00;
                                    EscapeModeWrite = (modeValue & EscapeConfWrite) != 0x00;
                                    break;
                                }

                                case 2:
                                    AdapterType = responseList[testTel.Length + 5] + (responseList[testTel.Length + 4] << 8);
                                    AdapterVersion = responseList[testTel.Length + 7] + (responseList[testTel.Length + 6] << 8);
                                    break;

                                case 3:
                                    AdapterSerial = responseList.GetRange(testTel.Length + 4, 8).ToArray();
                                    break;

                                case 4:
                                    AdapterVoltage = responseList[testTel.Length + 4];
                                    _lastVoltageUpdateTime = Stopwatch.GetTimestamp();
                                    break;
                            }
                            break;
                        }
                        if (Stopwatch.GetTimestamp() - startTime > _readTimeoutOffsetLong * TickResolMs)
                        {
                            if (Ediabas != null)
                            {
                                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, responseList.ToArray(), 0, responseList.Count, "AdResp");
                                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateAdapterInfo Type={0}: Response timeout", telType);
                            }
                            bool failure = true;
                            if (responseList.Count >= testTel.Length)
                            {
                                bool validEcho = !testTel.Where((t, i) => responseList[i] != t).Any();
                                if (validEcho)
                                {
                                    switch (telType)
                                    {
                                        case 0:
                                            AdapterType = 0;
                                            break;

                                        case 1:
                                            EscapeModeRead = false;
                                            EscapeModeWrite = false;
                                            failure = false;
                                            break;
                                    }
                                }
                            }

                            if (failure)
                            {
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateAdapterInfo Type={0}: Response failure", telType);
                                }
                                return false;
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", EdiabasNet.GetExceptionText(ex));
                ReconnectRequired = true;
                return false;
            }

            if (Ediabas != null)
            {
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "IgnitionStatus: {0:X02}", IgnitionStatus);
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EscapeModeRead: {0}", EscapeModeRead);
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "EscapeModeWrite: {0}", EscapeModeWrite);
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "AdapterType: {0}", AdapterType);
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "AdapterVersion: {0}.{1}", AdapterVersion >> 8, AdapterVersion & 0xFF);
                if (AdapterSerial != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "AdapterSerial: {0}", BitConverter.ToString(AdapterSerial).Replace("-", ""));
                }
                if (AdapterVoltage >= 0)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "AdapterVoltage: {0,4:0.0}", AdapterVoltage * AdapterVoltageScale);
                }
            }

            return true;
        }

        public EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            CurrentProtocol = protocol;
            CurrentBaudRate = baudRate;
            CurrentWordLength = dataBits;
            CurrentParity = parity;
            FastInit = false;
            ConvertBaudResponse = false;
            return EdInterfaceObd.InterfaceErrorResult.NoError;
        }

        public bool InterfaceSetInterByteTime(int time)
        {
            InterByteTime = time;
            return true;
        }

        public bool InterfaceSetCanIds(int canTxId, int canRxId, EdInterfaceObd.CanFlags canFlags)
        {
            CanTxId = canTxId;
            CanRxId = canRxId;
            CanFlags = canFlags;
            return true;
        }

        public bool InterfaceHasAutoKwp1281()
        {
            if (!UpdateAdapterInfo())
            {
                return false;
            }
            if (AdapterVersion < 0x0008)
            {
                return false;
            }
            return true;
        }

        public int? InterfaceAdapterVersion()
        {
            if (RawMode)
            {
                return null;
            }

            if (!UpdateAdapterInfo())
            {
                return null;
            }

            if (AdapterVersion < 0)
            {
                return null;
            }
            return AdapterVersion;
        }

        public byte[] InterfaceAdapterSerial()
        {
            if (RawMode)
            {
                return null;
            }

            return AdapterSerial;
        }

        public double? InterfaceAdapterVoltage()
        {
            if (RawMode)
            {
                return null;
            }

            if (AdapterVoltage < 0)
            {
                return null;
            }
            return AdapterVoltage * AdapterVoltageScale;
        }

        public bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;

            try
            {
                if (AdapterSerial != null)
                {
                    if (IsAdapterBlacklisted(AdapterSerial))
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Adapter is blacklisted!");
                        return false;
                    }
                }

                if ((CurrentProtocol == EdInterfaceObd.Protocol.Tp20) ||
                    (CurrentProtocol == EdInterfaceObd.Protocol.IsoTp))
                {
                    UpdateAdapterInfo();
                    byte[] adapterTel = CreateCanTelegram(sendData, length);
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    _sendDataFunc(adapterTel, adapterTel.Length);
                    LastCommTick = Stopwatch.GetTimestamp();
                    UpdateActiveSettings();
                    return true;
                }
                if (RawMode || CurrentBaudRate == 115200)
                {
                    // BMW-FAST
                    if (!RawMode)
                    {
                        UpdateAdapterInfo();
                    }
                    if (sendData.Length >= 5 && sendData[1] == 0xF1 && sendData[2] == 0xF1 && sendData[3] == 0xFA && sendData[4] == 0xFA)
                    {   // read clamp status
                        if (RawMode || AdapterVersion < 0x000A)
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Read clamp status not supported");
                            return false;
                        }
                    }

                    if (sendData.Length >= 6 && (sendData[0] & 0x3F) == 0x00 && sendData[3] == 0x00)
                    {   // long telegram format
                        if (!RawMode && AdapterVersion < 0x000C)
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Long telegrams not supported");
                            return false;
                        }
                    }

                    _sendDataFunc(sendData, length);
                    LastCommTick = Stopwatch.GetTimestamp();
                    // remove echo
                    byte[] receiveData = new byte[length];
                    if (!InterfaceReceiveData(receiveData, 0, length, _echoTimeout, _echoTimeout, null))
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Echo not received");
                        if (_echoFailureReconnect)
                        {
                            ReconnectRequired = true;
                        }
                        return false;
                    }
                    for (int i = 0; i < length; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Echo incorrect");
                            if (_echoFailureReconnect)
                            {
                                ReconnectRequired = true;
                            }
                            return false;
                        }
                    }
                }
                else
                {
                    UpdateAdapterInfo();
                    byte[] adapterTel = CreateAdapterTelegram(sendData, length, setDtr);
                    FastInit = false;
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    _sendDataFunc(adapterTel, adapterTel.Length);
                    LastCommTick = Stopwatch.GetTimestamp();
                    UpdateActiveSettings();
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", EdiabasNet.GetExceptionText(ex));
                ReconnectRequired = true;
                return false;
            }
            return true;
        }

        public bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            int timeoutOffset = _readTimeoutOffsetLong;
            if ((_readTimeoutOffsetShort >= 0) && (CurrentBaudRate >= 3000))
            {
                if (((Stopwatch.GetTimestamp() - LastCommTick) < 100 * TickResolMs) && (timeout < 100))
                {
                    timeoutOffset = _readTimeoutOffsetShort;
                }
            }

            //Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Timeout offset: {0}", timeoutOffset);
            timeout += timeoutOffset;
            timeoutTelEnd += timeoutOffset;

            bool convertBaudResponse = ConvertBaudResponse;
            bool autoKeyByteResponse = AutoKeyByteResponse;
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;

            try
            {
                if (!RawMode && SettingsUpdateRequired())
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "InterfaceReceiveData, update settings");
                    UpdateAdapterInfo();
                    byte[] adapterTel = CreatePulseTelegram(0, 0, 0, false, false, 0);
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    _sendDataFunc(adapterTel, adapterTel.Length);
                    LastCommTick = Stopwatch.GetTimestamp();
                    UpdateActiveSettings();
                }

                if (convertBaudResponse && length == 2)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Convert baud response");
                    length = 1;
                    AutoKeyByteResponse = true;
                }

                if (!_receiveDataFunc(receiveData, offset, length, timeout, timeoutTelEnd, ediabasLog))
                {
                    return false;
                }

                if (convertBaudResponse)
                {
                    ConvertStdBaudResponse(receiveData, offset);
                }

                if (autoKeyByteResponse && length == 2)
                {   // auto key byte response for old adapter
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Auto key byte response");
                    byte[] keyByteResponse = { (byte)~receiveData[offset + 1] };
                    byte[] adapterTel = CreateAdapterTelegram(keyByteResponse, keyByteResponse.Length, true);
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    _sendDataFunc(adapterTel, adapterTel.Length);
                    LastCommTick = Stopwatch.GetTimestamp();
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", EdiabasNet.GetExceptionText(ex));
                ReconnectRequired = true;
                return false;
            }
            return true;
        }

        public bool InterfaceSendPulse(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            ConvertBaudResponse = false;
            try
            {
                UpdateAdapterInfo();
                FastInit = IsFastInit(dataBits, length, pulseWidth);
                if (FastInit)
                {
                    // send next telegram with fast init
                    return true;
                }
                byte[] adapterTel = CreatePulseTelegram(dataBits, length, pulseWidth, setDtr, bothLines, autoKeyByteDelay);
                if (adapterTel == null)
                {
                    return false;
                }
                _sendDataFunc(adapterTel, adapterTel.Length);
                LastCommTick = Stopwatch.GetTimestamp();
                UpdateActiveSettings();
                Thread.Sleep(pulseWidth * length);
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", EdiabasNet.GetExceptionText(ex));
                ReconnectRequired = true;
                return false;
            }
            return true;
        }
    }
}
