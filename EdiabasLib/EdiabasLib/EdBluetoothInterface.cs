using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly string[] Elm327InitCommands = { "ATD", "ATE0", "ATSH6F1", "ATCF600", "ATCM700", "ATPBC001", "ATSPB", "ATAT0", "ATSTFF", "ATAL", "ATH1", "ATS0", "ATL0" };
        private static BluetoothSocket _bluetoothSocket;
        private static Stream _bluetoothInStream;
        private static Stream _bluetoothOutStream;
        private static bool _elm327Device;
        private static bool _elm327DataMode;
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
            _elm327DataMode = false;
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
#if false
            if (!Elm327SendCanTelegram(new byte[] {0x12, 0x02, 0x01A, 0x80, 0x00, 0x00, 0x00, 0x00}))
            {
                return false;
            }
            int[] data = Elm327ReceiveCanTelegram(Elm327CommandTimeout);
            if ((data == null) || (data.Length != 9))
            {
                return false;
            }
            if ((data[0] != 0x612) || (data[1] != 0xF1) || (data[2] != 0x10) || (data[3] != 0x3C))
            {
                return false;
            }
            if (!Elm327SendCanTelegram(new byte[] { 0x12, 0x30, 0x00, 0x00 }))
            {
                return false;
            }
            for (int i = 0; i < 10; i++)
            {
                data = Elm327ReceiveCanTelegram(Elm327CommandTimeout);
                if ((data == null) || (data.Length != 9))
                {
                    return false;
                }
                if ((data[0] != 0x612) || (data[1] != 0xF1) || (data[2] != (0x20 + ((i + 1) & 0x0F))))
                {
                    return false;
                }
            }
#endif
            return true;
        }

        private static bool Elm327SendCommand(string command, bool readAnswer = true)
        {
            try
            {
                if (!Elm327LeaveDataMode(Elm327CommandTimeout))
                {
                    _elm327DataMode = false;
                    return false;
                }
                FlushReceiveBuffer();
                byte[] sendData = Encoding.UTF8.GetBytes(command + "\r");
                _bluetoothOutStream.Write(sendData, 0, sendData.Length);
                if (readAnswer)
                {
                    string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                    // check for OK
                    if (!answer.Contains("OK\r"))
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool Elm327SendCanTelegram(byte[] canTelegram)
        {
            try
            {
                if (!Elm327LeaveDataMode(Elm327CommandTimeout))
                {
                    _elm327DataMode = false;
                    return false;
                }
                FlushReceiveBuffer();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte data in canTelegram)
                {
                    stringBuilder.Append((string.Format("{0:X02}", data)));
                }
                stringBuilder.Append("\r");
                byte[] sendData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                _bluetoothOutStream.Write(sendData, 0, sendData.Length);
                _elm327DataMode = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static int[] Elm327ReceiveCanTelegram(int timeout)
        {
            List<int> resultList = new List<int>();
            try
            {
                if (!_elm327DataMode)
                {
                    return null;
                }
                string answer = Elm327ReceiveAnswer(timeout, true);
                if (!_elm327DataMode)
                {   // switch to monitor mode
                    if (!Elm327SendCommand("ATMA", false))
                    {
                        return null;
                    }
                    _elm327DataMode = true;
                }
                if (string.IsNullOrEmpty(answer))
                {
                    return null;
                }
                if ((answer.Length & 0x01) == 0)
                {   // must be odd because of can header
                    return null;
                }
                if (!Regex.IsMatch(answer, @"\A[0-9a-fA-F]{3,19}\Z"))
                {
                    return null;
                }
                resultList.Add(Convert.ToInt32(answer.Substring(0, 3), 16));
                for (int i = 3; i < answer.Length; i += 2)
                {
                    resultList.Add(Convert.ToInt32(answer.Substring(i, 2), 16));
                }
            }
            catch (Exception)
            {
                return null;
            }
            return resultList.ToArray();
        }

        private static bool Elm327LeaveDataMode(int timeout)
        {
            if (!_elm327DataMode)
            {
                return true;
            }
            byte[] buffer = new byte[1];
            while (_bluetoothInStream.IsDataAvailable())
            {
                int bytesRead = _bluetoothInStream.Read(buffer, 0, 1);
                if (bytesRead > 0 && buffer[0] == 0x3E)
                {
                    _elm327DataMode = false;
                    return true;
                }
            }

            buffer[0] = 0x20;   // space
            _bluetoothOutStream.Write(buffer, 0, 1);

            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                while (_bluetoothInStream.IsDataAvailable())
                {
                    int bytesRead = _bluetoothInStream.Read(buffer, 0, 1);
                    if (bytesRead > 0 && buffer[0] == 0x3E)
                    {
                        _elm327DataMode = false;
                        return true;
                    }
                }
                if ((timeout == 0) || ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs))
                {
                    return false;
                }
                Thread.Sleep(10);
            }
        }

        private static string Elm327ReceiveAnswer(int timeout, bool canData = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte[] buffer = new byte[1];
            long startTime = Stopwatch.GetTimestamp();
            for (; ; )
            {
                while (_bluetoothInStream.IsDataAvailable())
                {
                    int bytesRead = _bluetoothInStream.Read(buffer, 0, 1);
                    byte data = buffer[0];
                    if (bytesRead > 0 && data != 0x00)
                    {   // remove 0x00
                        if (canData)
                        {
                            if (data == '\r')
                            {
                                return stringBuilder.ToString();
                            }
                            stringBuilder.Append(Convert.ToChar(data));
                        }
                        else
                        {
                            stringBuilder.Append(Convert.ToChar(data));
                        }
                        if (data == 0x3E)
                        {
                            _elm327DataMode = false;
                            if (canData)
                            {
                                return string.Empty;
                            }
                            return stringBuilder.ToString();
                        }
                    }
                }
                if ((timeout == 0) || ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs))
                {
                    return string.Empty;
                }
                Thread.Sleep(10);
            }
        }
    }
}
