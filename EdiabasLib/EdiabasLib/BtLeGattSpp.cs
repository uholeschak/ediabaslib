using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Bluetooth;
using Android.Content;
using Android.Nfc;

namespace EdiabasLib
{
    public class BtLeGattSpp : IDisposable
    {
        public delegate void LogStringDelegate(string message);

#if DEBUG
        private static readonly string Tag = typeof(BtLeGattSpp).FullName;
#endif
        private static readonly Java.Util.UUID GattServiceSpp = Java.Util.UUID.FromString("0000ffe0-0000-1000-8000-00805f9b34fb");
        private static readonly Java.Util.UUID GattCharacteristicSpp = Java.Util.UUID.FromString("0000ffe1-0000-1000-8000-00805f9b34fb");
        private static readonly Java.Util.UUID GattCharacteristicConfig = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        private bool _disposed;
        private readonly LogStringDelegate _logStringHandler;
        private readonly AutoResetEvent _btGattConnectEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _btGattDiscoveredEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _btGattReceivedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _btGattWriteEvent = new AutoResetEvent(false);
        private BluetoothGatt _bluetoothGatt;
        private BluetoothGattCharacteristic _gattCharacteristicSpp;
        private volatile State _gattConnectionState = State.Disconnected;
        private volatile bool _gattServicesDiscovered;
        private GattStatus _gattWriteStatus = GattStatus.Failure;
        private MemoryQueueBufferStream _btGattSppInStream;
        private BGattOutputStream _btGattSppOutStream;

        public State GattConnectionState => _gattConnectionState;
        public bool GattServicesDiscovered => _gattServicesDiscovered;
        public MemoryQueueBufferStream BtGattSppInStream => _btGattSppInStream;
        public BGattOutputStream BtGattSppOutStream => _btGattSppOutStream;

        public BtLeGattSpp(LogStringDelegate logStringHandler = null)
        {
            _logStringHandler = logStringHandler;
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
                    BtGattDisconnect();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public bool ConnectLeGattDevice(Context context, BluetoothDevice device)
        {
            try
            {
                BtGattDisconnect();

                _gattConnectionState = State.Connecting;
                _gattServicesDiscovered = false;
                _btGattSppInStream = new MemoryQueueBufferStream(true);
                _btGattSppOutStream = new BGattOutputStream(this);
                _bluetoothGatt = device.ConnectGatt(context, false, new BGattCallback(this));
                if (_bluetoothGatt == null)
                {
                    LogString("*** ConnectGatt failed");
                    return false;
                }

                _btGattConnectEvent.WaitOne(2000, false);
                if (_gattConnectionState != State.Connected)
                {
                    LogString("*** GATT connection timeout");
                    return false;
                }

                _btGattDiscoveredEvent.WaitOne(2000, false);
                if (!_gattServicesDiscovered)
                {
                    LogString("*** GATT service discovery timeout");
                    return false;
                }

                IList<BluetoothGattService> services = _bluetoothGatt.Services;
                if (services == null)
                {
                    LogString("*** No GATT services found");
                    return false;
                }

#if DEBUG
                foreach (BluetoothGattService gattService in services)
                {
                    if (gattService.Uuid == null || gattService.Characteristics == null)
                    {
                        continue;
                    }

                    Android.Util.Log.Info(Tag, string.Format("GATT service: {0}", gattService.Uuid));
                    foreach (BluetoothGattCharacteristic gattCharacteristic in gattService.Characteristics)
                    {
                        if (gattCharacteristic.Uuid == null)
                        {
                            continue;
                        }

                        Android.Util.Log.Info(Tag, string.Format("GATT characteristic: {0}", gattCharacteristic.Uuid));
                        Android.Util.Log.Info(Tag, string.Format("GATT properties: {0}", gattCharacteristic.Properties));
                    }
                }
#endif

                _gattCharacteristicSpp = null;
                BluetoothGattService gattServiceSpp = _bluetoothGatt.GetService(GattServiceSpp);
                BluetoothGattCharacteristic gattCharacteristicSpp = gattServiceSpp?.GetCharacteristic(GattCharacteristicSpp);
                if (gattCharacteristicSpp != null)
                {
                    if ((gattCharacteristicSpp.Properties & (GattProperty.Read | GattProperty.Write | GattProperty.Notify)) ==
                        (GattProperty.Read | GattProperty.Write | GattProperty.Notify))
                    {
                        _gattCharacteristicSpp = gattCharacteristicSpp;
#if DEBUG
                        Android.Util.Log.Info(Tag, "SPP characteristic found");
#endif
                    }
                }

                if (_gattCharacteristicSpp == null)
                {
                    LogString("*** No GATT SPP characteristic found");
                    return false;
                }

                if (!_bluetoothGatt.SetCharacteristicNotification(_gattCharacteristicSpp, true))
                {
                    LogString("*** GATT SPP enable notification failed");
                    return false;
                }

                BluetoothGattDescriptor descriptor = _gattCharacteristicSpp.GetDescriptor(GattCharacteristicConfig);
                if (descriptor == null)
                {
                    LogString("*** GATT SPP config descriptor not found");
                    return false;
                }

                if (BluetoothGattDescriptor.EnableNotificationValue == null)
                {
                    LogString("*** GATT SPP EnableNotificationValue not present");
                    return false;
                }

                _gattWriteStatus = GattStatus.Failure;
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                if (!_bluetoothGatt.WriteDescriptor(descriptor))
                {
                    LogString("*** GATT SPP write config descriptor failed");
                    return false;
                }

                if (!_btGattWriteEvent.WaitOne(2000))
                {
                    LogString("*** GATT SPP write config descriptor timeout");
                    return false;
                }

                if (_gattWriteStatus != GattStatus.Success)
                {
                    LogString("*** GATT SPP write config descriptor status failure");
                    return false;
                }

#if false
                byte[] sendData = Encoding.UTF8.GetBytes("ATI\r");
                _btGattSppOutStream.Write(sendData, 0, sendData.Length);

                while (_btGattReceivedEvent.WaitOne(2000, false))
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "GATT SPP data received");
#endif
                }

                while (_btGattSppInStream.HasData())
                {
                    int data = _btGattSppInStream.ReadByteAsync();
                    if (data < 0)
                    {
                        break;
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GATT SPP byte: {0:X02}", data));
#endif
                }
#endif
                return true;
            }
            catch (Exception)
            {
                _gattConnectionState = State.Disconnected;
                _gattServicesDiscovered = false;
                return false;
            }
        }

