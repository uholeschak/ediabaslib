//#define USE_BITBANG

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using Ftdi;

namespace EdiabasLib
{
    static class EdFtdiInterface
    {
        public const string PortID = "FTDI";
        private const int writeTimeout = 500;       // write timeout [ms]
        private const int readTimeoutCeMin = 500;   // min read timeout for CE [ms]
        private const int usbBufferSizeStd = 0x1000;    // standard usb buffer size
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private static readonly long tickResolMs = Stopwatch.Frequency / 1000;
        private static IntPtr handleFtdi = (IntPtr)0;
        private static int currentBaudRate = 0;
        private static int currentWordLength = 0;
        private static Parity currentParity = Parity.None;
#if USE_BITBANG
        [Flags]
        private enum bitBangBits
        {
            TXD = 0x01,     // not inverted
            RXD = 0x02,     // not inverted
            RTS = 0x04,     // inverted
            CTS = 0x08,     // inverted
            DTR = 0x10,     // inverted
            DSR = 0x20,     // inverted
            CDC = 0x40,     // inverted
            RI = 0x80,      // inverted
        }
        // tested range: 1-59
        // good values: 27, 29!, 33, 35
        private const int bitBangDivisor = 29;  // only odd values allowed!
        private const int bitBangRecBufferSize = 0x2000;
        private static bool bitBangMode = false;
        private static bitBangBits bitBangOutput = bitBangBits.DTR | bitBangBits.RTS | bitBangBits.TXD;
        private static int bitBangBitsPerSendByte = 0;
        private static int bitBangBitsPerRecByte = 0;
        private static byte[] bitBangSendBuffer;
        private static byte[][] bitBangRecBuffer;
        private static int recBufReadPos = 0;
        private static int recBufReadIndex = 0;
        private static int recBufLastIndex = -1;
#endif

        static EdFtdiInterface()
        {
#if USE_BITBANG
            bitBangSendBuffer = new byte[0x10000];

            bitBangRecBuffer = new byte[2][];
            bitBangRecBuffer[0] = new byte[bitBangRecBufferSize];
            bitBangRecBuffer[1] = new byte[bitBangRecBufferSize];
#endif
        }

        public static IntPtr HandleFtdi
        {
            get { return handleFtdi; }
        }

        public static int CurrentBaudRate
        {
            get { return currentBaudRate; }
        }

        public static int CurrentWordLength
        {
            get { return currentWordLength; }
        }

        public static Parity CurrentParity
        {
            get { return currentParity; }
        }

        public static bool InterfaceConnect(string port)
        {
            if (handleFtdi != (IntPtr)0)
            {
                return true;
            }
            try
            {
                Ftd2xx.FT_STATUS ftStatus;
                if (!port.StartsWith(PortID, StringComparison.OrdinalIgnoreCase))
                {
                    InterfaceDisconnect();
                    return false;
                }
                string portData = port.Remove(0, PortID.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    if (portData.StartsWith(":SER=", StringComparison.OrdinalIgnoreCase))
                    {   // serial number
                        string id = portData.Remove(0, 5);
                        ftStatus = Ftd2xx.FT_OpenEx(id, Ftd2xx.FT_OPEN_BY_SERIAL_NUMBER, out handleFtdi);
                    }
                    else if (portData.StartsWith(":DESC=", StringComparison.OrdinalIgnoreCase))
                    {   // description
                        string id = portData.Remove(0, 6);
                        ftStatus = Ftd2xx.FT_OpenEx(id, Ftd2xx.FT_OPEN_BY_DESCRIPTION, out handleFtdi);
                    }
                    else if (portData.StartsWith(":LOC=", StringComparison.OrdinalIgnoreCase))
                    {   // location
                        long loc = EdiabasNet.StringToValue(portData.Remove(0, 5));
                        ftStatus = Ftd2xx.FT_OpenEx((IntPtr)loc, Ftd2xx.FT_OPEN_BY_LOCATION, out handleFtdi);
                    }
                    else
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }
                else
                {
                    uint usbIndex = Convert.ToUInt32(port.Remove(0, PortID.Length));

                    ftStatus = Ftd2xx.FT_Open(usbIndex, out handleFtdi);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }

#if USE_BITBANG
                bitBangMode = false;
                bitBangOutput = bitBangBits.DTR | bitBangBits.RTS | bitBangBits.TXD;
#endif
                ftStatus = Ftd2xx.FT_SetBitMode(handleFtdi, 0x00, Ftd2xx.FT_BITMODE_RESET);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_SetUSBParameters(handleFtdi, usbBufferSizeStd, usbBufferSizeStd);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_SetLatencyTimer(handleFtdi, 2);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_SetBaudRate(handleFtdi, Ftd2xx.FT_BAUD_9600);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }
                currentBaudRate = 9600;

                ftStatus = Ftd2xx.FT_SetDataCharacteristics(handleFtdi, Ftd2xx.FT_BITS_8, Ftd2xx.FT_STOP_BITS_1, Ftd2xx.FT_PARITY_NONE);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }
                currentWordLength = 8;
                currentParity = Parity.None;

