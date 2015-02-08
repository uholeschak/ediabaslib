using System;
using System.Collections;
using System.IO;
using System.Linq;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CarControl;

namespace CarControlAndroid
{
    [Activity (Label = "@string/app_name", MainLauncher = true,
               ConfigurationChanges=Android.Content.PM.ConfigChanges.KeyboardHidden | Android.Content.PM.ConfigChanges.Orientation)]
    public class ActivityMain : Activity
    {
        enum activityRequest
        {
            REQUEST_SELECT_DEVICE,
            REQUEST_ENABLE_BT
        }

        private string deviceAddress = string.Empty;
        private string sharedAppName = "CarControl";
        private string ecuPath;
        private BluetoothAdapter bluetoothAdapter = null;
        private CommThread commThread;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Get local Bluetooth adapter
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (bluetoothAdapter == null)
            {
                Toast.MakeText (this, Resource.String.bt_not_available, ToastLength.Long).Show ();
                Finish ();
                return;
            }

            GetSettings ();

            ecuPath = Path.Combine (
                System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "Ecu");
            // copy asset files
            CopyAssets (ecuPath);

            commThread = new CommThread(ecuPath);
            commThread.DataUpdated += new CommThread.DataUpdatedEventHandler(DataUpdated);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button> (Resource.Id.buttonConnect);
            button.Click += ButtonClick;

            UpdateDisplay ();
        }

        protected override void OnStart ()
        {
            base.OnStart ();

            // If BT is not on, request that it be enabled.
            // setupChat() will then be called during onActivityResult
            if (!bluetoothAdapter.IsEnabled)
            {
                Intent enableIntent = new Intent (BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult (enableIntent, (int)activityRequest.REQUEST_ENABLE_BT);
            }
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            StopCommThread ();

            StoreSettings ();
        }

        protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
        {
            switch((activityRequest)requestCode)
            {
            case activityRequest.REQUEST_SELECT_DEVICE:
                // When DeviceListActivity returns with a device to connect
                if (resultCode == Result.Ok)
                {
                    // Get the device MAC address
                    deviceAddress = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                }
                break;

            case activityRequest.REQUEST_ENABLE_BT:
                // When the request to enable Bluetooth returns
                if (resultCode != Result.Ok)
                {
                    // User did not enable Bluetooth or an error occured
                    Toast.MakeText(this, Resource.String.bt_not_enabled_leaving, ToastLength.Short).Show();
                    Finish();
                }
                break;
            }
        }

        public override bool OnCreateOptionsMenu (IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu (IMenu menu)
        {
            bool commActive = commThread != null && commThread.ThreadRunning ();
            IMenuItem scanMenu = menu.FindItem (Resource.Id.scan);
            if (scanMenu != null)
            {
                scanMenu.SetEnabled (!commActive);
            }
            return base.OnPrepareOptionsMenu (menu);
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            switch (item.ItemId) 
            {
                case Resource.Id.scan:
                // Launch the DeviceListActivity to see devices and do scan
                Intent serverIntent = new Intent(this, typeof(DeviceListActivity));
                StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_SELECT_DEVICE);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected void ButtonClick (object sender, EventArgs e)
        {
            if (commThread != null && commThread.ThreadRunning())
            {
                StopCommThread ();
            }
            else
            {
                StartCommThread ();
            }
            UpdateDisplay();
        }

        private bool StartCommThread()
        {
            try
            {
                if (commThread == null)
                {
                    commThread = new CommThread(ecuPath);
                    commThread.DataUpdated += new CommThread.DataUpdatedEventHandler(DataUpdated);
                }
                commThread.StartThread("BLUETOOTH:" + deviceAddress, null, CommThread.SelectedDevice.Test);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StopCommThread()
        {
            if (commThread != null)
            {
                try
                {
                    commThread.StopThread();
                    commThread.DataUpdated -= DataUpdated;
                    commThread.Dispose();
                    commThread = null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private bool GetSettings()
        {
            try
            {
                ISharedPreferences prefs = Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                deviceAddress = prefs.GetString("DeviceAddress", "98:D3:31:40:13:56");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StoreSettings()
        {
            try
            {
                ISharedPreferences prefs = Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.PutString("DeviceAddress", deviceAddress);
                prefsEdit.Commit();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            RunOnUiThread(DataUpdatedMethode);
        }

        private void UpdateDisplay()
        {
            DataUpdatedMethode();
        }

        private void DataUpdatedMethode()
        {
            Button buttonConnect = FindViewById<Button> (Resource.Id.buttonConnect);
            TextView textView = FindViewById<TextView> (Resource.Id.textViewResult);

            bool testValid = false;
            if (commThread != null && commThread.ThreadRunning ())
            {
                buttonConnect.Text = GetString (Resource.String.button_disconnect);
                if (commThread.Device == CommThread.SelectedDevice.Test) testValid = true;
            }
            else
            {
                buttonConnect.Text = GetString (Resource.String.button_connect);
            }

            if (testValid)
            {
                lock (CommThread.DataLock)
                {
                    textView.Text = commThread.TestResult;
                }
            }
            else
            {
                textView.Text = string.Empty;
            }
        }

        private bool CopyAssets(string ecuPath)
        {
            try
            {
                if (!Directory.Exists(ecuPath))
                {
                    Directory.CreateDirectory (ecuPath);
                }

                string assetDir = "Ecu";
                string[] assetList = Assets.List (assetDir);
                foreach(string assetName in assetList)
                {
                    using (Stream asset = Assets.Open (Path.Combine(assetDir, assetName)))
                    {
                        string fileDest = Path.Combine(ecuPath, assetName);
                        bool copyFile = false;
                        if (!File.Exists(fileDest))
                        {
                            copyFile = true;
                        }
                        else
                        {
                            using (var fileComp = new FileStream(fileDest, FileMode.Open))
                            {
                                copyFile = !StreamEquals(asset, fileComp);
                            }
                        }
                        if (copyFile)
                        {
                            using (Stream dest = File.Create (fileDest))
                            {
                                asset.CopyTo (dest);
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        private static bool StreamEquals(Stream stream1, Stream stream2)
        {
            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize]; //buffer size
            byte[] buffer2 = new byte[bufferSize];
            while (true) {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                for (int i = 0; i < buffer1.Length; i++)
                {
                    if (buffer1 [i] != buffer2 [i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
