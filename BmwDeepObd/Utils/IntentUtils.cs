using Android.Content;
using Android.OS;
using System;

namespace BmwDeepObd
{
    public static class IntentUtils
    {
        public static bool OpenWebUrl(Context context, string url)
        {
            try
            {
                if (context == null)
                {
                    return false;
                }

                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://"));
                intent.AddCategory(Intent.CategoryBrowsable);
                ComponentName componentName = intent.ResolveActivity(context.PackageManager);
                if (componentName == null)
                {
                    return false;
                }

                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                intent.SetComponent(componentName);
                intent.SetData(Android.Net.Uri.Parse(url));
                context.StartActivity(intent);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool OpenPlayStoreForPackage(Context context, string packageName)
        {
            if (context == null || string.IsNullOrEmpty(packageName))
            {
                return false;
            }

            try
            {
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"market://details?id=" + packageName));
                intent.SetPackage("com.android.vending");
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return OpenWebUrl(context, "https://play.google.com/store/apps/details?id=" + packageName);
        }

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
