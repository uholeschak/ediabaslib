//#define IO_TEST
using Android.App.Backup;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.OS.Storage;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.Content.PM;
using AndroidX.DocumentFile.Provider;
using AndroidX.Lifecycle;
using BmwDeepObd.Dialogs;
using BmwFileReader;
using EdiabasLib;
using Hoho.Android.UsbSerial.Driver;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Skydoves.BalloonLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Org.BouncyCastle.Asn1.X509;
using UdsFileReader;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable ConvertToUsingDeclaration

namespace BmwDeepObd
{
    public class ActivityCommon : IDisposable
    {
        public class FileSystemBlockInfo
        {
            /// <summary>
            /// The path you asked to check file allocation blocks for
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// The file system block size, in bytes, for the given path
            /// </summary>
            public double BlockSizeBytes { get; set; }

            /// <summary>
            /// Total size of the file system at the given path
            /// </summary>
            public double TotalSizeBytes { get; set; }

            /// <summary>
            /// Available size of the file system at the given path
            /// </summary>
            public double AvailableSizeBytes { get; set; }

            /// <summary>
            /// Total free size of the file system at the given path
            /// </summary>
            public double FreeSizeBytes { get; set; }
        }

        public class YandexCloudIamTokenRequest
        {
            public YandexCloudIamTokenRequest(string oAuthToken)
            {
                OAuthToken = oAuthToken;
            }

            [JsonPropertyName("yandexPassportOauthToken")]
            public string OAuthToken { get; }
        }

        public class YandexCloudListLanguagesRequest
        {
            public YandexCloudListLanguagesRequest(string folderId = null)
            {
                FolderId = folderId;
            }

            [JsonPropertyName("folderId")]
            public string FolderId { get; }
        }

        public class YandexCloudTranslateRequest
        {
            public YandexCloudTranslateRequest(string[] textArray, string source, string target, string folderId = null)
            {
                TextArray = textArray;
                Source = source;
                Target = target;
                Format = "PLAIN_TEXT";
                FolderId = folderId;
            }

            [JsonPropertyName("texts")]
            public string[] TextArray { get; }

            [JsonPropertyName("sourceLanguageCode")]
            public string Source { get; }

            [JsonPropertyName("targetLanguageCode")]
            public string Target { get; }

            [JsonPropertyName("format")]
            public string Format { get; }

            [JsonPropertyName("folderId")]
            public string FolderId { get; }
        }

        public class DeeplTranslateRequest
        {
            public DeeplTranslateRequest(string[] textArray, string source, string target)
            {
                TextArray = textArray;
                Source = source;
                Target = target;
            }

            [JsonPropertyName("text")]
            public string[] TextArray { get; }

            [JsonPropertyName("source_lang")]
            public string Source { get; }

            [JsonPropertyName("target_lang")]
            public string Target { get; }
        }

        public class IbmTranslateRequest
        {
            public IbmTranslateRequest(string[] textArray, string source, string target)
            {
                TextArray = textArray;
                Source = source;
                Target = target;
            }

            [JsonPropertyName("text")]
            public string[] TextArray { get; }

            [JsonPropertyName("source")]
            public string Source { get; }

            [JsonPropertyName("target")]
            public string Target { get; }
        }

        [XmlType("SerialInfo")]
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
                    this.RecentLocale = GetLocaleSetting(activityCommon.Context, true);
                    this.RecentConfigFiles = GetRecentConfigList();
                    this.SerialInfo = GetSerialInfoList();
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
        public delegate void InitThreadFinishDelegate(bool result);
        public delegate bool InitThreadProgressDelegate(long progress);
        public delegate void CopyDocumentsThreadFinishDelegate(bool result, bool aborted);
        public delegate void DestroyDelegate();
        public delegate void EdiabasEventDelegate(bool connect);
        public delegate void RunnablePostDelegate();
        public const int UdsDtcStatusOverride = 0x2C;
        [SupportedOSPlatform("android23.0")]
        public const BuildVersionCodes MinEthernetSettingsVersion = BuildVersionCodes.M;
        public const long UpdateCheckDelayDefault = TimeSpan.TicksPerDay;
        public const ThemeType ThemeDefault = ThemeType.Dark;
        public const int FileIoRetries = 10;
        public const int FileIoRetryDelay = 1000;
        public const int MinSendCommErrors = 3;
        public const int UserNotificationIdMax = 1000;
        public const int BalloonDismissDuration = 4000;
        private const int StreamBufferSize = 4096;
        public const SslProtocols DefaultSslProtocols = SslProtocols.None;
        public const string PrimaryVolumeName = "primary";
        public const string MtcBtAppName = @"com.microntek.bluetooth";
        public const string FreeflaxAutosetAppName = @"freeflax.autoset";
        public const string AutomationAppName = @"com.jens.automation2";
        public const string MacrodroidAppName = @"com.arlosoft.macrodroid";
        public const string DefaultLang = "en";
        public const string BackupExt = ".bak";
        public const string ZipExt = ".zip";
        public const string TraceFileNameStd = "ifh.trc";
        public const string TraceFileNameZip = TraceFileNameStd + ZipExt;
        public const string TraceBackupDir = "TraceBackup";
        public const string ConfigBaseSubDir = "Configurations";
        public const string ConfigSampleSubDir = "Sample";
        public const string SecuritySubDir = "Security";
        public const string CaCertsSubDir = "CaCerts";
        public const string CertsSubDir = "Certificates";
        public const string PackageAssembliesDir = "PackageAssemblies";
        public const string EnetSsidEmpty = "***";
        public const string AdapterSsidDeepObd = "Deep OBD BMW";
        public const string AdapterSsidEnetLink = "ENET-LINK_";
        public const string AdapterSsidModBmw = "modBMW ENET";
        public const string AdapterSsidUniCar = "UniCarScan";
        public const string EmulatorEnetIp = ""; // = "169.254.0.1";
        public const string DeepObdAdapterIp = "192.168.100.1";
        public const string EnetLinkAdapterIp = "192.168.16.254";
        public const string ModBmwAdapterIp = "169.254.128.7";
        public const string DefaultPwdDeepObd = "root";
        public const string DefaultPwdModBmw = "admin";
        public const string SettingsFile = "Settings.xml";
        public const string DownloadDir = "Download";
        public const string LogDir = "Log";
        public const string EcuBaseDir = "Ecu";
        public const string VagBaseDir = "Vag";
        public const string BmwBaseDir = "Bmw";
        public const string EcuDirNameBmw = "EcuBmw";
        public const string EcuDirNameVag = "EcuVag";
        public const string AppNameSpace = "de.holeschak.bmw_deep_obd";
        public const string ContactMail = "ulrich@holeschak.de";
        public const string VagEndDate = "2017-08";
        public const string MimeTypeAppAny = @"application/*";
        public const string WifiApStateChangedAction = "android.net.wifi.WIFI_AP_STATE_CHANGED";
        public const string AppFolderName = AppNameSpace;
        public const string UsbPermissionAction = AppNameSpace + ".USB_PERMISSION";
        public const string PackageNameAction = AppNameSpace + ".Action.PackageName";
        public const string BroadcastXmlEditorPackageName = "XmlEditorPackageName";
        public const string BroadcastXmlEditorClassName = "XmlEditorClassName";
        public const string SettingBluetoothHciLog = "bluetooth_hci_log";
        public const string NotificationChannelCommunication = "NotificationCommunication";
        public const string NotificationChannelGroupCustom = "NotificationGroupCustom";
        public const string NotificationChannelCustomMin = "NotificationCustomMin";
        public const string NotificationChannelCustomLow = "NotificationCustomLow";
        public const string NotificationChannelCustomDefault = "NotificationCustomDefault";
        public const string NotificationChannelCustomHigh = "NotificationCustomHigh";
        public const string NotificationChannelCustomMax = "NotificationCustomMax";
        private const string MailInfoDownloadUrl = @"https://www.holeschak.de/BmwDeepObd/Mail.php";
        private const string UpdateCheckUrl = @"https://www.holeschak.de/BmwDeepObd/Update.php";
        private const string IbmTransVersion = @"version=2018-05-01";
        private const string IbmTransIdentLang = @"/v3/identifiable_languages";
        private const string IbmTransTranslate = @"/v3/translate";
        private const string DeeplFreeUrl = @"https://api-free.deepl.com";
        private const string DeeplProUrl = @"https://api.deepl.com";
        private const string DeeplIdentLang = @"/v2/languages?type=target";
        private const string DeeplTranslate = @"/v2/translate";
        public static Regex Ipv4RegEx = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        public static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        public enum PermissionRequestCodes
        {
            RequestPermissionExternalStorage,
            RequestPermissionNotifications,
            RequestPermissionBluetooth,
            RequestPermissionLocation,
        }

        public const int RequestPermissionExternalStorage = (int) PermissionRequestCodes.RequestPermissionExternalStorage;
        public const int RequestPermissionNotifications = (int)PermissionRequestCodes.RequestPermissionNotifications;
        public const int RequestPermissionBluetooth = (int)PermissionRequestCodes.RequestPermissionBluetooth;
        public const int RequestPermissionLocation = (int)PermissionRequestCodes.RequestPermissionLocation;

        [SupportedOSPlatform("android31.0")]
        public static readonly string[] PermissionsBluetooth =
        {
            Android.Manifest.Permission.BluetoothScan,
            Android.Manifest.Permission.BluetoothConnect,
        };

        public static readonly string[] PermissionsFineLocation =
        {
            Android.Manifest.Permission.AccessFineLocation,
        };

        public static readonly string[] PermissionsCombinedLocation =
        {
            Android.Manifest.Permission.AccessCoarseLocation,
            Android.Manifest.Permission.AccessFineLocation,
        };
#if DEBUG
        private static readonly string Tag = typeof(ActivityCommon).FullName;
#endif
        private static readonly Dictionary<long, string> VagDtcSaeDict = new Dictionary<long, string>
        {
            {16394, "P0010"}, {16395, "P0020"}, {16449, "P0065"}, {16450, "P0066"}, {16451, "P0067"},
            {16485, "P0101"}, {16486, "P0102"}, {16487, "P0103"}, {16489, "P0105"}, {16490, "P0106"},
            {16491, "P0107"}, {16492, "P0108"}, {16496, "P0112"}, {16497, "P0113"}, {16500, "P0116"},
            {16501, "P0117"}, {16502, "P0118"}, {16504, "P0120"}, {16505, "P0121"}, {16506, "P0122"},
            {16507, "P0123"}, {16509, "P0125"}, {16512, "P0128"}, {16514, "P0130"}, {16515, "P0131"},
            {16516, "P0132"}, {16517, "P0133"}, {16518, "P0134"}, {16519, "P0135"}, {16520, "P0136"},
            {16521, "P0137"}, {16522, "P0138"}, {16523, "P0139"}, {16524, "P0140"}, {16525, "P0141"},
            {16534, "P0150"}, {16535, "P0151"}, {16536, "P0152"}, {16537, "P0153"}, {16538, "P0154"},
            {16539, "P0155"}, {16540, "P0156"}, {16541, "P0157"}, {16542, "P0158"}, {16543, "P0159"},
            {16544, "P0160"}, {16545, "P0161"}, {16554, "P0170"}, {16555, "P0171"}, {16556, "P0172"},
            {16557, "P0173"}, {16558, "P0174"}, {16559, "P0175"}, {16566, "P0182"}, {16567, "P0183"},
            {16581, "P0197"}, {16582, "P0198"}, {16585, "P0201"}, {16586, "P0202"}, {16587, "P0203"},
            {16588, "P0204"}, {16589, "P0205"}, {16590, "P0206"}, {16591, "P0207"}, {16592, "P0208"},
            {16599, "P0215"}, {16600, "P0216"}, {16603, "P0219"}, {16605, "P0221"}, {16606, "P0222"},
            {16607, "P0223"}, {16609, "P0225"}, {16610, "P0226"}, {16611, "P0227"}, {16612, "P0228"},
            {16614, "P0230"}, {16618, "P0234"}, {16619, "P0235"}, {16620, "P0236"}, {16621, "P0237"},
            {16622, "P0238"}, {16627, "P0243"}, {16629, "P0245"}, {16630, "P0246"}, {16636, "P0252"},
            {16645, "P0261"}, {16646, "P0262"}, {16648, "P0264"}, {16649, "P0265"}, {16651, "P0267"},
            {16652, "P0268"}, {16654, "P0270"}, {16655, "P0271"}, {16657, "P0273"}, {16658, "P0274"},
            {16660, "P0276"}, {16661, "P0277"}, {16663, "P0279"}, {16664, "P0280"}, {16666, "P0282"},
            {16667, "P0283"}, {16684, "P0300"}, {16685, "P0301"}, {16686, "P0302"}, {16687, "P0303"},
            {16688, "P0304"}, {16689, "P0305"}, {16690, "P0306"}, {16691, "P0307"}, {16692, "P0308"},
            {16697, "P0313"}, {16698, "P0314"}, {16705, "P0321"}, {16706, "P0322"}, {16709, "P0325"},
            {16710, "P0326"}, {16711, "P0327"}, {16712, "P0328"}, {16716, "P0332"}, {16717, "P0333"},
            {16719, "P0335"}, {16720, "P0336"}, {16721, "P0337"}, {16724, "P0340"}, {16725, "P0341"},
            {16726, "P0342"}, {16727, "P0343"}, {16735, "P0351"}, {16736, "P0352"}, {16737, "P0353"},
            {16738, "P0354"}, {16739, "P0355"}, {16740, "P0356"}, {16741, "P0357"}, {16742, "P0358"},
            {16764, "P0380"}, {16784, "P0400"}, {16785, "P0401"}, {16786, "P0402"}, {16787, "P0403"},
            {16788, "P0404"}, {16789, "P0405"}, {16790, "P0406"}, {16791, "P0407"}, {16792, "P0408"},
            {16794, "P0410"}, {16795, "P0411"}, {16796, "P0412"}, {16802, "P0418"}, {16804, "P0420"},
            {16806, "P0422"}, {16811, "P0427"}, {16812, "P0428"}, {16816, "P0432"}, {16820, "P0436"},
            {16821, "P0437"}, {16822, "P0438"}, {16824, "P0440"}, {16825, "P0441"}, {16826, "P0442"},
            {16827, "P0443"}, {16836, "P0452"}, {16837, "P0453"}, {16839, "P0455"}, {16845, "P0461"},
            {16846, "P0462"}, {16847, "P0463"}, {16885, "P0501"}, {16887, "P0503"}, {16889, "P0505"},
            {16890, "P0506"}, {16891, "P0507"}, {16894, "P0510"}, {16915, "P0531"}, {16916, "P0532"},
            {16917, "P0533"}, {16935, "P0551"}, {16944, "P0560"}, {16946, "P0562"}, {16947, "P0563"},
            {16952, "P0568"}, {16955, "P0571"}, {16984, "P0600"}, {16985, "P0601"}, {16986, "P0602"},
            {16987, "P0603"}, {16988, "P0604"}, {16989, "P0605"}, {16990, "P0606"}, {17026, "P0642"},
            {17029, "P0645"}, {17034, "P0650"}, {17038, "P0654"}, {17040, "P0656"}, {17084, "P0700"},
            {17086, "P0702"}, {17087, "P0703"}, {17089, "P0705"}, {17090, "P0706"}, {17091, "P0707"},
            {17092, "P0708"}, {17094, "P0710"}, {17095, "P0711"}, {17096, "P0712"}, {17097, "P0713"},
            {17099, "P0715"}, {17100, "P0716"}, {17101, "P0717"}, {17105, "P0721"}, {17106, "P0722"},
            {17109, "P0725"}, {17110, "P0726"}, {17111, "P0727"}, {17114, "P0730"}, {17115, "P0731"},
            {17116, "P0732"}, {17117, "P0733"}, {17118, "P0734"}, {17119, "P0735"}, {17124, "P0740"},
            {17125, "P0741"}, {17132, "P0748"}, {17134, "P0750"}, {17135, "P0751"}, {17136, "P0752"},
            {17137, "P0753"}, {17140, "P0756"}, {17141, "P0757"}, {17142, "P0758"}, {17145, "P0761"},
            {17146, "P0762"}, {17147, "P0763"}, {17152, "P0768"}, {17157, "P0773"}, {17174, "P0790"},
            {17509, "P1101"}, {17510, "P1102"}, {17511, "P1103"}, {17512, "P1104"}, {17513, "P1105"},
            {17514, "P1106"}, {17515, "P1107"}, {17516, "P1108"}, {17517, "P1109"}, {17518, "P1110"},
            {17519, "P1111"}, {17520, "P1112"}, {17521, "P1113"}, {17522, "P1114"}, {17523, "P1115"},
            {17524, "P1116"}, {17525, "P1117"}, {17526, "P1118"}, {17527, "P1119"}, {17528, "P1120"},
            {17529, "P1121"}, {17530, "P1122"}, {17531, "P1123"}, {17532, "P1124"}, {17533, "P1125"},
            {17534, "P1126"}, {17535, "P1127"}, {17536, "P1128"}, {17537, "P1129"}, {17538, "P1130"},
            {17539, "P1131"}, {17540, "P1132"}, {17541, "P1133"}, {17542, "P1134"}, {17543, "P1135"},
            {17544, "P1136"}, {17545, "P1137"}, {17546, "P1138"}, {17547, "P1139"}, {17548, "P1140"},
            {17549, "P1141"}, {17550, "P1142"}, {17551, "P1143"}, {17552, "P1144"}, {17553, "P1145"},
            {17554, "P1146"}, {17555, "P1147"}, {17556, "P1148"}, {17557, "P1149"}, {17558, "P1150"},
            {17559, "P1151"}, {17560, "P1152"}, {17562, "P1154"}, {17563, "P1155"}, {17564, "P1156"},
            {17565, "P1157"}, {17566, "P1158"}, {17568, "P1160"}, {17569, "P1161"}, {17570, "P1162"},
            {17571, "P1163"}, {17572, "P1164"}, {17573, "P1165"}, {17574, "P1166"}, {17579, "P1171"},
            {17580, "P1172"}, {17581, "P1173"}, {17582, "P1174"}, {17584, "P1176"}, {17585, "P1177"},
            {17586, "P1178"}, {17587, "P1179"}, {17588, "P1180"}, {17589, "P1181"}, {17590, "P1182"},
            {17591, "P1183"}, {17592, "P1184"}, {17593, "P1185"}, {17594, "P1186"}, {17595, "P1187"},
            {17596, "P1188"}, {17597, "P1189"}, {17598, "P1190"}, {17604, "P1196"}, {17605, "P1197"},
            {17606, "P1198"}, {17607, "P1199"}, {17609, "P1201"}, {17610, "P1202"}, {17611, "P1203"},
            {17612, "P1204"}, {17613, "P1205"}, {17614, "P1206"}, {17615, "P1207"}, {17616, "P1208"},
            {17617, "P1209"}, {17618, "P1210"}, {17619, "P1211"}, {17621, "P1213"}, {17622, "P1214"},
            {17623, "P1215"}, {17624, "P1216"}, {17625, "P1217"}, {17626, "P1218"}, {17627, "P1219"},
            {17628, "P1220"}, {17629, "P1221"}, {17630, "P1222"}, {17631, "P1223"}, {17633, "P1225"},
            {17634, "P1226"}, {17635, "P1227"}, {17636, "P1228"}, {17637, "P1229"}, {17638, "P1230"},
            {17639, "P1231"}, {17640, "P1232"}, {17645, "P1237"}, {17646, "P1238"}, {17647, "P1239"},
            {17648, "P1240"}, {17649, "P1241"}, {17650, "P1242"}, {17651, "P1243"}, {17652, "P1244"},
            {17653, "P1245"}, {17654, "P1246"}, {17655, "P1247"}, {17656, "P1248"}, {17657, "P1249"},
            {17658, "P1250"}, {17659, "P1251"}, {17660, "P1252"}, {17661, "P1253"}, {17662, "P1254"},
            {17663, "P1255"}, {17664, "P1256"}, {17665, "P1257"}, {17666, "P1258"}, {17667, "P1259"},
            {17688, "P1280"}, {17691, "P1283"}, {17692, "P1284"}, {17693, "P1285"}, {17694, "P1286"},
            {17695, "P1287"}, {17696, "P1288"}, {17697, "P1289"}, {17704, "P1296"}, {17705, "P1297"},
            {17708, "P1300"}, {17721, "P1319"}, {17728, "P1320"}, {17729, "P1321"}, {17730, "P1322"},
            {17731, "P1323"}, {17732, "P1324"}, {17733, "P1325"}, {17734, "P1326"}, {17735, "P1327"},
            {17736, "P1328"}, {17737, "P1329"}, {17738, "P1330"}, {17739, "P1331"}, {17740, "P1332"},
            {17743, "P1335"}, {17744, "P1336"}, {17745, "P1337"}, {17746, "P1338"}, {17747, "P1339"},
            {17748, "P1340"}, {17749, "P1341"}, {17750, "P1342"}, {17751, "P1343"}, {17752, "P1344"},
            {17753, "P1345"}, {17754, "P1346"}, {17755, "P1347"}, {17756, "P1348"}, {17757, "P1349"},
            {17758, "P1350"}, {17762, "P1354"}, {17763, "P1355"}, {17764, "P1356"}, {17765, "P1357"},
            {17766, "P1358"}, {17767, "P1359"}, {17768, "P1360"}, {17769, "P1361"}, {17770, "P1362"},
            {17771, "P1363"}, {17772, "P1364"}, {17773, "P1365"}, {17774, "P1366"}, {17775, "P1367"},
            {17776, "P1368"}, {17777, "P1369"}, {17778, "P1370"}, {17779, "P1371"}, {17780, "P1372"},
            {17781, "P1373"}, {17782, "P1374"}, {17783, "P1375"}, {17784, "P1376"}, {17785, "P1377"},
            {17786, "P1378"}, {17794, "P1386"}, {17795, "P1387"}, {17796, "P1388"}, {17799, "P1391"},
            {17800, "P1392"}, {17801, "P1393"}, {17802, "P1394"}, {17803, "P1395"}, {17804, "P1396"},
            {17805, "P1397"}, {17806, "P1398"}, {17807, "P1399"}, {17808, "P1400"}, {17809, "P1401"},
            {17810, "P1402"}, {17811, "P1403"}, {17812, "P1404"}, {17814, "P1406"}, {17815, "P1407"},
            {17816, "P1408"}, {17817, "P1409"}, {17818, "P1410"}, {17819, "P1411"}, {17820, "P1412"},
            {17821, "P1413"}, {17822, "P1414"}, {17825, "P1417"}, {17826, "P1418"}, {17828, "P1420"},
            {17829, "P1421"}, {17830, "P1422"}, {17831, "P1423"}, {17832, "P1424"}, {17833, "P1425"},
            {17834, "P1426"}, {17840, "P1432"}, {17841, "P1433"}, {17842, "P1434"}, {17843, "P1435"},
            {17844, "P1436"}, {17847, "P1439"}, {17848, "P1440"}, {17849, "P1441"}, {17850, "P1442"},
            {17851, "P1443"}, {17852, "P1444"}, {17853, "P1445"}, {17854, "P1446"}, {17855, "P1447"},
            {17856, "P1448"}, {17857, "P1449"}, {17858, "P1450"}, {17859, "P1451"}, {17860, "P1452"},
            {17861, "P1453"}, {17862, "P1454"}, {17863, "P1455"}, {17864, "P1456"}, {17865, "P1457"},
            {17866, "P1458"}, {17867, "P1459"}, {17868, "P1460"}, {17869, "P1461"}, {17870, "P1462"},
            {17873, "P1465"}, {17874, "P1466"}, {17875, "P1467"}, {17876, "P1468"}, {17877, "P1469"},
            {17878, "P1470"}, {17879, "P1471"}, {17880, "P1472"}, {17881, "P1473"}, {17882, "P1474"},
            {17883, "P1475"}, {17884, "P1476"}, {17885, "P1477"}, {17886, "P1478"}, {17908, "P1500"},
            {17909, "P1501"}, {17910, "P1502"}, {17911, "P1503"}, {17912, "P1504"}, {17913, "P1505"},
            {17914, "P1506"}, {17915, "P1507"}, {17916, "P1508"}, {17917, "P1509"}, {17918, "P1510"},
            {17919, "P1511"}, {17920, "P1512"}, {17921, "P1513"}, {17922, "P1514"}, {17923, "P1515"},
            {17924, "P1516"}, {17925, "P1517"}, {17926, "P1518"}, {17927, "P1519"}, {17928, "P1520"},
            {17929, "P1521"}, {17930, "P1522"}, {17931, "P1523"}, {17933, "P1525"}, {17934, "P1526"},
            {17935, "P1527"}, {17936, "P1528"}, {17937, "P1529"}, {17938, "P1530"}, {17939, "P1531"},
            {17941, "P1533"}, {17942, "P1534"}, {17943, "P1535"}, {17944, "P1536"}, {17945, "P1537"},
            {17946, "P1538"}, {17947, "P1539"}, {17948, "P1540"}, {17949, "P1541"}, {17950, "P1542"},
            {17951, "P1543"}, {17952, "P1544"}, {17953, "P1545"}, {17954, "P1546"}, {17955, "P1547"},
            {17956, "P1548"}, {17957, "P1549"}, {17958, "P1550"}, {17959, "P1551"}, {17960, "P1552"},
            {17961, "P1553"}, {17962, "P1554"}, {17963, "P1555"}, {17964, "P1556"}, {17965, "P1557"},
            {17966, "P1558"}, {17967, "P1559"}, {17968, "P1560"}, {17969, "P1561"}, {17970, "P1562"},
            {17971, "P1563"}, {17972, "P1564"}, {17973, "P1565"}, {17974, "P1566"}, {17975, "P1567"},
            {17976, "P1568"}, {17977, "P1569"}, {17978, "P1570"}, {17979, "P1571"}, {17980, "P1572"},
            {17981, "P1573"}, {17982, "P1574"}, {17983, "P1575"}, {17984, "P1576"}, {17985, "P1577"},
            {17986, "P1578"}, {17987, "P1579"}, {17988, "P1580"}, {17989, "P1581"}, {17990, "P1582"},
            {17991, "P1583"}, {17992, "P1584"}, {17993, "P1585"}, {17994, "P1586"}, {17995, "P1587"},
            {17996, "P1588"}, {18008, "P1600"}, {18010, "P1602"}, {18011, "P1603"}, {18012, "P1604"},
            {18013, "P1605"}, {18014, "P1606"}, {18015, "P1607"}, {18016, "P1608"}, {18017, "P1609"},
            {18019, "P1611"}, {18020, "P1612"}, {18021, "P1613"}, {18022, "P1614"}, {18023, "P1615"},
            {18024, "P1616"}, {18025, "P1617"}, {18026, "P1618"}, {18027, "P1619"}, {18028, "P1620"},
            {18029, "P1621"}, {18030, "P1622"}, {18031, "P1623"}, {18032, "P1624"}, {18033, "P1625"},
            {18034, "P1626"}, {18035, "P1627"}, {18036, "P1628"}, {18037, "P1629"}, {18038, "P1630"},
            {18039, "P1631"}, {18040, "P1632"}, {18041, "P1633"}, {18042, "P1634"}, {18043, "P1635"},
            {18044, "P1636"}, {18045, "P1637"}, {18046, "P1638"}, {18047, "P1639"}, {18048, "P1640"},
            {18049, "P1641"}, {18050, "P1642"}, {18051, "P1643"}, {18052, "P1644"}, {18053, "P1645"},
            {18054, "P1646"}, {18055, "P1647"}, {18056, "P1648"}, {18057, "P1649"}, {18058, "P1650"},
            {18059, "P1651"}, {18060, "P1652"}, {18061, "P1653"}, {18062, "P1654"}, {18063, "P1655"},
            {18064, "P1656"}, {18065, "P1657"}, {18066, "P1658"}, {18084, "P1676"}, {18085, "P1677"},
            {18086, "P1678"}, {18087, "P1679"}, {18089, "P1681"}, {18092, "P1684"}, {18094, "P1686"},
            {18098, "P1690"}, {18099, "P1691"}, {18100, "P1692"}, {18101, "P1693"}, {18102, "P1694"},
            {18112, "P1704"}, {18113, "P1705"}, {18119, "P1711"}, {18124, "P1716"}, {18129, "P1721"},
            {18131, "P1723"}, {18132, "P1724"}, {18134, "P1726"}, {18136, "P1728"}, {18137, "P1729"},
            {18141, "P1733"}, {18147, "P1739"}, {18148, "P1740"}, {18149, "P1741"}, {18150, "P1742"},
            {18151, "P1743"}, {18152, "P1744"}, {18153, "P1745"}, {18154, "P1746"}, {18155, "P1747"},
            {18156, "P1748"}, {18157, "P1749"}, {18158, "P1750"}, {18159, "P1751"}, {18160, "P1752"},
            {18168, "P1760"}, {18169, "P1761"}, {18170, "P1762"}, {18171, "P1763"}, {18172, "P1764"},
            {18173, "P1765"}, {18174, "P1766"}, {18175, "P1767"}, {18176, "P1768"}, {18177, "P1769"},
            {18178, "P1770"}, {18179, "P1771"}, {18180, "P1772"}, {18181, "P1773"}, {18182, "P1774"},
            {18183, "P1775"}, {18184, "P1776"}, {18185, "P1777"}, {18186, "P1778"}, {18189, "P1781"},
            {18190, "P1782"}, {18192, "P1784"}, {18193, "P1785"}, {18194, "P1786"}, {18195, "P1787"},
            {18196, "P1788"}, {18197, "P1789"}, {18198, "P1790"}, {18199, "P1791"}, {18200, "P1792"},
            {18201, "P1793"}, {18203, "P1795"}, {18204, "P1796"}, {18205, "P1797"}, {18206, "P1798"},
            {18207, "P1799"}, {18221, "P1813"}, {18222, "P1814"}, {18223, "P1815"}, {18226, "P1818"},
            {18227, "P1819"}, {18228, "P1820"}, {18231, "P1823"}, {18232, "P1824"}, {18233, "P1825"},
            {18236, "P1828"}, {18237, "P1829"}, {18238, "P1830"}, {18242, "P1834"}, {18243, "P1835"},
            {18249, "P1841"}, {18250, "P1842"}, {18251, "P1843"}, {18252, "P1844"}, {18255, "P1847"},
            {18256, "P1848"}, {18257, "P1849"}, {18258, "P1850"}, {18259, "P1851"}, {18260, "P1852"},
            {18261, "P1853"}, {18262, "P1854"}, {18263, "P1855"}, {18264, "P1856"}, {18265, "P1857"},
            {18266, "P1858"}, {18267, "P1859"}, {18268, "P1860"}, {18269, "P1861"}, {18270, "P1862"},
            {18271, "P1863"}, {18272, "P1864"}, {18273, "P1865"}, {18274, "P1866"}
        };

        private bool _disposed;
        private volatile bool _terminating;
        private readonly Context _context;
        private readonly Android.App.Activity _activity;
        private readonly BaseActivity _baseActivity;
        private static Context _packageContext;
        private readonly BcReceiverUpdateDisplayDelegate _bcReceiverUpdateDisplayHandler;
        private readonly BcReceiverReceivedDelegate _bcReceiverReceivedHandler;
        private long? _versionCode;
        private bool? _usbSupport;
        private bool? _mtcBtService;
        private bool? _mtcBtManager;
        private static string _assetEcuFileName;
        private static string _mtcBtModuleName;
        private static readonly object LockObject = new object();
        private static readonly object JobReaderLockObject = new object();
        private static readonly object SettingsLockObject = new object();
        private static readonly object RecentConfigLockObject = new object();
        private static readonly object LastSerialLockObject = new object();
        private static readonly object SerialInfoLockObject = new object();
        private static readonly object CompileLock = new object();
        private static int _instanceCount;
        private static string _externalPath;
        private static string _externalWritePath;
        private static string _customStorageMedia;
        private static string _appId;
        private static bool _vagUdsActive;
        private static bool _ecuFunctionActive;
        private static int _btEnableCounter;
        private static JobReader _jobReader;
        private static string _adapterBlackList;
        private static string _lastAdapterSerial;
        private static TranslatorType _translatorType;
        private static Dictionary<string, UdsReader> _udsReaderDict;
        private static EcuFunctionReader _ecuFunctionReader;
        private static readonly List<string> _recentConfigList;
        private static readonly List<SerialInfoEntry> _serialInfoList;
        private readonly BluetoothAdapter _btAdapter;
        private readonly BluetoothManager _bluetoothManager;
        private readonly Java.Lang.Object _clipboardManager;
        private readonly WifiManager _maWifi;
        private readonly ConnectivityManager _maConnectivity;
        private readonly UsbManager _usbManager;
        private readonly LocationManager _locationManager;
        private readonly Android.App.NotificationManager _notificationManager;
        private readonly NotificationManagerCompat _notificationManagerCompat;
        private readonly PowerManager _powerManager;
        private readonly PackageManager _packageManager;
        private readonly BackupManager _backupManager;
        private readonly Android.App.ActivityManager _activityManager;
        private readonly MtcServiceConnection _mtcServiceConnection;
        private Thread _udsReaderThread;
        private Thread _ecuFuncReaderThread;
        private Thread _copyDocumentThread;
        private Thread _deleteDocumentThread;
        private PowerManager.WakeLock _wakeLockScreenBright;
        private PowerManager.WakeLock _wakeLockScreenDim;
        private PowerManager.WakeLock _wakeLockCpu;
        private readonly Tuple<LockType, PowerManager.WakeLock>[] _lockArray;
        private CellularCallback _cellularCallback;
        private WifiCallback _wifiCallback;
        private EthernetCallback _ethernetCallback;
        private readonly TcpClientWithTimeout.NetworkData _networkData;
        private Handler _btUpdateHandler;
        private readonly Java.Lang.Runnable _btUpdateRunnable;
        private Timer _usbCheckTimer;
        private int _usbDeviceDetectCount;
        private GlobalBroadcastReceiver _gbcReceiver;
        private Receiver _bcReceiver;
        private InterfaceType _selectedInterface;
        private AlertDialog _activateAlertDialog;
        private AlertDialog _selectMediaAlertDialog;
        private AlertDialog _selectInterfaceAlertDialog;
        private AlertDialog _selectManufacturerAlertDialog;
        private AlertDialog _ftdiWarningAlertDialog;
        private AlertDialog _batteryVoltageAlertDialog;
        private CustomProgressDialog _translateProgress;
        private HttpClient _translateHttpClient;
        private HttpClient _sendHttpClient;
        private HttpClient _updateHttpClient;
        private bool _updateCheckActive;
        private bool _translateLockAquired;
        private string _yandexCloudIamToken;
        private string _yandexCloudIamTokenExpires;
        private DateTime _yandexCloudIamTokenTime = DateTime.MinValue;
        private List<string> _transLangList;
        private List<string> _transList;
        private List<string> _transReducedStringList;
        private string _transCurrentLang;
        private readonly Dictionary<string, Dictionary<string, string>> _transDict;
        private Dictionary<string, string> _transCurrentLangDict;
        private Dictionary<string, List<string>> _vagDtcCodeDict;
        private bool? _lastInerfaceAvailable;
        private bool _usbPermissionRequested;
        private bool _usbPermissionRequestDisabled;

        public static string TraceFileName => CompressTrace ? TraceFileNameZip : TraceFileNameStd;

        public static bool IfhTraceBuffering => !CompressTrace;

        public bool Emulator { get; }

        public long VersionCode
        {
            get
            {
                if (_versionCode == null)
                {
                    _versionCode = GetVersionCode();
                }

                return _versionCode.Value;
            }
        }

        public bool TranslateActive => _translateProgress != null;

        public string ManufacturerEcuDirName
        {
            get
            {
                switch (SelectedManufacturer)
                {
                    case ManufacturerType.Bmw:
                        return Path.Combine(EcuBaseDir, EcuDirNameBmw);
                }
                return Path.Combine(EcuBaseDir, EcuDirNameVag);
            }
        }

        public bool UsbSupport
        {
            get
            {
                if (_usbSupport == null)
                {
                    try
                    {
                        _usbSupport = _usbManager?.DeviceList != null && (Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr1);
                    }
                    catch (Exception)
                    {
                        _usbSupport = false;
                    }
                }
                return _usbSupport??false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static bool IsMtcService(Context context)
        {
            try
            {
                PackageManager packageManager = context?.PackageManager;
                IList<ApplicationInfo> appList = GetInstalledApplications(packageManager, PackageInfoFlags.MatchAll);
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

        public bool MtcBtService
        {
            get
            {
                if (_mtcBtService == null)
                {
                    _mtcBtService = IsMtcService(_context);
                }
                return _mtcBtService ?? false;
            }
        }

        public bool MtcBtServiceBound
        {
            get
            {
                if (!MtcBtService)
                {
                    return false;
                }
                return _mtcServiceConnection != null && _mtcServiceConnection.Bound;
            }
        }

        public bool MtcBtConnected
        {
            get
            {
                if (!MtcBtService)
                {
                    return false;
                }
                return MtcBtConnectState;
            }
        }

        public bool MtcBtManager
        {
            get
            {
                if (_mtcBtManager == null)
                {
                    try
                    {
                        _mtcBtManager = _packageManager?.GetLaunchIntentForPackage(MtcBtAppName) != null;
                    }
                    catch (Exception)
                    {
                        _mtcBtManager = false;
                    }
                }
                return _mtcBtManager ?? false;
            }
        }

        public string MtcBtModuleName
        {
            get
            {
                if (!string.IsNullOrEmpty(_mtcBtModuleName))
                {
                    return _mtcBtModuleName;
                }

                if (_mtcServiceConnection == null)
                {
                    return null;
                }

                _mtcBtModuleName = _mtcServiceConnection.CarManagerGetBtModuleName();

                return _mtcBtModuleName;
            }
        }

        public bool MtcBtEscapeMode
        {
            get
            {
                string moduleName = MtcBtModuleName;
                if (string.IsNullOrEmpty(moduleName))
                {
                    return false;
                }

                if (string.Compare(moduleName, "SD-GT936", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                return false;
            }
        }

        public MtcServiceConnection MtcServiceConnection => _mtcServiceConnection;

        public bool MtcServiceStarted { get; private set; }

        public long MtcServiceStartTime { get; private set; }

        public static object GlobalSettingLockObject => SettingsLockObject;

        public static bool StaticDataInitialized { get; set; }

        public static string ExternalPath => _externalPath;

        public static string ExternalWritePath => _externalWritePath;

        public static string CustomStorageMedia
        {
            get => _customStorageMedia;
            set => _customStorageMedia = IsWritable(value) ? value : null;
        }

        public static string CopyToAppSrc { get; set; }

        public static string CopyToAppDst { get; set; }

        public static string CopyFromAppSrc { get; set; }

        public static string CopyFromAppDst { get; set; }

        public static string UsbFirmwareFileName { get; set; }

        public static string AssetEcuFileName { get; set; }

        public static long AssetEcuFileSize { get; set; }

        public static string AssetEcuId
        {
            get
            {
                StringBuilder sbName = new StringBuilder();
                string fileName = Path.GetFileName(AssetEcuFileName);

                if (!string.IsNullOrEmpty(fileName))
                {
                    sbName.Append(fileName);
                }

                if (AssetEcuFileSize >= 0)
                {
                    if (sbName.Length > 0)
                    {
                        sbName.Append("_");
                    }

                    sbName.Append(string.Format(CultureInfo.InvariantCulture, "{0}", AssetEcuFileSize));
                }

                return sbName.ToString();
            }
        }

        public static string AppId
        {
            get
            {
                if (string.IsNullOrEmpty(_appId))
                {
                    _appId = Guid.NewGuid().ToString();
                }
                return _appId;
            }
            set => _appId = value;
        }

        public static bool VagUdsChecked { get; private set; }

        public static bool VagUdsActive
        {
            get
            {
                if (OldVagMode)
                {
                    return false;
                }
                return _vagUdsActive;
            }
            private set => _vagUdsActive = value;
        }

        public static bool EcuFunctionsChecked { get; private set; }

        public static bool EcuFunctionsActive
        {
            get
            {
                if (!UseBmwDatabase)
                {
                    return false;
                }
                return _ecuFunctionActive;
            }
            private set => _ecuFunctionActive = value;
        }

        public static EcuFunctionReader EcuFunctionReader => _ecuFunctionReader;

        public static bool MtcBtConnectState { get; set; }

        public static bool BtInitiallyEnabled { get; set; }

        public static string RecentLocale { get; set; }

        public static ThemeType? SelectedTheme { get; set; }

        public static bool AutoHideTitleBar { get; set; }

        public static bool SuppressTitleBar { get; set; }

        public static bool FullScreenMode { get; set; }

        public static bool SwapMultiWindowOrientation { get; set; }

        public static InternetConnectionType SelectedInternetConnection { get; set; }

        public static ManufacturerType SelectedManufacturer { get; set; }

        public static BtEnableType BtEnbaleHandling { get; set; }

        public static BtDisableType BtDisableHandling { get; set; }

        public static LockType LockTypeCommunication { get; set; }

        public static LockType LockTypeLogging { get; set; }

        public static bool StoreDataLogSettings { get; set; }

        public static AutoConnectType AutoConnectHandling { get; set; }

        public static long UpdateCheckDelay { get; set; }

        public static bool DoubleClickForAppExit { get; set; }

        public static bool SendDataBroadcast { get; set; }

        public static bool CheckCpuUsage { get; set; }

        public static bool CheckEcuFiles { get; set; }

        public static bool OldVagMode { get; set; }

        public static bool UseBmwDatabase { get; set; }

        public static bool ShowOnlyRelevantErrors { get; set; }

        public static bool ScanAllEcus { get; set; }

        public static bool CollectDebugInfo { get; set; }

        public static bool CompressTrace { get; set; }

        public static bool DisableNetworkCheck { get; set; }

        public static bool DisableFileNameEncoding { get; set; }

        public static TranslatorType SelectedTranslator => _translatorType;

        public TranslatorType Translator
        {
            get => _translatorType;

            set
            {
                if (_translatorType != value)
                {
                    _transLangList = null;
                }

                _translatorType = value;
            }
        }

        public static string YandexApiKey { get; set; }

        public static string IbmTranslatorApiKey { get; set; }

        public static string IbmTranslatorUrl { get; set; }

        public static string DeeplApiKey { get; set; }

        public static string YandexCloudApiKey { get; set; }

        public static string YandexCloudFolderId { get; set; }

        public static string GoogleApisUrl { get; set; }

        public static bool EnableTranslation { get; set; }

        public static bool EnableTranslateRequested { get; set; }

        public static bool ShowBatteryVoltageWarning { get; set; }

        public static long BatteryWarnings { get; set; }

        public static double BatteryWarningVoltage { get; set; }

        public static string AdapterBlacklist
        {
            get
            {
                return _adapterBlackList;
            }
            set
            {
                _adapterBlackList = value;

                List<byte[]> blackList = new List<byte[]>();
                if (!string.IsNullOrEmpty(_adapterBlackList))
                {
                    string[] serialArray = _adapterBlackList.Split(";");
                    foreach (string serial in serialArray)
                    {
                        byte[] data = EdiabasNet.HexToByteArray(serial);
                        if (data.Length > 0)
                        {
                            blackList.Add(data);
                        }
                    }
                }

                EdCustomAdapterCommon.AdapterBlackList = blackList;
            }
        }

        public static JobReader JobReader
        {
            get
            {
                lock (JobReaderLockObject)
                {
                    return _jobReader;
                }
            }
            set
            {
                lock (JobReaderLockObject)
                {
                    _jobReader = value;
                }
            }
        }

        public static string LastAdapterSerial
        {
            get
            {
                lock (LastSerialLockObject)
                {
                    return _lastAdapterSerial;
                }
            }
            set
            {
                lock (LastSerialLockObject)
                {
                    _lastAdapterSerial = value;
                }
            }
        }



        public static string EmailAddress { get; set; }

        public static string TraceInfo { get; set; }

        public static EdiabasThread EdiabasThread { get; set; }

        public InterfaceType SelectedInterface
        {
            get => _selectedInterface;
            set
            {
                if (_selectedInterface != value)
                {
                    if (_baseActivity != null)
                    {
                        _baseActivity.InstanceDataCommon.LastEnetSsid = CommActive ? EnetSsidEmpty : string.Empty;
                    }

                    _lastInerfaceAvailable = null;
                }
                _selectedInterface = value;
                SetPreferredNetworkInterface();
            }
        }

        public string SelectedInterfaceIp
        {
            get
            {
                switch (SelectedInterface)
                {
                    case InterfaceType.Enet:
                        return SelectedEnetIp;

                    case InterfaceType.ElmWifi:
                        return SelectedElmWifiIp;

                    case InterfaceType.DeepObdWifi:
                        return SelectedDeepObdWifiIp;
                }
                return null;
            }

            set
            {
                switch (SelectedInterface)
                {
                    case InterfaceType.Enet:
                        SelectedEnetIp = value;
                        break;

                    case InterfaceType.ElmWifi:
                        SelectedElmWifiIp = value;
                        break;

                    case InterfaceType.DeepObdWifi:
                        SelectedDeepObdWifiIp = value;
                        break;
                }
            }
        }

        public string SelectedEnetIp { get; set; }

        public string SelectedElmWifiIp { get; set; }

        public string SelectedDeepObdWifiIp { get; set; }

        public bool MtcBtDisconnectWarnShown { get; set; }

        public bool AdapterCheckRequired => _selectedInterface == InterfaceType.ElmWifi || _selectedInterface == InterfaceType.DeepObdWifi;

        public Context Context => _context;

        public Android.App.Activity Activity => _activity;

        public Java.Lang.Object ClipboardManager => _clipboardManager;

        public BluetoothAdapter BtAdapter => _btAdapter;

        public WifiManager MaWifi => _maWifi;

        public ConnectivityManager MaConnectivity => _maConnectivity;

        public TcpClientWithTimeout.NetworkData NetworkData => _networkData;

        public UsbManager UsbManager => _usbManager;

        public LocationManager LocationManager => _locationManager;

        public Android.App.NotificationManager NotificationManager => _notificationManager;

        public NotificationManagerCompat NotificationManagerCompat => _notificationManagerCompat;

        public PowerManager PowerManager => _powerManager;

        public PackageManager PackageManager => _packageManager;

        public BackupManager BackupManager => _backupManager;

        // ReSharper disable once ConvertToAutoProperty
        public Android.App.ActivityManager ActivityManager => _activityManager;

        public GlobalBroadcastReceiver GbcReceiver => _gbcReceiver;

        public Receiver BcReceiver => _bcReceiver;

        public XDocument XmlDocDtcCodes { get; set; }

        static ActivityCommon()
        {
            JobReader = new JobReader(false);
            _recentConfigList = new List<string>();
            _serialInfoList = new List<SerialInfoEntry>();
            AssetEcuFileName = string.Empty;
            AssetEcuFileSize = -1;
            EdiabasNet.EncodeFileNameKey = string.Empty;
        }

        public ActivityCommon(Context context, BcReceiverUpdateDisplayDelegate bcReceiverUpdateDisplayHandler = null,
            BcReceiverReceivedDelegate bcReceiverReceivedHandler = null)
        {
            lock (LockObject)
            {
                _instanceCount++;
            }
            _context = context;
            _activity = context as Android.App.Activity;
            _baseActivity = context as BaseActivity;
            _bcReceiverUpdateDisplayHandler = bcReceiverUpdateDisplayHandler;
            _bcReceiverReceivedHandler = bcReceiverReceivedHandler;
            Emulator = IsEmulator();
            ResetYandexIamToken();
            _clipboardManager = context?.GetSystemService(Context.ClipboardService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
            {
                _bluetoothManager = context?.GetSystemService(Context.BluetoothService) as BluetoothManager;
                _btAdapter = _bluetoothManager?.Adapter;
            }
            else
            {
#pragma warning disable 618
#pragma warning disable CA1422
                _btAdapter = BluetoothAdapter.DefaultAdapter;
#pragma warning restore CA1422
#pragma warning restore 618
            }
            _btUpdateHandler = new Handler(Looper.MainLooper);
            _btUpdateRunnable = new Java.Lang.Runnable(() =>
            {
                if (_disposed)
                {
                    return;
                }
                if (_btUpdateHandler == null)
                {
                    return;
                }
                _bcReceiverUpdateDisplayHandler?.Invoke();
            });

            _maWifi = (WifiManager)context?.ApplicationContext?.GetSystemService(Context.WifiService);
            _maConnectivity = (ConnectivityManager)context?.ApplicationContext?.GetSystemService(Context.ConnectivityService);
            _networkData = new TcpClientWithTimeout.NetworkData(_maConnectivity);
            _usbManager = context?.GetSystemService(Context.UsbService) as UsbManager;
            _locationManager = context?.GetSystemService(Context.LocationService) as LocationManager;
            _notificationManager = context?.GetSystemService(Context.NotificationService) as Android.App.NotificationManager;
            _notificationManagerCompat = NotificationManagerCompat.From(context);
            RegisterNotificationChannels();
            _powerManager = context?.GetSystemService(Context.PowerService) as PowerManager;
            if (_powerManager != null)
            {
                _wakeLockScreenBright = _powerManager.NewWakeLock(WakeLockFlags.ScreenBright | WakeLockFlags.OnAfterRelease, "ScreenBrightLock");
                _wakeLockScreenBright.SetReferenceCounted(false);

                _wakeLockScreenDim = _powerManager.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.OnAfterRelease, "ScreenDimLock");
                _wakeLockScreenDim.SetReferenceCounted(false);

                _wakeLockCpu = _powerManager.NewWakeLock(WakeLockFlags.Partial, "PartialLock");
                _wakeLockCpu.SetReferenceCounted(false);
                _lockArray = new []
                    {
                        new Tuple<LockType, PowerManager.WakeLock>(LockType.Cpu, _wakeLockCpu),
                        new Tuple<LockType, PowerManager.WakeLock>(LockType.ScreenDim, _wakeLockScreenDim),
                        new Tuple<LockType, PowerManager.WakeLock>(LockType.ScreenBright, _wakeLockScreenBright)
                    };
            }
            _packageManager = context?.PackageManager;
            _backupManager = context != null ? new BackupManager(context) : null;
            _activityManager = context?.GetSystemService(Context.ActivityService) as Android.App.ActivityManager;
            _selectedInterface = InterfaceType.None;
            _transDict = new Dictionary<string, Dictionary<string, string>>();

            if (context != null)
            {
                RegisterInternetCellularCallback();
                RegisterWifiEnetNetworkCallback();

                if ((_bcReceiverUpdateDisplayHandler != null) || (_bcReceiverReceivedHandler != null))
                {
                    // android 8 rejects global receivers, so we register it locally
                    _gbcReceiver = new GlobalBroadcastReceiver();
                    ContextCompat.RegisterReceiver(context, _gbcReceiver, new IntentFilter(GlobalBroadcastReceiver.MtcBtSmallon), ContextCompat.ReceiverExported);
                    ContextCompat.RegisterReceiver(context, _gbcReceiver, new IntentFilter(GlobalBroadcastReceiver.MtcBtSmallon), ContextCompat.ReceiverExported);
                    ContextCompat.RegisterReceiver(context, _gbcReceiver, new IntentFilter(GlobalBroadcastReceiver.MtcBtSmalloff), ContextCompat.ReceiverExported);
                    ContextCompat.RegisterReceiver(context, _gbcReceiver, new IntentFilter(GlobalBroadcastReceiver.MicBtReport), ContextCompat.ReceiverExported);

                    _bcReceiver = new Receiver(this);
                    ContextCompat.RegisterReceiver(context, _bcReceiver, new IntentFilter(ForegroundService.ActionBroadcastCommand), ContextCompat.ReceiverExported);
                    ContextCompat.RegisterReceiver(context, _bcReceiver, new IntentFilter(GlobalBroadcastReceiver.NotificationBroadcastAction), ContextCompat.ReceiverExported);
                    ContextCompat.RegisterReceiver(context, _bcReceiver, new IntentFilter(WifiApStateChangedAction), ContextCompat.ReceiverExported);   // hidden system broadcasts
                    context.RegisterReceiver(_bcReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));   // system broadcasts

                    InternalBroadcastManager.InternalBroadcastManager.GetInstance(context).RegisterReceiver(_bcReceiver, new IntentFilter(ForegroundService.NotificationBroadcastAction));
                    InternalBroadcastManager.InternalBroadcastManager.GetInstance(context).RegisterReceiver(_bcReceiver, new IntentFilter(PackageNameAction));
                    if (UsbSupport)
                    {   // usb handling
                        context.RegisterReceiver(_bcReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));   // system broadcasts
                        context.RegisterReceiver(_bcReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));   // system broadcasts
                        ContextCompat.RegisterReceiver(context, _bcReceiver, new IntentFilter(UsbPermissionAction),ContextCompat.ReceiverExported);
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                        {   // attached event fails
                            _usbCheckTimer = new Timer(UsbCheckEvent, null, 1000, 1000);
                        }
                    }
                }
            }

            if (context != null && MtcBtService)
            {
                _mtcServiceConnection = new MtcServiceConnection(_context, connected =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    try
                    {
                        if (connected)
                        {
                            // ReSharper disable once UnusedVariable
                            sbyte state = _mtcServiceConnection.GetBtState();
                            MtcBtConnectState = state != 0;
                            if (!_mtcServiceConnection.GetAutoConnect())
                            {
                                _mtcServiceConnection.SetAutoConnect(true);
                            }
                        }

                        if (_bcReceiverUpdateDisplayHandler != null)
                        {
                            _activity?.RunOnUiThread(() =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                _bcReceiverUpdateDisplayHandler?.Invoke();
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    _terminating = true;
                    StopMtcService();

                    if (IsUdsReaderJobRunning())
                    {
                        _udsReaderThread?.Join();
                    }

                    if (IsEcuFuncReaderJobRunning())
                    {
                        _ecuFuncReaderThread?.Join();
                    }

                    if (IsCopyDocumentJobRunning())
                    {
                        _copyDocumentThread?.Join();
                    }

                    if (IsDeleteDocumentJobRunning())
                    {
                        _deleteDocumentThread?.Join();
                    }

                    if (_btUpdateHandler != null)
                    {
                        try
                        {
                            _btUpdateHandler.RemoveCallbacksAndMessages(null);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _btUpdateHandler = null;
                    }
                    if (_usbCheckTimer != null)
                    {
                        _usbCheckTimer.Dispose();
                        _usbCheckTimer = null;
                    }

                    if (_translateHttpClient != null)
                    {
                        try
                        {
                            _translateHttpClient.CancelPendingRequests();
                            _translateHttpClient.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _translateHttpClient = null;
                    }

                    if (_sendHttpClient != null)
                    {
                        try
                        {
                            _sendHttpClient.CancelPendingRequests();
                            _sendHttpClient.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _sendHttpClient = null;
                    }

                    if (_updateHttpClient != null)
                    {
                        try
                        {
                            _updateHttpClient.CancelPendingRequests();
                            _updateHttpClient.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _updateHttpClient = null;
                    }

                    UnRegisterInternetCellularCallback();
                    UnRegisterWifiEnetCallback();
                    if (_context != null)
                    {
                        if (_gbcReceiver != null)
                        {
                            InternalBroadcastManager.InternalBroadcastManager.GetInstance(_context).UnregisterReceiver(_gbcReceiver);
                            _context.UnregisterReceiver(_gbcReceiver);
                            _gbcReceiver = null;
                        }
                        if (_bcReceiver != null)
                        {
                            InternalBroadcastManager.InternalBroadcastManager.GetInstance(_context).UnregisterReceiver(_bcReceiver);
                            _context.UnregisterReceiver(_bcReceiver);
                            _bcReceiver = null;
                        }
                    }
                    if (_wakeLockScreenBright != null)
                    {
                        try
                        {
                            _wakeLockScreenBright.Release();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _wakeLockScreenBright = null;
                    }
                    if (_wakeLockScreenDim != null)
                    {
                        try
                        {
                            _wakeLockScreenDim.Release();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _wakeLockScreenDim = null;
                    }
                    if (_wakeLockCpu != null)
                    {
                        try
                        {
                            _wakeLockCpu.Release();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _wakeLockCpu = null;
                    }
                    lock (LockObject)
                    {
                        _instanceCount--;
                        if (_instanceCount == 0)
                        {
                            StopEdiabasThread(true);
                            BluetoothDisableAtExit();
                            MemoryStreamReader.CleanUp();
                        }
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public static bool CommActive => EdiabasThread != null && EdiabasThread.ThreadRunning();

        public static bool ErrorResetActive
        {
            get
            {
                bool active = false;
                if (CommActive)
                {
                    lock (EdiabasThread.DataLock)
                    {
                        if (EdiabasThread.ErrorResetList != null ||
                            !string.IsNullOrEmpty(EdiabasThread.ErrorResetSgbdFunc) ||
                            EdiabasThread.ErrorResetActive)
                        {
                            active = true;
                        }
                    }
                }

                return active;
            }
        }

        public string InterfaceName()
        {
            if (_context == null)
            {
                return string.Empty;
            }

            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    return _context.GetString(Resource.String.select_interface_bt);

                case InterfaceType.Enet:
                    return _context.GetString(Resource.String.select_interface_enet);

                case InterfaceType.ElmWifi:
                    return _context.GetString(Resource.String.select_interface_elmwifi);

                case InterfaceType.DeepObdWifi:
                    return _context.GetString(Resource.String.select_interface_deepobdwifi);

                case InterfaceType.Ftdi:
                    return _context.GetString(Resource.String.select_interface_ftdi);

                case InterfaceType.Simulation:
                    return _context.GetString(Resource.String.select_interface_simulation);
            }
            return string.Empty;
        }

        public string ManufacturerName()
        {
            if (_context == null)
            {
                return string.Empty;
            }

            switch (SelectedManufacturer)
            {
                case ManufacturerType.Bmw:
                    return _context.GetString(Resource.String.select_manufacturer_bmw);

                case ManufacturerType.Audi:
                    return _context.GetString(Resource.String.select_manufacturer_audi);

                case ManufacturerType.Seat:
                    return _context.GetString(Resource.String.select_manufacturer_seat);

                case ManufacturerType.Skoda:
                    return _context.GetString(Resource.String.select_manufacturer_skoda);

                case ManufacturerType.Vw:
                    return _context.GetString(Resource.String.select_manufacturer_vw);

            }
            return string.Empty;
        }

        public string TranslatorName()
        {
            if (_context == null)
            {
                return string.Empty;
            }

            switch (SelectedTranslator)
            {
                case TranslatorType.YandexTranslate:
                    return _context.GetString(Resource.String.select_translator_yantex);

                case TranslatorType.IbmWatson:
                    return _context.GetString(Resource.String.select_translator_ibm);

                case TranslatorType.Deepl:
                    return _context.GetString(Resource.String.select_translator_deepl);

                case TranslatorType.YandexCloud:
                    return _context.GetString(Resource.String.select_translator_yandex_cloud);

                case TranslatorType.GoogleApis:
                    return _context.GetString(Resource.String.select_translator_google_apis);
            }
            return string.Empty;
        }

        public bool GetAdapterIpName(out string longName, out string shortName)
        {
            longName = string.Empty;
            shortName = string.Empty;

            if (_context == null)
            {
                return false;
            }

            bool menuVisible = false;
            string menuName = string.Empty;
            string interfaceIp = SelectedInterfaceIp;

            switch (SelectedInterface)
            {
                case InterfaceType.Enet:
                    menuVisible = true;
                    menuName = _context.GetString(Resource.String.menu_enet_ip);
                    if (string.IsNullOrEmpty(interfaceIp))
                    {
                        interfaceIp = _context.GetString(Resource.String.select_enet_ip_auto);
                    }
                    break;

                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    menuVisible = true;
                    menuName = _context.GetString(Resource.String.menu_adapter_ip);
                    if (string.IsNullOrEmpty(interfaceIp) && !IsWifiApMode())
                    {
                        interfaceIp = _context.GetString(Resource.String.select_enet_ip_auto);
                    }
                    break;
            }

            if (!menuVisible)
            {
                return false;
            }

            if (string.IsNullOrEmpty(interfaceIp))
            {
                interfaceIp = _context.GetString(Resource.String.select_enet_ip_none);
            }

            longName = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", menuName, interfaceIp);
            shortName = interfaceIp;
            return true;
        }

        public bool IsNetworkAdapter()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    return true;
            }

            return false;
        }

        public bool IsInterfaceEnabled()
        {
            try
            {
                switch (_selectedInterface)
                {
                    case InterfaceType.Bluetooth:
                        if (_btAdapter == null)
                        {
                            return false;
                        }
                        return _btAdapter.IsEnabled;

                    case InterfaceType.Enet:
                    case InterfaceType.ElmWifi:
                    case InterfaceType.DeepObdWifi:
                        if (DisableNetworkCheck)
                        {
                            return true;
                        }
                        if (IsEmulator())
                        {
                            return true;
                        }
                        if (_maWifi == null)
                        {
                            return false;
                        }
                        return _maWifi.IsWifiEnabled;

                    case InterfaceType.Ftdi:
                        return true;

                    case InterfaceType.Simulation:
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsInterfaceAvailable(string simulationDir = null, bool ignoreIp = false)
        {
            try
            {
                switch (_selectedInterface)
                {
                    case InterfaceType.Bluetooth:
                        if (_btAdapter == null)
                        {
                            return false;
                        }
                        return _btAdapter.IsEnabled;

                    case InterfaceType.Enet:
                    case InterfaceType.ElmWifi:
                    case InterfaceType.DeepObdWifi:
                        if (DisableNetworkCheck)
                        {
                            return true;
                        }

                        if (IsEmulator())
                        {
                            return true;
                        }

                        if (IsValidWifiConnection(out _, out _, out _, out _))
                        {
                            return true;
                        }

                        switch (_selectedInterface)
                        {
                            case InterfaceType.Enet:
                                return IsValidEthernetConnection();

                            case InterfaceType.DeepObdWifi:
                                if (ignoreIp)
                                {
                                    return IsValidEthernetConnection();
                                }

                                if (!string.IsNullOrEmpty(SelectedDeepObdWifiIp))
                                {
                                    return IsValidEthernetConnection(SelectedDeepObdWifiIp);
                                }
                                break;
                        }

                        return false;

                    case InterfaceType.Ftdi:
                    {
                        List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                        if (availableDrivers.Count <= 0)
                        {
                            return false;
                        }
                        if (!_usbManager.HasPermission(availableDrivers[0].Device))
                        {
                            return false;
                        }
                        return true;
                    }

                    case InterfaceType.Simulation:
                        return IsValidSimDir(simulationDir);
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool StartMtcService()
        {
            if (_mtcServiceConnection == null)
            {
                return false;
            }
            if (_mtcServiceConnection.Bound)
            {
                return true;
            }
            try
            {
                Intent startServiceIntent = new Intent();
                Bind bindFlags = Bind.AutoCreate;
                if (Build.VERSION.SdkInt > BuildVersionCodes.Tiramisu)
                {
                    bindFlags |= Bind.AllowActivityStarts;
                }

                startServiceIntent.SetComponent(new ComponentName(MtcServiceConnection.ServicePkg, MtcServiceConnection.ServiceClsV1));
                if (!_context.BindService(startServiceIntent, _mtcServiceConnection, bindFlags))
                {
                    startServiceIntent.SetComponent(new ComponentName(MtcServiceConnection.ServicePkg, MtcServiceConnection.ServiceClsV2));
                    if (!_context.BindService(startServiceIntent, _mtcServiceConnection, bindFlags))
                    {
                        return false;
                    }
                }

                MtcServiceStartTime = Stopwatch.GetTimestamp();
                MtcServiceStarted = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool StopMtcService()
        {
            if (_mtcServiceConnection == null)
            {
                return false;
            }
            if (!_mtcServiceConnection.Connected)
            {
                return true;
            }
            try
            {
                _context.UnbindService(_mtcServiceConnection);
                _mtcServiceConnection.Bound = false;
                MtcServiceStarted = false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool ConnectMtcBtDevice(string deviceAddress)
        {
#if false
            if (SelectedInterface == InterfaceType.Bluetooth)
            {
                try
                {
                    if (MtcBtServiceBound)
                    {
                        string mac = deviceAddress.Replace(":", string.Empty);
                        MtcServiceConnection.ConnectObd(mac);
                        return true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
#endif
            return false;
        }

        public static bool ReadHciSnoopLogSettings(out bool enabled, out string logFileName)
        {
            enabled = false;
            logFileName = null;
            try
            {
                string[] confFileList =
                {
                    @"/etc/bluetooth/bt_stack.conf",
                    @"/vendor/etc/bluetooth/bt_stack.conf",
                    @"/system/etc/bluetooth/bt_stack.conf",
                };

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }

                string confFileName = null;
                foreach (string file in confFileList)
                {
                    if (File.Exists(file))
                    {
                        confFileName = file;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(confFileName))
                {
                    return false;
                }
                bool? enabledLocal = null;
                // ReSharper disable once AssignNullToNotNullAttribute
                using (StreamReader file = new StreamReader(confFileName))
                {
                    string line;
                    Regex regexLogOutput = new Regex("^BtSnoopLogOutput\\s*=\\s*(true|false)\\s*$", RegexOptions.IgnoreCase);
                    Regex regexFileName = new Regex("^BtSnoopFileName\\s*=\\s*(.*)$", RegexOptions.IgnoreCase);
                    while ((line = file.ReadLine()) != null)
                    {
                        MatchCollection matchesLogOutput = regexLogOutput.Matches(line);
                        if ((matchesLogOutput.Count == 1) && (matchesLogOutput[0].Groups.Count == 2))
                        {
                            enabledLocal = string.Compare(matchesLogOutput[0].Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase) == 0;
                        }
                        MatchCollection matchesFile = regexFileName.Matches(line);
                        if ((matchesFile.Count == 1) && (matchesFile[0].Groups.Count == 2))
                        {
                            logFileName = matchesFile[0].Groups[1].Value;
                        }
                    }
                }
                if (!enabledLocal.HasValue || string.IsNullOrEmpty(logFileName))
                {
                    return false;
                }
                enabled = enabledLocal.Value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetConfigHciSnoopLog(out bool enable)
        {
            enable = false;
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }
                int value = Android.Provider.Settings.Secure.GetInt(_context.ContentResolver, SettingBluetoothHciLog, 0);
                enable = value != 0;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SetConfigHciSnoopLog(bool enable)
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }
                // ReSharper disable once UseNullPropagation
                if (_btAdapter == null)
                {
                    return false;
                }
                Java.Lang.Reflect.Method configHciSnoopLog = _btAdapter.Class.GetMethod("configHciSnoopLog", Java.Lang.Boolean.Type);
                // ReSharper disable once UseNullPropagation
                if (configHciSnoopLog == null)
                {
                    return false;
                }
                if (enable)
                {
                    try
                    {
                        Android.Provider.Settings.Secure.PutInt(_context.ContentResolver, SettingBluetoothHciLog, 1);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                // ReSharper disable once UsePatternMatching
                Java.Lang.Boolean result = configHciSnoopLog.Invoke(_btAdapter, Java.Lang.Boolean.ValueOf(enable)) as Java.Lang.Boolean;
                if (!enable)
                {
                    try
                    {
                        Android.Provider.Settings.Secure.PutInt(_context.ContentResolver, SettingBluetoothHciLog, 0);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                if (result == null || result == Java.Lang.Boolean.False)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool IsAppInstalled(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return false;
            }

            try
            {
                Intent intent = _packageManager?.GetLaunchIntentForPackage(packageName);
                if (intent == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public bool StartApp(string packageName, bool marketRedirect = false)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return false;
            }

            try
            {
                Intent intent = _packageManager?.GetLaunchIntentForPackage(packageName);
                if (intent == null)
                {
                    if (!marketRedirect)
                    {
                        return false;
                    }

                    OpenPlayStoreForPackage(packageName);
                    return true;
                }

                intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.NewTask);
                _context.StartActivity(intent);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public bool OpenPlayStoreForPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return false;
            }

            try
            {
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"market://details?id=" + packageName));
                intent.SetPackage("com.android.vending");
                intent.SetFlags(ActivityFlags.NewTask);
                _context.StartActivity(intent);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return OpenWebUrl("https://play.google.com/store/apps/details?id=" + packageName);
        }

        public bool RestartAppHard(DestroyDelegate destroyDelegate)
        {
            try
            {
                if (_context == null)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                {
                    Intent intent = _packageManager?.GetLaunchIntentForPackage(_context.PackageName);
                    if (intent == null)
                    {
                        return false;
                    }

                    ComponentName componentName = intent.Component;
                    Intent mainIntent = Intent.MakeRestartActivityTask(componentName);
                    _context.StartActivity(mainIntent);
                }

                destroyDelegate?.Invoke();
                Java.Lang.Runtime.GetRuntime()?.Exit(0);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public bool RestartAppSoft()
        {
            try
            {
                if (_activity == null)
                {
                    return false;
                }

                Intent intent = _packageManager?.GetLaunchIntentForPackage(_activity.PackageName);
                if (intent == null)
                {
                    return false;
                }

                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                _activity.Finish();
                _activity.StartActivity(intent);

                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

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

        public static Android.Graphics.Color GetStyleColor(Context context, int attribute)
        {
            TypedArray typedArray = context.Theme.ObtainStyledAttributes([attribute]);
            return typedArray.GetColor(0, 0xFFFFFF);
        }

        public static Balloon.Builder GetBalloonBuilder(Context context)
        {
            Balloon.Builder balloonBuilder = new Balloon.Builder(context);
            balloonBuilder.SetDismissWhenClicked(true);
            balloonBuilder.SetDismissWhenTouchOutside(true);
            balloonBuilder.SetBackgroundColor(GetStyleColor(context, Resource.Attribute.balloonBackgroundColor));
            balloonBuilder.SetTextColor(GetStyleColor(context, Resource.Attribute.balloonTextColor));
            balloonBuilder.SetTextSize(14.0f);
            balloonBuilder.SetTextTypeface((int)Android.Graphics.TypefaceStyle.Italic);
            balloonBuilder.SetBalloonAnimation(BalloonAnimation.Elastic);
            balloonBuilder.SetAutoDismissDuration(BalloonDismissDuration);
            balloonBuilder.SetPadding(10);
            balloonBuilder.SetArrowOrientation(ArrowOrientation.Bottom);
            balloonBuilder.SetArrowPosition(0.5f);
            balloonBuilder.SetArrowOrientationRules(ArrowOrientationRules.AlignFixed);
            balloonBuilder.SetLifecycleOwner(ProcessLifecycleOwner.Get());

            return balloonBuilder;
        }

        public static bool ShowAlertDialogBallon(Context context, AlertDialog alertDialog, int resId, int dismissDuration = BalloonDismissDuration)
        {
            View decorView = alertDialog.Window?.DecorView;
            if (decorView != null)
            {
                Balloon.Builder balloonBuilder = GetBalloonBuilder(context);
                balloonBuilder.SetText(context.GetString(resId));
                balloonBuilder.SetAutoDismissDuration(dismissDuration);
                Balloon balloon = balloonBuilder.Build();
                balloon.ShowAlignTop(decorView);
                return true;
            }

            return false;
        }

        public static bool ShowAlertDialogBallon(Context context, AlertDialog alertDialog, string text, int dismissDuration = BalloonDismissDuration)
        {
            View decorView = alertDialog.Window?.DecorView;
            if (decorView != null)
            {
                Balloon.Builder balloonBuilder = GetBalloonBuilder(context);
                balloonBuilder.SetText(text);
                balloonBuilder.SetAutoDismissDuration(dismissDuration);
                Balloon balloon = balloonBuilder.Build();
                balloon.ShowAlignTop(decorView);
                return true;
            }

            return false;
        }

        public static bool IsBtReliable()
        {
            if (Build.VERSION.SdkInt == BuildVersionCodes.M &&
                string.Compare(Build.Model, "px5", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsDocumentTreeSupported()
        {
            // basically support starts with Android N, but makes no sense
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                return true;
            }
            return false;
        }

        public static bool IsCpuStatisticsSupported()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return false;
            }
            return true;
        }

        public static bool IsExtrenalStorageAccessRequired()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.Q)
            {
                return true;
            }
            return false;
        }

        public static bool IsNotificationsAccessRequired()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                return true;
            }
            return false;
        }

        public static List<string> GetRecentConfigList()
        {
            lock (RecentConfigLockObject)
            {
                return new List<string>(_recentConfigList);
            }
        }

        public static void SetRecentConfigList(List<string> configList)
        {
            lock (RecentConfigLockObject)
            {
                _recentConfigList.Clear();
                if (configList != null)
                {
                    foreach (string fileName in configList)
                    {
                        if (!_recentConfigList.Contains(fileName))
                        {
                            _recentConfigList.Add(fileName);
                        }
                    }
                }
            }
        }

        public static void RecentConfigListClear()
        {
            lock (RecentConfigLockObject)
            {
                _recentConfigList.Clear();
            }
        }

        public static bool RecentConfigListAdd(string fileName)
        {
            try
            {
                lock (RecentConfigLockObject)
                {
                    if (_recentConfigList.Contains(fileName))
                    {
                        _recentConfigList.Remove(fileName);
                    }

                    _recentConfigList.Insert(0, fileName);
                    while (_recentConfigList.Count > 10)
                    {
                        _recentConfigList.RemoveAt(_recentConfigList.Count - 1);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool RecentConfigListRemove(string fileName)
        {
            try
            {
                lock (RecentConfigLockObject)
                {
                    if (_recentConfigList.Contains(fileName))
                    {
                        _recentConfigList.Remove(fileName);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public static bool RecentConfigListCleanup()
        {
            try
            {
                lock (RecentConfigLockObject)
                {
                    int index = 0;
                    while (index < _recentConfigList.Count)
                    {
                        if (!File.Exists(_recentConfigList[index]))
                        {
                            _recentConfigList.RemoveAt(index);
                        }
                        else
                        {
                            index++;
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

        public static bool GetSerialInfo(string responseXml, out string serial, out string oem, out bool disabled)
        {
            serial = null;
            oem = null;
            disabled = false;
            try
            {
                if (string.IsNullOrEmpty(responseXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(responseXml);
                XElement serialInfoNode = xmlDoc.Root?.Element("serial_info");
                if (serialInfoNode != null)
                {
                    XAttribute serialAttr = serialInfoNode.Attribute("serial");
                    if (serialAttr != null)
                    {
                        serial = serialAttr.Value;
                    }

                    XAttribute oemAttr = serialInfoNode.Attribute("oem");
                    if (oemAttr != null)
                    {
                        oem = oemAttr.Value;
                    }

                    XAttribute disabledAttr = serialInfoNode.Attribute("disabled");
                    if (disabledAttr != null)
                    {
                        try
                        {
                            disabled = XmlConvert.ToBoolean(disabledAttr.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool UpdateSerialInfo(string responseXml)
        {
            SerialInfoEntry serialInfo = null;
            if (GetSerialInfo(responseXml, out string serial, out string oem, out bool disabled))
            {
                serialInfo = new SerialInfoEntry(serial, oem, disabled, true);
            }
            else
            {
                if (!string.IsNullOrEmpty(LastAdapterSerial))
                {
                    serialInfo = new SerialInfoEntry(LastAdapterSerial, string.Empty, false, false);
                }
            }

            if (serialInfo != null)
            {
                return SerialInfoListAdd(serialInfo);
            }

            return false;
        }

        public static List<SerialInfoEntry> GetSerialInfoList()
        {
            lock (SerialInfoLockObject)
            {
                return new List<SerialInfoEntry>(_serialInfoList);
            }
        }

        public static void SetSerialInfoList(List<SerialInfoEntry> serialList)
        {
            lock (SerialInfoLockObject)
            {
                _serialInfoList.Clear();
                if (serialList != null)
                {
                    foreach (SerialInfoEntry serialInfo in serialList)
                    {
                        if (string.IsNullOrEmpty(serialInfo.Serial))
                        {
                            continue;
                        }

                        if (_serialInfoList.Contains(serialInfo))
                        {
                            continue;
                        }

                        _serialInfoList.Add(serialInfo);
                    }
                }
            }
        }

        public static void SerialInfoListClear()
        {
            lock (SerialInfoLockObject)
            {
                _serialInfoList.Clear();
            }
        }

        public static bool SerialInfoListAdd(SerialInfoEntry serialInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(serialInfo.Serial))
                {
                    return false;
                }

                lock (SerialInfoLockObject)
                {
                    if (_serialInfoList.Contains(serialInfo))
                    {
                        _serialInfoList.Remove(serialInfo);
                    }

                    _serialInfoList.Insert(0, serialInfo);
                    while (_serialInfoList.Count > 10)
                    {
                        _serialInfoList.RemoveAt(_serialInfoList.Count - 1);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsSerialNumberCheckRequired()
        {
            string serialNumber = LastAdapterSerial;
            if (string.IsNullOrEmpty(serialNumber))
            {
                return false;
            }

            SerialInfoEntry serialInfo = new SerialInfoEntry(serialNumber, string.Empty, false, true);
            lock (SerialInfoLockObject)
            {
                return !_serialInfoList.Contains(serialInfo);
            }
        }

        public static List<int> GetCpuUsageStatistic()
        {
            if (!IsCpuStatisticsSupported())
            {
                return null;
            }

            try
            {
                string tempString = ExecuteTop();
                if (tempString == null)
                {
                    return null;
                }
                MatchCollection matches = Regex.Matches(tempString, "User +(\\d+)%, +System +(\\d+)%, +IOW +(\\d+)%, +IRQ +(\\d+)%", RegexOptions.IgnoreCase);
                if ((matches.Count != 1) || (matches[0].Groups.Count != 5))
                {
                    return null;
                }
                List<int> resultList = new List<int>();
                int index = 0;
                foreach (Group group in matches[0].Groups)
                {
                    if (index > 0)
                    {
                        if (Int32.TryParse(group.Value, out int value))
                        {
                            resultList.Add(value);
                        }
                    }
                    index++;
                }
                return resultList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> ExecuteCommand(string command)
        {
            Java.Lang.Process process = null;
            Java.IO.BufferedReader input = null;
            try
            {
                process = Java.Lang.Runtime.GetRuntime()?.Exec(command);
                if (process == null)
                {
                    return null;
                }

                input = new Java.IO.BufferedReader(new Java.IO.InputStreamReader(process.InputStream));
                if (process.WaitFor() != 0)
                {
                    return null;
                }

                List<string> resultList = new List<string>();
                for (; ; )
                {
                    string line = input.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (line.Length > 0)
                    {
                        resultList.Add(line);
                    }
                }
                return resultList;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                try
                {
                    input?.Close();
                    process?.Destroy();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static string ExecuteTop()
        {
            List<string> resultList = ExecuteCommand("top -n 1");
            if (resultList != null && resultList.Count > 0)
            {
                return resultList[0];
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static bool PostRunnable(Handler handler, Java.Lang.IRunnable runnable, RunnablePostDelegate runnablePostDelegate = null)
        {
            if (handler == null || runnable == null)
            {
                return false;
            }

            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    if (!handler.HasCallbacks(runnable))
                    {
                        runnablePostDelegate?.Invoke();
                        handler.Post(runnable);
                        return true;
                    }
                    return false;
                }

                handler.RemoveCallbacks(runnable);
                runnablePostDelegate?.Invoke();
                handler.Post(runnable);
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public bool RegisterInternetCellularCallback()
        {
            if (IsEmulator())
            {
                return false;
            }

            if (_maConnectivity == null)
            {
                return false;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                return false;
            }

            UnRegisterInternetCellularCallback();
            try
            {
                NetworkRequest.Builder builder = new NetworkRequest.Builder();
                builder.AddCapability(NetCapability.Internet);
                builder.AddTransportType(Android.Net.TransportType.Cellular);
                NetworkRequest networkRequest = builder.Build();
                _cellularCallback = new CellularCallback(this);
                _maConnectivity.RequestNetwork(networkRequest, _cellularCallback);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool UnRegisterInternetCellularCallback()
        {
            if (IsEmulator())
            {
                return false;
            }

            if (_maConnectivity == null)
            {
                return false;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                return true;
            }

            if (_cellularCallback != null)
            {
                try
                {
                    _maConnectivity.UnregisterNetworkCallback(_cellularCallback);
                }
                catch (Exception)
                {
                    return false;
                }
                _cellularCallback = null;
            }

            lock (_networkData.LockObject)
            {
                _networkData.ActiveCellularNetworks.Clear();
            }
            return true;
        }

        public bool RegisterWifiEnetNetworkCallback()
        {
            if (_maConnectivity == null)
            {
                return false;
            }
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                try
                {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
#pragma warning disable CA1422
                    _context?.RegisterReceiver(_bcReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));   // system broadcasts
#pragma warning restore CA1422
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }

            UnRegisterWifiEnetCallback();
            try
            {
                NetworkRequest.Builder builderWifi = new NetworkRequest.Builder();
                builderWifi.AddCapability(NetCapability.Internet);
                builderWifi.AddTransportType(Android.Net.TransportType.Wifi);
                NetworkRequest networkWifiRequest = builderWifi.Build();
                _wifiCallback = new WifiCallback(this);
                _maConnectivity.RequestNetwork(networkWifiRequest, _wifiCallback);

                NetworkRequest.Builder builderEthernet = new NetworkRequest.Builder();
                builderEthernet.AddCapability(NetCapability.Internet);
                builderEthernet.AddTransportType(Android.Net.TransportType.Ethernet);
                NetworkRequest networkEthernetRequest = builderEthernet.Build();
                _ethernetCallback = new EthernetCallback(this);
                _maConnectivity.RequestNetwork(networkEthernetRequest, _ethernetCallback);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool UnRegisterWifiEnetCallback()
        {
            if (_maConnectivity == null)
            {
                return false;
            }
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                return true;
            }

            if (_wifiCallback != null)
            {
                try
                {
                    _maConnectivity.UnregisterNetworkCallback(_wifiCallback);
                }
                catch (Exception)
                {
                    return false;
                }
                _wifiCallback = null;
            }

            if (_ethernetCallback != null)
            {
                try
                {
                    _maConnectivity.UnregisterNetworkCallback(_ethernetCallback);
                }
                catch (Exception)
                {
                    return false;
                }
                _ethernetCallback = null;
            }

            lock (_networkData.LockObject)
            {
                _networkData.ActiveWifiNetworks.Clear();
                _networkData.ActiveEthernetNetworks.Clear();
            }

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool SetPreferredNetworkInterface()
        {
            if (IsEmulator())
            {
                return false;
            }

            bool forceMobile = IsNetworkAdapter();

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                return false;
            }

            Network defaultNetwork = null;
            Network bindNetwork = null;
            lock (_networkData.LockObject)
            {
                List<Network> networkList = new List<Network>();

                switch (SelectedInternetConnection)
                {
                    case InternetConnectionType.Wifi:
                        networkList.AddRange(_networkData.ActiveWifiNetworks);
                        break;

                    case InternetConnectionType.Ethernet:
                        networkList.AddRange(_networkData.ActiveEthernetNetworks);
                        break;

                    default:
                        networkList.AddRange(_networkData.ActiveCellularNetworks);
                        break;
                }

                foreach (Network network in networkList)
                {
                    LinkProperties linkProperties = _maConnectivity.GetLinkProperties(network);
                    if (linkProperties != null)
                    {
                        bindNetwork = network;
                        break;
                    }
                }
            }

            if (forceMobile && bindNetwork != null)
            {
                defaultNetwork = bindNetwork;
            }

            //Android.Util.Log.WriteLine(Android.Util.LogPriority.Debug, "Network", (defaultNetwork != null) ? "Mobile selected" : "Mobile not selected");
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable 618
#pragma warning disable CA1422
                    ConnectivityManager.SetProcessDefaultNetwork(defaultNetwork);
#pragma warning restore CA1422
#pragma warning restore 618
                }
                else
                {
                    _maConnectivity.BindProcessToNetwork(defaultNetwork);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool RegisterNotificationChannels()
        {
            try
            {
                if (_notificationManagerCompat == null || _context == null)
                {
                    return false;
                }

                UnregisterNotificationChannels();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Android.App.NotificationChannel notificationChannelCommunication =
                        new Android.App.NotificationChannel(NotificationChannelCommunication, _context.Resources.GetString(Resource.String.notification_communication), Android.App.NotificationImportance.Min);
                    _notificationManagerCompat.CreateNotificationChannel(notificationChannelCommunication);

                    Android.App.NotificationChannelGroup notificationGroupCustom =
                        new Android.App.NotificationChannelGroup(NotificationChannelGroupCustom, _context.Resources.GetString(Resource.String.notification_group_custom));
                    _notificationManagerCompat.CreateNotificationChannelGroup(notificationGroupCustom);

                    Android.App.NotificationChannel notificationChannelCustomMin =
                        new Android.App.NotificationChannel(NotificationChannelCustomMin, _context.Resources.GetString(Resource.String.notification_custom_min), Android.App.NotificationImportance.Min);
                    notificationChannelCustomMin.Group = NotificationChannelGroupCustom;
                    _notificationManagerCompat.CreateNotificationChannel(notificationChannelCustomMin);

                    Android.App.NotificationChannel notificationChannelCustomLow =
                        new Android.App.NotificationChannel(NotificationChannelCustomLow, _context.Resources.GetString(Resource.String.notification_custom_low), Android.App.NotificationImportance.Low);
                    notificationChannelCustomLow.Group = NotificationChannelGroupCustom;
                    _notificationManagerCompat.CreateNotificationChannel(notificationChannelCustomLow);

                    Android.App.NotificationChannel notificationChannelCustomDefault =
                        new Android.App.NotificationChannel(NotificationChannelCustomDefault, _context.Resources.GetString(Resource.String.notification_custom_default), Android.App.NotificationImportance.Default);
                    notificationChannelCustomDefault.Group = NotificationChannelGroupCustom;
                    _notificationManagerCompat.CreateNotificationChannel(notificationChannelCustomDefault);

                    Android.App.NotificationChannel notificationChannelCustomHigh =
                        new Android.App.NotificationChannel(NotificationChannelCustomHigh, _context.Resources.GetString(Resource.String.notification_custom_high), Android.App.NotificationImportance.High);
                    notificationChannelCustomHigh.Group = NotificationChannelGroupCustom;
                    _notificationManagerCompat.CreateNotificationChannel(notificationChannelCustomHigh);

                    Android.App.NotificationChannel notificationChannelCustomMax =
                        new Android.App.NotificationChannel(NotificationChannelCustomMax, _context.Resources.GetString(Resource.String.notification_custom_max), Android.App.NotificationImportance.Max);
                    notificationChannelCustomMax.Group = NotificationChannelGroupCustom;
                    _notificationManagerCompat.CreateNotificationChannel(notificationChannelCustomMax);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UnregisterNotificationChannels(bool unregisterAll = false)
        {
            try
            {
                if (_notificationManagerCompat == null)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    if (unregisterAll)
                    {
                        _notificationManagerCompat.DeleteNotificationChannel(NotificationChannelCommunication);
                        _notificationManagerCompat.DeleteNotificationChannel(NotificationChannelCustomMin);
                        _notificationManagerCompat.DeleteNotificationChannel(NotificationChannelCustomLow);
                        _notificationManagerCompat.DeleteNotificationChannel(NotificationChannelCustomDefault);
                        _notificationManagerCompat.DeleteNotificationChannel(NotificationChannelCustomHigh);
                        _notificationManagerCompat.DeleteNotificationChannel(NotificationChannelCustomMax);
                        _notificationManagerCompat.DeleteNotificationChannelGroup(NotificationChannelGroupCustom);
                    }

                    _notificationManagerCompat.DeleteNotificationChannel("NotificationChannelMin");
                    _notificationManagerCompat.DeleteNotificationChannel("NotificationChannelLow");
                    _notificationManagerCompat.DeleteNotificationChannel("NotificationChannelDefault");
                    _notificationManagerCompat.DeleteNotificationChannel("NotificationChannelHigh");
                    _notificationManagerCompat.DeleteNotificationChannel("NotificationChannelMax");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool NotificationsEnabled(string channelId = null)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    if (_notificationManagerCompat == null)
                    {
                        return true;
                    }

                    if (!_notificationManagerCompat.AreNotificationsEnabled())
                    {
                        return false;
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        if (!string.IsNullOrEmpty(channelId))
                        {
                            NotificationChannelCompat notificationChannel = _notificationManagerCompat.GetNotificationChannelCompat(channelId);
                            if (notificationChannel != null)
                            {
                                if (notificationChannel.Importance == (int) Android.App.NotificationImportance.None)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool IsNotificationActive(int notificationId)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Android.Service.Notification.StatusBarNotification[] notifications = _notificationManager?.GetActiveNotifications();
                if (notifications != null)
                {
                    foreach (Android.Service.Notification.StatusBarNotification statusBarNotification in notifications)
                    {
                        if (statusBarNotification.Id == notificationId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool ShowNotification(int id, int priority, string title, string message, bool update = false)
        {
            try
            {
                if (_notificationManagerCompat == null || _context == null || id > UserNotificationIdMax)
                {
                    return false;
                }

                if (!NotificationsEnabled())
                {
                    return false;
                }

                if (!update && IsNotificationActive(id))
                {
                    return false;
                }

                string notificationChannel = NotificationChannelCustomDefault;
                switch (priority)
                {
                    case NotificationCompat.PriorityMin:
                        notificationChannel = NotificationChannelCustomMin;
                        break;

                    case NotificationCompat.PriorityLow:
                        notificationChannel = NotificationChannelCustomLow;
                        break;

                    case NotificationCompat.PriorityHigh:
                        notificationChannel = NotificationChannelCustomHigh;
                        break;

                    case NotificationCompat.PriorityMax:
                        notificationChannel = NotificationChannelCustomMax;
                        break;
                }

                Intent notificationIntent = new Intent(_context, typeof(ActivityMain));
                notificationIntent.SetFlags(ActivityFlags.NewTask);

                Android.App.PendingIntentFlags intentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    intentFlags |= Android.App.PendingIntentFlags.Immutable;
                }
                Android.App.PendingIntent pendingIntent = Android.App.PendingIntent.GetActivity(_context, 0, notificationIntent, intentFlags);

                Android.App.Notification notification = new NotificationCompat.Builder(_context, notificationChannel)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetSmallIcon(Resource.Drawable.ic_stat_obd)
                    .SetPriority(priority)
                    .SetContentIntent(pendingIntent)
                    .SetAutoCancel(true)
                    .SetOnlyAlertOnce(true)
                    .SetCategory(NotificationCompat.CategoryMessage)
                    .Build();

                _notificationManagerCompat.Notify(id, notification);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool HideNotification(int id)
        {
            try
            {
                if (_notificationManagerCompat == null || _context == null || id > UserNotificationIdMax)
                {
                    return false;
                }

                _notificationManagerCompat.Cancel(id);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AllowAdapterConfig(string deviceAddress)
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                {
                    if (string.IsNullOrEmpty(deviceAddress))
                    {
                        return false;
                    }
                    string[] stringList = deviceAddress.Split('#', ';');
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], EdBluetoothInterface.ElmDeepObdTag, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(stringList[1], EdBluetoothInterface.RawTag, StringComparison.OrdinalIgnoreCase) == 0)
                        {   // allow firmware change or flashing of corrupted Deep OBD adapters
                            return true;
                        }
                        return false;
                    }
                    return true;
                }

                case InterfaceType.Enet:
                {
                    if (string.IsNullOrEmpty(GetEnetAdapterIp(out string defaultPassword)))
                    {
                        return false;
                    }
                    if (defaultPassword == null)
                    {
                        return false;
                    }
                    return true;
                }

                case InterfaceType.ElmWifi:
                    return false;

                case InterfaceType.DeepObdWifi:
                    return true;

                case InterfaceType.Ftdi:
                    return true;
            }
            return false;
        }

        public static bool IsRawAdapter(InterfaceType interfaceType, string deviceAddress)
        {
            switch (interfaceType)
            {
                case InterfaceType.Bluetooth:
                    if (deviceAddress == null)
                    {
                        return false;
                    }
                    string[] stringList = deviceAddress.Split('#', ';');
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], EdBluetoothInterface.RawTag, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
            }

            return false;
        }

        public bool IsElmDevice(string deviceAddress)
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                {
                    if (string.IsNullOrEmpty(deviceAddress))
                    {
                        return false;
                    }
                    string[] stringList = deviceAddress.Split('#', ';');
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], EdBluetoothInterface.Elm327Tag, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(stringList[1], EdBluetoothInterface.ElmDeepObdTag, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                case InterfaceType.ElmWifi:
                    return true;
            }
            return false;
        }

        public bool IsBmwCodingInterface(string deviceAddress)
        {
            bool allowCoding = false;

            if (SelectedManufacturer == ManufacturerType.Bmw)
            {
                switch (_selectedInterface)
                {
                    case InterfaceType.Bluetooth:
                    case InterfaceType.Enet:
                    case InterfaceType.Ftdi:
                        allowCoding = true;
                        break;
                }
            }

            return allowCoding;
        }

        public static bool IsBmwCodingSeries(string series)
        {
            if (!string.IsNullOrEmpty(series) && series.Length > 0)
            {
                char typeChar = char.ToUpperInvariant(series[0]);
                if (char.IsLetter(typeChar) && typeChar > 'E')
                {
                    return true;
                }
            }

            return false;
        }

        public string GetEnetAdapterIp(out string defaultPassword)
        {
            defaultPassword = null;
            if ((_maWifi == null) || !_maWifi.IsWifiEnabled)
            {
                return null;
            }

            if (!IsValidWifiConnection(out _, out _, out string dhcpServerAddress, out string ssid))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(dhcpServerAddress))
            {
                string adapterIp = dhcpServerAddress;
                if (!string.IsNullOrEmpty(ssid))
                {
                    if (ssid.Contains(AdapterSsidDeepObd))
                    {
                        defaultPassword = DefaultPwdDeepObd;
                        return adapterIp;
                    }
                    if (ssid.Contains(AdapterSsidEnetLink))
                    {
                        defaultPassword = string.Empty;
                        return adapterIp;
                    }
                    if (ssid.Contains(AdapterSsidModBmw) || ssid.Contains(AdapterSsidUniCar))
                    {
                        defaultPassword = DefaultPwdModBmw;
                        return adapterIp;
                    }
                }

                if (string.Compare(adapterIp, DeepObdAdapterIp, StringComparison.Ordinal) == 0)
                {
                    defaultPassword = DefaultPwdDeepObd;
                    return adapterIp;
                }
                if (string.Compare(adapterIp, EnetLinkAdapterIp, StringComparison.Ordinal) == 0)
                {
                    defaultPassword = string.Empty;
                    return adapterIp;
                }
                if (string.Compare(adapterIp, ModBmwAdapterIp, StringComparison.Ordinal) == 0)
                {
                    defaultPassword = DefaultPwdModBmw;
                    return adapterIp;
                }
            }
            return null;
        }

        public bool IsWifiAdapterValid()
        {
            if (IsWifiApMode())
            {
                return true;
            }

            if ((_maWifi == null) || !_maWifi.IsWifiEnabled)
            {
                return false;
            }

            if (!IsValidWifiConnection(out _, out _, out string dhcpServerAddress, out _))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(SelectedInterfaceIp))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(dhcpServerAddress))
            {
                string adapterIp = dhcpServerAddress;
                bool ipStandard = false;
                bool ipEspLink = false;
                if (string.Compare(adapterIp, EdElmWifiInterface.ElmIp, StringComparison.Ordinal) == 0)
                {
                    ipStandard = true;
                }

                if (_selectedInterface == InterfaceType.DeepObdWifi)
                {
                    if (string.Compare(adapterIp, EdCustomWiFiInterface.AdapterIpEspLink, StringComparison.Ordinal) == 0)
                    {
                        ipEspLink = true;
                    }
                }

                if (!ipStandard && !ipEspLink)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool EnetAdapterConfig()
        {
            if (SelectedInterface == InterfaceType.Enet)
            {
                string adapterIp = GetEnetAdapterIp(out string defaultPassword);
                if (!string.IsNullOrEmpty(adapterIp))
                {
                    if (!string.IsNullOrEmpty(defaultPassword))
                    {
                        string message = string.Format(_context.GetString(Resource.String.enet_adapter_web_info), defaultPassword);
                        new AlertDialog.Builder(_context)
                            .SetMessage(message)
                            .SetTitle(Resource.String.alert_title_info)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) =>
                            {
                                StartEnetAdapterConfig(adapterIp);
                            })
                            .Show();
                        return true;
                    }

                    StartEnetAdapterConfig(adapterIp);
                    return true;
                }
            }
            return false;
        }

        public bool StartEnetAdapterConfig(string adapterIp)
        {
            return OpenWebUrl("http://" + adapterIp);
        }

        public bool OpenWebUrl(string url)
        {
            try
            {
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://"));
                if (intent.ResolveActivity(PackageManager) == null)
                {
                    return false;
                }

                intent.SetData(Android.Net.Uri.Parse(url));
                _context.StartActivity(intent);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool IsNetworkPresent(out string domains)
        {
            domains = null;
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return true;
                }

                bool present;
                lock (_networkData.LockObject)
                {
                    present = _networkData.ActiveCellularNetworks.Count > 0 ||
                              _networkData.ActiveWifiNetworks.Count > 0 ||
                              _networkData.ActiveEthernetNetworks.Count > 0;

                    if (present && _networkData.ActiveCellularNetworks.Count == 0)
                    {
                        foreach (Network network in _networkData.ActiveWifiNetworks)
                        {
                            try
                            {
                                LinkProperties linkProperties = _maConnectivity.GetLinkProperties(network);
                                if (linkProperties != null)
                                {
                                    if (!string.IsNullOrEmpty(linkProperties.Domains))
                                    {
                                        domains = linkProperties.Domains;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }

                return present;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsConnectedToWifiAdapter()
        {
            if (!string.IsNullOrEmpty(GetEnetAdapterIp(out string _)))
            {
                return true;
            }

            if (IsWifiAdapterValid())
            {
                return true;
            }
            return false;
        }

        public bool IsWifiApMode()
        {
            try
            {
                if (_maWifi == null)
                {
                    return false;
                }

                if (_maWifi.IsWifiEnabled)
                {
                    return false;
                }

                Java.Lang.Reflect.Method methodIsWifiApEnabled = _maWifi.Class.GetDeclaredMethod(@"isWifiApEnabled");
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (methodIsWifiApEnabled != null)
                {
                    methodIsWifiApEnabled.Accessible = true;
                    Java.Lang.Object wifiApEnabledResult = methodIsWifiApEnabled.Invoke(_maWifi);
                    Java.Lang.Boolean wifiApEnabled = Android.Runtime.Extensions.JavaCast<Java.Lang.Boolean>(wifiApEnabledResult);
                    return wifiApEnabled != Java.Lang.Boolean.False;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool IsValidWifiConnection(out string localAddress, out string localMask, out string dhcpServerAddress, out string ssid)
        {
            localAddress = null;
            localMask = null;
            dhcpServerAddress = null;
            ssid = null;

            try
            {
                if (IsWifiApMode())
                {
                    List<Network> networkList = new List<Network>();
                    lock (_networkData.LockObject)
                    {
                        networkList.AddRange(_networkData.ActiveCellularNetworks);
                        networkList.AddRange(_networkData.ActiveWifiNetworks);
                        networkList.AddRange(_networkData.ActiveEthernetNetworks);
                    }

                    Java.Net.InterfaceAddress interfaceAddrAp = null;
                    Java.Util.IEnumeration networkInterfaces = Java.Net.NetworkInterface.NetworkInterfaces;
                    while (networkInterfaces != null && networkInterfaces.HasMoreElements)
                    {
                        Java.Net.NetworkInterface netInterface = (Java.Net.NetworkInterface)networkInterfaces.NextElement();
                        if (netInterface == null)
                        {
                            continue;
                        }

                        if (netInterface.IsUp)
                        {
                            IList<Java.Net.InterfaceAddress> interfaceAdresses = netInterface.InterfaceAddresses;
                            if (interfaceAdresses == null)
                            {
                                continue;
                            }

                            foreach (Java.Net.InterfaceAddress interfaceAddress in interfaceAdresses)
                            {
                                if (interfaceAddress.Broadcast != null && interfaceAddress.Address != null)
                                {
                                    bool addrFound = false;
                                    foreach (Network network in networkList)
                                    {
                                        LinkProperties linkProperties = _maConnectivity.GetLinkProperties(network);
                                        if (linkProperties != null)
                                        {
                                            foreach (LinkAddress linkAddress in linkProperties.LinkAddresses)
                                            {
                                                if (linkAddress.Address != null && linkAddress.Address.Equals(interfaceAddress.Address))
                                                {
                                                    addrFound = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (!addrFound)
                                    {
                                        if (interfaceAddrAp != null)
                                        {   // multiple matches
                                            interfaceAddrAp = null;
                                            break;
                                        }

                                        interfaceAddrAp = interfaceAddress;
                                    }
                                }
                            }
                        }
                    }

                    if (interfaceAddrAp != null)
                    {
                        localAddress = interfaceAddrAp.Address.HostAddress;
                        localMask = TcpClientWithTimeout.PrefixLenToMask(interfaceAddrAp.NetworkPrefixLength).ToString();
                    }

                    return true;
                }

                if ((_maWifi == null) || !_maWifi.IsWifiEnabled)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.S)
                {
#pragma warning disable 618
#pragma warning disable CA1422
                    WifiInfo wifiInfo = _maWifi.ConnectionInfo;
                    if (wifiInfo != null && _maWifi.DhcpInfo != null && wifiInfo.IpAddress != 0)
                    {
                        localAddress = TcpClientWithTimeout.ConvertIpAddress(_maWifi.DhcpInfo.IpAddress);
                        localMask = TcpClientWithTimeout.ConvertIpAddress(_maWifi.DhcpInfo.Netmask);
                        dhcpServerAddress = TcpClientWithTimeout.ConvertIpAddress(_maWifi.DhcpInfo.ServerAddress);
                        ssid = GetWifiSsid(wifiInfo);
                        return !string.IsNullOrEmpty(dhcpServerAddress);
                    }
#pragma warning restore CA1422
#pragma warning restore 618
                    return false;
                }

                lock (_networkData.LockObject)
                {
                    foreach (Network network in _networkData.ActiveWifiNetworks)
                    {
                        NetworkCapabilities networkCapabilities = _maConnectivity.GetNetworkCapabilities(network);
                        LinkProperties linkProperties = _maConnectivity.GetLinkProperties(network);
                        if (networkCapabilities != null && linkProperties != null && linkProperties.DhcpServerAddress != null)
                        {
                            if (networkCapabilities.TransportInfo is WifiInfo wifiInfo)
                            {
                                string serverAddress = TcpClientWithTimeout.ConvertIpAddress(linkProperties.DhcpServerAddress);
                                if (!string.IsNullOrEmpty(serverAddress))
                                {
                                    foreach (LinkAddress linkAddress in linkProperties.LinkAddresses)
                                    {
                                        if (linkAddress.Address is Java.Net.Inet4Address inet4Address)
                                        {
                                            if (inet4Address.IsSiteLocalAddress || inet4Address.IsLinkLocalAddress)
                                            {
                                                localAddress = TcpClientWithTimeout.ConvertIpAddress(linkAddress.Address);
                                                localMask = TcpClientWithTimeout.PrefixLenToMask(linkAddress.PrefixLength).ToString();
                                                dhcpServerAddress = serverAddress;
                                                ssid = GetWifiSsid(wifiInfo);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(dhcpServerAddress))
                        {
                            break;
                        }
                    }
                }

                return !string.IsNullOrEmpty(dhcpServerAddress);
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public string GetWifiSsid(WifiInfo wifiInfo)
        {
            try
            {
                if (wifiInfo == null)
                {
                    return string.Empty;
                }

                string ssid = wifiInfo.SSID;
                if (!string.IsNullOrEmpty(ssid) && !ssid.Contains(WifiManager.UnknownSsid, StringComparison.OrdinalIgnoreCase))
                {
                    return ssid;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return string.Empty;
                }

                ssid = string.Empty;
                IList<ScanResult> scanResults = _maWifi.ScanResults;
                if (scanResults != null)
                {
                    int matches = 0;
                    foreach (ScanResult scanResult in scanResults)
                    {
                        if (wifiInfo.Frequency != scanResult.Frequency || wifiInfo.WifiStandard != scanResult.WifiStandard)
                        {
                            continue;
                        }

                        string tempSsid;
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                        {
                            tempSsid = scanResult.WifiSsid?.ToString();
                        }
                        else
                        {
#pragma warning disable CS0618
#pragma warning disable CA1422
                            tempSsid = scanResult.Ssid;
#pragma warning restore CA1422
#pragma warning restore CS0618
                        }

                        if (!string.IsNullOrEmpty(tempSsid) && string.Compare(tempSsid, ssid, StringComparison.Ordinal) != 0)
                        {
                            ssid = tempSsid;
                            matches++;
                        }
                    }

                    if (matches == 1)
                    {
                        return ssid;
                    }

                    return string.Empty;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return string.Empty;
        }

        public bool IsValidEthernetConnection(string ipAddrMatch = null)
        {
            return IsValidEthernetConnection(ipAddrMatch, out _, out _);
        }

        public bool IsValidEthernetConnection(string ipAddrMatch, out string localAddress, out string localMask)
        {
            localAddress = null;
            localMask = null;

            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }

                Java.Net.Inet4Address inet4AddrCheck = null;
                if (!string.IsNullOrEmpty(ipAddrMatch))
                {
                    try
                    {
                        Java.Net.InetAddress inetAddrMatch = Java.Net.InetAddress.GetByName(ipAddrMatch);
                        if (inetAddrMatch is Java.Net.Inet4Address inet4AddrMatch)
                        {
                            inet4AddrCheck = inet4AddrMatch;
                        }
                    }
                    catch (Exception)
                    {
                        inet4AddrCheck = null;
                    }
                }

                bool result = false;
                lock (_networkData.LockObject)
                {
                    foreach (Network network in _networkData.ActiveEthernetNetworks)
                    {
                        LinkProperties linkProperties = _maConnectivity.GetLinkProperties(network);
                        if (linkProperties != null)
                        {
                            foreach (LinkAddress linkAddress in linkProperties.LinkAddresses)
                            {
                                if (linkAddress.Address is Java.Net.Inet4Address inet4Address)
                                {
                                    if (inet4Address.IsSiteLocalAddress || inet4Address.IsLinkLocalAddress)
                                    {
                                        if (inet4AddrCheck != null)
                                        {
                                            if (TcpClientWithTimeout.IsIpMatchingSubnet(inet4AddrCheck, inet4Address, linkAddress.PrefixLength))
                                            {
                                                result = true;
                                            }
                                        }
                                        else
                                        {
                                            result = true;
                                        }

                                        if (result)
                                        {
                                            localAddress = TcpClientWithTimeout.ConvertIpAddress(linkAddress.Address);
                                            localMask = TcpClientWithTimeout.PrefixLenToMask(linkAddress.PrefixLength).ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ShowWifiConnectedWarning(WifiConnectedWarnDelegate handler)
        {
            if (!IsConnectedToWifiAdapter())
            {
                handler();
                return true;
            }
            new AlertDialog.Builder(_context)
            .SetMessage(Resource.String.connected_with_wifi_adapter)
            .SetTitle(Resource.String.alert_title_warning)
            .SetPositiveButton(Resource.String.button_yes, (s, e) =>
            {
                try
                {
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                    {
#pragma warning disable 618
#pragma warning disable CA1422
                        _maWifi?.SetWifiEnabled(false);
#pragma warning restore CA1422
#pragma warning restore 618
                    }
                    else
                    {
                        _context.StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                handler();
            })
            .SetNegativeButton(Resource.String.button_no, (s, e) =>
            {
                handler();
            })
            .Show();
            return false;
        }

        public bool ShowConnectWarning(EnetSsidWarnDelegate handler)
        {
            if (_selectedInterface == InterfaceType.Bluetooth)
            {
                if (!MtcBtService)
                {
                    return false;
                }

                if (!MtcServiceStarted || (Stopwatch.GetTimestamp() - MtcServiceStartTime < 1000 * TickResolMs))
                {
                    return false;
                }

                bool bound = MtcBtServiceBound;
                if (bound)
                {
                    if (MtcBtConnected)
                    {
                        return false;
                    }
                    if (MtcBtDisconnectWarnShown)
                    {
                        return false;
                    }
                }

                int msgId = Resource.String.mtc_disconnect_warn;
                if (!bound)
                {
                    msgId = Resource.String.mtc_not_bound_warn;
                }

                bool ignoreDismiss = false;
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                    .SetMessage(msgId)
                    .SetTitle(Resource.String.alert_title_warning)
                    .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                    {
                        if (bound)
                        {
                            MtcBtDisconnectWarnShown = true;
                        }
                    })
                    .SetNegativeButton(Resource.String.button_no, (s, e) =>
                    {
                        ignoreDismiss = true;
                    })
                    .Show();
                if (alertDialog != null)
                {
                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (!ignoreDismiss)
                        {
                            handler(SsidWarnAction.Continue);
                        }
                    };
                }

                return true;
            }
            if (_selectedInterface == InterfaceType.ElmWifi || _selectedInterface == InterfaceType.DeepObdWifi)
            {
                if (IsWifiApMode())
                {
                    if (string.IsNullOrEmpty(SelectedInterfaceIp))
                    {
                        SsidWarnAction action = SsidWarnAction.None;
                        AlertDialog alertDialogAp = new AlertDialog.Builder(_context)
                            .SetMessage(Resource.String.ap_mode_adapter_ip_error)
                            .SetTitle(Resource.String.alert_title_error)
                            .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                            {
                                action = SsidWarnAction.EditIp;
                            })
                            .SetNegativeButton(Resource.String.button_no, (s, e) =>
                            {
                                action = SsidWarnAction.None;
                            })
                            .Show();
                        if (alertDialogAp != null)
                        {
                            alertDialogAp.DismissEvent += (sender, args) =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }

                                handler(action);
                            };
                        }

                        return true;
                    }
                    return false;
                }

                if (IsWifiAdapterValid())
                {
                    return false;
                }

                if (_selectedInterface == InterfaceType.DeepObdWifi)
                {
                    if (!string.IsNullOrEmpty(SelectedInterfaceIp))
                    {
                        return false;
                    }
                }

                bool ignoreDismiss = false;
                int resourceId = _selectedInterface == InterfaceType.ElmWifi
                    ? Resource.String.elmwifi_adapter_warn
                    : Resource.String.deepobdwifi_adapter_warn;
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                .SetMessage(resourceId)
                .SetTitle(Resource.String.alert_title_warning)
                .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                {
                    ignoreDismiss = true;
                    ShowWifiSettings((sender, args) =>
                    {
                        handler(SsidWarnAction.None);
                    });
                })
                .SetNegativeButton(Resource.String.button_no, (s, e) => { })
                .Show();
                if (alertDialog != null)
                {
                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (!ignoreDismiss)
                        {
                            handler(SsidWarnAction.None);
                        }
                    };
                }
                return true;
            }

            if (_selectedInterface == InterfaceType.Enet)
            {
                if (IsEmulator())
                {
                    return false;
                }

                if (IsWifiApMode())
                {
                    return false;
                }

                bool result = false;
                string enetSsid = "NoSsid";
                bool validDeepObd = false;
                bool validEnetLink = false;
                bool validModBmw = false;
                if (IsValidWifiConnection(out _, out _, out string dhcpServerAddress, out string ssid))
                {
                    if (!string.IsNullOrEmpty(dhcpServerAddress))
                    {
                        if (!string.IsNullOrEmpty(ssid))
                        {
                            enetSsid = ssid;
                        }

                        string adapterIp = dhcpServerAddress;
                        if (string.Compare(adapterIp, DeepObdAdapterIp, StringComparison.Ordinal) == 0)
                        {
                            validDeepObd = true;
                        }
                        if (string.Compare(adapterIp, EnetLinkAdapterIp, StringComparison.Ordinal) == 0)
                        {
                            validEnetLink = true;
                        }
                        if (string.Compare(adapterIp, ModBmwAdapterIp, StringComparison.Ordinal) == 0)
                        {
                            validModBmw = true;
                        }
                    }
                }

                string lastEnetSsid = string.Empty;
                if (_baseActivity != null)
                {
                    lastEnetSsid = _baseActivity.InstanceDataCommon.LastEnetSsid ?? string.Empty;
                    if (string.Compare(lastEnetSsid, EnetSsidEmpty, StringComparison.Ordinal) == 0)
                    {
                        _baseActivity.InstanceDataCommon.LastEnetSsid = enetSsid;
                        lastEnetSsid = enetSsid;
                    }
                }

                bool validSsid = enetSsid.Contains(AdapterSsidDeepObd) || enetSsid.Contains(AdapterSsidEnetLink) || enetSsid.Contains(AdapterSsidModBmw) || enetSsid.Contains(AdapterSsidUniCar);
                bool validEthernet = IsValidEthernetConnection();
                bool ipSelected = !string.IsNullOrEmpty(SelectedEnetIp);

                if (!ipSelected && !validEthernet && !validDeepObd && !validEnetLink && !validModBmw &&
                    string.Compare(lastEnetSsid, enetSsid, StringComparison.Ordinal) != 0)
                {
                    if (_baseActivity != null)
                    {
                        _baseActivity.InstanceDataCommon.LastEnetSsid = enetSsid;
                    }

                    if (!validSsid && string.IsNullOrEmpty(SelectedEnetIp))
                    {
                        string message = _context.GetString(Resource.String.enet_adapter_ssid_warn);
                        if (Build.VERSION.SdkInt >= MinEthernetSettingsVersion)
                        {
                            message += "\n" + _context.GetString(Resource.String.enet_ethernet_hint);
                        }

                        bool ignoreDismiss = false;
                        AlertDialog alertDialog = new AlertDialog.Builder(_context)
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_warning)
                        .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                        {
                            ignoreDismiss = true;
                            ShowWifiSettings((sender, args) =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                handler(SsidWarnAction.None);
                            });
                        })
                        .SetNegativeButton(Resource.String.button_no, (s, e) => { })
                        .Show();
                        if (alertDialog != null)
                        {
                            alertDialog.DismissEvent += (sender, args) =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                if (!ignoreDismiss)
                                {
                                    handler(SsidWarnAction.Continue);
                                }
                            };
                        }

                        result = true;
                    }
                }
                return result;
            }
            return false;
        }

        public bool ShowWifiSettings(EventHandler handler)
        {
            if (_selectedInterface == InterfaceType.Enet)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                .SetMessage(Resource.String.enet_adapter_wifi_info)
                .SetTitle(Resource.String.alert_title_info)
                .SetNeutralButton(Resource.String.button_ok, (s, e) =>
                {
                    try
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                        {
                            if (_maWifi != null && !_maWifi.IsWifiEnabled)
                            {
#pragma warning disable 618
#pragma warning disable CA1422
                                _maWifi.SetWifiEnabled(true);
#pragma warning restore CA1422
#pragma warning restore 618
                            }
                        }
                        _context.StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                })
                .Show();
                if (alertDialog != null)
                {
                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        handler(sender, args);
                    };
                }
                return true;
            }
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                {
                    if (_maWifi != null && !_maWifi.IsWifiEnabled)
                    {
#pragma warning disable 618
#pragma warning disable CA1422
                        _maWifi.SetWifiEnabled(true);
#pragma warning restore CA1422
#pragma warning restore 618
                    }
                }
                _context.StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
            }
            catch (Exception)
            {
                // ignored
            }
            return true;
        }

        public void ShowAlert(string message, int titleId)
        {
            new AlertDialog.Builder(_context)
            .SetMessage(message)
            .SetTitle(titleId)
            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
            .Show();
        }

        public void SelectMedia(EventHandler<DialogClickEventArgs> handler)
        {
            if (_selectMediaAlertDialog != null)
            {
                return;
            }
            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(Resource.String.select_media);
            ListView listView = new ListView(_context);

            List<string> mediaNames = GetAllStorageMedia();
            int mediaIndex = 0;
            if (!string.IsNullOrEmpty(_customStorageMedia))
            {
                int index = 0;
                foreach (string name in mediaNames)
                {
                    if (string.CompareOrdinal(name, _customStorageMedia) == 0)
                    {
                        mediaIndex = index + 1;
                        break;
                    }
                    index++;
                }
            }
            List<string> displayNames = new List<string>();
            foreach (string name in mediaNames)
            {
                string displayName = name;
                try
                {
                    FileSystemBlockInfo blockInfo = GetFileSystemBlockInfo(name);
                    string shortName = GetTruncatedPathName(name);
                    if (!string.IsNullOrEmpty(shortName))
                    {
                        displayName = String.Format(new FileSizeFormatProvider(), "{0} ({1:fs1}/{2:fs1} {3})",
                            shortName, blockInfo.AvailableSizeBytes, blockInfo.TotalSizeBytes, _context.GetString(Resource.String.free_space));
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                displayNames.Add(displayName);
            }
            displayNames.Insert(0, _context.GetString(Resource.String.default_media));

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                Android.Resource.Layout.SimpleListItemSingleChoice, displayNames);
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            listView.SetItemChecked(mediaIndex, true);
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                ResetUdsReader();
                ResetEcuFunctionReader();
                switch (listView.CheckedItemPosition)
                {
                    case 0:
                        _customStorageMedia = null;
                        handler(sender, args);
                        break;

                    default:
                        _customStorageMedia = mediaNames[listView.CheckedItemPosition - 1];
                        handler(sender, args);
                        break;
                }
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            _selectMediaAlertDialog = builder.Show();
            if (_selectMediaAlertDialog != null)
            {
                _selectMediaAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    _selectMediaAlertDialog = null;
                };
            }
        }

        public void SelectInterface(EventHandler<DialogClickEventArgs> handler)
        {
            if (_selectInterfaceAlertDialog != null)
            {
                return;
            }
            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(Resource.String.select_interface);
            ListView listView = new ListView(_context);

            List<InterfaceType> interfaceTypes = new List<InterfaceType>();
            // ReSharper disable once UseObjectOrCollectionInitializer
            List<string> interfaceNames = new List<string>();
            interfaceNames.Add(_context.GetString(Resource.String.select_interface_bt));
            interfaceTypes.Add(InterfaceType.Bluetooth);

            if (SelectedManufacturer == ManufacturerType.Bmw)
            {
                interfaceNames.Add(_context.GetString(Resource.String.select_interface_enet));
                interfaceTypes.Add(InterfaceType.Enet);

                interfaceNames.Add(_context.GetString(Resource.String.select_interface_elmwifi));
                interfaceTypes.Add(InterfaceType.ElmWifi);
            }

            interfaceNames.Add(_context.GetString(Resource.String.select_interface_deepobdwifi));
            interfaceTypes.Add(InterfaceType.DeepObdWifi);

            if (SelectedManufacturer == ManufacturerType.Bmw && UsbSupport)
            {
                interfaceNames.Add(_context.GetString(Resource.String.select_interface_ftdi));
                interfaceTypes.Add(InterfaceType.Ftdi);
            }

            interfaceNames.Add(_context.GetString(Resource.String.select_interface_simulation));
            interfaceTypes.Add(InterfaceType.Simulation);

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                Android.Resource.Layout.SimpleListItemSingleChoice, interfaceNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            int index = interfaceTypes.IndexOf(_selectedInterface);
            if (index >= 0)
            {
                listView.SetItemChecked(index, true);
            }
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                int pos = listView.CheckedItemPosition;
                if (pos >= 0 && pos < interfaceTypes.Count)
                {
                    SelectedInterface = interfaceTypes[pos];
                    handler(sender, args);
                }
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                {
                });
            _selectInterfaceAlertDialog = builder.Show();
            if (_selectInterfaceAlertDialog != null)
            {
                _selectInterfaceAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    _selectInterfaceAlertDialog = null;
                };
            }
        }

        public void SelectManufacturer(EventHandler<DialogClickEventArgs> handler)
        {
            if (_disposed)
            {
                return;
            }

            if (_selectManufacturerAlertDialog != null)
            {
                return;
            }
            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(Resource.String.select_manufacturer);
            ListView listView = new ListView(_context);

            List<string> manufacturerNames = new List<string>
            {
                _context.GetString(Resource.String.select_manufacturer_bmw),
                _context.GetString(Resource.String.select_manufacturer_audi),
                _context.GetString(Resource.String.select_manufacturer_seat),
                _context.GetString(Resource.String.select_manufacturer_skoda),
                _context.GetString(Resource.String.select_manufacturer_vw),
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                Android.Resource.Layout.SimpleListItemSingleChoice, manufacturerNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            switch (SelectedManufacturer)
            {
                case ManufacturerType.Bmw:
                    listView.SetItemChecked(0, true);
                    break;

                case ManufacturerType.Audi:
                    listView.SetItemChecked(1, true);
                    break;

                case ManufacturerType.Seat:
                    listView.SetItemChecked(2, true);
                    break;

                case ManufacturerType.Skoda:
                    listView.SetItemChecked(3, true);
                    break;

                case ManufacturerType.Vw:
                    listView.SetItemChecked(4, true);
                    break;
            }
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                switch (listView.CheckedItemPosition)
                {
                    case 0:
                        SelectedManufacturer = ManufacturerType.Bmw;
                        handler(sender, args);
                        break;

                    case 1:
                        SelectedManufacturer = ManufacturerType.Audi;
                        handler(sender, args);
                        break;

                    case 2:
                        SelectedManufacturer = ManufacturerType.Seat;
                        handler(sender, args);
                        break;

                    case 3:
                        SelectedManufacturer = ManufacturerType.Skoda;
                        handler(sender, args);
                        break;

                    case 4:
                        SelectedManufacturer = ManufacturerType.Vw;
                        handler(sender, args);
                        break;
                }
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });

            _selectManufacturerAlertDialog = builder.Create();
            if (_selectManufacturerAlertDialog != null)
            {
                _selectManufacturerAlertDialog.ShowEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    string message = string.Format(CultureInfo.InvariantCulture, _context.GetString(Resource.String.vag_mode_info_ballon), VagEndDate);
                    ShowAlertDialogBallon(_context, _selectManufacturerAlertDialog, message, 20000);
                };

                _selectManufacturerAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _selectManufacturerAlertDialog = null;
                };
            }

            _selectManufacturerAlertDialog.Show();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool RequestBtPermissions()
        {
            if (MtcBtService)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.S)
                {
                    return true;
                }
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
            {
                return true;
            }

            try
            {
                if (PermissionsBluetooth.All(permission => ContextCompat.CheckSelfPermission(_activity, permission) == Permission.Granted))
                {
                    return true;
                }

                ActivityCompat.RequestPermissions(_activity, PermissionsBluetooth, RequestPermissionBluetooth);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string OpenExternalFile(string filePath, int requestCode)
        {
            if (_activity == null)
            {
                return string.Empty;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    return string.Empty;
                }

                string extension = Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(extension))
                {
                    return string.Empty;
                }

                string bareExt = extension.TrimStart('.');
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("OpenExternalFile File Ext: {0}", bareExt));
#endif
                Intent viewIntent = new Intent(Intent.ActionView);
                Android.Net.Uri fileUri = FileProvider.GetUriForFile(Android.App.Application.Context, _activity.PackageName + ".fileprovider", new Java.IO.File(filePath));
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("OpenExternalFile File Uri: {0}", fileUri?.ToString()));
#endif
                string mimeType = Android.Webkit.MimeTypeMap.Singleton?.GetMimeTypeFromExtension(bareExt);
                viewIntent.SetDataAndType(fileUri, mimeType);
                viewIntent.SetFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission | ActivityFlags.NewTask);

                IList<ResolveInfo> activities = QueryIntentActivities(viewIntent, PackageInfoFlags.MatchDefaultOnly);
                if (activities == null || activities.Count == 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "OpenExternalFile QueryIntentActivities failed");
#endif
                    return string.Format(CultureInfo.InvariantCulture, _activity.GetString(Resource.String.no_ext_app_installed), bareExt);
                }

                Intent chooseIntent = Intent.CreateChooser(viewIntent, _activity.GetString(Resource.String.choose_file_app));
                _activity.StartActivityForResult(chooseIntent, requestCode);
            }
            catch (Exception ex)
            {
                string errorMessage = EdiabasNet.GetExceptionText(ex);
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("OpenExternalFile Exception: {0}", errorMessage));
#endif
                string message = _activity.GetString(Resource.String.file_access_denied);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message += "\r\n" + errorMessage;
                }
                return message;
            }

            return null;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool ShowNotificationSettings(int requestCodeApp, int? requestCodeChannel = null, string channelId = null)
        {
            if (_activity == null)
            {
                return false;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return false;
            }

            if (NotificationsEnabled() && requestCodeChannel != null && !string.IsNullOrEmpty(channelId))
            {
                try
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionChannelNotificationSettings);
                    intent.PutExtra(Android.Provider.Settings.ExtraChannelId, channelId);
                    intent.PutExtra(Android.Provider.Settings.ExtraAppPackage, _activity.PackageName);
                    _activity.StartActivityForResult(intent, requestCodeChannel.Value);
                    return true;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            try
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionAppNotificationSettings);
                intent.PutExtra(Android.Provider.Settings.ExtraAppPackage, _activity.PackageName);
                _activity.StartActivityForResult(intent, requestCodeApp);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public void EnableInterface()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        Toast.MakeText(_context, Resource.String.bt_not_available, ToastLength.Long)?.Show();
                        break;
                    }
                    if (!_btAdapter.IsEnabled)
                    {
                        try
                        {
                            bool directEnable = true;
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                            {
                                try
                                {
                                    Intent intentBtEnabled = new Intent(BluetoothAdapter.ActionRequestEnable);
                                    _activity?.StartActivity(intentBtEnabled);
                                    directEnable = false;
                                }
                                catch (Exception)
                                {
                                    directEnable = true;
                                }
                            }

                            if (directEnable)
                            {
#pragma warning disable 0618
#pragma warning disable CA1422
                                if (_btAdapter.Enable())
#pragma warning restore CA1422
#pragma warning restore 0618
                                {
                                    _btEnableCounter = 2;
                                }
                            }

                            if (_bcReceiverUpdateDisplayHandler != null && _btUpdateHandler != null)
                            {   // some device don't send the update event
                                _btUpdateHandler.RemoveCallbacks(_btUpdateRunnable);
                                _btUpdateHandler.PostDelayed(_btUpdateRunnable, 1000);
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;

                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    if (_maWifi == null)
                    {
                        Toast.MakeText(_context, Resource.String.wifi_not_available, ToastLength.Long)?.Show();
                        break;
                    }
                    if (!_maWifi.IsWifiEnabled)
                    {
                        if (_baseActivity != null)
                        {
                            _baseActivity.InstanceDataCommon.LastEnetSsid = string.Empty;
                        }

                        try
                        {
                            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                            {
#pragma warning disable 618
#pragma warning disable CA1422
                                _maWifi.SetWifiEnabled(true);
#pragma warning restore CA1422
#pragma warning restore 618
                            }
                            else
                            {
                                _context.StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;
            }
        }

        public bool RequestInterfaceEnable(EventHandler handler)
        {
            if (_activateAlertDialog != null)
            {
                return true;
            }
            if (IsInterfaceAvailable())
            {
                return false;
            }
            if (IsInterfaceEnabled())
            {
                return false;
            }
            bool ignoreDismiss = false;
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (!RequestBtPermissions())
                    {
                        return false;
                    }

                    if (MtcBtService)
                    {
                        EnableInterface();
                        return false;
                    }
                    switch (BtEnbaleHandling)
                    {
                        case BtEnableType.Ask:
                            break;

                        case BtEnableType.Always:
                            EnableInterface();
                            return false;

                        default:
                            return false;
                    }
                    _activateAlertDialog = new AlertDialog.Builder(_context)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            EnableInterface();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.bt_enable)
                        .SetTitle(Resource.String.alert_title_question)
                        .Show();
                    break;

                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                {
                    string message = _context.GetString(Resource.String.wifi_enable);
                    if (_selectedInterface == InterfaceType.Enet && Build.VERSION.SdkInt >= MinEthernetSettingsVersion)
                    {
                        message += "\n" + _context.GetString(Resource.String.enet_ethernet_hint);
                    }

                    _activateAlertDialog = new AlertDialog.Builder(_context)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            EnableInterface();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetNeutralButton(Resource.String.button_select, (sender, args) =>
                        {
                            ignoreDismiss = true;
                            EnableInterface();
                            ShowWifiSettings((o, eventArgs) =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                _activateAlertDialog = null;
                                handler(sender, args);
                            });
                        })
                        .SetCancelable(true)
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_question)
                        .Show();
                    break;
                }

                default:
                    return false;
            }

            if (_activateAlertDialog != null)
            {
                _activateAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    if (!ignoreDismiss)
                    {
                        _activateAlertDialog = null;
                        handler(sender, args);
                    }
                };
            }
            return true;
        }

        public bool IsBluetoothEnabled()
        {
            if (_btAdapter == null)
            {
                return false;
            }
            return _btAdapter.IsEnabled;
        }

        public static bool IsBluetoothEnabledByApp()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                return false;
            }

            return _btEnableCounter > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool IsBluetoothConnected()
        {
            if (!IsBluetoothEnabled())
            {
                return false;
            }

            if (Build.VERSION.SdkInt > BuildVersionCodes.Tiramisu)
            {
                if (PermissionsBluetooth.All(permission => ContextCompat.CheckSelfPermission(_activity, permission) != Permission.Granted))
                {
                    return false;
                }
            }

            bool result = false;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                foreach (ProfileType profile in Enum.GetValues(typeof(ProfileType)))
                {
                    try
                    {
                        if (_btAdapter.GetProfileConnectionState(profile) != ProfileState.Disconnected)
                        {
                            result = true;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            return result;
        }

        public bool BluetoothDisable()
        {
            if (_btAdapter == null)
            {
                return false;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                return false;
            }

            try
            {
#pragma warning disable CS0618
#pragma warning disable CA1422
                return _btAdapter.Disable();
#pragma warning restore CA1422
#pragma warning restore CS0618
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool BluetoothDisableAtExit()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {   // BluetoothDisable not supported
                return false;
            }

            bool isEmpty = BaseActivity.IsActivityListEmpty(new List<Type> { typeof(ActivityMain) });
            if (isEmpty && !BtInitiallyEnabled && BtDisableHandling == BtDisableType.DisableIfByApp &&
                IsBluetoothEnabledByApp() && !IsBluetoothConnected() && !MtcBtService &&
                !CommActive)
            {
                return BluetoothDisable();
            }
            return false;
        }

        public bool RequestBluetoothDeviceSelect(int requestCode, string appDataDir, EventHandler<DialogClickEventArgs> handler)
        {
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return true;
            }
            if (!IsInterfaceAvailable())
            {
                return true;
            }
            new AlertDialog.Builder(_context)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    if (SelectBluetoothDevice(requestCode, appDataDir))
                    {
                        handler(sender, args);
                    }
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.bt_device_not_selected)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
            return false;
        }

        public bool SelectBluetoothDevice(int requestCode, string appDataDir)
        {
            if (!IsInterfaceAvailable())
            {
                return false;
            }
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return false;
            }
            Intent serverIntent = new Intent(_context, typeof(DeviceListActivity));
            serverIntent.PutExtra(XmlToolActivity.ExtraAppDataDir, appDataDir);
            _activity?.StartActivityForResult(serverIntent, requestCode);
            return true;
        }

        public bool SelectAdapterIp(EventHandler<DialogClickEventArgs> handler)
        {
            switch (SelectedInterface)
            {
                case InterfaceType.Enet:
                    break;

                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    return SelectAdapterIpManually(handler, true);

                default:
                    return false;
            }

            CustomProgressDialog progress = new CustomProgressDialog(_context);
            progress.SetMessage(_context.GetString(Resource.String.select_enet_ip_search));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();
            SetLock(LockTypeCommunication);

            Thread detectThread = new Thread(() =>
            {
                List<EdInterfaceEnet.EnetConnection> detectedVehicles;
                using (EdInterfaceEnet edInterface = new EdInterfaceEnet(false) { ConnectParameter = new EdInterfaceEnet.ConnectParameterType(_networkData, GenS29Certificate)
                })
                {
                    detectedVehicles = edInterface.DetectedVehicles("auto:all");
                }

                _activity?.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Dismiss();
                        progress = null;
                        SetLock(LockType.None);
                    }
                    AlertDialog.Builder builder = new AlertDialog.Builder(_context);
                    builder.SetTitle(Resource.String.select_enet_ip);
                    ListView listView = new ListView(_context);

                    List<string> interfaceNames = new List<string>
                    {
                        _context.GetString(Resource.String.select_enet_ip_auto)
                    };
                    int selIndex = 0;
                    int index = 0;
                    if (detectedVehicles != null)
                    {
                        foreach (EdInterfaceEnet.EnetConnection enetConnection in detectedVehicles)
                        {
                            if (!string.IsNullOrEmpty(SelectedEnetIp) &&
                                string.Compare(SelectedEnetIp, enetConnection.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                selIndex = index + 1;
                            }

                            interfaceNames.Add(enetConnection.ToString());
                            index++;
                        }
                    }

                    ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                        Android.Resource.Layout.SimpleListItemSingleChoice, interfaceNames.ToArray());
                    listView.Adapter = adapter;
                    listView.ChoiceMode = ChoiceMode.Single;
                    listView.SetItemChecked(selIndex, true);
                    builder.SetView(listView);
                    builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }

                        switch (listView.CheckedItemPosition)
                        {
                            case 0:
                                SelectedEnetIp = null;
                                handler(sender, args);
                                break;

                            default:
                                if (detectedVehicles != null && listView.CheckedItemPosition >= 1 &&
                                    listView.CheckedItemPosition - 1 < detectedVehicles.Count)
                                {
                                    EdInterfaceEnet.EnetConnection enetConnection = detectedVehicles[listView.CheckedItemPosition - 1];
                                    SelectedEnetIp = enetConnection.ToString();
                                    handler(sender, args);
                                }
                                break;
                        }
                    });
                    builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                    {
                    });
                    builder.SetNeutralButton(Resource.String.button_manually, (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }

                        SelectAdapterIpManually(handler);
                    });
                    builder.Show();
                });
            });
            detectThread.Start();
            return true;
        }

        public bool SelectAdapterIpManually(EventHandler<DialogClickEventArgs> handler, bool withPort = false)
        {
            try
            {
                string interfaceIp = SelectedInterfaceIp ?? string.Empty;
                string[] ipParts = interfaceIp.Split(':');
                string ipAddr = "0.0.0.0";
                string ipPort = string.Empty;
                StringBuilder sbInfo = new StringBuilder();

                if (!IsValidWifiConnection(out string localAddress, out string localMask, out string dhcpServerAddress, out _))
                {
                    dhcpServerAddress = null;
                    IsValidEthernetConnection(null, out localAddress, out localMask);
                }

                if (!string.IsNullOrEmpty(dhcpServerAddress))
                {
                    ipAddr = dhcpServerAddress;
                }
                else if (!string.IsNullOrEmpty(localAddress) && !string.IsNullOrEmpty(localMask))
                {
                    try
                    {
                        byte[] addrBytes = IPAddress.Parse(localAddress).GetAddressBytes();
                        byte[] maskBytes = IPAddress.Parse(localMask).GetAddressBytes();
                        if (addrBytes.Length == 4 && maskBytes.Length == 4)
                        {
                            byte[] ipBytes = new byte[addrBytes.Length];
                            addrBytes.CopyTo(ipBytes, 0);
                            for (int i = 0; i < addrBytes.Length; i++)
                            {
                                ipBytes[i] &= maskBytes[i];
                            }

                            if (ipBytes.SequenceEqual(addrBytes))
                            {
                                ipBytes[^1]++;
                            }

                            ipAddr = new IPAddress(ipBytes).ToString();
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (!string.IsNullOrEmpty(localAddress))
                {
                    if (sbInfo.Length > 0)
                    {
                        sbInfo.Append("\r\n");
                    }
                    sbInfo.Append(string.Format("{0}: {1}", _activity.GetString(Resource.String.select_enet_info_ip), localAddress));
                }

                if (!string.IsNullOrEmpty(localMask))
                {
                    if (sbInfo.Length > 0)
                    {
                        sbInfo.Append("\r\n");
                    }
                    sbInfo.Append(string.Format("{0}: {1}", _activity.GetString(Resource.String.select_enet_info_mask), localMask));
                }

                if (!string.IsNullOrEmpty(dhcpServerAddress))
                {
                    if (sbInfo.Length > 0)
                    {
                        sbInfo.Append("\r\n");
                    }
                    sbInfo.Append(string.Format("{0}: {1}", _activity.GetString(Resource.String.select_enet_info_dhcp_srv), dhcpServerAddress));
                }

                if (ipParts.Length > 0)
                {
                    string ip = ipParts[0];
                    if (!string.IsNullOrEmpty(ip))
                    {
                        if (Ipv4RegEx.IsMatch(ip))
                        {
                            ipAddr = ip;
                        }
                    }
                }

                if (ipParts.Length > 1)
                {
                    string port = ipParts[1];
                    if (!string.IsNullOrEmpty(port))
                    {
                        if (int.TryParse(port, out int portValue))
                        {
                            if (portValue >= 1 && portValue <= 0xFFFF)
                            {
                                ipPort = string.Format(CultureInfo.InvariantCulture, "{0}", portValue);
                            }
                        }
                    }
                }

                NumberInputDialog numberInputDialog = new NumberInputDialog(_activity);
                numberInputDialog.Info = sbInfo.ToString();
                numberInputDialog.InfoVisible = sbInfo.Length > 0;

                numberInputDialog.Message1 = _activity.GetString(Resource.String.select_enet_ip_edit);
                numberInputDialog.Digits1 = "0123456789.";
                numberInputDialog.Number1 = ipAddr;
                numberInputDialog.Visible1 = true;

                numberInputDialog.Message2 = _activity.GetString(Resource.String.select_enet_ip_port_edit);
                numberInputDialog.Digits2 = "0123456789";
                numberInputDialog.Number2 = ipPort;
                numberInputDialog.Visible2 = withPort;
                numberInputDialog.SetPositiveButton(Resource.String.button_ok, (s, arg) =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    string interfaceIpResult = null;
                    string ipAddrResult = numberInputDialog.Number1.Trim();
                    if (Ipv4RegEx.IsMatch(ipAddrResult) && IPAddress.TryParse(ipAddrResult, out IPAddress ipAddress))
                    {
                        byte[] ipBytes = ipAddress.GetAddressBytes();
                        if (ipBytes.Length == 4 && ipBytes.Any(x => x != 0))
                        {
                            interfaceIpResult = ipAddress.ToString();
                        }
                    }

                    if (withPort && !string.IsNullOrEmpty(interfaceIpResult))
                    {
                        string ipPortResult = numberInputDialog.Number2.Trim();
                        if (!string.IsNullOrEmpty(ipPortResult))
                        {
                            if (int.TryParse(ipPortResult, out int portValue))
                            {
                                if (portValue >= 1 && portValue <= 0xFFFF)
                                {
                                    interfaceIpResult += string.Format(CultureInfo.InvariantCulture, ":{0}", portValue);
                                }
                            }
                        }
                    }

                    SelectedInterfaceIp = interfaceIpResult;
                    handler(s, arg);
                });
                numberInputDialog.SetNegativeButton(Resource.String.button_reset, (s, arg) =>
                {
                    SelectedInterfaceIp = null;
                    handler(s, arg);
                });
                numberInputDialog.SetNeutralButton(Resource.String.button_abort, (s, arg) =>
                {
                });
                numberInputDialog.Show();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public void RequestUsbPermission(UsbDevice usbDevice)
        {
            if (!UsbSupport)
            {
                return;
            }
            if (_usbPermissionRequested)
            {
                return;
            }
            if (usbDevice == null)
            {
                List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                if (availableDrivers.Count > 0)
                {
                    usbDevice = availableDrivers[0].Device;
                }
            }
            if (usbDevice != null)
            {
                if (_usbManager.HasPermission(usbDevice))
                {
                    _usbPermissionRequestDisabled = true;
                }
                else if (EdFtdiInterface.IsValidUsbDevice(usbDevice, out bool fakeDevice))
                {
                    if (fakeDevice)
                    {
                        if (_ftdiWarningAlertDialog == null)
                        {
                            _ftdiWarningAlertDialog = new AlertDialog.Builder(_context)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.ftdi_fake_device)
                                .SetTitle(Resource.String.alert_title_warning)
                                .Show();
                            if (_ftdiWarningAlertDialog != null)
                            {
                                _ftdiWarningAlertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }
                                    _ftdiWarningAlertDialog = null;
                                };

                            }
                        }
                    }

                    if (!_usbPermissionRequestDisabled)
                    {
                        try
                        {
                            Android.App.PendingIntentFlags intentFlags = 0;
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                            {
                                intentFlags |= Android.App.PendingIntentFlags.Mutable;
                            }
                            Android.App.PendingIntent intent = Android.App.PendingIntent.GetBroadcast(_context, 0, new Intent(UsbPermissionAction), intentFlags);

                            _usbManager.RequestPermission(usbDevice, intent);
                            _usbPermissionRequested = true;
                        }
                        catch (Exception)
                        {
                            // seems to crash on Samsung 5.1.1 with android.permission.sec.MDM_APP_MGMT
                        }
                    }
                }
            }
        }

        public bool ShowBatteryWarning(double? batteryVoltage, byte[] adapterSerial)
        {
            if (adapterSerial != null && adapterSerial.Length > 0)
            {
                LastAdapterSerial = BitConverter.ToString(adapterSerial).Replace("-", "");
            }

            if (!batteryVoltage.HasValue || batteryVoltage.Value < 16.0)
            {
                return false;
            }

            BatteryWarnings++;
            BatteryWarningVoltage = batteryVoltage.Value;

            if (ShowBatteryVoltageWarning && _batteryVoltageAlertDialog == null)
            {
                string voltageText = string.Format(ActivityMain.Culture, "{0,4:0.0}", batteryVoltage.Value);
                string message = string.Format(_activity.GetString(Resource.String.battery_voltage_warn), voltageText);
                _batteryVoltageAlertDialog = new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                    })
                    .SetNegativeButton(Resource.String.button_hide, (sender, args) =>
                    {
                        ShowBatteryVoltageWarning = false;
                    })
                    .SetCancelable(true)
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_warning)
                    .Show();
                if (_batteryVoltageAlertDialog != null)
                {
                    _batteryVoltageAlertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        _batteryVoltageAlertDialog = null;
                    };

                }
                return true;
            }

            return false;
        }

        public LockType GetLock()
        {
            if (_disposed)
            {
                return LockType.None;
            }

            foreach (Tuple<LockType, PowerManager.WakeLock> tempLock in _lockArray)
            {
                try
                {
                    if (tempLock.Item2.IsHeld)
                    {
                        return tempLock.Item1;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return LockType.None;
        }

        public bool SetLock(LockType lockType)
        {
            if (_disposed)
            {
                return false;
            }

            bool result = true;
            PowerManager.WakeLock wakeLock = null;
            switch (lockType)
            {
                case LockType.None:
                    break;

                case LockType.Cpu:
                    wakeLock = _wakeLockCpu;
                    break;

                case LockType.ScreenDim:
                    wakeLock = _wakeLockScreenDim;
                    break;

                case LockType.ScreenBright:
                    wakeLock = _wakeLockScreenBright;
                    break;
            }

            LockType currentLock = GetLock();

            if (wakeLock != null)
            {
                if (currentLock != LockType.None)
                {
                    result = false;
                }
                else
                {
                    try
                    {
                        wakeLock.Acquire();
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                }
            }
            else
            {
                foreach (Tuple<LockType, PowerManager.WakeLock> tempLock in _lockArray)
                {
                    try
                    {
                        if (tempLock.Item2.IsHeld)
                        {
                            tempLock.Item2.Release();
                        }
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool SetClipboardText(string text)
        {
            try
            {
                // ReSharper disable once UsePatternMatching
                ClipboardManager clipboardManagerNew = _clipboardManager as ClipboardManager;
                if (clipboardManagerNew != null)
                {
                    ClipData clipData = ClipData.NewPlainText(@"text", text);
                    if (clipData != null)
                    {
                        clipboardManagerNew.PrimaryClip = clipData;
                    }
                }
                else
                {
#pragma warning disable 618
                    // ReSharper disable once UsePatternMatching
                    Android.Text.ClipboardManager clipboardManagerOld = _clipboardManager as Android.Text.ClipboardManager;
#pragma warning restore 618
                    if (clipboardManagerOld != null)
                    {
                        clipboardManagerOld.Text = text;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetClipboardText()
        {
            try
            {
                string clipText = null;
                // ReSharper disable once UsePatternMatching
                ClipboardManager clipboardManagerNew = _clipboardManager as ClipboardManager;
                if (clipboardManagerNew != null)
                {
                    if (clipboardManagerNew.HasPrimaryClip)
                    {
                        ClipData.Item item = clipboardManagerNew.PrimaryClip.GetItemAt(0);
                        if (item != null)
                        {
                            clipText = clipboardManagerNew.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain) ?
                                item.Text : item.CoerceToText(_context);
                        }
                    }
                }
                else
                {
#pragma warning disable 618
                    Android.Text.ClipboardManager clipboardManagerOld = _clipboardManager as Android.Text.ClipboardManager;
#pragma warning restore 618
                    clipText = clipboardManagerOld?.Text;
                }
                return clipText;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public EdInterfaceBase GetEdiabasInterfaceClass()
        {
            if (SelectedManufacturer != ManufacturerType.Bmw)
            {
                return new EdInterfaceEdic();
            }
            if (SelectedInterface == InterfaceType.Enet)
            {
                return new EdInterfaceEnet();
            }
            return new EdInterfaceObd();
        }

        public bool StartEdiabasThread(InstanceDataCommon instanceData, JobReader.PageInfo pageInfo, EdiabasEventDelegate ediabasEvent)
        {
            if (instanceData == null)
            {
                return false;
            }

            if (pageInfo == null)
            {
                return false;
            }

            if (CommActive)
            {
                return true;
            }

            if (EdiabasThread != null)
            {
                StopEdiabasThread(true, ediabasEvent);
            }

            JobReader jobReader = JobReader;
            lock (EdiabasThread.DataLock)
            {
                if (EdiabasThread == null)
                {
                    string ecuPath = string.IsNullOrEmpty(jobReader.EcuPath) ? instanceData.EcuPath : jobReader.EcuPath;
                    EdiabasThread = new EdiabasThread(ecuPath, this, _context);
                    ediabasEvent?.Invoke(true);
                }
            }

            string logDir = string.Empty;
            if (instanceData.DataLogActive && !string.IsNullOrEmpty(jobReader.LogPath))
            {
                logDir = Path.IsPathRooted(jobReader.LogPath) ? jobReader.LogPath : Path.Combine(instanceData.AppDataPath, jobReader.LogPath);
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch (Exception)
                {
                    logDir = string.Empty;
                }
            }
            instanceData.DataLogDir = logDir;

            instanceData.TraceDir = null;
            if (instanceData.TraceActive && !string.IsNullOrEmpty(instanceData.ConfigFileName))
            {
                instanceData.TraceDir = Path.Combine(instanceData.AppDataPath, LogDir);
            }

            string portName = string.Empty;
            object connectParameter = null;
            switch (SelectedInterface)
            {
                case InterfaceType.Bluetooth:
                    portName = EdBluetoothInterface.PortId + ":" + instanceData.DeviceAddress;
                    connectParameter = new EdBluetoothInterface.ConnectParameterType(NetworkData, MtcBtService, MtcBtEscapeMode,
                        () => EdiabasThread?.ActiveContext);
                    ConnectMtcBtDevice(instanceData.DeviceAddress);
                    break;

                case InterfaceType.Enet:
                    connectParameter = new EdInterfaceEnet.ConnectParameterType(NetworkData, GenS29Certificate);
                    if (Emulator && !string.IsNullOrEmpty(EmulatorEnetIp))
                    {
                        // broadcast is not working with emulator
                        portName = EmulatorEnetIp;
                        break;
                    }
                    portName = string.IsNullOrEmpty(SelectedEnetIp) ? "auto:all" : SelectedEnetIp;
                    break;

                case InterfaceType.ElmWifi:
                    portName = EdElmWifiInterface.PortId;
                    if (!string.IsNullOrEmpty(SelectedElmWifiIp))
                    {
                        portName += ":" + SelectedElmWifiIp;
                    }
                    connectParameter = new EdElmWifiInterface.ConnectParameterType(NetworkData);
                    break;

                case InterfaceType.DeepObdWifi:
                    portName = EdCustomWiFiInterface.PortId;
                    if (!string.IsNullOrEmpty(SelectedDeepObdWifiIp))
                    {
                        portName += ":" + SelectedDeepObdWifiIp;
                    }
                    connectParameter = new EdCustomWiFiInterface.ConnectParameterType(NetworkData, MaWifi);
                    break;

                case InterfaceType.Ftdi:
                    portName = EdFtdiInterface.PortId + "0";
                    connectParameter = new EdFtdiInterface.ConnectParameterType(UsbManager);
                    break;

                case InterfaceType.Simulation:
                    portName = EdInterfaceBase.PortIdSimulation;
                    break;
            }

            EdiabasThread?.StartThread(portName, connectParameter, pageInfo, true, instanceData);

            return true;
        }

        public bool StopEdiabasThread(bool wait, EdiabasEventDelegate ediabasEvent = null)
        {
            if (EdiabasThread != null)
            {
                try
                {
                    lock (EdiabasThread.DataLock)
                    {
                        if (EdiabasThread != null)
                        {
                            EdiabasThread.StopThread(wait);
                        }
                    }
                    if (wait)
                    {
                        StopForegroundService(_context);
                        lock (EdiabasThread.DataLock)
                        {
                            ediabasEvent?.Invoke(false);
                            if (EdiabasThread != null)
                            {
                                EdiabasThread.Dispose();
                                EdiabasThread = null;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        public void SetEdiabasInterface(EdiabasNet ediabas, string btDeviceAddress)
        {
            PackageInfo packageInfo = GetPackageInfo();
            if (packageInfo != null)
            {
                string versionName = packageInfo.VersionName ?? string.Empty;
                long versionCode = PackageInfoCompat.GetLongVersionCode(packageInfo);
                ediabas.LogInfo = string.Format(CultureInfo.InvariantCulture, "App version code: {0}\nApp version name: {1}", versionCode, versionName);
            }

            object connectParameter = null;
            if (ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
            {
                edInterfaceObd.UdsDtcStatusOverride = UdsDtcStatusOverride;
                edInterfaceObd.UdsEcuCanIdOverride = -1;
                edInterfaceObd.UdsTesterCanIdOverride = -1;
                edInterfaceObd.DisabledConceptsList = null;
                switch (SelectedInterface)
                {
                    case InterfaceType.Bluetooth:
                    default:
                        edInterfaceObd.ComPort = EdBluetoothInterface.PortId + ":" + btDeviceAddress;
                        connectParameter = new EdBluetoothInterface.ConnectParameterType(_networkData, MtcBtService, MtcBtEscapeMode, () => _context);
                        ConnectMtcBtDevice(btDeviceAddress);
                        break;

                    case InterfaceType.ElmWifi:
                    {
                        string comPort = EdElmWifiInterface.PortId;
                        if (!string.IsNullOrEmpty(SelectedElmWifiIp))
                        {
                            comPort += ":" + SelectedElmWifiIp;
                        }
                        edInterfaceObd.ComPort = comPort;
                        connectParameter = new EdElmWifiInterface.ConnectParameterType(_networkData);
                        break;
                    }

                    case InterfaceType.DeepObdWifi:
                    {
                        string comPort = EdCustomWiFiInterface.PortId;
                        if (!string.IsNullOrEmpty(SelectedDeepObdWifiIp))
                        {
                            comPort += ":" + SelectedDeepObdWifiIp;
                        }
                        edInterfaceObd.ComPort = comPort;
                        connectParameter = new EdCustomWiFiInterface.ConnectParameterType(_networkData, _maWifi);
                        break;
                    }

                    case InterfaceType.Ftdi:
                        edInterfaceObd.ComPort = EdFtdiInterface.PortId + "0";
                        connectParameter = new EdFtdiInterface.ConnectParameterType(_usbManager);
                        break;

                    case InterfaceType.Simulation:
                        break;
                }
            }
            else if (ediabas.EdInterfaceClass is EdInterfaceEnet edInterfaceEnet)
            {
                string remoteHost = string.IsNullOrEmpty(SelectedEnetIp) ? "auto:all" : SelectedEnetIp;
                if (Emulator && !string.IsNullOrEmpty(EmulatorEnetIp))
                {   // broadcast is not working with emulator
                    remoteHost = EmulatorEnetIp;
                }
                edInterfaceEnet.RemoteHost = remoteHost;
                connectParameter = new EdInterfaceEnet.ConnectParameterType(_networkData, GenS29Certificate);
            }

            ediabas.CloseSgbd();
            ediabas.EdInterfaceClass.ConnectParameter = connectParameter;
        }

        public List<X509CertificateStructure> GenS29Certificate(AsymmetricKeyParameter machinePublicKey, List<X509CertificateStructure> trustedCaCerts, string trustedKeyPath, string vin)
        {
            try
            {
                if (machinePublicKey == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(trustedKeyPath))
                {
                    return null;
                }

                if (!Directory.Exists(trustedKeyPath))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(vin))
                {
                    return null;
                }

                string[] pfxFiles = Directory.GetFiles(trustedKeyPath, "*.pfx", SearchOption.TopDirectoryOnly);
                if (pfxFiles.Length != 1)
                {
                    return null;
                }

                AsymmetricKeyParameter privateKeyResource = EdBcTlsUtilities.LoadPkcs12Key(pfxFiles[0], string.Empty, out X509CertificateEntry[] publicCertificateEntries);
                if (privateKeyResource == null || publicCertificateEntries == null || publicCertificateEntries.Length < 1)
                {
                    return null;
                }

                Org.BouncyCastle.X509.X509Certificate issuerCert = publicCertificateEntries[0].Certificate;
                X509Certificate2 s29Cert = EdSec4Diag.GenerateCertificate(issuerCert, machinePublicKey, privateKeyResource, EdSec4Diag.S29BmwCnName, vin);
                if (s29Cert == null)
                {
                    return null;
                }

                Org.BouncyCastle.X509.X509Certificate x509s29Cert = new X509CertificateParser().ReadCertificate(s29Cert.GetRawCertData());
                s29Cert.Dispose();

                List<X509CertificateStructure> certList = new List<X509CertificateStructure>();
                certList.Add(x509s29Cert.CertificateStructure);

                foreach (X509CertificateEntry certEntry in publicCertificateEntries)
                {
                    certList.Add(certEntry.Certificate.CertificateStructure);
                }

                List<Org.BouncyCastle.X509.X509Certificate> x509CertList = EdBcTlsUtilities.ConvertToX509CertList(certList);
                List<Org.BouncyCastle.X509.X509Certificate> rootCerts = EdBcTlsUtilities.ConvertToX509CertList(trustedCaCerts);
                if (!EdBcTlsUtilities.ValidateCertChain(x509CertList, rootCerts))
                {
                    return null;
                }

                return certList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool SetEdiabasUdsCanId(EdiabasNet ediabas, VehicleInfoVag.EcuAddressEntry ecuAddressEntry = null)
        {
            if (ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
            {
                int oldEcuId = edInterfaceObd.UdsEcuCanIdOverride;
                int oldTesterId = edInterfaceObd.UdsTesterCanIdOverride;

                if (ecuAddressEntry != null)
                {
                    edInterfaceObd.UdsEcuCanIdOverride = (int)ecuAddressEntry.IsoTpEcuCanId;
                    edInterfaceObd.UdsTesterCanIdOverride = (int)ecuAddressEntry.IsoTpTesterCanId;
                }
                else
                {
                    edInterfaceObd.UdsEcuCanIdOverride = -1;
                    edInterfaceObd.UdsTesterCanIdOverride = -1;
                }

                if (oldEcuId != edInterfaceObd.UdsEcuCanIdOverride ||
                    oldTesterId != edInterfaceObd.UdsTesterCanIdOverride)
                {
                    // force reload
                    ediabas.CloseSgbd();
                }

                return true;
            }

            return false;
        }

        public static bool SetEdiabasConfigProperties(EdiabasNet ediabas, string tracePath, string simulationPath, bool appendTrace = false)
        {
            if (ediabas == null)
            {
                return false;
            }

            try
            {
                if (!string.IsNullOrEmpty(tracePath))
                {
                    ediabas.SetConfigProperty("TracePath", tracePath);
                    ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                    ediabas.SetConfigProperty("AppendTrace", appendTrace ? "1" : "0");
                    ediabas.SetConfigProperty("IfhTraceBuffering", IfhTraceBuffering ? "1" : "0");
                    ediabas.SetConfigProperty("CompressTrace", CompressTrace ? "1" : "0");
                }
                else
                {
                    ediabas.SetConfigProperty("IfhTrace", "0");
                }

                if (!string.IsNullOrEmpty(simulationPath))
                {
                    ediabas.SetConfigProperty("Simulation", "1");
                    ediabas.SetConfigProperty("SimulationPath", simulationPath);
                    ediabas.SetConfigProperty("SimulationInterfaces", "OBD,ENET,EDIC,OBD_*,ENET_*,EDIC_*");
                }
                else
                {
                    ediabas.SetConfigProperty("Simulation", "0");
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void ResolveSgbdFile(EdiabasNet ediabas, string fileName)
        {
            if (ediabas == null)
            {
                return;
            }

            bool mapped = false;
            string baseFileName = Path.GetFileName(fileName);
            string sgbdName = baseFileName;
            if (SelectedManufacturer != ManufacturerType.Bmw)
            {
                if (baseFileName != null && baseFileName.StartsWith(XmlToolActivity.VagUdsCommonSgbd + "#", true, CultureInfo.InvariantCulture))
                {
                    string[] nameArray = baseFileName.Split('#');
                    if (nameArray.Length == 2)
                    {
                        sgbdName = nameArray[0];
                        object addressObj = new UInt32Converter().ConvertFromInvariantString(nameArray[1]);
                        if (!(addressObj is UInt32 address))
                        {
                            ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ResolveSgbdFile invalid number for: {0} ", baseFileName);
                            // ReSharper disable once NotResolvedInText
                            throw new ArgumentOutOfRangeException("ResolveSgbdFile", "Parsing address failed");
                        }
                        VehicleInfoVag.EcuAddressEntry ecuAddressEntry = VehicleInfoVag.GetAddressEntry(address);
                        if (ecuAddressEntry == null)
                        {
                            ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ResolveSgbdFile no address entry for: {0} ", baseFileName);
                            // ReSharper disable once NotResolvedInText
                            throw new ArgumentOutOfRangeException("ResolveSgbdFile", "No address entry found");
                        }
                        ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Mapped address {0:X02} to UDS CAN IDs: {1:X03}, {2:X03}",
                            address, ecuAddressEntry.IsoTpEcuCanId, ecuAddressEntry.IsoTpTesterCanId);

                        SetEdiabasUdsCanId(ediabas, ecuAddressEntry);
                        mapped = true;
                    }
                }
            }

            if (!mapped)
            {
                SetEdiabasUdsCanId(ediabas);
            }

            ediabas.ResolveSgbdFile(sgbdName);
        }

        public static string FormatResult(EdiabasNet ediabas, JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict, out Android.Graphics.Color? textColor, out double? dataValue)
        {
            textColor = null;
            dataValue = null;
            if (pageInfo == null)
            {
                return string.Empty;
            }

            MethodInfo formatResult = null;
            MethodInfo formatResultColor = null;
            MethodInfo formatResultMulti = null;
            MethodInfo formatResultValue = null;
            MethodInfo formatResultOverride = null;
            if (pageInfo.ClassObject != null)
            {
                Type pageType = pageInfo.ClassObject.GetType();
                formatResult = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(string) });
                formatResultColor = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType() });
                formatResultMulti = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType() });
                formatResultValue = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType(), typeof(double?).MakeByRefType() });
                formatResultOverride = pageType.GetMethod("FormatResult", new[] { typeof(EdiabasNet), typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(string), typeof(string).MakeByRefType(), typeof(Android.Graphics.Color?).MakeByRefType(), typeof(double?).MakeByRefType() });
            }

            string result = string.Empty;
            if (displayInfo.Format == null)
            {
                if (resultDict != null)
                {
                    try
                    {
                        if (formatResultValue != null)
                        {
                            object[] args = { pageInfo, resultDict, displayInfo.Result, null, null };
                            result = formatResultValue.Invoke(pageInfo.ClassObject, args) as string;
                            textColor = args[3] as Android.Graphics.Color?;
                            dataValue = args[4] as double?;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict, displayInfo.Result, ref textColor);
                        }
                        else if (formatResultMulti != null)
                        {
                            object[] args = { pageInfo, resultDict, displayInfo.Result, null };
                            result = formatResultMulti.Invoke(pageInfo.ClassObject, args) as string;
                            textColor = args[3] as Android.Graphics.Color?;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict, displayInfo.Result, ref textColor);
                        }
                        else if (formatResultColor != null)
                        {
                            object[] args = { pageInfo, resultDict.ToDictionary(), displayInfo.Result, null };
                            result = formatResultColor.Invoke(pageInfo.ClassObject, args) as string;
                            textColor = args[3] as Android.Graphics.Color?;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict.ToDictionary(), displayInfo.Result, ref textColor);
                        }
                        else if (formatResult != null)
                        {
                            object[] args = { pageInfo, resultDict.ToDictionary(), displayInfo.Result };
                            result = formatResult.Invoke(pageInfo.ClassObject, args) as string;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict.ToDictionary(), displayInfo.Result);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            else
            {
                result = FormatResultEdiabas(resultDict, displayInfo.Result, displayInfo.Format);
            }

            try
            {
                if (formatResultOverride != null)
                {
                    object[] args = { ediabas, pageInfo, resultDict, displayInfo.Result, result, null, null };
                    // ReSharper disable once UsePatternMatching
                    bool? valid = formatResultOverride.Invoke(pageInfo.ClassObject, args) as bool?;
                    if (valid.HasValue && valid.Value)
                    {
                        result = args[4] as string;
                        textColor = args[5] as Android.Graphics.Color?;
                        dataValue = args[6] as double?;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return result;
        }

        public static String FormatResultEdiabas(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            StringBuilder sbResult = new StringBuilder();
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    string result;
                    if (resultData.OpData.GetType() == typeof(byte[]))
                    {
                        StringBuilder sb = new StringBuilder();
                        byte[] data = (byte[])resultData.OpData;
                        foreach (byte value in data)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02} ", value));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = EdiabasNet.FormatResult(resultData, format) ?? "?";
                    }
                    if (sbResult.Length > 0)
                    {
                        sbResult.Append("\r\n");
                    }
                    sbResult.Append(result);
                }
            }
            return sbResult.ToString();
        }

        public static String FormatResultEcuFunction(JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict,
            out double? dataValue)
        {
            dataValue = null;
            if (string.IsNullOrWhiteSpace(displayInfo.EcuJobId) || string.IsNullOrWhiteSpace(displayInfo.EcuJobResultId))
            {
                return string.Empty;
            }

            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(displayInfo.Result.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    if (resultData is EdiabasThread.EcuFunctionResult ecuFunctionResult)
                    {
                        if (ecuFunctionResult.EcuJob != null && ecuFunctionResult.EcuJob.IdPresent(displayInfo.EcuJobId, pageInfo.UseCompatIds) &&
                            ecuFunctionResult.EcuJobResult != null && ecuFunctionResult.EcuJobResult.IdPresent(displayInfo.EcuJobResultId, pageInfo.UseCompatIds))
                        {
                            dataValue = ecuFunctionResult.ResultValue;
                            return ecuFunctionResult.ResultString;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static String FormatResultVagUds(string udsFileName, JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict,
            out double? dataValue)
        {
            dataValue = null;
            if (!VagUdsActive)
            {
                return string.Empty;
            }

            string[] displayParts = displayInfo.Name.Split('#');
            if (displayParts.Length != 3)
            {
                return string.Empty;
            }

            string uniqueIdString = displayParts[displayParts.Length - 1];
            UdsReader udsReader = GetUdsReader(udsFileName);
            UdsReader.ParseInfoMwb parseInfoMwb = udsReader?.GetMwbParseInfo(udsFileName, uniqueIdString);
            if (parseInfoMwb == null)
            {
                return string.Empty;
            }

            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(displayInfo.Result.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    if (resultData.OpData.GetType() == typeof(byte[]))
                    {
                        string resultText = parseInfoMwb.DataTypeEntry.ToString(CultureInfo.InvariantCulture, (byte[])resultData.OpData, out double? stringDataValue);
                        if (stringDataValue.HasValue && !string.IsNullOrEmpty(displayInfo.Format))
                        {
                            dataValue = stringDataValue;
                            resultText = EdiabasNet.FormatResult(new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeR, displayInfo.Result, stringDataValue.Value), displayInfo.Format) ?? string.Empty;
                        }
                        return resultText;
                    }
                }
            }

            return string.Empty;
        }

        public static bool IsTraceFilePresent(string traceDir)
        {
            if (CollectDebugInfo)
            {
                return true;
            }
            if (string.IsNullOrEmpty(traceDir))
            {
                return false;
            }
            string traceFile = Path.Combine(traceDir, TraceFileName);
            return File.Exists(traceFile);
        }

        public static bool IsValidSimDir(string simulationDir)
        {
            try
            {
                if (string.IsNullOrEmpty(simulationDir))
                {
                    return false;
                }

                if (!Directory.Exists(simulationDir))
                {
                    return false;
                }

                string[] simFiles = Directory.GetFiles(simulationDir, "*.sim", SearchOption.TopDirectoryOnly);
                if (simFiles.Length == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
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

        public PackageInfo GetPackageInfo(PackageInfoFlags infoFlags = 0)
        {
            return GetPackageInfo(_packageManager, _context?.PackageName, infoFlags);
        }

        public long GetVersionCode()
        {
            PackageInfo packageInfo = GetPackageInfo();
            return packageInfo != null ? PackageInfoCompat.GetLongVersionCode(packageInfo) : 0;
        }

        public ApplicationInfo GetApplicationInfo()
        {
            PackageInfo packageInfo = GetPackageInfo();
            return packageInfo?.ApplicationInfo;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public bool IsAppStorageLocationInternal()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return true;
            }

            ApplicationInfo applicationInfo = GetApplicationInfo();
            if (applicationInfo?.StorageUuid == null)
            {
                return false;
            }

            if (!applicationInfo.StorageUuid.Equals(StorageManager.UuidDefault))
            {
                return false;
            }

            return true;
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

        public string GetInstallerPackageName()
        {
            return GetInstallerPackageName(_packageManager, _context?.PackageName);
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

        public IList<PackageInfo> GetInstalledPackages(PackageInfoFlags infoFlags = 0)
        {
            return GetInstalledPackages(_packageManager, infoFlags);
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

        public IList<ApplicationInfo> GetInstalledApplications(PackageInfoFlags infoFlags = 0)
        {
            return GetInstalledApplications(_packageManager, infoFlags);
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

        public ActivityInfo GetActivityInfo(PackageInfoFlags infoFlags = 0)
        {
            return GetActivityInfo(_packageManager, _activity?.ComponentName, infoFlags);
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

        public IList<ResolveInfo> QueryIntentActivities(Intent intent, PackageInfoFlags infoFlags = 0)
        {
            return QueryIntentActivities(_packageManager, intent, infoFlags);
        }

        public string[] RetrievePermissions()
        {
            try
            {
                PackageInfo packageInfo = GetPackageInfo(PackageInfoFlags.Permissions);
                if (packageInfo?.RequestedPermissions != null)
                {
                    return packageInfo.RequestedPermissions.ToArray();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public string GetCertificateInfo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                Signature[] signatures;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                {
                    PackageInfo packageInfo = GetPackageInfo(PackageInfoFlags.SigningCertificates);
                    signatures = packageInfo?.SigningInfo?.GetApkContentsSigners();
                }
                else
                {
                    PackageInfo packageInfo = GetPackageInfo(PackageInfoFlags.Signatures);
#pragma warning disable 618
#pragma warning disable CA1422
                    signatures = packageInfo?.Signatures?.ToArray();
#pragma warning restore CA1422
#pragma warning restore 618
                }
                if (signatures != null)
                {
                    foreach (Signature signature in signatures)
                    {
                        try
                        {
                            byte[] signatureArray = signature.ToByteArray();
                            using (MD5 md5 = MD5.Create())
                            {
                                string hash = BitConverter.ToString(md5.ComputeHash(signatureArray)).Replace("-", "");
                                if (sb.Length > 0)
                                {
                                    sb.Append("\n");
                                }
                                sb.Append("Hash: ");
                                sb.Append(hash);
                            }

                            using (Stream certStream = new MemoryStream(signatureArray))
                            {
                                using (Java.Security.Cert.CertificateFactory certFactory = Java.Security.Cert.CertificateFactory.GetInstance("X.509"))
                                {
                                    using (Java.Security.Cert.X509Certificate x509Cert = (Java.Security.Cert.X509Certificate)certFactory.GenerateCertificate(certStream))
                                    {
                                        sb.Append("\n");
                                        sb.Append("Subject: ");
                                        sb.Append(x509Cert.SubjectDN.ToString());

                                        sb.Append("\n");
                                        sb.Append("Issuer: ");
                                        sb.Append(x509Cert.IssuerDN.ToString());

                                        sb.Append("\n");
                                        sb.Append("Serial: ");
                                        sb.Append(x509Cert.SerialNumber);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                return sb.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool RequestSendTraceFile(string appDataDir, string traceDir, Type classType, EventHandler<EventArgs> handler = null)
        {
            try
            {
                string traceFile = Path.Combine(traceDir, TraceFileName);
                if (!File.Exists(traceFile))
                {
                    return false;
                }
                FileInfo fileInfo = new FileInfo(traceFile);
                string message = string.Format(new FileSizeFormatProvider(), _context.GetString(Resource.String.send_trace_file_request), fileInfo.Length);
                new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        SendTraceFileInfoDlg(appDataDir, traceFile, null, classType, handler, true);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                        handler?.Invoke(this, new EventArgs());
                    })
                    .SetCancelable(true)
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SendTraceFile(string appDataDir, string traceDir, Type classType, EventHandler<EventArgs> handler = null)
        {
            try
            {
                string message = null;
                string traceFile = null;
                if (!string.IsNullOrEmpty(traceDir))
                {
                    traceFile = Path.Combine(traceDir, TraceFileName);
                }
                if (string.IsNullOrEmpty(traceFile) ||!File.Exists(traceFile))
                {
                    traceFile = null;
                    message = "No Trace file";
                }
                SendTraceFileInfoDlg(appDataDir, traceFile, message, classType, handler, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool RequestSendMessage(string appDataDir, string message, Type classType, EventHandler<EventArgs> handler = null)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return false;
                }
                new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        SendTraceFileInfoDlg(appDataDir, null, message, classType, handler);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                        handler?.Invoke(this, new EventArgs());
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.send_message_request)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool StoreBackupTraceFile(string appDataDir, string traceFile, string message)
        {
            try
            {
                bool storeMessage = false;
                if (!File.Exists(traceFile))
                {
                    if (string.IsNullOrEmpty(message))
                    {
                        return false;
                    }

                    storeMessage = true;
                }

                if (!storeMessage)
                {
                    if (IsBackupTraceFile(appDataDir, traceFile))
                    {
                        return false;
                    }
                }

                string traceBackupDir = Path.Combine(appDataDir, TraceBackupDir);
                try
                {
                    Directory.CreateDirectory(traceBackupDir);
                }
                catch (Exception)
                {
                    // ignored
                }

                string traceBackupFile = Path.Combine(traceBackupDir, TraceFileName);
                if (storeMessage)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (StreamWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(true), 1024, true))
                        {
                            streamWriter.Write(message);
                        }

                        ms.Position = 0;
                        if (CompressTrace)
                        {
                            if (!CreateZipFile(ms, TraceFileName, traceBackupFile))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            using (FileStream fileStream = new FileStream(traceBackupFile, FileMode.Create, FileAccess.Write))
                            {
                                ms.CopyTo(fileStream);
                            }
                        }
                    }
                }
                else
                {
                    File.Copy(traceFile, traceBackupFile, true);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool IsBackupTraceFile(string appDataDir, string traceFile)
        {
            try
            {
                string traceBackupDir = Path.Combine(appDataDir, TraceBackupDir);
                string traceFileDir = Path.GetDirectoryName(traceFile);
                if (string.IsNullOrEmpty(traceFileDir))
                {
                    return false;
                }

                if (string.Compare(traceFileDir, traceBackupDir, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool DeleteBackupTraceFile(string appDataDir)
        {
            try
            {
                string traceBackupDir = Path.Combine(appDataDir, TraceBackupDir);
                string traceBackupFile = Path.Combine(traceBackupDir, TraceFileName);
                if (!File.Exists(traceBackupFile))
                {
                    return false;
                }

                File.Delete(traceBackupFile);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public string CompressTraceFile(string traceFile, out bool compressFailure)
        {
            compressFailure = false;
            try
            {
                if (!File.Exists(traceFile))
                {
                    return null;
                }

                string fileExt = Path.GetExtension(traceFile);
                if (!string.IsNullOrEmpty(fileExt))
                {
                    if (string.Compare(fileExt, ZipExt, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return null;
                    }
                }

                string compressedTraceFile = traceFile + ZipExt;
                if (!CreateZipFile(new[] { traceFile }, compressedTraceFile, null))
                {
                    compressFailure = true;
                    return null;
                }

                return compressedTraceFile;
            }
            catch (Exception)
            {
                compressFailure = true;
            }

            return null;
        }

        public bool SendTraceFileInfoDlg(string appDataDir, string traceFile, string message, Type classType, EventHandler<EventArgs> handler, bool deleteFile = false)
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                TraceInfoInputDialog traceInfoInputDialog = new TraceInfoInputDialog(_context);
                traceInfoInputDialog.EmailAddress = EmailAddress;
                traceInfoInputDialog.InfoText = TraceInfo;
                traceInfoInputDialog.SetPositiveButton(Resource.String.button_yes, (s, arg) =>
                {
                    EmailAddress = traceInfoInputDialog.EmailAddress;
                    TraceInfo = traceInfoInputDialog.InfoText;
                    SendTraceFile(appDataDir, traceFile, message, true, classType, handler, deleteFile);
                });
                traceInfoInputDialog.SetNegativeButton(Resource.String.button_no, (s, arg) =>
                {
                    SendTraceFile(appDataDir, traceFile, message, false, classType, handler, deleteFile);
                });
                traceInfoInputDialog.SetNeutralButton(Resource.String.button_abort, (s, arg) =>
                {
                    handler?.Invoke(this, new EventArgs());
                });

                AlertDialog alertDialog = traceInfoInputDialog.Create();
                alertDialog.Show();
                alertDialog.Window?.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SendTraceFile(string appDataDir, string traceFile, string message, bool optionalInfo, Type classType, EventHandler<EventArgs> handler, bool deleteFile = false)
        {
            if ((string.IsNullOrEmpty(traceFile) || !File.Exists(traceFile)) &&
                string.IsNullOrEmpty(message))
            {
                return false;
            }

            if (!IsBackupTraceFile(appDataDir, traceFile))
            {
                DeleteBackupTraceFile(appDataDir);
            }

            if (_sendHttpClient == null)
            {
                _sendHttpClient = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = DefaultSslProtocols,
                    ServerCertificateCustomValidationCallback = (msg, certificate2, arg3, arg4) => true,
                    Proxy = GetProxySettings()
                });
            }

            PackageInfo packageInfo = GetPackageInfo();
            string certInfo = GetCertificateInfo();

            CustomProgressDialog progress = new CustomProgressDialog(_context);
            progress.SetMessage(_context.GetString(Resource.String.send_trace_file));
            progress.ButtonAbort.Enabled = false;
            progress.Show();
            SetLock(LockTypeCommunication);
            SetPreferredNetworkInterface();

            Thread sendThread = new Thread(() =>
            {
                string errorMessage = null;
                try
                {
                    bool cancelledClicked = false;

                    MultipartFormDataContent formDownload = new MultipartFormDataContent();

                    if (string.Compare(Path.GetExtension(MailInfoDownloadUrl), ".php", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        formDownload.Add(new StringContent(AppId), "appid");
                        formDownload.Add(new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", VersionCode)), "appver");
                        formDownload.Add(new StringContent(GetCurrentLanguage()), "lang");
                        formDownload.Add(new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", (long) Build.VERSION.SdkInt )), "android_ver");
                        formDownload.Add(new StringContent(Build.Fingerprint), "fingerprint");
                        formDownload.Add(new StringContent(SelectedInterface.ToDescriptionString()), "interface_type");
                        formDownload.Add(new StringContent(LastAdapterSerial ?? string.Empty), "adapter_serial");

                        if (!string.IsNullOrEmpty(certInfo))
                        {
                            formDownload.Add(new StringContent(certInfo), "cert");
                        }
                    }

                    System.Threading.Tasks.Task<HttpResponseMessage> taskDownload = _sendHttpClient.PostAsync(MailInfoDownloadUrl, formDownload);

                    CustomProgressDialog progressLocal = progress;
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }

                        if (progressLocal != null)
                        {
                            progressLocal.AbortClick = sender =>
                            {
                                try
                                {
                                    _sendHttpClient.CancelPendingRequests();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            };
                            progressLocal.ButtonAbort.Enabled = true;
                        }
                    });

                    HttpResponseMessage responseDownload = taskDownload.Result;
                    responseDownload.EnsureSuccessStatusCode();
                    string responseDownloadXml = responseDownload.Content.ReadAsStringAsync().Result;

                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }

                        if (progressLocal != null)
                        {
                            progressLocal.ButtonAbort.Enabled = false;
                        }
                    });

                    errorMessage = GetMailErrorMessage(responseDownloadXml, out string adapterBlacklistNew);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        throw new Exception("Error message present");
                    }
                    if (!GetMailKeyWordsInfo(responseDownloadXml, out string wordRegEx, out int maxWords))
                    {
                        throw new Exception("Invalid mail keywords info");
                    }
                    if (!GetMailLinesInfo(responseDownloadXml, out string linesRegEx))
                    {
                        throw new Exception("Invalid mail line info");
                    }

                    List<SerialInfoEntry> serialInfoListOld = GetSerialInfoList();
                    UpdateSerialInfo(responseDownloadXml);
                    string adapterBlacklistOld = AdapterBlacklist;
                    AdapterBlacklist = adapterBlacklistNew ?? string.Empty;

                    string installer = string.Empty;
                    try
                    {
                      installer = GetInstallerPackageName();
                    }
                    catch (Exception)
                    {
                      // ignored
                    }

                    string soArch = string.Empty;
                    try
                    {
                        soArch = Java.Lang.JavaSystem.GetProperty("os.arch");
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Deep OBD Trace info");
                    sb.Append(string.Format("\nDate: {0:u}", DateTime.Now));
                    sb.Append(string.Format("\nLanguage: {0}", GetCurrentLanguage()));
                    sb.Append(string.Format("\nAndroid version: {0}", (long) Build.VERSION.SdkInt));
                    sb.Append(string.Format("\nAndroid id: {0}", Build.Id ?? string.Empty));
                    sb.Append(string.Format("\nAndroid display: {0}", Build.Display ?? string.Empty));
                    sb.Append(string.Format("\nAndroid fingerprint: {0}", Build.Fingerprint ?? string.Empty));
                    sb.Append(string.Format("\nAndroid type: {0}", Build.Type ?? string.Empty));
                    sb.Append(string.Format("\nAndroid tags: {0}", Build.Tags ?? string.Empty));
                    sb.Append(string.Format("\nAndroid manufacturer: {0}", Build.Manufacturer ?? string.Empty));
                    sb.Append(string.Format("\nAndroid model: {0}", Build.Model ?? string.Empty));
                    sb.Append(string.Format("\nAndroid product: {0}", Build.Product ?? string.Empty));
                    sb.Append(string.Format("\nAndroid device: {0}", Build.Device ?? string.Empty));
                    sb.Append(string.Format("\nAndroid board: {0}", Build.Board ?? string.Empty));
                    sb.Append(string.Format("\nAndroid brand: {0}", Build.Brand ?? string.Empty));
                    sb.Append(string.Format("\nAndroid hardware: {0}", Build.Hardware ?? string.Empty));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        sb.Append(string.Format("\nAndroid abi: {0}", Build.SupportedAbis.Count > 0 ? Build.SupportedAbis[0] : string.Empty));
                    }

                    sb.Append(string.Format("\nOS arch: {0}", soArch ?? string.Empty));
                    sb.Append(string.Format("\nAndroid user: {0}", Build.User ?? string.Empty));
                    sb.Append(string.Format("\nApp version name: {0}", packageInfo?.VersionName ?? string.Empty));
                    sb.Append(string.Format("\nApp version code: {0}", VersionCode));
                    sb.Append(string.Format("\nApp id: {0}", AppId));
                    sb.Append(string.Format("\nInstaller: {0}", installer ?? string.Empty));
                    sb.Append(string.Format("\nEnable translation: {0}", EnableTranslation));
                    sb.Append(string.Format("\nManufacturer: {0}", ManufacturerName()));
                    sb.Append(string.Format("\nClass name: {0}", classType.FullName));

                    bool? validProxyHost = HasValidProxyHost();
                    if (validProxyHost != null)
                    {
                        sb.Append(string.Format("\nValid proxy: {0}", validProxyHost.Value));
                    }

                    if (optionalInfo)
                    {
                        if (!string.IsNullOrWhiteSpace(EmailAddress))
                        {
                            sb.Append(string.Format("\nEmail address: {0}", EmailAddress.Trim()));
                        }
                        if (!string.IsNullOrEmpty(TraceInfo))
                        {
                            string infoText = TraceInfo.Replace("\n", " ").Replace("\r", "").Trim();
                            if (!string.IsNullOrWhiteSpace(infoText))
                            {
                                sb.Append(string.Format("\nAdditional info: '{0}'", infoText));
                            }
                        }
                    }

                    foreach (SerialInfoEntry serialInfo in serialInfoListOld)
                    {
                        sb.Append(string.Format("\nSerial info: Serial={0}, Oem={1}, Disabled={2}, Valid={3}", serialInfo.Serial, serialInfo.Oem, serialInfo.Disabled, serialInfo.Valid));
                    }

                    if (!string.IsNullOrEmpty(adapterBlacklistOld))
                    {
                        sb.Append(string.Format("\nAdapter blacklist old: {0}", adapterBlacklistOld));
                    }
                    if (!string.IsNullOrEmpty(adapterBlacklistNew))
                    {
                        sb.Append(string.Format("\nAdapter blacklist new: {0}", adapterBlacklistNew));
                    }
                    if (!string.IsNullOrEmpty(LastAdapterSerial))
                    {
                        sb.Append(string.Format("\nAdapter serial: {0}", LastAdapterSerial));
                    }
                    if (BatteryWarnings > 0)
                    {
                        sb.Append(string.Format("\nBattery warnings: {0}", BatteryWarnings));
                        sb.Append(string.Format("\nBattery warning voltage: {0}", BatteryWarningVoltage));
                    }

                    if (MtcBtService)
                    {
                        if (MtcBtServiceBound)
                        {
                            sb.Append(string.Format("\nMTC API Version: {0}", _mtcServiceConnection.ApiVersion));
                            try
                            {
                                string btModuleName = _mtcServiceConnection.CarManagerGetBtModuleName();
                                if (!string.IsNullOrEmpty(btModuleName))
                                {
                                    sb.Append(string.Format("\nMTC BT module name: {0}", btModuleName));
                                }

                                string mcuVersion = _mtcServiceConnection.CarManagerGetMcuVersion();
                                if (!string.IsNullOrEmpty(mcuVersion))
                                {
                                    sb.Append(string.Format("\nMTC MCU version: {0}", mcuVersion));
                                }

                                string mcuDate = _mtcServiceConnection.CarManagerGetMcuDate();
                                if (!string.IsNullOrEmpty(mcuDate))
                                {
                                    sb.Append(string.Format("\nMTC MCU date: {0}", mcuDate));
                                }

                                IList<string> matchList = _mtcServiceConnection.GetMatchList();
                                foreach (string device in matchList)
                                {
                                    sb.Append(string.Format("\nMTC match device: {0}", device));
                                }

                                IList<string> deviceList = _mtcServiceConnection.GetDeviceList();
                                foreach (string device in deviceList)
                                {
                                    sb.Append(string.Format("\nMTC found device: {0}", device));
                                }

                                if (CollectDebugInfo)
                                {
                                    // getHistoryList
                                    int lastCommand = _mtcServiceConnection.ApiVersion >= 3 ? 44 + _mtcServiceConnection.ApiOffset : 35;
                                    for (int command = 1; command <= lastCommand; command++)
                                    {
                                        if (command >= lastCommand - 15 && command <= lastCommand - 3)
                                        {   // ignore setModuleName -> syncMatchList
                                            continue;
                                        }
                                        sb.Append(string.Format("\nMTC command test: {0} =", command));
                                        byte[] dataArray = _mtcServiceConnection.CommandTest(command);
                                        if (dataArray != null)
                                        {
                                            foreach (byte value in dataArray)
                                            {
                                                sb.Append(string.Format(" {0:X02}", value));
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        else
                        {
                            sb.Append("\nMTC service not bound");
                        }
                    }

                    try
                    {
                        StringBuilder sbProps = new StringBuilder();
                        Java.Util.Properties sysProps = Java.Lang.JavaSystem.Properties;
                        if (sysProps != null)
                        {
                            foreach (string propKey in sysProps.StringPropertyNames())
                            {
                                if (!propKey.StartsWith("java.", StringComparison.OrdinalIgnoreCase) &&
                                    !propKey.StartsWith("line.", StringComparison.OrdinalIgnoreCase))
                                {
                                    string propValue = sysProps.GetProperty(propKey);
                                    if (!string.IsNullOrWhiteSpace(propValue))
                                    {
                                        sbProps.Append("\n- '");
                                        sbProps.Append(propKey);
                                        sbProps.Append("': '");
                                        sbProps.Append(propValue);
                                        sbProps.Append("'");
                                    }
                                }
                            }

                            if (sbProps.Length > 0)
                            {
                                sb.Append("\nSystem properties:");
                                sb.Append(sbProps);
                                sb.Append("\n");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        StringBuilder sbPackages = new StringBuilder();
#pragma warning disable CA1416 // Plattformkompatibilit�t �berpr�fen
                        IList<PackageInfo> installedPackages = GetInstalledPackages(PackageInfoFlags.MatchSystemOnly);
#pragma warning restore CA1416 // Plattformkompatibilit�t �berpr�fen
                        if (installedPackages != null)
                        {
                            foreach (PackageInfo packageInfoLocal in installedPackages)
                            {
                                ApplicationInfo appInfo = packageInfoLocal.ApplicationInfo;
                                if (appInfo != null)
                                {
                                    string sourceDir = appInfo.PublicSourceDir;
                                    string packageName = appInfo.PackageName;
                                    if (!string.IsNullOrEmpty(sourceDir) && !string.IsNullOrEmpty(packageName) &&
                                        (CollectDebugInfo ||
                                         packageName.Contains("microntek", StringComparison.OrdinalIgnoreCase) ||
                                         packageName.Contains("hct", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        string fileName = Path.GetFileName(sourceDir);
                                        if (!string.IsNullOrEmpty(fileName))
                                        {
                                            sbPackages.Append("\n- '");
                                            sbPackages.Append(appInfo.PackageName);
                                            sbPackages.Append("': '");
                                            sbPackages.Append(fileName);
                                            sbPackages.Append("'");
                                        }
                                    }
                                }
                            }

                            if (sbPackages.Length > 0)
                            {
                                sb.Append("\nSystem packages:");
                                sb.Append(sbPackages);
                                sb.Append("\n");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    string sendTraceFile = traceFile;
                    string compressedTraceFile = CompressTraceFile(traceFile, out bool compressFailure);
                    if (compressFailure)
                    {
                        sb.Append("\nCompressing trace file failed!");
                        sendTraceFile = string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(compressedTraceFile))
                    {
                        sb.Append("\nTrace file compressed");
                        sendTraceFile = compressedTraceFile;
                    }

                    if (!string.IsNullOrEmpty(sendTraceFile))
                    {
                        Dictionary<string, int> wordDict = ExtractKeyWords(sendTraceFile, wordRegEx, maxWords, linesRegEx, null);
                        if (wordDict != null)
                        {
                            sb.Append("\nKeywords:");
                            foreach (var entry in wordDict)
                            {
                                sb.Append(" \"");
                                sb.Append(entry.Key);
                                sb.Append("\"=");
                                sb.Append(entry.Value);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(message))
                    {
                        sb.Append("\nInformation:\n");
                        sb.Append(message);
                    }

                    bool infoResult = GetMailInfo(responseDownloadXml, out string dbId, out string commitId, out string mailHost, out int mailPort, out bool mailSsl,
                        out string mailFrom, out string mailTo, out string mailUser, out string mailPassword);

                    if (!infoResult)
                    {
                        throw new Exception("Invalid mail info");
                    }

                    if (dbId != null)
                    {
                        MultipartFormDataContent formUpload = new MultipartFormDataContent
                        {
                            { new StringContent(dbId), "db_id" },
                            { new StringContent(commitId ?? string.Empty), "commit_id" },
                            { new StringContent(sb.ToString()), "info_text" }
                        };

                        if (!string.IsNullOrEmpty(sendTraceFile) && File.Exists(sendTraceFile))
                        {
                            FileStream fileStream = new FileStream(sendTraceFile, FileMode.Open);
                            formUpload.Add(new StreamContent(fileStream), "file", Path.GetFileName(sendTraceFile));
                        }

                        System.Threading.Tasks.Task<HttpResponseMessage> taskUpload = _sendHttpClient.PostAsync(MailInfoDownloadUrl, formUpload);

                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }

                            if (progressLocal != null)
                            {
                                progressLocal.ButtonAbort.Enabled = true;
                            }
                        });

                        HttpResponseMessage responseUpload = taskUpload.Result;
                        responseUpload.EnsureSuccessStatusCode();
                        string responseUploadXml = responseUpload.Content.ReadAsStringAsync().Result;

                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }

                            if (progressLocal != null)
                            {
                                progressLocal.ButtonAbort.Enabled = false;
                            }
                        });

                        errorMessage = GetMailErrorMessage(responseUploadXml, out string _);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            throw new Exception("Error message present");
                        }

                        if (progress != null)
                        {
                            progress.Dismiss();
                            progress = null;
                            SetLock(LockType.None);
                        }

                        if (deleteFile)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(traceFile))
                                {
                                    File.Delete(traceFile);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }

                            handler?.Invoke(this, new EventArgs());
                        });
                    }
                    else
                    {
                        MailMessage mail = new MailMessage()
                        {
                            Subject = "Deep OBD trace info",
                            BodyEncoding = Encoding.UTF8
                        };

                        if (!string.IsNullOrEmpty(traceFile) && File.Exists(traceFile))
                        {
                            mail.Attachments.Add(new Attachment(traceFile));
                        }

                        mail.Body = sb.ToString();

                        using (ManualResetEvent finishEvent = new ManualResetEvent(false))
                        {
#pragma warning disable 618
                            using (SmtpClient smtpClient = new SmtpClient
#pragma warning restore 618
                            {
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                            })
                            {
                                smtpClient.Host = mailHost;
                                smtpClient.Port = mailPort;
                                smtpClient.EnableSsl = mailSsl;
                                smtpClient.UseDefaultCredentials = false;
                                if (string.IsNullOrEmpty(mailUser) || string.IsNullOrEmpty(mailPassword))
                                {
                                    smtpClient.Credentials = null;
                                }
                                else
                                {
                                    smtpClient.Credentials = new NetworkCredential(mailUser, mailPassword);
                                }
                                mail.From = new MailAddress(mailFrom);
                                mail.To.Clear();
                                mail.To.Add(new MailAddress(mailTo));

                                ManualResetEvent finishEventLocal = finishEvent;
                                smtpClient.SendCompleted += (s, e) =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }
                                    _activity?.RunOnUiThread(() =>
                                    {
                                        if (_disposed)
                                        {
                                            return;
                                        }
                                        if (progress != null)
                                        {
                                            progress.Dismiss();
                                            progress = null;
                                            SetLock(LockType.None);
                                        }

                                        finishEventLocal.Set();

                                        if (e.Cancelled || cancelledClicked)
                                        {
                                            return;
                                        }
                                        if (e.Error != null)
                                        {
                                            new AlertDialog.Builder(_context)
                                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                                {
                                                    SendTraceFileInfoDlg(appDataDir, traceFile, message, classType, handler, deleteFile);
                                                })
                                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                                {
                                                })
                                                .SetCancelable(true)
                                                .SetMessage(Resource.String.send_trace_file_failed_retry)
                                                .SetTitle(Resource.String.alert_title_error)
                                                .Show();
                                            return;
                                        }
                                        if (deleteFile)
                                        {
                                            try
                                            {
                                                if (!string.IsNullOrEmpty(traceFile))
                                                {
                                                    File.Delete(traceFile);
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }
                                        }
                                        handler?.Invoke(this, new EventArgs());
                                    });
                                };

#pragma warning disable 618
                                SmtpClient smtpClientLocal = smtpClient;
#pragma warning restore 618
                                _activity?.RunOnUiThread(() =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }

                                    if (progressLocal != null)
                                    {
                                        progressLocal.AbortClick = sender =>
                                        {
                                            cancelledClicked = true;   // cancel flag in event seems to be missing
                                            try
                                            {
                                                smtpClientLocal.SendAsyncCancel();
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }
                                        };
                                        progressLocal.ButtonAbort.Enabled = true;
                                    }
                                });
                                smtpClient.SendAsync(mail, null);
                                finishEvent.WaitOne();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Dismiss();
                            progress = null;
                            SetLock(LockType.None);
                        }

                        bool cancelled = ex.InnerException is System.Threading.Tasks.TaskCanceledException;
                        if (!cancelled)
                        {
                            string messageText;
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                messageText = errorMessage + "\r\n" + _context.GetString(Resource.String.send_trace_file_failed_message);
                            }
                            else
                            {
                                messageText = _context.GetString(Resource.String.send_trace_file_failed_retry);
                            }

                            bool ignoreDismiss = false;
                            AlertDialog alterDialog = new AlertDialog.Builder(_context)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    SendTraceFileInfoDlg(appDataDir, traceFile, message, classType, handler, deleteFile);
                                    ignoreDismiss = true;
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(messageText)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            if (alterDialog != null)
                            {
                                alterDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }

                                    if (!ignoreDismiss)
                                    {
                                        if (StoreBackupTraceFile(appDataDir, traceFile, message))
                                        {
                                            ShowAlert(_activity.GetString(Resource.String.send_trace_backup_info), Resource.String.alert_title_info);
                                        }

                                        handler?.Invoke(this, new EventArgs());
                                    }
                                };

                                return;
                            }
                        }

                        handler?.Invoke(this, new EventArgs());
                    });
                }
            });
            sendThread.Start();
            return true;
        }

        private string GetMailErrorMessage(string mailXml, out string adapterBlacklist)
        {
            adapterBlacklist = null;

            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);
                if (xmlDoc.Root == null)
                {
                    return null;
                }

                foreach (XElement blacklistNode in xmlDoc.Root.Elements("blacklists"))
                {
                    XAttribute adaptersAttr = blacklistNode.Attribute("adapters");
                    if (adaptersAttr != null && !string.IsNullOrEmpty(adaptersAttr.Value))
                    {
                        adapterBlacklist = adaptersAttr.Value;
                    }
                }

                XElement errorNode = xmlDoc.Root.Element("error");
                // ReSharper disable once UseNullPropagation
                if (errorNode != null)
                {
                    XAttribute messageAttr = errorNode.Attribute("message");
                    if (messageAttr != null && !string.IsNullOrEmpty(messageAttr.Value))
                    {
                        return messageAttr.Value;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        private bool GetMailInfo(string mailXml, out string dbId, out string commitId, out string host, out int port, out bool ssl, out string from, out string to, out string name, out string password)
        {
            dbId = null;
            commitId = null;
            host = null;
            port = 0;
            ssl = false;
            from = null;
            to = null;
            name = null;
            password = null;
            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);

                XElement dbNode = xmlDoc.Root?.Element("db_info");
                if (dbNode != null)
                {
                    XAttribute dbIdAttr = dbNode.Attribute("db_id");
                    if (dbIdAttr == null)
                    {
                        return false;
                    }
                    dbId = dbIdAttr.Value;

                    XAttribute commitIdAttr = dbNode.Attribute("commit_id");
                    if (commitIdAttr == null)
                    {
                      return false;
                    }
                    commitId = commitIdAttr.Value;
                    return true;
                }

                XElement emailNode = xmlDoc.Root?.Element("email");
                XAttribute hostAttr = emailNode?.Attribute("host");
                if (hostAttr == null)
                {
                    return false;
                }
                host = hostAttr.Value;
                XAttribute portAttr = emailNode.Attribute("port");
                if (portAttr == null)
                {
                    return false;
                }
                port = XmlConvert.ToInt32(portAttr.Value);
                XAttribute sslAttr = emailNode.Attribute("ssl");
                if (sslAttr == null)
                {
                    return false;
                }
                ssl = XmlConvert.ToBoolean(sslAttr.Value);
                XAttribute fromAttr = emailNode.Attribute("from");
                if (fromAttr == null)
                {
                    return false;
                }
                from = fromAttr.Value;
                XAttribute toAttr = emailNode.Attribute("to");
                if (toAttr == null)
                {
                    return false;
                }
                to = toAttr.Value;
                XAttribute usernameAttr = emailNode.Attribute("username");
                if (usernameAttr != null)
                {
                    name = usernameAttr.Value;
                }
                XAttribute passwordAttr = emailNode.Attribute("password");
                if (passwordAttr != null)
                {
                    password = passwordAttr.Value;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool GetMailKeyWordsInfo(string mailXml, out string regEx, out int maxWords)
        {
            regEx = null;
            maxWords = 0;
            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);
                XElement keyWordsNode = xmlDoc.Root?.Element("keywords");
                XAttribute regexAttr = keyWordsNode?.Attribute("regex");
                if (regexAttr == null)
                {
                    return false;
                }
                regEx = regexAttr.Value;
                XAttribute maxWordsAttr = keyWordsNode.Attribute("max_words");
                if (maxWordsAttr != null)
                {
                    maxWords = XmlConvert.ToInt32(maxWordsAttr.Value);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool GetMailLinesInfo(string mailXml, out string regEx)
        {
            regEx = null;
            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);
                XElement keyWordsNode = xmlDoc.Root?.Element("lineinfo");
                XAttribute regexAttr = keyWordsNode?.Attribute("regex");
                if (regexAttr == null)
                {
                    return false;
                }
                regEx = regexAttr.Value;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool UpdateCheck(UpdateCheckDelegate handler, int updateSkipVersion)
        {
            try
            {
                if (_updateCheckActive)
                {
                    return false;
                }

                if (handler == null)
                {
                    return false;
                }

                if (_updateHttpClient == null)
                {
                    _updateHttpClient = new HttpClient(new HttpClientHandler()
                    {
                        SslProtocols = DefaultSslProtocols,
                        ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                        Proxy = GetProxySettings()
                    });
                }

                string certInfo = GetCertificateInfo();
                string installer = GetInstallerPackageName();

                MultipartFormDataContent formUpdate = new MultipartFormDataContent
                {
                    { new StringContent(_activity?.PackageName), "package_name" },
                    { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", VersionCode)), "app_ver" },
                    { new StringContent(AppId), "app_id" },
                    { new StringContent(GetCurrentLanguage()), "lang" },
                    { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", (long) Build.VERSION.SdkInt)), "android_ver" },
                    { new StringContent(Build.Fingerprint), "fingerprint" },
                    { new StringContent(installer ?? string.Empty), "installer" },
                    { new StringContent(SelectedInterface.ToDescriptionString()), "interface_type" },
                    { new StringContent(LastAdapterSerial ?? string.Empty), "adapter_serial" },
                };

                if (!string.IsNullOrEmpty(certInfo))
                {
                    formUpdate.Add(new StringContent(certInfo), "cert");
                }

                if (updateSkipVersion >= 0)
                {
                    formUpdate.Add(new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", updateSkipVersion)), "app_ver_ignore");
                }

                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(LastAdapterSerial))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("\n");
                    }
                    sb.Append(string.Format("Adapter serial: {0}", LastAdapterSerial));
                }
                if (BatteryWarnings > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("\n");
                    }
                    sb.Append(string.Format("Battery warnings: {0}", BatteryWarnings));
                    sb.Append(string.Format("\nBattery warning voltage: {0}", BatteryWarningVoltage));
                }
                formUpdate.Add(new StringContent(sb.ToString()), "info_text");

                System.Threading.Tasks.Task<HttpResponseMessage> taskDownload = _updateHttpClient.PostAsync(UpdateCheckUrl, formUpdate);
                _updateCheckActive = true;
                taskDownload.ContinueWith((task, o) =>
                {
                    UpdateCheckDelegate handlerLocal = o as UpdateCheckDelegate;
                    _updateCheckActive = false;
                    try
                    {
                        HttpResponseMessage responseUpdate = task.Result;
                        responseUpdate.EnsureSuccessStatusCode();
                        string responseUpdateXml = responseUpdate.Content.ReadAsStringAsync().Result;
                        bool success = GetUpdateInfo(responseUpdateXml, out int? appVer, out string appVerName, out string infoText, out string errorMessage, out string adapterBlacklist);
                        bool updateAvailable = false;
                        StringBuilder sbMessage = new StringBuilder();

                        if (success)
                        {
                            UpdateSerialInfo(responseUpdateXml);
                            AdapterBlacklist = adapterBlacklist ?? string.Empty;

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                sbMessage.Append(errorMessage);
                            }
                            else
                            {
                                if (appVer.HasValue)
                                {
                                    updateAvailable = true;
                                    sbMessage.Append(_context.GetString(Resource.String.update_header));
                                    if (!string.IsNullOrEmpty(appVerName))
                                    {
                                        sbMessage.Append("\r\n");
                                        sbMessage.Append(string.Format(_context.GetString(Resource.String.update_version), appVerName));
                                    }
                                    if (!string.IsNullOrEmpty(infoText))
                                    {
                                        sbMessage.Append("\r\n");
                                        sbMessage.Append(_context.GetString(Resource.String.update_info));
                                        sbMessage.Append("\r\n");
                                        sbMessage.Append(infoText);
                                    }
                                    sbMessage.Append("\r\n");
                                    sbMessage.Append("\r\n");
                                    sbMessage.Append(_context.GetString(Resource.String.update_show));
                                }
                            }
                        }

                        handlerLocal?.Invoke(success, updateAvailable, appVer, sbMessage.ToString());
                    }
                    catch (Exception)
                    {
                        handlerLocal?.Invoke(false, false, null, null);
                    }
                }, handler, System.Threading.Tasks.TaskContinuationOptions.None);
            }
            catch (Exception)
            {
                _updateCheckActive = false;
                return false;
            }

            return true;
        }

        private bool GetUpdateInfo(string xmlResult, out int? appVer, out string appVerName, out string infoText, out string errorMessage, out string adapterBlacklist)
        {
            appVer = null;
            appVerName = null;
            infoText = null;
            errorMessage = null;
            adapterBlacklist = null;

            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return false;
                }

                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return false;
                }

                foreach (XElement blacklistNode in xmlDoc.Root.Elements("blacklists"))
                {
                    XAttribute adaptersAttr = blacklistNode.Attribute("adapters");
                    if (adaptersAttr != null && !string.IsNullOrEmpty(adaptersAttr.Value))
                    {
                        adapterBlacklist = adaptersAttr.Value;
                    }
                }

                XElement errorNode = xmlDoc.Root.Element("error");
                // ReSharper disable once UseNullPropagation
                if (errorNode != null)
                {
                    XAttribute messageAttr = errorNode.Attribute("message");
                    if (messageAttr != null && !string.IsNullOrEmpty(messageAttr.Value))
                    {
                        errorMessage = messageAttr.Value;
                        return true;
                    }
                }

                XElement updateNode = xmlDoc.Root.Element("update");
                if (updateNode != null)
                {
                    XAttribute appVerAttr = updateNode.Attribute("app_ver");
                    if (appVerAttr != null && !string.IsNullOrEmpty(appVerAttr.Value))
                    {
                        if (int.TryParse(appVerAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int appVerValue))
                        {
                            appVer = appVerValue;
                        }
                    }

                    XAttribute appVerNameAttr = updateNode.Attribute("app_ver_name");
                    if (appVerNameAttr != null && !string.IsNullOrEmpty(appVerNameAttr.Value))
                    {
                        appVerName = appVerNameAttr.Value;
                    }

                    XAttribute infoAttr = updateNode.Attribute("info");
                    if (infoAttr != null && !string.IsNullOrEmpty(infoAttr.Value))
                    {
                        infoText = infoAttr.Value;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetDefaultSettings(bool globalOnly = false, bool includeTheme = false)
        {
            if (!globalOnly)
            {
                EnableTranslation = false;
                YandexApiKey = string.Empty;
                IbmTranslatorApiKey = string.Empty;
                IbmTranslatorUrl = string.Empty;
                DeeplApiKey = string.Empty;
                GoogleApisUrl = string.Empty;
                BatteryWarnings = 0;
                BatteryWarningVoltage = 0;
                AdapterBlacklist = string.Empty;
                LastAdapterSerial = string.Empty;
                EmailAddress = string.Empty;
                TraceInfo = string.Empty;
                // keep last AppId
                //AppId = string.Empty;
            }

            if (includeTheme)
            {
                SelectedTheme = ThemeDefault;
            }

            CustomStorageMedia = string.Empty;
            ShowBatteryVoltageWarning = true;
            AutoHideTitleBar = false;
            SuppressTitleBar = false;
            FullScreenMode = false;
            SwapMultiWindowOrientation = false;
            SelectedInternetConnection = InternetConnectionType.Cellular;
            SelectedManufacturer = ManufacturerType.Bmw;
            BtEnbaleHandling = BtEnableType.Ask;
            BtDisableHandling = BtDisableType.DisableIfByApp;
            LockTypeCommunication = LockType.ScreenDim;
            LockTypeLogging = LockType.Cpu;
            StoreDataLogSettings = false;
            AutoConnectHandling = AutoConnectType.Offline;
            UpdateCheckDelay = UpdateCheckDelayDefault;
            DoubleClickForAppExit = false;
            SendDataBroadcast = false;
            CheckCpuUsage = true;
            CheckEcuFiles = true;
            OldVagMode = false;
            UseBmwDatabase = true;
            ShowOnlyRelevantErrors = true;
            ScanAllEcus = false;
            CollectDebugInfo = false;
            CompressTrace = true;
            Translator = TranslatorType.Deepl;
        }

        public string GetCurrentLanguage(bool iso3 = false)
        {
            return GetCurrentLanguageStatic(iso3, _context);
        }

        public static string GetCurrentLanguageStatic(bool iso3 = false, Context context = null)
        {
            try
            {
                Java.Util.Locale locale = Java.Util.Locale.Default;
                string selectedLocale = GetLocaleSetting(context);
                if (!string.IsNullOrEmpty(selectedLocale))
                {
                    locale = new Java.Util.Locale(selectedLocale);
                }

                string language = locale?.Language;
                if (iso3)
                {
                    string iso3Language = locale?.ISO3Language;
                    if (!string.IsNullOrEmpty(iso3Language))
                    {
                        language = iso3Language;
                    }
                }

                return language ?? DefaultLang;
            }
            catch (Exception)
            {
                return DefaultLang;
            }
        }

        public static WebProxy GetProxySettings()
        {
            try
            {
                string proxyHost = Java.Lang.JavaSystem.GetProperty("http.proxyHost");
                string proxyPort = Java.Lang.JavaSystem.GetProperty("http.proxyPort");
                string nonProxyHosts = Java.Lang.JavaSystem.GetProperty("http.nonProxyHosts");

                if (!string.IsNullOrEmpty(proxyHost))
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(proxyHost.TrimEnd('/'));
                    string address = string.Empty;
                    foreach (IPAddress ipAddress in hostEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            address = ipAddress.ToString();
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(address))
                    {
                        return null;
                    }

                    int portNum = 0;
                    if (!string.IsNullOrEmpty(proxyPort))
                    {
                        if (!int.TryParse(proxyPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out portNum))
                        {
                            portNum = 0;
                        }
                    }

                    if (portNum < 1 || portNum > 0xFFFF)
                    {
                        return null;
                    }

                    address += string.Format(CultureInfo.InvariantCulture, ":{0}", portNum);
                    string[] bypassList = Array.Empty<string>();
                    if (!string.IsNullOrEmpty(nonProxyHosts))
                    {
                        bypassList = nonProxyHosts.Split('|');
                    }

                    return new WebProxy(address, true, bypassList);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public static bool? HasValidProxyHost()
        {
            try
            {
                string proxyHost = Java.Lang.JavaSystem.GetProperty("http.proxyHost");
                if (string.IsNullOrEmpty(proxyHost))
                {
                    return null;
                }

                if (GetProxySettings() == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool ReadBatteryVoltage(EdiabasNet ediabas, out double? batteryVoltage, out byte[] adapterSerial)
        {
            batteryVoltage = null;
            adapterSerial = null;
            try
            {
                if (ediabas == null)
                {
                    return false;
                }
                if (ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
                {
                    if (!edInterfaceObd.Connected)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                // this only reads cached values
                batteryVoltage = ediabas.EdInterfaceClass.AdapterVoltage;
                adapterSerial = ediabas.EdInterfaceClass.AdapterSerial;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int TelLengthKwp2000(byte[] dataBuffer, int offset, out int dataLength, out int dataOffset)
        {
            int bufferLen = dataBuffer.Length - offset;
            dataLength = 0;
            dataOffset = 0;
            if (bufferLen < 4)
            {
                return -1;
            }
            dataLength = dataBuffer[0 + offset] & 0x3F;
            int telLength = dataLength;
            if (telLength == 0)
            {   // with length byte
                dataLength = dataBuffer[3 + offset];
                telLength = dataLength + 5;
                dataOffset = 4;
            }
            else
            {
                telLength += 4;
                dataOffset = 3;
            }
            if (bufferLen < telLength)
            {
                return -1;
            }
            return telLength;
        }

        public static List<VagDtcEntry> ParseEcuDtcResponse(byte[] dataBuffer, bool saeMode)
        {
            if (dataBuffer == null)
            {
                return null;
            }
            List<VagDtcEntry> dtcList = new List<VagDtcEntry>();
            int dtcCount = -1;
            int offset = 0;
            for (;;)
            {
                int telLength = TelLengthKwp2000(dataBuffer, offset, out int dataLength, out int dataOffset);
                if (telLength < 0)
                {
                    return null;
                }
                if (dataLength < 1)
                {
                    return null;
                }

                int pos = 0;
                int responseCode = dataBuffer[offset + dataOffset + pos++];
                if (responseCode != 0x7F)
                {
                    if (dtcCount < 0)
                    {
                        if (dataLength < 2)
                        {
                            return null;
                        }
                        dtcCount = dataBuffer[offset + dataOffset + pos++];
                        if (dtcCount == 0)
                        {
                            return dtcList;
                        }
                    }

                    while (pos + 2 < dataLength)
                    {
                        int index = offset + dataOffset + pos;
                        uint dtcCode = (uint) ((dataBuffer[index] << 8) | dataBuffer[index + 1]);
                        byte dtcDetail = dataBuffer[index + 2];
                        pos += 3;

                        VagDtcEntry dtcEntry = new VagDtcEntry(dtcCode, dtcDetail, saeMode? DataReader.ErrorType.Sae : DataReader.ErrorType.Kwp2000);
                        dtcList.Add(dtcEntry);
                        if (dtcList.Count >= dtcCount)
                        {
                            break;
                        }
                    }
                }
                if (dtcList.Count >= dtcCount)
                {
                    break;
                }
                offset += telLength;
                if (offset == dataBuffer.Length)
                {
                    break;
                }
                if (offset > dataBuffer.Length)
                {
                    return null;
                }
            }

            return dtcList;
        }

        public static List<byte[]> ExtractValidEcuResponses(byte[] dataBuffer)
        {
            if (dataBuffer == null)
            {
                return null;
            }
            List<byte[]> responseList = new List<byte[]>();
            int offset = 0;
            for (; ; )
            {
                int telLength = TelLengthKwp2000(dataBuffer, offset, out int dataLength, out int dataOffset);
                if (telLength < 0)
                {
                    return null;
                }
                if (dataLength < 1)
                {
                    return null;
                }

                int responseCode = dataBuffer[offset + dataOffset];
                if (responseCode != 0x7F)
                {
                    byte[] response = new byte[dataLength];
                    Array.Copy(dataBuffer, offset + dataOffset, response, 0, dataLength);
                    responseList.Add(response);
                    offset += telLength;
                    if (offset == dataBuffer.Length)
                    {
                        break;
                    }

                    if (offset > dataBuffer.Length)
                    {
                        return null;
                    }
                }
            }

            return responseList;
        }

        public static byte[] ParseSaeDetailDtcResponse(byte[] dataBuffer)
        {
            List<byte[]> responseList = ExtractValidEcuResponses(dataBuffer);
            if (responseList == null)
            {
                return null;
            }

            foreach (byte[] response in responseList)
            {
                if (response.Length >= 3)
                {
                    int blockStart = 2;
                    int blockType = response[blockStart];
                    if (blockType == 0x6C)
                    {
                        byte[] blockData = new byte[response.Length - 2];
                        Array.Copy(response, blockStart, blockData, 0, blockData.Length);
                        return blockData;
                    }
                }
            }
            return null;
        }

        public static byte[] ExtractUdsEcuResponses(byte[] dataBuffer)
        {
            if (dataBuffer == null)
            {
                return null;
            }

            int dataOffset = 18;
            if (dataBuffer.Length < dataOffset + 2)
            {
                return null;
            }

            if ((dataBuffer[dataOffset] & 0x40) == 0x00 || dataBuffer[dataOffset + 1] == 0x7F)
            {
                return null;    // error response
            }

            byte[] data = new byte[dataBuffer.Length - dataOffset];
            Array.Copy(dataBuffer, dataOffset, data, 0, data.Length);
            return data;
        }

        public static string GetVagDatUkdDir(string ecuPath, bool ignoreManufacturer = false)
        {
            string lang = GetCurrentLanguageStatic();
            string dirName = Path.Combine(ecuPath, "dat.ukd", lang);
            if (!Directory.Exists(dirName))
            {
                dirName = Path.Combine(ecuPath, "dat.ukd", DefaultLang);
            }

            if (!ignoreManufacturer)
            {
                string manufactName = string.Empty;
                switch (SelectedManufacturer)
                {
                    case ManufacturerType.Audi:
                        manufactName = "audi";
                        break;

                    case ManufacturerType.Seat:
                        manufactName = "seat";
                        break;

                    case ManufacturerType.Skoda:
                        manufactName = "skoda";
                        break;

                    case ManufacturerType.Vw:
                        manufactName = "vw";
                        break;
                }
                if (!string.IsNullOrEmpty(manufactName))
                {
                    dirName = Path.Combine(dirName, manufactName);
                }
            }
            return dirName;
        }

        public static string SaeCode16ToString(long code)
        {
            string locationName;
            switch ((code >> 14) & 0x03)
            {
                default:    // power train
                    locationName = "P";
                    break;

                case 0x1:  // chassis
                    locationName = "C";
                    break;

                case 0x2:  // body
                    locationName = "B";
                    break;

                case 0x3:  // network
                    locationName = "U";
                    break;
            }
            return string.Format("{0}{1:X04}", locationName, code & 0x3FFF);
        }

        public List<string> ConvertVagDtcCode(string ecuPath, long code, List<long> typeList, bool kwp1281, bool saeMode)
        {
            try
            {
                if (_vagDtcCodeDict == null)
                {
                    _vagDtcCodeDict = new Dictionary<string, List<string>>();
                }
                StringBuilder srKey = new StringBuilder();
                srKey.Append("L=");
                srKey.Append(GetCurrentLanguage());
                srKey.Append(";P=");
                srKey.Append(ecuPath);
                srKey.Append(string.Format(";C={0}", code));
                foreach (long errorType in typeList)
                {
                    srKey.Append(string.Format(";T={0}", errorType));
                }
                srKey.Append(string.Format(";K={0}", kwp1281));
                srKey.Append(string.Format(";S={0}", saeMode));

                string dictKey = srKey.ToString();
                List<string> textListDict;
                if (_vagDtcCodeDict.TryGetValue(dictKey, out textListDict))
                {
                    return textListDict;
                }

                string datUkdPath = GetVagDatUkdDir(ecuPath);
                string xmlFile = Path.Combine(datUkdPath, "xml", "Zustand_KonzernASAM.xml");
                if (XmlDocDtcCodes == null)
                {
                    XmlDocDtcCodes = XDocument.Load(xmlFile);
                    if (XmlDocDtcCodes.Root == null)
                    {
                        XmlDocDtcCodes = null;
                        return null;
                    }
                }
                string tableNameDtc = "DTC-table";
                if (saeMode)
                {
                    string codeName = SaeCode16ToString(code >> 8);
                    long detailCode = code & 0xFF;
                    string detailName = string.Format("{0:X02}", detailCode);
                    List<string> textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName + detailName);
                    if (textList.Count == 0 && detailCode != 0x00)
                    {
                        textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName + "00");
                        if (textList.Count > 0)
                        {
                            // get detail text from other entry
                            List<string> textListDetail =  ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, "[PCBU][0-9A-F]{4}" + detailName);
                            if (textListDetail.Count >= 2)
                            {
                                textList.Add(textListDetail[1]);
                            }
                        }
                    }
                    if (typeList.Count == 8)
                    {
                        if (typeList[3] != 0)
                        {
                            textList.Add(_context.GetString(Resource.String.error_code_error_present));
                        }
                        else
                        {
                            if (typeList[2] != 0)
                            {
                                textList.Add(_context.GetString(Resource.String.error_code_error_temporary));
                            }
                        }
                        if (typeList[5] != 0)
                        {
                            textList.Add(_context.GetString(Resource.String.error_code_error_stored));
                        }
                    }

                    _vagDtcCodeDict.Add(dictKey, textList);
                    return textList;
                }
                else
                {
                    string codeName = string.Format("VAG{0:00000}", code);
                    List<string> textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName);
                    if (textList.Count == 0)
                    {
                        if (!VagDtcSaeDict.TryGetValue(code, out codeName))
                        {
                            return null;
                        }
                        codeName += "00";
                        textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName);
                    }
                    else
                    {
                        if (typeList.Count > 0)
                        {
                            string tableNameType = kwp1281 ? "DTC fault symptoms KWP 1281" : "DTC fault symptoms KWP 2000";
                            string typeName = string.Format(kwp1281 ? "FSE{0:X05}" : "FST{0:X05}", typeList[0]);
                            textList.AddRange(ReadVagDtcEntry(XmlDocDtcCodes, tableNameType, typeName));
                        }
                    }

                    _vagDtcCodeDict.Add(dictKey, textList);
                    return textList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> ReadVagDtcEntry(XDocument xmlDoc, string tableName, string entryRegex)
        {
            List<string> textList = new List<string>();
            try
            {
                if (xmlDoc.Root == null)
                {
                    return textList;
                }
                Regex regex = new Regex(entryRegex, RegexOptions.IgnoreCase);
                foreach (XElement tableNode in xmlDoc.Root.Elements("textTable"))
                {
                    XAttribute attrName = tableNode.Attribute("name");
                    if (attrName == null)
                    {
                        continue;
                    }
                    if (string.Compare(attrName.Value, tableName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (XElement textNode in tableNode.Elements("text"))
                    {
                        XAttribute attrId = textNode.Attribute("id");
                        if (attrId == null)
                        {
                            continue;
                        }
                        if (!regex.IsMatch(attrId.Value))
                        {
                            continue;
                        }
                        foreach (XElement lineNode in textNode.Elements("line"))
                        {
                            using (XmlReader reader = lineNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string line = reader.ReadInnerXml();
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    textList.Add(line);
                                }
                            }
                        }
                        return textList;
                    }
                }
            }
            catch (Exception)
            {
                return textList;
            }
            return textList;
        }

        public static List<VagEcuEntry> ReadVagEcuList(string ecuPath)
        {
            try
            {
                string datUkdPath = GetVagDatUkdDir(ecuPath);
                string xmlFile = Path.Combine(datUkdPath, "xml", "GWVL-ECU_config.xml");
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement ecuListNode = xmlDoc.Root?.Element("EcuList");
                if (ecuListNode == null)
                {
                    return null;
                }
                List<VagEcuEntry> ecuList = new List<VagEcuEntry>();
                foreach (XElement ecuNode in ecuListNode.Elements("Ecu"))
                {
                    XElement sysNode = ecuNode.Element("SysName");
                    if (sysNode == null)
                    {
                        continue;
                    }
                    if (sysNode.Attribute("Type") != null)
                    {
                        continue;
                    }
                    string sysName;
                    using (XmlReader reader = sysNode.CreateReader())
                    {
                        reader.MoveToContent();
                        sysName = reader.ReadInnerXml();
                    }
                    if (string.IsNullOrWhiteSpace(sysName))
                    {
                        continue;
                    }

                    XAttribute attrAddr = ecuNode.Attribute("AddrDez");
                    if (attrAddr == null)
                    {
                        continue;
                    }
                    int address;
                    try
                    {
                        address = XmlConvert.ToInt32(attrAddr.Value);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    ecuList.Add(new VagEcuEntry(sysName, address));
                }
                return ecuList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Convert russian mwtab xml files first with (linux):
        // mkdir ../mwru
        // find -name "*.xml" -exec sh -c "recode UTF8..ISO-8859-1 < '{}' | recode windows-1251..UTF8 > ../mwru/'{}'" \;
        public static List<MwTabEntry> ReadVagMwTab(string fileName, bool ignoreEmptyUnits = false)
        {
            try
            {
                Regex commentRegex = new Regex(@"(^I|\\(N[YZ ]|.))", RegexOptions.Multiline);
                XDocument xmlDoc = XDocument.Load(fileName);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                List<MwTabEntry> mwTabList = new List<MwTabEntry>();
                foreach (XElement measureNode in xmlDoc.Root.Elements("Messwert"))
                {
                    try
                    {
                        int blockNumber;
                        XElement agNode = measureNode.Element("AG");
                        if (agNode == null)
                        {
                            continue;
                        }
                        using (XmlReader reader = agNode.CreateReader())
                        {
                            reader.MoveToContent();
                            blockNumber = XmlConvert.ToInt32(reader.ReadInnerXml());
                        }

                        int? valueIndex = null;
                        XElement afNode = measureNode.Element("AF");
                        if (afNode != null)
                        {
                            using (XmlReader reader = afNode.CreateReader())
                            {
                                reader.MoveToContent();
                                try
                                {
                                    valueIndex = XmlConvert.ToInt32(reader.ReadInnerXml());
                                }
                                catch (Exception)
                                {
                                    valueIndex = null;
                                }
                            }
                        }

                        string description = string.Empty;
                        XElement nameNode = measureNode.Element("Name");
                        if (nameNode == null)
                        {
                            continue;
                        }
                        foreach (XElement textNode in nameNode.Elements("Text"))
                        {
                            XAttribute tiAttrib = textNode.Attribute("TI");
                            if (tiAttrib?.Value.Length > 0 && valueIndex.HasValue)
                            {
                                continue;
                            }
                            using (XmlReader reader = textNode.CreateReader())
                            {
                                reader.MoveToContent();
                                description = reader.ReadInnerXml();
                            }
                        }
                        if (string.IsNullOrEmpty(description))
                        {
                            continue;
                        }

                        XElement commentNode = measureNode.Element("Meldungstext");
                        string comment = string.Empty;
                        if (commentNode != null)
                        {
                            using (XmlReader reader = commentNode.CreateReader())
                            {
                                reader.MoveToContent();
                                comment = reader.ReadInnerXml();
                            }
                        }
                        comment = commentRegex.Replace(comment, string.Empty);

                        double? valueMin = null;
                        XElement minNode = measureNode.Element("analogerSWunten");
                        if (minNode != null)
                        {
                            using (XmlReader reader = minNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string value = reader.ReadInnerXml();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    try
                                    {
                                        valueMin = XmlConvert.ToDouble(value);
                                    }
                                    catch (Exception)
                                    {
                                        valueMin = null;
                                    }
                                }
                            }
                        }

                        double? valueMax = null;
                        XElement maxNode = measureNode.Element("analogerSWoben");
                        if (maxNode != null)
                        {
                            using (XmlReader reader = maxNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string value = reader.ReadInnerXml();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    try
                                    {
                                        valueMax = XmlConvert.ToDouble(value);
                                    }
                                    catch (Exception)
                                    {
                                        valueMax = null;
                                    }
                                }
                            }
                        }

                        string valueUnit = string.Empty;
                        XElement sweNode = measureNode.Element("SWE");
                        if (sweNode != null)
                        {
                            using (XmlReader reader = sweNode.CreateReader())
                            {
                                reader.MoveToContent();
                                valueUnit = reader.ReadInnerXml();
                            }
                        }
                        if (ignoreEmptyUnits && string.IsNullOrWhiteSpace(valueUnit))
                        {
                            continue;
                        }
                        if (valueUnit.ToLowerInvariant().StartsWith("proz"))
                        {
                            valueUnit = "%";
                        }

                        string valueType = string.Empty;
                        XElement swfNode = measureNode.Element("SWF");
                        if (swfNode != null)
                        {
                            using (XmlReader reader = swfNode.CreateReader())
                            {
                                reader.MoveToContent();
                                valueType = reader.ReadInnerXml();
                            }
                        }

                        mwTabList.Add(new MwTabEntry(blockNumber, valueIndex, description, comment, valueUnit, valueType, valueMin, valueMax));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return mwTabList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetMatchingVagMwTabUdsFiles(string mwTabDir, long ecuAddr)
        {
            try
            {
                string fileMask = string.Format("UDS_{0:X02}_*.xml", ecuAddr);
                string[] files = Directory.GetFiles(mwTabDir, fileMask, SearchOption.TopDirectoryOnly);
                return files.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetMatchingVagMwTabFiles(string mwTabDir, string sgName)
        {
            try
            {
                List<string> fileList = new List<string>();
                string xmlEntry = string.Format("<SgVariante>{0}</SgVariante>", sgName);
                string[] files = Directory.GetFiles(mwTabDir, "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(file))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                string line = streamReader.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }
                                line = line.Trim();
                                if (string.Compare(line, xmlEntry, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    fileList.Add(file);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return fileList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<MwTabFileEntry> GetMatchingVagMwTabsUds(string mwTabDir, long ecuAddr)
        {
            List<MwTabFileEntry> mwTabFileList = new List<MwTabFileEntry>();
            List<string> fileList = GetMatchingVagMwTabUdsFiles(mwTabDir, ecuAddr);
            if (fileList == null)
            {
                return mwTabFileList;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string fileName in fileList)
            {
                List<MwTabEntry> mwTabList = ReadVagMwTab(fileName);
                if (mwTabList == null)
                {
                    continue;
                }
                List<MwTabEntry> mwTabListCleaned = mwTabList.Where(entry => !entry.ValueIndex.HasValue).ToList();
                mwTabFileList.Add(new MwTabFileEntry(fileName, mwTabListCleaned));
            }
            return mwTabFileList;
        }

        public static List<MwTabFileEntry> GetMatchingVagMwTabs(string mwTabDir, string sgName)
        {
            List<MwTabFileEntry> mwTabFileList = new List<MwTabFileEntry>();
            List<string> fileList = GetMatchingVagMwTabFiles(mwTabDir, sgName);
            if (fileList == null)
            {
                return mwTabFileList;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string fileName in fileList)
            {
                List<MwTabEntry> mwTabList = ReadVagMwTab(fileName, true);
                if (mwTabList == null)
                {
                    continue;
                }
                List<MwTabEntry> mwTabListCleaned = mwTabList.Where(entry => entry.ValueIndex.HasValue).ToList();
                mwTabFileList.Add(new MwTabFileEntry(fileName, mwTabListCleaned));
            }
            return mwTabFileList;
        }

        public static SortedSet<long> ExtractVagMwBlocks(List<MwTabFileEntry> mwTabFileList)
        {
            SortedSet<long> mwBlocks = new SortedSet<long>();

            foreach (MwTabFileEntry mwTabFileEntry in mwTabFileList)
            {
                foreach (MwTabEntry mwTabEntry in mwTabFileEntry.MwTabList)
                {
                    if (!mwBlocks.Contains(mwTabEntry.BlockNumber))
                    {
                        mwBlocks.Add(mwTabEntry.BlockNumber);
                    }
                }
            }

            return mwBlocks;
        }

        public static Dictionary<int, string> GetVagEcuNamesDict(string ecuPath)
        {
            try
            {
                string datUkdBasePath = GetVagDatUkdDir(ecuPath, true);
                string xmlFile = Path.Combine(datUkdBasePath, "xml", "ECU_Names.xml");
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement ecuListNode = xmlDoc.Root?.Element("EcuList");
                if (ecuListNode == null)
                {
                    return null;
                }
                Dictionary<int, string> ecuNamesDict = new Dictionary<int, string>();
                foreach (XElement ecuNode in ecuListNode.Elements("Ecu"))
                {
                    XAttribute attrName = ecuNode.Attribute("Name");
                    string ecuName = attrName?.Value;
                    if (string.IsNullOrWhiteSpace(ecuName))
                    {
                        continue;
                    }
                    XAttribute attrAddr = ecuNode.Attribute("AddrHex");
                    if (attrAddr == null)
                    {
                        continue;
                    }
                    int address;
                    try
                    {
                        address = Convert.ToInt32(attrAddr.Value, 16);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    string existingName;
                    if (ecuNamesDict.TryGetValue(address, out existingName))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (existingName.Length > ecuName.Length)
                        {   // use shortest name
                            ecuNamesDict[address] = ecuName;
                        }
                    }
                    else
                    {
                        ecuNamesDict.Add(address, ecuName);
                    }
                }
                return ecuNamesDict;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsTranslationAvailable()
        {
            switch (SelectedTranslator)
            {
                case TranslatorType.YandexTranslate:
                    return !string.IsNullOrWhiteSpace(YandexApiKey);

                case TranslatorType.IbmWatson:
                    return !string.IsNullOrWhiteSpace(IbmTranslatorApiKey) && !string.IsNullOrWhiteSpace(IbmTranslatorUrl);

                case TranslatorType.Deepl:
                    return !string.IsNullOrWhiteSpace(DeeplApiKey);

                case TranslatorType.YandexCloud:
                    return !string.IsNullOrWhiteSpace(YandexCloudApiKey);

                case TranslatorType.GoogleApis:
                    return !string.IsNullOrWhiteSpace(GoogleApisUrl);
            }

            return false;
        }

        public static bool TranslationRequiresApiKey()
        {
            switch (SelectedTranslator)
            {
                case TranslatorType.GoogleApis:
                    return false;
            }

            return true;
        }

        public bool IsTranslationRequired()
        {
#if true
            if (SelectedManufacturer != ManufacturerType.Bmw)
            {
                return false;
            }
            string lang = GetCurrentLanguage();
            return string.Compare(lang, "de", StringComparison.OrdinalIgnoreCase) != 0;
#else
            return false;
#endif
        }

        public static bool IsYandexCloudOauthToken(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            string keyTrim = apiKey.Trim();
            if (keyTrim.Length > 40 && keyTrim[2] == '_')
            {
                return true;
            }

            return false;
        }

        public static bool VagFilesRequired()
        {
            return SelectedManufacturer != ManufacturerType.Bmw;
        }

        public static bool VagUdsFilesRequired()
        {
            return VagFilesRequired() && !OldVagMode;
        }

        public bool StoreTranslationCache(string fileName)
        {
            try
            {
                XElement xmlLangNodes = new XElement("LanguageCache");
                foreach (string language in _transDict.Keys)
                {
                    Dictionary<string, string> langDict = _transDict[language];
                    XElement xmlLang = new XElement("Language");
                    xmlLang.Add(new XAttribute("Name", language));
                    foreach (string key in langDict.Keys)
                    {
                        XElement xmlTransNode = new XElement("Trans");
                        xmlTransNode.Add(new XAttribute("Key", key));
                        xmlTransNode.Add(new XAttribute("Value", langDict[key]));
                        xmlLang.Add(xmlTransNode);
                    }
                    xmlLangNodes.Add(xmlLang);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    XDocument doc = new XDocument(xmlLangNodes);
                    doc.Save(ms);
                    ms.Position = 0;
                    return CreateZipFile(ms, Path.GetFileName(fileName), fileName +".zip");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ReadTranslationCache(string fileName)
        {
            try
            {
                _transDict.Clear();
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!ExtractZipFile(fileName + ".zip", Path.GetFileName(fileName), ms))
                    {
                        return false;
                    }
                    ms.Position = 0;
                    XDocument xmlLangDoc = XDocument.Load(ms);
                    if (xmlLangDoc.Root == null)
                    {
                        return false;
                    }
                    foreach (XElement langNode in xmlLangDoc.Root.Elements("Language"))
                    {
                        XAttribute attrName = langNode.Attribute("Name");
                        if (attrName == null)
                        {
                            continue;
                        }
                        string language = attrName.Value;
                        if (!_transDict.ContainsKey(language))
                        {
                            Dictionary<string, string> langDict = new Dictionary<string, string>();
                            foreach (XElement transNode in langNode.Elements("Trans"))
                            {
                                XAttribute attrKey = transNode.Attribute("Key");
                                if (attrKey == null)
                                {
                                    continue;
                                }
                                XAttribute attrValue = transNode.Attribute("Value");
                                if (attrValue == null)
                                {
                                    continue;
                                }
                                if (!langDict.ContainsKey(attrKey.Value))
                                {
                                    langDict.Add(attrKey.Value, attrValue.Value);
                                }
                            }
                            _transDict.Add(language, langDict);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void ClearTranslationCache()
        {
            _transDict.Clear();
        }

        public bool IsTranslationCacheEmpty()
        {
            return _transDict.Count == 0;
        }

        public bool RequestEnableTranslate(EventHandler<EventArgs> handler = null)
        {
            try
            {
                if (!IsTranslationRequired() || EnableTranslation || EnableTranslateRequested)
                {
                    return false;
                }
                EnableTranslateRequested = true;
                string message = string.Format(_context.GetString(Resource.String.translate_enable_request), TranslatorName());
                if (TranslationRequiresApiKey())
                {
                    message += "\r\n" + _context.GetString(Resource.String.translate_enable_request_key);
                }

                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        EnableTranslation = true;
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                if (alertDialog != null)
                {
                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        handler?.Invoke(sender, args);
                    };

                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool TranslateStrings(List<string> stringList, TranslateDelegate handler, bool disableCache = false)
        {
            if ((stringList.Count == 0) || (handler == null))
            {
                return false;
            }

            if (!IsTranslationAvailable())
            {
                return false;
            }

            if (_translateHttpClient == null)
            {
                _translateHttpClient = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = DefaultSslProtocols,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    Proxy = GetProxySettings()
                });
            }

            if (_translateProgress == null)
            {
                _transList = null;
                // try to translate with cache first
                _transCurrentLang = (GetCurrentLanguage()).ToLowerInvariant();

                if (!_transDict.TryGetValue(_transCurrentLang, out _transCurrentLangDict) || (_transCurrentLangDict == null))
                {
                    _transCurrentLangDict = new Dictionary<string, string>();
                    _transDict.Add(_transCurrentLang, _transCurrentLangDict);
                }
                _transReducedStringList = new List<string>();
                List<string> translationList = new List<string>();
                foreach (string text in stringList)
                {
                    string translation;
                    if (!disableCache && _transCurrentLangDict.TryGetValue(text, out translation))
                    {
                        translationList.Add(translation);
                    }
                    else
                    {
                        _transReducedStringList.Add(text);
                    }
                }
                if (_transReducedStringList.Count == 0)
                {
                    handler(translationList);
                    return true;
                }

                _translateProgress = new CustomProgressDialog(_context);
                _translateProgress.SetMessage(_context.GetString(Resource.String.translate_text));
                _translateProgress.AbortClick = sender =>
                {
                    if (_translateHttpClient != null)
                    {
                        try
                        {
                            _translateHttpClient.CancelPendingRequests();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                };
                _translateProgress.Indeterminate = false;
                _translateProgress.Progress = 0;
                _translateProgress.Max = 100;
                _translateProgress.Show();
                _translateLockAquired = SetLock(LockTypeCommunication);
            }
            _translateProgress.ButtonAbort.Enabled = false;
            _translateProgress.Progress = (_transList?.Count ?? 0) * 100 / _transReducedStringList.Count;
            SetPreferredNetworkInterface();

            Thread translateThread = new Thread(() =>
            {
                try
                {
                    System.Threading.Tasks.Task<HttpResponseMessage> taskTranslate = null;
                    int stringCount = 0;
                    int urlIndex = 0;
                    List<string> transUrlList = null;
                    List<string> transRequestList = null;
                    HttpContent httpContent = null;
                    StringBuilder sbUrl = new StringBuilder();

                    bool success = false;
                    string responseTranslateResult = string.Empty;
                    for (int retry = 0; retry < 5; retry++)
                    {
                        switch (SelectedTranslator)
                        {
                            case TranslatorType.YandexTranslate:
                                {
                                    if (_transLangList == null)
                                    {
                                        // no language list present, get it first
                                        sbUrl.Append(@"https://translate.yandex.net/api/v1.5/tr/getLangs?");
                                        sbUrl.Append("key=");
                                        sbUrl.Append(System.Uri.EscapeDataString(YandexApiKey));
                                    }
                                    else
                                    {
                                        string langPair = "de-" + _transCurrentLang;
                                        string langPairTemp = langPair;     // prevent warning
                                        if (_transLangList.All(lang => string.Compare(lang, langPairTemp, StringComparison.OrdinalIgnoreCase) != 0))
                                        {
                                            // language not found
                                            langPair = "de-en";
                                        }

                                        sbUrl.Append(@"https://translate.yandex.net/api/v1.5/tr/translate?");
                                        sbUrl.Append("key=");
                                        sbUrl.Append(System.Uri.EscapeDataString(YandexApiKey));
                                        sbUrl.Append("&lang=");
                                        sbUrl.Append(langPair);
                                        int offset = _transList?.Count ?? 0;
                                        for (int i = offset; i < _transReducedStringList.Count; i++)
                                        {
                                            sbUrl.Append("&text=");
                                            sbUrl.Append(System.Uri.EscapeDataString(_transReducedStringList[i]));
                                            stringCount++;
                                            if (sbUrl.Length > 8000)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    _translateHttpClient.DefaultRequestHeaders.Authorization = null;
                                    taskTranslate = _translateHttpClient.GetAsync(sbUrl.ToString());
                                    break;
                                }

                            case TranslatorType.IbmWatson:
                                {
                                    if (_transLangList == null)
                                    {
                                        // no language list present, get it first
                                        sbUrl.Append(IbmTranslatorUrl);
                                        sbUrl.Append(IbmTransIdentLang);
                                        sbUrl.Append(@"?");
                                        sbUrl.Append(IbmTransVersion);
                                    }
                                    else
                                    {
                                        sbUrl.Append(IbmTranslatorUrl);
                                        sbUrl.Append(IbmTransTranslate);
                                        sbUrl.Append(@"?");
                                        sbUrl.Append(IbmTransVersion);

                                        List<string> transList = new List<string>();
                                        int offset = _transList?.Count ?? 0;
                                        int sumLength = 0;
                                        for (int i = offset; i < _transReducedStringList.Count; i++)
                                        {
                                            string testString = "\"" + JsonEncodedText.Encode(_transReducedStringList[i]) + "\",";
                                            sumLength += testString.Length;
                                            if (sumLength > 40000)
                                            {
                                                break;
                                            }

                                            transList.Add(_transReducedStringList[i]);
                                            stringCount++;
                                        }

                                        string targetLang = _transCurrentLang;
                                        if (_transLangList.All(lang => string.Compare(lang, _transCurrentLang, StringComparison.OrdinalIgnoreCase) != 0))
                                        {
                                            // language not found
                                            targetLang = "en";
                                        }

                                        IbmTranslateRequest translateRequest = new IbmTranslateRequest(transList.ToArray(), "de", targetLang);
                                        string jsonString = JsonSerializer.Serialize(translateRequest);

                                        httpContent = new StringContent(jsonString);
                                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                    }

                                    string authParameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("apikey:{0}", IbmTranslatorApiKey)));
                                    _translateHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authParameter);
                                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                    if (httpContent != null)
                                    {
                                        taskTranslate = _translateHttpClient.PostAsync(sbUrl.ToString(), httpContent);
                                    }
                                    else
                                    {
                                        taskTranslate = _translateHttpClient.GetAsync(sbUrl.ToString());
                                    }
                                    break;
                                }

                            case TranslatorType.Deepl:
                                {
                                    string deeplApiUrl = DeeplProUrl;
                                    if (!string.IsNullOrEmpty(DeeplApiKey))
                                    {
                                        if (DeeplApiKey.EndsWith(":fx"))
                                        {
                                            deeplApiUrl = DeeplFreeUrl;
                                        }
                                    }

                                    if (_transLangList == null)
                                    {
                                        // no language list present, get it first
                                        sbUrl.Append(deeplApiUrl);
                                        sbUrl.Append(DeeplIdentLang);
                                    }
                                    else
                                    {
                                        sbUrl.Append(deeplApiUrl);
                                        sbUrl.Append(DeeplTranslate);

                                        List<string> transList = new List<string>();
                                        int offset = _transList?.Count ?? 0;
                                        int sumLength = 0;
                                        for (int i = offset; i < _transReducedStringList.Count; i++)
                                        {
                                            string testString = "\"" + JsonEncodedText.Encode(_transReducedStringList[i]) + "\",";
                                            sumLength += testString.Length;
                                            if (sumLength > 120 * 1024)
                                            {   // real limit is 128KiB
                                                break;
                                            }

                                            transList.Add(_transReducedStringList[i]);
                                            stringCount++;
                                        }

                                        string targetLang = "EN-US";
                                        foreach (string lang in _transLangList)
                                        {
                                            if (lang.StartsWith(_transCurrentLang, StringComparison.OrdinalIgnoreCase))
                                            {
                                                targetLang = lang;
                                                if (lang.Length == 2)
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        DeeplTranslateRequest translateRequest = new DeeplTranslateRequest(transList.ToArray(), "DE", targetLang);
                                        string jsonString = JsonSerializer.Serialize(translateRequest);

                                        httpContent = new StringContent(jsonString);
                                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                    }

                                    _translateHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("DeepL-Auth-Key", DeeplApiKey);
                                    if (httpContent != null)
                                    {
                                        taskTranslate = _translateHttpClient.PostAsync(sbUrl.ToString(), httpContent);
                                    }
                                    else
                                    {
                                        taskTranslate = _translateHttpClient.GetAsync(sbUrl.ToString());
                                    }
                                    break;
                                }

                            case TranslatorType.YandexCloud:
                                {
                                    JsonSerializerOptions jsonOptions = new()
                                    {
                                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                                    };
                                    bool oauthToken = IsYandexCloudOauthToken(YandexCloudApiKey);
                                    string folderId = oauthToken ? YandexCloudFolderId : null;
                                    if (string.IsNullOrWhiteSpace(folderId))
                                    {
                                        folderId = null;
                                    }

                                    TimeSpan tokenAge = DateTime.Now - _yandexCloudIamTokenTime;
                                    if (tokenAge.TotalHours > 1)
                                    {
                                        ResetYandexIamToken();
                                    }

                                    if (oauthToken && string.IsNullOrEmpty(_yandexCloudIamToken))
                                    {
                                        ResetYandexIamToken();
                                        // no IAM Token present
                                        sbUrl.Append("https://iam.api.cloud.yandex.net/iam/v1/tokens");
                                        YandexCloudIamTokenRequest languagesRequest = new YandexCloudIamTokenRequest(YandexCloudApiKey);
                                        string jsonString = JsonSerializer.Serialize(languagesRequest, jsonOptions);
                                        httpContent = new StringContent(jsonString);
                                    }
                                    else if (_transLangList == null)
                                    {
                                        // no language list present, get it first
                                        sbUrl.Append("https://translate.api.cloud.yandex.net/translate/v2/languages");
                                        YandexCloudListLanguagesRequest languagesRequest = new YandexCloudListLanguagesRequest(folderId);
                                        string jsonString = JsonSerializer.Serialize(languagesRequest, jsonOptions);
                                        httpContent = new StringContent(jsonString);
                                    }
                                    else
                                    {
                                        sbUrl.Append("https://translate.api.cloud.yandex.net/translate/v2/translate");

                                        List<string> transList = new List<string>();
                                        int offset = _transList?.Count ?? 0;
                                        int sumLength = 0;
                                        for (int i = offset; i < _transReducedStringList.Count; i++)
                                        {
                                            string testString = "\"" + JsonEncodedText.Encode(_transReducedStringList[i]) + "\",";
                                            sumLength += testString.Length;
                                            if (sumLength > 9000)
                                            {
                                                break;
                                            }

                                            transList.Add(_transReducedStringList[i]);
                                            stringCount++;
                                        }

                                        string targetLang = _transCurrentLang;
                                        if (_transLangList.All(lang => string.Compare(lang, _transCurrentLang, StringComparison.OrdinalIgnoreCase) != 0))
                                        {
                                            // language not found
                                            targetLang = "en";
                                        }

                                        YandexCloudTranslateRequest translateRequest = new YandexCloudTranslateRequest(transList.ToArray(), "de", targetLang, folderId);
                                        string jsonString = JsonSerializer.Serialize(translateRequest, jsonOptions);

                                        httpContent = new StringContent(jsonString);
                                    }

                                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                                    if (oauthToken)
                                    {
                                        if (!string.IsNullOrEmpty(_yandexCloudIamToken))
                                        {
                                            _translateHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _yandexCloudIamToken);
                                        }
                                    }
                                    else
                                    {
                                        _translateHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", YandexCloudApiKey);
                                    }

                                    taskTranslate = _translateHttpClient.PostAsync(sbUrl.ToString(), httpContent);
                                    break;
                                }

                            case TranslatorType.GoogleApis:
                            {
                                    transUrlList = new List<string>();
                                    if (!string.IsNullOrEmpty(GoogleApisUrl))
                                    {
                                        string[] urlList = GoogleApisUrl.Split('\n', '\r');
                                        foreach (string url in urlList)
                                        {
                                            if (!string.IsNullOrEmpty(url))
                                            {
                                                transUrlList.Add(url);
                                            }
                                        }
                                    }

                                    _transLangList = GetGoogleApisLanguages();
                                    string targetLang = _transCurrentLang;
                                    if (_transLangList.All(lang =>
                                            string.Compare(lang, targetLang, StringComparison.OrdinalIgnoreCase) != 0))
                                    {
                                        // language not found
                                        targetLang = "en";
                                    }

                                    if (urlIndex >= transUrlList.Count)
                                    {
                                        break;
                                    }

                                    string transUrl = transUrlList[urlIndex];
                                    int offset = _transList?.Count ?? 0;
                                    if (transUrl.Contains("client=gtx"))
                                    {
                                        sbUrl.Append(transUrl);
                                        sbUrl.Append("&sl=de");
                                        sbUrl.Append("&tl=");
                                        sbUrl.Append(targetLang);

                                        transRequestList = new List<string>();
                                        sbUrl.Append("&q=");
                                        for (int i = offset; i < _transReducedStringList.Count; i++)
                                        {
                                            string line = _transReducedStringList[i].TrimEnd();
                                            transRequestList.Add(line);
                                            if (stringCount > 0)
                                            {
                                                line = "\n" + line;
                                            }

                                            sbUrl.Append(System.Uri.EscapeDataString(line));
                                            stringCount++;
                                            if (sbUrl.Length > 8000)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sbUrl.Append(transUrl);
                                        sbUrl.Append("&sl=de");
                                        sbUrl.Append("&tl=");
                                        sbUrl.Append(targetLang);

                                        for (int i = offset; i < _transReducedStringList.Count; i++)
                                        {
                                            string line = _transReducedStringList[i].TrimEnd();
                                            sbUrl.Append("&q=");
                                            sbUrl.Append(System.Uri.EscapeDataString(line));
                                            stringCount++;
                                            if (sbUrl.Length > 8000)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    _translateHttpClient.DefaultRequestHeaders.Authorization = null;
                                    _translateHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");
                                    taskTranslate = _translateHttpClient.GetAsync(sbUrl.ToString());
                                    break;
                                }
                        }

                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }

                            if (_translateProgress != null)
                            {
                                _translateProgress.ButtonAbort.Enabled = true;
                            }
                        });

                        if (taskTranslate != null)
                        {
                            HttpResponseMessage responseTranslate = taskTranslate.Result;
                            success = responseTranslate.IsSuccessStatusCode;
                            responseTranslateResult = responseTranslate.Content.ReadAsStringAsync().Result;
                        }

                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }

                            if (_translateProgress != null)
                            {
                                _translateProgress.ButtonAbort.Enabled = false;
                            }
                        });

                        if (!success)
                        {
                            switch (SelectedTranslator)
                            {
                                case TranslatorType.GoogleApis:
                                    urlIndex++;
                                    if (transUrlList != null && urlIndex < transUrlList.Count)
                                    {
                                        continue;
                                    }
                                    break;
                            }
                        }

                        break;
                    }

                    if (success)
                    {
                        bool responseEvaluated = false;
                        switch (SelectedTranslator)
                        {
                            case TranslatorType.GoogleApis:
                                break;

                            case TranslatorType.YandexCloud:
                                if (IsYandexCloudOauthToken(YandexCloudApiKey) && string.IsNullOrEmpty(_yandexCloudIamToken))
                                {
                                    _yandexCloudIamToken = GetYandexCloudIamToken(responseTranslateResult, out _yandexCloudIamTokenExpires);
                                    if (!string.IsNullOrEmpty(_yandexCloudIamToken))
                                    {
                                        _yandexCloudIamTokenTime = DateTime.Now;
                                        _activity?.RunOnUiThread(() =>
                                        {
                                            if (_disposed)
                                            {
                                                return;
                                            }
                                            TranslateStrings(stringList, handler, disableCache);
                                        });
                                        return;
                                    }

                                    responseEvaluated = true;
                                    // error
                                    _transList = null;
                                }
                                break;
                        }

                        if (!responseEvaluated)
                        {
                            if (_transLangList == null)
                            {
                                switch (SelectedTranslator)
                                {
                                    case TranslatorType.YandexTranslate:
                                        _transLangList = GetYandexLanguages(responseTranslateResult);
                                        break;

                                    case TranslatorType.IbmWatson:
                                        _transLangList = GetIbmLanguages(responseTranslateResult);
                                        break;

                                    case TranslatorType.Deepl:
                                        _transLangList = GetDeeplLanguages(responseTranslateResult);
                                        break;

                                    case TranslatorType.YandexCloud:
                                        _transLangList = GetYandexCloudLanguages(responseTranslateResult);
                                        break;

                                    case TranslatorType.GoogleApis:
                                        _transLangList = GetGoogleApisLanguages();
                                        break;
                                }

                                if (_transLangList != null)
                                {
                                    _activity?.RunOnUiThread(() =>
                                    {
                                        if (_disposed)
                                        {
                                            return;
                                        }
                                        TranslateStrings(stringList, handler, disableCache);
                                    });
                                    return;
                                }
                            }
                            else
                            {
                                List<string> transList = null;
                                switch (SelectedTranslator)
                                {
                                    case TranslatorType.YandexTranslate:
                                        transList = GetYandexTranslations(responseTranslateResult);
                                        break;

                                    case TranslatorType.IbmWatson:
                                        transList = GetIbmTranslations(responseTranslateResult);
                                        break;

                                    case TranslatorType.Deepl:
                                        transList = GetDeeplTranslations(responseTranslateResult);
                                        break;

                                    case TranslatorType.YandexCloud:
                                        transList = GetYandexCloudTranslations(responseTranslateResult);
                                        break;

                                    case TranslatorType.GoogleApis:
                                        transList = GetGoogleApisTranslations(responseTranslateResult, transRequestList);
                                        break;
                                }

                                if (transList != null && transList.Count == stringCount)
                                {
                                    if (_transList == null)
                                    {
                                        _transList = transList;
                                    }
                                    else
                                    {
                                        _transList.AddRange(transList);
                                    }
                                    if (_transList.Count < _transReducedStringList.Count)
                                    {
                                        _activity?.RunOnUiThread(() =>
                                        {
                                            if (_disposed)
                                            {
                                                return;
                                            }
                                            TranslateStrings(stringList, handler, disableCache);
                                        });
                                        return;
                                    }
                                }
                                else
                                {
                                    // error
                                    _transList = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        // error
                        _transList = null;
                        ResetYandexIamToken();
                    }

                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (_translateProgress != null)
                        {
                            _translateProgress.Dismiss();
                            _translateProgress.Dispose();
                            _translateProgress = null;
                            if (_translateLockAquired)
                            {
                                SetLock(LockType.None);
                                _translateLockAquired = false;
                            }
                        }
                        if ((_transLangList == null) || (_transList == null))
                        {
                            string errorMessage = string.Empty;
                            if (!success)
                            {
                                switch (SelectedTranslator)
                                {
                                    case TranslatorType.YandexTranslate:
                                        errorMessage = GetYandexTranslationError(responseTranslateResult, out int _);
                                        break;

                                    case TranslatorType.IbmWatson:
                                        errorMessage = GetIbmTranslationError(responseTranslateResult, out int _);
                                        break;

                                    case TranslatorType.Deepl:
                                        errorMessage = GetDeeplTranslationError(responseTranslateResult);
                                        break;

                                    case TranslatorType.YandexCloud:
                                        errorMessage = GetYandexCloudTranslationError(responseTranslateResult, out int _);
                                        break;
                                }
                            }

                            string message = string.IsNullOrEmpty(errorMessage) ?
                                _context.GetString(Resource.String.translate_failed) : string.Format(_context.GetString(Resource.String.translate_failed_message), errorMessage);
                            bool yesSelected = false;
                            AlertDialog altertDialog = new AlertDialog.Builder(_context)
                                .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                                {
                                    yesSelected = true;
                                    TranslateStrings(stringList, handler, disableCache);
                                })
                                .SetNegativeButton(Resource.String.button_no, (s, a) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(message)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            if (altertDialog != null)
                            {
                                altertDialog.DismissEvent += (o, eventArgs) =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }
                                    if (!yesSelected)
                                    {
                                        handler(null);
                                    }
                                };

                            }
                        }
                        else
                        {
                            if (disableCache)
                            {
                                handler(_transReducedStringList.Count == _transList.Count
                                    ? _transList : null);
                            }
                            else
                            {
                                // add translation to cache
                                if (_transReducedStringList.Count == _transList.Count)
                                {
                                    for (int i = 0; i < _transList.Count; i++)
                                    {
                                        string key = _transReducedStringList[i];
                                        if (!_transCurrentLangDict.ContainsKey(key))
                                        {
                                            _transCurrentLangDict.Add(key, _transList[i]);
                                        }
                                    }
                                }
                                // create full list
                                List<string> transListFull = new List<string>();
                                foreach (string text in stringList)
                                {
                                    string translation;
                                    if (_transCurrentLangDict.TryGetValue(text, out translation))
                                    {
                                        transListFull.Add(translation);
                                    }
                                    else
                                    {
                                        // should not happen
                                        transListFull = null;
                                        break;
                                    }
                                }
                                handler(transListFull);
                            }
                        }
                    });
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (_translateProgress != null)
                        {
                            _translateProgress.ButtonAbort.Enabled = true;
                        }
                    });
                }
                catch (Exception ex)
                {
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (_translateProgress != null)
                        {
                            _translateProgress.Dismiss();
                            _translateProgress.Dispose();
                            _translateProgress = null;
                            if (_translateLockAquired)
                            {
                                SetLock(LockType.None);
                                _translateLockAquired = false;
                            }
                        }

                        bool cancelled = ex.InnerException is System.Threading.Tasks.TaskCanceledException;
                        if (!cancelled)
                        {
                            bool yesSelected = false;
                            AlertDialog altertDialog = new AlertDialog.Builder(_context)
                                .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                                {
                                    yesSelected = true;
                                    TranslateStrings(stringList, handler, disableCache);
                                })
                                .SetNegativeButton(Resource.String.button_no, (s, a) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.translate_failed)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            if (altertDialog != null)
                            {
                                altertDialog.DismissEvent += (o, eventArgs) =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }
                                    if (!yesSelected)
                                    {
                                        handler(null);
                                    }
                                };

                            }
                        }
                    });
                }
            });
            translateThread.Start();
            return true;
        }

        private void ResetYandexIamToken()
        {
            _yandexCloudIamToken = null;
            _yandexCloudIamTokenExpires = null;
            _yandexCloudIamTokenTime = DateTime.MinValue;
        }

        private string GetYandexTranslationError(string xmlResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return message;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return message;
                }
                XAttribute attrCode = xmlDoc.Root.Attribute("code");
                if (attrCode != null)
                {
                    errorCode = XmlConvert.ToInt32(attrCode.Value);
                }
                XAttribute attrMessage = xmlDoc.Root.Attribute("message");
                if (attrMessage != null)
                {
                    message = attrMessage.Value;
                }
                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        private string GetIbmTranslationError(string jsonResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return message;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                if (jsonDocument.RootElement.TryGetProperty("code", out JsonElement jsonErrorCode))
                {
                    errorCode = jsonErrorCode.GetInt32();
                }

                if (jsonDocument.RootElement.TryGetProperty("error", out JsonElement jsonErrorText))
                {
                    message = jsonErrorText.GetString();
                }

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        private string GetDeeplTranslationError(string jsonResult)
        {
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return message;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                if (jsonDocument.RootElement.TryGetProperty("message", out JsonElement jsonErrorText))
                {
                    message = jsonErrorText.GetString();
                }

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        private string GetYandexCloudTranslationError(string jsonResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return message;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                if (jsonDocument.RootElement.TryGetProperty("code", out JsonElement jsonErrorCode))
                {
                    errorCode = jsonErrorCode.GetInt32();
                }

                if (jsonDocument.RootElement.TryGetProperty("message", out JsonElement jsonErrorText))
                {
                    message = jsonErrorText.GetString();
                }

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        private string GetYandexCloudIamToken(string jsonResult, out string tokenExpires)
        {
            tokenExpires = null;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                string iamTokenString = null;
                if (jsonDocument.RootElement.TryGetProperty("iamToken", out JsonElement iamToken))
                {
                    iamTokenString = iamToken.GetString();
                }

                if (!string.IsNullOrEmpty(iamTokenString))
                {
                    if (jsonDocument.RootElement.TryGetProperty("expiresAt", out JsonElement expiresAt))
                    {
                        tokenExpires = expiresAt.GetString();
                    }
                }

                return iamTokenString;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetYandexLanguages(string xmlResult)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                XElement dirsNode = xmlDoc.Root?.Element("dirs");
                if (dirsNode == null)
                {
                    return null;
                }
                List<string> transList = new List<string>();
                foreach (XElement textNode in dirsNode.Elements("string"))
                {
                    using (XmlReader reader = textNode.CreateReader())
                    {
                        reader.MoveToContent();
                        transList.Add(reader.ReadInnerXml());
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetIbmLanguages(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("languages", out JsonElement languages))
                {
                    return null;
                }

                foreach (JsonElement translation in languages.EnumerateArray())
                {
                    if (translation.TryGetProperty("language", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetDeeplLanguages(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                foreach (JsonElement translation in jsonDocument.RootElement.EnumerateArray())
                {
                    if (translation.TryGetProperty("language", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetYandexCloudLanguages(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("languages", out JsonElement languages))
                {
                    return null;
                }

                foreach (JsonElement language in languages.EnumerateArray())
                {
                    if (language.TryGetProperty("code", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetGoogleApisLanguages()
        {
            List<string> transList = new List<string>()
            {
                "af", "sq", "am", "ar", "hy", "az", "eu", "be", "bn", "bs", "bg", "my", "ca", "ca",
                "ceb", "zh-cn", "co", "cs", "da", "nl", "nl", "en", "eo", "et", "fi", "fr", "fy", "ka",
                "de", "gd", "gd", "ga", "gl", "el", "gu", "ht", "ht", "ha", "haw", "he", "hi", "hr",
                "hu", "ig", "is", "id", "it", "jw", "ja", "kn", "kk", "km", "ky", "ky", "ko", "ku",
                "lo", "la", "lv", "lt", "lb", "lb", "mk", "ml", "mi", "mr", "ms", "mg", "mt", "mn",
                "ne", "no", "ny", "ny", "ny", "or", "pa", "pa", "fa", "pl", "pt", "ps", "ps", "ro",
                "ro", "ro", "ru", "si", "si", "sk", "sl", "sm", "sn", "sd", "so", "st", "es", "es",
                "sr", "su", "sw", "sv", "ta", "te", "tg", "tl", "th", "tr", "ug", "ug", "uk", "ur",
                "uz", "vi", "cy", "xh", "yi", "yo", "zu", "zh-CN", "zh-TW"
            };

            return transList;
        }

        private List<string> GetYandexTranslations(string xmlResult)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                List<string> transList = new List<string>();
                foreach (XElement textNode in xmlDoc.Root.Elements("text"))
                {
                    using (XmlReader reader = textNode.CreateReader())
                    {
                        reader.MoveToContent();
                        transList.Add(reader.ReadInnerXml());
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetIbmTranslations(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("translations", out JsonElement translations))
                {
                    return null;
                }

                foreach (JsonElement translation in translations.EnumerateArray())
                {
                    if (translation.TryGetProperty("translation", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetDeeplTranslations(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("translations", out JsonElement translations))
                {
                    return null;
                }

                foreach (JsonElement translation in translations.EnumerateArray())
                {
                    if (translation.TryGetProperty("text", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetYandexCloudTranslations(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("translations", out JsonElement translations))
                {
                    return null;
                }

                foreach (JsonElement translation in translations.EnumerateArray())
                {
                    if (translation.TryGetProperty("text", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetGoogleApisTranslations(string jsonResult, List<string> transRequestList)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array || jsonDocument.RootElement.GetArrayLength() < 1)
                {
                    return null;
                }

                if (transRequestList != null)
                {
                    JsonElement element0 = jsonDocument.RootElement[0];
                    int itemCount = element0.GetArrayLength();
                    if (element0.ValueKind != JsonValueKind.Array || itemCount < 1)
                    {
                        return null;
                    }

                    int requestIndex = 0;
                    string sourceParts = string.Empty;
                    string translationParts = string.Empty;

                    for (int i = 0; i < itemCount; i++)
                    {
                        JsonElement element1 = element0[i];
                        if (element1.ValueKind != JsonValueKind.Array || element1.GetArrayLength() < 2)
                        {
                            return null;
                        }

                        if (requestIndex >= transRequestList.Count)
                        {
                            return null;
                        }

                        string requestCleaned = transRequestList[requestIndex].TrimEnd('\n').Replace(" ", string.Empty);
                        string source = element1[1].GetString() ?? string.Empty;
                        string translation = element1[0].GetString() ?? string.Empty;

                        sourceParts += source;
                        translationParts += translation;

                        string sourceCleanded = sourceParts.TrimEnd('\n').Replace(" ", string.Empty);
                        if (sourceCleanded.Length < requestCleaned.Length)
                        {
                            continue;
                        }

                        if (sourceCleanded.Length == requestCleaned.Length)
                        {
                            if (string.Compare(sourceCleanded, requestCleaned, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                return null;
                            }
                        }
                        else
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("GetGoogleApisTranslations Mismatch: Source='{0}', Request='{1}'", sourceCleanded, requestCleaned));
#endif
                        }

                        transList.Add(translationParts.TrimEnd('\n'));
                        requestIndex++;
                        sourceParts = string.Empty;
                        translationParts = string.Empty;
                    }

                    if (requestIndex != transRequestList.Count)
                    {
                        return null;
                    }
                }
                else
                {
                    int itemCount = jsonDocument.RootElement.GetArrayLength();
                    for (int i = 0; i < itemCount; i++)
                    {
                        JsonElement element = jsonDocument.RootElement[i];
                        if (element.ValueKind != JsonValueKind.String)
                        {
                            return null;
                        }

                        string translation = element.GetString() ?? string.Empty;
                        transList.Add(translation);
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static Java.Lang.ICharSequence FromHtml(string source)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return Android.Text.Html.FromHtml(source, Android.Text.FromHtmlOptions.ModeLegacy);
            }
#pragma warning disable 618
#pragma warning disable CA1422
            return Android.Text.Html.FromHtml(source);
#pragma warning restore CA1422
#pragma warning restore 618
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(toPath))
            {
                return fromPath;
            }
            System.Uri fromUri = new System.Uri(AppendDirectorySeparatorChar(fromPath));
            System.Uri toUri = new System.Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {   // path can't be made relative.
                return toPath; 
            }

            System.Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, System.Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        public static int DecToBcd(int value)
        {
            return (((value / 10) << 4) | (value % 10));
        }

        public static int BcdToDec(int value)
        {
            return (((value >> 4) * 10) + (value & 0xF));
        }

        public static Android.Text.InputTypes ConvertVagUdsDataTypeToInputType(UInt32 dataTypeId)
        {
            Android.Text.InputTypes inputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagNoSuggestions;

            UdsReader.DataType dataType = (UdsReader.DataType)(dataTypeId & UdsReader.DataTypeMaskEnum);
            switch (dataType)
            {
                case UdsReader.DataType.FloatScaled:
                    inputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
                    break;

                case UdsReader.DataType.Integer1:
                case UdsReader.DataType.Integer2:
                    inputType = Android.Text.InputTypes.ClassNumber;
                    break;
            }

            if ((inputType & Android.Text.InputTypes.MaskClass) == Android.Text.InputTypes.ClassNumber)
            {
                if ((dataTypeId & UdsReader.DataTypeMaskSigned) != 0x00)
                {
                    inputType |= Android.Text.InputTypes.NumberFlagSigned;
                }
            }
            return inputType;
        }

        public static bool CheckZipFile(string archiveFilename)
        {
            ZipFile zf = null;
            try
            {
                try
                {
                    FileStream fs = File.OpenRead(archiveFilename);
                    zf = new ZipFile(fs);
                    return zf.TestArchive(false);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public static string GetAssetEcuFilename()
        {
            if (!string.IsNullOrEmpty(_assetEcuFileName))
            {
                return _assetEcuFileName;
            }

            try
            {
                AssetManager assets = GetPackageContext()?.Assets;
                if (assets != null)
                {
                    string[] assetFiles = assets.List(string.Empty);
                    if (assetFiles != null)
                    {
                        Regex regex = new Regex(@"^Ecu.*\.bin$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        foreach (string fileName in assetFiles)
                        {
                            if (regex.IsMatch(fileName))
                            {
                                _assetEcuFileName = fileName;
                                return fileName;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _assetEcuFileName = string.Empty;
            return null;
        }

        public static void ExtractZipFile(AssetManager assetManager, Assembly resourceAssembly, string archiveFilenameIn, string outFolder, string key,
            List<string> ignoreFolders, List<string> encodeExtensions, ProgressZipDelegate progressHandler)
        {
#if DEBUG
            string lastFileName = string.Empty;
            Android.Util.Log.Info(Tag, string.Format("ExtractZipFile Archive: '{0}', Folder: '{1}'", archiveFilenameIn, outFolder));
#endif
            Stream fs = null;
            ZipFile zf = null;
            string tempFile = Path.Combine(outFolder, "temp.zip");
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    using (Aes crypto = Aes.Create())
                    {
                        crypto.Mode = CipherMode.CBC;
                        crypto.Padding = PaddingMode.PKCS7;
                        crypto.KeySize = 256;

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                        using (MD5 md5 = MD5.Create())
                        {
                            crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }

                        bool extractAborted = false;
                        for (int retry = 0;; retry++)
                        {
                            try
                            {
                                AssetFileDescriptor assetFile = null;
                                Stream fsRead = null;
                                try
                                {
                                    if (assetManager != null)
                                    {
                                        assetFile = assetManager.OpenFd(archiveFilenameIn);
                                        fsRead = assetFile.CreateInputStream();
                                    }
                                    else if (resourceAssembly != null)
                                    {
                                        fsRead = resourceAssembly.GetManifestResourceStream(archiveFilenameIn);
                                    }
                                    else
                                    {
                                        fsRead = File.OpenRead(archiveFilenameIn);
                                    }

                                    if (fsRead == null)
                                    {
                                        extractAborted = true;
                                        throw new IOException("Opening archive failed");
                                    }

                                    using (CryptoStream crStream = new CryptoStream(fsRead, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                                    {
                                        byte[] buffer = new byte[StreamBufferSize];
                                        using (FileStream fsWrite = File.Create(tempFile))
                                        {
                                            bool aborted = false;
                                            StreamUtils.Copy(crStream, fsWrite, buffer, (sender, args) =>
                                            {
                                                if (progressHandler != null)
                                                {
                                                    if (progressHandler((int)args.PercentComplete, true))
                                                    {
                                                        args.ContinueRunning = false;
                                                        aborted = true;
                                                    }
                                                }
                                            }, TimeSpan.FromSeconds(1), null, null, fsRead.Length);
                                            if (aborted)
                                            {
                                                extractAborted = true;
                                                return;
                                            }
#if IO_TEST
                                            if (retry == 0)
                                            {
                                                throw new IOException("Exception test");
                                            }
#endif
                                        }
                                    }
                                }
                                finally
                                {
                                    fsRead?.Dispose();
                                    assetFile?.Close();
                                }

                                break;
                            }
#pragma warning disable 168
                            catch (Exception ex)
#pragma warning restore 168
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, EdiabasNet.GetExceptionText(ex));
#endif
                                if (extractAborted || retry > FileIoRetries)
                                {
                                    throw;
                                }

                                Thread.Sleep(FileIoRetryDelay);
                            }
                        }
                    }
                    fs = File.OpenRead(tempFile);
                }
                else
                {
                    if (assetManager != null)
                    {
                        AssetFileDescriptor assetFile = null;
                        try
                        {
                            assetFile = assetManager.OpenFd(archiveFilenameIn);
                            using (Stream inputStream = assetFile.CreateInputStream())
                            {
                                if (inputStream == null)
                                {
                                    throw new IOException("Opening asset stream failed");
                                }

                                byte[] buffer = new byte[StreamBufferSize];
                                string tempFileName = Path.GetTempFileName();
                                fs = File.Create(tempFileName, StreamBufferSize, FileOptions.DeleteOnClose);
                                StreamUtils.Copy(inputStream, fs, buffer);
                                fs.Seek(0, SeekOrigin.Begin);
                            }
                        }
                        finally
                        {
                            assetFile?.Close();
                        }
                    }
                    else if (resourceAssembly != null)
                    {
                        fs = resourceAssembly.GetManifestResourceStream(archiveFilenameIn);
                    }
                    else
                    {
                        fs = File.OpenRead(archiveFilenameIn);
                    }
                }

                zf = new ZipFile(fs);

                // count files to extract, to display the correct progress
                int fileCount = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    string entryFileName = zipEntry.Name;
                    if (ignoreFolders != null)
                    {
                        bool ignoreFile = false;
                        foreach (string ignoreFolder in ignoreFolders)
                        {
                            if (entryFileName.StartsWith(ignoreFolder))
                            {
                                ignoreFile = true;
                                break;
                            }
                        }
                        if (ignoreFile)
                        {
                            continue;
                        }
                    }
                    fileCount++;
                }
                if (fileCount == 0)
                {
                    fileCount = 1;
                }
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("ExtractZipFile FileCount: {0}", fileCount));
#if IO_TEST
                int testCount = 0;
#endif
#endif
                long index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / fileCount)))
                        {
                            return;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    string entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    if (ignoreFolders != null)
                    {
                        bool ignoreFile = false;
                        foreach (string ignoreFolder in ignoreFolders)
                        {
                            if (entryFileName.StartsWith(ignoreFolder))
                            {
                                ignoreFile = true;
                                break;
                            }
                        }
                        if (ignoreFile)
                        {
                            continue;
                        }
                    }

                    for (int retry = 0; ; retry++)
                    {
                        bool noRetry = false;
                        try
                        {
                            // Manipulate the output filename here as desired.
                            string fullZipToPath = Path.Combine(outFolder, entryFileName);
                            string fileExt = Path.GetExtension(fullZipToPath);
                            if (encodeExtensions != null && encodeExtensions.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
                            {
                                fullZipToPath = EdiabasNet.EncodeFilePath(fullZipToPath);
                            }
                            string directoryName = Path.GetDirectoryName(fullZipToPath);
                            if (!string.IsNullOrEmpty(directoryName))
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                Directory.CreateDirectory(directoryName);
                            }

                            byte[] buffer = new byte[StreamBufferSize];
                            noRetry = true;     // no retry for zip stream exception
                            using (Stream zipStream = zf.GetInputStream(zipEntry))
                            {
                                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                                // of the file, but does not waste memory.
                                // The "using" will close the stream even if an exception occurs.
                                noRetry = false;
#if DEBUG
                                lastFileName = fullZipToPath;
#if IO_TEST
                                testCount++;
                                if (testCount % 100 == 0)
                                {
                                    throw new IOException("Exception test");
                                }
#endif
#endif
                                using (FileStream streamWriter = File.Create(fullZipToPath))
                                {
                                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                                }
                            }
                            break;
                        }
#pragma warning disable 168
                        catch (Exception ex)
#pragma warning restore 168
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, EdiabasNet.GetExceptionText(ex));
#endif
                            if (noRetry || retry > FileIoRetries)
                            {
                                throw;
                            }

                            Thread.Sleep(FileIoRetryDelay);
                        }
                    }

                    index++;
                }
#if DEBUG
                Android.Util.Log.Info(Tag, "ExtractZipFile done");
#endif
            }
#if DEBUG
            catch (Exception ex)
            {
                string exceptionText = EdiabasNet.GetExceptionText(ex);
                Android.Util.Log.Info(Tag, string.Format("ExtractZipFile File: '{0}', Exception: '{1}', ", lastFileName, exceptionText));
                throw;
            }
#endif
            finally
            {
                // ReSharper disable once ConstantConditionalAccessQualifier
                zf?.Close(); // Ensure we release resources
                // ReSharper disable once ConstantConditionalAccessQualifier
                fs?.Close();
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static bool CreateZipFile(string[] inputFiles, string archiveFilenameOut, ProgressZipDelegate progressHandler)
        {
            try
            {
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    long index = 0;
                    foreach (string filename in inputFiles)
                    {
                        if (progressHandler != null)
                        {
                            if (progressHandler((int)(100 * index / inputFiles.Length)))
                            {
                                return false;
                            }
                        }

                        FileInfo fi = new FileInfo(filename);
                        string entryName = Path.GetFileName(filename);

                        ZipEntry newEntry = new ZipEntry(entryName)
                        {
                            DateTime = fi.LastWriteTime,
                            Size = fi.Length
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[StreamBufferSize];
                        using (FileStream streamReader = File.OpenRead(filename))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer);
                        }
                        zipStream.CloseEntry();
                        index++;
                    }
                }
                finally
                {
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool ExtractZipFile(string archiveFilenameIn, string archiveName, Stream outStream)
        {
            try
            {
                ZipFile zf = null;
                try
                {
                    FileStream fs = File.OpenRead(archiveFilenameIn);
                    zf = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue; // Ignore directories
                        }
                        if (string.Compare(zipEntry.Name, archiveName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            byte[] buffer = new byte[StreamBufferSize];
                            using (Stream zipStream = zf.GetInputStream(zipEntry))
                            {
                                StreamUtils.Copy(zipStream, outStream, buffer);
                            }
                            break;
                        }
                    }
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CreateZipFile(Stream inStream, string archiveName, string archiveFilenameOut)
        {
            try
            {
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    ZipEntry newEntry = new ZipEntry(archiveName)
                    {
                        Size = inStream.Length
                    };
                    zipStream.PutNextEntry(newEntry);
                    byte[] buffer = new byte[StreamBufferSize];
                    StreamUtils.Copy(inStream, zipStream, buffer);
                    zipStream.CloseEntry();
                }
                finally
                {
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool VerifyContent(string contentFile, bool checkMd5, ProgressVerifyDelegate progressHandler)
        {
            try
            {
                string baseDir = Path.GetDirectoryName(contentFile);
                if (string.IsNullOrEmpty(baseDir))
                {
                    return false;
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                bool vagUdsDirPresent = Directory.Exists(Path.Combine(baseDir, VagBaseDir));
                bool vagEcuDirPresent = Directory.Exists(Path.Combine(baseDir, EcuDirNameVag));
                if (vagUdsDirPresent && !vagEcuDirPresent)
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Load(contentFile);
                if (xmlDoc.Root == null)
                {
                    return false;
                }

                XElement[] fileNodes = xmlDoc.Root.Elements("file").ToArray();
                int fileCount = 0;
                foreach (XElement fileNode in fileNodes)
                {
                    XAttribute nameAttr = fileNode.Attribute("name");
                    if (nameAttr == null)
                    {
                        return false;
                    }
                    string fileName = nameAttr.Value;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return false;
                    }

                    bool vagUdsFile = fileName.StartsWith(AppendDirectorySeparatorChar(VagBaseDir));
                    if (!vagUdsDirPresent && vagUdsFile)
                    {
                        continue;   // ignore VAG UDS files if not present
                    }
                    bool vagEcuFile = fileName.StartsWith(AppendDirectorySeparatorChar(EcuDirNameVag));
                    if (!vagEcuDirPresent && vagEcuFile)
                    {
                        continue;   // ignore VAG ECU files if not present
                    }

                    fileCount++;
                }
                if (fileCount == 0)
                {
                    fileCount = 1;
                }

                int index = 0;
                int lastPercent = -1;
                foreach (XElement fileNode in fileNodes)
                {
                    if (progressHandler != null)
                    {
                        int percent = 100 * index / fileCount;
                        if (lastPercent != percent)
                        {
                            lastPercent = percent;
                            if (progressHandler.Invoke(percent))
                            {
                                return false;
                            }
                        }
                    }

                    XAttribute nameAttr = fileNode.Attribute("name");
                    if (nameAttr == null)
                    {
                        return false;
                    }
                    string fileName = nameAttr.Value;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return false;
                    }

                    bool vagUdsFile = fileName.StartsWith(AppendDirectorySeparatorChar(VagBaseDir));
                    if (!vagUdsDirPresent && vagUdsFile)
                    {
                        continue;   // ignore VAG UDS files if not present
                    }
                    bool vagEcuFile = fileName.StartsWith(AppendDirectorySeparatorChar(EcuDirNameVag));
                    if (!vagEcuDirPresent && vagEcuFile)
                    {
                        continue;   // ignore VAG ECU files if not present
                    }
                    XAttribute sizeAttr = fileNode.Attribute("size");
                    if (sizeAttr == null)
                    {
                        return false;
                    }
                    long fileSize = XmlConvert.ToInt64(sizeAttr.Value);

                    string fullFileName = Path.Combine(baseDir, fileName);
#if DEBUG && false
                    string encodedFileName = EdiabasNet.EncodeFilePath(fullFileName);
                    if (!string.IsNullOrEmpty(encodedFileName))
                    {
                        string decodedFilePath = EdiabasNet.DecodeFilePath(encodedFileName);
                        if (string.Compare(fullFileName, decodedFilePath, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            Android.Util.Log.Error(Tag, string.Format(CultureInfo.InvariantCulture, "Encoding invalid: {0} {1}", fullFileName, decodedFilePath));
                        }
                    }
#endif
                    // ReSharper disable once AssignNullToNotNullAttribute
                    string filePath = EdiabasNet.GetExistingEncodedFilePath(fullFileName);
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (!fileInfo.Exists)
                    {
                        return false;
                    }

                    if (fileInfo.Length != fileSize)
                    {
                        return false;
                    }

                    if (checkMd5)
                    {
                        XAttribute md5Attr = fileNode.Attribute("md5");
                        if (md5Attr == null)
                        {
                            return false;
                        }

                        string md5String = md5Attr.Value;
                        using (MD5 md5 = MD5.Create())
                        {
                            using (FileStream stream = File.OpenRead(filePath))
                            {
                                byte[] md5Data = md5.ComputeHash(stream);
                                StringBuilder sb = new StringBuilder();
                                foreach (byte value in md5Data)
                                {
                                    sb.Append($"{value:X02}");
                                }

                                if (string.Compare(sb.ToString(), md5String, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    index++;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool ExtraktPackageAssemblies(string outputPath, ProgressApkDelegate progressApkDelegate = null, bool forceUpdate = false)
        {
            try
            {
                if (string.IsNullOrEmpty(outputPath))
                {
                    return false;
                }

                PackageInfo packageInfo = GetPackageInfo();
                string packageFile = packageInfo.ApplicationInfo?.SourceDir;
                if (string.IsNullOrEmpty(packageFile))
                {
                    return false;
                }

                if (!File.Exists(packageFile))
                {
                    return false;
                }

                string packageInfoFile = Path.Combine(outputPath, "PackageInfo.xml");
                DateTime packageFileTime = File.GetLastWriteTimeUtc(packageFile);
                string packageTimeStamp = packageFileTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string oldTimeStamp = null;

                if (File.Exists(packageInfoFile))
                {
                    XDocument xmlInfo = XDocument.Load(packageInfoFile);
                    XAttribute dateAttr = xmlInfo.Root?.Attribute("Date");
                    if (dateAttr != null)
                    {
                        oldTimeStamp = dateAttr.Value;
                    }
                }

                if (!forceUpdate && !string.IsNullOrEmpty(oldTimeStamp))
                {
                    if (string.Compare(oldTimeStamp, packageTimeStamp, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }

                List<string> apkFileList = new List<string> { packageFile };

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
#pragma warning disable CA1416
                    IList<string> packageSplitFiles = packageInfo.ApplicationInfo?.SplitSourceDirs;
#pragma warning restore CA1416
                    if (packageSplitFiles != null)
                    {
                        foreach (string splitName in packageSplitFiles)
                        {
                            if (File.Exists(splitName))
                            {
                                apkFileList.Add(splitName);
                            }
                        }
                    }
                }

                if (Directory.Exists(outputPath))
                {
                    Directory.Delete(outputPath, true);
                }

                bool result = false;
                ApkUncompress2.ApkUncompressCommon apkUncompress = new ApkUncompress2.ApkUncompressCommon();
                foreach (string apkFile in apkFileList)
                {
                    if (apkUncompress.UncompressFromAPK(apkFile, outputPath, percent =>
                        {
                            if (progressApkDelegate != null)
                            {
                                return progressApkDelegate(percent);
                            }
                            return true;
                        }))
                    {
                        result = true;
                        break;
                    }
                }

                if (result && Directory.Exists(outputPath))
                {
                    XElement xmlInfo = new XElement("PackageInfo");
                    xmlInfo.Add(new XAttribute("Name", packageFile));
                    xmlInfo.Add(new XAttribute("Date", packageTimeStamp));
                    xmlInfo.Save(packageInfoFile);
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<string> GetCurrentAbiDirs()
        {
            bool isArm = false;
            string abi = null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                if (Build.SupportedAbis != null && Build.SupportedAbis.Count > 0)
                {
                    abi = Build.SupportedAbis[0];
                }
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                abi = Build.CpuAbi;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(abi))
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("GetCurrentAbiDirs ABI={0}", abi));
#endif
                if (abi.Contains("arm", StringComparison.OrdinalIgnoreCase))
                {
                    isArm = true;
                }
            }

            try
            {
                string soArch = Java.Lang.JavaSystem.GetProperty("os.arch");
                if (!string.IsNullOrEmpty(soArch))
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GetCurrentAbiDirs Arch={0}", soArch));
#endif
                    if (soArch.Contains("arm", StringComparison.OrdinalIgnoreCase))
                    {
                        isArm = true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            bool is64Bit = IntPtr.Size == 8;
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetCurrentAbiDirs ARM={0}, 64Bit={1}", isArm, is64Bit));
#endif
            if (isArm)
            {
                // the release version uses _ and debug uses -
                return is64Bit ? ["arm64", "arm64_v8a", "arm64-v8a", "aarch64"] : ["armeabi_v7a", "armeabi-v7a"];
            }

            // only the _ variant is used for release and debug
            return is64Bit ? ["x86_64", "x86-64"] : ["x86"];
        }

        public static List<Microsoft.CodeAnalysis.MetadataReference> GetLoadedMetadataReferences(string packageAssembiesDir, out List<string> errorList)
        {
            string assembliesDir = packageAssembiesDir;
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Microsoft.CodeAnalysis.MetadataReference> referencesList = new List<Microsoft.CodeAnalysis.MetadataReference>();
            errorList = new List<string>();

            List<string> abiDirs = GetCurrentAbiDirs();
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences ABI dirs={0}", string.Join(", ", abiDirs)));
#endif
            foreach (Assembly assembly in loadedAssemblies)
            {
                // the location contain no path for embedded assemblies
#pragma warning disable IL3000
                string location = assembly.Location;
#pragma warning restore IL3000
                if (string.IsNullOrEmpty(location))
                {
                    continue;
                }

                if (!File.Exists(location))
                {
                    string fileName = Path.GetFileName(location);

                    const string dllExtension = ".dll";
                    if (!fileName.EndsWith(dllExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += dllExtension;
                    }

                    location = Path.Combine(assembliesDir, fileName);
                    if (!File.Exists(location))
                    {
                        foreach (string abi in abiDirs)
                        {
                            location = Path.Combine(assembliesDir, abi, fileName);
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences ABI={0}, Location={1}", abi, location));
#endif
                            if (File.Exists(location))
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File found: {0}", fileName));
#endif
                                break;
                            }
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File not found: {0}", fileName));
#endif
                            location = null;
                        }
                    }
                }

                if (location != null)
                {
                    try
                    {
                        using (File.Open(location, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                        }
                    }
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File read Exception: {0}", ex.Message));
#endif
                        location = null;
                    }
                }

                if (location != null)
                {
                    referencesList.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(location));
                }
                else
                {
                    // the location contain no path for embedded assemblies
#pragma warning disable IL3000
                    string fileName = Path.GetFileName(assembly.Location);
#pragma warning restore IL3000
                    errorList.Add(fileName);
                }
            }

#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences Error count: {0}", errorList.Count));
#endif
            return referencesList;
        }

        public string CompileCode(JobReader.PageInfo pageInfo, List<Microsoft.CodeAnalysis.MetadataReference> referencesList)
        {
            string classCode = @"
using Android.Views;
using Android.Widget;
using Android.Content;
using EdiabasLib;
using BmwDeepObd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;"
        + pageInfo.ClassCode;

            string result = string.Empty;
            try
            {
                // ToDo: Mono init bug, limit to one thread: https://github.com/dotnet/runtime/issues/96804
                lock (CompileLock)
                {
                    Microsoft.CodeAnalysis.SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(classCode);
                    Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                        "UserCode",
                        new[] { syntaxTree },
                        referencesList,
                        new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        Microsoft.CodeAnalysis.Emit.EmitResult emitResult = compilation.Emit(ms);
                        if (!emitResult.Success)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (Microsoft.CodeAnalysis.Diagnostic diagnostic in emitResult.Diagnostics)
                            {
                                sb.AppendLine(diagnostic.ToString());
                            }

                            result = sb.ToString();
                        }
                        else
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            Assembly assembly = Assembly.Load(ms.ToArray());
                            Type pageClassType = assembly.GetType("PageClass");
                            if (pageClassType == null)
                            {
                                throw new Exception("Compiling PageClass failed");
                            }

                            if (((pageInfo.JobsInfo == null) || (pageInfo.JobsInfo.JobList.Count == 0)) &&
                                ((pageInfo.ErrorsInfo == null) || (pageInfo.ErrorsInfo.EcuList.Count == 0)))
                            {
                                if (pageClassType.GetMethod("ExecuteJob") == null)
                                {
                                    throw new Exception("No ExecuteJob method");
                                }
                            }

                            object pageClassInstance = Activator.CreateInstance(pageClassType);
                            if (pageClassInstance == null)
                            {
                                throw new Exception("Compiling PageClass failed");
                            }

                            pageInfo.ClassObject = pageClassInstance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pageInfo.ClassObject = null;
                if (string.IsNullOrEmpty(result))
                {
                    result = EdiabasNet.GetExceptionText(ex, false, false);
                }
            }

            return result;
        }

        public static Dictionary<string, int> ExtractKeyWords(string archiveFilename, string wordRegEx, int maxWords, string lineRegEx, ProgressZipDelegate progressHandler)
        {
            Dictionary<string, int> wordDict = new Dictionary<string, int>();
            ZipFile zf = null;
            try
            {
                Regex regExWord = new Regex(wordRegEx);
                Regex regExLine = string.IsNullOrWhiteSpace(lineRegEx) ? null: new Regex(lineRegEx);
                long index = 0;
                FileStream fs = File.OpenRead(archiveFilename);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / zf.Count)))
                        {
                            return null;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    using (Stream zipStream = zf.GetInputStream(zipEntry))
                    {
                        using (StreamReader sr = new StreamReader(zipStream))
                        {
                            bool exit = false;
                            for (; ; )
                            {
                                string line = sr.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }

                                if (regExLine != null)
                                {
                                    MatchCollection matchesLine = regExLine.Matches(line);
                                    if ((matchesLine.Count == 1) && (matchesLine[0].Groups.Count > 1))
                                    {
                                        for (int match = 1; match < matchesLine[0].Groups.Count; match++)
                                        {
                                            if (matchesLine[0].Groups[match].Success)
                                            {
                                                string key = matchesLine[0].Groups[match].Value;
                                                if (!string.IsNullOrWhiteSpace(key))
                                                {
                                                    if (wordDict.ContainsKey(key))
                                                    {
                                                        wordDict[key] += 1;
                                                    }
                                                    else
                                                    {
                                                        wordDict[key] = 1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                string[] words = line.Split(' ', '\t', '\n', '\r');
                                foreach (string word in words)
                                {
                                    if (string.IsNullOrEmpty(word))
                                    {
                                        continue;
                                    }
                                    if (regExWord.IsMatch(word))
                                    {
                                        if (wordDict.ContainsKey(word))
                                        {
                                            wordDict[word] += 1;
                                        }
                                        else
                                        {
                                            if (maxWords > 0 && wordDict.Count > maxWords)
                                            {
                                                exit = true;
                                            }
                                            wordDict[word] = 1;
                                        }
                                    }
                                }
                                if (exit)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    index++;
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return wordDict;
        }

        public static string CreateValidFileName(string s, char replaceChar = '_', char[] includeChars = null)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            if (includeChars != null) invalid = invalid.Union(includeChars).ToArray();
            return string.Join(string.Empty, s.ToCharArray().Select(o => invalid.Contains(o) ? replaceChar : o));
        }

        public bool InitReaderThread(string bmwPath, string vagPath, InitThreadFinishDelegate handler)
        {
            if (SelectedManufacturer == ManufacturerType.Bmw)
            {
                return InitEcuFunctionReaderThread(bmwPath, handler);
            }

            return InitUdsReaderThread(vagPath, handler);
        }

        public bool IsInitUdsReaderRequired()
        {
            if (OldVagMode || VagUdsChecked || SelectedManufacturer == ManufacturerType.Bmw)
            {
                return false;
            }

            return true;
        }

        public bool InitUdsReaderThread(string vagPath, InitThreadFinishDelegate handler)
        {
            if (!IsInitUdsReaderRequired())
            {
                return false;
            }

            if (IsUdsReaderJobRunning())
            {
                return false;
            }

            if (!Directory.Exists(vagPath))
            {
                AlertDialog altertDialog = new AlertDialog.Builder(_context)
                    .SetMessage(Resource.String.vag_uds_error)
                    .SetTitle(Resource.String.alert_title_error)
                    .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                    .Show();
                if (altertDialog != null)
                {
                    altertDialog.DismissEvent += (o, eventArgs) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        handler?.Invoke(false);
                    };

                }
                return true;
            }

            bool abortInit = false;
            CustomProgressDialog progress = new CustomProgressDialog(_activity);
            progress.SetMessage(_activity.GetString(Resource.String.vag_uds_init));
            progress.Indeterminate = true;
            progress.ButtonAbort.Visibility = ViewStates.Visible;
            progress.ButtonAbort.Enabled = true;
            progress.AbortClick = sender =>
            {
                _activity.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    abortInit = true;
                    progress.ButtonAbort.Enabled = false;
                });
            };
            progress.Show();
            SetLock(LockTypeCommunication);
            _udsReaderThread = new Thread(() =>
            {
                long lastPercent = -1;
                bool result = InitUdsReader(vagPath, out string errorMessage, percent =>
                {
                    if (_terminating)
                    {
                        return true;
                    }

                    if (lastPercent == percent)
                    {
                        return abortInit;
                    }

                    lastPercent = percent;
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }

                        if (progress.Indeterminate)
                        {
                            progress.Indeterminate = false;
                        }
                        progress.Progress = (int)percent;
                    });
                    return abortInit;
                });

                _activity?.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress = null;
                    SetLock(LockType.None);
                    if (!result)
                    {
                        string message = _context.GetString(Resource.String.vag_uds_error);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message += "\r\n" + errorMessage;
                        }

                        AlertDialog altertDialog = new AlertDialog.Builder(_context)
                            .SetMessage(message)
                            .SetTitle(Resource.String.alert_title_error)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                            .Show();
                        if (altertDialog != null)
                        {
                            altertDialog.DismissEvent += (o, eventArgs) =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                handler?.Invoke(false);
                            };
                        }

                        return;
                    }

                    handler?.Invoke(VagUdsActive);
                });
            });
            _udsReaderThread.Start();
            return true;
        }

        public bool IsUdsReaderJobRunning()
        {
            if (_udsReaderThread == null)
            {
                return false;
            }
            if (_udsReaderThread.IsAlive)
            {
                return true;
            }
            _udsReaderThread = null;
            return false;
        }

        public static void ResetUdsReader()
        {
            VagUdsChecked = false;
            VagUdsActive = false;
            _udsReaderDict = null;
        }

        public static bool InitUdsReader(string vagDir, out string errorMessage, InitThreadProgressDelegate progressHandler = null)
        {
            errorMessage = null;
            if (OldVagMode)
            {
                return true;
            }
            try
            {
                VagUdsActive = false;
                if (!Directory.Exists(vagDir))
                {
                    return false;
                }

                if (_udsReaderDict == null)
                {
                    _udsReaderDict = new Dictionary<string, UdsReader>();
                    string[] subdirs = Directory.GetDirectories(vagDir, "*", SearchOption.TopDirectoryOnly);
                    long maxProgress = subdirs.Length * 5;
                    long progressCount = 0;
                    foreach (string subdir in subdirs)
                    {
                        string langDir = Path.GetFileName(subdir) ?? string.Empty;
                        string key = langDir.ToLowerInvariant();
                        if (key.Length == 2)
                        {
                            if (!_udsReaderDict.ContainsKey(key))
                            {
                                UdsReader udsReader = new UdsReader();
                                if (!udsReader.Init(subdir,
                                    new HashSet<UdsReader.SegmentType>
                                    {
                                        UdsReader.SegmentType.Adp,
                                        UdsReader.SegmentType.Mwb,
                                        UdsReader.SegmentType.Dtc
                                    }, out errorMessage, increment =>
                                    {
                                        progressCount += increment;
                                        if (progressHandler != null)
                                        {
                                            return progressHandler.Invoke(progressCount * 100 / maxProgress);
                                        }

                                        return false;
                                    }))
                                {
                                    return false;
                                }

                                udsReader.LanguageDir = langDir;
                                _udsReaderDict.Add(key, udsReader);
                            }
                        }
                    }
                }

                if (!_udsReaderDict.ContainsKey(DefaultLang))
                {
                    return false;   // no default reader
                }

                VagUdsActive = true;
                VagUdsChecked = true;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = EdiabasNet.GetExceptionText(ex);
                return false;
            }
        }

        public static UdsReader GetUdsReader(string fileName = null)
        {
            try
            {
                UdsReader udsReader;
                if (!string.IsNullOrEmpty(fileName))
                {
                    DirectoryInfo dirInfoParent = Directory.GetParent(fileName);
                    for (int i = 0; i < 3; i++)
                    {
                        if (dirInfoParent == null)
                        {
                            break;
                        }

                        string key = dirInfoParent.Name.ToLowerInvariant();
                        if (_udsReaderDict.TryGetValue(key, out udsReader))
                        {
                            return udsReader;
                        }
                        dirInfoParent = Directory.GetParent(dirInfoParent.FullName);
                    }
                }

                string lang = GetCurrentLanguageStatic();
                if (_udsReaderDict.TryGetValue(lang, out udsReader))
                {
                    return udsReader;
                }

                if (_udsReaderDict.TryGetValue(DefaultLang, out udsReader))
                {
                    return udsReader;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        public static List<UdsReader> GetUdsReaderList(UdsReader ignoreReader = null)
        {
            try
            {
                List<UdsReader> udsReaderList = new List<UdsReader>(_udsReaderDict.Values);
                foreach (KeyValuePair<string, UdsReader> keyValuePair in _udsReaderDict)
                {
                    if (ignoreReader != null && keyValuePair.Value == ignoreReader)
                    {
                        continue;
                    }

                    udsReaderList.Add(keyValuePair.Value);
                }

                return udsReaderList;
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }


        public bool IsInitEcuFunctionsRequired()
        {
            if (!UseBmwDatabase || SelectedManufacturer != ManufacturerType.Bmw)
            {
                return false;
            }

            if (EcuFunctionsChecked && _ecuFunctionReader != null)
            {
                if (_ecuFunctionReader.IsInitRequired(GetCurrentLanguage()))
                {
                    EcuFunctionsChecked = false;
                }
            }

            if (EcuFunctionsChecked)
            {
                return false;
            }

            return true;
        }

        public bool InitEcuFunctionReaderThread(string bmwPath, InitThreadFinishDelegate handler)
        {
            if (!IsInitEcuFunctionsRequired())
            {
                return false;
            }

            if (IsEcuFuncReaderJobRunning())
            {
                return false;
            }

            if (!Directory.Exists(bmwPath))
            {
                AlertDialog altertDialog = new AlertDialog.Builder(_context)
                    .SetMessage(Resource.String.bmw_ecu_func_error)
                    .SetTitle(Resource.String.alert_title_error)
                    .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                    .Show();
                if (altertDialog != null)
                {
                    altertDialog.DismissEvent += (o, eventArgs) =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        handler?.Invoke(false);
                    };
                }
                return true;
            }

            bool abortInit = false;
            CustomProgressDialog progress = new CustomProgressDialog(_activity);
            progress.SetMessage(_activity.GetString(Resource.String.bmw_ecu_func_init));
            progress.Indeterminate = true;
            progress.Progress = 0;
            progress.Max = 100;
            progress.ButtonAbort.Visibility = ViewStates.Visible;
            progress.ButtonAbort.Enabled = true;
            progress.AbortClick = sender =>
            {
                _activity.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    abortInit = true;
                    progress.ButtonAbort.Enabled = false;
                });
            };
            progress.Show();
            SetLock(LockTypeCommunication);
            _ecuFuncReaderThread = new Thread(() =>
            {
                long lastPercent = -1;
                bool result = InitEcuFunctionReader(bmwPath, out string errorMessage, percent =>
                {
                    if (_terminating)
                    {
                        return true;
                    }

                    if (lastPercent == percent)
                    {
                        return abortInit;
                    }

                    lastPercent = percent;
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }

                        if (progress.Indeterminate)
                        {
                            progress.Indeterminate = false;
                        }
                        progress.Progress = (int)percent;
                    });
                    return abortInit;
                });

                _activity?.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress = null;
                    SetLock(LockType.None);
                    if (!result)
                    {
                        string message = _context.GetString(Resource.String.bmw_ecu_func_error);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message += "\r\n" + errorMessage;
                        }

                        AlertDialog altertDialog = new AlertDialog.Builder(_context)
                            .SetMessage(message)
                            .SetTitle(Resource.String.alert_title_error)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                            .Show();
                        if (altertDialog != null)
                        {
                            altertDialog.DismissEvent += (o, eventArgs) =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                handler?.Invoke(false);
                            };
                        }

                        return;
                    }

                    handler?.Invoke(EcuFunctionsActive);
                });
            });
            _ecuFuncReaderThread.Start();
            return true;
        }

        public bool IsEcuFuncReaderJobRunning()
        {
            if (_ecuFuncReaderThread == null)
            {
                return false;
            }
            if (_ecuFuncReaderThread.IsAlive)
            {
                return true;
            }
            _ecuFuncReaderThread = null;
            return false;
        }

        public static void ResetEcuFunctionReader()
        {
            _ecuFunctionReader = null;
            EcuFunctionsChecked = false;
            EcuFunctionsActive = false;
        }

        public static bool InitEcuFunctionReader(string bmwPath, out string errorMessage, InitThreadProgressDelegate progressHandler = null)
        {
            errorMessage = null;

            if (!UseBmwDatabase)
            {
                return true;
            }

            try
            {
                EcuFunctionsActive = false;
                if (!Directory.Exists(bmwPath))
                {
                    return false;
                }

                if (_ecuFunctionReader == null)
                {
                    _ecuFunctionReader = new EcuFunctionReader(bmwPath);
                }

                if (!_ecuFunctionReader.Init(GetCurrentLanguageStatic(), out errorMessage, progress =>
                    {
                        if (progressHandler != null)
                        {
                            return progressHandler.Invoke(progress);
                        }

                        return false;
                    }))
                {
                    return false;
                }

                EcuFunctionsActive = true;
                EcuFunctionsChecked = true;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = EdiabasNet.GetExceptionText(ex);
            }

            return false;
        }

        private static bool IsEmulator()
        {
            try
            {
                string brand = Build.Brand ?? string.Empty;
                string device = Build.Device ?? string.Empty;
                string fingerprint = Build.Fingerprint ?? string.Empty;
                string hardware = Build.Hardware ?? string.Empty;
                string model = Build.Model ?? string.Empty;
                string manufact = Build.Manufacturer ?? string.Empty;
                string product = Build.Product ?? string.Empty;

                bool isEmulator =
                    (brand.StartsWith("generic") && device.StartsWith("generic"))
                    || fingerprint.StartsWith("generic")
                    || fingerprint.StartsWith("unknown")
                    || hardware.Contains("goldfish")
                    || hardware.Contains("ranchu")
                    || model.Contains("google_sdk")
                    || model.Contains("Emulator")
                    || model.Contains("Android SDK built for x86")
                    || manufact.Contains("Genymotion")
                    || product.Contains("sdk_google")
                    || product.Contains("google_sdk")
                    || product.Contains("sdk")
                    || product.Contains("sdk_x86")
                    || product.Contains("sdk_gphone64_arm64")
                    || product.Contains("vbox86p")
                    || product.Contains("emulator")
                    || product.Contains("simulator");

                return isEmulator;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource != null)
                {
                    using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }
            }
        }

        public static bool IsCommunicationError(string exceptionText)
        {
            if (string.IsNullOrEmpty(exceptionText))
            {
                return false;
            }
            if (exceptionText.Contains("executeJob aborted"))
            {
                return false;
            }
            return true;
        }

        public static string GetSettingsFileName(bool secondLocation = false)
        {
            Java.IO.File filesDir = secondLocation ?
                Android.App.Application.Context.NoBackupFilesDir : Android.App.Application.Context.FilesDir;

            if (filesDir == null)
            {
                return string.Empty;
            }

            return Path.Combine(filesDir.AbsolutePath, SettingsFile);
        }

        public static StorageData GetStorageData(SettingsMode settingsMode = SettingsMode.All)
        {
            for (int i = 0; i < 2; i++)
            {
                string settingsFile = GetSettingsFileName(i > 0);
                if (string.IsNullOrEmpty(settingsFile))
                {
                    continue;
                }

                StorageData storageData = GetStorageDataFromFile(settingsFile, settingsMode);
                if (storageData != null)
                {
                    return storageData;
                }

                string backupFileName = settingsFile + BackupExt;
                if (File.Exists(backupFileName))
                {
                    storageData = GetStorageDataFromFile(backupFileName, settingsMode);
                    if (storageData != null)
                    {
                        try
                        {
                            File.Copy(backupFileName, settingsFile, true);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        return storageData;
                    }
                }

            }
            return new StorageData();
        }

        public static StorageData GetStorageDataFromFile(string fileName, SettingsMode settingsMode = SettingsMode.All)
        {
            StorageData storageData = null;
            try
            {
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    try
                    {
                        lock (GlobalSettingLockObject)
                        {
                            XmlAttributeOverrides storageClassAttributes = GetStoreXmlAttributeOverrides(settingsMode);
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageData), storageClassAttributes);
                            using (StreamReader sr = new StreamReader(fileName))
                            {
                                storageData = xmlSerializer.Deserialize(sr) as StorageData;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        storageData = null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return storageData;
        }

        public static XmlAttributeOverrides GetStoreXmlAttributeOverrides(SettingsMode settingsMode)
        {
            if (settingsMode == SettingsMode.All)
            {
                return null;
            }

            StorageData storageData = new StorageData();
            Type storageType = storageData.GetType();
            XmlAttributes ignoreXmlAttributes = new XmlAttributes
            {
                XmlIgnore = true
            };

            XmlAttributeOverrides storageClassAttributes = new XmlAttributeOverrides();
            storageClassAttributes.Add(storageType, nameof(storageData.LastAppState), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.UpdateCheckTime), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.UpdateSkipVersion), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.LastVersionCode), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.StorageRequirementsAccepted), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.BatteryWarnings), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.BatteryWarningVoltage), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.SerialInfo), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.AdapterBlacklist), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.LastAdapterSerial), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.AppId), ignoreXmlAttributes);
            if (settingsMode == SettingsMode.Public)
            {
                storageClassAttributes.Add(storageType, nameof(storageData.SelectedEnetIp), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.SelectedElmWifiIp), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.SelectedDeepObdWifiIp), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.MtcBtDisconnectWarnShown), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.DeviceName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.DeviceAddress), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.ConfigFileName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.XmlEditorPackageName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.XmlEditorClassName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.RecentLocale), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.RecentConfigFiles), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.CustomStorageMedia), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.UsbFirmwareFileName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.YandexApiKey), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.IbmTranslatorApiKey), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.IbmTranslatorUrl), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.GoogleApisUrl), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.EmailAddress), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.TraceInfo), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.DisableFileNameEncoding), ignoreXmlAttributes);
            }

            return storageClassAttributes;
        }

        public static string GetLocaleSetting(Context context = null, bool updateRecent = false)
        {
            try
            {
                bool useRecent = false;
                string languageTags = null;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (context != null)
                    {
#pragma warning disable CA1416
                        if (context.ApplicationContext?.GetSystemService(Java.Lang.Class.FromType(typeof(Android.App.LocaleManager))) is Android.App.LocaleManager localeManager)
                        {
                            LocaleList appLocales = localeManager.ApplicationLocales;
                            languageTags = appLocales.ToLanguageTags();
                        }
#pragma warning restore CA1416
                    }
                }

                if (languageTags == null)
                {
                    AndroidX.Core.OS.LocaleListCompat appLocales = AppCompatDelegate.ApplicationLocales;
                    languageTags = appLocales.ToLanguageTags();
                    useRecent = true;
                }

                if (!string.IsNullOrEmpty(languageTags))
                {
                    string[] languages = languageTags.Split(',');
                    if (languages.Length > 0)
                    {
                        string language = languages[0];
                        if (language.Length > 2)
                        {
                            language = language.Substring(0, 2);
                        }

                        if (language.Length == 2)
                        {
                            if (updateRecent)
                            {
                                RecentLocale = language;
                            }
                            return language;
                        }
                    }
                }

                if (updateRecent)
                {
                    RecentLocale = string.Empty;
                    return string.Empty;
                }

                if (useRecent && RecentLocale != null)
                {
                    return RecentLocale;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool GetStorageThemeSettings(bool updateTheme)
        {
            StorageData storageData = GetStorageData();

            if (updateTheme)
            {
                SelectedTheme = storageData.SelectedTheme;
            }

            return true;
        }

        public static void GetThemeSettings(InstanceDataCommon instanceData = null)
        {
            try
            {
                if (instanceData == null)
                {
                    if (SelectedTheme == null)
                    {
                        GetStorageThemeSettings(true);
                    }

                    return;
                }

                instanceData.LastThemeType = SelectedTheme;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static Context GetLocaleContext(Context context)
        {
            try
            {
                string selectedLocale = GetLocaleSetting(context);
                if (string.IsNullOrEmpty(selectedLocale))
                {
                    return context;
                }

                Java.Util.Locale locale = new Java.Util.Locale(selectedLocale);
                Resources resources = context.Resources;
                Configuration configuration = resources?.Configuration;
                if (configuration != null)
                {
                    configuration.SetLocale(locale);
                    return context.CreateConfigurationContext(configuration);
                }

                return context;
            }
            catch (Exception)
            {
                return context;
            }
        }

        public void CheckSettingsVersionChange(InstanceDataCommon instanceData)
        {
            if (instanceData.LastVersionCode != VersionCode)
            {
                instanceData.StorageRequirementsAccepted = false;
                instanceData.UpdateCheckTime = DateTime.MinValue.Ticks;
                instanceData.UpdateSkipVersion = -1;
                BatteryWarnings = 0;
                BatteryWarningVoltage = 0;
            }
        }

        public bool GetSettings(InstanceDataCommon instanceData, SettingsMode settingsMode, bool forceInit)
        {
            bool result = GetSettingsFromFile(instanceData, null, settingsMode, forceInit);
            EdiabasNet.EncodeFileNameKey = AppId.ToLowerInvariant();
            return result;
        }

        public bool GetSettingsFromFile(InstanceDataCommon instanceData, string fileName, SettingsMode settingsMode, bool forceInit = false)
        {
            if (instanceData == null)
            {
                return false;
            }

            bool import = settingsMode != SettingsMode.All;
            string hash = string.Empty;
            try
            {
                bool init = false;
                if (!StaticDataInitialized || forceInit)
                {
                    init = true;
                    SetDefaultSettings();
                }

                StorageData storageData;
                if (string.IsNullOrEmpty(fileName))
                {
                    storageData = GetStorageData(settingsMode);
                }
                else
                {
                    storageData = GetStorageDataFromFile(fileName, settingsMode);
                }

                if (storageData == null)
                {
                    return false;
                }

                hash = storageData.CalcualeHash();

                if (init || import)
                {
                    instanceData.LastAppState = storageData.LastAppState;
                    instanceData.LastSelectedJobIndex = storageData.LastSelectedJobIndex;
                    SelectedEnetIp = storageData.SelectedEnetIp;
                    SelectedElmWifiIp = storageData.SelectedElmWifiIp;
                    SelectedDeepObdWifiIp = storageData.SelectedDeepObdWifiIp;
                    MtcBtDisconnectWarnShown = storageData.MtcBtDisconnectWarnShown;
                    instanceData.DeviceName = storageData.DeviceName;
                    instanceData.DeviceAddress = storageData.DeviceAddress;
                    instanceData.ConfigFileName = storageData.ConfigFileName;
                    instanceData.UpdateCheckTime = storageData.UpdateCheckTime;
                    instanceData.UpdateSkipVersion = storageData.UpdateSkipVersion;
                    instanceData.LastVersionCode = storageData.LastVersionCode;
                    instanceData.StorageRequirementsAccepted = storageData.StorageRequirementsAccepted;
                    instanceData.XmlEditorPackageName = storageData.XmlEditorPackageName;
                    instanceData.XmlEditorClassName = storageData.XmlEditorClassName;

                    SelectedTheme = storageData.SelectedTheme;
                    RecentLocale = storageData.RecentLocale;
                    SetRecentConfigList(storageData.RecentConfigFiles);
                    CustomStorageMedia = storageData.CustomStorageMedia;
                    CopyToAppSrc = storageData.CopyToAppSrc;
                    CopyToAppDst = storageData.CopyToAppDst;
                    CopyFromAppSrc = storageData.CopyFromAppSrc;
                    CopyFromAppDst = storageData.CopyFromAppDst;
                    UsbFirmwareFileName = storageData.UsbFirmwareFileName;
                    EnableTranslation = storageData.EnableTranslation;
                    YandexApiKey = storageData.YandexApiKey;
                    IbmTranslatorApiKey = storageData.IbmTranslatorApiKey;
                    IbmTranslatorUrl = storageData.IbmTranslatorUrl;
                    DeeplApiKey = storageData.DeeplApiKey;
                    YandexCloudApiKey = storageData.YandexCloudApiKey;
                    YandexCloudFolderId = storageData.YandexCloudFolderId;
                    GoogleApisUrl = storageData.GoogleApisUrl;
                    Translator = storageData.Translator;
                    ShowBatteryVoltageWarning = storageData.ShowBatteryVoltageWarning;
                    BatteryWarnings = storageData.BatteryWarnings;
                    BatteryWarningVoltage = storageData.BatteryWarningVoltage;
                    SetSerialInfoList(storageData.SerialInfo);
                    AdapterBlacklist = storageData.AdapterBlacklist;
                    LastAdapterSerial = storageData.LastAdapterSerial;
                    EmailAddress = storageData.EmailAddress;
                    TraceInfo = storageData.TraceInfo;
                    AppId = storageData.AppId;
                    AutoHideTitleBar = storageData.AutoHideTitleBar;
                    SuppressTitleBar = storageData.SuppressTitleBar;
                    FullScreenMode = storageData.FullScreenMode;
                    SwapMultiWindowOrientation = storageData.SwapMultiWindowOrientation;
                    SelectedInternetConnection = storageData.SelectedInternetConnection;
                    SelectedManufacturer = storageData.SelectedManufacturer;
                    BtEnbaleHandling = storageData.BtEnbaleHandling;
                    BtDisableHandling = storageData.BtDisableHandling;
                    LockTypeCommunication = storageData.LockTypeCommunication;
                    LockTypeLogging = storageData.LockTypeLogging;
                    StoreDataLogSettings = storageData.StoreDataLogSettings;
                    if (StoreDataLogSettings)
                    {
                        instanceData.DataLogActive = storageData.DataLogActive;
                        instanceData.DataLogAppend = storageData.DataLogAppend;
                    }
                    AutoConnectHandling = storageData.AutoConnectHandling;
                    UpdateCheckDelay = storageData.UpdateCheckDelay;
                    DoubleClickForAppExit = storageData.DoubleClickForAppExit;
                    SendDataBroadcast = storageData.SendDataBroadcast;
                    CheckCpuUsage = storageData.CheckCpuUsage;
                    CheckEcuFiles = storageData.CheckEcuFiles;
                    OldVagMode = storageData.OldVagMode;
                    UseBmwDatabase = storageData.UseBmwDatabase;
                    ShowOnlyRelevantErrors = storageData.ShowOnlyRelevantErrors;
                    ScanAllEcus = storageData.ScanAllEcus;
                    CollectDebugInfo = storageData.CollectDebugInfo;
                    CompressTrace = storageData.CompressTrace;
                    DisableNetworkCheck = storageData.DisableNetworkCheck;
                    DisableFileNameEncoding = storageData.DisableFileNameEncoding;

                    CheckSettingsVersionChange(instanceData);
                }

                if (!import)
                {
                    instanceData.LastThemeType = SelectedTheme;
                }

                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                StaticDataInitialized = true;
                if (!import)
                {
                    instanceData.LastSettingsHash = hash;
                }

                instanceData.GetSettingsCalled = true;
            }
            return false;
        }

        public bool StoreSettings(InstanceDataCommon instanceData, SettingsMode settingsMode, out string errorMessage)
        {
            string settingsFile = GetSettingsFileName();
            if (!StoreSettingsToFile(instanceData, settingsFile, settingsMode, out errorMessage, true))
            {
                return false;
            }

            EdiabasNet.EncodeFileNameKey = AppId.ToLowerInvariant();
            string settingsFile2 = GetSettingsFileName(true);
            if (File.Exists(settingsFile))
            {
                try
                {
                    File.Copy(settingsFile, settingsFile2, true);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            string backupFileName = settingsFile + BackupExt;
            string backupFile2Name = settingsFile2 + BackupExt;
            if (File.Exists(backupFileName))
            {
                try
                {
                    File.Copy(backupFileName, backupFile2Name, true);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            else
            {
                try
                {
                    File.Delete(backupFile2Name);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return true;
        }

        public bool StoreSettingsToFile(InstanceDataCommon instanceData, string fileName, SettingsMode settingsMode, out string errorMessage, bool createBackup = false)
        {
            errorMessage = null;
            if (instanceData == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            bool export = settingsMode != SettingsMode.All;
            try
            {
                if (!StaticDataInitialized || !instanceData.GetSettingsCalled)
                {
                    return false;
                }

                string settingsDir = Path.GetDirectoryName(fileName);
                if (string.IsNullOrEmpty(settingsDir))
                {
                    return false;
                }

                if (!Directory.Exists(settingsDir))
                {
                    try
                    {
                        Directory.CreateDirectory(settingsDir);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                lock (GlobalSettingLockObject)
                {
                    StorageData storageData = new StorageData(instanceData, this, true);
                    string hash = storageData.CalcualeHash();

                    if (!export && string.Compare(hash, instanceData.LastSettingsHash, StringComparison.Ordinal) == 0)
                    {
                        return true;
                    }

                    XmlAttributeOverrides storageClassAttributes = GetStoreXmlAttributeOverrides(settingsMode);
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageData), storageClassAttributes);
                    Java.IO.File tempFile = Java.IO.File.CreateTempFile("Settings", ".xml", Android.App.Application.Context.CacheDir);
                    if (tempFile == null)
                    {
                        return false;
                    }

                    tempFile.DeleteOnExit();
                    string tempFileName = tempFile.AbsolutePath;
                    using (StreamWriter sw = new StreamWriter(tempFileName))
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(sw, settings))
                        {
                            xmlSerializer.Serialize(writer, storageData);
                        }
                    }

                    if (createBackup && File.Exists(fileName))
                    {
                        try
                        {
                            string backupFileName = fileName + BackupExt;
                            File.Move(fileName, backupFileName, true);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    File.Move(tempFileName, fileName, true);

                    if (!export)
                    {
                        instanceData.LastSettingsHash = hash;
                    }
                }

                _backupManager?.DataChanged();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = EdiabasNet.GetExceptionText(ex, false, false);
            }
            return false;
        }

        public static AutoConnectType GetAutoConnectSetting()
        {
            StorageData storageData = GetStorageData();
            return storageData.AutoConnectHandling;
        }

        public bool UpdateDirectories(InstanceDataCommon instanceData)
        {
            if (instanceData == null)
            {
                return false;
            }

            instanceData.AppDataPath = string.Empty;
            instanceData.EcuPath = string.Empty;
            instanceData.VagPath = string.Empty;
            instanceData.BmwPath = string.Empty;
            instanceData.SimulationPath = string.Empty;
            instanceData.UserEcuFiles = false;
            if (string.IsNullOrEmpty(CustomStorageMedia))
            {
                if (string.IsNullOrEmpty(ExternalWritePath))
                {
                    if (string.IsNullOrEmpty(ExternalPath))
                    {
                        return false;
                    }
                    instanceData.AppDataPath = Path.Combine(ExternalPath, AppFolderName);
                }
                else
                {
                    instanceData.AppDataPath = ExternalWritePath;
                }
            }
            else
            {
                instanceData.AppDataPath = Path.Combine(CustomStorageMedia, AppFolderName);
            }

            instanceData.EcuPath = Path.Combine(instanceData.AppDataPath, ManufacturerEcuDirName);
            instanceData.VagPath = Path.Combine(instanceData.AppDataPath, EcuBaseDir, VagBaseDir);
            instanceData.BmwPath = Path.Combine(instanceData.AppDataPath, EcuBaseDir, BmwBaseDir);
            instanceData.TraceBackupDir = Path.Combine(instanceData.AppDataPath, TraceBackupDir);
            instanceData.PackageAssembliesDir = Path.Combine(instanceData.AppDataPath, PackageAssembliesDir);

            JobReader jobReader = JobReader;
            if (jobReader != null)
            {
                if (jobReader.Interface == InterfaceType.Simulation)
                {
                    try
                    {
                        string simulationPath = jobReader.SimulationPath;
                        if (string.IsNullOrEmpty(simulationPath) || !Directory.Exists(simulationPath))
                        {
                            string xmlFileName = jobReader.XmlFileName;
                            if (!string.IsNullOrEmpty(xmlFileName))
                            {
                                simulationPath = Path.GetDirectoryName(xmlFileName);
                            }
                        }

                        instanceData.SimulationPath = simulationPath;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return true;
        }

        public List<string> GetAllStorageMedia()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                List<string> storageList = new List<string>();
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                if (externalFilesDirs != null)
                {
                    foreach (Java.IO.File file in externalFilesDirs)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (file != null)
                        {
                            string extState = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                                Android.OS.Environment.GetExternalStorageState(file) : Android.OS.Environment.ExternalStorageState;
                            if (extState != null && extState.Equals(Android.OS.Environment.MediaMounted) && IsWritable(file.AbsolutePath))
                            {
                                storageList.Add(file.AbsolutePath);
                            }
                        }
                    }
                }
                return storageList;
            }

            string procMounts = ReadProcMounts();
            return ParseStorageMedia(procMounts);
        }

        public static string GetDocumentPath(DocumentFile documentFile)
        {
            if (documentFile?.Uri == null)
            {
                return null;
            }

            try
            {
                string docId = Android.Provider.DocumentsContract.GetTreeDocumentId(documentFile.Uri);
                if (!string.IsNullOrEmpty(docId))
                {
                    string[] parts = docId.Split(':');
                    if (parts.Length > 1)
                    {
                        string volumeId = parts[0];
                        string docPath = parts[1];
                        string volumePath = GetVolumePath(volumeId);
                        if (!string.IsNullOrEmpty(docPath) && !string.IsNullOrEmpty(volumePath))
                        {
                            string fullPath = Path.Combine(volumePath, docPath);
                            return fullPath;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return documentFile.Uri.Path;
        }

        public static List<string> GetPersistedStorages()
        {
            List<string> storageList = new List<string>();
            if (IsDocumentTreeSupported())
            {
                IList<UriPermission> uriPermissions = Android.App.Application.Context.ContentResolver?.PersistedUriPermissions;
                if (uriPermissions != null)
                {
                    foreach (UriPermission uriPermission in uriPermissions)
                    {
                        try
                        {
                            if (uriPermission.Uri != null && uriPermission.IsWritePermission)
                            {
                                string docId = Android.Provider.DocumentsContract.GetTreeDocumentId(uriPermission.Uri);
                                if (!string.IsNullOrEmpty(docId))
                                {
                                    string[] parts = docId.Split(':');
                                    if (parts.Length > 1)
                                    {
                                        string volumeId = parts[0];
                                        string docPath = parts[1];
                                        string volumePath = GetVolumePath(volumeId);
                                        if (!string.IsNullOrEmpty(docPath) && !string.IsNullOrEmpty(volumePath))
                                        {
                                            string fullPath = Path.Combine(volumePath, docPath);
                                            storageList.Add(fullPath);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }

            return storageList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static string GetVolumePath(string volumeId)
        {
            try
            {
                if (!IsDocumentTreeSupported())
                {
                    return null;
                }

                if (string.IsNullOrEmpty(volumeId))
                {
                    return null;
                }

                StorageManager storageManager = Android.App.Application.Context.GetSystemService(Context.StorageService) as StorageManager;
                IList<StorageVolume> storageVolumes = storageManager?.StorageVolumes;
                if (storageVolumes == null)
                {
                    return null;
                }

                foreach (StorageVolume storageVolume in storageVolumes)
                {
                    if (storageVolume.IsPrimary && PrimaryVolumeName.Equals(volumeId))
                    {
                        return storageVolume.Directory?.Path;
                    }

                    string uuid = storageVolume.Uuid;
                    if (!string.IsNullOrEmpty(uuid) && uuid.Equals(volumeId))
                    {
                        return storageVolume.Directory?.Path;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool RequestCopyDocumentsThread(DocumentFile documentSrc, DocumentFile documentDst, CopyDocumentsThreadFinishDelegate handler)
        {
            try
            {
                if (documentSrc == null || documentDst == null)
                {
                    return false;
                }

                string documentPath = GetDocumentPath(documentDst);
                if (string.IsNullOrEmpty(documentPath))
                {
                    return false;
                }

                documentPath = Path.Combine(documentPath, documentSrc.Name);
                bool exists = documentDst.FindFile(documentSrc.Name) != null;
                int resIdMsg = exists ? Resource.String.copy_documents_exists : Resource.String.copy_documents_create;
                int resIdTitle = exists ? Resource.String.alert_title_warning : Resource.String.alert_title_question;
                string message = string.Format(_context.GetString(resIdMsg), documentPath);
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        CopyDocumentsThread(documentSrc, documentDst, handler);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(message)
                    .SetTitle(resIdTitle)
                    .Show();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool CopyDocumentsThread(DocumentFile documentSrc, DocumentFile documentDst, CopyDocumentsThreadFinishDelegate handler)
        {
            if (IsCopyDocumentJobRunning())
            {
                return false;
            }

            bool aborted = false;
            CustomProgressDialog progress = new CustomProgressDialog(_activity);
            progress.SetMessage(_activity.GetString(Resource.String.copy_documents_progress));
            progress.Indeterminate = false;
            progress.Progress = 0;
            progress.Max = 100;
            progress.AbortClick = sender =>
            {
                aborted = true;
            };
            progress.Show();
            SetLock(LockTypeCommunication);
            _copyDocumentThread = new Thread(() =>
            {
                bool result = false;
                int docCount = GetDocumentCount(documentSrc, name =>
                {
                    if (_disposed)
                    {
                        return false;
                    }

                    if (aborted)
                    {
                        return false;
                    }

                    return true;
                });

                if (docCount >= 0)
                {
                    if (docCount == 0)
                    {
                        docCount++;
                    }

                    int fileCount = 0;
                    result = CopyDocumentsRecursive(documentSrc, documentDst, name =>
                    {
                        if (_disposed)
                        {
                            return false;
                        }

                        if (aborted)
                        {
                            return false;
                        }

                        fileCount++;

                        _activity?.RunOnUiThread(() =>
                        {
                            progress.Progress = fileCount * 100 / docCount;
                        });
                        return true;
                    });
                }

                _activity?.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    progress.Dismiss();
                    progress = null;
                    SetLock(LockType.None);
                    if (!result && !aborted)
                    {
                        new AlertDialog.Builder(_context)
                            .SetMessage(Resource.String.copy_documents_error)
                            .SetTitle(Resource.String.alert_title_error)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                            .Show();
                    }
                    handler?.Invoke(result, aborted);
                });
            });
            _copyDocumentThread.Start();
            return true;
        }

        public bool IsCopyDocumentJobRunning()
        {
            if (_copyDocumentThread == null)
            {
                return false;
            }
            if (_copyDocumentThread.IsAlive)
            {
                return true;
            }
            _copyDocumentThread = null;
            return false;
        }

        public static bool CopyDocumentsRecursive(DocumentFile documentSrc, DocumentFile documentDst, ProgressDocumentCopyDelegate progressHandler)
        {
            try
            {
                if (documentSrc?.Uri == null || documentDst?.Uri == null)
                {
                    return false;
                }

                if (!documentSrc.Exists())
                {
                    return false;
                }

                if (documentSrc.IsFile)
                {
                    DocumentFile fileDst = documentDst;
                    if (documentDst.IsDirectory)
                    {
                        DocumentFile oldFile = documentDst.FindFile(documentSrc.Name);
                        if (oldFile != null)
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Delete file: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                            oldFile.Delete();
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("Create file: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                        fileDst = documentDst.CreateFile(MimeTypeAppAny, documentSrc.Name);
                    }

                    if (!fileDst.IsFile)
                    {
                        return false;
                    }

                    if (progressHandler != null)
                    {
                        if (!progressHandler.Invoke(documentSrc.Name))
                        {
                            return false;
                        }
                    }

                    ContentResolver contentResolver = Android.App.Application.Context.ContentResolver;
                    if (contentResolver == null)
                    {
                        return false;
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Copy file: Name={0}, Type={1}, URI={2}", documentSrc.Name, documentSrc.Type, documentSrc.Uri.ToString()));
#endif
                    using (Stream inputStream = contentResolver.OpenInputStream(documentSrc.Uri))
                    {
                        if (inputStream == null)
                        {
                            return false;
                        }

                        using (Stream outputStream = contentResolver.OpenOutputStream(fileDst.Uri))
                        {
                            if (outputStream == null)
                            {
                                return false;
                            }

                            inputStream.CopyTo(outputStream);
                        }
                    }

                    return true;
                }

                if (!documentSrc.IsDirectory || !documentDst.IsDirectory)
                {
                    return false;
                }

                DocumentFile oldDir = documentDst.FindFile(documentSrc.Name);
                if (oldDir != null)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Delete dir: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                    oldDir.Delete();
                }

#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("Create dir: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                DocumentFile subDirDst = documentDst.CreateDirectory(documentSrc.Name);
                DocumentFile[] files = documentSrc.ListFiles();
                foreach (DocumentFile documentFile in files)
                {
                    if (documentFile.IsFile)
                    {
                        DocumentFile oldFile = subDirDst.FindFile(documentFile.Name);
                        if (oldFile != null)
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Delete file: Name={0}, URI={1}", documentFile.Name, documentFile.Uri.ToString()));
#endif
                            oldFile.Delete();
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("Create file: Name={0}, URI={1}", documentFile.Name, documentFile.Uri.ToString()));
#endif
                        DocumentFile dstFile = subDirDst.CreateFile(MimeTypeAppAny, documentFile.Name);
                        if (!CopyDocumentsRecursive(documentFile, dstFile, progressHandler))
                        {
                            return false;
                        }
                        continue;
                    }

                    if (documentFile.IsDirectory)
                    {
                        if (!CopyDocumentsRecursive(documentFile, subDirDst, progressHandler))
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool RequestDeleteDocumentsThread(DocumentFile documentFile, CopyDocumentsThreadFinishDelegate handler)
        {
            try
            {
                if (documentFile == null)
                {
                    return false;
                }

                string documentPath = GetDocumentPath(documentFile);
                if (string.IsNullOrEmpty(documentPath))
                {
                    return false;
                }

                string message = string.Format(_context.GetString(Resource.String.del_document_request), documentPath);
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        DeleteDocumentsThread(documentFile, handler);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool DeleteDocumentsThread(DocumentFile documentFile, CopyDocumentsThreadFinishDelegate handler)
        {
            if (IsDeleteDocumentJobRunning())
            {
                return false;
            }

            bool aborted = false;
            CustomProgressDialog progress = new CustomProgressDialog(_activity);
            progress.SetMessage(_activity.GetString(Resource.String.del_documents_progress));
            progress.Indeterminate = true;
            progress.AbortClick = sender =>
            {
                aborted = true;
            };
            progress.Show();
            SetLock(LockTypeCommunication);
            _deleteDocumentThread = new Thread(() =>
            {
                bool result = false;
                if (documentFile.Exists())
                {
                    result = documentFile.Delete();
                }

                _activity?.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }

                    progress.Dismiss();
                    progress = null;
                    SetLock(LockType.None);
                    if (!result && !aborted)
                    {
                        new AlertDialog.Builder(_context)
                            .SetMessage(Resource.String.del_documents_error)
                            .SetTitle(Resource.String.alert_title_error)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                            .Show();
                    }
                    handler?.Invoke(result, aborted);
                });
            });
            _deleteDocumentThread.Start();
            return true;
        }

        public bool IsDeleteDocumentJobRunning()
        {
            if (_deleteDocumentThread == null)
            {
                return false;
            }
            if (_deleteDocumentThread.IsAlive)
            {
                return true;
            }
            _deleteDocumentThread = null;
            return false;
        }

        public static int GetDocumentCount(DocumentFile document, ProgressDocumentCopyDelegate progressHandler)
        {
            try
            {
                if (document == null)
                {
                    return -1;
                }

                if (document.IsFile)
                {
                    if (progressHandler != null)
                    {
                        if (!progressHandler.Invoke(document.Name))
                        {
                            return -1;
                        }
                    }

                    if (document.Exists())
                    {
                        return 1;
                    }

                    return 0;
                }

                if (!document.IsDirectory)
                {
                    return -1;
                }

                int fileCount = 0;
                DocumentFile[] files = document.ListFiles();
                foreach (DocumentFile documentFile in files)
                {
                    if (documentFile.IsFile)
                    {
                        if (document.Exists())
                        {
                            fileCount++;
                        }
                        continue;
                    }

                    if (documentFile.IsDirectory)
                    {
                        int count = GetDocumentCount(documentFile, progressHandler);
                        if (count < 0)
                        {
                            return -1;
                        }

                        fileCount += count;
                    }
                }

                return fileCount;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool DocumentExists(DocumentFile document)
        {
            try
            {
                if (document == null)
                {
                    return false;
                }

                if (document.Exists())
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public bool IsExStorageAvailable()
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                {
                    string extState = Android.OS.Environment.ExternalStorageState;
                    if (string.IsNullOrEmpty(extState))
                    {
                        return false;
                    }

                    return IsMediaMounted(extState);
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {   // writing to external disk is only allowed in special directories.
                    Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                    if (externalFilesDirs?.Length > 0)
                    {
                        foreach (Java.IO.File extDir in externalFilesDirs)
                        {
                            if (extDir != null)
                            {
                                string extState = Android.OS.Environment.GetExternalStorageState(extDir);
                                if (string.IsNullOrEmpty(extState))
                                {
                                    return false;
                                }

                                if (!IsMediaMounted(extState))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
#if DEBUG
                Android.Util.Log.Info(Tag, "IsExStorageAvailable: All media mounted");
#endif
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsMediaMounted(string extState)
        {
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("IsMediaMounted: Media State={0}", extState));
#endif
            if (!extState.Equals(Android.OS.Environment.MediaRemoved))
            {
                if (!extState.Equals(Android.OS.Environment.MediaMounted))
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "IsMediaMounted: Media not mounted");
#endif
                    return false;
                }
            }

#if DEBUG
            Android.Util.Log.Info(Tag, "IsMediaMounted: Media mounted");
#endif
            return true;
        }

        public static void SetStoragePath()
        {
            _externalPath = string.Empty;
            _externalWritePath = string.Empty;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            {
#pragma warning disable 618
                Java.IO.File extDir = Android.OS.Environment.ExternalStorageDirectory;
#pragma warning restore 618
                string extState = Android.OS.Environment.ExternalStorageState;
                if (extDir != null && extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted))
                {
                    _externalPath = extDir.AbsolutePath;
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("SetStoragePath: ExternalStorageDirectory: {0}", _externalPath));
#endif
                }
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {   // writing to external disk is only allowed in special directories.
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                if (externalFilesDirs?.Length > 0)
                {
                    // index 0 is the internal disk
                    if (externalFilesDirs.Length > 1)
                    {
                        Java.IO.File extDir = externalFilesDirs[1];
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (extDir != null)
                        {
                            string extState = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                                Android.OS.Environment.GetExternalStorageState(extDir) : Android.OS.Environment.ExternalStorageState;
                            if (extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted) && IsWritable(extDir.AbsolutePath))
                            {
                                _externalWritePath = extDir.AbsolutePath;
#if DEBUG
                                Android.Util.Log.Info(Tag, string.Format("SetStoragePath: GetExternalFilesDirs[1]: {0}", _externalWritePath));
#endif
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_externalWritePath))
                    {
                        Java.IO.File extDir = externalFilesDirs[0];
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (extDir != null)
                        {
                            string extState = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                                Android.OS.Environment.GetExternalStorageState(extDir) : Android.OS.Environment.ExternalStorageState;
                            if (extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted) && IsWritable(extDir.AbsolutePath))
                            {
                                _externalWritePath = extDir.AbsolutePath;
#if DEBUG
                                Android.Util.Log.Info(Tag, string.Format("SetStoragePath: GetExternalFilesDirs[0]: {0}", _externalWritePath));
#endif
                            }
                        }
                    }
                }
            }
            else
            {
                string procMounts = ReadProcMounts();
                string sdCardEntry = ParseProcMounts(procMounts, _externalPath);
                if (!string.IsNullOrEmpty(sdCardEntry))
                {
                    _externalPath = sdCardEntry;
                }
            }
        }

        private static string ReadProcMounts()
        {
            try
            {
                string contents = File.ReadAllText("/proc/mounts");
                return contents;
            }
            catch (Exception)
            {
                // ignored
            }
            return string.Empty;
        }

        public static bool IsWritable(string pathToTest)
        {
            bool result = false;

            if (!string.IsNullOrWhiteSpace(pathToTest))
            {
                const string testText = "test text";
                string testFile = Guid.NewGuid() + ".txt";
                string testPath = Path.Combine(pathToTest, testFile);
                try
                {
                    File.WriteAllText(testPath, testText);
                    // case sensitive file systems are supported now
                    if (File.Exists(testPath))
                    {
                        result = true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    try
                    {
                        if (File.Exists(testPath))
                        {
                            File.Delete(testPath);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            return result;
        }

        private static string ParseProcMounts(string procMounts, string externalPath)
        {
            string sdCardEntry = string.Empty;
            if (!string.IsNullOrWhiteSpace(procMounts))
            {
                List<string> procMountEntries = procMounts.Split('\n', '\r').ToList();
                foreach (string entry in procMountEntries)
                {
                    string[] sdCardEntries = entry.Split(' ');
                    if (sdCardEntries.Length > 2)
                    {
                        string path = sdCardEntries[1];
                        if (path.StartsWith(externalPath, StringComparison.OrdinalIgnoreCase) &&
                            string.Compare(path, externalPath, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (IsWritable(path))
                            {
                                sdCardEntry = path;
                                break;
                            }
                        }
                    }
                }
            }
            return sdCardEntry;
        }

        private static List<string> ParseStorageMedia(string procMounts)
        {
            List<string> sdCardList = new List<string>();
            if (!string.IsNullOrWhiteSpace(procMounts))
            {
                List<string> procMountEntries = procMounts.Split('\n', '\r').ToList();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (string entry in procMountEntries)
                {
                    string[] sdCardEntries = entry.Split(' ');
                    if (sdCardEntries.Length > 2)
                    {
                        string path = sdCardEntries[1];
                        if (IsWritable(path))
                        {
                            sdCardList.Add(path);
                        }
                    }
                }
            }
            return sdCardList;
        }

        public static FileSystemBlockInfo GetFileSystemBlockInfo(string path)
        {
            var statFs = new StatFs(path);
            var fsbi = new FileSystemBlockInfo();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
            {
                fsbi.Path = path;
                fsbi.BlockSizeBytes = statFs.BlockSizeLong;
                fsbi.TotalSizeBytes = statFs.BlockCountLong * statFs.BlockSizeLong;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocksLong * statFs.BlockSizeLong;
                fsbi.FreeSizeBytes = statFs.FreeBlocksLong * statFs.BlockSizeLong;
            }
            else // this was deprecated in API level 18 (Android 4.3), so if your device is below level 18, this is what will be used instead.
            {
                fsbi.Path = path;
                // you may want to disable warning about obsoletes, earlier versions of Android are using the deprecated versions
#pragma warning disable 618
                fsbi.BlockSizeBytes = statFs.BlockSize;
                fsbi.TotalSizeBytes = statFs.BlockCount * (long)statFs.BlockSize;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocks * (long)statFs.BlockSize;
                fsbi.FreeSizeBytes = statFs.FreeBlocks * (long)statFs.BlockSize;
#pragma warning restore 618
            }
            return fsbi;
        }

        public static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(path, "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            return a.Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
        }

        public static string GetTruncatedPathName(string path, int maxLength = 30)
        {
            try
            {
                if (path == null)
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                List<string> dirList = path.Split(Path.DirectorySeparatorChar).ToList();
                dirList.RemoveAll(string.IsNullOrWhiteSpace);

                int maxRootParts = 1;
                if (dirList.Count >= 1)
                {
                    int sumLength = dirList[dirList.Count - 1].Length + 1;
                    for (int i = 0; i < dirList.Count - 1; i++)
                    {
                        sumLength += dirList[i].Length + 1;
                        if (sumLength > maxLength)
                        {
                            break;
                        }
                        maxRootParts = i + 1;
                    }
                }

                for (int i = 0; i < dirList.Count; i++)
                {
                    sb.Append(Path.DirectorySeparatorChar.ToString());
                    sb.Append(dirList[i]);
                    if (i + 1 >= maxRootParts)
                    {
                        if (dirList.Count > maxRootParts + 1)
                        {
                            sb.Append(Path.DirectorySeparatorChar.ToString());
                            sb.Append("...");
                        }

                        if (dirList.Count > maxRootParts)
                        {
                            sb.Append(Path.DirectorySeparatorChar.ToString());
                            sb.Append(dirList[dirList.Count - 1]);
                        }

                        break;
                    }
                }

                string name = sb.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.DirectorySeparatorChar.ToString();
                }

                return name;
            }
            catch (Exception )
            {
                return string.Empty;
            }
        }

        private void NetworkStateChanged()
        {
            _activity?.RunOnUiThread(() =>
            {
                if (_disposed)
                {
                    return;
                }
                switch (_selectedInterface)
                {
                    case InterfaceType.Bluetooth:
                    case InterfaceType.Enet:
                    case InterfaceType.ElmWifi:
                    case InterfaceType.DeepObdWifi:
                    {
                        bool interfaceAvailable = IsInterfaceAvailable();
                        if (!_lastInerfaceAvailable.HasValue ||
                            _lastInerfaceAvailable.Value != interfaceAvailable)
                        {
                            _lastInerfaceAvailable = interfaceAvailable;
                            _bcReceiverUpdateDisplayHandler?.Invoke();
                        }
                        break;
                    }
                }

                SetPreferredNetworkInterface();
            });
        }

        private void UsbCheckEvent(Object state)
        {
            if (_usbCheckTimer == null)
            {
                return;
            }
            _activity?.RunOnUiThread(() =>
            {
                if (_disposed)
                {
                    return;
                }
                List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                if (availableDrivers.Count > _usbDeviceDetectCount)
                {   // device attached
                    _bcReceiverReceivedHandler?.Invoke(_context, null);
                    _bcReceiverUpdateDisplayHandler?.Invoke();
                }
                _usbDeviceDetectCount = availableDrivers.Count;
            });
        }

        public class Receiver : BroadcastReceiver
        {
            readonly ActivityCommon _activityCommon;

            public Receiver(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                if (intent == null)
                {
                    return;
                }

                string action = intent.Action;
                _activityCommon._bcReceiverReceivedHandler?.Invoke(context, intent);
                switch (action)
                {
                    case BluetoothAdapter.ActionStateChanged:
#pragma warning disable CS0618 // Typ oder Element ist veraltet
#pragma warning disable CA1422
                    case ConnectivityManager.ConnectivityAction:
                        _activityCommon.NetworkStateChanged();
#pragma warning restore CA1422
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                        if (action == BluetoothAdapter.ActionStateChanged)
                        {
                            if (_activityCommon._activity is ActivityMain)
                            {
                                State extraState = (State)intent.GetIntExtra(BluetoothAdapter.ExtraState, (int)State.Disconnected);
                                switch (extraState)
                                {
                                    case State.TurningOn:
                                        _btEnableCounter--;
                                        break;

                                    case State.TurningOff:
                                        _btEnableCounter = 0;
                                        break;
                                }
                            }
                        }
                        break;

                    case WifiApStateChangedAction:
                        _activityCommon.NetworkStateChanged();
                        break;

                    case UsbManager.ActionUsbDeviceAttached:
                    case UsbManager.ActionUsbDeviceDetached:
                        {
                            UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                            if (usbDevice != null && EdFtdiInterface.IsValidUsbDevice(usbDevice))
                            {
                                _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                            }
                            break;
                        }

                    case UsbPermissionAction:
                        _activityCommon._usbPermissionRequested = false;
                        _activityCommon._usbPermissionRequestDisabled = true;
                        if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
                        {
                            _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                        }
                        break;

                    case GlobalBroadcastReceiver.NotificationBroadcastAction:
                        _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                        break;
                }
            }
        }

        public class CellularCallback : ConnectivityManager.NetworkCallback
        {
            private readonly ActivityCommon _activityCommon;

            public CellularCallback(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnAvailable(Network network)
            {
                lock (_activityCommon._networkData.LockObject)
                {
                    _activityCommon._networkData.ActiveCellularNetworks.RemoveAll(x => x.GetHashCode() == network.GetHashCode());
                    _activityCommon._networkData.ActiveCellularNetworks.Insert(0, network);
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Added cellular network: hash={0}, count={1}", network.GetHashCode(), _activityCommon._networkData.ActiveCellularNetworks.Count));
#endif
                }
                _activityCommon.SetPreferredNetworkInterface();
            }

            public override void OnLost(Network network)
            {
                lock (_activityCommon._networkData.LockObject)
                {
                    _activityCommon._networkData.ActiveCellularNetworks.RemoveAll(x => x.GetHashCode() == network.GetHashCode());
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Removed cellular network: hash={0}, count={1}", network.GetHashCode(), _activityCommon._networkData.ActiveCellularNetworks.Count));
#endif
                }
                _activityCommon.SetPreferredNetworkInterface();
            }
        }

        public class WifiCallback : ConnectivityManager.NetworkCallback
        {
            private readonly ActivityCommon _activityCommon;

            public WifiCallback(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnAvailable(Network network)
            {
                lock (_activityCommon._networkData.LockObject)
                {
                    _activityCommon._networkData.ActiveWifiNetworks.RemoveAll(x => x.GetHashCode() == network.GetHashCode());
                    _activityCommon._networkData.ActiveWifiNetworks.Insert(0, network);
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Added WiFi network: hash={0}, count={1}", network.GetHashCode(), _activityCommon._networkData.ActiveWifiNetworks.Count));
#endif
                }
                _activityCommon.NetworkStateChanged();
            }

            public override void OnLost(Network network)
            {
                lock (_activityCommon._networkData.LockObject)
                {
                    _activityCommon._networkData.ActiveWifiNetworks.RemoveAll(x => x.GetHashCode() == network.GetHashCode());
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Removed WiFi network: hash={0}, count={1}", network.GetHashCode(), _activityCommon._networkData.ActiveWifiNetworks.Count));
#endif
                }
                _activityCommon.NetworkStateChanged();
            }
        }

        public class EthernetCallback : ConnectivityManager.NetworkCallback
        {
            private readonly ActivityCommon _activityCommon;

            public EthernetCallback(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnAvailable(Network network)
            {
                lock (_activityCommon._networkData.LockObject)
                {
                    _activityCommon._networkData.ActiveEthernetNetworks.RemoveAll(x => x.GetHashCode() == network.GetHashCode());
                    _activityCommon._networkData.ActiveEthernetNetworks.Insert(0, network);
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Added ethernet network: hash={0}, count={1}", network.GetHashCode(), _activityCommon._networkData.ActiveEthernetNetworks.Count));
#endif
                }

                _activityCommon.NetworkStateChanged();
            }

            public override void OnLost(Network network)
            {
                lock (_activityCommon._networkData.LockObject)
                {
                    _activityCommon._networkData.ActiveEthernetNetworks.RemoveAll(x => x.GetHashCode() == network.GetHashCode());
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Removed ethernet network: hash={0}, count={1}", network.GetHashCode(), _activityCommon._networkData.ActiveEthernetNetworks.Count));
#endif
                }

                _activityCommon.NetworkStateChanged();
            }
        }
    }

    public static class EnumExtensions
    {
        public static string ToDescriptionString(this ActivityCommon.InterfaceType val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }

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
