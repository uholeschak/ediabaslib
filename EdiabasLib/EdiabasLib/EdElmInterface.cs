using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdElmInterface : IDisposable
    {
        public class ElmInitEntry
        {
            public ElmInitEntry(string command, int version = -1, bool okResponse = true)
            {
                Command = command;
                OkResponse = okResponse;
                Version = version;
            }

            public string Command { get; }
            public bool OkResponse { get; }
            public int Version { get; }
        }

        public static ElmInitEntry[] Elm327InitCommands =
        {
            new ElmInitEntry("ATD"),
            new ElmInitEntry("ATE0"),
            new ElmInitEntry("ATPP2COFF"),      // reject fake elms (disables also WGSoft adapters)
            new ElmInitEntry("ATSH6F1"),
            new ElmInitEntry("ATCF600"),
            new ElmInitEntry("ATCM700"),
            new ElmInitEntry("ATPBC001"),
            new ElmInitEntry("ATSPB"),
            new ElmInitEntry("ATAT0"),
            new ElmInitEntry("ATSTFF"),
            new ElmInitEntry("ATAL"),
            new ElmInitEntry("ATH1"),
            new ElmInitEntry("ATS0"),
            new ElmInitEntry("ATL0"),
            new ElmInitEntry("ATCSM0", 210),    // disable silent monitoring
            new ElmInitEntry("ATCTM5", 210),    // timer multiplier 5
            new ElmInitEntry("ATJE", 130),      // ELM data format, used for fake ELM detection
            //new ElmInitEntry("ATPPS", -1, false),     // some BT chips have a short buffer, so this test will fail
        };

        public static ElmInitEntry[] Elm327InitFullTransport =
        {
            new ElmInitEntry("ATSH6F1"),
            new ElmInitEntry("ATFCSH6F1"),
            new ElmInitEntry("ATPBC101"),   // set Parameter for CAN B Custom Protocol 11/500 with var. DLC
            new ElmInitEntry("ATBI"),       // bypass init sequence
        };

        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int Elm327ReadTimeoutOffset = 1000;
        private const int Elm327CommandTimeout = 1500;
        private const int Elm327DataTimeout = 2000;
        private const int Elm327CanBlockSize = 8;
        private const int Elm327CanSepTime = 0;
        private bool _disposed;
        private readonly Stream _inStream;
        private readonly Stream _outStream;
        private long _elm327ReceiveStartTime;
        private bool _elm327DataMode;
        private bool _elm327FullTransport;
        private int _elm327CanHeader;
        private int _elm327Timeout;
        private Thread _elm327Thread;
        private bool _elm327TerminateThread;
        private readonly AutoResetEvent _elm327RequEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _elm327RespEvent = new AutoResetEvent(false);
        private volatile byte[] _elm327RequBuffer;
        private readonly Queue<byte> _elm327RespQueue = new Queue<byte>();
        private readonly Object _elm327BufferLock = new Object();

        public bool StreamFailure { get; set; }
        public EdiabasNet Ediabas { get; set; }

        public EdElmInterface(EdiabasNet ediabas, Stream inStream, Stream outStream)
        {
            Ediabas = ediabas;
            _inStream = inStream;
            _outStream = outStream;
        }

        public bool InterfaceDisconnect()
        {
            Elm327StopThread();
            Elm327Exit();
            StreamFailure = false;

            return true;
        }

        public bool InterfacePurgeInBuffer()
        {
            lock (_elm327BufferLock)
            {
                _elm327RespQueue.Clear();
            }
            return true;
        }

        public bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            lock (_elm327BufferLock)
            {
                if (_elm327RequBuffer != null)
                {
                    return false;
                }
            }
            byte[] data = new byte[length];
            Array.Copy(sendData, data, length);
            lock (_elm327BufferLock)
            {
                _elm327RequBuffer = data;
            }
            _elm327RequEvent.Set();

            return true;
        }

        public bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            timeout += Elm327ReadTimeoutOffset;
            _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                lock (_elm327BufferLock)
                {
                    if (_elm327RespQueue.Count >= length)
                    {
                        break;
                    }
                }
                if ((Stopwatch.GetTimestamp() - _elm327ReceiveStartTime) > timeout * TickResolMs)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive timeout");
                    return false;
                }
                _elm327RespEvent.WaitOne(timeout, false);
            }
            lock (_elm327BufferLock)
            {
                for (int i = 0; i < length; i++)
                {
                    receiveData[i + offset] = _elm327RespQueue.Dequeue();
                }
            }
            return true;
        }

        public bool Elm327Init()
        {
            _elm327DataMode = false;
            lock (_elm327BufferLock)
            {
                _elm327RequBuffer = null;
                _elm327RespQueue.Clear();
            }
            bool firstCommand = true;
            foreach (ElmInitEntry elmInitEntry in Elm327InitCommands)
            {
                bool optional = elmInitEntry.Version >= 0;
                if (!Elm327SendCommand(elmInitEntry.Command, elmInitEntry.OkResponse))
                {
                    if (!firstCommand)
                    {
                        if (!optional)
                        {
                            return false;
                        }
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM optional command {0} failed", elmInitEntry.Command);
                    }
                    if (firstCommand && !optional)
                    {
                        if (!Elm327SendCommand(elmInitEntry.Command, elmInitEntry.OkResponse))
                        {
                            return false;
                        }
                    }
                }
                if (!elmInitEntry.OkResponse)
                {
                    string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                    if (string.IsNullOrEmpty(answer))
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM no answer");
                    }
                }
                firstCommand = false;
            }

            if (!Elm327SendCommand("AT@1", false))
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Sending @1 failed");
                return false;
            }
            string elmDevDesc = Elm327ReceiveAnswer(Elm327CommandTimeout);
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM ID: {0}", elmDevDesc);

            if (!Elm327SendCommand("AT#1", false))
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Sending #1 failed");
                return false;
            }
            string elmManufact = Elm327ReceiveAnswer(Elm327CommandTimeout);
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM Manufacturer: {0}", elmManufact);

            _elm327FullTransport = elmManufact.ToUpperInvariant().Contains("WGSOFT");
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM full transport: {0}", _elm327FullTransport);

            if (_elm327FullTransport)
            {
                foreach (ElmInitEntry elmInitEntry in Elm327InitFullTransport)
                {
                    if (!Elm327SendCommand(elmInitEntry.Command))
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM full transport command {0} failed", elmInitEntry.Command);
                        return false;
                    }
                }
            }

            _elm327CanHeader = 0x6F1;
            _elm327Timeout = -1;
            StreamFailure = false;
            Elm327StartThread();
            return true;
        }

        public void Elm327Exit()
        {
            try
            {
                Elm327LeaveDataMode(Elm327CommandTimeout);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Elm327StartThread()
        {
            if (_elm327Thread != null)
            {
                return;
            }
            _elm327TerminateThread = false;
            _elm327RequEvent.Reset();
            _elm327RespEvent.Reset();
            _elm327Thread = new Thread(Elm327ThreadFunc)
            {
                Priority = ThreadPriority.Highest
            };
            _elm327Thread.Start();
        }

        private void Elm327StopThread()
        {
            if (_elm327Thread != null)
            {
                _elm327TerminateThread = true;
                _elm327RequEvent.Set();
                _elm327Thread.Join();
                _elm327Thread = null;
                _elm327RequBuffer = null;
                _elm327RespQueue.Clear();
            }
        }

        private void Elm327ThreadFunc()
        {
            while (!_elm327TerminateThread)
            {
                if (_elm327FullTransport)
                {
                    Elm327CanSenderFull();
                }
                else
                {
                    Elm327CanSender();
                }
                Elm327CanReceiver();
                _elm327RequEvent.WaitOne(10, false);
            }
        }

        private void Elm327CanSenderFull()
        {
            byte[] requBuffer;
            lock (_elm327BufferLock)
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
                        _elm327CanHeader = -1;
                        return;
                    }
                    if (!Elm327SendCommand("ATFCSH" + string.Format("{0:X03}", canHeader)))
                    {
                        _elm327CanHeader = -1;
                        return;
                    }
                    _elm327CanHeader = canHeader;
                }
                if (!Elm327SendCommand("ATFCSD" + string.Format("{0:X02}300000", targetAddr)))
                {
                    return;
                }
                if (!Elm327SendCommand("ATCEA" + string.Format("{0:X02}", targetAddr)))
                {
                    return;
                }
                if (!Elm327SendCommand("ATFCSM1"))
                {
                    return;
                }

                byte[] canSendBuffer = new byte[dataLength];
                Array.Copy(requBuffer, dataOffset, canSendBuffer, 0, dataLength);
                Elm327SendCanTelegram(canSendBuffer);
            }
        }

        private void Elm327CanSender()
        {
            byte[] requBuffer;
            lock (_elm327BufferLock)
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
                        _elm327CanHeader = -1;
                        return;
                    }
                    _elm327CanHeader = canHeader;
                }
                byte[] canSendBuffer = new byte[8];
                if (dataLength <= 6)
                {
                    // single frame
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Send SF");
                    canSendBuffer[0] = targetAddr;
                    canSendBuffer[1] = (byte)(0x00 | dataLength); // SF
                    Array.Copy(requBuffer, dataOffset, canSendBuffer, 2, dataLength);
                    Elm327SendCanTelegram(canSendBuffer);
                }
                else
                {
                    // first frame
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Send FF");
                    canSendBuffer[0] = targetAddr;
                    canSendBuffer[1] = (byte)(0x10 | ((dataLength >> 8) & 0xFF)); // FF
                    canSendBuffer[2] = (byte)dataLength;
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
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Wait for FC");
                            bool wait = false;
                            do
                            {
                                int[] canRecData = Elm327ReceiveCanTelegram(Elm327DataTimeout);
                                if (canRecData == null)
                                {
                                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** FC timeout");
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
                                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Wait for next FC");
                                            wait = true;
                                            break;

                                        default:
                                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid FC: {0:X01}", frameControl);
                                            return;
                                    }
                                    blockSize = (byte)canRecData[1 + 2];
                                    sepTime = (byte)canRecData[1 + 3];
                                    _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BS={0} ST={1}", blockSize, sepTime);
                                }
                                if (_elm327TerminateThread)
                                {
                                    return;
                                }
                            }
                            while (wait);
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
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Send CF");
                        bool expectResponse = (waitForFc || (dataLength <= 6));
                        // consecutive frame
                        Array.Clear(canSendBuffer, 0, canSendBuffer.Length);
                        canSendBuffer[0] = targetAddr;
                        canSendBuffer[1] = (byte)(0x20 | (blockCount & 0x0F)); // CF
                        telLen = dataLength;
                        if (telLen > 6)
                        {
                            telLen = 6;
                        }
                        Array.Copy(requBuffer, dataOffset, canSendBuffer, 2, telLen);
                        dataLength -= telLen;
                        dataOffset += telLen;
                        blockCount++;
                        if (!Elm327SendCanTelegram(canSendBuffer, expectResponse))
                        {
                            return;
                        }
                        if (dataLength <= 0)
                        {
                            break;
                        }

                        if (!waitForFc)
                        {   // we have to wait here, otherwise thread requires too much computation time
                            Thread.Sleep(sepTime < 50 ? 50 : sepTime);
                        }
                        if (_elm327TerminateThread)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void Elm327CanReceiver()
        {
            byte blockCount = 0;
            byte sourceAddr = 0;
            byte targetAddr = 0;
            byte fcCount = 0;
            int recLen = 0;
            byte[] recDataBuffer = null;
            for (;;)
            {
                if (recLen == 0 && !DataAvailable())
                {
                    return;
                }
                int[] canRecData = Elm327ReceiveCanTelegram(Elm327DataTimeout);
                if (canRecData != null && canRecData.Length >= (1 + 2))
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
                                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Rec SF");
                                telLen = canRecData[2] & 0x0F;
                                if (telLen > (canRecData.Length - 1 - 2))
                                {
                                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Invalid length");
                                    continue;
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
                                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Rec FF");
                                    if (canRecData.Length < (1 + 8))
                                    {
                                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Invalid length");
                                        continue;
                                    }
                                    telLen = ((canRecData[1 + 1] & 0x0F) << 8) + canRecData[1 + 2];
                                    recDataBuffer = new byte[telLen];
                                    recLen = 5;
                                    for (int i = 0; i < recLen; i++)
                                    {
                                        recDataBuffer[i] = (byte)canRecData[1 + 3 + i];
                                    }
                                    blockCount = 1;

                                    if (!_elm327FullTransport)
                                    {
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
                                    _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                                    break;
                                }

                            default:
                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Rec invalid frame {0:X01}", frameType);
                                continue;
                        }
                    }
                    else
                    {
                        // next frame
                        if (frameType == 2 && recDataBuffer != null &&
                            (sourceAddr == (canRecData[0] & 0xFF)) && (targetAddr == canRecData[1 + 0]))
                        {
                            int blockCount1 = canRecData[1 + 1] & 0x0F;
                            int blockCount2 = blockCount & 0x0F;
                            if (blockCount1 != blockCount2)
                            {
                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Invalid block count: {0} {1}", blockCount1, blockCount2);
                                continue;
                            }
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Rec CF");
                            telLen = recDataBuffer.Length - recLen;
                            if (telLen > 6)
                            {
                                telLen = 6;
                            }
                            if (telLen > (canRecData.Length - 1 - 2))
                            {
                                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Invalid length");
                                continue;
                            }
                            for (int i = 0; i < telLen; i++)
                            {
                                recDataBuffer[recLen + i] = (byte)canRecData[1 + 2 + i];
                            }
                            recLen += telLen;
                            blockCount++;
                            if (!_elm327FullTransport && fcCount > 0 && recLen < recDataBuffer.Length)
                            {
                                fcCount--;
                                if (fcCount == 0)
                                {   // send FC
                                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "(Rec) Send FC");
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
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Received length: {0}", recLen);
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
                byte checkSum = CalcChecksumBmwFast(responseTel, 0, responseTel.Length);
                lock (_elm327BufferLock)
                {
                    foreach (byte data in responseTel)
                    {
                        _elm327RespQueue.Enqueue(data);
                    }
                    _elm327RespQueue.Enqueue(checkSum);
                }
                _elm327RespEvent.Set();
            }
        }

        private bool Elm327SendCommand(string command, bool readAnswer = true)
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
                _outStream.Write(sendData, 0, sendData.Length);
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CMD send: {0}", command);
                if (readAnswer)
                {
                    string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                    // check for OK
                    if (!answer.Contains("OK\r"))
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM invalid response: {0}", answer);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM stream failure: {0}", ex.Message);
                StreamFailure = true;
                return false;
            }
            return true;
        }

        private bool Elm327SendCanTelegram(byte[] canTelegram, bool expectResponse = true)
        {
            try
            {
                int timeout = expectResponse ? 0xFF : 0x00;
                if ((timeout == 0x00) || (timeout != _elm327Timeout))
                {
                    if (!Elm327SendCommand(string.Format("ATST{0:X02}", timeout), false))
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Setting timeout failed");
                        _elm327Timeout = -1;
                        return false;
                    }
                    string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                    // check for OK
                    if (!answer.Contains("OK\r") && !answer.Contains("STOPPED\r") && !answer.Contains("NO DATA\r") && !answer.Contains("DATA ERROR\r"))
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM set timeout invalid response: {0}", answer);
                        _elm327Timeout = -1;
                        return false;
                    }
                }
                _elm327Timeout = timeout;

                if (!Elm327LeaveDataMode(Elm327CommandTimeout))
                {
                    _elm327DataMode = false;
                    _elm327Timeout = -1;
                    return false;
                }
                FlushReceiveBuffer();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte data in canTelegram)
                {
                    stringBuilder.Append((string.Format("{0:X02}", data)));
                }
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN send: {0}", stringBuilder.ToString());
                stringBuilder.Append("\r");
                byte[] sendData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                _outStream.Write(sendData, 0, sendData.Length);
                _elm327DataMode = expectResponse;
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM stream failure: {0}", ex.Message);
                StreamFailure = true;
                return false;
            }
            return true;
        }

        private int[] Elm327ReceiveCanTelegram(int timeout)
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
#if false
                    // Monitor mode disables CAN ack,
                    // for testing a second CAN node is required.
                    // With this hack this can be avoided
                    if (!Elm327SendCanTelegram(new byte[] { 0x00 }))
