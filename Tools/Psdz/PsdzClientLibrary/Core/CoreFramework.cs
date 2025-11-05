using System.Reflection;
using System;

namespace PsdzClient.Core
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

        public static string AssemblyVersionWithoutRevision
        {
            get
            {
                string assemblyVersion = AssemblyVersion;
                if (string.IsNullOrWhiteSpace(assemblyVersion))
                {
                    return assemblyVersion;
                }

                if (assemblyVersion.Split('.').Length > 3)
                {
                    return assemblyVersion.Substring(0, assemblyVersion.LastIndexOf('.'));
                }

                return assemblyVersion;
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
            // [UH] modified
            validLicense = true;
            lastCompileTime = null;
            debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.CoreFramework.DebugLevel", debuglevel);
            Log.Info("CoreFramework.CoreFramework()", "CoreFramework.DebugLevel is {0}", debuglevel);
        }
    }
}