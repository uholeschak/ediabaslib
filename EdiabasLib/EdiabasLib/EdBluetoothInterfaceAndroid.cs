using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Android.Bluetooth;
using Java.Util;

namespace EdiabasLib
{
    public class EdBluetoothInterface
    {
        public class ConnectParameterType
        {
            public delegate Android.Content.Context GetContextDelegate();

            public ConnectParameterType(Android.Net.ConnectivityManager connectivityManager, bool mtcBtService, GetContextDelegate getContextHandler)
            {
                GetContextHandler = getContextHandler;
                ConnectivityManager = connectivityManager;
                MtcBtService = mtcBtService;
            }

            public GetContextDelegate GetContextHandler { get; }

            public Android.Net.ConnectivityManager ConnectivityManager { get; }

            public bool MtcBtService { get; }
        }

        public static readonly EdElmInterface.ElmInitEntry[] Elm327InitCommands = EdElmInterface.Elm327InitCommands;
        public const string PortId = "BLUETOOTH";
        public const string Elm327Tag = "ELM327";
        public const string RawTag = "RAW";
        private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private const int ReadTimeoutOffsetLong = 1000;
        private const int ReadTimeoutOffsetShort = 100;
        protected const int EchoTimeout = 500;
        private static readonly EdCustomAdapterCommon CustomAdapter =
            new EdCustomAdapterCommon(SendData, ReceiveData, DiscardInBuffer, ReadInBuffer, ReadTimeoutOffsetLong, ReadTimeoutOffsetShort, EchoTimeout, false);
        private static BluetoothSocket _bluetoothSocket;
        private static Stream _bluetoothInStream;
        private static Stream _bluetoothOutStream;
        private static bool _elm327Device;
        private static string _connectPort;
        private static ConnectParameterType _connectParameter;
        private static EdElmInterface _edElmInterface;
        private static readonly AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
        private static string _connectDeviceAddress = string.Empty;
        private static bool _deviceConnected;

        public static EdiabasNet Ediabas
        {
            get => CustomAdapter.Ediabas;
            set => CustomAdapter.Ediabas = value;
        }

        static EdBluetoothInterface()
        {
        }

        public static BluetoothSocket BluetoothSocket => _bluetoothSocket;

