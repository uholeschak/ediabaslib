using PsdzClient.Core;

namespace PsdzClientLibrary.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.Win32;

    public enum OperationalMode
    {
        ISTA,
        ISTA_PLUS,
        ISTA_LIGHT,
        ISTA_POWERTRAIN,
        TELESERVICE,
        TeleServiceConsole,
        RITA,
        ISTAHV
    }

    public enum PropertyEnum
    {
        PersistentProperty,
        AcrossSessionProperty
    }

    public enum EnumOrderTypePreselection
    {
        Workshop,
        Breakdown
    }

    public enum PrintFormat
    {
        XPS,
        PDF,
        ALL
    }

    public class ConfigSettings
    {
        public const string BMW_RHEINGOLD_CONFIG_KEY = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";

        private const string BMW_RHEINGOLD_KEY_ISIGNORED = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";

        private const string BMW_RHEINGOLD_GLOBALCONFIG_KEY = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";

        private const string BMW_RHEINGOLD_PERSISTENCY_DATASTORE = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\Persistency";

        private const string BMW_RHEINGOLD_PERSISTENCYACROSSSESSION_DATASTORE = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\AcrossPersistency";

        private const string BMW_RHEINGOLD_SOFTWARE_REPOSITORY = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\SoftwareRepository";

        private const string BMW_ISTA_GLOBALCONFIG_KEY = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\ISTA";

        private const string BMW_ISTA_CONFIG_KEY = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\ISTA";

        private const string BMW_ISTA_KEY = "SOFTWARE\\BMWGroup\\ISPI\\Rheingold";

        public const string BMW_ISTALAUNCHER_CONFIG_KEY = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMW\\ISPI\\TRIC\\ISTALauncher";

        private const string CONFIG_FILE = "Configuration file";

        private const string KeyOssParamFile = "OssParamFile";

        private const string KeyIsProgrammingLocked = "IsProgrammingLocked";

        private const string KeyIsProgrammingDataInValid = "IsProgrammingDataInValid";

        public const string TEST_DISTRIBUTION_PARTNER_NUMBER = "40626";

        private static string currentUiCulture;

        private static CultureInfo currentCultureInfo;

        private static readonly ConcurrentDictionary<string, ConfigValue> currentConfigValues;

        private static readonly List<LogValue> firstLogging;

        private static bool isLogStarted;

        private static bool isMaster;

        private static bool isProgrammingLocked;

        private static bool areSDPPacksInvalid;

        private static bool? ignoreRegKeys;

        private static bool runIstaRsuRepairMode;

        public static string patchDbVersion { get; set; }

        public static CultureInfo CurrentCultureInfo => currentCultureInfo;

        public static bool IsVerificationMode => getConfigStringAsBoolean("BMW.Rheingold.VerificationMode", defaultValue: false);

        public static string CurrentUICulture
        {
            get
            {
                if (!string.IsNullOrEmpty(currentUiCulture))
                {
                    return currentUiCulture;
                }
                CurrentUICulture = "en-GB";
                return "en-GB";
            }
            set
            {
                if (!string.IsNullOrEmpty(currentUiCulture) && currentUiCulture.Equals(value))
                {
                    return;
                }
                currentUiCulture = value ?? "en-GB";
                currentCultureInfo = new CultureInfo(currentUiCulture);
                LogInfo("ConfigSettings.set_CurrentCulture()", "set UI culture to {0}", currentUiCulture);
                try
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentUiCulture);
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(currentUiCulture);
                    if (ConfigSettings.LanguageChangedEventhandler != null)
                    {
                        ConfigSettings.LanguageChangedEventhandler(Thread.CurrentThread.CurrentUICulture, new EventArgs());
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("ConfigSettings.set_CurrentCulture()", exception);
                }
            }
        }

        public static string CurrentUICulture2
        {
            get
            {
                if (!string.IsNullOrEmpty(currentUiCulture))
                {
                    if (currentUiCulture.Length > 1)
                    {
                        return currentUiCulture.Substring(0, 2);
                    }
                    return "en";
                }
                return null;
            }
        }

        internal static bool IsUnittestModeActive { get; set; }

        public static string IstaGuiLogFullName { get; set; }

        public static OperationalMode OperationalMode
        {
            get
            {
                string configString = getConfigString("BMW.Rheingold.OperationalMode", "ISTA");
                if (IsLightModeActive)
                {
                    return OperationalMode.ISTA_LIGHT;
                }
                if (IsOssModeActive)
                {
                    if ("ISTA_PLUS".Equals(configString))
                    {
                        return OperationalMode.ISTA_PLUS;
                    }
                    return OperationalMode.ISTA;
                }
                if (Enum.TryParse<OperationalMode>(configString, ignoreCase: true, out var result))
                {
                    return result;
                }
                return OperationalMode.ISTA;
            }
        }

        public static bool IsISTAModeHO => OperationalMode == OperationalMode.ISTA;

        public static bool IsISTAModeTS => OperationalMode == OperationalMode.TELESERVICE;

        public static CharacteristicExpression.EnumBrand SelectedBrand
        {
            get
            {
#if false
                if (IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))
                {
                    return CharacteristicExpression.EnumBrand.TOYOTA;
                }
#endif
                string configString = getConfigString("TesterGUI.SelectedBrand", "");
                if (Enum.TryParse<CharacteristicExpression.EnumBrand>(configString, ignoreCase: true, out var result))
                {
                    return result;
                }
                Log.Error("ConfigSettings.get_SelectedBrand()", "Unknown selected Brand '{0}'!", configString);
                return CharacteristicExpression.EnumBrand.Unknown;
            }
        }

        public static string AppBaseDirectory { get; set; }

        public static bool IsOssModeActive => getConfigStringAsBoolean("BMW.Rheingold.CoreFramework.OSSModeActive", defaultValue: false);

        public static bool IsLightModeActive => getConfigStringAsBoolean("BMW.Rheingold.CoreFramework.LightModeActive", defaultValue: false);

        public static bool IsMaster => isMaster;

        public static bool IsILeanActive
        {
            get
            {
#if false
                if (IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))
                {
                    return false;
                }
#endif
                bool configStringAsBoolean = getConfigStringAsBoolean("BMW.Rheingold.CoreFramework.ILeanActive", defaultValue: false);
                Log.Info("ConfigSettings.IsILeanActive()", "Property IsILeanActive is set to {0}", configStringAsBoolean);
                return configStringAsBoolean;
            }
        }

        public static string OssParamFile { get; set; }

        private static bool IsProgrammingLocked
        {
            get
            {
                if (IsLightModeActive)
                {
                    return !RunIstaRsuRepairMode;
                }
                return isProgrammingLocked;
            }
        }

        public static bool RunIstaRsuRepairMode
        {
            get
            {
                return runIstaRsuRepairMode;
            }
            set
            {
                runIstaRsuRepairMode = value;
            }
        }

        public static event EventHandler<EventArgs> LanguageChangedEventhandler;

        static ConfigSettings()
        {
            isMaster = true;
            runIstaRsuRepairMode = true;
            firstLogging = new List<LogValue>();
            currentConfigValues = new ConcurrentDictionary<string, ConfigValue>();
            LogInfo("ConfigSettings.ConfigSettings()", "OperationalMode is {0}", OperationalMode);
        }

        public static void SetupOperation(IDictionary<string, string> currentConfiguration)
        {
            isMaster = false;
            currentConfigValues.Clear();
            foreach (KeyValuePair<string, string> item in currentConfiguration)
            {
                AddNewKey(item.Key, item.Value);
            }
            isProgrammingLocked = getConfigStringAsBoolean("IsProgrammingLocked", defaultValue: false);
            areSDPPacksInvalid = getConfigStringAsBoolean("IsProgrammingDataInValid", defaultValue: false);
            OssParamFile = getConfigString("OssParamFile");
        }

        public static string GetLogisticBaseVersion()
        {
            return Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\ISTA", "LogisticBaseVersion", null) as string;
        }

        public static string GetSwiDataUX()
        {
            return GetIstaConfigString("SWIDataUX", null);
        }

        public static string GetSwiDataRSU()
        {
            return GetIstaConfigString("SWIDataRSU", null);
        }

        private static ISet<string> RetrieveAllKeys()
        {
            ISet<string> set = new HashSet<string>();
            IList<string> list = new List<string>();
            IList<string> list2 = new List<string>();
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\BMWGroup\\ISPI\\Rheingold"))
            {
                if (registryKey != null)
                {
                    list = registryKey.GetValueNames().ToList();
                }
            }
            using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\BMWGroup\\ISPI\\Rheingold"))
            {
                if (registryKey2 != null)
                {
                    list2 = registryKey2.GetValueNames().ToList();
                }
            }
            string[] allKeys = ConfigurationManager.AppSettings.AllKeys;
            foreach (string item2 in list)
            {
                set.Add(item2);
            }
            foreach (string item3 in list2)
            {
                set.Add(item3);
            }
            string[] array = allKeys;
            foreach (string item in array)
            {
                set.Add(item);
            }
            return set;
        }

        public static string GetTempFolder(string tempFolderFallback = null)
        {
            string empty = string.Empty;
            empty = ((string.IsNullOrEmpty(tempFolderFallback) || !Directory.Exists(Path.GetPathRoot(tempFolderFallback))) ? getPathString("BMW.Rheingold.Temp", "C:\\Temp") : getPathString("BMW.Rheingold.Temp", tempFolderFallback));
            if (!Directory.Exists(empty))
            {
                Directory.CreateDirectory(empty);
            }
            return empty;
        }

        public static string getPathString(object hTMLTOPDFCONVERTERLOCATION, string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<string, string> GetCurrentConfiguration()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            ISet<string> set = RetrieveAllKeys();
            if (set != null)
            {
                foreach (string item in set)
                {
                    dictionary[item] = GetConfigStringInternal(item, string.Empty, setOrigin: false);
                }
            }
            dictionary.Add("OssParamFile", OssParamFile);
            dictionary.Add("IsProgrammingLocked", IsProgrammingLocked.ToString());
            dictionary.Add("IsProgrammingDataInValid", areSDPPacksInvalid.ToString());
            return dictionary;
        }

        public static void ToggleProgrammingSDPExpired()
        {
            areSDPPacksInvalid = true;
        }

        public static string GetCulture(string lang)
        {
            lang = lang ?? string.Empty;
            switch (lang)
            {
                case "el":
                    return "el-GR";
                case "id":
                    return "id-ID";
                case "th":
                    return "th-TH";
                case "en":
                    return "en-GB";
                case "pl":
                    return "pl-PL";
                case "ko":
                    return "ko-KR";
                case "cs":
                    return "cs-CZ";
                case "es":
                    return "es-ES";
                case "zh":
                    return "zh-CN";
                case "tr":
                    return "tr-TR";
                case "it":
                    return "it-IT";
                case "sv":
                    return "sv-SE";
                case "ru":
                    return "ru-RU";
                case "de":
                    return "de-DE";
                case "fr":
                    return "fr-FR";
                case "ja":
                    return "ja-JP";
                case "nl":
                    return "nl-NL";
                default:
                    Log.Error("ConfigSettings.GetCulture()", "Unknown language string \"{0}\", returning \"{1}\".", lang, "en-GB");
                    return "en-GB";
                case "pt":
                    return "pt-PT";
            }
        }

        public static DateTime GetConfigDateTime(string key, DateTime defaultValue)
        {
            AddNewKey(key, defaultValue);
            string configStringInternal = GetConfigStringInternal(key, string.Empty);
            if (string.IsNullOrEmpty(configStringInternal))
            {
                return defaultValue;
            }
            try
            {
                if (DateTime.TryParse(configStringInternal, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    StoreAndLogChangedValue(key, result);
                    return result;
                }
                return DateTime.MinValue;
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.getConfigDateTime()", exception);
                return defaultValue;
            }
        }

        public static double GetConfigDouble(string key, double defaultValue)
        {
            try
            {
                AddNewKey(key, defaultValue);
                string configStringInternal = GetConfigStringInternal(key, null);
                if (configStringInternal != null)
                {
                    double num2 = Convert.ToDouble(configStringInternal, CultureInfo.InvariantCulture);
                    StoreAndLogChangedValue(key, num2);
                    return num2;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.GetConfigDouble()", exception);
            }
            return defaultValue;
        }

        public static long GetConfigInt64(string key, long defaultValue)
        {
            try
            {
                AddNewKey(key, defaultValue);
                string configStringInternal = GetConfigStringInternal(key, null);
                if (configStringInternal != null)
                {
                    long num2 = Convert.ToInt64(configStringInternal, CultureInfo.InvariantCulture);
                    StoreAndLogChangedValue(key, num2);
                    return num2;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.GetConfigInt64()", exception);
            }
            return defaultValue;
        }

        public static byte[] GetPersistencyData(PropertyEnum pe, string key)
        {
            string keyName = ((pe == PropertyEnum.PersistentProperty) ? "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\Persistency" : "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\AcrossPersistency");
            try
            {
                return (byte[])Registry.GetValue(keyName, key, null);
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.getPersistencyData()", exception);
            }
            return null;
        }

        public static void CleanPersistedProperties()
        {
            try
            {
                string text = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\AcrossPersistency";
                text = text.Remove(0, text.IndexOf('\\') + 1);
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(text, writable: true);
                if (registryKey == null)
                {
                    return;
                }
                string[] valueNames = registryKey.GetValueNames();
                foreach (string text2 in valueNames)
                {
                    try
                    {
                        registryKey.DeleteValue(text2);
                        Log.Info("ConfigSettings.CleanPersistedProperties()", $"removed persisted property: {text2}");
                    }
                    catch (Exception)
                    {
                        Log.Info("ConfigSettings.CleanPersistedProperties()", $"could not remove persisted property: {text2}");
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.CleanPersistedProperties()", exception);
            }
        }

        public static string IniReadValue(string path, string section, string key)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder(255);
                GetPrivateProfileString(section, key, string.Empty, stringBuilder, 255, path);
                return stringBuilder.ToString();
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.IniReadValue()", exception);
            }
            return null;
        }

        public static void IniWriteValue(string path, string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, path);
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.IniWriteValue()", exception);
            }
        }

        public static string Get2DigitLanguageCode()
        {
            if (CurrentCultureInfo.TwoLetterISOLanguageName == null)
            {
                return "en";
            }
            return CurrentCultureInfo.TwoLetterISOLanguageName;
        }

        public static string InitCulture()
        {
            string configString = getConfigString("TesterGUI.Language", null);
            try
            {
#if false
                if (IsOssModeActive)
                {
                    SetCurrentUICultureFromPortalConfig();
                }
                else if (!string.IsNullOrEmpty(configString))
                {
                    LogInfo("ConfigSettings.InitCulture()", "found already configured language: {0}", configString);
                    CurrentUICulture = configString;
                }
                else
                {
                    Log.Warning("ConfigSettings.InitCulture()", "no preconfigured language found. Checking available languages");
                    IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                    if (instance.DatabaseAccessType == DatabaseType.SQLite)
                    {
                        SetCurrentUICultureFromDB(instance);
                    }
                    else
                    {
                        SetCurrentUICultureToDefault("ConfigSettings.InitCulture()", "no sqlite database properly configured");
                    }
                }
#else
                // [UH] simplified
                if (!string.IsNullOrEmpty(configString))
                {
                    LogInfo("ConfigSettings.InitCulture()", "found already configured language: {0}", configString);
                    CurrentUICulture = configString;
                }
                else
                {
                    SetCurrentUICultureToDefault("ConfigSettings.InitCulture()", "no sqlite database properly configured");
                }
#endif
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.InitCulture()", exception);
                CurrentUICulture = getConfigString("TesterGUI.Language", "en-GB");
            }
            return currentUiCulture;
        }

        private static void SetCurrentUICultureToDefault(string callingMethod, string logMessage)
        {
            Log.Warning(callingMethod, logMessage);
            CurrentUICulture = getConfigString("TesterGUI.Language", "en-GB");
        }

        public static bool PutGlobalConfigString(string path, string key, string value)
        {
            try
            {
                Registry.SetValue(path, key, value);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.putGlobalConfigString()", exception);
            }
            return false;
        }

        public static void PutPersistencyData(PropertyEnum pe, string key, byte[] value)
        {
            string keyName = ((pe == PropertyEnum.PersistentProperty) ? "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\Persistency" : "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold\\AcrossPersistency");
            try
            {
                Registry.SetValue(keyName, key, value);
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.putPersistencyData()", exception);
            }
        }

        private static string getConfigStringLocalMachine(string path, string key, string defaultValue)
        {
            path = path.Replace("HKEY_LOCAL_MACHINE\\", string.Empty);
            RegistryKey registryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                path = path.Replace("SOFTWARE", "SOFTWARE\\WOW6432Node");
            }
            else
            {
                registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }
            if (registryKey.OpenSubKey(path)?.GetValue(key) == null)
            {
                return defaultValue;
            }
            return registryKey.OpenSubKey(path)?.GetValue(key)?.ToString();
        }

        protected static void AddNewKey(string key, object defaultValue)
        {
            if (currentConfigValues.ContainsKey(key))
            {
                currentConfigValues[key].DefaultValue = defaultValue;
            }
            else
            {
                currentConfigValues[key] = new ConfigValue(defaultValue, null);
            }
        }

        private static void StoreAndLogChangedValue(string key, object value, bool obfuscate = false)
        {
            if (obfuscate)
            {
                if ((value == null && currentConfigValues[key].Value != null) || (value != null && !value.Equals(currentConfigValues[key].Value) && !string.IsNullOrEmpty(currentConfigValues[key].Origin)))
                {
                    LogInfo("ConfigSettings.StoreAndLogChangedValue()", "Change configuration \"{0}\" (read from \"{1}\").", key, currentConfigValues[key].Origin);
                }
            }
            else if (value == null)
            {
                if (currentConfigValues[key].Value != null && !string.IsNullOrEmpty(currentConfigValues[key].Origin))
                {
                    LogInfo("ConfigSettings.StoreAndLogChangedValue()", "Change configuration \"{0}\" from \"{1}\" to \"{2}\" (read from \"{3}\").", key, ObfuscatePassword(key, currentConfigValues[key].Value), ObfuscatePassword(key, value), currentConfigValues[key].Origin);
                    currentConfigValues[key].Value = value;
                }
            }
            else if (!currentConfigValues[key].IsLogged && !value.Equals(currentConfigValues[key].DefaultValue))
            {
                string text = ObfuscatePassword(key, currentConfigValues[key].DefaultValue);
                string text2 = ObfuscatePassword(key, value);
                currentConfigValues[key].Value = value;
                if (string.Compare(text, text2, StringComparison.OrdinalIgnoreCase) != 0 && string.IsNullOrEmpty(currentConfigValues[key].Origin))
                {
                    LogInfo("ConfigSettings.StoreAndLogChangedValue()", "Change configuration '{0}' from '{1}' to '{2}'. But Origin is not set, so the source is unknown.", key, text, text2);
                    currentConfigValues[key].IsLogged = true;
                }
                else if ((!(value is bool) || string.Compare(text, text2, StringComparison.OrdinalIgnoreCase) != 0) && !string.IsNullOrEmpty(currentConfigValues[key].Origin))
                {
                    LogInfo("ConfigSettings.StoreAndLogChangedValue()", "Change configuration \"{0}\" from \"{1}\" to \"{2}\" (read from \"{3}\").", key, text, text2, currentConfigValues[key].Origin);
                    currentConfigValues[key].IsLogged = true;
                }
            }
        }

        public static string ObfuscatePassword(string key, object logString)
        {
            if (key.IndexOf("Password", StringComparison.OrdinalIgnoreCase) <= -1 && key.IndexOf("License", StringComparison.OrdinalIgnoreCase) <= -1)
            {
                string text = null;
                try
                {
                    string result = null;
                    if (logString != null)
                    {
                        text = logString.ToString();
                        int num2 = text.IndexOf("PASSWORD=", StringComparison.OrdinalIgnoreCase);
                        if (num2 < 0)
                        {
                            return text;
                        }
                        num2 += 9;
                        int num3 = text.IndexOf(";", num2, StringComparison.Ordinal);
                        if (num3 < 0)
                        {
                            num3 = text.Length;
                        }
                        result = text.Substring(0, num2) + "*****" + text.Substring(num3);
                    }
                    return result;
                }
                catch (Exception exception)
                {
                    Log.ErrorException("ConfigSettings.ObfuscatePassword()", exception);
                    return text;
                }
            }
            return "*****";
        }

        public static string GetLicense()
        {
            AddNewKey("License", string.Empty);
            string configStringInternal = GetConfigStringInternal("License", string.Empty);
            StoreAndLogChangedValue("License", configStringInternal, obfuscate: true);
            return configStringInternal;
        }

        public static string getConfigString(string key, string defaultValue)
        {
            AddNewKey(key, defaultValue);
            string configStringInternal = GetConfigStringInternal(key, defaultValue);
            StoreAndLogChangedValue(key, configStringInternal);
            return configStringInternal;
        }

        public static string getConfigString(string key)
        {
            return getConfigString(key, string.Empty);
        }

        private static string GetConfigStringInternal(string key, string defaultValue, bool setOrigin = true)
        {
            try
            {
                if (!ignoreRegKeys.HasValue)
                {
                    ignoreRegKeys = false;
                    ignoreRegKeys = getConfigStringAsBoolean("BMW.Rheingold.IGNORE.REGKEYS", defaultValue: false);
                }
                if (!ignoreRegKeys.Value)
                {
                    if (!isMaster)
                    {
                        return (!currentConfigValues.ContainsKey(key)) ? defaultValue : ((currentConfigValues[key].Value == null) ? null : Convert.ToString(currentConfigValues[key].Value, CultureInfo.InvariantCulture));
                    }
                    try
                    {
                        object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold", key, null);
                        if (value != null)
                        {
                            string text = string.Empty;
                            if (value is string text2)
                            {
                                text = text2;
                                if (!string.IsNullOrEmpty(text))
                                {
                                    if (setOrigin)
                                    {
                                        currentConfigValues[key].Origin = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";
                                    }
                                    return text;
                                }
                            }
                            if (value is string[] array)
                            {
                                string[] array2 = array;
                                foreach (string text3 in array2)
                                {
                                    text += text3;
                                }
                                if (!string.IsNullOrEmpty(text))
                                {
                                    if (setOrigin)
                                    {
                                        currentConfigValues[key].Origin = "HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";
                                    }
                                    return text;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("ConfigSettings.getConfigString()", "Read configuration for \"{0}\" failed: {1}", key, ex);
                    }
                    try
                    {
                        object value2 = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold", key, null);
                        if (value2 != null)
                        {
                            string text4 = string.Empty;
                            if (value2 is string text5)
                            {
                                text4 = text5;
                                if (!string.IsNullOrEmpty(text4))
                                {
                                    if (setOrigin)
                                    {
                                        currentConfigValues[key].Origin = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";
                                    }
                                    return text4;
                                }
                            }
                            if (value2 is string[] array3)
                            {
                                string[] array2 = array3;
                                foreach (string text6 in array2)
                                {
                                    text4 += text6;
                                }
                                if (!string.IsNullOrEmpty(text4))
                                {
                                    if (setOrigin)
                                    {
                                        currentConfigValues[key].Origin = "HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold";
                                    }
                                    return text4;
                                }
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        Log.Warning("ConfigSettings.getConfigString()", "Read configuration for \"{0}\" failed: {1}", key, ex2);
                    }
                }
                try
                {
                    string text7 = ConfigurationManager.AppSettings[key];
                    if (!string.IsNullOrEmpty(text7))
                    {
                        if (setOrigin)
                        {
                            currentConfigValues[key].Origin = "Configuration file";
                        }
                        return text7;
                    }
                }
                catch (Exception ex3)
                {
                    Log.Warning("ConfigSettings.getConfigString()", "Read configuration for \"{0}\" failed: {1}", key, ex3);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ConfigSettings.getConfigString()", exception);
            }
            return defaultValue;
        }

        public static bool getConfigStringAsBoolean(string key)
        {
            return getConfigStringAsBoolean(key, defaultValue: false);
        }

        public static bool getConfigStringAsBoolean(string key, bool defaultValue)
        {
            if (IsUnittestModeActive && !string.IsNullOrEmpty(key) && key.Equals("BMW.Rheingold.OnlineMode"))
            {
                return false;
            }
            AddNewKey(key, defaultValue);
            string configStringInternal = GetConfigStringInternal(key, null);
            if (configStringInternal != null)
            {
                bool flag = string.Compare(configStringInternal, "true", StringComparison.OrdinalIgnoreCase) == 0;
                StoreAndLogChangedValue(key, flag);
                return flag;
            }
            return defaultValue;
        }

        public bool GetConfigStringAsBoolean(string key, bool defaultValue)
        {
            if (IsUnittestModeActive && !string.IsNullOrEmpty(key) && key.Equals("BMW.Rheingold.OnlineMode"))
            {
                return false;
            }
            AddNewKey(key, defaultValue);
            string configStringInternal = GetConfigStringInternal(key, null);
            if (configStringInternal != null)
            {
                bool flag = string.Compare(configStringInternal, "true", StringComparison.OrdinalIgnoreCase) == 0;
                StoreAndLogChangedValue(key, flag);
                return flag;
            }
            return defaultValue;
        }

        public static int getConfigint(string key, int defaultValue)
        {
            try
            {
                AddNewKey(key, defaultValue);
                string configStringInternal = GetConfigStringInternal(key, null);
                if (configStringInternal != null)
                {
                    int num2 = Convert.ToInt32(configStringInternal, CultureInfo.InvariantCulture);
                    StoreAndLogChangedValue(key, num2);
                    return num2;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ConfigSettings.getConfigint()", "Read configuration for \"{0}\" failed: {1}", key, ex);
            }
            return defaultValue;
        }

        public static string getPathString(string key, string defaultValue)
        {
            AddNewKey(key, defaultValue);
            string configStringInternal = GetConfigStringInternal(key, defaultValue);
            string text = configStringInternal;
            if (configStringInternal != null && configStringInternal.Contains("%"))
            {
                text = Environment.ExpandEnvironmentVariables(configStringInternal);
            }
            StoreAndLogChangedValue(key, text);
            if (!string.IsNullOrEmpty(text))
            {
                return Path.Combine(Path.GetDirectoryName(text), Path.GetFileName(text));
            }
            return text;
        }

        public static string GetIstaConfigString(string key, string defaultValue)
        {
            try
            {
                string configString = getConfigString("HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\ISTA", key, null);
                if (configString == null)
                {
                    configString = getConfigString("HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\ISTA", key, null);
                    if (configString != null)
                    {
                        return configString;
                    }
                    return defaultValue;
                }
                return configString;
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.GetIstaConfigString()", exception);
            }
            return null;
        }

        public static bool putConfigString(string path, string key, string value)
        {
            try
            {
                Registry.SetValue(path, key, value);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.putConfigString()", exception);
            }
            return false;
        }

        private static bool PutConfigStringForMultisession(string key, string value)
        {
            try
            {
                bool num2 = currentConfigValues.ContainsKey(key);
                string text = (num2 ? (currentConfigValues[key].Value as string) : null);
                currentConfigValues[key] = new ConfigValue(value, null);
                if (num2)
                {
                    LogInfo("ConfigSettings.PutConfigStringForMultisession()", "Change value for key \"{0}\": from \"{1}\" to \"{2}\"", key, text, value);
                }
                else
                {
                    LogInfo("ConfigSettings.PutConfigStringForMultisession()", "Set value for key \"{0}\": \"{1}\"", key, value);
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("ConfigSettings.PutConfigStringForMultisession()", exception);
            }
            return false;
        }

        public static bool putGlobalConfigString(string key, string value)
        {
            return PutGlobalConfigString("HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold", key, value);
        }

        public static IDictionary<string, string> GetKeyValuePairs(string configKey)
        {
            string configStringInternal = GetConfigStringInternal(configKey, string.Empty, setOrigin: false);
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(configStringInternal))
            {
                string[] array = configStringInternal.Split('|');
                for (int i = 0; i < array.Length; i++)
                {
                    Match match = Regex.Match(array[i], "^\\s*(?<key>[^,]+)\\s*,\\s*(?<value>[^,]+)\\s*$");
                    if (match.Success)
                    {
                        dictionary.Add(match.Groups["key"].Value, match.Groups["value"].Value);
                    }
                }
            }
            return dictionary;
        }

        public static void SerializeObject<T>(string key, T obj)
        {
            if (obj == null)
            {
                putConfigString(key, string.Empty);
                return;
            }
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, obj);
                memoryStream.Position = 0L;
                xmlDocument.Load(memoryStream);
            }
            if (!string.IsNullOrEmpty(xmlDocument.InnerXml))
            {
                putConfigString(key, xmlDocument.InnerXml);
            }
        }

        public static T DeserializeObject<T>(string key, ref T obj)
        {
            string configStringInternal = GetConfigStringInternal(key, string.Empty, setOrigin: false);
            if (!string.IsNullOrEmpty(configStringInternal))
            {
                try
                {
                    using (StringReader textReader = new StringReader(configStringInternal))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                        obj = (T)xmlSerializer.Deserialize(textReader);
                    }
                }
                catch (Exception exception)
                {
                    Log.ErrorException("ConfigSettings.DeserializeObject()", exception);
                    return obj;
                }
            }
            return obj;
        }

        public static bool CheckBMWGroupClient()
        {
            bool result = false;
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\BMWGroup\\Image Information"))
                {
                    result = registryKey != null;
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ConfigSettings.CheckBMWGroupClient()", exception);
            }
            return result;
        }

        private static void LogInfo(string method, string msg, params object[] args)
        {
            if (isLogStarted)
            {
                Log.Info(method, msg, args);
            }
            else
            {
                firstLogging.Add(new LogValue(method, msg, args));
            }
        }

        public static void NotifyLogStarted()
        {
            isLogStarted = true;
            foreach (LogValue item in firstLogging)
            {
                Log.Info(item.Method, item.Message, item.Arguments);
            }
            firstLogging.Clear();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        public static bool IsProgrammingEnabled()
        {
            if (IsProgrammingLocked)
            {
                return false;
            }
            if (areSDPPacksInvalid)
            {
                return false;
            }
            return getConfigStringAsBoolean("BMW.Rheingold.Programming.Enabled");
        }

        public static bool IsLogisticBaseEnabled()
        {
            return !IsProgrammingLocked;
        }

        public static bool IsEnabledLogJobTimeSpanActive()
        {
            return getConfigStringAsBoolean("BMW.Rheingold.FASTA.Developer.EnableLogJobTimeSpan", defaultValue: false);
        }

        public static bool IsVehicleTestReadFastaDataActive()
        {
            if (IsLightModeActive)
            {
                return !RunIstaRsuRepairMode;
            }
            return true;
        }

        public static bool IsVehicleIdentReadFastaDataActive()
        {
            bool defaultValue = !IsLightModeActive || !RunIstaRsuRepairMode;
            return getConfigStringAsBoolean("BMW.Rheingold.Diagnostics.VehicleIdent.ReadFASTAData", defaultValue);
        }

        public static bool IsStartWithNativeMeasurePlanEnabled()
        {
            return getConfigStringAsBoolean("BMW.Rheingold.Programming.StartWithNativeMeasurePlanEnabled", defaultValue: false);
        }

        public static bool ShowReleaseNotesOnIstaStart()
        {
            if (!IsOssModeActive && !IsLightModeActive)
            {
                return getConfigStringAsBoolean("BMW.Rheingold.ISTAGUI.Pages.StartPage.ShowReleaseNotes", defaultValue: false);
            }
            return false;
        }

        public static bool ShowDisclaimerOnIstaStart()
        {
            bool defaultValue = !IsLightModeActive;
            if (!IsLightModeActive)
            {
                return getConfigStringAsBoolean("BMW.Rheingold.ISTAGUI.Pages.StartPage.ShowDisclaimer", defaultValue);
            }
            return false;
        }

        public static bool DoInitialIPSAvailabilityCheck()
        {
            bool defaultValue = !IsLightModeActive;
            return getConfigStringAsBoolean("BMW.Rheingold.ISTAGUI.App.DoInitialIpsAvailabilityCheck", defaultValue);
        }

        public static bool IsCheckAppVersionEnabled()
        {
            bool defaultValue = !IsLightModeActive;
            return getConfigStringAsBoolean("BMW.Rheingold.Database.CheckAppVersion.Enabled", defaultValue);
        }

        public static EnumOrderTypePreselection GetOrderTypePreSelection()
        {
            if (IsLightModeActive)
            {
                return EnumOrderTypePreselection.Breakdown;
            }
            return EnumParseConfig("BMW.Rheingold.BreakdownSelection", EnumOrderTypePreselection.Workshop);
        }

        public static bool IsIgnoreIstaPackageCheckEnabled()
        {
            bool defaultValue = (IsLightModeActive ? true : false);
            return getConfigStringAsBoolean("BMW.Rheingold.ISTAGui.IgnoreCommandCheckISTAPackages", defaultValue);
        }

        public static bool IsPatchVersion(string appVersion)
        {
            string configString = getConfigString("HKEY_LOCAL_MACHINE\\SOFTWARE\\BMWGroup\\ISPI\\ISTA", "PatchVersion", string.Empty);
            if (string.IsNullOrEmpty(configString))
            {
                return false;
            }
            try
            {
                List<int> list = (from c in appVersion.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries)
                                  select int.Parse(c)).ToList();
                List<int> list2 = (from c in configString.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries)
                                   select int.Parse(c)).ToList();
                if (list.Count != list2.Count)
                {
                    return false;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != list2[i])
                    {
                        Log.Info(Log.CurrentMethod(), "Patch and App Version are not the same");
                        return false;
                    }
                }
            }
            catch
            {
                Log.Warning(Log.CurrentMethod(), "AppVersion and Patch version could not be compared");
                return false;
            }
            return true;
        }

        public static T EnumParseConfig<T>(string key, T defaultValue)
        {
            try
            {
                string configString = getConfigString(key, defaultValue.ToString());
                return (T)Enum.Parse(typeof(T), configString);
            }
            catch (Exception ex)
            {
                Log.Warning("GlobalSettingsObject.ParseConfig()", $"Failed to read value from key '{key}' of type '{typeof(T)}'. Setting default: '{defaultValue}'.", ex);
                return defaultValue;
            }
        }

        public static PrintFormat GetPrintingFormat()
        {
            PrintFormat printFormat = PrintFormat.PDF;
            string text = string.Empty;
            try
            {
                text = getConfigString("BMW.Rheingold.Print.Formats", printFormat.ToString()).Trim(',').ToUpper();
                IOrderedEnumerable<string> first = from e in text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   select e.Trim() into t
                                                   orderby t
                                                   select t;
                IOrderedEnumerable<string> second = from t in Enum.GetNames(typeof(PrintFormat))
                                                    where t != PrintFormat.ALL.ToString()
                                                    orderby t
                                                    select t;
                if (first.SequenceEqual(second))
                {
                    return PrintFormat.ALL;
                }
                return (PrintFormat)Enum.Parse(typeof(PrintFormat), text);
            }
            catch (Exception ex)
            {
                Log.Warning("GlobalSettingsObject.GetPrintingFormat()", string.Format("Unable to parse {0} read from key '{0}' of type '{1}'. Setting default: '{2}'.", text, "BMW.Rheingold.Print.Formats", typeof(PrintFormat), printFormat), ex);
                return printFormat;
            }
        }

        public static bool AreCheckControlMessagesEnabled()
        {
            return getConfigStringAsBoolean("BMW.Rheingold.NewFaultMemory.ExpertMode.CCM.Enabled", defaultValue: true);
        }

        public static string getConfigString(string path, string key, string defaultValue)
        {
            try
            {
                if (path.Contains("HKEY_LOCAL_MACHINE"))
                {
                    try
                    {
                        return getConfigStringLocalMachine(path, key, defaultValue);
                    }
                    catch (Exception)
                    {
                        Log.Error("ConfigSettings.getConfigStringLocalMachine()", "Cannot find Key: {0} in Path: {1}", key, path);
                    }
                }
                object value = Registry.GetValue(path, key, null);
                if (value != null)
                {
                    string text = string.Empty;
                    if (value is string text2)
                    {
                        text = text2;
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text;
                        }
                    }
                    if (value is string[] array)
                    {
                        string[] array2 = array;
                        foreach (string text3 in array2)
                        {
                            text += text3;
                        }
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text;
                        }
                    }
                }
                try
                {
                    string text4 = ConfigurationManager.AppSettings[key];
                    if (!string.IsNullOrEmpty(text4))
                    {
                        return text4;
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("ConfigSettings.getConfigString()", exception);
                }
            }
            catch (Exception exception2)
            {
                Log.WarningException("ConfigSettings.getConfigString()", exception2);
            }
            return defaultValue;
        }

        public static bool putConfigString(string key, string value)
        {
            if (isMaster)
            {
                return putConfigString("HKEY_CURRENT_USER\\SOFTWARE\\BMWGroup\\ISPI\\Rheingold", key, value);
            }
            return PutConfigStringForMultisession(key, value);
        }
    }
}
