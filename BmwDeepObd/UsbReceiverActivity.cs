using System;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;

[assembly: Android.App.UsesFeature("android.hardware.usb.host")]

namespace BmwDeepObd
{
    [Android.App.IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Android.App.MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]

    [Android.App.Activity(Label = "@string/app_name",
        Theme = "@style/Theme.Transparent",
        DirectBootAware = true,
        NoHistory = true,
        ExcludeFromRecents = true,
        Exported = false)]
    class UsbReceiverActivity : AppCompatActivity
    {
#if DEBUG
        private static readonly string Tag = typeof(UsbReceiverActivity).FullName;
#endif
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
#if DEBUG
            Android.Util.Log.Info(Tag, "UsbReceiverActivity created");
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();

            Intent intent = Intent;
            if (intent != null)
            {
                string action = intent.Action;
                switch (action)
                {
                    case UsbManager.ActionUsbDeviceAttached:
#if DEBUG
                        Android.Util.Log.Info(Tag, "USB device attached");
#endif
                        ShowApplication();
                        break;
                }
            }

            Finish();
        }

        private void ShowApplication()
        {
            try
            {
                Intent intent = new Intent(this, typeof(ExpansionDownloaderActivity));
                intent.SetAction(Intent.ActionMain);
                intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask);
                StartActivity(intent);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