        public static bool InterfaceConnect(string port, object parameter)
        {
            if (_bluetoothSocket != null)
            {
                return true;
            }
            CustomAdapter.Init();

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
            _connectPort = port;
            _connectParameter = parameter as ConnectParameterType;
            bool mtcBtService = _connectParameter != null && _connectParameter.MtcBtService;
            try
            {
                BluetoothDevice device;
                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    string addr = portData.Remove(0, 1);
                    string[] stringList = addr.Split('#', ';');
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
                        else if (string.Compare(stringList[1], RawTag, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            CustomAdapter.RawMode = true;
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

                bool usedRfCommSocket = false;
                Receiver receiver = null;
                Android.Content.Context context = null;
                try
                {
                    if (_connectParameter?.GetContextHandler != null)
                    {
                        context = _connectParameter.GetContextHandler();
                        if (context != null)
                        {
                            receiver = new Receiver();
                            Android.Content.IntentFilter filter = new Android.Content.IntentFilter();
                            filter.AddAction(BluetoothDevice.ActionAclConnected);
                            filter.AddAction(BluetoothDevice.ActionAclDisconnected);
                            context.RegisterReceiver(receiver, filter);
                        }
                    }
                    _connectDeviceAddress = device.Address;

                    _bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);
                    try
                    {
                        _bluetoothSocket.Connect();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            // sometimes the second connect is working
                            _bluetoothSocket.Connect();
                        }
                        catch (Exception)
                        {
                            _bluetoothSocket.Close();
                            _bluetoothSocket = null;
                        }
                    }

                    if (_bluetoothSocket == null)
                    {
                        // this socket sometimes looses data for long telegrams
                        IntPtr createRfcommSocket = Android.Runtime.JNIEnv.GetMethodID(device.Class.Handle,
                            "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
                        if (createRfcommSocket == IntPtr.Zero)
                        {
                            throw new Exception("No createRfcommSocket");
                        }
                        IntPtr rfCommSocket = Android.Runtime.JNIEnv.CallObjectMethod(device.Handle,
                            createRfcommSocket, new Android.Runtime.JValue(1));
                        if (rfCommSocket == IntPtr.Zero)
                        {
                            throw new Exception("No rfCommSocket");
                        }
                        _bluetoothSocket = Java.Lang.Object.GetObject<BluetoothSocket>(rfCommSocket, Android.Runtime.JniHandleOwnership.TransferLocalRef);
                        _bluetoothSocket.Connect();
                        usedRfCommSocket = true;
                    }

                    int connectTimeout = mtcBtService ? 1000 : 2000;
                    ConnectedEvent.WaitOne(connectTimeout, false);
                    CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device connected: {0}", _deviceConnected);
                }
                finally
                {
                    if (receiver != null)
                    {
                        context.UnregisterReceiver(receiver);
                    }
                }

                _bluetoothInStream = _bluetoothSocket.InputStream;
                _bluetoothOutStream = _bluetoothSocket.OutputStream;

                if (_elm327Device)
                {
                    _edElmInterface = new EdElmInterface(CustomAdapter.Ediabas, _bluetoothInStream, _bluetoothOutStream);
                    if (mtcBtService && !usedRfCommSocket)
                    {
                        bool connected = false;
                        for (int retry = 0; retry < 20; retry++)
                        {
                            CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Test connection");
                            if (_edElmInterface.Elm327Init())
                            {
                                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Connected");
                                connected = true;
                                break;
                            }
                            _edElmInterface.Dispose();
                            _bluetoothSocket.Close();
                            _bluetoothSocket.Connect();
                            _bluetoothInStream = _bluetoothSocket.InputStream;
                            _bluetoothOutStream = _bluetoothSocket.OutputStream;
                            _edElmInterface = new EdElmInterface(CustomAdapter.Ediabas, _bluetoothInStream, _bluetoothOutStream);
                        }
                        if (!connected)
                        {
                            CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No response from adapter");
                            InterfaceDisconnect();
                            return false;
                        }
                    }
                    else
                    {
                        if (!_edElmInterface.Elm327Init())
                        {
                            InterfaceDisconnect();
                            return false;
                        }
                    }
                }
                else
                {   // not ELM327
                    if (!CustomAdapter.RawMode && mtcBtService && !usedRfCommSocket)
                    {
                        bool connected = false;
                        for (int retry = 0; retry < 20; retry++)
                        {
                            CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Test connection");
                            if (retry > 0)
                            {
                                _bluetoothSocket.Close();
                                _bluetoothSocket.Connect();
                                _bluetoothInStream = _bluetoothSocket.InputStream;
                                _bluetoothOutStream = _bluetoothSocket.OutputStream;
                            }
                            if (CustomAdapter.UpdateAdapterInfo(true))
                            {
                                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Connected");
                                connected = true;
                                break;
                            }
                        }
                        CustomAdapter.ReconnectRequired = false;     // is set by UpdateAdapterInfo()
                        if (!connected)
                        {
                            CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No response from adapter");
                            InterfaceDisconnect();
                            return false;
                        }
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
            if (_edElmInterface != null)
            {
                _edElmInterface.Dispose();
                _edElmInterface = null;
            }
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

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (_bluetoothSocket == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }

            return CustomAdapter.InterfaceSetConfig(protocol, baudRate, dataBits, parity, allowBitBang);
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
            if ((_bluetoothSocket == null) || (_bluetoothInStream == null))
            {
                return false;
            }
            if (_elm327Device)
            {
                if (_edElmInterface == null)
                {
                    return false;
                }
                return _edElmInterface.InterfacePurgeInBuffer();
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
            if ((_bluetoothSocket == null) || (_bluetoothOutStream == null))
            {
                return false;
            }
            if (_elm327Device)
            {
                if ((CustomAdapter.CurrentProtocol != EdInterfaceObd.Protocol.Uart) ||
                    (CustomAdapter.CurrentBaudRate != 115200) || (CustomAdapter.CurrentWordLength != 8) || (CustomAdapter.CurrentParity != EdInterfaceObd.SerialParity.None))
                {
                    return false;
                }
                if (_edElmInterface == null)
                {
                    return false;
                }
                if (_edElmInterface.StreamFailure)
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                    InterfaceDisconnect();
                    if (!InterfaceConnect(_connectPort, null))
                    {
                        _edElmInterface.StreamFailure = true;
                        return false;
                    }
                }
                return _edElmInterface.InterfaceSendData(sendData, length, setDtr, dtrTimeCorr);
            }

            for (int retry = 0; retry < 2; retry++)
            {
                if (CustomAdapter.ReconnectRequired)
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                    InterfaceDisconnect();
                    if (!InterfaceConnect(_connectPort, null))
                    {
                        CustomAdapter.ReconnectRequired = true;
                        return false;
                    }

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
            if ((_bluetoothSocket == null) || (_bluetoothInStream == null))
            {
                return false;
            }
            if (_elm327Device)
            {
                if (_edElmInterface == null)
                {
                    return false;
                }
                return _edElmInterface.InterfaceReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, ediabasLog);
            }

            return CustomAdapter.InterfaceReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, ediabasLog);
        }

        public static bool InterfaceSendPulse(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            if ((_bluetoothSocket == null) || (_bluetoothOutStream == null))
            {
                return false;
            }
            if (_elm327Device)
            {
                return false;
            }
            if (CustomAdapter.ReconnectRequired)
            {
                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect();
                if (!InterfaceConnect(_connectPort, null))
                {
                    CustomAdapter.ReconnectRequired = true;
                    return false;
                }
                CustomAdapter.ReconnectRequired = false;
            }
            return CustomAdapter.InterfaceSendPulse(dataBits, length, pulseWidth, setDtr, bothLines, autoKeyByteDelay);
        }

        private static void SendData(byte[] buffer, int length)
        {
            _bluetoothOutStream.Write(buffer, 0, length);
        }

        private static bool ReceiveData(byte[] buffer, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog = null)
        {
            int recLen = 0;
            long startTime = Stopwatch.GetTimestamp();
            while (recLen < length)
            {
                int currTimeout = (recLen == 0) ? timeout : timeoutTelEnd;
                if (_bluetoothInStream.IsDataAvailable())
                {
                    int bytesRead = _bluetoothInStream.Read(buffer, offset + recLen, length - recLen);
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
                if ((Stopwatch.GetTimestamp() - startTime) > currTimeout * EdCustomAdapterCommon.TickResolMs)
                {
                    ediabasLog?.LogData(EdiabasNet.EdLogLevel.Ifh, buffer, offset, recLen, "Rec ");
                    return false;
                }
                Thread.Sleep(10);
            }
            return true;
        }

        private static void DiscardInBuffer()
        {
            while (_bluetoothInStream.IsDataAvailable())
            {
                _bluetoothInStream.ReadByte();
            }
        }

        private static List<byte> ReadInBuffer()
        {
            List<byte> responseList = new List<byte>();
            _bluetoothInStream.Flush();
            while (_bluetoothInStream.IsDataAvailable())
            {
                int data = _bluetoothInStream.ReadByte();
                if (data >= 0)
                {
                    CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
                    responseList.Add((byte)data);
                }
            }
            return responseList;
        }

        private class Receiver : Android.Content.BroadcastReceiver
        {
            public override void OnReceive(Android.Content.Context context, Android.Content.Intent intent)
            {
                try
                {
                    string action = intent.Action;

                    switch (action)
                    {
                        case BluetoothDevice.ActionAclConnected:
                        case BluetoothDevice.ActionAclDisconnected:
                            {
                                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                                if (device != null)
                                {
                                    if (!string.IsNullOrEmpty(_connectDeviceAddress) &&
                                            string.Compare(device.Address, _connectDeviceAddress, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        _deviceConnected = action == BluetoothDevice.ActionAclConnected;
                                        ConnectedEvent.Set();
                                    }
                                }
                                break;
                            }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
