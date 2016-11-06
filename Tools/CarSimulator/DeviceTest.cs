using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Windows.Forms;

namespace CarSimulator
{
    public class DeviceTest : IDisposable
    {
        private readonly MainForm _form;
        private Stream _btStream;
        private bool _disposed;

        public DeviceTest(MainForm form)
        {
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
                    DisconnectBtDevice();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        private BluetoothDeviceInfo DiscoverBtDevice()
        {
            BluetoothClient cli = new BluetoothClient();
            BluetoothDeviceInfo[] peers = cli.DiscoverDevices();
            foreach (BluetoothDeviceInfo device in peers)
            {
                Debug.WriteLine("{0} : {1}", device.DeviceAddress, device.DeviceName);
            }
            SelectBluetoothDeviceDialog dlg = new SelectBluetoothDeviceDialog();
            DialogResult result = dlg.ShowDialog(_form);
            if (result != DialogResult.OK)
            {
                return null;
            }
            return dlg.SelectedDevice;
        }

        private bool ConnectBtDevice(BluetoothDeviceInfo device)
        {
            try
            {
                BluetoothEndPoint ep = new BluetoothEndPoint(device.DeviceAddress, BluetoothService.SerialPort);
                BluetoothClient cli = new BluetoothClient();
                cli.SetPin("1234");
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

        public bool ExecuteTest()
        {
            BluetoothDeviceInfo device = DiscoverBtDevice();
            if (device == null)
            {
                _form.UpdateTestStatusText("No device selected");
                return false;
            }
            if (!ConnectBtDevice(device))
            {
                _form.UpdateTestStatusText("Connection faild");
                return false;
            }
            RunTest();
            return true;
        }

        private bool RunTest()
        {
            StringBuilder sr = new StringBuilder();
            byte[] btName = AdapterCommandCustom(0x85, new byte[] { 0x85 });
            sr.Append("Name: ");
            int length = btName.TakeWhile(value => value != 0x00).Count();
            sr.Append(Encoding.UTF8.GetString(btName, 0, length));
            _form.UpdateTestStatusText(sr.ToString());

            byte[] serialNumber = AdapterCommandCustom(0xFB, new byte[] { 0xFB });
            sr.Append("\r\n");
            sr.Append("Serial: ");
            sr.Append(BitConverter.ToString(serialNumber).Replace("-", ""));
            _form.UpdateTestStatusText(sr.ToString());
            return true;
        }

        private byte[] AdapterCommandCustom(byte command, byte[] data)
        {
            if (_btStream == null)
            {
                return null;
            }
            byte[] request = new byte[4 + data.Length + 1]; // +1 for checksum
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
            if (echoLength != request.Length - 1)
            {
                return null;
            }
            int length = ReceiveBmwFast(response);
            if ((length < 5) || (response[3] != command))
            {
                return null;
            }
            byte[] result = new byte[length - 5];
            Array.Copy(response, 4, result, 0, result.Length);
            return result;
        }

        private bool SendBmwfast(byte[] sendData)
        {
            if (_btStream == null)
            {
                return false;
            }
            int sendLength = sendData[0] & 0x3F;
            if (sendLength == 0)
            {   // with length byte
                sendLength = sendData[3] + 4;
            }
            else
            {
                sendLength += 3;
            }
            sendData[sendLength] = CommThread.CalcChecksumBmwFast(sendData, sendLength);
            sendLength++;
            try
            {
                _btStream.Write(sendData, 0, sendLength);
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
                if (_btStream.Read(receiveData, 0, 4) != 4)
                {
                    _btStream.Flush();
                    return 0;
                }
                if ((receiveData[0] & 0x80) != 0x80)
                {   // 0xC0: Broadcast
                    _btStream.Flush();
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
                if (_btStream.Read(receiveData, 4, recLength - 3) != recLength - 3)
                {
                    _btStream.Flush();
                    return 0;
                }
                if (CommThread.CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
                {
                    _btStream.Flush();
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
