using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;
using Microsoft.Win32;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Threading.Tasks;
using EdiabasLib;
using System.Reflection;

namespace EdiabasLibConfigTool
{
    public partial class FormMain : Form
    {
        private readonly BluetoothClient _cli;
        private readonly List<BluetoothDeviceInfo> _deviceList;
        private readonly WifiMod _wifi;
        private readonly WlanClient _wlanClient;
        private readonly Test _test;
        private IBluetoothClient _icli;
        private bool _lastActiveProbing;
        private string _ediabasDirBmw;
        private string _ediabasDirVag;
        private string _ediabasDirIstad;
        private string _initMessage;
        private volatile bool _searching;
        private volatile bool _vehicleTaskActive;
        private List<EdInterfaceEnet.EnetConnection> _detectedVehicles;
        private ListViewItem _selectedItem;
        private bool _ignoreSelection;

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
            Icon = Properties.Resources.AppIcon;

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
            listViewDevices.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            textBoxBluetoothPin.Text = @"1234";
            textBoxWifiPassword.Text = @"deepobdbmw";

            StringBuilder sr = new StringBuilder();
            try
            {
                _cli = new BluetoothClient();
                FieldInfo impField = typeof(BluetoothClient).GetField("m_impl", BindingFlags.NonPublic | BindingFlags.Instance);
                if (impField != null)
                {
                    _icli = impField.GetValue(_cli) as IBluetoothClient;
                }
            }
            catch (Exception ex)
            {
                sr.Append(string.Format(Resources.Strings.BtInitError, ex.Message));
            }
            _deviceList = new List<BluetoothDeviceInfo>();
            _wifi = new WifiMod();
            _wlanClient = new WlanClient();
            _test = new Test(this);
            if (_wifi.NoWifiAvailable || _wlanClient.NoWifiAvailable)
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

        private void ApplyResources(ComponentResourceManager resources, Control.ControlCollection ctls)
        {
            foreach (Control ctl in ctls)
            {
                resources.ApplyResources(ctl, ctl.Name);
                ApplyResources(resources, ctl.Controls);
            }
        }

