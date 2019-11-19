/*
* Copyright (C) 2009 The Android Open Source Project
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using EdiabasLib;
using Android.Text.Method;

namespace BmwDeepObd
{

    /// <summary>
    /// This Activity appears as a dialog. It lists any paired devices and
    /// devices detected in the area after discovery. When a device is chosen
    /// by the user, the MAC address of the device is sent back to the parent
    /// Activity in the result Intent.
    /// </summary>
    [Android.App.Activity (Label = "@string/select_device",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                Android.Content.PM.ConfigChanges.Orientation |
                Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DeviceListActivity : AppCompatActivity, View.IOnClickListener
    {
        enum AdapterType
        {
            ConnectionFailed,   // connection to adapter failed
            Unknown,            // unknown adapter
            Elm327,             // ELM327
            Elm327Custom,       // ELM327 with custom firmware
            Elm327Invalid,      // ELM327 invalid type
            Elm327Fake,         // ELM327 fake version
            Elm327FakeOpt,      // ELM327 fake version optional command
            Elm327NoCan,        // ELM327 no CAN support
            Custom,             // custom adapter
            CustomUpdate,       // custom adapter with firmware update
            EchoOnly,           // only echo response
        }

        enum BtOperation
        {
            SelectAdapter,      // select the adapter
            ConnectObd,         // connect device as OBD
            ConnectPhone,       // connect device as phone
            DisconnectPhone,    // dosconnect phone
            DeleteDevice,       // delete device
        }

        public class InstanceData
        {
            public string MtcBtModuleName { get; set; }
            public bool MtcAntennaInfoShown { get; set; }
            public bool MtcBtModuleErrorShown { get; set; }
            public bool MtcErrorShown { get; set; }
            public bool MtcOffline { get; set; }
        }

        private static readonly Java.Util.UUID SppUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private const int ResponseTimeout = 1000;

        // Return Intent extra
#if DEBUG
        private static readonly string Tag = typeof(DeviceListActivity).FullName;
#endif
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraCallAdapterConfig = "adapter_configuration";

        // Member fields
        private InstanceData _instanceData = new InstanceData();
        private BluetoothAdapter _btAdapter;
        private Timer _deviceUpdateTimer;
        private ListView _pairedListView;
        private ArrayAdapter<string> _pairedDevicesArrayAdapter;
        private View _pairedViewClick;
        private ListView _newDevicesListView;
        private ArrayAdapter<string> _newDevicesArrayAdapter;
        private View _newDevicesViewClick;
        private Receiver _receiver;
        private ProgressBar _progressBar;
        private TextView _textViewTitlePairedDevices;
        private Button _scanButton;
        private ActivityCommon _activityCommon;
        private string _appDataDir;
        private readonly StringBuilder _sbLog = new StringBuilder();
        private readonly AutoResetEvent _connectedEvent = new AutoResetEvent(false);
        private volatile string _connectDeviceAddress = string.Empty;
        private volatile bool _deviceConnected;
        private int _elmVerH = -1;
        private int _elmVerL = -1;

        protected override void OnCreate (Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate (savedInstanceState);
            if (savedInstanceState != null)
            {
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            // Setup the window
            SetContentView(Resource.Layout.device_list);

            // Set result CANCELED incase the user backs out
            SetResult (Android.App.Result.Canceled);

            // ReSharper disable once UseObjectOrCollectionInitializer
            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityCommon.MtcBtServiceBound)
                {
                    UpdateMtcDevices();
                }
            },
            (context, intent) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (intent != null && intent.Action == GlobalBroadcastReceiver.NotificationBroadcastAction)
                {
                    if (intent.HasExtra(GlobalBroadcastReceiver.BtScanFinished))
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "BtScanFinished");
#endif
                        ShowScanState(false);
                    }
                }
            });
            _activityCommon.SelectedInterface = ActivityCommon.InterfaceType.Bluetooth;

            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);

            _progressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
            _textViewTitlePairedDevices = FindViewById<TextView>(Resource.Id.title_paired_devices);
            // Initialize the button to perform device discovery
            _scanButton = FindViewById<Button>(Resource.Id.button_scan);
            _scanButton.Click += (sender, e) =>
            {
                if (_activityCommon.MtcBtServiceBound)
                {
                    DoMtcDiscovery();
                }
                else
                {
                    DoDiscovery();
                }
            };

            // Initialize array adapters. One for already paired devices and
            // one for newly discovered devices
            _pairedDevicesArrayAdapter = new ArrayAdapter<string> (this, Resource.Layout.device_name);
            _newDevicesArrayAdapter = new ArrayAdapter<string> (this, Resource.Layout.device_name);

            // Find and set up the ListView for paired devices
            _pairedListView = FindViewById<ListView> (Resource.Id.paired_devices);
            _pairedListView.Adapter = _pairedDevicesArrayAdapter;
            _pairedListView.ItemClick += (sender, args) =>
            {
                DeviceListClick(sender, args, true);
            };
            _pairedViewClick = FindViewById<View>(Resource.Id.paired_devices_click);
            _pairedViewClick.SetOnClickListener(this);

            // Find and set up the ListView for newly discovered devices
            _newDevicesListView = FindViewById<ListView> (Resource.Id.new_devices);
            _newDevicesListView.Adapter = _newDevicesArrayAdapter;
            //pairedListView.SetOnClickListener(this);
            _newDevicesListView.ItemClick += (sender, args) =>
            {
                DeviceListClick(sender, args, false);
            };
            _newDevicesViewClick = FindViewById<View>(Resource.Id.new_devices_click);
            _newDevicesViewClick.SetOnClickListener(this);

            // Register for broadcasts when a device is discovered
            _receiver = new Receiver (this);
            var filter = new IntentFilter (BluetoothDevice.ActionFound);
            RegisterReceiver (_receiver, filter);

            // Register for broadcasts when a device name changed
            filter = new IntentFilter(BluetoothDevice.ActionNameChanged);
            RegisterReceiver(_receiver, filter);

            // Register for broadcasts when discovery has finished
            filter = new IntentFilter (BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver (_receiver, filter);

            // register device changes
            filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionAclConnected);
            filter.AddAction(BluetoothDevice.ActionAclDisconnected);
            RegisterReceiver(_receiver, filter);

            // Get the local Bluetooth adapter
            _btAdapter = BluetoothAdapter.DefaultAdapter;

            // Get a set of currently paired devices
            if (!_activityCommon.MtcBtService)
            {
                UpdatePairedDevices();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            ActivityCommon.StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_activityCommon.MtcBtService)
            {
                _activityCommon.StartMtcService();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_activityCommon.MtcBtService)
            {
                if (_deviceUpdateTimer == null)
                {
                    _deviceUpdateTimer = new Timer(state =>
                    {
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            UpdateMtcDevices();
                        });
                    }, null, 1000, 5000);
                }
            }
            else
            {
                UpdatePairedDevices();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_deviceUpdateTimer != null)
            {
                _deviceUpdateTimer.Dispose();
                _deviceUpdateTimer = null;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (_activityCommon.MtcBtService)
            {
                MtcStopScan();
                _activityCommon.StopMtcService();
            }
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            // Make sure we're not doing discovery anymore
            _btAdapter?.CancelDiscovery ();

            // Unregister broadcast listeners
            UnregisterReceiver (_receiver);
            _activityCommon.Dispose();
            _activityCommon = null;
        }

        public void OnClick(View v)
        {
            MtcManualAddressEntry();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void UpdatePairedDevices()
        {
            // Get a set of currently paired devices
            var pairedDevices = _btAdapter.BondedDevices;

            // If there are paired devices, add each one to the ArrayAdapter
            _pairedDevicesArrayAdapter.Clear();
            if (pairedDevices.Count > 0)
            {
                foreach (var device in pairedDevices)
                {
                    if (device == null)
                    {
                        continue;
                    }
                    try
                    {
                        ParcelUuid[] uuids = device.GetUuids();
                        if ((uuids == null) || (uuids.Any(uuid => SppUuid.CompareTo(uuid.Uuid) == 0)))
                        {
                            _pairedDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            if (_pairedDevicesArrayAdapter.Count == 0)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (_btAdapter.IsEnabled)
                {
                    _pairedDevicesArrayAdapter.Add(Resources.GetText(Resource.String.none_paired));
                }
                else
                {
                    _pairedDevicesArrayAdapter.Add(Resources.GetText(Resource.String.bt_not_enabled));
                }
            }
        }

        private void UpdateMtcDevices()
        {
            _pairedDevicesArrayAdapter.Clear();
            _newDevicesArrayAdapter.Clear();
            if (!_activityCommon.MtcBtServiceBound)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "UpdateMtcDevices: Service not bound");
#endif
                return;
            }
            MtcServiceConnection mtcServiceConnection = _activityCommon.MtcServiceConnection;
            try
            {
                FindViewById<View>(Resource.Id.layout_new_devices).Visibility = ViewStates.Visible;

                bool autoConnect = mtcServiceConnection.GetAutoConnect();
#if DEBUG
                sbyte btState = mtcServiceConnection.GetBtState();
                Android.Util.Log.Info(Tag, string.Format("UpdateMtcDevices: api={0}, time={1:yyyy-MM-dd HH:mm:ss}", mtcServiceConnection.ApiVersion, DateTime.Now));
                Android.Util.Log.Info(Tag, string.Format("BtState: {0}", btState));
                Android.Util.Log.Info(Tag, string.Format("AutoConnect: {0}", autoConnect));
#endif
                bool oldOffline = _instanceData.MtcOffline;
                bool newOffline = false;
                if (!autoConnect)
                {
                    mtcServiceConnection.SetAutoConnect(true);
                    if (!mtcServiceConnection.GetAutoConnect())
                    {
                        newOffline = true;
                    }
                }
                _instanceData.MtcOffline = newOffline;
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("MTC offline: {0}", _instanceData.MtcOffline));
#endif
                if (_instanceData.MtcOffline)
                {
                    if (!_instanceData.MtcErrorShown)
                    {
                        _instanceData.MtcErrorShown = true;
                        _activityCommon.ShowAlert(GetString(Resource.String.bt_mtc_service_error), Resource.String.alert_title_warning);
                    }
                    UpdatePairedDevices();
                    ShowScanState(false);
                    return;
                }

                if (_instanceData.MtcBtModuleName == null)
                {
                    string btModuleName = mtcServiceConnection.CarManagerGetBtModuleName();
                    _instanceData.MtcBtModuleName = btModuleName ?? string.Empty;
                }

                StringBuilder sbTitle = new StringBuilder();
                sbTitle.Append(GetString(Resource.String.title_paired_devices));
                if (!string.IsNullOrEmpty(_instanceData.MtcBtModuleName))
                {
                    sbTitle.Append(" ");
                    sbTitle.Append(string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.bt_module_name), _instanceData.MtcBtModuleName));
                }

                string titlePairedDevices = sbTitle.ToString();
                if (_textViewTitlePairedDevices.Text != titlePairedDevices)
                {
                    _textViewTitlePairedDevices.Text = titlePairedDevices;
                }
#if false
                if (!_instanceData.MtcAntennaInfoShown && !string.IsNullOrEmpty(_instanceData.MtcBtModuleName) &&
                    string.Compare(_instanceData.MtcBtModuleName, "WQ_BC6", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _instanceData.MtcAntennaInfoShown = true;
                    _activityCommon.ShowAlert(GetString(Resource.String.bt_mtc_antenna_info), Resource.String.alert_title_info);
                }
#endif
                if (!_instanceData.MtcBtModuleErrorShown && !string.IsNullOrEmpty(_instanceData.MtcBtModuleName) &&
                    string.Compare(_instanceData.MtcBtModuleName, "SD-GT936", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _instanceData.MtcBtModuleErrorShown = true;
                    _activityCommon.ShowAlert(GetString(Resource.String.bt_mtc_module_error), Resource.String.alert_title_warning);
                }

                if (oldOffline != _instanceData.MtcOffline)
                {
                    ShowScanState(false);
                }

                long nowDevAddr = mtcServiceConnection.GetNowDevAddr();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("NowDevAddr: {0}", nowDevAddr));
#endif
                string nowDevAddrString = string.Format(CultureInfo.InvariantCulture, "{0:X012}", nowDevAddr);
                IList<string> deviceList = mtcServiceConnection.GetDeviceList();
                IList<string> matchList = mtcServiceConnection.GetMatchList();
                int offset = mtcServiceConnection.ApiVersion < 2 ? 0 : 1;

                if (matchList != null)
                {
                    foreach (string device in matchList)
                    {
                        if (string.IsNullOrEmpty(device))
                        {
                            continue;
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("MatchList: device={0}", device));
#endif
                        if (ExtractMtcDeviceInfo(offset, device, out string name, out string address))
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Extracted name={0}, address={1}", name, address));
#endif
                            string mac = address.Replace(":", string.Empty);
                            if (string.Compare(mac, nowDevAddrString, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                name += " " + GetString(Resource.String.bt_device_connected);
                            }
                            _pairedDevicesArrayAdapter.Add(name + "\n" + address);
                        }
                    }
                }
                if (_pairedDevicesArrayAdapter.Count == 0)
                {
                    _pairedDevicesArrayAdapter.Add(Resources.GetText(Resource.String.none_paired));
                }

                if (deviceList != null)
                {
                    foreach (string device in deviceList)
                    {
                        if (string.IsNullOrEmpty(device))
                        {
                            continue;
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("DeviceList: device={0}", device));
#endif
                        if (ExtractMtcDeviceInfo(offset, device, out string name, out string address))
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Extracted name={0}, address={1}", name, address));
#endif
                            _newDevicesArrayAdapter.Add(name + "\n" + address);
                        }
                    }
                }
                if (_newDevicesArrayAdapter.Count == 0)
                {
                    _newDevicesArrayAdapter.Add(Resources.GetText(Resource.String.none_found));
                }
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                // ignored
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("UpdateMtcDevices exception: {0}", (ex.Message ?? string.Empty)));
#endif
            }
        }

        /// <summary>
        /// Extract device info for MTC devices
        /// </summary>
        /// <param name="offset">MAC offset: 0, 1</param>
        /// <param name="device">Complete device info text</param>
        /// <param name="name">Device name</param>
        /// <param name="address">Device address</param>
        /// <returns>True: Success</returns>
        private static bool ExtractMtcDeviceInfo(int offset, string device, out string name, out string address)
        {
            name = string.Empty;
            address = string.Empty;
            if (device.Length < offset + 12)
            {
                return false;
            }
            string mac = device.Substring(offset, 12);
            StringBuilder sb = new StringBuilder();
            address = string.Empty;
            for (int i = 0; i < 12; i += 2)
            {
                if (sb.Length > 0)
                {
                    sb.Append(":");
                }
                sb.Append(mac.Substring(i, 2));
            }
            address = sb.ToString();
            name = device.Substring(offset + 12);
            return true;
        }

        /// <summary>
        /// MTC stop BT scan
        /// </summary>
        /// <returns>True if successful</returns>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool MtcStopScan()
        {
            if (_activityCommon.MtcBtServiceBound)
            {
                try
                {
                    _activityCommon.MtcServiceConnection.ScanStop();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Show scan state
        /// </summary>
        /// <param name="enabled">True if scanning is enabled</param>
        private void ShowScanState(bool enabled)
        {
            if (_activityCommon.MtcBtServiceBound && _instanceData.MtcOffline)
            {
                _progressBar.Visibility = ViewStates.Invisible;
                SetTitle(Resource.String.select_device);
                _scanButton.Enabled = false;
                return;
            }

            if (enabled)
            {
                _progressBar.Visibility = ViewStates.Visible;
                SetTitle(Resource.String.scanning);
                _scanButton.Enabled = false;
            }
            else
            {
                _progressBar.Visibility = ViewStates.Invisible;
                SetTitle(Resource.String.select_device);
                _scanButton.Enabled = true;
            }
        }

        /// <summary>
        /// Start device discover with the BluetoothAdapter
        /// </summary>
        private void DoDiscovery ()
        {
            // Log.Debug (Tag, "doDiscovery()");

            // If we're already discovering, stop it
            if (_btAdapter.IsDiscovering)
            {
                _btAdapter.CancelDiscovery ();
            }
            _newDevicesArrayAdapter.Clear();

            // Request discover from BluetoothAdapter
            if (_btAdapter.StartDiscovery())
            {
                // Indicate scanning in the title
                ShowScanState(true);

                // Turn on area for new devices
                FindViewById<View>(Resource.Id.layout_new_devices).Visibility = ViewStates.Visible;
            }
        }

        /// <summary>
        /// Start MTC device discovery
        /// </summary>
        private void DoMtcDiscovery()
        {
            // Log.Debug (Tag, "doDiscovery()");

            if (_activityCommon.MtcBtServiceBound)
            {
                try
                {
                    _activityCommon.MtcServiceConnection.ScanStart();
                    // Indicate scanning in the title
                    ShowScanState(true);

                    // Turn on area for new devices
                    FindViewById<View>(Resource.Id.layout_new_devices).Visibility = ViewStates.Visible;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Start adapter detection
        /// </summary>
        /// <param name="deviceAddress">Device Bluetooth address</param>
        /// <param name="deviceName">Device Bleutooth name</param>
        private void DetectAdapter(string deviceAddress, string deviceName)
        {
            if (string.IsNullOrEmpty(deviceAddress) || string.IsNullOrEmpty(deviceName))
            {
                return;
            }

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.detect_adapter));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            _sbLog.Clear();
            _deviceConnected = false;

            LogString("Device address: " + deviceAddress);
            LogString("Device name: " + deviceName);

            _activityCommon.ConnectMtcBtDevice(deviceAddress);

            Thread detectThread = new Thread(() =>
            {
                AdapterType adapterType = AdapterType.Unknown;
                try
                {
                    BluetoothDevice device = _btAdapter.GetRemoteDevice(deviceAddress.ToUpperInvariant());
                    if (device != null)
                    {
                        int connectTimeout = _activityCommon.MtcBtService ? 1000 : 2000;
                        _connectDeviceAddress = device.Address;
                        BluetoothSocket bluetoothSocket = null;
                        LogString("Bond state: " + device.BondState);

                        adapterType = AdapterType.ConnectionFailed;
                        if (adapterType == AdapterType.ConnectionFailed)
                        {
                            try
                            {
                                LogString("Connect with CreateRfcommSocketToServiceRecord");
                                bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);
                                if (bluetoothSocket != null)
                                {
                                    try
                                    {
                                        bluetoothSocket.Connect();
                                    }
                                    catch (Exception)
                                    {
                                        // sometimes the second connect is working
                                        bluetoothSocket.Connect();
                                    }
                                    _connectedEvent.WaitOne(connectTimeout, false);
                                    LogString(_deviceConnected ? "Bt device is connected" : "Bt device is not connected");
                                    adapterType = AdapterTypeDetection(bluetoothSocket);
                                    if (_activityCommon.MtcBtService && adapterType == AdapterType.Unknown)
                                    {
                                        for (int retry = 0; retry < 20; retry++)
                                        {
                                            LogString("Retry connect");
                                            bluetoothSocket.Close();
                                            bluetoothSocket.Connect();
                                            adapterType = AdapterTypeDetection(bluetoothSocket);
                                            if (adapterType != AdapterType.Unknown &&
                                                adapterType != AdapterType.ConnectionFailed)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogString("*** Connect exception: " + (ex.Message ?? string.Empty));
                                adapterType = AdapterType.ConnectionFailed;
                            }
                            finally
                            {
                                bluetoothSocket?.Close();
                            }
                        }

                        if (adapterType == AdapterType.ConnectionFailed && !_activityCommon.MtcBtService)
                        {
                            try
                            {
                                LogString("Connect with createRfcommSocket");
                                // this socket sometimes looses data for long telegrams
                                IntPtr createRfcommSocket = Android.Runtime.JNIEnv.GetMethodID(device.Class.Handle,
                                    "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
                                if (createRfcommSocket == IntPtr.Zero)
                                {
                                    throw new Exception("No createRfcommSocket");
                                }
                                IntPtr rfCommSocket = Android.Runtime.JNIEnv.CallObjectMethod(device.Handle,
                                    createRfcommSocket, new Android.Runtime.JValue(1));
                                if (rfCommSocket == IntPtr.Zero)
                                {
                                    throw new Exception("No rfCommSocket");
                                }
                                bluetoothSocket = GetObject<BluetoothSocket>(rfCommSocket,
                                    Android.Runtime.JniHandleOwnership.TransferLocalRef);
                                if (bluetoothSocket != null)
                                {
                                    bluetoothSocket.Connect();
                                    _connectedEvent.WaitOne(connectTimeout, false);
                                    LogString(_deviceConnected ? "Bt device is connected" : "Bt device is not connected");
                                    adapterType = AdapterTypeDetection(bluetoothSocket);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogString("*** Connect exception: " + (ex.Message ?? string.Empty));
                                adapterType = AdapterType.ConnectionFailed;
                            }
                            finally
                            {
                                bluetoothSocket?.Close();
                            }
                        }
                    }
                    else
                    {
                        LogString("*** GetRemoteDevice failed");
                    }
                }
                catch (Exception ex)
                {
                    LogString("*** General exception: " + (ex.Message ?? string.Empty));
                    adapterType = AdapterType.ConnectionFailed;
                }

                if (_sbLog.Length == 0)
                {
                    LogString("Empty log");
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress.Dispose();

                    switch (adapterType)
                    {
                        case AdapterType.ConnectionFailed:
                        {
                            if (_activityCommon.MtcBtService)
                            {
                                AlertDialog alertDialog = new AlertDialog.Builder(this)
                                    .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.adapter_connection_mtc_failed)
                                    .SetTitle(Resource.String.alert_title_error)
                                    .Show();
                                alertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(),
                                        GetType(), (o, eventArgs) => { });
                                };
                                break;
                            }

                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    ReturnDeviceTypeRawWarn(deviceAddress, deviceName);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_connection_failed)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            break;
                        }

                        case AdapterType.Unknown:
                        {
                            if (_activityCommon.MtcBtService)
                            {
                                AlertDialog alertDialog1 = new AlertDialog.Builder(this)
                                    .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.adapter_connection_mtc_failed)
                                    .SetTitle(Resource.String.alert_title_error)
                                    .Show();
                                alertDialog1.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(),
                                        GetType(), (o, eventArgs) => { });
                                };
                                break;
                            }

                            bool yesSelected = false;
                            AlertDialog alertDialog2 = new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    yesSelected = true;
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.unknown_adapter_type)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            alertDialog2.DismissEvent += (sender, args) =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(), GetType(), (o, eventArgs) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        if (yesSelected)
                                        {
                                            ReturnDeviceTypeRawWarn(deviceAddress, deviceName);
                                        }
                                    });
                            };
                            break;
                        }

                        case AdapterType.Elm327:
                        {
                            AlertDialog alertDialog = new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                                })
                                .SetCancelable(true)
                                .SetMessage(ActivityCommon.FromHtml(GetString(Resource.String.adapter_elm_replacement)))
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                            if (messageView != null)
                            {
                                messageView.MovementMethod = new LinkMovementMethod();
                            }
                            break;
                        }

                        case AdapterType.Elm327Custom:
                        {
                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.ElmDeepObdTag, deviceName, true);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.ElmDeepObdTag, deviceName);
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_elm_firmware)
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            break;
                        }

                        case AdapterType.Elm327Invalid:
                        case AdapterType.Elm327Fake:
                        case AdapterType.Elm327FakeOpt:
                        case AdapterType.Elm327NoCan:
                        {
                            bool yesSelected = false;
                            AlertDialog.Builder builder = new AlertDialog.Builder(this);

                            string message;
                            switch (adapterType)
                            {
                                case AdapterType.Elm327Fake:
                                case AdapterType.Elm327FakeOpt:
                                    message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.fake_elm_adapter_type), _elmVerH, _elmVerL);
                                    message += "<br>" + GetString(Resource.String.recommened_adapter_type);
                                    break;

                                case AdapterType.Elm327NoCan:
                                    message = GetString(Resource.String.elm_no_can);
                                    message += "<br>" + GetString(Resource.String.adapter_elm_replacement);
                                    break;

                                default:
                                    message = GetString(Resource.String.invalid_adapter_type);
                                    message += "<br>" + GetString(Resource.String.recommened_adapter_type);
                                    break;
                            }

                            if (adapterType == AdapterType.Elm327FakeOpt)
                            {
                                message += "<br>" + GetString(Resource.String.fake_elm_try);
                                builder.SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    yesSelected = true;
                                });
                                builder.SetNegativeButton(Resource.String.button_no, (sender, args) => { });
                                builder.SetTitle(Resource.String.alert_title_warning);
                            }
                            else
                            {
                                builder.SetNeutralButton(Resource.String.button_ok, (sender, args) => { });
                                builder.SetTitle(Resource.String.alert_title_error);
                            }

                            builder.SetCancelable(true);
                            builder.SetMessage(ActivityCommon.FromHtml(message));

                            AlertDialog alertDialog = builder.Show();
                            alertDialog.DismissEvent += (sender, args) =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(), GetType(), (o, eventArgs) =>
                                {
                                    if (yesSelected)
                                    {
                                        ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                                    }
                                });
                            };
                            TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                            if (messageView != null)
                            {
                                messageView.MovementMethod = new LinkMovementMethod();
                            }
                            break;
                        }

                        case AdapterType.Custom:
                        case AdapterType.CustomUpdate:
                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress, deviceName, true);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress, deviceName);
                                })
                                .SetCancelable(true)
                                .SetMessage(adapterType == AdapterType.CustomUpdate ? Resource.String.adapter_fw_update : Resource.String.adapter_cfg_required)
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            break;

                        case AdapterType.EchoOnly:
                            ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.RawTag, deviceName);
                            break;

                        default:
                            ReturnDeviceType(deviceAddress, deviceName);
                            break;
                    }
                });
            })
            {
                Priority = System.Threading.ThreadPriority.Highest
            };
            detectThread.Start();
        }

        private void ReturnDeviceTypeRawWarn(string deviceAddress, string deviceName)
        {
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.RawTag, deviceName);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.adapter_raw_warn)
                .SetTitle(Resource.String.alert_title_warning)
                .Show();
        }

        /// <summary>
        /// Return specified device type to caller
        /// </summary>
        /// <param name="deviceAddress">Device Bluetooth address</param>
        /// <param name="deviceName">Device Bleutooth name</param>
        /// <param name="adapterConfig">Opend adapter configuration</param>
        private void ReturnDeviceType(string deviceAddress, string deviceName, bool adapterConfig = false)
        {
            // Create the result Intent and include the MAC address
            Intent intent = new Intent();
            intent.PutExtra(ExtraDeviceName, deviceName);
            intent.PutExtra(ExtraDeviceAddress, deviceAddress);
            intent.PutExtra(ExtraCallAdapterConfig, adapterConfig);

            // Set result and finish this Activity
            SetResult(Android.App.Result.Ok, intent);
            Finish();
        }

        /// <summary>
        /// Detects the CAN adapter type
        /// </summary>
        /// <param name="bluetoothSocket">Bluetooth socket for communication</param>
        /// <returns>Adapter type</returns>
        private AdapterType AdapterTypeDetection(BluetoothSocket bluetoothSocket)
        {
            AdapterType adapterType = AdapterType.Unknown;
            _elmVerH = -1;
            _elmVerL = -1;

            try
            {
                Stream bluetoothInStream = bluetoothSocket.InputStream;
                Stream bluetoothOutStream = bluetoothSocket.OutputStream;

                const int versionRespLen = 9;
                byte[] customData = { 0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x5E };
                // custom adapter
                bluetoothInStream.Flush();
                while (bluetoothInStream.IsDataAvailable())
                {
                    bluetoothInStream.ReadByte();
                }
                LogData(customData, 0, customData.Length, "Send");
                bluetoothOutStream.Write(customData, 0, customData.Length);

                LogData(null, 0, 0, "Resp");
                List<byte> responseList = new List<byte>();
                long startTime = Stopwatch.GetTimestamp();
                for (; ; )
                {
                    while (bluetoothInStream.IsDataAvailable())
                    {
                        int data = bluetoothInStream.ReadByte();
                        if (data >= 0)
                        {
                            LogByte((byte)data);
                            responseList.Add((byte)data);
                            startTime = Stopwatch.GetTimestamp();
                        }
                    }
                    if (responseList.Count >= customData.Length + versionRespLen)
                    {
                        LogString("Custom adapter length");
                        bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                        if (!validEcho)
                        {
                            LogString("*** Echo incorrect");
                            break;
                        }
                        byte checkSum = 0x00;
                        for (int i = 0; i < versionRespLen - 1; i++)
                        {
                            checkSum += responseList[i + customData.Length];
                        }
                        if (checkSum != responseList[customData.Length + versionRespLen - 1])
                        {
                            LogString("*** Checksum incorrect");
                            break;
                        }
                        int adapterTypeId = responseList[customData.Length + 5] + (responseList[customData.Length + 4] << 8);
                        int fwVersion = responseList[customData.Length + 7] + (responseList[customData.Length + 6] << 8);
                        int fwUpdateVersion = PicBootloader.GetFirmwareVersion((uint)adapterTypeId);
                        if (fwUpdateVersion >= 0 && fwUpdateVersion > fwVersion)
                        {
                            LogString("Custom adapter with old firmware detected");
                            return AdapterType.CustomUpdate;
                        }
                        LogString("Custom adapter detected");

                        if (adapterTypeId >= 0x0002)
                        {
                            ReadCustomSerial(bluetoothInStream, bluetoothOutStream);
                        }

                        return AdapterType.Custom;
                    }
                    if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                    {
                        if (responseList.Count >= customData.Length)
                        {
                            bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                            if (validEcho)
                            {
                                LogString("Valid echo detected");
                                adapterType = AdapterType.EchoOnly;
                            }
                        }
                        break;
                    }
                }
                LogString("No custom adapter found");

                // ELM327
                bool elmReports2X = false;
                Regex elmVerRegEx = new Regex(@"ELM327\s+v(\d)\.(\d)", RegexOptions.IgnoreCase);
                for (int retries = 0; retries < 2; retries++)
                {
                    bluetoothInStream.Flush();
                    while (bluetoothInStream.IsDataAvailable())
                    {
                        bluetoothInStream.ReadByte();
                    }
                    byte[] sendData = Encoding.UTF8.GetBytes("ATI\r");
                    LogData(sendData, 0, sendData.Length, "Send");
                    bluetoothOutStream.Write(sendData, 0, sendData.Length);

                    string response = GetElm327Reponse(bluetoothInStream);
                    if (response != null)
                    {
                        MatchCollection matchesVer = elmVerRegEx.Matches(response);
                        if ((matchesVer.Count == 1) && (matchesVer[0].Groups.Count == 3))
                        {
                            if (!Int32.TryParse(matchesVer[0].Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _elmVerH))
                            {
                                _elmVerH = -1;
                            }
                            if (!Int32.TryParse(matchesVer[0].Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _elmVerL))
                            {
                                _elmVerL = -1;
                            }
                        }
                        if (_elmVerH >= 0 && _elmVerL >= 0)
                        {
                            LogString(string.Format("ELM327 detected: {0}.{1}", _elmVerH, _elmVerL));
                            if (_elmVerH >= 2)
                            {
                                LogString("Version >= 2.x detected");
                                elmReports2X = true;
                            }
                            adapterType = AdapterType.Elm327;
                            break;
                        }
                    }
                }
                if (adapterType == AdapterType.Elm327)
                {
                    foreach (EdElmInterface.ElmInitEntry elmInitEntry in EdBluetoothInterface.Elm327InitCommands)
                    {
                        bluetoothInStream.Flush();
                        while (bluetoothInStream.IsDataAvailable())
                        {
                            bluetoothInStream.ReadByte();
                        }
                        if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, elmInitEntry.Command, false))
                        {
                            adapterType = AdapterType.Elm327Invalid;
                            break;
                        }

                        string response = GetElm327Reponse(bluetoothInStream);
                        if (response == null)
                        {
                            LogString("*** No ELM response");
                            adapterType = AdapterType.Elm327Invalid;
                            break;
                        }

                        if (elmInitEntry.OkResponse)
                        {
                            if (!response.Contains("OK\r"))
                            {
                                LogString("*** No ELM OK found");
                                bool optional = elmInitEntry.Version >= 0;
                                if (!optional)
                                {
                                    adapterType = AdapterType.Elm327Invalid;
                                    break;
                                }
                                if (elmReports2X && elmInitEntry.Version >= 200)
                                {
                                    LogString("*** ELM command optional, fake 2.X");
                                    adapterType = AdapterType.Elm327FakeOpt;
                                }
                                else
                                {
                                    LogString("*** ELM command optional, fake");
                                }
                            }
                        }
                    }

                    switch (adapterType)
                    {
                        case AdapterType.Elm327Invalid:
                            if (elmReports2X)
                            {
                                adapterType = AdapterType.Elm327Fake;
                            }
                            break;

                        case AdapterType.Elm327:
                        case AdapterType.Elm327FakeOpt:
                        {
                            if (!Elm327CheckCustomFirmware(bluetoothInStream, bluetoothOutStream, out bool customFirmware))
                            {
                                LogString("*** ELM firmware detection failed");
                            }
                            if (customFirmware)
                            {
                                adapterType = AdapterType.Elm327Custom;
                                break;
                            }

                            if (!Elm327CheckCan(bluetoothInStream, bluetoothOutStream, out bool canSupport))
                            {
                                LogString("*** ELM CAN detection failed");
                                adapterType = AdapterType.Elm327Invalid;
                                break;
                            }
                            if (!canSupport)
                            {
                                LogString("*** ELM no vehicle CAN support");
                                adapterType = AdapterType.Elm327NoCan;
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogString("*** Exception: " + (ex.Message ?? string.Empty));
                return AdapterType.ConnectionFailed;
            }
            LogString("Adapter type: " + adapterType);
            return adapterType;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ReadCustomSerial(Stream bluetoothInStream, Stream bluetoothOutStream)
        {
            const int idRespLen = 13;
            byte[] idData = { 0x82, 0xF1, 0xF1, 0xFB, 0xFB, 0x5A };

            LogString("Reading id data");

            LogData(idData, 0, idData.Length, "Send");
            bluetoothOutStream.Write(idData, 0, idData.Length);

            LogData(null, 0, 0, "Resp");
            List<byte> responseList = new List<byte>();
            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                while (bluetoothInStream.IsDataAvailable())
                {
                    int data = bluetoothInStream.ReadByte();
                    if (data >= 0)
                    {
                        LogByte((byte) data);
                        responseList.Add((byte) data);
                        startTime = Stopwatch.GetTimestamp();
                    }
                }

                if (responseList.Count >= idData.Length + idRespLen)
                {
                    LogString("Id data length");
                    bool validEcho = !idData.Where((t, i) => responseList[i] != t).Any();
                    if (!validEcho)
                    {
                        LogString("*** Echo incorrect");
                        break;
                    }

                    byte checkSum = 0x00;
                    for (int i = 0; i < idRespLen - 1; i++)
                    {
                        checkSum += responseList[i + idData.Length];
                    }

                    if (checkSum != responseList[idData.Length + idRespLen - 1])
                    {
                        LogString("*** Checksum incorrect");
                        return false;
                    }
                    byte[] adapterSerial = responseList.GetRange(idData.Length + 4, 8).ToArray();
                    LogString("AdapterSerial: " + BitConverter.ToString(adapterSerial).Replace("-", ""));
                    break;
                }
                if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                {
                    LogString("*** Id data timeout");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check for vehicle can support
        /// </summary>
        /// <param name="bluetoothInStream"></param>
        /// <param name="bluetoothOutStream"></param>
        /// <param name="canSupport">True: CAN supported</param>
        /// <returns></returns>
        private bool Elm327CheckCan(Stream bluetoothInStream, Stream bluetoothOutStream, out bool canSupport)
        {
            canSupport = true;
            Elm327SendCommand(bluetoothInStream, bluetoothOutStream, "ATCTM1");     // standard multiplier
            int timeout = 1000 / 4; // 1sec
            if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, string.Format("ATST{0:X02}", timeout)))
            {
                LogString("*** ELM setting timeout failed");
                return false;
            }

            if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, "0000000000000000", false)) // dummy data
            {
                LogString("*** ELM sending data failed");
                return false;
            }
            string answer = GetElm327Reponse(bluetoothInStream);
            if (answer != null)
            {
                if (answer.Contains("CAN ERROR\r"))
                {
                    LogString("*** ELM CAN error");
                    canSupport = false;
                }
            }

            if (canSupport)
            {
                // fake adapters are not able to send short telegrams
                if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, "00", false))
                {
                    LogString("*** ELM sending data failed");
                    return false;
                }
                answer = GetElm327Reponse(bluetoothInStream);
                if (answer != null)
                {
                    if (answer.Contains("CAN ERROR\r"))
                    {
                        LogString("*** ELM CAN error, fake adapter");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool Elm327CheckCustomFirmware(Stream bluetoothInStream, Stream bluetoothOutStream, out bool customFirmware)
        {
            customFirmware = false;
            bluetoothInStream.Flush();
            while (bluetoothInStream.IsDataAvailable())
            {
                bluetoothInStream.ReadByte();
            }

            if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, @"AT@2", false))
            {
                LogString("*** ELM read device identifier failed");
                return false;
            }

            string answer = GetElm327Reponse(bluetoothInStream);
            if (answer != null)
            {
                if (answer.StartsWith("DEEPOBD"))
                {
                    customFirmware = true;
                }
            }

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (customFirmware)
            {
                LogString("Custom ELM firmware");
            }
            else
            {
                LogString("No custom ELM firmware");
            }

            return true;
        }

        /// <summary>
        /// Send command to EL327
        /// </summary>
        /// <param name="bluetoothInStream">Bluetooth input stream</param>
        /// <param name="bluetoothOutStream">Bluetooth output stream</param>
        /// <param name="command">Command to send</param>
        /// <param name="readAnswer">True: Check for valid answer</param>
        /// <returns>True: command ok</returns>
        private bool Elm327SendCommand(Stream bluetoothInStream, Stream bluetoothOutStream, string command, bool readAnswer = true)
        {
            byte[] sendData = Encoding.UTF8.GetBytes(command + "\r");
            LogData(sendData, 0, sendData.Length, "Send");
            bluetoothOutStream.Write(sendData, 0, sendData.Length);
            LogString("ELM CMD send: " + command);

            if (readAnswer)
            {
                string answer = GetElm327Reponse(bluetoothInStream);
                if (answer == null)
                {
                    LogString("*** No ELM response");
                    return false;
                }
                // check for OK
                if (!answer.Contains("OK\r"))
                {
                    LogString("*** ELM invalid response");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get response from EL327
        /// </summary>
        /// <param name="bluetoothInStream">Bluetooth input stream</param>
        /// <returns>Response string, null for no reponse</returns>
        private string GetElm327Reponse(Stream bluetoothInStream)
        {
            LogData(null, 0, 0, "Resp");
            string response = null;
            StringBuilder stringBuilder = new StringBuilder();
            long startTime = Stopwatch.GetTimestamp();
            bool lengthMessage = false;
            for (; ; )
            {
                while (bluetoothInStream.IsDataAvailable())
                {
                    int data = bluetoothInStream.ReadByte();
                    if (data >= 0 && data != 0x00)
                    {
                        // remove 0x00
                        LogByte((byte)data);
                        stringBuilder.Append(Convert.ToChar(data));
                        startTime = Stopwatch.GetTimestamp();
                    }
                    if (data == 0x3E)
                    {
                        // prompt
                        response = stringBuilder.ToString();
                        break;
                    }
                    if (stringBuilder.Length > 500)
                    {
                        if (!lengthMessage)
                        {
                            lengthMessage = true;
                            LogString("*** ELM response too long");
                        }
                        break;
                    }
                }
                if (response != null)
                {
                    break;
                }
                if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                {
                    LogString("*** ELM response timeout");
                    break;
                }
            }
            if (response == null)
            {
                LogString("*** No ELM prompt");
            }
            else
            {
                LogString("ELM CMD rec: " + response.Replace("\r", "").Replace(">", ""));
            }
            return response;
        }

        /// <summary>
        /// The on-click listener for all devices in the ListViews
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        private void DeviceListClick(object sender, AdapterView.ItemClickEventArgs e, bool paired)
        {
            if (_activityCommon.MtcBtServiceBound)
            {
                MtcStopScan();
            }
            else
            {
                // Cancel discovery because it's costly and we're about to connect
                if (_btAdapter.IsDiscovering)
                {
                    _btAdapter.CancelDiscovery();
                }
            }
            ShowScanState(false);

            if (e.View is TextView textView)
            {
                string info = textView.Text;
                if (!ExtractDeviceInfo(info, out string name, out string address))
                {
                    MtcManualAddressEntry();
                    return;
                }

                if (_activityCommon.MtcBtServiceBound && !_instanceData.MtcOffline)
                {
                    SelectMtcDeviceAction(name, address, paired);
                }
                else
                {
                    DetectAdapter(address, name);
                }
            }
        }

        /// <summary>
        /// Manual Bluettoth address entry in MTC mode
        /// </summary>
        private void MtcManualAddressEntry()
        {
            if (!_activityCommon.MtcBtService)
            {
                return;
            }

            TextInputDialog textInputDialog = new TextInputDialog(this);
            textInputDialog.Message = GetString(Resource.String.bt_device_enter_mac);
            textInputDialog.MessageDetail = string.Empty;
            textInputDialog.Text = "00:19:5D:24:B7:64";
            textInputDialog.SetPositiveButton(Resource.String.button_ok, (s, arg) =>
            {
                string address = textInputDialog.Text.Trim().ToUpperInvariant();
                if (!BluetoothAdapter.CheckBluetoothAddress(address))
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.bt_device_mac_invalid), Resource.String.alert_title_error);
                    return;
                }
                string name = address;
                DetectAdapter(address, name);
            });
            textInputDialog.SetNegativeButton(Resource.String.button_abort, (s, arg) =>
            {
            });
            textInputDialog.Show();
        }

        /// <summary>
        /// Select action for device in MTC mode
        /// </summary>
        private void SelectMtcDeviceAction(string name, string address, bool paired)
        {
            if (!_activityCommon.MtcBtServiceBound)
            {
                return;
            }
            string mac = address.Replace(":", string.Empty);
            long nowDevAddr = _activityCommon.MtcServiceConnection.GetNowDevAddr();
            string nowDevAddrString = string.Format(CultureInfo.InvariantCulture, "{0:X012}", nowDevAddr);
            bool connectedPhone = string.Compare(nowDevAddrString, mac, StringComparison.OrdinalIgnoreCase) == 0;

            List<BtOperation> operationList = new List<BtOperation>();
            List<string> itemList = new List<string>();
            if (paired && !connectedPhone)
            {
                itemList.Add(GetString(Resource.String.bt_device_select));
                operationList.Add(BtOperation.SelectAdapter);
            }
            if (!paired)
            {
                itemList.Add(GetString(Resource.String.bt_device_connect_obd));
                operationList.Add(BtOperation.ConnectObd);
            }
            if (!connectedPhone)
            {
                itemList.Add(GetString(Resource.String.bt_device_connect_phone));
                operationList.Add(BtOperation.ConnectPhone);
            }
            if (paired && connectedPhone)
            {
                itemList.Add(GetString(Resource.String.bt_device_disconnect_phone));
                operationList.Add(BtOperation.DisconnectPhone);
            }
            itemList.Add(GetString(Resource.String.bt_device_delete));
            operationList.Add(BtOperation.DeleteDevice);

            Java.Lang.ICharSequence[] items = new Java.Lang.ICharSequence[itemList.Count];
            for (int i = 0; i < itemList.Count; i++)
            {
                items[i] = new Java.Lang.String(itemList[i]);
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.bt_device_menu_tite);
            builder.SetItems(items, (sender, args) =>
                {
                    if (!_activityCommon.MtcBtServiceBound)
                    {
                        return;
                    }
                    if (args.Which < 0 || args.Which >= operationList.Count)
                    {
                        return;
                    }
                    try
                    {
                        switch (operationList[args.Which])
                        {
                            case BtOperation.SelectAdapter:
                                if (!_activityCommon.MtcBtConnected)
                                {
                                    new AlertDialog.Builder(this)
                                        .SetMessage(Resource.String.mtc_disconnect_warn)
                                        .SetTitle(Resource.String.alert_title_warning)
                                        .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                                        {
                                            DetectAdapter(address, name);
                                        })
                                        .SetNegativeButton(Resource.String.button_no, (s, e) =>
                                        {
                                        })
                                        .Show();
                                    break;
                                }
                                DetectAdapter(address, name);
                                break;

                            case BtOperation.ConnectObd:
                                _activityCommon.MtcServiceConnection.ConnectObd(mac);
                                break;

                            case BtOperation.ConnectPhone:
                                _activityCommon.MtcServiceConnection.ConnectBt(mac);
                                break;

                            case BtOperation.DisconnectPhone:
                                _activityCommon.MtcServiceConnection.DisconnectBt(mac);
                                break;

                            case BtOperation.DeleteDevice:
                                _activityCommon.MtcServiceConnection.DeleteBt(mac);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
            builder.Show();
        }

        /// <summary>
        /// Extract device info from name
        /// </summary>
        /// <param name="info">Complete device info text</param>
        /// <param name="name">Device name</param>
        /// <param name="address">Device address</param>
        private static bool ExtractDeviceInfo(string info, out string name, out string address)
        {
            string[] parts = info.Split('\n');
            if (parts.Length < 2)
            {
                name = string.Empty;
                address = string.Empty;
                return false;
            }
            name = parts[0];
            address = parts[1];
            return true;
        }

        private void LogData(byte[] data, int offset, int length, string info = null)
        {
            if (!string.IsNullOrEmpty(info))
            {
                if (_sbLog.Length > 0)
                {
                    _sbLog.Append("\n");
                }
                _sbLog.Append(" (");
                _sbLog.Append(info);
                _sbLog.Append("): ");
            }
            if (data != null)
            {
                for (int i = 0; i < length; i++)
                {
                    _sbLog.Append(string.Format(ActivityMain.Culture, "{0:X02} ", data[offset + i]));
                }
            }
        }

        private void LogString(string info)
        {
            if (_sbLog.Length > 0)
            {
                _sbLog.Append("\n");
            }
            _sbLog.Append(info);
        }

        private void LogByte(byte data)
        {
            _sbLog.Append(string.Format(ActivityMain.Culture, "{0:X02} ", data));
        }

        public class Receiver : BroadcastReceiver
        {
            readonly DeviceListActivity _chat;

            public Receiver(DeviceListActivity chat)
            {
                _chat = chat;
            }

            public override void OnReceive (Context context, Intent intent)
            {
                try
                {
                    string action = intent.Action;

                    switch (action)
                    {
                        case BluetoothDevice.ActionFound:
                        case BluetoothDevice.ActionNameChanged:
                        {
                            // Get the BluetoothDevice object from the Intent
                            BluetoothDevice device = (BluetoothDevice) intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                            // If it's already paired, skip it, because it's been listed already
                            if (device.BondState != Bond.Bonded)
                            {
                                ParcelUuid[] uuids = device.GetUuids();
                                if ((uuids == null) || (uuids.Any(uuid => SppUuid.CompareTo(uuid.Uuid) == 0)))
                                {
                                    // check for multiple entries
                                    int index = -1;
                                    for (int i = 0; i < _chat._newDevicesArrayAdapter.Count; i++)
                                    {
                                        string item = _chat._newDevicesArrayAdapter.GetItem(i);
                                        if (!ExtractDeviceInfo(_chat._newDevicesArrayAdapter.GetItem(i), out string _, out string address))
                                        {
                                            return;
                                        }
                                        if (string.Compare(address, device.Address, StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            _chat._newDevicesArrayAdapter.Remove(item);
                                            index = i;
                                            break;
                                        }
                                    }
                                    string newName = device.Name + "\n" + device.Address;
                                    if (index < 0)
                                    {
                                        _chat._newDevicesArrayAdapter.Add(newName);
                                    }
                                    else
                                    {
                                        _chat._newDevicesArrayAdapter.Insert(newName, index);
                                    }
                                }
                            }
                            break;
                        }

                        case BluetoothAdapter.ActionDiscoveryFinished:
                            // When discovery is finished, change the Activity title
                            _chat.ShowScanState(false);
                            if (_chat._newDevicesArrayAdapter.Count == 0)
                            {
                                _chat._newDevicesArrayAdapter.Add(_chat.Resources.GetText(Resource.String.none_found));
                            }
                            break;

                        case BluetoothDevice.ActionAclConnected:
                        case BluetoothDevice.ActionAclDisconnected:
                        {
                            BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                            if (device != null)
                            {
                                if (!string.IsNullOrEmpty(_chat._connectDeviceAddress) &&
                                        string.Compare(device.Address, _chat._connectDeviceAddress, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    _chat._deviceConnected = action == BluetoothDevice.ActionAclConnected;
                                    _chat._connectedEvent.Set();
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class SelectListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        private readonly Context _context;

        // ReSharper disable once UnusedMember.Global
        public SelectListener(Context context)
        {
            _context = context;
        }

        public void OnClick(IDialogInterface dialog, Int32 which)
        {
            switch (which)
            {
                case 1:
                    Toast.MakeText(_context, "Button1", ToastLength.Long);
                    break;

                case 2:
                    Toast.MakeText(_context, "Button2", ToastLength.Long);
                    break;

                case 3:
                    Toast.MakeText(_context, "Button3", ToastLength.Long);
                    break;

                case 4:
                    Toast.MakeText(_context, "Button4", ToastLength.Long);
                    break;
            }
        }
    }
}
