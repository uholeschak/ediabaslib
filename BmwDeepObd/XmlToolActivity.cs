using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using BmwDeepObd.FilePicker;
using EdiabasLib;
using AndroidX.AppCompat.App;
using BmwFileReader;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/xml_tool_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(XmlToolActivity),
        ConfigurationChanges = ActivityConfigChanges)]
    public class XmlToolActivity : BaseActivity
    {
        private enum ActivityRequest
        {
            RequestAppDetailBtSettings,
            RequestSelectSgbd,
            RequestSelectDevice,
            RequestAdapterConfig,
            RequestSelectJobs,
            RequestOpenExternalFile,
            RequestYandexKey,
            RequestEdiabasTool,
        }

        private enum VagUdsS22DataType
        {
            EcuInfo,
            Vin,
            PartNum,
            HwPartNum,
            SysName,
            AsamData,
            AsamRev,
            Coding,
            ProgDate,
            SubSystems
        }

        private enum VagUdsS22SubSysDataType
        {
            Coding,
            PartNum,
            SysName
        }

        // ReSharper disable UnusedMember.Global
        public enum SupportedFuncType
        {
            ActuatorDiag = 0x0003,      // func 0x0102
            ActuatorDiag2 = 0x0067,     // func 0x0107
            Adaption = 0x000A,          // func 0x0103
            AdaptionLong = 0x03F2,      // func 0x010A
            AdaptionLong2 = 0x07DA,     // func 0x0113
        }
        // ReSharper restore UnusedMember.Global

        public enum EcuFunctionCallType
        {
            None,
            BmwActuator,
            VagCoding,
            VagCoding2,
            VagAdaption,
            VagLogin,
            VagSecAccess,
        }

        public enum DisplayFontSize
        {
            Small,
            Medium,
            Large
        }

        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string description, string sgbd, string grp,
                JobReader.PageInfo.DisplayModeType displayMode = JobReader.PageInfo.DisplayModeType.List,
                DisplayFontSize fontSize = DisplayFontSize.Small, int gaugesPortrait = JobReader.GaugesPortraitDefault, int gaugesLandscape = JobReader.GaugesLandscapeDefault,
                string mwTabFileName = null, Dictionary<long, EcuMwTabEntry> mwTabEcuDict = null, string vagDataFileName = null, string vagUdsFileName = null)
            {
                Name = name;
                Address = address;
                Description = description;
                DescriptionTrans = null;
                DescriptionTransRequired = true;
                Sgbd = sgbd;
                Grp = grp;
                Selected = false;
                NoUpdate = false;

                InitReadValues();

                PageName = name;
                EcuName = name;
                DisplayMode = displayMode;
                FontSize = fontSize;
                GaugesPortrait = gaugesPortrait;
                GaugesLandscape = gaugesLandscape;
                JobList = null;
                MwTabFileName = mwTabFileName;
                MwTabList = null;
                MwTabEcuDict = mwTabEcuDict;
                VagDataFileName = vagDataFileName;
                VagUdsFileName = vagUdsFileName;
                ReadCommand = null;
                UseCompatIds = false;
            }

            public void InitReadValues()
            {
                Vin = null;
                VagSupportedFuncHash = null;
                VagPartNumber = null;
                VagHwPartNumber = null;
                VagSysName = null;
                VagAsamData = null;
                VagAsamRev = null;
                VagEquipmentNumber = null;
                VagImporterNumber = null;
                VagWorkshopNumber = null;
                VagCodingRequestType = null;
                VagCodingTypeValue = null;
                VagCodingMax = null;
                VagCodingShort = null;
                VagCodingLong = null;
                VagProgDate = null;
                SubSystems = null;
            }

            public bool HasVagCoding()
            {
                if (!ActivityCommon.VagUdsActive)
                {
                    return false;
                }

                if (VagCodingShort != null || VagCodingLong != null)
                {
                    return true;
                }

                if (SubSystems != null)
                {
                    foreach (EcuInfoSubSys subSystem in SubSystems)
                    {
                        if (subSystem.VagCodingLong != null)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public bool HasVagCoding2()
            {
                if (!ActivityCommon.VagUdsActive)
                {
                    return false;
                }

                if (!Is1281Ecu(this) && !IsUdsEcu(this))
                {
                    return true;
                }

                return false;
            }

            public bool HasVagLogin()
            {
                if (!ActivityCommon.VagUdsActive)
                {
                    return false;
                }

                if (Is1281Ecu(this))
                {
                    return true;
                }

                return false;
            }

            public enum CodingRequestType
            {
                ShortV1,
                ShortV2,
                LongUds,
                ReadLong,
                CodingS22,
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Description { get; set; }

            public string DescriptionTrans { get; set; }

            public bool DescriptionTransRequired { get; set; }

            public string Sgbd { get; set; }

            public string Grp { get; set; }

            public string Vin { get; set; }

            public HashSet<UInt64> VagSupportedFuncHash { get; set; }

            public string VagPartNumber { get; set; }

            public string VagHwPartNumber { get; set; }

            public string VagSysName { get; set; }

            public string VagAsamData { get; set; }

            public string VagAsamRev { get; set; }

            public UInt64? VagEquipmentNumber { get; set; }

            public UInt64? VagImporterNumber { get; set; }

            public UInt64? VagWorkshopNumber { get; set; }

            public CodingRequestType? VagCodingRequestType { get; set; }

            public UInt64? VagCodingTypeValue { get; set; }

            public UInt64? VagCodingMax { get; set; }

            public UInt64? VagCodingShort { get; set; }

            public byte[] VagCodingLong { get; set; }

            public byte[] VagProgDate { get; set; }

            public List<EcuInfoSubSys> SubSystems { get; set; }

            public bool Selected { get; set; }

            public bool NoUpdate { get; set; }

            public string PageName { get; set; }

            public string EcuName { get; set; }

            public JobReader.PageInfo.DisplayModeType DisplayMode { get; set; }

            public DisplayFontSize FontSize { get; set; }

            public int GaugesPortrait { get; set; }

            public int GaugesLandscape { get; set; }

            public List<XmlToolEcuActivity.JobInfo> JobList { get; set; }

            public string MwTabFileName { get; set; }

            public List<ActivityCommon.MwTabEntry> MwTabList { get; set; }

            public Dictionary<long, EcuMwTabEntry> MwTabEcuDict { get; set; }

            public string VagDataFileName { get; set; }

            public string VagUdsFileName { get; set; }

            public bool IgnoreXmlFile { get; set; }

            public string ReadCommand { get; set; }

            public bool UseCompatIds { get; set; }
        }

        public class EcuInfoSubSys
        {
            public EcuInfoSubSys(uint subSysAddr)
            {
                SubSysAddr = subSysAddr;
            }

            public uint SubSysAddr { get; }

            public int SubSysIndex { get; set; }

            public byte[] VagCodingLong { get; set; }

            public string VagPartNumber { get; set; }

            public string VagSysName { get; set; }

            public string VagDataFileName { get; set; }

            public string Name { get; set; }
        }

        public class EcuMwTabEntry
        {
            public EcuMwTabEntry(int blockNumber, int valueIndex, string valueType, string valueUnit)
            {
                BlockNumber = blockNumber;
                ValueIndex = valueIndex;
                ValueType = valueType;
                ValueUnit = valueUnit;
            }

            public int BlockNumber { get; }
            public int ValueIndex { get; }
            public string ValueUnit { get; }
            public string ValueType { get; }
        }

        public class InstanceData
        {
            public InstanceData()
            {
                AddErrorsPage = true;
                NoErrorsPageUpdate = false;
                EcuSearchAbortIndex = -1;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                TraceActive = true;
                SgbdFunctional = string.Empty;
                Vin = string.Empty;
                VehicleType = string.Empty;
            }

            public bool ForceAppend { get; set; }
            public bool AutoStart { get; set; }
            public bool AdapterCheckOk { get; set; }
            public int AutoStartSearchStartIndex { get; set; }
            public bool AddErrorsPage { get; set; }
            public bool NoErrorsPageUpdate { get; set; }
            public int ManualConfigIdx { get; set; }
            public int EcuSearchAbortIndex { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string TraceDir { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public string SgbdFunctional { get; set; }
            public string Vin { get; set; }
            public string VehicleType { get; set; }
            public string CDate { get; set; }
            public bool CommErrorsOccurred { get; set; }
        }

#if DEBUG
        private static readonly string Tag = typeof(XmlToolActivity).FullName;
#endif
        private static readonly Encoding VagUdsEncoding = Encoding.GetEncoding(1252);
        private const string XmlDocumentFrame =
            @"<?xml version=""1.0"" encoding=""utf-8"" ?>
            <{0} xmlns=""http://www.holeschak.de/BmwDeepObd""
            xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
            xsi:schemaLocation=""http://www.holeschak.de/BmwDeepObd BmwDeepObd.xsd"">
            </{0}>";
        private const string XsdFileName = "BmwDeepObd.xsd";
        private const string TranslationFileName = "Translation.xml";
        private const int MotorAddrVag = 1;

        private const string PageExtension = ".ccpage";
        private const string ErrorsFileName = "Errors.ccpage";
        private const string PagesFileName = "Pages.ccpages";
        private const string ConfigFileExtension = ".cccfg";
        private const string DisplayNamePage = "!PAGE_NAME";
        private const string DisplayNameJobPrefix = "!JOB#";
        private const string DisplayNameEcuPrefix = "!ECU#";
        private const string ManualConfigName = "Manual";
        private const string UnknownVinConfigName = "Unknown";
        private static readonly string[] EcuFileNames =
        {
            "e60", "e65", "e70", "e81", "e87", "e89X", "e90", "m12", "r56", "f01", "f01bn2k", "rr01"
        };
        private static readonly string[] ReadVinJobs =
        {
            "C_FG_LESEN_FUNKTIONAL", "PROG_FG_NR_LESEN_FUNKTIONAL", "AIF_LESEN_FUNKTIONAL"
        };

        private static readonly Tuple<VagUdsS22DataType, int>[] VagUdsS22Data =
        {
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.EcuInfo, -1),
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.Vin, 0xF190),
            //new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.PartNum, 0xF187),
            //new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.HwPartNum, 0xF191),
            //new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.SysName, 0xF197),
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.AsamData, 0xF19E),
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.AsamRev, 0xF1A2),
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.Coding, 0x0600),
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.ProgDate, 0xF199),
            new Tuple<VagUdsS22DataType, int>(VagUdsS22DataType.SubSystems, 0x0608),
        };

        private static readonly Tuple<VagUdsS22SubSysDataType, int>[] VagUdsS22SubSysData =
        {
            new Tuple<VagUdsS22SubSysDataType, int>(VagUdsS22SubSysDataType.Coding, 0x6000),
            new Tuple<VagUdsS22SubSysDataType, int>(VagUdsS22SubSysDataType.PartNum, 0x6200),
            new Tuple<VagUdsS22SubSysDataType, int>(VagUdsS22SubSysDataType.SysName, 0x6C00),
        };

        private static readonly Tuple<string, string>[] EcuInfoVagJobs =
        {
            new Tuple<string, string>("Steuergeraeteversion_abfragen", ""),
            new Tuple<string, string>("Steuergeraeteversion_abfragen2", ""),
            new Tuple<string, string>("Fahrgestellnr_abfragen", ""),
            new Tuple<string, string>("ErwIdentifikation_abfragen", ""),
            new Tuple<string, string>("UnterstFunktionen_abfragen", ""),
            new Tuple<string, string>("Verbauliste_abfragen", ""),
            new Tuple<string, string>("Fehlerspeicher_abfragen", "MW_LESEN"),
            new Tuple<string, string>("FehlerspeicherSAE_abfragen", "MW_LESEN"),
            new Tuple<string, string>("Tageszaehler_abfragen", ""),
            new Tuple<string, string>("CodierungS22_lesen", ""),
            new Tuple<string, string>("IdentMasterS22_abfragen", ""),
            new Tuple<string, string>("IdentDatenMasterS22_abfragen", "VWDataSetNumberOrECUDataContainerNumber"),
            new Tuple<string, string>("IdentDatenMasterS22_abfragen", "VWDataSetVersionNumber"),
            new Tuple<string, string>("IdentDatenMasterS22_abfragen", "VWFAZITIdentificationString"),
            new Tuple<string, string>("IdentDatenMasterS22_abfragen", "VehicleEquipmentCodeAndPRNumberCombination"),
            new Tuple<string, string>("LangeCodierung_lesen", ""),
            new Tuple<string, string>("Messwerteblock_lesen", "1;START"),
            new Tuple<string, string>("Messwerteblock_lesen", "1;STOP"),
            new Tuple<string, string>("Messwerteblock_lesen", "1;"),
            new Tuple<string, string>("Messwerteblock_lesen", "2;"),
            new Tuple<string, string>("Messwerteblock_lesen", "4;"),
            new Tuple<string, string>("Messwerteblock_lesen", "12;"),
            new Tuple<string, string>("Messwerteblock_lesen", "19;"),
            new Tuple<string, string>("Messwerteblock_lesen", "20;"),
            new Tuple<string, string>("Messwerteblock_lesen", "22;"),
            new Tuple<string, string>("Messwerteblock_lesen", "34;"),
            new Tuple<string, string>("Messwerteblock_lesen", "81;"),
            new Tuple<string, string>("Messwerteblock_lesen", "100;"),
            new Tuple<string, string>("Messwerteblock_lesen", "101;"),
            new Tuple<string, string>("Messwerteblock_lesen", "102;"),
            new Tuple<string, string>("Messwerteblock_lesen", "103;"),
            new Tuple<string, string>("Messwerteblock_lesen", "121;"),
            new Tuple<string, string>("Messwerteblock_lesen", "122;"),
            new Tuple<string, string>("Messwerteblock_lesen", "123;"),
            new Tuple<string, string>("Messwerteblock_lesen", "124;"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0101"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0405"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0407"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0408"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0409"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x040A"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x040B"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x040C"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x04A1"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0600"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0601"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0606"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0607"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0608"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0610"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0640"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0670"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x06A0"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x06D0"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0700"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0730"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x0760"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x07A0"),
            new Tuple<string, string>("GenerischS22_abfragen", "0x2A2E"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF15B"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF17B"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF17C"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF17D"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF17E"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF17F"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF181"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF182"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF183"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF184"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF186"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF187"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF189"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF18B"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF18C"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF190"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF191"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF197"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF198"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF199"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF19A"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF19B"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF19E"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A0"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A1"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A2"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A3"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A4"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A5"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A6"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A7"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A8"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A9"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1AA"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1AB"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1AC"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1AD"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1DF"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF401"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF442"),
            //new Tuple<string, string>("Fehlerspeicher_loeschen", ""),
        };

        private static readonly byte[][] EcuInfoVagUdsRaw =
        {
            new byte[] {0x09, 0x02},    // VIN
        };

        public const int VagUdsRawDataOffset = 18;
        public const string EmptyMwTab = "-";
        public const string VagUdsCommonSgbd = @"mot7000";
        public const string JobReadMwBlock = @"Messwerteblock_lesen";
        public const string JobReadS22Uds = @"GenerischS22_abfragen";
        public const string JobWriteS2EUds = @"GenerischS2E_schreiben";
        public const string JobReadStatMwBlock = @"STATUS_MESSWERTBLOCK_LESEN";
        public const string JobReadStatBlock = @"STATUS_BLOCK_LESEN";
        public const string JobReadStat = @"STATUS_LESEN";
        public const string JobReadSupportedFunc = @"UnterstFunktionen_abfragen";
        public const string JobReadEcuVersion = @"Steuergeraeteversion_abfragen";
        public const string JobReadEcuVersion2 = @"Steuergeraeteversion_abfragen2";
        public const string JobReadVin = @"Fahrgestellnr_abfragen";
        public const string JobWriteEcuCoding = @"Steuergeraet_Codieren";
        public const string JobWriteEcuCoding2 = @"Steuergeraet_Codieren2";
        public const string JobWriteLogin = @"Login";
        public const string JobReadCoding = @"CodierungS22_lesen";
        public const string JobWriteCoding = @"CodierungS2E_schreiben";
        public const string JobReadLongCoding = @"LangeCodierung_lesen";
        public const string JobWriteLongCoding = @"LangeCodierung_schreiben";
        public const string DataTypeString = @"string";
        public const string DataTypeReal = @"real";
        public const string DataTypeInteger = @"integer";
        public const string DataTypeBinary = @"binary";
        public const string DataTypeStringReal = @"string/real";

        // Intent extra
        public const string ExtraInitDir = "init_dir";
        public const string ExtraVagDir = "vag_dir";
        public const string ExtraBmwDir = "bmw_dir";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraPageFileName = "page_file_name";
        public const string ExtraEcuFuncCall = "ecu_func_call";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraElmWifiIp = "elmwifi_ip";
        public const string ExtraDeepObdWifiIp = "deepobdwifi_ip";
        public const string ExtraFileName = "file_name";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");

        public delegate void MwTabFileSelected(string fileName);
        public delegate void UpdateEcuDelegate(bool error);

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private View _barView;
        private Button _buttonRead;
        private Button _buttonSafe;
        private ListView _listViewEcu;
        private EcuListAdapter _ecuListAdapter;
        private TextView _textViewCarInfo;
        private string _ecuDir;
        private string _vagDir;
        private string _bmwDir;
        private string _appDataDir;
        private string _pageFileName = string.Empty;
        private EcuFunctionCallType _ecuFuncCall = EcuFunctionCallType.None;
        private EcuFunctionCallType _ecuFuncCallMenu = EcuFunctionCallType.None;
        private string _lastFileName = string.Empty;
        private string _datUkdDir = string.Empty;
        private bool _activityActive;
        private volatile bool _ediabasJobAbort;
        private ActivityCommon _activityCommon;
        private RuleEvalBmw _ruleEvalBmw;
        private CheckAdapter _checkAdapter;
        private EdiabasNet _ediabas;
        private SgFunctions _sgFunctions;
        private Thread _jobThread;
        private static List<EcuInfo> _ecuList = new List<EcuInfo>();
        private EcuInfo _ecuInfoMot;
        private EcuInfo _ecuInfoDid;
        private bool _translateEnabled = true;
        private bool _translateActive;
        private bool _ecuListTranslated;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }
            else
            {
                _ecuList = new List<EcuInfo>();
            }

            _pageFileName = Intent.GetStringExtra(ExtraPageFileName);
            _ecuFuncCall = (EcuFunctionCallType)Intent.GetIntExtra(ExtraEcuFuncCall, (int)EcuFunctionCallType.None);
            if (IsPageSelectionActive())
            {
                SupportActionBar.Hide();
            }
            else
            {
                SupportActionBar.SetHomeButtonEnabled(true);
                SupportActionBar.SetDisplayShowHomeEnabled(true);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowCustomEnabled(true);
            }
            SetContentView(Resource.Layout.xml_tool);

            _barView = LayoutInflater.Inflate(Resource.Layout.bar_xml_tool, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(_barView, barLayoutParams);

            SetResult(Android.App.Result.Canceled);

            _buttonRead = _barView.FindViewById<Button>(Resource.Id.buttonXmlRead);
            _buttonRead.Click += (sender, args) =>
            {
                if (_instanceData.ManualConfigIdx > 0)
                {
                    ShowEditMenu(_buttonRead);
                    return;
                }
                PerformAnalyze();
            };
            _buttonSafe = _barView.FindViewById<Button>(Resource.Id.buttonXmlSafe);
            _buttonSafe.Click += (sender, args) =>
            {
                SaveConfiguration(false);
            };

            _textViewCarInfo = FindViewById<TextView>(Resource.Id.textViewCarInfo);
            _listViewEcu = FindViewById<ListView>(Resource.Id.listEcu);
            _ecuListAdapter = new EcuListAdapter(this);
            _ecuListAdapter.CheckChanged += EcuCheckChanged;
            _ecuListAdapter.MenuOptionsSelected += MenuOptionsSelected;
            _listViewEcu.Adapter = _ecuListAdapter;
            _listViewEcu.ItemClick += (sender, args) =>
            {
                int pos = args.Position;
                if (pos >= 0)
                {
                    PerformJobsRead(_ecuList[pos]);
                }
            };
            _listViewEcu.ItemLongClick += (sender, args) =>
            {
                ShowContextMenu(args.View, args.Position);
            };

            _ruleEvalBmw = new RuleEvalBmw();
            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                    UpdateDisplay();
                }
            }, BroadcastReceived)
            {
                SelectedInterface = (ActivityCommon.InterfaceType)
                    Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None)
            };

            _checkAdapter = new CheckAdapter(_activityCommon);

            _ecuDir = Intent.GetStringExtra(ExtraInitDir);
            _vagDir = Intent.GetStringExtra(ExtraVagDir);
            _bmwDir = Intent.GetStringExtra(ExtraBmwDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            if (!_activityRecreated)
            {
                _instanceData.DeviceName = Intent.GetStringExtra(ExtraDeviceName);
                _instanceData.DeviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            }
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);
            _lastFileName = Intent.GetStringExtra(ExtraFileName);
            _datUkdDir = ActivityCommon.GetVagDatUkdDir(_ecuDir);

            ViewStates visibility = IsPageSelectionActive() ? ViewStates.Gone : ViewStates.Visible;
            _buttonRead.Visibility = visibility;
            _buttonSafe.Visibility = visibility;
            _textViewCarInfo.Visibility = visibility;
            _listViewEcu.Visibility = visibility;

            string configName = Path.GetFileNameWithoutExtension(_lastFileName);
            if (!string.IsNullOrEmpty(configName) && configName.StartsWith(ManualConfigName))
            {
                try
                {
                    _instanceData.ManualConfigIdx = Convert.ToInt32(configName.Substring(ManualConfigName.Length, 1));
                }
                catch (Exception)
                {
                    _instanceData.ManualConfigIdx = 0;
                }
            }
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && _instanceData.ManualConfigIdx == 0)
            {
                _instanceData.ManualConfigIdx = 1;
            }

            _activityCommon.SetPreferredNetworkInterface();

            EdiabasClose(_instanceData.ForceAppend);
            if (!_activityRecreated && (_instanceData.ManualConfigIdx > 0 || IsPageSelectionActive()))
            {
                EdiabasOpen();
                ReadAllXml();
                if (_instanceData.ManualConfigIdx > 0)
                {
                    ExecuteUpdateEcuInfo(error =>
                    {
                        if (!error && IsPageSelectionActive())
                        {
                            if (!SelectPageFile(_pageFileName))
                            {
                                ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_msg_page_not_avail);
                            }
                        }
                    });
                }
                else
                {
                    if (IsPageSelectionActive())
                    {
                        if (!SelectPageFile(_pageFileName))
                        {
                            ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_msg_page_not_avail);
                        }

                        return;
                    }
                }
            }
            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_activityCommon != null)
            {
                if (_activityCommon.MtcBtService)
                {
                    _activityCommon.StartMtcService();
                }
                _activityCommon?.RequestUsbPermission(null);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            _activityActive = true;
            if (!_activityCommon.RequestEnableTranslate((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                HandleStartDialogs();
            }))
            {
                HandleStartDialogs();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            _instanceData.ForceAppend = true;   // OnSaveInstanceState is called before OnStop
            _activityActive = false;
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (_activityCommon != null && _activityCommon.MtcBtService)
            {
                _activityCommon.StopMtcService();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ediabasJobAbort = true;
            if (IsJobRunning())
            {
                _jobThread.Join();
            }
            EdiabasClose(true);

            _checkAdapter?.Dispose();
            _checkAdapter = null;

            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressedEvent()
        {
            if (IsJobRunning())
            {
                return;
            }
            if (!_buttonSafe.Enabled)
            {
                OnBackPressedContinue();
                return;
            }

            int resourceId = Resource.String.xml_tool_msg_save_config_select;
            if (!_ecuList.Any(x => x.Selected))
            {
                resourceId = Resource.String.xml_tool_msg_save_config_empty;
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    SaveConfiguration(true);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    OnBackPressedContinue();
                })
                .SetNeutralButton(Resource.String.button_abort, (sender, args) =>
                {
                })
                .SetMessage(resourceId)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
        }

        private void OnBackPressedContinue()
        {
            if (!SendTraceFile((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                base.OnBackPressedEvent();
            }))
            {
                base.OnBackPressedEvent();
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestAppDetailBtSettings:
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestSelectSgbd:
                    // When FilePickerActivity returns with a file
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        string fileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        if (string.IsNullOrEmpty(fileName))
                        {
                            break;
                        }
                        string ecuName = Path.GetFileNameWithoutExtension(fileName);
                        if (string.IsNullOrEmpty(ecuName))
                        {
                            break;
                        }
                        if (_ecuList.Any(ecuInfo => string.Compare(ecuInfo.Sgbd, ecuName, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            break;
                        }
                        EcuInfo ecuInfoNew = new EcuInfo(ecuName, -1, string.Empty, ecuName, string.Empty)
                        {
                            PageName = string.Empty,
                            EcuName = string.Empty
                        };
                        _ecuList.Add(ecuInfoNew);
                        ExecuteUpdateEcuInfo();
                        UpdateOptionsMenu();
                        UpdateDisplay();
                    }
                    break;

                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _instanceData.DeviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _instanceData.DeviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        bool callAdapterConfig = data.Extras.GetBoolean(DeviceListActivity.ExtraCallAdapterConfig, false);
                        EdiabasClose();
                        UpdateOptionsMenu();
                        if (callAdapterConfig)
                        {
                            AdapterConfig();
                        }
                        else if (_instanceData.AutoStart)
                        {
                            ExecuteAnalyzeJob(_instanceData.AutoStartSearchStartIndex);
                        }
                    }
                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestAdapterConfig:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        bool invalidateAdapter = data.Extras.GetBoolean(CanAdapterActivity.ExtraInvalidateAdapter, false);
                        if (invalidateAdapter)
                        {
                            _instanceData.DeviceName = string.Empty;
                            _instanceData.DeviceAddress = string.Empty;
                            UpdateOptionsMenu();
                        }
                    }
                    break;

                case ActivityRequest.RequestSelectJobs:
                    if (IsPageSelectionActive())
                    {
                        Finish();
                    }

                    _ecuFuncCallMenu = EcuFunctionCallType.None;
                    if (XmlToolEcuActivity.IntentEcuInfo?.JobList != null)
                    {
                        int selectCount = XmlToolEcuActivity.IntentEcuInfo.JobList.Count(job => job.Selected);
                        XmlToolEcuActivity.IntentEcuInfo.Selected = selectCount > 0;
                        _ecuListAdapter.NotifyDataSetChanged();
                        UpdateDisplay();
                    }

                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        bool callEdiabasTool = data.Extras.GetBoolean(XmlToolEcuActivity.ExtraCallEdiabasTool, false);
                        if (callEdiabasTool)
                        {
                            StartEdiabasTool(XmlToolEcuActivity.IntentEcuInfo);
                        }
                    }
                    break;

                case ActivityRequest.RequestOpenExternalFile:
                    UpdateOptionsMenu();
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = ActivityCommon.IsTranslationAvailable();
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestEdiabasTool:
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (_activityCommon == null)
            {
                return;
            }
            switch (requestCode)
            {
                case ActivityCommon.RequestPermissionBluetooth:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        UpdateOptionsMenu();
                        break;
                    }

                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            ActivityCommon.OpenAppSettingDetails(this, (int)ActivityRequest.RequestAppDetailBtSettings);
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.access_permission_rejected)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (IsPageSelectionActive())
            {
                return false;
            }

            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.xml_tool_menu, menu);
            return true;
        }

        public override void PrepareOptionsMenu(IMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            bool commActive = IsJobRunning();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable();

            IMenuItem selInterfaceMenu = menu.FindItem(Resource.Id.menu_tool_sel_interface);
            if (selInterfaceMenu != null)
            {
                selInterfaceMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_interface), _activityCommon.InterfaceName()));
                selInterfaceMenu.SetEnabled(!commActive);
            }

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_adapter), _instanceData.DeviceName));
                scanMenu.SetEnabled(!commActive && interfaceAvailable);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem adapterConfigMenu = menu.FindItem(Resource.Id.menu_adapter_config);
            if (adapterConfigMenu != null)
            {
                adapterConfigMenu.SetEnabled(interfaceAvailable && !commActive);
                adapterConfigMenu.SetVisible(_activityCommon.AllowAdapterConfig(_instanceData.DeviceAddress));
            }

            IMenuItem enetIpMenu = menu.FindItem(Resource.Id.menu_enet_ip);
            if (enetIpMenu != null)
            {
                bool menuVisible = _activityCommon.GetAdapterIpName(out string longName, out string _);

                enetIpMenu.SetTitle(longName);
                enetIpMenu.SetEnabled(interfaceAvailable && !commActive);
                enetIpMenu.SetVisible(menuVisible);
            }

            IMenuItem addErrorsMenu = menu.FindItem(Resource.Id.menu_xml_tool_add_errors_page);
            if (addErrorsMenu != null)
            {
                addErrorsMenu.SetEnabled(_ecuList.Count > 0 && !_instanceData.NoErrorsPageUpdate);
                addErrorsMenu.SetChecked(_instanceData.AddErrorsPage);
            }

            IMenuItem cfgTypeSubMenu = menu.FindItem(Resource.Id.menu_xml_tool_submenu_cfg_type);
            if (cfgTypeSubMenu != null)
            {
                cfgTypeSubMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_xml_tool_cfg_type),
                    (_instanceData.ManualConfigIdx > 0) ?
                    GetString(Resource.String.xml_tool_man_config) + " " + _instanceData.ManualConfigIdx.ToString(CultureInfo.InvariantCulture) :
                    GetString(Resource.String.xml_tool_auto_config)));
                cfgTypeSubMenu.SetEnabled(interfaceAvailable && !commActive);
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            logSubMenu?.SetEnabled(interfaceAvailable && !commActive);

            IMenuItem traceSubmenu = menu.FindItem(Resource.Id.menu_trace_submenu);
            traceSubmenu?.SetEnabled(!commActive);

            bool tracePresent = ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir);
            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);

            IMenuItem openTraceMenu = menu.FindItem(Resource.Id.menu_open_trace);
            openTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);

            IMenuItem translationSubmenu = menu.FindItem(Resource.Id.menu_translation_submenu);
            if (translationSubmenu != null)
            {
                translationSubmenu.SetEnabled(true);
                translationSubmenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            IMenuItem translationEnableMenu = menu.FindItem(Resource.Id.menu_translation_enable);
            if (translationEnableMenu != null)
            {
                translationEnableMenu.SetEnabled(!commActive || ActivityCommon.IsTranslationAvailable());
                translationEnableMenu.SetVisible(ActivityCommon.IsTranslationRequired());
                translationEnableMenu.SetChecked(ActivityCommon.EnableTranslation);
            }

            IMenuItem translationYandexKeyMenu = menu.FindItem(Resource.Id.menu_translation_yandex_key);
            if (translationYandexKeyMenu != null)
            {
                translationYandexKeyMenu.SetEnabled(!commActive);
                translationYandexKeyMenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            IMenuItem translationClearCacheMenu = menu.FindItem(Resource.Id.menu_translation_clear_cache);
            if (translationClearCacheMenu != null)
            {
                translationClearCacheMenu.SetEnabled(!_activityCommon.IsTranslationCacheEmpty());
                translationClearCacheMenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    UpdateDisplay();
                    if (_buttonSafe.Enabled)
                    {
                        int resourceId = Resource.String.xml_tool_msg_save_config_select;
                        if (!_ecuList.Any(x => x.Selected))
                        {
                            resourceId = Resource.String.xml_tool_msg_save_config_empty;
                        }
                        new AlertDialog.Builder(this)
                            .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                            {
                                SaveConfiguration(true);
                            })
                            .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                            {
                                FinishContinue();
                            })
                            .SetNeutralButton(Resource.String.button_abort, (sender, args) =>
                            {
                            })
                            .SetMessage(resourceId)
                            .SetTitle(Resource.String.alert_title_question)
                            .Show();
                    }
                    else
                    {
                        FinishContinue();
                    }
                    return true;

                case Resource.Id.menu_tool_sel_interface:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectInterface();
                    return true;

                case Resource.Id.menu_scan:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectBluetoothDevice();
                    return true;

                case Resource.Id.menu_adapter_config:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    AdapterConfig();
                    return true;

                case Resource.Id.menu_enet_ip:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    AdapterIpConfig();
                    return true;

                case Resource.Id.menu_xml_tool_add_errors_page:
                    _instanceData.AddErrorsPage = !_instanceData.AddErrorsPage;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    return true;

                case Resource.Id.menu_xml_tool_submenu_cfg_type:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectConfigTypeRequest();
                    return true;

                case Resource.Id.menu_submenu_log:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectDataLogging();
                    return true;

                case Resource.Id.menu_send_trace:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SendTraceFileAlways((sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        UpdateOptionsMenu();
                    });
                    return true;

                case Resource.Id.menu_open_trace:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    OpenTraceFile();
                    return true;

                case Resource.Id.menu_translation_enable:
                    if (!ActivityCommon.EnableTranslation && !ActivityCommon.IsTranslationAvailable())
                    {
                        EditYandexKey();
                        return true;
                    }

                    ActivityCommon.EnableTranslation = !ActivityCommon.EnableTranslation;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    return true;

                case Resource.Id.menu_translation_yandex_key:
                    EditYandexKey();
                    return true;

                case Resource.Id.menu_translation_clear_cache:
                    _activityCommon.ClearTranslationCache();
                    ResetTranslations();
                    _ecuListTranslated = false;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    return true;

                case Resource.Id.menu_submenu_help:
                    _activityCommon.ShowWifiConnectedWarning(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        try
                        {
                            StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://github.com/uholeschak/ediabaslib/blob/master/docs/Configuration_Generator.md")));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void FinishContinue()
        {
            if (!SendTraceFile((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                Finish();
            }))
            {
                Finish();
            }
        }

        private void HandleStartDialogs()
        {
            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.None)
            {
                SelectInterface();
            }
            SelectInterfaceEnable();
            UpdateOptionsMenu();
            UpdateDisplay();
        }

        private void EdiabasOpen()
        {
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet
                {
                    EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                    AbortJobFunc = AbortEdiabasJob
                };
                _ediabas.SetConfigProperty("EcuPath", _ecuDir);
                _sgFunctions = new SgFunctions(_ediabas);
                UpdateLogInfo();
            }

            _ediabas.EdInterfaceClass.EnableTransmitCache = false;
            _activityCommon.SetEdiabasInterface(_ediabas, _instanceData.DeviceAddress);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool EdiabasClose(bool forceAppend = false)
        {
            if (IsJobRunning())
            {
                return false;
            }

            if (_sgFunctions != null)
            {
                _sgFunctions.Dispose();
                _sgFunctions = null;
            }

            if (_ediabas != null)
            {
                _ediabas.Dispose();
                _ediabas = null;
            }
            _instanceData.ForceAppend = forceAppend;
            UpdateDisplay();
            UpdateOptionsMenu();
            return true;
        }

        private void ClearVehicleInfo()
        {
            _instanceData.SgbdFunctional = string.Empty;
            _instanceData.Vin = string.Empty;
            _instanceData.VehicleType = string.Empty;
            _instanceData.CDate = string.Empty;
        }

        private void ClearEcuList()
        {
            ClearVehicleInfo();
            _ecuList.Clear();
            _ecuInfoMot = null;
            _ecuInfoDid = null;
            _ecuListTranslated = false;
            _instanceData.EcuSearchAbortIndex = -1;
        }

        private bool IsJobRunning()
        {
            if (_jobThread == null)
            {
                return false;
            }
            if (_jobThread.IsAlive)
            {
                return true;
            }
            _jobThread = null;
            return false;
        }

        private bool IsPageSelectionActive()
        {
            return !string.IsNullOrEmpty(_pageFileName);
        }

        private bool SendTraceFile(EventHandler<EventArgs> handler)
        {
            if (_instanceData.CommErrorsOccurred && _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.RequestSendTraceFile(_appDataDir, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (ActivityCommon.CollectDebugInfo ||
                (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir)))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.SendTraceFile(_appDataDir, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        private bool OpenTraceFile()
        {
            string baseDir = _instanceData.TraceDir;
            if (string.IsNullOrEmpty(baseDir))
            {
                return false;
            }

            if (!EdiabasClose())
            {
                return false;
            }

            string traceFile = Path.Combine(baseDir, ActivityCommon.TraceFileName);
            string errorMessage = _activityCommon.OpenExternalFile(traceFile, (int)ActivityRequest.RequestOpenExternalFile);
            if (errorMessage != null)
            {
                if (string.IsNullOrEmpty(traceFile))
                {
                    return true;
                }

                string message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.open_trace_file_failed), traceFile);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message = errorMessage + "\r\n" + message;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return false;
            }
            return true;
        }

        private void UpdateDisplay()
        {
            if (IsPageSelectionActive())
            {
                _buttonRead.Enabled = false;
                _buttonSafe.Enabled = false;
                return;
            }

            if (ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation && !ActivityCommon.IsTranslationAvailable())
            {
                EditYandexKey();
                return;
            }

            _ecuListAdapter.Items.Clear();
            if (_ecuList.Count == 0)
            {
                ClearEcuList();
            }
            else
            {
                if (TranslateEcuText((sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    UpdateDisplay();
                }))
                {
                    return;
                }
                foreach (EcuInfo ecu in _ecuList)
                {
                    _ecuListAdapter.Items.Add(ecu);
                }
            }
            if (!ActivityCommon.EnableTranslation)
            {
                _ecuListTranslated = false;
            }

            _buttonRead.Text = GetString((_instanceData.ManualConfigIdx > 0) ?
                Resource.String.button_xml_tool_edit : Resource.String.button_xml_tool_read);
            _buttonRead.Enabled = _activityCommon.IsInterfaceAvailable();
            int selectedCount = _ecuList.Count(ecuInfo => ecuInfo.Selected);
            _buttonSafe.Enabled = (_ecuList.Count > 0) && (_instanceData.AddErrorsPage || (selectedCount > 0));
            _ecuListAdapter.NotifyDataSetChanged();

            string statusText = string.Empty;
            if (_ecuList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(GetString(Resource.String.xml_tool_ecu_list));
                if (!string.IsNullOrEmpty(_instanceData.Vin))
                {
                    sb.Append(" (");
                    sb.Append(GetString(Resource.String.xml_tool_info_vin));
                    sb.Append(": ");
                    sb.Append(_instanceData.Vin);
                    if (!string.IsNullOrEmpty(_instanceData.VehicleType))
                    {
                        sb.Append("/");
                        sb.Append(_instanceData.VehicleType);
                    }
                    if (!string.IsNullOrEmpty(_instanceData.CDate))
                    {
                        sb.Append("/");
                        sb.Append(_instanceData.CDate);
                    }
                    sb.Append(")");
                }
                statusText = sb.ToString();
            }
            _textViewCarInfo.Text = statusText;
        }

        private bool TranslateEcuText(EventHandler<EventArgs> handler = null)
        {
            if (_translateEnabled && !_translateActive && ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation)
            {
                if (!_ecuListTranslated)
                {
                    _ecuListTranslated = true;
                    List<string> stringList = new List<string>();
                    foreach (EcuInfo ecu in _ecuList)
                    {
                        if (!string.IsNullOrEmpty(ecu.Description) && ecu.DescriptionTransRequired && ecu.DescriptionTrans == null)
                        {
                            stringList.Add(ecu.Description);
                        }
                        if (ecu.JobList != null)
                        {
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach (XmlToolEcuActivity.JobInfo jobInfo in ecu.JobList)
                            {
                                if (jobInfo.Comments != null && jobInfo.CommentsTransRequired && jobInfo.CommentsTrans == null &&
                                    XmlToolEcuActivity.IsValidJob(jobInfo, ecu))
                                {
                                    foreach (string comment in jobInfo.Comments)
                                    {
                                        if (!string.IsNullOrEmpty(comment))
                                        {
                                            stringList.Add(comment);
                                        }
                                    }
                                }
                                if (jobInfo.Results != null)
                                {
                                    foreach (XmlToolEcuActivity.ResultInfo result in jobInfo.Results)
                                    {
                                        if (result.Comments != null && result.CommentsTransRequired && result.CommentsTrans == null)
                                        {
                                            foreach (string comment in result.Comments)
                                            {
                                                if (!string.IsNullOrEmpty(comment))
                                                {
                                                    stringList.Add(comment);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // ReSharper restore LoopCanBeConvertedToQuery
                        }
                    }
                    if (stringList.Count == 0)
                    {
                        return false;
                    }
                    _translateActive = true;
                    if (_activityCommon.TranslateStrings(stringList, transList =>
                    {
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            _translateActive = false;
                            try
                            {
                                if (transList != null && transList.Count == stringList.Count)
                                {
                                    int transIndex = 0;
                                    foreach (EcuInfo ecu in _ecuList)
                                    {
                                        if (!string.IsNullOrEmpty(ecu.Description) && ecu.DescriptionTransRequired && ecu.DescriptionTrans == null)
                                        {
                                            if (transIndex < transList.Count)
                                            {
                                                ecu.DescriptionTrans = transList[transIndex++];
                                            }
                                        }
                                        if (ecu.JobList != null)
                                        {
                                            foreach (XmlToolEcuActivity.JobInfo jobInfo in ecu.JobList)
                                            {
                                                if (jobInfo.Comments != null && jobInfo.CommentsTransRequired && jobInfo.CommentsTrans == null &&
                                                    XmlToolEcuActivity.IsValidJob(jobInfo, ecu))
                                                {
                                                    jobInfo.CommentsTrans = new List<string>();
                                                    foreach (string comment in jobInfo.Comments)
                                                    {
                                                        if (!string.IsNullOrEmpty(comment))
                                                        {
                                                            if (transIndex < transList.Count)
                                                            {
                                                                jobInfo.CommentsTrans.Add(transList[transIndex++]);
                                                            }
                                                        }
                                                    }
                                                }
                                                if (jobInfo.Results != null)
                                                {
                                                    foreach (XmlToolEcuActivity.ResultInfo result in jobInfo.Results)
                                                    {
                                                        if (result.Comments != null && result.CommentsTransRequired && result.CommentsTrans == null)
                                                        {
                                                            result.CommentsTrans = new List<string>();
                                                            foreach (string comment in result.Comments)
                                                            {
                                                                if (!string.IsNullOrEmpty(comment))
                                                                {
                                                                    if (transIndex < transList.Count)
                                                                    {
                                                                        result.CommentsTrans.Add(transList[transIndex++]);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            handler?.Invoke(this, new EventArgs());
                        });
                    }))
                    {
                        return true;
                    }
                    _translateActive = false;
                }
            }
            else
            {
                ResetTranslations();
            }
            return false;
        }

        private void ResetTranslations()
        {
            foreach (EcuInfo ecu in _ecuList)
            {
                ecu.DescriptionTrans = null;
                if (ecu.JobList != null)
                {
                    foreach (XmlToolEcuActivity.JobInfo jobInfo in ecu.JobList)
                    {
                        jobInfo.CommentsTrans = null;
                        if (jobInfo.Results != null)
                        {
                            foreach (XmlToolEcuActivity.ResultInfo result in jobInfo.Results)
                            {
                                result.CommentsTrans = null;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateLogInfo()
        {
            if (_ediabas == null)
            {
                return;
            }

            string logDir = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(_appDataDir))
                {
                    logDir = Path.Combine(_appDataDir, "LogConfigTool");
                    Directory.CreateDirectory(logDir);
                }
            }
            catch (Exception)
            {
                logDir = string.Empty;
            }

            _instanceData.TraceDir = string.Empty;
            if (_instanceData.TraceActive)
            {
                _instanceData.TraceDir = logDir;
            }

            if (!string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                _ediabas.SetConfigProperty("TracePath", _instanceData.TraceDir);
                _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                _ediabas.SetConfigProperty("AppendTrace", _instanceData.TraceAppend || _instanceData.ForceAppend ? "1" : "0");
                _ediabas.SetConfigProperty("CompressTrace", "1");
            }
            else
            {
                _ediabas.SetConfigProperty("IfhTrace", "0");
            }
        }

        private void SelectSgbdFile(bool groupFile)
        {
            // Launch the FilePickerActivity to select a sgbd file
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.tool_select_sgbd));
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, _ecuDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, groupFile ? ".grp" : ".prg");
            serverIntent.PutExtra(FilePickerActivity.ExtraDirChange, false);
            serverIntent.PutExtra(FilePickerActivity.ExtraShowExtension, false);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectSgbd);
        }

        private void SelectJobs(EcuInfo ecuInfo)
        {
            try
            {
                if (ecuInfo?.JobList == null)
                {
                    return;
                }

                if (!EdiabasClose(true))
                {
                    return;
                }

                if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                {
                    string ecuSgbdName = ecuInfo.Sgbd ?? string.Empty;
                    EcuFunctionStructs.EcuVariant ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(ecuSgbdName);
                    _ruleEvalBmw?.UpdateEvalEcuProperties(ecuVariant);
                }

                XmlToolEcuActivity.IntentEcuInfo = ecuInfo;
                XmlToolEcuActivity.IntentRuleEvalBmw = _ruleEvalBmw;
                Intent serverIntent = new Intent(this, typeof(XmlToolEcuActivity));
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraAppDataDir, _appDataDir);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraEcuName, ecuInfo.Name);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraEcuDir, _ecuDir);
                if (!string.IsNullOrEmpty(_instanceData.VehicleType))
                {
                    serverIntent.PutExtra(XmlToolEcuActivity.ExtraVehicleType, _instanceData.VehicleType);
                }
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraTraceDir, _instanceData.TraceDir);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraTraceAppend, _instanceData.TraceAppend || _instanceData.ForceAppend);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(XmlToolEcuActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);

                EcuFunctionCallType ecuFuncCall = _ecuFuncCallMenu;
                if (ecuFuncCall == EcuFunctionCallType.None)
                {
                    ecuFuncCall = _ecuFuncCall;
                }

                if (ecuFuncCall != EcuFunctionCallType.None)
                {
                    serverIntent.PutExtra(XmlToolEcuActivity.ExtraEcuFuncCall, (int)ecuFuncCall);
                }

                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectJobs);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool EditYandexKey()
        {
            try
            {
                Intent serverIntent = new Intent(this, typeof(YandexKeyActivity));
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestYandexKey);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void StartEdiabasTool(EcuInfo ecuInfo)
        {
            if (ecuInfo == null)
            {
                return;
            }
            if (!EdiabasClose(true))
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
            EdiabasToolActivity.IntentTranslateActivty = _activityCommon;
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInitDir, _ecuDir);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraAppDataDir, _appDataDir);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraSgbdFile, Path.Combine(_ecuDir, ecuInfo.Sgbd));
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceName, _instanceData.DeviceName);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEdiabasTool);
        }

        private void SelectInterface()
        {
            if (IsJobRunning())
            {
                return;
            }
            _activityCommon.SelectInterface((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _instanceData.AdapterCheckOk = false;
                EdiabasClose();
                UpdateOptionsMenu();
                SelectInterfaceEnable();
            });
        }

        private void SelectInterfaceEnable()
        {
            _activityCommon.RequestInterfaceEnable((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                UpdateOptionsMenu();
            });
        }

        private void SelectDataLogging()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.menu_submenu_log);
            ListView listView = new ListView(this);

            List<string> logNames = new List<string>
            {
                GetString(Resource.String.datalog_enable_trace),
                GetString(Resource.String.datalog_append_trace),
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemMultipleChoice, logNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Multiple;
            listView.SetItemChecked(0, _instanceData.TraceActive);
            listView.SetItemChecked(1, _instanceData.TraceAppend);

            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                SparseBooleanArray sparseArray = listView.CheckedItemPositions;
                for (int i = 0; i < sparseArray.Size(); i++)
                {
                    bool value = sparseArray.ValueAt(i);
                    switch (sparseArray.KeyAt(i))
                    {
                        case 0:
                            _instanceData.TraceActive = value;
                            break;

                        case 1:
                            _instanceData.TraceAppend = value;
                            break;
                    }
                }
                UpdateLogInfo();
                UpdateOptionsMenu();
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            builder.Show();
        }

        private void SelectConfigTypeRequest()
        {
            UpdateDisplay();
            if (_buttonSafe.Enabled)
            {
                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        SaveConfiguration(false);
                        SelectConfigType();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                        SelectConfigType();
                    })
                    .SetNeutralButton(Resource.String.button_abort, (sender, args) =>
                    {
                    })
                    .SetMessage(Resource.String.xml_tool_msg_save_config)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            else
            {
                SelectConfigType();
            }
        }

        private void SelectConfigType()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.menu_xml_tool_cfg_type);
            ListView listView = new ListView(this);

            List<string> manualNames = new List<string>();
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                manualNames.Add(GetString(Resource.String.xml_tool_auto_config));
            }
            for (int i = 0; i < 4; i++)
            {
                manualNames.Add(GetString(Resource.String.xml_tool_man_config) + " " + (i + 1).ToString(CultureInfo.InvariantCulture));
            }
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemSingleChoice, manualNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                listView.SetItemChecked(_instanceData.ManualConfigIdx, true);
            }
            else
            {
                listView.SetItemChecked(_instanceData.ManualConfigIdx - 1, true);
            }

            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                {
                    _instanceData.ManualConfigIdx = listView.CheckedItemPosition >= 0 ? listView.CheckedItemPosition : 0;
                }
                else
                {
                    _instanceData.ManualConfigIdx = listView.CheckedItemPosition + 1;
                }
                ClearEcuList();
                if (_instanceData.ManualConfigIdx > 0)
                {
                    EdiabasOpen();
                    ReadAllXml();
                    ExecuteUpdateEcuInfo();
                }
                UpdateOptionsMenu();
                UpdateDisplay();
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            builder.Show();
        }

        private void RequestClearEcu()
        {
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                {
                    ClearEcuList();
                    UpdateDisplay();
                    PerformAnalyze();
                })
                .SetNegativeButton(Resource.String.button_no, (s, a) =>
                {
                    PerformAnalyze();
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.xml_tool_clear_ecus)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
        }

        private void ShowEditMenu(View anchor)
        {
            AndroidX.AppCompat.Widget.PopupMenu popupEdit = new AndroidX.AppCompat.Widget.PopupMenu(this, anchor);
            popupEdit.Inflate(Resource.Menu.xml_tool_edit);
            IMenuItem detectMenuMenu = popupEdit.Menu.FindItem(Resource.Id.menu_xml_tool_edit_detect);
            detectMenuMenu?.SetVisible(ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw);
            popupEdit.MenuItemClick += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                switch (args.Item.ItemId)
                {
                    case Resource.Id.menu_xml_tool_edit_detect:
                        if (_ecuList.Count > 0)
                        {
                            if (_instanceData.EcuSearchAbortIndex >= 0)
                            {
                                new AlertDialog.Builder(this)
                                    .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                                    {
                                        PerformAnalyze(_instanceData.EcuSearchAbortIndex);
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (s, a) =>
                                    {
                                        RequestClearEcu();
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.xml_tool_continue_search)
                                    .SetTitle(Resource.String.alert_title_question)
                                    .Show();
                                break;
                            }
                            RequestClearEcu();
                            break;
                        }
                        PerformAnalyze();
                        break;

                    case Resource.Id.menu_xml_tool_edit_grp:
                    case Resource.Id.menu_xml_tool_edit_prg:
                        SelectSgbdFile(args.Item.ItemId == Resource.Id.menu_xml_tool_edit_grp);
                        break;

                    case Resource.Id.menu_xml_tool_edit_del:
                    {
                        for (int i = 0; i < _ecuList.Count; i++)
                        {
                            EcuInfo ecuInfo = _ecuList[i];
                            if (!ecuInfo.Selected)
                            {
                                _ecuList.Remove(ecuInfo);
                                i = 0;
                            }
                        }
                        UpdateDisplay();
                        break;
                    }

                    case Resource.Id.menu_xml_tool_edit_del_all:
                        new AlertDialog.Builder(this)
                            .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                            {
                                DeleteAllXml();
                            })
                            .SetNegativeButton(Resource.String.button_no, (s, a) =>
                            {
                            })
                            .SetCancelable(true)
                            .SetMessage(Resource.String.xml_tool_del_all_info)
                            .SetTitle(Resource.String.alert_title_question)
                            .Show();
                        break;
                }
            };
            popupEdit.Show();
        }

        private void ShowContextMenu(View anchor, int itemPos)
        {
            AndroidX.AppCompat.Widget.PopupMenu popupContext = new AndroidX.AppCompat.Widget.PopupMenu(this, anchor);
            popupContext.Inflate(Resource.Menu.xml_tool_context);

            IMenuItem moveTopMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_top);
            moveTopMenu?.SetEnabled(itemPos > 0);

            IMenuItem moveUpMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_up);
            moveUpMenu?.SetEnabled(itemPos > 0);

            IMenuItem moveDownMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_down);
            moveDownMenu?.SetEnabled((itemPos + 1) < _ecuListAdapter.Items.Count);

            IMenuItem moveBottomMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_bottom);
            moveBottomMenu?.SetEnabled((itemPos + 1) < _ecuListAdapter.Items.Count);

            bool enableMenuAction = itemPos >= 0 && itemPos < _ecuListAdapter.Items.Count && !IsJobRunning();
            bool bmwVisible = ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw;
            bool vagVisible = ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw;

            IMenuItem ediabasToolMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_ediabas_tool);
            ediabasToolMenu?.SetEnabled(enableMenuAction);

            IMenuItem bmwActuatorMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_bmw_actuator);
            if (bmwActuatorMenu != null)
            {
                bmwActuatorMenu.SetEnabled(enableMenuAction);
                bmwActuatorMenu.SetVisible(bmwVisible);
            }

            IMenuItem vagCodingMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_vag_coding);
            if (vagCodingMenu != null)
            {
                vagCodingMenu.SetEnabled(enableMenuAction);
                vagCodingMenu.SetVisible(vagVisible);
            }

            IMenuItem vagCoding2Menu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_vag_coding2);
            if (vagCoding2Menu != null)
            {
                vagCoding2Menu.SetEnabled(enableMenuAction);
                vagCoding2Menu.SetVisible(vagVisible);
            }

            IMenuItem vagAdaptionMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_vag_adaption);
            if (vagAdaptionMenu != null)
            {
                vagAdaptionMenu.SetEnabled(enableMenuAction);
                vagAdaptionMenu.SetVisible(vagVisible);
            }

            IMenuItem vagLoginMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_vag_login);
            if (vagLoginMenu != null)
            {
                vagLoginMenu.SetEnabled(enableMenuAction);
                vagLoginMenu.SetVisible(vagVisible);
            }

            IMenuItem vagSecAccessMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_vag_sec_access);
            if (vagSecAccessMenu != null)
            {
                vagSecAccessMenu.SetEnabled(enableMenuAction);
                vagSecAccessMenu.SetVisible(vagVisible);
            }

            popupContext.MenuItemClick += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                switch (args.Item.ItemId)
                {
                    case Resource.Id.menu_xml_tool_move_top:
                    {
                        EcuInfo oldItem = _ecuList[itemPos];
                        _ecuList.RemoveAt(itemPos);
                        _ecuList.Insert(0, oldItem);
                        UpdateDisplay();
                        break;
                    }

                    case Resource.Id.menu_xml_tool_move_up:
                        (_ecuList[itemPos], _ecuList[itemPos - 1]) = (_ecuList[itemPos - 1], _ecuList[itemPos]);
                        UpdateDisplay();
                        break;

                    case Resource.Id.menu_xml_tool_move_down:
                        (_ecuList[itemPos], _ecuList[itemPos + 1]) = (_ecuList[itemPos + 1], _ecuList[itemPos]);
                        UpdateDisplay();
                        break;

                    case Resource.Id.menu_xml_tool_move_bottom:
                    {
                        EcuInfo oldItem = _ecuList[itemPos];
                        _ecuList.RemoveAt(itemPos);
                        _ecuList.Add(oldItem);
                        UpdateDisplay();
                        break;
                    }

                    case Resource.Id.menu_xml_tool_ediabas_tool:
                        StartEdiabasTool(_ecuList[itemPos]);
                        break;

                    case Resource.Id.menu_xml_tool_bmw_actuator:
                        CallEcuFunction(_ecuList[itemPos], EcuFunctionCallType.BmwActuator);
                        break;

                    case Resource.Id.menu_xml_tool_vag_coding:
                        CallEcuFunction(_ecuList[itemPos], EcuFunctionCallType.VagCoding);
                        break;

                    case Resource.Id.menu_xml_tool_vag_coding2:
                        CallEcuFunction(_ecuList[itemPos], EcuFunctionCallType.VagCoding2);
                        break;

                    case Resource.Id.menu_xml_tool_vag_adaption:
                        CallEcuFunction(_ecuList[itemPos], EcuFunctionCallType.VagAdaption);
                        break;

                    case Resource.Id.menu_xml_tool_vag_login:
                        CallEcuFunction(_ecuList[itemPos], EcuFunctionCallType.VagLogin);
                        break;

                    case Resource.Id.menu_xml_tool_vag_sec_access:
                        CallEcuFunction(_ecuList[itemPos], EcuFunctionCallType.VagSecAccess);
                        break;
                }
            };
            popupContext.Show();
        }

        private bool CallEcuFunction(EcuInfo ecuInfo, EcuFunctionCallType callType)
        {
            if (ecuInfo == null)
            {
                return false;
            }

            _ecuFuncCallMenu = callType;
            PerformJobsRead(ecuInfo);

            return true;
        }

        private bool SelectBluetoothDevice()
        {
            EdiabasClose();
            _instanceData.AutoStart = false;
            return _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice, _appDataDir);
        }

        private void AdapterConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }

            if (_activityCommon.ShowConnectWarning(action =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                switch (action)
                {
                    case ActivityCommon.SsidWarnAction.Continue:
                        AdapterConfig();
                        break;

                    case ActivityCommon.SsidWarnAction.EditIp:
                        AdapterIpConfig();
                        break;
                }
            }))
            {
                return;
            }

            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet)
            {
                _activityCommon.EnetAdapterConfig();
                return;
            }

            Intent serverIntent = new Intent(this, typeof(CanAdapterActivity));
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
            serverIntent.PutExtra(CanAdapterActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            serverIntent.PutExtra(CanAdapterActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(CanAdapterActivity.ExtraAppDataDir, _appDataDir);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestAdapterConfig);
        }

        private void AdapterIpConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }
            _activityCommon.SelectAdapterIp((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                UpdateOptionsMenu();
            });
        }

        private void PerformAnalyze(int searchStartIndex = -1)
        {
            if (IsJobRunning())
            {
                return;
            }
            _instanceData.AutoStart = false;
            if (string.IsNullOrEmpty(_instanceData.DeviceAddress))
            {
                if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, _appDataDir, (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    _instanceData.AutoStart = true;
                    _instanceData.AutoStartSearchStartIndex = searchStartIndex;
                }))
                {
                    return;
                }
            }
            if (_activityCommon.ShowConnectWarning(action =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                switch (action)
                {
                    case ActivityCommon.SsidWarnAction.Continue:
                        PerformAnalyze(searchStartIndex);
                        break;

                    case ActivityCommon.SsidWarnAction.EditIp:
                        AdapterIpConfig();
                        break;
                }
            }))
            {
                return;
            }

            if (!_instanceData.AdapterCheckOk && _activityCommon.AdapterCheckRequired)
            {
                if (EdiabasClose())
                {
                    if (_checkAdapter.StartCheckAdapter(_appDataDir,
                            _activityCommon.SelectedInterface, _instanceData.DeviceAddress,
                            checkError =>
                            {
                                RunOnUiThread(() =>
                                {
                                    if (!checkError)
                                    {
                                        _instanceData.AdapterCheckOk = true;
                                        ExecuteAnalyzeJob(searchStartIndex);
                                    }

                                    UpdateOptionsMenu();
                                });
                            }))
                    {
                        return;
                    }
                }
            }

            ExecuteAnalyzeJob(searchStartIndex);
        }

        private void ExecuteAnalyzeJob(int searchStartIndex = -1)
        {
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                ExecuteAnalyzeJobBmw();
                return;
            }
            ExecuteAnalyzeJobVag(searchStartIndex);
        }

        private void ExecuteAnalyzeJobBmw()
        {
            _translateEnabled = false;
            bool elmDevice = _activityCommon.IsElmDevice(_instanceData.DeviceAddress);
            EdiabasOpen();
            ClearEcuList();
            UpdateDisplay();

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            progress.Indeterminate = false;
            progress.Progress = 0;
            progress.Max = 100;
            progress.AbortClick = sender => 
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _ediabasJobAbort = true;
                progress.Indeterminate = true;
                progress.ButtonAbort.Enabled = false;
                progress.SetMessage(GetString(Resource.String.xml_tool_aborting));
            };
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockType.ScreenDim);

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                int bestInvalidCount = 0;
                int bestInvalidVinCount = 0;
                List<EcuInfo> ecuListBest = null;
                string ecuFileNameBest = null;
                List<string> ecuFileNameList = null;

                DetectVehicleBmw detectVehicleBmw = new DetectVehicleBmw(_ediabas, _bmwDir);
                detectVehicleBmw.AbortFunc = () => _ediabasJobAbort;
                detectVehicleBmw.ProgressFunc = percent =>
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Progress = percent;
                        }
                    });
                };

                string groupSgbd = null;
                string detectedVin = null;
                if (detectVehicleBmw.DetectVehicleBmwFast())
                {
                    groupSgbd = detectVehicleBmw.GroupSgdb;
                    detectedVin = detectVehicleBmw.Vin;
                    _instanceData.VehicleType = detectVehicleBmw.Series;
                    _instanceData.CDate = null;
                    if (detectVehicleBmw.ConstructYear != null && detectVehicleBmw.ConstructMonth != null)
                    {
                        _instanceData.CDate = detectVehicleBmw.ConstructYear + "-" + detectVehicleBmw.ConstructMonth;
                    }
                }

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!string.IsNullOrEmpty(groupSgbd))
                {
                    ecuFileNameList = new List<string> { groupSgbd };
                }
                else
                {
                    if (elmDevice)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Statistic approach too slow for ELM327 devices");
                    }
                    else
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Fallback to statistic approach");
                        ecuFileNameList = EcuFileNames.ToList();
                    }
                }

                if (ecuFileNameList != null && detectedVin != null && !_ediabasJobAbort)
                {
                    for (int retries = 0; retries < 2; retries++)
                    {
                        _ediabas.EdInterfaceClass.EnableTransmitCache = true;
                        int index = 0;
                        foreach (string fileName in ecuFileNameList)
                        {
                            try
                            {
                                if (_ediabasJobAbort)
                                {
                                    ecuListBest = null;
                                    break;
                                }
                                int invalidEcuCount = 0;
                                int localIndex = index;
                                int fileNameListCount = ecuFileNameList.Count;
                                RunOnUiThread(() =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    if (progress != null && fileNameListCount > 1)
                                    {
                                        progress.Progress = 100 * localIndex / fileNameListCount;
                                    }
                                });

                                ActivityCommon.ResolveSgbdFile(_ediabas, fileName);

                                _ediabas.ArgString = string.Empty;
                                _ediabas.ArgBinaryStd = null;
                                _ediabas.ResultsRequests = string.Empty;
                                _ediabas.ExecuteJob("IDENT_FUNKTIONAL");

                                List<EcuInfo> ecuList = new List<EcuInfo>();
                                List<long> invalidAddrList = new List<long>();
                                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                                if (resultSets != null && resultSets.Count >= 2)
                                {
                                    int dictIndex = 0;
                                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                                    {
                                        if (dictIndex == 0)
                                        {
                                            dictIndex++;
                                            continue;
                                        }
                                        bool ecuDataPresent = false;
                                        string ecuName = string.Empty;
                                        Int64 ecuAdr = -1;
                                        string ecuDesc = string.Empty;
                                        string ecuSgbd = string.Empty;
                                        string ecuGroup = string.Empty;
                                        Int64 dateYear = 0;
                                        // ReSharper disable once InlineOutVariableDeclaration
                                        EdiabasNet.ResultData resultData;
                                        if (resultDict.TryGetValue("ECU_GROBNAME", out resultData))
                                        {
                                            if (resultData.OpData is string)
                                            {
                                                ecuName = (string)resultData.OpData;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ID_SG_ADR", out resultData))
                                        {
                                            if (resultData.OpData is Int64)
                                            {
                                                ecuAdr = (Int64)resultData.OpData;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ECU_NAME", out resultData))
                                        {
                                            if (resultData.OpData is string)
                                            {
                                                ecuDesc = (string)resultData.OpData;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ECU_SGBD", out resultData))
                                        {
                                            ecuDataPresent = true;
                                            if (resultData.OpData is string)
                                            {
                                                ecuSgbd = (string)resultData.OpData;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ECU_GRUPPE", out resultData))
                                        {
                                            ecuDataPresent = true;
                                            if (resultData.OpData is string)
                                            {
                                                ecuGroup = (string)resultData.OpData;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ID_DATUM_JAHR", out resultData))
                                        {
                                            if (resultData.OpData is Int64)
                                            {
                                                dateYear = (Int64)resultData.OpData;
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(ecuName) && ecuAdr >= 0 && !string.IsNullOrEmpty(ecuSgbd))
                                        {
                                            if (ecuList.All(ecuInfo => ecuInfo.Address != ecuAdr))
                                            {
                                                // address not existing
                                                EcuInfo ecuInfo = new EcuInfo(ecuName, ecuAdr, ecuDesc, ecuSgbd, ecuGroup);
                                                if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                                                {
                                                    string ecuSgbdName = ecuInfo.Sgbd ?? string.Empty;
                                                    EcuFunctionStructs.EcuVariant ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(ecuSgbdName);
                                                    if (ecuVariant == null)
                                                    {
                                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "IDENT_FUNKTIONAL No ECU variant found for: Sgbd={0}, Addr={1}, Group={2}", ecuSgbdName, ecuInfo.Address, ecuInfo.Grp);
                                                    }
                                                    else
                                                    {
                                                        string title = ecuVariant.Title?.GetTitle(ActivityCommon.GetCurrentLanguage());
                                                        if (!string.IsNullOrEmpty(title))
                                                        {
                                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "IDENT_FUNKTIONAL ECU variant found for: Sgbd={0}, Title={1}", ecuSgbdName, title);
                                                            ecuInfo.PageName = title;
                                                            ecuInfo.Description = title;
                                                            ecuInfo.DescriptionTransRequired = false;
                                                        }
                                                    }
                                                }

                                                ecuList.Add(ecuInfo);
                                            }
                                        }
                                        else
                                        {
                                            if (ecuDataPresent)
                                            {
                                                if (!ecuName.StartsWith("VIRTSG", StringComparison.OrdinalIgnoreCase) && (dateYear != 0))
                                                {
                                                    invalidAddrList.Add(ecuAdr);
                                                }
                                            }
                                        }
                                        dictIndex++;
                                    }
                                    // ReSharper disable once LoopCanBeConvertedToQuery
                                    foreach (long addr in invalidAddrList)
                                    {
                                        if (ecuList.All(ecuInfo => ecuInfo.Address != addr))
                                        {
                                            invalidEcuCount++;
                                        }
                                    }
                                }

                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detect result: count={0}, invalid={1}", ecuList.Count, invalidEcuCount);
                                int invalidVinCount = 0;
                                bool acceptEcu = false;
                                if (ecuListBest == null || elmDevice)
                                {
                                    acceptEcu = true;
                                }
                                else
                                {
                                    _ediabas.ArgString = string.Empty;
                                    _ediabas.ArgBinaryStd = null;
                                    _ediabas.ResultsRequests = string.Empty;
                                    bool readVinOk = false;
                                    foreach (string vinJob in ReadVinJobs)
                                    {
                                        try
                                        {
                                            _ediabas.ExecuteJob(vinJob);
                                            readVinOk = true;
                                            break;
                                        }
                                        catch (Exception)
                                        {
                                            // ignored
                                        }
                                    }

                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read VIN ok={0}", readVinOk);
                                    invalidVinCount = readVinOk ? 0 : 1;
                                    if (ecuListBest.Count < ecuList.Count)
                                    {
                                        acceptEcu = true;
                                    }
                                    else
                                    {
                                        if (ecuListBest.Count == ecuList.Count && (bestInvalidCount + bestInvalidVinCount) > (invalidEcuCount + invalidVinCount))
                                        {
                                            acceptEcu = true;
                                        }
                                    }
                                }

                                if (acceptEcu)
                                {
                                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Selected ECU");
                                    ecuListBest = ecuList;
                                    ecuFileNameBest = fileName;
                                    bestInvalidCount = invalidEcuCount;
                                    bestInvalidVinCount = invalidVinCount;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            index++;
                        }

                        if (ecuListBest != null)
                        {
                            break;
                        }

                        if (ecuFileNameList.Count > 1)
                        {
                            break;
                        }

                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No response, retry with statistic approach");
                        ecuFileNameList = EcuFileNames.ToList();
                    }
                }

                if (ecuListBest != null)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Selected Ecu file: {0}", ecuFileNameBest);
                    _ecuList.AddRange(ecuListBest.OrderBy(x => x.Name));
                    _instanceData.SgbdFunctional = ecuFileNameBest;

                    if (!elmDevice || string.IsNullOrEmpty(detectedVin))
                    {
                        try
                        {
                            ActivityCommon.ResolveSgbdFile(_ediabas, ecuFileNameBest);
                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            bool readVinOk = false;
                            foreach (string vinJob in ReadVinJobs)
                            {
                                try
                                {
                                    if (_ediabasJobAbort)
                                    {
                                        break;
                                    }
                                    _ediabas.ExecuteJob(vinJob);
                                    readVinOk = true;
                                    break;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                            if (!readVinOk)
                            {
                                throw new Exception("Read VIN failed");
                            }

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                int dictIndex = 0;
                                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                                {
                                    if (dictIndex == 0)
                                    {
                                        dictIndex++;
                                        continue;
                                    }
                                    Int64 ecuAdr = -1;
                                    string ecuVin = string.Empty;
                                    // ReSharper disable once InlineOutVariableDeclaration
                                    EdiabasNet.ResultData resultData;
                                    if (resultDict.TryGetValue("ID_SG_ADR", out resultData))
                                    {
                                        if (resultData.OpData is Int64)
                                        {
                                            ecuAdr = (Int64)resultData.OpData;
                                        }
                                    }
                                    if (resultDict.TryGetValue("FG_NR", out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            ecuVin = (string)resultData.OpData;
                                        }
                                    }
                                    if (resultDict.TryGetValue("FG_NR_KURZ", out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            ecuVin = (string)resultData.OpData;
                                        }
                                    }
                                    if (resultDict.TryGetValue("AIF_FG_NR", out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            ecuVin = (string)resultData.OpData;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(ecuVin) && DetectVehicleBmw.VinRegex.IsMatch(ecuVin))
                                    {
                                        foreach (EcuInfo ecuInfo in _ecuList)
                                        {
                                            if (ecuInfo.Address == ecuAdr)
                                            {
                                                ecuInfo.Vin = ecuVin;
                                                break;
                                            }
                                        }
                                    }
                                    dictIndex++;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    // get vin
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (!string.IsNullOrEmpty(detectedVin))
                    {
                        _instanceData.Vin = detectedVin;
                    }
                    else
                    {
                        _instanceData.Vin = GetBestVin(_ecuList);
                    }

                    if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                    {
                        _ruleEvalBmw?.SetEvalProperties(detectVehicleBmw, null);
                    }

                    ReadAllXml();
                }
                _ediabas.EdInterfaceClass.EnableTransmitCache = false;

                bool pin78ConnRequire = false;
                if (!_ediabasJobAbort && ecuListBest == null && !elmDevice)
                {
                    string detectedVinDs2 = null;
                    if (detectVehicleBmw.DetectVehicleDs2())
                    {
                        pin78ConnRequire = detectVehicleBmw.Pin78ConnectRequire;
                        detectedVinDs2 = detectVehicleBmw.Vin;
                        ecuListBest = DetectDs2Ecus(progress, detectedVinDs2, detectVehicleBmw.Series, detectVehicleBmw.Ds2GroupFiles);
                    }
                    else
                    {
                        ecuListBest = null;
                    }

                    if (ecuListBest != null)
                    {
                        _ecuList.AddRange(ecuListBest.OrderBy(x => x.Name));
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        _instanceData.VehicleType = detectVehicleBmw.Series;
                        _instanceData.CDate = null;
                        if (detectVehicleBmw.ConstructYear != null)
                        {
                            _instanceData.CDate = detectVehicleBmw.ConstructYear;
                        }

                        if (!string.IsNullOrEmpty(detectedVinDs2))
                        {
                            _instanceData.Vin = detectedVinDs2;
                        }
                        else
                        {
                            _instanceData.Vin = GetBestVin(_ecuList);
                        }

                        if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                        {
                            _ruleEvalBmw?.SetEvalProperties(detectVehicleBmw, null);
                        }

                        ReadAllXml();
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress.Dispose();
                    progress = null;
                    _activityCommon.SetLock(ActivityCommon.LockType.None);

                    _translateEnabled = true;
                    UpdateOptionsMenu();
                    UpdateDisplay();

                    if (!_ediabasJobAbort)
                    {
                        if (ecuListBest == null)
                        {
                            _instanceData.CommErrorsOccurred = true;
                            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth && _activityCommon.MtcBtService)
                            {
                                if (IsPageSelectionActive())
                                {
                                    ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_no_response);
                                }
                                else
                                {
                                    new AlertDialog.Builder(this)
                                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                        {
                                            SelectBluetoothDevice();
                                        })
                                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                        {
                                        })
                                        .SetCancelable(true)
                                        .SetMessage(Resource.String.xml_tool_no_response_adapter)
                                        .SetTitle(Resource.String.alert_title_warning)
                                        .Show();
                                }
                            }
                            else
                            {
                                if (IsPageSelectionActive())
                                {
                                    ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_no_response);
                                }
                                else
                                {
                                    bool rawMode = ActivityCommon.IsRawAdapter(_activityCommon.SelectedInterface, _instanceData.DeviceAddress);
                                    if (rawMode)
                                    {
                                        new AlertDialog.Builder(this)
                                            .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                            {
                                                SelectBluetoothDevice();
                                            })
                                            .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                            {
                                            })
                                            .SetCancelable(true)
                                            .SetMessage(Resource.String.xml_tool_no_response_raw)
                                            .SetTitle(Resource.String.alert_title_warning)
                                            .Show();
                                    }
                                    else
                                    {
                                        AlertDialog alertDialog = new AlertDialog.Builder(this)
                                            .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                            {
                                                SelectConfigTypeRequest();
                                            })
                                            .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                            {
                                            })
                                            .SetCancelable(true)
                                            .SetMessage(Resource.String.xml_tool_no_response_manual)
                                            .SetTitle(Resource.String.alert_title_warning)
                                            .Show();
                                        if (alertDialog != null)
                                        {
                                            TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                                            if (messageView != null)
                                            {
                                                messageView.MovementMethod = new LinkMovementMethod();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (pin78ConnRequire)
                            {
                                if (!IsPageSelectionActive())
                                {
                                    ShowAlert(Resource.String.alert_title_warning, Resource.String.xml_tool_msg_pin78);
                                }
                            }
                            else if (bestInvalidCount > 0)
                            {
                                _instanceData.CommErrorsOccurred = true;
                                if (!IsPageSelectionActive())
                                {
                                    ShowAlert(Resource.String.alert_title_warning, Resource.String.xml_tool_msg_ecu_error);
                                }
                            }
                        }
                    }
                });
            });
            _jobThread.Start();
        }

        private List<EcuInfo> DetectDs2Ecus(CustomProgressDialog progress, string vin, string vehicleType, string groupFiles)
        {
            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Get DS2 ecu list");

            try
            {
                List<EcuInfo> ecuList = new List<EcuInfo>();

                if (!string.IsNullOrEmpty(groupFiles))
                {
                    if (!string.IsNullOrEmpty(vin))
                    {
                        _instanceData.Vin = vin;
                        ReadAllXml(true);
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ECUs found for VIN: {0}", _ecuList.Count);
                        bool readEcus = true;
                        if (_ecuList.Count > 0)
                        {
                            readEcus = false;
                            Semaphore waitSem = new Semaphore(0, 1);
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                                builder.SetMessage(Resource.String.xml_tool_read_ecu_again);
                                builder.SetTitle(Resource.String.alert_title_question);
                                builder.SetPositiveButton(Resource.String.button_yes, (s, e) =>
                                {
                                    readEcus = true;
                                });
                                builder.SetNegativeButton(Resource.String.button_no, (s, e) =>
                                {
                                    readEcus = false;
                                });
                                AlertDialog alertDialog = builder.Show();
                                if (alertDialog != null)
                                {
                                    alertDialog.DismissEvent += (sender, args) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        waitSem.Release();
                                    };
                                }
                            });
                            waitSem.WaitOne();
                        }
                        if (!readEcus)
                        {
                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Keep existing ECU list");
                            ecuList = new List<EcuInfo>(_ecuList);
                            ClearEcuList();
                            foreach (EcuInfo ecuInfo in ecuList)
                            {
                                try
                                {
                                    ActivityCommon.ResolveSgbdFile(_ediabas, ecuInfo.Sgbd);

                                    _ediabas.ArgString = string.Empty;
                                    _ediabas.ArgBinaryStd = null;
                                    _ediabas.ResultsRequests = string.Empty;
                                    _ediabas.NoInitForVJobs = true;
                                    _ediabas.ExecuteJob("_VERSIONINFO");

                                    string title = null;
                                    if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                                    {
                                        string ecuSgbdName = ecuInfo.Sgbd ?? string.Empty;
                                        EcuFunctionStructs.EcuVariant ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(ecuSgbdName);
                                        if (ecuVariant == null)
                                        {
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No ECU variant found for: {0}", ecuSgbdName);
                                        }
                                        else
                                        {
                                            title = ecuVariant.Title?.GetTitle(ActivityCommon.GetCurrentLanguage());
                                        }
                                    }

                                    if (string.IsNullOrEmpty(title))
                                    {
                                        ecuInfo.Description = GetEcuComment(_ediabas.ResultSets);
                                    }
                                    else
                                    {
                                        ecuInfo.PageName = title;
                                        ecuInfo.Description = title;
                                        ecuInfo.DescriptionTransRequired = false;
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                            return ecuList;
                        }
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Read ECU list from vehicle");
                        ClearEcuList();
                    }

                    string groupFilesUse = groupFiles;
                    if (ActivityCommon.ScanAllEcus)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Scan all ECUs requested, ignoring detected groups");
                        groupFilesUse = DetectVehicleBmw.AllDs2GroupFiles;
                    }

                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group files: {0}", groupFilesUse);
                    string[] groupArray = groupFilesUse.Split(',');
                    List<string> groupList;
                    VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(vehicleType, null, _ediabas);
                    if (vehicleSeriesInfo != null)
                    {
                        groupList = new List<string>();
                        foreach (string group in groupArray)
                        {
                            VehicleStructsBmw.VehicleEcuInfo ecuInfo = VehicleInfoBmw.GetEcuInfoByGroupName(vehicleSeriesInfo, group);
                            if (ecuInfo != null)
                            {
                                groupList.Add(group);
                            }
                        }
                    }
                    else
                    {
                        groupList = groupArray.ToList();
                    }

                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Final group list: {0}", string.Join(",", groupList.ToArray()));

                    int index = 0;
                    foreach (string ecuGroup in groupList)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detect ECU: {0}", ecuGroup);
                        string ecuName = string.Empty;
                        string ecuDesc = string.Empty;
                        if (_ediabasJobAbort)
                        {
                            break;
                        }

                        try
                        {
                            int localIndex = index;
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                if (progress != null)
                                {
                                    progress.Progress = 100 * localIndex / groupList.Count;
                                }
                            });

                            ActivityCommon.ResolveSgbdFile(_ediabas, ecuGroup);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.NoInitForVJobs = true;
                            _ediabas.ExecuteJob("_VERSIONINFO");

                            ecuDesc = GetEcuComment(_ediabas.ResultSets);

                            _ediabas.ExecuteJob("IDENT");

                            ecuName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName) ?? string.Empty;
                        }
                        catch (Exception)
                        {
                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No ECU response");
                            // ignored
                        }

                        EcuInfo ecuInfo = null;
                        if (!string.IsNullOrEmpty(ecuName))
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ECU name: {0}", ecuName);
                            bool ecuPresent = ecuList.Any(ecuInfoTemp => string.Compare(ecuInfoTemp.Sgbd, ecuName, StringComparison.OrdinalIgnoreCase) == 0);
                            if (!ecuPresent)
                            {
                                ecuInfo = new EcuInfo(ecuName.ToUpperInvariant(), 0, ecuDesc, ecuName, ecuGroup);
                                ecuList.Add(ecuInfo);
                            }
                        }

                        if (ecuInfo != null)
                        {
                            try
                            {
                                _ediabas.ArgString = string.Empty;
                                _ediabas.ArgBinaryStd = null;
                                _ediabas.ResultsRequests = string.Empty;
                                _ediabas.ExecuteJob("AIF_LESEN");

                                List<Dictionary<string, EdiabasNet.ResultData>> resultSets =_ediabas.ResultSets;
                                if (resultSets != null && resultSets.Count >= 2)
                                {
                                    int dictIndex = 0;
                                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                                    {
                                        if (dictIndex == 0)
                                        {
                                            dictIndex++;
                                            continue;
                                        }
                                        string ecuVin = string.Empty;
                                        if (resultDict.TryGetValue("AIF_FG_NR", out EdiabasNet.ResultData resultData))
                                        {
                                            if (resultData.OpData is string)
                                            {
                                                ecuVin = (string)resultData.OpData;
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VIN found: {0}", ecuVin);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(ecuVin) && DetectVehicleBmw.VinRegex.IsMatch(ecuVin))
                                        {
                                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "VIN valid");
                                            ecuInfo.Vin = ecuVin;
                                            break;
                                        }
                                        dictIndex++;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        index++;
                    }
                }
                if (ecuList.Count == 0)
                {
                    return null;
                }
                return ecuList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool IsMwTabEmpty(string mwTabFileName)
        {
            return string.Compare(mwTabFileName, EmptyMwTab, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private string GetBestVin(List<EcuInfo> ecuList)
        {
            var vinInfo = ecuList.GroupBy(x => x.Vin)
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .OrderByDescending(x => x.Count())
                .FirstOrDefault();
            return vinInfo != null ? vinInfo.Key : string.Empty;
        }

        private bool ReadVagMotInfo(CustomProgressDialog progress = null)
        {
            try
            {
                if (!ActivityCommon.VagUdsActive)
                {
                    return true;
                }
                if (_ecuInfoMot != null)
                {
                    return true;
                }

                // for UDS read VIN from motor
                ActivityCommon.ResolveSgbdFile(_ediabas, "mot_01");

                _ediabas.ArgString = string.Empty;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.ExecuteJob("_JOBS");    // force to load file

                string ecuName = _ediabas.SgbdFileName;
                EcuInfo ecuInfoMot = new EcuInfo(ecuName.ToUpperInvariant(), 1, string.Empty, ecuName, string.Empty);
                if (!GetVagEcuDetailInfo(ecuInfoMot, progress))
                {
                    return false;
                }
                _ecuInfoMot = ecuInfoMot;
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private bool ReadVagDidInfo(CustomProgressDialog progress = null)
        {
            try
            {
                if (!ActivityCommon.VagUdsActive)
                {
                    return true;
                }
                if (_ecuInfoDid != null)
                {
                    return true;
                }

                // for UDS read hw info from did
                ActivityCommon.ResolveSgbdFile(_ediabas, "did_19");

                _ediabas.ArgString = string.Empty;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.ExecuteJob("_JOBS");    // force to load file

                string ecuName = _ediabas.SgbdFileName;
                EcuInfo ecuInfoDid = new EcuInfo(ecuName.ToUpperInvariant(), 19, string.Empty, ecuName, string.Empty);
                if (!GetVagEcuDetailInfo(ecuInfoDid, progress))
                {
                    return false;
                }
                _ecuInfoDid = ecuInfoDid;
                return true;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private bool ReadVagEcuInfo(EcuInfo ecuInfo, CustomProgressDialog progress = null)
        {
            try
            {
                if (!ActivityCommon.VagUdsActive)
                {
                    return true;
                }

                if (!GetVagEcuDetailInfo(ecuInfo, progress))
                {
                    return false;
                }

                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader();
                if (udsReader == null)
                {
                    return false;
                }
                string vagDirLang = Path.Combine(_vagDir, udsReader.LanguageDir);
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Resolving VAG part number: {0}, HW part number: {1}",
                    ecuInfo.VagPartNumber ?? string.Empty, ecuInfo.VagHwPartNumber ?? string.Empty);
                UdsFileReader.DataReader.FileNameResolver dataResolver = new UdsFileReader.DataReader.FileNameResolver(udsReader.DataReader, ecuInfo.VagPartNumber, ecuInfo.VagHwPartNumber, (int)ecuInfo.Address);
                string dataFileName = dataResolver.GetFileName(vagDirLang);
                ecuInfo.VagDataFileName = dataFileName ?? string.Empty;
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VAG data file: {0}", ecuInfo.VagDataFileName);

                if (IsUdsEcu(ecuInfo))
                {
                    if (_ecuInfoMot == null || _ecuInfoDid == null)
                    {
                        return false;
                    }
                    UdsFileReader.UdsReader.FileNameResolver udsResolver = new UdsFileReader.UdsReader.FileNameResolver(udsReader,
                        _ecuInfoMot.Vin, ecuInfo.VagAsamData, ecuInfo.VagAsamRev, ecuInfo.VagPartNumber, _ecuInfoDid.VagHwPartNumber);
                    string udsFileName = udsResolver.GetFileName(vagDirLang);
                    ecuInfo.VagUdsFileName = udsFileName ?? string.Empty;
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VAG uds file: {0}", ecuInfo.VagUdsFileName);
                    List<string> udsFileList = UdsFileReader.UdsReader.FileNameResolver.GetAllFiles(ecuInfo.VagUdsFileName);
                    if (udsFileList != null)
                    {
                        foreach (string fileName in udsFileList)
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VAG include file: {0}", fileName);
                        }
                    }

                    if (ecuInfo.SubSystems != null)
                    {
                        foreach (EcuInfoSubSys subSystem in ecuInfo.SubSystems)
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Resolving sub sys: {0}, VAG part number: {1}",
                                subSystem.SubSysIndex, subSystem.VagPartNumber ?? string.Empty);
                            UdsFileReader.DataReader.FileNameResolver dataResolverSubSys =
                                new UdsFileReader.DataReader.FileNameResolver(udsReader.DataReader, ecuInfo.VagPartNumber, ecuInfo.VagHwPartNumber,
                                    subSystem.VagPartNumber, (int)ecuInfo.Address, subSystem.SubSysIndex);
                            string dataFileNameSubSys = dataResolverSubSys.GetFileName(vagDirLang);
                            subSystem.VagDataFileName = dataFileNameSubSys ?? string.Empty;
                            string name = string.Empty;
                            if (!string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                            {
                                UdsFileReader.UdsReader.ParseInfoSlv.SlaveInfo slaveInfo = udsReader.GetSlvInfo(ecuInfo.VagUdsFileName, subSystem.SubSysAddr);
                                if (slaveInfo != null)
                                {
                                    name = slaveInfo.Name;
                                }
                            }
                            subSystem.Name = name ?? string.Empty;
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Sub sys: {0}, data file: {1}, name: {2}", subSystem.SubSysIndex, subSystem.VagDataFileName, subSystem.Name);
                        }
                    }
                }

                ecuInfo.MwTabFileName = string.Empty;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ExecuteAnalyzeJobVag(int searchStartIndex)
        {
            List<ActivityCommon.VagEcuEntry> ecuVagList = ActivityCommon.ReadVagEcuList(_ecuDir);
            if ((ecuVagList == null) || (ecuVagList.Count == 0))
            {
                ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_read_ecu_info_failed);
                return;
            }

            EdiabasOpen();
            ClearVehicleInfo();
            _instanceData.EcuSearchAbortIndex = -1;

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            progress.Indeterminate = false;
            progress.Progress = 0;
            progress.Max = 100;
            progress.AbortClick = sender => 
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _ediabasJobAbort = true;
                progress.Indeterminate = true;
                progress.ButtonAbort.Enabled = false;
                progress.SetMessage(GetString(Resource.String.xml_tool_aborting));
            };
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockType.ScreenDim);

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                Dictionary<int, string> ecuNameDict = ActivityCommon.GetVagEcuNamesDict(_ecuDir);
                int maxEcuAddress = 0xFF;
                int ecuCount = ecuVagList.Count;
                int index = 0;
                int detectCount = 0;
                if (searchStartIndex >= 0)
                {
                    detectCount = _ecuList.Count;
                }
                foreach (ActivityCommon.VagEcuEntry ecuEntry in ecuVagList)
                {
                    if (_ediabasJobAbort)
                    {
                        _instanceData.EcuSearchAbortIndex = (index > 0) ? index - 1 : 0;
                        break;
                    }
#if false
                    if (index > 3)
                    {
                        break;
                    }
#endif
                    if (ecuEntry.Address != MotorAddrVag && searchStartIndex >= 0 && index < searchStartIndex)
                    {
                        index++;
                        continue;
                    }
                    if (ecuEntry.Address > maxEcuAddress)
                    {
                        index++;
                        continue;
                    }
                    int localIndex = index;
                    int localDetectCount = detectCount;
                    int localEcuCount = ecuCount;
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (!_ediabasJobAbort && progress != null)
                        {
                            progress.Progress = 100 * localIndex / localEcuCount;
                            progress.SetMessage(string.Format(GetString(Resource.String.xml_tool_search_ecus), localDetectCount, localIndex));
                        }
                    }
                    );
                    try
                    {
                        string sgbdFileNameOverride = null;
                        EcuInfo thisEcuInfo = null;
                        try
                        {
                            ActivityCommon.ResolveSgbdFile(_ediabas, ecuEntry.SysName);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob("_JOBS");    // force to load file

                            string jobName = JobReadEcuVersion;
                            if (!_ediabas.IsJobExisting(jobName))
                            {
                                jobName = JobReadEcuVersion2;
                            }
                            _ediabas.ExecuteJob(jobName);
                        }
                        catch (Exception)
                        {
                            if (ecuEntry.Address == MotorAddrVag)
                            {
                                throw;  // should always work with ISOTP
                            }

                            sgbdFileNameOverride = string.Format(CultureInfo.InvariantCulture, VagUdsCommonSgbd + "#0x{0:X02}", ecuEntry.Address);
                            ActivityCommon.ResolveSgbdFile(_ediabas, sgbdFileNameOverride);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob("_JOBS");    // force to load file

                            string jobName = JobReadEcuVersion;
                            if (!_ediabas.IsJobExisting(jobName))
                            {
                                jobName = JobReadEcuVersion2;
                            }
                            _ediabas.ExecuteJob(jobName);
                        }
                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[0];
                            if (resultDict.TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    //string result = (string)resultData.OpData;
                                    //if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        string ecuName = sgbdFileNameOverride;
                                        if (string.IsNullOrEmpty(ecuName))
                                        {
                                            ecuName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName) ?? string.Empty;
                                        }
                                        thisEcuInfo = _ecuList.FirstOrDefault(ecuInfo => string.Compare(ecuInfo.Sgbd, ecuName, StringComparison.OrdinalIgnoreCase) == 0);
                                        if ((searchStartIndex < 0) || (thisEcuInfo == null))
                                        {
                                            detectCount++;
                                        }
                                        if (thisEcuInfo == null)
                                        {
                                            if ((ecuNameDict == null) || !ecuNameDict.TryGetValue(ecuEntry.Address, out string displayName))
                                            {
                                                displayName = ecuName;
                                            }
                                            thisEcuInfo = new EcuInfo(ecuName.ToUpperInvariant(), ecuEntry.Address, string.Empty, ecuName, string.Empty)
                                            {
                                                PageName = displayName,
                                                EcuName = displayName
                                            };
                                            _ecuList.Add(thisEcuInfo);
                                        }
                                    }
                                }
                            }
                        }

                        if (thisEcuInfo != null)
                        {
                            bool udsEcu = IsUdsEcu(thisEcuInfo);
                            if (ActivityCommon.VagUdsActive && udsEcu)
                            {
                                bool resolveSgbd = false;
                                if (_ecuInfoMot == null)
                                {
                                    resolveSgbd = true;
                                    if (!ReadVagMotInfo())
                                    {
                                        throw new Exception("Read mot info failed");
                                    }
                                }
                                if (_ecuInfoDid == null)
                                {
                                    resolveSgbd = true;
                                    if (!ReadVagDidInfo())
                                    {
                                        throw new Exception("Read did info failed");
                                    }
                                }
                                if (resolveSgbd)
                                {
                                    ActivityCommon.ResolveSgbdFile(_ediabas, thisEcuInfo.Sgbd);
                                }
                                if (!ReadVagEcuInfo(thisEcuInfo))
                                {
                                    throw new Exception("Read ecu info failed");
                                }
                            }
                            if (ActivityCommon.CollectDebugInfo)
                            {
                                // get more ecu infos
                                string readCommand = GetReadCommand(thisEcuInfo);
                                foreach (Tuple<string, string> job in EcuInfoVagJobs)
                                {
                                    try
                                    {
                                        if (_ediabas.IsJobExisting(job.Item1))
                                        {
                                            string jobArgs = job.Item2;
                                            if (!string.IsNullOrEmpty(readCommand))
                                            {
                                                if (string.Compare(job.Item1, JobReadMwBlock, StringComparison.OrdinalIgnoreCase) == 0 && jobArgs.EndsWith(";"))
                                                {
                                                    jobArgs += readCommand;
                                                }
                                            }
                                            _ediabas.ArgString = jobArgs;
                                            _ediabas.ArgBinaryStd = null;
                                            _ediabas.ResultsRequests = string.Empty;
                                            _ediabas.ExecuteJob(job.Item1);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }

                                if (udsEcu)
                                {
                                    foreach (byte[] sendData in EcuInfoVagUdsRaw)
                                    {
                                        try
                                        {
                                            _ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendData.Length, "RawData");
                                            _ediabas.EdInterfaceClass.TransmitData(sendData, out byte[] _);
                                        }
                                        catch (Exception)
                                        {
                                            // ignored
                                        }
                                    }
                                }
                            }
                        }
                        if (ecuEntry.Address == MotorAddrVag)
                        {   // motor ECU, check communication interface
                            string sgbdFileNameUpper = _ediabas.SgbdFileName.ToUpperInvariant();
                            if (sgbdFileNameUpper.Contains("2000") || sgbdFileNameUpper.Contains("1281"))
                            {   // bit 7 is parity bit
                                maxEcuAddress = 0x7F;
                                int addressTemp = maxEcuAddress;    // prevent warning
                                ecuCount = ecuVagList.Count(x => x.Address <= addressTemp);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (ecuEntry.Address == MotorAddrVag)
                        {   // motor must be present, abort
                            break;
                        }
                    }
                    index++;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress.Dispose();
                    progress = null;
                    _activityCommon.SetLock(ActivityCommon.LockType.None);

                    UpdateOptionsMenu();
                    UpdateDisplay();

                    if (!_ediabasJobAbort && ((_ecuList.Count == 0) || (detectCount == 0)))
                    {
                        _instanceData.CommErrorsOccurred = true;
                        ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_no_response);
                    }
                });
            });
            _jobThread.Start();
        }

        private void PerformJobsRead(EcuInfo ecuInfo)
        {
            if (IsJobRunning())
            {
                return;
            }
            _instanceData.AutoStart = false;
            if (string.IsNullOrEmpty(_instanceData.DeviceAddress))
            {
                if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, _appDataDir, (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        // ReSharper disable once RedundantJumpStatement
                        return;
                    }
                    // no auto start
                }))
                {
                    return;
                }
            }
            if (_activityCommon.ShowConnectWarning(action =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                switch (action)
                {
                    case ActivityCommon.SsidWarnAction.Continue:
                        PerformJobsRead(ecuInfo);
                        break;

                    case ActivityCommon.SsidWarnAction.EditIp:
                        AdapterIpConfig();
                        break;
                }
            }))
            {
                return;
            }
            ExecuteJobsRead(ecuInfo);
        }

        private void ExecuteJobsRead(EcuInfo ecuInfo)
        {
            if (ecuInfo == null)
            {
                return;
            }

            if (ecuInfo.JobList != null)
            {
                SelectJobs(ecuInfo);
                return;
            }

            EdiabasOpen();
            _translateEnabled = false;

            UpdateDisplay();

            bool mwTabNotPresent = false;
            if (!ActivityCommon.VagUdsActive)
            {
                mwTabNotPresent = string.IsNullOrEmpty(ecuInfo.MwTabFileName) || (ecuInfo.MwTabEcuDict == null) ||
                                (!IsMwTabEmpty(ecuInfo.MwTabFileName) && !File.Exists(ecuInfo.MwTabFileName));
            }

            CustomProgressDialog progress = new CustomProgressDialog(this)
            {
                Progress = 0,
                Max = 100
            };
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            if ((ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw) &&
                mwTabNotPresent)
            {
                progress.Indeterminate = false;
            }
            progress.AbortClick = sender => 
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _ediabasJobAbort = true;
                progress.Indeterminate = true;
                progress.ButtonAbort.Enabled = false;
                progress.SetMessage(GetString(Resource.String.xml_tool_aborting));
            };
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockType.ScreenDim);

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                bool readFailed = false;
                try
                {
                    EcuFunctionStructs.EcuVariant ecuVariant = null;
                    if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                    {
                        if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                        {
                            string ecuSgbdName = ecuInfo.Sgbd ?? string.Empty;
                            ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(ecuSgbdName);
                            if (ecuVariant == null)
                            {
                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No ECU variant found for: {0}", ecuSgbdName);
                            }
                        }
                    }
                    else
                    {
                        bool udsEcu = IsUdsEcu(ecuInfo);
                        if (ActivityCommon.VagUdsActive && udsEcu)
                        {
                            if (!ReadVagMotInfo(progress))
                            {
                                throw new Exception("Read mot info failed");
                            }
                            if (!ReadVagDidInfo(progress))
                            {
                                throw new Exception("Read did info failed");
                            }
                        }
                    }

                    ActivityCommon.ResolveSgbdFile(_ediabas, ecuInfo.Sgbd);

                    _ediabas.ArgString = "ALL";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.NoInitForVJobs = true;
                    _ediabas.ExecuteJob("_JOBS");

                    List<XmlToolEcuActivity.JobInfo> jobList = new List<XmlToolEcuActivity.JobInfo>();

                    if (ecuVariant != null)
                    {
                        string language = ActivityCommon.GetCurrentLanguage();
                        List<Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>> fixedFuncStructList = ActivityCommon.EcuFunctionReader.GetFixedFuncStructList(ecuVariant);
                        foreach (var ecuFixedFuncStructPair in fixedFuncStructList)
                        {
                            EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct = ecuFixedFuncStructPair.Item1;
                            EcuFunctionStructs.EcuFuncStruct ecuFuncStruct = ecuFixedFuncStructPair.Item2;
                            EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType nodeClassType = ecuFixedFuncStruct.GetNodeClassType();
                            switch (nodeClassType)
                            {
                                case EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.Identification:
                                case EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ReadState:
                                case EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator:
                                {
                                    XmlToolEcuActivity.JobInfo jobInfo = new XmlToolEcuActivity.JobInfo(ecuFixedFuncStruct.Id);
                                    string displayName = ecuFixedFuncStruct.Title?.GetTitle(language);
                                    if (!string.IsNullOrWhiteSpace(displayName))
                                    {
                                        jobInfo.DisplayName = displayName;
                                    }

                                    jobInfo.Comments = new List<string>();
                                    jobInfo.EcuFixedFuncStruct = ecuFixedFuncStruct;
                                    jobInfo.EcuFuncStruct = ecuFuncStruct;

                                    foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                                    {
                                        foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJob.EcuJobResultList)
                                        {
                                            if (ecuJobResult.EcuFuncRelevant.ConvertToInt() > 0)
                                            {
                                                string resultTitle = ecuJobResult.Title?.GetTitle(language) ?? string.Empty;
                                                string resultName = ecuJobResult.Name;
                                                string resultType = DataTypeStringReal;
                                                string comment = resultName + " (" + ecuJob.Name + ")";
                                                List<string> resultCommentList = new List<string> { comment };
                                                XmlToolEcuActivity.ResultInfo resultInfo =
                                                    new XmlToolEcuActivity.ResultInfo(resultName, resultTitle, resultType, null, resultCommentList);
                                                resultInfo.CommentsTransRequired = false;
                                                resultInfo.EcuJob = ecuJob;
                                                resultInfo.EcuJobResult = ecuJobResult;
                                                jobInfo.Results.Add(resultInfo);
                                                jobInfo.Comments.Add(resultTitle);
                                            }
                                        }
                                    }

                                    jobInfo.CommentsTransRequired = false;
                                    jobList.Add(jobInfo);
                                    break;
                                }

                                default:
#if DEBUG
                                    Log.Info(Tag, string.Format("ExecuteJobsRead: Unknown node class={0}, name={1}",
                                        ecuFixedFuncStruct.NodeClassName, ecuFixedFuncStruct.Title?.GetTitle(language)));
#endif
                                    break;
                            }
                        }
                    }

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        int dictIndex = 0;
                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                        {
                            if (dictIndex == 0)
                            {
                                dictIndex++;
                                continue;
                            }
                            if (resultDict.TryGetValue("JOBNAME", out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    jobList.Add(new XmlToolEcuActivity.JobInfo((string)resultData.OpData));
                                }
                            }
                            dictIndex++;
                        }
                    }

                    foreach (XmlToolEcuActivity.JobInfo job in jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_JOBCOMMENTS");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            for (int i = 0; ; i++)
                            {
                                if (resultDict.TryGetValue("JOBCOMMENT" + i.ToString(Culture), out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        job.Comments.Add((string)resultData.OpData);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    foreach (XmlToolEcuActivity.JobInfo job in jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("_ARGUMENTS");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            int dictIndex = 0;
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                            {
                                if (dictIndex == 0)
                                {
                                    dictIndex++;
                                    continue;
                                }
                                uint argCount = 0;
                                if (resultDict.TryGetValue("ARG", out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        argCount++;
                                    }
                                }
                                job.ArgCount = argCount;
                                dictIndex++;
                            }
                        }
                    }

                    if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                    {
                        if (ActivityCommon.VagUdsActive)
                        {
                            if (!ReadVagEcuInfo(ecuInfo, progress))
                            {
                                throw new Exception("Read ecu info failed");
                            }
                        }
                        else
                        {
                            ecuInfo.VagDataFileName = string.Empty;
                            ecuInfo.VagUdsFileName = string.Empty;

                            if (mwTabNotPresent)
                            {
                                List<string> mwTabFileNames = GetBestMatchingMwTab(ecuInfo, progress);
                                if (mwTabFileNames == null)
                                {
                                    throw new Exception("Read mwtab jobs failed");
                                }
                                if (mwTabFileNames.Count == 0)
                                {
                                    ecuInfo.MwTabFileName = EmptyMwTab;
                                }
                                else if (mwTabFileNames.Count == 1)
                                {
                                    ecuInfo.MwTabFileName = mwTabFileNames[0];
                                }
                                else
                                {
                                    RunOnUiThread(() =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        SelectMwTabFromListInfo(mwTabFileNames, name =>
                                        {
                                            if (_activityCommon == null)
                                            {
                                                return;
                                            }
                                            if (string.IsNullOrEmpty(name))
                                            {
                                                ReadJobThreadDone(ecuInfo, progress, true);
                                            }
                                            else
                                            {
                                                ecuInfo.MwTabFileName = name;
                                                ecuInfo.MwTabList = ActivityCommon.ReadVagMwTab(ecuInfo.MwTabFileName);
                                                ecuInfo.ReadCommand = GetReadCommand(ecuInfo);
                                                _jobThread = new Thread(() =>
                                                {
                                                    readFailed = false;
                                                    try
                                                    {
                                                        JobsReadThreadPart2(ecuInfo, jobList);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        readFailed = true;
                                                    }
                                                    RunOnUiThread(() =>
                                                    {
                                                        if (_activityCommon == null)
                                                        {
                                                            return;
                                                        }
                                                        ReadJobThreadDone(ecuInfo, progress, readFailed);
                                                    });
                                                });
                                                _jobThread.Start();
                                            }
                                        });
                                    });
                                    return;
                                }
                            }
                        }
                        ecuInfo.MwTabList = (!string.IsNullOrEmpty(ecuInfo.MwTabFileName) && !IsMwTabEmpty(ecuInfo.MwTabFileName)) ?
                            ActivityCommon.ReadVagMwTab(ecuInfo.MwTabFileName) : null;
                    }
                    ecuInfo.ReadCommand = GetReadCommand(ecuInfo);

                    JobsReadThreadPart2(ecuInfo, jobList);
                }
                catch (Exception)
                {
                    readFailed = true;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    ReadJobThreadDone(ecuInfo, progress, readFailed);
                });
            });
            _jobThread.Start();
        }

        private void JobsReadThreadPart2(EcuInfo ecuInfo, List<XmlToolEcuActivity.JobInfo> jobList)
        {
            List<UdsFileReader.DataReader.DataInfo> measDataList = null;
            List<UdsFileReader.UdsReader.ParseInfoBase> mwbSegmentList = null;
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && ActivityCommon.VagUdsActive)
            {
                if (IsUdsEcu(ecuInfo))
                {
                    List<string> udsFileList = UdsFileReader.UdsReader.FileNameResolver.GetAllFiles(ecuInfo.VagUdsFileName);
                    if (udsFileList != null)
                    {
                        UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(ecuInfo.VagUdsFileName);
                        if (udsReader != null)
                        {
                            mwbSegmentList = udsReader.ExtractFileSegment(udsFileList, UdsFileReader.UdsReader.SegmentType.Mwb);
                            if (!string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                            {
                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Found {0} MWB segments for: {1}", mwbSegmentList?.Count, ecuInfo.VagUdsFileName);
                            }
                        }
                    }
                }
                else
                {
                    UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(ecuInfo.VagDataFileName);
                    if (udsReader != null)
                    {
                        measDataList = udsReader.DataReader.ExtractDataType(ecuInfo.VagDataFileName, UdsFileReader.DataReader.DataType.Measurement);
                        if (!string.IsNullOrEmpty(ecuInfo.VagDataFileName))
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Found {0} measurement entries for: {1}", measDataList?.Count, ecuInfo.VagDataFileName);
                        }
                    }
                }
            }
            foreach (XmlToolEcuActivity.JobInfo job in jobList)
            {
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    if (XmlToolEcuActivity.IsVagReadJob(job, ecuInfo))
                    {
                        if (mwbSegmentList != null)
                        {
                            foreach (UdsFileReader.UdsReader.ParseInfoBase parseInfo in mwbSegmentList)
                            {
                                if (parseInfo is UdsFileReader.UdsReader.ParseInfoMwb parseInfoMwb)
                                {
                                    string valueName = parseInfoMwb.Name ?? string.Empty;
                                    StringBuilder sbDispText = new StringBuilder();
                                    if (!string.IsNullOrEmpty(parseInfoMwb.DataIdString))
                                    {
                                        sbDispText.Append(parseInfoMwb.DataIdString);
                                    }
                                    if (!string.IsNullOrEmpty(parseInfoMwb.Name))
                                    {
                                        if (sbDispText.Length > 0)
                                        {
                                            sbDispText.Append(" ");
                                        }
                                        sbDispText.Append(parseInfoMwb.Name);
                                    }
                                    string displayText = sbDispText.ToString();

                                    StringBuilder sbComment = new StringBuilder();
                                    sbComment.Append(string.Format(Culture, "{0:00000}", parseInfoMwb.ServiceId));
                                    if (parseInfoMwb.DataTypeEntry.NameDetail != null)
                                    {
                                        sbComment.Append(" ");
                                        sbComment.Append(parseInfoMwb.DataTypeEntry.NameDetail);
                                    }
                                    List<string> commentList = new List<string> {sbComment.ToString()};

                                    string name = parseInfoMwb.UniqueIdString;
                                    string type = parseInfoMwb.DataTypeEntry.HasDataValue() ? DataTypeStringReal : DataTypeString;
                                    ActivityCommon.MwTabEntry mwTabEntry =
                                        new ActivityCommon.MwTabEntry((int) parseInfoMwb.ServiceId, null, valueName, string.Empty, string.Empty, string.Empty, null, null);
                                    job.Results.Add(new XmlToolEcuActivity.ResultInfo(name, displayText, type, null, commentList, mwTabEntry)
                                        { NameOld = parseInfoMwb.UniqueIdStringOld });
                                }
                            }
                            continue;
                        }

                        if (measDataList != null)
                        {
                            string heading = string.Empty;
                            int lastGroup = -1;
                            foreach (UdsFileReader.DataReader.DataInfo dataInfo in measDataList)
                            {
                                if (!dataInfo.Value1.HasValue || !dataInfo.Value2.HasValue)
                                {
                                    continue;
                                }

                                if (lastGroup != dataInfo.Value1.Value)
                                {
                                    lastGroup = dataInfo.Value1.Value;
                                    heading = string.Empty;
                                }
                                if (dataInfo.Value2.Value == 0)
                                {
                                    heading = dataInfo.TextArray.Length > 0 ? dataInfo.TextArray[0] : string.Empty;
                                    continue;
                                }
                                string valueName = dataInfo.TextArray.Length > 0 ? dataInfo.TextArray[0] : string.Empty;
                                if (!string.IsNullOrEmpty(heading))
                                {
                                    valueName = heading + ": " + valueName;
                                }

                                List<string> commentList = dataInfo.TextArray.ToList();
                                if (commentList.Count > 0)
                                {
                                    commentList.RemoveAt(0);
                                }

                                string name = string.Format(Culture, "{0}/{1}", dataInfo.Value1.Value, dataInfo.Value2.Value);
                                string displayText = string.Format(Culture, "{0:000}/{1} {2}", dataInfo.Value1.Value, dataInfo.Value2.Value, valueName);
                                string type = DataTypeReal;
                                ActivityCommon.MwTabEntry mwTabEntry =
                                    new ActivityCommon.MwTabEntry(dataInfo.Value1.Value, dataInfo.Value2.Value, valueName, string.Empty, string.Empty, string.Empty, null, null);
                                job.Results.Add(new XmlToolEcuActivity.ResultInfo(name, displayText, type, null, commentList, mwTabEntry));
                            }
                            continue;
                        }

                        if (ecuInfo.MwTabList != null)
                        {
                            job.Comments = new List<string> { GetString(Resource.String.xml_tool_job_read_mwblock) };
                            foreach (ActivityCommon.MwTabEntry mwTabEntry in ecuInfo.MwTabList)
                            {
                                if (ecuInfo.MwTabEcuDict != null)
                                {
                                    long key = (mwTabEntry.BlockNumber << 16) + mwTabEntry.ValueIndexTrans;
                                    if (!ecuInfo.MwTabEcuDict.ContainsKey(key))
                                    {
                                        continue;
                                    }
                                }
                                string name;
                                string displayText;
                                if (mwTabEntry.ValueIndex.HasValue)
                                {
                                    name = string.Format(Culture, "{0}/{1}", mwTabEntry.BlockNumber, mwTabEntry.ValueIndexTrans);
                                    displayText = string.Format(Culture, "{0:000}/{1} {2}", mwTabEntry.BlockNumber, mwTabEntry.ValueIndexTrans, mwTabEntry.Description);
                                }
                                else
                                {
                                    name = string.Format(Culture, "{0}", mwTabEntry.BlockNumber);
                                    displayText = string.Format(Culture, "{0:000} {1}", mwTabEntry.BlockNumber, mwTabEntry.Description);
                                }
                                string comment = string.Empty;
                                if (mwTabEntry.ValueMin != null && mwTabEntry.ValueMax != null)
                                {
                                    comment = string.Format(Culture, "{0} - {1}", mwTabEntry.ValueMin, mwTabEntry.ValueMax);
                                }
                                else if (mwTabEntry.ValueMin != null)
                                {
                                    comment = string.Format(Culture, "> {0}", mwTabEntry.ValueMin);
                                }
                                else if (mwTabEntry.ValueMax != null)
                                {
                                    comment = string.Format(Culture, "< {0}", mwTabEntry.ValueMax);
                                }
                                if (!string.IsNullOrEmpty(mwTabEntry.ValueUnit))
                                {
                                    comment += string.Format(Culture, " [{0}]", mwTabEntry.ValueUnit);
                                }
                                List<string> commentList = new List<string>();
                                if (!string.IsNullOrEmpty(comment))
                                {
                                    commentList.Add(comment);
                                }
                                if (!string.IsNullOrEmpty(mwTabEntry.Comment))
                                {
                                    commentList.Add(mwTabEntry.Comment);
                                }

                                string type = (string.Compare(mwTabEntry.ValueType, "R", StringComparison.OrdinalIgnoreCase) == 0) ? DataTypeReal : DataTypeInteger;
                                job.Results.Add(new XmlToolEcuActivity.ResultInfo(name, displayText, type, null, commentList, mwTabEntry));
                            }
                        }
                        // fill up with virtual entries
                        if (ecuInfo.MwTabEcuDict != null)
                        {
                            foreach (long key in ecuInfo.MwTabEcuDict.Keys)
                            {
                                EcuMwTabEntry ecuMwTabEntry = ecuInfo.MwTabEcuDict[key];
                                int block = ecuMwTabEntry.BlockNumber;
                                int index = ecuMwTabEntry.ValueIndex;
                                bool entryFound = false;

                                bool udsJob = string.Compare(job.Name, JobReadS22Uds, StringComparison.OrdinalIgnoreCase) == 0;
                                foreach (XmlToolEcuActivity.ResultInfo resultInfo in job.Results)
                                {
                                    if (udsJob)
                                    {
                                        if (resultInfo.MwTabEntry.BlockNumber == block)
                                        {
                                            entryFound = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (resultInfo.MwTabEntry.BlockNumber == block &&
                                            resultInfo.MwTabEntry.ValueIndex == index)
                                        {
                                            entryFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (!entryFound)
                                {
                                    int? indexStore;
                                    string name;
                                    string displayText;
                                    string type;
                                    if (!udsJob)
                                    {
                                        indexStore = index;
                                        name = string.Format(Culture, "{0}/{1}", block, index);
                                        displayText = string.Format(Culture, "{0:000}/{1}", block, index);
                                        type = ecuMwTabEntry.ValueUnit;
                                    }
                                    else
                                    {
                                        indexStore = null;
                                        name = string.Format(Culture, "{0}", block);
                                        displayText = string.Format(Culture, "{0:000}", block);
                                        type = DataTypeBinary;
                                    }
                                    ActivityCommon.MwTabEntry mwTabEntry =
                                        new ActivityCommon.MwTabEntry(block, indexStore, string.Empty, string.Empty, string.Empty, string.Empty, null, null, true);
                                    job.Results.Add(new XmlToolEcuActivity.ResultInfo(name, displayText, type, null, null, mwTabEntry));
                                }
                            }
                        }
                    }
                    else if (string.Compare(job.Name, JobReadVin, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        job.Comments = new List<string> { GetString(Resource.String.xml_tool_job_read_vin) };
                        job.Results.Add(new XmlToolEcuActivity.ResultInfo("Fahrgestellnr", GetString(Resource.String.xml_tool_result_vin), DataTypeString, null, null));
                    }
                    continue;
                }

                bool bmwStatJob = XmlToolEcuActivity.IsBmwReadStatusTypeJob(job);
                if (bmwStatJob)
                {
                    AddSgFunctionResults(job);
                    continue;
                }

                _ediabas.ArgString = job.Name;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.NoInitForVJobs = true;
                _ediabas.ExecuteJob("_RESULTS");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                if (resultSets != null && resultSets.Count >= 2)
                {
                    int dictIndex = 0;
                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                    {
                        if (dictIndex == 0)
                        {
                            dictIndex++;
                            continue;
                        }
                        // ReSharper disable once InlineOutVariableDeclaration
                        EdiabasNet.ResultData resultData;
                        string result = string.Empty;
                        string resultType = string.Empty;
                        List<string> resultCommentList = new List<string>();
                        if (resultDict.TryGetValue("RESULT", out resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                result = (string) resultData.OpData;
                            }
                        }
                        if (resultDict.TryGetValue("RESULTTYPE", out resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                resultType = (string) resultData.OpData;
                            }
                        }
                        for (int i = 0;; i++)
                        {
                            if (resultDict.TryGetValue("RESULTCOMMENT" + i.ToString(Culture), out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    resultCommentList.Add((string) resultData.OpData);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        job.Results.Add(new XmlToolEcuActivity.ResultInfo(result, result, resultType, null, resultCommentList));
                        dictIndex++;
                    }
                }
            }

            ecuInfo.JobList = jobList;

            if (ecuInfo.IgnoreXmlFile)
            {
                if (string.IsNullOrWhiteSpace(ecuInfo.PageName))
                {
                    ecuInfo.PageName = ecuInfo.EcuName;
                }
            }
            else
            {
                string xmlFileDir = XmlFileDir();
                if (xmlFileDir != null)
                {
                    string xmlFile = Path.Combine(xmlFileDir, ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension));
                    if (File.Exists(xmlFile))
                    {
                        try
                        {
                            ReadPageXml(ecuInfo, XDocument.Load(xmlFile));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
        }

        private void AddSgFunctionResults(XmlToolEcuActivity.JobInfo job)
        {
            if (_sgFunctions == null)
            {
                return;
            }

            bool statMwBlock = XmlToolEcuActivity.IsBmwReadStatusMwBlockJob(job);
            List<SgFunctions.SgFuncInfo> sgFuncInfoList = statMwBlock ? _sgFunctions.ReadMwTabTable() : _sgFunctions.ReadSgFuncTable();
            if (sgFuncInfoList == null)
            {
                return;
            }

            int serviceId = SgFunctions.GetJobService(job.Name);
            if (serviceId < 0)
            {
                return;
            }

            int groupId = 0;
            foreach (SgFunctions.SgFuncInfo funcInfo in sgFuncInfoList)
            {
                if (statMwBlock)
                {
                    if (funcInfo.ServiceList != null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (funcInfo.ServiceList == null || !funcInfo.ServiceList.Contains(serviceId))
                    {
                        continue;
                    }
                }

                string arg = funcInfo.Arg;
                string id = funcInfo.Id;
                string displayBaseName = string.Empty;
                if (!statMwBlock)
                {
                    displayBaseName = arg;
                    if (!string.IsNullOrEmpty(id))
                    {
                        displayBaseName += " (" + id + ")";
                    }
                }

                if (funcInfo.ResInfoList != null)
                {
                    List<SgFunctions.SgFuncNameInfo> resultList = new List<SgFunctions.SgFuncNameInfo>();
                    foreach (SgFunctions.SgFuncNameInfo funcNameInfo in funcInfo.ResInfoList)
                    {
                        if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                        {
                            if (funcBitFieldInfo.TableDataType == SgFunctions.TableDataType.Bit &&
                                funcBitFieldInfo.NameInfoList != null)
                            {
                                foreach (SgFunctions.SgFuncNameInfo nameInfo in funcBitFieldInfo.NameInfoList)
                                {
                                    if (nameInfo is SgFunctions.SgFuncBitFieldInfo nameInfoBitField)
                                    {
                                        if (!string.IsNullOrEmpty(nameInfoBitField.ResultName))
                                        {
                                            resultList.Add(nameInfoBitField);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(funcBitFieldInfo.ResultName))
                                {
                                    resultList.Add(funcNameInfo);
                                }
                            }
                        }
                    }

                    int groupSize = 0;
                    foreach (SgFunctions.SgFuncNameInfo funcNameInfo in resultList)
                    {
                        if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                        {
                            if (!string.IsNullOrEmpty(funcBitFieldInfo.ResultName))
                            {
                                string displayName = string.Empty;
                                if (!string.IsNullOrEmpty(displayBaseName))
                                {
                                    displayName = displayBaseName + "/";
                                }
                                displayName += funcBitFieldInfo.ResultName;

                                List<string> commentList = new List<string>();
                                if (!string.IsNullOrEmpty(funcInfo.Info))
                                {
                                    commentList.Add(funcInfo.Info);
                                }

                                if (!string.IsNullOrEmpty(funcBitFieldInfo.Info))
                                {
                                    commentList.Add(funcBitFieldInfo.Info);
                                }

                                if (!string.IsNullOrEmpty(funcBitFieldInfo.Unit))
                                {
                                    commentList.Add("[" + funcBitFieldInfo.Unit + "]");
                                }

                                XmlToolEcuActivity.ResultInfo resultInfo = new XmlToolEcuActivity.ResultInfo(funcBitFieldInfo.ResultName, displayName, DataTypeReal, arg, commentList)
                                {
                                    GroupId = groupId
                                };

                                if (groupSize == 0)
                                {
                                    string displayNameGroup = displayBaseName;
                                    List<string> commentListGroup = new List<string>();
                                    if (!string.IsNullOrEmpty(funcInfo.Info))
                                    {
                                        commentListGroup.Add(funcInfo.Info);
                                    }

                                    XmlToolEcuActivity.ResultInfo resultInfoGroup = new XmlToolEcuActivity.ResultInfo(arg, displayNameGroup, string.Empty, arg, commentListGroup)
                                    {
                                        GroupId = groupId,
                                        GroupVisible = true
                                    };
                                    job.Results.Add(resultInfoGroup);
                                }

                                job.Results.Add(resultInfo);
                                groupSize++;
                            }
                        }
                    }

                    if (groupSize > 0)
                    {
                        groupId++;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(funcInfo.Result))
                    {
                        string displayName = string.Empty;
                        if (!string.IsNullOrEmpty(displayBaseName))
                        {
                            displayName = displayBaseName + "/";
                        }
                        displayName += funcInfo.Result;

                        List<string> commentList = new List<string>();
                        if (!string.IsNullOrEmpty(funcInfo.Info))
                        {
                            commentList.Add(funcInfo.Info);
                        }

                        if (!string.IsNullOrEmpty(funcInfo.Info))
                        {
                            commentList.Add(funcInfo.Info);
                        }

                        if (!string.IsNullOrEmpty(funcInfo.Unit))
                        {
                            commentList.Add("[" + funcInfo.Unit + "]");
                        }

                        XmlToolEcuActivity.ResultInfo resultInfo = new XmlToolEcuActivity.ResultInfo(funcInfo.Result, displayName, DataTypeReal, arg, commentList);
                        job.Results.Add(resultInfo);
                    }
                }
            }
        }

        private void ReadJobThreadDone(EcuInfo ecuInfo, CustomProgressDialog progress, bool readFailed)
        {
            progress.Dismiss();
            progress.Dispose();
            _activityCommon.SetLock(ActivityCommon.LockType.None);

            UpdateOptionsMenu();
            UpdateDisplay();
            if (_ediabasJobAbort)
            {
                return;
            }
            if (readFailed || (ecuInfo.JobList?.Count == 0))
            {
                ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_read_jobs_failed);
            }
            else
            {
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    bool tableNotFound = false;
                    if (ActivityCommon.VagUdsActive)
                    {
                        if (IsUdsEcu(ecuInfo))
                        {
                            if (string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                            {
                                tableNotFound = true;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ecuInfo.VagDataFileName))
                            {
                                tableNotFound = true;
                            }
                        }
                    }
                    else
                    {
                        if ((ecuInfo.MwTabList == null) || (ecuInfo.MwTabList.Count == 0))
                        {
                            tableNotFound = true;
                        }
                    }

                    if (tableNotFound)
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(this)
                            .SetMessage(Resource.String.xml_tool_no_mwtab)
                            .SetTitle(Resource.String.alert_title_info)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) =>
                            {
                                TranslateAndSelectJobs(ecuInfo);
                            });
                        AlertDialog alertDialog = builder.Show();
                        if (alertDialog != null)
                        {
                            alertDialog.DismissEvent += DialogDismissEvent;
                        }
                        return;
                    }
                }
                // wait for thread to finish
                if (IsJobRunning())
                {
                    _jobThread.Join();
                }
                TranslateAndSelectJobs(ecuInfo);
            }
        }

        private void TranslateAndSelectJobs(EcuInfo ecuInfo)
        {
            _ecuListTranslated = false;
            _translateEnabled = true;
            if (!TranslateEcuText((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                SelectJobs(ecuInfo);
            }))
            {
                SelectJobs(ecuInfo);
            }
        }

        public static bool IsUdsEcuName(string sgdbName)
        {
            if (string.IsNullOrEmpty(sgdbName))
            {
                return false;
            }

            return sgdbName.Contains("7000", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUdsEcu(EcuInfo ecuInfo)
        {
            return IsUdsEcuName(ecuInfo.Sgbd);
        }

        public static bool Is1281EcuName(string sgdbName)
        {
            if (string.IsNullOrEmpty(sgdbName))
            {
                return false;
            }

            return sgdbName.Contains("1281", StringComparison.OrdinalIgnoreCase);
        }

        public static bool Is1281Ecu(EcuInfo ecuInfo)
        {
            return Is1281EcuName(ecuInfo.Sgbd);
        }

        public static string GetReadCommand(EcuInfo ecuInfo)
        {
            if (IsUdsEcu(ecuInfo))
            {
                return string.Empty;
            }
            return Is1281Ecu(ecuInfo) ? "WertEinmalLesen" : "LESEN";
        }

        private void SelectMwTabFromListInfo(List<string> fileNames, MwTabFileSelected handler)
        {
            bool handlerCalled = false;
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(Resource.String.xml_tool_sel_mwtab_info);
            builder.SetTitle(Resource.String.alert_title_info);
            builder.SetPositiveButton(Resource.String.button_ok, (s, e) =>
            {
                handlerCalled = true;
                SelectMwTabFromList(fileNames, handler);
            });
            builder.SetNegativeButton(Resource.String.button_abort, (s, e) =>
            {
                handlerCalled = true;
                handler(null);
            });
            AlertDialog alertDialog = builder.Show();
            if (alertDialog != null)
            {
                alertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (!handlerCalled)
                    {
                        handler(null);
                    }
                };
            }
        }

        private void SelectMwTabFromList(List<string> fileNames, MwTabFileSelected handler)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.xml_tool_select_ecu_type);
            ListView listView = new ListView(this);
            bool handlerCalled = false;

            List<string> displayNames = fileNames.Select(Path.GetFileNameWithoutExtension).ToList();
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemSingleChoice, displayNames);
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            listView.SetItemChecked(0, true);
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                string fileName = fileNames[listView.CheckedItemPosition];
                handlerCalled = true;
                handler(fileName);
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
                handlerCalled = true;
                handler(null);
            });
            AlertDialog alertDialog = builder.Show();
            if (alertDialog != null)
            {
                alertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (!handlerCalled)
                    {
                        handler(null);
                    }
                };
            }
        }

        private List<string> GetBestMatchingMwTab(EcuInfo ecuInfo, CustomProgressDialog progress)
        {
            string readCommand = GetReadCommand(ecuInfo);
            if (string.IsNullOrEmpty(readCommand))
            {
                return GetBestMatchingMwTabUds(ecuInfo, progress);
            }

            List<ActivityCommon.MwTabFileEntry> wmTabList = ActivityCommon.GetMatchingVagMwTabs(Path.Combine(_datUkdDir, "mwtabs"), ecuInfo.Sgbd);
#if false
            SortedSet<long> mwBlocks = ActivityCommon.ExtractVagMwBlocks(wmTabList);
#else
            SortedSet<int> mwBlocks = new SortedSet<int>();
            for (int i = 0; i < 0x100; i++)
            {
                mwBlocks.Add(i);
            }
#endif
#if false
            {
                StringBuilder sr = new StringBuilder();
                sr.Append(string.Format("-s \"{0}\"", ecuInfo.Sgbd));
                foreach (int block in mwBlocks)
                {
                    sr.Append(string.Format(" -j \"" + XmlToolActivity.JobReadMwBlock + "#{0};{1}\"", block, readCommand));
                }
                Log.Debug("MwTab", sr.ToString());
            }
#endif
            Dictionary <long, EcuMwTabEntry> mwTabEcuDict = new Dictionary<long, EcuMwTabEntry>();
            int valueCount = 0;
            ecuInfo.MwTabEcuDict = null;

            try
            {
                int errorCount = 0;
                int blockIndex = 0;
                foreach (int block in mwBlocks)
                {
                    if (_ediabasJobAbort)
                    {
                        return null;
                    }
                    int localBlockIndex = blockIndex;
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Progress = 100 * localBlockIndex / mwBlocks.Count;
                        }
                    });
                    for (int retry = 0; retry < 2; retry++)
                    {
                        try
                        {
                            _ediabas.ArgString = string.Format("{0};{1}", block, readCommand);
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(JobReadMwBlock);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                int dictIndex = 0;
                                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                                {
                                    if (dictIndex == 0)
                                    {
                                        dictIndex++;
                                        continue;
                                    }
                                    string valueType = string.Empty;
                                    // ReSharper disable once InlineOutVariableDeclaration
                                    EdiabasNet.ResultData resultData;
                                    if (resultDict.TryGetValue("MW_WERT", out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            // ReSharper disable once TryCastAlwaysSucceeds
                                            string value = resultData.OpData as string;
                                            if (!string.IsNullOrWhiteSpace(value))
                                            {
                                                valueType = DataTypeString;
                                            }
                                        }
                                        else if (resultData.OpData is double)
                                        {
                                            valueType = DataTypeReal;
                                        }
                                        else if (resultData.OpData is long)
                                        {
                                            valueType = DataTypeInteger;
                                        }
                                        else if (resultData.OpData is byte[])
                                        {
                                            valueType = DataTypeBinary;
                                        }
                                        if (!string.IsNullOrWhiteSpace(valueType))
                                        {
                                            valueCount++;
                                        }
                                    }
                                    string unitText = string.Empty;
                                    if (resultDict.TryGetValue("MWEINH_TEXT", out resultData))
                                    {
                                        unitText = resultData.OpData as string ?? string.Empty;
                                    }
                                    if (!string.IsNullOrWhiteSpace(valueType) || !string.IsNullOrWhiteSpace(unitText))
                                    {
                                        long key = (block << 16) + dictIndex;
                                        if (!mwTabEcuDict.ContainsKey(key))
                                        {
                                            mwTabEcuDict.Add(key, new EcuMwTabEntry(block, dictIndex, valueType, unitText));
                                        }
                                    }
                                    dictIndex++;
                                }
                            }
                            break;
                        }
                        catch (Exception)
                        {
                            errorCount++;
                            if (errorCount > 10)
                            {
                                return null;
                            }
                        }
                    }
                    blockIndex++;
                }
            }
            catch (Exception)
            {
                return null;
            }
            ecuInfo.MwTabEcuDict = mwTabEcuDict;
            if (valueCount == 0)
            {
                return new List<string>();
            }

            foreach (ActivityCommon.MwTabFileEntry mwTabFileEntry in wmTabList)
            {
                int compareCount = 0;
                int matchCount = 0;
                int existCount = 0;
                //Log.Debug("Match", "File: " + mwTabFileEntry.FileName);
                foreach (ActivityCommon.MwTabEntry mwTabEntry in mwTabFileEntry.MwTabList)
                {
                    if (!mwTabEntry.ValueIndex.HasValue)
                    {
                        continue;
                    }
                    long key = (mwTabEntry.BlockNumber << 16) + mwTabEntry.ValueIndex.Value;
                    if (mwTabEcuDict.TryGetValue(key, out EcuMwTabEntry ecuMwTabEntry))
                    {
                        existCount++;
                        if (string.IsNullOrEmpty(ecuMwTabEntry.ValueUnit))
                        {
                            continue;
                        }
                        compareCount++;
                        // remove all spaces
                        string valueUnit = mwTabEntry.ValueUnit;
                        string unitText = Regex.Replace(ecuMwTabEntry.ValueUnit, @"\s+", "");
                        valueUnit = Regex.Replace(valueUnit, @"\s+", "");

                        if (unitText.ToLowerInvariant().StartsWith("mon"))
                        {
                            unitText = "m/s²";
                        }
                        if (valueUnit.ToLowerInvariant().StartsWith("mon"))
                        {
                            valueUnit = "m/s²";
                        }
                        unitText = Regex.Replace(unitText, @"²", "2");
                        valueUnit = Regex.Replace(valueUnit, @"²", "2");

                        string valueUnitLower = valueUnit.ToLowerInvariant();
                        string unitTextLower = unitText.ToLowerInvariant();
                        if (valueUnitLower.Contains(unitTextLower) || unitTextLower.Contains(valueUnitLower))
                        {
                            //Log.Debug("Match", "Match: '" + unitText + "' '" + valueUnit + "'");
                            matchCount++;
                        }
                        else
                        {
                            //Log.Debug("Match", "Mismatch: '" + unitText + "' '" + valueUnit + "'");
                        }
                    }
                }
                mwTabFileEntry.ExistCount = existCount;
                mwTabFileEntry.CompareCount = compareCount;
                mwTabFileEntry.MatchCount = matchCount;
                if (compareCount > 0)
                {
                    mwTabFileEntry.MatchRatio = matchCount * ActivityCommon.MwTabFileEntry.MaxMatchRatio / compareCount;
                }
                else
                {
                    mwTabFileEntry.MatchRatio = (mwTabFileEntry.MwTabList.Count == 0) ? ActivityCommon.MwTabFileEntry.MaxMatchRatio : 0;
                }
            }
            List<ActivityCommon.MwTabFileEntry> wmTabListSorted = wmTabList.Where(x => x.ExistCount > 0).OrderByDescending(x => x).ToList();
            if (wmTabListSorted.Count == 0)
            {
                return new List<string>();
            }
            return wmTabListSorted.
                TakeWhile(mwTabFileEntry => mwTabFileEntry.MatchRatio == wmTabListSorted[0].MatchRatio /*&& mwTabFileEntry.MatchCount >= wmTabListSorted[0].MatchCount / 10*/).
                Select(mwTabFileEntry => mwTabFileEntry.FileName).ToList();
        }

        private List<string> GetBestMatchingMwTabUds(EcuInfo ecuInfo, CustomProgressDialog progress)
        {
#if false
            List<ActivityCommon.MwTabFileEntry> wmTabList = ActivityCommon.GetMatchingVagMwTabsUds(Path.Combine(_datUkdDir, "mwtabs"), ecuInfo.Address);
            SortedSet<long> mwIds = ActivityCommon.ExtractVagMwBlocks(wmTabList);
#else
            SortedSet<int> mwIds = new SortedSet<int>();
            for (int i = 0x1000; i <= 0x16FF; i++)
            {
                mwIds.Add(i);
            }
            for (int i = 0xF400; i <= 0xF4FF; i++)
            {
                mwIds.Add(i);
            }
#endif

            Dictionary<long, EcuMwTabEntry> mwTabEcuDict = new Dictionary<long, EcuMwTabEntry>();
            int valueCount = 0;
            ecuInfo.MwTabEcuDict = null;

            try
            {
                int errorCount = 0;
                int idIndex = 0;
                foreach (int id in mwIds)
                {
                    if (_ediabasJobAbort)
                    {
                        return null;
                    }
                    int localIdIndex = idIndex;
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Progress = 100 * localIdIndex / mwIds.Count;
                        }
                    });
                    for (int retry = 0; retry < 2; retry++)
                    {
                        try
                        {
                            _ediabas.ArgString = string.Format(CultureInfo.InvariantCulture, "{0}", id);
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(JobReadS22Uds);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict0 = resultSets[0];
                                // ReSharper disable once InlineOutVariableDeclaration
                                EdiabasNet.ResultData resultData;
                                bool resultOk = false;
                                if (resultDict0.TryGetValue("JOBSTATUS", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        string result = (string) resultData.OpData;
                                        if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            resultOk = true;
                                        }
                                    }
                                }
                                if (!resultOk)
                                {
                                    break;
                                }

                                Dictionary<string, EdiabasNet.ResultData> resultDict1 = resultSets[1];
                                string valueType = string.Empty;
                                if (resultDict1.TryGetValue("ERGEBNIS1WERT", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        // ReSharper disable once TryCastAlwaysSucceeds
                                        string value = resultData.OpData as string;
                                        if (!string.IsNullOrWhiteSpace(value))
                                        {
                                            valueType = DataTypeString;
                                        }
                                    }
                                    else if (resultData.OpData is double)
                                    {
                                        valueType = DataTypeReal;
                                    }
                                    else if (resultData.OpData is long)
                                    {
                                        valueType = DataTypeInteger;
                                    }
                                    else if (resultData.OpData is byte[])
                                    {
                                        valueType = DataTypeBinary;
                                    }
                                    if (!string.IsNullOrWhiteSpace(valueType))
                                    {
                                        valueCount++;
                                    }
                                }
                                long key = (id << 16) + 0;
                                if (!mwTabEcuDict.ContainsKey(key))
                                {
                                    mwTabEcuDict.Add(key, new EcuMwTabEntry(id, 0, valueType, string.Empty));
                                }
                            }
                            break;
                        }
                        catch (Exception)
                        {
                            errorCount++;
                            if (errorCount > 10)
                            {
                                return null;
                            }
                        }
                    }
                    idIndex++;
                }
            }
            catch (Exception)
            {
                return null;
            }
            ecuInfo.MwTabEcuDict = mwTabEcuDict;
            if (valueCount == 0)
            {
                return new List<string>();
            }
