using System;

namespace BmwDeepObd
{
    [Android.App.Application(ResizeableActivity = true, LargeHeap = true)]
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
        }
    }
}
