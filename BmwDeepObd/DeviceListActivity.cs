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
using Android.Views;
using Android.Widget;
using EdiabasLib;
using Java.Util;

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
            EchoOnly,           // only echo response
        }

        private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const int ResponseTimeout = 1000;

        // Return Intent extra
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";

        // Member fields
        private BluetoothAdapter _btAdapter;
        private static ArrayAdapter<string> _pairedDevicesArrayAdapter;
        private static ArrayAdapter<string> _newDevicesArrayAdapter;
        private Receiver _receiver;

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

            // Initialize the button to perform device discovery
            var scanButton = FindViewById<Button> (Resource.Id.button_scan);
            scanButton.Click += (sender, e) =>
            {
                DoDiscovery ();
                var view = sender as View;
                if (view != null) view.Visibility = ViewStates.Gone;
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
                FindViewById<View> (Resource.Id.title_paired_devices).Visibility = ViewStates.Visible;
                foreach (var device in pairedDevices)
                {
                    ParcelUuid[] uuids = device.GetUuids();
                    if ((uuids == null) || (uuids.Any(uuid => SppUuid.CompareTo(uuids[0].Uuid) == 0)))
                    {
                        _pairedDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
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
            if (_btAdapter != null) {
                _btAdapter.CancelDiscovery ();
            }

            // Unregister broadcast listeners
            UnregisterReceiver (_receiver);
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

            // Turn on sub-title for new devices
            FindViewById<View> (Resource.Id.title_new_devices).Visibility = ViewStates.Visible;

            // If we're already discovering, stop it
            if (_btAdapter.IsDiscovering)
            {
                _btAdapter.CancelDiscovery ();
            }

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
                            bluetoothSocket.Connect();
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
                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress, deviceName);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_connection_failed)
                                .SetTitle(Resource.String.adapter_type_title)
                                .Show();
                            break;

                        case AdapterType.Unknown:
                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    ReturnDeviceType(deviceAddress, deviceName);
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.unknown_adapter_type)
                                .SetTitle(Resource.String.adapter_type_title)
                                .Show();
                            break;

                        case AdapterType.Elm327:
                            ReturnDeviceType(deviceAddress + ";" + EdBluetoothInterface.Elm327Tag, deviceName);
                            break;

                        case AdapterType.Elm327Invalid:
                        case AdapterType.Elm327Fake21:
                        {
                            string message =
                                GetString(adapterType == AdapterType.Elm327Fake21
                                    ? Resource.String.fake_elm_adapter_type
                                    : Resource.String.invalid_adapter_type);
                            message += "\n" + GetString(Resource.String.recommened_adapter_type);
                            new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(message)
                                .SetTitle(Resource.String.adapter_type_title)
                                .Show();
                            break;
                        }

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

        /// <summary>
        /// Return specified device type to caller
        /// </summary>
        /// <param name="deviceAddress">Device Bluetooth address</param>
        /// <param name="deviceName">Device Bleutooth name</param>
        private void ReturnDeviceType(string deviceAddress, string deviceName)
        {
            // Create the result Intent and include the MAC address
            Intent intent = new Intent();
            intent.PutExtra(ExtraDeviceName, deviceName);
            intent.PutExtra(ExtraDeviceAddress, deviceAddress);

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
            const int versionRespLen = 8;
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
                    bluetoothOutStream.Write(customData, 0, customData.Length);

                    List<byte> responseList = new List<byte>();
                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        while (bluetoothInStream.IsDataAvailable())
                        {
                            int data = bluetoothInStream.ReadByte();
                            if (data >= 0)
                            {
                                responseList.Add((byte)data);
                                startTime = Stopwatch.GetTimestamp();
                            }
                        }
                        if (responseList.Count >= customData.Length + versionRespLen)
                        {
                            bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                            if (!validEcho)
                            {
                                break;
                            }
                            byte checkSum = 0x00;
                            for (int i = 0; i < versionRespLen - 1; i++)
                            {
                                checkSum += responseList[i + customData.Length];
                            }
                            if (checkSum != responseList[customData.Length + versionRespLen - 1])
                            {
                                break;
                            }
                            return AdapterType.Custom;
                        }
                        if ((Stopwatch.GetTimestamp() - startTime) > ResponseTimeout * TickResolMs)
                        {
                            if (responseList.Count >= customData.Length)
                            {
                                bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                                if (validEcho)
                                {
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
                    bluetoothOutStream.Write(sendData, 0, sendData.Length);

                    string response = GetElm327Reponse(bluetoothInStream);
                    if (response != null)
                    {
                        if (response.Contains("ELM327"))
                        {
                            if (response.Contains("ELM327 v2.1"))
                            {
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
                        bluetoothOutStream.Write(sendData, 0, sendData.Length);

                        string response = GetElm327Reponse(bluetoothInStream);
                        if (response == null)
                        {
                            adapterType = AdapterType.Elm327Invalid;
                            break;
                        }
                        if (!response.Contains("OK\r"))
                        {
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
                        break;
                    }
                }
                if (response != null)
                {
                    break;
                }
                if ((Stopwatch.GetTimestamp() - startTime) > ResponseTimeout * TickResolMs)
                {
                    break;
                }
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

        public class Receiver : BroadcastReceiver
        {
            readonly Android.App.Activity _chat;

            public Receiver(Android.App.Activity chat)
            {
                _chat = chat;
            }

            public override void OnReceive (Context context, Intent intent)
            {
                string action = intent.Action;

                // When discovery finds a device
                if (action == BluetoothDevice.ActionFound)
                {
                    // Get the BluetoothDevice object from the Intent
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra (BluetoothDevice.ExtraDevice);
                    // If it's already paired, skip it, because it's been listed already
                    if (device.BondState != Bond.Bonded)
                    {
                        ParcelUuid[] uuids = device.GetUuids();
                        if ((uuids == null) || (uuids.Any(uuid => SppUuid.CompareTo(uuids[0].Uuid) == 0)))
                        {
                            _newDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                        }
                    }
                    // When discovery is finished, change the Activity title
                }
                else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    //_chat.SetProgressBarIndeterminateVisibility (false);
                    _chat.FindViewById<ProgressBar>(Resource.Id.progress_bar).Visibility = ViewStates.Invisible;
                    _chat.SetTitle (Resource.String.select_device);
                    if (_newDevicesArrayAdapter.Count == 0)
                    {
                        var noDevices = _chat.Resources.GetText (Resource.String.none_found);
                        _newDevicesArrayAdapter.Add (noDevices);
                    }
                }
            }
        }
    }
}
