using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdBluetoothInterface : EdBluetoothInterfaceBase
    {
        public const string PortId = "BLUETOOTH";
        private static readonly long TickResolMs = Stopwatch.Frequency/1000;
        private const int ReadTimeoutOffsetLong = 1000;
        private const int ReadTimeoutOffsetShort = 100;
        protected const int EchoTimeout = 500;
        protected static System.IO.Ports.SerialPort SerialPort;
        protected static NetworkStream BtStream;
        protected static AutoResetEvent CommReceiveEvent;
        protected static Stopwatch StopWatch = new Stopwatch();
        private static bool _reconnectRequired;
        private static string _connectPort;

        static EdBluetoothInterface()
        {
            SerialPort = new System.IO.Ports.SerialPort();
            SerialPort.DataReceived += SerialDataReceived;
            CommReceiveEvent = new AutoResetEvent(false);
        }

        public static bool InterfaceConnect(string port, object parameter)
        {
            if (IsInterfaceOpen())
            {
                return true;
            }
            FastInit = false;
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;
            AdapterType = -1;
            AdapterVersion = -1;
            LastCommTick = DateTime.MinValue.Ticks;

            if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
            {
                InterfaceDisconnect();
                return false;
            }
            _connectPort = port;
            _reconnectRequired = false;
            try
            {
                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {
                    // special id
                    string portName = portData.Remove(0, 1);
                    string[] stringList = portName.Split('#', ';');
                    if (stringList.Length == 1)
                    {
                        SerialPort.PortName = portName;
                        SerialPort.BaudRate = 115200;
                        SerialPort.DataBits = 8;
                        SerialPort.Parity = System.IO.Ports.Parity.None;
                        SerialPort.StopBits = System.IO.Ports.StopBits.One;
                        SerialPort.Handshake = System.IO.Ports.Handshake.None;
                        SerialPort.DtrEnable = false;
                        SerialPort.RtsEnable = false;
                        SerialPort.ReadTimeout = 1;
                        SerialPort.Open();
                    }
#if BLUETOOTH
                    else if (stringList.Length == 2)
                    {
                        InTheHand.Net.BluetoothEndPoint ep =
                            new InTheHand.Net.BluetoothEndPoint(InTheHand.Net.BluetoothAddress.Parse(stringList[0]), InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
                        InTheHand.Net.Sockets.BluetoothClient cli = new InTheHand.Net.Sockets.BluetoothClient();
                        cli.SetPin(stringList[1]);
                        cli.Connect(ep);
                        BtStream = cli.GetStream();
                        BtStream.ReadTimeout = 1;
                    }
#endif
                    else
                    {
                        InterfaceDisconnect();
                        return false;
                    }
                }
                else
                {
                    InterfaceDisconnect();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Connect failure: {0}", ex.Message);
                InterfaceDisconnect();
                return false;
            }
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            bool result = true;
            try
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                }
            }
            catch (Exception)
            {
                result = false;
            }
            try
            {
                if (BtStream != null)
                {
                    BtStream.Close();
                    BtStream.Dispose();
                    BtStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (!IsInterfaceOpen())
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }
            CurrentProtocol = protocol;
            CurrentBaudRate = baudRate;
            CurrentWordLength = dataBits;
            CurrentParity = parity;
            FastInit = false;
            ConvertBaudResponse = false;
            return EdInterfaceObd.InterfaceErrorResult.NoError;
        }

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (!IsInterfaceOpen())
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetRts(bool rts)
        {
            if (!IsInterfaceOpen())
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = true;
            if (!IsInterfaceOpen())
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            return false;
        }

        public static bool InterfaceSetInterByteTime(int time)
        {
            InterByteTime = time;
            return true;
        }

        public static bool InterfaceSetCanIds(int canTxId, int canRxId)
        {
            CanTxId = canTxId;
            CanRxId = canRxId;
            return true;
        }

        public static bool InterfacePurgeInBuffer()
        {
            if (!IsInterfaceOpen())
            {
                return false;
            }
            try
            {
                if (BtStream != null)
                {
                    BtStream.ReadTimeout = 1;
                    while (BtStream.DataAvailable)
                    {
                        try
                        {
                            BtStream.ReadByte();
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    SerialPort.DiscardInBuffer();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceAdapterEcho()
        {
            return false;
        }

        public static bool InterfaceHasPreciseTimeout()
        {
            return false;
        }

        public static bool InterfaceHasAutoBaudRate()
        {
            return true;
        }

        public static bool InterfaceHasAutoKwp1281()
        {
            if (!UpdateAdapterInfo())
            {
                return false;
            }
            if (AdapterVersion < 0x0008)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;
            if (!IsInterfaceOpen())
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Port closed");
                return false;
            }
            if (_reconnectRequired)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect();
                if (!InterfaceConnect(_connectPort, null))
                {
                    _reconnectRequired = true;
                    return false;
                }
                _reconnectRequired = false;
            }
            try
            {
                if ((CurrentProtocol == EdInterfaceObd.Protocol.Tp20) ||
                    (CurrentProtocol == EdInterfaceObd.Protocol.IsoTp))
                {
                    UpdateAdapterInfo();
                    byte[] adapterTel = CreateCanTelegram(sendData, length);
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    if (BtStream != null)
                    {
                        BtStream.Write(adapterTel, 0, adapterTel.Length);
                    }
                    else
                    {
                        SerialPort.Write(adapterTel, 0, adapterTel.Length);
                    }
                    LastCommTick = Stopwatch.GetTimestamp();
                    UpdateActiveSettings();
                    return true;
                }
                if (CurrentBaudRate == 115200)
                {
                    // BMW-FAST
                    if (BtStream != null)
                    {
                        BtStream.Write(sendData, 0, length);
                    }
                    else
                    {
                        SerialPort.Write(sendData, 0, length);
                    }
                    LastCommTick = Stopwatch.GetTimestamp();
                    // remove echo
                    byte[] receiveData = new byte[length];
                    if (!InterfaceReceiveData(receiveData, 0, length, EchoTimeout, EchoTimeout, null))
                    {
                        return false;
                    }
                    for (int i = 0; i < length; i++)
                    {
                        if (receiveData[i] != sendData[i])
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    UpdateAdapterInfo();
                    byte[] adapterTel = CreateAdapterTelegram(sendData, length, setDtr);
                    FastInit = false;
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    if (BtStream != null)
                    {
                        BtStream.Write(adapterTel, 0, adapterTel.Length);
                    }
                    else
                    {
                        SerialPort.Write(adapterTel, 0, adapterTel.Length);
                    }
                    LastCommTick = Stopwatch.GetTimestamp();
                    UpdateActiveSettings();
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", ex.Message);
                _reconnectRequired = true;
                return false;
            }
            return true;
        }

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout,
            int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            bool convertBaudResponse = ConvertBaudResponse;
            bool autoKeyByteResponse = AutoKeyByteResponse;
            ConvertBaudResponse = false;
            AutoKeyByteResponse = false;

            if (!IsInterfaceOpen())
            {
                return false;
            }
            int timeoutOffset = ReadTimeoutOffsetLong;
            if (((Stopwatch.GetTimestamp() - LastCommTick) < 100*TickResolMs) && (timeout < 100))
            {
                timeoutOffset = ReadTimeoutOffsetShort;
            }
            //Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Timeout offset {0}", timeoutOffset);
            timeout += timeoutOffset;
            timeoutTelEnd += timeoutOffset;
            try
            {
                if (SettingsUpdateRequired())
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "InterfaceReceiveData, update settings");
                    UpdateAdapterInfo();
                    byte[] adapterTel = CreatePulseTelegram(0, 0, 0, false, false, 0);
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    if (BtStream != null)
                    {
                        BtStream.Write(adapterTel, 0, adapterTel.Length);
                    }
                    else
                    {
                        SerialPort.Write(adapterTel, 0, adapterTel.Length);
                    }
                    LastCommTick = Stopwatch.GetTimestamp();
                    UpdateActiveSettings();
                }

                if (convertBaudResponse && length == 2)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Convert baud response");
                    length = 1;
                    AutoKeyByteResponse = true;
                }

                int recLen = 0;
                if (BtStream != null)
                {
                    BtStream.ReadTimeout = timeout;
                    int data;
                    try
                    {
                        data = BtStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        return false;
                    }
                    receiveData[offset + recLen] = (byte)data;
                    recLen++;

                    BtStream.ReadTimeout = timeoutTelEnd;
                    for (;;)
                    {
                        if (recLen >= length)
                        {
                            break;
                        }
                        try
                        {
                            data = BtStream.ReadByte();
                        }
                        catch (Exception)
                        {
                            data = -1;
                        }
                        if (data < 0)
                        {
                            return false;
                        }
                        receiveData[offset + recLen] = (byte)data;
                        recLen++;
                    }
                }
                else
                {
                    // wait for first byte
                    int lastBytesToRead;
                    StopWatch.Reset();
                    StopWatch.Start();
                    for (;;)
                    {
                        lastBytesToRead = SerialPort.BytesToRead;
                        if (lastBytesToRead > 0)
                        {
                            break;
                        }
                        if (StopWatch.ElapsedMilliseconds > timeout)
                        {
                            StopWatch.Stop();
                            return false;
                        }
                        CommReceiveEvent.WaitOne(1, false);
                    }

                    StopWatch.Reset();
                    StopWatch.Start();
                    for (;;)
                    {
                        int bytesToRead = SerialPort.BytesToRead;
                        if (bytesToRead >= length)
                        {
                            int bytesRead = SerialPort.Read(receiveData, offset + recLen, length - recLen);
                            if (bytesRead > 0)
                            {
                                LastCommTick = Stopwatch.GetTimestamp();
                            }
                            recLen += bytesRead;
                        }
                        if (recLen >= length)
                        {
                            break;
                        }
                        if (lastBytesToRead != bytesToRead)
                        {   // bytes received
                            StopWatch.Reset();
                            StopWatch.Start();
                            lastBytesToRead = bytesToRead;
                        }
                        else
                        {
                            if (StopWatch.ElapsedMilliseconds > timeoutTelEnd)
                            {
                                break;
                            }
                        }
                        CommReceiveEvent.WaitOne(1, false);
                    }
                    StopWatch.Stop();
                }
                ediabasLog?.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, offset, recLen, "Rec ");
                if (recLen < length)
                {
                    return false;
                }
                if (convertBaudResponse)
                {
                    ConvertStdBaudResponse(receiveData, offset);
                }
                if (autoKeyByteResponse && length == 2)
                {   // auto key byte response for old adapter
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Auto key byte response");
                    byte[] keyByteResponse = { (byte) ~receiveData[offset + 1] };
                    byte[] adapterTel = CreateAdapterTelegram(keyByteResponse, keyByteResponse.Length, true);
                    if (adapterTel == null)
                    {
                        return false;
                    }
                    if (BtStream != null)
                    {
                        BtStream.Write(adapterTel, 0, adapterTel.Length);
                    }
                    else
                    {
                        SerialPort.Write(adapterTel, 0, adapterTel.Length);
                    }
                    LastCommTick = Stopwatch.GetTimestamp();
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", ex.Message);
                _reconnectRequired = true;
                return false;
            }
            return true;
        }

        public static bool InterfaceSendPulse(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            ConvertBaudResponse = false;
            if (!IsInterfaceOpen())
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Port closed");
                return false;
            }
            if (_reconnectRequired)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect();
                if (!InterfaceConnect(_connectPort, null))
                {
                    _reconnectRequired = true;
                    return false;
                }
                _reconnectRequired = false;
            }
            try
            {
                UpdateAdapterInfo();
                FastInit = IsFastInit(dataBits, length, pulseWidth);
                if (FastInit)
                {
                    // send next telegram with fast init
                    return true;
                }
                byte[] adapterTel = CreatePulseTelegram(dataBits, length, pulseWidth, setDtr, bothLines, autoKeyByteDelay);
                if (adapterTel == null)
                {
                    return false;
                }
                if (BtStream != null)
                {
                    BtStream.Write(adapterTel, 0, adapterTel.Length);
                }
                else
                {
                    SerialPort.Write(adapterTel, 0, adapterTel.Length);
                }
                LastCommTick = Stopwatch.GetTimestamp();
                UpdateActiveSettings();
                Thread.Sleep(pulseWidth * length);
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", ex.Message);
                _reconnectRequired = true;
                return false;
            }
            return true;
        }

        private static bool IsInterfaceOpen()
        {
            return SerialPort.IsOpen || (BtStream != null);
        }

        private static void SerialDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            CommReceiveEvent.Set();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool UpdateAdapterInfo(bool forceUpdate = false)
        {
            if (!IsInterfaceOpen())
            {
                return false;
            }
            if (!forceUpdate && AdapterType >= 0)
            {
                // only read once
                return true;
            }
            AdapterType = -1;
            try
            {
                const int versionRespLen = 9;
                byte[] identTel = {0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x5E};
                if (BtStream != null)
                {
                    BtStream.ReadTimeout = 1;
                    while (BtStream.DataAvailable)
                    {
                        try
                        {
                            BtStream.ReadByte();
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    BtStream.Write(identTel, 0, identTel.Length);
                }
                else
                {
                    SerialPort.DiscardInBuffer();
                    SerialPort.Write(identTel, 0, identTel.Length);
                }
                LastCommTick = Stopwatch.GetTimestamp();

                List<byte> responseList = new List<byte>();
                long startTime = Stopwatch.GetTimestamp();
                for (;;)
                {
                    if (BtStream != null)
                    {
                        BtStream.ReadTimeout = 1;
                        while (BtStream.DataAvailable)
                        {
                            int data;
                            try
                            {
                                data = BtStream.ReadByte();
                            }
                            catch (Exception)
                            {
                                data = -1;
                            }
                            if (data >= 0)
                            {
                                LastCommTick = Stopwatch.GetTimestamp();
                                responseList.Add((byte)data);
                                startTime = Stopwatch.GetTimestamp();
                            }
                        }
                    }
                    else
                    {
                        while (SerialPort.BytesToRead > 0)
                        {
                            int data = SerialPort.ReadByte();
                            if (data >= 0)
                            {
                                LastCommTick = Stopwatch.GetTimestamp();
                                responseList.Add((byte)data);
                                startTime = Stopwatch.GetTimestamp();
                            }
                        }
                    }
                    if (responseList.Count >= identTel.Length + versionRespLen)
                    {
                        bool validEcho = !identTel.Where((t, i) => responseList[i] != t).Any();
                        if (!validEcho)
                        {
                            return false;
                        }
                        if (CalcChecksumBmwFast(responseList.ToArray(), identTel.Length, versionRespLen - 1) !=
                            responseList[identTel.Length + versionRespLen - 1])
                        {
                            return false;
                        }
                        AdapterType = responseList[identTel.Length + 5] + (responseList[identTel.Length + 4] << 8);
                        AdapterVersion = responseList[identTel.Length + 7] + (responseList[identTel.Length + 6] << 8);
                        break;
                    }
                    if (Stopwatch.GetTimestamp() - startTime > ReadTimeoutOffsetLong*TickResolMs)
                    {
                        if (responseList.Count >= identTel.Length)
                        {
                            bool validEcho = !identTel.Where((t, i) => responseList[i] != t).Any();
                            if (validEcho)
                            {
                                AdapterType = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", ex.Message);
                _reconnectRequired = true;
                return false;
            }

            return true;
        }
    }
}
