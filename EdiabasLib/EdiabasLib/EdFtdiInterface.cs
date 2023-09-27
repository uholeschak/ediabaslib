#define USE_BITBANG

using System;
using System.Diagnostics;
using System.Threading;
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public static class EdFtdiInterface
    {
        public const string PortId = "FTDI";
        private const int WriteTimeout = 500;       // write timeout [ms]
        private const int UsbBufferSizeStd = 0x1000;    // standard usb buffer size
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static IntPtr _handleFtdi = (IntPtr)0;
        private static int _currentBaudRate;
        private static int _currentWordLength;
        private static EdInterfaceObd.SerialParity _currentParity = EdInterfaceObd.SerialParity.None;
#if USE_BITBANG
        [Flags]
        private enum BitBangBits
        {
            // ReSharper disable UnusedMember.Local
            Txd = 0x01,     // not inverted
            Rxd = 0x02,     // not inverted
            Rts = 0x04,     // inverted
            Cts = 0x08,     // inverted
            Dtr = 0x10,     // inverted
            Dsr = 0x20,     // inverted
            Cdc = 0x40,     // inverted
            Ri = 0x80,      // inverted
            // ReSharper restore UnusedMember.Local
        }
        private const int BitBangRecBufferSize = 0x800;
        private static bool _bitBangMode;
        private static BitBangBits _bitBangOutput = BitBangBits.Dtr | BitBangBits.Rts | BitBangBits.Txd;
        private static Ftd2Xx.FT_DEVICE _bitBangDeviceType;
        private static UInt32 _bitBangDeviceId;
        private static int _bitBangBitsPerSendByte;
        private static int _bitBangBitsPerRecByte;
        private static readonly byte[] BitBangSendBuffer;
        private static readonly byte[][] BitBangRecBuffer;
        private static int _recBufReadPos;
        private static int _recBufReadIndex;
        private static int _recBufLastIndex = -1;
#endif

        static EdFtdiInterface()
        {
#if USE_BITBANG
            BitBangSendBuffer = new byte[0x10000];

            BitBangRecBuffer = new byte[2][];
            BitBangRecBuffer[0] = new byte[BitBangRecBufferSize];
            BitBangRecBuffer[1] = new byte[BitBangRecBufferSize];
#endif
        }

        public static EdiabasNet Ediabas { get; set; }

        public static IntPtr HandleFtdi
        {
            get { return _handleFtdi; }
        }

        public static int CurrentBaudRate
        {
            get { return _currentBaudRate; }
        }

        public static int CurrentWordLength
        {
            get { return _currentWordLength; }
        }

        public static EdInterfaceObd.SerialParity CurrentParity
        {
            get { return _currentParity; }
        }

        public static bool InterfaceConnect(string port, object parameter)
        {
            if (_handleFtdi != (IntPtr)0)
            {
                return true;
            }
            try
            {
                Ftd2Xx.FT_STATUS ftStatus;
                if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
                {
                    InterfaceDisconnect();
                    return false;
                }
                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    if (portData.StartsWith(":SER=", StringComparison.OrdinalIgnoreCase))
                    {   // serial number
                        string id = portData.Remove(0, 5);
                        ftStatus = Ftd2Xx.FT_OpenEx(id, Ftd2Xx.FT_OPEN_BY_SERIAL_NUMBER, out _handleFtdi);
                    }
                    else if (portData.StartsWith(":DESC=", StringComparison.OrdinalIgnoreCase))
                    {   // description
                        string id = portData.Remove(0, 6);
                        ftStatus = Ftd2Xx.FT_OpenEx(id, Ftd2Xx.FT_OPEN_BY_DESCRIPTION, out _handleFtdi);
                    }
                    else if (portData.StartsWith(":LOC=", StringComparison.OrdinalIgnoreCase))
                    {   // location
                        long loc = EdiabasNet.StringToValue(portData.Remove(0, 5));
                        ftStatus = Ftd2Xx.FT_OpenEx((IntPtr)loc, Ftd2Xx.FT_OPEN_BY_LOCATION, out _handleFtdi);
                    }
                    else
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }
                else
                {
                    uint usbIndex = Convert.ToUInt32(port.Remove(0, PortId.Length));

                    ftStatus = Ftd2Xx.FT_Open(usbIndex, out _handleFtdi);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }

#if USE_BITBANG
                _bitBangMode = false;
                _bitBangOutput = BitBangBits.Dtr | BitBangBits.Rts | BitBangBits.Txd;
#endif
                ftStatus = Ftd2Xx.FT_SetBitMode(_handleFtdi, 0x00, Ftd2Xx.FT_BITMODE_RESET);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_SetUSBParameters(_handleFtdi, UsbBufferSizeStd, UsbBufferSizeStd);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_SetLatencyTimer(_handleFtdi, 2);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_SetBaudRate(_handleFtdi, Ftd2Xx.FT_BAUD_9600);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }
                _currentBaudRate = 9600;

                ftStatus = Ftd2Xx.FT_SetDataCharacteristics(_handleFtdi, Ftd2Xx.FT_BITS_8, Ftd2Xx.FT_STOP_BITS_1, Ftd2Xx.FT_PARITY_NONE);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }
                _currentWordLength = 8;
                _currentParity = EdInterfaceObd.SerialParity.None;

                ftStatus = Ftd2Xx.FT_SetTimeouts(_handleFtdi, 0, WriteTimeout);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_SetFlowControl(_handleFtdi, Ftd2Xx.FT_FLOW_NONE, 0, 0);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_ClrDtr(_handleFtdi);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_ClrRts(_handleFtdi);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    InterfaceDisconnect();
                    return false;
                }

                ftStatus = Ftd2Xx.FT_Purge(_handleFtdi, Ftd2Xx.FT_PURGE_TX | Ftd2Xx.FT_PURGE_RX);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
                if (_handleFtdi != (IntPtr)0)
                {
                    Ftd2Xx.FT_SetBitMode(_handleFtdi, 0x00, Ftd2Xx.FT_BITMODE_RESET);
                    Ftd2Xx.FT_Close(_handleFtdi);
                    _handleFtdi = (IntPtr)0;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceTransmitCancel(bool cancel)
        {
            return true;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (_handleFtdi == (IntPtr)0)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            if (protocol != EdInterfaceObd.Protocol.Uart)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            try
            {
                Ftd2Xx.FT_STATUS ftStatus;

                _currentBaudRate = baudRate;
                _currentWordLength = dataBits;
                _currentParity = parity;

#if USE_BITBANG
                bool bitBangOld = _bitBangMode;
                if (allowBitBang && _currentBaudRate <= 19200)
                {
                    _bitBangMode = true;
                }
                else
                {
                    _bitBangMode = false;
                }
                if (_bitBangMode)
                {
                    byte[] sernum = new byte[16];
                    byte[] desc = new byte[64];

                    ftStatus = Ftd2Xx.FT_GetDeviceInfo(_handleFtdi, out _bitBangDeviceType, out _bitBangDeviceId, sernum, desc, IntPtr.Zero);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                    }

                    int divisor;
                    switch (_bitBangDeviceType)
                    {
                        case Ftd2Xx.FT_DEVICE.FT_DEVICE_232R:
                            // tested range: 1-59
                            // good values: 27, 29!, 33, 35
                            divisor = 29;  // only odd values allowed!
                            _bitBangBitsPerSendByte = 12000000 / divisor / _currentBaudRate;
                            _bitBangBitsPerRecByte = 12000000 / 16 / _currentBaudRate + 2;
                            return EdInterfaceObd.InterfaceErrorResult.DeviceTypeError;

                        case Ftd2Xx.FT_DEVICE.FT_DEVICE_232H:
                            divisor = 120000000 / 2 / 50 / 9600;
                            _bitBangBitsPerSendByte = 120000000 / 2 / divisor / _currentBaudRate;
                            _bitBangBitsPerRecByte = _bitBangBitsPerSendByte;
                            break;

                        default:
                            return EdInterfaceObd.InterfaceErrorResult.DeviceTypeError;
                    }

                    if (_bitBangMode != bitBangOld)
                    {
                        // set al to input to prevent start glitch
                        ftStatus = Ftd2Xx.FT_SetBitMode(_handleFtdi, 0x00, Ftd2Xx.FT_BITMODE_ASYNC_BITBANG);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                        ftStatus = Ftd2Xx.FT_SetDivisor(_handleFtdi, (UInt16)divisor);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                        ftStatus = Ftd2Xx.FT_SetTimeouts(_handleFtdi, WriteTimeout, WriteTimeout);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                        ftStatus = Ftd2Xx.FT_SetUSBParameters(_handleFtdi, BitBangRecBufferSize, 0x10000);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                        if (!SetBitBangOutput(_bitBangOutput))
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                        ftStatus = Ftd2Xx.FT_SetBitMode(_handleFtdi, (byte)(BitBangBits.Dtr | BitBangBits.Rts | BitBangBits.Txd), Ftd2Xx.FT_BITMODE_ASYNC_BITBANG);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                        ftStatus = Ftd2Xx.FT_Purge(_handleFtdi, Ftd2Xx.FT_PURGE_TX | Ftd2Xx.FT_PURGE_RX);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                        }
                    }
                    return EdInterfaceObd.InterfaceErrorResult.NoError;
                }
#endif
                byte parityLocal;
                switch (parity)
                {
                    case EdInterfaceObd.SerialParity.None:
                        parityLocal = Ftd2Xx.FT_PARITY_NONE;
                        break;

                    case EdInterfaceObd.SerialParity.Even:
                        parityLocal = Ftd2Xx.FT_PARITY_EVEN;
                        break;

                    case EdInterfaceObd.SerialParity.Odd:
                        parityLocal = Ftd2Xx.FT_PARITY_ODD;
                        break;

                    case EdInterfaceObd.SerialParity.Mark:
                        parityLocal = Ftd2Xx.FT_PARITY_MARK;
                        break;

                    case EdInterfaceObd.SerialParity.Space:
                        parityLocal = Ftd2Xx.FT_PARITY_SPACE;
                        break;

                    default:
                        return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }

                byte wordLengthLocal;
                switch (dataBits)
                {
                    case 5:
                        wordLengthLocal = Ftd2Xx.FT_BITS_5;
                        break;

                    case 6:
                        wordLengthLocal = Ftd2Xx.FT_BITS_6;
                        break;

                    case 7:
                        wordLengthLocal = Ftd2Xx.FT_BITS_7;
                        break;

                    case 8:
                        wordLengthLocal = Ftd2Xx.FT_BITS_8;
                        break;

                    default:
                        return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }

                ftStatus = Ftd2Xx.FT_SetBitMode(_handleFtdi, 0x00, Ftd2Xx.FT_BITMODE_RESET);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                ftStatus = Ftd2Xx.FT_SetUSBParameters(_handleFtdi, UsbBufferSizeStd, UsbBufferSizeStd);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                ftStatus = Ftd2Xx.FT_SetBaudRate(_handleFtdi, (uint)baudRate);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                ftStatus = Ftd2Xx.FT_SetDataCharacteristics(_handleFtdi, wordLengthLocal, Ftd2Xx.FT_STOP_BITS_1, parityLocal);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                ftStatus = Ftd2Xx.FT_Purge(_handleFtdi, Ftd2Xx.FT_PURGE_TX | Ftd2Xx.FT_PURGE_RX);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
            }
            catch (Exception)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            return EdInterfaceObd.InterfaceErrorResult.NoError;
        }

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if USE_BITBANG
            if (dtr)
            {
                _bitBangOutput &= ~BitBangBits.Dtr;
            }
            else
            {
                _bitBangOutput |= BitBangBits.Dtr;
            }
            if (_bitBangMode)
            {
                if (!SetBitBangOutput(_bitBangOutput))
                {
                    return false;
                }
                return true;
            }
