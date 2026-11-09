using Android.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UdsFileReader;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable ConvertToUsingDeclaration

namespace BmwDeepObd
{
    public partial class ActivityCommon
    {
        public class SerialInfoEntry: IEquatable<SerialInfoEntry>
        {
            public SerialInfoEntry() : this(string.Empty, string.Empty, false, false)
            {
            }

            public SerialInfoEntry(string serial, string oem, bool disabled, bool valid)
            {
                Serial = serial;
                Oem = oem;
                Disabled = disabled;
                Valid = valid;
                hashCode = null;
            }

            [XmlElement("Serial")]
            public string Serial { get; set; }

            [XmlElement("Oem")]
            public string Oem { get; set; }

            [XmlElement("Disabled")]
            public bool Disabled { get; set; }

            [XmlElement("Valid")]
            public bool Valid { get; set; }

            private int? hashCode;

            public bool IsDefaultSerial()
            {
                if (string.IsNullOrEmpty(Serial))
                {
                    return true;
                }

                if (string.Compare(Serial, "0000000000000000", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                if (string.Compare(Serial, "FFFFFFFFFFFFFFFF", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                return false;
            }

            public override bool Equals(object obj)
            {
                SerialInfoEntry serialInfo = obj as SerialInfoEntry;
                if ((object)serialInfo == null)
                {
                    return false;
                }

                return Equals(serialInfo);
            }

            public bool Equals(SerialInfoEntry serialInfo)
            {
                if (Serial == null || (object)serialInfo == null || serialInfo.Serial == null)
                {
                    return false;
                }

                if (string.Compare(serialInfo.Serial, Serial, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                if (!hashCode.HasValue)
                {
                    string serial = Serial ?? string.Empty;
                    hashCode = serial.GetHashCode();
                }

                return hashCode.Value;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }

            public static bool operator == (SerialInfoEntry lhs, SerialInfoEntry rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return Object.Equals(lhs, rhs);
                }

                return lhs.Equals(rhs);
            }

            public static bool operator !=(SerialInfoEntry lhs, SerialInfoEntry rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return !Object.Equals(lhs, rhs);
                }

                return !(lhs == rhs);
            }
        }

        [XmlInclude(typeof(SerialInfoEntry))]
        [XmlType("Settings")]
        public class StorageData
        {
            public StorageData()
            {
                InitCommonData();
                if (BaseActivity.GetActivityFromStack(typeof(ActivityMain)) is ActivityMain activityMain)
                {
                    InitData(activityMain.InstanceDataMain, activityMain.ActivityCommonMain);
                }
            }

            public StorageData(InstanceDataCommon instanceData, ActivityCommon activityCommon, bool storage = false)
            {
                InitCommonData();
                InitData(instanceData, activityCommon, storage);
            }

            public void InitCommonData()
            {
                this.LastAppState = LastAppState.Init;
                this.SelectedTheme = ActivityCommon.SelectedTheme ?? ThemeDefault;
                this.DeviceName = string.Empty;
                this.DeviceAddress = string.Empty;
                this.ConfigFileName = string.Empty;
                this.UpdateCheckTime = DateTime.MinValue.Ticks;
                this.UpdateSkipVersion = -1;
                this.LastVersionCode = -1;
                this.StorageRequirementsAccepted = false;
                this.XmlEditorPackageName = string.Empty;
                this.XmlEditorClassName = string.Empty;

                this.RecentConfigFiles = new List<string>();
                this.CustomStorageMedia = ActivityCommon.CustomStorageMedia;
                this.CopyToAppSrc = ActivityCommon.CopyToAppSrc;
                this.CopyToAppDst = ActivityCommon.CopyToAppDst;
                this.CopyFromAppSrc = ActivityCommon.CopyFromAppSrc;
                this.CopyFromAppDst = ActivityCommon.CopyFromAppDst;
                this.UsbFirmwareFileName = ActivityCommon.UsbFirmwareFileName;
                this.EnableTranslation = ActivityCommon.EnableTranslation;
                this.YandexApiKey = ActivityCommon.YandexApiKey;
                this.IbmTranslatorApiKey = ActivityCommon.IbmTranslatorApiKey;
                this.IbmTranslatorUrl = ActivityCommon.IbmTranslatorUrl;
                this.DeeplApiKey = ActivityCommon.DeeplApiKey;
                this.YandexCloudApiKey = ActivityCommon.YandexCloudApiKey;
                this.YandexCloudFolderId = ActivityCommon.YandexCloudFolderId;
                this.GoogleApisUrl = ActivityCommon.GoogleApisUrl;
                this.Translator = ActivityCommon.SelectedTranslator;
                this.ShowBatteryVoltageWarning = ActivityCommon.ShowBatteryVoltageWarning;
                this.BatteryWarnings = ActivityCommon.BatteryWarnings;
                this.BatteryWarningVoltage = ActivityCommon.BatteryWarningVoltage;
                this.SerialInfo = new List<SerialInfoEntry>();
                this.AdapterBlacklist = ActivityCommon.AdapterBlacklist;
                this.LastAdapterSerial = ActivityCommon.LastAdapterSerial;
                this.EmailAddress = ActivityCommon.EmailAddress;
                this.TraceInfo = ActivityCommon.TraceInfo;
                this.AppId = ActivityCommon.AppId;
                this.AutoHideTitleBar = ActivityCommon.AutoHideTitleBar;
                this.SuppressTitleBar = ActivityCommon.SuppressTitleBar;
                this.FullScreenMode = ActivityCommon.FullScreenMode;
                this.SwapMultiWindowOrientation = ActivityCommon.SwapMultiWindowOrientation;
                this.SelectedInternetConnection = ActivityCommon.SelectedInternetConnection;
                this.SelectedManufacturer = ActivityCommon.SelectedManufacturer;
                this.BtEnbaleHandling = ActivityCommon.BtEnbaleHandling;
                this.BtDisableHandling = ActivityCommon.BtDisableHandling;
                this.LockTypeCommunication = ActivityCommon.LockTypeCommunication;
                this.LockTypeLogging = ActivityCommon.LockTypeLogging;
                this.StoreDataLogSettings = ActivityCommon.StoreDataLogSettings;
                this.AutoConnectHandling = ActivityCommon.AutoConnectHandling;
                this.UpdateCheckDelay = ActivityCommon.UpdateCheckDelay;
                this.DoubleClickForAppExit = ActivityCommon.DoubleClickForAppExit;
                this.SendDataBroadcast = ActivityCommon.SendDataBroadcast;
                this.CheckCpuUsage = ActivityCommon.CheckCpuUsage;
                this.CheckEcuFiles = ActivityCommon.CheckEcuFiles;
                this.OldVagMode = ActivityCommon.OldVagMode;
                this.UseBmwDatabase = ActivityCommon.UseBmwDatabase;
                this.ShowOnlyRelevantErrors = ActivityCommon.ShowOnlyRelevantErrors;
                this.ScanAllEcus = ActivityCommon.ScanAllEcus;
                this.CollectDebugInfo = ActivityCommon.CollectDebugInfo;
                this.CompressTrace = ActivityCommon.CompressTrace;
                this.DisableNetworkCheck = ActivityCommon.DisableNetworkCheck;
                this.DisableFileNameEncoding = ActivityCommon.DisableFileNameEncoding;
            }

            public void InitData(InstanceDataCommon instanceData, ActivityCommon activityCommon, bool storage = false)
            {
                if (instanceData == null || activityCommon == null)
                {
                    return;
                }

                this.LastAppState = instanceData.LastAppState;
                this.LastSelectedJobIndex = instanceData.LastSelectedJobIndex;
                this.SelectedEnetIp = activityCommon.SelectedEnetIp;
                this.SelectedElmWifiIp = activityCommon.SelectedElmWifiIp;
                this.SelectedDeepObdWifiIp = activityCommon.SelectedDeepObdWifiIp;
                this.MtcBtDisconnectWarnShown = activityCommon.MtcBtDisconnectWarnShown;
                this.DeviceName = instanceData.DeviceName;
                this.DeviceAddress = instanceData.DeviceAddress;
                this.ConfigFileName = instanceData.ConfigFileName;
                this.UpdateCheckTime = instanceData.UpdateCheckTime;
                this.UpdateSkipVersion = instanceData.UpdateSkipVersion;
                this.LastVersionCode = activityCommon.VersionCode;
                this.StorageRequirementsAccepted = instanceData.StorageRequirementsAccepted;
                this.XmlEditorPackageName = instanceData.XmlEditorPackageName ?? string.Empty;
                this.XmlEditorClassName = instanceData.XmlEditorClassName ?? string.Empty;
                this.DataLogActive = instanceData.DataLogActive;
                this.DataLogAppend = instanceData.DataLogAppend;
                if (storage)
                {
                    this.RecentLocale = LocaleUtils.GetLocaleSetting(activityCommon.Context, true);
                    this.RecentConfigFiles = RecentConfigUtils.GetRecentConfigList();
                    this.SerialInfo = SerialInfoUtils.GetSerialInfoList();
                }
            }

            public string CalcualeHash()
            {
                StringBuilder sb = new StringBuilder();
                PropertyInfo[] properties = GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    object value = property.GetValue(this);
                    if (value != null)
                    {
                        sb.Append(value);
                    }
                }

                using (SHA256 sha256 = SHA256.Create())
                {
                    return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "");
                }
            }

            [XmlElement("LastAppState")] public LastAppState LastAppState { get; set; }
            [XmlElement("LastSelectedJobIndex")] public int LastSelectedJobIndex { get; set; }
            [XmlElement("Theme")] public ThemeType SelectedTheme { get; set; }
            [XmlElement("EnetIp")] public string SelectedEnetIp { get; set; }
            [XmlElement("ElmWifiIp")] public string SelectedElmWifiIp { get; set; }
            [XmlElement("DeepObdWifiIp")] public string SelectedDeepObdWifiIp { get; set; }
            [XmlElement("MtcBtDisconnectWarnShown")] public bool MtcBtDisconnectWarnShown { get; set; }
            [XmlElement("DeviceName")] public string DeviceName { get; set; }
            [XmlElement("DeviceAddress")] public string DeviceAddress { get; set; }
            [XmlElement("ConfigFile")] public string ConfigFileName { get; set; }
            [XmlElement("UpdateCheckTime")] public long UpdateCheckTime { get; set; }
            [XmlElement("UpdateSkipVersion")] public int UpdateSkipVersion { get; set; }
            [XmlElement("VersionCode")] public long LastVersionCode { get; set; }
            [XmlElement("StorageAccepted")] public bool StorageRequirementsAccepted { get; set; }
            [XmlElement("XmlEditorPackageName")] public string XmlEditorPackageName { get; set; }
            [XmlElement("XmlEditorClassName")] public string XmlEditorClassName { get; set; }
            [XmlElement("DataLogActive")] public bool DataLogActive { get; set; }
            [XmlElement("DataLogAppend")] public bool DataLogAppend { get; set; }

            [XmlElement("RecentLocale")] public string RecentLocale { get; set; }
            [XmlElement("RecentConfigFiles")] public List<string> RecentConfigFiles { get; set; }
            [XmlElement("StorageMedia")] public string CustomStorageMedia { get; set; }
            [XmlElement("CopyToAppSrc")] public string CopyToAppSrc { get; set; }
            [XmlElement("CopyToAppDst")] public string CopyToAppDst { get; set; }
            [XmlElement("CopyFromAppSrc")] public string CopyFromAppSrc { get; set; }
            [XmlElement("CopyFromAppDst")] public string CopyFromAppDst { get; set; }
            [XmlElement("UsbFirmwareFile")] public string UsbFirmwareFileName { get; set; }
            [XmlElement("EnableTranslation")] public bool EnableTranslation { get; set; }
            [XmlElement("YandexApiKey")] public string YandexApiKey { get; set; }
            [XmlElement("IbmTranslatorApiKey")] public string IbmTranslatorApiKey { get; set; }
            [XmlElement("IbmTranslatorUrl")] public string IbmTranslatorUrl { get; set; }
            [XmlElement("DeeplApiKey")] public string DeeplApiKey { get; set; }
            [XmlElement("YandexCloudApiKey")] public string YandexCloudApiKey { get; set; }
            [XmlElement("YandexCloudFolderId")] public string YandexCloudFolderId { get; set; }
            [XmlElement("GoogleApisUrl")] public string GoogleApisUrl { get; set; }
            [XmlElement("Translator")] public TranslatorType Translator { get; set; }
            [XmlElement("ShowBatteryVoltageWarning")] public bool ShowBatteryVoltageWarning { get; set; }
            [XmlElement("BatteryWarnings")] public long BatteryWarnings { get; set; }
            [XmlElement("BatteryWarningVoltage")] public double BatteryWarningVoltage { get; set; }
            [XmlElement("SerialInfo")] public List<SerialInfoEntry> SerialInfo { get; set; }
            [XmlElement("AdapterBlacklist")] public string AdapterBlacklist { get; set; }
            [XmlElement("LastAdapterSerial")] public string LastAdapterSerial { get; set; }
            [XmlElement("EmailAddress")] public string EmailAddress { get; set; }
            [XmlElement("TraceInfo")] public string TraceInfo { get; set; }
            [XmlElement("AppId")] public string AppId { get; set; }
            [XmlElement("AutoHideTitleBar")] public bool AutoHideTitleBar { get; set; }
            [XmlElement("SuppressTitleBar")] public bool SuppressTitleBar { get; set; }
            [XmlElement("FullScreenMode")] public bool FullScreenMode { get; set; }
            [XmlElement("SwapMultiWindowOrientation")] public bool SwapMultiWindowOrientation { get; set; }
            [XmlElement("InternetConnection")] public InternetConnectionType SelectedInternetConnection { get; set; }
            [XmlElement("Manufacturer")] public ManufacturerType SelectedManufacturer { get; set; }
            [XmlElement("BtEnbale")] public BtEnableType BtEnbaleHandling { get; set; }
            [XmlElement("BtDisable")] public BtDisableType BtDisableHandling { get; set; }
            [XmlElement("LockComm")] public LockType LockTypeCommunication { get; set; }
            [XmlElement("LockLog")] public LockType LockTypeLogging { get; set; }
            [XmlElement("StoreDataLogSettings")] public bool StoreDataLogSettings { get; set; }
            [XmlElement("AutoConnect")] public AutoConnectType AutoConnectHandling { get; set; }
            [XmlElement("UpdateCheckDelay")] public long UpdateCheckDelay { get; set; }
            [XmlElement("DoubleClickForAppExit")] public bool DoubleClickForAppExit { get; set; }
            [XmlElement("SendDataBroadcast")] public bool SendDataBroadcast { get; set; }
            [XmlElement("CheckCpuUsage")] public bool CheckCpuUsage { get; set; }
            [XmlElement("CheckEcuFiles")] public bool CheckEcuFiles { get; set; }
            [XmlElement("OldVagMode")] public bool OldVagMode { get; set; }
            [XmlElement("UseBmwDatabase")] public bool UseBmwDatabase { get; set; }
            [XmlElement("ShowOnlyRelevantErrors")] public bool ShowOnlyRelevantErrors { get; set; }
            [XmlElement("ScanAllEcus")] public bool ScanAllEcus { get; set; }
            [XmlElement("CollectDebugInfo")] public bool CollectDebugInfo { get; set; }
            [XmlElement("CompressTrace")] public bool CompressTrace { get; set; }
            // hidden settings
            [XmlElement("DisableNetworkCheck")] public bool DisableNetworkCheck { get; set; }
            [XmlElement("DisableFileNameEncoding"), DefaultValue(false)] public bool DisableFileNameEncoding { get; set; }
        }

        public class VagEcuEntry
        {
            public VagEcuEntry(string sysName, int address)
            {
                SysName = sysName;
                Address = address;
            }

            public string SysName { get; }
            public int Address { get; }
        }

        public class VagDtcEntry
        {
            public VagDtcEntry(uint dtcCode, byte dtcDetail, DataReader.ErrorType errorType)
            {
                DtcCode = dtcCode;
                DtcDetail = dtcDetail;
                ErrorType = errorType;
            }

            public uint DtcCode { get; }
            public byte DtcDetail { get; }
            public DataReader.ErrorType ErrorType { get; }
        }

        public class MwTabEntry
        {
            public MwTabEntry(long blockNumber, long? valueIndex, string description, string comment, string valueUnit, string valueType, double? valueMin, double? valueMax, bool dummy = false)
            {
                BlockNumber = blockNumber;
                ValueIndex = valueIndex;
                Description = description;
                Comment = comment;
                ValueUnit = valueUnit;
                ValueType = valueType;
                ValueMin = valueMin;
                ValueMax = valueMax;
                Dummy = dummy;
            }

            public long BlockNumber { get; }
            public long? ValueIndex { get; }
            public long ValueIndexTrans
            {
                get
                {
                    if (!ValueIndex.HasValue)
                    {
                        return 0;
                    }
                    return ValueIndex.Value;
                }
            }
            public string Description { get; }
            public string Comment { get; }
            public string ValueUnit { get; }
            public string ValueType { get; }
            public double? ValueMin { get; }
            public double? ValueMax { get; }
            public bool Dummy { get; }
        }

        public class MwTabFileEntry : IComparable<MwTabFileEntry>
        {
            public MwTabFileEntry(string fileName, List<MwTabEntry> mwTabList)
            {
                FileName = fileName;
                MwTabList = mwTabList;
                ExistCount = 0;
                CompareCount = 0;
                MatchCount = 0;
                MatchRatio = 0;
            }

            public const int MaxMatchRatio = 1000;
            public string FileName { get; }
            public List<MwTabEntry> MwTabList { get; }
            public int ExistCount { get; set; }
            public int CompareCount { get; set; }
            public int MatchCount { get; set; }
            public int MatchRatio { get; set; }

            public int CompareTo(MwTabFileEntry mwTabFileEntry)
            {
                if (MatchRatio == mwTabFileEntry.MatchRatio)
                {
                    if (MatchCount == mwTabFileEntry.MatchCount)
                    {
                        // ReSharper disable once StringCompareToIsCultureSpecific
                        return mwTabFileEntry.FileName.CompareTo(FileName);
                    }
                    return MatchCount - mwTabFileEntry.MatchCount;
                }
                return MatchRatio - mwTabFileEntry.MatchRatio;
            }
        }

        public class InstanceDataCommon
        {
            public InstanceDataCommon()
            {
                this.LastAppState = LastAppState.Init;
                this.LastSelectedJobIndex = -1;
                this.LastSettingsHash = string.Empty;
                this.AppDataPath = string.Empty;
                this.EcuPath = string.Empty;
                this.VagPath = string.Empty;
                this.TraceActive = true;
                this.DeviceName = string.Empty;
                this.DeviceAddress = string.Empty;
                this.ConfigFileName = string.Empty;
                this.CheckCpuUsage = true;
                this.ExtractSampleFiles = true;
                this.ExtractCaCertFiles = true;
                this.VerifyEcuFiles = true;
                this.SelectedEnetIp = string.Empty;
                this.SelectedElmWifiIp = string.Empty;
                this.SelectedDeepObdWifiIp = string.Empty;
            }

            public ThemeType? LastThemeType { get; set; }
            public LastAppState LastAppState { get; set; }
            public int LastSelectedJobIndex { get; set; }
            public string LastSettingsHash { get; set; }
            public bool GetSettingsCalled { get; set; }
            public string AppDataPath { get; set; }
            public string EcuPath { get; set; }
            public string VagPath { get; set; }
            public string BmwPath { get; set; }
            public string SimulationPath { get; set; }
            public bool UserEcuFiles { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool DataLogActive { get; set; }
            public bool DataLogAppend { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string ConfigFileName { get; set; }
            public long LastVersionCode { get; set; }
            public bool VersionInfoShown { get; set; }
            public bool StorageRequirementsAccepted { get; set; }
            public bool LocationProviderShown { get; set; }
            public bool BatteryWarningShown { get; set; }
            public bool ConfigMatchVehicleShown { get; set; }
            public bool DataLogTemporaryShown { get; set; }
            public bool CheckCpuUsage { get; set; }
            public bool ExtractSampleFiles { get; set; }
            public bool ExtractCaCertFiles { get; set; }
            public bool VerifyEcuFiles { get; set; }
            public bool VerifyEcuMd5 { get; set; }
            public int CommErrorsCount { get; set; }
            public bool AutoStart { get; set; }
            public bool AdapterCheckOk { get; set; }
            public bool VagInfoShown { get; set; }
            public string DataLogDir { get; set; }
            public string TraceDir { get; set; }
            public string TraceBackupDir { get; set; }
            public string PackageAssembliesDir { get; set; }
            public bool UpdateAvailable { get; set; }
            public int UpdateVersionCode { get; set; }
            public string UpdateMessage { get; set; }
            public long UpdateCheckTime { get; set; }
            public int UpdateSkipVersion { get; set; }
            public string XmlEditorPackageName { get; set; }
            public string XmlEditorClassName { get; set; }

            public InterfaceType SelectedInterface { get; set; }
            public string SelectedEnetIp { get; set; }
            public string SelectedElmWifiIp { get; set; }
            public string SelectedDeepObdWifiIp { get; set; }
        }

        public enum ThemeType
        {
            [XmlEnum(Name = "Dark")] Dark,
            [XmlEnum(Name = "Light")] Light,
            [XmlEnum(Name = "System")] System,
        }

        public enum InterfaceType
        {
            [XmlEnum(Name = "None"), Description("None")] None,
            [XmlEnum(Name = "Bluetooth"), Description("Bluetooth")] Bluetooth,
            [XmlEnum(Name = "Enet"), Description("Enet")] Enet,
            [XmlEnum(Name = "ElmWifi"), Description("ElmWifi")] ElmWifi,
            [XmlEnum(Name = "DeepObdWifi"), Description("DeepObdWifi")] DeepObdWifi,
            [XmlEnum(Name = "Ftdi"), Description("Ftdi")] Ftdi,
            [XmlEnum(Name = "Simulations"), Description("Simulation")] Simulation,
        }

        public enum InternetConnectionType
        {
            [XmlEnum(Name = "Cellular")] Cellular,
            [XmlEnum(Name = "Wifi")] Wifi,
            [XmlEnum(Name = "Ethernet")] Ethernet,
        }

        public enum ManufacturerType
        {
            [XmlEnum(Name = "Bmw")] Bmw,
            [XmlEnum(Name = "Audi")] Audi,
            [XmlEnum(Name = "Seat")] Seat,
            [XmlEnum(Name = "Skoda")] Skoda,
            [XmlEnum(Name = "Vw")] Vw,
        }

        public enum LockType
        {
            [XmlEnum(Name = "None")] None,                  // no lock
            [XmlEnum(Name = "Cpu")] Cpu,                    // CPU lock
            [XmlEnum(Name = "ScreenDim")] ScreenDim,        // screen dim lock
            [XmlEnum(Name = "ScreenBright")] ScreenBright,  // screen bright lock
        }

        public enum BtEnableType
        {
            [XmlEnum(Name = "Ask")] Ask,                    // ask for enbale
            [XmlEnum(Name = "Always")] Always,              // always enable
            [XmlEnum(Name = "Nothing")] Nothing             // no handling
        }

        public enum BtDisableType
        {
            [XmlEnum(Name = "DisableIfByApp")] DisableIfByApp,  // disable if enabled by app
            [XmlEnum(Name = "Nothing")] Nothing                 // no handling
        }

        public enum AutoConnectType
        {
            [XmlEnum(Name = "Offline")] Offline,                // no auto connect
            [XmlEnum(Name = "Connect")] Connect,                // auto connect
            [XmlEnum(Name = "ConnectClose")] ConnectClose,      // auto connect and close app
            [XmlEnum(Name = "StartBoot")] StartBoot,            // auto start at boot
        }

        public enum TranslatorType
        {
            [XmlEnum(Name = "YandexTranslate")] YandexTranslate,    // Yandex.translate
            [XmlEnum(Name = "IbmWatson")] IbmWatson,                // IBM Watson Translator
            [XmlEnum(Name = "DeepL")] Deepl,                        // DeepL
            [XmlEnum(Name = "YandexCloud")] YandexCloud,            // Yandex cloud
            [XmlEnum(Name = "GoogleApis")] GoogleApis,              // Google APIs
        }

        public enum LastAppState
        {
            [XmlEnum(Name = "Init")] Init,
            [XmlEnum(Name = "Compile")] Compile,
            [XmlEnum(Name = "Compiled")] Compiled,
            [XmlEnum(Name = "TabsCreated")] TabsCreated,
            [XmlEnum(Name = "Stopped")] Stopped,
        }

        public enum SettingsMode
        {
            All,
            Private,
            Public,
        }

        public enum SsidWarnAction
        {
            None,
            Continue,
            EditIp,
        }

        public delegate bool ProgressZipDelegate(int percent, bool decrypt = false);
        public delegate bool ProgressVerifyDelegate(int percent);
        public delegate bool ProgressApkDelegate(int percent);
        public delegate bool ProgressDocumentCopyDelegate(string name);
        public delegate void BcReceiverUpdateDisplayDelegate();
        public delegate void BcReceiverReceivedDelegate(Context context, Intent intent);
        public delegate void TranslateDelegate(List<string> transList);
        public delegate void TranslateLoginDelegate(bool success);
        public delegate void UpdateCheckDelegate(bool success, bool updateAvailable, int? appVer, string message);
        public delegate void EnetSsidWarnDelegate(SsidWarnAction action);
        public delegate void WifiConnectedWarnDelegate();
        public delegate void WifiPermissionGranted(bool granted);
        public delegate void BtPermissionGranted(bool granted);
        public delegate void InitThreadFinishDelegate(bool result);
        public delegate bool InitThreadProgressDelegate(long progress);
        public delegate void CopyDocumentsThreadFinishDelegate(bool result, bool aborted);
        public delegate void DownloadConfigThreadFinishDelegate(bool result, bool aborted = false);
        public delegate void DestroyDelegate();
        public delegate void EdiabasEventDelegate(bool connect);
        public delegate void SendEnetBroadacstDelegate(int count);
        public delegate void RunnablePostDelegate();

        public enum BalloonAlligment
        {
            Center,
            Top,
            Bottom,
        }

        public enum DoIpCertificateStatus
        {
            Unknown,
            ExternalCertValid,
            ExternalCertExpired,
            ExternalCertInvalid,
            InternalCertValid,
            InternalCertInvalid,
        }

        public enum PermissionRequestCodes
        {
            RequestPermissionExternalStorage,
            RequestPermissionNotifications,
            RequestPermissionBluetooth,
            RequestPermissionLocation,
        }
    }
}