                ftStatus = Ftd2xx.FT_SetTimeouts(handleFtdi, 0, writeTimeout);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_SetFlowControl(handleFtdi, Ftd2xx.FT_FLOW_NONE, 0, 0);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_ClrDtr(handleFtdi);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_ClrRts(handleFtdi);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_TX | Ftd2xx.FT_PURGE_RX);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }
            }
            catch (Exception)
            {
                InterfaceDisconnect();
                return false;
            }
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            try
            {
                if (handleFtdi != (IntPtr)0)
                {
                    Ftd2xx.FT_SetBitMode(handleFtdi, 0x00, Ftd2xx.FT_BITMODE_RESET);
                    Ftd2xx.FT_Close(handleFtdi);
                    handleFtdi = (IntPtr)0;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetConfig(int baudRate, int dataBits, Parity parity, bool allowBitBang)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2xx.FT_STATUS ftStatus;

                currentBaudRate = baudRate;
                currentWordLength = dataBits;
                currentParity = parity;

#if USE_BITBANG
                bool bitBangOld = bitBangMode;
                if (allowBitBang && currentBaudRate <= 19200)
                {
                    bitBangMode = true;
                }
                else
                {
                    bitBangMode = false;
                }
                if (bitBangMode)
                {
                    bitBangBitsPerSendByte = 12000000 / bitBangDivisor / currentBaudRate;
                    bitBangBitsPerRecByte = 12000000 / 16 / currentBaudRate + 2;
                    if (bitBangMode != bitBangOld)
                    {
                        ftStatus = Ftd2xx.FT_SetBitMode(handleFtdi, (byte)(bitBangBits.DTR | bitBangBits.RTS | bitBangBits.TXD), Ftd2xx.FT_BITMODE_ASYNC_BITBANG);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_SetDivisor(handleFtdi, bitBangDivisor);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_SetTimeouts(handleFtdi, writeTimeout, writeTimeout);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_SetUSBParameters(handleFtdi, bitBangRecBufferSize, 0x10000);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_TX | Ftd2xx.FT_PURGE_RX);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        if (!SetBitBangOutput(bitBangOutput))
                        {
                            return false;
                        }
                    }
                    return true;
                }
#endif
                byte parityLocal;
                switch (parity)
                {
                    case Parity.None:
                        parityLocal = Ftd2xx.FT_PARITY_NONE;
                        break;

                    case Parity.Even:
                        parityLocal = Ftd2xx.FT_PARITY_EVEN;
                        break;

                    case Parity.Odd:
                        parityLocal = Ftd2xx.FT_PARITY_ODD;
                        break;

                    case Parity.Mark:
                        parityLocal = Ftd2xx.FT_PARITY_MARK;
                        break;

                    case Parity.Space:
                        parityLocal = Ftd2xx.FT_PARITY_SPACE;
                        break;

                    default:
                        return false;
                }

                byte wordLengthLocal;
                switch (dataBits)
                {
                    case 5:
                        wordLengthLocal = Ftd2xx.FT_BITS_5;
                        break;

                    case 6:
                        wordLengthLocal = Ftd2xx.FT_BITS_6;
                        break;

                    case 7:
                        wordLengthLocal = Ftd2xx.FT_BITS_7;
                        break;

                    case 8:
                        wordLengthLocal = Ftd2xx.FT_BITS_8;
                        break;

                    default:
                        return false;
                }

                ftStatus = Ftd2xx.FT_SetBitMode(handleFtdi, 0x00, Ftd2xx.FT_BITMODE_RESET);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                ftStatus = Ftd2xx.FT_SetUSBParameters(handleFtdi, usbBufferSizeStd, usbBufferSizeStd);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                ftStatus = Ftd2xx.FT_SetBaudRate(handleFtdi, (uint)baudRate);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                ftStatus = Ftd2xx.FT_SetDataCharacteristics(handleFtdi, wordLengthLocal, Ftd2xx.FT_STOP_BITS_1, parityLocal);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_TX | Ftd2xx.FT_PURGE_RX);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
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

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if USE_BITBANG
            if (dtr)
            {
                bitBangOutput &= ~bitBangBits.DTR;
            }
            else
            {
                bitBangOutput |= bitBangBits.DTR;
            }
            if (bitBangMode)
            {
                if (!SetBitBangOutput(bitBangOutput))
                {
                    return false;
                }
                return true;
            }
