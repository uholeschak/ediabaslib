using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;
using System.Collections.Generic;

namespace BmwDeepObd
{
    public static class PackageUtils
    {
        private static Context _packageContext;

        public static Context GetPackageContext()
        {
            try
            {
                if (_packageContext != null)
                {
                    return _packageContext;
                }

                _packageContext = Android.App.Application.Context.CreatePackageContext(Android.App.Application.Context.PackageName, 0);
                return _packageContext;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static PackageInfo GetPackageInfo(PackageManager packageManager, string packageName, PackageInfoFlags infoFlags = 0)
        {
            try
            {
                if (packageManager == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(packageName))
                {
                    return null;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CS0618
                    return packageManager.GetPackageInfo(packageName, infoFlags);
#pragma warning restore CS0618
                }

                return packageManager.GetPackageInfo(packageName, PackageManager.PackageInfoFlags.Of((int)infoFlags));
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static string GetInstallerPackageName(PackageManager packageManager, string packageName)
        {
            try
            {
                if (packageManager == null)
                {
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(packageName))
                {
                    return string.Empty;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.R)
                {
#pragma warning disable CS0618
#pragma warning disable CA1422
                    return packageManager.GetInstallerPackageName(packageName);
#pragma warning restore CA1422
#pragma warning restore CS0618
                }

                return packageManager.GetInstallSourceInfo(packageName).InstallingPackageName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static IList<PackageInfo> GetInstalledPackages(PackageManager packageManager, PackageInfoFlags infoFlags = 0)
        {
            try
            {
                if (packageManager == null)
                {
                    return null;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CS0618
                    return packageManager.GetInstalledPackages(infoFlags);
#pragma warning restore CS0618
                }

                return packageManager.GetInstalledPackages(PackageManager.PackageInfoFlags.Of((int)infoFlags));
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static IList<ApplicationInfo> GetInstalledApplications(PackageManager packageManager, PackageInfoFlags infoFlags = 0)
        {
            try
            {
                if (packageManager == null)
                {
                    return null;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CS0618
                    return packageManager.GetInstalledApplications(infoFlags);
#pragma warning restore CS0618
                }

                return packageManager.GetInstalledApplications(PackageManager.ApplicationInfoFlags.Of((int)infoFlags));
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static ActivityInfo GetActivityInfo(PackageManager packageManager, ComponentName componentName, PackageInfoFlags infoFlags = 0)
        {
            try
            {
                if (packageManager == null)
                {
                    return null;
                }

                if (componentName == null)
                {
                    return null;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CS0618
                    return packageManager.GetActivityInfo(componentName, infoFlags);
#pragma warning restore CS0618
                }

                return packageManager.GetActivityInfo(componentName, PackageManager.ComponentInfoFlags.Of((int)infoFlags));
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static IList<ResolveInfo> QueryIntentActivities(PackageManager packageManager, Intent intent, PackageInfoFlags infoFlags = 0)
        {
            try
            {
                if (packageManager == null)
                {
                    return null;
                }

                if (intent == null)
                {
                    return null;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CS0618
                    return packageManager.QueryIntentActivities(intent, infoFlags);
#pragma warning restore CS0618
                }

                return packageManager.QueryIntentActivities(intent, PackageManager.ResolveInfoFlags.Of((int)infoFlags));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