        private bool ReadGattSppData(BluetoothGattCharacteristic characteristic)
        {
            try
            {
                if (characteristic.Uuid != null && characteristic.Uuid.Equals(GattCharacteristicSpp))
                {
                    byte[] data = characteristic.GetValue();
                    if (data != null)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("GATT SPP data received: {0} '{1}'",
                            BitConverter.ToString(data).Replace("-", ""), Encoding.UTF8.GetString(data)));
#endif
                        _btGattSppInStream?.Write(data);
                        _btGattReceivedEvent.Set();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public void BtGattDisconnect()
        {
            try
            {
                _gattCharacteristicSpp = null;
                _gattConnectionState = State.Disconnected;
                _gattServicesDiscovered = false;

                if (_bluetoothGatt != null)
                {
                    _bluetoothGatt.Disconnect();
                    _bluetoothGatt.Dispose();
                    _bluetoothGatt = null;
                }

                if (_btGattSppInStream != null)
                {
                    _btGattSppInStream.Dispose();
                    _btGattSppInStream = null;
                }

                if (_btGattSppOutStream != null)
                {
                    _btGattSppOutStream.Dispose();
                    _btGattSppOutStream = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void LogString(string info)
        {
            _logStringHandler?.Invoke(info);
        }

        private class BGattCallback : BluetoothGattCallback
        {
            readonly BtLeGattSpp _btLeGattSpp;

            public BGattCallback(BtLeGattSpp btLeGattSpp)
            {
                _btLeGattSpp = btLeGattSpp;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                if (newState == ProfileState.Connected)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "Connected to GATT server.");
#endif
                    _btLeGattSpp._gattConnectionState = State.Connected;
                    _btLeGattSpp._gattServicesDiscovered = false;
                    _btLeGattSpp._btGattConnectEvent.Set();
                    gatt.DiscoverServices();
                }
                else if (newState == ProfileState.Disconnected)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "Disconnected from GATT server.");
#endif
                    _btLeGattSpp._gattConnectionState = State.Disconnected;
                    _btLeGattSpp._gattServicesDiscovered = false;
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                if (status == GattStatus.Success)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "GATT services discovered.");
#endif
                    _btLeGattSpp._gattServicesDiscovered = true;
                    _btLeGattSpp._btGattDiscoveredEvent.Set();
                }
                else
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GATT services discovery failed: {0}", status));
#endif
                }
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                if (status == GattStatus.Success)
                {
                    _btLeGattSpp.ReadGattSppData(characteristic);
                }
            }

            public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                GattStatus resultStatus = GattStatus.Failure;
                if (status == GattStatus.Success)
                {
                    if (characteristic.Uuid != null && characteristic.Uuid.Equals(GattCharacteristicSpp))
                    {
                        resultStatus = status;
                    }
                }

                _btLeGattSpp._gattWriteStatus = resultStatus;
                _btLeGattSpp._btGattWriteEvent.Set();
            }

            public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                _btLeGattSpp._gattWriteStatus = status;
                _btLeGattSpp._btGattWriteEvent.Set();
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                _btLeGattSpp.ReadGattSppData(characteristic);
            }
        }

        public class BGattOutputStream : MemoryQueueBufferStream
        {
            readonly BtLeGattSpp _btLeGattSpp;

            public BGattOutputStream(BtLeGattSpp btLeGattSpp)
            {
                _btLeGattSpp = btLeGattSpp;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                base.Write(buffer, offset, count);

                long dataLength = Length;
                if (dataLength > 0 && _btLeGattSpp._gattCharacteristicSpp != null)
                {
                    byte[] sendData = new byte[dataLength];
                    int length = Read(sendData, 0, (int)dataLength);
                    if (length != dataLength)
                    {
                        throw new IOException("Read failed");
                    }

#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GATT SPP data write: {0} '{1}'",
                        BitConverter.ToString(sendData).Replace("-", ""), Encoding.UTF8.GetString(sendData)));
#endif
                    _btLeGattSpp._gattWriteStatus = GattStatus.Failure;
                    _btLeGattSpp._gattCharacteristicSpp.SetValue(sendData);
                    if (!_btLeGattSpp._bluetoothGatt.WriteCharacteristic(_btLeGattSpp._gattCharacteristicSpp))
                    {
                        throw new IOException("WriteCharacteristic failed");
                    }

                    if (!_btLeGattSpp._btGattWriteEvent.WaitOne(2000))
                    {
                        throw new IOException("WriteCharacteristic timeout");
                    }

                    if (_btLeGattSpp._gattWriteStatus != GattStatus.Success)
                    {
                        throw new IOException("WriteCharacteristic status failure");
                    }
                }
            }
        }
    }
}
