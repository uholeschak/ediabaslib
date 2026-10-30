using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using System;
using System.Collections.Generic;

namespace BmwDeepObd
{
    public static class ServiceUtils
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static bool IsMtcService(Context context)
        {
            try
            {
                PackageManager packageManager = context?.PackageManager;
                IList<ApplicationInfo> appList = PackageUtils.GetInstalledApplications(packageManager, PackageInfoFlags.MatchAll);
                if (appList != null)
                {
                    foreach (ApplicationInfo appInfo in appList)
                    {
                        if (string.Compare(appInfo.PackageName, "android.microntek.mtcser", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public static bool StartForegroundService(Context context, bool startComm = false)
        {
            try
            {
                Intent startServiceIntent = new Intent(context, typeof(ForegroundService));
                startServiceIntent.SetAction(ForegroundService.ActionStartService);
                startServiceIntent.PutExtra(ForegroundService.ExtraStartComm, startComm);
                ContextCompat.StartForegroundService(context, startServiceIntent);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool StopForegroundService(Context context, bool abortThread = false)
        {
            try
            {
                Intent stopServiceIntent = new Intent(context, typeof(ForegroundService));
                stopServiceIntent.SetAction(ForegroundService.ActionStopService);
                stopServiceIntent.PutExtra(ForegroundService.ExtraAbortThread, abortThread);
                context.StopService(stopServiceIntent);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
