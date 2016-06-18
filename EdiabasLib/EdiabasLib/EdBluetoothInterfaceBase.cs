using System;
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdBluetoothInterfaceBase
    {
        // flags
        // ReSharper disable InconsistentNaming
        public static byte KLINEF_PARITY_MASK = 0x7;
        public static byte KLINEF_PARITY_NONE = 0x0;
        public static byte KLINEF_PARITY_EVEN = 0x1;
        public static byte KLINEF_PARITY_ODD = 0x2;
        public static byte KLINEF_PARITY_MARK = 0x3;
        public static byte KLINEF_PARITY_SPACE = 0x4;
        public static byte KLINEF_USE_LLINE = 0x08;
        public static byte KLINEF_SEND_PULSE = 0x10;
        public static byte KLINEF_NO_ECHO = 0x20;
        public static byte KLINEF_FAST_INIT = 0x40;
        public static byte KLINEF_USE_KLINE = 0x80;

        public static byte CANF_NO_ECHO = 0x01;
        public static byte CANF_CAN_ERROR = 0x02;
        public static byte CANF_CONNECT_CHECK = 0x04;
        public static byte CANF_DISCONNECT = 0x08;

        // CAN protocols
        public static byte CAN_PROT_BMW = 0x00;
        public static byte CAN_PROT_TP20 = 0x01;
        // ReSharper restore InconsistentNaming

        public static EdiabasNet Ediabas { get; set; }

        public static EdInterfaceObd.Protocol CurrentProtocol { get; protected set; }

        public static EdInterfaceObd.Protocol ActiveProtocol { get; protected set; }

        public static int CurrentBaudRate { get; protected set; }

        public static int ActiveBaudRate { get; protected set; }

        public static int CurrentWordLength { get; protected set; }

        public static int ActiveWordLength { get; protected set; }

        public static EdInterfaceObd.SerialParity CurrentParity { get; protected set; }

        public static EdInterfaceObd.SerialParity ActiveParity { get; protected set; }

        public static int InterByteTime { get; protected set; }

        public static bool FastInit { get; protected set; }

        public static bool ConvertBaudResponse { get; protected set; }

        public static bool AutoKeyByteResponse { get; protected set; }

        public static int AdapterType { get; protected set; }

        public static int AdapterVersion { get; protected set; }

        public static long LastCommTick { get; protected set; }

        static EdBluetoothInterfaceBase()
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
            AdapterType = -1;
            AdapterVersion = -1;
        }

        public static byte[] CreateAdapterTelegram(byte[] sendData, int length, bool setDtr)
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

            byte[] resultArray = new byte[length + 9];
            resultArray[0] = 0x00;   // header
            resultArray[1] = 0x00;   // telegram type

            uint baudHalf;
            byte flags = KLINEF_NO_ECHO;
            if (CurrentBaudRate == 115200)
            {
                baudHalf = 0;
            }
            else
            {
                baudHalf = (uint) (CurrentBaudRate >> 1);
                if (!setDtr)
                {
                    flags |= KLINEF_USE_LLINE;
                }
                flags |= CalcParityFlags();
                if (FastInit)
                {
                    flags |= KLINEF_FAST_INIT;
                }
            }
            resultArray[2] = (byte)(baudHalf >> 8);     // baud rate / 2 high
            resultArray[3] = (byte)baudHalf;            // baud rate / 2 low
            resultArray[4] = flags;                 // flags
            resultArray[5] = (byte)InterByteTime;   // interbyte time
            resultArray[6] = (byte)(length >> 8);   // telegram length high
            resultArray[7] = (byte)length;          // telegram length low
            Array.Copy(sendData, 0, resultArray, 8, length);
            resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            return resultArray;
        }

        public static byte[] CreatePulseTelegram(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines)
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

            int dataBytes = (length + 7) >> 3;
            byte[] resultArray = new byte[dataBytes + 2 + 9];
            resultArray[0] = 0x00;   // header
            resultArray[1] = 0x00;   // telegram type

            uint baudHalf = (uint)(CurrentBaudRate >> 1);
            byte flags = (byte)(KLINEF_SEND_PULSE | KLINEF_NO_ECHO);
            if (bothLines)
            {
                flags |= (byte)(KLINEF_USE_LLINE | KLINEF_USE_KLINE);
            }
            else if (!setDtr)
            {
                flags |= KLINEF_USE_LLINE;
            }
            flags |= CalcParityFlags();
            resultArray[2] = (byte)(baudHalf >> 8);     // baud rate / 2 high
            resultArray[3] = (byte)baudHalf;            // baud rate / 2 low
            resultArray[4] = flags;                 // flags
            resultArray[5] = (byte)InterByteTime;   // interbyte time
            resultArray[6] = 0x00;                  // telegram length high
            resultArray[7] = (byte)(dataBytes + 2); // telegram length low
            resultArray[8] = (byte)pulseWidth;
            resultArray[9] = (byte)length;
            for (int i = 0; i < dataBytes; i++)
            {
                resultArray[10 + i] = (byte)(dataBits >> (i << 3));
            }
            resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            return resultArray;
        }

        public static byte[] CreateCanTelegram(byte[] sendData, int length)
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
            if (CurrentProtocol != EdInterfaceObd.Protocol.Tp20)
            {
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

            byte[] resultArray = new byte[length + 11];
            resultArray[0] = 0x00;   // header
            resultArray[1] = 0x01;   // telegram type

            byte flags = (byte)(CANF_NO_ECHO | CANF_CAN_ERROR);
            if (length == 5 && sendData[0] == 0x01)
            {
                switch (sendData[3])
                {
                    case 0x00:  // connect check
                        flags |= CANF_CONNECT_CHECK;
                        break;

                    default:  // disconnect
                        flags |= CANF_DISCONNECT;
                        break;
                }
                sendData[0] = 0x81;
            }
            resultArray[2] = CAN_PROT_TP20;         // protocol TP2.0
            resultArray[3] = (byte)((CurrentBaudRate == 500000) ? 0x01 : 0x09);     // baud rate
            resultArray[4] = flags;                 // flags
            resultArray[5] = 0x0F;                  // block size
            resultArray[6] = 0x0A;                  // packet interval (1ms)
            resultArray[7] = 1000 / 10;             // idle time (10ms)
            resultArray[8] = (byte)(length >> 8);   // telegram length high
            resultArray[9] = (byte)length;          // telegram length low
            Array.Copy(sendData, 0, resultArray, 10, length);
            resultArray[resultArray.Length - 1] = CalcChecksumBmwFast(resultArray, 0, resultArray.Length - 1);
            return resultArray;
        }

        public static bool IsFastInit(UInt64 dataBits, int length, int pulseWidth)
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

        static public byte CalcChecksumBmwFast(byte[] data, int offset, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i + offset];
            }
            return sum;
        }

        public static byte CalcParityFlags()
        {
            byte flags = 0x00;
            switch (CurrentParity)
            {
                case EdInterfaceObd.SerialParity.None:
                    flags |= KLINEF_PARITY_NONE;
                    break;

                case EdInterfaceObd.SerialParity.Odd:
                    flags |= KLINEF_PARITY_ODD;
                    break;

                case EdInterfaceObd.SerialParity.Even:
                    flags |= KLINEF_PARITY_EVEN;
                    break;

                case EdInterfaceObd.SerialParity.Mark:
                    flags |= KLINEF_PARITY_MARK;
                    break;

                case EdInterfaceObd.SerialParity.Space:
                    flags |= KLINEF_PARITY_SPACE;
                    break;
            }
            return flags;
        }

        public static void UpdateActiveSettings()
        {
            ActiveProtocol = CurrentProtocol;
            ActiveBaudRate = CurrentBaudRate;
            ActiveWordLength = CurrentWordLength;
            ActiveParity = CurrentParity;
        }

        public static bool SettingsUpdateRequired()
        {
            if (CurrentProtocol != EdInterfaceObd.Protocol.Uart)
            {
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
