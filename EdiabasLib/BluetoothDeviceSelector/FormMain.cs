using System;
using System.Linq;
using System.Windows.Forms;
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
                IAsyncResult ar = _cli.BeginDiscoverDevices(1000, true, false, true, true, delegate(IAsyncResult result)
                {
                    BluetoothClient thisDevice = result.AsyncState as BluetoothClient;
                    if (result.IsCompleted)
                    {
                        if (thisDevice != null)
                        {
                            //Get the list of obtained devices and end the discovery process
                            BluetoothDeviceInfo[] devices = thisDevice.EndDiscoverDevices(result);
                            //Do what is required with the array of devices
                            BeginInvoke((Action) (() =>
                            {
                                listViewDevices.BeginUpdate();
                                listViewDevices.Items.Clear();
                                try
                                {
                                    foreach (BluetoothDeviceInfo device in devices.OrderBy(dev => dev.DeviceAddress.ToString()))
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
                                listViewDevices.EndUpdate();
                                buttonSearch.Enabled = true;
                                buttonClose.Enabled = true;
                            }));
                        }
                    }
                }, _cli);
                if (ar == null)
                {
                    return false;
                }
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
    }
}
