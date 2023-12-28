using System;
using AndroidX.AppCompat.App;

namespace BmwDeepObd
{
    [Android.App.Application(
        ResizeableActivity = true,
        LargeHeap = true,
        UsesCleartextTraffic = true,
#if DEBUG
        BackupAgent = typeof(CustomBackupAgent),
#endif
        FullBackupOnly = true,
        RestoreAnyVersion = true,
        Name = ActivityCommon.AppNameSpace + ".DeepObd"
        )]
    [Android.App.MetaData("android.webkit.WebView.EnableSafeBrowsing", Value = "false")]
    [Android.App.MetaData("android.webkit.WebView.MetricsOptOut", Value = "true")]
    [Android.App.MetaData("com.google.android.backup.api_key", Value = "unused")]
    // ReSharper disable once UnusedMember.Global
    public class MyApplication : Android.App.Application
    {
        public MyApplication(IntPtr handle, Android.Runtime.JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void OnCreate()
        {
            base.OnCreate();
            AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
    }
}
