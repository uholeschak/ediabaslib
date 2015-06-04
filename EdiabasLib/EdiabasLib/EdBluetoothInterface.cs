using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Android.Bluetooth;
using Java.Util;

namespace EdiabasLib
{
    static public class EdBluetoothInterface
    {
        public const string PortId = "BLUETOOTH";
        private static readonly UUID SppUuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int ReadTimeoutOffset = 1000;

        private static BluetoothSocket _bluetoothSocket;
        private static Stream _bluetoothInStream;
        private static Stream _bluetoothOutStream;
        private static int _currentBaudRate;
        private static int _currentWordLength;
        private static EdInterfaceObd.SerialParity _currentParity = EdInterfaceObd.SerialParity.None;

        static EdBluetoothInterface()
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

        public static bool InterfaceConnect(string port)
        {
            if (_bluetoothSocket != null)
            {
                return true;
            }
            if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
            {
                InterfaceDisconnect();
                return false;
            }
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter == null)
            {
                return false;
            }
            try
            {
                BluetoothDevice device;
                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    string addr = portData.Remove(0, 1);
                    device = bluetoothAdapter.GetRemoteDevice(addr);
                }
                else
                {
                    InterfaceDisconnect();
                    return false;
                }
                if (device == null)
                {
                    InterfaceDisconnect();
                    return false;
                }
                bluetoothAdapter.CancelDiscovery();
                _bluetoothSocket = device.CreateRfcommSocketToServiceRecord (SppUuid);
                _bluetoothSocket.Connect();
                _bluetoothInStream = _bluetoothSocket.InputStream;
                _bluetoothOutStream = _bluetoothSocket.OutputStream;
            }
            catch (Exception)
            {
                InterfaceDisconnect ();
                return false;
            }
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            bool result = true;
            try
            {
                if (_bluetoothInStream != null)
                {
                    _bluetoothInStream.Close();
                    _bluetoothInStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            try
            {
                if (_bluetoothOutStream != null)
                {
                    _bluetoothOutStream.Close();
                    _bluetoothOutStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            try
            {
                if (_bluetoothSocket != null)
                {
                    _bluetoothSocket.Close();
                    _bluetoothSocket = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (_bluetoothSocket == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.CONFIG_ERROR;
            }
            _currentBaudRate = baudRate;
            _currentWordLength = dataBits;
            _currentParity = parity;
            return EdInterfaceObd.InterfaceErrorResult.NO_ERROR;
        }

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (_bluetoothSocket == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetRts(bool rts)
        {
            if (_bluetoothSocket == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = true;
            if (_bluetoothSocket == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            return false;
        }

        public static bool InterfacePurgeInBuffer()
        {
            if ((_bluetoothSocket == null) || (_bluetoothInStream == null))
            {
                return false;
            }
            try
            {
                _bluetoothInStream.Flush ();
                while (_bluetoothInStream.IsDataAvailable())
                {
                    _bluetoothInStream.ReadByte();
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
            if ((_bluetoothSocket == null) || (_bluetoothOutStream == null))
            {
                return false;
            }
            if ((_currentBaudRate != 115200) || (_currentWordLength != 8) || (_currentParity != EdInterfaceObd.SerialParity.None))
            {
                return false;
            }
            try
            {
                _bluetoothOutStream.Write (sendData, 0, length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            if ((_bluetoothSocket == null) || (_bluetoothInStream == null))
            {
                return false;
            }
            timeout += ReadTimeoutOffset;
            timeoutTelEnd += ReadTimeoutOffset;
            try
            {
                int recLen = 0;
                long startTime = Stopwatch.GetTimestamp();
                while (recLen < length)
                {
                    int currTimeout = (recLen == 0) ? timeout : timeoutTelEnd;
                    if (_bluetoothInStream.IsDataAvailable())
                    {
                        int bytesRead = _bluetoothInStream.Read (receiveData, offset + recLen, length - recLen);
                        recLen += bytesRead;
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if ((Stopwatch.GetTimestamp() - startTime) > currTimeout * TickResolMs)
                    {
                        if (ediabasLog != null)
                        {
                            ediabasLog.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, offset, recLen, "Rec ");
                        }
                        return false;
                    }
                    Thread.Sleep(10);
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
