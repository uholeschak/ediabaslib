#if DEBUG && ANDROID
#define DEBUG_ANDROID
#endif
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

            public ConnectParameterType(TcpClientWithTimeout.NetworkData networkData, bool mtcBtService, bool mtcBtEscapeMode, GetContextDelegate getContextHandler)
            {
                NetworkData = networkData;
                MtcBtService = mtcBtService;
                MtcBtEscapeMode = mtcBtEscapeMode;
                GetContextHandler = getContextHandler;
            }

            public TcpClientWithTimeout.NetworkData NetworkData { get; }

            public bool MtcBtService { get; }

            public bool MtcBtEscapeMode { get; }

            public GetContextDelegate GetContextHandler { get; }
        }

#if DEBUG_ANDROID
        private static readonly string Tag = typeof(EdBluetoothInterface).FullName;
#endif
        public static readonly EdElmInterface.ElmInitEntry[] Elm327InitCommands = EdElmInterface.Elm327InitCommands;
        public const string PortId = "BLUETOOTH";
        public const string Elm327Tag = "ELM327";
        public const string ElmDeepObdTag = "ELMDEEPOBD";
        public const string RawTag = "RAW";
        public const int BtConnectDelay = 50;
        private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private const int ReadTimeoutOffsetLong = 1000;
        private const int ReadTimeoutOffsetShort = 100;
        protected const int EchoTimeout = 500;
        protected const int ConnectRetries = 20;
        private static readonly EdCustomAdapterCommon CustomAdapter =
            new EdCustomAdapterCommon(SendData, ReceiveData, DiscardInBuffer, ReadInBuffer, ReadTimeoutOffsetLong, ReadTimeoutOffsetShort, EchoTimeout, false);
        private static BtLeGattSpp _btLeGattSpp;
        private static BluetoothSocket _bluetoothSocket;
        private static Stream _bluetoothInStream;
        private static Stream _bluetoothOutStream;
        private static bool _elm327Device;
        private static string _connectPort;
        private static ConnectParameterType _connectParameter;
        private static EdElmInterface _edElmInterface;
        private static readonly ManualResetEvent TransmitCancelEvent = new ManualResetEvent(false);
        private static readonly AutoResetEvent ConnectedEvent = new AutoResetEvent(false);
        private static string _connectDeviceAddress = string.Empty;

        public static Stream BluetoothInStream => _bluetoothInStream;
        public static Stream BluetoothOutStream => _bluetoothOutStream;

        public static EdiabasNet Ediabas
        {
            get => CustomAdapter.Ediabas;
            set => CustomAdapter.Ediabas = value;
        }

        static EdBluetoothInterface()
        {
        }

        public static bool InterfaceConnect(string port, object parameter)
        {
            return InterfaceConnect(port, parameter, false);
        }

        public static bool InterfaceConnect(string port, object parameter, bool reconnect)
        {
            if (_bluetoothInStream != null && _bluetoothOutStream != null)
            {
                return true;
            }

            InterfaceDisconnect();
            CustomAdapter.Init();

            if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Invalid port id: {0}", port);
                InterfaceDisconnect();
                return false;
            }

            TransmitCancelEvent.Reset();
            _elm327Device = false;
            _connectPort = port;
            _connectParameter = parameter as ConnectParameterType;

            bool mtcBtService = _connectParameter != null && _connectParameter.MtcBtService;
            bool mtcBtEscapeMode = _connectParameter != null && _connectParameter.MtcBtEscapeMode;
            try
            {
                Android.Content.Context context = null;
                if (_connectParameter?.GetContextHandler != null)
                {
                    context = _connectParameter.GetContextHandler();
                }

                BluetoothAdapter bluetoothAdapter;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr2)
                {
                    BluetoothManager bluetoothManager = context?.GetSystemService(Android.Content.Context.BluetoothService) as BluetoothManager;
                    bluetoothAdapter = bluetoothManager?.Adapter;
                }
                else
                {
#pragma warning disable 618
#pragma warning disable CA1422
                    bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
#pragma warning restore CA1422
#pragma warning restore 618
                }

                if (bluetoothAdapter == null)
                {
                    InterfaceDisconnect();
                    return false;
                }

                BluetoothDevice device;
                string portData = port.Remove(0, PortId.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    string addr = portData.Remove(0, 1);
                    string[] stringList = addr.Split('#', ';');
                    if (stringList.Length == 0)
                    {
                        CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Invalid port parameters: {0}", port);
                        InterfaceDisconnect();
                        return false;
                    }
                    device = bluetoothAdapter.GetRemoteDevice(stringList[0].ToUpperInvariant());
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], Elm327Tag, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(stringList[1], ElmDeepObdTag, StringComparison.OrdinalIgnoreCase) == 0)
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
                    CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Missing port parameters: {0}", port);
                    InterfaceDisconnect();
                    return false;
                }
                if (device == null)
                {
                    CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Invalid remote device: {0}", port);
                    InterfaceDisconnect();
                    return false;
                }
                bluetoothAdapter.CancelDiscovery();

                bool usedRfCommSocket = false;
                Receiver receiver = null;
                int connectTimeout = mtcBtService ? 1000 : 2000;
                try
                {
                    if (context != null)
                    {
                        receiver = new Receiver();
                        Android.Content.IntentFilter filter = new Android.Content.IntentFilter();
                        filter.AddAction(BluetoothDevice.ActionAclConnected);
                        filter.AddAction(BluetoothDevice.ActionAclDisconnected);
                        context.RegisterReceiver(receiver, filter);   // system broadcasts
                    }

                    _connectDeviceAddress = device.Address;

                    CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Raw mode: {0}, ELM device: {1}", CustomAdapter.RawMode, _elm327Device);
                    Bond bondState = Bond.None;
                    BluetoothDeviceType deviceType = BluetoothDeviceType.Unknown;
                    if (!mtcBtService)
                    {
                        try
                        {
                            bondState = device.BondState;
                            deviceType = device.Type;
                            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device bond state: {0}", bondState);
                            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device type: {0}", deviceType);
                        }
                        catch (Exception ex)
                        {
                            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device state exception: {0}", EdiabasNet.GetExceptionText(ex));
                        }
                    }

                    if (!mtcBtService && context != null)
                    {
                        if (deviceType == BluetoothDeviceType.Le || (deviceType == BluetoothDeviceType.Dual && bondState == Bond.None))
                        {
                            _btLeGattSpp ??= new BtLeGattSpp();

                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (!_btLeGattSpp.ConnectLeGattDevice(context, device, TransmitCancelEvent))
                            {
                                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Connect to LE GATT device failed");
                            }
                            else
                            {
                                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Connect to LE GATT device success");
                                _bluetoothInStream = _btLeGattSpp.BtGattSppInStream;
                                _bluetoothOutStream = _btLeGattSpp.BtGattSppOutStream;
                            }
                        }
                    }

                    if (_bluetoothInStream == null || _bluetoothOutStream == null)
                    {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (mtcBtService || bondState == Bond.Bonded)
                        {
                            _bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);
                        }
                        else
                        {
                            _bluetoothSocket = device.CreateInsecureRfcommSocketToServiceRecord(SppUuid);
                        }

                        if (!BluetoothConnect(_bluetoothSocket))
                        {
                            // sometimes the second connect is working
                            if (!BluetoothConnect(_bluetoothSocket))
                            {
#if DEBUG_ANDROID
                                Android.Util.Log.Info(Tag, "Connecting: Closing socket");
#endif
                                _bluetoothSocket.Close();
                                _bluetoothSocket = null;
                            }
                        }

                        if (_bluetoothSocket == null)
                        {
                            // this socket sometimes looses data for long telegrams
                            nint createRfcommSocket = Android.Runtime.JNIEnv.GetMethodID(device.Class.Handle,
                                "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
                            if (createRfcommSocket == nint.Zero)
                            {
                                throw new Exception("No createRfcommSocket");
                            }
                            nint rfCommSocket = Android.Runtime.JNIEnv.CallObjectMethod(device.Handle,
                                createRfcommSocket, new Android.Runtime.JValue(1));
                            if (rfCommSocket == nint.Zero)
                            {
                                throw new Exception("No rfCommSocket");
                            }
                            _bluetoothSocket = Java.Lang.Object.GetObject<BluetoothSocket>(rfCommSocket, Android.Runtime.JniHandleOwnership.TransferLocalRef);
                            if (!BluetoothConnect(_bluetoothSocket))
                            {
                                throw new Exception("Bt connect failed");
                            }

                            usedRfCommSocket = true;
                        }

                        WaitForConnectEvent(connectTimeout);
                        CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device connected: {0}", _bluetoothSocket.IsConnected);
                    }
                }
                finally
                {
                    if (receiver != null)
                    {
                        context.UnregisterReceiver(receiver);
                    }
                }

                if (_elm327Device)
                {
                    if (_bluetoothSocket != null)
                    {
                        _bluetoothInStream = _bluetoothSocket.InputStream;
                        _bluetoothOutStream = _bluetoothSocket.OutputStream;
                    }

                    _edElmInterface = new EdElmInterface(CustomAdapter.Ediabas, _bluetoothInStream, _bluetoothOutStream, TransmitCancelEvent);
                    if (mtcBtService && !reconnect && !usedRfCommSocket && _bluetoothSocket != null)
                    {
                        bool connected = false;
                        for (int retry = 0; retry < ConnectRetries; retry++)
                        {
                            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Test connection, Retry: {0}", retry);
                            if (_edElmInterface.Elm327Init())
                            {
                                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Connected");
                                connected = true;
                                break;
                            }

#if DEBUG_ANDROID
                            Android.Util.Log.Info(Tag, "Connecting retry: Closing socket");
#endif
                            InterfaceDisconnect();
                            _bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);

                            if (!BluetoothConnect(_bluetoothSocket))
                            {
                                throw new Exception("Bt connect failed");
                            }

                            WaitForConnectEvent(connectTimeout);
                            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device connected: {0}", _bluetoothSocket.IsConnected);

                            _bluetoothInStream = _bluetoothSocket.InputStream;
                            _bluetoothOutStream = _bluetoothSocket.OutputStream;
                            _edElmInterface = new EdElmInterface(CustomAdapter.Ediabas, _bluetoothInStream, _bluetoothOutStream, TransmitCancelEvent);
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
                    CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Escape mode: {0}", mtcBtEscapeMode);
                    CustomAdapter.EscapeModeRead = mtcBtEscapeMode;
                    CustomAdapter.EscapeModeWrite = mtcBtEscapeMode;
                    if (_bluetoothSocket != null)
                    {
                        _bluetoothInStream = new EscapeStreamReader(_bluetoothSocket.InputStream);
                        _bluetoothOutStream = new EscapeStreamWriter(_bluetoothSocket.OutputStream);
                    }

                    if (!CustomAdapter.RawMode && mtcBtService && !reconnect && !usedRfCommSocket && _bluetoothSocket != null)
                    {
                        bool connected = false;
                        for (int retry = 0; retry < ConnectRetries; retry++)
                        {
                            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Test connection, Retry: {0}", retry);
                            CustomAdapter.EscapeModeRead = mtcBtEscapeMode;
                            CustomAdapter.EscapeModeWrite = mtcBtEscapeMode;
                            if (retry > 0)
                            {
#if DEBUG_ANDROID
                                Android.Util.Log.Info(Tag, "Connecting retry: Closing socket");
#endif
                                InterfaceDisconnect();
                                _bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);

                                if (!BluetoothConnect(_bluetoothSocket))
                                {
                                    throw new Exception("Bt connect failed");
                                }

                                WaitForConnectEvent(connectTimeout);
                                CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Device connected: {0}", _bluetoothSocket.IsConnected);

                                _bluetoothInStream = new EscapeStreamReader(_bluetoothSocket.InputStream);
                                _bluetoothOutStream = new EscapeStreamWriter(_bluetoothSocket.OutputStream);
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
            catch (Exception ex)
            {
                CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "InterfaceConnect exception: {0}", EdiabasNet.GetExceptionText(ex));
                InterfaceDisconnect();
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
                    _bluetoothInStream.Dispose();
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
                    _bluetoothOutStream.Dispose();
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

            if (_btLeGattSpp != null)
            {
                _btLeGattSpp.Dispose();
                _btLeGattSpp = null;
            }

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
            if (_bluetoothInStream == null || _bluetoothOutStream == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }

            return CustomAdapter.InterfaceSetConfig(protocol, baudRate, dataBits, parity, allowBitBang);
        }

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static bool InterfaceSetDtr(bool dtr)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            if (_bluetoothInStream == null || _bluetoothOutStream == null)
            {
                return false;
            }
            return true;
        }

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static bool InterfaceSetRts(bool rts)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            if (_bluetoothInStream == null || _bluetoothOutStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = true;
            if (_bluetoothInStream == null || _bluetoothOutStream == null)
            {
                return false;
            }
            return true;
        }

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static bool InterfaceSetBreak(bool enable)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
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
            if (_bluetoothInStream == null || _bluetoothOutStream == null)
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
            if (_elm327Device)
            {
                return false;
            }
            return CustomAdapter.InterfaceHasAutoKwp1281();
        }

        public static int? InterfaceAdapterVersion()
        {
            if (_elm327Device)
            {
                return null;
            }
            return CustomAdapter.InterfaceAdapterVersion();
        }

        public static byte[] InterfaceAdapterSerial()
        {
            if (_elm327Device)
            {
                return null;
            }
            return CustomAdapter.InterfaceAdapterSerial();
        }

        public static double? InterfaceAdapterVoltage()
        {
            if (_elm327Device)
            {
                return null;
            }
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
                if (_bluetoothInStream == null || _bluetoothOutStream == null)
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Stream closed");
                    return false;
                }
            }

            if (_elm327Device)
            {
                if ((CustomAdapter.CurrentProtocol != EdInterfaceObd.Protocol.Uart) ||
                    (CustomAdapter.CurrentBaudRate != 115200) || (CustomAdapter.CurrentWordLength != 8) || (CustomAdapter.CurrentParity != EdInterfaceObd.SerialParity.None))
                {
                    return false;
                }

                if (_edElmInterface != null && _edElmInterface.StreamFailure)
                {
                    CustomAdapter.ReconnectRequired = true;
                }

                if (CustomAdapter.ReconnectRequired)
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                    InterfaceDisconnect();
                    if (!InterfaceConnect(_connectPort, _connectParameter, true))
                    {
                        CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        CustomAdapter.ReconnectRequired = true;
                        return false;
                    }

                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected");
                    CustomAdapter.ReconnectRequired = false;
                }

                if (_edElmInterface == null)
                {
                    return false;
                }
                return _edElmInterface.InterfaceSendData(sendData, length, setDtr, dtrTimeCorr);
            }

            for (int retry = 0; retry < 2; retry++)
            {
                if (CustomAdapter.ReconnectRequired)
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                    InterfaceDisconnect();
                    if (!InterfaceConnect(_connectPort, _connectParameter, true))
                    {
                        CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        CustomAdapter.ReconnectRequired = true;
                        return false;
                    }

                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected");
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
            if (_bluetoothInStream == null || _bluetoothOutStream == null)
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
            if (!CustomAdapter.ReconnectRequired)
            {
                if (_bluetoothInStream == null || _bluetoothOutStream == null)
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Stream closed");
                    return false;
                }
            }
            if (_elm327Device)
            {
                return false;
            }
            if (CustomAdapter.ReconnectRequired)
            {
                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect();
                if (!InterfaceConnect(_connectPort, _connectParameter, true))
                {
                    CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                    CustomAdapter.ReconnectRequired = true;
                    return false;
                }

                CustomAdapter.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected");
                CustomAdapter.ReconnectRequired = false;
            }
            return CustomAdapter.InterfaceSendPulse(dataBits, length, pulseWidth, setDtr, bothLines, autoKeyByteDelay);
        }

        private static void SendData(byte[] buffer, int length)
        {
            if (_bluetoothOutStream is EscapeStreamWriter outStream)
            {
                if (outStream.EscapeMode != CustomAdapter.EscapeModeWrite)
                {
                    outStream.SetEscapeMode(CustomAdapter.EscapeModeWrite);
                }
                outStream.Write(buffer, 0, length);
            }
            else
            {
                _bluetoothOutStream.Write(buffer, 0, length);
            }
        }

        private static bool ReceiveData(byte[] buffer, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog = null)
        {
            if (TransmitCancelEvent.WaitOne(0))
            {
                return false;
            }

            int recLen = 0;
            long startTime = Stopwatch.GetTimestamp();
            while (recLen < length)
            {
                bool dataReceived = false;
                int currTimeout = (recLen == 0) ? timeout : timeoutTelEnd;
                if (_bluetoothInStream.HasData())
                {
                    int bytesRead = _bluetoothInStream.ReadBytesAsync(buffer, offset + recLen, length - recLen, TransmitCancelEvent);
                    if (bytesRead < 0)
                    {
                        ediabasLog?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** ReceiveData Aborted");
                        break;
                    }

                    if (bytesRead > 0)
                    {
                        dataReceived = true;
                        CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
                        startTime = CustomAdapter.LastCommTick;
                    }
                    recLen += bytesRead;
                }

                if (recLen >= length)
                {
                    break;
                }

                if (!dataReceived && (Stopwatch.GetTimestamp() - startTime) > currTimeout * EdCustomAdapterCommon.TickResolMs)
                {
                    ediabasLog?.LogData(EdiabasNet.EdLogLevel.Ifh, buffer, offset, recLen, "Rec ");
                    ediabasLog?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** ReceiveData Length={0}, Expected={1}: Timeout", recLen, length);
                    return false;
                }

                if (TransmitCancelEvent.WaitOne(10))
                {
                    ediabasLog?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** ReceiveData Cancelled");
                    return false;
                }
            }
            return true;
        }

        private static void DiscardInBuffer()
        {
            while (_bluetoothInStream.HasData())
            {
                if (_bluetoothInStream.ReadByteAsync(TransmitCancelEvent) < 0)
                {
                    break;
                }
            }
        }

        private static List<byte> ReadInBuffer()
        {
            List<byte> responseList = new List<byte>();
            if (_bluetoothInStream is EscapeStreamReader inStream)
            {
                if (inStream.EscapeMode != CustomAdapter.EscapeModeRead)
                {
                    inStream.SetEscapeMode(CustomAdapter.EscapeModeRead);
                }
            }

            while (_bluetoothInStream.HasData())
            {
                int data = _bluetoothInStream.ReadByteAsync(TransmitCancelEvent);
                if (data >= 0)
                {
                    CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
                    responseList.Add((byte)data);
                }
                else
                {
                    break;
                }
            }
            return responseList;
        }

        private static bool BluetoothConnect(BluetoothSocket bluetoothSocket, int timeout = 0)
        {
            if (bluetoothSocket == null)
            {
                return false;
            }

            if (TransmitCancelEvent.WaitOne(0))
            {
                return false;
            }

            bool connectOk = false;
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    if (!bluetoothSocket.IsConnected)
                    {
                        bluetoothSocket.Connect();
                    }

                    connectOk = true;
                }
