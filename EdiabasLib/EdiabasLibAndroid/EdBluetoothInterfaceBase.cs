using System;

// ReSharper disable once CheckNamespace
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
        public static byte KLINEF_AUTO_LLINE = 0x08;
        public static byte KLINEF_SET_LLINE = 0x10;
        public static byte KLINEF_SET_KLINE = 0x20;
        public static byte KLINEF_NO_ECHO = 0x40;
        // ReSharper restore InconsistentNaming

        public static int CurrentBaudRate { get; protected set; }

        public static int CurrentWordLength { get; protected set; }

        public static EdInterfaceObd.SerialParity CurrentParity { get; protected set; }

        public static int InterByteTime { get; protected set; }

        static EdBluetoothInterfaceBase()
        {
            CurrentBaudRate = 0;
            CurrentWordLength = 0;
            CurrentParity = EdInterfaceObd.SerialParity.None;
            InterByteTime = 0;
        }

        public static byte[] CreateAdapterTelegram(byte[] sendData, int length, bool setDtr)
        {
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
                if (setDtr)
                {
                    flags |= KLINEF_AUTO_LLINE;
                }
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
            }
            resultArray[2] = (byte)(baudHalf >> 8);     // baud rate / 2 high
            resultArray[3] = (byte)baudHalf;            // baud rate / 2 low
            resultArray[4] = flags;                 // flags
            resultArray[5] = (byte)InterByteTime;   // interbyte time
            resultArray[6] = (byte)(length >> 8);   // telegram length high
            resultArray[7] = (byte)length;          // telegram length low
            Array.Copy(sendData, 0, resultArray, 8, length);
            byte checkSum = 0x00;
            for (int i = 0; i < resultArray.Length - 1; i++)
            {
                checkSum += resultArray[i];
            }
            resultArray[resultArray.Length - 1] = checkSum;
            return resultArray;
        }
    }
}
