using Android.Content;
using Android.OS;
using System;

namespace BmwDeepObd
{
    public static class AndroidExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static T GetParcelableExtraType<T>(this Intent intent, string name)
        {
            object parcel;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                parcel = intent.GetParcelableExtra(name, Java.Lang.Class.FromType(typeof(T)));
            }
            else
            {
#pragma warning disable CS0618
#pragma warning disable CA1422
                parcel = intent.GetParcelableExtra(name);
#pragma warning restore CA1422
#pragma warning restore CS0618
            }

            return (T)Convert.ChangeType(parcel, typeof(T));
        }
    }
}
