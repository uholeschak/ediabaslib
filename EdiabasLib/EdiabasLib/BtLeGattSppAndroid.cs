using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Bluetooth;
using Android.Content;
using Android.OS;

namespace EdiabasLib
{
    public class BtLeGattSpp : IDisposable
    {
        private class GattSppInfo
        {
            public GattSppInfo(string name, Java.Util.UUID serviceUuid, Java.Util.UUID characteristicReadUuid, Java.Util.UUID characteristicWriteUuid)
            {
                Name = name;
                ServiceUuid = serviceUuid;
                CharacteristicReadUuid = characteristicReadUuid;
                CharacteristicWriteUuid = characteristicWriteUuid;
            }

            public string Name { get; }
            public Java.Util.UUID ServiceUuid { get; }
            public Java.Util.UUID CharacteristicReadUuid { get; }
            public Java.Util.UUID CharacteristicWriteUuid { get; }
        }

        public delegate void LogStringDelegate(string message);

#if DEBUG
        private static readonly string Tag = typeof(BtLeGattSpp).FullName;
#endif
        private static readonly Java.Util.UUID GattCharacteristicConfig = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        private static readonly List<GattSppInfo> _gattSppInfoList = new List<GattSppInfo>()
        {
            new GattSppInfo("Deep OBD", Java.Util.UUID.FromString("0000ffe0-0000-1000-8000-00805f9b34fb"),
                Java.Util.UUID.FromString("0000ffe1-0000-1000-8000-00805f9b34fb"), Java.Util.UUID.FromString("0000ffe2-0000-1000-8000-00805f9b34fb")),
            new GattSppInfo("Carly", Java.Util.UUID.FromString("0000ffe0-0000-1000-8000-00805f9b34fb"),
                Java.Util.UUID.FromString("0000ffe1-0000-1000-8000-00805f9b34fb"), Java.Util.UUID.FromString("0000ffe1-0000-1000-8000-00805f9b34fb")),
            new GattSppInfo("WgSoft", Java.Util.UUID.FromString("0000fff0-0000-1000-8000-00805f9b34fb"),
            Java.Util.UUID.FromString("0000fff1-0000-1000-8000-00805f9b34fb"), Java.Util.UUID.FromString("0000fff2-0000-1000-8000-00805f9b34fb")),
            new GattSppInfo("vLinker", Java.Util.UUID.FromString("e7810a71-73ae-499d-8c15-faa9aef0c3f2"),
            Java.Util.UUID.FromString("bef8d6c9-9c21-4c9e-b632-bd58c1009f9f"), Java.Util.UUID.FromString("bef8d6c9-9c21-4c9e-b632-bd58c1009f9f"))
        };

        private bool _disposed;
        private readonly LogStringDelegate _logStringHandler;
        private AutoResetEvent _btGattConnectEvent;
        private AutoResetEvent _btGattDiscoveredEvent;
        private AutoResetEvent _btGattReceivedEvent;
        private AutoResetEvent _btGattWriteEvent;
        private BluetoothGatt _bluetoothGatt;
        private BluetoothGattCharacteristic _gattCharacteristicSppRead;
        private BluetoothGattCharacteristic _gattCharacteristicSppWrite;
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
            _btGattConnectEvent = new AutoResetEvent(false);
            _btGattDiscoveredEvent = new AutoResetEvent(false);
            _btGattReceivedEvent = new AutoResetEvent(false);
            _btGattWriteEvent = new AutoResetEvent(false);
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

                    if (_btGattConnectEvent != null)
                    {
                        _btGattConnectEvent.Dispose();
                        _btGattConnectEvent = null;
                    }

                    if (_btGattDiscoveredEvent != null)
                    {
                        _btGattDiscoveredEvent.Dispose();
                        _btGattDiscoveredEvent = null;
                    }

                    if (_btGattReceivedEvent != null)
                    {
                        _btGattReceivedEvent.Dispose();
                        _btGattReceivedEvent = null;
                    }