#endif
            try
            {
                Ftd2Xx.FT_STATUS ftStatus = dtr ? Ftd2Xx.FT_SetDtr(_handleFtdi) : Ftd2Xx.FT_ClrDtr(_handleFtdi);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if USE_BITBANG
            if (rts)
            {
                _bitBangOutput &= ~BitBangBits.Rts;
            }
            else
            {
                _bitBangOutput |= BitBangBits.Rts;
            }
            if (_bitBangMode)
            {
                if (!SetBitBangOutput(_bitBangOutput))
                {
                    return false;
                }
                return true;
            }
#endif
            try
            {
                Ftd2Xx.FT_STATUS ftStatus = rts ? Ftd2Xx.FT_SetRts(_handleFtdi) : Ftd2Xx.FT_ClrRts(_handleFtdi);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2Xx.FT_STATUS ftStatus;
#if USE_BITBANG
                if (_bitBangMode)
                {
                    byte mode;
                    ftStatus = Ftd2Xx.FT_GetBitMode(_handleFtdi, out mode);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    dsr = (mode & (int)BitBangBits.Dsr) == 0;
                    return true;
                }
#endif
                uint modemStatus = 0x0000;
                ftStatus = Ftd2Xx.FT_GetModemStatus(_handleFtdi, ref modemStatus);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
#if USE_BITBANG
            if (enable)
            {
                _bitBangOutput &= ~BitBangBits.Txd;
            }
            else
            {
                _bitBangOutput |= BitBangBits.Txd;
            }
            if (_bitBangMode)
            {
                if (!SetBitBangOutput(_bitBangOutput))
                {
                    return false;
                }
                return true;
            }
#endif
            try
            {
                Ftd2Xx.FT_STATUS ftStatus = enable ? Ftd2Xx.FT_SetBreakOn(_handleFtdi) : Ftd2Xx.FT_SetBreakOff(_handleFtdi);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2Xx.FT_STATUS ftStatus;
#if USE_BITBANG
                if (_bitBangMode)
                {
                    UInt32 rxBytes;
                    ftStatus = Ftd2Xx.FT_GetQueueStatus(_handleFtdi, out rxBytes);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    if (rxBytes >= 0x10000)
                    {   // restart communication after buffer overrun
                        ftStatus = Ftd2Xx.FT_Purge(_handleFtdi, Ftd2Xx.FT_PURGE_RX);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                        Thread.Sleep(10);
                    }
                }
#endif
                ftStatus = Ftd2Xx.FT_Purge(_handleFtdi, Ftd2Xx.FT_PURGE_RX);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                Ftd2Xx.FT_STATUS ftStatus;
                uint bytesWritten;

#if USE_BITBANG
                if (_bitBangMode)
                {
                    int bufferSize = (_currentWordLength + 4) * _bitBangBitsPerSendByte * (length + 2);
                    if (bufferSize > BitBangSendBuffer.Length)
                    {
                        return false;
                    }

                    int dataLen = 0;
                    for (int i = 0; i < length; i++)
                    {
                        if (i == 0)
                        {
                            if (setDtr)
                            {
                                _bitBangOutput &= ~BitBangBits.Dtr;       // DTR on
                                for (int k = 0; k < _bitBangBitsPerSendByte; k++)
                                {
                                    BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                                }
                            }
                        }
                        _bitBangOutput &= ~BitBangBits.Txd;       // Start bit
                        for (int k = 0; k < _bitBangBitsPerSendByte; k++)
                        {
                            BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                        }
                        bool parity = false;
                        for (int j = 0; j < _currentWordLength; j++)
                        {
                            bool bitSet = (sendData[i] & (1 << j)) != 0;
                            if (bitSet) parity = !parity;
                            if (bitSet)
                            {
                                _bitBangOutput |= BitBangBits.Txd;
                            }
                            else
                            {
                                _bitBangOutput &= ~BitBangBits.Txd;
                            }
                            for (int k = 0; k < _bitBangBitsPerSendByte; k++)
                            {
                                BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                            }
                        }
                        switch (_currentParity)
                        {
                            case EdInterfaceObd.SerialParity.Even:
                                {
                                    if (parity)
                                    {
                                        _bitBangOutput |= BitBangBits.Txd;
                                    }
                                    else
                                    {
                                        _bitBangOutput &= ~BitBangBits.Txd;
                                    }
                                    for (int k = 0; k < _bitBangBitsPerSendByte; k++)
                                    {
                                        BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                                    }
                                    break;
                                }

                            case EdInterfaceObd.SerialParity.Odd:
                                {
                                    if (parity)
                                    {
                                        _bitBangOutput &= ~BitBangBits.Txd;
                                    }
                                    else
                                    {
                                        _bitBangOutput |= BitBangBits.Txd;
                                    }
                                    for (int k = 0; k < _bitBangBitsPerSendByte; k++)
                                    {
                                        BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                                    }
                                    break;
                                }
                        }
                        // 2 stop bits for time correction
                        _bitBangOutput |= BitBangBits.Txd;   // Stop bit
                        for (int k = 0; k < _bitBangBitsPerSendByte * 2; k++)
                        {
                            BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                        }
                        if ((i + 1) == length)
                        {
                            if (setDtr)
                            {
                                _bitBangOutput |= BitBangBits.Dtr;      // DTR off
                                BitBangSendBuffer[dataLen++] = (byte)_bitBangOutput;
                            }
                        }
                    }
                    _recBufLastIndex = -1;
                    ftStatus = Ftd2Xx.FT_Purge(_handleFtdi, Ftd2Xx.FT_PURGE_RX);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    ftStatus = Ftd2Xx.FT_WriteWrapper(_handleFtdi, BitBangSendBuffer, dataLen, 0, out bytesWritten);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    return true;
                }
#endif
                int bitCount = (_currentParity == EdInterfaceObd.SerialParity.None) ? (_currentWordLength + 2) : (_currentWordLength + 3);
                double byteTime = 1.0d / _currentBaudRate * 1000 * bitCount;
                if (setDtr)
                {
                    long waitTime = (long)((dtrTimeCorr + byteTime * length) * TickResolMs);
                    ftStatus = Ftd2Xx.FT_SetDtr(_handleFtdi);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    long startTime = Stopwatch.GetTimestamp();

                    ftStatus = Ftd2Xx.FT_WriteWrapper(_handleFtdi, sendData, length, 0, out bytesWritten);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }

                    while ((Stopwatch.GetTimestamp() - startTime) < waitTime)
                    {
                    }
                    ftStatus = Ftd2Xx.FT_ClrDtr(_handleFtdi);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                }
                else
                {
                    long waitTime = (long)(byteTime * length);
                    const int sendBlockSize = 4;
                    for (int i = 0; i < length; i += sendBlockSize)
                    {
                        int sendLength = length - i;
                        if (sendLength > sendBlockSize) sendLength = sendBlockSize;
                        ftStatus = Ftd2Xx.FT_WriteWrapper(_handleFtdi, sendData, sendLength, i, out bytesWritten);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            return false;
                        }
                    }

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
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }

            try
            {
                int recLen;
#if USE_BITBANG
                if (_bitBangMode)
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
                            if (recLen >= length)
                            {
                                break;
                            }
                            if ((Stopwatch.GetTimestamp() - startTime) > currTimeout * TickResolMs)
                            {
                                if (ediabasLog != null)
                                {
                                    ediabasLog.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
                                }
                                return false;
                            }
                        }
                    }
                    if (ediabasLog != null)
                    {
                        ediabasLog.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
                    }
                    return true;
                }
