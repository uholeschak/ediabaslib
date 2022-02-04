using System;
using AndroidX.AppCompat.App;

namespace BmwDeepObd
{
    [Android.App.Application(
        ResizeableActivity = true,
        LargeHeap = true,
        UsesCleartextTraffic = true,
        AllowBackup = false,
        Name = ActivityCommon.AppNameSpace + ".DeepObd"
        )]
    [Android.App.MetaData("android.webkit.WebView.EnableSafeBrowsing", Value = "false")]
    [Android.App.MetaData("android.webkit.WebView.MetricsOptOut", Value = "true")]
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
        }
    }
}
