using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdBluetoothInterface
    {
        public const string PortId = "BLUETOOTH";
        public const int BtConnectDelay = 50;
        public const int BtDisconnectTimeout = 10000;
        protected const int DisconnectWait = 2000;
        protected const int ReadTimeoutOffsetLong = 1000;
        protected const int ReadTimeoutOffsetShort = 100;
        protected const int EchoTimeout = 500;
        private static readonly EdCustomAdapterCommon CustomAdapter =
            new EdCustomAdapterCommon(SendData, ReceiveData, DiscardInBuffer, ReadInBuffer, ReadTimeoutOffsetLong, ReadTimeoutOffsetShort, EchoTimeout, false);
        protected static System.IO.Ports.SerialPort SerialPort;
#if BLUETOOTH
        protected static InTheHand.Net.Sockets.BluetoothClient BtClient;
#endif
        protected static NetworkStream BtStream;
        protected static ManualResetEvent TransmitCancelEvent;
        protected static AutoResetEvent CommReceiveEvent;
        protected static long LastDisconnectTime = DateTime.MinValue.Ticks;
        protected static Stopwatch StopWatch = new Stopwatch();
        private static string _connectPort;

        public static Stream BluetoothInStream => BtStream;
        public static Stream BluetoothOutStream => BtStream;

        public static EdiabasNet Ediabas
        {
            get => CustomAdapter.Ediabas;
            set => CustomAdapter.Ediabas = value;
        }

        static EdBluetoothInterface()
        {
            SerialPort = new System.IO.Ports.SerialPort();
            SerialPort.DataReceived += SerialDataReceived;
            TransmitCancelEvent = new ManualResetEvent(false);
            CommReceiveEvent = new AutoResetEvent(false);
        }

        public static bool InterfaceConnect(string port, object parameter)
        {
            if (IsInterfaceOpen())
            {
                if (_connectPort == port)
                {
                    return true;
                }
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Bluetooth port {0} different, disconnect", port);
                InterfaceDisconnect(true);
            }
            CustomAdapter.Init();

            if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
            {
                InterfaceDisconnect(true);
                return false;
            }

            TransmitCancelEvent.Reset();
            _connectPort = port;
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Bluetooth connect: {0}", port);
            long disconnectDiff = (Stopwatch.GetTimestamp() - LastDisconnectTime) / EdCustomAdapterCommon.TickResolMs;
            if (disconnectDiff < DisconnectWait)
            {
                long delay = DisconnectWait - disconnectDiff;
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Disconnect delay: {0}", delay);
                Thread.Sleep((int)delay);
            }
            long startTime = Stopwatch.GetTimestamp();
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
                        InTheHand.Net.BluetoothAddress btAddress = InTheHand.Net.BluetoothAddress.Parse(stringList[0]);
                        string pin = stringList[1];
                        InTheHand.Net.Sockets.BluetoothDeviceInfo device = null;

                        try
                        {
                            device = new InTheHand.Net.Sockets.BluetoothDeviceInfo(btAddress);
                        }
                        catch (Exception ex)
                        {
                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** BluetoothDeviceInfo constructor exception: {0}", EdiabasNet.GetExceptionText(ex));
                        }

                        if (device != null)
                        {
                            long startTimeDisconnect = Stopwatch.GetTimestamp();
                            for (; ; )
                            {
                                device.Refresh();
                                if (!device.Connected)
                                {
                                    break;
                                }

                                if ((Stopwatch.GetTimestamp() - startTimeDisconnect) / EdCustomAdapterCommon.TickResolMs > BtDisconnectTimeout)
                                {
                                    break;
                                }
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** BluetoothDeviceInfo is null");
                        }

                        if (device != null)
                        {
                            if (!device.Authenticated)
                            {
                                InTheHand.Net.Bluetooth.BluetoothSecurity.PairRequest(btAddress, pin);
                            }
                        }

                        InTheHand.Net.BluetoothEndPoint ep =
                            new InTheHand.Net.BluetoothEndPoint(btAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
                        BtClient = new InTheHand.Net.Sockets.BluetoothClient();

                        try
                        {
                            BtClient.Connect(ep);
                        }
                        catch (Exception)
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Connect failed, removing device");
                            InTheHand.Net.Bluetooth.BluetoothSecurity.RemoveDevice(btAddress);
                            Thread.Sleep(1000);

                            if (device != null)
                            {
                                device.Refresh();
                                if (!device.Authenticated)
                                {
                                    InTheHand.Net.Bluetooth.BluetoothSecurity.PairRequest(btAddress, pin);
                                }
                            }

                            BtClient.Connect(ep);
                        }
                        BtStream = BtClient.GetStream();
                        BtStream.ReadTimeout = 1;
                        Thread.Sleep(BtConnectDelay);
                    }
#endif
                    else
                    {
                        InterfaceDisconnect(true);
                        return false;
                    }
                }
                else
                {
                    InterfaceDisconnect(true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Connect failure: {0}", EdiabasNet.GetExceptionText(ex));
                InterfaceDisconnect(true);
                return false;
            }

            long timeDiff = (Stopwatch.GetTimestamp() - startTime) / EdCustomAdapterCommon.TickResolMs;
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connect time: {0}ms", timeDiff);
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            return InterfaceDisconnect(false);
        }

        public static bool InterfaceDisconnect(bool forceClose)
        {
            if (!forceClose && Ediabas != null && !Ediabas.Unloading)
            {
                int keepConnectionOpen = 0;
                string prop = Ediabas.GetConfigProperty("ObdKeepConnectionOpen");
                if (prop != null)
                {
                    keepConnectionOpen = (int)EdiabasNet.StringToValue(prop);
                }

                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ObdKeepConnectionOpen: {0}", keepConnectionOpen);
                if (keepConnectionOpen != 0)
                {
                    return true;
                }
            }

            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Bluetooth disconnect");
            bool result = true;
            try
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                    LastDisconnectTime = Stopwatch.GetTimestamp();
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
                    BtStream = null;
                    LastDisconnectTime = Stopwatch.GetTimestamp();
                }
            }
            catch (Exception)
            {
                BtStream = null;
                result = false;
            }
