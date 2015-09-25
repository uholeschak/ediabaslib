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
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace BmwDiagnostics
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
        private static readonly UUID SppUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

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
                    if ((uuids == null) || ((uuids.Length == 1) && (SppUuid.CompareTo(uuids[0].Uuid) == 0)))
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
        /// The on-click listener for all devices in the ListViews
        /// </summary>
        void DeviceListClick (object sender, AdapterView.ItemClickEventArgs e)
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

                // Create the result Intent and include the MAC address
                Intent intent = new Intent ();
                intent.PutExtra (ExtraDeviceName, name);
                intent.PutExtra (ExtraDeviceAddress, address);

                // Set result and finish this Activity
                SetResult(Android.App.Result.Ok, intent);
            }
            Finish ();
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
                        if ((uuids == null) || ((uuids.Length == 1) && (SppUuid.CompareTo(uuids[0].Uuid) == 0)))
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