#endif
            try
            {
                Ftd2xx.FT_STATUS ftStatus;

                if (dtr)
                {
                    ftStatus = Ftd2xx.FT_SetDtr(handleFtdi);
                }
                else
                {
                    ftStatus = Ftd2xx.FT_ClrDtr(handleFtdi);
                }
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
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

        public static bool InterfaceSetRts(bool rts)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if USE_BITBANG
            if (rts)
            {
                bitBangOutput &= ~bitBangBits.RTS;
            }
            else
            {
                bitBangOutput |= bitBangBits.RTS;
            }
            if (bitBangMode)
            {
                if (!SetBitBangOutput(bitBangOutput))
                {
                    return false;
                }
                return true;
            }
#endif
            try
            {
                Ftd2xx.FT_STATUS ftStatus;

                if (rts)
                {
                    ftStatus = Ftd2xx.FT_SetRts(handleFtdi);
                }
                else
                {
                    ftStatus = Ftd2xx.FT_ClrRts(handleFtdi);
                }
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
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

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = false;
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2xx.FT_STATUS ftStatus;
#if USE_BITBANG
                if (bitBangMode)
                {
                    byte mode;
                    ftStatus = Ftd2xx.FT_GetBitMode(handleFtdi, out mode);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    if ((mode & (int)bitBangBits.DSR) == 0)
                    {
                        dsr = true;
                    }
                    else
                    {
                        dsr = false;
                    }
                    return true;
                }
#endif
                dsr = false;
                uint modemStatus = 0x0000;
                ftStatus = Ftd2xx.FT_GetModemStatus(handleFtdi, ref modemStatus);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                dsr = (modemStatus & 0x20) != 0;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if USE_BITBANG
            if (enable)
            {
                bitBangOutput &= ~bitBangBits.TXD;
            }
            else
            {
                bitBangOutput |= bitBangBits.TXD;
            }
            if (bitBangMode)
            {
                if (!SetBitBangOutput(bitBangOutput))
                {
                    return false;
                }
                return true;
            }
#endif
            try
            {
                Ftd2xx.FT_STATUS ftStatus;

                if (enable)
                {
                    ftStatus = Ftd2xx.FT_SetBreakOn(handleFtdi);
                }
                else
                {
                    ftStatus = Ftd2xx.FT_SetBreakOff(handleFtdi);
                }
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
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

        public static bool InterfacePurgeInBuffer()
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2xx.FT_STATUS ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_RX);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
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

        public static bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesWritten = 0;

#if USE_BITBANG
                if (bitBangMode)
                {
                    int bufferSize = (currentWordLength + 4) * bitBangBitsPerSendByte * (length + 2);
                    if (bufferSize > bitBangSendBuffer.Length)
                    {
                        return false;
                    }

                    int dataLen = 0;
                    for (int i = 0; i < length; i++)
                    {
                        if (i == 0)
                        {
                            for (int k = 0; k < bitBangBitsPerSendByte * 9; k++)
                            {
                                bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                            }
                            if (setDtr)
                            {
                                bitBangOutput &= ~bitBangBits.DTR;       // DTR on
                                for (int k = 0; k < bitBangBitsPerSendByte; k++)
                                {
                                    bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                                }
                            }
                        }
                        bitBangOutput &= ~bitBangBits.TXD;       // Start bit
                        for (int k = 0; k < bitBangBitsPerSendByte; k++)
                        {
                            bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                        }
                        bool parity = false;
                        for (int j = 0; j < currentWordLength; j++)
                        {
                            bool bitSet = (sendData[i] & (1 << j)) != 0;
                            if (bitSet) parity = !parity;
                            if (bitSet)
                            {
                                bitBangOutput |= bitBangBits.TXD;
                            }
                            else
                            {
                                bitBangOutput &= ~bitBangBits.TXD;
                            }
                            for (int k = 0; k < bitBangBitsPerSendByte; k++)
                            {
                                bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                            }
                        }
                        switch (currentParity)
                        {
                            case Parity.Even:
                                {
                                    if (parity)
                                    {
                                        bitBangOutput |= bitBangBits.TXD;
                                    }
                                    else
                                    {
                                        bitBangOutput &= ~bitBangBits.TXD;
                                    }
                                    for (int k = 0; k < bitBangBitsPerSendByte; k++)
                                    {
                                        bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                                    }
                                    break;
                                }

                            case Parity.Odd:
                                {
                                    if (parity)
                                    {
                                        bitBangOutput &= ~bitBangBits.TXD;
                                    }
                                    else
                                    {
                                        bitBangOutput |= bitBangBits.TXD;
                                    }
                                    for (int k = 0; k < bitBangBitsPerSendByte; k++)
                                    {
                                        bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                                    }
                                    break;
                                }
                        }
                        // 2 stop bits for time correction
                        bitBangOutput |= bitBangBits.TXD;   // Stop bit
                        for (int k = 0; k < bitBangBitsPerSendByte * 2; k++)
                        {
                            bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                        }
                        if ((i + 1) == length)
                        {
                            if (setDtr)
                            {
                                bitBangOutput |= bitBangBits.DTR;      // DTR off
                                bitBangSendBuffer[dataLen++] = (byte)bitBangOutput;
                            }
                        }
                    }
                    recBufLastIndex = -1;
                    ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_RX);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, bitBangSendBuffer, dataLen, 0, out bytesWritten);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    return true;
                }
