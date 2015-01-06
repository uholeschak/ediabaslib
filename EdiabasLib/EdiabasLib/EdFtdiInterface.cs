#define USE_BITBANG

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
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private static readonly long tickResolMs = Stopwatch.Frequency / 1000;
        private static IntPtr handleFtdi = (IntPtr)0;
        private static int currentBaudRate = 0;
        private static int currentWordLength = 0;
        private static Parity currentParity = Parity.None;
#if USE_BITBANG
        private const int bitBangBaudFactor = 8;    // baud factor in bit bang mode
        private static byte[] tempBuffer = new byte[1000];
#endif

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

                ftStatus = Ftd2xx.FT_SetBitMode(handleFtdi, 0x00, Ftd2xx.FT_BITMODE_RESET);
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
            if (handleFtdi != (IntPtr)0)
            {
                Ftd2xx.FT_Close(handleFtdi);
                handleFtdi = (IntPtr)0;
            }
            return true;
        }

        public static bool InterfaceSetConfig(int baudRate, int dataBits, Parity parity)
        {
            if (handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2xx.FT_STATUS ftStatus;

                ftStatus = Ftd2xx.FT_SetBaudRate(handleFtdi, (uint)baudRate);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }

                byte wordLength;

                switch (dataBits)
                {
                    case 5:
                        wordLength = Ftd2xx.FT_BITS_5;
                        break;

                    case 6:
                        wordLength = Ftd2xx.FT_BITS_6;
                        break;

                    case 7:
                        wordLength = Ftd2xx.FT_BITS_7;
                        break;

                    case 8:
                        wordLength = Ftd2xx.FT_BITS_8;
                        break;

                    default:
                        return false;
                }

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

                ftStatus = Ftd2xx.FT_SetDataCharacteristics(handleFtdi, wordLength, Ftd2xx.FT_STOP_BITS_1, parityLocal);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                currentBaudRate = baudRate;
                currentWordLength = wordLength;
                currentParity = parity;
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
                dsr = false;
                uint modemStatus = 0x0000;
                Ftd2xx.FT_STATUS ftStatus = Ftd2xx.FT_GetModemStatus(handleFtdi, ref modemStatus);
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

                int bitCount = (currentParity == Parity.None) ? (currentWordLength + 2) : (currentWordLength + 3);
                double byteTime = 1.0d / currentBaudRate * 1000 * bitCount;
                if (setDtr)
                {
#if !USE_BITBANG
                    try
                    {
#if !WindowsCE
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
#endif
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
                    finally
                    {
#if !WindowsCE
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
#endif
                    }
#else
                    for (int i = 0; i < length; i++)
                    {
                        sendData[i] = 0x55;
                    }
                    int bitBangBitCount = 16 / bitBangBaudFactor;
                    int bufferSize = (currentWordLength + 3) * bitBangBitCount + 2;
                    if (bufferSize > tempBuffer.Length)
                    {
                        return false;
                    }
                    try
                    {
                        uint bytesRead = 0;
                        // Bit 0=TXD, 2=RTS, 4=DTR
                        ftStatus = Ftd2xx.FT_SetBaudRate(handleFtdi, (uint)(currentBaudRate / bitBangBaudFactor));
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_SetTimeouts(handleFtdi, writeTimeout, writeTimeout);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_SetBitMode(handleFtdi, 0x15, Ftd2xx.FT_BITMODE_ASYNC_BITBANG);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
#if false
                        ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_RX);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
#endif
                        Thread.Sleep(50);
                        int dataLen = 0;
                        for (int i = 0; i < length; i++)
                        {
                            if (i == 0)
                            {
                                tempBuffer[dataLen++] = 0x01;   // DTR on
                            }
                            for (int k = 0; k < bitBangBitCount; k++)
                            {
                                tempBuffer[dataLen++] = 0x00;   // Start bit
                            }
                            bool parity = false;
                            for (int j = 0; j < currentWordLength; j++)
                            {
                                bool bitSet = (sendData[i] & (1 << j)) != 0;
                                if (bitSet) parity = !parity;
                                byte value = (byte)(bitSet ? 0x01 : 0x00);
                                for (int k = 0; k < bitBangBitCount; k++)
                                {
                                    tempBuffer[dataLen++] = value;
                                }
                            }
                            switch (currentParity)
                            {
                                case Parity.Even:
                                    {
                                        byte value = (byte)(parity ? 0x01 : 0x00);
                                        for (int k = 0; k < bitBangBitCount; k++)
                                        {
                                            tempBuffer[dataLen++] = value;
                                        }
                                        break;
                                    }

                                case Parity.Odd:
                                    {
                                        byte value = (byte)(parity ? 0x00 : 0x01);
                                        for (int k = 0; k < bitBangBitCount; k++)
                                        {
                                            tempBuffer[dataLen++] = value;
                                        }
                                        break;
                                    }
                            }
                            for (int k = 0; k < bitBangBitCount; k++)
                            {
                                tempBuffer[dataLen++] = 0x01;   // Stop bit
                            }
                            if ((i + 1) == length)
                            {
                                tempBuffer[dataLen++] = 0x11;   // DTR off
                            }
                        }
                        ftStatus = Ftd2xx.FT_WriteWrapper(handleFtdi, tempBuffer, dataLen, 0, out bytesWritten);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        ftStatus = Ftd2xx.FT_ReadWrapper(handleFtdi, tempBuffer, 1, 0, out bytesRead);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        ftStatus = Ftd2xx.FT_SetBitMode(handleFtdi, 0x00, Ftd2xx.FT_BITMODE_RESET);
                        ftStatus = Ftd2xx.FT_SetBaudRate(handleFtdi, (uint)currentBaudRate);
                        ftStatus = Ftd2xx.FT_Purge(handleFtdi, Ftd2xx.FT_PURGE_RX);
                    }
#endif
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
            // add extra delay for internal signal transitions
            timeout += 20;
            timeoutTelEnd += 20;
#endif
            try
            {
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesRead = 0;
                int recLen = 0;

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

    }
}
