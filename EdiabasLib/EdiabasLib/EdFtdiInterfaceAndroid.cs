using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Android.Hardware.Usb;
using Android.OS;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseNullPropagation
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace EdiabasLib
{
    public static class EdFtdiInterface
    {
        public class ConnectParameterType
        {
            public ConnectParameterType(UsbManager usbManager)
            {
                UsbManager = usbManager;
            }

            public UsbManager UsbManager { get; private set; }
        }

        public const string PortId = "FTDI";
        private const int WriteTimeout = 500;
        private const int ReadTimeoutOffset = 1000;
        private const int UsbBlockSize = 0x4000;
        private const int LatencyTime = 50;     // large value required to prevent data loss
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static IUsbSerialPort _usbPort;
        private static SerialInputOutputManager _serialIoManager;
        private static readonly AutoResetEvent DataReceiveEvent = new AutoResetEvent(false);
        private static readonly Queue<byte> ReadQueue = new Queue<byte>();
        private static readonly object QueueLock = new object();
        private static int _currentBaudRate;
        private static int _currentWordLength;
        private static EdInterfaceObd.SerialParity _currentParity = EdInterfaceObd.SerialParity.None;

        static EdFtdiInterface()
        {
        }

        public static EdiabasNet Ediabas { get; set; }

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
            if (_usbPort != null)
            {
                return true;
            }
            try
            {
                if (!(parameter is ConnectParameterType connectParameter))
                {
                    return false;
                }

                if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
                {
                    InterfaceDisconnect();
                    return false;
                }

                List<IUsbSerialDriver> availableDrivers = GetDriverList(connectParameter.UsbManager);
                if (availableDrivers.Count <= 0)
                {
                    InterfaceDisconnect();
                    return false;
                }

                string portData = port.Remove(0, PortId.Length);
                int portIndex = -1;
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    if (portData.StartsWith(":SER=", StringComparison.OrdinalIgnoreCase))
                    {   // serial number
                        string id = portData.Remove(0, 5);
                        int index = 0;
                        foreach (IUsbSerialDriver serialDriver in availableDrivers)
                        {
                            if (serialDriver.Ports[0] != null && string.Compare(serialDriver.Ports[0].Serial, id, StringComparison.Ordinal) == 0)
                            {
                                portIndex = index;
                                break;
                            }
                            index++;
                        }
                    }
                }
                else
                {
                    portIndex = Convert.ToInt32(port.Remove(0, PortId.Length));
                }

                if ((portIndex < 0) || (portIndex >= availableDrivers.Count))
                {
                    InterfaceDisconnect();
                    return false;
                }
                IUsbSerialDriver driver = availableDrivers[portIndex];
                UsbDeviceConnection connection = connectParameter.UsbManager.OpenDevice(driver.Device);
                if (connection == null)
                {
                    InterfaceDisconnect();
                    return false;
                }
                if (driver.Ports.Count < 1)
                {
                    InterfaceDisconnect();
                    return false;
                }
                _usbPort = driver.Ports[0];
                _usbPort.Open(connection);
                _usbPort.SetParameters(9600, 8, StopBits.One, Parity.None);
                if (_usbPort is FtdiSerialDriver.FtdiSerialPort ftdiPort)
                {
                    ftdiPort.LatencyTimer = LatencyTime;
                    if (ftdiPort.LatencyTimer != LatencyTime)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }
                _currentWordLength = 8;
                _currentParity = EdInterfaceObd.SerialParity.None;

                _usbPort.DTR = false;
                _usbPort.RTS = false;
                lock (QueueLock)
                {
                    ReadQueue.Clear();
                }

                _serialIoManager = new SerialInputOutputManager(_usbPort);
                _serialIoManager.DataReceived += (sender, e) =>
                {
                    lock (QueueLock)
                    {
                        foreach (byte value in e.Data)
                        {
                            ReadQueue.Enqueue(value);
                        }
                        DataReceiveEvent.Set();
                    }
                };
                _serialIoManager.Start(UsbBlockSize);
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
                if (_serialIoManager != null)
                {
                    _serialIoManager.Stop();
                    _serialIoManager.Dispose();
                    _serialIoManager = null;
                }
                if (_usbPort != null)
                {
                    _usbPort.Close();
                    _usbPort.Dispose();
                    _usbPort = null;
                }
                lock (QueueLock)
                {
                    ReadQueue.Clear();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (_usbPort == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            if (protocol != EdInterfaceObd.Protocol.Uart)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            try
            {
                _currentBaudRate = baudRate;
                _currentWordLength = dataBits;
                _currentParity = parity;

                Parity parityLocal;
                switch (parity)
                {
                    case EdInterfaceObd.SerialParity.None:
                        parityLocal = Parity.None;
                        break;

                    case EdInterfaceObd.SerialParity.Even:
                        parityLocal = Parity.Even;
                        break;

                    case EdInterfaceObd.SerialParity.Odd:
                        parityLocal = Parity.Odd;
                        break;

                    case EdInterfaceObd.SerialParity.Mark:
                        parityLocal = Parity.Mark;
                        break;

                    case EdInterfaceObd.SerialParity.Space:
                        parityLocal = Parity.Space;
                        break;

                    default:
                        return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }

                _usbPort.SetParameters(baudRate, dataBits, StopBits.One, parityLocal);
                if (!_usbPort.PurgeHwBuffers(true, true))
                {
                    return EdInterfaceObd.InterfaceErrorResult.ConfigError;
                }
                lock (QueueLock)
                {
                    ReadQueue.Clear();
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
            if (_usbPort == null)
            {
                return false;
            }
            try
            {
                _usbPort.DTR = dtr;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetRts(bool rts)
        {
            if (_usbPort == null)
            {
                return false;
            }
            try
            {
                _usbPort.RTS = rts;
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
            if (_usbPort == null)
            {
                return false;
            }
            try
            {
                dsr = _usbPort.DSR;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            if (_usbPort == null)
            {
                return false;
            }
            if (enable == false)
            {
                return false;
            }
            return true;
        }

        public static bool InterfacePurgeInBuffer()
        {
            if (_usbPort == null)
            {
                return false;
            }
            try
            {
                if (!_usbPort.PurgeHwBuffers(true, false))
                {
                    return false;
                }
                lock (QueueLock)
                {
                    ReadQueue.Clear();
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
            if (_usbPort == null)
            {
                return false;
            }
            if ((_serialIoManager == null) || !_serialIoManager.IsStarted)
            {
                return false;
            }
            try
            {
                int bytesWritten;
                byte[] sendBuffer = new byte[length];
                Array.Copy(sendData, sendBuffer, sendBuffer.Length);

                int bitCount = (_currentParity == EdInterfaceObd.SerialParity.None)
                    ? (_currentWordLength + 2)
                    : (_currentWordLength + 3);
                double byteTime = 1.0d/_currentBaudRate*1000*bitCount;
                if (setDtr)
                {
                    long waitTime = (long) ((dtrTimeCorr + byteTime*length)*TickResolMs);
                    _usbPort.DTR = true;
                    long startTime = Stopwatch.GetTimestamp();

                    bytesWritten = _usbPort.Write(sendBuffer, WriteTimeout);
                    if (bytesWritten != length)
                    {
                        return false;
                    }
                    while ((Stopwatch.GetTimestamp() - startTime) < waitTime)
                    {
                    }
                    _usbPort.DTR = false;
                }
                else
                {
                    long waitTime = (long) (byteTime*length);

                    bytesWritten = _usbPort.Write(sendBuffer, WriteTimeout);
                    if (bytesWritten != length)
                    {
                        return false;
                    }
                    if (waitTime > 10)
                    {
                        Thread.Sleep((int) waitTime);
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
            if (_usbPort == null)
            {
                return false;
            }
            if ((_serialIoManager == null) || !_serialIoManager.IsStarted)
            {
                return false;
            }
            if (length <= 0)
            {
                return true;
            }
            timeout += ReadTimeoutOffset;
            timeoutTelEnd += ReadTimeoutOffset;

            try
            {
                int recLen = 0;
                if (!ReadData(receiveData, offset + recLen, 1, timeout))
                {
                    return false;
                }
                recLen++;
                if (recLen < length)
                {
                    if (!ReadData(receiveData, offset + recLen, length - recLen, timeoutTelEnd))
                    {
                        return false;
                    }
                }
                if (ediabasLog != null)
                {
                    ediabasLog.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool ReadData(byte[] buffer, int offset, int length, int timeout)
        {
            if (_usbPort == null)
            {
                return false;
            }
            if ((_serialIoManager == null) || !_serialIoManager.IsStarted)
            {
                return false;
            }
            if (buffer.Length < offset + length)
            {
                return false;
            }
            if (length <= 0)
            {
                return true;
            }
            try
            {
                int recLen = 0;
                for (;;)
                {
                    lock (QueueLock)
                    {
                        while (ReadQueue.Count > 0)
                        {
                            buffer[offset + recLen] = ReadQueue.Dequeue();
                            recLen++;
                            if (recLen >= length)
                            {
                                return true;
                            }
                        }
                        DataReceiveEvent.Reset();
                    }
                    DataReceiveEvent.WaitOne(timeout, false);
                    lock (QueueLock)
                    {
                        if (ReadQueue.Count == 0)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidUsbDevice(UsbDevice usbDevice)
        {
            return IsValidUsbDevice(usbDevice, out bool _);
        }

        public static bool IsValidUsbDevice(UsbDevice usbDevice, out bool fakeDevice)
        {
            fakeDevice = false;
            if (usbDevice != null)
            {
                if (usbDevice.VendorId == 0x0403)
                {
                    switch (usbDevice.ProductId)
                    {
                        case 0x6001:
                            return true;

                        case 0x0000:
                            fakeDevice = true;
                            return true;
                    }
                }
            }
            return false;
        }

        public static List<IUsbSerialDriver> GetDriverList(UsbManager usbManager)
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.HoneycombMr1)
                {
                    return new List<IUsbSerialDriver>();
                }
                IList<IUsbSerialDriver> availableDrivers = UsbSerialProber.DefaultProber.FindAllDrivers(usbManager);
                return availableDrivers.Where(driver => IsValidUsbDevice(driver.Device)).ToList();
            }
            catch (Exception)
            {
                return new List<IUsbSerialDriver>();
            }
        }
    }
}