#endif
                int bitCount = (currentParity == Parity.None) ? (currentWordLength + 2) : (currentWordLength + 3);
                double byteTime = 1.0d / currentBaudRate * 1000 * bitCount;
                if (setDtr)
                {
                    long waitTime = (long)((dtrTimeCorr + byteTime * length) * tickResolMs);
                    ftStatus = Ftd2xx.FT_SetDtr(handleFtdi);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    long startTime = Stopwatch.GetTimestamp();
#if WindowsCE
                            const int sendBlockSize = 4;
                            for (int i = 0; i < length; i += sendBlockSize)
                            {
                                int sendLength = length - i;
                                if (sendLength > sendBlockSize) sendLength = sendBlockSize;
                                ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, sendData, sendLength, i, out bytesWritten);
                                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                                {
                                    return false;
                                }
                            }
#else
                    ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, sendData, length, 0, out bytesWritten);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
#endif
                    while ((Stopwatch.GetTimestamp() - startTime) < waitTime)
                    {
                    }
                    ftStatus = Ftd2xx.FT_ClrDtr(handleFtdi);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                }
                else
                {
                    long waitTime = (long)(byteTime * length);
#if WindowsCE
                    const int sendBlockSize = 4;
                    for (int i = 0; i < length; i += sendBlockSize)
                    {
                        int sendLength = length - i;
                        if (sendLength > sendBlockSize) sendLength = sendBlockSize;
                        ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, sendData, sendLength, i, out bytesWritten);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                    }
#else
                    ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, sendData, length, 0, out bytesWritten);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
#endif
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

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if WindowsCE
            if (timeout < readTimeoutCeMin)
            {
                timeout = readTimeoutCeMin;
            }
            if (timeoutTelEnd < readTimeoutCeMin)
            {
                timeoutTelEnd = readTimeoutCeMin;
            }
#else
            timeout += 20;
            timeoutTelEnd += 20;
#endif
            try
            {
                int recLen;
#if USE_BITBANG
                if (bitBangMode)
                {
                    recLen = 0;
                    while (recLen < length)
                    {
                        int currTimeout = (recLen == 0) ? timeout : timeoutTelEnd;
                        long startTime = Stopwatch.GetTimestamp();
                        for (; ; )
                        {
                            byte value;
                            if (ReceiveByteFromBitBangStream(out value))
                            {
                                receiveData[offset + recLen] = value;
                                recLen++;
                                break;
                            }
                            if ((Stopwatch.GetTimestamp() - startTime) > currTimeout * tickResolMs)
                            {
                                if (ediabasLog != null)
                                {
                                    ediabasLog.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, offset, recLen, "Rec ");
                                }
                                return false;
                            }
                        }
                    }
                    if (ediabasLog != null)
                    {
                        ediabasLog.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, offset, recLen, "Rec ");
                    }
                    return true;
                }
#endif
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesRead = 0;

                ftStatus = Ftd2xx.FT_SetTimeouts(handleFtdi, (uint)timeout, writeTimeout);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                ftStatus = Ftd2xx.FT_ReadWrapper(handleFtdi, receiveData, 1, offset, out bytesRead);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                recLen = (int)bytesRead;
                if (recLen < 1)
                {
                    return false;
                }
                if (recLen < length)
                {
                    ftStatus = Ftd2xx.FT_SetTimeouts(handleFtdi, (uint)timeoutTelEnd, writeTimeout);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    while (recLen < length)
                    {
                        ftStatus = Ftd2xx.FT_ReadWrapper(handleFtdi, receiveData, length - recLen, offset + recLen, out bytesRead);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        if (bytesRead <= 0)
                        {
                            break;
                        }
                        recLen += (int)bytesRead;
                    }
                }
                if (ediabasLog != null)
                {
                    ediabasLog.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, offset, recLen, "Rec ");
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

