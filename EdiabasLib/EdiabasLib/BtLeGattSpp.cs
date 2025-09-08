using InTheHand.Bluetooth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdiabasLib
{
    public class BtLeGattSpp : IDisposable
    {
        private class GattSppInfo
        {
            public GattSppInfo(string name, Guid serviceUuid, Guid characteristicReadUuid, Guid characteristicWriteUuid)
            {
                Name = name;
                ServiceUuid = serviceUuid;
                CharacteristicReadUuid = characteristicReadUuid;
                CharacteristicWriteUuid = characteristicWriteUuid;
            }

            public string Name { get; }
            public Guid ServiceUuid { get; }
            public Guid CharacteristicReadUuid { get; }
            public Guid CharacteristicWriteUuid { get; }
        }

        public delegate void LogStringDelegate(string message);

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        private static readonly List<GattSppInfo> _gattSppInfoList = new List<GattSppInfo>()
        {
            new GattSppInfo("Deep OBD", new Guid("0000ffe0-0000-1000-8000-00805f9b34fb"),
                new Guid("0000ffe1-0000-1000-8000-00805f9b34fb"), new Guid("0000ffe2-0000-1000-8000-00805f9b34fb")),
        };

        private bool _disposed;
        private readonly LogStringDelegate _logStringHandler;
        private AutoResetEvent _btGattReceivedEvent;
        private BluetoothDevice _bluetoothDevice;
        private RemoteGattServer _bluetoothGatt;
        private GattCharacteristic _gattCharacteristicSppRead;
        private GattCharacteristic _gattCharacteristicSppWrite;
        private volatile ConnectionState _gattConnectionState = ConnectionState.Disconnected;
        private MemoryQueueBufferStream _btGattSppInStream;
        private BGattOutputStream _btGattSppOutStream;
        private CancellationTokenSource _cancellationTokenSource;

        public ConnectionState GattConnectionState => _gattConnectionState;
        public MemoryQueueBufferStream BtGattSppInStream => _btGattSppInStream;
        public BGattOutputStream BtGattSppOutStream => _btGattSppOutStream;

        public BtLeGattSpp(LogStringDelegate logStringHandler = null)
        {
            _logStringHandler = logStringHandler;
            _btGattReceivedEvent = new AutoResetEvent(false);
            _cancellationTokenSource = new CancellationTokenSource();
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

                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                        _cancellationTokenSource = null;
                    }

                    if (_btGattReceivedEvent != null)
                    {
                        _btGattReceivedEvent.Dispose();
                        _btGattReceivedEvent = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public bool ConnectLeGattDevice(BluetoothDevice device, ManualResetEvent cancelEvent = null)
        {
            try
            {
                BtGattDisconnect();

                _bluetoothDevice = device;
                _gattConnectionState = ConnectionState.Connecting;
                _btGattSppInStream = new MemoryQueueBufferStream(true);
                _btGattSppOutStream = new BGattOutputStream(this);

                bool connectResult = Task.Run(async () => await ConnectAsync(cancelEvent)).Result;
                if (!connectResult)
                {
                    BtGattDisconnect();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogString($"*** ConnectLeGattDevice exception: {ex.Message}");
                _gattConnectionState = ConnectionState.Disconnected;
                return false;
            }
        }

        private async Task<bool> ConnectAsync(ManualResetEvent cancelEvent)
        {
            try
            {
                // Connect to GATT server
                _bluetoothGatt = _bluetoothDevice.Gatt;
                if (_bluetoothGatt == null)
                {
                    LogString("*** GATT server not available");
                    return false;
                }

                // ConnectAsync returns Task (void), not Task<bool>
                await _bluetoothGatt.ConnectAsync();

                // Check connection status
                if (_bluetoothGatt.IsConnected)
                {
#if DEBUG
                    Debug.WriteLine("Connected to GATT server.");
#endif
                    _gattConnectionState = ConnectionState.Connected;
                }
                else
                {
                    LogString("*** GATT connection failed - not connected after ConnectAsync");
                    return false;
                }

                // Get services
                List<GattService> services = await _bluetoothGatt.GetPrimaryServicesAsync();
                if (services == null || !services.Any())
                {
#if DEBUG
                    Debug.WriteLine("No GATT services found");
#endif
                    LogString("*** No GATT services found");
                    return false;
                }

#if DEBUG
                foreach (GattService gattService in services)
                {
                    Debug.WriteLine($"GATT service: UUID={gattService.Uuid}");
                    try
                    {
                        IReadOnlyList<GattCharacteristic> characteristics = await gattService.GetCharacteristicsAsync();
                        foreach (GattCharacteristic gattCharacteristic in characteristics)
                        {
                            Debug.WriteLine($"GATT characteristic: {gattCharacteristic.Uuid}");
                            Debug.WriteLine($"GATT properties: {gattCharacteristic.Properties}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error getting characteristics: {ex.Message}");
                    }
                }
#endif

                _gattCharacteristicSppRead = null;
                _gattCharacteristicSppWrite = null;

                // Look for known SPP characteristics
                foreach (GattSppInfo gattSppInfo in _gattSppInfoList)
                {
                    GattService gattServiceSpp = services.FirstOrDefault(s => s.Uuid == gattSppInfo.ServiceUuid);
                    if (gattServiceSpp != null)
                    {
                        try
                        {
                            IReadOnlyList<GattCharacteristic> characteristics = await gattServiceSpp.GetCharacteristicsAsync();
                            GattCharacteristic gattCharacteristicSppRead = characteristics.FirstOrDefault(c => c.Uuid == gattSppInfo.CharacteristicReadUuid);
                            GattCharacteristic gattCharacteristicSppWrite = characteristics.FirstOrDefault(c => c.Uuid == gattSppInfo.CharacteristicWriteUuid);

                            if (gattCharacteristicSppRead != null && gattCharacteristicSppWrite != null)
                            {
                                bool validRead = (gattCharacteristicSppRead.Properties & (GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify)) == (GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify) ||
                                                 (gattCharacteristicSppRead.Properties & (GattCharacteristicProperties.Write | GattCharacteristicProperties.Notify)) == (GattCharacteristicProperties.Write | GattCharacteristicProperties.Notify);
                                bool validWrite = (gattCharacteristicSppWrite.Properties & GattCharacteristicProperties.Write) == GattCharacteristicProperties.Write;
                                if (validWrite && gattCharacteristicSppRead != gattCharacteristicSppWrite)
                                {
                                    validWrite = (gattCharacteristicSppWrite.Properties & (GattCharacteristicProperties.Write | GattCharacteristicProperties.Notify)) == GattCharacteristicProperties.Write;
                                }
                                if (validRead && validWrite)
                                {
                                    _gattCharacteristicSppRead = gattCharacteristicSppRead;
                                    _gattCharacteristicSppWrite = gattCharacteristicSppWrite;
#if DEBUG
                                    Debug.WriteLine("SPP characteristic found: " + gattSppInfo.Name);
#endif
                                    LogString("GATT SPP characteristic found: " + gattSppInfo.Name);
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogString($"Error checking known SPP service {gattSppInfo.Name}: {ex.Message}");
                        }
                    }
                }

                // Auto-detect SPP characteristics if not found
                if (_gattCharacteristicSppRead == null || _gattCharacteristicSppWrite == null)
                {
#if DEBUG
                    Debug.WriteLine("No known GATT SPP characteristic start autodetect");
#endif
                    LogString("*** No known GATT SPP characteristic start autodetect");
                    foreach (var gattService in services)
                    {
                        try
                        {
                            LogString($"GATT service: UUID={gattService.Uuid}");
                            GattCharacteristic gattCharacteristicSppRead = null;
                            GattCharacteristic gattCharacteristicSppWrite = null;
                            bool sppValid = true;

                            IReadOnlyList<GattCharacteristic> characteristics = await gattService.GetCharacteristicsAsync();
                            foreach (var gattCharacteristic in characteristics)
                            {
                                LogString($"GATT properties: {gattCharacteristic.Properties}");
                                bool validRead = (gattCharacteristic.Properties & (GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify)) == (GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify) ||
                                                 (gattCharacteristic.Properties & (GattCharacteristicProperties.Write | GattCharacteristicProperties.Notify)) == (GattCharacteristicProperties.Write | GattCharacteristicProperties.Notify);
                                if (validRead)
                                {
                                    if (gattCharacteristicSppRead != null)
                                    {
                                        sppValid = false;
                                        break;
                                    }
                                    gattCharacteristicSppRead = gattCharacteristic;
                                }

                                bool validWrite = (gattCharacteristic.Properties & GattCharacteristicProperties.Write) == GattCharacteristicProperties.Write;
                                if (validWrite && gattCharacteristicSppRead != null && gattCharacteristicSppRead != gattCharacteristic)
                                {
                                    validWrite = (gattCharacteristic.Properties & (GattCharacteristicProperties.Write | GattCharacteristicProperties.Notify)) == (GattCharacteristicProperties.Write);
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
                                Debug.WriteLine("Generic SPP characteristic found");
#endif
                                LogString("GATT generic SPP characteristic found");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogString($"Error checking service {gattService.Uuid}: {ex.Message}");
                        }
                    }
                }

                if (_gattCharacteristicSppRead == null || _gattCharacteristicSppWrite == null)
                {
#if DEBUG
                    Debug.WriteLine("No GATT SPP characteristic found");
#endif
                    LogString("*** No GATT SPP characteristic found");
                    return false;
                }

#if DEBUG
                Debug.WriteLine($"Read UUID: {_gattCharacteristicSppRead.Uuid}, Properties: {_gattCharacteristicSppRead.Properties}");
                Debug.WriteLine($"Write UUID: {_gattCharacteristicSppWrite.Uuid}, Properties: {_gattCharacteristicSppWrite.Properties}");
#endif

                // Enable notifications for read characteristic
                _gattCharacteristicSppRead.CharacteristicValueChanged += OnCharacteristicValueChanged;
                await _gattCharacteristicSppRead.StartNotificationsAsync();
#if false
                byte[] sendData = { 0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x5E };
                _btGattSppOutStream.Write(sendData, 0, sendData.Length);

                while (_btGattReceivedEvent.WaitOne(2000))
                {
#if DEBUG
                    Debug.WriteLine("GATT SPP data received");
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
                    Debug.WriteLine(string.Format("GATT SPP byte: {0:X02}", data));
#endif
                }
#endif
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"ConnectAsync exception: {ex.Message}");
#endif
                LogString($"*** ConnectAsync exception: {ex.Message}");
                return false;
            }
        }

        private void OnCharacteristicValueChanged(object sender, GattCharacteristicValueChangedEventArgs e)
        {
            try
            {
                byte[] value = e.Value;
                if (value != null && value.Length > 0)
                {
#if DEBUG
                    string dataText = Encoding.UTF8.GetString(value);
                    dataText = dataText.Replace("\r", "");
                    Debug.WriteLine($"GATT SPP data received: {BitConverter.ToString(value).Replace("-", "")} '{dataText}'");
#endif
                    _btGattSppInStream?.Write(value, 0, value.Length);
                    _btGattReceivedEvent.Set();
                }
            }
            catch (Exception ex)
            {
                LogString($"OnCharacteristicValueChanged error: {ex.Message}");
            }
        }

        public void BtGattDisconnect()
        {
            try
            {
                if (_gattCharacteristicSppRead != null)
                {
                    try
                    {
                        _gattCharacteristicSppRead.CharacteristicValueChanged -= OnCharacteristicValueChanged;
                        Task.Run(async () => await _gattCharacteristicSppRead.StopNotificationsAsync()).Wait(1000);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    _gattCharacteristicSppRead = null;
                }

                _gattCharacteristicSppWrite = null;
                _gattConnectionState = ConnectionState.Disconnected;

                _bluetoothGatt = null;
                _bluetoothDevice = null;

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

        public class BGattOutputStream : MemoryQueueBufferStream
        {
            private const int MaxWriteLength = 20;
            readonly BtLeGattSpp _btLeGattSpp;

            public BGattOutputStream(BtLeGattSpp btLeGattSpp) : base(true)
            {
                _btLeGattSpp = btLeGattSpp;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                base.Write(buffer, offset, count);

                if (_btLeGattSpp._gattConnectionState != ConnectionState.Connected ||
                    _btLeGattSpp._gattCharacteristicSppWrite == null)
                {
#if DEBUG
                    Debug.WriteLine("Write GATT disconnected");
#endif
                    throw new IOException("GATT disconnected");
                }

                Task.Run(async () => await WriteAsync()).Wait(2000);
            }

            private async Task WriteAsync()
            {
                while (Length > 0)
                {
                    byte[] writeBuffer = new byte[MaxWriteLength];
                    int length = Read(writeBuffer, 0, writeBuffer.Length);
                    if (length <= 0)
                    {
#if DEBUG
                        Debug.WriteLine("Stream write: write chunk failed");
#endif
                        throw new IOException("Stream write: write chunk failed");
                    }

                    byte[] sendData = new byte[length];
                    Array.Copy(writeBuffer, 0, sendData, 0, length);

#if DEBUG
                    Debug.WriteLine($"GATT SPP data write: {BitConverter.ToString(sendData).Replace("-", "")} '{Encoding.UTF8.GetString(sendData)}'");
#endif
                    try
                    {
                        await _btLeGattSpp._gattCharacteristicSppWrite.WriteValueWithResponseAsync(sendData);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Debug.WriteLine($"WriteCharacteristic error: {ex.Message}");
#endif
                        _btLeGattSpp.LogString($"WriteCharacteristic error: {ex.Message}");
                        throw new IOException($"WriteCharacteristic failed: {ex.Message}");
                    }
                }
            }
        }
    }
}
