using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BluetoothDeviceSelector
{
    public partial class FormMain : Form
    {
        private readonly BluetoothClient _cli;
        private readonly List<BluetoothDeviceInfo> _deviceList;
        private NetworkStream _btStream;

        public FormMain()
        {
            InitializeComponent();
            listViewDevices.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.None);
            listViewDevices.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            textBoxBluetoothPin.Text = @"1234";
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

        private bool ExecuteTest()
        {
            BluetoothDeviceInfo devInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                devInfo = listViewDevices.SelectedItems[0].Tag as BluetoothDeviceInfo;
            }
            if (devInfo == null)
            {
                return false;
            }
            try
            {
                if (!ConnectBtDevice(devInfo, textBoxBluetoothPin.Text))
                {
                    return false;
                }
                RunTest();
            }
            finally
            {
                DisconnectBtDevice();
            }
            return true;
        }

        private bool RunTest()
        {
            StringBuilder sr = new StringBuilder();

            byte[] firmware = AdapterCommandCustom(0xFD, new byte[] {0xFD});
            if ((firmware == null) || (firmware.Length < 4))
            {
                sr.Append("Read adapter type failed!");
                UpdateStatusText(sr.ToString());
                return false;
            }
            sr.Append("Firmware: ");
            sr.Append(string.Format("{0}.{1}", firmware[2], firmware[3]));
            if ((firmware[2] != 0x00) || (firmware[3] != 0x08))
            {
                sr.Append("Incorrect firmware version!");
                UpdateStatusText(sr.ToString());
                return false;
            }
            UpdateStatusText(sr.ToString());
            return true;
        }

        private bool ConnectBtDevice(BluetoothDeviceInfo device, string pin)
        {
            try
            {
                BluetoothEndPoint ep = new BluetoothEndPoint(device.DeviceAddress, BluetoothService.SerialPort);
                BluetoothClient cli = new BluetoothClient();
                cli.SetPin(pin);
                cli.Connect(ep);
                _btStream = cli.GetStream();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DisconnectBtDevice()
        {
            if (_btStream != null)
            {
                _btStream.Close();
                _btStream.Dispose();
                _btStream = null;
            }
        }

        private byte[] AdapterCommandCustom(byte command, byte[] data)
        {
            if (_btStream == null)
            {
                return null;
            }
            byte[] request = new byte[4 + data.Length];
            request[0] = (byte)(0x81 + data.Length);
            request[1] = 0xF1;
            request[2] = 0xF1;
            request[3] = command;
            Array.Copy(data, 0, request, 4, data.Length);

            if (!SendBmwfast(request))
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

        private bool SendBmwfast(byte[] sendData)
        {
            if (_btStream == null)
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
                _btStream.Write(telBuffer, 0, sendLength);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private int ReceiveBmwFast(byte[] receiveData)
        {
            if (_btStream == null)
            {
                return 0;
            }
            try
            {
                // header byte
                _btStream.ReadTimeout = 1000;
                for (int i = 0; i < 4; i++)
                {
                    int data;
                    try
                    {
                        data = _btStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        while (_btStream.DataAvailable)
                        {
                            try
                            {
                                _btStream.ReadByte();
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                        return 0;
                    }
                    receiveData[i] = (byte)data;
                }

                if ((receiveData[0] & 0x80) != 0x80)
                {   // 0xC0: Broadcast
                    while (_btStream.DataAvailable)
                    {
                        try
                        {
                            _btStream.ReadByte();
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
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
                        data = _btStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        while (_btStream.DataAvailable)
                        {
                            try
                            {
                                _btStream.ReadByte();
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                        return 0;
                    }
                    receiveData[i + 4] = (byte)data;
                }

                if (CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                {
                    while (_btStream.DataAvailable)
                    {
                        try
                        {
                            _btStream.ReadByte();
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    return 0;
                }
                return recLength;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        static public byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
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

        private void textBoxBluetoothPin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            ExecuteTest();
        }
    }
}