#if false
            foreach (ActivityCommon.MwTabFileEntry mwTabFileEntry in wmTabList)
            {
                int compareCount = 0;
                int matchCount = 0;
                //Log.Debug("Match", "File: " + mwTabFileEntry.FileName);
                foreach (ActivityCommon.MwTabEntry mwTabEntry in mwTabFileEntry.MwTabList)
                {
                    compareCount++;
                    long key = (mwTabEntry.BlockNumber << 16) + 0;
                    EcuMwTabEntry ecuMwTabEntry;
                    if (mwTabEcuDict.TryGetValue(key, out ecuMwTabEntry))
                    {
                        //Log.Debug("Match", "Match: {0}", mwTabEntry.BlockNumber);
                        matchCount++;
                    }
                }
                mwTabFileEntry.ExistCount = 1;
                mwTabFileEntry.CompareCount = compareCount;
                mwTabFileEntry.MatchCount = matchCount;
                if (compareCount > 0)
                {
                    mwTabFileEntry.MatchRatio = matchCount * ActivityCommon.MwTabFileEntry.MaxMatchRatio / compareCount;
                }
                else
                {
                    mwTabFileEntry.MatchRatio = (mwTabFileEntry.MwTabList.Count == 0) ? ActivityCommon.MwTabFileEntry.MaxMatchRatio : 0;
                }
            }
            List<ActivityCommon.MwTabFileEntry> wmTabListSorted = wmTabList.Where(x => x.ExistCount > 0).OrderByDescending(x => x).ToList();
            if (wmTabListSorted.Count == 0)
            {
                return new List<string>();
            }
