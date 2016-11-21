using System;
using System.Linq;
using System.Windows.Forms;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BluetoothDeviceSelector
{
    public partial class FormMain : Form
    {
        private readonly BluetoothClient _cli;

        public FormMain()
        {
            InitializeComponent();
            listViewDevices.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listViewDevices.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            _cli = new BluetoothClient();
        }

        private bool StartDeviceSearch()
        {
            try
            {
                BluetoothComponent bco = new BluetoothComponent(_cli);
                bco.DiscoverDevicesProgress += (sender, args) =>
                {
                };

                bco.DiscoverDevicesComplete += (sender, args) =>
                {
                    BeginInvoke((Action)(() =>
                    {
                        listViewDevices.BeginUpdate();
                        listViewDevices.Items.Clear();
                        if (args.Error == null)
                        {
                            try
                            {
                                foreach (BluetoothDeviceInfo device in args.Devices.OrderBy(dev => dev.DeviceAddress.ToString()))
                                {
                                    ListViewItem listViewItem =
                                        new ListViewItem(new[] { device.DeviceAddress.ToString(), device.DeviceName })
                                        {
                                            Tag = device
                                        };
                                    listViewDevices.Items.Add(listViewItem);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        else
                        {
                            listViewDevices.Items.Add(new ListViewItem(new[] { "Searching failed", args.Error.Message }));
                        }
                        listViewDevices.EndUpdate();
                        buttonSearch.Enabled = true;
                        buttonClose.Enabled = true;
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
                listViewDevices.BeginUpdate();
                listViewDevices.Items.Clear();
                listViewDevices.Items.Add(new ListViewItem(new[] { "Searching ...", string.Empty }));
                listViewDevices.EndUpdate();
                buttonSearch.Enabled = false;
                buttonClose.Enabled = false;
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cli.Dispose();
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
            buttonSearch_Click(buttonSearch, EventArgs.Empty);
        }
    }
}
