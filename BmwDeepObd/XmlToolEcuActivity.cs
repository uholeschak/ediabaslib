using Android.Content;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using BmwDeepObd.Dialogs;
using BmwFileReader;
using EdiabasLib;
using Skydoves.BalloonLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using static Android.Util.EventLogTags;

namespace BmwDeepObd
{
    [Android.App.Activity(
        Name = ActivityCommon.AppNameSpace + "." + nameof(XmlToolEcuActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class XmlToolEcuActivity : BaseActivity, View.IOnTouchListener
    {
        public class ResultInfo
        {
            public ResultInfo(string name, string displayName, string type, string args, List<string> comments, ActivityCommon.MwTabEntry mwTabEntry = null)
            {
                Name = name;
                DisplayName = displayName;
                Type = type;
                Args = args;
                Comments = comments;
                CommentsTrans = null;
                CommentsTransRequired = true;
                MwTabEntry = mwTabEntry;
                EcuJob = null;
                EcuJobResult = null;
                Selected = false;
                GroupId = null;
                GroupSelected = false;
                GroupVisible = false;
                Format = string.Empty;
                GridType = JobReader.DisplayInfo.GridModeType.Text;
                MinValue = 0;
                MaxValue = 100;
                DisplayText = displayName;
                DisplayOrder = 0;
                LogTag = name;
            }

            public string Name { get; }

            public string NameOld { get; set; }

            public string DisplayName { get; }

            public string Type { get; }

            public string Args { get; }

            public List<string> Comments { get; }

            public ActivityCommon.MwTabEntry MwTabEntry { get; }

            public EcuFunctionStructs.EcuJob EcuJob { get; set; }

            public EcuFunctionStructs.EcuJobResult EcuJobResult { get; set; }

            public List<string> CommentsTrans { get; set; }

            public bool CommentsTransRequired { get; set; }

            public bool Selected { get; set; }

            public bool ItemSelected => Selected && !GroupVisible;

            public int? GroupId { get; set; }

            public bool GroupSelected { get; set; }

            public bool GroupVisible { get; set; }

            public string Format { get; set; }

            public JobReader.DisplayInfo.GridModeType GridType { get; set; }

            public double MinValue { get; set; }

            public double MaxValue { get; set; }

            public string DisplayText { get; set; }

            public UInt32 DisplayOrder { get; set; }

            public string LogTag { get; set; }
        }

        class ResultInfoComparer : IComparer<ResultInfo>
        {
            public int Compare(ResultInfo x, ResultInfo y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                string name1 = x.DisplayName;
                string name2 = y.DisplayName;
                if (x.EcuJob != null && y.EcuJob != null)
                {
                    if (x.EcuJob != y.EcuJob)
                    {
                        name1 = x.EcuJob.Name;
                        name2 = y.EcuJob.Name;
                    }
                }

                // ReSharper disable once StringCompareToIsCultureSpecific
                return name1.CompareTo(name2);
            }
        }

        public class JobInfo
        {
            public JobInfo(string name)
            {
                Name = name;
                DisplayName = name;
                Comments = new List<string>();
                CommentsTrans = null;
                CommentsTransRequired = true;
                Results = new List<ResultInfo>();
                ArgCount = 0;
                ArgLimit = -1;
                Selected = false;
                EcuFixedFuncStruct = null;
                EcuFuncStruct = null;
            }

            public string Name { get; }

            public string DisplayName { get; set; }

            public List<string> Comments { get; set; }

            public List<string> CommentsTrans { get; set; }

            public bool CommentsTransRequired { get; set; }

            public List<ResultInfo> Results { get; }

            public uint ArgCount { get; set; }

            public int ArgLimit { get; set; }

            public bool Selected { get; set; }

            public EcuFunctionStructs.EcuFixedFuncStruct EcuFixedFuncStruct { get; set; }

            public EcuFunctionStructs.EcuFuncStruct EcuFuncStruct { get; set; }
        }

        class JobInfoComparer : IComparer<JobInfo>
        {
            public int Compare(JobInfo x, JobInfo y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                string name1 = x.Name;
                string name2 = y.Name;
                if (x.EcuFixedFuncStruct != null && y.EcuFixedFuncStruct == null)
                {
                    return -1;
                }

                if (x.EcuFixedFuncStruct == null && y.EcuFixedFuncStruct != null)
                {
                    return 1;
                }

                if (x.EcuFixedFuncStruct != null && y.EcuFixedFuncStruct != null)
                {
                    name1 = x.DisplayName;
                    name2 = y.DisplayName;
                }

                // ReSharper disable once StringCompareToIsCultureSpecific
                return name1.CompareTo(name2);
            }
        }

        private enum ActivityRequest
        {
            RequestBmwActuator,
            RequestBmwCoding,
            RequestVagCoding,
            RequestVagAdaption,
        }

        enum FormatType
        {
            None,
            User,
            Real,
            Long,
            Double,
            Text,
        }

        public class InstanceData
        {
            public InstanceData()
            {
                ArgLimitCritical = false;
            }

            public bool ArgLimitCritical { get; set; }
        }

        public delegate void AcceptDelegate(bool accepted);

        // Intent extra
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraEcuName = "ecu_name";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraVehicleSeries = "vehicle_series";
        public const string ExtraBmwServiceFunctions = "bmw_service_functions";
        public const string ExtraSimulationDir = "simulation_dir";
        public const string ExtraTraceDir = "trace_dir";
        public const string ExtraTraceAppend = "trace_append";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraElmWifiIp = "elmwifi_ip";
        public const string ExtraDeepObdWifiIp = "deepobdwifi_ip";
        public const string ExtraEcuFuncCall = "ecu_func_call";
        // Intent results
        public const string ExtraCallEdiabasTool = "ediabas_tool";
        public const string ExtraShowBwmServiceMenu = "show_bmw_service_menu";
        private static readonly int[] LengthValues = {0, 1, 2, 3, 4, 5, 6, 8, 10, 15, 20, 25, 30, 35, 40};

        public static XmlToolActivity.EcuInfo IntentEcuInfo { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private InputMethodManager _imm;
        private View _contentView;
        private AndroidX.AppCompat.Widget.SearchView _searchView;
        private LinearLayout _layoutXmlToolEcu;
        private TextView _textViewPageNameTitle;
        private EditText _editTextPageName;
        private TextView _textViewEcuNameTitle;
        private EditText _editTextEcuName;
        private CheckBox _checkBoxDisplayTypeGrid;
        private TextView _textViewFontSizeTitle;
        private Spinner _spinnerFontSize;
        private StringObjAdapter _spinnerFontSizeAdapter;
        private TextView _textViewGridCount;
        private TextView _textViewGridCountPortraitValue;
        private EditText _editTextGridCountPortraitValue;
        private TextView _textViewGridCountLandscapeValue;
        private EditText _editTextGridCountLandscapeValue;
        private Spinner _spinnerJobs;
        private JobListAdapter _spinnerJobsAdapter;
        private CheckBox _checkBoxShowAllJobs;
        private TextView _textViewJobCommentsTitle;
        private TextView _textViewJobComments;
        private LinearLayout _layoutJobConfig;
        private Spinner _spinnerJobResults;
        private ResultListAdapter _spinnerJobResultsAdapter;
        private CheckBox _checkBoxShowAllResults;
        private TextView _textViewArgLimitTitle;
        private Spinner _spinnerArgLimit;
        private StringObjAdapter _spinnerArgLimitAdapter;
        private TextView _textViewResultCommentsTitle;
        private TextView _textViewResultComments;
        private EditText _editTextDisplayText;
        private EditText _editTextDisplayOrder;
        private TextView _textViewGridType;
        private Spinner _spinnerGridType;
        private StringObjAdapter _spinnerGridTypeAdapter;
        private TextView _textViewMinValue;
        private EditText _editTextMinValue;
        private TextView _textViewMaxValue;
        private EditText _editTextMaxValue;
        private EditText _editTextLogTag;
        private TextView _textViewFormatDot;
        private EditText _editTextFormat;
        private Spinner _spinnerFormatPos;
        private StringAdapter _spinnerFormatPosAdapter;
        private Spinner _spinnerFormatLength1;
        private StringObjAdapter _spinnerFormatLength1Adapter;
        private Spinner _spinnerFormatLength2;
        private StringObjAdapter _spinnerFormatLength2Adapter;
        private Spinner _spinnerFormatType;
        private StringObjAdapter _spinnerFormatTypeAdapter;
        private Button _buttonTestFormat;
        private TextView _textViewTestFormatOutput;
        private Button _buttonEdiabasTool;
        private Button _buttonBmwActuator;
        private Button _buttonBmwService;
        private Button _buttonBmwCoding;
        private Button _buttonCoding;
        private Button _buttonCoding2;
        private Button _buttonAdaption;
        private Button _buttonLogin;
        private Button _buttonSecurityAccess;
        private ActivityCommon _activityCommon;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private JobInfo _selectedJob;
        private ResultInfo _selectedResult;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private bool _activityActive;
        private bool _ediabasJobAbort;
        private string _appDataDir;
        private string _ecuDir;
        private string _vehicleSeries;
        private bool _bmwServiceFunctions;
        private string _simulationDir;
        private string _traceDir;
        private bool _traceAppend;
        private string _deviceAddress;
        private XmlToolActivity.EcuFunctionCallType _ecuFuncCall = XmlToolActivity.EcuFunctionCallType.None;
        private string _searchFilterText;
        private bool _filterResultsActive;
        private bool _displayEcuInfo;
        private bool _ignoreItemSelection;
        private bool _ignoreFormatSelection;

        private bool FilterResultActive
        {
            get => _filterResultsActive;
            set
            {
                if (value != _filterResultsActive)
                {
                    CloseSearchView();
                }

                _filterResultsActive = value;

            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme();
            base.OnCreate(savedInstanceState);
            _allowTitleHiding = false;
            if (savedInstanceState != null)
            {
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetDisplayShowCustomEnabled(true);
            SupportActionBar.Title = string.Format(GetString(Resource.String.xml_tool_ecu_title), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);
            SetContentView(Resource.Layout.xml_tool_ecu);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            if (IntentEcuInfo == null)
            {
                Finish();
                return;
            }

            _activityCommon = new ActivityCommon(this, () =>
            {

            }, BroadcastReceived);

            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _vehicleSeries = Intent.GetStringExtra(ExtraVehicleSeries);
            _bmwServiceFunctions = Intent.GetBooleanExtra(ExtraBmwServiceFunctions, false);
            _simulationDir = Intent.GetStringExtra(ExtraSimulationDir);
            _traceDir = Intent.GetStringExtra(ExtraTraceDir);
            _traceAppend = Intent.GetBooleanExtra(ExtraTraceAppend, true);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int) ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);
            _ecuFuncCall = (XmlToolActivity.EcuFunctionCallType)Intent.GetIntExtra(ExtraEcuFuncCall, (int)XmlToolActivity.EcuFunctionCallType.None);

            _ecuInfo = IntentEcuInfo;

            _layoutXmlToolEcu = FindViewById<LinearLayout>(Resource.Id.layoutXmlToolEcu);
            _layoutXmlToolEcu.SetOnTouchListener(this);
            _layoutXmlToolEcu.Visibility = _ecuFuncCall == XmlToolActivity.EcuFunctionCallType.None ? ViewStates.Visible : ViewStates.Gone;

            _textViewPageNameTitle = FindViewById<TextView>(Resource.Id.textViewPageNameTitle);
            _textViewPageNameTitle.SetOnTouchListener(this);

            _editTextPageName = FindViewById<EditText>(Resource.Id.editTextPageName);
            _editTextPageName.SetOnTouchListener(this);
            _editTextPageName.Text = _ecuInfo.PageName;

            _textViewEcuNameTitle = FindViewById<TextView>(Resource.Id.textViewEcuNameTitle);
            _textViewEcuNameTitle.SetOnTouchListener(this);

            _editTextEcuName = FindViewById<EditText>(Resource.Id.editTextEcuName);
            _editTextEcuName.SetOnTouchListener(this);
            _editTextEcuName.Text = _ecuInfo.EcuName;

            _checkBoxDisplayTypeGrid = FindViewById<CheckBox>(Resource.Id.checkBoxDisplayTypeGrid);
            _checkBoxDisplayTypeGrid.Checked = _ecuInfo.DisplayMode == JobReader.PageInfo.DisplayModeType.Grid;
            _checkBoxDisplayTypeGrid.CheckedChange += (sender, args) =>
            {
                DisplayTypeSelected();
            };

            _textViewFontSizeTitle = FindViewById<TextView>(Resource.Id.textViewFontSizeTitle);
            _spinnerFontSize = FindViewById<Spinner>(Resource.Id.spinnerFontSize);
            _spinnerFontSizeAdapter = new StringObjAdapter(this);
            _spinnerFontSize.Adapter = _spinnerFontSizeAdapter;
            _ignoreItemSelection = true;
            _spinnerFontSizeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_font_size_small), XmlToolActivity.DisplayFontSize.Small));
            _spinnerFontSizeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_font_size_medium), XmlToolActivity.DisplayFontSize.Medium));
            _spinnerFontSizeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_font_size_large), XmlToolActivity.DisplayFontSize.Large));
            _spinnerFontSizeAdapter.NotifyDataSetChanged();

            int fontSelection = 0;
            for (int i = 0; i < _spinnerFontSizeAdapter.Count; i++)
            {
                if ((XmlToolActivity.DisplayFontSize)_spinnerFontSizeAdapter.Items[i].Data == _ecuInfo.FontSize)
                {
                    fontSelection = i;
                }
            }
            _spinnerFontSize.SetSelection(fontSelection);
            _ignoreItemSelection = false;
            _spinnerFontSize.ItemSelected += FontItemSelected;

            _textViewGridCount = FindViewById<TextView>(Resource.Id.textViewGridCount);
            _textViewGridCountPortraitValue = FindViewById<TextView>(Resource.Id.textViewGridCountPortraitValue);
            _editTextGridCountPortraitValue = FindViewById<EditText>(Resource.Id.editTextGridCountPortraitValue);
            _textViewGridCountLandscapeValue = FindViewById<TextView>(Resource.Id.textViewGridCountLandscapeValue);
            _editTextGridCountLandscapeValue = FindViewById<EditText>(Resource.Id.editTextGridCountLandscapeValue);

            _editTextGridCountPortraitValue.Text = _ecuInfo.GaugesPortrait.ToString(CultureInfo.InvariantCulture);
            _editTextGridCountLandscapeValue.Text = _ecuInfo.GaugesLandscape.ToString(CultureInfo.InvariantCulture);

            _spinnerJobs = FindViewById<Spinner>(Resource.Id.spinnerJobs);
            _spinnerJobsAdapter = new JobListAdapter(this);
            _spinnerJobsAdapter.CheckChanged += JobCheckChanged;
            _spinnerJobs.Adapter = _spinnerJobsAdapter;
            _spinnerJobs.SetOnTouchListener(this);
            _spinnerJobs.ItemSelected += (sender, args) =>
            {
                if (_ignoreItemSelection)
                {
                    return;
                }

                HideKeyboard();
                int pos = args.Position;
                JobInfo jobInfo = null;
                if (pos >= 0 && pos < _spinnerJobsAdapter.Items.Count)
                {
                    jobInfo = _spinnerJobsAdapter.Items[pos];
                }

                JobSelected(jobInfo);
                if (_displayEcuInfo)
                {
                    DisplayEcuInfo();
                    _displayEcuInfo = false;
                }
            };

            _checkBoxShowAllJobs = FindViewById<CheckBox>(Resource.Id.checkBoxShowAllJobs);
            bool showAllJobsVisible = false;
            bool showAllJobsChecked = false;
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                if (_ecuInfo.JobList.Any(jobInfo => jobInfo.EcuFixedFuncStruct != null))
                {
                    showAllJobsVisible = true;
                }

                if (showAllJobsVisible)
                {
                    foreach (JobInfo jobInfo in _ecuInfo.JobList)
                    {
                        if (jobInfo.EcuFixedFuncStruct == null && jobInfo.Selected)
                        {
                            if (jobInfo.Results.Any(resultInfo => resultInfo.ItemSelected))
                            {
                                showAllJobsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            _checkBoxShowAllJobs.Visibility = showAllJobsVisible ? ViewStates.Visible : ViewStates.Gone;
            _checkBoxShowAllJobs.Checked = showAllJobsChecked;
            _checkBoxShowAllJobs.Click += (sender, args) =>
            {
                UpdateDisplay(true);
            };

            _layoutJobConfig = FindViewById<LinearLayout>(Resource.Id.layoutJobConfig);
            _layoutJobConfig.SetOnTouchListener(this);

            _textViewJobCommentsTitle = FindViewById<TextView>(Resource.Id.textViewJobCommentsTitle);
            _textViewJobComments = FindViewById<TextView>(Resource.Id.textViewJobComments);

            _spinnerJobResults = FindViewById<Spinner>(Resource.Id.spinnerJobResults);
            _spinnerJobResultsAdapter = new ResultListAdapter(this);
            _spinnerJobResultsAdapter.CheckChanged += ResultCheckChanged;
            _spinnerJobResults.Adapter = _spinnerJobResultsAdapter;
            _spinnerJobResults.SetOnTouchListener(this);
            _spinnerJobResults.ItemSelected += (sender, args) =>
            {
                if (_ignoreItemSelection)
                {
                    return;
                }

                //HideKeyboard();
                ResultSelected(args.Position);
            };

            _checkBoxShowAllResults = FindViewById<CheckBox>(Resource.Id.checkBoxShowAllResults);
            bool showAllResultsChecked = false;
            foreach (JobInfo jobInfo in _ecuInfo.JobList)
            {
                if (IsVagReadJob(jobInfo, _ecuInfo))
                {
                    if (jobInfo.Results.All(resultInfo => resultInfo.MwTabEntry != null && resultInfo.MwTabEntry.Dummy))
                    {
                        showAllResultsChecked = true;
                        break;
                    }

                    if (jobInfo.Selected)
                    {
                        if (jobInfo.Results.Any(resultInfo => resultInfo.ItemSelected && resultInfo.MwTabEntry != null && resultInfo.MwTabEntry.Dummy))
                        {
                            showAllResultsChecked = true;
                            break;
                        }
                    }
                }
            }
            _checkBoxShowAllResults.Checked = showAllResultsChecked;
            _checkBoxShowAllResults.Click += (sender, args) =>
            {
                JobSelected(_selectedJob);
            };

            _textViewArgLimitTitle = FindViewById<TextView>(Resource.Id.textViewArgLimitTitle);
            _spinnerArgLimit = FindViewById<Spinner>(Resource.Id.spinnerArgLimit);
            _spinnerArgLimitAdapter = new StringObjAdapter(this);
            _spinnerArgLimit.Adapter = _spinnerArgLimitAdapter;

            _ignoreItemSelection = true;
            _spinnerArgLimitAdapter.Items.Clear();
            for (int i = 0; i < 20; i++)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (i == 0)
                {
                    _spinnerArgLimitAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_arg_limit_off), i));
                }
                else
                {
                    _spinnerArgLimitAdapter.Items.Add(new StringObjType(string.Format(XmlToolActivity.Culture, "{0}", i), i));
                }
            }

            _spinnerArgLimitAdapter.NotifyDataSetChanged();
            _spinnerArgLimit.SetSelection(0);
            _ignoreItemSelection = false;

            _spinnerArgLimit.ItemSelected += (sender, args) =>
            {
                if (_ignoreItemSelection)
                {
                    return;
                }

                HideKeyboard();
                if (_selectedJob != null && _spinnerArgLimit.Visibility == ViewStates.Visible)
                {
                    int pos = args.Position;
                    if (pos >= 0 && pos < _spinnerArgLimitAdapter.Items.Count)
                    {
                        _selectedJob.ArgLimit = (int)_spinnerArgLimitAdapter.Items[pos].Data;
                    }

                    ShowArgLimitHint();
                }
            };

            _textViewResultCommentsTitle = FindViewById<TextView>(Resource.Id.textViewResultCommentsTitle);
            _textViewResultComments = FindViewById<TextView>(Resource.Id.textViewResultComments);
            _editTextDisplayText = FindViewById<EditText>(Resource.Id.editTextDisplayText);
            _editTextDisplayOrder = FindViewById<EditText>(Resource.Id.editTextDisplayOrder);

            _textViewGridType = FindViewById<TextView>(Resource.Id.textViewGridType);

            _spinnerGridType = FindViewById<Spinner>(Resource.Id.spinnerGridType);
            _spinnerGridTypeAdapter = new StringObjAdapter(this);
            _spinnerGridType.Adapter = _spinnerGridTypeAdapter;

            _textViewMinValue = FindViewById<TextView>(Resource.Id.textViewMinValue);
            _editTextMinValue = FindViewById<EditText>(Resource.Id.editTextMinValue);
            _textViewMaxValue = FindViewById<TextView>(Resource.Id.textViewMaxValue);
            _editTextMaxValue = FindViewById<EditText>(Resource.Id.editTextMaxValue);

            _editTextLogTag = FindViewById<EditText>(Resource.Id.editTextLogTag);

            _textViewFormatDot = FindViewById<TextView>(Resource.Id.textViewFormatDot);
            _editTextFormat = FindViewById<EditText>(Resource.Id.editTextFormat);

            _spinnerFormatPos = FindViewById<Spinner>(Resource.Id.spinnerFormatPos);
            _spinnerFormatPosAdapter = new StringAdapter(this);
            _spinnerFormatPos.Adapter = _spinnerFormatPosAdapter;
            _spinnerFormatPosAdapter.Items.Add(GetString(Resource.String.xml_tool_ecu_format_right));
            _spinnerFormatPosAdapter.Items.Add(GetString(Resource.String.xml_tool_ecu_format_left));
            _spinnerFormatPosAdapter.NotifyDataSetChanged();
            _spinnerFormatPos.ItemSelected += FormatItemSelected;

            _spinnerFormatLength1 = FindViewById<Spinner>(Resource.Id.spinnerFormatLength1);
            _spinnerFormatLength1Adapter = new StringObjAdapter(this);
            _spinnerFormatLength1.Adapter = _spinnerFormatLength1Adapter;
            _spinnerFormatLength1Adapter.Items.Add(new StringObjType("--", -1));
            foreach (int value in LengthValues)
            {
                _spinnerFormatLength1Adapter.Items.Add(new StringObjType(value.ToString(), value));
            }
            _spinnerFormatLength1Adapter.NotifyDataSetChanged();
            _spinnerFormatLength1.ItemSelected += FormatItemSelected;

            _spinnerFormatLength2 = FindViewById<Spinner>(Resource.Id.spinnerFormatLength2);
            _spinnerFormatLength2Adapter = new StringObjAdapter(this);
            _spinnerFormatLength2.Adapter = _spinnerFormatLength2Adapter;
            _spinnerFormatLength2Adapter.Items.Add(new StringObjType("--", -1));
            foreach (int value in LengthValues)
            {
                _spinnerFormatLength2Adapter.Items.Add(new StringObjType(value.ToString(), value));
            }
            _spinnerFormatLength2Adapter.NotifyDataSetChanged();
            _spinnerFormatLength2.ItemSelected += FormatItemSelected;

            _spinnerFormatType = FindViewById<Spinner>(Resource.Id.spinnerFormatType);
            _spinnerFormatTypeAdapter = new StringObjAdapter(this);
            _spinnerFormatType.Adapter = _spinnerFormatTypeAdapter;
            _spinnerFormatTypeAdapter.Items.Add(new StringObjType("--", FormatType.None));
            _spinnerFormatTypeAdapter.NotifyDataSetChanged();
            _spinnerFormatType.ItemSelected += FormatItemSelected;

            _buttonTestFormat = FindViewById<Button>(Resource.Id.buttonTestFormat);
            _buttonTestFormat.Click += (sender, args) =>
            {
                ExecuteTestFormat();
            };
            _textViewTestFormatOutput = FindViewById<TextView>(Resource.Id.textViewTestFormatOutput);

            _buttonEdiabasTool = FindViewById<Button>(Resource.Id.buttonEdiabasTool);
            _buttonEdiabasTool.Enabled = true;
            _buttonEdiabasTool.Click += (sender, args) =>
            {
                Intent intent = new Intent();
                intent.PutExtra(ExtraCallEdiabasTool, true);
                SetResult(Android.App.Result.Ok, intent);
                StoreResults();
                Finish();
            };

            ViewStates bmwButtonsVisibility = ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw?
                ViewStates.Visible : ViewStates.Gone;

            bool bmwActuatorEnabled = ControlActuatorCount(_ecuInfo) > 0;
            _buttonBmwActuator = FindViewById<Button>(Resource.Id.buttonBmwActuator);
            _buttonBmwActuator.Visibility = bmwButtonsVisibility;
            _buttonBmwActuator.Enabled = bmwActuatorEnabled;
            _buttonBmwActuator.Click += (sender, args) =>
            {
                StartBmwActuator();
            };

            bool bmwServiceEnabled = false;
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                bmwServiceEnabled = _bmwServiceFunctions;
            }

            _buttonBmwService = FindViewById<Button>(Resource.Id.buttonBmwService);
            _buttonBmwService.Visibility = bmwButtonsVisibility;
            _buttonBmwService.Enabled = bmwServiceEnabled;
            _buttonBmwService.Click += (sender, args) =>
            {
                Intent intent = new Intent();
                intent.PutExtra(ExtraShowBwmServiceMenu, true);
                SetResult(Android.App.Result.Ok, intent);
                StoreResults();
                Finish();
            };

            bool bmwCodingEnabled = false;
            if (_activityCommon.IsBmwCodingInterface(_deviceAddress))
            {
                if (ActivityCommon.IsBmwCodingSeries(_vehicleSeries))
                {
                    bmwCodingEnabled = true;
                }
            }

            _buttonBmwCoding = FindViewById<Button>(Resource.Id.buttonBmwCoding);
            _buttonBmwCoding.Visibility = bmwButtonsVisibility;
            _buttonBmwCoding.Enabled = bmwCodingEnabled;
            _buttonBmwCoding.Click += (sender, args) =>
            {
                StartBmwCoding();
            };

            bool vagCodingEnabled = _ecuInfo.HasVagCoding();
            ViewStates vagButtonsVisibility = ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && ActivityCommon.VagUdsActive ?
                ViewStates.Visible : ViewStates.Gone;
            _buttonCoding = FindViewById<Button>(Resource.Id.buttonCoding);
            _buttonCoding.Visibility = vagButtonsVisibility;
            _buttonCoding.Enabled = vagCodingEnabled;
            _buttonCoding.Click += (sender, args) =>
            {
                StartVagCoding(VagCodingActivity.CodingMode.Coding);
            };

            bool vagCoding2Enabled = false;
            if (_ecuInfo.HasVagCoding2())
            {
                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_ecuInfo.VagDataFileName);
                if (udsReader != null)
                {
                    List<UdsFileReader.DataReader.DataInfo> dataInfoCodingList = udsReader.DataReader.ExtractDataType(_ecuInfo.VagDataFileName, UdsFileReader.DataReader.DataType.Login);
                    if (dataInfoCodingList?.Count > 0)
                    {
                        vagCoding2Enabled = true;
                    }
                }
            }

            bool vagAdaptionEnabled = false;
            if (XmlToolActivity.Is1281Ecu(_ecuInfo))
            {
                vagAdaptionEnabled = true;
            }
            else if (XmlToolActivity.IsUdsEcu(_ecuInfo))
            {
                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_ecuInfo.VagUdsFileName);
                if (udsReader != null)
                {
                    List<UdsFileReader.UdsReader.ParseInfoAdp> parseInfoAdaptionList = udsReader.GetAdpParseInfoList(_ecuInfo.VagUdsFileName);
                    if (parseInfoAdaptionList?.Count > 0)
                    {
                        vagAdaptionEnabled = true;
                    }
                }
            }
            else
            {
                if (_ecuInfo.VagSupportedFuncHash != null)
                {
                    vagAdaptionEnabled =
                        _ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.Adaption) ||
                        _ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.AdaptionLong) ||
                        _ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.AdaptionLong2);
                }
            }

            _buttonCoding2 = FindViewById<Button>(Resource.Id.buttonCoding2);
            _buttonCoding2.Visibility = vagButtonsVisibility;
            _buttonCoding2.Enabled = vagCoding2Enabled;
            _buttonCoding2.Click += (sender, args) =>
            {
                StartVagCoding(VagCodingActivity.CodingMode.Coding2);
            };

            _buttonAdaption = FindViewById<Button>(Resource.Id.buttonAdaption);
            _buttonAdaption.Visibility = vagButtonsVisibility;
            _buttonAdaption.Enabled = vagAdaptionEnabled;
            _buttonAdaption.Click += (sender, args) =>
            {
                StartVagAdaption();
            };

            bool vagLoginEnabled = _ecuInfo.HasVagLogin();
            _buttonLogin = FindViewById<Button>(Resource.Id.buttonLogin);
            _buttonLogin.Visibility = vagButtonsVisibility;
            _buttonLogin.Enabled = vagLoginEnabled;
            _buttonLogin.Click += (sender, args) =>
            {
                StartVagCoding(VagCodingActivity.CodingMode.Login);
            };

            bool vagAuthEnabled = !XmlToolActivity.Is1281Ecu(_ecuInfo);
            _buttonSecurityAccess = FindViewById<Button>(Resource.Id.buttonSecurityAccess);
            _buttonSecurityAccess.Visibility = vagButtonsVisibility;
            _buttonSecurityAccess.Enabled = vagAuthEnabled;
            _buttonSecurityAccess.Click += (sender, args) =>
            {
                StartVagCoding(VagCodingActivity.CodingMode.SecurityAccess);
            };

            _layoutJobConfig.Visibility = ViewStates.Gone;

            if (_ecuFuncCall != XmlToolActivity.EcuFunctionCallType.None)
            {
                switch (_ecuFuncCall)
                {
                    case XmlToolActivity.EcuFunctionCallType.BmwActuator:
                        if (bmwActuatorEnabled && StartBmwActuator())
                        {
                            return;
                        }
                        break;

                    case XmlToolActivity.EcuFunctionCallType.VagCoding:
                        if (vagCodingEnabled && StartVagCoding(VagCodingActivity.CodingMode.Coding))
                        {
                            return;
                        }
                        break;

                    case XmlToolActivity.EcuFunctionCallType.VagCoding2:
                        if (vagCoding2Enabled && StartVagCoding(VagCodingActivity.CodingMode.Coding2))
                        {
                            return;
                        }
                        break;

                    case XmlToolActivity.EcuFunctionCallType.VagAdaption:
                        if (vagAdaptionEnabled && StartVagAdaption())
                        {
                            return;
                        }
                        break;

                    case XmlToolActivity.EcuFunctionCallType.VagLogin:
                        if (vagLoginEnabled && StartVagCoding(VagCodingActivity.CodingMode.Login))
                        {
                            return;
                        }
                        break;

                    case XmlToolActivity.EcuFunctionCallType.VagSecAccess:
                        if (vagAuthEnabled && StartVagCoding(VagCodingActivity.CodingMode.SecurityAccess))
                        {
                            return;
                        }
                        break;
                }

                AlertDialog.Builder builder = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                        Finish();
                    })
                    .SetMessage(Resource.String.xml_tool_ecu_msg_func_not_avail)
                    .SetTitle(Resource.String.alert_title_error);
                AlertDialog alertDialog = builder.Show();
                if (alertDialog != null)
                {
                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        Finish();
                    };
                }
                return;
            }

            UpdateDisplay();
            DisplayTypeSelected();
            ResetTestResult();
            DisplayEcuInfo();
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
        }

        protected override void OnPause()
        {
            base.OnPause();
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
                _jobThread?.Join();
            }
            EdiabasClose();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressedEvent()
        {
            UpdateResultSettings(_selectedResult);
            NoSelectionWarn(accepted =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (accepted)
                {
                    StoreResults();
                    base.OnBackPressedEvent();
                }
            });
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest) requestCode)
            {
                case ActivityRequest.RequestBmwActuator:
                case ActivityRequest.RequestBmwCoding:
                case ActivityRequest.RequestVagCoding:
                case ActivityRequest.RequestVagAdaption:
                    if (resultCode == Android.App.Result.Ok || _ecuFuncCall != XmlToolActivity.EcuFunctionCallType.None)
                    {
                        Finish();
                        break;
                    }
                    EdiabasClose();
                    UpdateDisplay();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.xml_ecu_tool_menu, menu);
            IMenuItem menuSearch = menu.FindItem(Resource.Id.action_search);
            if (menuSearch != null)
            {
                menuSearch.SetActionView(new AndroidX.AppCompat.Widget.SearchView(this));

                if (menuSearch.ActionView is AndroidX.AppCompat.Widget.SearchView searchViewV7)
                {
                    _searchView = searchViewV7;
                    searchViewV7.QueryTextChange += (sender, e) =>
                    {
                        e.Handled = OnQueryTextChange(e.NewText, false);
                    };

                    searchViewV7.QueryTextSubmit += (sender, e) =>
                    {
                        e.Handled = OnQueryTextChange(e.NewText, true);
                    };
                }
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            UpdateResultSettings(_selectedResult);
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    NoSelectionWarn(accepted =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (accepted)
                        {
                            StoreResults();
                            Finish();
                        }
                    });
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool IsFinishAllowed()
        {
            if (IsJobRunning())
            {
                return false;
            }
            return true;
        }

        public override void CloseSearchView()
        {
            if (_searchView != null && !_searchView.Iconified)
            {
                _searchView.Iconified = true;
                _searchView.Iconified = true;
            }

            _filterResultsActive = false;
            _searchFilterText = string.Empty;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (v == _textViewPageNameTitle || v == _editTextPageName ||
                        v == _textViewEcuNameTitle || v == _editTextEcuName)
                    {
                        DisplayEcuInfo();
                        break;
                    }

                    if (v == _spinnerJobs)
                    {
                        FilterResultActive = false;
                    }

                    if (v == _spinnerJobResults)
                    {
                        FilterResultActive = true;
                    }

                    UpdateResultSettings(_selectedResult);
                    HideKeyboard();
                    break;
            }
            return false;
        }

        private bool OnQueryTextChange(string text, bool submit)
        {
            if (string.Compare(_searchFilterText, text, StringComparison.Ordinal) != 0)
            {
                _searchFilterText = text;

                if (FilterResultActive)
                {
                    JobSelected(_selectedJob);
                }
                else
                {
                    UpdateDisplay(true);
                }
            }

            if (submit)
            {
                HideKeyboard(true);
            }
            return true;
        }

        public static bool IsVagReadJob(JobInfo job, XmlToolActivity.EcuInfo ecuInfo)
        {
            if (job == null)
            {
                return false;
            }
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                return false;
            }
            if (XmlToolActivity.IsUdsEcu(ecuInfo))
            {
                return string.Compare(job.Name, XmlToolActivity.JobReadS22Uds, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return string.Compare(job.Name, XmlToolActivity.JobReadMwBlock, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsBmwReadStatusJob(JobInfo job)
        {
            if (job == null)
            {
                return false;
            }
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return false;
            }
            return string.Compare(job.Name, XmlToolActivity.JobReadStat, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsBmwReadStatusBlockJob(JobInfo job)
        {
            if (job == null)
            {
                return false;
            }
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return false;
            }
            return string.Compare(job.Name, XmlToolActivity.JobReadStatBlock, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsBmwReadStatusMwBlockJob(JobInfo job)
        {
            if (job == null)
            {
                return false;
            }
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return false;
            }
            return string.Compare(job.Name, XmlToolActivity.JobReadStatMwBlock, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsBmwReadStatusTypeJob(JobInfo job)
        {
            return IsBmwReadStatusJob(job) || IsBmwReadStatusBlockJob(job) || IsBmwReadStatusMwBlockJob(job);
        }

        public static bool IsValidJob(JobInfo job, XmlToolActivity.EcuInfo ecuInfo, RuleEvalBmw ruleEvalBmw = null)
        {
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                if (IsVagReadJob(job, ecuInfo))
                {
                    return true;
                }
                if (string.Compare(job.Name, XmlToolActivity.JobReadVin, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                return false;
            }

            if (job.EcuFixedFuncStruct != null)
            {
                if (job.EcuFixedFuncStruct.EcuJobList == null || job.EcuFixedFuncStruct.EcuJobList.Count == 0)
                {
                    return false;
                }

                EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType nodeClassType = job.EcuFixedFuncStruct.GetNodeClassType();
                if (nodeClassType != EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator)
                {
                    bool validId = ruleEvalBmw == null || ruleEvalBmw.EvaluateRule(job.EcuFixedFuncStruct.Id, RuleEvalBmw.RuleType.EcuFunc);
                    if (validId)
                    {
                        foreach (EcuFunctionStructs.EcuJob ecuJob in job.EcuFixedFuncStruct.EcuJobList)
                        {
                            foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJob.EcuJobResultList)
                            {
                                if (ecuJobResult.EcuFuncRelevant.ConvertToInt() > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            if (IsBmwReadStatusTypeJob(job))
            {
                return true;
            }

            if (string.Compare(job.Name, "FS_LESEN", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(job.Name, "IS_LESEN", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(job.Name, "AIF_LESEN", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            bool validResult = false;
            foreach (ResultInfo result in job.Results)
            {
                if (result.Name.EndsWith("_WERT", StringComparison.OrdinalIgnoreCase))
                {
                    validResult = true;
                }
                if (result.Name.StartsWith("STAT_", StringComparison.OrdinalIgnoreCase) || result.Name.StartsWith("STATUS_", StringComparison.OrdinalIgnoreCase))
                {
                    validResult = true;
                }
            }
            if (job.Name.StartsWith("STAT_", StringComparison.OrdinalIgnoreCase) ||
                job.Name.StartsWith("STATUS_", StringComparison.OrdinalIgnoreCase))
            {
                validResult = true;
            }
            return job.ArgCount == 0 && validResult;
        }

        public static int ControlActuatorCount(XmlToolActivity.EcuInfo ecuInfo)
        {
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return -1;
            }

            int count = 0;
            if (ecuInfo?.JobList != null)
            {
                foreach (JobInfo jobInfo in ecuInfo.JobList)
                {
                    if (jobInfo.EcuFixedFuncStruct != null &&
                        jobInfo.EcuFixedFuncStruct.GetNodeClassType() == EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static string GetJobArgs(ActivityCommon.MwTabEntry mwTabEntry, XmlToolActivity.EcuInfo ecuInfo)
        {
            if (string.IsNullOrEmpty(ecuInfo.ReadCommand))
            {
                return string.Format(XmlToolActivity.Culture, "{0}", mwTabEntry.BlockNumber);
            }
            return string.Format(XmlToolActivity.Culture, "{0};{1}", mwTabEntry.BlockNumber, ecuInfo.ReadCommand);
        }

        public static string GetJobArgs(JobInfo job, List<ResultInfo> resultInfoList, XmlToolActivity.EcuInfo ecuInfo, out string jobArgs2, bool selectAll = false)
        {
            jobArgs2 = string.Empty;
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return string.Empty;
            }

            string argHead = string.Empty;
            string argHead2 = string.Empty;
            if (IsBmwReadStatusMwBlockJob(job))
            {
                argHead = "JA";
            }
            else if (IsBmwReadStatusBlockJob(job))
            {
                argHead = "10;JA;ARG";
                argHead2 = "10;NEIN;ARG";
            }
            else if (IsBmwReadStatusJob(job))
            {
                argHead = "ARG";
            }

            if (!string.IsNullOrEmpty(argHead))
            {
                HashSet<string> argHashSet = new HashSet<string>();
                StringBuilder sb = new StringBuilder();
                foreach (ResultInfo resultInfo in resultInfoList)
                {
                    string arg = resultInfo.Args;
                    if ((selectAll || resultInfo.ItemSelected) && !string.IsNullOrEmpty(arg))
                    {
                        if (!argHashSet.Contains(arg))
                        {
                            argHashSet.Add(arg);
                            sb.Append(";");
                            sb.Append(arg);
                        }
                    }
                }

                if (sb.Length == 0)
                {
                    return string.Empty;
                }

                if (!string.IsNullOrEmpty(argHead2))
                {
                    jobArgs2 = argHead2 + sb;
                }

                return argHead + sb;
            }

            return string.Empty;
        }

        public static string FormatResult(EdiabasNet.ResultData resultData, string format)
        {
            if (resultData.OpData.GetType() == typeof(byte[]))
            {
                StringBuilder sb = new StringBuilder();
                byte[] data = (byte[]) resultData.OpData;
                foreach (byte value in data)
                {
                    sb.Append(string.Format(XmlToolActivity.Culture, "{0:X02} ", value));
                }
                return sb.ToString();
            }
            return EdiabasNet.FormatResult(resultData, format) ?? string.Empty;
        }

        public static void AppendSbText(StringBuilder sb, string text)
        {
            EdiabasToolActivity.AppendSbText(sb, text);
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
                ActivityCommon.SetEdiabasConfigProperties(_ediabas, _traceDir, _simulationDir, _traceAppend);
            }

            _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress, _appDataDir);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool EdiabasClose()
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
            return true;
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

        private void UpdateDisplay(bool filterJobs = false)
        {
            int selection = -1;

            _spinnerJobsAdapter.Items.Clear();
            List<JobInfo> jobListSort = new List<JobInfo>(_ecuInfo.JobList);
            jobListSort.Sort(new JobInfoComparer());
            foreach (JobInfo job in jobListSort)
            {
                if (IsValidJob(job, _ecuInfo))
                {
                    bool addJob = true;
                    if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                    {
                        if (_checkBoxShowAllJobs.Visibility == ViewStates.Visible && !_checkBoxShowAllJobs.Checked)
                        {
                            if (job.EcuFixedFuncStruct == null)
                            {
                                addJob = false;
                            }
                        }
                    }

                    if (filterJobs && !FilterResultActive && !string.IsNullOrWhiteSpace(_searchFilterText))
                    {
                        if (!IsSearchFilterMatching(job.DisplayName, _searchFilterText))
                        {
                            addJob = false;
                        }
                    }

                    if (addJob)
                    {
                        foreach (ResultInfo resultInfo in job.Results)
                        {
                            resultInfo.GroupSelected = false;
                        }

                        _spinnerJobsAdapter.Items.Add(job);
                        if (selection < 0)
                        {
                            if (job.Selected)
                            {
                                selection = _spinnerJobsAdapter.Items.Count - 1;
                            }
                        }

                        if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                        {
                            if (selection < 0 && IsVagReadJob(job, _ecuInfo))
                            {
                                selection = _spinnerJobsAdapter.Items.Count - 1;
                            }
                        }
                    }
                }
            }

            _ignoreItemSelection = true;
            _spinnerJobsAdapter.NotifyDataSetChanged();
            _ignoreItemSelection = false;
            if (_spinnerJobsAdapter.Items.Count > 0)
            {
                if (filterJobs)
                {
                    if (_selectedJob != null)
                    {
                        int selectedIndex = _spinnerJobsAdapter.Items.IndexOf(_selectedJob);
                        if (selectedIndex >= 0)
                        {
                            _ignoreItemSelection = true;
                            _spinnerJobs.SetSelection(selectedIndex);
                            _ignoreItemSelection = false;
                        }
                    }
                }
                else
                {
                    _ignoreItemSelection = true;
                    int selectedIndex = selection < 0 ? 0 : selection;
                    _spinnerJobs.SetSelection(selectedIndex);
                    _ignoreItemSelection = false;
                }

                int selPos = _spinnerJobs.SelectedItemPosition;
                if (selPos >= 0 && selPos < _spinnerJobsAdapter.Items.Count)
                {
                    JobSelected(_spinnerJobsAdapter.Items[selPos]);
                }
            }
            else
            {
                JobSelected(null);
            }
        }

        private void ResetTestResult()
        {
            _textViewTestFormatOutput.Text = string.Empty;
            _buttonTestFormat.Enabled = (_selectedJob != null) && (_selectedResult != null);
        }

        public static bool IsResultBinary(ResultInfo resultInfo)
        {
            return string.Compare(resultInfo.Type, XmlToolActivity.DataTypeBinary, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsResultString(ResultInfo resultInfo)
        {
            return string.Compare(resultInfo.Type, XmlToolActivity.DataTypeString, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private void UpdateFormatFields(ResultInfo resultInfo, bool userFormat, bool initialCall = false)
        {
            string format = resultInfo.Format;
            string parseString = format;
            Int32 length1 = -1;
            Int32 length2 = -1;
            char convertType = '\0';
            bool leftAlign = false;
            if (!string.IsNullOrEmpty(parseString))
            {
                if (parseString[0] == '-')
                {
                    leftAlign = true;
                    parseString = parseString.Substring(1);
                }
            }
            if (!string.IsNullOrEmpty(parseString))
            {
                convertType = parseString[parseString.Length - 1];
                parseString = parseString.Remove(parseString.Length - 1, 1);
            }
            if (!string.IsNullOrEmpty(parseString))
            {
                string[] words = parseString.Split('.');
                try
                {
                    if (words.Length > 0)
                    {
                        if (words[0].Length > 0)
                        {
                            length1 = Convert.ToInt32(words[0], 10);
                        }
                    }
                    if (words.Length > 1)
                    {
                        if (words[1].Length > 0)
                        {
                            length2 = Convert.ToInt32(words[1], 10);
                        }
                    }
                }
                catch (Exception)
                {
                    length1 = -1;
                    length2 = -1;
                }
            }

            _ignoreFormatSelection = true;

            bool resultBinary = IsResultBinary(resultInfo);
            bool resultString = IsResultString(resultInfo);

            _spinnerFormatTypeAdapter.Items.Clear();
            _spinnerFormatTypeAdapter.Items.Add(new StringObjType("--", FormatType.None));
            if (!resultBinary)
            {
                _spinnerFormatTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_user_format), FormatType.User));
                if (!resultString)
                {
                    _spinnerFormatTypeAdapter.Items.Add(new StringObjType("(R)eal", FormatType.Real));
                    _spinnerFormatTypeAdapter.Items.Add(new StringObjType("(L)ong", FormatType.Long));
                    _spinnerFormatTypeAdapter.Items.Add(new StringObjType("(D)ouble", FormatType.Double));
                }
                _spinnerFormatTypeAdapter.Items.Add(new StringObjType("(T)ext", FormatType.Text));
            }
            _spinnerFormatTypeAdapter.NotifyDataSetChanged();

            FormatType formatType = FormatType.User;
            switch (convertType)
            {
                case '\0':
                    formatType = FormatType.None;
                    break;

                case 'R':
                    formatType = FormatType.Real;
                    break;

                case 'L':
                    formatType = FormatType.Long;
                    break;

                case 'D':
                    formatType = FormatType.Double;
                    break;

                case 'T':
                    formatType = FormatType.Text;
                    break;
            }
            if (userFormat)
            {
                formatType = FormatType.User;
            }

            int selection = 0;
            for (int i = 0; i < _spinnerFormatTypeAdapter.Count; i++)
            {
                if ((FormatType)_spinnerFormatTypeAdapter.Items[i].Data == formatType)
                {
                    selection = i;
                }
            }
            _spinnerFormatType.SetSelection(selection);

            if (selection > 0)
            {
                _spinnerFormatPos.Enabled = true;
                _spinnerFormatPos.SetSelection(leftAlign ? 1 : 0);

                int index1 = 0;
                for (int i = 0; i < _spinnerFormatLength1Adapter.Count; i++)
                {
                    if ((int)_spinnerFormatLength1Adapter.Items[i].Data == length1)
                    {
                        index1 = i;
                    }
                }
                _spinnerFormatLength1.Enabled = true;
                _spinnerFormatLength1.SetSelection(index1);

                int index2 = 0;
                for (int i = 0; i < _spinnerFormatLength2Adapter.Count; i++)
                {
                    if ((int)_spinnerFormatLength2Adapter.Items[i].Data == length2)
                    {
                        index2 = i;
                    }
                }
                _spinnerFormatLength2.Enabled = true;
                _spinnerFormatLength2.SetSelection(index2);
            }
            else
            {
                _spinnerFormatPos.Enabled = false;
                _spinnerFormatPos.SetSelection(0);

                _spinnerFormatLength1.Enabled = false;
                _spinnerFormatLength1.SetSelection(0);

                _spinnerFormatLength2.Enabled = false;
                _spinnerFormatLength2.SetSelection(0);
            }

            if (initialCall)
            {
                if (GetFormatString() != format)
                {
                    selection = 1;
                    _spinnerFormatType.SetSelection(selection);
                }
            }
            _editTextFormat.Text = format;
            _ignoreFormatSelection = false;

            ViewStates viewState;
            if (selection == 1)
            {
                _editTextFormat.Visibility = ViewStates.Visible;
                viewState = ViewStates.Gone;
            }
            else
            {
                _editTextFormat.Visibility = ViewStates.Gone;
                viewState = ViewStates.Visible;
            }
            _spinnerFormatPos.Visibility = viewState;
            _spinnerFormatLength1.Visibility = viewState;
            _textViewFormatDot.Visibility = viewState;
            _spinnerFormatLength2.Visibility = viewState;
        }

        private string GetFormatString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            FormatType formatType = FormatType.None;
            int formatIndex = _spinnerFormatType.SelectedItemPosition;
            if (formatIndex >= 0 && formatIndex < _spinnerFormatTypeAdapter.Items.Count)
            {
                formatType = (FormatType)_spinnerFormatTypeAdapter.Items[formatIndex].Data;
            }

            string convertType = string.Empty;
            switch (formatType)
            {
                case FormatType.User:
                    stringBuilder.Append(_editTextFormat.Text);
                    break;

                case FormatType.Real:
                    convertType = "R";
                    break;

                case FormatType.Long:
                    convertType = "L";
                    break;

                case FormatType.Double:
                    convertType = "D";
                    break;

                case FormatType.Text:
                    convertType = "T";
                    break;
            }
            if (!string.IsNullOrEmpty(convertType))
            {
                if (_spinnerFormatPos.SelectedItemPosition > 0)
                {
                    stringBuilder.Append("-");
                }

                int format1Index = _spinnerFormatLength1.SelectedItemPosition;
                if (format1Index >= 0 && format1Index < _spinnerFormatLength1Adapter.Items.Count)
                {
                    int value = (int) _spinnerFormatLength1Adapter.Items[format1Index].Data;
                    if (value >= 0)
                    {
                        stringBuilder.Append(value.ToString());
                    }
                }

                int format2Index = _spinnerFormatLength2.SelectedItemPosition;
                if (format2Index >= 0 && format2Index < _spinnerFormatLength2Adapter.Count)
                {
                    int value = (int)_spinnerFormatLength2Adapter.Items[format2Index].Data;
                    if (value >= 0)
                    {
                        stringBuilder.Append(".");
                        stringBuilder.Append(value.ToString());
                    }
                }
                stringBuilder.Append(convertType);
            }

            return stringBuilder.ToString();
        }

        private void UpdateResultSettings(ResultInfo resultInfo)
        {
            if (resultInfo != null)
            {
                resultInfo.DisplayText = _editTextDisplayText.Text;
                if (UInt32.TryParse(_editTextDisplayOrder.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 displayOrder))
                {
                    resultInfo.DisplayOrder = displayOrder;
                }

                int typeIndex = _spinnerGridType.SelectedItemPosition;
                if (typeIndex >= 0 && typeIndex < _spinnerGridTypeAdapter.Items.Count)
                {
                    _selectedResult.GridType = (JobReader.DisplayInfo.GridModeType)_spinnerGridTypeAdapter.Items[typeIndex].Data;
                }
                if (Double.TryParse(_editTextMinValue.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double minValue))
                {
                    resultInfo.MinValue = minValue;
                }
                if (Double.TryParse(_editTextMaxValue.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double maxValue))
                {
                    resultInfo.MaxValue = maxValue;
                }
                resultInfo.LogTag = _editTextLogTag.Text;
            }
            UpdateFormatString(resultInfo);
        }

        private void UpdateFormatString(ResultInfo resultInfo)
        {
            if (resultInfo == null)
            {
                return;
            }
            resultInfo.Format = GetFormatString();

            FormatType formatType = FormatType.None;
            int formatIndex = _spinnerFormatType.SelectedItemPosition;
            if (formatIndex >= 0 && formatIndex < _spinnerFormatTypeAdapter.Items.Count)
            {
                formatType = (FormatType)_spinnerFormatTypeAdapter.Items[formatIndex].Data;
            }
            UpdateFormatFields(resultInfo, formatType == FormatType.User);
        }

        private bool AnyResultsSelected(bool checkGrid)
        {
            bool gridMode = checkGrid && _checkBoxDisplayTypeGrid.Checked;
            foreach (JobInfo jobInfo in _ecuInfo.JobList)
            {
                if (jobInfo.Selected)
                {
                    if (gridMode)
                    {
                        if (jobInfo.Results.Any(resultInfo => resultInfo.ItemSelected &&
                                                              resultInfo.GridType != JobReader.DisplayInfo.GridModeType.Hidden &&
                                                              resultInfo.GridType != JobReader.DisplayInfo.GridModeType.Text))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (jobInfo.Results.Any(resultInfo => resultInfo.ItemSelected))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void NoSelectionWarn(AcceptDelegate handler)
        {
            if (_ecuFuncCall != XmlToolActivity.EcuFunctionCallType.None)
            {
                handler(true);
                return;
            }

            if (_ecuInfo.NoUpdate)
            {
                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                        handler(true);
                    })
                    .SetMessage(Resource.String.xml_tool_ecu_msg_save_lock)
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
                return;
            }

            if (AnyResultsSelected(true))
            {
                handler(true);
                return;
            }

            int resourceId = Resource.String.xml_tool_ecu_msg_no_selection;
            if (_checkBoxDisplayTypeGrid.Checked)
            {
                if (AnyResultsSelected(false))
                {
                    resourceId = Resource.String.xml_tool_ecu_msg_no_grid_selection;
                }
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    handler(true);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    handler(false);
                })
                .SetMessage(resourceId)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
        }

        private void StoreResults()
        {
            UpdateResultSettings(_selectedResult);
            string pageName = _editTextPageName.Text?.Trim();
            if (string.IsNullOrEmpty(pageName))
            {
                pageName = _ecuInfo.Description;
            }
            _ecuInfo.PageName = pageName;

            _ecuInfo.EcuName = _editTextEcuName.Text;
            _ecuInfo.DisplayMode = _checkBoxDisplayTypeGrid.Checked ? JobReader.PageInfo.DisplayModeType.Grid : JobReader.PageInfo.DisplayModeType.List;

            XmlToolActivity.DisplayFontSize fontSize = XmlToolActivity.DisplayFontSize.Small;
            int sizeIndex = _spinnerFontSize.SelectedItemPosition;
            if (sizeIndex >= 0 && sizeIndex < _spinnerFontSizeAdapter.Items.Count)
            {
                fontSize = (XmlToolActivity.DisplayFontSize)_spinnerFontSizeAdapter.Items[sizeIndex].Data;
            }
            _ecuInfo.FontSize = fontSize;

            if (Int32.TryParse(_editTextGridCountPortraitValue.Text, NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out Int32 gaugesPortrait))
            {
                if (gaugesPortrait >= 1)
                {
                    _ecuInfo.GaugesPortrait = gaugesPortrait;
                }
            }

            if (Int32.TryParse(_editTextGridCountLandscapeValue.Text, NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out Int32 gaugesLandscape))
            {
                if (gaugesLandscape >= 1)
                {
                    _ecuInfo.GaugesLandscape = gaugesLandscape;
                }
            }
        }

        private void DisplayTypeSelected()
        {
            HideKeyboard();
            ViewStates viewStateGrid = _checkBoxDisplayTypeGrid.Checked ? ViewStates.Visible : ViewStates.Gone;
            ViewStates viewStateStd = _checkBoxDisplayTypeGrid.Checked ? ViewStates.Gone : ViewStates.Visible;

            _textViewFontSizeTitle.Visibility = viewStateStd;
            _spinnerFontSize.Visibility = viewStateStd;

            _textViewGridCount.Visibility = viewStateGrid;
            _textViewGridCountPortraitValue.Visibility = viewStateGrid;
            _editTextGridCountPortraitValue.Visibility = viewStateGrid;
            _textViewGridCountLandscapeValue.Visibility = viewStateGrid;
            _editTextGridCountLandscapeValue.Visibility = viewStateGrid;

            _textViewGridType.Visibility = viewStateGrid;
            _spinnerGridType.Visibility = viewStateGrid;
            _textViewMinValue.Visibility = viewStateGrid;
            _editTextMinValue.Visibility = viewStateGrid;
            _textViewMaxValue.Visibility = viewStateGrid;
            _editTextMaxValue.Visibility = viewStateGrid;
        }

        private void FontItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (_ignoreItemSelection)
            {
                return;
            }

            HideKeyboard();
        }

        private void FormatItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (_ignoreFormatSelection)
            {
                return;
            }

            HideKeyboard();
            UpdateFormatString(_selectedResult);
        }

        private void JobSelected(JobInfo jobInfo)
        {
            _selectedJob = jobInfo;

            bool vagReadJob = IsVagReadJob(_selectedJob, _ecuInfo);
            _checkBoxShowAllResults.Visibility = (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw) &&
                                                 vagReadJob && !ActivityCommon.VagUdsActive ? ViewStates.Visible : ViewStates.Gone;

            ResetTestResult();
            _spinnerJobResultsAdapter.Items.Clear();
            int selection = -1;
            if (jobInfo != null)
            {
                bool bmwStatJob = IsBmwReadStatusTypeJob(_selectedJob);
                ViewStates limitVisibility = bmwStatJob ? ViewStates.Visible : ViewStates.Gone;
                _textViewArgLimitTitle.Visibility = limitVisibility;
                _spinnerArgLimit.Visibility = limitVisibility;

                ShowArgLimitHint();
                if (limitVisibility == ViewStates.Visible)
                {
                    if (_selectedJob.ArgLimit < 0)
                    {
                        _selectedJob.ArgLimit = 5;
                    }

                    int limitSelection = 0;
                    for (int i = 0; i < _spinnerArgLimitAdapter.Count; i++)
                    {
                        if ((int)_spinnerArgLimitAdapter.Items[i].Data == _selectedJob.ArgLimit)
                        {
                            limitSelection = i;
                        }
                    }

                    _spinnerArgLimit.SetSelection(limitSelection);
                }

                bool udsJob = false;
                bool ecuFunction = ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && jobInfo.EcuFixedFuncStruct != null;
                _layoutJobConfig.Visibility = ViewStates.Visible;
                List<ResultInfo> orderedResults;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if ((ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw) && vagReadJob)
                {
                    udsJob = string.Compare(jobInfo.Name, XmlToolActivity.JobReadS22Uds, StringComparison.OrdinalIgnoreCase) == 0;
                    List<ResultInfo> showResults = _selectedJob.Results;
                    if (!_checkBoxShowAllResults.Checked || _checkBoxShowAllResults.Visibility != ViewStates.Visible)
                    {
                        showResults = new List<ResultInfo>();
                        showResults.AddRange(_selectedJob.Results.Where(result => result.MwTabEntry != null && !result.MwTabEntry.Dummy));
                    }
                    orderedResults = showResults.OrderBy(x => (x.MwTabEntry?.BlockNumber << 16) + x.MwTabEntry?.ValueIndexTrans).ToList();
                }
                else
                {
                    orderedResults = new List<ResultInfo>(_selectedJob.Results);
                    orderedResults.Sort(new ResultInfoComparer());
                }

                ResultInfo resultGroupHeader = null;
                foreach (ResultInfo result in orderedResults)
                {
                    if (!udsJob && !ecuFunction && string.Compare(result.Type, XmlToolActivity.DataTypeBinary, StringComparison.OrdinalIgnoreCase) == 0)
                    {   // ignore binary results
                        continue;
                    }

                    if (result.GroupVisible)
                    {
                        resultGroupHeader = result;
                        continue;
                    }

                    if (FilterResultActive && !string.IsNullOrWhiteSpace(_searchFilterText))
                    {
                        if (!IsSearchFilterMatching(result.DisplayName, _searchFilterText))
                        {
                            continue;   // filter is not matching
                        }
                    }

                    if (resultGroupHeader != null && resultGroupHeader.GroupId != result.GroupId)
                    {
                        resultGroupHeader = null;
                    }

                    if (resultGroupHeader != null)
                    {
                        _spinnerJobResultsAdapter.Items.Add(resultGroupHeader);
                        resultGroupHeader = null;
                    }

                    _spinnerJobResultsAdapter.Items.Add(result);
                    if (result.ItemSelected && selection < 0)
                    {
                        selection = _spinnerJobResultsAdapter.Items.Count - 1;
                    }
                }
                if (_spinnerJobResultsAdapter.Items.Count > 0 && selection < 0 && jobInfo.Selected)
                {
                    // no selection
                    if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && !bmwStatJob)
                    {
                        // auto select all value types
                        int index = 0;
                        foreach (ResultInfo result in _spinnerJobResultsAdapter.Items)
                        {
                            bool autoSelect = false;
                            if (result.EcuJob != null)
                            {
                                if (result.EcuJobResult.EcuFuncRelevant.ConvertToInt() > 0)
                                {
                                    autoSelect = true;
                                }
                            }
                            else
                            {
                                if (result.Name.EndsWith("_WERT", StringComparison.OrdinalIgnoreCase))
                                {
                                    autoSelect = true;
                                }
                            }

                            if (autoSelect)
                            {
                                result.Selected = true;
                                if (selection < 0)
                                {
                                    selection = index;
                                }
                            }
                            index++;
                        }
                        if (selection < 0)
                        {
                            index = 0;
                            foreach (ResultInfo result in _spinnerJobResultsAdapter.Items)
                            {
                                if (result.Name.StartsWith("STAT_", StringComparison.OrdinalIgnoreCase) ||
                                    result.Name.StartsWith("STATUS_", StringComparison.OrdinalIgnoreCase))
                                {
                                    result.Selected = true;
                                    if (selection < 0)
                                    {
                                        selection = index;
                                    }
                                }
                                index++;
                            }
                        }
                    }
                    else
                    {
                        // auto select single entry
                        if (_spinnerJobResultsAdapter.Items.Count == 1)
                        {
                            _spinnerJobResultsAdapter.Items[0].Selected = true;
                            selection = 0;
                        }
                    }
                }

                if (_spinnerJobResultsAdapter.Items.Count > 0 && selection < 0)
                {
                    selection = 0;
                }

                _textViewJobCommentsTitle.Text = string.Format(GetString(Resource.String.xml_tool_ecu_job_comments), _selectedJob.DisplayName);

                StringBuilder stringBuilderComments = new StringBuilder();
                List<string> commentList = _selectedJob.CommentsTransRequired && _selectedJob.CommentsTrans != null ?
                    _selectedJob.CommentsTrans : _selectedJob.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
                    }
                }
                _textViewJobComments.Text = stringBuilderComments.ToString();
            }
            else
            {
                _layoutJobConfig.Visibility = ViewStates.Gone;
            }

            _ignoreItemSelection = true;
            _spinnerJobResultsAdapter.NotifyDataSetChanged();
            _spinnerJobResults.SetSelection(selection);
            _ignoreItemSelection = false;
            ResultSelected(selection);
        }

        private void ShowArgLimitHint()
        {
            bool bmwStatJob = IsBmwReadStatusTypeJob(_selectedJob);
            bool argLimitCritical = bmwStatJob && _selectedJob.ArgLimit != 1;
            if (argLimitCritical)
            {
                if (!_instanceData.ArgLimitCritical)
                {
                    Balloon.Builder balloonBuilder = ActivityCommon.GetBalloonBuilder(this);
                    balloonBuilder.SetText(GetString(Resource.String.xml_tool_ecu_arg_limit_hint));
                    Balloon balloon = balloonBuilder.Build();
                    balloon.ShowAtCenter(_spinnerArgLimit);
                }
            }

            _instanceData.ArgLimitCritical = argLimitCritical;
        }

        private void DisplayEcuInfo()
        {
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                return;
            }
            if (!ActivityCommon.VagUdsActive)
            {
                return;
            }
            _textViewJobCommentsTitle.Text = GetString(Resource.String.xml_tool_ecu_job_comments_ecu_info);

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(GetString(Resource.String.xml_tool_ecu_job_comments_ecu_info_addr), _ecuInfo.Address));
            sb.Append(" ");
            bool append = false;
            if (!string.IsNullOrEmpty(_ecuInfo.VagPartNumber))
            {
                sb.Append(_ecuInfo.VagPartNumber);
                append = true;
            }
            if (!string.IsNullOrEmpty(_ecuInfo.VagHwPartNumber))
            {
                if (append)
                {
                    sb.Append(" / ");
                }
                sb.Append(_ecuInfo.VagHwPartNumber);
            }
            if (!string.IsNullOrEmpty(_ecuInfo.VagSysName))
            {
                if (append)
                {
                    sb.Append(" / ");
                }
                sb.Append(_ecuInfo.VagSysName);
            }

            if (_ecuInfo.SubSystems != null)
            {
                foreach (XmlToolActivity.EcuInfoSubSys subSystem in _ecuInfo.SubSystems)
                {
                    sb.Append("\r\n");
                    sb.Append(string.Format(GetString(Resource.String.xml_tool_ecu_job_comments_ecu_info_subsys), subSystem.SubSysIndex + 1));
                    sb.Append(" ");
                    append = false;
                    if (!string.IsNullOrEmpty(subSystem.VagPartNumber))
                    {
                        sb.Append(subSystem.VagPartNumber);
                        append = true;
                    }
                    if (!string.IsNullOrEmpty(subSystem.VagSysName))
                    {
                        if (append)
                        {
                            sb.Append(" / ");
                        }
                        sb.Append(subSystem.VagSysName);
                        append = true;
                    }
                    if (!string.IsNullOrEmpty(subSystem.Name))
                    {
                        if (append)
                        {
                            sb.Append(" / ");
                        }
                        sb.Append(subSystem.Name);
                    }
                }
            }
            _textViewJobComments.Text = sb.ToString();
            _displayEcuInfo = true;
        }

        private void ResultSelected(int pos)
        {
            UpdateResultSettings(_selectedResult);  // store old settings
            if (pos >= 0 && pos < _spinnerJobResultsAdapter.ItemsVisible.Count)
            {
                _selectedResult = _spinnerJobResultsAdapter.ItemsVisible[pos];
                _textViewResultCommentsTitle.Text = string.Format(GetString(Resource.String.xml_tool_ecu_result_comments), _selectedResult.Name);

                StringBuilder stringBuilderComments = new StringBuilder();
                stringBuilderComments.Append(GetString(Resource.String.xml_tool_ecu_result_type));
                stringBuilderComments.Append(": ");
                stringBuilderComments.Append(_selectedResult.Type);
                List<string> commentList = _selectedResult.CommentsTransRequired && _selectedResult.CommentsTrans != null ?
                    _selectedResult.CommentsTrans : _selectedResult.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
                    }
                }
                _textViewResultComments.Text = stringBuilderComments.ToString();
                _editTextDisplayText.Text = _selectedResult.DisplayText;
                _editTextDisplayOrder.Text = _selectedResult.DisplayOrder.ToString(CultureInfo.InvariantCulture);

                bool resultBinary = IsResultBinary(_selectedResult);
                bool resultString = IsResultString(_selectedResult);

                _ignoreItemSelection = true;
                _spinnerGridTypeAdapter.Items.Clear();
                _spinnerGridTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_grid_type_hidden), JobReader.DisplayInfo.GridModeType.Hidden));
                _spinnerGridTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_grid_type_text), JobReader.DisplayInfo.GridModeType.Text));
                if (!resultBinary && !resultString)
                {
                    _spinnerGridTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_grid_type_simple_square), JobReader.DisplayInfo.GridModeType.Simple_Gauge_Square));
                    _spinnerGridTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_grid_type_simple_round), JobReader.DisplayInfo.GridModeType.Simple_Gauge_Round));
                    _spinnerGridTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_grid_type_simple_dot), JobReader.DisplayInfo.GridModeType.Simple_Gauge_Dot));
                }

                _spinnerGridTypeAdapter.NotifyDataSetChanged();

                int gridSelection = 0;
                for (int i = 0; i < _spinnerGridTypeAdapter.Count; i++)
                {
                    if ((JobReader.DisplayInfo.GridModeType)_spinnerGridTypeAdapter.Items[i].Data == _selectedResult.GridType)
                    {
                        gridSelection = i;
                    }
                }
                _spinnerGridType.SetSelection(gridSelection);
                _ignoreItemSelection = false;

                _editTextMinValue.Text = _selectedResult.MinValue.ToString(CultureInfo.InvariantCulture);
                _editTextMaxValue.Text = _selectedResult.MaxValue.ToString(CultureInfo.InvariantCulture);
                _editTextLogTag.Text = _selectedResult.LogTag;

                UpdateFormatFields(_selectedResult, false, true);
            }
            else
            {
                _selectedResult = null;
                _textViewResultComments.Text = string.Empty;
            }
            UpdateResultSettings(_selectedResult);
            ResetTestResult();
        }

        private void JobCheckChanged(JobInfo jobInfo)
        {
            if (jobInfo.Selected)
            {
                JobSelected(jobInfo);
            }
        }

        private void ResultCheckChanged(ResultInfo resultInfo)
        {
            if ((_selectedJob == null) || (_selectedResult == null))
            {
                return;
            }
            int selectCount = _selectedJob.Results.Count(resultInfo => resultInfo.ItemSelected);
            bool selectJob = selectCount > 0;
            if (_selectedJob.Selected != selectJob)
            {
                _selectedJob.Selected = selectJob;
                _spinnerJobsAdapter.NotifyDataSetChanged();
            }
        }

        private void HideKeyboard(bool forceClose = false)
        {
            if (!forceClose)
            {
                if (_searchView != null && !_searchView.Iconified)
                {
                    return;
                }
            }

            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private void ExecuteTestFormat()
        {
            _textViewTestFormatOutput.Text = string.Empty;
            if ((_selectedJob == null) || (_selectedResult == null))
            {
                return;
            }
            EdiabasOpen();

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.xml_tool_execute_test_job));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            string resultText = string.Empty;
            bool executeFailed = false;
            _jobThread = new Thread(() =>
            {
                try
                {
                    bool udsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
                    ActivityCommon.ResolveSgbdFile(_ediabas, _ecuInfo.Sgbd);

                    if (_selectedJob.EcuFixedFuncStruct?.EcuJobList != null)
                    {
                        List<EdiabasThread.EcuFunctionResult> ecuFunctionResultList = EdiabasThread.ExecuteEcuJobs(_ediabas, _selectedJob.EcuFixedFuncStruct);
                        if (ecuFunctionResultList != null)
                        {
                            foreach (EdiabasThread.EcuFunctionResult ecuFunctionResult in ecuFunctionResultList)
                            {
                                if (string.Compare(ecuFunctionResult.EcuJobResult.Name, _selectedResult.EcuJobResult.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    string text = ecuFunctionResult.ResultString;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Result text: {0}", text);
                                    if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(resultText))
                                    {
                                        resultText += "; ";
                                    }
                                    resultText += text;
                                }
                            }
                        }
                    }
                    else
                    {
                        _ediabas.ArgString = string.Empty;
                        if (_selectedResult.MwTabEntry != null && _ecuInfo.ReadCommand != null)
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "MWTAB file: {0}", _ecuInfo.MwTabFileName ?? "No file");
                            if (_selectedResult.MwTabEntry.ValueIndex.HasValue)
                            {
                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "MWTAB Block={0} Index={1}", _selectedResult.MwTabEntry.BlockNumber, _selectedResult.MwTabEntry.ValueIndexTrans);
                            }
                            else
                            {
                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "MWTAB Block={0}", _selectedResult.MwTabEntry.BlockNumber);
                            }
                            _ediabas.ArgString = GetJobArgs(_selectedResult.MwTabEntry, _ecuInfo);
                        }
                        else
                        {
                            _ediabas.ArgString = GetJobArgs(_selectedJob, new List<ResultInfo> { _selectedResult }, _ecuInfo, out string _, true);
                        }
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(_selectedJob.Name);

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
                                EdiabasNet.ResultData resultData;
                                if (_selectedResult.MwTabEntry != null)
                                {
                                    if (_selectedResult.MwTabEntry.ValueIndex.HasValue)
                                    {
                                        if (_selectedResult.MwTabEntry.ValueIndex.Value == dictIndex)
                                        {
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "MWTAB index found: {0}", dictIndex);
                                            string valueUnit = _selectedResult.MwTabEntry.ValueUnit;
                                            if (string.IsNullOrEmpty(valueUnit))
                                            {
                                                if (resultDict.TryGetValue("MWEINH_TEXT", out resultData))
                                                {
                                                    valueUnit = resultData.OpData as string ?? string.Empty;
                                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "MWEINH_TEXT: {0}", valueUnit);
                                                }
                                            }
                                            else
                                            {
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "MWTAB unit: {0}", valueUnit);
                                            }
                                            if (resultDict.TryGetValue("MW_WERT", out resultData))
                                            {
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Data type: {0}", resultData.ResType.ToString());
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Format: {0}", _selectedResult.Format ?? "No format");
                                                resultText = FormatResult(resultData, _selectedResult.Format);
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Result text: {0}", resultText);
                                                if (!string.IsNullOrWhiteSpace(resultText) && !string.IsNullOrWhiteSpace(valueUnit))
                                                {
                                                    resultText += " " + valueUnit;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (resultDict.TryGetValue("ERGEBNIS1WERT", out resultData))
                                        {
                                            resultText = string.Empty;
                                            if (ActivityCommon.VagUdsActive && udsEcu && resultData.OpData.GetType() == typeof(byte[]))
                                            {
                                                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_ecuInfo.VagUdsFileName);
                                                UdsFileReader.UdsReader.ParseInfoMwb parseInfoMwb = udsReader?.GetMwbParseInfo(_ecuInfo.VagUdsFileName, _selectedResult.Name);
                                                if (parseInfoMwb != null)
                                                {
                                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UniqueId match: {0}", parseInfoMwb.UniqueIdString);
                                                    resultText = parseInfoMwb.DataTypeEntry.ToString(CultureInfo.InvariantCulture, (byte[])resultData.OpData, out double? stringDataValue);
                                                    if (stringDataValue.HasValue && !string.IsNullOrEmpty(_selectedResult.Format))
                                                    {
                                                        resultText = EdiabasNet.FormatResult(new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeR, "ERGEBNIS1WERT", stringDataValue.Value), _selectedResult.Format);
                                                    }
                                                }
                                            }

                                            if (string.IsNullOrEmpty(resultText))
                                            {
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Data type: {0}", resultData.ResType.ToString());
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Format: {0}", _selectedResult.Format ?? "No format");
                                                resultText = FormatResult(resultData, _selectedResult.Format);
                                            }
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Result text: {0}", resultText);
                                            break;
                                        }
                                    }
                                    dictIndex++;
                                    continue;
                                }
                                if (resultDict.TryGetValue(_selectedResult.Name.ToUpperInvariant(), out resultData))
                                {
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Data type: {0}", resultData.ResType.ToString());
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Format: {0}", _selectedResult.Format ?? "No format");
                                    string text = FormatResult(resultData, _selectedResult.Format);
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Result text: {0}", text);
                                    if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(resultText))
                                    {
                                        resultText += "; ";
                                    }
                                    resultText += text;
                                }
                                dictIndex++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    executeFailed = true;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();
                    _textViewTestFormatOutput.Text = resultText;

                    if (executeFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_read_test_job_failed), Resource.String.alert_title_error);
                    }
                });
            });
            _jobThread.Start();
        }

        private bool StartBmwActuator()
        {
            try
            {
                StoreResults();
                EdiabasClose();

                BmwActuatorActivity.IntentEcuInfo = _ecuInfo;
                Intent serverIntent = new Intent(this, typeof(BmwActuatorActivity));
                serverIntent.PutExtra(BmwActuatorActivity.ExtraEcuName, _ecuInfo.Name);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraAppDataDir, _appDataDir);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraEcuDir, _ecuDir);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraSimulationDir, _simulationDir);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraTraceDir, _traceDir);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraTraceAppend, _traceAppend);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraDeviceAddress, _deviceAddress);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(BmwActuatorActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestBmwActuator);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool StartBmwCoding()
        {
            try
            {
                StoreResults();
                EdiabasClose();

                Intent serverIntent = new Intent(this, typeof(BmwCodingActivity));
                serverIntent.PutExtra(BmwCodingActivity.ExtraAppDataDir, _appDataDir);
                serverIntent.PutExtra(BmwCodingActivity.ExtraEcuDir, _ecuDir);
                serverIntent.PutExtra(BmwCodingActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(BmwCodingActivity.ExtraDeviceAddress, _deviceAddress);
                serverIntent.PutExtra(BmwCodingActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(BmwCodingActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(BmwCodingActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestBmwCoding);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool StartVagCoding(VagCodingActivity.CodingMode codingMode)
        {
            try
            {
                StoreResults();
                EdiabasClose();

                VagCodingActivity.IntentEcuInfo = _ecuInfo;
                Intent serverIntent = new Intent(this, typeof(VagCodingActivity));
                serverIntent.PutExtra(VagCodingActivity.ExtraCodingMode, (int)codingMode);
                serverIntent.PutExtra(VagCodingActivity.ExtraEcuName, _ecuInfo.Name);
                serverIntent.PutExtra(VagCodingActivity.ExtraAppDataDir, _appDataDir);
                serverIntent.PutExtra(VagCodingActivity.ExtraEcuDir, _ecuDir);
                serverIntent.PutExtra(VagCodingActivity.ExtraSimulationDir, _simulationDir);
                serverIntent.PutExtra(VagCodingActivity.ExtraTraceDir, _traceDir);
                serverIntent.PutExtra(VagCodingActivity.ExtraTraceAppend, _traceAppend);
                serverIntent.PutExtra(VagCodingActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(VagCodingActivity.ExtraDeviceAddress, _deviceAddress);
                serverIntent.PutExtra(VagCodingActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(VagCodingActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(VagCodingActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestVagCoding);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool StartVagAdaption()
        {
            try
            {
                StoreResults();
                EdiabasClose();

                VagAdaptionActivity.IntentEcuInfo = _ecuInfo;
                Intent serverIntent = new Intent(this, typeof(VagAdaptionActivity));
                serverIntent.PutExtra(VagAdaptionActivity.ExtraEcuName, _ecuInfo.Name);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraAppDataDir, _appDataDir);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraEcuDir, _ecuDir);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraSimulationDir, _simulationDir);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraTraceDir, _traceDir);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraTraceAppend, _traceAppend);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraDeviceAddress, _deviceAddress);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(VagAdaptionActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestVagAdaption);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class JobListAdapter : BaseAdapter<JobInfo>
        {
            public delegate void CheckChangedEventHandler(JobInfo jobInfo);
            public event CheckChangedEventHandler CheckChanged;

            private readonly List<JobInfo> _items;

            public List<JobInfo> Items => _items;

            private readonly XmlToolEcuActivity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public JobListAdapter(XmlToolEcuActivity context)
            {
                _context = context;
                _items = new List<JobInfo>();
                _backgroundColor = ActivityCommon.GetStyleColor(context, Android.Resource.Attribute.ColorBackground);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override JobInfo this[int position] => _items[position];

            public override int Count => _items.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.job_select_list, null);
                view.SetBackgroundColor(_backgroundColor);

                CheckBox checkBoxGroupSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxGroupSelect);
                checkBoxGroupSelect.Visibility = ViewStates.Gone;

                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textJobName = view.FindViewById<TextView>(Resource.Id.textJobName);
                TextView textJobDesc = view.FindViewById<TextView>(Resource.Id.textJobDesc);
                textJobName.Text = item.DisplayName;

                StringBuilder stringBuilderComments = new StringBuilder();
                List<string> commentList = item.CommentsTransRequired && item.CommentsTrans != null ?
                    item.CommentsTrans : item.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
                    }
                }
                textJobDesc.Text = stringBuilderComments.ToString();

                return view;
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = sender as CheckBox;
                    if (checkBox?.Tag is TagInfo tagInfo)
                    {
                        if (tagInfo.Info.Selected != args.IsChecked)
                        {
                            tagInfo.Info.Selected = args.IsChecked;
                            CheckChanged?.Invoke(tagInfo.Info);
                            NotifyDataSetChanged();
                        }
                    }
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(JobInfo info)
                {
                    Info = info;
                }

                public JobInfo Info { get; }
            }
        }

        private class ResultListAdapter : BaseAdapter<ResultInfo>
        {
            public delegate void CheckChangedEventHandler(ResultInfo resultInfo);
            public event CheckChangedEventHandler CheckChanged;

            public delegate void GroupChangedEventHandler(ResultInfo resultInfo);
            public event GroupChangedEventHandler GroupChanged;

            private readonly List<ResultInfo> _items;
            private readonly List<ResultInfo> _itemsVisible;
            public List<ResultInfo> Items => _items;
            public List<ResultInfo> ItemsVisible => _itemsVisible;

            private readonly XmlToolEcuActivity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public ResultListAdapter(XmlToolEcuActivity context)
            {
                _context = context;
                _items = new List<ResultInfo>();
                _itemsVisible = new List<ResultInfo>();
                _backgroundColor = ActivityCommon.GetStyleColor(context, Android.Resource.Attribute.ColorBackground);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ResultInfo this[int position] => _itemsVisible[position];

            public override int Count => _itemsVisible.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _itemsVisible[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.job_select_list, null);
                view.SetBackgroundColor(_backgroundColor);

                CheckBox checkBoxGroupSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxGroupSelect);

                ViewStates viewStateGroup = ViewStates.Gone;
                if (item.GroupVisible)
                {
                    viewStateGroup = ViewStates.Visible;
                }
                else if (item.GroupId.HasValue)
                {
                    viewStateGroup = ViewStates.Invisible;
                }

                _ignoreCheckEvent = true;
                checkBoxGroupSelect.Visibility = viewStateGroup;
                checkBoxGroupSelect.Checked = item.GroupSelected;
                _ignoreCheckEvent = false;

                checkBoxGroupSelect.Tag = new TagInfo(item);
                checkBoxGroupSelect.CheckedChange -= OnGroupChanged;
                checkBoxGroupSelect.CheckedChange += OnGroupChanged;

                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                checkBoxSelect.Enabled = !item.GroupVisible || (item.GroupVisible && item.Selected);
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textJobName = view.FindViewById<TextView>(Resource.Id.textJobName);
                TextView textJobDesc = view.FindViewById<TextView>(Resource.Id.textJobDesc);
                textJobName.Text = item.DisplayName;
                if (!string.IsNullOrEmpty(item.Type))
                {
                    textJobName.Text += " (" + item.Type + ")";
                }

                StringBuilder stringBuilderComments = new StringBuilder();
                List<string> commentList = item.CommentsTransRequired && item.CommentsTrans != null ?
                    item.CommentsTrans : item.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
                    }
                }
                textJobDesc.Text = stringBuilderComments.ToString();

                return view;
            }

            public override void NotifyDataSetChanged()
            {
                UpdateGroupList();
                base.NotifyDataSetChanged();
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = sender as CheckBox;
                    if (checkBox?.Tag is TagInfo tagInfo)
                    {
                        if (tagInfo.Info.Selected != args.IsChecked)
                        {
                            if (tagInfo.Info.GroupId.HasValue && tagInfo.Info.GroupVisible && !args.IsChecked)
                            {
                                DeselectGroup(tagInfo.Info.GroupId.Value);
                            }

                            tagInfo.Info.Selected = args.IsChecked;
                            CheckChanged?.Invoke(tagInfo.Info);
                            NotifyDataSetChanged();
                        }
                    }
                }
            }

            private void OnGroupChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = sender as CheckBox;
                    if (checkBox?.Tag is TagInfo tagInfo)
                    {
                        if (tagInfo.Info.GroupSelected != args.IsChecked)
                        {
                            tagInfo.Info.GroupSelected = args.IsChecked;
                            GroupChanged?.Invoke(tagInfo.Info);
                            NotifyDataSetChanged();
                        }
                    }
                }
            }

            private void UpdateGroupList()
            {
                HashSet<int> visibleGroups = new HashSet<int>();
                HashSet<int> checkedGroups = new HashSet<int>();
                foreach (ResultInfo resultInfo in _items)
                {
                    if (resultInfo.GroupId.HasValue && resultInfo.GroupVisible && resultInfo.GroupSelected)
                    {
                        visibleGroups.Add(resultInfo.GroupId.Value);
                    }

                    if (resultInfo.GroupId.HasValue && resultInfo.ItemSelected)
                    {
                        checkedGroups.Add(resultInfo.GroupId.Value);
                    }
                }

                _itemsVisible.Clear();
                foreach (ResultInfo resultInfo in _items)
                {
                    bool itemVisible = true;
                    if (resultInfo.GroupId.HasValue && !resultInfo.GroupVisible)
                    {
                        if (!visibleGroups.Contains(resultInfo.GroupId.Value))
                        {
                            itemVisible = false;
                        }
                    }

                    if (itemVisible)
                    {
                        _itemsVisible.Add(resultInfo);
                    }

                    if (resultInfo.GroupId.HasValue && resultInfo.GroupVisible)
                    {
                        resultInfo.Selected = checkedGroups.Contains(resultInfo.GroupId.Value);
                    }
                }
            }

            private void DeselectGroup(int groupId)
            {
                foreach (ResultInfo resultInfo in _items)
                {
                    if (resultInfo.GroupId.HasValue && !resultInfo.GroupVisible)
                    {
                        if (resultInfo.GroupId.Value == groupId)
                        {
                            resultInfo.Selected = false;
                        }
                    }
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(ResultInfo info)
                {
                    Info = info;
                }

                public ResultInfo Info { get; }
            }
        }
    }
}