#if BLUETOOTH
            try
            {
                if (BtClient != null)
                {
                    BtClient.Close();
                    BtClient.Dispose();
                    BtClient = null;
                }
            }
            catch (Exception)
            {
                BtClient = null;
                result = false;
            }
#endif
            return result;
        }

        public static bool InterfaceTransmitCancel(bool cancel)
        {
            if (cancel)
            {
                TransmitCancelEvent.Set();
            }
            else
            {
                TransmitCancelEvent.Reset();
            }

            return true;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (!IsInterfaceOpen())
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }

            return CustomAdapter.InterfaceSetConfig(protocol, baudRate, dataBits, parity, allowBitBang);
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
            return CustomAdapter.InterfaceSetInterByteTime(time);
        }

        public static bool InterfaceSetCanIds(int canTxId, int canRxId, EdInterfaceObd.CanFlags canFlags)
        {
            return CustomAdapter.InterfaceSetCanIds(canTxId, canRxId, canFlags);
        }

        public static bool InterfacePurgeInBuffer()
        {
            if (CustomAdapter.ReconnectRequired)
            {
                return true;
            }
            if (!IsInterfaceOpen())
            {
                return false;
            }
            try
            {
                DiscardInBuffer();
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
            return CustomAdapter.InterfaceHasAutoKwp1281();
        }

        public static int? InterfaceAdapterVersion()
        {
            return CustomAdapter.InterfaceAdapterVersion();
        }

        public static byte[] InterfaceAdapterSerial()
        {
            return CustomAdapter.InterfaceAdapterSerial();
        }

        public static double? InterfaceAdapterVoltage()
        {
            return CustomAdapter.InterfaceAdapterVoltage();
        }

        public static bool InterfaceHasIgnitionStatus()
        {
            return true;
        }

        public static bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            if (!CustomAdapter.ReconnectRequired)
            {
                if (!IsInterfaceOpen())
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Port closed");
                    return false;
                }
            }

            for (int retry = 0; retry < 2; retry++)
            {
                if (CustomAdapter.ReconnectRequired)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(_connectPort, null))
                    {
                        Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        CustomAdapter.ReconnectRequired = true;
                        return false;
                    }

                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected");
                    CustomAdapter.ReconnectRequired = false;
                }

                if (CustomAdapter.InterfaceSendData(sendData, length, setDtr, dtrTimeCorr))
                {
                    return true;
                }

                if (!CustomAdapter.ReconnectRequired)
                {
                    return false;
                }
            }

            return false;
        }

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            if (!IsInterfaceOpen())
            {
                return false;
            }

            return CustomAdapter.InterfaceReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, ediabasLog);
        }

        public static bool InterfaceSendPulse(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            if (!CustomAdapter.ReconnectRequired)
            {
                if (!IsInterfaceOpen())
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Port closed");
                    return false;
                }
            }
            if (CustomAdapter.ReconnectRequired)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect(true);
                if (!InterfaceConnect(_connectPort, null))
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                    CustomAdapter.ReconnectRequired = true;
                    return false;
                }

                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected");
                CustomAdapter.ReconnectRequired = false;
            }

            return CustomAdapter.InterfaceSendPulse(dataBits, length, pulseWidth, setDtr, bothLines, autoKeyByteDelay);
        }

        private static bool IsInterfaceOpen()
        {
            return SerialPort.IsOpen || (BtStream != null);
        }

        private static void SerialDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            CommReceiveEvent.Set();
        }

        private static void SendData(byte[] buffer, int length)
        {
            if (BtStream != null)
            {
                BtStream.Write(buffer, 0, length);
            }
            else
            {
                SerialPort.Write(buffer, 0, length);
            }
        }

        private static bool ReceiveData(byte[] buffer, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog = null)
        {
            if (TransmitCancelEvent.WaitOne(0))
            {
                return false;
            }

            int recLen = 0;
            if (BtStream != null)
            {
                int data;
                try
                {
                    data = BtStream.ReadByteAsync(TransmitCancelEvent, timeout);
                }
                catch (Exception)
                {
                    data = -1;
                }
                if (data < 0)
                {
                    return false;
                }
                buffer[offset + recLen] = (byte)data;
                recLen++;

                for (; ; )
                {
                    if (recLen >= length)
                    {
                        break;
                    }
                    try
                    {
                        data = BtStream.ReadByteAsync(TransmitCancelEvent, timeoutTelEnd);
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        return false;
                    }
                    buffer[offset + recLen] = (byte)data;
                    recLen++;
                }
            }
            else
            {
                // wait for first byte
                int lastBytesToRead;
                StopWatch.Reset();
                StopWatch.Start();
                for (; ; )
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
                    CommReceiveEvent.WaitOne(1);
                }

                StopWatch.Reset();
                StopWatch.Start();
                for (; ; )
                {
                    int bytesToRead = SerialPort.BytesToRead;
                    if (bytesToRead >= length)
                    {
                        int bytesRead = SerialPort.Read(buffer, offset + recLen, length - recLen);
                        if (bytesRead > 0)
                        {
                            CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
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
                    CommReceiveEvent.WaitOne(1);
                }
                StopWatch.Stop();
            }
            if (recLen < length)
            {
                ediabasLog?.LogData(EdiabasNet.EdLogLevel.Ifh, buffer, offset, recLen, "Rec ");
                return false;
            }
            return true;
        }

        private static void DiscardInBuffer()
        {
            if (BtStream != null)
            {
                while (BtStream.DataAvailable)
                {
                    try
                    {
                        if (BtStream.ReadByteAsync(TransmitCancelEvent) < 0)
                        {
                            break;
                        }
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

        private static List<byte> ReadInBuffer()
        {
            List<byte> responseList = new List<byte>();
            if (BtStream != null)
            {
                while (BtStream.DataAvailable)
                {
                    int data;
                    try
                    {
                        data = BtStream.ReadByteAsync(TransmitCancelEvent, 100);
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data >= 0)
                    {
                        CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
                        responseList.Add((byte)data);
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
                        CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
                        responseList.Add((byte)data);
                    }
                }
            }
            return responseList;
        }
    }
}
