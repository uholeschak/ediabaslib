using Android.Content;
using Android.OS;
using System;

namespace BmwDeepObd
{
    public static class IntentUtils
    {
        public static bool OpenAppSettingDetails(Android.App.Activity activity, int requestCode)
        {
            try
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings,
                    Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName));
                activity.StartActivityForResult(intent, requestCode);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool OpenLocationSettings(Android.App.Activity activity, int requestCode)
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.P)
                {
                    return false;
                }

                Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                    activity.StartActivityForResult(intent, requestCode);
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static bool OpenAppSettingAccessFiles(Android.App.Activity activity, int requestCode)
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.R)
                {
                    return false;
                }

                try
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission,
                        Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName));
                    activity.StartActivityForResult(intent, requestCode);
                }
                catch (Exception)
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission);
                    activity.StartActivityForResult(intent, requestCode);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
