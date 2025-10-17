using EdiabasLib;
using InTheHand.Bluetooth;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static EdiabasLibConfigTool.FormMain;

namespace EdiabasLibConfigTool
{
    [SupportedOSPlatform("windows")]
    public class Test : IDisposable
    {
        public static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        public const int FtdiLatencyTimer = 1;
        private const string ElmIp = @"192.168.0.10";
        private const int ElmPort = 35000;
        private const string EspLinkIp = @"192.168.4.1";
        private const int EspLinkPort = 23;
        private readonly FormMain _form;
        private BluetoothClient _btClient;
        private BtLeGattSpp _btLeGattSpp;
        private Stream _dataStream;
        private Stream _dataStreamWrite;
        private HttpClient _httpClient;
        private SerialPort _serialPort;
        private volatile Thread _testThread;
        private bool _disposed;
        public bool TestOk { get; set; }
        public bool ConfigPossible { get; set; }
        public int AdapterType { get; private set; }
        public bool ThreadActive => _testThread != null;

        private enum AdapterMode
        {
            CanOff = 0x00,
            Can500 = 0x01,
            Can100 = 0x09,
            CanAuto = 0xFF,
        }

        public Test(FormMain form)
        {
            _form = form;
            _form.UpdateStatusText(string.Empty);

            try
            {
                _serialPort = new SerialPort();
            }
            catch (Exception)
            {
                // ignored
            }
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

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    DisconnectStream();

                    if (_httpClient != null)
                    {
                        _httpClient.Dispose();
                        _httpClient = null;
                    }

                    if (_serialPort != null)
                    {
                        _serialPort.Dispose();
                        _serialPort = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        public bool ExecuteTest(bool configure)
        {
            if (_testThread != null)
            {
                return false;
            }

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TestOk = false;
            ConfigPossible = false;
            AdapterType = -1;

            AccessPoint ap = _form.GetSelectedAp();
            if (ap != null)
            {
                AuthRequest authRequest = new AuthRequest(ap);
                if (authRequest.IsPasswordRequired)
                {
                    if (ap.Name.StartsWith(Patch.AdapterSsidEnetLink, StringComparison.OrdinalIgnoreCase))
                    {
                        authRequest.Password = Patch.PasswordWifiEnetLink;
                    }
                    else if (ap.Name.StartsWith(Patch.AdapterSsidModBmw, StringComparison.OrdinalIgnoreCase))
                    {
                        authRequest.Password = Patch.PasswordWifiModBmw;
                    }
                    else if (ap.Name.StartsWith(Patch.AdapterSsidUniCar, StringComparison.OrdinalIgnoreCase))
                    {
                        authRequest.Password = Patch.PasswordWifiUniCar;
                    }
                    else if (ap.Name.StartsWith(Patch.AdapterSsidMhd, StringComparison.OrdinalIgnoreCase))
                    {
                        authRequest.Password = Patch.PasswordWifiMhd;
                    }
                    else
                    {
                        authRequest.Password = _form.WifiPassword;
                    }
                }
                ap.ConnectAsync(authRequest, true, success =>
                {
                    _form.BeginInvoke((Action)(() =>
                    {
                        if (!success)
                        {
                            _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                        }
                        else
                        {
                            _form.PerformSearch();
                        }
                    }));
                });
                return true;
            }

            WlanInterface wlanIface = _form.GetSelectedWifiDevice();
            if (wlanIface != null)
            {
                try
                {
                    WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                    string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                    string ipAddr = string.Empty;
                    bool isEnet = string.Compare(ssidString, Patch.AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0;
                    bool isEnetLink = ssidString.StartsWith(Patch.AdapterSsidEnetLink, StringComparison.OrdinalIgnoreCase);
                    bool isModBmw = ssidString.StartsWith(Patch.AdapterSsidModBmw, StringComparison.OrdinalIgnoreCase);
                    bool isUniCar = ssidString.StartsWith(Patch.AdapterSsidUniCar, StringComparison.OrdinalIgnoreCase);
                    bool isMhd = ssidString.StartsWith(Patch.AdapterSsidMhd, StringComparison.OrdinalIgnoreCase);

                    IPInterfaceProperties ipProp = wlanIface.NetworkInterface.GetIPProperties();
                    if (ipProp == null)
                    {
                        _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                        return false;
                    }
                    ipAddr = (from addr in ipProp.DhcpServerAddresses where addr.AddressFamily == AddressFamily.InterNetwork select addr.ToString()).FirstOrDefault();
                    if (string.IsNullOrEmpty(ipAddr))
                    {
                        _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                        return false;
                    }

                    if (isEnet || isEnetLink || isModBmw || isUniCar || isMhd)
                    {
                        if (configure)
                        {
                            string rootPwd = isModBmw || isUniCar ? "admin" : "root";
                            string url = string.Format("http://{0}", ipAddr);

                            try
                            {
                                Process.Start(new ProcessStartInfo()
                                {
                                    FileName = url,
                                    UseShellExecute = true,
                                });
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            _form.UpdateStatusText(string.Format(Resources.Strings.WifiUrlOk, rootPwd));
                            TestOk = true;
                            ConfigPossible = true;
                            return true;
                        }
                    }
                    _testThread = new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.CurrentCulture = cultureInfo;
                            Thread.CurrentThread.CurrentUICulture = cultureInfo;
                            if (isEnet || isEnetLink || isModBmw || isUniCar || isMhd)
                            {
                                TestOk = RunWifiTestEnetRetry(ipAddr, out bool configRequired);
                                if (TestOk && configRequired)
                                {
                                    ConfigPossible = true;
                                }
                            }
                            else
                            {
                                TestOk = RunWifiTestElm(ipAddr, configure, out bool configRequired);
                                if (TestOk && configRequired)
                                {
                                    ConfigPossible = true;
                                }
                            }
                        }
                        finally
                        {
                            _testThread = null;
                            _form.UpdateButtonStatus();
                        }
                    });
                    _testThread.Start();
                }
                catch (Exception)
                {
                    _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                    return false;
                }
                return true;
            }

            BluetoothItem devInfo = _form.GetSelectedBtDevice();
            if (devInfo != null)
            {
                string pin = _form.BluetoothPin;
                _testThread = new Thread(() =>
                {
                    try
                    {
                        Thread.CurrentThread.CurrentCulture = cultureInfo;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;
                        _form.UpdateStatusText(Resources.Strings.Connecting);
                        if (!ConnectBtDevice(devInfo, pin, out string failureReason))
                        {
                            if (!string.IsNullOrEmpty(failureReason))
                            {
                                _form.UpdateStatusText(failureReason);
                            }
                            else
                            {
                                _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                            }
                            return;
                        }
                        TestOk = RunBtTest(configure, out bool configRequired);
                        if (TestOk && configRequired)
                        {
                            ConfigPossible = true;
                        }
                    }
                    finally
                    {
                        DisconnectStream();
                        _testThread = null;
                        _form.UpdateButtonStatus();
                    }
                });
                _testThread.Start();
                return true;
            }

            Patch.UsbInfo usbInfo = _form.GetSelectedUsbInfo();
            if (usbInfo != null)
            {
                _testThread = new Thread(() =>
                {
                    bool resetRequired = false;
                    try
                    {
                        Thread.CurrentThread.CurrentCulture = cultureInfo;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;
                        _form.UpdateStatusText(Resources.Strings.Connecting);
                        if (!ConnectUsbDevice(usbInfo))
                        {
                            _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                            return;
                        }

                        TestOk = RunUsbTest(usbInfo, out resetRequired);
                        if (TestOk && !resetRequired)
                        {
                            ConfigPossible = true;
                        }
                    }
                    finally
                    {
                        DisconnectStream();
                    }

                    if (!resetRequired)
                    {
                        _testThread = null;
                        _form.UpdateButtonStatus();
                        return;
                    }

                    _form.UpdateStatusText(Resources.Strings.ResettingUsbDevice, true);
                    bool resetOk = Patch.ResetFtdiDevice(usbInfo);
                    Thread.Sleep(3000);
                    _testThread = null;

                    _form.BeginInvoke((Action)(() =>
                    {
                        _form.UpdateButtonStatus();
                        if (!resetOk)
                        {
                            _form.UpdateStatusText(Resources.Strings.ResetUsbDeviceFailed, true);
                        }
                        else
                        {
                            _form.PerformSearch();
                        }
                    }));
                });
                _testThread.Start();
                return true;
            }

            return false;
        }

        private bool RunWifiTestEnetRetry(string ipAddr, out bool configRequired)
        {
            configRequired = false;
            for (int i = 0; i < 2; i++)
            {
                if (RunWifiTestEnet(ipAddr, out configRequired))
                {
                    return true;
                }
            }
            return false;
        }

        private bool RunWifiTestEnet(string ipAddr, out bool configRequired)
        {
            configRequired = false;
            _form.UpdateStatusText(Resources.Strings.Connecting);

            StringBuilder sr = new StringBuilder();
            string responseFromServer = null;
            try
            {
                try
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient();
                        _httpClient.Timeout = TimeSpan.FromSeconds(10);
                    }

                    string url = string.Format("http://{0}", ipAddr);
                    System.Threading.Tasks.Task<HttpResponseMessage> taskGet = _httpClient.GetAsync(url);
                    HttpResponseMessage responseGet = taskGet.Result;

                    bool success = responseGet.IsSuccessStatusCode;
                    if (!success)
                    {
                        if (responseGet.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            sr.Append(Resources.Strings.Connected);
                            sr.Append("\r\n");
                            sr.Append(Resources.Strings.TestOk);
                            _form.UpdateStatusText(sr.ToString());
                            return true;
                        }
                    }
                    else
                    {
                        responseFromServer = responseGet.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                if (string.IsNullOrEmpty(responseFromServer))
                {
                    _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                    return false;
                }

                sr.Append(Resources.Strings.Connected);
                if (!responseFromServer.Contains(@"LuCI - Lua Configuration Interface"))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.HttpResponseIncorrect);
                    _form.UpdateStatusText(sr.ToString());
                    return false;
                }

                sr.Append("\r\n");
                sr.Append(Resources.Strings.HttpResponseOk);
            }
            catch (Exception)
            {
                _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                return false;
            }

