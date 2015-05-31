using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Widget;
using System;

namespace CarControlAndroid
{
    public class ActivityCommon
    {
        public enum InterfaceType
        {
            NONE,
            BLUETOOTH,
            ENET,
        }

        public const string EMULATOR_ENET_IP = "192.168.10.244";

        private Activity activity;
        private bool emulator;
        private BluetoothAdapter btAdapter;
        private WifiManager maWifi;
        private ConnectivityManager maConnectivity;
        private InterfaceType selectedInterface;
        private bool activateRequest;

        public bool Emulator
        {
            get
            {
                return emulator;
            }
        }

        public InterfaceType SelectedInterface
        {
            get
            {
                return selectedInterface;
            }
            set
            {
                selectedInterface = value;
            }
        }

        public BluetoothAdapter BtAdapter
        {
            get
            {
                return btAdapter;
            }
        }

        public WifiManager MaWifi
        {
            get
            {
                return maWifi;
            }
        }

        public ConnectivityManager MaConnectivity
        {
            get
            {
                return maConnectivity;
            }
        }

        public ActivityCommon(Activity activity)
        {
            this.activity = activity;
            emulator = IsEmulator();

            btAdapter = BluetoothAdapter.DefaultAdapter;
            maWifi = (WifiManager)activity.GetSystemService(Context.WifiService);
            maConnectivity = (ConnectivityManager)activity.GetSystemService(Context.ConnectivityService);
            selectedInterface = InterfaceType.NONE;
            activateRequest = false;
        }

        public bool IsInterfaceEnabled()
        {
            switch (selectedInterface)
            {
                case InterfaceType.BLUETOOTH:
                    if (btAdapter == null)
                    {
                        return false;
                    }
                    return btAdapter.IsEnabled;

                case InterfaceType.ENET:
                    if (maWifi == null)
                    {
                        return false;
                    }
                    return maWifi.IsWifiEnabled;
            }
            return false;
        }

        public bool IsInterfaceAvailable()
        {
            switch (selectedInterface)
            {
                case InterfaceType.BLUETOOTH:
                    if (btAdapter == null)
                    {
                        return false;
                    }
                    return btAdapter.IsEnabled;

                case InterfaceType.ENET:
                    if (maConnectivity == null)
                    {
                        return false;
                    }
                    NetworkInfo networkInfo = maConnectivity.ActiveNetworkInfo;
                    if (networkInfo == null)
                    {
                        return false;
                    }
                    return networkInfo.IsConnected;
            }
            return false;
        }

        public void ShowAlert(string message)
        {
            new AlertDialog.Builder(activity)
            .SetMessage(message)
            .SetNeutralButton(Resource.String.compile_ok_btn, (s, e) => { })
            .Show();
        }

        public void SelectInterface(EventHandler<DialogClickEventArgs> handler)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetTitle(Resource.String.select_interface);
            ListView listView = new ListView(activity);
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(activity, Android.Resource.Layout.SimpleListItemSingleChoice,
                new string[] {
                    activity.GetString(Resource.String.select_interface_bt),
                    activity.GetString(Resource.String.select_interface_enet)
                });
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            switch (selectedInterface)
            {
                case InterfaceType.BLUETOOTH:
                    listView.SetItemChecked(0, true);
                    break;

                case InterfaceType.ENET:
                    listView.SetItemChecked(1, true);
                    break;
            }
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                {
                    switch (listView.CheckedItemPosition)
                    {
                        case 0:
                            selectedInterface = InterfaceType.BLUETOOTH;
                            handler(sender, args);
                            break;

                        case 1:
                            selectedInterface = InterfaceType.ENET;
                            handler(sender, args);
                            break;
                    }
                });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                {
                });
            builder.Show();
        }

        public void EnableInterface()
        {
            switch (selectedInterface)
            {
                case ActivityCommon.InterfaceType.BLUETOOTH:
                    if (btAdapter == null)
                    {
                        Toast.MakeText(activity, Resource.String.bt_not_available, ToastLength.Long).Show();
                        break;
                    }
                    if (!btAdapter.IsEnabled)
                    {
                        try
                        {
#pragma warning disable 0618
                            btAdapter.Enable();
#pragma warning restore 0618
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case ActivityCommon.InterfaceType.ENET:
                    if (maWifi == null)
                    {
                        Toast.MakeText(activity, Resource.String.wifi_not_available, ToastLength.Long).Show();
                        break;
                    }
                    if (!maWifi.IsWifiEnabled)
                    {
                        try
                        {
                            maWifi.SetWifiEnabled(true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;
            }
        }

        public void RequestInterfaceEnable(EventHandler<DialogClickEventArgs> handler)
        {
            if (activateRequest)
            {
                return;
            }
            if (IsInterfaceAvailable())
            {
                return;
            }
            if (IsInterfaceEnabled())
            {
                return;
            }
            int resourceID;
            switch (selectedInterface)
            {
                case ActivityCommon.InterfaceType.BLUETOOTH:
                    resourceID = Resource.String.bt_enable;
                    break;

                case ActivityCommon.InterfaceType.ENET:
                    resourceID = Resource.String.wifi_enable;
                    break;

                default:
                    return;
            }
            activateRequest = true;
            new AlertDialog.Builder(activity)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    activateRequest = false;
                    EnableInterface();
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    activateRequest = false;
                })
                .SetCancelable(false)
                .SetMessage(resourceID)
                .SetTitle(Resource.String.interface_activate)
                .Show();
        }

        private static bool IsEmulator()
        {
            string fing = Build.Fingerprint;
            bool isEmulator = false;
            if (fing != null)
            {
                isEmulator = fing.Contains("vbox") || fing.Contains("generic");
            }
            return isEmulator;
        }
    }
}
