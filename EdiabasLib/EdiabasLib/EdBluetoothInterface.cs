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
        public const string Elm327Tag = "ELM327";
        public static readonly string[] Elm327InitCommands = { "ATD", "ATE0", "ATSH6F1", "ATCF600", "ATCM700", "ATPBC001", "ATSPB", "ATAT0", "ATSTFF", "ATAL", "ATH1", "ATS0", "ATL0" };
        private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int ReadTimeoutOffset = 1000;
        private const int Elm327ReadTimeoutOffset = 1000;
        private const int Elm327CommandTimeout = 1500;
        private const int Elm327DataTimeout = 2000;
        private const int Elm327CanBlockSize = 8;
        private const int Elm327CanSepTime = 0;
        private static BluetoothSocket _bluetoothSocket;
        private static Stream _bluetoothInStream;
        private static Stream _bluetoothOutStream;
        private static bool _elm327Device;
        private static long _elm327ReceiveStartTime;
        private static bool _elm327DataMode;
        private static int _elm327CanHeader;
        private static Thread _elm327Thread;
        private static bool _elm327TerminateThread;
        private static readonly AutoResetEvent Elm327RequEvent = new AutoResetEvent(false);
        private static readonly AutoResetEvent Elm327RespEvent = new AutoResetEvent(false);
        private static byte[] _elm327RequBuffer;
        private static readonly Queue<byte> Elm327RespQueue = new Queue<byte>();
        private static readonly Object Elm327BufferLock = new Object();
        private static int _currentBaudRate;
        private static int _currentWordLength;
        private static EdInterfaceObd.SerialParity _currentParity = EdInterfaceObd.SerialParity.None;

        static EdBluetoothInterface()
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
                        if (string.Compare(stringList[1], Elm327Tag, StringComparison.OrdinalIgnoreCase) == 0)
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
            Elm327StopThread();
            Elm327Exit();
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
                lock (Elm327BufferLock)
                {
                    Elm327RespQueue.Clear();
                }
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
                lock (Elm327BufferLock)
                {
                    if (_elm327RequBuffer != null)
                    {
                        return false;
                    }
                }
                byte[] data = new byte[length];
                Array.Copy(sendData, data, length);
                lock (Elm327BufferLock)
                {
                    _elm327RequBuffer = data;
                }
                Elm327RequEvent.Set();
                // create echo
                lock (Elm327BufferLock)
                {
                    for (int i = 0; i < length; i++)
                    {
                        Elm327RespQueue.Enqueue(sendData[i]);
                    }
                }

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
                timeout += Elm327ReadTimeoutOffset;
                _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                for (;;)
                {
                    lock (Elm327BufferLock)
                    {
                        if (Elm327RespQueue.Count >= length)
                        {
                            break;
                        }
                    }
                    if ((Stopwatch.GetTimestamp() - Volatile.Read(ref _elm327ReceiveStartTime)) > timeout * TickResolMs)
                    {
                        if (Ediabas != null)
                        {
                            Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive timeout");
                        }
                        return false;
                    }
                    Elm327RespEvent.WaitOne(timeout, false);
                }
                lock (Elm327BufferLock)
                {
                    for (int i = 0; i < length; i++)
                    {
                        receiveData[i + offset] = Elm327RespQueue.Dequeue();
                    }
                }
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

        static public byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        private static bool Elm327Init()
        {
            _elm327DataMode = false;
            lock (Elm327BufferLock)
            {
                _elm327RequBuffer = null;
                Elm327RespQueue.Clear();
            }
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
            _elm327CanHeader = 0x6F1;
            Elm327StartThread();
            return true;
        }

        private static void Elm327Exit()
        {
            if (_elm327Device)
            {
                Elm327LeaveDataMode(Elm327CommandTimeout);
            }
        }

        private static void Elm327StartThread()
        {
            if (_elm327Thread != null)
            {
                return;
            }
            _elm327TerminateThread = false;
            Elm327RequEvent.Reset();
            Elm327RespEvent.Reset();
            _elm327Thread = new Thread(Elm327ThreadFunc)
            {
                Priority = ThreadPriority.Highest
            };
            _elm327Thread.Start();
        }

        private static void Elm327StopThread()
        {
            if (_elm327Thread != null)
            {
                _elm327TerminateThread = true;
                Elm327RequEvent.Set();
                _elm327Thread.Join();
                _elm327Thread = null;
                _elm327RequBuffer = null;
                Elm327RespQueue.Clear();
            }
        }

        private static void Elm327ThreadFunc()
        {
            while (!_elm327TerminateThread)
            {
                Elm327CanSender();
                Elm327CanReceiver();
                Elm327RequEvent.WaitOne(10, false);
            }
        }

        private static void Elm327CanSender()
        {
            byte[] requBuffer;
            lock (Elm327BufferLock)
            {
                requBuffer = _elm327RequBuffer;
                _elm327RequBuffer = null;
            }
            if (requBuffer != null && requBuffer.Length >= 4)
            {
                byte targetAddr = requBuffer[1];
                byte sourceAddr = requBuffer[2];
                int dataOffset = 3;
                int dataLength = requBuffer[0] & 0x3F;
                if (dataLength == 0)
                {
                    // with length byte
                    dataLength = requBuffer[3];
                    dataOffset = 4;
                }
                if (requBuffer.Length < (dataOffset + dataLength))
                {
                    return;
                }

                int canHeader = 0x600 | sourceAddr;
                if (_elm327CanHeader != canHeader)
                {
                    if (!Elm327SendCommand("ATSH" + string.Format("{0:X03}", canHeader)))
                    {
                        return;
                    }
                    _elm327CanHeader = canHeader;
                }
                byte[] canSendBuffer = new byte[8];
                if (dataLength <= 6)
                {
                    // single frame
                    if (Ediabas != null)
                    {
                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Send SF");
                    }
                    canSendBuffer[0] = targetAddr;
                    canSendBuffer[1] = (byte) (0x00 | dataLength); // SF
                    Array.Copy(requBuffer, dataOffset, canSendBuffer, 2, dataLength);
                    Elm327SendCanTelegram(canSendBuffer);
                }
                else
                {
                    // first frame
                    if (Ediabas != null)
                    {
                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Send FF");
                    }
                    canSendBuffer[0] = targetAddr;
                    canSendBuffer[1] = (byte) (0x10 | ((dataLength >> 8) & 0xFF)); // FF
                    canSendBuffer[2] = (byte) dataLength;
                    int telLen = 5;
                    Array.Copy(requBuffer, dataOffset, canSendBuffer, 3, telLen);
                    dataLength -= telLen;
                    dataOffset += telLen;
                    if (!Elm327SendCanTelegram(canSendBuffer))
                    {
                        return;
                    }
                    byte blockSize = 0;
                    byte sepTime = 0;
                    bool waitForFc = true;
                    byte blockCount = 1;
                    for (;;)
                    {
                        if (waitForFc)
                        {
                            if (Ediabas != null)
                            {
                                Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Wait for FC");
                            }
                            bool wait = false;
                            do
                            {
                                int[] canRecData = Elm327ReceiveCanTelegram(Elm327DataTimeout);
                                if (canRecData == null)
                                {
                                    if (Ediabas != null)
                                    {
                                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "*** FC timeout");
                                    }
                                    return;
                                }
                                if (canRecData.Length >= 5 &&
                                    ((canRecData[0] & 0xFF00) == 0x0600) &&
                                    ((canRecData[0] & 0xFF) == targetAddr) && (canRecData[1 + 0] == sourceAddr) &&
                                    ((canRecData[1 + 1] & 0xF0) == 0x30)
                                    )
                                {
                                    byte frameControl = (byte)(canRecData[1 + 1] & 0x0F);
                                    switch (frameControl)
                                    {
                                        case 0: // CTS
                                            wait = false;
                                            break;

                                        case 1: // Wait
                                            if (Ediabas != null)
                                            {
                                                Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Wait for next FC");
                                            }
                                            wait = true;
                                            break;

                                        default:
                                            if (Ediabas != null)
                                            {
                                                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid FC: {0:X01}", frameControl);
                                            }
                                            return;
                                    }
                                    blockSize = (byte) canRecData[1 + 2];
                                    sepTime = (byte) canRecData[1 + 3];
                                    _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                                    if (Ediabas != null)
                                    {
                                        Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BS={0} ST={1}", blockSize, sepTime);
                                    }
                                }
                                if (_elm327TerminateThread)
                                {
                                    return;
                                }
                            }
                            while (wait);
                        }

                        if (Ediabas != null)
                        {
                            Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Send CF");
                        }
                        // consecutive frame
                        Array.Clear(canSendBuffer, 0, canSendBuffer.Length);
                        canSendBuffer[0] = targetAddr;
                        canSendBuffer[1] = (byte) (0x20 | (blockCount & 0x0F)); // CF
                        telLen = dataLength;
                        if (telLen > 6)
                        {
                            telLen = 6;
                        }
                        Array.Copy(requBuffer, dataOffset, canSendBuffer, 2, telLen);
                        dataLength -= telLen;
                        dataOffset += telLen;
                        blockCount++;
                        if (!Elm327SendCanTelegram(canSendBuffer))
                        {
                            return;
                        }
                        if (dataLength <= 0)
                        {
                            break;
                        }

                        waitForFc = false;
                        if (blockSize > 0)
                        {
                            if (blockSize == 1)
                            {
                                waitForFc = true;
                            }
                            blockSize--;
                        }
                        if (!waitForFc)
                        {   // we have to wait here, otherwise thread requires too much compuation time
                            Thread.Sleep(sepTime < 10 ? 10 : sepTime);
                        }
                        if (_elm327TerminateThread)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private static void Elm327CanReceiver()
        {
            byte blockCount = 0;
            byte sourceAddr = 0;
            byte targetAddr = 0;
            byte fcCount = 0;
            int recLen = 0;
            byte[] recDataBuffer = null;
            for (; ; )
            {
                if (recLen == 0 && !_bluetoothInStream.IsDataAvailable())
                {
                    return;
                }
                int[] canRecData = Elm327ReceiveCanTelegram(Elm327DataTimeout);
                if (canRecData != null && canRecData.Length >= 9)
                {
                    byte frameType = (byte)((canRecData[1 + 1] >> 4) & 0x0F);
                    int telLen;
                    if (recLen == 0)
                    {
                        // first telegram
                        sourceAddr = (byte)(canRecData[0] & 0xFF);
                        targetAddr = (byte)canRecData[1 + 0];
                        switch (frameType)
                        {
                            case 0: // single frame
                                if (Ediabas != null)
                                {
                                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Rec SF");
                                }
                                telLen = canRecData[2] & 0x0F;
                                if (telLen > 6)
                                {
                                    return;
                                }
                                recDataBuffer = new byte[telLen];
                                for (int i = 0; i < telLen; i++)
                                {
                                    recDataBuffer[i] = (byte)canRecData[1 + 2 + i];
                                }
                                recLen = telLen;
                                _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                                break;

                            case 1: // first frame
                            {
                                if (Ediabas != null)
                                {
                                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Rec FF");
                                }
                                telLen = ((canRecData[1 + 1] & 0x0F) << 8) + canRecData[1 + 2];
                                recDataBuffer = new byte[telLen];
                                recLen = 5;
                                for (int i = 0; i < recLen; i++)
                                {
                                    recDataBuffer[i] = (byte)canRecData[1 + 3 + i];
                                }
                                blockCount = 1;

                                byte[] canSendBuffer = new byte[8];
                                canSendBuffer[0] = sourceAddr;
                                canSendBuffer[1] = 0x30; // FC
                                canSendBuffer[2] = Elm327CanBlockSize;
                                canSendBuffer[3] = Elm327CanSepTime;
                                fcCount = Elm327CanBlockSize;
                                if (!Elm327SendCanTelegram(canSendBuffer))
                                {
                                    return;
                                }
                                _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                                break;
                            }

                            default:
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Rec invalid frame {0:X01}", frameType);
                                }
                                return;
                        }
                    }
                    else
                    {
                        // next frame
                        if (frameType == 2 && recDataBuffer != null &&
                            (sourceAddr == (canRecData[0] & 0xFF)) && (targetAddr == canRecData[1 + 0]) &&
                            (canRecData[1 + 1] & 0x0F) == (blockCount & 0x0F)
                            )
                        {
                            if (Ediabas != null)
                            {
                                Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Rec CF");
                            }
                            telLen = recDataBuffer.Length - recLen;
                            if (telLen > 6)
                            {
                                telLen = 6;
                            }
                            for (int i = 0; i < telLen; i++)
                            {
                                recDataBuffer[recLen + i] = (byte)canRecData[1 + 2 + i];
                            }
                            recLen += telLen;
                            blockCount++;
                            if (fcCount > 0 && recLen < recDataBuffer.Length)
                            {
                                fcCount--;
                                if (fcCount == 0)
                                {   // send FC
                                    if (Ediabas != null)
                                    {
                                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "(Rec) Send FC");
                                    }
                                    byte[] canSendBuffer = new byte[8];
                                    canSendBuffer[0] = sourceAddr;
                                    canSendBuffer[1] = 0x30; // FC
                                    canSendBuffer[2] = Elm327CanBlockSize;
                                    canSendBuffer[3] = Elm327CanSepTime;
                                    fcCount = Elm327CanBlockSize;
                                    if (!Elm327SendCanTelegram(canSendBuffer))
                                    {
                                        return;
                                    }
                                }
                            }
                            _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                        }
                    }
                    if (recDataBuffer != null && recLen >= recDataBuffer.Length)
                    {
                        break;
                    }
                }
                else
                {
                    if (canRecData == null)
                    {   // nothing received
                        return;
                    }
                }
                if (_elm327TerminateThread)
                {
                    return;
                }
            }

            if (recLen >= recDataBuffer.Length)
            {
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Received length: {0}", recLen);
                }
                byte[] responseTel;
                // create BMW-FAST telegram
                if (recDataBuffer.Length > 0x3F)
                {
                    responseTel = new byte[recDataBuffer.Length + 4];
                    responseTel[0] = 0x80;
                    responseTel[1] = targetAddr;
                    responseTel[2] = sourceAddr;
                    responseTel[3] = (byte)recDataBuffer.Length;
                    Array.Copy(recDataBuffer, 0, responseTel, 4, recDataBuffer.Length);
                }
                else
                {
                    responseTel = new byte[recDataBuffer.Length + 3];
                    responseTel[0] = (byte)(0x80 | recDataBuffer.Length);
                    responseTel[1] = targetAddr;
                    responseTel[2] = sourceAddr;
                    Array.Copy(recDataBuffer, 0, responseTel, 3, recDataBuffer.Length);
                }
                byte checkSum = CalcChecksumBmwFast(responseTel, responseTel.Length);
                lock (Elm327BufferLock)
                {
                    foreach (byte data in responseTel)
                    {
                        Elm327RespQueue.Enqueue(data);
                    }
                    Elm327RespQueue.Enqueue(checkSum);
                }
                Elm327RespEvent.Set();
            }
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
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CMD send: {0}", command);
                }
                if (readAnswer)
                {
                    string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                    // check for OK
                    if (!answer.Contains("OK\r"))
                    {
                        if (Ediabas != null)
                        {
                            Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM invalid response: {0}", answer);
                        }
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
                if (Ediabas != null)
                {
                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN send: {0}", stringBuilder.ToString());
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
            bool elmThread = _elm327Thread != null && Thread.CurrentThread == _elm327Thread;
            StringBuilder stringBuilder = new StringBuilder();
            while (_bluetoothInStream.IsDataAvailable())
            {
                int data = _bluetoothInStream.ReadByte();
                if (data >= 0)
                {
                    stringBuilder.Append(Convert.ToChar(data));
                    if (data == 0x3E)
                    {
                        if (Ediabas != null)
                        {
                            Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM data mode already terminated");
                        }
                        _elm327DataMode = false;
                        return true;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                _bluetoothOutStream.WriteByte(0x20);    // space
            }
            if (Ediabas != null)
            {
                Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM send SPACE");
            }

            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                while (_bluetoothInStream.IsDataAvailable())
                {
                    int data = _bluetoothInStream.ReadByte();
                    if (data >= 0)
                    {
                        stringBuilder.Append(Convert.ToChar(data));
                        if (data == 0x3E)
                        {
                            if (Ediabas != null)
                            {
                                string response = stringBuilder.ToString();
                                if (!response.Contains("STOPPED\r"))
                                {
                                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM data mode not stopped: " + stringBuilder);
                                }
                                else
                                {
                                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM data mode terminated");
                                }
                            }
                            _elm327DataMode = false;
                            return true;
                        }
                    }
                }
                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    if (Ediabas != null)
                    {
                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "*** ELM leave data mode timeout");
                    }
                    return false;
                }
                if (elmThread)
                {
                    if (_elm327TerminateThread)
                    {
                        return false;
                    }
                    Elm327RequEvent.WaitOne(10, false);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private static string Elm327ReceiveAnswer(int timeout, bool canData = false)
        {
            bool elmThread = _elm327Thread != null && Thread.CurrentThread == _elm327Thread;
            StringBuilder stringBuilder = new StringBuilder();
            long startTime = Stopwatch.GetTimestamp();
            for (; ; )
            {
                while (_bluetoothInStream.IsDataAvailable())
                {
                    int data = _bluetoothInStream.ReadByte();
                    if (data >= 0 && data != 0x00)
                    {   // remove 0x00
                        if (canData)
                        {
                            if (data == '\r')
                            {
                                string answer = stringBuilder.ToString();
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN rec: {0}", answer);
                                }
                                return answer;
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
                                if (Ediabas != null)
                                {
                                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM Data mode aborted");
                                }
                                return string.Empty;
                            }
                            string answer = stringBuilder.ToString();
                            if (Ediabas != null)
                            {
                                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CMD rec: {0}", answer);
                            }
                            return answer;
                        }
                    }
                }
                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    if (Ediabas != null)
                    {
                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM rec timeout");
                    }
                    return string.Empty;
                }
                if (elmThread)
                {
                    if (_elm327TerminateThread)
                    {
                        return string.Empty;
                    }
                    Elm327RequEvent.WaitOne(10, false);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
