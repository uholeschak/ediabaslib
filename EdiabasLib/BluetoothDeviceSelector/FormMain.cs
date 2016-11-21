using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BluetoothDeviceSelector
{
    public partial class FormMain : Form
    {
        private readonly BluetoothClient _cli;
        private readonly List<BluetoothDeviceInfo> _deviceList;

        public FormMain()
        {
            InitializeComponent();
            listViewDevices.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listViewDevices.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            try
            {
                _cli = new BluetoothClient();
            }
            catch (Exception ex)
            {
                UpdateStatusText(string.Format(Strings.BtInitError, ex.Message));
                buttonSearch.Enabled = false;
            }
            _deviceList = new List<BluetoothDeviceInfo>();
        }

        private bool StartDeviceSearch()
        {
            if (_cli == null)
            {
                return false;
            }
            try
            {
                _deviceList.Clear();
                BluetoothComponent bco = new BluetoothComponent(_cli);
                bco.DiscoverDevicesProgress += (sender, args) =>
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (args.Error == null && !args.Cancelled)
                        {
                            UpdateDeviceList(args.Devices, false);
                        }
                    }));
                };

                bco.DiscoverDevicesComplete += (sender, args) =>
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (args.Error == null && !args.Cancelled)
                        {
                            UpdateDeviceList(args.Devices, true);
                            UpdateStatusText(string.Format(Strings.FoundDevices, args.Devices?.Length));
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
                        buttonSearch.Enabled = true;
                        buttonClose.Enabled = true;
                        UpdateButtonStatus();
                    }));
                };
                bco.DiscoverDevicesAsync(1000, true, false, true, true, bco);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void UpdateDeviceList(BluetoothDeviceInfo[] devices, bool completed)
        {
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
                    foreach (BluetoothDeviceInfo device in devices.OrderBy(dev => dev.DeviceAddress.ToString()))
                    {
                        bool found = _deviceList.Any(dev => device.DeviceAddress == dev.DeviceAddress);
                        if (!found)
                        {
                            _deviceList.Add(device);
                        }
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
        }

        private void UpdateButtonStatus()
        {
            BluetoothDeviceInfo devInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                devInfo = listViewDevices.SelectedItems[0].Tag as BluetoothDeviceInfo;
            }
            buttonTest.Enabled = buttonSearch.Enabled && devInfo != null;
        }

        private void UpdateStatusText(string text)
        {
            textBoxStatus.Text = text;
            textBoxStatus.SelectionStart = textBoxStatus.TextLength;
            textBoxStatus.Update();
            textBoxStatus.ScrollToCaret();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (!buttonSearch.Enabled)
            {
                return;
            }
            if (StartDeviceSearch())
            {
                listViewDevices.Items.Clear();
                UpdateStatusText(Strings.Searching);
                buttonSearch.Enabled = false;
                buttonClose.Enabled = false;
                UpdateButtonStatus();
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cli?.Dispose();
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
            buttonSearch_Click(buttonSearch, EventArgs.Empty);
        }

        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStatus();
        }
    }
}
