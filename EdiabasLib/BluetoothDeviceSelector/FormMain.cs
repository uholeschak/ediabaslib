using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        private volatile bool _searching;
        private volatile Thread _testThread;
        private NetworkStream _btStream;

        private enum AdapterMode
        {
            CanOff = 0x00,
            Can500 = 0x01,
            Can100 = 0x09,
            CanAuto = 0xFF,
        }

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
            }
            _deviceList = new List<BluetoothDeviceInfo>();
            UpdateButtonStatus();
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
                    _searching = false;
                    UpdateButtonStatus();
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
                    }));
                };
                listViewDevices.Items.Clear();
                bco.DiscoverDevicesAsync(1000, true, false, true, true, bco);
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
        }

        private void UpdateButtonStatus()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action) UpdateButtonStatus);
                return;
            }
            buttonSearch.Enabled = !_searching && _cli != null;
            buttonClose.Enabled = !_searching;

            BluetoothDeviceInfo devInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                devInfo = listViewDevices.SelectedItems[0].Tag as BluetoothDeviceInfo;
            }
            buttonTest.Enabled = buttonSearch.Enabled && devInfo != null && _testThread == null;
            textBoxBluetoothPin.Enabled = _testThread == null;
            checkBoxAutoMode.Enabled = _testThread == null;
        }

        private bool ExecuteTest()
        {
            if (_testThread != null)
            {
                return false;
            }
            BluetoothDeviceInfo devInfo = null;
            if (listViewDevices.SelectedItems.Count > 0)
            {
                devInfo = listViewDevices.SelectedItems[0].Tag as BluetoothDeviceInfo;
            }
            if (devInfo == null)
            {
                return false;
            }
            string pin = textBoxBluetoothPin.Text;
            bool autoMode = checkBoxAutoMode.Checked;
            _testThread = new Thread(() =>
            {
                try
                {
                    UpdateStatusText(Strings.Connecting);
                    if (!ConnectBtDevice(devInfo, pin))
                    {
                        UpdateStatusText(Strings.ConnectionFailed);
                        return;
                    }
                    RunTest(autoMode);
                }
                finally
                {
                    DisconnectBtDevice();
                    _testThread = null;
                    UpdateButtonStatus();
                }
            });
            _testThread.Start();
            return true;
        }

        private bool RunTest(bool autoMode)
        {
            StringBuilder sr = new StringBuilder();

            sr.Append(Strings.Connected);
            byte[] firmware = AdapterCommandCustom(0xFD, new byte[] {0xFD});
            if ((firmware == null) || (firmware.Length < 4))
            {
                sr.Append("\r\n");
                sr.Append(Strings.ReadFirmwareVersionFailed);
                UpdateStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append(Strings.FirmwareVersion);
            sr.Append(string.Format(" {0}.{1}", firmware[2], firmware[3]));
            int version = (firmware[2] << 8) + firmware[3];
            if (version < 8)
            {
                sr.Append("\r\n");
                sr.Append(Strings.FirmwareTooOld);
                UpdateStatusText(sr.ToString());
                return false;
            }
            byte[] canMode = AdapterCommandCustom(0x82, new byte[] { 0x00 });
            if ((canMode == null) || (canMode.Length < 1))
            {
                sr.Append("\r\n");
                sr.Append(Strings.ReadModeFailed);
                UpdateStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append(Strings.CanMode);
            sr.Append(" ");
            switch ((AdapterMode) canMode[0])
            {
                case AdapterMode.CanOff:
                    sr.Append(Strings.CanModeOff);
                    break;

                case AdapterMode.Can500:
                    sr.Append(Strings.CanMode500);
                    break;

                case AdapterMode.Can100:
                    sr.Append(Strings.CanMode100);
                    break;

                case AdapterMode.CanAuto:
                    sr.Append(Strings.CanModeAuto);
                    break;

                default:
                    sr.Append(Strings.CanModeUnknown);
                    break;
            }
            if ((AdapterMode)canMode[0] != AdapterMode.CanAuto)
            {
                if (autoMode)
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.CanModeChangeAuto);
                    canMode = AdapterCommandCustom(0x02, new[] {(byte) AdapterMode.CanAuto});
                    if ((canMode == null) || (canMode.Length < 1) || ((AdapterMode) canMode[0] != AdapterMode.CanAuto))
                    {
                        sr.Append("\r\n");
                        sr.Append(Strings.CanModeChangeFailed);
                    }
                }
                else
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.CanModeNotAuto);
                }
            }
            sr.Append("\r\n");
            sr.Append(Strings.TestOk);
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
            if (InvokeRequired)
            {
                BeginInvoke((Action) (() =>
                {
                    UpdateStatusText(text);
                }));
                return;
            }
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
            UpdateButtonStatus();
        }
    }
}
