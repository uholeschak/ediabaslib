using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace CarSimulator
{
    public class DeviceTest : IDisposable
    {
        private readonly Wifi _wifi;
        private readonly WlanClient _wlanClient;
        private readonly MainForm _form;
        private BluetoothClient _btClient;
        private NetworkStream _dataStream;
        private bool _disposed;

        private const string DefaultBtName = "Deep OBD";
        private const string DefaultBtPin = "1234";
        public const string AdapterSsidElm = @"WiFi_OBDII";
        private const string ElmIp = @"192.168.0.10";
        private const int ElmPort = 35000;

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
        // ReSharper disable MemberCanBePrivate.Global
        public const byte KLINEF1_PARITY_MASK = 0x7;
        public const byte KLINEF1_PARITY_NONE = 0x0;
        public const byte KLINEF1_PARITY_EVEN = 0x1;
        public const byte KLINEF1_PARITY_ODD = 0x2;
        public const byte KLINEF1_PARITY_MARK = 0x3;
        public const byte KLINEF1_PARITY_SPACE = 0x4;
        public const byte KLINEF1_USE_LLINE = 0x08;
        public const byte KLINEF1_SEND_PULSE = 0x10;
        public const byte KLINEF1_NO_ECHO = 0x20;
        public const byte KLINEF1_FAST_INIT = 0x40;
        public const byte KLINEF1_USE_KLINE = 0x80;

        public const byte KLINEF2_KWP1281_DETECT = 0x01;

        public const byte KWP1281_TIMEOUT = 60;
        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Global
        // ReSharper restore MemberCanBePrivate.Global

        public DeviceTest(MainForm form)
        {
            _wifi = new Wifi();
            _wlanClient = new WlanClient();

            _form = form;
            _form.UpdateTestStatusText(string.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    DisconnectStream();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        private BluetoothDeviceInfo DiscoverBtDevice()
        {
            try
            {
#if true
                BluetoothSearch dlgSearch = new BluetoothSearch();
                DialogResult result = dlgSearch.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return null;
                }
                return dlgSearch.GetSelectedBtDevice();
#else
                SelectBluetoothDeviceDialog dlg = new SelectBluetoothDeviceDialog
                {
                    ShowAuthenticated = true,
                    ShowRemembered = false,
                    ShowUnknown = true
                };
                dlg.ClassOfDevices.Clear();
                dlg.ClassOfDevices.Add(new ClassOfDevice(DeviceClass.Uncategorized, ServiceClass.None));
                DialogResult result = dlg.ShowDialog(_form);
                if (result != DialogResult.OK)
                {
                    return null;
                }
                return dlg.SelectedDevice;
#endif
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool ConnectBtDevice(BluetoothDeviceInfo device)
        {
            try
            {
                BluetoothSecurity.SetPin(device.DeviceAddress, DefaultBtPin);
                BluetoothEndPoint ep = new BluetoothEndPoint(device.DeviceAddress, BluetoothService.SerialPort);
                _btClient = new BluetoothClient();
                _btClient.SetPin(DefaultBtPin);
                try
                {
                    _btClient.Connect(ep);
                }
                catch (Exception)
                {
                    BluetoothSecurity.RemoveDevice(device.DeviceAddress);
                    Thread.Sleep(1000);
                    _btClient.Connect(ep);
                }
                _dataStream = _btClient.GetStream();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ConnectWifiDevice(string comPort)
        {
            try
            {
                foreach (WlanInterface wlanIface in _wlanClient.Interfaces)
                {
                    if (wlanIface.InterfaceState == WlanInterfaceState.Connected)
                    {
                        WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                        string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                        if (string.Compare(ssidString, AdapterSsidElm, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
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
                        if (string.Compare(ap.Name, AdapterSsidElm, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            AuthRequest authRequest = new AuthRequest(ap);
                            ap.ConnectAsync(authRequest, true, success =>
                            {
                                _form.BeginInvoke((Action)(() =>
                                {
                                    if (!success)
                                    {
                                        _form.UpdateTestStatusText("Connection faild");
                                    }
                                    else
                                    {
                                        _form.UpdateTestStatusText("Wifi connected");
                                        Thread.Sleep(1000);
                                        ExecuteTest(true, comPort);
                                    }
                                }));
                            });
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            _form.UpdateTestStatusText("No WiFi device found");

            return false;
        }

        private void DisconnectStream()
        {
            if (_dataStream != null)
            {
                _dataStream.Close();
                _dataStream.Dispose();
                _dataStream = null;
            }

            if (_btClient != null)
            {
                _btClient.Close();
                _btClient.Dispose();
                _btClient = null;
            }
        }

        public bool ExecuteTest(bool wifi, string comPort)
        {
            if (!comPort.StartsWith("COM"))
            {
                _form.UpdateTestStatusText("No COM port selected");
                return false;
            }

            try
            {
                if (wifi)
                {
                    _form.UpdateTestStatusText("Connecting ...");
                    if (!ConnectWifiDevice(comPort))
                    {
                        return false;
                    }
                    try
                    {
                        using (TcpClient tcpClient = new TcpClient())
                        {
                            IPEndPoint ipTcp = new IPEndPoint(IPAddress.Parse(ElmIp), ElmPort);
                            tcpClient.Connect(ipTcp);
                            _dataStream = tcpClient.GetStream();
                            if (!RunTest(comPort))
                            {
                                return false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _form.UpdateTestStatusText("Connection faild");
                        return false;
                    }
                }
                else
                {
                    _form.UpdateTestStatusText("Discovering devices ...");
                    BluetoothDeviceInfo device = DiscoverBtDevice();
                    if (device == null)
                    {
                        _form.UpdateTestStatusText("No device selected");
                        return false;
                    }
                    _form.UpdateTestStatusText("Connecting ...");
                    if (!ConnectBtDevice(device))
                    {
                        _form.UpdateTestStatusText("Connection faild");
                        return false;
                    }
                    if (!RunTest(comPort))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                DisconnectStream();
            }
            return true;
        }

        private bool RunTest(string comPort)
        {
            StringBuilder sr = new StringBuilder();

            _form.commThread.StopThread();

            byte[] firmware = AdapterCommandCustom(0xFD, new byte[] { 0xFD });
            if ((firmware == null) || (firmware.Length < 4))
            {
                sr.Append("Read firmware version failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            int adapterType = firmware[0] << 8 | firmware[1];
            string adapterName;
            switch (adapterType)
            {
                case 2:
                    adapterName = "Default";
                    break;

                case 3:
                    adapterName = "BC04";
                    break;

                case 4:
                    adapterName = "HC04";
                    break;

                case 5:
                    adapterName = "SPP-UART";
                    break;

                default:
                    sr.Append(string.Format("Invalid adapter type: {0}", adapterType));
                    _form.UpdateTestStatusText(sr.ToString());
                    return false;
            }
            sr.Append("Type: ");
            sr.Append(adapterName);
            sr.Append("\r\n");
            sr.Append("Firmware: ");
            sr.Append(string.Format("{0}.{1}", firmware[2], firmware[3]));
            if ((firmware[2] != 0x00) || (firmware[3] < 0x09))
            {
                sr.Append("\r\n");
                sr.Append("Incorrect firmware version!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }

            byte[] btName = AdapterCommandCustom(0x85, new byte[] { 0x85 });
            if (btName == null)
            {
                sr.Append("\r\n");
                sr.Append("Read name failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            if (btName.Length > 0)
            {
                sr.Append("\r\n");
                sr.Append("Name: ");
                int nameLength = btName.TakeWhile(value => value != 0x00).Count();
                string nameText = Encoding.UTF8.GetString(btName, 0, nameLength);
                sr.Append(nameText);
                _form.UpdateTestStatusText(sr.ToString());
                if (/*adapterType == 5 ||*/ string.Compare(nameText, DefaultBtName, StringComparison.Ordinal) != 0)
                {
                    sr.Append("\r\n");
                    sr.Append("Setting default name!");
                    _form.UpdateTestStatusText(sr.ToString());
                    byte[] response = AdapterCommandCustom(0x05, Encoding.UTF8.GetBytes(DefaultBtName));
                    if (response == null)
                    {
                        sr.Append("\r\n");
                        sr.Append("Settings name failed!");
                        _form.UpdateTestStatusText(sr.ToString());
                        return false;
                    }
                }
            }

            byte[] btPin = AdapterCommandCustom(0x84, new byte[] { 0x85 });
            if (btPin == null)
            {
                sr.Append("\r\n");
                sr.Append("Read pin failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            if (btPin.Length > 0)
            {
                sr.Append("\r\n");
                sr.Append("Pin: ");
                int pinLength = btPin.TakeWhile(value => value != 0x00).Count();
                string pinText = Encoding.ASCII.GetString(btPin, 0, pinLength);
                sr.Append(pinText);
                _form.UpdateTestStatusText(sr.ToString());
                if (string.Compare(pinText, DefaultBtPin, StringComparison.Ordinal) != 0)
                {
                    sr.Append("\r\n");
                    sr.Append("Setting default pin!");
                    _form.UpdateTestStatusText(sr.ToString());
                    byte[] response = AdapterCommandCustom(0x04, Encoding.ASCII.GetBytes(DefaultBtPin));
                    if (response == null)
                    {
                        sr.Append("\r\n");
                        sr.Append("Settings pin failed!");
                        _form.UpdateTestStatusText(sr.ToString());
                        return false;
                    }
                }
            }

            byte[] serialNumber = AdapterCommandCustom(0xFB, new byte[] { 0xFB });
            if (serialNumber == null)
            {
                sr.Append("\r\n");
                sr.Append("Read serial number failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append("Serial: ");
            sr.Append(BitConverter.ToString(serialNumber).Replace("-", ""));
            _form.UpdateTestStatusText(sr.ToString());

            byte[] voltage = AdapterCommandCustom(0xFC, new byte[] { 0xFC });
            if ((voltage == null) || (voltage.Length != 1))
            {
                sr.Append("\r\n");
                sr.Append("Read voltage failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append("Voltage: ");
            sr.Append(string.Format("{0,4:0.0}V", (double)voltage[0] / 10));
            _form.UpdateTestStatusText(sr.ToString());
            if ((voltage[0] < 110) || (voltage[0] > 140))
            {
                sr.Append("\r\n");
                sr.Append("Voltage out of range!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }

            if (!_form.ReadResponseFile(Path.Combine(_form.responseDir, "e61.txt"), CommThread.ConceptType.ConceptBwmFast))
            {
                sr.Append("\r\n");
                sr.Append("Reading response file failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }

            if (!_form.commThread.StartThread("CAN", CommThread.ConceptType.ConceptBwmFast, false, true,
                CommThread.ResponseType.E61, _form.threadConfigData, true))
            {
                sr.Append("\r\n");
                sr.Append("Start CAN thread failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            Thread.Sleep(100);

            try
            {
                // can mode 500
                if (AdapterCommandCustom(0x02, new byte[] { 0x01 }) == null)
                {
                    sr.Append("\r\n");
                    sr.Append("Set CAN mode failed!");
                    _form.UpdateTestStatusText(sr.ToString());
                    return false;
                }

                for (int i = 0; i < 10; i++)
                {
                    if (!BmwFastTest())
                    {
                        sr.Append("\r\n");
                        sr.Append("CAN test failed");
                        _form.UpdateTestStatusText(sr.ToString());
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append("CAN test OK");
                _form.UpdateTestStatusText(sr.ToString());
            }
            finally
            {
                _form.commThread.StopThread();
            }

            if (!_form.commThread.StartThread(comPort, CommThread.ConceptType.ConceptBwmFast, false, true,
                CommThread.ResponseType.E61, _form.threadConfigData, true))
            {
                sr.Append("\r\n");
                sr.Append("Start COM thread failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            Thread.Sleep(100);

            try
            {
                // can mode off
                if (AdapterCommandCustom(0x02, new byte[] { 0x00 }) == null)
                {
                    sr.Append("\r\n");
                    sr.Append("Set CAN mode failed!");
                    _form.UpdateTestStatusText(sr.ToString());
                    return false;
                }

                for (int i = 0; i < 10; i++)
                {
                    if (!BmwFastTest())
                    {
                        sr.Append("\r\n");
                        sr.Append("K-LINE test failed");
                        _form.UpdateTestStatusText(sr.ToString());
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append("K-LINE test OK");
                _form.UpdateTestStatusText(sr.ToString());
            }
            finally
            {
                _form.commThread.StopThread();
            }

            if (!_form.commThread.StartThread(comPort, CommThread.ConceptType.ConceptKwp2000Bmw, false, true,
                CommThread.ResponseType.E61, _form.threadConfigData, true))
            {
                sr.Append("\r\n");
                sr.Append("Start COM thread failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }
            Thread.Sleep(100);

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if (!BmwFastTest(true))
                    {
                        sr.Append("\r\n");
                        sr.Append("L-LINE test failed");
                        _form.UpdateTestStatusText(sr.ToString());
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append("L-LINE test OK");
                _form.UpdateTestStatusText(sr.ToString());
            }
            finally
            {
                _form.commThread.StopThread();
            }

            // can mode auto
            if (AdapterCommandCustom(0x02, new byte[] { 0xFF }) == null)
            {
                sr.Append("\r\n");
                sr.Append("Set CAN mode failed!");
                _form.UpdateTestStatusText(sr.ToString());
                return false;
            }

            return true;
        }

        private byte[] AdapterCommandCustom(byte command, byte[] data)
        {
            if (_dataStream == null)
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

        private byte[] CreateAdapterTelegram(byte[] sendData, int length, int baudRate, byte parity, bool useLline)
        {
            byte telType = 0x02;
            byte[] resultArray = new byte[length + ((telType == 0x00) ? 9 : 11)];
            resultArray[0] = 0x00;      // header
            resultArray[1] = telType;   // telegram type

            uint baudHalf;
            byte flags1 = KLINEF1_NO_ECHO;
            if (baudRate == 115200)
            {
                baudHalf = 0;
            }
            else
            {
                baudHalf = (uint)(baudRate >> 1);
                if (useLline)
                {
                    flags1 |= KLINEF1_USE_LLINE;
                }
                flags1 |= parity;
            }

            byte flags2 = 0x00;
            //flags2 |= KLINEF2_KWP1281_DETECT;

            resultArray[2] = (byte)(baudHalf >> 8);     // baud rate / 2 high
            resultArray[3] = (byte)baudHalf;            // baud rate / 2 low
            resultArray[4] = flags1;                    // flags 1
            if (telType == 0x00)
            {
                resultArray[5] = 0x00;                  // interbyte time
                resultArray[6] = (byte)(length >> 8);   // telegram length high
                resultArray[7] = (byte)length;          // telegram length low
                Array.Copy(sendData, 0, resultArray, 8, length);
                resultArray[resultArray.Length - 1] = CommThread.CalcChecksumBmwFast(resultArray, resultArray.Length - 1);
            }
            else
            {
                resultArray[5] = flags2;                // flags 2
                resultArray[6] = 0x00;                  // interbyte time
                resultArray[7] = KWP1281_TIMEOUT;       // KWP1281 timeout
                resultArray[8] = (byte)(length >> 8);   // telegram length high
                resultArray[9] = (byte)length;          // telegram length low
                Array.Copy(sendData, 0, resultArray, 10, length);
                resultArray[resultArray.Length - 1] = CommThread.CalcChecksumBmwFast(resultArray, resultArray.Length - 1);
            }
            return resultArray;
        }

        private bool BmwFastTest(bool lline = false)
        {
            byte[] identRequest = { 0x82, 0x12, 0xF1, 0x1A, 0x80};
            byte[] identResponse = {
            0xBC, 0xF1, 0x12, 0x5A, 0x80, 0x00, 0x00, 0x07, 0x80, 0x81,
            0x25, 0x00, 0x00, 0x00, 0x12, 0x4C, 0x50, 0x20, 0x08, 0x02,
            0x15, 0x08, 0x08, 0x02, 0x30, 0x39, 0x34, 0x37, 0x03, 0x03,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x79, 0x51, 0x46,
            0x31, 0x65, 0x57, 0x28, 0x30, 0x30, 0x38, 0x39, 0x51, 0x39,
            0x30, 0x30, 0x30, 0x38, 0x39, 0x51, 0x39, 0x30, 0x41, 0x39,
            0x34, 0x37, 0x42};

            if (lline)
            {
                byte[] sendTel = new byte[identRequest.Length + 1];
                Array.Copy(identRequest, sendTel, identRequest.Length);
                sendTel[sendTel.Length - 1] = CommThread.CalcChecksumBmwFast(sendTel, sendTel.Length - 1);
                byte[] adapterTel = CreateAdapterTelegram(sendTel, sendTel.Length, 10400, KLINEF1_PARITY_NONE, true);
                try
                {
                    _dataStream.Write(adapterTel, 0, adapterTel.Length);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                if (!SendBmwfast(identRequest))
                {
                    return false;
                }
            }

            byte[] response = new byte[0x100];
            if (!lline)
            {
                // receive echo
                int echoLength = ReceiveBmwFast(response);
                if (echoLength != identRequest.Length)
                {
                    return false;
                }
            }
            int dataLength = ReceiveBmwFast(response);
            if (dataLength != identResponse.Length)
            {
                return false;
            }
            return !identResponse.Where((t, i) => response[i] != t).Any();
        }

        private bool SendBmwfast(byte[] sendData)
        {
            if (_dataStream == null)
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
            telBuffer[sendLength] = CommThread.CalcChecksumBmwFast(telBuffer, sendLength);
            sendLength++;
            try
            {
                _dataStream.Write(telBuffer, 0, sendLength);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private int ReceiveBmwFast(byte[] receiveData)
        {
            if (_dataStream == null)
            {
                return 0;
            }
            try
            {
                // header byte
                _dataStream.ReadTimeout = 2000;
                for (int i = 0; i < 4; i++)
                {
                    int data;
                    try
                    {
                        data = _dataStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        while (_dataStream.DataAvailable)
                        {
                            try
                            {
                                _dataStream.ReadByte();
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
                    while (_dataStream.DataAvailable)
                    {
                        try
                        {
                            _dataStream.ReadByte();
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
                        data = _dataStream.ReadByte();
                    }
                    catch (Exception)
                    {
                        data = -1;
                    }
                    if (data < 0)
                    {
                        while (_dataStream.DataAvailable)
                        {
                            try
                            {
                                _dataStream.ReadByte();
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

                if (CommThread.CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                {
                    while (_dataStream.DataAvailable)
                    {
                        try
                        {
                            _dataStream.ReadByte();
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
    }
}