                    if (_btGattWriteEvent != null)
                    {
                        _btGattWriteEvent.Dispose();
                        _btGattWriteEvent = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public bool ConnectLeGattDevice(Context context, BluetoothDevice device, ManualResetEvent cancelEvent = null)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return false;
            }

            try
            {
                BtGattDisconnect();

                _gattConnectionState = State.Connecting;
                _gattServicesDiscovered = false;
                _btGattSppInStream = new MemoryQueueBufferStream(true);
                _btGattSppOutStream = new BGattOutputStream(this);
                BGattBaseCallback bGattCallback = Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu ? new BGatt2Callback(this) : new BGatt1Callback(this);
#pragma warning disable CA1416
                _bluetoothGatt = device.ConnectGatt(context, false, bGattCallback, BluetoothTransports.Le);
#pragma warning restore CA1416
                if (_bluetoothGatt == null)
                {
                    LogString("*** ConnectGatt failed");
                    return false;
                }

                bool isBonding = false;
                if (device.BondState != Bond.Bonded)
                {
                    LogString("Waiting for device bonding");
                    device.CreateBond();

                    int bondRetry = 0;
                    for (;;)
                    {
                        if (device.BondState == Bond.Bonded)
                        {
                            LogString("Device bonded");
                            break;
                        }

                        if (device.BondState == Bond.Bonding)
                        {
                            isBonding = true;
                        }

                        if (isBonding && device.BondState == Bond.None)
                        {
                            LogString("*** Device bonding failed");
                            return false;
                        }

                        if (cancelEvent != null)
                        {
                            if (cancelEvent.WaitOne(0))
                            {
                                LogString("*** GATT bonding cancelled");
                                return false;
                            }
                        }
                        else
                        {
                            bondRetry++;
                            if (bondRetry > 10)
                            {
                                LogString("*** Device bonding timeout");
                                return false;
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }

                int retry = 0;
                for (;;)
                {
                    if (_btGattConnectEvent.WaitOne(1000))
                    {
                        if (_gattConnectionState != State.Connected)
                        {
                            LogString("*** GATT connection timeout");
                            return false;
                        }

                        break;
                    }

                    if (cancelEvent != null)
                    {
                        if (cancelEvent.WaitOne(0))
                        {
                            LogString("*** GATT connection cancelled");
                            return false;
                        }
                    }
                    else
                    {
                        retry++;
                        if (retry > 5)
                        {
                            LogString("*** GATT connection timeout");
                            return false;
                        }
                    }
                }

                retry = 0;
                for (;;)
                {
                    if (_btGattDiscoveredEvent.WaitOne(1000))
                    {
                        if (!_gattServicesDiscovered)
                        {
                            LogString("*** GATT service discovery timeout");
                            return false;
                        }
                        break;
                    }

                    if (cancelEvent != null)
                    {
                        if (cancelEvent.WaitOne(0))
                        {
                            LogString("*** GATT connection cancelled");
                            return false;
                        }
                    }
                    else
                    {
                        retry++;
                        if (retry > 5)
                        {
                            LogString("*** GATT service discovery timeout");
                            return false;
                        }
                    }
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

                    Android.Util.Log.Info(Tag, string.Format("GATT service: UUID={0}, Type={1}", gattService.Uuid, gattService.Type));
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

                _gattCharacteristicSppRead = null;
                _gattCharacteristicSppWrite = null;

                foreach (GattSppInfo gattSppInfo in _gattSppInfoList)
                {
                    BluetoothGattService gattServiceSpp = _bluetoothGatt.GetService(gattSppInfo.ServiceUuid);
                    if (gattServiceSpp != null)
                    {
                        BluetoothGattCharacteristic gattCharacteristicSppRead = gattServiceSpp.GetCharacteristic(gattSppInfo.CharacteristicReadUuid);
                        BluetoothGattCharacteristic gattCharacteristicSppWrite = gattServiceSpp.GetCharacteristic(gattSppInfo.CharacteristicWriteUuid);
                        if (gattCharacteristicSppRead != null && gattCharacteristicSppWrite != null)
                        {
                            bool validRead = (gattCharacteristicSppRead.Properties & GattProperty.Notify) == GattProperty.Notify;
                            bool validWrite = (gattCharacteristicSppWrite.Properties & GattProperty.Write) == GattProperty.Write;
                            if (validWrite && gattCharacteristicSppRead == gattCharacteristicSppWrite)
                            {
                                validWrite = (gattCharacteristicSppWrite.Properties & (GattProperty.Read | GattProperty.Write | GattProperty.Notify)) ==
                                             (GattProperty.Read | GattProperty.Write | GattProperty.Notify);
                            }

                            if (validRead && validWrite)
                            {
                                _gattCharacteristicSppRead = gattCharacteristicSppRead;
                                _gattCharacteristicSppWrite = gattCharacteristicSppWrite;
#if DEBUG
                                Android.Util.Log.Info(Tag, "SPP characteristic found: " + gattSppInfo.Name);
#endif
                                LogString("GATT SPP characteristic found: " + gattSppInfo.Name);
                                break;
                            }
                        }
                    }
                }

                if (_gattCharacteristicSppRead == null || _gattCharacteristicSppWrite == null)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "No known GATT SPP characteristic start autodetect");
#endif
                    LogString("*** No known GATT SPP characteristic start autodetect");
                    foreach (BluetoothGattService gattService in services)
                    {
                        if (gattService.Uuid == null || gattService.Characteristics == null || gattService.Type != GattServiceType.Primary)
                        {
                            continue;
                        }

                        LogString(string.Format("GATT service: UUID={0}", gattService.Uuid));
                        BluetoothGattCharacteristic gattCharacteristicSppRead = null;
                        BluetoothGattCharacteristic gattCharacteristicSppWrite = null;
                        bool sppValid = true;
                        foreach (BluetoothGattCharacteristic gattCharacteristic in gattService.Characteristics)
                        {
                            if (gattCharacteristic.Uuid == null)
                            {
                                continue;
                            }

                            LogString(string.Format("GATT properties: {0}", gattCharacteristic.Properties));
                            bool validRead = (gattCharacteristic.Properties & GattProperty.Notify) == GattProperty.Notify;
                            if (validRead)
                            {
                                if (gattCharacteristicSppRead != null)
                                {
                                    sppValid = false;
                                    break;
                                }

                                gattCharacteristicSppRead = gattCharacteristic;
                            }

                            bool validWrite = (gattCharacteristic.Properties & GattProperty.Write) == GattProperty.Write;
                            if (validWrite && gattCharacteristicSppRead != null && gattCharacteristicSppRead == gattCharacteristic)
                            {
                                validWrite = (gattCharacteristic.Properties & (GattProperty.Read | GattProperty.Write | GattProperty.Notify)) ==
                                             (GattProperty.Read | GattProperty.Write | GattProperty.Notify);
                            }

                            if (validWrite)
                            {
                                if (gattCharacteristicSppWrite != null)
                                {
                                    sppValid = false;
                                    break;
                                }

                                gattCharacteristicSppWrite = gattCharacteristic;
                            }
                        }

                        if (sppValid && gattCharacteristicSppRead != null && gattCharacteristicSppWrite != null)
                        {
                            _gattCharacteristicSppRead = gattCharacteristicSppRead;
                            _gattCharacteristicSppWrite = gattCharacteristicSppWrite;
#if DEBUG
                            Android.Util.Log.Info(Tag, "Generic SPP characteristic found");
#endif
                            LogString("GATT generic SPP characteristic found");
                            break;
                        }
                    }
                }

                if (_gattCharacteristicSppRead == null || _gattCharacteristicSppWrite == null)
                {
                    LogString("*** No GATT SPP characteristic found");
                    return false;
                }
#if DEBUG
                Android.Util.Log.Info(Tag, $"Read UUID: {_gattCharacteristicSppRead.Uuid}, Properties: {_gattCharacteristicSppRead.Properties}");
                Android.Util.Log.Info(Tag, $"Write UUID: {_gattCharacteristicSppWrite.Uuid}, Properties: {_gattCharacteristicSppWrite.Properties}");
#endif
                LogString($"GATT Read UUID: {_gattCharacteristicSppRead.Uuid}, Properties: {_gattCharacteristicSppRead.Properties}");
                LogString($"GATT Write UUID: {_gattCharacteristicSppWrite.Uuid}, Properties: {_gattCharacteristicSppWrite.Properties}");

                if (!_bluetoothGatt.SetCharacteristicNotification(_gattCharacteristicSppRead, true))
                {
                    LogString("*** GATT SPP enable notification failed");
                    return false;
                }

                BluetoothGattDescriptor descriptor = _gattCharacteristicSppRead.GetDescriptor(GattCharacteristicConfig);
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
                byte[] enableNotifyArray = BluetoothGattDescriptor.EnableNotificationValue.ToArray();
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CA1416
                    if (_bluetoothGatt.WriteDescriptor(descriptor, enableNotifyArray) != (int) CurrentBluetoothStatusCodes.Success)
#pragma warning restore CA1416
                    {
                        LogString("*** GATT SPP write config descriptor failed");
                        return false;
                    }
                }
                else
                {
#pragma warning disable CS0618
#pragma warning disable CA1422
                    descriptor.SetValue(enableNotifyArray);
                    if (!_bluetoothGatt.WriteDescriptor(descriptor))
#pragma warning restore CA1422
#pragma warning restore CS0618
                    {
                        LogString("*** GATT SPP write config descriptor failed");
                        return false;
                    }
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

                while (_btGattReceivedEvent.WaitOne(2000))
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

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ReceiveGattSppData(BluetoothGattCharacteristic characteristic, byte[] value = null)
        {
            try
            {
                if (characteristic.Uuid != null && _gattCharacteristicSppRead?.Uuid != null &&
                    characteristic.Uuid.Equals(_gattCharacteristicSppRead.Uuid))
                {
                    byte[] data = value;
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                    {
#pragma warning disable CS0618
#pragma warning disable CA1422
                        data = characteristic.GetValue();
#pragma warning restore CA1422
#pragma warning restore CS0618
                    }

                    if (data != null)
                    {
#if DEBUG
                        string dataText = Encoding.UTF8.GetString(data);
                        dataText = dataText.Replace("\r", "");
                        Android.Util.Log.Info(Tag, string.Format("GATT SPP data received: {0} '{1}'",
                            BitConverter.ToString(data).Replace("-", ""), dataText));
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
                if (_gattCharacteristicSppRead != null)
                {
                    try
                    {
                        _bluetoothGatt?.SetCharacteristicNotification(_gattCharacteristicSppRead, false);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    _gattCharacteristicSppRead = null;
                }

                _gattCharacteristicSppWrite = null;

                _gattConnectionState = State.Disconnected;
                _gattServicesDiscovered = false;

                if (_bluetoothGatt != null)
                {
                    try
                    {
                        _bluetoothGatt.Disconnect();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

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

        private class BGattBaseCallback : BluetoothGattCallback
        {
            protected readonly BtLeGattSpp _btLeGattSpp;

            protected BGattBaseCallback(BtLeGattSpp btLeGattSpp)
            {
                _btLeGattSpp = btLeGattSpp;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                switch (newState)
                {
                    case ProfileState.Connected:
#if DEBUG
                        Android.Util.Log.Info(Tag, "Connected to GATT server.");
#endif
                        _btLeGattSpp._gattConnectionState = State.Connected;
                        _btLeGattSpp._gattServicesDiscovered = false;
                        _btLeGattSpp._btGattConnectEvent.Set();
                        gatt.DiscoverServices();
                        break;

                    case ProfileState.Disconnected:
#if DEBUG
                        Android.Util.Log.Info(Tag, "Disconnected from GATT server.");
#endif
                        _btLeGattSpp._gattConnectionState = State.Disconnected;
                        _btLeGattSpp._gattServicesDiscovered = false;
                        _btLeGattSpp._btGattConnectEvent.Set();
                        break;
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
                    _btLeGattSpp._gattServicesDiscovered = false;
                    _btLeGattSpp._btGattDiscoveredEvent.Set();
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
                    if (characteristic.Uuid != null && _btLeGattSpp._gattCharacteristicSppWrite?.Uuid != null &&
                        characteristic.Uuid.Equals(_btLeGattSpp._gattCharacteristicSppWrite.Uuid))
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
        }

        private class BGatt1Callback : BGattBaseCallback
        {
            public BGatt1Callback(BtLeGattSpp btLeGattSpp) : base(btLeGattSpp)
            {
            }

#pragma warning disable CS0672
            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
#pragma warning restore CS0672
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                if (status == GattStatus.Success)
                {
                    _btLeGattSpp.ReceiveGattSppData(characteristic);
                }
            }

#pragma warning disable CS0672
            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
#pragma warning restore CS0672
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    return;
                }

                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                _btLeGattSpp.ReceiveGattSppData(characteristic);
            }
        }

        private class BGatt2Callback : BGattBaseCallback
        {
            public BGatt2Callback(BtLeGattSpp btLeGattSpp) : base(btLeGattSpp)
            {
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value, GattStatus status)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                if (status == GattStatus.Success)
                {
                    _btLeGattSpp.ReceiveGattSppData(characteristic, value);
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value)
            {
                if (gatt != _btLeGattSpp._bluetoothGatt)
                {
                    return;
                }

                _btLeGattSpp.ReceiveGattSppData(characteristic, value);
            }
        }

        public class BGattOutputStream : MemoryQueueBufferStream
        {
            private const int MaxWriteLength = 20;
            readonly BtLeGattSpp _btLeGattSpp;

            public BGattOutputStream(BtLeGattSpp btLeGattSpp) : base(true)
            {
                _btLeGattSpp = btLeGattSpp;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
            public override void Write(byte[] buffer, int offset, int count)
            {
                base.Write(buffer, offset, count);

                if (_btLeGattSpp._gattConnectionState != State.Connected ||
                    _btLeGattSpp._gattCharacteristicSppWrite == null)
                {
                    throw new IOException("GATT disconnected");
                }

                while (Length > 0)
                {
                    byte[] writeBuffer = new byte[MaxWriteLength];
                    int length = Read(writeBuffer, 0, writeBuffer.Length);
                    if (length <= 0)
                    {
                        throw new IOException("Stream write: write chunk failed");
                    }

                    byte[] sendData = new byte[length];
                    Array.Copy(writeBuffer, 0, sendData, 0, length);

#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GATT SPP data write: {0} '{1}'",
                        BitConverter.ToString(sendData).Replace("-", ""), Encoding.UTF8.GetString(sendData)));
#endif
                    _btLeGattSpp._gattWriteStatus = GattStatus.Failure;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    {
                        if (_btLeGattSpp._bluetoothGatt.WriteCharacteristic(_btLeGattSpp._gattCharacteristicSppWrite, sendData, (int) GattWriteType.Default)
                            != (int)CurrentBluetoothStatusCodes.Success)
                        {
                            throw new IOException("WriteCharacteristic failed");
                        }
                    }
                    else
                    {
#pragma warning disable CS0618
#pragma warning disable CA1422
                        _btLeGattSpp._gattCharacteristicSppWrite.SetValue(sendData);
                        if (!_btLeGattSpp._bluetoothGatt.WriteCharacteristic(_btLeGattSpp._gattCharacteristicSppWrite))
#pragma warning restore CA1422
#pragma warning restore CS0618
                        {
                            throw new IOException("WriteCharacteristic failed");
                        }
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
