using System;

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
        // ReSharper restore InconsistentNaming

        public static int CurrentBaudRate { get; protected set; }

        public static int ActiveBaudRate { get; protected set; }

        public static int CurrentWordLength { get; protected set; }

        public static int ActiveWordLength { get; protected set; }

        public static EdInterfaceObd.SerialParity CurrentParity { get; protected set; }

        public static EdInterfaceObd.SerialParity ActiveParity { get; protected set; }

        public static int InterByteTime { get; protected set; }

        public static int AdapterType { get; protected set; }

        public static int AdapterVersion { get; protected set; }

        static EdBluetoothInterfaceBase()
        {
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
            if ((AdapterType < 0x0002) || (AdapterType > 0x0003) ||
                (AdapterVersion < 0x0002))
            {
                return null;
            }
            if ((CurrentBaudRate != 115200) &&
                ((CurrentBaudRate < 9600) || (CurrentBaudRate > 19200)))
            {
                return null;
            }
            if ((InterByteTime < 0) || (InterByteTime > 255))
            {
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

        public static byte[] CreatePulseTelegram(UInt64 dataBits, int length, int pulseWidth, bool setDtr)
        {
            if ((AdapterType < 0x0002) || (AdapterType > 0x0003) ||
                (AdapterVersion < 0x0002))
            {
                return null;
            }
            if ((CurrentBaudRate < 9600) || (CurrentBaudRate > 19200))
            {
                return null;
            }
            if ((length < 0) || (length > 64))
            {
                return null;
            }
            if ((pulseWidth < 0) || (pulseWidth > 255))
            {
                return null;
            }
            int dataBytes = (length + 7) >> 3;
            byte[] resultArray = new byte[dataBytes + 2 + 9];
            resultArray[0] = 0x00;   // header
            resultArray[1] = 0x00;   // telegram type

            uint baudHalf = (uint)(CurrentBaudRate >> 1);
            byte flags = (byte)(KLINEF_SEND_PULSE | KLINEF_NO_ECHO);
            if (!setDtr)
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
            ActiveBaudRate = CurrentBaudRate;
            ActiveWordLength = CurrentWordLength;
            ActiveParity = CurrentParity;
        }

        public static bool SettingsUpdateRequired()
        {
            if (CurrentBaudRate == 115200)
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
