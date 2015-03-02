using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Android.Bluetooth;
using Java.Util;

namespace EdiabasLib
{
    static public class EdBluetoothInterface
    {
        public const string PortID = "BLUETOOTH";
        private static UUID SPP_UUID = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly long tickResolMs = Stopwatch.Frequency / 1000;
        private const int readTimeoutOffset = 1000;

        private static BluetoothSocket bluetoothSocket;
        private static Stream bluetoothInStream;
        private static Stream bluetoothOutStream;
        private static int currentBaudRate = 0;
        private static int currentWordLength = 0;
        private static EdInterfaceObd.SerialParity currentParity = EdInterfaceObd.SerialParity.None;

        static EdBluetoothInterface()
        {
        }

        public static int CurrentBaudRate
        {
            get { return currentBaudRate; }
        }

        public static int CurrentWordLength
        {
            get { return currentWordLength; }
        }

        public static EdInterfaceObd.SerialParity CurrentParity
        {
            get { return currentParity; }
        }

        public static bool InterfaceConnect(string port)
        {
            if (bluetoothSocket != null)
            {
                return true;
            }
            if (!port.StartsWith(PortID, StringComparison.OrdinalIgnoreCase))
            {
                InterfaceDisconnect();
                return false;
            }
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter == null)
            {
                return false;
            }
            try
            {
                BluetoothDevice device = null;
                string portData = port.Remove(0, PortID.Length);
                if ((portData.Length > 0) && (portData[0] == ':'))
                {   // special id
                    string addr = portData.Remove(0, 1);
                    device = bluetoothAdapter.GetRemoteDevice(addr);
                }
                else
                {
                    InterfaceDisconnect();
                    return false;
                }
                if (device == null)
                {
                    InterfaceDisconnect();
                    return false;
                }
                bluetoothAdapter.CancelDiscovery();
                bluetoothSocket = device.CreateRfcommSocketToServiceRecord (SPP_UUID);
                bluetoothSocket.Connect();
                bluetoothInStream = bluetoothSocket.InputStream;
                bluetoothOutStream = bluetoothSocket.OutputStream;
            }
            catch (Exception)
            {
                InterfaceDisconnect ();
                return false;
            }
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            bool result = true;
            try
            {
                if (bluetoothInStream != null)
                {
                    bluetoothInStream.Close();
                    bluetoothInStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            try
            {
                if (bluetoothOutStream != null)
                {
                    bluetoothOutStream.Close();
                    bluetoothOutStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            try
            {
                if (bluetoothSocket != null)
                {
                    bluetoothSocket.Close();
                    bluetoothSocket = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (bluetoothSocket == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.CONFIG_ERROR;
            }
            currentBaudRate = baudRate;
            currentWordLength = dataBits;
            currentParity = parity;
            return EdInterfaceObd.InterfaceErrorResult.NO_ERROR;
        }

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (bluetoothSocket == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetRts(bool rts)
        {
            if (bluetoothSocket == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = true;
            if (bluetoothSocket == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            return false;
        }

        public static bool InterfacePurgeInBuffer()
        {
            if ((bluetoothSocket == null) || (bluetoothInStream == null))
            {
                return false;
            }
            try
            {
                bluetoothInStream.Flush ();
                while (bluetoothInStream.IsDataAvailable())
                {
                    bluetoothInStream.ReadByte();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            if ((bluetoothSocket == null) || (bluetoothOutStream == null))
            {
                return false;
            }
            if ((currentBaudRate != 115200) || (currentWordLength != 8) || (currentParity != EdInterfaceObd.SerialParity.None))
            {
                return false;
            }
            try
            {
                bluetoothOutStream.Write (sendData, 0, length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            if ((bluetoothSocket == null) || (bluetoothInStream == null))
            {
                return false;
            }
            timeout += readTimeoutOffset;
            timeoutTelEnd += readTimeoutOffset;
            try
            {
                int recLen = 0;
                long startTime = Stopwatch.GetTimestamp();
                while (recLen < length)
                {
                    int currTimeout = (recLen == 0) ? timeout : timeoutTelEnd;
                    if (bluetoothInStream.IsDataAvailable())
                    {
                        int bytesRead = bluetoothInStream.Read (receiveData, offset + recLen, length - recLen);
                        recLen += (int)bytesRead;
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if ((Stopwatch.GetTimestamp() - startTime) > currTimeout * tickResolMs)
                    {
                        if (ediabasLog != null)
                        {
                            ediabasLog.LogData(EdiabasNet.ED_LOG_LEVEL.IFH, receiveData, offset, recLen, "Rec ");
                        }
                        return false;
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
