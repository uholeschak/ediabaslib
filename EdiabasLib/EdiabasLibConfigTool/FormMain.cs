using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using Microsoft.Win32;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Threading.Tasks;
using EdiabasLib;
using System.Runtime.Versioning;
using System.Drawing;
using Windows.Foundation;
using System.Diagnostics;

namespace EdiabasLibConfigTool
{
    [SupportedOSPlatform("windows")]
    public partial class FormMain : Form
    {
        public const int FtdiDefaultVid = 0x0403;
        public const int FtdiDefaultPid232R = 0x6001;
        public const int FtdiDefaultPidXSer = 0x6015;

        private readonly BluetoothClient _cli;
        private readonly List<BluetoothDeviceInfo> _deviceList;
        private readonly Wifi _wifi;
        private readonly WlanClient _wlanClient;
        private readonly Test _test;
#if BT3
        private InTheHand.Net.Bluetooth.Factory.IBluetoothClient _icli;
#endif
        private bool _lastActiveProbing;
        private string _ediabasDirBmw;
        private string _ediabasDirVag;
        private string _ediabasDirIstad;
        private string _initMessage;
        private volatile bool _launchingSettings;
        private volatile bool _searching;
        private volatile bool _vehicleTaskActive;
        private List<EdInterfaceEnet.EnetConnection> _detectedVehicles;
        private ListViewItem _selectedItem;
        private bool _ignoreSelection;
        private int _removedUsbDevices;

        public string BluetoothPin => textBoxBluetoothPin.Text;
        public string WifiPassword => textBoxWifiPassword.Text;

        private class LanguageInfo
        {
            public LanguageInfo(string name, string culture)
            {
                Name = name;
                Culture = culture;
            }
            // ReSharper disable once MemberCanBePrivate.Local
            public string Name { get; }
            public string Culture { get; }
            public override string ToString()
            {
                return Name;
            }
        }

