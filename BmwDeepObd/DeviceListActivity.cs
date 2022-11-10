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
using Android.Views;
using Android.Widget;
using EdiabasLib;
using Android.Text.Method;
using Android.Content.PM;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace BmwDeepObd
{

    /// <summary>
    /// This Activity appears as a dialog. It lists any paired devices and
    /// devices detected in the area after discovery. When a device is chosen
    /// by the user, the MAC address of the device is sent back to the parent
    /// Activity in the result Intent.
    /// </summary>
    [Android.App.Activity (Label = "@string/select_device",
        Name = ActivityCommon.AppNameSpace + "." + nameof(DeviceListActivity),
        ConfigurationChanges = ActivityConfigChanges)]
    public class DeviceListActivity : BaseActivity, View.IOnClickListener
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
            Elm327Limited,      // ELM327 limited support
            StnFwUpdate,        // STN fimrware update required
            Elm327NoCan,        // ELM327 no CAN support
            Custom,             // custom adapter
            CustomNoEscape,     // custom adapter with no escape support
            CustomUpdate,       // custom adapter with firmware update
            EchoOnly,           // only echo response
        }

        enum BtOperation
        {
            SelectAdapter,          // select the adapter
            SelectAdapterSecure,    // select the adapter secure
            ConnectObd,             // connect device as OBD
            ConnectPhone,           // connect device as phone
            DisconnectPhone,        // disconnect phone
            DeleteDevice,           // delete device
        }

        public class InstanceData
        {
            public bool BtPermissionWarningShown { get; set; }
            public bool LocationProviderShown { get; set; }
            public bool MtcAntennaInfoShown { get; set; }
            public bool MtcBtModuleErrorShown { get; set; }
            public bool MtcBtPwdMismatchShown { get; set; }
            public bool MtcBtEscapeModeShown { get; set; }
            public bool MtcErrorShown { get; set; }
            public bool MtcOffline { get; set; }
        }

        private const string ObdLinkPackageName = "OCTech.Mobile.Applications.OBDLink";
        private static readonly Java.Util.UUID SppUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly Java.Util.UUID ZeroUuid = Java.Util.UUID.FromString("00000000-0000-0000-0000-000000000000");
        private const string DefaultModulePwd = "1234";
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
        private BtLeGattSpp _btLeGattSpp;
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
        private bool _btPermissionRequested;
        private bool _btPermissionGranted;
        private int _elmVerH = -1;
        private int _elmVerL = -1;

        private enum ActivityRequest
        {
            RequestBluetoothSettings,
            RequestLocationSettings,
            RequestAppDetailSettings,
        }

        protected override void OnCreate (Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate (savedInstanceState);
            _allowFullScreenMode = false;
            if (savedInstanceState != null)
            {
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
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
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            RegisterReceiver(_receiver, filter);

            // Get the local Bluetooth adapter
            _btAdapter = _activityCommon.BtAdapter;
            _btLeGattSpp = new BtLeGattSpp(LogString);

            // Get a set of currently paired devices
            if (!_activityCommon.MtcBtService)
            {
                UpdatePairedDevices();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _btPermissionRequested = false;
            _btPermissionGranted = false;

            if (_activityCommon != null)
            {
                if (_activityCommon.MtcBtService)
                {
                    _activityCommon.StartMtcService();
                }
                _activityCommon?.RequestUsbPermission(null);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            RequestBtPermissions();
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
            if (_activityCommon != null && _activityCommon.MtcBtService)
            {
                MtcStopScan();
                _activityCommon.StopMtcService();
            }
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            // Make sure we're not doing discovery anymore
            try
            {
                _btAdapter?.CancelDiscovery();
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                // ignored
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("OnDestroy exception: {0}", EdiabasNet.GetExceptionText(ex)));
#endif
            }

            if (_btLeGattSpp != null)
            {
                _btLeGattSpp.Dispose();
                _btLeGattSpp = null;
            }

            // Unregister broadcast listeners
            UnregisterReceiver(_receiver);
            _activityCommon?.Dispose();
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

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestBluetoothSettings:
                    UpdatePairedDevices();
                    break;

                case ActivityRequest.RequestLocationSettings:
                    break;

                case ActivityRequest.RequestAppDetailSettings:
                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (_activityCommon == null)
            {
                return;
            }

            switch (requestCode)
            {
                case ActivityCommon.RequestPermissionBluetooth:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        BtPermissionGranted();
                        break;
                    }

                    if (!_instanceData.BtPermissionWarningShown)
                    {
                        _instanceData.BtPermissionWarningShown = true;
                        new AlertDialog.Builder(this)
                            .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                            {
                                ActivityCommon.OpenAppSettingDetails(this, (int) ActivityRequest.RequestAppDetailSettings);
                            })
                            .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                            {
                            })
                            .SetCancelable(true)
                            .SetMessage(Resource.String.access_permission_rejected)
                            .SetTitle(Resource.String.alert_title_warning)
                            .Show();
                    }
                    break;
            }
        }

        private void RequestBtPermissions()
        {
            if (_activityCommon.MtcBtService)
            {
                return;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            {
                return;
            }

            if (_btPermissionRequested || _btPermissionGranted)
            {
                return;
            }

            string[] requestPermissions = Build.VERSION.SdkInt < BuildVersionCodes.S ? ActivityCommon.PermissionsFineLocation : ActivityCommon.PermissionsBluetooth;
            if (requestPermissions.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
            {
                BtPermissionGranted();
                return;
            }

            _btPermissionRequested = true;
            ActivityCompat.RequestPermissions(this, requestPermissions, ActivityCommon.RequestPermissionBluetooth);
        }

        private void BtPermissionGranted()
        {
            _btPermissionGranted = true;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && Build.VERSION.SdkInt < BuildVersionCodes.S)
            {
                if (_activityCommon.LocationManager != null)
                {
                    try
                    {
                        if (!_activityCommon.LocationManager.IsLocationEnabled)
                        {
                            if (!_instanceData.LocationProviderShown)
                            {
                                _instanceData.LocationProviderShown = true;
                                new AlertDialog.Builder(this)
                                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                    {
                                        ActivityCommon.OpenLocationSettings(this, (int)ActivityRequest.RequestLocationSettings);
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.location_provider_disabled_bt)
                                    .SetTitle(Resource.String.alert_title_warning)
                                    .Show();
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

        private void UpdatePairedDevices()
        {
            try
            {
                // If there are paired devices, add each one to the ArrayAdapter
                _pairedDevicesArrayAdapter.Clear();

                // Get a set of currently paired devices
                ICollection<BluetoothDevice> pairedDevices = _btAdapter.BondedDevices;
                if (pairedDevices?.Count > 0)
                {
                    foreach (BluetoothDevice device in pairedDevices)
                    {
                        if (device == null)
                        {
                            continue;
                        }
                        try
                        {
                            if (IsBtDeviceValid(device))
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
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                // ignored
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("UpdatePairedDevices exception: {0}", EdiabasNet.GetExceptionText(ex)));
#endif
            }
        }

        private static bool IsBtDeviceValid(BluetoothDevice device)
        {
            ParcelUuid[] uuids = device?.GetUuids();
            List<Java.Util.UUID> uuidList = new List<Java.Util.UUID>();
            if (uuids != null)
            {
                foreach (ParcelUuid parcelUuid in uuids)
                {
                    if (parcelUuid.Uuid != null && ZeroUuid.CompareTo(parcelUuid.Uuid) != 0)
                    {
                        uuidList.Add(parcelUuid.Uuid);
                    }
                }
            }

            return uuidList.Count == 0 || uuidList.Any(uuid => SppUuid.CompareTo(uuid) == 0);
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
                string modulePwd = mtcServiceConnection.GetModulePassword() ?? string.Empty;
#if DEBUG
                string moduleName = mtcServiceConnection.GetModuleName() ?? string.Empty;
                string btPin = mtcServiceConnection.CarManagerGetBtPin() ?? string.Empty;
                string btName = mtcServiceConnection.CarManagerGetBtName() ?? string.Empty;
                sbyte btState = mtcServiceConnection.GetBtState();
                Android.Util.Log.Info(Tag, string.Format("UpdateMtcDevices: api={0}, time={1:yyyy-MM-dd HH:mm:ss}", mtcServiceConnection.ApiVersion, DateTime.Now));
                Android.Util.Log.Info(Tag, string.Format("BtState: {0}", btState));
                Android.Util.Log.Info(Tag, string.Format("AutoConnect: {0}", autoConnect));
                Android.Util.Log.Info(Tag, string.Format("Module Pwd: {0}", modulePwd));
                Android.Util.Log.Info(Tag, string.Format("Module Name: {0}", moduleName));
                Android.Util.Log.Info(Tag, string.Format("Bt Pin: {0}", btPin));
                Android.Util.Log.Info(Tag, string.Format("Bt Name: {0}", btName));
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

                StringBuilder sbTitle = new StringBuilder();
                sbTitle.Append(GetString(Resource.String.title_paired_devices));
                if (!string.IsNullOrEmpty(_activityCommon.MtcBtModuleName))
                {
                    sbTitle.Append(" ");
                    sbTitle.Append(string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.bt_module_name), _activityCommon.MtcBtModuleName));
                }

                string titlePairedDevices = sbTitle.ToString();
                if (_textViewTitlePairedDevices.Text != titlePairedDevices)
                {
                    _textViewTitlePairedDevices.Text = titlePairedDevices;
                }
#if false
                if (!_instanceData.MtcAntennaInfoShown && !string.IsNullOrEmpty(_activityCommon.MtcBtModuleName) &&
                    string.Compare(_activityCommon.MtcBtModuleName, "WQ_BC6", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _instanceData.MtcAntennaInfoShown = true;
                    _activityCommon.ShowAlert(GetString(Resource.String.bt_mtc_antenna_info), Resource.String.alert_title_info);
                }
#endif
#if false
                if (!_instanceData.MtcBtModuleErrorShown && !string.IsNullOrEmpty(_activityCommon.MtcBtModuleName) &&
                    string.Compare(_activityCommon.MtcBtModuleName, "SD-GT936", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _instanceData.MtcBtModuleErrorShown = true;
                    _activityCommon.ShowAlert(GetString(Resource.String.bt_mtc_module_error), Resource.String.alert_title_warning);
                }
#endif

                if (!_instanceData.MtcBtPwdMismatchShown &&
                    !string.IsNullOrEmpty(modulePwd) && string.Compare(modulePwd, DefaultModulePwd, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    _instanceData.MtcBtPwdMismatchShown = true;
                    string message = string.Format(GetString(Resource.String.bt_mtc_module_pwd), modulePwd, DefaultModulePwd);
                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            try
                            {
                                mtcServiceConnection.SetModulePassword(DefaultModulePwd);
                            }
                            catch (Exception)
                            {
                                _instanceData.MtcBtPwdMismatchShown = false;
                            }
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();
                }
                else
                {
                    if (!_instanceData.MtcBtEscapeModeShown && _activityCommon.MtcBtEscapeMode)
                    {
                        _instanceData.MtcBtEscapeModeShown = true;
                        string message = string.Format(GetString(Resource.String.bt_mtc_module_escape_mode), _activityCommon.MtcBtModuleName);
                        _activityCommon.ShowAlert(message, Resource.String.alert_title_info);
                    }
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
                Android.Util.Log.Info(Tag, string.Format("UpdateMtcDevices exception: {0}", EdiabasNet.GetExceptionText(ex)));
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

            try
            {
                _newDevicesArrayAdapter.Clear();

                // If we're already discovering, stop it
                if (_btAdapter.IsDiscovering)
                {
                    _btAdapter.CancelDiscovery();
                }

                // Request discover from BluetoothAdapter
                if (_btAdapter.StartDiscovery())
                {
                    // Indicate scanning in the title
                    ShowScanState(true);

                    // Turn on area for new devices
                    FindViewById<View>(Resource.Id.layout_new_devices).Visibility = ViewStates.Visible;
                }
                else
                {
                    try
                    {
                        Intent intent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);
                        StartActivityForResult(intent, (int)ActivityRequest.RequestBluetoothSettings);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                // ignored
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("DoDiscovery Exception: {0}", EdiabasNet.GetExceptionText(ex)));
#endif
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
        /// <param name="deviceName">Device Bluetooth name</param>
        /// <param name="forceSecure">Force secure connection</param>
        private void DetectAdapter(string deviceAddress, string deviceName, bool forceSecure = false)
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
            LogString("Escape mode: " + (_activityCommon.MtcBtEscapeMode ? "1" : "0"));
            if (!string.IsNullOrEmpty(_activityCommon.MtcBtModuleName))
            {
                LogString("Bt module: " + _activityCommon.MtcBtModuleName);
            }

            _activityCommon.ConnectMtcBtDevice(deviceAddress);

            Thread detectThread = new Thread(() =>
            {
                AdapterType adapterType = AdapterType.Unknown;
                try
                {
                    BluetoothDevice device = _btAdapter.GetRemoteDevice(deviceAddress.ToUpperInvariant());
                    if (device != null)
                    {
                        bool mtcBtService = _activityCommon.MtcBtService;
                        int connectTimeout = mtcBtService ? 1000 : 2000;
                        _connectDeviceAddress = device.Address;
                        BluetoothSocket bluetoothSocket = null;
                        LogString("Device bond state: " + device.BondState);
                        LogString("Device type: " + device.Type);

                        adapterType = AdapterType.ConnectionFailed;
                        if (!mtcBtService && _btLeGattSpp != null)
                        {
                            if (device.Type == BluetoothDeviceType.Le || (device.Type == BluetoothDeviceType.Dual && device.BondState == Bond.None && !forceSecure))
                            {
                                try
                                {
                                    if (!_btLeGattSpp.ConnectLeGattDevice(this, device))
                                    {
                                        LogString("Connect to LE GATT device failed");
                                    }
                                    else
                                    {
                                        LogString("Connect to LE GATT device success");
                                        adapterType = AdapterTypeDetection(_btLeGattSpp.BtGattSppInStream, _btLeGattSpp.BtGattSppOutStream);
                                    }
                                }
                                finally
                                {
                                    _btLeGattSpp.BtGattDisconnect();
                                }
                            }
                        }

                        if (adapterType == AdapterType.ConnectionFailed)
                        {
                            try
                            {
                                if (forceSecure || mtcBtService || device.BondState == Bond.Bonded)
                                {
                                    LogString("Connect with CreateRfcommSocketToServiceRecord");
                                    bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);
                                }
                                else
                                {
                                    LogString("Connect with CreateInsecureRfcommSocketToServiceRecord");
                                    bluetoothSocket = device.CreateInsecureRfcommSocketToServiceRecord(SppUuid);
                                }

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

                                    if (_connectedEvent.WaitOne(connectTimeout, false))
                                    {
                                        Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
                                    }
                                    LogString(_deviceConnected ? "Bt device is connected" : "Bt device is not connected");
                                    adapterType = AdapterTypeDetection(bluetoothSocket.InputStream, bluetoothSocket.OutputStream);
                                    if (mtcBtService && adapterType == AdapterType.Unknown)
                                    {
                                        for (int retry = 0; retry < 20; retry++)
                                        {
                                            LogString("Retry connect");
                                            bluetoothSocket.Close();
                                            bluetoothSocket.Connect();
                                            if (_connectedEvent.WaitOne(connectTimeout, false))
                                            {
                                                Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
                                            }

                                            LogString(_deviceConnected ? "Bt device is connected" : "Bt device is not connected");
                                            adapterType = AdapterTypeDetection(bluetoothSocket.InputStream, bluetoothSocket.OutputStream);
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
                                LogString("*** Connect exception: " + EdiabasNet.GetExceptionText(ex));
                                adapterType = AdapterType.ConnectionFailed;
                            }
                            finally
                            {
                                bluetoothSocket?.Close();
                            }
                        }

                        if (adapterType == AdapterType.ConnectionFailed && !mtcBtService)
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
                                    if (_connectedEvent.WaitOne(connectTimeout, false))
                                    {
                                        Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
                                    }

                                    LogString(_deviceConnected ? "Bt device is connected" : "Bt device is not connected");
                                    adapterType = AdapterTypeDetection(bluetoothSocket.InputStream, bluetoothSocket.OutputStream);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogString("*** Connect exception: " + EdiabasNet.GetExceptionText(ex));
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
                    LogString("*** General exception: " + EdiabasNet.GetExceptionText(ex));
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
                                if (alertDialog != null)
                                {
                                    alertDialog.DismissEvent += (sender, args) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(),
                                            GetType(), (o, eventArgs) => { });
                                    };
                                }
                                break;
                            }

                            new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
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
                                if (alertDialog1 != null)
                                {
                                    alertDialog1.DismissEvent += (sender, args) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(),
                                            GetType(), (o, eventArgs) => { });
                                    };
                                }
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
                            if (alertDialog2 != null)
                            {
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
                            }
                            break;
                        }

                        case AdapterType.Elm327:
                        case AdapterType.Elm327Limited:
                        {
                            string message;
                            switch (adapterType)
                            {
                                case AdapterType.Elm327Limited:
                                    message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.limited_elm_adapter_type), _elmVerH, _elmVerL);
                                    message += "<br>" + GetString(Resource.String.recommened_adapter_type);
                                    break;

                                default:
                                    message = GetString(Resource.String.adapter_elm_replacement);
                                    break;
                            }

                            AlertDialog alertDialog = new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                                })
                                .SetCancelable(true)
                                .SetMessage(ActivityCommon.FromHtml(message))
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            if (alertDialog != null)
                            {
                                TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                                if (messageView != null)
                                {
                                    messageView.MovementMethod = new LinkMovementMethod();
                                }
                            }
                            break;
                        }

                        case AdapterType.StnFwUpdate:
                        {
                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    _activityCommon.StartApp(ObdLinkPackageName, true);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_stn_firmware)
                                .SetTitle(Resource.String.alert_title_warning)
                                .Show();
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
                            if (alertDialog != null)
                            {
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

                        case AdapterType.CustomNoEscape:
                            new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_no_escape_mode)
                                .SetTitle(Resource.String.alert_title_error)
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
        /// <param name="bluetoothInStream">Bluetooth input stream</param>
        /// <param name="bluetoothOutStream">Bluetooth output stream</param>
        /// <returns>Adapter type</returns>
        private AdapterType AdapterTypeDetection(Stream bluetoothInStream, Stream bluetoothOutStream)
        {
            AdapterType adapterType = AdapterType.Unknown;
            _elmVerH = -1;
            _elmVerL = -1;

            try
            {
                const int minIgnitionRespLen = 6;
                byte[] customData = { 0x82, 0xF1, 0xF1, 0xFE, 0xFE, 0x00 }; // ignition state
                customData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(customData, 0, customData.Length - 1);
                // custom adapter
                bluetoothInStream.Flush();
                while (bluetoothInStream.HasData())
                {
                    bluetoothInStream.ReadByteAsync();
                }
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("Send: {0}", BitConverter.ToString(customData).Replace("-", " ")));
#endif
                LogData(customData, 0, customData.Length, "Send");
                bluetoothOutStream.Write(customData, 0, customData.Length);

                LogData(null, 0, 0, "Resp");
                List<byte> responseList = new List<byte>();
                long startTime = Stopwatch.GetTimestamp();
                for (; ; )
                {
                    while (bluetoothInStream.HasData())
                    {
                        int data = bluetoothInStream.ReadByteAsync();
                        if (data >= 0)
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Rec: {0:X02}", data));
#endif
                            LogByte((byte)data);
                            responseList.Add((byte)data);
                            startTime = Stopwatch.GetTimestamp();
                        }
                    }

                    if (responseList.Count >= customData.Length + minIgnitionRespLen &&
                        responseList.Count >= customData.Length + (responseList[customData.Length] & 0x3F) + 3)
                    {
                        LogString("Custom adapter length");
                        bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                        if (!validEcho)
                        {
                            LogString("*** Echo incorrect");
                            break;
                        }

                        if (responseList.Count > customData.Length)
                        {
                            byte[] addResponse = responseList.GetRange(customData.Length, responseList.Count - customData.Length).ToArray();
                            if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                            {
                                LogString("*** Checksum incorrect");
                                break;
                            }
                        }

                        LogString("Ignition response ok");
                        LogString("Escape mode: " + (_activityCommon.MtcBtEscapeMode ? "1" : "0"));
                        if (!string.IsNullOrEmpty(_activityCommon.MtcBtModuleName))
                        {
                            LogString("Bt module: " + _activityCommon.MtcBtModuleName);
                        }

                        bool escapeMode = _activityCommon.MtcBtEscapeMode;
                        BtEscapeStreamReader inStream = new BtEscapeStreamReader(bluetoothInStream);
                        BtEscapeStreamWriter outStream = new BtEscapeStreamWriter(bluetoothOutStream);
                        if (!SetCustomEscapeMode(inStream, outStream, ref escapeMode, out bool noEscapeSupport))
                        {
                            LogString("*** Set escape mode failed");
                        }

                        inStream.SetEscapeMode(escapeMode);
                        outStream.SetEscapeMode(escapeMode);

                        if (!ReadCustomFwVersion(inStream, outStream, out int adapterTypeId, out int fwVersion))
                        {
                            LogString("*** Read firmware version failed");
                            if (noEscapeSupport && _activityCommon.MtcBtEscapeMode)
                            {
                                LogString("Custom adapter with no escape mode support");
                                return AdapterType.CustomNoEscape;
                            }
                            break;
                        }
                        LogString(string.Format("AdapterType: {0}", adapterTypeId));
                        LogString(string.Format("AdapterVersion: {0}.{1}", fwVersion >> 8, fwVersion & 0xFF));

                        if (adapterTypeId >= 0x0002)
                        {
                            if (ReadCustomSerial(inStream, outStream, out byte[] adapterSerial))
                            {
                                LogString("AdapterSerial: " + BitConverter.ToString(adapterSerial).Replace("-", ""));
                            }
                        }

                        int fwUpdateVersion = PicBootloader.GetFirmwareVersion((uint)adapterTypeId);
                        if (fwUpdateVersion >= 0 && fwUpdateVersion > fwVersion)
                        {
                            LogString("Custom adapter with old firmware detected");
                            return AdapterType.CustomUpdate;
                        }
                        LogString("Custom adapter detected");

                        return AdapterType.Custom;
                    }
                    if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                    {
                        if (responseList.Count >= customData.Length)
                        {
                            bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                            if (validEcho)
                            {
                                if (responseList.Count > customData.Length)
                                {
                                    byte[] addResponse = responseList.GetRange(customData.Length, responseList.Count - customData.Length).ToArray();
                                    if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[addResponse.Length - 1])
                                    {
                                        LogString("*** Additional response checksum incorrect");
                                        break;
                                    }
                                }

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
                Regex elmVerRegEx = new Regex(@"ELM327\s+v(\d+)\.(\d+)", RegexOptions.IgnoreCase);
                for (int retries = 0; retries < 2; retries++)
                {
                    bluetoothInStream.Flush();
                    while (bluetoothInStream.HasData())
                    {
                        bluetoothInStream.ReadByteAsync();
                    }

                    string command = "ATI\r";
                    byte[] sendData = Encoding.UTF8.GetBytes(command);
                    LogData(sendData, 0, sendData.Length, "Send");
                    bluetoothOutStream.Write(sendData, 0, sendData.Length);
                    LogString("ELM CMD send: " + command);

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
                            LogString(string.Format("ELM327 version detected: {0}.{1}", _elmVerH, _elmVerL));
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
                        while (bluetoothInStream.HasData())
                        {
                            bluetoothInStream.ReadByteAsync();
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

                            if (!Elm327CheckCompatibility(bluetoothInStream, bluetoothOutStream, out bool restricted, out bool fwUpdate))
                            {
                                LogString("*** ELM not compatible");
                                adapterType = AdapterType.Elm327Fake;
                                break;
                            }

                            if (restricted)
                            {
                                adapterType = AdapterType.Elm327Limited;
                            }
                            else if (fwUpdate)
                            {
                                adapterType = AdapterType.StnFwUpdate;
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
                LogString("*** Exception: " + EdiabasNet.GetExceptionText(ex));
                return AdapterType.ConnectionFailed;
            }
            LogString("Adapter type: " + adapterType);
            return adapterType;
        }

        private bool SetCustomEscapeMode(BtEscapeStreamReader inStream, BtEscapeStreamWriter outStream, ref bool escapeMode, out bool noEscapeSupport)
        {
            const int escapeRespLen = 8;
            byte escapeModeValue = (byte) ((escapeMode ? 0x03 : 0x00) ^ EdCustomAdapterCommon.EscapeXor);
            byte[] escapeData = { 0x84, 0xF1, 0xF1, 0x06, escapeModeValue,
                EdCustomAdapterCommon.EscapeCodeDefault ^ EdCustomAdapterCommon.EscapeXor, EdCustomAdapterCommon.EscapeMaskDefault ^ EdCustomAdapterCommon.EscapeXor, 0x00 };
            escapeData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(escapeData, 0, escapeData.Length - 1);

            LogString(string.Format("Set escape mode: {0}", escapeMode));

            noEscapeSupport = false;

            LogData(escapeData, 0, escapeData.Length, "Send");
            outStream.Write(escapeData, 0, escapeData.Length);

            LogData(null, 0, 0, "Resp");
            List<byte> responseList = new List<byte>();
            long startTime = Stopwatch.GetTimestamp();
            for (; ; )
            {
                while (inStream.HasData())
                {
                    int data = inStream.ReadByte();
                    if (data >= 0)
                    {
                        LogByte((byte)data);
                        responseList.Add((byte)data);
                        startTime = Stopwatch.GetTimestamp();
                    }
                }

                if (responseList.Count >= escapeData.Length + escapeRespLen)
                {
                    LogString("Escape mode length");
                    bool validEcho = !escapeData.Where((t, i) => responseList[i] != t).Any();
                    if (!validEcho)
                    {
                        LogString("*** Echo incorrect");
                        break;
                    }

                    byte[] addResponse = responseList.GetRange(escapeData.Length, responseList.Count - escapeData.Length).ToArray();
                    if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                    {
                        LogString("*** Checksum incorrect");
                        escapeMode = false;
                        return false;
                    }

                    if (responseList[escapeData.Length + 4] != escapeModeValue)
                    {
                        LogString("*** Escape mode incorrect");
                        escapeMode = false;
                        return false;
                    }

                    if (escapeMode)
                    {
                        if (responseList[escapeData.Length + 5] != (EdCustomAdapterCommon.EscapeCodeDefault ^ EdCustomAdapterCommon.EscapeXor))
                        {
                            LogString("*** Escape code incorrect");
                            escapeMode = false;
                            return false;
                        }

                        if (responseList[escapeData.Length + 6] != (EdCustomAdapterCommon.EscapeMaskDefault ^ EdCustomAdapterCommon.EscapeXor))
                        {
                            LogString("*** Escape mask incorrect");
                            escapeMode = false;
                            return false;
                        }
                    }

                    break;
                }
                if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                {
                    if (responseList.Count == escapeData.Length)
                    {
                        bool validEcho = !escapeData.Where((t, i) => responseList[i] != t).Any();
                        if (validEcho)
                        {
                            LogString("Escape mode echo correct");
                            escapeMode = false;
                            noEscapeSupport = true;
                            break;
                        }
                    }

                    LogString("*** Escape mode timeout");
                    escapeMode = false;
                    return false;
                }
            }
            return true;
        }

        private bool ReadCustomFwVersion(BtEscapeStreamReader inStream, BtEscapeStreamWriter outStream, out int adapterTypeId, out int fwVersion)
        {
            adapterTypeId = -1;
            fwVersion = -1;
            const int fwRespLen = 9;
            byte[] fwData = { 0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x00 };
            fwData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(fwData, 0, fwData.Length - 1);

            LogString("Reading firmware version");

            LogData(fwData, 0, fwData.Length, "Send");
            outStream.Write(fwData, 0, fwData.Length);

            LogData(null, 0, 0, "Resp");
            List<byte> responseList = new List<byte>();
            long startTime = Stopwatch.GetTimestamp();
            for (; ; )
            {
                while (inStream.HasData())
                {
                    int data = inStream.ReadByte();
                    if (data >= 0)
                    {
                        LogByte((byte)data);
                        responseList.Add((byte)data);
                        startTime = Stopwatch.GetTimestamp();
                    }
                }

                if (responseList.Count >= fwData.Length + fwRespLen)
                {
                    LogString("FW data length");
                    bool validEcho = !fwData.Where((t, i) => responseList[i] != t).Any();
                    if (!validEcho)
                    {
                        LogString("*** Echo incorrect");
                        break;
                    }

                    byte[] addResponse = responseList.GetRange(fwData.Length, responseList.Count - fwData.Length).ToArray();
                    if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                    {
                        LogString("*** Checksum incorrect");
                        return false;
                    }

                    adapterTypeId = responseList[fwData.Length + 5] + (responseList[fwData.Length + 4] << 8);
                    fwVersion = responseList[fwData.Length + 7] + (responseList[fwData.Length + 6] << 8);
                    break;
                }
                if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                {
                    LogString("*** FW data timeout");
                    return false;
                }
            }
            return true;
        }

        private bool ReadCustomSerial(BtEscapeStreamReader inStream, BtEscapeStreamWriter outStream, out byte[] adapterSerial)
        {
            adapterSerial = null;
            const int idRespLen = 13;
            byte[] idData = { 0x82, 0xF1, 0xF1, 0xFB, 0xFB, 0x00 };
            idData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(idData, 0, idData.Length - 1);

            LogString("Reading id data");

            LogData(idData, 0, idData.Length, "Send");
            outStream.Write(idData, 0, idData.Length);

            LogData(null, 0, 0, "Resp");
            List<byte> responseList = new List<byte>();
            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                while (inStream.HasData())
                {
                    int data = inStream.ReadByte();
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

                    byte[] addResponse = responseList.GetRange(idData.Length, responseList.Count - idData.Length).ToArray();
                    if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                    {
                        LogString("*** Checksum incorrect");
                        return false;
                    }

                    adapterSerial = responseList.GetRange(idData.Length + 4, 8).ToArray();
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
            while (bluetoothInStream.HasData())
            {
                bluetoothInStream.ReadByteAsync();
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

        private bool Elm327CheckCompatibility(Stream bluetoothInStream, Stream bluetoothOutStream, out bool restricted, out bool fwUpdate)
        {
            restricted = false;
            fwUpdate = false;
            bluetoothInStream.Flush();
            while (bluetoothInStream.HasData())
            {
                bluetoothInStream.ReadByteAsync();
            }

            if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, @"AT@1", false))
            {
                LogString("*** ELM read device description failed");
                return false;
            }

            string elmDevDesc = GetElm327Reponse(bluetoothInStream);
            if (elmDevDesc != null)
            {
                LogString(string.Format("ELM ID: {0}", elmDevDesc));
                if (elmDevDesc.ToUpperInvariant().Contains(EdElmInterface.Elm327CarlyIdentifier))
                {
                    restricted = true;
                }
            }

            if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, @"AT#1", false))
            {
                LogString("*** ELM read manufacturer failed");
                return false;
            }

            string elmManufact = GetElm327Reponse(bluetoothInStream);
            if (elmManufact != null)
            {
                LogString(string.Format("ELM Manufacturer: {0}", elmManufact));
                if (elmManufact.ToUpperInvariant().Contains(EdElmInterface.Elm327WgSoftIdentifier))
                {
                    if (elmDevDesc != null)
                    {
                        string verString = elmDevDesc.Trim('\r', '\n', '>', ' ');
                        if (double.TryParse(verString, NumberStyles.Float, CultureInfo.InvariantCulture, out double version))
                        {
                            if (version < EdElmInterface.Elm327WgSoftMinVer)
                            {
                                restricted = true;
                            }
                        }
                    }
                }
            }

            if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, @"STI", false))
            {
                LogString("*** STN read firmware version failed");
                return false;
            }

            string stnVers = GetElm327Reponse(bluetoothInStream);
            if (stnVers != null)
            {
                //stnVers = "STN1100 v1.2.3";
                LogString(string.Format("STN Version: {0}", stnVers));
                Regex stnVerRegEx = new Regex(@"STN(\d+)\s+v(\d+)\.(\d+)\.(\d+)", RegexOptions.IgnoreCase);
                MatchCollection matchesVer = stnVerRegEx.Matches(stnVers);
                if ((matchesVer.Count == 1) && (matchesVer[0].Groups.Count == 5))
                {
                    if (!Int32.TryParse(matchesVer[0].Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnType))
                    {
                        stnType = -1;
                    }
                    if (!Int32.TryParse(matchesVer[0].Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnVerH))
                    {
                        stnVerH = -1;
                    }
                    if (!Int32.TryParse(matchesVer[0].Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnVerM))
                    {
                        stnVerM = -1;
                    }
                    if (!Int32.TryParse(matchesVer[0].Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnVerL))
                    {
                        stnVerL = -1;
                    }

                    if (stnType >= 0 && stnVerL >= 0 && stnVerM >= 0 && stnVerH >= 0)
                    {
                        LogString(string.Format("STN{0:0000} version detected: {1}.{2}.{3}", stnType, stnVerH, stnVerM, stnVerL));
                        int stnVer = stnVerH * 10000 + stnVerM * 100 + stnVerL;
                        if (stnVer < 50107)
                        {
                            fwUpdate = true;
                        }
                    }
                }
            }

            if (!restricted)
            {
                if (!Elm327SendCommand(bluetoothInStream, bluetoothOutStream, @"ATPP2COFF"))
                {
                    LogString("*** ELM ATPP2COFF failed, fake device");
                    return false;
                }
            }

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (restricted)
            {
                LogString("Restricted ELM firmware");
            }
            else
            {
                LogString("Standard ELM firmware");
            }

            if (fwUpdate)
            {
                LogString("STN firmware update required");
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
                while (bluetoothInStream.HasData())
                {
                    int data = bluetoothInStream.ReadByteAsync();
                    if (data >= 0 && data != 0x00)
                    {
                        // remove 0x00
                        LogByte((byte)data);
                        stringBuilder.Append(EdElmInterface.ConvertToChar(data));
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
                    SelectBtDeviceAction(name, address, paired);
                }
            }
        }

        /// <summary>
        /// Manual Bluetooth address entry in MTC mode
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
        /// Select action for Blutooth device in standard mode
        /// </summary>
        private void SelectBtDeviceAction(string name, string address, bool paired)
        {
            bool showMenu = !paired;

            try
            {
                BluetoothDevice device = _btAdapter.GetRemoteDevice(address.ToUpperInvariant());
                if (device != null && device.Type == BluetoothDeviceType.Le)
                {
                    showMenu = false;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (!showMenu)
            {
                DetectAdapter(address, name);
                return;
            }

            List<BtOperation> operationList = new List<BtOperation>();
            List<string> itemList = new List<string>();
            itemList.Add(GetString(Resource.String.bt_device_select_secure));
            operationList.Add(BtOperation.SelectAdapterSecure);

            itemList.Add(GetString(Resource.String.bt_device_select));
            operationList.Add(BtOperation.SelectAdapter);

            Java.Lang.ICharSequence[] items = new Java.Lang.ICharSequence[itemList.Count];
            for (int i = 0; i < itemList.Count; i++)
            {
                items[i] = new Java.Lang.String(itemList[i]);
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.bt_device_menu_tite);
            builder.SetItems(items, (sender, args) =>
            {
                if (args.Which < 0 || args.Which >= operationList.Count)
                {
                    return;
                }
                try
                {
                    switch (operationList[args.Which])
                    {
                        case BtOperation.SelectAdapterSecure:
                            DetectAdapter(address, name, true);
                            break;

                        case BtOperation.SelectAdapter:
                            DetectAdapter(address, name);
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
                            // ReSharper disable once UsePatternMatching
                            BluetoothDevice device = intent.GetParcelableExtraType<BluetoothDevice>(BluetoothDevice.ExtraDevice);
                            // If it's already paired, skip it, because it's been listed already
                            if (device != null && device.BondState != Bond.Bonded)
                            {
                                if (IsBtDeviceValid(device))
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
                            // ReSharper disable once UsePatternMatching
                            BluetoothDevice device = intent.GetParcelableExtraType<BluetoothDevice>(BluetoothDevice.ExtraDevice);
                            if (device != null)
                            {
                                if (!string.IsNullOrEmpty(_chat._connectDeviceAddress) &&
                                        string.Compare(device.Address, _chat._connectDeviceAddress, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    bool connected = action == BluetoothDevice.ActionAclConnected;
                                    _chat._deviceConnected = connected;
                                    if (connected)
                                    {
                                        _chat._connectedEvent.Set();
                                    }
                                }
                            }
                            break;
                        }

                        case BluetoothDevice.ActionBondStateChanged:
                        {
                            BluetoothDevice device = intent.GetParcelableExtraType<BluetoothDevice>(BluetoothDevice.ExtraDevice);
                            if (device != null)
                            {
                                if (device.BondState == Bond.Bonded)
                                {
                                    for (int i = 0; i < _chat._newDevicesArrayAdapter.Count; i++)
                                    {
                                        string item = _chat._newDevicesArrayAdapter.GetItem(i);
                                        if (ExtractDeviceInfo(_chat._newDevicesArrayAdapter.GetItem(i), out string _, out string address))
                                        {
                                            if (string.Compare(address, device.Address, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                _chat._newDevicesArrayAdapter.Remove(item);
                                            }
                                        }
                                    }

                                    _chat.UpdatePairedDevices();
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
}
