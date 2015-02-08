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

        private string deviceAddress = "98:D3:31:40:13:56";
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

            string ecuPath = Path.Combine (
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

            commThread.StopThread();
            commThread.DataUpdated -= DataUpdated;
            commThread.Dispose();
            commThread = null;
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
                    //deviceAddress = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
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

        protected void ButtonClick (object sender, EventArgs e)
        {
            if (commThread.ThreadRunning())
            {
                commThread.StopThread();
            }
            else
            {
                commThread.StartThread("BLUETOOTH:" + deviceAddress, null, CommThread.SelectedDevice.Test);
            }
            UpdateDisplay();
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
            if (commThread == null)
            {
                return;
            }
            Button buttonConnect = FindViewById<Button> (Resource.Id.buttonConnect);
            TextView textView = FindViewById<TextView> (Resource.Id.textViewResult);

            bool testValid = false;
            if (commThread.ThreadRunning ())
            {
                buttonConnect.Text = GetString (Resource.String.disconnect);
                if (commThread.Device == CommThread.SelectedDevice.Test) testValid = true;
            }
            else
            {
                buttonConnect.Text = GetString (Resource.String.connect);
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
