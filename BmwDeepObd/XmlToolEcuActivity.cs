using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Text;
using Android.Content;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class XmlToolEcuActivity : AppCompatActivity, View.IOnTouchListener
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
                MwTabEntry = mwTabEntry;
                Selected = false;
                Format = string.Empty;
                GridType = JobReader.DisplayInfo.GridModeType.Hidden;
                MinValue = 0;
                MaxValue = 100;
                DisplayText = displayName;
                LogTag = name;
            }

            public string Name { get; }

            public string DisplayName { get; }

            public string Type { get; }

            public string Args { get; }

            public List<string> Comments { get; }

            public ActivityCommon.MwTabEntry MwTabEntry { get; }

            public List<string> CommentsTrans { get; set; }

            public bool Selected { get; set; }

            public string Format { get; set; }

            public JobReader.DisplayInfo.GridModeType GridType { get; set; }

            public double MinValue { get; set; }

            public double MaxValue { get; set; }

            public string DisplayText { get; set; }

            public string LogTag { get; set; }
        }

        public class JobInfo
        {
            public JobInfo(string name)
            {
                Name = name;
                Comments = new List<string>();
                Results = new List<ResultInfo>();
                ArgCount = 0;
                Selected = false;
            }

            public string Name { get; }

            public List<string> Comments { get; set; }

            public List<string> CommentsTrans { get; set; }

            public List<ResultInfo> Results { get; }

            public uint ArgCount { get; set; }

            public bool Selected { get; set; }
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
            public bool IgnoreFormatSelection { get; set; }
        }

        // Intent extra
        public const string ExtraEcuName = "ecu_name";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraTraceDir = "trace_dir";
        public const string ExtraTraceAppend = "trace_append";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        // Intent results
        public const string ExtraCallEdiabasTool = "ediabas_tool";
        private static readonly int[] LengthValues = {0, 1, 2, 3, 4, 5, 6, 8, 10, 15, 20, 25, 30, 35, 40};

        public static XmlToolActivity.EcuInfo IntentEcuInfo { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private InputMethodManager _imm;
        private View _contentView;
        private EditText _editTextPageName;
        private EditText _editTextEcuName;
        private CheckBox _checkBoxDisplayTypeGrid;
        private Spinner _spinnerFontSize;
        private StringObjAdapter _spinnerFontSizeAdapter;
        private Spinner _spinnerJobs;
        private JobListAdapter _spinnerJobsAdapter;
        private TextView _textViewJobCommentsTitle;
        private TextView _textViewJobComments;
        private LinearLayout _layoutJobConfig;
        private Spinner _spinnerJobResults;
        private ResultListAdapter _spinnerJobResultsAdapter;
        private CheckBox _checkBoxShowAllResults;
        private TextView _textViewResultCommentsTitle;
        private TextView _textViewResultComments;
        private EditText _editTextDisplayText;
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
        private ActivityCommon _activityCommon;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private JobInfo _selectedJob;
        private ResultInfo _selectedResult;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private bool _activityActive;
        private bool _ediabasJobAbort;
        private string _ecuDir;
        private string _traceDir;
        private bool _traceAppend;
        private string _deviceAddress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
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

            _activityCommon = new ActivityCommon(this, () =>
            {

            }, BroadcastReceived);

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _traceDir = Intent.GetStringExtra(ExtraTraceDir);
            _traceAppend = Intent.GetBooleanExtra(ExtraTraceAppend, true);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int) ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEcuDir);

            _ecuInfo = IntentEcuInfo;

            _editTextPageName = FindViewById<EditText>(Resource.Id.editTextPageName);
            _editTextPageName.Text = _ecuInfo.PageName;

            _editTextEcuName = FindViewById<EditText>(Resource.Id.editTextEcuName);
            _editTextEcuName.Text = _ecuInfo.EcuName;

            _checkBoxDisplayTypeGrid = FindViewById<CheckBox>(Resource.Id.checkBoxDisplayTypeGrid);
            _checkBoxDisplayTypeGrid.Checked = _ecuInfo.DisplayMode == JobReader.PageInfo.DisplayModeType.Grid;
            _checkBoxDisplayTypeGrid.CheckedChange += (sender, args) =>
            {
                DisplayTypeSelected();
            };

            _spinnerFontSize = FindViewById<Spinner>(Resource.Id.spinnerFontSize);
            _spinnerFontSizeAdapter = new StringObjAdapter(this);
            _spinnerFontSize.Adapter = _spinnerFontSizeAdapter;
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
            _spinnerFontSize.ItemSelected += FontItemSelected;

            _spinnerJobs = FindViewById<Spinner>(Resource.Id.spinnerJobs);
            _spinnerJobsAdapter = new JobListAdapter(this);
            _spinnerJobs.Adapter = _spinnerJobsAdapter;
            _spinnerJobs.SetOnTouchListener(this);
            _spinnerJobs.ItemSelected += (sender, args) =>
            {
                int pos = args.Position;
                JobSelected(pos >= 0 ? _spinnerJobsAdapter.Items[pos] : null);
            };

            _layoutJobConfig = FindViewById<LinearLayout>(Resource.Id.layoutJobConfig);
            _layoutJobConfig.SetOnTouchListener(this);

            _textViewJobCommentsTitle = FindViewById<TextView>(Resource.Id.textViewJobCommentsTitle);
            _textViewJobComments = FindViewById<TextView>(Resource.Id.textViewJobComments);

            _spinnerJobResults = FindViewById<Spinner>(Resource.Id.spinnerJobResults);
            _spinnerJobResultsAdapter = new ResultListAdapter(this);
            _spinnerJobResults.Adapter = _spinnerJobResultsAdapter;
            _spinnerJobResults.ItemSelected += (sender, args) =>
            {
                ResultSelected(args.Position);
            };

            _checkBoxShowAllResults = FindViewById<CheckBox>(Resource.Id.checkBoxShowAllResults);
            bool showAll = false;
            foreach (JobInfo jobInfo in _ecuInfo.JobList)
            {
                if (IsVagReadJob(jobInfo, _ecuInfo))
                {
                    if (jobInfo.Results.All(resultInfo => resultInfo.MwTabEntry != null && resultInfo.MwTabEntry.Dummy))
                    {
                        showAll = true;
                        break;
                    }
                    if (jobInfo.Results.Any(resultInfo => resultInfo.Selected && resultInfo.MwTabEntry != null && resultInfo.MwTabEntry.Dummy))
                    {
                        showAll = true;
                        break;
                    }
                }
            }
            _checkBoxShowAllResults.Checked = showAll;
            _checkBoxShowAllResults.Click += (sender, args) =>
            {
                JobSelected(_selectedJob);
            };

            _textViewResultCommentsTitle = FindViewById<TextView>(Resource.Id.textViewResultCommentsTitle);
            _textViewResultComments = FindViewById<TextView>(Resource.Id.textViewResultComments);
            _editTextDisplayText = FindViewById<EditText>(Resource.Id.editTextDisplayText);

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
                Finish();
            };

            _layoutJobConfig.Visibility = ViewStates.Gone;
            UpdateDisplay();
            DisplayTypeSelected();
            ResetTestResult();
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
        }

        protected override void OnPause()
        {
            base.OnPause();
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
            EdiabasClose();
            _activityCommon.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            StoreResults();
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            UpdateResultSettings(_selectedResult);
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    StoreResults();
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    UpdateResultSettings(_selectedResult);
                    HideKeyboard();
                    break;
            }
            return false;
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
            if (ecuInfo.Sgbd.Contains("7000"))
            {
                return string.Compare(job.Name, XmlToolActivity.JobReadMwUds, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return string.Compare(job.Name, XmlToolActivity.JobReadMwBlock, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsValidJob(JobInfo job, XmlToolActivity.EcuInfo ecuInfo)
        {
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                if (IsVagReadJob(job, ecuInfo))
                {
                    return true;
                }
                if (string.Compare(job.Name, "Fahrgestellnr_abfragen", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                return false;
            }
            if (string.Compare(job.Name, XmlToolActivity.JobReadStatMwBlock, StringComparison.OrdinalIgnoreCase) == 0)
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

        public static string GetJobArgs(ActivityCommon.MwTabEntry mwTabEntry, XmlToolActivity.EcuInfo ecuInfo)
        {
            if (string.IsNullOrEmpty(ecuInfo.ReadCommand))
            {
                return string.Format(XmlToolActivity.Culture, "{0}", mwTabEntry.BlockNumber);
            }
            return string.Format(XmlToolActivity.Culture, "{0};{1}", mwTabEntry.BlockNumber, ecuInfo.ReadCommand);
        }

        public static string GetJobArgs(JobInfo job, List<ResultInfo> resultInfoList, XmlToolActivity.EcuInfo ecuInfo, bool selectAll = false)
        {
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return string.Empty;
            }
            if (string.Compare(job.Name, XmlToolActivity.JobReadStatMwBlock, StringComparison.OrdinalIgnoreCase) == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("JA");
                foreach (ResultInfo resultInfo in resultInfoList)
                {
                    if ((selectAll || resultInfo.Selected) && !string.IsNullOrEmpty(resultInfo.Args))
                    {
                        sb.Append(";");
                        sb.Append(resultInfo.Args);
                    }
                }
                return sb.ToString();
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
                if (!string.IsNullOrEmpty(_traceDir))
                {
                    _ediabas.SetConfigProperty("TracePath", _traceDir);
                    _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                    _ediabas.SetConfigProperty("AppendTrace", _traceAppend ? "1" : "0");
                    _ediabas.SetConfigProperty("CompressTrace", "1");
                }
                else
                {
                    _ediabas.SetConfigProperty("IfhTrace", "0");
                }
            }

            _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress);
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
                        if (intent.GetParcelableExtra(UsbManager.ExtraDevice) is UsbDevice usbDevice)
                        {
                            _activityCommon.RequestUsbPermission(usbDevice);
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

        private void UpdateDisplay()
        {
            int selection = 0;
            _spinnerJobsAdapter.Items.Clear();
            foreach (JobInfo job in _ecuInfo.JobList.OrderBy(x => x.Name))
            {
                if (IsValidJob(job, _ecuInfo))
                {
                    _spinnerJobsAdapter.Items.Add(job);
                    if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                    {
                        if (IsVagReadJob(job, _ecuInfo))
                        {
                            selection = _spinnerJobsAdapter.Items.Count - 1;
                        }
                    }
                }
            }
            _spinnerJobsAdapter.NotifyDataSetChanged();
            if (_spinnerJobsAdapter.Items.Count > 0)
            {
                _spinnerJobs.SetSelection(selection);
                JobSelected(_spinnerJobsAdapter.Items[selection]);
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

        private bool IsResultBinary(ResultInfo resultInfo)
        {
            return string.Compare(resultInfo.Type, XmlToolActivity.DataTypeBinary, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private bool IsResultString(ResultInfo resultInfo)
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

            _instanceData.IgnoreFormatSelection = true;

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
            _instanceData.IgnoreFormatSelection = false;

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
            if (_spinnerFormatType.SelectedItemPosition >= 0)
            {
                formatType = (FormatType)_spinnerFormatTypeAdapter.Items[_spinnerFormatType.SelectedItemPosition].Data;
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
                if (_spinnerFormatLength1.SelectedItemPosition >= 0)
                {
                    int value = (int) _spinnerFormatLength1Adapter.Items[_spinnerFormatLength1.SelectedItemPosition].Data;
                    if (value >= 0)
                    {
                        stringBuilder.Append(value.ToString());
                    }
                }
                if (_spinnerFormatLength2.SelectedItemPosition >= 0)
                {
                    int value = (int)_spinnerFormatLength2Adapter.Items[_spinnerFormatLength2.SelectedItemPosition].Data;
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
                if (_spinnerGridType.SelectedItemPosition >= 0)
                {
                    _selectedResult.GridType = (JobReader.DisplayInfo.GridModeType)_spinnerGridTypeAdapter.Items[_spinnerGridType.SelectedItemPosition].Data;
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
            if ((resultInfo == null) || _instanceData.IgnoreFormatSelection)
            {
                return;
            }
            resultInfo.Format = GetFormatString();

            FormatType formatType = FormatType.None;
            if (_spinnerFormatType.SelectedItemPosition >= 0)
            {
                formatType = (FormatType)_spinnerFormatTypeAdapter.Items[_spinnerFormatType.SelectedItemPosition].Data;
            }
            UpdateFormatFields(resultInfo, formatType == FormatType.User);
        }

        private void StoreResults()
        {
            UpdateResultSettings(_selectedResult);
            _ecuInfo.PageName = _editTextPageName.Text;
            _ecuInfo.EcuName = _editTextEcuName.Text;
            _ecuInfo.DisplayMode = _checkBoxDisplayTypeGrid.Checked ? JobReader.PageInfo.DisplayModeType.Grid : JobReader.PageInfo.DisplayModeType.List;

            XmlToolActivity.DisplayFontSize fontSize = XmlToolActivity.DisplayFontSize.Small;
            if (_spinnerFontSize.SelectedItemPosition >= 0)
            {
                fontSize = (XmlToolActivity.DisplayFontSize)_spinnerFontSizeAdapter.Items[_spinnerFontSize.SelectedItemPosition].Data;
            }
            _ecuInfo.FontSize = fontSize;
        }

        private void DisplayTypeSelected()
        {
            HideKeyboard();
            ViewStates viewState = _checkBoxDisplayTypeGrid.Checked ? ViewStates.Visible : ViewStates.Gone;
            _textViewGridType.Visibility = viewState;
            _spinnerGridType.Visibility = viewState;
            _textViewMinValue.Visibility = viewState;
            _editTextMinValue.Visibility = viewState;
            _textViewMaxValue.Visibility = viewState;
            _editTextMaxValue.Visibility = viewState;
        }

        private void FontItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
        }

        private void FormatItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            UpdateFormatString(_selectedResult);
        }

        private void JobSelected(JobInfo jobInfo)
        {
            _selectedJob = jobInfo;

            bool vagReadJob = IsVagReadJob(_selectedJob, _ecuInfo);
            _checkBoxShowAllResults.Visibility = (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw) && vagReadJob ?
                ViewStates.Visible : ViewStates.Gone;

            ResetTestResult();
            _spinnerJobResultsAdapter.Items.Clear();
            int selection = -1;
            if (jobInfo != null)
            {
                bool udsJob = false;
                _layoutJobConfig.Visibility = ViewStates.Visible;
                IEnumerable<ResultInfo> orderedResults;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if ((ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw) && vagReadJob)
                {
                    udsJob = string.Compare(jobInfo.Name, XmlToolActivity.JobReadMwUds, StringComparison.OrdinalIgnoreCase) == 0;
                    List<ResultInfo> showResults = new List<ResultInfo>();
                    if (_checkBoxShowAllResults.Checked && _checkBoxShowAllResults.Visibility == ViewStates.Visible)
                    {
                        showResults = _selectedJob.Results;
                    }
                    else
                    {
                        showResults.AddRange(_selectedJob.Results.Where(result => result.MwTabEntry != null && !result.MwTabEntry.Dummy));
                    }
                    orderedResults = showResults.OrderBy(x => (x.MwTabEntry?.BlockNumber << 16) + x.MwTabEntry?.ValueIndexTrans);
                }
                else
                {
                    orderedResults = _selectedJob.Results.OrderBy(x => x.Name);
                }
                foreach (ResultInfo result in orderedResults)
                {
                    if (!udsJob && string.Compare(result.Type, XmlToolActivity.DataTypeBinary, StringComparison.OrdinalIgnoreCase) == 0)
                    {   // ignore binary results
                        continue;
                    }
                    _spinnerJobResultsAdapter.Items.Add(result);
                    if (result.Selected && selection < 0)
                    {
                        selection = _spinnerJobResultsAdapter.Items.Count - 1;
                    }
                }
                if (_spinnerJobResultsAdapter.Items.Count > 0 && selection < 0 && jobInfo.Selected)
                {
                    // no selection
                    if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw &&
                        string.Compare(_selectedJob.Name, XmlToolActivity.JobReadStatMwBlock, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        // auto select all value types
                        int index = 0;
                        foreach (ResultInfo result in _spinnerJobResultsAdapter.Items)
                        {
                            if (result.Name.EndsWith("_WERT", StringComparison.OrdinalIgnoreCase))
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

                _textViewJobCommentsTitle.Text = string.Format(GetString(Resource.String.xml_tool_ecu_job_comments), _selectedJob.Name);

                StringBuilder stringBuilderComments = new StringBuilder();
                List<string> commentList = _selectedJob.CommentsTrans ?? _selectedJob.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        if (stringBuilderComments.Length > 0)
                        {
                            stringBuilderComments.Append("\r\n");
                        }
                        stringBuilderComments.Append(comment);
                    }
                }
                _textViewJobComments.Text = stringBuilderComments.ToString();
            }
            else
            {
                _layoutJobConfig.Visibility = ViewStates.Gone;
            }
            _spinnerJobResultsAdapter.NotifyDataSetChanged();
            _spinnerJobResults.SetSelection(selection);
            ResultSelected(selection);
        }

        private void ResultSelected(int pos)
        {
            UpdateResultSettings(_selectedResult);  // store old settings
            if (pos >= 0)
            {
                _selectedResult = _spinnerJobResultsAdapter.Items[pos];
                _textViewResultCommentsTitle.Text = string.Format(GetString(Resource.String.xml_tool_ecu_result_comments), _selectedResult.Name);

                StringBuilder stringBuilderComments = new StringBuilder();
                stringBuilderComments.Append(GetString(Resource.String.xml_tool_ecu_result_type));
                stringBuilderComments.Append(": ");
                stringBuilderComments.Append(_selectedResult.Type);
                List<string> commentList = _selectedResult.CommentsTrans ?? _selectedResult.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                }
                _textViewResultComments.Text = stringBuilderComments.ToString();
                _editTextDisplayText.Text = _selectedResult.DisplayText;

                bool resultBinary = IsResultBinary(_selectedResult);
                bool resultString = IsResultString(_selectedResult);

                _spinnerGridTypeAdapter.Items.Clear();
                _spinnerGridTypeAdapter.Items.Add(new StringObjType(GetString(Resource.String.xml_tool_ecu_grid_type_hidden), JobReader.DisplayInfo.GridModeType.Hidden));
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

        private void ResultCheckChanged()
        {
            if ((_selectedJob == null) || (_selectedResult == null))
            {
                return;
            }
            int selectCount = _selectedJob.Results.Count(resultInfo => resultInfo.Selected);
            bool selectJob = selectCount > 0;
            if (_selectedJob.Selected != selectJob)
            {
                _selectedJob.Selected = selectJob;
                _spinnerJobsAdapter.NotifyDataSetChanged();
            }
        }

        private void HideKeyboard()
        {
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
                    _ediabas.ResolveSgbdFile(_ecuInfo.Sgbd);

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
                        _ediabas.ArgString = GetJobArgs(_selectedJob, new List<ResultInfo> {_selectedResult}, _ecuInfo, true);
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
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Data type: {0}", resultData.ResType.ToString());
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Format: {0}", _selectedResult.Format ?? "No format");
                                        resultText = FormatResult(resultData, _selectedResult.Format);
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
                    progress.Dispose();
                    _textViewTestFormatOutput.Text = resultText;

                    if (executeFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_read_test_job_failed), Resource.String.alert_title_error);
                    }
                });
            });
            _jobThread.Start();
        }

        private class JobListAdapter : BaseAdapter<JobInfo>
        {
            private readonly List<JobInfo> _items;

            public List<JobInfo> Items => _items;

            private readonly XmlToolEcuActivity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public JobListAdapter(XmlToolEcuActivity context)
            {
                _context = context;
                _items = new List<JobInfo>();
                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new[] { Android.Resource.Attribute.ColorBackground });
                _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
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
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxJobSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textJobName = view.FindViewById<TextView>(Resource.Id.textJobName);
                TextView textJobDesc = view.FindViewById<TextView>(Resource.Id.textJobDesc);
                textJobName.Text = item.Name;

                StringBuilder stringBuilderComments = new StringBuilder();
                List<string> commentList = item.CommentsTrans ?? item.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        if (stringBuilderComments.Length > 0)
                        {
                            stringBuilderComments.Append("; ");
                        }
                        stringBuilderComments.Append(comment);
                    }
                }
                textJobDesc.Text = stringBuilderComments.ToString();

                return view;
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = (CheckBox) sender;
                    TagInfo tagInfo = (TagInfo) checkBox.Tag;
                    if (tagInfo.Info.Selected != args.IsChecked)
                    {
                        tagInfo.Info.Selected = args.IsChecked;
                        _context.JobCheckChanged(tagInfo.Info);
                        NotifyDataSetChanged();
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
            private readonly List<ResultInfo> _items;

            public List<ResultInfo> Items => _items;

            private readonly XmlToolEcuActivity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public ResultListAdapter(XmlToolEcuActivity context)
            {
                _context = context;
                _items = new List<ResultInfo>();
                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new[] { Android.Resource.Attribute.ColorBackground });
                _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ResultInfo this[int position] => _items[position];

            public override int Count => _items.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.job_select_list, null);
                view.SetBackgroundColor(_backgroundColor);
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxJobSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
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
                List<string> commentList = item.CommentsTrans ?? item.Comments;
                if (commentList != null)
                {
                    foreach (string comment in commentList)
                    {
                        if (stringBuilderComments.Length > 0)
                        {
                            stringBuilderComments.Append("; ");
                        }
                        stringBuilderComments.Append(comment);
                    }
                }
                textJobDesc.Text = stringBuilderComments.ToString();

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
                        NotifyDataSetChanged();
                        _context.ResultCheckChanged();
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
