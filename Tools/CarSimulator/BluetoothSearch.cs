using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Bluetooth;
using InTheHand.Net.Sockets;

namespace CarSimulator
{
    public partial class BluetoothSearch : Form
    {
        public class BluetoothItem
        {
            public BluetoothItem(BluetoothDeviceInfo deviceInfo)
            {
                DeviceInfo = deviceInfo;
                Device = null;
                Address = deviceInfo.DeviceAddress.ToString();
                Name = deviceInfo.DeviceName;
            }

            public BluetoothItem(BluetoothDevice device)
            {
                Device = device;
                DeviceInfo = null;
                Address = device.Id;
                Name = device.Name;
            }

            public BluetoothDeviceInfo DeviceInfo { get; }
            public BluetoothDevice Device { get; }
            public string Address { get; set; }
            public string Name { get; set; }
        }

        private readonly BluetoothClient _cli;
#if BT3
        private InTheHand.Net.Bluetooth.Factory.IBluetoothClient _icli;
#endif
        private readonly List<BluetoothItem> _deviceList;
        private volatile bool _searchingBt;
        private volatile bool _searchingLe;
        private ListViewItem _selectedItem;
        private bool _ignoreSelection;
        private bool _autoSelect;
        private readonly List<string> _autoSelectNames;

        public BluetoothSearch(List<string> autoSelectNames = null)
        {
            InitializeComponent();
            _autoSelectNames = autoSelectNames;
            ActiveControl = buttonOk;
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
                UpdateStatusText(ex.Message);
            }
            _deviceList = new List<BluetoothItem>();
            UpdateButtonStatus();
        }

        private bool StartDeviceSearch()
        {
            if (_searchingBt || _searchingLe)
            {
                return false;
            }

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
            UpdateDeviceList(null, true);

            try
            {
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
                                UpdateStatusText(listViewDevices.Items.Count > 0 ? "Devices found" : "No devices found");
                            }));
                        }
                        catch (Exception ex)
                        {
                            UpdateStatusText(string.Format("Searching failed: {0}", ex.Message));
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
                UpdateStatusText("Searching ...");
                UpdateButtonStatus();
#else
#if true
                Task<IReadOnlyCollection<BluetoothDevice>> scanTask = Bluetooth.ScanForDevicesAsync();
                _searchingLe = true;
                scanTask.ContinueWith(t =>
                {
                    _searchingLe = false;
                    UpdateButtonStatus();
                    if (t.IsCompletedSuccessfully)
                    {
                        BluetoothDevice[] devices = t.Result.ToArray();
                        BeginInvoke((Action)(() =>
                        {
                            UpdateDeviceList(devices.Select(d => new BluetoothItem(d)).ToArray(), false);
                            ShowSearchEndMessage();
                        }));
                    }
                    else if (t.IsFaulted)
                    {
                        ShowSearchEndMessage(string.Format("Searching failed: {0}", t.Exception?.GetBaseException().Message));
                    }
                });
#endif
                IAsyncEnumerable<BluetoothDeviceInfo> devices = _cli.DiscoverDevicesAsync();
                _searchingBt = true;
                Task.Run(async () =>
                {
                    try
                    {
                        await foreach (BluetoothDeviceInfo device in devices)
                        {
                            BeginInvoke((Action)(() =>
                            {
                                UpdateDeviceList(new[] { new BluetoothItem(device) }, false);
                            }));
                        }

                        _searchingBt = false;
                        UpdateButtonStatus();
                        ShowSearchEndMessage();
                    }
                    catch (Exception ex)
                    {
                        _searchingBt = false;
                        UpdateButtonStatus();
                        ShowSearchEndMessage(string.Format("Searching failed: {0}", ex.Message));
                    }
                });

                UpdateStatusText("Searching ...");
                UpdateButtonStatus();
#endif
            }
            catch (Exception)
            {
                UpdateStatusText("Searching failed");
                return false;
            }
            return true;
        }

        private void UpdateDeviceList(BluetoothItem[] devices, bool completed)
        {
            _ignoreSelection = true;
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();
            if (devices != null)
            {
                if (completed)
                {
                    _deviceList.Clear();
                    _deviceList.AddRange(devices);
                }
                else
                {
                    foreach (BluetoothItem device in devices.OrderBy(dev => dev.Address))
                    {
                        for (int i = 0; i < _deviceList.Count; i++)
                        {
                            if (_deviceList[i].Address == device.Address)
                            {
                                _deviceList.RemoveAt(i);
                                i--;
                            }
                        }
                        _deviceList.Add(device);
                    }
                }

                foreach (BluetoothItem device in _deviceList.OrderBy(dev => dev.Address))
                {
                    ListViewItem listViewItem =
                        new ListViewItem(new[] { device.Address, device.Name })
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

                    if (listViewItem.SubItems.Count > 0)
                    {
                        if (string.Compare(listViewItem.SubItems[0].Text, _selectedItem.SubItems[0].Text, StringComparison.Ordinal) == 0)
                        {
                            listViewItem.Selected = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (ListViewItem listViewItem in listViewDevices.Items)
                {
                    if (listViewItem.SubItems.Count > 1)
                    {
                        string deviceName = listViewItem.SubItems[1].Text;
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.ToUpperInvariant();
                            if (_autoSelectNames != null)
                            {
                                foreach (string selectName in _autoSelectNames)
                                {
                                    if (deviceName.Contains(selectName))
                                    {
                                        listViewItem.Selected = true;
                                        _autoSelect = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if (listViewDevices.SelectedItems.Count == 0)
            {
                if (listViewDevices.Items.Count > 0)
                {
                    listViewDevices.Items[0].Selected = true;
                }
            }

            listViewDevices.EndUpdate();
            _ignoreSelection = false;
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

        public void UpdateButtonStatus()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action) UpdateButtonStatus);
                return;
            }

            bool searching = _searchingBt || _searchingLe;
            BluetoothDeviceInfo devInfo = GetSelectedBtDevice();
            buttonSearch.Enabled = !searching && _cli != null;
            buttonCancel.Enabled = searching;
            buttonOk.Enabled = buttonSearch.Enabled && devInfo != null;
        }

        public void ShowSearchEndMessage(string errorMessage = null)
        {
            if (_searchingBt || _searchingLe)
            {
                return;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                UpdateStatusText(errorMessage);
                return;
            }

            UpdateStatusText(listViewDevices.Items.Count > 0 ? "Devices found" : "No devices found");
            if (_autoSelect)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public void UpdateStatusText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateStatusText(text);
                }));
                return;
            }
            string message = text;
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

        private bool IsWinVistaOrHigher()
        {
            OperatingSystem os = Environment.OSVersion;
            return (os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6);
        }

        private void BluetoothSearch_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cli?.Dispose();
        }

        private void BluetoothSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_searchingBt || _searchingLe)
            {
                e.Cancel = true;
            }
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
            UpdateButtonStatus();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void BluetoothSearch_Shown(object sender, EventArgs e)
        {
            listViewDevices.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
            UpdateButtonStatus();
            PerformSearch();
        }

        private void listViewDevices_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            UpdateButtonStatus();
            PerformSearch();
        }

        private void listViewDevices_DoubleClick(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count == 1)
            {
                if (buttonOk.Enabled)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_searchingBt || _searchingLe)
            {
                DialogResult = DialogResult.None;
            }
        }
    }
}
