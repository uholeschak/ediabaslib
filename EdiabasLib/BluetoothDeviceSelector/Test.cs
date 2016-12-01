using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using SimpleWifi;
using SimpleWifi.Win32;

namespace BluetoothDeviceSelector
{
    class Test : IDisposable
    {
        private readonly FormMain _form;
        private NetworkStream _btStream;
        private volatile Thread _testThread;
        private bool _disposed;
        public bool TestOk { get; set; }
        public bool ConfigPossible { get; set; }
        public bool ThreadActive => _testThread != null;

        private enum AdapterMode
        {
            CanOff = 0x00,
            Can500 = 0x01,
            Can100 = 0x09,
            CanAuto = 0xFF,
        }

        public Test(FormMain form)
        {
            _form = form;
            _form.UpdateStatusText(string.Empty);
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
                    DisconnectBtDevice();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        public bool ExecuteTest(bool configure)
        {
            if (_testThread != null)
            {
                return false;
            }
            TestOk = false;
            ConfigPossible = false;
            AccessPoint ap = _form.GetSelectedAp();
            if (ap != null)
            {
                AuthRequest authRequest = new AuthRequest(ap);
                if (authRequest.IsPasswordRequired)
                {
                    authRequest.Password = _form.WifiPassword;
                }
                ap.ConnectAsync(authRequest, true, success =>
                {
                    _form.BeginInvoke((Action)(() =>
                    {
                        if (!success)
                        {
                            _form.UpdateStatusText(Strings.ConnectionFailed);
                        }
                        else
                        {
                            _form.PerformSearch();
                        }
                    }));
                });
                return true;
            }

            WlanInterface wlanIface = _form.GetSelectedWifiDevice();
            if (wlanIface != null)
            {
                try
                {
                    IPInterfaceProperties ipProp = wlanIface.NetworkInterface.GetIPProperties();
                    if (ipProp == null)
                    {
                        _form.UpdateStatusText(Strings.ConnectionFailed);
                        return false;
                    }
                    string ipAddr = (from addr in ipProp.DhcpServerAddresses where addr.AddressFamily == AddressFamily.InterNetwork select addr.ToString()).FirstOrDefault();
                    if (string.IsNullOrEmpty(ipAddr))
                    {
                        _form.UpdateStatusText(Strings.ConnectionFailed);
                        return false;
                    }
                    if (configure)
                    {
                        Process.Start(string.Format("http://{0}", ipAddr));
                        _form.UpdateStatusText(Strings.WifiUrlOk);
                        TestOk = true;
                        ConfigPossible = true;
                        return true;
                    }
                    _testThread = new Thread(() =>
                    {
                        try
                        {
                            TestOk = RunWifiTest(ipAddr);
                            if (TestOk)
                            {
                                ConfigPossible = true;
                            }
                        }
                        finally
                        {
                            _testThread = null;
                            _form.UpdateButtonStatus();
                        }
                    });
                    _testThread.Start();
                }
                catch (Exception)
                {
                    _form.UpdateStatusText(Strings.ConnectionFailed);
                    return false;
                }
                return true;
            }

            BluetoothDeviceInfo devInfo = _form.GetSelectedBtDevice();
            if (devInfo == null)
            {
                return false;
            }
            string pin = _form.BluetoothPin;
            _testThread = new Thread(() =>
            {
                try
                {
                    _form.UpdateStatusText(Strings.Connecting);
                    if (!ConnectBtDevice(devInfo, pin))
                    {
                        _form.UpdateStatusText(Strings.ConnectionFailed);
                        return;
                    }
                    bool configRequired;
                    TestOk = RunBtTest(configure, out configRequired);
                    if (TestOk && configRequired)
                    {
                        ConfigPossible = true;
                    }
                }
                finally
                {
                    DisconnectBtDevice();
                    _testThread = null;
                    _form.UpdateButtonStatus();
                }
            });
            _testThread.Start();
            return true;
        }

        private bool RunWifiTest(string ipAddr)
        {
            _form.UpdateStatusText(Strings.Connecting);

            StringBuilder sr = new StringBuilder();
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                WebRequest request = WebRequest.Create(string.Format("http://{0}", ipAddr));
                response = request.GetResponse();

                Stream dataStream = response.GetResponseStream();
                if (dataStream == null)
                {
                    _form.UpdateStatusText(Strings.ConnectionFailed);
                    return false;
                }
                sr.Append(Strings.Connected);
                reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                if (!responseFromServer.Contains(@"LuCI - Lua Configuration Interface"))
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.HttpResponseIncorrect);
                    _form.UpdateStatusText(sr.ToString());
                    return false;
                }
                sr.Append("\r\n");
                sr.Append(Strings.HttpResponseOk);
            }
            catch (Exception)
            {
                _form.UpdateStatusText(Strings.ConnectionFailed);
                return false;
            }
            finally
            {
                reader?.Close();
                response?.Close();
            }
            sr.Append("\r\n");
            sr.Append(Strings.TestOk);
            _form.UpdateStatusText(sr.ToString());
            return true;
        }

        private bool RunBtTest(bool configure, out bool configRequired)
        {
            configRequired = false;
            StringBuilder sr = new StringBuilder();
            sr.Append(Strings.Connected);
            byte[] firmware = AdapterCommandCustom(0xFD, new byte[] { 0xFD });
            if ((firmware == null) || (firmware.Length < 4))
            {
                sr.Append("\r\n");
                sr.Append(Strings.ReadFirmwareVersionFailed);
                _form.UpdateStatusText(sr.ToString());
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
                _form.UpdateStatusText(sr.ToString());
                return false;
            }
            byte[] canMode = AdapterCommandCustom(0x82, new byte[] { 0x00 });
            if ((canMode == null) || (canMode.Length < 1))
            {
                sr.Append("\r\n");
                sr.Append(Strings.ReadModeFailed);
                _form.UpdateStatusText(sr.ToString());
                return false;
            }
            sr.Append("\r\n");
            sr.Append(Strings.CanMode);
            sr.Append(" ");
            switch ((AdapterMode)canMode[0])
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
                if (configure)
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.CanModeChangeAuto);
                    canMode = AdapterCommandCustom(0x02, new[] { (byte)AdapterMode.CanAuto });
                    if ((canMode == null) || (canMode.Length < 1) || ((AdapterMode)canMode[0] != AdapterMode.CanAuto))
                    {
                        sr.Append("\r\n");
                        sr.Append(Strings.CanModeChangeFailed);
                    }
                }
                else
                {
                    configRequired = true;
                    sr.Append("\r\n");
                    sr.Append(Strings.CanModeNotAuto);
                }
            }
            sr.Append("\r\n");
            sr.Append(Strings.TestOk);
            _form.UpdateStatusText(sr.ToString());
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
    }
}
