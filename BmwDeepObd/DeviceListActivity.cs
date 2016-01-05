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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using EdiabasLib;
using Java.Util;
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
    public class DeviceListActivity : AppCompatActivity
    {
        enum AdapterType
        {
            ConnectionFailed,   // connection to adapter failed
            Unknown,            // unknown adapter
            Elm327,             // ELM327
            Elm327Invalid,      // ELM327 invalid type
            Elm327Fake21,       // ELM327 fake 2.1 version
            Custom,             // custom adapter
            CustomUpdate,       // custom adapter with firmware update
            EchoOnly,           // only echo response
        }

        private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int ResponseTimeout = 1000;

        // Return Intent extra
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraCallAdapterConfig = "adapter_configuration";

        // Member fields
        private BluetoothAdapter _btAdapter;
        private static ArrayAdapter<string> _pairedDevicesArrayAdapter;
        private static ArrayAdapter<string> _newDevicesArrayAdapter;
        private Receiver _receiver;
        private AlertDialog _altertInfoDialog;
        private Button _scanButton;
        private ActivityCommon _activityCommon;
        private string _appDataDir;
        private readonly StringBuilder _sbLog = new StringBuilder();

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            // Setup the window
            SetContentView(Resource.Layout.device_list);

            // Set result CANCELED incase the user backs out
            SetResult (Android.App.Result.Canceled);

            _activityCommon = new ActivityCommon(this);

            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);

            // Initialize the button to perform device discovery
            _scanButton = FindViewById<Button>(Resource.Id.button_scan);
            _scanButton.Click += (sender, e) =>
            {
                DoDiscovery ();
                _scanButton.Enabled = false;
            };

            // Initialize array adapters. One for already paired devices and
            // one for newly discovered devices
            _pairedDevicesArrayAdapter = new ArrayAdapter<string> (this, Resource.Layout.device_name);
            _newDevicesArrayAdapter = new ArrayAdapter<string> (this, Resource.Layout.device_name);

            // Find and set up the ListView for paired devices
            var pairedListView = FindViewById<ListView> (Resource.Id.paired_devices);
            pairedListView.Adapter = _pairedDevicesArrayAdapter;
            pairedListView.ItemClick += DeviceListClick;

            // Find and set up the ListView for newly discovered devices
            var newDevicesListView = FindViewById<ListView> (Resource.Id.new_devices);
            newDevicesListView.Adapter = _newDevicesArrayAdapter;
            newDevicesListView.ItemClick += DeviceListClick;

            // Register for broadcasts when a device is discovered
            _receiver = new Receiver (this);
            var filter = new IntentFilter (BluetoothDevice.ActionFound);
            RegisterReceiver (_receiver, filter);

            // Register for broadcasts when discovery has finished
            filter = new IntentFilter (BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver (_receiver, filter);

            // Get the local Bluetooth adapter
            _btAdapter = BluetoothAdapter.DefaultAdapter;

            // Get a set of currently paired devices
            var pairedDevices = _btAdapter.BondedDevices;

            // If there are paired devices, add each one to the ArrayAdapter
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
            else
            {
                String noDevices = Resources.GetText (Resource.String.none_paired);
                _pairedDevicesArrayAdapter.Add (noDevices);
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

        /// <summary>
        /// Start device discover with the BluetoothAdapter
        /// </summary>
        private void DoDiscovery ()
        {
            // Log.Debug (Tag, "doDiscovery()");

            // Indicate scanning in the title
            FindViewById<ProgressBar>(Resource.Id.progress_bar).Visibility = ViewStates.Visible;
            SetTitle (Resource.String.scanning);

            // Turn on area for new devices
            FindViewById<View>(Resource.Id.layout_new_devices).Visibility = ViewStates.Visible;

            // If we're already discovering, stop it
            if (_btAdapter.IsDiscovering)
            {
                _btAdapter.CancelDiscovery ();
            }
            _newDevicesArrayAdapter.Clear();

            // Request discover from BluetoothAdapter
            _btAdapter.StartDiscovery ();
        }

        /// <summary>
        /// Start adapter detection
        /// </summary>
        /// <param name="deviceAddress">Device Bluetooth address</param>
        /// <param name="deviceName">Device Bleutooth name</param>
        private void DetectAdapter(string deviceAddress, string deviceName)
        {
            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.detect_adapter));
            progress.Show();

            _sbLog.Clear();

            Thread detectThread = new Thread(() =>
            {
                AdapterType adapterType = AdapterType.Unknown;
                try
                {
                    BluetoothDevice device = _btAdapter.GetRemoteDevice(deviceAddress);
                    if (device != null)
                    {
                        using (BluetoothSocket bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SppUuid))
                        {
                            try
                            {
                                bluetoothSocket.Connect();
                            }
                            catch (Exception)
                            {   // sometimes connection failes in the first attempt
                                bluetoothSocket.Connect();
                            }
                            adapterType = AdapterTypeDetection(bluetoothSocket);
                            bluetoothSocket.Close();
                        }
                    }
                }
                catch (Exception)
                {
                    adapterType = AdapterType.ConnectionFailed;
                }

                RunOnUiThread(() =>
                {
                    progress.Hide();
                    progress.Dispose();
                    switch (adapterType)
                    {
                        case AdapterType.ConnectionFailed:
                            _altertInfoDialog = new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress, deviceName);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_connection_failed)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            _altertInfoDialog.DismissEvent += (sender, args) =>
                            {
                                _altertInfoDialog = null;
                            };
                            break;

                        case AdapterType.Unknown:
                        {
                            bool yesSelected = false;
                            _altertInfoDialog = new AlertDialog.Builder(this)
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
                            _altertInfoDialog.DismissEvent += (sender, args) =>
                            {
                                _altertInfoDialog = null;
                                _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(),
                                    PackageManager.GetPackageInfo(PackageName, 0), GetType(), (o, eventArgs) =>
                                    {
                                        if (yesSelected)
                                        {
                                            ReturnDeviceType(deviceAddress, deviceName);
                                        }
                                    });
                            };
                            break;
                        }

                        case AdapterType.Elm327:
                        {
                            _altertInfoDialog = new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_elm_replacement)
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            _altertInfoDialog.DismissEvent += (sender, args) =>
                            {
                                _altertInfoDialog = null;
                            };
                            TextView messageView = _altertInfoDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                            if (messageView != null)
                            {
                                messageView.MovementMethod = new LinkMovementMethod();
                            }
                            break;
                        }

                        case AdapterType.Elm327Invalid:
                        case AdapterType.Elm327Fake21:
                        {
                            string message =
                                GetString(adapterType == AdapterType.Elm327Fake21
                                    ? Resource.String.fake_elm_adapter_type
                                    : Resource.String.invalid_adapter_type);
                            message += "<br>" + GetString(Resource.String.recommened_adapter_type);
                            _altertInfoDialog = new AlertDialog.Builder(this)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Html.FromHtml(message))
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            _altertInfoDialog.DismissEvent += (sender, args) =>
                            {
                                _altertInfoDialog = null;
                                _activityCommon.RequestSendMessage(_appDataDir, _sbLog.ToString(), PackageManager.GetPackageInfo(PackageName, 0), GetType());
                            };
                            TextView messageView = _altertInfoDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                            if (messageView != null)
                            {
                                messageView.MovementMethod = new LinkMovementMethod();
                            }
                            break;
                        }

                        case AdapterType.Custom:
                        case AdapterType.CustomUpdate:
                            _altertInfoDialog = new AlertDialog.Builder(this)
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
                            _altertInfoDialog.DismissEvent += (sender, args) =>
                            {
                                _altertInfoDialog = null;
                            };
                            break;

                        default:
                        {
                            ReturnDeviceType(deviceAddress, deviceName);
                            break;
                        }
                    }
                });
            })
            {
                Priority = System.Threading.ThreadPriority.Highest
            };
            detectThread.Start();
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
            const int versionRespLen = 9;
            byte[] customData = { 0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x5E };
            AdapterType adapterType = AdapterType.Unknown;

            try
            {
                Stream bluetoothInStream = bluetoothSocket.InputStream;
                Stream bluetoothOutStream = bluetoothSocket.OutputStream;

                {
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
                            return AdapterType.Custom;
                        }
                        if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * TickResolMs)
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
                }

                // ELM327
                bool elmReports21 = false;
                for (int i = 0; i < 2; i++)
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
                        if (response.Contains("ELM327"))
                        {
                            LogString("ELM327 detected");
                            if (response.Contains("ELM327 v2.1"))
                            {
                                LogString("Version 2.1 detected");
                                elmReports21 = true;
                            }
                            adapterType = AdapterType.Elm327;
                            break;
                        }
                    }
                }
                if (adapterType == AdapterType.Elm327)
                {
                    foreach (string command in EdBluetoothInterface.Elm327InitCommands)
                    {
                        bluetoothInStream.Flush();
                        while (bluetoothInStream.IsDataAvailable())
                        {
                            bluetoothInStream.ReadByte();
                        }
                        byte[] sendData = Encoding.UTF8.GetBytes(command + "\r");
                        LogData(sendData, 0, sendData.Length, "Send");
                        bluetoothOutStream.Write(sendData, 0, sendData.Length);

                        string response = GetElm327Reponse(bluetoothInStream);
                        if (response == null)
                        {
                            LogString("*** No ELM response");
                            adapterType = AdapterType.Elm327Invalid;
                            break;
                        }
                        if (!response.Contains("OK\r"))
                        {
                            LogString("*** No ELM OK found");
                            adapterType = AdapterType.Elm327Invalid;
                            break;
                        }
                    }
                    if (adapterType == AdapterType.Elm327Invalid && elmReports21)
                    {
                        adapterType = AdapterType.Elm327Fake21;
                    }
                }
            }
            catch (Exception)
            {
                return AdapterType.Unknown;
            }
            return adapterType;
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
                    if (stringBuilder.Length > 100)
                    {
                        LogString("*** ELM response too long");
                        break;
                    }
                }
                if (response != null)
                {
                    break;
                }
                if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * TickResolMs)
                {
                    LogString("*** ELM response timeout");
                    break;
                }
            }
            if (response == null)
            {
                LogString("*** No ELM prompt");
            }
            return response;
        }

        /// <summary>
        /// The on-click listener for all devices in the ListViews
        /// </summary>
        private void DeviceListClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Cancel discovery because it's costly and we're about to connect
            if (_btAdapter.IsDiscovering)
            {
                _btAdapter.CancelDiscovery();
            }
            _scanButton.Enabled = true;
            //_chat.SetProgressBarIndeterminateVisibility (false);
            FindViewById<ProgressBar>(Resource.Id.progress_bar).Visibility = ViewStates.Invisible;
            SetTitle(Resource.String.select_device);

            TextView textView = e.View as TextView;
            if (textView != null)
            {
                string info = textView.Text;
                string[] parts = info.Split('\n');
                if (parts.Length < 2)
                {
                    return;
                }
                string name = parts[0];
                string address = parts[1];

                DetectAdapter(address, name);
            }
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

                    // When discovery finds a device
                    if (action == BluetoothDevice.ActionFound)
                    {
                        // Get the BluetoothDevice object from the Intent
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        // If it's already paired, skip it, because it's been listed already
                        if (device.BondState != Bond.Bonded)
                        {
                            ParcelUuid[] uuids = device.GetUuids();
                            if ((uuids == null) || (uuids.Any(uuid => SppUuid.CompareTo(uuid.Uuid) == 0)))
                            {
                                _newDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                            }
                        }
                        // When discovery is finished, change the Activity title
                    }
                    else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                    {
                        _chat._scanButton.Enabled = true;
                        //_chat.SetProgressBarIndeterminateVisibility (false);
                        _chat.FindViewById<ProgressBar>(Resource.Id.progress_bar).Visibility = ViewStates.Invisible;
                        _chat.SetTitle(Resource.String.select_device);
                        if (_newDevicesArrayAdapter.Count == 0)
                        {
                            var noDevices = _chat.Resources.GetText(Resource.String.none_found);
                            _newDevicesArrayAdapter.Add(noDevices);
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
