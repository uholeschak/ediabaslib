using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private const int Elm327CommandTimeout = 2000;
        private static readonly string[] Elm327InitCommands = { "AT D", "AT E0", "AT SH 6F1", "AT CAF0", "AT CF 600", "AT CM 700", "AT SP 6", "AT AL", "AT H1", "AT S0", "AT L0" };
        private static BluetoothSocket _bluetoothSocket;
        private static Stream _bluetoothInStream;
        private static Stream _bluetoothOutStream;
        private static bool _elm327Device;
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

        public static bool InterfaceConnect(string port, object parameter)
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
            _elm327Device = false;
            try
            {
                BluetoothDevice device;
                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    string addr = portData.Remove(0, 1);
                    string[] stringList = addr.Split(';');
                    if (stringList.Length == 0)
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                    device = bluetoothAdapter.GetRemoteDevice(stringList[0]);
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], "ELM327", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _elm327Device = true;
                        }
                    }
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

                if (_elm327Device)
                {
                    if (!Elm327Init())
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }
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
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            _currentBaudRate = baudRate;
            _currentWordLength = dataBits;
            _currentParity = parity;
            return EdInterfaceObd.InterfaceErrorResult.NoError;
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
            if (_elm327Device)
            {
                return true;
            }
            try
            {
                FlushReceiveBuffer();
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
            if (_elm327Device)
            {
                return true;
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
            if (_elm327Device)
            {
                return true;
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
                            ediabasLog.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
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

        private static void FlushReceiveBuffer()
        {
            _bluetoothInStream.Flush();
            while (_bluetoothInStream.IsDataAvailable())
            {
                _bluetoothInStream.ReadByte();
            }
        }

        private static bool Elm327Init()
        {
            bool firstCommand = true;
            foreach (string command in Elm327InitCommands)
            {
                if (!Elm327SendCommand(command))
                {
                    if (!firstCommand)
                    {
                        return false;
                    }
                    if (!Elm327SendCommand(command))
                    {
                        return false;
                    }
                }
                firstCommand = false;
            }

            return true;
        }

        private static bool Elm327SendCommand(string command)
        {
            try
            {
                FlushReceiveBuffer();
                byte[] sendData = Encoding.UTF8.GetBytes(command + "\r");
                _bluetoothOutStream.Write(sendData, 0, sendData.Length);

                string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                // check for OK
                if (!answer.Contains("OK\r"))
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
#if false
        private static bool Elm327SendCanTelegram(byte[] canTelegram)
        {
            try
            {
                FlushReceiveBuffer();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte data in canTelegram)
                {
                    stringBuilder.Append((string.Format("{0:X02}", data)));
                }
                stringBuilder.Append("\r");
                byte[] sendData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                _bluetoothOutStream.Write(sendData, 0, sendData.Length);

                string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
#endif
        private static string Elm327ReceiveAnswer(int timeout)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte[] buffer = new byte[1];
            long startTime = Stopwatch.GetTimestamp();
            for (; ; )
            {
                while (_bluetoothInStream.IsDataAvailable())
                {
                    int bytesRead = _bluetoothInStream.Read(buffer, 0, 1);
                    if (bytesRead > 0 && buffer[0] != 0x00)
                    {   // remove 0x00
                        stringBuilder.Append(Convert.ToChar(buffer[0]));
                        if (buffer[0] == 0x3E)
                        {
                            return stringBuilder.ToString();
                        }
                    }
                }
                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    return string.Empty;
                }
                Thread.Sleep(10);
            }
        }
    }
}