        private bool IsWinVistaOrHigher()
        {
            OperatingSystem os = Environment.OSVersion;
            return (os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6);
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
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Softing\EDIS-VW2"))
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
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\SIDIS\ENV"))
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
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Softing\VASEGD2"))
                    {
                        string dirVag = key?.GetValue("strEdiabasApi32Path", null) as string;
                        if (Patch.IsValid(dirVag))
                        {
                            _ediabasDirVag = dirVag;
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
                _ediabasDirIstad = Properties.Settings.Default.IstadDir;
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
                foreach (WlanInterface wlanIface in _wlanClient.Interfaces)
                {
                    if (wlanIface.InterfaceState == WlanInterfaceState.Connected)
                    {
                        WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                        string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                        if (string.Compare(ssidString, Patch.AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(ssidString, Patch.AdapterSsidElm, StringComparison.OrdinalIgnoreCase) == 0 ||
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
            catch (Exception)
            {
                // ignored
            }
            try
            {
                foreach (AccessPoint ap in _wifi.GetAccessPoints())
                {
                    if (!ap.IsConnected)
                    {
                        if (string.Compare(ap.Name, Patch.AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(ap.Name, Patch.AdapterSsidElm, StringComparison.OrdinalIgnoreCase) == 0 || 
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
                listView.Items.Add(listViewItem);
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
            if (_cli == null || _icli == null)
            {
                return false;
            }

            try
            {
                _test.TestOk = false;
                _test.ConfigPossible = false;
                _deviceList.Clear();

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
                        new ListViewItem(new[] {device.DeviceAddress.ToString(), device.DeviceName})
                        {
                            Tag = device
                        };
                    listViewDevices.Items.Add(listViewItem);
                }
            }
            // select last selected item
            if (_selectedItem != null)
            {
                foreach (ListViewItem listViewItem in listViewDevices.Items)
                {
                    if (listViewItem.Tag.GetType() != _selectedItem.Tag.GetType())
                    {
                        continue;
                    }
                    if (string.Compare(listViewItem.SubItems[0].Text, _selectedItem.SubItems[0].Text, StringComparison.Ordinal) == 0)
                    {
                        listViewItem.Selected = true;
                        break;
                    }
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
                detectedVehicles = edInterface.DetectedVehicles("auto:all");
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

        public void UpdateButtonStatus()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action) UpdateButtonStatus);
                return;
            }
            if (_test == null)
            {
                return;
            }

            bool searching = _searching || _vehicleTaskActive;
            comboBoxLanguage.Enabled = !searching && !_test.ThreadActive;
            buttonSearch.Enabled = !searching && !_test.ThreadActive;
            buttonClose.Enabled = !searching && !_test.ThreadActive;

            BluetoothDeviceInfo devInfo = GetSelectedBtDevice();
            WlanInterface wlanIface = GetSelectedWifiDevice();
            EdInterfaceEnet.EnetConnection enetConnection = GetSelectedEnetDevice();
            AccessPoint ap = GetSelectedAp();
            buttonTest.Enabled = buttonSearch.Enabled && ((devInfo != null) || (wlanIface != null) || (ap != null)) && !_test.ThreadActive;

            bool allowPatch = false;
            if (!searching)
            {
                if (enetConnection != null)
                {
                    allowPatch = true;
                }
                else
                {
                    allowPatch = buttonTest.Enabled && _test.TestOk && ((wlanIface != null) || (devInfo != null));
                }
            }

            bool allowRestore = !searching && !_test.ThreadActive;

            bool bmwValid = Patch.IsValid(_ediabasDirBmw);
            groupBoxEdiabas.Enabled = bmwValid;
            buttonPatchEdiabas.Enabled = bmwValid && allowPatch;
            buttonRestoreEdiabas.Enabled = bmwValid && allowRestore && Patch.IsPatched(_ediabasDirBmw);

            bool vagValid = Patch.IsValid(_ediabasDirVag);
            groupBoxVasPc.Enabled = vagValid;
            buttonPatchVasPc.Enabled = vagValid && allowPatch && (devInfo != null);
            buttonRestoreVasPc.Enabled = vagValid && allowRestore && Patch.IsPatched(_ediabasDirVag);

            bool istadValid = Patch.IsValid(_ediabasDirIstad);
            groupBoxIstad.Enabled = true;
            buttonDirIstad.Enabled = allowRestore;
            buttonPatchIstad.Enabled = istadValid && allowPatch;
            buttonRestoreIstad.Enabled = istadValid && allowRestore && Patch.IsPatched(_ediabasDirIstad);

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
        }

        public void ShowSearchEndMessage()
        {
            if (!_searching && !_vehicleTaskActive)
            {
                UpdateStatusText(listViewDevices.Items.Count > 0 ? Resources.Strings.DevicesFound : Resources.Strings.DevicesNotFound);
            }
        }

        public void UpdateStatusText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action) (() =>
                {
                    UpdateStatusText(text);
                }));
                return;
            }
            string message = text;
            if (!string.IsNullOrEmpty(_initMessage))
            {
                message = _initMessage + "\r\n" + text;
            }
            textBoxStatus.Text = message;
            textBoxStatus.SelectionStart = textBoxStatus.TextLength;
            textBoxStatus.Update();
            textBoxStatus.ScrollToCaret();
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

        private void FormMain_Shown(object sender, EventArgs e)
        {
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
            if (devInfo == null && wlanIface == null && enetConnection == null)
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
            }
            if (!string.IsNullOrEmpty(dirName))
            {
                StringBuilder sr = new StringBuilder();
                Patch.PatchEdiabas(sr, patchType, _test.AdapterType, dirName, devInfo, wlanIface, enetConnection, textBoxBluetoothPin.Text);
                UpdateStatusText(sr.ToString());
            }
            UpdateButtonStatus();
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            string dirName = null;
            if (sender == buttonRestoreEdiabas)
            {
                dirName = _ediabasDirBmw;
            }
            else if (sender == buttonRestoreVasPc)
            {
                dirName = _ediabasDirVag;
            }
            else if (sender == buttonRestoreIstad)
            {
                dirName = _ediabasDirIstad;
            }
            if (!string.IsNullOrEmpty(dirName))
            {
                StringBuilder sr = new StringBuilder();
                Patch.RestoreEdiabas(sr, dirName);
                UpdateStatusText(sr.ToString());
            }
            UpdateButtonStatus();
        }

        private void buttonDirIstad_Click(object sender, EventArgs e)
        {
            openFileDialogConfigFile.InitialDirectory = _ediabasDirIstad??string.Empty;
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
    }
}