#endif
                uint bytesRead;

                Ftd2Xx.FT_STATUS ftStatus = Ftd2Xx.FT_SetTimeouts(_handleFtdi, (uint)timeout, WriteTimeout);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return false;
                }
                ftStatus = Ftd2Xx.FT_ReadWrapper(_handleFtdi, receiveData, 1, offset, out bytesRead);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
                    ftStatus = Ftd2Xx.FT_SetTimeouts(_handleFtdi, (uint)timeoutTelEnd, WriteTimeout);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    while (recLen < length)
                    {
                        ftStatus = Ftd2Xx.FT_ReadWrapper(_handleFtdi, receiveData, length - recLen, offset + recLen, out bytesRead);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
                    ediabasLog.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
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
        private static bool SetBitBangOutput(BitBangBits output)
        {
            if (_handleFtdi == (IntPtr)0)
            {
                return false;
            }
            try
            {
                BitBangSendBuffer[0] = (byte)output;
                uint bytesWritten;
                Ftd2Xx.FT_STATUS ftStatus = Ftd2Xx.FT_WriteWrapper(_handleFtdi, BitBangSendBuffer, 1, 0, out bytesWritten);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
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
                Ftd2Xx.FT_STATUS ftStatus;
                uint bytesRead;

                if (_recBufLastIndex < 0)
                {   // all buffers empty
                    //Debug.WriteLine("Start");
                    ftStatus = Ftd2Xx.FT_ReadWrapper(_handleFtdi, BitBangRecBuffer[0], BitBangRecBufferSize, 0, out bytesRead);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    ftStatus = Ftd2Xx.FT_ReadWrapper(_handleFtdi, BitBangRecBuffer[1], BitBangRecBufferSize, 0, out bytesRead);
                    if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    _recBufLastIndex = 1;
                    _recBufReadPos = 0;
                    _recBufReadIndex = 0;
                }
                else
                {
                    if (_recBufLastIndex == _recBufReadIndex)
                    {   // get next buffer
                        //Debug.WriteLine("New buf");
                        _recBufLastIndex = (_recBufLastIndex == 0) ? 1 : 0;
                        ftStatus = Ftd2Xx.FT_ReadWrapper(_handleFtdi, BitBangRecBuffer[_recBufLastIndex], BitBangRecBufferSize, 0, out bytesRead);
                        if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            _recBufLastIndex = -1;
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
                byte recVal = BitBangRecBuffer[_recBufReadIndex][_recBufReadPos];
                if ((recVal & 0x02) == 0)
                {
                    break;
                }
                _recBufReadPos++;
                if (_recBufReadPos >= BitBangRecBufferSize)
                {
                    _recBufReadPos = 0;
                    _recBufReadIndex = (_recBufReadIndex == 0) ? 1 : 0;
                    //Debug.WriteLine(string.Format("No S {0}", recBufReadIndex));
                    return false;
                }
            }
            // middle of next data bit
            _recBufReadPos += _bitBangBitsPerRecByte + _bitBangBitsPerRecByte / 2;
            if (_recBufReadPos >= BitBangRecBufferSize)
            {
                _recBufReadPos -= BitBangRecBufferSize;
                _recBufReadIndex = (_recBufReadIndex == 0) ? 1 : 0;
                //Debug.WriteLine(string.Format("B {0}", recBufReadIndex));
            }
            // read the data bits
            bool dataValid = true;
            uint recData = 0;
            // don't read stop bit, so we are able to sync next time
            int dataBits = (_currentParity == EdInterfaceObd.SerialParity.None) ? (_currentWordLength + 0) : (_currentWordLength + 1);
            for (int i = 0; i < dataBits; i++)
            {
                byte recVal = BitBangRecBuffer[_recBufReadIndex][_recBufReadPos];
                if ((recVal & 0x02) != 0)
                {
                    recData |= (uint)(1 << i);
                }
                _recBufReadPos += _bitBangBitsPerRecByte;
                if (_recBufReadPos >= BitBangRecBufferSize)
                {
                    _recBufReadPos -= BitBangRecBufferSize;
                    _recBufReadIndex = (_recBufReadIndex == 0) ? 1 : 0;
                    //Debug.WriteLine(string.Format("B {0}", recBufReadIndex));
                }
            }
            //Debug.WriteLine(string.Format("{0:X03}", recData));

            bool parity = false;
            for (int i = 0; i < _currentWordLength; i++)
            {
                if ((recData & (1 << i)) != 0)
                {
                    parity = !parity;
                }
            }
            switch (_currentParity)
            {
                case EdInterfaceObd.SerialParity.Even:
                    {
                        bool recParity = (recData & (1 << (dataBits - 1))) != 0;
                        if (recParity != parity)
                        {
                            dataValid = false;
                        }
                        break;
                    }

                case EdInterfaceObd.SerialParity.Odd:
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