#if USE_BITBANG
        private static bool SetBitBangOutput(bitBangBits output)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                bitBangSendBuffer[0] = (byte)output;
                uint bytesWritten = 0;
                Ftd2xx.FT_STATUS ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, bitBangSendBuffer, 1, 0, out bytesWritten);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
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

        private static bool ReceiveByteFromBitBangStream(out byte value)
        {
            value = 0;
            try
            {
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesRead = 0;

                if (recBufLastIndex < 0)
                {   // all buffers empty
                    //Debug.WriteLine("Start");
                    ftStatus = Ftd2xx.FT_ReadWrapper(handleFtdi, bitBangRecBuffer[0], bitBangRecBufferSize, 0, out bytesRead);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    ftStatus = Ftd2xx.FT_ReadWrapper(handleFtdi, bitBangRecBuffer[1], bitBangRecBufferSize, 0, out bytesRead);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    recBufLastIndex = 1;
                    recBufReadPos = 0;
                    recBufReadIndex = 0;
                }
                else
                {
                    if (recBufLastIndex == recBufReadIndex)
                    {   // get next buffer
                        //Debug.WriteLine("New buf");
                        recBufLastIndex = (recBufLastIndex == 0) ? 1 : 0;
                        ftStatus = Ftd2xx.FT_ReadWrapper(handleFtdi, bitBangRecBuffer[recBufLastIndex], bitBangRecBufferSize, 0, out bytesRead);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            recBufLastIndex = -1;
                            return false;
                        }
                    }
                }
                if (!GetByteFromDataStream(out value))
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

        private static bool GetByteFromDataStream(out byte value)
        {
            value = 0;
            // find start bit
            for (; ; )
            {
                byte recVal = bitBangRecBuffer[recBufReadIndex][recBufReadPos];
                if ((recVal & 0x02) == 0)
                {
                    break;
                }
                recBufReadPos++;
                if (recBufReadPos >= bitBangRecBufferSize)
                {
                    recBufReadPos = 0;
                    recBufReadIndex = (recBufReadIndex == 0) ? 1 : 0;
                    //Debug.WriteLine(string.Format("No S {0}", recBufReadIndex));
                    return false;
                }
            }
            // middle of next data bit
            recBufReadPos += bitBangBitsPerRecByte + bitBangBitsPerRecByte / 2;
            if (recBufReadPos >= bitBangRecBufferSize)
            {
                recBufReadPos -= bitBangRecBufferSize;
                recBufReadIndex = (recBufReadIndex == 0) ? 1 : 0;
                //Debug.WriteLine(string.Format("B {0}", recBufReadIndex));
            }
            // read the data bits
            bool dataValid = true;
            uint recData = 0;
            // don't read stop bit, so we are able to sync next time
            int dataBits = (currentParity == Parity.None) ? (currentWordLength + 0) : (currentWordLength + 1);
            for (int i = 0; i < dataBits; i++)
            {
                byte recVal = bitBangRecBuffer[recBufReadIndex][recBufReadPos];
                if ((recVal & 0x02) != 0)
                {
                    recData |= (uint)(1 << i);
                }
                recBufReadPos += bitBangBitsPerRecByte;
                if (recBufReadPos >= bitBangRecBufferSize)
                {
                    recBufReadPos -= bitBangRecBufferSize;
                    recBufReadIndex = (recBufReadIndex == 0) ? 1 : 0;
                    //Debug.WriteLine(string.Format("B {0}", recBufReadIndex));
                }
            }
            //Debug.WriteLine(string.Format("{0:X03}", recData));

            bool parity = false;
            for (int i = 0; i < currentWordLength; i++)
            {
                if ((recData & (1 << i)) != 0)
                {
                    parity = !parity;
                }
            }
            switch (currentParity)
            {
                case Parity.Even:
                    {
                        bool recParity = (recData & (1 << (dataBits - 1))) != 0;
                        if (recParity != parity)
                        {
                            dataValid = false;
                        }
                        break;
                    }

                case Parity.Odd:
                    {
                        bool recParity = (recData & (1 << (dataBits - 1))) == 0;
                        if (recParity != parity)
                        {
                            dataValid = false;
                        }
                        break;
                    }
            }
            if (!dataValid)
            {
                //Debug.WriteLine(string.Format("E", recBufReadIndex));
                return false;
            }
            value = (byte)recData;
            return true;
        }
#endif

    }
}
