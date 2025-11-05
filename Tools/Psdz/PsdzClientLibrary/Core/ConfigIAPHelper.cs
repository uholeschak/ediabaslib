using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PsdzClient.Core
{
    public static class ConfigIAPHelper
    {
        public static string GetCompatibleICOMVersion()
        {
            return ConfigSettings.getConfigString("Icom.Package.Version.Compatible", "03.15.04");
        }

        public static string GetCompatibleICOMNextVersion()
        {
            return ConfigSettings.getConfigString("IcomNext.Package.Version.Compatible", "03.15.06");
        }

        public static string GetFastaPath()
        {
            return ConfigSettings.getPathString("FASTADirPath", "..\\..\\..\\FASTAOut");
        }

        public static string GetICOMImageLocation()
        {
            return ConfigSettings.getPathString("IcomFirmwareLocation", "%ISPIDATA%\\BMW\\ISPI\\data\\TRIC\\ICOM");
        }

        public static string GetICOMNextImageLocation()
        {
            return ConfigSettings.getPathString("IcomNextFirmwareLocation", "%ISPIDATA%\\BMW\\ISPI\\data\\TRIC\\ICOMNext");
        }

        public static string GetICOMPackageVersion()
        {
            return ConfigSettings.getConfigString("Icom.Package.Version", "00-00-00");
        }

        public static string GetICOMNextPackageVersion()
        {
            return ConfigSettings.getConfigString("IcomNext.Package.Version", "00-00-00");
        }

        public static string GetWindowsOSVersion()
        {
            return ConfigSettings.getConfigString("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", "0.0.0");
        }

        public static string GetWindowsOSVersion(int versionInt)
        {
            switch (versionInt)
            {
                case 50:
                    return "Windows 2000";
                case 51:
                    return "Windows XP";
                case 52:
                    return "Windowx XP 64-bit";
                case 60:
                    return "Windows Vista";
                case 61:
                    return "Windows 7";
                case 62:
                    return "Windows 8";
                case 63:
                    return "Windows 8.1";
                case 100:
                    return "Windows 10";
                case 110:
                    return "Windows 11";
                default:
                    return "Unknown";
            }
        }

        public static int GetWindowsOSVersionInt()
        {
            int num = 0;
            num = Environment.OSVersion.Version.Major * 10;
            num += Environment.OSVersion.Version.Minor;
            int? num2 = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentMajorVersionNumber", null) as int?;
            if (num2.HasValue)
            {
                if (num2 == 10 && GetCurrentBuildVersion() >= 22000)
                {
                    return 110;
                }
                return (num2 * 10).Value;
            }
            return num;
        }

        public static int GetCurrentBuildVersion()
        {
            return Convert.ToInt32(Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion").GetValue("CurrentBuildNumber"));
        }

        public static string GetICOMTracePath()
        {
            return ConfigSettings.getPathString("ICOMTracePath", "%ISPIDATA%\\BMW\\ISPI\\logs\\TRIC\\ICOM");
        }

        public static string GetILeanAdminClientVersion()
        {
            return ConfigSettings.getConfigString("HKEY_LOCAL_MACHINE\\Software\\BMWGroup\\ISPI\\iLean\\ISPI Admin Client\\", "ProductVersion", "0.0.0");
        }

        public static string GetMetaFilePath()
        {
            return ConfigSettings.getPathString("MetaFileDirectory", "..\\..\\..\\Transactions");
        }

        public static string GetOutletNo()
        {
            return ConfigSettings.getConfigString("BMW.Rheingold.FASTA.FASTACfgBtrNr", string.Empty);
        }

        public static string GetTransactionPath()
        {
            return ConfigSettings.getPathString("TransFileDirectory", "..\\..\\..\\Transactions");
        }

        public static string GetVciConfigPort()
        {
            return ConfigSettings.getConfigString("VciConfigPort", "58000");
        }

        public static string GetVersionPartDelimiter()
        {
            return Regex.Escape(ConfigSettings.getConfigString("VersionDelimiter", "."));
        }

        public static bool IsDecentralVciConfigActive()
        {
            return ConfigSettings.getConfigStringAsBoolean("UseDecentralVciConfig", defaultValue: false);
        }

        public static bool IsPUKActive()
        {
            if (!ConfigSettings.IsLightModeActive)
            {
                return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.CoreFramework.PUKActive", defaultValue: false);
            }
            return false;
        }

        public static bool IsTricZenctralActive()
        {
            bool configStringAsBoolean = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.CoreFramework.TRICZentralActive", defaultValue: false);
            Log.Info("ConfigSettings.IsTricZenctralActive()", "Method IsTricZenctralActive returned {0}", configStringAsBoolean);
            return configStringAsBoolean;
        }

        public static bool IsLogInterupters()
        {
            return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.ApiMode.LogInterupters", defaultValue: false);
        }
    }
}