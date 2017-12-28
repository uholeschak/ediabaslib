using System;
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdCustomAdapterCommon
    {
        // flags
        // ReSharper disable InconsistentNaming
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
        public const byte CAN_PROT_BMW = 0x00;
        public const byte CAN_PROT_TP20 = 0x01;
        public const byte CAN_PROT_ISOTP = 0x02;

        public const byte KWP1281_TIMEOUT = 60;
        // ReSharper restore InconsistentNaming

        public EdiabasNet Ediabas { get; set; }

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

        public int AdapterType { get; set; }

        public int AdapterVersion { get; set; }

        public long LastCommTick { get; set; }

        public EdCustomAdapterCommon()
        {
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
            AdapterType = -1;
            AdapterVersion = -1;
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
                ((CurrentBaudRate < 4000) || (CurrentBaudRate > 25000)))
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
                resultArray[5] = (byte)InterByteTime;   // interbyte time
                resultArray[6] = (byte)(length >> 8);   // telegram length high
                resultArray[7] = (byte)length;          // telegram length low
                Array.Copy(sendData, 0, resultArray, 8, length);
                resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            }
            else
            {
                resultArray[5] = flags2;                // flags 2
                resultArray[6] = (byte)InterByteTime;   // interbyte time
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
            if ((CurrentBaudRate != EdInterfaceBase.BaudAuto) && ((CurrentBaudRate < 4000) || (CurrentBaudRate > 25000)))
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
    }
}
