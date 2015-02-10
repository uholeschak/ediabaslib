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
    public class ActivityMain : TabActivity
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
        private Button buttonConnect;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.tabs);
            CreateTab(typeof(TestActivity), "test", "Test", Resource.Drawable.ic_tab_test);
            buttonConnect = FindViewById<Button> (Resource.Id.buttonConnect);

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

            // Get our button from the layout resource,
            // and attach an event to it
            buttonConnect.Click += ButtonClick;

            UpdateDisplay ();
        }

        private void CreateTab(Type activityType, string tag, string label, int drawableId )
        {
            Intent intent = new Intent(this, activityType);
            intent.AddFlags(ActivityFlags.NewTask);

            TabHost.TabSpec spec = TabHost.NewTabSpec(tag);
            var drawableIcon = Resources.GetDrawable(drawableId);
            spec.SetIndicator(label, drawableIcon);
            spec.SetContent(intent);

            TabHost.AddTab(spec);
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

        protected override void OnStop ()
        {
            base.OnStop ();

            StopCommThread (false);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            StopCommThread (true);

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
                StopCommThread (false);
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
                    commThread.ThreadTerminated += new CommThread.ThreadTerminatedEventHandler(ThreadTerminated);
                }
                commThread.StartThread("BLUETOOTH:" + deviceAddress, null, CommThread.SelectedDevice.Test);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StopCommThread(bool wait)
        {
            if (commThread != null)
            {
                try
                {
                    commThread.StopThread(wait);
                    if (wait)
                    {
                        commThread.DataUpdated -= DataUpdated;
                        commThread.ThreadTerminated -= ThreadTerminated;
                        commThread.Dispose();
                        commThread = null;
                    }
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

        private void ThreadTerminated(object sender, EventArgs e)
        {
            RunOnUiThread(ThreadTerminatedMethode);
        }

        private void ThreadTerminatedMethode()
        {
            StopCommThread(true);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            DataUpdatedMethode();
        }

        private void DataUpdatedMethode()
        {
            bool testValid = false;
            bool buttonEnable = true;
            if (commThread != null && commThread.ThreadRunning ())
            {
                if (commThread.ThreadStopping ())
                {
                    buttonEnable = false;
                }
                buttonConnect.Text = GetString (Resource.String.button_disconnect);
                if (commThread.Device == CommThread.SelectedDevice.Test) testValid = true;
            }
            else
            {
                buttonConnect.Text = GetString (Resource.String.button_connect);
            }
            buttonConnect.Enabled = buttonEnable;

            string textViewText = string.Empty;
            if (testValid)
            {
                lock (CommThread.DataLock)
                {
                    textViewText = commThread.TestResult;
                }
            }

            Intent broadcastIntent = new Intent(TestActivity.ACTION_UPDATE_TEXT);
            broadcastIntent.PutExtra(TestActivity.INDENT_TEXT_INFO, textViewText);
            SendOrderedBroadcast (broadcastIntent, null);
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

    [Activity]
    public class TestActivity : Activity
    {
        public const string ACTION_UPDATE_TEXT = "UPDATE_TEXT";
        public const string INDENT_TEXT_INFO = "TEXT";
        private Receiver receiver;
        private TextView textView;

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);
            SetContentView (Resource.Layout.Main);

            textView = FindViewById<TextView> (Resource.Id.textViewResult);
            receiver = new Receiver ();
        }

        protected override void OnStart ()
        {
            base.OnStart ();

            var filter = new IntentFilter (ACTION_UPDATE_TEXT);
            RegisterReceiver (receiver, filter);
        }

        protected override void OnStop ()
        {
            base.OnStop ();

            UnregisterReceiver (receiver);
        }

        public void UpdateText(string text)
        {
            RunOnUiThread (() => {
                if (textView != null)
                {
                    textView.Text = text;
                }
            });
        }

        public class Receiver : BroadcastReceiver
        {
            public override void OnReceive (Context context, Intent intent)
            {
                string action = intent.Action;
                TestActivity activity = context as TestActivity;

                if (action == ACTION_UPDATE_TEXT)
                {
                    if (activity != null)
                    {
                        string text = intent.Extras.GetString(INDENT_TEXT_INFO);
                        if (text != null)
                        {
                            activity.UpdateText (text);
                        }
                    }
                }
            }
        }
    }
}
