using System;
using System.Diagnostics;
using System.Threading;
using Com.Ftdi.J2xx;

namespace EdiabasLib
{
    static public class EdFtdiInterface
    {
        public class ConnectParameter
        {
            public ConnectParameter(Android.Content.Context parentContext, D2xxManager d2XxManager)
            {
                ParentContext = parentContext;
                Manager = d2XxManager;
            }

            public Android.Content.Context ParentContext { get; private set; }
            public D2xxManager Manager { get; private set; }
        }

        public const string PortId = "FTDI";
#if WindowsCE
        private const int ReadTimeoutCeMin = 500;   // min read timeout for CE [ms]
#endif
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static FT_Device _ftDevice;
        private static int _currentBaudRate;
        private static int _currentWordLength;
        private static EdInterfaceObd.SerialParity _currentParity = EdInterfaceObd.SerialParity.None;

        static EdFtdiInterface()
        {
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
            if (_ftDevice != null)
            {
                return true;
            }
            try
            {
                ConnectParameter connectParameter = parameter as ConnectParameter;
                if (connectParameter == null)
                {
                    return false;
                }

                if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
                {
                    InterfaceDisconnect();
                    return false;
                }

                D2xxManager.DriverParameters driverParameters = new D2xxManager.DriverParameters();
                driverParameters.SetBufferNumber(16);
                driverParameters.SetMaxBufferSize(0x4000);
                driverParameters.SetMaxTransferSize(0x1000);
                driverParameters.SetReadTimeout(100);

                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    if (portData.StartsWith(":SER=", StringComparison.OrdinalIgnoreCase))
                    {   // serial number
                        string id = portData.Remove(0, 5);
                        _ftDevice = connectParameter.Manager.OpenBySerialNumber(connectParameter.ParentContext, id, driverParameters);
                    }
                    else if (portData.StartsWith(":DESC=", StringComparison.OrdinalIgnoreCase))
                    {   // description
                        string id = portData.Remove(0, 6);
                        _ftDevice = connectParameter.Manager.OpenByDescription(connectParameter.ParentContext, id, driverParameters);
                    }
                    else if (portData.StartsWith(":LOC=", StringComparison.OrdinalIgnoreCase))
                    {   // location
                        long loc = EdiabasNet.StringToValue(portData.Remove(0, 5));
                        _ftDevice = connectParameter.Manager.OpenByLocation(connectParameter.ParentContext, (int)loc, driverParameters);
                    }
                    else
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                    if (_ftDevice == null)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }
                else
                {
                    int usbIndex = Convert.ToInt32(port.Remove(0, PortId.Length));

                    _ftDevice = connectParameter.Manager.OpenByIndex(connectParameter.ParentContext, usbIndex, driverParameters);
                    if (_ftDevice == null)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }

                if (!_ftDevice.SetBitMode(0x00, D2xxManager.FtBitmodeReset))
                {
                    InterfaceDisconnect();
                    return false;
                }

                if (!_ftDevice.SetLatencyTimer(2))
                {
                    InterfaceDisconnect();
                    return false;
                }

                if (!_ftDevice.SetBaudRate(9600))
                {
                    InterfaceDisconnect();
                    return false;
                }
                _currentBaudRate = 9600;

                if (!_ftDevice.SetDataCharacteristics(D2xxManager.FtDataBits8, D2xxManager.FtStopBits1, D2xxManager.FtParityNone))
                {
                    InterfaceDisconnect();
                    return false;
                }
                _currentWordLength = 8;
                _currentParity = EdInterfaceObd.SerialParity.None;

                if (!_ftDevice.SetFlowControl(D2xxManager.FtFlowNone, 0, 0))
                {
                    InterfaceDisconnect();
                    return false;
                }

                if (!_ftDevice.ClrDtr())
                {
                    InterfaceDisconnect();
                    return false;
                }

                if (!_ftDevice.ClrRts())
                {
                    InterfaceDisconnect();
                    return false;
                }

                if (!_ftDevice.Purge(D2xxManager.FtPurgeTx | D2xxManager.FtPurgeRx))
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
                if (_ftDevice != null)
                {
                    _ftDevice.SetBitMode(0x00, D2xxManager.FtBitmodeReset);
                    _ftDevice.Close();
                    _ftDevice.Dispose();
                    _ftDevice = null;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (_ftDevice == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            try
            {
                _currentBaudRate = baudRate;
                _currentWordLength = dataBits;
                _currentParity = parity;

                sbyte parityLocal;
                switch (parity)
                {
                    case EdInterfaceObd.SerialParity.None:
                        parityLocal = D2xxManager.FtParityNone;
                        break;

                    case EdInterfaceObd.SerialParity.Even:
                        parityLocal = D2xxManager.FtParityEven;
                        break;

                    case EdInterfaceObd.SerialParity.Odd:
                        parityLocal = D2xxManager.FtParityOdd;
                        break;

                    case EdInterfaceObd.SerialParity.Mark:
                        parityLocal = D2xxManager.FtParityMark;
                        break;

                    case EdInterfaceObd.SerialParity.Space:
                        parityLocal = D2xxManager.FtParitySpace;
                        break;

                    default:
                        return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }

                sbyte wordLengthLocal;
                switch (dataBits)
                {
                    case 7:
                        wordLengthLocal = D2xxManager.FtDataBits7;
                        break;

                    case 8:
                        wordLengthLocal = D2xxManager.FtDataBits8;
                        break;

                    default:
                        return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }

                if (!_ftDevice.SetBitMode(0x00, D2xxManager.FtBitmodeReset))
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                if (!_ftDevice.SetBaudRate(baudRate))
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                if (!_ftDevice.SetDataCharacteristics(wordLengthLocal, D2xxManager.FtStopBits1, parityLocal))
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                if (!_ftDevice.Purge(D2xxManager.FtPurgeTx | D2xxManager.FtPurgeRx))
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
            if (_ftDevice == null)
            {
                return false;
            }
            try
            {
                bool result = dtr ? _ftDevice.SetDtr() : _ftDevice.ClrDtr();
                if (!result)
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
            if (_ftDevice == null)
            {
                return false;
            }
            try
            {
                bool result = rts ? _ftDevice.SetRts() : _ftDevice.ClrRts();
                if (!result)
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
            if (_ftDevice == null)
            {
                return false;
            }
            try
            {
                short modemStatus = _ftDevice.ModemStatus;
                if (modemStatus < 0)
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
            if (_ftDevice == null)
            {
                return false;
            }
            try
            {
                bool result = enable ? _ftDevice.SetBreakOn() : _ftDevice.SetBreakOff();
                if (!result)
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
            if (_ftDevice == null)
            {
                return false;
            }
            try
            {
                if (!_ftDevice.Purge(D2xxManager.FtPurgeTx | D2xxManager.FtPurgeRx))
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
            if (_ftDevice == null)
            {
                return false;
            }
            try
            {
                int bytesWritten;

                int bitCount = (_currentParity == EdInterfaceObd.SerialParity.None) ? (_currentWordLength + 2) : (_currentWordLength + 3);
                double byteTime = 1.0d / _currentBaudRate * 1000 * bitCount;
                if (setDtr)
                {
                    long waitTime = (long)((dtrTimeCorr + byteTime * length) * TickResolMs);
                    if (!_ftDevice.SetDtr())
                    {
                        return false;
                    }
                    long startTime = Stopwatch.GetTimestamp();

                    bytesWritten = _ftDevice.Write(sendData, length);
                    if (bytesWritten != length)
                    {
                        return false;
                    }
                    while ((Stopwatch.GetTimestamp() - startTime) < waitTime)
                    {
                    }
                    if (!_ftDevice.ClrDtr())
                    {
                        return false;
                    }
                }
                else
                {
                    long waitTime = (long)(byteTime * length);

                    bytesWritten = _ftDevice.Write(sendData, length);
                    if (bytesWritten != length)
                    {
                        return false;
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
            if (_ftDevice == null)
            {
                return false;
            }
#if WindowsCE
            if (timeout < ReadTimeoutCeMin)
            {
                timeout = ReadTimeoutCeMin;
            }
            if (timeoutTelEnd < ReadTimeoutCeMin)
            {
                timeoutTelEnd = ReadTimeoutCeMin;
            }
#endif
            try
            {
                byte[] buffer = new byte[length];

                int recLen = _ftDevice.Read(buffer, 1, timeout);
                if (recLen < 1)
                {
                    return false;
                }
                Array.Copy(buffer, 0, receiveData, offset, recLen);
                if (recLen < length)
                {
                    while (recLen < length)
                    {
                        int bytesRead = _ftDevice.Read(buffer, length - recLen, timeout);
                        if (bytesRead < 0)
                        {
                            return false;
                        }
                        Array.Copy(buffer, 0, receiveData, offset + recLen, bytesRead);
                        if (bytesRead <= 0)
                        {
                            break;
                        }
                        recLen += bytesRead;
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
    }
}
