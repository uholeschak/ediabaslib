using InTheHand.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarSimulator
{
    public partial class BluetoothSearch : Form
    {
        public class BluetoothItem : IComparable<BluetoothItem>, IEquatable<BluetoothItem>
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
            private int? hashCode;

            public string DeviceType {
                get
                {
                    if (DeviceInfo != null)
                    {
                        return "EDR";
                    }

                    if (Device != null)
                    {
                        return "BLE";
                    }

                    return string.Empty;
                }
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", Name, DeviceType);
            }

            public int CompareTo(BluetoothItem bluetoothItem)
            {
                if (bluetoothItem == null)
                {
                    return 0;
                }

                int result = string.Compare(Address, bluetoothItem.Address, StringComparison.Ordinal);
                if (result != 0)
                {
                    return result;
                }

                result = string.Compare(DeviceType, bluetoothItem.DeviceType, StringComparison.Ordinal);
                if (result != 0)
                {
                    return result;
                }

                return 0;
            }

            public override bool Equals(object obj)
            {
                BluetoothItem bluetoothItem = obj as BluetoothItem;
                if ((object)bluetoothItem == null)
                {
                    return false;
                }

                return Equals(bluetoothItem);
            }

            public bool Equals(BluetoothItem bluetoothItem)
            {
                if ((object)bluetoothItem == null)
                {
                    return false;
                }

                return string.Compare(Address, bluetoothItem.Address, StringComparison.Ordinal) == 0 &&
                       string.Compare(DeviceType, bluetoothItem.DeviceType, StringComparison.Ordinal) == 0;
            }

            public override int GetHashCode()
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                if (!hashCode.HasValue)
                {
                    hashCode = ToString().GetHashCode();
                }

                return hashCode.Value;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }

            public static bool operator == (BluetoothItem lhs, BluetoothItem rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return Object.Equals(lhs, rhs);
                }

                return lhs.Equals(rhs);
            }

            public static bool operator != (BluetoothItem lhs, BluetoothItem rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return !Object.Equals(lhs, rhs);
                }

                return !(lhs == rhs);
            }
        }

        private readonly BluetoothClient _cli;
        private readonly List<BluetoothItem> _deviceList;
        private object _searchLock = new object();
        private volatile bool _searchingBt;
        private volatile bool _searchingLe;
        private volatile string _errorText;
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
            lock (_searchLock)
            {
                if (_searchingBt || _searchingLe)
                {
                    return false;
                }
            }

            if (_cli == null)
            {
                return false;
            }

            UpdateDeviceList(null, true);
            _errorText = null;

            try
            {
                _deviceList.Clear();

                Task<IReadOnlyCollection<BluetoothDevice>> scanTask = Bluetooth.ScanForDevicesAsync();
                lock (_searchLock)
                {
                    _searchingLe = true;
                }

                scanTask.ContinueWith(t =>
                {
                    lock (_searchLock)
                    {
                        _searchingLe = false;
                    }
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

                IAsyncEnumerable<BluetoothDeviceInfo> devices = _cli.DiscoverDevicesAsync();
                lock (_searchLock)
                {
                    _searchingBt = true;
                }

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

                        lock (_searchLock)
                        {
                            _searchingBt = false;
                        }
                        UpdateButtonStatus();
                        ShowSearchEndMessage();
                    }
                    catch (Exception ex)
                    {
                        lock (_searchLock)
                        {
                            _searchingBt = false;
                        }
                        UpdateButtonStatus();
                        ShowSearchEndMessage(string.Format("Searching failed: {0}", ex.Message));
                    }
                });

                UpdateStatusText("Searching ...");
                UpdateButtonStatus();
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
                    foreach (BluetoothItem device in devices.Order())
                    {
                        for (int i = 0; i < _deviceList.Count; i++)
                        {
                            if (_deviceList[i] == device)
                            {
                                _deviceList.RemoveAt(i);
                                i--;
                            }
                        }
                        _deviceList.Add(device);
                    }
                }

                foreach (BluetoothItem device in _deviceList.Order())
                {
                    ListViewItem listViewItem =
                        new ListViewItem(new[] { device.Address, device.Name, device.DeviceType })
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
                    if (listViewItem.Tag == null || _selectedItem.Tag == null)
                    {
                        continue;
                    }

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

        public BluetoothItem GetSelectedBtDevice()
        {
            BluetoothItem devInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                devInfo = listViewDevices.SelectedItems[0].Tag as BluetoothItem;
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
            BluetoothItem devInfo = GetSelectedBtDevice();
            buttonSearch.Enabled = !searching && _cli != null;
            buttonCancel.Enabled = searching;
            buttonOk.Enabled = buttonSearch.Enabled && devInfo != null;
        }

        public void ShowSearchEndMessage(string errorMessage = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ShowSearchEndMessage(errorMessage)));
                return;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                UpdateStatusText(errorMessage);
                _errorText = errorMessage;
            }

            lock (_searchLock)
            {
                if (_searchingBt || _searchingLe)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(_errorText))
            {
                UpdateStatusText(_errorText);
            }
            else
            {
                UpdateStatusText(listViewDevices.Items.Count > 0 ? "Devices found" : "No devices found");
            }

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

        private void BluetoothSearch_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cli?.Dispose();
        }

        private void BluetoothSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (_searchLock)
            {
                if (_searchingBt || _searchingLe)
                {
                    e.Cancel = true;
                }
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
            listViewDevices.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
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
            lock (_searchLock)
            {
                if (_searchingBt || _searchingLe)
                {
                    DialogResult = DialogResult.None;
                }
            }
        }
    }
}