#pragma warning disable CS0168
                catch (Exception ex)
#pragma warning restore CS0168
                {
#if DEBUG_ANDROID
                    Android.Util.Log.Info(Tag, string.Format("BluetoothConnect Exception={0}", EdiabasNet.GetExceptionText(ex, true, true)));
#endif
                    connectOk = false;
                }
            })
            {
                Priority = ThreadPriority.Normal
            };
            connectThread.Start();

            long startTime = Stopwatch.GetTimestamp();
            bool abort = false;
            for (;;)
            {
                if (connectThread.Join(100))
                {
                    break;
                }

                if (timeout > 0)
                {
                    if ((Stopwatch.GetTimestamp() - startTime) > timeout * EdCustomAdapterCommon.TickResolMs)
                    {
                        abort = true;
#if DEBUG_ANDROID
                        Android.Util.Log.Info(Tag, "BluetoothConnect Timeout");
#endif
                        CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BluetoothConnect timeout aborting");
                        break;
                    }
                }

                if (TransmitCancelEvent.WaitOne(0))
                {
                    abort = true;
#if DEBUG_ANDROID
                    Android.Util.Log.Info(Tag, "BluetoothConnect Cancelled");
#endif
                    CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BluetoothConnect transmit cancel");
                    break;
                }
            }

            if (abort)
            {
                connectOk = false;
                try
                {
#if DEBUG_ANDROID
                    Android.Util.Log.Info(Tag, "BluetoothConnect Closing socket");
#endif
                    bluetoothSocket.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            connectThread.Join();

#if DEBUG_ANDROID
            Android.Util.Log.Info(Tag, string.Format("BluetoothConnect Ok={0}", connectOk));
#endif
            CustomAdapter.Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BluetoothConnect Ok={0}", connectOk);
            return connectOk;
        }

        private static bool WaitForConnectEvent(int connectTimeout)
        {
            if (TransmitCancelEvent.WaitOne(0))
            {
                throw new Exception("Canceled");
            }

            if (WaitHandle.WaitAny(new WaitHandle[] { ConnectedEvent, TransmitCancelEvent }, connectTimeout) == 0)
            {
                Thread.Sleep(BtConnectDelay);
            }
            else
            {
                if (TransmitCancelEvent.WaitOne(0))
                {
                    throw new Exception("Canceled");
                }
            }

            return true;
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
                                BluetoothDevice device = intent.GetParcelableExtraType<BluetoothDevice>(BluetoothDevice.ExtraDevice);
                                if (device != null)
                                {
                                    if (!string.IsNullOrEmpty(_connectDeviceAddress) &&
                                            string.Compare(device.Address, _connectDeviceAddress, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        bool connected = action == BluetoothDevice.ActionAclConnected;
                                        if (connected)
                                        {
                                            ConnectedEvent.Set();
                                        }
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

    public static class AndroidExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static T GetParcelableExtraType<T>(this Android.Content.Intent intent, string name)
        {
            object parcel;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                parcel = intent.GetParcelableExtra(name, Java.Lang.Class.FromType(typeof(T)));
            }
            else
            {
#pragma warning disable CS0618
#pragma warning disable CA1422
                parcel = intent.GetParcelableExtra(name);
#pragma warning restore CA1422
#pragma warning restore CS0618
            }

            return (T)Convert.ChangeType(parcel, typeof(T));
        }
    }
}