#else
                    if (!Elm327SendCommand("ATMA", false))
#endif
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

        private bool Elm327LeaveDataMode(int timeout)
        {
            if (!_elm327DataMode)
            {
                return true;
            }
            bool elmThread = _elm327Thread != null && Thread.CurrentThread == _elm327Thread;
            StringBuilder stringBuilder = new StringBuilder();
            while (DataAvailable())
            {
                int data = _inStream.ReadByte();
                if (data >= 0)
                {
                    stringBuilder.Append(Convert.ToChar(data));
                    if (data == 0x3E)
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM data mode already terminated: " + stringBuilder);
                        _elm327DataMode = false;
                        return true;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                _outStream.WriteByte(0x20);    // space
            }
            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM send SPACE");

            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                while (DataAvailable())
                {
                    int data = _inStream.ReadByte();
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
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** ELM leave data mode timeout");
                    return false;
                }
                if (elmThread)
                {
                    if (_elm327TerminateThread)
                    {
                        return false;
                    }
                    _elm327RequEvent.WaitOne(10, false);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private string Elm327ReceiveAnswer(int timeout, bool canData = false)
        {
            bool elmThread = _elm327Thread != null && Thread.CurrentThread == _elm327Thread;
            StringBuilder stringBuilder = new StringBuilder();
            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                while (DataAvailable())
                {
                    int data = _inStream.ReadByte();
                    if (data >= 0 && data != 0x00)
                    {   // remove 0x00
                        if (canData)
                        {
                            if (data == '\r')
                            {
                                string answer = stringBuilder.ToString();
                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN rec: {0}", answer);
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
                                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM Data mode aborted");
                                return string.Empty;
                            }
                            string answer = stringBuilder.ToString();
                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CMD rec: {0}", answer);
                            return answer;
                        }
                    }
                }
                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM rec timeout");
                    return string.Empty;
                }
                if (elmThread)
                {
                    if (_elm327TerminateThread)
                    {
                        return string.Empty;
                    }
                    _elm327RequEvent.WaitOne(10, false);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void FlushReceiveBuffer()
        {
            _inStream.Flush();
            while (DataAvailable())
            {
                _inStream.ReadByte();
            }
        }

        private bool DataAvailable()
        {
#if Android
            return _inStream.IsDataAvailable();
#else
            if (!(_inStream is System.Net.Sockets.NetworkStream networkStream))
            {
                return false;
            }
            return networkStream.DataAvailable;
#endif
        }

        public static byte CalcChecksumBmwFast(byte[] data, int offset, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i + offset];
            }
            return sum;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                InterfaceDisconnect();
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
