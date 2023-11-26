using System.Reflection;
using System;

namespace PsdzClientLibrary.Core
{
    public sealed class CoreFramework
    {
        private static bool ValidLicense;

        private static readonly DateTime? lastCompileTime;

        private static int debuglevel;

        public static string AssemblyVersion
        {
            get
            {
                try
                {
                    object[] customAttributes = Assembly.GetAssembly(typeof(CoreFramework)).GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: true);
                    if (customAttributes != null && customAttributes[0] != null)
                    {
                        return (customAttributes[0] as AssemblyFileVersionAttribute).Version;
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("CoreFramework.get_AssemblyVersion()", exception);
                }
                return null;
            }
        }

        public static DateTime LastCompileTime
        {
            get
            {
                if (!lastCompileTime.HasValue)
                {
                    return DateTime.MinValue;
                }
                return lastCompileTime.Value;
            }
        }

        public static int DebugLevel
        {
            get
            {
                return debuglevel;
            }
            set
            {
                debuglevel = value;
            }
        }

        public static bool validLicense
        {
            get
            {
                return ValidLicense;
            }
            set
            {
                ValidLicense = value;
            }
        }

        public static bool OSSModeActive => ConfigSettings.IsOssModeActive;

        public static bool IsLightModeActive => ConfigSettings.IsLightModeActive;

        static CoreFramework()
        {
            Log.Info("CoreFramework.CoreFramework()", "ctor called.");
            validLicense = true;
            lastCompileTime = null;
            debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.CoreFramework.DebugLevel", debuglevel);
            Log.Info("CoreFramework.CoreFramework()", "CoreFramework.DebugLevel is {0}", debuglevel);
        }
    }
}
