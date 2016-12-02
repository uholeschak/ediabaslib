using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.Win32;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace EdiabasLibConfigTool
{
    public partial class FormMain : Form
    {
        private readonly BluetoothClient _cli;
        private readonly List<BluetoothDeviceInfo> _deviceList;
        private readonly Wifi _wifi;
        private readonly WlanClient _wlanClient;
        private readonly Test _test;
        private readonly string _ediabasDirBmw;
        private readonly string _ediabasDirVag;
        private string _initMessage;
        private volatile bool _searching;

        private const string AdapterSsid = @"Deep OBD BMW";

        public string BluetoothPin => textBoxBluetoothPin.Text;
        public string WifiPassword => textBoxWifiPassword.Text;

        public FormMain()
        {
            InitializeComponent();
            Icon = Properties.Resources.AppIcon;
            listViewDevices.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listViewDevices.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            textBoxBluetoothPin.Text = @"1234";
            textBoxWifiPassword.Text = @"deepobdbmw";
            StringBuilder sr = new StringBuilder();
            try
            {
                _cli = new BluetoothClient();
            }
            catch (Exception ex)
            {
                sr.Append(string.Format(Strings.BtInitError, ex.Message));
            }
            _deviceList = new List<BluetoothDeviceInfo>();
            _wifi = new Wifi();
            _wlanClient = new WlanClient();
            _test = new Test(this);
            if (_wifi.NoWifiAvailable || _wlanClient.NoWifiAvailable)
            {
                if (sr.Length > 0)
                {
                    sr.Append("\r\n");
                }
                sr.Append(Strings.WifiAdapterError);
            }
            _ediabasDirBmw = Environment.GetEnvironmentVariable("ediabas_config_dir");
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Softing\EDIS-VW2"))
                {
                    string path = key?.GetValue("EDIABASPath", null) as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        _ediabasDirVag = Path.Combine(path, @"bin");
                    }
                }
                if (string.IsNullOrEmpty(_ediabasDirVag))
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\SIDIS\ENV"))
                    {
                        _ediabasDirVag = key?.GetValue("FLASHINIPATH", null) as string;
                    }
                }
                if (string.IsNullOrEmpty(_ediabasDirVag))
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Softing\VASEGD2"))
                    {
                        _ediabasDirVag = key?.GetValue("strEdiabasApi32Path", null) as string;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _initMessage = sr.ToString();
            UpdateStatusText(string.Empty);
            UpdateButtonStatus();
        }

        private bool IsWinVistaOrHigher()
        {
            OperatingSystem os = Environment.OSVersion;
            return (os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6);
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
                        if (string.Compare(ssidString, AdapterSsid, StringComparison.OrdinalIgnoreCase) == 0)
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
                        if (string.Compare(ap.Name, AdapterSsid, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            ListViewItem listViewItem =
                                new ListViewItem(new[] { Strings.DisconnectedAdapter, ap.Name })
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

        private bool StartDeviceSearch()
        {
            UpdateDeviceList(null, true);
            if (_cli == null)
            {
                UpdateStatusText(listViewDevices.Items.Count > 0 ? Strings.DevicesFound : Strings.DevicesNotFound);
                return false;
            }
            try
            {
                _test.TestOk = false;
                _test.ConfigPossible = false;
                _deviceList.Clear();
                BluetoothComponent bco = new BluetoothComponent(_cli);
                bco.DiscoverDevicesProgress += (sender, args) =>
                {
                    if (args.Error == null && !args.Cancelled && args.Devices != null)
                    {
                        try
                        {
                            foreach (BluetoothDeviceInfo device in args.Devices)
                            {
                                device.Refresh();
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        BeginInvoke((Action)(() =>
                        {
                            UpdateDeviceList(args.Devices, false);
                        }));
                    }
                };

                bco.DiscoverDevicesComplete += (sender, args) =>
                {
                    _searching = false;
                    UpdateButtonStatus();
                    BeginInvoke((Action)(() =>
                    {
                        if (args.Error == null && !args.Cancelled)
                        {
                            UpdateDeviceList(args.Devices, true);
                            UpdateStatusText(listViewDevices.Items.Count > 0 ? Strings.DevicesFound : Strings.DevicesNotFound);
                        }
                        else
                        {
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (args.Error != null)
                            {
                                UpdateStatusText(string.Format(Strings.SearchingFailedMessage, args.Error.Message));
                            }
                            else
                            {
                                UpdateStatusText(Strings.SearchingFailed);
                            }
                        }
                    }));
                };
                bco.DiscoverDevicesAsync(1000, true, false, true, IsWinVistaOrHigher(), bco);
                _searching = true;
                UpdateStatusText(Strings.Searching);
                UpdateButtonStatus();
            }
            catch (Exception)
            {
                UpdateStatusText(Strings.SearchingFailed);
                return false;
            }
            return true;
        }

        private void UpdateDeviceList(BluetoothDeviceInfo[] devices, bool completed)
        {
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();
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
            listViewDevices.EndUpdate();
            UpdateButtonStatus();
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
            buttonSearch.Enabled = !_searching && !_test.ThreadActive && ((_cli != null) || !_wlanClient.NoWifiAvailable);
            buttonClose.Enabled = !_searching && !_test.ThreadActive;

            BluetoothDeviceInfo devInfo = GetSelectedBtDevice();
            WlanInterface wlanIface = GetSelectedWifiDevice();
            AccessPoint ap = GetSelectedAp();
            buttonTest.Enabled = buttonSearch.Enabled && ((devInfo != null) || (wlanIface != null) || (ap != null)) && !_test.ThreadActive;
            buttonUpdateConfigFile.Enabled = buttonTest.Enabled && _test.TestOk && ((wlanIface != null) || (devInfo != null));
            textBoxBluetoothPin.Enabled = !_test.ThreadActive;
            textBoxWifiPassword.Enabled = !_test.ThreadActive;
            if ((devInfo != null) || (wlanIface != null))
            {
                if (_test.TestOk && _test.ConfigPossible)
                {
                    buttonTest.Text = Strings.ButtonTestConfiguration;
                }
                else
                {
                    buttonTest.Text = Strings.ButtonTestCheck;
                }
            }
            else if (ap != null)
            {
                buttonTest.Text = Strings.ButtonTestConnect;
            }
            else
            {
                buttonTest.Text = Strings.ButtonTestCheck;
            }
        }

        private void UpdateConfigNode(XElement settingsNode, string key, string value, bool onlyExisting = false)
        {
            XElement node = (from addNode in settingsNode.Elements("add")
                    let keyAttrib = addNode.Attribute("key") where keyAttrib != null
                    where string.Compare(keyAttrib.Value, key, StringComparison.OrdinalIgnoreCase) == 0
                    select addNode).FirstOrDefault();
            if (node == null)
            {
                if (onlyExisting)
                {
                    return;
                }
                node = new XElement("add");
                node.Add(new XAttribute("key", key));
                settingsNode.AddFirst(node);
            }
            XAttribute valueAttrib = node.Attribute("value");
            if (valueAttrib == null)
            {
                valueAttrib = new XAttribute("value", value);
                node.Add(valueAttrib);
            }
            else
            {
                valueAttrib.Value = value;
            }
        }

        private bool UpdateConfigFile(string fileName, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, string pin)
        {
            try
            {
                XDocument xDocument = XDocument.Load(fileName);
                XElement settingsNode = xDocument.Root?.Element("appSettings");
                if (settingsNode == null)
                {
                    return false;
                }
                if (wlanIface != null)
                {
                    UpdateConfigNode(settingsNode, @"EnetRemoteHost", @"auto:all");
                    UpdateConfigNode(settingsNode, @"Interface", @"ENET");
                }
                else if (devInfo != null)
                {
                    string interfaceValue = @"STD:OBD";
                    if (fileName.ToLowerInvariant().Contains(@"\SIDIS\home\DBaseSys2\".ToLowerInvariant()))
                    {   // VAS-PC instalation
                        interfaceValue = @"EDIC";
                    }
                    string portValue = string.Format("BLUETOOTH:{0}#{1}", devInfo.DeviceAddress, pin);

                    UpdateConfigNode(settingsNode, @"ObdComPort", portValue);
                    UpdateConfigNode(settingsNode, @"Interface", interfaceValue);
                }
                else
                {
                    return false;
                }
                xDocument.Save(fileName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void ClearInitMessage()
        {
            _initMessage = string.Empty;
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
            _cli?.Dispose();
            _test?.Dispose();
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

        private void buttonUpdateConfigFile_Click(object sender, EventArgs e)
        {
            ClearInitMessage();
            BluetoothDeviceInfo devInfo = GetSelectedBtDevice();
            WlanInterface wlanIface = GetSelectedWifiDevice();
            if (devInfo == null && wlanIface == null)
            {
                return;
            }
            string initDir = !string.IsNullOrEmpty(_ediabasDirBmw) ? _ediabasDirBmw : _ediabasDirVag;
            openFileDialogConfigFile.InitialDirectory = initDir ?? string.Empty;
            if (openFileDialogConfigFile.ShowDialog() == DialogResult.OK)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (UpdateConfigFile(openFileDialogConfigFile.FileName, devInfo, wlanIface, textBoxBluetoothPin.Text))
                {
                    UpdateStatusText(Strings.ConfigUpdateOk);
                }
                else
                {
                    UpdateStatusText(Strings.ConfigUpdateFailed);
                }
            }
        }
    }
}
