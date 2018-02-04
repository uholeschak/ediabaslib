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
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using BmwDeepObd.FilePicker;
using EdiabasLib;
using System.Collections.ObjectModel;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/xml_tool_title",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                        Android.Content.PM.ConfigChanges.Orientation |
                        Android.Content.PM.ConfigChanges.ScreenSize)]
    public class XmlToolActivity : AppCompatActivity
    {
        private enum ActivityRequest
        {
            RequestSelectSgbd,
            RequestSelectDevice,
            RequestCanAdapterConfig,
            RequestSelectJobs,
            RequestYandexKey,
            RequestEdiabasTool,
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
                string mwTabFileName = null, Dictionary<long, EcuMwTabEntry> mwTabEcuDict = null)
            {
                Name = name;
                Address = address;
                Description = description;
                DescriptionTrans = null;
                Sgbd = sgbd;
                Grp = grp;
                Selected = false;
                Vin = null;
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
                ReadCommand = null;
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Description { get; set; }

            public string DescriptionTrans { get; set; }

            public string Sgbd { get; set; }

            public string Grp { get; set; }

            public string Vin { get; set; }

            public bool Selected { get; set; }

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

            public bool IgnoreXmlFile { get; set; }

            public string ReadCommand { get; set; }
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
                EcuSearchAbortIndex = -1;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                TraceActive = true;
                Vin = string.Empty;
                VehicleType = string.Empty;
            }

            public bool ForceAppend { get; set; }
            public bool AutoStart { get; set; }
            public bool AddErrorsPage { get; set; }
            public int ManualConfigIdx { get; set; }
            public int EcuSearchAbortIndex { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string TraceDir { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public string Vin { get; set; }
            public string VehicleType { get; set; }
            public bool CommErrorsOccured { get; set; }
        }

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
        private static readonly Tuple<string, string, string>[] ReadVinJobsBmwFast =
        {
            new Tuple<string, string, string>("G_ZGW", "STATUS_VIN_LESEN", "STAT_VIN"),
            new Tuple<string, string, string>("ZGW_01", "STATUS_VIN_LESEN", "STAT_VIN"),
            new Tuple<string, string, string>("G_CAS", "STATUS_FAHRGESTELLNUMMER", "STAT_FGNR17_WERT"),
            new Tuple<string, string, string>("D_CAS", "STATUS_FAHRGESTELLNUMMER", "FGNUMMER"),
        };
        private static readonly Tuple<string, string, string>[] ReadIdentJobsBmwFast =
        {
            new Tuple<string, string, string>("G_ZGW", "STATUS_VCM_GET_FA", "STAT_BAUREIHE"),
            new Tuple<string, string, string>("ZGW_01", "STATUS_VCM_GET_FA", "STAT_BAUREIHE"),
            new Tuple<string, string, string>("D_CAS", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_LM", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_KBM", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
        };
        private static readonly Tuple<string, string, string>[] ReadVinJobsDs2 =
        {
            new Tuple<string, string, string>("ZCS_ALL", "FGNR_LESEN", "FG_NR"),
            new Tuple<string, string, string>("D_0080", "AIF_FG_NR_LESEN", "AIF_FG_NR"),
            new Tuple<string, string, string>("D_0010", "AIF_LESEN", "AIF_FG_NR"),
        };
        private static readonly Tuple<string, string, string>[] ReadIdentJobsDs2 =
        {
            new Tuple<string, string, string>("FZGIDENT", "GRUNDMERKMALE_LESEN", "BR_TXT"),
            new Tuple<string, string, string>("FZGIDENT", "STRINGS_LESEN", "BR_TXT"),
        };
        private static readonly string[] ReadMotorJobsDs2 =
        {
            "D_0012", "D_MOTOR", "D_0010", "D_0013", "D_0014"
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
            new Tuple<string, string>("GenerischS22_abfragen", "0x0606"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF187"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF189"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF191"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF19E"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A2"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1A3"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1AA"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1AD"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF1DF"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF401"),
            new Tuple<string, string>("GenerischS22_abfragen", "0xF442"),
            //new Tuple<string, string>("Fehlerspeicher_loeschen", ""),
        };

        private readonly Regex _vinRegex = new Regex(@"^(?!0{7,})([a-zA-Z0-9]{7,})$");

        public const string EmptyMwTab = "-";
        public const string JobReadMwBlock = @"Messwerteblock_lesen";
        public const string JobReadMwUds = @"GenerischS22_abfragen";
        public const string JobReadStatMwBlock = @"STATUS_MESSWERTBLOCK_LESEN";
        public const string DataTypeString = @"string";
        public const string DataTypeReal = @"real";
        public const string DataTypeInteger = @"integer";
        public const string DataTypeBinary = @"binary";

        // Intent extra
        public const string ExtraInitDir = "init_dir";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraFileName = "file_name";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");

        public delegate void MwTabFileSelected(string fileName);

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _updateOptionsMenu;
        private View _barView;
        private Button _buttonRead;
        private Button _buttonSafe;
        private EcuListAdapter _ecuListAdapter;
        private TextView _textViewCarInfo;
        private string _ecuDir;
        private string _appDataDir;
        private string _lastFileName = string.Empty;
        private string _datUkdDir = string.Empty;
        private bool _activityActive;
        private volatile bool _ediabasJobAbort;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private static List<EcuInfo> _ecuList = new List<EcuInfo>();
        private bool _translateEnabled = true;
        private bool _translateActive;
        private bool _ecuListTranslated;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }
            else
            {
                _ecuList = new List<EcuInfo>();
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
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
            ListView listViewEcu = FindViewById<ListView>(Resource.Id.listEcu);
            _ecuListAdapter = new EcuListAdapter(this);
            listViewEcu.Adapter = _ecuListAdapter;
            listViewEcu.ItemClick += (sender, args) =>
            {
                int pos = args.Position;
                if (pos >= 0)
                {
                    ExecuteJobsRead(_ecuList[pos]);
                }
            };
            listViewEcu.ItemLongClick += (sender, args) =>
            {
                ShowContextMenu(args.View, args.Position);
            };

            _activityCommon = new ActivityCommon(this, () =>
            {
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

            _ecuDir = Intent.GetStringExtra(ExtraInitDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            if (!_activityRecreated)
            {
                _instanceData.DeviceName = Intent.GetStringExtra(ExtraDeviceName);
                _instanceData.DeviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            }
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _lastFileName = Intent.GetStringExtra(ExtraFileName);
            _datUkdDir = ActivityCommon.GetVagDatUkdDir(_ecuDir);
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

            EdiabasClose(_instanceData.ForceAppend);
            if (!_activityRecreated && _instanceData.ManualConfigIdx > 0)
            {
                EdiabasOpen();
                ReadAllXml();
                ExecuteUpdateEcuInfo();
            }
            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            ActivityCommon.StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_activityCommon.MtcBtService)
            {
                _activityCommon.StartMtcService();
            }
            _activityCommon.RequestUsbPermission(null);
        }

        protected override void OnResume()
        {
            base.OnResume();

            _activityActive = true;
            if (!_activityCommon.RequestEnableTranslate((sender, args) =>
            {
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
            if (_activityCommon.MtcBtService)
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
            _activityCommon.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
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
            int resourceId = Resource.String.xml_tool_msg_save_config;
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
                base.OnBackPressed();
            }))
            {
                base.OnBackPressed();
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestSelectSgbd:
                    // When FilePickerActivity returns with a file
                    if (data != null && resultCode == Android.App.Result.Ok)
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
                    if (data != null && resultCode == Android.App.Result.Ok)
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
                            ExecuteAnalyzeJob();
                        }
                    }
                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestCanAdapterConfig:
                    break;

                case ActivityRequest.RequestSelectJobs:
                    if (XmlToolEcuActivity.IntentEcuInfo.JobList != null)
                    {
                        int selectCount = XmlToolEcuActivity.IntentEcuInfo.JobList.Count(job => job.Selected);
                        XmlToolEcuActivity.IntentEcuInfo.Selected = selectCount > 0;
                        _ecuListAdapter.NotifyDataSetChanged();
                        UpdateDisplay();
                    }
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        bool callEdiabasTool = data.Extras.GetBoolean(XmlToolEcuActivity.ExtraCallEdiabasTool, false);
                        if (callEdiabasTool)
                        {
                            StartEdiabasTool(XmlToolEcuActivity.IntentEcuInfo);
                        }
                    }
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey);
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestEdiabasTool:
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.xml_tool_menu, menu);
            return true;
        }

        public override bool OnMenuOpened(int featureId, IMenu menu)
        {
            if (_updateOptionsMenu)
            {
                _updateOptionsMenu = false;
                OnPrepareOptionsMenu(menu);
            }
            return base.OnMenuOpened(featureId, menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
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
                enetIpMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_enet_ip),
                    string.IsNullOrEmpty(_activityCommon.SelectedEnetIp) ? GetString(Resource.String.select_enet_ip_auto) : _activityCommon.SelectedEnetIp));
                enetIpMenu.SetEnabled(interfaceAvailable && !commActive);
                enetIpMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet);
            }

            IMenuItem addErrorsMenu = menu.FindItem(Resource.Id.menu_xml_tool_add_errors_page);
            if (addErrorsMenu != null)
            {
                addErrorsMenu.SetEnabled(_ecuList.Count > 0);
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

            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir));

            IMenuItem translationSubmenu = menu.FindItem(Resource.Id.menu_translation_submenu);
            if (translationSubmenu != null)
            {
                translationSubmenu.SetEnabled(true);
                translationSubmenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            IMenuItem translationEnableMenu = menu.FindItem(Resource.Id.menu_translation_enable);
            if (translationEnableMenu != null)
            {
                translationEnableMenu.SetEnabled(!commActive || !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey));
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

            return base.OnPrepareOptionsMenu(menu);
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
                        int resourceId = Resource.String.xml_tool_msg_save_config;
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
                    _instanceData.AutoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice, _appDataDir);
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
                    EnetIpConfig();
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
                        UpdateOptionsMenu();
                    });
                    return true;

                case Resource.Id.menu_translation_enable:
                    if (!ActivityCommon.EnableTranslation && string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey))
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
                        StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://github.com/uholeschak/ediabaslib/blob/master/docs/Configuration_Generator.md")));
                    });
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void FinishContinue()
        {
            if (!SendTraceFile((sender, args) =>
            {
                Finish();
            }))
            {
                Finish();
            }
        }

        private void UpdateOptionsMenu()
        {
            _updateOptionsMenu = true;
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
            _instanceData.Vin = string.Empty;
            _instanceData.VehicleType = string.Empty;
        }

        private void ClearEcuList()
        {
            ClearVehicleInfo();
            _ecuList.Clear();
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

        private bool SendTraceFile(EventHandler<EventArgs> handler)
        {
            if (_instanceData.CommErrorsOccured && _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.RequestSendTraceFile(_appDataDir, _instanceData.TraceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
            }
            return false;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.SendTraceFile(_appDataDir, _instanceData.TraceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
            }
            return false;
        }

        private void UpdateDisplay()
        {
            if (ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation && string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey))
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
                        if (!string.IsNullOrEmpty(ecu.Description) && ecu.DescriptionTrans == null)
                        {
                            stringList.Add(ecu.Description);
                        }
                        if (ecu.JobList != null)
                        {
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach (XmlToolEcuActivity.JobInfo jobInfo in ecu.JobList)
                            {
                                if (jobInfo.Comments != null && jobInfo.CommentsTrans == null &&
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
                                        if (result.Comments != null && result.CommentsTrans == null)
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
                                        if (!string.IsNullOrEmpty(ecu.Description) && ecu.DescriptionTrans == null)
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
                                                if (jobInfo.Comments != null && jobInfo.CommentsTrans == null &&
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
                                                        if (result.Comments != null && result.CommentsTrans == null)
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
            string logDir = Path.Combine(_appDataDir, "LogConfigTool");
            try
            {
                Directory.CreateDirectory(logDir);
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
            if (ecuInfo.JobList == null)
            {
                return;
            }
            if (!EdiabasClose(true))
            {
                return;
            }
            XmlToolEcuActivity.IntentEcuInfo = ecuInfo;
            Intent serverIntent = new Intent(this, typeof(XmlToolEcuActivity));
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraEcuName, ecuInfo.Name);
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraEcuDir, _ecuDir);
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraTraceDir, _instanceData.TraceDir);
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraTraceAppend, _instanceData.TraceAppend || _instanceData.ForceAppend);
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectJobs);
        }

        private void EditYandexKey()
        {
            Intent serverIntent = new Intent(this, typeof(YandexKeyActivity));
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestYandexKey);
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
                EdiabasClose();
                UpdateOptionsMenu();
                SelectInterfaceEnable();
            });
        }

        private void SelectInterfaceEnable()
        {
            _activityCommon.RequestInterfaceEnable((sender, args) =>
            {
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
                    ExecuteAnalyzeJob();
                })
                .SetNegativeButton(Resource.String.button_no, (s, a) =>
                {
                    ExecuteAnalyzeJob();
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.xml_tool_clear_ecus)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
        }

        private void ShowEditMenu(View anchor)
        {
            Android.Support.V7.Widget.PopupMenu popupEdit = new Android.Support.V7.Widget.PopupMenu(this, anchor);
            popupEdit.Inflate(Resource.Menu.xml_tool_edit);
            IMenuItem detectMenuMenu = popupEdit.Menu.FindItem(Resource.Id.menu_xml_tool_edit_detect);
            detectMenuMenu?.SetVisible(ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw);
            popupEdit.MenuItemClick += (sender, args) =>
            {
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
                                        ExecuteAnalyzeJob(_instanceData.EcuSearchAbortIndex);
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
                        ExecuteAnalyzeJob();
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
            Android.Support.V7.Widget.PopupMenu popupContext = new Android.Support.V7.Widget.PopupMenu(this, anchor);
            popupContext.Inflate(Resource.Menu.xml_tool_context);
            IMenuItem moveTopMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_top);
            moveTopMenu?.SetEnabled(itemPos > 0);

            IMenuItem moveUpMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_up);
            moveUpMenu?.SetEnabled(itemPos > 0);

            IMenuItem moveDownMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_down);
            moveDownMenu?.SetEnabled((itemPos + 1) < _ecuListAdapter.Items.Count);

            IMenuItem moveBottomMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_move_bottom);
            moveBottomMenu?.SetEnabled((itemPos + 1) < _ecuListAdapter.Items.Count);

            IMenuItem ediabasToolMenu = popupContext.Menu.FindItem(Resource.Id.menu_xml_tool_ediabas_tool);
            ediabasToolMenu?.SetEnabled(itemPos >= 0 && itemPos < _ecuListAdapter.Items.Count && !IsJobRunning());

            popupContext.MenuItemClick += (sender, args) =>
            {
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
                    {
                        EcuInfo oldItem = _ecuList[itemPos - 1];
                        _ecuList[itemPos - 1] = _ecuList[itemPos];
                        _ecuList[itemPos] = oldItem;
                        UpdateDisplay();
                        break;
                    }

                    case Resource.Id.menu_xml_tool_move_down:
                    {
                        EcuInfo oldItem = _ecuList[itemPos + 1];
                        _ecuList[itemPos + 1] = _ecuList[itemPos];
                        _ecuList[itemPos] = oldItem;
                        UpdateDisplay();
                        break;
                    }

                    case Resource.Id.menu_xml_tool_move_bottom:
                    {
                        EcuInfo oldItem = _ecuList[itemPos];
                        _ecuList.RemoveAt(itemPos);
                        _ecuList.Add(oldItem);
                        UpdateDisplay();
                        break;
                    }

                    case Resource.Id.menu_xml_tool_ediabas_tool:
                    {
                        EcuInfo ecuInfo = _ecuList[itemPos];
                        StartEdiabasTool(ecuInfo);
                        break;
                    }
                }
            };
            popupContext.Show();
        }

        private void AdapterConfig()
        {
            if (!EdiabasClose())
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
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestCanAdapterConfig);
        }

        private void EnetIpConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }
            _activityCommon.SelectEnetIp((sender, args) =>
            {
                UpdateOptionsMenu();
            });
        }

        private void PerformAnalyze()
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
                    _instanceData.AutoStart = true;
                }))
                {
                    return;
                }
            }
            if (_activityCommon.ShowConnectWarning(retry =>
            {
                if (retry)
                {
                    PerformAnalyze();
                }
            }))
            {
                return;
            }
            ExecuteAnalyzeJob();
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
                _ediabasJobAbort = true;
                progress.Indeterminate = true;
                progress.ButtonAbort.Enabled = false;
                progress.SetMessage(GetString(Resource.String.xml_tool_aborting));
            };
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                int bestInvalidCount = 0;
                int bestInvalidVinCount = 0;
                List<EcuInfo> ecuListBest = null;
                string ecuFileNameBest = null;
                List<string> ecuFileNameList;

                string groupSgbd = DetectVehicleBmwFast(progress, out string detectedVin, out string vehicleType);
                _instanceData.VehicleType = vehicleType;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!string.IsNullOrEmpty(groupSgbd))
                {
                    ecuFileNameList = new List<string> { groupSgbd };
                }
                else
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Fallback to statistic approach");
                    ecuFileNameList = EcuFileNames.ToList();
                }
                if (detectedVin != null && !_ediabasJobAbort)
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
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                if (progress != null && ecuFileNameList.Count > 1)
                                {
                                    progress.Progress = 100 * localIndex / ecuFileNameList.Count;
                                }
                            });

                            _ediabas.ResolveSgbdFile(fileName);

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
                                            ecuList.Add(new EcuInfo(ecuName, ecuAdr, ecuDesc, ecuSgbd, ecuGroup));
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

                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detect result: count={0}, invalid={1}, vinok={2}", ecuList.Count, invalidEcuCount, readVinOk);
                            int invalidVinCount = readVinOk ? 0 : 1;
                            bool acceptEcu = false;
                            if (ecuListBest == null)
                            {
                                acceptEcu = true;
                            }
                            else
                            {
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
                }
                if (ecuListBest != null)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Selected Ecu file: {0}", ecuFileNameBest);
                    _ecuList.AddRange(ecuListBest.OrderBy(x => x.Name));

                    try
                    {
                        _ediabas.ResolveSgbdFile(ecuFileNameBest);
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
                                        ecuAdr = (Int64) resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("FG_NR", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        ecuVin = (string) resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("FG_NR_KURZ", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        ecuVin = (string) resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("AIF_FG_NR", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        ecuVin = (string) resultData.OpData;
                                    }
                                }
                                if (!string.IsNullOrEmpty(ecuVin) && _vinRegex.IsMatch(ecuVin))
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
                    ReadAllXml();
                }
                _ediabas.EdInterfaceClass.EnableTransmitCache = false;

                bool pin78ConnRequire = false;
                if (!_ediabasJobAbort && ecuListBest == null)
                {
                    ecuListBest = DetectVehicleDs2(progress, out detectedVin, out vehicleType, out pin78ConnRequire);
                    _instanceData.VehicleType = vehicleType;
                    if (ecuListBest != null)
                    {
                        _ecuList.AddRange(ecuListBest.OrderBy(x => x.Name));
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (!string.IsNullOrEmpty(detectedVin))
                        {
                            _instanceData.Vin = detectedVin;
                        }
                        else
                        {
                            _instanceData.Vin = GetBestVin(_ecuList);
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
                            _instanceData.CommErrorsOccured = true;
                            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth && _activityCommon.MtcBtService)
                            {
                                new AlertDialog.Builder(this)
                                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                    {
                                        _instanceData.AutoStart = false;
                                        _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice, _appDataDir);
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.xml_tool_no_response_adapter)
                                    .SetTitle(Resource.String.alert_title_warning)
                                    .Show();
                            }
                            else
                            {
                                AlertDialog altertDialog = new AlertDialog.Builder(this)
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
                                TextView messageView = altertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                                if (messageView != null)
                                {
                                    messageView.MovementMethod = new LinkMovementMethod();
                                }
                            }
                        }
                        else
                        {
                            if (pin78ConnRequire)
                            {
                                _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_msg_pin78), Resource.String.alert_title_warning);
                            }
                            else if (bestInvalidCount > 0)
                            {
                                _instanceData.CommErrorsOccured = true;
                                _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_msg_ecu_error), Resource.String.alert_title_warning);
                            }
                        }
                    }
                });
            });
            _jobThread.Start();
        }

        private string DetectVehicleBmwFast(CustomProgressDialog progress, out string detectedVin, out string detectedVehicleType)
        {
            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Try to detect vehicle BMW fast");
            detectedVin = null;
            detectedVehicleType = null;
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Progress = 0;
                    }
                });

                int jobCount = ReadVinJobsDs2.Length + ReadIdentJobsBmwFast.Length;
                int index = 0;
                foreach (Tuple<string, string, string> job in ReadVinJobsBmwFast)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read VIN job: {0}", job.Item1);
                    try
                    {
                        if (_ediabasJobAbort)
                        {
                            break;
                        }
                        int localIndex = index;
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            if (progress != null)
                            {
                                progress.Progress = 100 * localIndex / jobCount;
                            }
                        });
                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            if (detectedVin == null)
                            {
                                detectedVin = string.Empty;
                            }
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                            {
                                string vin = resultData.OpData as string;
                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!string.IsNullOrEmpty(vin) && _vinRegex.IsMatch(vin))
                                {
                                    detectedVin = vin;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected VIN: {0}", detectedVin);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        invalidSgbdSet.Add(job.Item1);
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No VIN response");
                        // ignored
                    }
                    index++;
                }

                if (_ediabasJobAbort || string.IsNullOrEmpty(detectedVin))
                {
                    return null;
                }
                string vehicleType = null;

                foreach (Tuple<string, string, string> job in ReadIdentJobsBmwFast)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read BR job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", job.Item1);
                        index++;
                        continue;
                    }
                    try
                    {
                        if (_ediabasJobAbort)
                        {
                            break;
                        }
                        bool readFa = string.Compare(job.Item2, "C_FA_LESEN", StringComparison.OrdinalIgnoreCase) == 0;
                        int localIndex = index;
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            if (progress != null)
                            {
                                progress.Progress = 100 * localIndex / jobCount;
                            }
                        });

                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                            {
                                if (readFa)
                                {
                                    string fa = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(fa))
                                    {
                                        _ediabas.ResolveSgbdFile("FA");

                                        _ediabas.ArgString = "1;" + fa;
                                        _ediabas.ArgBinaryStd = null;
                                        _ediabas.ResultsRequests = string.Empty;
                                        _ediabas.ExecuteJob("FA_STREAM2STRUCT");

                                        List<Dictionary<string, EdiabasNet.ResultData>> resultSetsFa = _ediabas.ResultSets;
                                        if (resultSetsFa != null && resultSetsFa.Count >= 2)
                                        {
                                            Dictionary<string, EdiabasNet.ResultData> resultDictFa = resultSetsFa[1];
                                            if (resultDictFa.TryGetValue("BR", out EdiabasNet.ResultData resultDataBa))
                                            {
                                                string br = resultDataBa.OpData as string;
                                                if (!string.IsNullOrEmpty(br))
                                                {
                                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected BR: {0}", br);
                                                    string vtype = VehicleInfo.GetVehicleTypeFromBrName(br, _ediabas);
                                                    if (!string.IsNullOrEmpty(vtype))
                                                    {
                                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle type: {0}", vtype);
                                                        vehicleType = vtype;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string br = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(br))
                                    {
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected BR: {0}", br);
                                        string vtype = VehicleInfo.GetVehicleTypeFromBrName(br, _ediabas);
                                        if (!string.IsNullOrEmpty(vtype))
                                        {
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle type: {0}", vtype);
                                            vehicleType = vtype;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No BR response");
                        // ignored
                    }
                    index++;
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

                if (string.IsNullOrEmpty(vehicleType))
                {
                    vehicleType = VehicleInfo.GetVehicleTypeFromVin(detectedVin, _ediabas);
                }
                detectedVehicleType = vehicleType;
                string groupSgbd = VehicleInfo.GetGroupSgbdFromVehicleType(vehicleType, _ediabas);
                if (string.IsNullOrEmpty(groupSgbd))
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No group SGBD found");
                    return null;
                }
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group SGBD: {0}", groupSgbd);
                return groupSgbd;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<EcuInfo> DetectVehicleDs2(CustomProgressDialog progress, out string detectedVin, out string detectedVehicleType, out bool pin78ConnRequire)
        {
            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Try to detect DS2 vehicle");
            detectedVin = null;
            detectedVehicleType = null;
            pin78ConnRequire = false;
            try
            {
                List<EcuInfo> ecuList = new List<EcuInfo>();
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Progress = 0;
                    }
                });

                string groupFiles = null;
                try
                {
                    _ediabas.ResolveSgbdFile("d_0044");

                    _ediabas.ArgString = "6";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("KD_DATEN_LESEN");

                    string kdData1 = null;
                    resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                        if (resultDict.TryGetValue("KD_DATEN_TEXT", out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                kdData1 = (string)resultData.OpData;
                            }
                        }
                    }

                    _ediabas.ArgString = "7";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("KD_DATEN_LESEN");

                    string kdData2 = null;
                    resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                        if (resultDict.TryGetValue("KD_DATEN_TEXT", out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                kdData2 = (string)resultData.OpData;
                            }
                        }
                    }

                    if (!_ediabasJobAbort && !string.IsNullOrEmpty(kdData1) && !string.IsNullOrEmpty(kdData2))
                    {
                        _ediabas.ResolveSgbdFile("grpliste");

                        _ediabas.ArgString = kdData1 + kdData2 + ";ja";
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("GRUPPENDATEI_ERZEUGE_LISTE_AUS_DATEN");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue("GRUPPENDATEI", out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    groupFiles = (string)resultData.OpData;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "KD group files: {0}", groupFiles);
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(groupFiles))
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "KD data empty, using fallback");
                        groupFiles = "d_0000,d_0008,d_000d,d_0010,d_0011,d_0012,d_motor,d_0013,d_0014,d_0015,d_0016,d_0020,d_0021,d_0022,d_0024,d_0028,d_002c,d_002e,d_0030,d_0032,d_0035,d_0036,d_003b,d_0040,d_0044,d_0045,d_0050,d_0056,d_0057,d_0059,d_005a,d_005b,d_0060,d_0068,d_0069,d_006a,d_006c,d_0070,d_0071,d_0072,d_007f,d_0080,d_0086,d_0099,d_009a,d_009b,d_009c,d_009d,d_009e,d_00a0,d_00a4,d_00a6,d_00a7,d_00ac,d_00b0,d_00b9,d_00bb,d_00c0,d_00c8,d_00cd,d_00d0,d_00da,d_00e0,d_00e8,d_00ed,d_00f0,d_00f5,d_00ff,d_b8_d0,,d_m60_10,d_m60_12,d_spmbt,d_spmft,d_szm,d_zke3bt,d_zke3ft,d_zke3pm,d_zke3sb,d_zke3sd,d_zke_gm,d_zuheiz,d_sitz_f,d_sitz_b,d_0047,d_0048,d_00ce,d_00ea,d_abskwp,d_0031,d_0019,d_smac,d_0081,d_xen_l,d_xen_r";
                    }
                }
                catch (Exception)
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Read KD data failed");
                    // ignored
                }

                if (!string.IsNullOrEmpty(groupFiles))
                {
                    int index = 0;
                    foreach (Tuple<string, string, string> job in ReadVinJobsDs2)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read VIN job: {0}", job.Item1);
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
                                    progress.Progress = 100 * localIndex / ReadVinJobsDs2.Length;
                                }
                            });

                            _ediabas.ResolveSgbdFile(job.Item1);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(job.Item2);

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                                {
                                    string vin = resultData.OpData as string;
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    if (!string.IsNullOrEmpty(vin) && _vinRegex.IsMatch(vin))
                                    {
                                        detectedVin = vin;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected VIN: {0}", detectedVin);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No VIN response");
                            // ignored
                        }
                        index++;
                    }
                }
                else
                {
                    int index = 0;
                    foreach (string fileName in ReadMotorJobsDs2)
                    {
                        try
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read motor job: {0}", fileName);
                            int localIndex = index;
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                if (progress != null)
                                {
                                    progress.Progress = 100 * localIndex / ReadMotorJobsDs2.Length;
                                }
                            });

                            _ediabas.ResolveSgbdFile(fileName);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob("IDENT");

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (resultDict.TryGetValue("JOB_STATUS", out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        string status = (string)resultData.OpData;
                                        if (String.Compare(status, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            groupFiles = fileName;
                                            pin78ConnRequire = true;
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Motor ECUs detected: {0}", groupFiles);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        index++;
                    }
                }

                string vehicleType = null;
                if (!string.IsNullOrEmpty(detectedVin) && detectedVin.Length == 17)
                {
                    string typeSnr = detectedVin.Substring(3, 4);
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type SNR: {0}", typeSnr);
                    foreach (Tuple<string, string, string> job in ReadIdentJobsDs2)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read vehicle type job: {0},{1}", job.Item1, job.Item2);
                        try
                        {
                            _ediabas.ResolveSgbdFile(job.Item1);

                            _ediabas.ArgString = typeSnr;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(job.Item2);

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                                {
                                    string detectedType = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(vehicleType) &&
                                        string.Compare(vehicleType, "UNBEK", StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        vehicleType = detectedType;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected Vehicle type: {0}", vehicleType);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No vehicle type response");
                            // ignored
                        }
                    }
                }

                if (!string.IsNullOrEmpty(groupFiles))
                {
                    if (!string.IsNullOrEmpty(detectedVin))
                    {
                        _instanceData.Vin = detectedVin;
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
                                alertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    waitSem.Release();
                                };
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
                                    _ediabas.ResolveSgbdFile(ecuInfo.Sgbd);

                                    _ediabas.ArgString = string.Empty;
                                    _ediabas.ArgBinaryStd = null;
                                    _ediabas.ResultsRequests = string.Empty;
                                    _ediabas.NoInitForVJobs = true;
                                    _ediabas.ExecuteJob("_VERSIONINFO");

                                    ecuInfo.Description = GetEcuComment(_ediabas.ResultSets);
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
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group files: {0}", groupFiles);
                    if (string.IsNullOrEmpty(vehicleType))
                    {
                        vehicleType = VehicleInfo.GetVehicleTypeFromVin(detectedVin, _ediabas);
                    }
                    detectedVehicleType = vehicleType;
                    ReadOnlyCollection<VehicleInfo.IEcuLogisticsEntry> ecuLogistics = VehicleInfo.GetEcuLogisticsFromVehicleType(vehicleType, _ediabas);
                    string[] groupArray = groupFiles.Split(',');
                    List<string> groupList;
                    if (ecuLogistics != null)
                    {
                        groupList = new List<string>();
                        foreach (string group in groupArray)
                        {
                            VehicleInfo.IEcuLogisticsEntry entry = VehicleInfo.GetEcuLogisticsByGroupName(ecuLogistics, group);
                            if (entry != null)
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

                            _ediabas.ResolveSgbdFile(ecuGroup);

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
                                        string ecuVin = string.Empty;
                                        if (resultDict.TryGetValue("AIF_FG_NR", out EdiabasNet.ResultData resultData))
                                        {
                                            if (resultData.OpData is string)
                                            {
                                                ecuVin = (string)resultData.OpData;
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VIN found: {0}", ecuVin);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(ecuVin) && _vinRegex.IsMatch(ecuVin))
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

        private void ExecuteAnalyzeJobVag(int searchStartIndex)
        {
            List<ActivityCommon.VagEcuEntry> ecuVagList = ActivityCommon.ReadVagEcuList(_ecuDir);
            if ((ecuVagList == null) || (ecuVagList.Count == 0))
            {
                _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_read_ecu_info_failed), Resource.String.alert_title_error);
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
                _ediabasJobAbort = true;
                progress.Indeterminate = true;
                progress.ButtonAbort.Enabled = false;
                progress.SetMessage(GetString(Resource.String.xml_tool_aborting));
            };
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);

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
                        try
                        {
                            _ediabas.ResolveSgbdFile(ecuEntry.SysName);
                        }
                        catch (Exception)
                        {
                            if (string.Compare(ecuEntry.SysName, "sch_17", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                // sch_17 is not resolving sch7000
                                _ediabas.ResolveSgbdFile("sch7000");
                            }
                            else
                            {
                                throw;
                            }
                        }

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("_JOBS");    // force to load file

                        string jobName = "Steuergeraeteversion_abfragen";
                        if (!_ediabas.IsJobExisting(jobName))
                        {
                            jobName = "Steuergeraeteversion_abfragen2";
                        }
                        EcuInfo thisEcuInfo = null;
                        _ediabas.ExecuteJob(jobName);
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
                                        string ecuName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName) ?? string.Empty;
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
                        if (ActivityCommon.CollectDebugInfo && thisEcuInfo != null)
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
                        }
                        if (ecuEntry.Address == MotorAddrVag)
                        {   // motor ECU, check communication interface
                            string sgbdFileName = _ediabas.SgbdFileName.ToUpperInvariant();
                            if (sgbdFileName.Contains("2000") || sgbdFileName.Contains("1281"))
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
                        _instanceData.CommErrorsOccured = true;
                        _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_no_response), Resource.String.alert_title_error);
                    }
                });
            });
            _jobThread.Start();
        }

        private void ExecuteJobsRead(EcuInfo ecuInfo)
        {
            EdiabasOpen();
            if (ecuInfo.JobList != null)
            {
                SelectJobs(ecuInfo);
                return;
            }
            _translateEnabled = false;

            UpdateDisplay();
            bool mwTabNotPresent = string.IsNullOrEmpty(ecuInfo.MwTabFileName) || (ecuInfo.MwTabEcuDict == null) ||
                    (!IsMwTabEmpty(ecuInfo.MwTabFileName) && !File.Exists(ecuInfo.MwTabFileName));

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            if ((ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw) &&
                mwTabNotPresent)
            {
                progress.Indeterminate = false;
                progress.Progress = 0;
                progress.Max = 100;
            }
            progress.AbortClick = sender => 
            {
                _ediabasJobAbort = true;
                progress.Indeterminate = true;
                progress.ButtonAbort.Enabled = false;
                progress.SetMessage(GetString(Resource.String.xml_tool_aborting));
            };
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                bool readFailed = false;
                try
                {
                    _ediabas.ResolveSgbdFile(ecuInfo.Sgbd);

                    _ediabas.ArgString = "ALL";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.NoInitForVJobs = true;
                    _ediabas.ExecuteJob("_JOBS");

                    List<XmlToolEcuActivity.JobInfo> jobList = new List<XmlToolEcuActivity.JobInfo>();
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
            foreach (XmlToolEcuActivity.JobInfo job in jobList)
            {
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    if (XmlToolEcuActivity.IsVagReadJob(job, ecuInfo))
                    {
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
                        foreach (long key in ecuInfo.MwTabEcuDict.Keys)
                        {
                            EcuMwTabEntry ecuMwTabEntry = ecuInfo.MwTabEcuDict[key];
                            int block = ecuMwTabEntry.BlockNumber;
                            int index = ecuMwTabEntry.ValueIndex;
                            bool entryFound = false;

                            bool udsJob = string.Compare(job.Name, JobReadMwUds, StringComparison.OrdinalIgnoreCase) == 0;
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
                    else if (string.Compare(job.Name, "Fahrgestellnr_abfragen", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        job.Comments = new List<string> { GetString(Resource.String.xml_tool_job_read_vin) };
                        job.Results.Add(new XmlToolEcuActivity.ResultInfo("Fahrgestellnr", GetString(Resource.String.xml_tool_result_vin), DataTypeString, null, null));
                    }
                    continue;
                }

                if (string.Compare(job.Name, JobReadStatMwBlock, StringComparison.OrdinalIgnoreCase) == 0)
                {   // use data from table instead of results
                    try
                    {
                        _ediabas.ArgString = "MESSWERTETAB";
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_TABLE");

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSetsTab = _ediabas.ResultSets;
                        if (resultSetsTab != null && resultSetsTab.Count >= 2)
                        {
                            int argIndex = -1;
                            int resultIndex = -1;
                            int unitIndex = -1;
                            int infoIndex = -1;
                            int dictIndex = 0;
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSetsTab)
                            {
                                if (dictIndex == 0)
                                {
                                    dictIndex++;
                                    continue;
                                }
                                string args = string.Empty;
                                string result = string.Empty;
                                string unit = string.Empty;
                                string info = string.Empty;
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("COLUMN" + i.ToString(Culture), out EdiabasNet.ResultData resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            string entry = (string) resultData.OpData;
                                            if (dictIndex == 1)
                                            {   // header
                                                if (string.Compare(entry, "ARG", StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    argIndex = i;
                                                }
                                                else if (string.Compare(entry, "EINHEIT", StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    unitIndex = i;
                                                }
                                                else if (string.Compare(entry, "RESULTNAME", StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    resultIndex = i;
                                                }
                                                else if (string.Compare(entry, "INFO", StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    infoIndex = i;
                                                }
                                            }
                                            else
                                            {
                                                if (i == argIndex)
                                                {
                                                    args = entry;
                                                }
                                                else if (i == unitIndex)
                                                {
                                                    if (entry != "-")
                                                    {
                                                        unit = entry;
                                                    }
                                                }
                                                else if (i == resultIndex)
                                                {
                                                    result = entry;
                                                }
                                                else if (i == infoIndex)
                                                {
                                                    info = entry;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(args) && !string.IsNullOrEmpty(result))
                                {
                                    string comments = info;
                                    if (!string.IsNullOrEmpty(unit))
                                    {
                                        comments += " [" + unit + "]";
                                    }
                                    job.Results.Add(new XmlToolEcuActivity.ResultInfo(result, result, DataTypeReal, args, new List <string> { comments }));
                                }
                                dictIndex++;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
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
                ecuInfo.PageName = ecuInfo.EcuName;
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

        private void ReadJobThreadDone(EcuInfo ecuInfo, CustomProgressDialog progress, bool readFailed)
        {
            progress.Dismiss();
            progress.Dispose();
            _activityCommon.SetLock(ActivityCommon.LockType.None);

            UpdateOptionsMenu();
            UpdateDisplay();
            if (_ediabasJobAbort || ecuInfo.JobList == null)
            {
                return;
            }
            if (readFailed || (ecuInfo.JobList.Count == 0))
            {
                _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_read_jobs_failed), Resource.String.alert_title_error);
            }
            else
            {
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw &&
                    ((ecuInfo.MwTabList == null) || (ecuInfo.MwTabList.Count == 0)))
                {
                    new AlertDialog.Builder(this)
                    .SetMessage(Resource.String.xml_tool_no_mwtab)
                    .SetTitle(Resource.String.alert_title_info)
                    .SetNeutralButton(Resource.String.button_ok, (s, e) =>
                    {
                        TranslateAndSelectJobs(ecuInfo);
                    })
                    .Show();
                    return;
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
                SelectJobs(ecuInfo);
            }))
            {
                SelectJobs(ecuInfo);
            }
        }

        private string GetReadCommand(EcuInfo ecuInfo)
        {
            if (ecuInfo.Sgbd.Contains("7000"))
            {
                return string.Empty;
            }
            return ecuInfo.Sgbd.Contains("1281") ? "WertEinmalLesen" : "LESEN";
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

        private List<string> GetBestMatchingMwTab(EcuInfo ecuInfo, CustomProgressDialog progress)
        {
            string readCommand = GetReadCommand(ecuInfo);
            if (string.IsNullOrEmpty(readCommand))
            {
                return GetBestMatchingMwTabUds(ecuInfo, progress);
            }

            List<ActivityCommon.MwTabFileEntry> wmTabList = ActivityCommon.GetMatchingVagMwTabs(Path.Combine(_datUkdDir, "mwtabs"), ecuInfo.Sgbd);
#if false
            SortedSet<int> mwBlocks = ActivityCommon.ExtractVagMwBlocks(wmTabList);
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
            SortedSet<int> mwIds = ActivityCommon.ExtractVagMwBlocks(wmTabList);
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
                            _ediabas.ArgString = string.Format("{0}", id);
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(JobReadMwUds);

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

        private void ExecuteUpdateEcuInfo()
        {
            _translateEnabled = false;
            EdiabasOpen();

            UpdateDisplay();
            if ((_ecuList.Count == 0) || (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.None))
            {
                _translateEnabled = true;
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
                        _ediabas.ResolveSgbdFile(ecuInfo.Sgbd);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_VERSIONINFO");

                        if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                        {
                            ecuInfo.Description = GetEcuComment(_ediabas.ResultSets);
                        }
                        string ecuName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName) ?? string.Empty;
                        if (_ecuList.Any(info => !info.Equals(ecuInfo) && string.Compare(info.Sgbd, ecuName, StringComparison.OrdinalIgnoreCase) == 0))
                        {   // already existing
                            _ecuList.Remove(ecuInfo);
                            continue;
                        }
                        string displayName = ecuName;
                        int address = 0;
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
                        _instanceData.CommErrorsOccured = true;
                        _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_read_ecu_info_failed), Resource.String.alert_title_error);
                    }
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
                ExecuteJobsRead(ecuInfo);
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
                        if (intent.GetParcelableExtra(UsbManager.ExtraDevice) is UsbDevice usbDevice)
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
                        if (intent.GetParcelableExtra(UsbManager.ExtraDevice) is UsbDevice usbDevice &&
                            EdFtdiInterface.IsValidUsbDevice(usbDevice))
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
                XElement jobNode = GetJobNode(job, ns, jobsNode);
                if (jobNode != null)
                {
                    job.Selected = true;
                    foreach (XmlToolEcuActivity.ResultInfo result in job.Results)
                    {
                        if (result.MwTabEntry != null)
                        {
                            jobNode = GetJobNode(job, ns, jobsNode, XmlToolEcuActivity.GetJobArgs(result.MwTabEntry, ecuInfo));
                            if (jobNode == null)
                            {
                                continue;
                            }
                        }
                        XElement displayNode = GetDisplayNode(result, ns, jobNode);
                        if (displayNode != null)
                        {
                            result.Selected = true;
                            XAttribute formatAttr = displayNode.Attribute("format");
                            if (formatAttr != null)
                            {
                                result.Format = formatAttr.Value;
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

        private string ReadPageSgbd(XDocument document, out JobReader.PageInfo.DisplayModeType displayMode, out DisplayFontSize fontSize,
            out int gaugesPortrait, out int gaugesLandscape,
            out string mwTabFileName, out Dictionary<long, EcuMwTabEntry> mwTabEcuDict)
        {
            displayMode = JobReader.PageInfo.DisplayModeType.List;
            fontSize = DisplayFontSize.Small;
            gaugesPortrait = JobReader.GaugesPortraitDefault;
            gaugesLandscape = JobReader.GaugesLandscapeDefault;
            mwTabFileName = null;
            mwTabEcuDict = null;
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
                    jobsNodeNew.ReplaceAttributes(from el in jobsNodeOld.Attributes() where (el.Name != "sgbd" && el.Name != "mwtab" && el.Name != "mwdata") select new XAttribute(el));
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
                        jobNodeOld = GetJobNode(job, ns, jobsNodeOld);
                        if (jobNodeOld != null)
                        {
                            jobNodeNew.ReplaceAttributes(from el in jobNodeOld.Attributes() where (el.Name != "name" && el.Name != "args") select new XAttribute(el));
                        }
                    }

                    jobNodeNew.Add(new XAttribute("name", job.Name));
                    string jobArgs = XmlToolEcuActivity.GetJobArgs(job, job.Results, ecuInfo);
                    if (!string.IsNullOrEmpty(jobArgs))
                    {
                        jobNodeNew.Add(new XAttribute("args", jobArgs));
                    }

                    int jobId = 1;
                    int lastBlockNumber = -1;
                    foreach (XmlToolEcuActivity.ResultInfo result in job.Results)
                    {
                        if (!result.Selected)
                        {
                            continue;
                        }
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
                                    jobNodeOld = GetJobNode(job, ns, jobsNodeOld, args);
                                    if (jobNodeOld != null)
                                    {
                                        jobNodeNew.ReplaceAttributes(from el in jobNodeOld.Attributes() where (el.Name != "id" && el.Name != "name" && el.Name != "args") select new XAttribute(el));
                                    }
                                }

                                jobNodeNew.Add(new XAttribute("id", (jobId++).ToString(Culture)));
                                jobNodeNew.Add(new XAttribute("name", job.Name));
                                jobNodeNew.Add(new XAttribute("args", args));
                                lastBlockNumber = result.MwTabEntry.BlockNumber;
                            }
                        }

                        XElement displayNodeOld = null;
                        XElement displayNodeNew = new XElement(ns + "display");
                        if (jobNodeOld != null)
                        {
                            displayNodeOld = GetDisplayNode(result, ns, jobNodeOld);
                            if (displayNodeOld != null)
                            {
                                displayNodeNew.ReplaceAttributes(from el in displayNodeOld.Attributes()
                                                                 where el.Name != "result" && el.Name != "format" && el.Name != "grid-type" &&
                                                                 el.Name != "min-value" && el.Name != "max-value" && el.Name != "log_tag"
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

                        string displayTag = DisplayNameJobPrefix + job.Name + "#" + result.Name;
                        if (displayNodeNew.Attribute("name") == null)
                        {
                            displayNodeNew.Add(new XAttribute("name", displayTag));
                        }
                        string resultName = result.Name;
                        if (result.MwTabEntry != null)
                        {
                            resultName = result.MwTabEntry.ValueIndex.HasValue ? string.Format(Culture, "{0}#MW_Wert", result.MwTabEntry.ValueIndexTrans) : "1#ERGEBNIS1WERT";
                        }
                        displayNodeNew.Add(new XAttribute("result", resultName));
                        displayNodeNew.Add(new XAttribute("format", result.Format));
                        if (result.GridType != JobReader.DisplayInfo.GridModeType.Hidden)
                        {
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
                    if (stringsNode != null)
                    {
                        string displayTag = DisplayNameEcuPrefix + ecuInfo.Name;
                        string displayText = GetStringEntry(displayTag, ns, stringsNode);
                        if (displayText != null)
                        {
                            ecuInfo.EcuName = displayText;
                        }
                    }
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
                    bool ecuFound = _ecuList.Any(ecuInfo => string.Compare(sgbdName, ecuInfo.Sgbd, StringComparison.OrdinalIgnoreCase) == 0);
                    if (!ecuFound)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        EcuInfo ecuInfo = new EcuInfo(sgbdName.ToUpperInvariant(), -1, string.Empty, sgbdName, string.Empty,
                            JobReader.PageInfo.DisplayModeType.List, DisplayFontSize.Small, JobReader.GaugesPortraitDefault, JobReader.GaugesLandscapeDefault, string.Empty)
                        {
                            PageName = string.Empty,
                            EcuName = string.Empty
                        };
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
                    }
                    string displayTag = DisplayNameEcuPrefix + ecuInfo.Name;
                    errorsNodeNew.Add(ecuNode);
                    ecuNode.Add(new XAttribute("name", displayTag));
                    ecuNode.Add(new XAttribute("sgbd", ecuInfo.Sgbd));

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
                        string sgbdName = ReadPageSgbd(XDocument.Load(xmlPageFile), out JobReader.PageInfo.DisplayModeType displayMode, out DisplayFontSize fontSize,
                            out int gaugesPortrait, out int gaugesLandscape,
                            out string mwTabFileName, out Dictionary<long, EcuMwTabEntry> mwTabEcuDict);
                        if (!string.IsNullOrEmpty(sgbdName))
                        {
                            _ecuList.Add(new EcuInfo(ecuName, -1, string.Empty, sgbdName, string.Empty, displayMode, fontSize, gaugesPortrait, gaugesLandscape, mwTabFileName, mwTabEcuDict)
                            {
                                Selected = true
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
                                string sgbdName = ReadPageSgbd(XDocument.Load(xmlPageFile), out JobReader.PageInfo.DisplayModeType displayMode, out DisplayFontSize fontSize,
                                    out int gaugesPortrait, out int gaugesLandscape,
                                    out string mwTabFileName, out Dictionary<long, EcuMwTabEntry> mwTabEcuDict);
                                if (!string.IsNullOrEmpty(sgbdName))
                                {
                                    _ecuList.Insert(0, new EcuInfo(ecuName, -1, string.Empty, sgbdName, string.Empty, displayMode, fontSize, gaugesPortrait, gaugesLandscape, mwTabFileName, mwTabEcuDict)
                                    {
                                        Selected = true
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
            string xmlFileName = SaveAllXml();
            if (xmlFileName == null)
            {
                _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_save_xml_failed), Resource.String.alert_title_error);
                return false;
            }
            if (!finish)
            {
                return true;
            }
            Intent intent = new Intent();
            intent.PutExtra(ExtraFileName, xmlFileName);

            // Set result and finish this Activity
            SetResult(Android.App.Result.Ok, intent);
            FinishContinue();
            return true;
        }

        private XElement GetJobNode(XmlToolEcuActivity.JobInfo job, XNamespace ns, XElement jobsNode)
        {
            return (from node in jobsNode.Elements(ns + "job")
                    let nameAttrib = node.Attribute("name")
                    where nameAttrib != null
                    where string.Compare(nameAttrib.Value, job.Name, StringComparison.OrdinalIgnoreCase) == 0 select node).FirstOrDefault();
        }

        private XElement GetJobNode(XmlToolEcuActivity.JobInfo job, XNamespace ns, XElement jobsNode, string args)
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

        private XElement GetDisplayNode(XmlToolEcuActivity.ResultInfo result, XNamespace ns, XElement jobNode)
        {
            if (result.MwTabEntry != null)
            {
                string resultName = result.MwTabEntry.ValueIndex.HasValue ? string.Format(Culture, "{0}#MW_Wert", result.MwTabEntry.ValueIndexTrans, result.Name) : "1#ERGEBNIS1WERT";
                return (from node in jobNode.Elements(ns + "display")
                        let nameAttrib = node.Attribute("result")
                        where nameAttrib != null
                        where string.Compare(nameAttrib.Value, resultName, StringComparison.OrdinalIgnoreCase) == 0
                        select node).FirstOrDefault();
            }
            return (from node in jobNode.Elements(ns + "display")
                    let nameAttrib = node.Attribute("result")
                    where nameAttrib != null
                    where string.Compare(nameAttrib.Value, result.Name, StringComparison.OrdinalIgnoreCase) == 0 select node).FirstOrDefault();
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

        private string XmlFileDir()
        {
            if (string.IsNullOrEmpty(_appDataDir))
            {
                return null;
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

        private class EcuListAdapter : BaseAdapter<EcuInfo>
        {
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
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textEcuName = view.FindViewById<TextView>(Resource.Id.textEcuName);
                TextView textEcuDesc = view.FindViewById<TextView>(Resource.Id.textEcuDesc);

                StringBuilder stringBuilderName = new StringBuilder();
                stringBuilderName.Append(!string.IsNullOrEmpty(item.EcuName) ? item.EcuName : item.Name);
                if (!string.IsNullOrEmpty(item.Description))
                {
                    stringBuilderName.Append(": ");
                    stringBuilderName.Append(!string.IsNullOrEmpty(item.DescriptionTrans)
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
                    CheckBox checkBox = (CheckBox)sender;
                    TagInfo tagInfo = (TagInfo)checkBox.Tag;
                    if (tagInfo.Info.Selected != args.IsChecked)
                    {
                        tagInfo.Info.Selected = args.IsChecked;
                        _context.EcuCheckChanged(tagInfo.Info);
                        NotifyDataSetChanged();
                    }
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