        public FormMain()
        {
            InitializeComponent();

            try
            {
                string language = Properties.Settings.Default.Language;
                if (!string.IsNullOrEmpty(language))
                {
                    SetCulture(language);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            comboBoxLanguage.Items.Clear();
            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Add(new LanguageInfo(Resources.Strings.LanguageEn, "en"));
            comboBoxLanguage.Items.Add(new LanguageInfo(Resources.Strings.LanguageDe, "de"));
            comboBoxLanguage.Items.Add(new LanguageInfo(Resources.Strings.LanguageRu, "ru"));
            comboBoxLanguage.Items.Add(new LanguageInfo(Resources.Strings.LanguageFr, "fr"));
            comboBoxLanguage.EndUpdate();

            string culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            int index = 0;
            int selIndex = -1;
            foreach (LanguageInfo languageInfo in comboBoxLanguage.Items)
            {
                if (string.Compare(languageInfo.Culture, culture, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    selIndex = index;
                }
                index++;
            }
            comboBoxLanguage.SelectedIndex = selIndex;

            listViewDevices.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            textBoxBluetoothPin.Text = @"1234";
            textBoxWifiPassword.Text = @"deepobdbmw";

            StringBuilder sr = new StringBuilder();
            try
            {
                _cli = new BluetoothClient();
#if BT3
                FieldInfo impField = typeof(BluetoothClient).GetField("m_impl", BindingFlags.NonPublic | BindingFlags.Instance);
                if (impField != null)
                {
                    _icli = impField.GetValue(_cli) as InTheHand.Net.Bluetooth.Factory.IBluetoothClient;
                }
#endif
            }
            catch (Exception ex)
            {
                sr.Append(string.Format(Resources.Strings.BtInitError, ex.Message));
            }
            _deviceList = new List<BluetoothDeviceInfo>();
            try
            {
                _wifi = new Wifi();
            }
            catch (Exception)
            {
                _wifi = null;
            }

            try
            {
                _wlanClient = new WlanClient();
            }
            catch (Exception)
            {
                _wlanClient = null;
            }

            _test = new Test(this);
            if (_wifi == null || _wlanClient == null)
            {
                if (sr.Length > 0)
                {
                    sr.Append("\r\n");
                }

                string message = string.Format(CultureInfo.InvariantCulture, Resources.Strings.WifiAccessRejected, "file://ms-settings/privacy-location");
                sr.Append(message);
            }
            else if (_wifi.NoWifiAvailable || _wlanClient.NoWifiAvailable)
            {
                if (sr.Length > 0)
                {
                    sr.Append("\r\n");
                }
                sr.Append(Resources.Strings.WifiAdapterError);
            }

            GetDirectories();

            _lastActiveProbing = GetEnableActiveProbing();
            if (_lastActiveProbing)
            {
                SetEnableActiveProbing(false);
            }

            _initMessage = sr.ToString();
            UpdateStatusText(string.Empty);
            UpdateButtonStatus();
        }

        private void SetCulture(string culture)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                ComponentResourceManager resources = new ComponentResourceManager(typeof(FormMain));
                resources.ApplyResources(this, "$this");
                ApplyResources(resources, Controls);
                UpdateStatusText(string.Empty);
                UpdateButtonStatus();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public bool StartSettingsApp(string settingsType)
        {
            try
            {
                if (string.IsNullOrEmpty(settingsType))
                {
                    return false;
                }

                if (_launchingSettings)
                {
                    return false;
                }

#pragma warning disable CA1416
                _launchingSettings = true;
                IAsyncOperation<bool> launchUri =
                    Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:" + settingsType));
                launchUri.AsTask().ContinueWith(task =>
                {
                    if (InvokeRequired)
                    {
                        _launchingSettings = false;
                        BeginInvoke((Action)UpdateButtonStatus);
                    }
                });
#pragma warning restore CA1416
                return true;
            }
            catch (Exception)
            {
                _launchingSettings = false;
                return false;
            }
            finally
            {
                UpdateButtonStatus();
            }
        }

        private void ApplyResources(ComponentResourceManager resources, Control.ControlCollection ctls)
        {
            foreach (Control ctl in ctls)
            {
                resources.ApplyResources(ctl, ctl.Name);
                ApplyResources(resources, ctl.Controls);
            }
        }

        private void GetDirectories()
        {
            string dirBmw = Environment.GetEnvironmentVariable("ediabas_config_dir");
            if (!Patch.IsValid(dirBmw))
            {
                string path = Environment.GetEnvironmentVariable("EDIABAS_PATH");
                if (!string.IsNullOrEmpty(path))
                {
                    dirBmw = Path.Combine(path, @"bin");
                }
            }

            if (!Patch.IsValid(dirBmw))
            {
                string path = LocateFileInPath(Patch.Api32DllName);
                if (!string.IsNullOrEmpty(path))
                {
                    dirBmw = path;
                }
            }

            if (Patch.IsValid(dirBmw))
            {
                _ediabasDirBmw = dirBmw;
            }

            try
            {
                _ediabasDirIstad = Properties.Settings.Default.IstadDir;
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key = localMachine32.OpenSubKey(@"SOFTWARE\Softing\EDIS-VW2"))
                    {
                        string path = key?.GetValue("EDIABASPath", null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dirVag = Path.Combine(path, @"bin");
                            if (Patch.IsValid(dirVag))
                            {
                                _ediabasDirVag = dirVag;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_ediabasDirVag))
                    {
                        using (RegistryKey key = localMachine32.OpenSubKey(@"Software\SIDIS\ENV"))
                        {
                            string dirVag = key?.GetValue("FLASHINIPATH", null) as string;
                            if (Patch.IsValid(dirVag))
                            {
                                _ediabasDirVag = dirVag;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_ediabasDirVag))
                    {
                        using (RegistryKey currentUser32 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                        {
                            using (RegistryKey key = currentUser32.OpenSubKey(@"Software\Softing\VASEGD2"))
                            {
                                string dirVag = key?.GetValue("strEdiabasApi32Path", null) as string;
                                if (Patch.IsValid(dirVag))
                                {
                                    _ediabasDirVag = dirVag;
                                }
                            }
                        }
                    }

                    using (RegistryKey key = localMachine32.OpenSubKey(Patch.RegKeyIsta))
                    {
                        string path = key?.GetValue(Patch.RegValueIstaLocation, null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dirIstad = Path.Combine(path, Patch.EdiabasDirName, Patch.EdiabasBinDirName);
                            if (Patch.IsValid(dirIstad))
                            {
                                _ediabasDirIstad = dirIstad;
                            }
                        }
                    }
                }

                using (RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey key = localMachine64.OpenSubKey(Patch.RegKeyIsta))
                    {
                        string path = key?.GetValue(Patch.RegValueIstaLocation, null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dirIstad = Path.Combine(path, Patch.EdiabasDirName, Patch.EdiabasBinDirName);
                            if (Patch.IsValid(dirIstad))
                            {
                                _ediabasDirIstad = dirIstad;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private string LocateFileInPath(string fileName)
        {
            string envPath = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(envPath))
            {
                return null;
            }

            string result = envPath
                .Split(';')
                .FirstOrDefault(s => File.Exists(Path.Combine(s, fileName)));

            return result;
        }

        private bool GetEnableActiveProbing()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\NlaSvc\Parameters\Internet"))
                {
                    int? activeProbing = key?.GetValue("EnableActiveProbing", null) as int?;
                    if (activeProbing.HasValue && activeProbing.Value != 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SetEnableActiveProbing(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\NlaSvc\Parameters\Internet",
                    RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.SetValue))
                {
                    if (key != null)
                    {
                        int value = enable ? 1 : 0;
                        key.SetValue("EnableActiveProbing", value);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private void AddWifiAdapters(ListView listView)
        {
            try
            {
                if (_wlanClient != null)
                {
                    foreach (WlanInterface wlanIface in _wlanClient.Interfaces)
                    {
                        if (wlanIface.InterfaceState == WlanInterfaceState.Connected)
                        {
                            WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                            string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                            if (string.Compare(ssidString, Patch.AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(ssidString, Patch.AdapterSsidElm1, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(ssidString, Patch.AdapterSsidElm2, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(ssidString, Patch.AdapterSsidEspLink, StringComparison.OrdinalIgnoreCase) == 0 ||
                                ssidString.StartsWith(Patch.AdapterSsidEnetLink, StringComparison.OrdinalIgnoreCase) ||
                                ssidString.StartsWith(Patch.AdapterSsidModBmw, StringComparison.OrdinalIgnoreCase) ||
                                ssidString.StartsWith(Patch.AdapterSsidUniCar, StringComparison.OrdinalIgnoreCase)
                               )
                            {
                                string bssString = conn.wlanAssociationAttributes.Dot11Bssid.ToString();
                                ListViewItem listViewItem =
                                    new ListViewItem(new[] { bssString, conn.profileName })
                                    {
                                        Tag = wlanIface
                                    };
                                listView.Items.Add(listViewItem);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (_wifi != null)
                {
                    foreach (AccessPoint ap in _wifi.GetAccessPoints())
                    {
                        if (!ap.IsConnected)
                        {
                            if (string.Compare(ap.Name, Patch.AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(ap.Name, Patch.AdapterSsidElm1, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(ap.Name, Patch.AdapterSsidElm2, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(ap.Name, Patch.AdapterSsidEspLink, StringComparison.OrdinalIgnoreCase) == 0 ||
                                ap.Name.StartsWith(Patch.AdapterSsidEnetLink, StringComparison.OrdinalIgnoreCase) ||
                                ap.Name.StartsWith(Patch.AdapterSsidModBmw, StringComparison.OrdinalIgnoreCase) ||
                                ap.Name.StartsWith(Patch.AdapterSsidUniCar, StringComparison.OrdinalIgnoreCase)
                               )
                            {
                                ListViewItem listViewItem =
                                    new ListViewItem(new[] { Resources.Strings.DisconnectedAdapter, ap.Name })
                                    {
                                        Tag = ap
                                    };
                                listView.Items.Add(listViewItem);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void AddDetectedVehicles(ListView listView)
        {
            if (_detectedVehicles == null)
            {
                return;
            }

            List<ListViewItem> addItems = new List<ListViewItem>();
            foreach (EdInterfaceEnet.EnetConnection enetConnection in _detectedVehicles)
            {
                string ipAddress = enetConnection.ToString();
                StringBuilder sbInfo = new StringBuilder();
                if (!string.IsNullOrEmpty(enetConnection.Vin))
                {
                    sbInfo.Append(enetConnection.Vin);
                }

                if (!string.IsNullOrEmpty(enetConnection.Mac))
                {
                    if (sbInfo.Length > 0)
                    {
                        sbInfo.Append(" / ");
                    }

                    sbInfo.Append("MAC=");
                    sbInfo.Append(enetConnection.Mac.ToUpperInvariant());
                }


                ListViewItem listViewItem =
                    new ListViewItem(new[] { ipAddress, sbInfo.ToString() })
                    {
                        Tag = enetConnection
                    };
                addItems.Add(listViewItem);
            }

            foreach (ListViewItem addItem in addItems.OrderBy(x => ((EdInterfaceEnet.EnetConnection)x.Tag)?.ToString() ?? string.Empty))
            {
                listView.Items.Add(addItem);
            }
        }

        private void AddFtdiDevices(ListView listView)
        {
            try
            {
                _removedUsbDevices = 0;
                UInt32 deviceCount = 0;
                Ftd2Xx.FT_STATUS ftStatus = Ftd2Xx.FT_CreateDeviceInfoList(ref deviceCount);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    return;
                }

                if (deviceCount < 1)
                {
                    return;
                }

                List<ListViewItem> addItems = new List<ListViewItem>();
                for (uint index = 0; index < deviceCount; index++)
                {
                    try
                    {
                        byte[] serialNumber = new byte[16];
                        byte[] description = new byte[64];
                        ftStatus = Ftd2Xx.FT_GetDeviceInfoDetail(index, out UInt32 deviceFlags, out Ftd2Xx.FT_DEVICE deviceType,
                            out UInt32 idValue, out UInt32 deviceLocId, serialNumber, description, out IntPtr handleTemp);
                        if (ftStatus == Ftd2Xx.FT_STATUS.FT_OK)
                        {
                            uint deviceId = idValue & 0xFFFF;
                            uint vendorId = (idValue >> 16) & 0xFFFF;

                            int serialNullIdx = Array.IndexOf(serialNumber, (byte)0);
                            serialNullIdx = serialNullIdx >= 0 ? serialNullIdx : serialNumber.Length;
                            string serialString = Encoding.ASCII.GetString(serialNumber, 0, serialNullIdx);

                            int descNullIdx = Array.IndexOf(description, (byte)0);
                            descNullIdx = descNullIdx >= 0 ? descNullIdx : description.Length;
                            string descriptionString = Encoding.ASCII.GetString(description, 0, descNullIdx);

                            ftStatus = Ftd2Xx.FT_OpenEx((IntPtr)deviceLocId, Ftd2Xx.FT_OPEN_BY_LOCATION, out IntPtr handleFtdi);
                            if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                            {
                                handleFtdi = IntPtr.Zero;
                            }

                            Patch.UsbInfo usbInfo = null;
                            if (handleFtdi != IntPtr.Zero)
                            {
                                string comPortName = null;
                                ftStatus = Ftd2Xx.FT_GetComPortNumber(handleFtdi, out Int32 comPort);
                                if (ftStatus == Ftd2Xx.FT_STATUS.FT_OK)
                                {
                                    if (comPort >= 0)
                                    {
                                        comPortName = "COM" + comPort.ToString(CultureInfo.InvariantCulture);
                                    }
                                }

                                int? latencyTimer = null;
                                ftStatus = Ftd2Xx.FT_GetLatencyTimer(handleFtdi, out byte latency);
                                if (ftStatus == Ftd2Xx.FT_STATUS.FT_OK)
                                {
                                    latencyTimer = latency;
                                }

                                List<int> regLatencyTimers = Patch.GetFtdiLatencyTimer(comPortName);
                                if (!string.IsNullOrEmpty(comPortName) && latencyTimer != null && regLatencyTimers != null)
                                {
                                    int maxRegLatencyTimer = regLatencyTimers.Max();
                                    usbInfo = new Patch.UsbInfo(deviceLocId, comPort, comPortName, latencyTimer.Value, maxRegLatencyTimer);
                                }
                            }

                            if (handleFtdi != IntPtr.Zero)
                            {
                                Ftd2Xx.FT_Close(handleFtdi);
                            }

                            bool validDevice = usbInfo != null;
                            switch (deviceType)
                            {
                                case Ftd2Xx.FT_DEVICE.FT_DEVICE_232R:
                                    if (deviceId != FtdiDefaultPid232R)
                                    {
                                        validDevice = false;
                                    }
                                    break;

                                case Ftd2Xx.FT_DEVICE.FT_DEVICE_X_SERIES:
                                    if (deviceId != FtdiDefaultPidXSer)
                                    {
                                        validDevice = false;
                                    }
                                    break;

                                default:
                                    validDevice = false;
                                    break;
                            }

                            if (vendorId != FtdiDefaultVid)
                            {
                                validDevice = false;
                            }

                            if (validDevice)
                            {
                                StringBuilder sbInfo = new StringBuilder();
                                if (!string.IsNullOrEmpty(descriptionString))
                                {
                                    sbInfo.Append(descriptionString);
                                }

                                if (!string.IsNullOrEmpty(serialString))
                                {
                                    if (sbInfo.Length > 0)
                                    {
                                        sbInfo.Append(" ");
                                    }
                                    sbInfo.Append("(");
                                    sbInfo.Append(serialString);
                                    sbInfo.Append(")");
                                }

                                int maxLatencyTimer = Math.Max(usbInfo.LatencyTimer, usbInfo.MaxRegLatencyTimer);
                                if (sbInfo.Length > 0)
                                {
                                    sbInfo.Append(" / ");
                                }
                                sbInfo.Append(string.Format(Resources.Strings.LatencyTime, maxLatencyTimer));

                                ListViewItem listViewItem =
                                    new ListViewItem(new[] { usbInfo.ComPortName, sbInfo.ToString() })
                                    {
                                        Tag = usbInfo
                                    };
                                addItems.Add(listViewItem);
                            }
                            else
                            {
                                _removedUsbDevices++;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                foreach (ListViewItem addItem in addItems.OrderBy(x => ((Patch.UsbInfo) x.Tag)?.ComPortNum ?? 0))
                {
                    listView.Items.Add(addItem);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool StartDeviceSearch()
        {
            if (_searching)
            {
                return false;
            }

            UpdateStatusText(Resources.Strings.Searching);
            _vehicleTaskActive = true;
            _detectedVehicles = null;
            SearchVehiclesTask().ContinueWith(task =>
            {
                _vehicleTaskActive = false;
                BeginInvoke((Action)(() =>
                {
                    List<EdInterfaceEnet.EnetConnection> detectedVehicles = task.Result;
                    if (detectedVehicles != null)
                    {
                        _detectedVehicles = detectedVehicles;
                    }

                    UpdateDeviceList(Array.Empty<BluetoothDeviceInfo>(), false);
                    UpdateButtonStatus();
                    ShowSearchEndMessage();
                }));
            });

            UpdateDeviceList(null, true);
            if (_cli == null)
            {
                return false;
            }
#if BT3
            if (_icli == null)
            {
                return false;
            }
#endif
            try
            {
                _test.TestOk = false;
                _test.ConfigPossible = false;
                _deviceList.Clear();
#if BT3
                IAsyncResult asyncResult = _icli.BeginDiscoverDevices(255, true, false, true, IsWinVistaOrHigher(), ar =>
                {
                    if (ar.IsCompleted)
                    {
                        _searching = false;
                        UpdateButtonStatus();

                        try
                        {
                            BluetoothDeviceInfo[] devices = _cli.EndDiscoverDevices(ar);
                            BeginInvoke((Action)(() =>
                            {
                                UpdateDeviceList(devices, true);
                                ShowSearchEndMessage();
                            }));
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                            if (string.IsNullOrEmpty(message))
                            {
                                UpdateStatusText(Resources.Strings.SearchingFailed);
                            }
                            else
                            {
                                UpdateStatusText(string.Format(Resources.Strings.SearchingFailedMessage, message));
                            }
                        }
                    }
                }, this, (p1, p2) =>
                {
                    BluetoothDeviceInfo deviceInfo = new BluetoothDeviceInfo(p1.DeviceAddress);
                    try
                    {
                        deviceInfo.Refresh();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    BeginInvoke((Action)(() =>
                    {
                        UpdateDeviceList(new[] { deviceInfo }, false);
                    }));

                }, this);
#else
                Task searchTask = new Task(() =>
                {
                    try
                    {
                        IReadOnlyCollection<BluetoothDeviceInfo> devices = _cli.DiscoverDevices();
                        _searching = false;
                        UpdateButtonStatus();

                        BeginInvoke((Action)(() =>
                        {
                            UpdateDeviceList(devices.ToArray(), true);
                            ShowSearchEndMessage();
                        }));
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        if (string.IsNullOrEmpty(message))
                        {
                            UpdateStatusText(Resources.Strings.SearchingFailed);
                        }
                        else
                        {
                            UpdateStatusText(string.Format(Resources.Strings.SearchingFailedMessage, message));
                        }
                    }
                });

                searchTask.Start();
#endif
                _searching = true;
                UpdateButtonStatus();
            }
            catch (Exception)
            {
                UpdateStatusText(Resources.Strings.SearchingFailed);
                return false;
            }
            return true;
        }

        private void UpdateDeviceList(BluetoothDeviceInfo[] devices, bool completed)
        {
            _ignoreSelection = true;
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();
            AddDetectedVehicles(listViewDevices);
            AddWifiAdapters(listViewDevices);
            AddFtdiDevices(listViewDevices);
            if (devices != null)
            {
                if (completed)
                {
                    _deviceList.Clear();
                    _deviceList.AddRange(devices);
                }
                else
                {
                    foreach (BluetoothDeviceInfo device in devices.OrderBy(dev => dev.DeviceAddress.ToString()))
                    {
                        for (int i = 0; i < _deviceList.Count; i++)
                        {
                            if (_deviceList[i].DeviceAddress == device.DeviceAddress)
                            {
                                _deviceList.RemoveAt(i);
                                i--;
                            }
                        }
                        _deviceList.Add(device);
                    }
                }

                foreach (BluetoothDeviceInfo device in _deviceList.OrderBy(dev => dev.DeviceAddress.ToString()))
                {
                    ListViewItem listViewItem =
                        new ListViewItem(new[] { device.DeviceAddress.ToString(), device.DeviceName })
                        {
                            Tag = device
                        };
                    listViewDevices.Items.Add(listViewItem);
                }
            }
            // select last selected item
            if (_selectedItem != null && _selectedItem.Tag != null && _selectedItem.SubItems.Count >= 2)
            {
                ListViewItem listViewItemSelect = null;
                foreach (ListViewItem listViewItem in listViewDevices.Items)
                {
                    if (listViewItem.Tag == null)
                    {
                        continue;
                    }

                    if (listViewItem.SubItems.Count < 2)
                    {
                        continue;
                    }

                    if (listViewItem.Tag.GetType() != _selectedItem.Tag.GetType())
                    {
                        continue;
                    }

                    if (string.Compare(listViewItem.SubItems[0].Text, _selectedItem.SubItems[0].Text, StringComparison.Ordinal) == 0)
                    {
                        listViewItemSelect = listViewItem;
                        break;
                    }
                }

                if (listViewItemSelect == null)
                {
                    if (_selectedItem.Tag.GetType() == typeof(AccessPoint))
                    {
                        foreach (ListViewItem listViewItem in listViewDevices.Items)
                        {
                            if (listViewItem.Tag == null)
                            {
                                continue;
                            }

                            if (listViewItem.SubItems.Count < 2)
                            {
                                continue;
                            }

                            if (listViewItem.Tag.GetType() != typeof(WlanInterface))
                            {
                                continue;
                            }

                            if (string.Compare(listViewItem.SubItems[1].Text, _selectedItem.SubItems[1].Text, StringComparison.Ordinal) == 0)
                            {
                                listViewItemSelect = listViewItem;
                                break;
                            }
                        }
                    }
                }

                if (listViewItemSelect != null)
                {
                    listViewItemSelect.Selected = true;
                }
            }

            listViewDevices.EndUpdate();
            _ignoreSelection = false;
            UpdateButtonStatus();
        }

        private async Task<List<EdInterfaceEnet.EnetConnection>> SearchVehiclesTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => SearchVehicles()).ConfigureAwait(false);
        }

        private List<EdInterfaceEnet.EnetConnection> SearchVehicles()
        {
            List<EdInterfaceEnet.EnetConnection> detectedVehicles;
            using (EdInterfaceEnet edInterface = new EdInterfaceEnet(false))
            {
                detectedVehicles = edInterface.DetectedVehicles(EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll);
            }

            return detectedVehicles;
        }

        public BluetoothDeviceInfo GetSelectedBtDevice()
        {
            BluetoothDeviceInfo devInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                devInfo = listViewDevices.SelectedItems[0].Tag as BluetoothDeviceInfo;
            }
            return devInfo;
        }

        public WlanInterface GetSelectedWifiDevice()
        {
            WlanInterface wlanIface = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                wlanIface = listViewDevices.SelectedItems[0].Tag as WlanInterface;
            }
            return wlanIface;
        }

        public EdInterfaceEnet.EnetConnection GetSelectedEnetDevice()
        {
            EdInterfaceEnet.EnetConnection enetConnection = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                enetConnection = listViewDevices.SelectedItems[0].Tag as EdInterfaceEnet.EnetConnection;
            }
            return enetConnection;
        }

        public AccessPoint GetSelectedAp()
        {
            AccessPoint ap = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                ap = listViewDevices.SelectedItems[0].Tag as AccessPoint;
            }
            return ap;
        }

        public Patch.UsbInfo GetSelectedUsbInfo()
        {
            Patch.UsbInfo usbInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                usbInfo = listViewDevices.SelectedItems[0].Tag as Patch.UsbInfo;
            }
            return usbInfo;
        }

        public void UpdateButtonStatus()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)UpdateButtonStatus);
                return;
            }
            if (_test == null)
            {
                return;
            }

            bool processing = _launchingSettings || _searching || _vehicleTaskActive;
            comboBoxLanguage.Enabled = !processing && !_test.ThreadActive;
            buttonSearch.Enabled = !processing && !_test.ThreadActive;
            buttonClose.Enabled = !processing && !_test.ThreadActive;

            BluetoothDeviceInfo devInfo = GetSelectedBtDevice();
            WlanInterface wlanIface = GetSelectedWifiDevice();
            EdInterfaceEnet.EnetConnection enetConnection = GetSelectedEnetDevice();
            AccessPoint ap = GetSelectedAp();
            Patch.UsbInfo usbInfo = GetSelectedUsbInfo();
            buttonTest.Enabled = buttonSearch.Enabled && ((devInfo != null) || (wlanIface != null) || (ap != null) || (usbInfo != null)) && !_test.ThreadActive;

            bool allowPatch = false;
            if (!processing)
            {
                if (enetConnection != null)
                {
                    allowPatch = true;
                }
                else
                {
                    allowPatch = buttonTest.Enabled && _test.TestOk && ((wlanIface != null) || (devInfo != null) || (usbInfo != null));
                }
            }

            bool allowRestore = !processing && !_test.ThreadActive;

            bool bmwValid = Patch.IsValid(_ediabasDirBmw);
            groupBoxEdiabas.Enabled = bmwValid;
            buttonPatchEdiabas.Enabled = bmwValid && allowPatch;
            buttonRestoreEdiabas.Enabled = bmwValid && allowRestore && Patch.IsPatched(_ediabasDirBmw, Patch.PatchType.Ediabas);

            bool vagValid = Patch.IsValid(_ediabasDirVag);
            groupBoxVasPc.Enabled = vagValid;
            buttonPatchVasPc.Enabled = vagValid && allowPatch && (devInfo != null);
            buttonRestoreVasPc.Enabled = vagValid && allowRestore && Patch.IsPatched(_ediabasDirVag, Patch.PatchType.VasPc);

            bool istadValid = Patch.IsValid(_ediabasDirIstad);
            groupBoxIstad.Enabled = true;
            textBoxIstaLocation.Enabled = true;
            textBoxIstaLocation.Text = _ediabasDirIstad ?? string.Empty;
            buttonDirIstad.Enabled = allowRestore;
            buttonPatchIstad.Enabled = istadValid && allowPatch;
            buttonRestoreIstad.Enabled = istadValid && allowRestore && Patch.IsPatched(_ediabasDirIstad, Patch.PatchType.Istad);

            textBoxBluetoothPin.Enabled = !_test.ThreadActive;
            textBoxWifiPassword.Enabled = !_test.ThreadActive;
            if ((devInfo != null) || (wlanIface != null))
            {
                if (_test.TestOk && _test.ConfigPossible)
                {
                    buttonTest.Text = Resources.Strings.ButtonTestConfiguration;
                }
                else
                {
                    buttonTest.Text = Resources.Strings.ButtonTestCheck;
                }
            }
            else if (ap != null)
            {
                buttonTest.Text = Resources.Strings.ButtonTestConnect;
            }
            else
            {
                buttonTest.Text = Resources.Strings.ButtonTestCheck;
            }
        }

        private void ClearInitMessage()
        {
            _initMessage = string.Empty;
            UpdateStatusText(string.Empty);
        }

        public void ShowSearchEndMessage()
        {
            if (!_searching && !_vehicleTaskActive)
            {
                StringBuilder sb = new StringBuilder();
                if (_removedUsbDevices > 0)
                {
                    string ftdiVid = string.Format(CultureInfo.InvariantCulture, "{0:X4}h", FtdiDefaultVid);
                    string ftdiPids232R = string.Format(CultureInfo.InvariantCulture, "{0:X4}h/{1:X4}h", FtdiDefaultPid232R, FtdiDefaultPidXSer);
                    sb.Append(string.Format(CultureInfo.InvariantCulture, Resources.Strings.UsbAdaptersHidden, ftdiVid, ftdiPids232R));
                }

                if (sb.Length > 0)
                {
                    sb.Append("\r\n");
                }
                sb.Append(listViewDevices.Items.Count > 0 ? Resources.Strings.DevicesFound : Resources.Strings.DevicesNotFound);

                UpdateStatusText(sb.ToString());
            }
        }

        public void UpdateStatusText(string text, bool appendText = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateStatusText(text, appendText);
                }));
                return;
            }

            StringBuilder sb = new StringBuilder();
            if (appendText)
            {
                string lastText = richTextBoxStatus.Text;
                if (!string.IsNullOrEmpty(lastText))
                {
                    sb.Append(lastText);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_initMessage))
                {
                    sb.Append(_initMessage);
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (sb.Length > 0)
                {
                    sb.Append("\r\n");
                }
                sb.Append(text);
            }

            richTextBoxStatus.Text = sb.ToString();
            richTextBoxStatus.SelectionStart = richTextBoxStatus.TextLength;
            richTextBoxStatus.Update();
            richTextBoxStatus.ScrollToCaret();
        }

        public void PerformSearch()
        {
            if (!buttonSearch.Enabled)
            {
                return;
            }
            if (StartDeviceSearch())
            {
                UpdateButtonStatus();
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            ClearInitMessage();
            PerformSearch();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_lastActiveProbing)
            {
                SetEnableActiveProbing(true);
            }

            _cli?.Dispose();
            _test?.Dispose();
            try
            {
                Properties.Settings.Default.IstadDir = _ediabasDirIstad ?? string.Empty;
                Properties.Settings.Default.Language = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!buttonClose.Enabled)
            {
                e.Cancel = true;
            }
        }

        private void listViewDevices_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.NewWidth = listViewDevices.Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream(Properties.Resources.AppIcon))
            {
                Icon = new Icon(ms);
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            listViewDevices.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
            UpdateButtonStatus();
            PerformSearch();
        }

        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreSelection)
            {
                return;
            }
            if (listViewDevices.SelectedItems.Count > 0)
            {
                _selectedItem = listViewDevices.SelectedItems[0];
            }
            _test.TestOk = false;
            _test.ConfigPossible = false;
            UpdateButtonStatus();
        }

        private void textBoxBluetoothPin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            ClearInitMessage();
            if (buttonTest.Enabled)
            {
                _test.ExecuteTest(_test.TestOk && _test.ConfigPossible);
                UpdateButtonStatus();
            }
        }

        private void listViewDevices_DoubleClick(object sender, EventArgs e)
        {
            buttonTest_Click(sender, e);
        }

        private void buttonPatch_Click(object sender, EventArgs e)
        {
            ClearInitMessage();
            BluetoothDeviceInfo devInfo = GetSelectedBtDevice();
            WlanInterface wlanIface = GetSelectedWifiDevice();
            EdInterfaceEnet.EnetConnection enetConnection = GetSelectedEnetDevice();
            Patch.UsbInfo usbInfo = GetSelectedUsbInfo();
            if (devInfo == null && wlanIface == null && enetConnection == null && usbInfo == null)
            {
                return;
            }

            string dirName = null;
            Patch.PatchType patchType = Patch.PatchType.Ediabas;
            if (sender == buttonPatchEdiabas)
            {
                dirName = _ediabasDirBmw;
                patchType = Patch.PatchType.Ediabas;
            }
            else if (sender == buttonPatchVasPc)
            {
                dirName = _ediabasDirVag;
                patchType = Patch.PatchType.VasPc;
            }
            else if (sender == buttonPatchIstad)
            {
                dirName = _ediabasDirIstad;
                patchType = Patch.PatchType.Istad;

                if (Patch.GetIstaReg() != null)
                {
                    string userDir = EdiabasNet.GetEdiabasLibUserDir();
                    if (!string.IsNullOrEmpty(userDir))
                    {
                        string message = string.Format(Resources.Strings.IstaRegExtMessage, Patch.RegKeyIstaBinFull, userDir);
                        DialogResult result = MessageBox.Show(message, Resources.Strings.IstaRegExtTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        switch (result)
                        {
                            case DialogResult.Yes:
                                patchType = Patch.PatchType.IstadExt;
                                dirName = userDir;
                                break;

                            case DialogResult.No:
                                patchType = Patch.PatchType.Istad;
                                break;

                            default:
                                return;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(dirName))
            {
                StringBuilder sr = new StringBuilder();
                Patch.PatchEdiabas(sr, patchType, _test.AdapterType, dirName, devInfo, wlanIface, enetConnection, usbInfo, textBoxBluetoothPin.Text);
                UpdateStatusText(sr.ToString());
            }
            UpdateButtonStatus();
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            string dirName = null;
            Patch.PatchType patchType = Patch.PatchType.Ediabas;
            if (sender == buttonRestoreEdiabas)
            {
                dirName = _ediabasDirBmw;
                patchType = Patch.PatchType.Ediabas;
            }
            else if (sender == buttonRestoreVasPc)
            {
                dirName = _ediabasDirVag;
                patchType = Patch.PatchType.VasPc;
            }
            else if (sender == buttonRestoreIstad)
            {
                dirName = _ediabasDirIstad;
                patchType = Patch.PatchType.Istad;
            }

            if (!string.IsNullOrEmpty(dirName))
            {
                StringBuilder sr = new StringBuilder();
                Patch.RestoreEdiabas(sr, patchType, dirName);
                UpdateStatusText(sr.ToString());
            }
            UpdateButtonStatus();
        }

        private void buttonDirIstad_Click(object sender, EventArgs e)
        {
            openFileDialogConfigFile.InitialDirectory = _ediabasDirIstad ?? string.Empty;
            openFileDialogConfigFile.FileName = string.Empty;
            if (openFileDialogConfigFile.ShowDialog() == DialogResult.OK)
            {
                _ediabasDirIstad = Path.GetDirectoryName(openFileDialogConfigFile.FileName);
            }
            UpdateButtonStatus();
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            LanguageInfo languageInfo = comboBoxLanguage.SelectedItem as LanguageInfo;
            if (languageInfo != null)
            {
                SetCulture(languageInfo.Culture);
            }
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            listViewDevices.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void richTextBoxStatus_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            string url = e.LinkText;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (url.Contains("privacy-location"))
            {
                StartSettingsApp("privacy-location");
                return;
            }

            if (url.StartsWith(@"https://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith(@"http://", StringComparison.OrdinalIgnoreCase))
            {
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
            }
        }
    }
}