            sr.Append("\r\n");
            sr.Append(Resources.Strings.TestOk);
            _form.UpdateStatusText(sr.ToString());
            configRequired = true;
            return true;
        }

        private bool RunWifiTestElm(string ipAddress, bool configure, out bool configRequired)
        {
            configRequired = false;
            _form.UpdateStatusText(Resources.Strings.Connecting);

            int port = -1;
            if (string.Compare(ipAddress, ElmIp, StringComparison.OrdinalIgnoreCase) == 0)
            {
                port = ElmPort;
            }
            else if (string.Compare(ipAddress, EspLinkIp, StringComparison.OrdinalIgnoreCase) == 0)
            {
                port = EspLinkPort;
            }

            if (port < 0)
            {
                _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
            }

            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    IPEndPoint ipTcp = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    tcpClient.Connect(ipTcp);
                    _dataStream = tcpClient.GetStream();
                    _dataStreamWrite = new EscapeStreamWriter(_dataStream);
                    return RunBtTest(configure, out configRequired);
                }
            }
            catch (Exception)
            {
                _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                return false;
            }
            finally
            {
                DisconnectStream();
            }
        }

        private bool ConnectUsbDevice(Patch.UsbInfo usbInfo)
        {
            if (usbInfo == null)
            {
                return false;
            }

            try
            {
                _serialPort.PortName = usbInfo.ComPortName;
                _serialPort.BaudRate = 115200;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReadTimeout = 1000;

                _serialPort.Open();
                if (!_serialPort.IsOpen)
                {
                    DisconnectStream();
                    return false;
                }

                _dataStream = _serialPort.BaseStream;
                return true;
            }
            catch (Exception)
            {
                _serialPort.Close();
                return false;
            }
        }

        private bool RunUsbTest(Patch.UsbInfo usbInfo, out bool resetRequired)
        {
            resetRequired = false;
            StringBuilder sr = new StringBuilder();

            try
            {
                sr.Append(Resources.Strings.Connected);
                byte[] firmware = AdapterCommandCustom(0xFD, new byte[] { 0xFD });
                if ((firmware == null) || (firmware.Length < 4))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.ReadFirmwareVersionFailed);
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.SupportedUsbAdapters);
                    sr.Append("\r\n");
                    sr.Append(Patch.DcanKlineLink);
                    _form.UpdateStatusText(sr.ToString());
                    return false;
                }

                sr.Append("\r\n");
                sr.Append(Resources.Strings.FirmwareVersion);
                sr.Append(string.Format(" {0}.{1}", firmware[2], firmware[3]));
                AdapterType = (firmware[0] << 8) + firmware[1];

                sr.Append("\r\n");
                sr.Append(Resources.Strings.TestOk);

                if (usbInfo != null)
                {
                    int maxLatencyTimer = Math.Max(usbInfo.LatencyTimer, usbInfo.MaxRegLatencyTimer);
                    if (maxLatencyTimer > FtdiLatencyTimer)
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchingLatencyTime, maxLatencyTimer, FtdiLatencyTimer));
                        if (!Patch.SetFtdiLatencyTimer(usbInfo.ComPortName, FtdiLatencyTimer))
                        {
                            sr.Append("\r\n");
                            sr.Append(Resources.Strings.PatchingLatencyTimeFailed);
                            _form.UpdateStatusText(sr.ToString());
                            return false;
                        }

                        resetRequired = true;
                    }
                }

                _form.UpdateStatusText(sr.ToString());
                return true;
            }
            catch (Exception)
            {
                _form.UpdateStatusText(Resources.Strings.ConnectionFailed);
                return false;
            }
        }

        private bool RunBtTest(bool configure, out bool configRequired)
        {
            configRequired = false;
            StringBuilder sr = new StringBuilder();
            sr.Append(Resources.Strings.Connected);
            byte[] firmware = AdapterCommandCustom(0xFD, new byte[] { 0xFD });
            if (firmware == null)
            {
                for (int retry = 0; retry < 2; retry++)
                {
                    if (Elm327SendCommand("ATI", false))
                    {
                        Regex elmVerRegEx = new Regex(@"ELM327\s+v(\d)\.(\d)", RegexOptions.IgnoreCase);
                        string response = GetElm327Reponse();
                        if (response != null)
                        {
                            MatchCollection matchesVer = elmVerRegEx.Matches(response);
                            int elmVerH = -1;
                            int elmVerL = -1;
                            if ((matchesVer.Count == 1) && (matchesVer[0].Groups.Count == 3))
                            {
                                if (!Int32.TryParse(matchesVer[0].Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out elmVerH))
                                {
                                    elmVerH = -1;
                                }
                                if (!Int32.TryParse(matchesVer[0].Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out elmVerL))
                                {
                                    elmVerL = -1;
                                }
                            }

                            if (elmVerH >= 0 && elmVerL >= 0)
                            {
                                if (Elm327SendCommand(@"AT@2", false))
                                {
                                    string answer = GetElm327Reponse();
                                    if (answer != null)
                                    {
                                        if (answer.Contains("DEEPOBD"))
                                        {
                                            sr.Append("\r\n");
                                            sr.Append(Resources.Strings.FlashReplacementFirmware);
                                            _form.UpdateStatusText(sr.ToString());
                                            return false;
                                        }
                                    }
                                }

                                sr.Append("\r\n");
                                sr.Append(Resources.Strings.ElmAdapterConnected);
                                _form.UpdateStatusText(sr.ToString());
                                return false;
                            }
                        }
                    }
                }
            }
            if ((firmware == null) || (firmware.Length < 4))
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.ReadFirmwareVersionFailed);
                _form.UpdateStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append(Resources.Strings.FirmwareVersion);
            sr.Append(string.Format(" {0}.{1}", firmware[2], firmware[3]));
            AdapterType = (firmware[0] << 8) + firmware[1];

            bool escapeModeWrite = false;
            EscapeStreamWriter streamWriter = _dataStreamWrite as EscapeStreamWriter;
            if (streamWriter != null)
            {
                streamWriter.SetEscapeMode();
                // check if escape mode is required
                byte[] canModeTest = AdapterCommandCustom(0x82, new byte[] { 0x00 });
                if (canModeTest == null)
                {
                    escapeModeWrite = true;
                }
            }

            int fwVersion = (firmware[2] << 8) + firmware[3];
            if (!escapeModeWrite && fwVersion < 15)
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.FirmwareTooOld);
                _form.UpdateStatusText(sr.ToString());
                return false;
            }

            int modeValue = 0x00;
            if (escapeModeWrite)
            {
                modeValue |= EdCustomAdapterCommon.EscapeConfWrite;
            }

            byte[] escapeState = AdapterCommandCustom(0x06, new byte[] {
                (byte) (modeValue ^ EdCustomAdapterCommon.EscapeXor),
                EdCustomAdapterCommon.EscapeCodeDefault ^ EdCustomAdapterCommon.EscapeXor,
                EdCustomAdapterCommon.EscapeMaskDefault ^ EdCustomAdapterCommon.EscapeXor });

            if (escapeState == null || escapeState.Length < 1)
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.WriteModeFailed);
                _form.UpdateStatusText(sr.ToString());
                return false;
            }

            streamWriter?.SetEscapeMode(escapeModeWrite);

            byte[] canMode = AdapterCommandCustom(0x82, new byte[] { 0x00 });
            if ((canMode == null) || (canMode.Length < 1))
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.ReadModeFailed);
                _form.UpdateStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append(Resources.Strings.CanMode);
            sr.Append(" ");
            switch ((AdapterMode)canMode[0])
            {
                case AdapterMode.CanOff:
                    sr.Append(Resources.Strings.CanModeOff);
                    break;

                case AdapterMode.Can500:
                    sr.Append(Resources.Strings.CanMode500);
                    break;

                case AdapterMode.Can100:
                    sr.Append(Resources.Strings.CanMode100);
                    break;

                case AdapterMode.CanAuto:
                    sr.Append(Resources.Strings.CanModeAuto);
                    break;

                default:
                    sr.Append(Resources.Strings.CanModeUnknown);
                    break;
            }
            if ((AdapterMode)canMode[0] != AdapterMode.CanAuto)
            {
                if (configure)
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.CanModeChangeAuto);
                    canMode = AdapterCommandCustom(0x02, new[] { (byte)AdapterMode.CanAuto });
                    if ((canMode == null) || (canMode.Length < 1) || ((AdapterMode)canMode[0] != AdapterMode.CanAuto))
                    {
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.CanModeChangeFailed);
                    }
                }
                else
                {
                    configRequired = true;
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.CanModeNotAuto);
                }
            }
            sr.Append("\r\n");
            sr.Append(Resources.Strings.TestOk);
            _form.UpdateStatusText(sr.ToString());
            return true;
        }

        private bool ConnectBtDevice(BluetoothItem devInfo, string pin, out string failureReason)
        {
            failureReason = string.Empty;

            if (devInfo == null)
            {
                return false;
            }

            BluetoothDeviceInfo device = devInfo.DeviceInfo;
            if (device != null)
            {
                return ConnectBtDevice(device, pin);
            }

            BluetoothDevice btDevice = devInfo.Device;
            if (btDevice != null)
            {
                return ConnectBtLeDevice(btDevice, out failureReason);
            }

            return false;
        }

        private bool ConnectBtLeDevice(BluetoothDevice device, out string failureReason)
        {
            failureReason = string.Empty;
            try
            {
                string deviceId = device.Id;
                BluetoothDevice bluetoothDevice = BluetoothDevice.FromIdAsync(deviceId).Result;
                if (bluetoothDevice == null)
                {
                    DisconnectStream();
                    return false;
                }

                if (!bluetoothDevice.IsPaired)
                {
                    bluetoothDevice.PairAsync().Wait(BtLeGattSpp.BtLePairTimeout);
                    bluetoothDevice.Gatt.ConnectAsync().Wait(BtLeGattSpp.BtLeConnectTimeout);
                    failureReason = Resources.Strings.BtConfirmPairRequest;
                    DisconnectStream();
                    return false;
                }

                _btLeGattSpp = new BtLeGattSpp();
                if (!_btLeGattSpp.ConnectLeGattDevice(deviceId))
                {
                    DisconnectStream();
                    return false;
                }

                _dataStream = _btLeGattSpp.BtGattSppInStream;
                _dataStreamWrite = _btLeGattSpp.BtGattSppOutStream;
            }
            catch (Exception)
            {
                DisconnectStream();
                return false;
            }
            return true;
        }

        private bool ConnectBtDevice(BluetoothDeviceInfo device, string pin)
        {
            try
            {
                long startTimeDisconnect = Stopwatch.GetTimestamp();
                for (; ; )
                {
                    device.Refresh();
                    if (!device.Connected)
                    {
                        break;
                    }

                    if ((Stopwatch.GetTimestamp() - startTimeDisconnect) / TickResolMs > EdBluetoothInterface.BtDisconnectTimeout)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }

                device.Refresh();
                if (!device.Authenticated)
                {
                    BluetoothSecurity.PairRequest(device.DeviceAddress, pin);
                }
                BluetoothEndPoint ep = new BluetoothEndPoint(device.DeviceAddress, BluetoothService.SerialPort);
                _btClient = new BluetoothClient();

                try
                {
                    _btClient.Connect(ep);
                }
                catch (Exception)
                {
                    BluetoothSecurity.RemoveDevice(device.DeviceAddress);
                    Thread.Sleep(1000);
                    device.Refresh();
                    if (!device.Authenticated)
                    {
                        BluetoothSecurity.PairRequest(device.DeviceAddress, pin);
                    }
                    _btClient.Connect(ep);
                }
                _dataStream = _btClient.GetStream();
                _dataStreamWrite = null;
                Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DisconnectStream()
        {
            try
            {
                if (_dataStreamWrite != null)
                {
                    _dataStreamWrite.Close();
                    _dataStreamWrite = null;
                }
            }
            catch (Exception)
            {
                _dataStreamWrite = null;
            }

            try
            {
                if (_dataStream != null)
                {
                    _dataStream.Close();
                    _dataStream = null;
                }
            }
            catch (Exception)
            {
                _dataStream = null;
            }

            try
            {
                if (_btClient != null)
                {
                    _btClient.Close();
                    _btClient.Dispose();
                    _btClient = null;
                }
            }
            catch (Exception)
            {
                _btClient = null;
            }

            try
            {
                if (_btLeGattSpp != null)
                {
                    _btLeGattSpp.Dispose();
                    _btLeGattSpp = null;
                }
            }
            catch (Exception)
            {
                _btLeGattSpp = null;
            }

            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private byte[] AdapterCommandCustom(byte command, byte[] data)
        {
            if (_dataStream == null)
            {
                return null;
            }
            byte[] request = new byte[4 + data.Length];
            request[0] = (byte)(0x81 + data.Length);
            request[1] = 0xF1;
            request[2] = 0xF1;
            request[3] = command;
            Array.Copy(data, 0, request, 4, data.Length);

            if (!SendBmwFast(request))
            {
                return null;
            }
            byte[] response = new byte[0x100];
            // receive echo
            int echoLength = ReceiveBmwFast(response);
            if (echoLength != request.Length)
            {
                return null;
            }
            int length = ReceiveBmwFast(response);
            if ((length < 4) || (response[3] != command))
            {
                return null;
            }
            byte[] result = new byte[length - 4];
            Array.Copy(response, 4, result, 0, result.Length);
            return result;
        }

        private bool SendBmwFast(byte[] sendData)
        {
            if (_dataStream == null)
            {
                return false;
            }
            byte[] telBuffer = new byte[sendData.Length + 1];
            Array.Copy(sendData, telBuffer, sendData.Length);

            int sendLength = telBuffer[0] & 0x3F;
            if (sendLength == 0)
            {   // with length byte
                sendLength = telBuffer[3] + 4;
            }
            else
            {
                sendLength += 3;
            }
            telBuffer[sendLength] = CalcChecksumBmwFast(telBuffer, sendLength);
            sendLength++;
            try
            {
                if (_dataStreamWrite != null)
                {
                    _dataStreamWrite.Write(telBuffer, 0, sendLength);
                }
                else
                {
                    _dataStream.Write(telBuffer, 0, sendLength);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private int ReceiveBmwFast(byte[] receiveData)
        {
            if (_dataStream == null)
            {
                return 0;
            }
            try
            {
                MemoryQueueBufferStream memoryQueueStream = _dataStream as MemoryQueueBufferStream;
                if (memoryQueueStream == null)
                {
                    _dataStream.ReadTimeout = 1000;
                }

                // header byte
                for (int i = 0; i < 4; i++)
                {
                    int data;
                    try
                    {
                        if (memoryQueueStream != null)
                        {
                            data = memoryQueueStream.ReadByteAsync();
                        }
                        else
                        {
                            data = _dataStream.ReadByte();
                        }
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }

                    if (data < 0)
                    {
                        PurgeInStream(_dataStream);
                        return 0;
                    }
                    receiveData[i] = (byte)data;
                }

                if ((receiveData[0] & 0xC0) != 0x80)
                {   // 0xC0: Broadcast
                    PurgeInStream(_dataStream);
                    return 0;
                }

                int recLength = receiveData[0] & 0x3F;
                if (recLength == 0)
                {   // with length byte
                    recLength = receiveData[3] + 4;
                }
                else
                {
                    recLength += 3;
                }

                for (int i = 0; i < recLength - 3; i++)
                {
                    int data;
                    try
                    {
                        if (memoryQueueStream != null)
                        {
                            data = memoryQueueStream.ReadByteAsync();
                        }
                        else
                        {
                            data = _dataStream.ReadByte();
                        }
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        PurgeInStream(_dataStream);
                        return 0;
                    }
                    receiveData[i + 4] = (byte)data;
                }

                if (CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                {
                    PurgeInStream(_dataStream);
                    return 0;
                }
                return recLength;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private bool PurgeInStream(Stream inStream)
        {
            if (inStream == null)
            {
                return false;
            }

            NetworkStream networkStream = inStream as NetworkStream;
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
            }
            else if (networkStream != null)
            {
                while (networkStream.DataAvailable)
                {
                    try
                    {
                        inStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            else
            {
                while (inStream.HasData())
                {
                    try
                    {
                        inStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            return true;
        }

        public static byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        private bool Elm327SendCommand(string command, bool readAnswer = true)
        {
            if (_dataStream == null)
            {
                return false;
            }

            try
            {
                byte[] sendData = Encoding.UTF8.GetBytes(command + "\r");
                _dataStream.Write(sendData, 0, sendData.Length);

                if (readAnswer)
                {
                    string answer = GetElm327Reponse();
                    if (answer == null)
                    {
                        return false;
                    }
                    // check for OK
                    if (!answer.Contains("OK\r"))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetElm327Reponse()
        {
            if (_dataStream == null)
            {
                return null;
            }

            try
            {
                NetworkStream networkStream = _dataStream as NetworkStream;
                string response = null;
                StringBuilder stringBuilder = new StringBuilder();
                _dataStream.ReadTimeout = 1000;
                for (; ; )
                {
                    int data;
                    try
                    {
                        data = _dataStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }

                    if (data < 0)
                    {
                        if (networkStream != null)
                        {
                            while (networkStream.DataAvailable)
                            {
                                try
                                {
                                    networkStream.ReadByte();
                                }
                                catch (Exception)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (_serialPort.IsOpen)
                            {
                                _serialPort.DiscardInBuffer();
                            }
                        }
                        return null;
                    }

                    if (data >= 0 && data != 0x00)
                    {
                        // remove 0x00
                        stringBuilder.Append(Convert.ToChar(data));
                    }
                    if (data == 0x3E)
                    {
                        // prompt
                        response = stringBuilder.ToString();
                        break;
                    }
                    if (stringBuilder.Length > 500)
                    {
                        break;
                    }
                }

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