#if false
            return wmTabListSorted.
                TakeWhile(mwTabFileEntry => mwTabFileEntry.MatchRatio == wmTabListSorted[0].MatchRatio /*&& mwTabFileEntry.MatchCount >= wmTabListSorted[0].MatchCount / 10*/).
                Select(mwTabFileEntry => mwTabFileEntry.FileName).ToList();
#else
            return wmTabListSorted.Select(mwTabFileEntry => mwTabFileEntry.FileName).ToList();
#endif
#else
            return new List<string>();
#endif
        }

        private bool GetVagEcuDetailInfo(EcuInfo ecuInfo, CustomProgressDialog progress)
        {
            try
            {
                string readCommand = GetReadCommand(ecuInfo);
                if (string.IsNullOrEmpty(readCommand))
                {
                    return GetVagEcuDetailInfoUds(ecuInfo, progress);
                }

                ecuInfo.InitReadValues();

                int maxIndex = 6;
                for (int index = 0; index <= maxIndex; index++)
                {
                    int indexLocal = index;
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Progress = 100 * indexLocal / maxIndex;
                        }
                    });

                    string jobName = null;
                    string jobArgs = string.Empty;
                    string resultName = null;
                    EcuInfo.CodingRequestType? codingRequestType = null;
                    switch (index)
                    {
                        case 0:
                            jobName = JobReadEcuVersion2;
                            break;

                        case 1:
                            if (!string.IsNullOrEmpty(ecuInfo.VagPartNumber))
                            {
                                break;
                            }
                            jobName = JobReadEcuVersion;
                            resultName = "GERAETENUMMER";
                            break;

                        case 2:
                            jobName = JobReadSupportedFunc;
                            break;

                        case 3:
                            jobName = JobReadVin;
                            resultName = "FAHRGESTELLNR";
                            break;

                        case 4:
                            jobName = JobReadS22Uds;
                            jobArgs = "0x0600";
                            resultName = "ERGEBNIS1WERT";
                            codingRequestType = EcuInfo.CodingRequestType.LongUds;
                            break;

                        case 5:
                            if (ecuInfo.VagCodingLong != null)
                            {
                                break;
                            }
                            jobName = JobReadLongCoding;
                            resultName = "CODIERUNGWERTBINAER";
                            codingRequestType = EcuInfo.CodingRequestType.ReadLong;
                            break;

                        case 6:
                            if (ecuInfo.VagCodingLong != null)
                            {
                                break;
                            }
                            jobName = JobReadCoding;
                            resultName = "CODIERUNGWERTBINAER";
                            codingRequestType = EcuInfo.CodingRequestType.CodingS22;
                            break;
                    }
                    if (string.IsNullOrEmpty(jobName) || !_ediabas.IsJobExisting(jobName))
                    {
                        continue;
                    }
                    try
                    {
                        _ediabas.ArgString = jobArgs;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(jobName);

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[0];
                            bool resultOk = false;
                            if (resultDict.TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    string result = (string)resultData.OpData;
                                    if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        resultOk = true;
                                    }
                                }
                            }
                            if (resultOk)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict1 = resultSets[1];
                                switch (index)
                                {
                                    case 0:
                                        EvalResponseJobReadEcuVersion2(ecuInfo, resultDict1);
                                        break;

                                    case 1:
                                        EvalResponseJobReadEcuVersion(ecuInfo, resultDict1);
                                        break;

                                    case 2:
                                        EvalResponseJobReadSupportedFunc(ecuInfo, resultSets);
                                        break;

                                    case 3:
                                        if (resultDict1.TryGetValue(resultName, out resultData))
                                        {
                                            if (resultData.OpData is string text)
                                            {
                                                string vin = text.TrimEnd();
                                                if (DetectVehicleBmw.VinRegex.IsMatch(vin))
                                                {
                                                    ecuInfo.Vin = vin;
                                                }
                                            }
                                        }
                                        break;

                                    case 4:
                                    case 5:
                                    case 6:
                                        if (resultDict1.TryGetValue(resultName, out resultData))
                                        {
                                            if (resultData.OpData is byte[] coding)
                                            {
                                                ecuInfo.VagCodingLong = coding;
                                                ecuInfo.VagCodingRequestType = codingRequestType;
                                            }
                                        }
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

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Progress = 100;
                    }
                });

                if (ecuInfo.VagPartNumber == null)
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

        private bool GetVagEcuDetailInfoUds(EcuInfo ecuInfo, CustomProgressDialog progress)
        {
            try
            {
                ecuInfo.InitReadValues();

                int index = 0;
                foreach (Tuple<VagUdsS22DataType, int> udsInfo in VagUdsS22Data)
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Progress = 100 * index / VagUdsS22Data.Length;
                        }
                    });

                    string jobName = JobReadS22Uds;
                    string argString = string.Empty;
                    switch (udsInfo.Item1)
                    {
                        case VagUdsS22DataType.EcuInfo:
                            jobName = JobReadEcuVersion2;
                            break;

                        default:
                            argString = string.Format(CultureInfo.InvariantCulture, "{0}", udsInfo.Item2);
                            break;
                    }

                    bool optional = true;
                    switch (udsInfo.Item1)
                    {
                        case VagUdsS22DataType.PartNum:
                        case VagUdsS22DataType.HwPartNum:
                        case VagUdsS22DataType.AsamData:
                        case VagUdsS22DataType.AsamRev:
                            optional = false;
                            break;
                    }

                    bool binary = false;
                    switch (udsInfo.Item1)
                    {
                        case VagUdsS22DataType.ProgDate:
                        case VagUdsS22DataType.Coding:
                        case VagUdsS22DataType.SubSystems:
                            binary = true;
                            break;
                    }

                    _ediabas.ArgString = argString;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob(jobName);

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[0];
                        bool resultOk = false;
                        if (resultDict.TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                string result = (string)resultData.OpData;
                                if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    resultOk = true;
                                }
                            }
                        }
                        if (!resultOk)
                        {
                            if (optional)
                            {
                                continue;
                            }
                            return false;
                        }

                        Dictionary<string, EdiabasNet.ResultData> resultDict1 = resultSets[1];
                        if (udsInfo.Item1 == VagUdsS22DataType.EcuInfo)
                        {
                            EvalResponseJobReadEcuVersion2(ecuInfo, resultDict1);
                            continue;
                        }

                        string dataString = null;
                        byte[] dataBytes = null;
                        if (resultDict1.TryGetValue("ERGEBNIS1WERT", out resultData))
                        {
                            if (resultData.OpData is byte[] data)
                            {
                                dataBytes = data;
                                if (!binary)
                                {
                                    dataString = VagUdsEncoding.GetString(data).TrimEnd('\0', ' ');
                                }
                            }
                        }

                        if (binary)
                        {
                            if (dataBytes == null)
                            {
                                if (optional)
                                {
                                    continue;
                                }
                                return false;
                            }
                        }
                        else
                        {
                            if (dataString == null)
                            {
                                if (optional)
                                {
                                    continue;
                                }
                                return false;
                            }
                        }

                        switch (udsInfo.Item1)
                        {
                            case VagUdsS22DataType.Vin:
                                if (DetectVehicleBmw.VinRegex.IsMatch(dataString ?? string.Empty))
                                {
                                    ecuInfo.Vin = dataString;
                                }
                                break;

                            case VagUdsS22DataType.PartNum:
                                ecuInfo.VagPartNumber = dataString;
                                break;

                            case VagUdsS22DataType.HwPartNum:
                                ecuInfo.VagHwPartNumber = dataString;
                                break;

                            case VagUdsS22DataType.SysName:
                                ecuInfo.VagSysName = dataString;
                                break;

                            case VagUdsS22DataType.AsamData:
                                ecuInfo.VagAsamData = dataString;
                                break;

                            case VagUdsS22DataType.AsamRev:
                                ecuInfo.VagAsamRev = dataString;
                                break;

                            case VagUdsS22DataType.Coding:
                                ecuInfo.VagCodingRequestType = EcuInfo.CodingRequestType.LongUds;
                                ecuInfo.VagCodingLong = dataBytes;
                                break;

                            case VagUdsS22DataType.ProgDate:
                                ecuInfo.VagProgDate = dataBytes;
                                break;

                            case VagUdsS22DataType.SubSystems:
                            {
                                List<EcuInfoSubSys> subSystems = new List<EcuInfoSubSys>();
                                for (int i = 0; i < dataBytes.Length - 1; i += 2)
                                {
                                    UInt16 value = (UInt16) ((dataBytes[i] << 8) + dataBytes[i + 1]);
                                    if (value != 0xFFFF)
                                    {
                                        subSystems.Add(new EcuInfoSubSys(value));
                                    }
                                }

                                List<EcuInfoSubSys> subSystemsOrder = subSystems.OrderBy(x => x.SubSysAddr).ToList();
                                int subIndex = 0;
                                foreach (EcuInfoSubSys subSystem in subSystemsOrder)
                                {
                                    subSystem.SubSysIndex = subIndex++;
                                }
                                ecuInfo.SubSystems = subSystemsOrder;
                                break;
                            }
                        }
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Progress = 100;
                    }
                });

                if (!GetVagEcuSubSysInfoUds(ecuInfo, progress))
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

        private void EvalResponseJobReadSupportedFunc(EcuInfo ecuInfo, List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            HashSet<UInt64> supportedFuncHash = new HashSet<UInt64>();
            StringBuilder sb = new StringBuilder();
            sb.Append("Supported functions: ");
            int dictIndex = 0;
            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
            {
                if (dictIndex == 0)
                {
                    dictIndex++;
                    continue;
                }
                if (resultDict.TryGetValue("FUNKTIONSNR", out EdiabasNet.ResultData resultData))
                {
                    if (resultData.OpData is Int64 funcNr)
                    {
                        sb.Append(string.Format(Culture, "{0:X04} ", funcNr));
                        if (Enum.IsDefined(typeof(SupportedFuncType), (int)funcNr))
                        {
                            SupportedFuncType funcType = (SupportedFuncType)funcNr;
                            sb.Append(string.Format(Culture, "({0}) ", funcType));
                        }
                        supportedFuncHash.Add((UInt64) funcNr);
                    }
                }
                dictIndex++;
            }

            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, sb.ToString());
            ecuInfo.VagSupportedFuncHash = supportedFuncHash;
        }

        private void EvalResponseJobReadEcuVersion(EcuInfo ecuInfo, Dictionary<string, EdiabasNet.ResultData> resultDict)
        {
            if (resultDict.TryGetValue("GERAETENUMMER", out EdiabasNet.ResultData resultData))
            {
                if (resultData.OpData is string text)
                {
                    string swPartNumber = text.TrimEnd();
                    if (!string.IsNullOrWhiteSpace(swPartNumber))
                    {
                        ecuInfo.VagPartNumber = swPartNumber;
                    }
                }
            }

            if (resultDict.TryGetValue("GERAETECODE", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagEquipmentNumber = value;
                    }
                }
            }

            if (resultDict.TryGetValue("IMPORTEURSCODE", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagImporterNumber = value;
                    }
                }
            }

            if (resultDict.TryGetValue("WERKSTATTCODE", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagWorkshopNumber = value;
                    }
                }
            }

            if (resultDict.TryGetValue("GERAETEERKENNUNG", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    string sysName = text.TrimEnd();
                    if (!string.IsNullOrWhiteSpace(sysName))
                    {
                        ecuInfo.VagSysName = sysName;
                    }
                }
            }

            if (resultDict.TryGetValue("MAXWERTPARAMETERCODE", out resultData))
            {
                if (resultData.OpData is Int64 value)
                {
                    ecuInfo.VagCodingMax = (UInt64)value;
                }
            }

            if (resultDict.TryGetValue("GERAETECODIERUNG", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagCodingShort = value;
                        ecuInfo.VagCodingRequestType = EcuInfo.CodingRequestType.ShortV1;
                    }
                }
            }

            if (resultDict.TryGetValue("GERAETECODIERUNGTYP", out resultData))
            {
                if (resultData.OpData is Int64 value)
                {
                    ecuInfo.VagCodingTypeValue = (UInt64)value;
                }
            }
        }

        private void EvalResponseJobReadEcuVersion2(EcuInfo ecuInfo, Dictionary<string, EdiabasNet.ResultData> resultDict)
        {
            if (resultDict.TryGetValue("SWTEILENUMMER", out EdiabasNet.ResultData resultData))
            {
                if (resultData.OpData is string text)
                {
                    string swPartNumber = text.TrimEnd();
                    if (!string.IsNullOrWhiteSpace(swPartNumber))
                    {
                        ecuInfo.VagPartNumber = swPartNumber;
                    }
                }
            }

            if (resultDict.TryGetValue("HWTEILENUMMER", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    string hwPartNumber = text.TrimEnd();
                    if (!string.IsNullOrWhiteSpace(hwPartNumber))
                    {
                        ecuInfo.VagHwPartNumber = hwPartNumber;
                    }
                }
            }

            if (resultDict.TryGetValue("GERAETENUMMER", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagEquipmentNumber = value;
                    }
                }
            }

            if (resultDict.TryGetValue("IMPORTEURSNUMMER", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagImporterNumber = value;
                    }
                }
            }

            if (resultDict.TryGetValue("BETRIEBSNUMMER", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagWorkshopNumber = value;
                    }
                }
            }

            if (resultDict.TryGetValue("SYSTEMBEZEICHNUNG", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    string sysName = text.TrimEnd();
                    if (!string.IsNullOrWhiteSpace(sysName))
                    {
                        ecuInfo.VagSysName = sysName;
                    }
                }
            }

            if (resultDict.TryGetValue("MAXWERTCODIERUNG", out resultData))
            {
                if (resultData.OpData is Int64 value)
                {
                    ecuInfo.VagCodingMax = (UInt64)value;
                }
            }

            if (resultDict.TryGetValue("CODIERUNG", out resultData))
            {
                if (resultData.OpData is string text)
                {
                    if (UInt64.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        ecuInfo.VagCodingShort = value;
                        ecuInfo.VagCodingRequestType = EcuInfo.CodingRequestType.ShortV2;
                    }
                }
            }

            if (resultDict.TryGetValue("CODIERUNGTYP", out resultData))
            {
                if (resultData.OpData is Int64 value)
                {
                    ecuInfo.VagCodingTypeValue = (UInt64)value;
                }
            }
        }

        private bool GetVagEcuSubSysInfoUds(EcuInfo ecuInfo, CustomProgressDialog progress)
        {
            try
            {
                if (ecuInfo.SubSystems == null || ecuInfo.SubSystems.Count == 0)
                {
                    return true;
                }

                int maxItems = VagUdsS22SubSysData.Length * ecuInfo.SubSystems.Count;
                int index = 0;
                foreach (EcuInfoSubSys subSystem in ecuInfo.SubSystems)
                {
                    subSystem.VagCodingLong = null;
                    subSystem.VagPartNumber = null;
                    subSystem.VagSysName = null;

                    foreach (Tuple<VagUdsS22SubSysDataType, int> udsInfo in VagUdsS22SubSysData)
                    {
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            if (progress != null)
                            {
                                progress.Progress = 100 * index / maxItems;
                            }
                        });

                        try
                        {
                            _ediabas.ArgString = string.Format(CultureInfo.InvariantCulture, "{0}", udsInfo.Item2 + subSystem.SubSysAddr);
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(JobReadS22Uds);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[0];
                                bool resultOk = false;
                                if (resultDict.TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        string result = (string)resultData.OpData;
                                        if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            resultOk = true;
                                        }
                                    }
                                }
                                if (!resultOk)
                                {
                                    continue;
                                }

                                bool binary = false;
                                switch (udsInfo.Item1)
                                {
                                    case VagUdsS22SubSysDataType.Coding:
                                        binary = true;
                                        break;
                                }
                                string dataString = null;
                                byte[] dataBytes = null;
                                Dictionary<string, EdiabasNet.ResultData> resultDict1 = resultSets[1];
                                if (resultDict1.TryGetValue("ERGEBNIS1WERT", out resultData))
                                {
                                    if (resultData.OpData is byte[] data)
                                    {
                                        dataBytes = data;
                                        if (!binary)
                                        {
                                            dataString = VagUdsEncoding.GetString(data).TrimEnd('\0', ' ');
                                        }
                                    }
                                }

                                if (binary)
                                {
                                    if (dataBytes == null)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (dataString == null)
                                    {
                                        continue;
                                    }
                                }

                                switch (udsInfo.Item1)
                                {
                                    case VagUdsS22SubSysDataType.Coding:
                                        subSystem.VagCodingLong = dataBytes;
                                        break;

                                    case VagUdsS22SubSysDataType.PartNum:
                                        subSystem.VagPartNumber = dataString;
                                        break;

                                    case VagUdsS22SubSysDataType.SysName:
                                        subSystem.VagSysName = dataString;
                                        break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Progress = 100;
                    }
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ExecuteUpdateEcuInfo(UpdateEcuDelegate handler = null)
        {
            _translateEnabled = false;
            EdiabasOpen();

            UpdateDisplay();
            if ((_ecuList.Count == 0) || (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.None))
            {
                _translateEnabled = true;
                handler?.Invoke(true);
                return;
            }

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            bool readFailed = false;
            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                List<ActivityCommon.VagEcuEntry> ecuVagList = null;
                Dictionary<int, string> ecuNameDict = null;
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    ecuVagList = ActivityCommon.ReadVagEcuList(_ecuDir);
                    ecuNameDict = ActivityCommon.GetVagEcuNamesDict(_ecuDir);
                }
                for (int idx = 0; idx < _ecuList.Count; idx++)
                {
                    EcuInfo ecuInfo = _ecuList[idx];
                    if (ecuInfo.Address >= 0)
                    {
                        continue;
                    }
                    try
                    {
                        ActivityCommon.ResolveSgbdFile(_ediabas, ecuInfo.Sgbd);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_VERSIONINFO");

                        if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                        {
                            string title = null;
                            if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                            {
                                string ecuSgbdName = ecuInfo.Sgbd ?? string.Empty;
                                EcuFunctionStructs.EcuVariant ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(ecuSgbdName);
                                if (ecuVariant == null)
                                {
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No ECU variant found for: {0}", ecuSgbdName);
                                }
                                else
                                {
                                    title = ecuVariant.Title?.GetTitle(ActivityCommon.GetCurrentLanguage());
                                }
                            }

                            if (string.IsNullOrEmpty(title))
                            {
                                ecuInfo.Description = GetEcuComment(_ediabas.ResultSets);
                            }
                            else
                            {
                                ecuInfo.PageName = title;
                                ecuInfo.Description = title;
                                ecuInfo.DescriptionTransRequired = false;
                            }
                        }

                        string ecuName = null;
                        int address = -1;
                        if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                        {
                            if (ecuInfo.Sgbd.StartsWith(VagUdsCommonSgbd + "#", true, CultureInfo.InvariantCulture))
                            {
                                ecuName = ecuInfo.Sgbd;
                                string[] nameArray = ecuName.Split('#');
                                if (nameArray.Length == 2)
                                {
                                    object addressObj = new System.ComponentModel.UInt32Converter().ConvertFromInvariantString(nameArray[1]);
                                    if ((addressObj is UInt32 ecuAddress))
                                    {
                                        address = (int) ecuAddress;
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(ecuName))
                        {
                            ecuName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName) ?? string.Empty;
                        }

                        if (_ecuList.Any(info => !info.Equals(ecuInfo) && string.Compare(info.Sgbd, ecuName, StringComparison.OrdinalIgnoreCase) == 0))
                        {   // already existing
                            _ecuList.Remove(ecuInfo);
                            continue;
                        }
                        string displayName = ecuName;
                        if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && ecuVagList != null && ecuNameDict != null)
                        {
                            string compareName = ecuInfo.Sgbd;
                            bool limitString = false;
                            if ((string.Compare(ecuInfo.Sgbd, ecuName, StringComparison.OrdinalIgnoreCase) == 0) &&
                                (compareName.Length > 3))
                            {   // no group file selected
                                compareName = compareName.Substring(0, 3);
                                limitString = true;
                            }

                            if (address < 0)
                            {
                                foreach (ActivityCommon.VagEcuEntry entry in ecuVagList)
                                {
                                    string entryName = entry.SysName;
                                    if (limitString && entryName.Length > 3)
                                    {
                                        entryName = entryName.Substring(0, 3);
                                    }
                                    if (string.Compare(entryName, compareName, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        ecuNameDict.TryGetValue(entry.Address, out displayName);
                                        address = entry.Address;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                ecuNameDict.TryGetValue(address, out displayName);
                            }
                        }
                        ecuInfo.Name = ecuName.ToUpperInvariant();
                        if (string.IsNullOrEmpty(ecuInfo.PageName) && string.IsNullOrEmpty(ecuInfo.EcuName))
                        {
                            ecuInfo.PageName = displayName;
                            ecuInfo.EcuName = displayName;
                        }
                        ecuInfo.Sgbd = ecuName;
                        ecuInfo.Address = address;
                    }
                    catch (Exception)
                    {
                        readFailed = true;
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress.Dispose();
                    progress = null;

                    _ecuListTranslated = false;
                    _translateEnabled = true;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    if (readFailed)
                    {
                        _instanceData.CommErrorsOccurred = true;
                        ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_read_ecu_info_failed);
                    }
                    handler?.Invoke(readFailed);
                });
            });
            _jobThread.Start();
        }

        private string GetEcuComment(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (resultSets != null && resultSets.Count >= 2)
            {
                int dictIndex = 0;
                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    if (dictIndex == 0)
                    {
                        dictIndex++;
                        continue;
                    }
                    for (int i = 0; ; i++)
                    {
                        if (resultDict.TryGetValue("ECUCOMMENT" + i.ToString(Culture), out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                if (stringBuilder.Length > 0)
                                {
                                    stringBuilder.Append(";");
                                }
                                stringBuilder.Append((string)resultData.OpData);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    dictIndex++;
                }
            }
            return stringBuilder.ToString();
        }

        private void EcuCheckChanged(EcuInfo ecuInfo)
        {
            if (ecuInfo.Selected)
            {
                PerformJobsRead(ecuInfo);
            }
            else
            {
                int selectCount = 0;
                if (ecuInfo.JobList != null)
                {
                    selectCount = ecuInfo.JobList.Count(job => job.Selected);
                }
                if ((selectCount > 0) || !string.IsNullOrEmpty(ecuInfo.MwTabFileName))
                {
                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            ecuInfo.JobList = null;
                            ecuInfo.MwTabFileName = string.Empty;
                            ecuInfo.MwTabList = null;
                            ecuInfo.IgnoreXmlFile = true;
                            UpdateDisplay();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) => { })
                        .SetMessage(Resource.String.xml_tool_reset_ecu_setting)
                        .SetTitle(Resource.String.alert_title_question)
                        .Show();
                }
            }
            UpdateDisplay();
        }

        private void MenuOptionsSelected(EcuInfo ecuInfo, View view)
        {
            int itemIndex = _ecuListAdapter.Items.IndexOf(ecuInfo);
            if (itemIndex < 0)
            {
                return;
            }

            ShowContextMenu(view, itemIndex);
        }

        private bool SelectPageFile(string pageFileName)
        {
            try
            {
                if (IsJobRunning())
                {
                    return false;
                }

                if (string.IsNullOrEmpty(pageFileName))
                {
                    return false;
                }

                string xmlFileDir = XmlFileDir();
                if (string.IsNullOrEmpty(xmlFileDir))
                {
                    return false;
                }

                foreach (EcuInfo ecuInfo in _ecuList)
                {
                    string xmlPageFile = Path.Combine(xmlFileDir, ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension));
                    if (string.Compare(xmlPageFile, pageFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ExecuteJobsRead(ecuInfo);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private bool AbortEdiabasJob()
        {
            if (_ediabasJobAbort)
            {
                return true;
            }
            return false;
        }

        private void BroadcastReceived(Context context, Intent intent)
        {
            if (intent == null)
            {   // from usb check timer
                if (_activityActive)
                {
                    _activityCommon.RequestUsbPermission(null);
                }
                return;
            }
            string action = intent.Action;
            switch (action)
            {
                case UsbManager.ActionUsbDeviceAttached:
                    if (_activityActive)
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                        if (usbDevice != null)
                        {
                            _activityCommon.RequestUsbPermission(usbDevice);
                            UpdateOptionsMenu();
                            UpdateDisplay();
                        }
                    }
                    break;

                case UsbManager.ActionUsbDeviceDetached:
                    if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Ftdi)
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                        if (usbDevice != null && EdFtdiInterface.IsValidUsbDevice(usbDevice))
                        {
                            EdiabasClose();
                        }
                    }
                    break;
            }
        }

        private void ReadPageXml(EcuInfo ecuInfo, XDocument document)
        {
            if (document.Root == null)
            {
                return;
            }
            XNamespace ns = document.Root.GetDefaultNamespace();
            XElement pageNode = document.Root.Element(ns + "page");
            if (pageNode == null)
            {
                return;
            }

            bool noUpdate = false;
            XAttribute pageNoUpdateAttr = pageNode.Attribute("no_update");
            if (pageNoUpdateAttr != null)
            {
                try
                {
                    noUpdate = XmlConvert.ToBoolean(pageNoUpdateAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            ecuInfo.NoUpdate = noUpdate;

            XAttribute displayModeAttr = pageNode.Attribute("display-mode");
            if (displayModeAttr != null)
            {
                if (Enum.TryParse(displayModeAttr.Value, true, out JobReader.PageInfo.DisplayModeType displayMode))
                {
                    ecuInfo.DisplayMode = displayMode;
                }
            }

            XAttribute fontSizeAttr = pageNode.Attribute("fontsize");
            if (fontSizeAttr != null)
            {
                if (Enum.TryParse(fontSizeAttr.Value, true, out DisplayFontSize fontSize))
                {
                    ecuInfo.FontSize = fontSize;
                }
            }

            XAttribute gaugesPortraitAttr = pageNode.Attribute("gauges-portrait");
            if (gaugesPortraitAttr != null)
            {
                try
                {
                    ecuInfo.GaugesPortrait = XmlConvert.ToInt32(gaugesPortraitAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            XAttribute gaugesLandscapeAttr = pageNode.Attribute("gauges-landscape");
            if (gaugesLandscapeAttr != null)
            {
                try
                {
                    ecuInfo.GaugesLandscape = XmlConvert.ToInt32(gaugesLandscapeAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            bool useCompatIds = true;
            XAttribute dbNameAttr = pageNode.Attribute("db_name");
            if (dbNameAttr != null)
            {
                string dbName = Path.GetFileName(ActivityCommon.AssetFileName) ?? string.Empty;
                if (string.Compare(dbName, dbNameAttr.Value, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    useCompatIds = false;
                }
            }
            ecuInfo.UseCompatIds = useCompatIds;

            XElement stringsNode = GetDefaultStringsNode(ns, pageNode);
            XElement jobsNode = pageNode.Element(ns + "jobs");
            if (jobsNode == null)
            {
                return;
            }

            if (stringsNode != null)
            {
                string pageName = GetStringEntry(DisplayNamePage, ns, stringsNode);
                if (pageName != null)
                {
                    ecuInfo.PageName = pageName;
                }
            }

            foreach (XmlToolEcuActivity.JobInfo job in ecuInfo.JobList)
            {
                XElement jobNode = GetJobNode(ecuInfo, job, ns, jobsNode);
                if (jobNode != null)
                {
                    job.Selected = true;
                    int argLimit = -1;
                    XAttribute argLimitAttr = jobNode.Attribute("arg_limit");
                    if (argLimitAttr != null)
                    {
                        try
                        {
                            argLimit = XmlConvert.ToInt32(argLimitAttr.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    job.ArgLimit = argLimit;

                    foreach (XmlToolEcuActivity.ResultInfo result in job.Results)
                    {
                        if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                        {
                            if (result.MwTabEntry != null)
                            {
                                jobNode = GetJobNode(ecuInfo, job, ns, jobsNode, XmlToolEcuActivity.GetJobArgs(result.MwTabEntry, ecuInfo));
                                if (jobNode == null)
                                {
                                    continue;
                                }
                            }
                        }

                        XElement displayNode = GetDisplayNode(ecuInfo, result, job, ns, jobNode);
                        if (displayNode != null)
                        {
                            result.Selected = true;
                            XAttribute formatAttr = displayNode.Attribute("format");
                            if (formatAttr != null)
                            {
                                result.Format = formatAttr.Value;
                            }

                            XAttribute dispOrderAttr = displayNode.Attribute("display-order");
                            if (dispOrderAttr != null)
                            {
                                try
                                {
                                    result.DisplayOrder = XmlConvert.ToUInt32(dispOrderAttr.Value);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            XAttribute gridTypeAttr = displayNode.Attribute("grid-type");
                            if (gridTypeAttr != null)
                            {
                                if (Enum.TryParse(gridTypeAttr.Value.Replace("-", "_"), true, out JobReader.DisplayInfo.GridModeType gridType))
                                {
                                    result.GridType = gridType;
                                }
                            }

                            XAttribute minValueAttr = displayNode.Attribute("min-value");
                            if (minValueAttr != null)
                            {
                                try
                                {
                                    result.MinValue = XmlConvert.ToDouble(minValueAttr.Value);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            XAttribute maxValueAttr = displayNode.Attribute("max-value");
                            if (maxValueAttr != null)
                            {
                                try
                                {
                                    result.MaxValue = XmlConvert.ToDouble(maxValueAttr.Value);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            XAttribute logTagAttr = displayNode.Attribute("log_tag");
                            if (logTagAttr != null)
                            {
                                result.LogTag = logTagAttr.Value;
                            }
                        }
                        if (stringsNode != null)
                        {
                            string displayTag = DisplayNameJobPrefix + job.Name + "#" + result.Name;
                            string displayText = GetStringEntry(displayTag, ns, stringsNode);
                            if (displayText != null)
                            {
                                result.DisplayText = displayText;
                            }
                        }
                    }
                }
            }
        }

        private string ReadPageSgbd(XDocument document, out bool noUpdate, out JobReader.PageInfo.DisplayModeType displayMode, out DisplayFontSize fontSize,
            out int gaugesPortrait, out int gaugesLandscape,
            out string mwTabFileName, out Dictionary<long, EcuMwTabEntry> mwTabEcuDict,
            out string vagDataFileName, out string vagUdsFileName)
        {
            noUpdate = false;
            displayMode = JobReader.PageInfo.DisplayModeType.List;
            fontSize = DisplayFontSize.Small;
            gaugesPortrait = JobReader.GaugesPortraitDefault;
            gaugesLandscape = JobReader.GaugesLandscapeDefault;
            mwTabFileName = null;
            mwTabEcuDict = null;
            vagDataFileName = null;
            vagUdsFileName = null;
            if (document.Root == null)
            {
                return null;
            }
            XNamespace ns = document.Root.GetDefaultNamespace();
            XElement pageNode = document.Root.Element(ns + "page");
            if (pageNode == null)
            {
                return null;
            }

            XAttribute pageNoUpdateAttr = pageNode.Attribute("no_update");
            if (pageNoUpdateAttr != null)
            {
                try
                {
                    noUpdate = XmlConvert.ToBoolean(pageNoUpdateAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            XAttribute displayModeAttr = pageNode.Attribute("display-mode");
            if (displayModeAttr != null)
            {
                if (!Enum.TryParse(displayModeAttr.Value, true, out displayMode))
                {
                    displayMode = JobReader.PageInfo.DisplayModeType.List;
                }
            }

            XAttribute fontSizeAttr = pageNode.Attribute("fontsize");
            if (fontSizeAttr != null)
            {
                if (!Enum.TryParse(fontSizeAttr.Value, true, out fontSize))
                {
                    fontSize = DisplayFontSize.Small;
                }
            }

            XAttribute gaugesPortraitAttr = pageNode.Attribute("gauges-portrait");
            if (gaugesPortraitAttr != null)
            {
                try
                {
                    gaugesPortrait = XmlConvert.ToInt32(gaugesPortraitAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            XAttribute gaugesLandscapeAttr = pageNode.Attribute("gauges-landscape");
            if (gaugesLandscapeAttr != null)
            {
                try
                {
                    gaugesLandscape = XmlConvert.ToInt32(gaugesLandscapeAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            XElement jobsNode = pageNode.Element(ns + "jobs");
            XAttribute sgbdAttr = jobsNode?.Attribute("sgbd");
            XAttribute mwTabAttr = jobsNode?.Attribute("mwtab");
            XAttribute mwDataAttr = jobsNode?.Attribute("mwdata");
            XAttribute vagDataAttr = jobsNode?.Attribute("vag_data_file");
            XAttribute vagUdsAttr = jobsNode?.Attribute("vag_uds_file");
            if (mwTabAttr != null)
            {
                mwTabFileName = !IsMwTabEmpty(mwTabAttr.Value) ? Path.Combine(_ecuDir, mwTabAttr.Value) : mwTabAttr.Value;
            }
            if (mwDataAttr != null)
            {
                mwTabEcuDict = new Dictionary<long, EcuMwTabEntry>();
                string[] dataArray = mwDataAttr.Value.Split(';');
                foreach (string data in dataArray)
                {
                    try
                    {
                        string[] elements = data.Split(',');
                        if (elements.Length == 4)
                        {
                            int block = Convert.ToInt32(elements[0]);
                            int index = Convert.ToInt32(elements[1]);
                            string valueType = elements[2];
                            byte[] unitBytes = Convert.FromBase64String(elements[3]);
                            string valueUnit = Encoding.UTF8.GetString(unitBytes);
                            mwTabEcuDict.Add((block << 16) + index, new EcuMwTabEntry(block, index, valueType, valueUnit));
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            if (vagDataAttr != null)
            {
                vagDataFileName = Path.Combine(_vagDir, vagDataAttr.Value);
            }
            if (vagUdsAttr != null)
            {
                vagUdsFileName = Path.Combine(_vagDir, vagUdsAttr.Value);
            }
            return sgbdAttr?.Value;
        }

        private XDocument GeneratePageXml(EcuInfo ecuInfo, XDocument documentOld)
        {
            try
            {
                if (ecuInfo.JobList == null)
                {
                    return null;
                }
                XDocument document = documentOld;
                if (document?.Root == null)
                {
                    document = XDocument.Parse(string.Format(XmlDocumentFrame, "fragment"));
                }
                if (document.Root == null)
                {
                    return null;
                }
                XNamespace ns = document.Root.GetDefaultNamespace();

                XElement pageNode = document.Root.Element(ns + "page");
                if (pageNode == null)
                {
                    pageNode = new XElement(ns + "page");
                    document.Root.Add(pageNode);
                }

                XAttribute pageNoUpdateAttr = pageNode.Attribute("no_update");
                if (pageNoUpdateAttr != null)
                {
                    bool noUpdate = false;
                    try
                    {
                        noUpdate = XmlConvert.ToBoolean(pageNoUpdateAttr.Value);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (noUpdate)
                    {
                        return null;
                    }
                }

                XAttribute pageNameAttr = pageNode.Attribute("name");
                if (pageNameAttr == null)
                {
                    pageNode.Add(new XAttribute("name", DisplayNamePage));
                }

                string displayModeName = ecuInfo.DisplayMode.ToString().ToLowerInvariant();
                XAttribute displayModeAttr = pageNode.Attribute("display-mode");
                if (displayModeAttr == null)
                {
                    pageNode.Add(new XAttribute("display-mode", displayModeName));
                }
                else
                {
                    displayModeAttr.Value = displayModeName;
                }

                string fontSizeName = ecuInfo.FontSize.ToString().ToLowerInvariant();
                XAttribute pageFontSizeAttr = pageNode.Attribute("fontsize");
                if (pageFontSizeAttr == null)
                {
                    pageNode.Add(new XAttribute("fontsize", fontSizeName));
                }
                else
                {
                    pageFontSizeAttr.Value = fontSizeName;
                }

                XAttribute pageGaugesPortraitAttr = pageNode.Attribute("gauges-portrait");
                if (pageGaugesPortraitAttr == null)
                {
                    pageNode.Add(new XAttribute("gauges-portrait", ecuInfo.GaugesPortrait));
                }
                else
                {
                    pageGaugesPortraitAttr.Value = ecuInfo.GaugesPortrait.ToString(CultureInfo.InvariantCulture);
                }

                XAttribute pageGaugesLandscapeAttr = pageNode.Attribute("gauges-landscape");
                if (pageGaugesLandscapeAttr == null)
                {
                    pageNode.Add(new XAttribute("gauges-landscape", ecuInfo.GaugesLandscape));
                }
                else
                {
                    pageGaugesLandscapeAttr.Value = ecuInfo.GaugesLandscape.ToString(CultureInfo.InvariantCulture);
                }

                XAttribute pageLogFileAttr = pageNode.Attribute("logfile");
                if (pageLogFileAttr == null)
                {
                    pageNode.Add(new XAttribute("logfile", ActivityCommon.CreateValidFileName(ecuInfo.Name + ".log")));
                }

                string dbName = Path.GetFileName(ActivityCommon.AssetFileName) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    XAttribute dbNameAttr = pageNode.Attribute("db_name");
                    if (dbNameAttr == null)
                    {
                        pageNode.Add(new XAttribute("db_name", dbName));
                    }
                    else
                    {
                        dbNameAttr.Value = dbName;
                    }
                }

                XElement stringsNode = GetDefaultStringsNode(ns, pageNode);
                if (stringsNode == null)
                {
                    stringsNode = new XElement(ns + "strings");
                    pageNode.Add(stringsNode);
                }
                else
                {
                    RemoveGeneratedStringEntries(ns, stringsNode);
                }

                XElement stringNodePage = new XElement(ns + "string", ecuInfo.PageName);
                stringNodePage.Add(new XAttribute("name", DisplayNamePage));
                stringsNode.Add(stringNodePage);

                XElement jobsNodeOld = pageNode.Element(ns + "jobs");
                XElement jobsNodeNew = new XElement(ns + "jobs");
                if (jobsNodeOld != null)
                {
                    jobsNodeNew.ReplaceAttributes(from el in jobsNodeOld.Attributes() where (el.Name != "sgbd" && el.Name != "mwtab" && el.Name != "mwdata" &&
                                                                                             el.Name != "vag_data_file" && el.Name != "vag_uds_file") select new XAttribute(el));
                }

                jobsNodeNew.Add(new XAttribute("sgbd", ecuInfo.Sgbd));
                if (!string.IsNullOrEmpty(ecuInfo.MwTabFileName))
                {
                    if (!IsMwTabEmpty(ecuInfo.MwTabFileName))
                    {
                        string relativePath = ActivityCommon.MakeRelativePath(_ecuDir, ecuInfo.MwTabFileName);
                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            jobsNodeNew.Add(new XAttribute("mwtab", relativePath));
                        }
                    }
                    else
                    {
                        jobsNodeNew.Add(new XAttribute("mwtab", ecuInfo.MwTabFileName));
                    }
                    if (ecuInfo.MwTabEcuDict != null)
                    {
                        StringBuilder sr = new StringBuilder();
                        foreach (long key in ecuInfo.MwTabEcuDict.Keys)
                        {
                            try
                            {
                                EcuMwTabEntry ecuMwTabEntry = ecuInfo.MwTabEcuDict[key];
                                if (sr.Length > 0)
                                {
                                    sr.Append(";");
                                }
                                sr.Append(ecuMwTabEntry.BlockNumber);
                                sr.Append(",");
                                sr.Append(ecuMwTabEntry.ValueIndex);
                                sr.Append(",");
                                sr.Append(ecuMwTabEntry.ValueType);
                                sr.Append(",");
                                byte[] unitArray = Encoding.UTF8.GetBytes(ecuMwTabEntry.ValueUnit);
                                sr.Append(Convert.ToBase64String(unitArray));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        jobsNodeNew.Add(new XAttribute("mwdata", sr.ToString()));
                    }
                }

                if (!string.IsNullOrEmpty(ecuInfo.VagDataFileName))
                {
                    string relativePath = ActivityCommon.MakeRelativePath(_vagDir, ecuInfo.VagDataFileName);
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        jobsNodeNew.Add(new XAttribute("vag_data_file", relativePath));
                    }
                }

                if (!string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                {
                    string relativePath = ActivityCommon.MakeRelativePath(_vagDir, ecuInfo.VagUdsFileName);
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        jobsNodeNew.Add(new XAttribute("vag_uds_file", relativePath));
                    }
                }

                foreach (XmlToolEcuActivity.JobInfo job in ecuInfo.JobList)
                {
                    if (!job.Selected)
                    {
                        continue;
                    }

                    XElement jobNodeOld = null;
                    XElement jobNodeNew = new XElement(ns + "job");
                    if (jobsNodeOld != null)
                    {
                        jobNodeOld = GetJobNode(ecuInfo, job, ns, jobsNodeOld);
                        if (jobNodeOld != null)
                        {
                            jobNodeNew.ReplaceAttributes(from el in jobNodeOld.Attributes()
                                where (el.Name != "id" && el.Name != "name" && el.Name != "args_first" && el.Name != "args" && el.Name != "arg_limit" && el.Name != "fixed_func_struct_id")
                                select new XAttribute(el));
                        }
                    }

                    jobNodeNew.Add(new XAttribute("name", job.Name));

                    if (job.EcuFixedFuncStruct != null && !string.IsNullOrWhiteSpace(job.EcuFixedFuncStruct.Id))
                    {
                        jobNodeNew.Add(new XAttribute("fixed_func_struct_id", job.EcuFixedFuncStruct.Id));
                    }

                    string jobArgs1 = XmlToolEcuActivity.GetJobArgs(job, job.Results, ecuInfo, out string jobArgs2);
                    if (!string.IsNullOrEmpty(jobArgs1))
                    {
                        if (!string.IsNullOrEmpty(jobArgs2))
                        {
                            jobNodeNew.Add(new XAttribute("args_first", jobArgs1));
                            jobNodeNew.Add(new XAttribute("args", jobArgs2));
                        }
                        else
                        {
                            jobNodeNew.Add(new XAttribute("args", jobArgs1));
                        }
                    }

                    if (job.ArgLimit >= 0)
                    {
                        jobNodeNew.Add(new XAttribute("arg_limit", job.ArgLimit));
                    }

                    int jobId = 1;
                    long lastBlockNumber = -1;
                    foreach (XmlToolEcuActivity.ResultInfo result in job.Results)
                    {
                        if (!result.ItemSelected)
                        {
                            continue;
                        }

                        string resultNamePrefix = string.Empty;
                        string displayTagPostfix = string.Empty;
                        if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                        {
                            if (result.MwTabEntry != null)
                            {
                                if (lastBlockNumber != result.MwTabEntry.BlockNumber)
                                {
                                    if (lastBlockNumber >= 0)
                                    {
                                        // store last generated job
                                        jobNodeOld?.Remove();
                                        jobsNodeNew.Add(jobNodeNew);
                                    }

                                    string args = XmlToolEcuActivity.GetJobArgs(result.MwTabEntry, ecuInfo);
                                    jobNodeOld = null;
                                    jobNodeNew = new XElement(ns + "job");
                                    if (jobsNodeOld != null)
                                    {
                                        jobNodeOld = GetJobNode(ecuInfo, job, ns, jobsNodeOld, args);
                                        if (jobNodeOld != null)
                                        {
                                            jobNodeNew.ReplaceAttributes(from el in jobNodeOld.Attributes()
                                                                         where (el.Name != "id" && el.Name != "name" && el.Name != "args" && el.Name != "arg_limit")
                                                                         select new XAttribute(el));
                                        }
                                    }

                                    jobNodeNew.Add(new XAttribute("id", (jobId++).ToString(Culture)));
                                    jobNodeNew.Add(new XAttribute("name", job.Name));
                                    jobNodeNew.Add(new XAttribute("args", args));
                                    if (job.ArgLimit >= 0)
                                    {
                                        jobNodeNew.Add(new XAttribute("arg_limit", job.ArgLimit));
                                    }
                                    lastBlockNumber = result.MwTabEntry.BlockNumber;
                                }
                            }
                        }

                        XElement displayNodeOld = null;
                        XElement displayNodeNew = new XElement(ns + "display");
                        if (jobNodeOld != null)
                        {
                            displayNodeOld = GetDisplayNode(ecuInfo, result, job, ns, jobNodeOld);
                            if (displayNodeOld != null)
                            {
                                displayNodeNew.ReplaceAttributes(from el in displayNodeOld.Attributes()
                                                                 where el.Name != "result" && el.Name != "ecu_job_id" && el.Name != "ecu_job_result_id" &&
                                                                       el.Name != "format" && el.Name != "display-order" &&
                                                                       el.Name != "grid-type" && el.Name != "min-value" && el.Name != "max-value" &&
                                                                       el.Name != "log_tag"
                                                                 select new XAttribute(el));
                            }
                        }
                        XAttribute nameAttr = displayNodeNew.Attribute("name");
                        if (nameAttr != null)
                        {
                            if (nameAttr.Value.StartsWith(DisplayNameJobPrefix, StringComparison.Ordinal))
                            {
                                nameAttr.Remove();
                            }
                        }

                        string displayTag = DisplayNameJobPrefix + job.Name + "#" + result.Name + displayTagPostfix;
                        if (displayNodeNew.Attribute("name") == null)
                        {
                            displayNodeNew.Add(new XAttribute("name", displayTag));
                        }

                        string resultName = resultNamePrefix + result.Name;
                        if (result.MwTabEntry != null)
                        {
                            resultName = result.MwTabEntry.ValueIndex.HasValue ? string.Format(Culture, "{0}#MW_Wert", result.MwTabEntry.ValueIndexTrans) : "1#ERGEBNIS1WERT";
                        }

                        displayNodeNew.Add(new XAttribute("result", resultName));
                        if (result.EcuJob != null && !string.IsNullOrWhiteSpace(result.EcuJob.Id) &&
                            result.EcuJobResult != null && !string.IsNullOrWhiteSpace(result.EcuJobResult.Id))
                        {
                            displayNodeNew.Add(new XAttribute("ecu_job_id", result.EcuJob.Id));
                            displayNodeNew.Add(new XAttribute("ecu_job_result_id", result.EcuJobResult.Id));
                        }
                        displayNodeNew.Add(new XAttribute("format", result.Format));
                        if (result.GridType != JobReader.DisplayInfo.GridModeType.Hidden)
                        {
                            displayNodeNew.Add(new XAttribute("display-order", result.DisplayOrder));
                            displayNodeNew.Add(new XAttribute("grid-type", result.GridType.ToString().ToLowerInvariant().Replace("_", "-")));
                            displayNodeNew.Add(new XAttribute("min-value", result.MinValue));
                            displayNodeNew.Add(new XAttribute("max-value", result.MaxValue));
                        }
                        if (!string.IsNullOrEmpty(result.LogTag))
                        {
                            displayNodeNew.Add(new XAttribute("log_tag", result.LogTag));
                        }

                        XElement stringNode = new XElement(ns + "string", result.DisplayText);
                        stringNode.Add(new XAttribute("name", displayTag));
                        stringsNode.Add(stringNode);

                        displayNodeOld?.Remove();
                        jobNodeNew.Add(displayNodeNew);
                    }

                    jobNodeOld?.Remove();
                    jobsNodeNew.Add(jobNodeNew);
                }
                jobsNodeOld?.Remove();
                pageNode.Add(jobsNodeNew);

                return document;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ReadErrorsXml(XDocument document, bool addUnusedEcus)
        {
            if (document.Root == null)
            {
                return;
            }
            XNamespace ns = document.Root.GetDefaultNamespace();
            XElement pageNode = document.Root.Element(ns + "page");
            if (pageNode == null)
            {
                return;
            }

            bool noUpdate = false;
            XAttribute pageNoUpdateAttr = pageNode.Attribute("no_update");
            if (pageNoUpdateAttr != null)
            {
                try
                {
                    noUpdate = XmlConvert.ToBoolean(pageNoUpdateAttr.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            _instanceData.NoErrorsPageUpdate = noUpdate;

            XElement stringsNode = GetDefaultStringsNode(ns, pageNode);
            XElement errorsNode = pageNode.Element(ns + "read_errors");
            if (errorsNode == null)
            {
                return;
            }

            foreach (EcuInfo ecuInfo in _ecuList)
            {
                XElement ecuNode = GetEcuNode(ecuInfo, ns, errorsNode);
                if (ecuNode != null)
                {
                    SetEcuNameFromStringsNode(ns, ecuInfo, stringsNode);
                }
            }
            if (addUnusedEcus || (_instanceData.ManualConfigIdx > 0))
            {   // manual mode, add missing entries
                foreach (XElement ecuNode in errorsNode.Elements(ns + "ecu"))
                {
                    XAttribute sgbdAttr = ecuNode.Attribute("sgbd");
                    string sgbdName = sgbdAttr?.Value;
                    if (string.IsNullOrEmpty(sgbdName))
                    {
                        continue;
                    }

                    XAttribute vagDataAttr = ecuNode.Attribute("vag_data_file");
                    string vagDataFileName = vagDataAttr?.Value;
                    XAttribute vagUdsAttr = ecuNode.Attribute("vag_uds_file");
                    string vagUdsFileName = vagUdsAttr?.Value;
                    bool ecuFound = _ecuList.Any(ecuInfo => string.Compare(sgbdName, ecuInfo.Sgbd, StringComparison.OrdinalIgnoreCase) == 0);
                    if (!ecuFound)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        EcuInfo ecuInfo = new EcuInfo(sgbdName.ToUpperInvariant(), -1, string.Empty, sgbdName, string.Empty,
                            JobReader.PageInfo.DisplayModeType.List, DisplayFontSize.Small, JobReader.GaugesPortraitDefault, JobReader.GaugesLandscapeDefault, string.Empty, null,
                            vagDataFileName, vagUdsFileName)
                        {
                            PageName = string.Empty,
                            EcuName = string.Empty
                        };
                        if (SetEcuNameFromStringsNode(ns, ecuInfo, stringsNode))
                        {
                            ecuInfo.PageName = ecuInfo.EcuName;
                        }
                        _ecuList.Add(ecuInfo);
                    }
                }
            }
        }

        private XDocument GenerateErrorsXml(XDocument documentOld)
        {
            try
            {
                XDocument document = documentOld;
                if (document?.Root == null)
                {
                    document = XDocument.Parse(string.Format(XmlDocumentFrame, "fragment"));
                }
                if (document.Root == null)
                {
                    return null;
                }
                XNamespace ns = document.Root.GetDefaultNamespace();
                XElement pageNode = document.Root.Element(ns + "page");
                if (pageNode == null)
                {
                    pageNode = new XElement(ns + "page");
                    document.Root.Add(pageNode);
                }

                XAttribute pageNoUpdateAttr = pageNode.Attribute("no_update");
                if (pageNoUpdateAttr != null)
                {
                    bool noUpdate = false;
                    try
                    {
                        noUpdate = XmlConvert.ToBoolean(pageNoUpdateAttr.Value);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (noUpdate)
                    {
                        return null;
                    }
                }

                XAttribute pageNameAttr = pageNode.Attribute("name");
                if (pageNameAttr == null)
                {
                    pageNode.Add(new XAttribute("name", DisplayNamePage));
                }

                XElement stringsNode = GetDefaultStringsNode(ns, pageNode);
                if (stringsNode == null)
                {
                    stringsNode = new XElement(ns + "strings");
                    pageNode.Add(stringsNode);
                }
                else
                {
                    RemoveGeneratedStringEntries(ns, stringsNode);
                }

                XElement stringNodePage = new XElement(ns + "string", GetString(Resource.String.xml_tool_errors_page));
                stringNodePage.Add(new XAttribute("name", DisplayNamePage));
                stringsNode.Add(stringNodePage);

                XElement errorsNodeOld = pageNode.Element(ns + "read_errors");
                XElement errorsNodeNew = new XElement(ns + "read_errors");
                if (errorsNodeOld != null)
                {
                    errorsNodeNew.ReplaceAttributes(from el in errorsNodeOld.Attributes() select new XAttribute(el));
                }

                XAttribute attrSgbdFunc = errorsNodeNew.Attribute("sgbd_functional");
                attrSgbdFunc?.Remove();
                if (!string.IsNullOrEmpty(_instanceData.SgbdFunctional))
                {
                    errorsNodeNew.Add(new XAttribute("sgbd_functional", _instanceData.SgbdFunctional));
                }

                XAttribute attrVehicleSeries = errorsNodeNew.Attribute("vehicle_series");
                attrVehicleSeries?.Remove();
                if (!string.IsNullOrEmpty(_instanceData.VehicleType))
                {
                    errorsNodeNew.Add(new XAttribute("vehicle_series", _instanceData.VehicleType));
                }

                foreach (EcuInfo ecuInfo in _ecuList)
                {
                    XElement ecuNode = null;
                    if (errorsNodeOld != null)
                    {
                        ecuNode = GetEcuNode(ecuInfo, ns, errorsNodeOld);
                        if (ecuNode != null)
                        {
                            ecuNode = new XElement(ecuNode);
                        }
                    }
                    if (ecuNode == null)
                    {
                        ecuNode = new XElement(ns + "ecu");
                    }
                    else
                    {
                        XAttribute attr = ecuNode.Attribute("name");
                        attr?.Remove();
                        attr = ecuNode.Attribute("sgbd");
                        attr?.Remove();
                        attr = ecuNode.Attribute("vag_data_file");
                        attr?.Remove();
                        attr = ecuNode.Attribute("vag_uds_file");
                        attr?.Remove();
                    }
                    string displayTag = DisplayNameEcuPrefix + ecuInfo.Name;
                    errorsNodeNew.Add(ecuNode);
                    ecuNode.Add(new XAttribute("name", displayTag));
                    ecuNode.Add(new XAttribute("sgbd", ecuInfo.Sgbd));
                    if (!string.IsNullOrEmpty(ecuInfo.VagDataFileName))
                    {
                        string relativePath = ActivityCommon.MakeRelativePath(_vagDir, ecuInfo.VagDataFileName);
                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            ecuNode.Add(new XAttribute("vag_data_file", relativePath));
                        }
                    }
                    if (!string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                    {
                        string relativePath = ActivityCommon.MakeRelativePath(_vagDir, ecuInfo.VagUdsFileName);
                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            ecuNode.Add(new XAttribute("vag_uds_file", relativePath));
                        }
                    }

                    XElement stringNode = new XElement(ns + "string", ecuInfo.EcuName);
                    stringNode.Add(new XAttribute("name", displayTag));
                    stringsNode.Add(stringNode);
                }
                errorsNodeOld?.Remove();
                pageNode.Add(errorsNodeNew);

                return document;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ReadPagesXml(XDocument document)
        {
            string xmlFileDir = XmlFileDir();
            if (xmlFileDir == null)
            {
                return;
            }
            if (document.Root == null)
            {
                return;
            }
            XNamespace ns = document.Root.GetDefaultNamespace();
            XElement pagesNode = document.Root.Element(ns + "pages");
            if (pagesNode == null)
            {
                return;
            }

            if (_instanceData.ManualConfigIdx > 0)
            {   // manual mode, create ecu list
                ClearEcuList();
                foreach (XElement node in pagesNode.Elements(ns + "include"))
                {
                    XAttribute fileAttrib = node.Attribute("filename");
                    if (fileAttrib == null)
                    {
                        continue;
                    }
                    string fileName = fileAttrib.Value;
                    if (string.Compare(fileName, ErrorsFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }
                    string xmlPageFile = Path.Combine(xmlFileDir, fileName);
                    if (!File.Exists(xmlPageFile))
                    {
                        continue;
                    }
                    string ecuName = Path.GetFileNameWithoutExtension(fileName);
                    if (string.IsNullOrEmpty(ecuName))
                    {
                        continue;
                    }
                    try
                    {
                        string sgbdName = ReadPageSgbd(XDocument.Load(xmlPageFile), out bool noUpdate, out JobReader.PageInfo.DisplayModeType displayMode, out DisplayFontSize fontSize,
                            out int gaugesPortrait, out int gaugesLandscape,
                            out string mwTabFileName, out Dictionary<long, EcuMwTabEntry> mwTabEcuDict, out string vagDataFileName, out string vagUdsFileName);
                        if (!string.IsNullOrEmpty(sgbdName))
                        {
                            _ecuList.Add(new EcuInfo(ecuName, -1, string.Empty, sgbdName, string.Empty, displayMode, fontSize, gaugesPortrait, gaugesLandscape,
                                            mwTabFileName, mwTabEcuDict, vagDataFileName, vagUdsFileName)
                            {
                                Selected = true,
                                NoUpdate = noUpdate
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            else
            {   // auto mode, reorder list and set selections, add missing entries
                foreach (XElement node in pagesNode.Elements(ns + "include").Reverse())
                {
                    XAttribute fileAttrib = node.Attribute("filename");
                    if (fileAttrib == null)
                    {
                        continue;
                    }
                    string fileName = fileAttrib.Value;
                    if (string.Compare(fileName, ErrorsFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }
                    string xmlPageFile = Path.Combine(xmlFileDir, fileName);
                    if (!File.Exists(xmlPageFile))
                    {
                        continue;
                    }
                    bool found = false;
                    for (int i = 0; i < _ecuList.Count; i++)
                    {
                        EcuInfo ecuInfo = _ecuList[i];
                        string ecuFileName = ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension);
                        if (string.Compare(ecuFileName, fileName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            found = true;
                            ecuInfo.Selected = true;
                            _ecuList.Remove(ecuInfo);
                            _ecuList.Insert(0, ecuInfo);
                            break;
                        }
                    }
                    if (!found)
                    {
                        string ecuName = Path.GetFileNameWithoutExtension(fileName);
                        if (!string.IsNullOrEmpty(ecuName))
                        {
                            try
                            {
                                string sgbdName = ReadPageSgbd(XDocument.Load(xmlPageFile), out bool noUpdate, out JobReader.PageInfo.DisplayModeType displayMode, out DisplayFontSize fontSize,
                                    out int gaugesPortrait, out int gaugesLandscape,
                                    out string mwTabFileName, out Dictionary<long, EcuMwTabEntry> mwTabEcuDict, out string vagDataFileName, out string vagUdsFileName);
                                if (!string.IsNullOrEmpty(sgbdName))
                                {
                                    _ecuList.Insert(0, new EcuInfo(ecuName, -1, string.Empty, sgbdName, string.Empty, displayMode, fontSize, gaugesPortrait, gaugesLandscape,
                                                        mwTabFileName, mwTabEcuDict, vagDataFileName, vagUdsFileName)
                                    {
                                        Selected = true,
                                        NoUpdate = noUpdate
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }

            {
                string fileName = ErrorsFileName;
                XElement fileNode = GetFileNode(fileName, ns, pagesNode);
                if ((fileNode == null) || !File.Exists(Path.Combine(xmlFileDir, fileName)))
                {
                    _instanceData.AddErrorsPage = false;
                }
            }
        }

        private XDocument GeneratePagesXml(XDocument documentOld)
        {
            string xmlFileDir = XmlFileDir();
            if (xmlFileDir == null)
            {
                return null;
            }
            try
            {
                XDocument document = documentOld;
                if (document?.Root == null)
                {
                    document = XDocument.Parse(string.Format(XmlDocumentFrame, "fragment"));
                }
                if (document.Root == null)
                {
                    return null;
                }
                XNamespace ns = document.Root.GetDefaultNamespace();
                XElement pagesNodeOld = document.Root.Element(ns + "pages");
                XElement pagesNodeNew = new XElement(ns + "pages");
                if (pagesNodeOld != null)
                {
                    pagesNodeNew.ReplaceAttributes(from el in pagesNodeOld.Attributes() select new XAttribute(el));
                }

                foreach (EcuInfo ecuInfo in _ecuList)
                {
                    string fileName = ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension);
                    if (!ecuInfo.Selected || !File.Exists(Path.Combine(xmlFileDir, fileName)))
                    {
                        continue;
                    }
                    XElement fileNode = null;
                    if (pagesNodeOld != null)
                    {
                        fileNode = GetFileNode(fileName, ns, pagesNodeOld);
                        if (fileNode != null)
                        {
                            fileNode = new XElement(fileNode);
                        }
                    }
                    if (fileNode == null)
                    {
                        fileNode = new XElement(ns + "include");
                    }
                    else
                    {
                        XAttribute attr = fileNode.Attribute("filename");
                        attr?.Remove();
                    }

                    fileNode.Add(new XAttribute("filename", fileName));
                    pagesNodeNew.Add(fileNode);
                }

                {
                    // errors file
                    string fileName = ErrorsFileName;
                    if (_instanceData.AddErrorsPage && File.Exists(Path.Combine(xmlFileDir, fileName)))
                    {
                        XElement fileNode = null;
                        if (pagesNodeOld != null)
                        {
                            fileNode = GetFileNode(fileName, ns, pagesNodeOld);
                            if (fileNode != null)
                            {
                                fileNode = new XElement(fileNode);
                            }
                        }
                        if (fileNode == null)
                        {
                            fileNode = new XElement(ns + "include");
                        }
                        else
                        {
                            XAttribute attr = fileNode.Attribute("filename");
                            attr?.Remove();
                        }
                        fileNode.Add(new XAttribute("filename", fileName));
                        pagesNodeNew.Add(fileNode);
                    }
                }
                pagesNodeOld?.Remove();
                document.Root.Add(pagesNodeNew);

                return document;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ReadConfigXml(XDocument document)
        {
            _instanceData.EcuSearchAbortIndex = -1;
            if (document.Root == null)
            {
                return;
            }
            XNamespace ns = document.Root.GetDefaultNamespace();
            XElement globalNode = document.Root.Element(ns + "global");
            // ReSharper disable once UseNullPropagation
            if (globalNode == null)
            {
                return;
            }
            XAttribute abortAttrib = globalNode.Attribute("search_abort_index");
            if (abortAttrib != null)
            {
                try
                {
                    _instanceData.EcuSearchAbortIndex = XmlConvert.ToInt32(abortAttrib.Value);
                }
                catch (Exception)
                {
                    _instanceData.EcuSearchAbortIndex = -1;
                }
            }
        }

        private XDocument GenerateConfigXml(XDocument documentOld)
        {
            string xmlFileDir = XmlFileDir();
            if (xmlFileDir == null)
            {
                return null;
            }
            try
            {
                XDocument document = documentOld;
                if (document?.Root == null)
                {
                    document = XDocument.Parse(string.Format(XmlDocumentFrame, "configuration"));
                }
                if (document.Root == null)
                {
                    return null;
                }
                XNamespace ns = document.Root.GetDefaultNamespace();
                XElement globalNode = document.Root.Element(ns + "global");
                if (globalNode == null)
                {
                    globalNode = new XElement(ns + "global");
                    document.Root.Add(globalNode);
                }
                else
                {
                    XAttribute attr = globalNode.Attribute("ecu_path");
                    attr?.Remove();
                    attr = globalNode.Attribute("manufacturer");
                    attr?.Remove();
                    attr = globalNode.Attribute("interface");
                    attr?.Remove();
                    attr = globalNode.Attribute("search_abort_index");
                    attr?.Remove();
                    attr = globalNode.Attribute("vehicle_series");
                    attr?.Remove();
                }

                XAttribute logPathAttr = globalNode.Attribute("log_path");
                if (logPathAttr == null)
                {
                    globalNode.Add(new XAttribute("log_path", "Log"));
                }

                string manufacturerName = string.Empty;
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Bmw:
                        manufacturerName = "BMW";
                        break;

                    case ActivityCommon.ManufacturerType.Audi:
                        manufacturerName = "Audi";
                        break;

                    case ActivityCommon.ManufacturerType.Seat:
                        manufacturerName = "Seat";
                        break;

                    case ActivityCommon.ManufacturerType.Skoda:
                        manufacturerName = "Skoda";
                        break;

                    case ActivityCommon.ManufacturerType.Vw:
                        manufacturerName = "VW";
                        break;
                }
                globalNode.Add(new XAttribute("manufacturer", manufacturerName));

                string interfaceName = string.Empty;
                switch (_activityCommon.SelectedInterface)
                {
                    case ActivityCommon.InterfaceType.Bluetooth:
                        interfaceName = "BLUETOOTH";
                        break;

                    case ActivityCommon.InterfaceType.Enet:
                        interfaceName = "ENET";
                        break;

                    case ActivityCommon.InterfaceType.ElmWifi:
                        interfaceName = "ELMWIFI";
                        break;

                    case ActivityCommon.InterfaceType.DeepObdWifi:
                        interfaceName = "DEEPOBDWIFI";
                        break;

                    case ActivityCommon.InterfaceType.Ftdi:
                        interfaceName = "FTDI";
                        break;
                }
                globalNode.Add(new XAttribute("interface", interfaceName));
                if (_instanceData.EcuSearchAbortIndex >= 0)
                {
                    globalNode.Add(new XAttribute("search_abort_index", _instanceData.EcuSearchAbortIndex));
                }

                if (!string.IsNullOrEmpty(_instanceData.VehicleType))
                {
                    globalNode.Add(new XAttribute("vehicle_series", _instanceData.VehicleType));
                }

                XElement includeNode = document.Root.Element(ns + "include");
                if (includeNode == null)
                {
                    includeNode = new XElement(ns + "include");
                    document.Root.Add(includeNode);
                    includeNode.Add(new XAttribute("filename", PagesFileName));
                }
                return document;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ReadAllXml(bool addUnusedEcus = false)
        {
            _instanceData.AddErrorsPage = true;
            _instanceData.NoErrorsPageUpdate = false;
            string xmlFileDir = XmlFileDir();
            if (xmlFileDir == null)
            {
                return;
            }

            string xmlPagesFile = Path.Combine(xmlFileDir, PagesFileName);
            if (File.Exists(xmlPagesFile))
            {
                try
                {
                    XDocument documentPages = XDocument.Load(xmlPagesFile);
                    ReadPagesXml(documentPages);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            string xmlErrorsFile = Path.Combine(xmlFileDir, ErrorsFileName);
            if (File.Exists(xmlErrorsFile))
            {
                try
                {
                    XDocument documentPages = XDocument.Load(xmlErrorsFile);
                    ReadErrorsXml(documentPages, addUnusedEcus);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            string xmlConfigFile = ConfigFileName(xmlFileDir);
            if (File.Exists(xmlConfigFile))
            {
                try
                {
                    XDocument documentPages = XDocument.Load(xmlConfigFile);
                    ReadConfigXml(documentPages);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            _activityCommon.ReadTranslationCache(Path.Combine(xmlFileDir, TranslationFileName));
        }

        private string SaveAllXml()
        {
            string xmlFileDir = XmlFileDir();
            if (xmlFileDir == null)
            {
                return null;
            }
            try
            {
                Directory.CreateDirectory(xmlFileDir);
                // page files
                foreach (EcuInfo ecuInfo in _ecuList)
                {
                    if (ecuInfo.JobList == null) continue;
                    if (!ecuInfo.Selected) continue;
                    string xmlPageFile = Path.Combine(xmlFileDir, ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension));
                    XDocument documentPage = null;
                    if (File.Exists(xmlPageFile))
                    {
                        try
                        {
                            documentPage = XDocument.Load(xmlPageFile);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    XDocument documentPageNew = GeneratePageXml(ecuInfo, documentPage);
                    if (documentPageNew != null)
                    {
                        try
                        {
                            documentPageNew.Save(xmlPageFile);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }

                {
                    // errors file
                    string xmlErrorsFile = Path.Combine(xmlFileDir, ErrorsFileName);
                    XDocument documentPage = null;
                    if (File.Exists(xmlErrorsFile))
                    {
                        try
                        {
                            documentPage = XDocument.Load(xmlErrorsFile);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    XDocument documentPageNew = GenerateErrorsXml(documentPage);
                    if (documentPageNew != null)
                    {
                        try
                        {
                            documentPageNew.Save(xmlErrorsFile);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }

                // pages file
                string xmlPagesFile = Path.Combine(xmlFileDir, PagesFileName);
                XDocument documentPages = null;
                if (File.Exists(xmlPagesFile))
                {
                    try
                    {
                        documentPages = XDocument.Load(xmlPagesFile);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                XDocument documentPagesNew = GeneratePagesXml(documentPages);
                if (documentPagesNew != null)
                {
                    try
                    {
                        documentPagesNew.Save(xmlPagesFile);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                // config file
                string xmlConfigFile = ConfigFileName(xmlFileDir);
                XDocument documentConfig = null;
                if (File.Exists(xmlConfigFile))
                {
                    try
                    {
                        documentConfig = XDocument.Load(xmlConfigFile);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                XDocument documentConfigNew = GenerateConfigXml(documentConfig);
                if (documentConfigNew != null)
                {
                    try
                    {
                        documentConfigNew.Save(xmlConfigFile);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                ActivityCommon.WriteResourceToFile(typeof(XmlToolActivity).Namespace + ".Xml." + XsdFileName, Path.Combine(xmlFileDir, XsdFileName));
                _activityCommon.StoreTranslationCache(Path.Combine(xmlFileDir, TranslationFileName));
                return xmlConfigFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool DeleteAllXml()
        {
            ClearEcuList();
            UpdateDisplay();
            try
            {
                string xmlFileDir = XmlFileDir();
                if (xmlFileDir == null)
                {
                    return false;
                }
                Directory.Delete(xmlFileDir, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SaveConfiguration(bool finish)
        {
            if (IsJobRunning())
            {
                return false;
            }

            if (IsPageSelectionActive())
            {
                return false;
            }

            string xmlFileName = SaveAllXml();
            if (xmlFileName == null)
            {
                ShowAlert(Resource.String.alert_title_error, Resource.String.xml_tool_save_xml_failed);
                return false;
            }
            if (!finish)
            {
                return true;
            }
            Intent intent = new Intent();
            intent.PutExtra(ExtraInterface, (int)_activityCommon.SelectedInterface);
            intent.PutExtra(ExtraDeviceName, _instanceData.DeviceName);
            intent.PutExtra(ExtraDeviceAddress, _instanceData.DeviceAddress);
            intent.PutExtra(ExtraEnetIp, _activityCommon.SelectedEnetIp);
            intent.PutExtra(ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
            intent.PutExtra(ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
            intent.PutExtra(ExtraFileName, xmlFileName);

            // Set result and finish this Activity
            SetResult(Android.App.Result.Ok, intent);
            FinishContinue();
            return true;
        }

        private XElement GetJobNode(EcuInfo ecuInfo, XmlToolEcuActivity.JobInfo job, XNamespace ns, XElement jobsNode)
        {
            if (job.EcuFixedFuncStruct != null && !string.IsNullOrWhiteSpace(job.EcuFixedFuncStruct.Id))
            {
                return (from node in jobsNode.Elements(ns + "job")
                    let nameAttrib = node.Attribute("name")
                    let structIdAttrib = node.Attribute("fixed_func_struct_id")
                    where structIdAttrib != null
                    where job.EcuFixedFuncStruct.IdPresent(structIdAttrib.Value, ecuInfo.UseCompatIds)
                    select node).FirstOrDefault();
            }

            return (from node in jobsNode.Elements(ns + "job")
                    let nameAttrib = node.Attribute("name")
                    where nameAttrib != null
                    where string.Compare(nameAttrib.Value, job.Name, StringComparison.OrdinalIgnoreCase) == 0 select node).FirstOrDefault();
        }

        private XElement GetJobNode(EcuInfo ecuInfo, XmlToolEcuActivity.JobInfo job, XNamespace ns, XElement jobsNode, string args)
        {
            return (from node in jobsNode.Elements(ns + "job")
                    let nameAttrib = node.Attribute("name")
                    let argsAttrib = node.Attribute("args")
                    where nameAttrib != null
                    where argsAttrib != null
                    where string.Compare(nameAttrib.Value, job.Name, StringComparison.OrdinalIgnoreCase) == 0
                    where string.Compare(argsAttrib.Value, args, StringComparison.OrdinalIgnoreCase) == 0
                    select node).FirstOrDefault();
        }

        private XElement GetDisplayNode(EcuInfo ecuInfo, XmlToolEcuActivity.ResultInfo result, XmlToolEcuActivity.JobInfo job, XNamespace ns, XElement jobNode)
        {
            string resultName = result.Name;
            if (result.MwTabEntry != null)
            {
                bool compareDisplayTag = false;
                foreach (XElement node in jobNode.Elements(ns + "display"))
                {
                    XAttribute nameAttrib = node.Attribute("name");
                    if (nameAttrib != null)
                    {
                        string[] nameArray = nameAttrib.Value.Split('#');
                        if (nameArray.Length >= 3 && nameArray[2].Contains("-"))
                        {
                            compareDisplayTag = true;
                            break;
                        }
                    }
                }

                if (compareDisplayTag)
                {
                    string displayTag = DisplayNameJobPrefix + job.Name + "#" + result.Name;
                    string displayTagOld = displayTag;
                    if (!string.IsNullOrEmpty(result.NameOld))
                    {
                        displayTagOld = DisplayNameJobPrefix + job.Name + "#" + result.NameOld;
                    }
                    return (from node in jobNode.Elements(ns + "display")
                        let nameAttrib = node.Attribute("name")
                        where nameAttrib != null
                        where string.Compare(nameAttrib.Value, displayTag, StringComparison.OrdinalIgnoreCase) == 0 ||
                               string.Compare(nameAttrib.Value, displayTagOld, StringComparison.OrdinalIgnoreCase) == 0
                            select node).FirstOrDefault();

                }

                resultName = result.MwTabEntry.ValueIndex.HasValue ? string.Format(Culture, "{0}#MW_Wert", result.MwTabEntry.ValueIndexTrans) : "1#ERGEBNIS1WERT";
                return (from node in jobNode.Elements(ns + "display")
                        let resultAttrib = node.Attribute("result")
                        where resultAttrib != null
                        where string.Compare(resultAttrib.Value, resultName, StringComparison.OrdinalIgnoreCase) == 0
                        select node).FirstOrDefault();
            }

            if (result.EcuJob != null && !string.IsNullOrWhiteSpace(result.EcuJob.Id) &&
                result.EcuJobResult != null && !string.IsNullOrWhiteSpace(result.EcuJobResult.Id))
            {
                return (from node in jobNode.Elements(ns + "display")
                    let resultAttrib = node.Attribute("result")
                    let jobIdAttrib = node.Attribute("ecu_job_id")
                    let jobIdResultAttrib = node.Attribute("ecu_job_result_id")
                    where resultAttrib != null
                    where jobIdAttrib != null
                    where jobIdResultAttrib != null
                    where string.Compare(resultAttrib.Value, resultName, StringComparison.OrdinalIgnoreCase) == 0
                    where result.EcuJob.IdPresent(jobIdAttrib.Value, ecuInfo.UseCompatIds)
                    where result.EcuJobResult.IdPresent(jobIdResultAttrib.Value, ecuInfo.UseCompatIds)
                    select node).FirstOrDefault();
            }

            return (from node in jobNode.Elements(ns + "display")
                    let resultAttrib = node.Attribute("result")
                    where resultAttrib != null
                    where string.Compare(resultAttrib.Value, resultName, StringComparison.OrdinalIgnoreCase) == 0
                    select node).FirstOrDefault();
        }

        private XElement GetFileNode(string fileName, XNamespace ns, XElement pagesNode)
        {
            return (from node in pagesNode.Elements(ns + "include")
                    let fileAttrib = node.Attribute("filename")
                    where fileAttrib != null
                    where string.Compare(fileAttrib.Value, fileName, StringComparison.OrdinalIgnoreCase) == 0
                    select node).FirstOrDefault();
        }

        private XElement GetEcuNode(EcuInfo ecuInfo, XNamespace ns, XElement errorsNode)
        {
            return (from node in errorsNode.Elements(ns + "ecu")
                    let nameAttrib = node.Attribute("sgbd")
                    where nameAttrib != null
                    where string.Compare(nameAttrib.Value, ecuInfo.Sgbd, StringComparison.OrdinalIgnoreCase) == 0
                    select node).FirstOrDefault();
        }

        private XElement GetDefaultStringsNode(XNamespace ns, XElement pageNode)
        {
            return pageNode.Elements(ns + "strings").FirstOrDefault(node => node.Attribute("lang") == null);
        }

        private string GetStringEntry(string entryName, XNamespace ns, XElement stringsNode)
        {
            return (from node in stringsNode.Elements(ns + "string")
                    let nameAttr = node.Attribute("name")
                    where nameAttr != null
                    where string.Compare(nameAttr.Value, entryName, StringComparison.Ordinal) == 0 select node.FirstNode).OfType<XText>().Select(text => text.Value).FirstOrDefault();
        }

        private void RemoveGeneratedStringEntries(XNamespace ns, XElement stringsNode)
        {
            List<XElement> removeList =
                (from node in stringsNode.Elements(ns + "string")
                    let nameAttr = node.Attribute("name")
                    where nameAttr != null &&
                          (nameAttr.Value.StartsWith(DisplayNameJobPrefix, StringComparison.Ordinal) ||
                           nameAttr.Value.StartsWith(DisplayNameEcuPrefix, StringComparison.Ordinal) ||
                           string.Compare(nameAttr.Value, DisplayNamePage, StringComparison.Ordinal) == 0)
                    select node).ToList();
            foreach (XElement node in removeList)
            {
                node.Remove();
            }
        }

        private bool SetEcuNameFromStringsNode(XNamespace ns, EcuInfo ecuInfo, XElement stringsNode)
        {
            if (stringsNode != null)
            {
                string displayTag = DisplayNameEcuPrefix + ecuInfo.Name;
                string displayText = GetStringEntry(displayTag, ns, stringsNode);
                if (displayText != null)
                {
                    ecuInfo.EcuName = displayText;
                    return true;
                }
            }
            return false;
        }

        private string XmlFileDir()
        {
            if (string.IsNullOrEmpty(_appDataDir))
            {
                return null;
            }

            if (IsPageSelectionActive() && !string.IsNullOrEmpty(_lastFileName))
            {
                try
                {
                    return Path.GetDirectoryName(_lastFileName);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            string configBaseDir = Path.Combine(_appDataDir, "Configurations");
            switch (ActivityCommon.SelectedManufacturer)
            {
                case ActivityCommon.ManufacturerType.Audi:
                    configBaseDir += "Audi";
                    break;

                case ActivityCommon.ManufacturerType.Seat:
                    configBaseDir += "Seat";
                    break;

                case ActivityCommon.ManufacturerType.Skoda:
                    configBaseDir += "Skoda";
                    break;

                case ActivityCommon.ManufacturerType.Vw:
                    configBaseDir += "Vw";
                    break;
            }
            string vin = _instanceData.Vin;
            if (_instanceData.ManualConfigIdx > 0)
            {
                vin = ManualConfigName + _instanceData.ManualConfigIdx.ToString(CultureInfo.InvariantCulture);
            }
            if (string.IsNullOrEmpty(vin))
            {
                vin = UnknownVinConfigName;
            }
            else
            {
                if (vin.Length == 17)
                {
                    vin = vin.Substring(10, 7);
                    string dirShort = Path.Combine(configBaseDir, ActivityCommon.CreateValidFileName(vin));
                    string dirLong = Path.Combine(configBaseDir, ActivityCommon.CreateValidFileName(_instanceData.Vin));
                    if (Directory.Exists(dirLong) && !Directory.Exists(dirShort))
                    {   // use long VIN if present for compatibility
                        vin = _instanceData.Vin;
                    }
                }
            }
            try
            {
                return Path.Combine(configBaseDir, ActivityCommon.CreateValidFileName(vin));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string ConfigFileName(string xmlFileDir)
        {
            string interfaceType = string.Empty;
            switch (_activityCommon.SelectedInterface)
            {
                case ActivityCommon.InterfaceType.Bluetooth:
                    interfaceType = "Bt";
                    break;

                case ActivityCommon.InterfaceType.Enet:
                    interfaceType = "Enet";
                    break;

                case ActivityCommon.InterfaceType.ElmWifi:
                    interfaceType = "ElmWifi";
                    break;

                case ActivityCommon.InterfaceType.DeepObdWifi:
                    interfaceType = "DeepObdWifi";
                    break;

                case ActivityCommon.InterfaceType.Ftdi:
                    interfaceType = "Ftdi";
                    break;
            }

            string prefix;
            if (_instanceData.ManualConfigIdx > 0)
            {
                prefix = ManualConfigName + _instanceData.ManualConfigIdx.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                string vin = _instanceData.Vin;
                if (!string.IsNullOrEmpty(vin))
                {
                    if (vin.Length == 17)
                    {
                        vin = vin.Substring(10, 7);
                        string fileShort = Path.Combine(xmlFileDir, ActivityCommon.CreateValidFileName(vin) + "_" + interfaceType + ConfigFileExtension);
                        string fileLong = Path.Combine(xmlFileDir, ActivityCommon.CreateValidFileName(_instanceData.Vin) + "_" + interfaceType + ConfigFileExtension);
                        if (File.Exists(fileLong) && !File.Exists(fileShort))
                        {   // use long VIN if present for compatibility
                            vin = _instanceData.Vin;
                        }
                    }
                }
                else
                {
                    vin = UnknownVinConfigName;
                }
                prefix = ActivityCommon.CreateValidFileName(vin);
            }
            return Path.Combine(xmlFileDir, prefix + "_" + interfaceType + ConfigFileExtension);
        }

        private void ShowAlert(int titleId, int messageId)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_ok, (sender, args) => { })
                .SetMessage(messageId)
                .SetTitle(titleId);
            AlertDialog alertDialog = builder.Show();
            if (alertDialog != null)
            {
                alertDialog.DismissEvent += DialogDismissEvent;
            }
        }

        private void DialogDismissEvent(object eventObject, EventArgs eventArgs)
        {
            if (_activityCommon == null)
            {
                return;
            }

            if (IsPageSelectionActive())
            {
                Finish();
            }
        }

        private class EcuListAdapter : BaseAdapter<EcuInfo>
        {
            public delegate void CheckChangedEventHandler(EcuInfo ecuInfo);
            public delegate void MenuOptionsSelectedEventHandler(EcuInfo ecuInfo, View view);
            public event CheckChangedEventHandler CheckChanged;
            public event MenuOptionsSelectedEventHandler MenuOptionsSelected;

            private readonly List<EcuInfo> _items;
            public List<EcuInfo> Items => _items;
            private readonly XmlToolActivity _context;
            private bool _ignoreCheckEvent;

            public EcuListAdapter(XmlToolActivity context)
            {
                _context = context;
                _items = new List<EcuInfo>();
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override EcuInfo this[int position] => _items[position];

            public override int Count => _items.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ecu_select_list, null);
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxEcuSelect);
                LinearLayout buttonEcuOptionsMenu = view.FindViewById<LinearLayout>(Resource.Id.buttonEcuOptionsMenu);

                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                buttonEcuOptionsMenu.Tag = new TagInfo(item);
                buttonEcuOptionsMenu.Click -= OnEcuOptionsClick;
                buttonEcuOptionsMenu.Click += OnEcuOptionsClick;

                TextView textEcuName = view.FindViewById<TextView>(Resource.Id.textEcuName);
                TextView textEcuDesc = view.FindViewById<TextView>(Resource.Id.textEcuDesc);

                StringBuilder stringBuilderName = new StringBuilder();
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && item.Address >= 0)
                {
                    stringBuilderName.Append(string.Format("{0:X02} ", item.Address));
                }
                stringBuilderName.Append(!string.IsNullOrEmpty(item.EcuName) ? item.EcuName : item.Name);
                if (!string.IsNullOrEmpty(item.Description))
                {
                    stringBuilderName.Append(": ");
                    stringBuilderName.Append(item.DescriptionTransRequired && !string.IsNullOrEmpty(item.DescriptionTrans)
                        ? item.DescriptionTrans
                        : item.Description);
                }
                textEcuName.Text = stringBuilderName.ToString();

                StringBuilder stringBuilderInfo = new StringBuilder();
                stringBuilderInfo.Append(_context.GetString(Resource.String.xml_tool_info_sgbd));
                stringBuilderInfo.Append(": ");
                stringBuilderInfo.Append(item.Sgbd);
                if (!string.IsNullOrEmpty(item.Grp))
                {
                    stringBuilderInfo.Append(", ");
                    stringBuilderInfo.Append(_context.GetString(Resource.String.xml_tool_info_grp));
                    stringBuilderInfo.Append(": ");
                    stringBuilderInfo.Append(item.Grp);
                }
                if (!string.IsNullOrEmpty(item.Vin))
                {
                    stringBuilderInfo.Append(", ");
                    stringBuilderInfo.Append(_context.GetString(Resource.String.xml_tool_info_vin));
                    stringBuilderInfo.Append(": ");
                    stringBuilderInfo.Append(item.Vin);
                }
                textEcuDesc.Text = stringBuilderInfo.ToString();

                return view;
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = sender as CheckBox;
                    TagInfo tagInfo = checkBox?.Tag as TagInfo;
                    if (tagInfo != null && tagInfo.Info.Selected != args.IsChecked)
                    {
                        tagInfo.Info.Selected = args.IsChecked;
                        CheckChanged?.Invoke(tagInfo.Info);
                        NotifyDataSetChanged();
                    }
                }
            }

            private void OnEcuOptionsClick(object sender, EventArgs args)
            {
                LinearLayout button = sender as LinearLayout;
                TagInfo tagInfo = button?.Tag as TagInfo;
                if (tagInfo != null)
                {
                    MenuOptionsSelected?.Invoke(tagInfo.Info, button);
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(EcuInfo info)
                {
                    Info = info;
                }

                public EcuInfo Info { get; }
            }
        }
    }
}
