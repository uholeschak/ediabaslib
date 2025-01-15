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
using BmwDeepObd.Dialogs;

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
        enum BtOperation
        {
            SelectAdapter,          // select the adapter
            PairDevice,             // pair device
            UnpairDevice,           // unpair device
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
            public bool MtcFirmwareErrorShown { get; set; }
            public bool MtcBtPwdMismatchShown { get; set; }
            public bool MtcBtEscapeModeShown { get; set; }
            public bool MtcErrorShown { get; set; }
            public bool MtcOffline { get; set; }
        }

        private static readonly Java.Util.UUID SppUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly Java.Util.UUID ZeroUuid = Java.Util.UUID.FromString("00000000-0000-0000-0000-000000000000");
        private const string DefaultModulePwd = "1234";
        private const string InvalidHtcFwName = "hct2.20221115";
        private const string InvalidHtcFwUpdateLink = "https://xtrons.ibus-app.de/index.php?title=Aktuelle_Firmware#Xtrons_Android_12.0_Octacore_Xtrons_ROM_IQ-Serie_[Qualcomm_665]";
        private const string InvalidHtcFwHowToLink = "https://xtrons.ibus-app.de/index.php?title=Firmwareupdate";

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
        private AdapterTypeDetect _adapterTypeDetect;
        private Thread _detectThread;
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
        private Button _btSettingsButton;
        private ActivityCommon _activityCommon;
        private string _appDataDir;
        private ManualResetEvent _transmitCancelEvent;
        private AutoResetEvent _connectedEvent;
        private volatile string _connectDeviceAddress = string.Empty;
        private bool _btPermissionRequested;
        private bool _btPermissionGranted;

        private enum ActivityRequest
        {
            RequestBluetoothSettings,
            RequestLocationSettings,
            RequestAppDetailSettings,
        }

        protected override void OnCreate (Bundle savedInstanceState)
        {
            SetTheme();
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

            _transmitCancelEvent = new ManualResetEvent(false);
            _connectedEvent = new AutoResetEvent(false);
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

            _btSettingsButton = FindViewById<Button>(Resource.Id.button_bt_settings);
            _btSettingsButton.Visibility = _activityCommon.MtcBtService ? ViewStates.Gone : ViewStates.Visible;
            _btSettingsButton.Click += (sender, e) =>
            {
                OpenBluetoothSettings();
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
            RegisterReceiver (_receiver, filter);   // system broadcasts

            // Register for broadcasts when a device name changed
            filter = new IntentFilter(BluetoothDevice.ActionNameChanged);
            RegisterReceiver(_receiver, filter);   // system broadcasts

            // Register for broadcasts when discovery has finished
            filter = new IntentFilter (BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver (_receiver, filter);   // system broadcasts

            // register device changes
            filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionAclConnected);
            filter.AddAction(BluetoothDevice.ActionAclDisconnected);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            RegisterReceiver(_receiver, filter);   // system broadcasts

            _adapterTypeDetect = new AdapterTypeDetect(_activityCommon);
            // Get the local Bluetooth adapter
            _btAdapter = _activityCommon.BtAdapter;
            _btLeGattSpp = new BtLeGattSpp(LogString);

            // Get a set of currently paired devices
            if (!_activityCommon.MtcBtService)
            {
                UpdatePairedDevices();
            }
        }

        private void _btSettingsButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
            _transmitCancelEvent.Reset();

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
            DisposeTimer();
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

            if (IsJobRunning())
            {
                _detectThread?.Join();
            }

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

            DisposeTimer();
            if (_btLeGattSpp != null)
            {
                _btLeGattSpp.Dispose();
                _btLeGattSpp = null;
            }

            // Unregister broadcast listeners
            UnregisterReceiver(_receiver);
            if (_activityCommon != null)
            {
                _activityCommon.Dispose();
                _activityCommon = null;
            }

            if (_connectedEvent != null)
            {
                _connectedEvent.Dispose();
                _connectedEvent = null;
            }

            if (_transmitCancelEvent != null)
            {
                _transmitCancelEvent.Dispose();
                _transmitCancelEvent = null;
            }
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

        public override bool IsFinishAllowed()
        {
            if (IsJobRunning())
            {
                return false;
            }

            return true;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        private void RequestBtPermissions()
        {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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
                            _pairedDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
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

        private void DisposeTimer()
        {
            if (_deviceUpdateTimer != null)
            {
                _deviceUpdateTimer.Dispose();
                _deviceUpdateTimer = null;
            }
        }

        private static bool IsBtDeviceValid(BluetoothDevice device)
        {
            ParcelUuid[] uuids = device?.GetUuids();
            List<Java.Util.UUID> uuidList = new List<Java.Util.UUID>();
            if (uuids != null)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "IsBtDeviceValid: Name: {0}", device.Name ?? string.Empty);
#endif
                foreach (ParcelUuid parcelUuid in uuids)
                {
                    if (parcelUuid.Uuid != null && ZeroUuid.CompareTo(parcelUuid.Uuid) != 0)
                    {
                        uuidList.Add(parcelUuid.Uuid);
#if DEBUG
                        Android.Util.Log.Info(Tag, "IsBtDeviceValid: UUID: {0}", parcelUuid.Uuid.ToString());
#endif
                    }
                }
            }

            if (uuidList.Count == 0)
            {
                return true;
            }

            return uuidList.Any(uuid => SppUuid.CompareTo(uuid) == 0);
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
                    string.Compare(_activityCommon.MtcBtModuleName, "FSC-BW124", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _instanceData.MtcBtModuleErrorShown = true;
                    _activityCommon.ShowAlert(GetString(Resource.String.bt_mtc_module_error), Resource.String.alert_title_warning);
                }
#endif
                if (!_instanceData.MtcFirmwareErrorShown && !string.IsNullOrEmpty(Build.Fingerprint))
                {
                    if (Build.Fingerprint.Contains(InvalidHtcFwName, StringComparison.OrdinalIgnoreCase))
                    {
                        _instanceData.MtcFirmwareErrorShown = true;
                        string message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.bt_mtc_firmware_error), InvalidHtcFwName, InvalidHtcFwUpdateLink, InvalidHtcFwHowToLink);
                        AlertDialog alertDialog = new AlertDialog.Builder(this)
                            .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                            .SetCancelable(true)
                            .SetMessage(ActivityCommon.FromHtml(message))
                            .SetTitle(Resource.String.alert_title_warning)
                            .Show();
                        if (alertDialog != null)
                        {
                            TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                            if (messageView != null)
                            {
                                messageView.MovementMethod = new LinkMovementMethod();
                            }
                        }
                    }
                }

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
                _btSettingsButton.Enabled = false;
                return;
            }

            if (enabled)
            {
                _progressBar.Visibility = ViewStates.Visible;
                SetTitle(Resource.String.scanning);
                _scanButton.Enabled = false;
                _btSettingsButton.Enabled = false;
            }
            else
            {
                _progressBar.Visibility = ViewStates.Invisible;
                SetTitle(Resource.String.select_device);
                _scanButton.Enabled = true;
                _btSettingsButton.Enabled = true;
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
                    OpenBluetoothSettings();
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

        private bool OpenBluetoothSettings()
        {
            try
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);
                StartActivityForResult(intent, (int)ActivityRequest.RequestBluetoothSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
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
        private void DetectAdapter(string deviceAddress, string deviceName)
        {
            if (string.IsNullOrEmpty(deviceAddress) || string.IsNullOrEmpty(deviceName))
            {
                return;
            }

            if (IsJobRunning())
            {
                return;
            }

            _transmitCancelEvent.Reset();
            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.detect_adapter));
            progress.ButtonAbort.Visibility = ViewStates.Visible;
            progress.ButtonAbort.Enabled = false;
            progress.AbortClick = sender =>
            {
                try
                {
                    _transmitCancelEvent.Set();
                }
                catch (Exception)
                {
                    // ignored
                }
            };
            progress.Show();

            _adapterTypeDetect.SbLog.Clear();

            LogString("Device address: " + deviceAddress);
            LogString("Device name: " + deviceName);
            LogString("Escape mode: " + (_activityCommon.MtcBtEscapeMode ? "1" : "0"));
            if (!string.IsNullOrEmpty(_activityCommon.MtcBtModuleName))
            {
                LogString("Bt module: " + _activityCommon.MtcBtModuleName);
            }

            _activityCommon.ConnectMtcBtDevice(deviceAddress);

            _detectThread = new Thread(() =>
            {
                AdapterTypeDetect.AdapterType adapterType = AdapterTypeDetect.AdapterType.Unknown;
                try
                {
                    BluetoothDevice device = _btAdapter.GetRemoteDevice(deviceAddress.ToUpperInvariant());
                    if (device != null)
                    {
                        CustomProgressDialog progressLocal = progress;
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }

                            progressLocal.ButtonAbort.Enabled = true;
                        });

                        bool mtcBtService = _activityCommon.MtcBtService;
                        int connectTimeout = mtcBtService ? 1000 : 2000;
                        _connectDeviceAddress = device.Address;
                        BluetoothSocket bluetoothSocket = null;
                        Stream bluetoothInStream = null;
                        Stream bluetoothOutStream = null;
                        Bond bondState = Bond.None;
                        BluetoothDeviceType deviceType = BluetoothDeviceType.Unknown;

                        if (!mtcBtService)
                        {
                            try
                            {
                                bondState = device.BondState;
                                deviceType = device.Type;
                                LogString("Device bond state: " + bondState);
                                LogString("Device type: " + deviceType);
                            }
                            catch (Exception ex)
                            {
                                LogString("*** Device state exception: " + EdiabasNet.GetExceptionText(ex));
                            }
                        }

                        adapterType = AdapterTypeDetect.AdapterType.ConnectionFailed;
                        if (!mtcBtService && _btLeGattSpp != null)
                        {
                            if (deviceType == BluetoothDeviceType.Le || (deviceType == BluetoothDeviceType.Dual && bondState == Bond.None))
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
                                        adapterType = _adapterTypeDetect.AdapterTypeDetection(_btLeGattSpp.BtGattSppInStream, _btLeGattSpp.BtGattSppOutStream, _transmitCancelEvent);
                                    }
                                }
                                finally
                                {
                                    _btLeGattSpp.BtGattDisconnect();
                                }
                            }
                        }

                        if (adapterType == AdapterTypeDetect.AdapterType.ConnectionFailed)
                        {
                            try
                            {
                                if (mtcBtService || bondState == Bond.Bonded)
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
                                    if (!BluetoothConnect(bluetoothSocket))
                                    {
                                        // sometimes the second connect is working
                                        if (!BluetoothConnect(bluetoothSocket))
                                        {
                                            throw new Exception("Bt connect failed");
                                        }
                                    }

                                    if (_transmitCancelEvent.WaitOne(0))
                                    {
                                        throw new Exception("Aborted");
                                    }

                                    if (_connectedEvent.WaitOne(connectTimeout))
                                    {
                                        Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
                                    }

                                    LogString(bluetoothSocket.IsConnected ? "Bt device is connected" : "Bt device is not connected");
                                    bluetoothInStream = bluetoothSocket.InputStream;
                                    bluetoothOutStream = bluetoothSocket.OutputStream;
                                    adapterType = _adapterTypeDetect.AdapterTypeDetection(bluetoothInStream, bluetoothOutStream, _transmitCancelEvent);
                                    if (mtcBtService && adapterType == AdapterTypeDetect.AdapterType.Unknown)
                                    {
                                        for (int retry = 0; retry < 20; retry++)
                                        {
                                            LogString("Retry connect");

                                            bluetoothInStream?.Close();
                                            bluetoothOutStream?.Close();
                                            bluetoothSocket.Close();

                                            bluetoothInStream = null;
                                            bluetoothOutStream = null;

                                            bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid);
                                            if (!BluetoothConnect(bluetoothSocket))
                                            {
                                                throw new Exception("Bt connect failed");
                                            }

                                            if (_transmitCancelEvent.WaitOne(0))
                                            {
                                                throw new Exception("Aborted");
                                            }

                                            if (_connectedEvent.WaitOne(connectTimeout))
                                            {
                                                Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
                                            }

                                            LogString(bluetoothSocket.IsConnected ? "Bt device is connected" : "Bt device is not connected");
                                            bluetoothInStream = bluetoothSocket.InputStream;
                                            bluetoothOutStream = bluetoothSocket.OutputStream;
                                            adapterType = _adapterTypeDetect.AdapterTypeDetection(bluetoothInStream, bluetoothOutStream, _transmitCancelEvent);
                                            if (adapterType != AdapterTypeDetect.AdapterType.Unknown &&
                                                adapterType != AdapterTypeDetect.AdapterType.ConnectionFailed)
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
                                adapterType = AdapterTypeDetect.AdapterType.ConnectionFailed;
                            }
                            finally
                            {
                                bluetoothInStream?.Close();
                                bluetoothOutStream?.Close();
                                bluetoothSocket?.Close();

                                bluetoothInStream = null;
                                bluetoothOutStream = null;
                            }
                        }

                        if (adapterType == AdapterTypeDetect.AdapterType.ConnectionFailed && !mtcBtService)
                        {
                            try
                            {
                                LogString("Connect with createRfcommSocket");
                                // this socket sometimes looses data for long telegrams
                                nint createRfcommSocket = Android.Runtime.JNIEnv.GetMethodID(device.Class.Handle,
                                    "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
                                if (createRfcommSocket == nint.Zero)
                                {
                                    throw new Exception("No createRfcommSocket");
                                }
                                nint rfCommSocket = Android.Runtime.JNIEnv.CallObjectMethod(device.Handle,
                                    createRfcommSocket, new Android.Runtime.JValue(1));
                                if (rfCommSocket == nint.Zero)
                                {
                                    throw new Exception("No rfCommSocket");
                                }
                                bluetoothSocket = GetObject<BluetoothSocket>(rfCommSocket,
                                    Android.Runtime.JniHandleOwnership.TransferLocalRef);
                                if (bluetoothSocket != null)
                                {
                                    if (!BluetoothConnect(bluetoothSocket))
                                    {
                                        throw new Exception("Bt connect failed");
                                    }

                                    if (_transmitCancelEvent.WaitOne(0))
                                    {
                                        throw new Exception("Aborted");
                                    }

                                    if (_connectedEvent.WaitOne(connectTimeout))
                                    {
                                        Thread.Sleep(EdBluetoothInterface.BtConnectDelay);
                                    }

                                    LogString(bluetoothSocket.IsConnected ? "Bt device is connected" : "Bt device is not connected");
                                    bluetoothInStream = bluetoothSocket.InputStream;
                                    bluetoothOutStream = bluetoothSocket.OutputStream;
                                    adapterType = _adapterTypeDetect.AdapterTypeDetection(bluetoothInStream, bluetoothOutStream, _transmitCancelEvent);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogString("*** Connect exception: " + EdiabasNet.GetExceptionText(ex));
                                adapterType = AdapterTypeDetect.AdapterType.ConnectionFailed;
                            }
                            finally
                            {
                                bluetoothInStream?.Close();
                                bluetoothOutStream?.Close();
                                bluetoothSocket?.Close();

                                bluetoothInStream = null;
                                bluetoothOutStream = null;
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
                    adapterType = AdapterTypeDetect.AdapterType.ConnectionFailed;
                }

                if (_adapterTypeDetect.SbLog.Length == 0)
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

                    switch (adapterType)
                    {
                        case AdapterTypeDetect.AdapterType.ConnectionFailed:
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
                                        _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(),
                                            GetType(), (o, eventArgs) => { });
                                    };
                                }
                                break;
                            }

                            AlertDialog alertDialog2 = new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_connection_failed)
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
                                    _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(),
                                        GetType(), (o, eventArgs) => { });
                                };
                            }
                            break;
                        }

                        case AdapterTypeDetect.AdapterType.Unknown:
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
                                        _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(),
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
                                    _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(), GetType(), (o, eventArgs) =>
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

                        case AdapterTypeDetect.AdapterType.Elm327:
                        case AdapterTypeDetect.AdapterType.Elm327Limited:
                        {
                            string message;
                            switch (adapterType)
                            {
                                case AdapterTypeDetect.AdapterType.Elm327Limited:
                                    message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.limited_elm_adapter_type), _adapterTypeDetect.ElmVerH, _adapterTypeDetect.ElmVerL);
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

                        case AdapterTypeDetect.AdapterType.StnFwUpdate:
                        {
                            bool yesSelected = false;
                            AlertDialog alertDialog = new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    _activityCommon.StartApp(CheckAdapter.ObdLinkPackageName, true);
                                    yesSelected = true;
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_stn_firmware)
                                .SetTitle(Resource.String.alert_title_warning)
                                .Show();
                            if (alertDialog != null)
                            {
                                alertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }

                                    if (yesSelected)
                                    {
                                        return;
                                    }

                                    _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(), GetType(), (o, eventArgs) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }

                                        ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                                    });
                                };
                            }
                            break;
                        }

                        case AdapterTypeDetect.AdapterType.Elm327Custom:
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

                        case AdapterTypeDetect.AdapterType.Elm327Invalid:
                        case AdapterTypeDetect.AdapterType.Elm327Fake:
                        case AdapterTypeDetect.AdapterType.Elm327FakeOpt:
                        case AdapterTypeDetect.AdapterType.Elm327NoCan:
                        {
                            bool yesSelected = false;
                            AlertDialog.Builder builder = new AlertDialog.Builder(this);

                            string message;
                            switch (adapterType)
                            {
                                case AdapterTypeDetect.AdapterType.Elm327Fake:
                                case AdapterTypeDetect.AdapterType.Elm327FakeOpt:
                                    message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.fake_elm_adapter_type), _adapterTypeDetect.ElmVerH, _adapterTypeDetect.ElmVerL);
                                    message += "<br>" + GetString(Resource.String.recommened_adapter_type);
                                    break;

                                case AdapterTypeDetect.AdapterType.Elm327NoCan:
                                    message = GetString(Resource.String.elm_no_can);
                                    message += "<br>" + GetString(Resource.String.adapter_elm_replacement);
                                    break;

                                default:
                                    message = GetString(Resource.String.invalid_adapter_type);
                                    message += "<br>" + GetString(Resource.String.recommened_adapter_type);
                                    break;
                            }

                            if (adapterType == AdapterTypeDetect.AdapterType.Elm327FakeOpt)
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

                                    _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(), GetType(), (o, eventArgs) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }

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

                        case AdapterTypeDetect.AdapterType.Custom:
                            ReturnDeviceType(deviceAddress, deviceName);
                            break;

                        case AdapterTypeDetect.AdapterType.CustomUpdate:
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
                                .SetMessage(Resource.String.adapter_fw_update)
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            break;

                        case AdapterTypeDetect.AdapterType.CustomNoEscape:
                            new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_no_escape_mode)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            break;

                        case AdapterTypeDetect.AdapterType.EchoOnly:
                            ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.RawTag, deviceName);
                            break;

                        default:
                            ReturnDeviceType(deviceAddress, deviceName);
                            break;
                    }
                });
            })
            {
                Priority = System.Threading.ThreadPriority.Normal
            };
            _detectThread.Start();
        }

        public bool IsJobRunning()
        {
            if (_detectThread == null)
            {
                return false;
            }
            if (_detectThread.IsAlive)
            {
                return true;
            }
            _detectThread = null;
            return false;
        }

        private bool BluetoothConnect(BluetoothSocket bluetoothSocket, int timeout = 0)
        {
            if (bluetoothSocket == null)
            {
                return false;
            }

            if (_transmitCancelEvent.WaitOne(0))
            {
                return false;
            }

            bool connectOk = false;
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    if (!bluetoothSocket.IsConnected)
                    {
                        bluetoothSocket.Connect();
                    }

                    connectOk = true;
                }
                catch (Exception)
                {
                    connectOk = false;
                }
            })
            {
                Priority = System.Threading.ThreadPriority.Normal
            };
            connectThread.Start();

            long startTime = Stopwatch.GetTimestamp();
            bool abort = false;
            for (; ; )
            {
                if (connectThread.Join(100))
                {
                    break;
                }

                if (timeout > 0)
                {
                    if ((Stopwatch.GetTimestamp() - startTime) > timeout * EdCustomAdapterCommon.TickResolMs)
                    {
                        abort = true;
                        break;
                    }
                }

                if (_transmitCancelEvent.WaitOne(0))
                {
                    abort = true;
                    break;
                }
            }

            if (abort)
            {
                connectOk = false;
                try
                {
                    bluetoothSocket.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            connectThread.Join();

            return connectOk;
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
            bool leDevice = false;
            BluetoothDevice device = null;

            try
            {
                device = _btAdapter.GetRemoteDevice(address.ToUpperInvariant());
                if (device != null && device.Type == BluetoothDeviceType.Le)
                {
                    leDevice = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            List<BtOperation> operationList = new List<BtOperation>();
            List<string> itemList = new List<string>();
            if (paired)
            {
                if (IsBtDeviceValid(device))
                {
                    itemList.Add(GetString(Resource.String.bt_device_select));
                    operationList.Add(BtOperation.SelectAdapter);
                }

                itemList.Add(GetString(Resource.String.bt_device_unpair));
                operationList.Add(BtOperation.UnpairDevice);
            }
            else
            {
                itemList.Add(GetString(Resource.String.bt_device_select));
                operationList.Add(BtOperation.SelectAdapter);

                if (!leDevice)
                {
                    itemList.Add(GetString(Resource.String.bt_device_pair));
                    operationList.Add(BtOperation.PairDevice);
                }
            }

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
                        case BtOperation.PairDevice:
                            if (device != null)
                            {
                                try
                                {
                                    device.CreateBond();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                            break;

                        case BtOperation.UnpairDevice:
                            if (device != null)
                            {
                                try
                                {
                                    Java.Lang.Reflect.Method removeBond = device.Class.GetMethod("removeBond");
                                    if (removeBond != null)
                                    {
                                        removeBond.Invoke(device);
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
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

        private void LogString(string info)
        {
            _adapterTypeDetect.LogString(info);
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
                                        if (!ExtractDeviceInfo(item, out string _, out string address))
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
                                bool changed = false;
                                if (device.BondState == Bond.Bonded)
                                {
                                    for (int i = 0; i < _chat._newDevicesArrayAdapter.Count; i++)
                                    {
                                        string item = _chat._newDevicesArrayAdapter.GetItem(i);
                                        if (ExtractDeviceInfo(item, out string _, out string address))
                                        {
                                            if (string.Compare(address, device.Address, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                _chat._newDevicesArrayAdapter.Remove(item);
                                                changed = true;
                                            }
                                        }
                                    }
                                }
                                else if (device.BondState == Bond.None)
                                {
                                    for (int i = 0; i < _chat._pairedDevicesArrayAdapter.Count; i++)
                                    {
                                        string item = _chat._pairedDevicesArrayAdapter.GetItem(i);
                                        if (ExtractDeviceInfo(item, out string _, out string address))
                                        {
                                            if (string.Compare(address, device.Address, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                changed = true;
                                            }
                                        }
                                    }
                                }

                                if (changed)
                                {
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
