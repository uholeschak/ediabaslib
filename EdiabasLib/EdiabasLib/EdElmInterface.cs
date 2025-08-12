using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdElmInterface : IDisposable
    {
        public enum TransportType
        {
            Standard,
            WgSoft,
            Carly,
        }

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

        public class ElmRequQueueEntry
        {
            public ElmRequQueueEntry(byte[] data, int timeout = -1)
            {
                Data = data;
                Timeout = timeout;
            }

            public byte[] Data { get; }
            public int Timeout { get; }
        }

#if ANDROID && DEBUG
        private static readonly string Tag = typeof(EdElmInterface).FullName;
#endif
        public const string Elm327CarlyIdentifier = "CARLY-UNIVERSAL";
        public const string Elm327WgSoftIdentifier = "WGSOFT";
        public const double Elm327WgSoftMinVer = 2.4;

        public static ElmInitEntry[] Elm327InitCommands =
        {
            new ElmInitEntry("ATD"),
            new ElmInitEntry("ATE0"),
            //new ElmInitEntry("ATPP2COFF"),      // reject fake elms (disables also WGSoft adapters)
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
            new ElmInitEntry("ATGB1"),      // switch to binary mode, optional
            new ElmInitEntry("ATSH6F1"),
            new ElmInitEntry("ATFCSH6F1"),
            new ElmInitEntry("ATPBC101"),   // set Parameter for CAN B Custom Protocol 11/500 with var. DLC
            new ElmInitEntry("ATBI"),       // bypass init sequence
        };

        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int Elm327ReadTimeoutOffset = 1000;
        private const int Elm327CommandTimeout = 1500;
        private const int Elm327DataTimeout = 2000;
        private const int Elm327DataFullTimeout = 500;
        private const int Elm327DataFullFuncTimeout = 200;
        private const int Elm327CanBlockSize = 3;
        private const int Elm327CanSepTime = 0;
        private const int Elm327TimeoutBaseMultiplier = 4;
        private bool _disposed;
        private readonly Stream _inStream;
        private readonly Stream _outStream;
        private readonly ManualResetEvent _cancelEvent;
        private long _elm327ReceiveStartTime;
        private bool _elm327DataMode;
        private TransportType _elm327TransportType;
        private volatile bool _elm327ReceiverBusy;
        private int _elm327TimeoutMultiplier = 1;
        private bool _elm327BinaryData;
        private int _elm327CanHeader;
        private int _elm327Timeout;
        private Thread _elm327Thread;
        private bool _elm327TerminateThread;
        private AutoResetEvent _elm327RequEvent;
        private AutoResetEvent _elm327RespEvent;
        private volatile byte[] _elm327RequBuffer;
        private readonly Queue<ElmRequQueueEntry> _elm327RequQueue = new Queue<ElmRequQueueEntry>();
        private readonly Queue<byte> _elm327RespQueue = new Queue<byte>();
        private volatile List<string> _elm327FullRespList;
        private readonly Object _elm327BufferLock = new Object();

        public bool StreamFailure { get; set; }
        public EdiabasNet Ediabas { get; set; }

        public EdElmInterface(EdiabasNet ediabas, Stream inStream, Stream outStream, ManualResetEvent cancelEvent = null)
        {
            Ediabas = ediabas;
            _inStream = inStream;
            _outStream = outStream;
            _cancelEvent = cancelEvent;
            _elm327RequEvent = new AutoResetEvent(false);
            _elm327RespEvent = new AutoResetEvent(false);
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

                if (ReceiverBusy())
                {
                    _elm327ReceiveStartTime = Stopwatch.GetTimestamp();
                }
                else
                {
                    if ((Stopwatch.GetTimestamp() - _elm327ReceiveStartTime) > timeout * TickResolMs)
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Receive timeout");
                        return false;
                    }
                }

                _elm327RespEvent.WaitOne(timeout);
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
            _elm327ReceiverBusy = false;
            _elm327TimeoutMultiplier = 1;
            _elm327BinaryData = false;
            _elm327TransportType = TransportType.Standard;
            lock (_elm327BufferLock)
            {
                _elm327RequBuffer = null;
                _elm327RequQueue.Clear();
                _elm327RespQueue.Clear();
                _elm327FullRespList = null;
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
                else
                {
                    if (string.Compare(elmInitEntry.Command, "ATCTM5", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _elm327TimeoutMultiplier = 5;
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

            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM timeout multiplier: {0}", _elm327TimeoutMultiplier);

            if (!Elm327SendCommand("AT@1", false))
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Sending @1 failed");
                return false;
            }

            string elmDevDesc = TrimElmResponse(Elm327ReceiveAnswer(Elm327CommandTimeout));
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM ID: {0}", elmDevDesc);
            if (elmDevDesc.ToUpperInvariant().Contains(Elm327CarlyIdentifier))
            {
                _elm327TransportType = TransportType.Carly;
            }

            if (!Elm327SendCommand("AT#1", false))
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Sending #1 failed");
                return false;
            }

            string elmManufact = TrimElmResponse(Elm327ReceiveAnswer(Elm327CommandTimeout));
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM Manufacturer: {0}", elmManufact);

            if (_elm327TransportType == TransportType.Standard && elmManufact.ToUpperInvariant().Contains(Elm327WgSoftIdentifier))
            {
                if (double.TryParse(elmDevDesc, NumberStyles.Float, CultureInfo.InvariantCulture, out double version))
                {
                    if (version < Elm327WgSoftMinVer)
                    {
                        _elm327TransportType = TransportType.WgSoft;
                    }
                }
            }

            if (!Elm327SendCommand("STI", false))
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Sending STI failed");
                return false;
            }

            string stnVers = TrimElmResponse(Elm327ReceiveAnswer(Elm327CommandTimeout));
            if (stnVers.ToUpperInvariant().Contains("STN"))
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "STN Version: {0}", stnVers);
                if (!Elm327SendCommand("STIX", false))
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Sending STIX failed");
                    return false;
                }

                string stnVersExt = TrimElmResponse(Elm327ReceiveAnswer(Elm327CommandTimeout));
                if (!string.IsNullOrEmpty(stnVersExt))
                {
                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "STN Ext Version: {0}", stnVersExt);
                }
            }

            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM transport type: {0}", _elm327TransportType);

            if (_elm327TransportType != TransportType.Standard)
            {
                foreach (ElmInitEntry elmInitEntry in Elm327InitFullTransport)
                {
                    bool binaryCmd = string.Compare(elmInitEntry.Command, "ATGB1", StringComparison.OrdinalIgnoreCase) == 0;
                    if (!Elm327SendCommand(elmInitEntry.Command))
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM carly transport command {0} failed", elmInitEntry.Command);
                        if (!binaryCmd)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (binaryCmd)
                        {
                            _elm327BinaryData = true;
                        }
                    }
                }
            }

            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM binary data: {0}", _elm327BinaryData);

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
                _elm327RequQueue.Clear();
                _elm327RespQueue.Clear();
            }
        }

        private void Elm327ThreadFunc()
        {
            while (!_elm327TerminateThread)
            {
                if (_elm327TransportType != TransportType.Standard)
                {
                    Elm327CanSenderFull();
                }
                else
                {
                    Elm327CanSender();
                }

                Elm327CanReceiver();
                _elm327RequEvent.WaitOne(10);
            }
        }

        private void Elm327CanSenderFull()
        {
            ElmRequQueueEntry reqEntry = null;

            lock (_elm327BufferLock)
            {
                if (_elm327RequQueue.Count == 0)
                {
                    byte[] tempBuffer = _elm327RequBuffer;
                    _elm327RequBuffer = null;

                    if (tempBuffer != null && tempBuffer.Length >= 4)
                    {
                        bool funcAddress = (tempBuffer[0] & 0xC0) == 0xC0;     // functional address
                        if (funcAddress)
                        {
                            List<byte> ecuAddrList = new List<byte>();
                            if (Ediabas != null)
                            {
                                List<List<string>> tableLines = Ediabas.GetTableLines("GROBNAME");
                                List<string> addrList = Ediabas.GetTableColumn(tableLines, "ADR");
                                if (addrList != null)
                                {
                                    foreach (string addrString in addrList)
                                    {
                                        Int64 value = EdiabasNet.StringToValue(addrString, out bool valid);
                                        if (valid)
                                        {
                                            ecuAddrList.Add((byte) value);
                                        }
                                    }
                                }
                            }

                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Functional address replacement list size: {0}", ecuAddrList.Count);
                            if (ecuAddrList.Count == 0)
                            {
                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ignoring functional address request");
                            }
                            else
                            {
                                foreach (byte addr in ecuAddrList)
                                {
                                    byte[] queueBuffer = new byte[tempBuffer.Length];
                                    Array.Copy(tempBuffer, queueBuffer, tempBuffer.Length);
                                    queueBuffer[0] = (byte)((queueBuffer[0] & ~0xC0) | 0x80);
                                    queueBuffer[1] = addr;
                                    _elm327RequQueue.Enqueue(new ElmRequQueueEntry(queueBuffer, Elm327DataFullFuncTimeout));
                                }
                            }
                        }
                        else
                        {
                            _elm327RequQueue.Enqueue(new ElmRequQueueEntry(tempBuffer));
                        }
                    }
                }

                if (_elm327RequQueue.Count > 0)
                {
                    reqEntry = _elm327RequQueue.Dequeue();
                }
            }

            if (reqEntry != null && reqEntry.Data.Length >= 4)
            {
                byte[] reqBuffer = reqEntry.Data;
                byte targetAddr = reqBuffer[1];
                byte sourceAddr = reqBuffer[2];
                int dataOffset = 3;
                int dataLength = reqBuffer[0] & 0x3F;
                if (dataLength == 0)
                {
                    // with length byte
                    if (reqBuffer[3] == 0x00)
                    {
                        dataLength = (reqBuffer[4] << 8) + reqBuffer[5];
                        dataOffset = 6;
                    }
                    else
                    {
                        dataLength = reqBuffer[3];
                        dataOffset = 4;
                    }
                }

                if (reqBuffer.Length < (dataOffset + dataLength))
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

                int blockSize = 0x00;
                int sepTime = _elm327TransportType == TransportType.Carly ? 0x02 : 0x00;
                if (!Elm327SendCommand("ATFCSD" + string.Format("{0:X02}30{1:X02}{2:X02}", targetAddr, blockSize, sepTime)))
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
                Array.Copy(reqBuffer, dataOffset, canSendBuffer, 0, dataLength);
                Elm327SendCanTelegram(canSendBuffer, true, reqEntry.Timeout);
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
                    if (requBuffer[3] == 0x00)
                    {
                        dataLength = (requBuffer[4] << 8) + requBuffer[5];
                        dataOffset = 6;
                    }
                    else
                    {
                        dataLength = requBuffer[3];
                        dataOffset = 4;
                    }
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
                    canSendBuffer[1] = (byte)(0x10 | ((dataLength >> 8) & 0x0F)); // FF
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
                                if (AbortTransmission())
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
                        if (AbortTransmission())
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
                bool dataAvailable = _elm327TransportType != TransportType.Standard || DataAvailable();
                if (recLen == 0 && !dataAvailable)
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

                                    if (_elm327TransportType == TransportType.Carly)
                                    {
                                        bool respListEmpty;
                                        lock (_elm327BufferLock)
                                        {
                                            respListEmpty = _elm327FullRespList == null || _elm327FullRespList.Count == 0;
                                        }

                                        if (respListEmpty)
                                        {
                                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Carly aborted transmission, creating dummy response");
                                            recLen = recDataBuffer.Length;
                                            // send dummy byte to abort transmission
                                            byte[] canSendBuffer = new byte[1];
                                            canSendBuffer[0] = 0x00;
                                            if (!Elm327SendCanTelegram(canSendBuffer))
                                            {
                                                return;
                                            }
                                            break;
                                        }
                                    }

                                    if (_elm327TransportType == TransportType.Standard)
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
                            if (_elm327TransportType == TransportType.Standard && fcCount > 0 && recLen < recDataBuffer.Length)
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
                if (AbortTransmission())
                {
                    return;
                }
            }

            if (recLen >= recDataBuffer.Length)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Received length: {0}", recLen);
                byte[] responseTel;
                // create BMW-FAST telegram
                if (recDataBuffer.Length > 0xFF)
                {
                    responseTel = new byte[recDataBuffer.Length + 6];
                    responseTel[0] = 0x80;
                    responseTel[1] = targetAddr;
                    responseTel[2] = sourceAddr;
                    responseTel[3] = 0x00;
                    responseTel[4] = (byte)(recDataBuffer.Length >> 8);
                    responseTel[5] = (byte)recDataBuffer.Length;
                    Array.Copy(recDataBuffer, 0, responseTel, 6, recDataBuffer.Length);
                }
                else if (recDataBuffer.Length > 0x3F)
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
#if ANDROID && DEBUG
                Android.Util.Log.Info(Tag, string.Format("ELM CMD send: {0}", command));
#endif
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CMD send: {0}", command);
                if (readAnswer)
                {
                    string answer = Elm327ReceiveAnswer(Elm327CommandTimeout);
                    // check for OK
                    if (!answer.Contains("OK\r"))
                    {
#if ANDROID && DEBUG
                        Android.Util.Log.Info(Tag, string.Format("ELM invalid response: {0}", answer));
#endif
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM invalid response: {0}", answer);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
#if ANDROID && DEBUG
                Android.Util.Log.Info(Tag, string.Format("ELM stream failure: {0}", EdiabasNet.GetExceptionText(ex)));
#endif
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM stream failure: {0}", EdiabasNet.GetExceptionText(ex));
                StreamFailure = true;
                return false;
            }
            return true;
        }

        private bool Elm327SendCanTelegram(byte[] canTelegram, bool expectResponse = true, int specialTimeout = -1)
        {
            try
            {
                int timeout = expectResponse? 0xFF : 0x00;
                if (_elm327TransportType != TransportType.Standard)
                {
                    if (specialTimeout >= 0)
                    {
                        timeout = specialTimeout / Elm327TimeoutBaseMultiplier / _elm327TimeoutMultiplier;
                    }
                    else
                    {
                        timeout = Elm327DataFullTimeout / Elm327TimeoutBaseMultiplier / _elm327TimeoutMultiplier;
                    }
                }

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
#if ANDROID && DEBUG
                Android.Util.Log.Info(Tag, string.Format("ELM CAN send: {0}", stringBuilder));
#endif
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN send: {0}", stringBuilder.ToString());
                stringBuilder.Append("\r");
                byte[] sendData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                _outStream.Write(sendData, 0, sendData.Length);
                _elm327DataMode = expectResponse;
            }
            catch (Exception ex)
            {
#if ANDROID && DEBUG
                Android.Util.Log.Info(Tag, string.Format("ELM stream failure: {0}", EdiabasNet.GetExceptionText(ex)));
#endif
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ELM stream failure: {0}", EdiabasNet.GetExceptionText(ex));
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

                string answer;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (_elm327TransportType != TransportType.Standard)
                {
                    answer = Elm327DataFullAnswer(timeout);
                }
                else
                {
                    answer = Elm327ReceiveAnswer(timeout, true);
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
                }

                if (string.IsNullOrEmpty(answer))
                {
                    return null;
                }
                // remove all spaces
                answer = answer.Replace(" ", string.Empty);
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
            if (_elm327TransportType != TransportType.Standard)
            {
                _elm327DataMode = false;
                _elm327ReceiverBusy = false;
                lock (_elm327BufferLock)
                {
                    _elm327FullRespList = null;
                }
                return true;
            }

            if (!_elm327DataMode)
            {
                return true;
            }

            bool elmThread = _elm327Thread != null && Thread.CurrentThread == _elm327Thread;
            StringBuilder stringBuilder = new StringBuilder();
            while (DataAvailable())
            {
#if ANDROID
                int data = _inStream.ReadByte();
#else
                int data = _inStream.ReadByteAsync(_cancelEvent);
#endif
                if (data >= 0)
                {
                    stringBuilder.Append(ConvertToChar(data));
                    if (data == 0x3E)
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM data mode already terminated: " + stringBuilder);
                        _elm327DataMode = false;
                        return true;
                    }
                }
                else
                {
                    break;
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
#if ANDROID
                    int data = _inStream.ReadByte();
#else
                    int data = _inStream.ReadByteAsync(_cancelEvent);
#endif
                    if (data >= 0)
                    {
                        stringBuilder.Append(ConvertToChar(data));
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
                    else
                    {
                        break;
                    }
                }
                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** ELM leave data mode timeout");
                    return false;
                }

                if (AbortTransmission())
                {
                    return false;
                }

                if (elmThread)
                {
                    _elm327RequEvent.WaitOne(10);
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
#if ANDROID
                    int data = _inStream.ReadByte();
#else
                    int data = _inStream.ReadByteAsync(_cancelEvent);
#endif
                    if (data < 0)
                    {
                        break;
                    }

                    if (data != 0x00)
                    {   // remove 0x00
                        if (canData)
                        {
                            if (data == '\r')
                            {
                                string answer = stringBuilder.ToString();
#if ANDROID && DEBUG
                                Android.Util.Log.Info(Tag, string.Format("ELM CMD rec: {0}", answer));
#endif
                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN rec: {0}", answer);
                                return answer;
                            }
                            stringBuilder.Append(ConvertToChar(data));
                        }
                        else
                        {
                            stringBuilder.Append(ConvertToChar(data));
                        }
                        if (data == 0x3E)
                        {
                            _elm327DataMode = false;
                            if (canData)
                            {
#if ANDROID && DEBUG
                                Android.Util.Log.Info(Tag, "ELM Data mode aborted");
#endif
                                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM Data mode aborted");
                                return string.Empty;
                            }
                            string answer = stringBuilder.ToString();
#if ANDROID && DEBUG
                            Android.Util.Log.Info(Tag, string.Format("ELM CMD rec: {0}", answer));
#endif
                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CMD rec: {0}", answer);
                            return answer;
                        }
                    }
                }

                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
#if ANDROID && DEBUG
                    Android.Util.Log.Info(Tag, "ELM rec timeout");
#endif
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM rec timeout");
                    return string.Empty;
                }

                if (AbortTransmission())
                {
                    return string.Empty;
                }

                if (elmThread)
                {
                    _elm327RequEvent.WaitOne(10);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private string Elm327DataFullAnswer(int timeout)
        {
            lock (_elm327BufferLock)
            {
                if (_elm327FullRespList != null && _elm327FullRespList.Count > 0)
                {
                    string answer = _elm327FullRespList[0];
                    _elm327FullRespList.RemoveAt(0);
                    if (_elm327FullRespList.Count == 0)
                    {
                        _elm327DataMode = false;
                        _elm327FullRespList = null;
                    }

                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN rec cached: {0}", answer);
                    return answer;
                }

                _elm327FullRespList = null;
            }

            try
            {
                bool elmThread = _elm327Thread != null && Thread.CurrentThread == _elm327Thread;
                int recTimeout = timeout;
                if (_elm327Timeout > 0)
                {
                    recTimeout = (_elm327Timeout * 4 * _elm327TimeoutMultiplier) + 200;
                }
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM receive timeout: {0}", recTimeout);

                List<byte> recData = new List<byte>();
                StringBuilder recString = new StringBuilder();
                byte[] buffer = new byte[100];
                long startTime = Stopwatch.GetTimestamp();
                _elm327ReceiverBusy = true;

                for (; ; )
                {
                    while (DataAvailable())
                    {
                        int length = _inStream.ReadBytesAsync(buffer, 0, buffer.Length);
                        if (length < 0)
                        {
                            break;
                        }

                        if (length > 0)
                        {
                            startTime = Stopwatch.GetTimestamp();
                            if (!_elm327BinaryData)
                            {
                                bool finished = false;
                                for (int pos = 0; pos < length; pos++)
                                {
                                    switch (buffer[pos])
                                    {
                                        case (byte)'\n':
                                            break;

                                        case 0x3E:
                                            finished = true;
                                            break;

                                        default:
                                            recString.Append(ConvertToChar(buffer[pos]));
                                            break;
                                    }
                                }

                                if (finished)
                                {
                                    string[] recArray = recString.ToString().Split('\r');
                                    List<string> recList = new List<string>();
                                    foreach (string line in recArray)
                                    {
                                        string trimmedLine = line.Trim();
                                        if (!string.IsNullOrWhiteSpace(trimmedLine))
                                        {
                                            recList.Add(trimmedLine);
                                        }
                                    }

                                    if (recList.Count == 0)
                                    {
                                        _elm327DataMode = false;
                                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM CAN receive list empty");
                                        return string.Empty;
                                    }

                                    string answer = recList[0];
                                    recList.RemoveAt(0);
                                    if (recList.Count == 0)
                                    {
                                        _elm327DataMode = false;
                                    }
                                    else
                                    {
                                        lock (_elm327BufferLock)
                                        {
                                            _elm327FullRespList = recList;
                                        }
                                    }

                                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN rec: {0}", answer);
                                    return answer;
                                }

                                break;
                            }

                            if (length < 2)
                            {
                                break;
                            }

                            bool lastBlock = false;
                            switch (buffer[0])
                            {
                                case 0xBB:
                                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM rec bin length: {0}", length - 1);
                                    break;

                                case 0xBE:
                                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM rec bin last length: {0}", length - 1);
                                    lastBlock = true;
                                    break;

                                default:
                                    _elm327DataMode = false;
                                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM rec no binary data");
                                    return string.Empty;
                            }

                            recData.AddRange(buffer.ToList().GetRange(1, length - 1));

                            if (lastBlock)
                            {
                                List<string> recList = new List<string>();
                                int telLength = 10;
                                if (recData.Count % telLength != 0)
                                {
                                    _elm327DataMode = false;
                                    Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN data length invalid: {0}", recData.Count);
                                    return string.Empty;
                                }

                                int pos = 0;
                                for (; ; )
                                {
                                    if (pos >= recData.Count)
                                    {
                                        break;
                                    }

                                    if (recData[pos] != 0x06)
                                    {   // invalid CAN high byte
                                        _elm327DataMode = false;
                                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN data high byte invalid: {0:X02}", recData[pos]);
                                        return string.Empty;
                                    }

                                    if (pos + telLength > recData.Count)
                                    {
                                        break;
                                    }

                                    StringBuilder stringBuilder = new StringBuilder();
                                    int source = (recData[pos + 0] << 8) + recData[pos + 1];
                                    stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X03}", source));

                                    for (int i = 2; i < telLength; i++)
                                    {
                                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02}", recData[pos + i]));
                                    }

                                    recList.Add(stringBuilder.ToString());

                                    pos += telLength;
                                }

                                if (recList.Count == 0)
                                {
                                    _elm327DataMode = false;
                                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM CAN receive list empty");
                                    return string.Empty;
                                }

                                string answer = recList[0];
                                recList.RemoveAt(0);
                                if (recList.Count == 0)
                                {
                                    _elm327DataMode = false;
                                }
                                else
                                {
                                    lock (_elm327BufferLock)
                                    {
                                        _elm327FullRespList = recList;
                                    }
                                }

                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ELM CAN rec: {0}", answer);
                                return answer;
                            }
                        }
                    }

                    if (_cancelEvent != null)
                    {
                        if (_cancelEvent.WaitOne(0))
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM transmit cancelled");
                            return string.Empty;
                        }
                    }

                    if ((Stopwatch.GetTimestamp() - startTime) > recTimeout * TickResolMs)
                    {
                        _elm327DataMode = false;
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM rec timeout");
                        return string.Empty;
                    }

                    if (AbortTransmission())
                    {
                        return string.Empty;
                    }

                    if (elmThread)
                    {
                        _elm327RequEvent.WaitOne(10);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            finally
            {
                _elm327ReceiverBusy = false;
            }
        }

        private void FlushReceiveBuffer()
        {
            _inStream.Flush();
            while (DataAvailable())
            {
#if ANDROID
                int data = _inStream.ReadByte();
#else
                int data = _inStream.ReadByteAsync(_cancelEvent);
#endif
                if (data < 0)
                {
                    break;
                }
            }
        }

        private bool DataAvailable()
        {
#if ANDROID
            return _inStream.HasData();
#else
            if (!(_inStream is System.Net.Sockets.NetworkStream networkStream))
            {
                return false;
            }
            return networkStream.DataAvailable;
#endif
        }

        bool ReceiverBusy()
        {
            if (_elm327TransportType != TransportType.Standard)
            {
                if (_elm327ReceiverBusy)
                {
                    return true;
                }

                lock (_elm327BufferLock)
                {
                    if (_elm327RequQueue.Count > 0)
                    {
                        return true;
                    }

                    if (_elm327FullRespList != null && _elm327FullRespList.Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        bool AbortTransmission()
        {
            if (_elm327TerminateThread)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM thread terminate request");
                return true;
            }

            if (_cancelEvent != null)
            {
                if (_cancelEvent.WaitOne(0))
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ELM transmit cancel request");
                    return true;
                }
            }

            return false;
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

        public static char ConvertToChar(int data)
        {
            try
            {
                return Convert.ToChar(data, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return '.';
            }
        }

        public static string TrimElmResponse(string elmResponse)
        {
            if (string.IsNullOrWhiteSpace(elmResponse))
            {
                return string.Empty;
            }

            return elmResponse.Trim('\r', '\n', '>', ' ');
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
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    InterfaceDisconnect();
                    if (_elm327RequEvent != null)
                    {
                        _elm327RequEvent.Dispose();
                        _elm327RequEvent = null;
                    }

                    if (_elm327RespEvent != null)
                    {
                        _elm327RespEvent.Dispose();
                        _elm327RespEvent = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
