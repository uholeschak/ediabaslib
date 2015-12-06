using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BmwDeepObd.FilePicker;
using EdiabasLib;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
// ReSharper disable LoopCanBeConvertedToQuery

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/tool_title",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                        Android.Content.PM.ConfigChanges.Orientation |
                        Android.Content.PM.ConfigChanges.ScreenSize)]
    public class EdiabasToolActivity : AppCompatActivity, View.IOnTouchListener
    {
        private enum ActivityRequest
        {
            RequestSelectSgbd,
            RequestSelectDevice,
            RequestCanAdapterConfig,
        }

        private class ExtraInfo
        {
            public ExtraInfo(string name, string type, List<string> commentList)
            {
                _name = name;
                _type = type;
                _commentList = commentList;
                Selected = false;
            }

            private readonly string _name;
            private readonly string _type;
            private readonly List<string> _commentList;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public string Type
            {
                get
                {
                    return _type;
                }
            }

            public List<string> CommentList
            {
                get
                {
                    return _commentList;
                }
            }

            public bool Selected { get; set; }
        }

        private class JobInfo
        {
            public JobInfo(string name)
            {
                _name = name;
                _comments = new List<string>();
                _arguments = new List<ExtraInfo>();
                _results = new List<ExtraInfo>();
            }

            private readonly string _name;
            private readonly List<string> _comments;
            private readonly List<ExtraInfo> _arguments;
            private readonly List<ExtraInfo> _results;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public List<string> Comments
            {
                get
                {
                    return _comments;
                }
            }

            public List<ExtraInfo> Arguments
            {
                get
                {
                    return _arguments;
                }
            }

            public List<ExtraInfo> Results
            {
                get
                {
                    return _results;
                }
            }
        }

        // Intent extra
        public const string ExtraInitDir = "init_dir";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");

        private InputMethodManager _imm;
        private View _contentView;
        private View _barView;
        private CheckBox _checkBoxContinuous;
        private ToggleButton _buttonConnect;
        private Spinner _spinnerJobs;
        private JobListAdapter _jobListAdapter;
        private EditText _editTextArgs;
        private Spinner _spinnerResults;
        private ResultSelectListAdapter _resultSelectListAdapter;
        private int _resultSelectLastItem;
        private ListView _listViewInfo;
        private ResultListAdapter _infoListAdapter;
        private string _initDirStart;
        private string _appDataDir;
        private bool _autoStart;
        private int _autoStartItemId;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private StreamWriter _swDataLog;
        private string _dataLogDir;
        private string _traceDir;
        private Thread _jobThread;
        private volatile bool _runContinuous;
        private volatile bool _ediabasJobAbort;
        private string _sgbdFileName = string.Empty;
        private string _deviceName = string.Empty;
        private string _deviceAddress = string.Empty;
        private bool _traceActive = true;
        private bool _traceAppend;
        private bool _dataLogActive;
        private bool _commErrorsOccured;
        private bool _activityActive;
        private readonly List<JobInfo> _jobList = new List<JobInfo>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.ediabas_tool);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            _barView = LayoutInflater.Inflate(Resource.Layout.bar_ediabas_tool, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(_barView, barLayoutParams);

            SetResult(Android.App.Result.Canceled);

            _checkBoxContinuous = _barView.FindViewById<CheckBox>(Resource.Id.checkBoxContinuous);
            _checkBoxContinuous.SetOnTouchListener(this);

            _buttonConnect = _barView.FindViewById<ToggleButton>(Resource.Id.buttonConnect);
            _buttonConnect.SetOnTouchListener(this);
            _buttonConnect.Click += (sender, args) =>
            {
                if (_buttonConnect.Checked)
                {
                    ExecuteSelectedJob(_checkBoxContinuous.Checked);
                }
                else
                {
                    _runContinuous = false;
                    UpdateDisplay();
                }
            };

            _spinnerJobs = FindViewById<Spinner>(Resource.Id.spinnerJobs);
            _jobListAdapter = new JobListAdapter(this);
            _spinnerJobs.Adapter = _jobListAdapter;
            _spinnerJobs.SetOnTouchListener(this);
            _spinnerJobs.ItemSelected += (sender, args) =>
                {
                    NewJobSelected();
                    DisplayJobComments();
                };

            _editTextArgs = FindViewById<EditText>(Resource.Id.editTextArgs);
            _editTextArgs.SetOnTouchListener(this);

            _spinnerResults = FindViewById<Spinner>(Resource.Id.spinnerResults);
            _resultSelectListAdapter = new ResultSelectListAdapter(this);
            _spinnerResults.Adapter = _resultSelectListAdapter;
            _spinnerResults.SetOnTouchListener(this);
            _spinnerResults.ItemSelected += (sender, args) =>
                {
                    if (_resultSelectLastItem != _spinnerResults.SelectedItemPosition)
                    {
                        DisplayJobResult();
                    }
                    _resultSelectLastItem = -1;
                };
            _resultSelectLastItem = -1;

            _listViewInfo = FindViewById<ListView>(Resource.Id.infoList);
            _infoListAdapter = new ResultListAdapter(this);
            _listViewInfo.Adapter = _infoListAdapter;
            _listViewInfo.SetOnTouchListener(this);

            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                }
            }, BroadcastReceived)
            {
                SelectedInterface = (ActivityCommon.InterfaceType)
                    Intent.GetIntExtra(ExtraInterface, (int) ActivityCommon.InterfaceType.None)
            };

            _initDirStart = Intent.GetStringExtra(ExtraInitDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _deviceName = Intent.GetStringExtra(ExtraDeviceName);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);

            EdiabasClose();
            UpdateDisplay();
        }

        protected override void OnResume()
        {
            base.OnResume();

            _activityActive = true;
            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.None)
            {
                SelectInterface();
            }
            SelectInterfaceEnable();
            SupportInvalidateOptionsMenu();
        }

        protected override void OnPause()
        {
            base.OnPause();

            _activityActive = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _runContinuous = false;
            _ediabasJobAbort = true;
            if (IsJobRunning())
            {
                _jobThread.Join();
            }
            EdiabasClose();
            _activityCommon.Dispose();
        }

        public override void OnBackPressed()
        {
            if (!IsJobRunning())
            {
                if (!SendTraceFile((sender, args) =>
                {
                    base.OnBackPressed();
                }))
                {
                    base.OnBackPressed();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestSelectSgbd:
                    // When FilePickerActivity returns with a file
                    if (resultCode == Android.App.Result.Ok)
                    {
                        _sgbdFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        SupportInvalidateOptionsMenu();
                        ReadSgbd();
                    }
                    break;

                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _deviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _deviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        bool callAdapterConfig = data.Extras.GetBoolean(DeviceListActivity.ExtraCallAdapterConfig, false);
                        EdiabasClose();
                        SupportInvalidateOptionsMenu();
                        if (callAdapterConfig)
                        {
                            AdapterConfig();
                        }
                        else if (_autoStart)
                        {
                            SelectSgbdFile(_autoStartItemId == Resource.Id.menu_tool_sel_sgbd_grp);
                        }
                    }
                    _autoStart = false;
                    break;

                case ActivityRequest.RequestCanAdapterConfig:
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.ediabas_tool_menu, menu);
            return true;
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

            IMenuItem selSgbdGrpMenu = menu.FindItem(Resource.Id.menu_tool_sel_sgbd_grp);
            if (selSgbdGrpMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(_sgbdFileName))
                {
                    bool groupFile = string.Compare(Path.GetExtension(_sgbdFileName), ".grp", StringComparison.OrdinalIgnoreCase) == 0;
                    if (groupFile)
                    {
                        fileName = Path.GetFileNameWithoutExtension(_sgbdFileName);
                    }
                }
                selSgbdGrpMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_sgbd_grp), fileName));
                selSgbdGrpMenu.SetEnabled(!commActive && interfaceAvailable);
            }

            IMenuItem selSgbdPrgMenu = menu.FindItem(Resource.Id.menu_tool_sel_sgbd_prg);
            if (selSgbdPrgMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(_sgbdFileName))
                {
                    if (_ediabas != null && !string.IsNullOrEmpty(_ediabas.SgbdFileName))
                    {
                        fileName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName);
                    }
                }
                selSgbdPrgMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_sgbd_prg), fileName));
                selSgbdPrgMenu.SetEnabled(!commActive && interfaceAvailable);
            }

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_adapter), _deviceName));
                scanMenu.SetEnabled(!commActive && interfaceAvailable);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem adapterConfigMenu = menu.FindItem(Resource.Id.menu_adapter_config);
            if (adapterConfigMenu != null)
            {
                adapterConfigMenu.SetEnabled(interfaceAvailable && !commActive);
                adapterConfigMenu.SetVisible(_activityCommon.AllowCanAdapterConfig(_deviceAddress));
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            if (logSubMenu != null)
            {
                logSubMenu.SetEnabled(interfaceAvailable && !commActive);
            }

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    if (!SendTraceFile((sender, args) =>
                    {
                        Finish();
                    }))
                    {
                        Finish();
                    }
                    return true;

                case Resource.Id.menu_tool_sel_interface:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectInterface();
                    return true;

                case Resource.Id.menu_tool_sel_sgbd_grp:
                case Resource.Id.menu_tool_sel_sgbd_prg:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    _autoStart = false;
                    if (string.IsNullOrEmpty(_deviceAddress))
                    {
                        if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, (sender, args) =>
                            {
                                _autoStart = true;
                                _autoStartItemId = item.ItemId;
                            }))
                        {
                            return true;
                        }
                    }
                    SelectSgbdFile(item.ItemId == Resource.Id.menu_tool_sel_sgbd_grp);
                    return true;

                case Resource.Id.menu_scan:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    _autoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice);
                    return true;

                case Resource.Id.menu_adapter_config:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    AdapterConfig();
                    return true;

                case Resource.Id.menu_submenu_log:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectDataLogging();
                    return true;

                case Resource.Id.menu_submenu_help:
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://ediabaslib.codeplex.com/wikipage?title=Ediabas Tool")));
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (v == _spinnerJobs)
                    {
                        DisplayJobComments();
                    }
                    else if (v == _spinnerResults)
                    {
                        DisplayJobResult();
                    }
                    else if (v == _editTextArgs)
                    {
                        DisplayJobArguments();
                        break;
                    }
                    HideKeyboard();
                    break;
            }
            return false;
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
            _jobList.Clear();
            CloseDataLog();
            UpdateDisplay();
            SupportInvalidateOptionsMenu();
            return true;
        }

        private void CloseDataLog()
        {
            if (_swDataLog != null)
            {
                _swDataLog.Dispose();
                _swDataLog = null;
            }
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
            _runContinuous = false;
            return false;
        }

        private void HideKeyboard()
        {
            if (_imm != null)
            {
                _imm.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
            }
        }

        private bool SendTraceFile(EventHandler<EventArgs> handler)
        {
            if (_commErrorsOccured && _traceActive && !string.IsNullOrEmpty(_traceDir))
            {
                EdiabasClose();
                return _activityCommon.RequestSendTraceFile(_traceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
            }
            return false;
        }

        private void UpdateDisplay()
        {
            bool checkContinuousEnable = true;
            bool buttonConnectEnable = true;
            bool inputsEnabled = true;
            if ((_ediabas == null) || (_jobList.Count == 0))
            {
                _jobListAdapter.Items.Clear();
                _jobListAdapter.NotifyDataSetChanged();
                _resultSelectListAdapter.Items.Clear();
                _resultSelectListAdapter.NotifyDataSetChanged();
                _infoListAdapter.Items.Clear();
                _infoListAdapter.NotifyDataSetChanged();
                inputsEnabled = false;
                checkContinuousEnable = false;
                buttonConnectEnable = false;
            }
            else
            {
                if (!_activityCommon.IsInterfaceAvailable())
                {
                    buttonConnectEnable = false;
                }
            }
            if (IsJobRunning())
            {
                checkContinuousEnable = false;
                buttonConnectEnable = _runContinuous;
                inputsEnabled = false;
            }
            _checkBoxContinuous.Enabled = checkContinuousEnable;
            _buttonConnect.Enabled = buttonConnectEnable;
            if (!buttonConnectEnable)
            {
                _buttonConnect.Checked = false;
            }
            _spinnerJobs.Enabled = inputsEnabled;
            _editTextArgs.Enabled = inputsEnabled;
            _spinnerResults.Enabled = inputsEnabled;

            HideKeyboard();
        }

        private void SelectSgbdFile(bool groupFile)
        {
            // Launch the FilePickerActivity to select a sgbd file
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = _initDirStart;
            try
            {
                if (!string.IsNullOrEmpty(_sgbdFileName))
                {
                    initDir = Path.GetDirectoryName(_sgbdFileName);
                }
            }
            catch (Exception)
            {
                initDir = _initDirStart;
            }
            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.tool_select_sgbd));
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, groupFile ? ".grp" : ".prg");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectSgbd);
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
                _sgbdFileName = string.Empty;
                SupportInvalidateOptionsMenu();
                SelectInterfaceEnable();
            });
        }

        private void SelectInterfaceEnable()
        {
            _activityCommon.RequestInterfaceEnable((sender, args) =>
            {
                SupportInvalidateOptionsMenu();
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
                GetString(Resource.String.datalog_enable_datalog),
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemMultipleChoice, logNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Multiple;
            listView.SetItemChecked(0, _traceActive);
            listView.SetItemChecked(1, _traceAppend);
            listView.SetItemChecked(2, _dataLogActive);

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
                            _traceActive = value;
                            break;

                        case 1:
                            _traceAppend = value;
                            break;

                        case 2:
                            _dataLogActive = value;
                            break;
                    }
                }
                if (!_dataLogActive)
                {
                    CloseDataLog();
                }
                UpdateLogInfo();
                SupportInvalidateOptionsMenu();
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            builder.Show();
        }

        private void AdapterConfig()
        {
            EdiabasClose();
            Intent serverIntent = new Intent(this, typeof(CanAdapterActivity));
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeviceAddress, _deviceAddress);
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestCanAdapterConfig);
        }

        private JobInfo GetSelectedJob()
        {
            int pos = _spinnerJobs.SelectedItemPosition;
            if (pos < 0)
            {
                return null;
            }
            return _jobListAdapter.Items[pos];
        }

        private void NewJobSelected()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _resultSelectListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                foreach (ExtraInfo result in jobInfo.Results.OrderBy(x => x.Name))
                {
                    _resultSelectListAdapter.Items.Add(result);
                }
            }
            _resultSelectListAdapter.NotifyDataSetChanged();
            _resultSelectLastItem = 0;
            _editTextArgs.Text = string.Empty;
        }

        private void DisplayJobComments()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_job), null));
                StringBuilder stringBuilderComments = new StringBuilder();
                stringBuilderComments.Append(jobInfo.Name);
                stringBuilderComments.Append(":");
                foreach (string comment in jobInfo.Comments)
                {
                    stringBuilderComments.Append("\r\n");
                    stringBuilderComments.Append(comment);
                }
                _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        private void DisplayJobArguments()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_arguments), null));
                if (string.Compare(jobInfo.Name, "FS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_arguments_error_detail), null));
                }
                foreach (ExtraInfo info in jobInfo.Arguments.OrderBy(x => x.Name))
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                    _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        private void DisplayJobResult()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_result), null));
                if (_spinnerResults.SelectedItemPosition >= 0)
                {
                    ExtraInfo info = _resultSelectListAdapter.Items[_spinnerResults.SelectedItemPosition];
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                    _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        private void UpdateLogInfo()
        {
            if (_ediabas == null)
            {
                return;
            }

            string logDir = Path.Combine(_appDataDir, "LogEdiabasTool");
            try
            {
                Directory.CreateDirectory(logDir);
            }
            catch (Exception)
            {
                logDir = string.Empty;
            }
            _dataLogDir = logDir;

            _traceDir = null;
            if (_traceActive && !string.IsNullOrEmpty(_sgbdFileName))
            {
                _traceDir = logDir;
            }

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

        private void ReadSgbd()
        {
            if (string.IsNullOrEmpty(_sgbdFileName))
            {
                return;
            }
            CloseDataLog();
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet();
                if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet)
                {
                    _ediabas.EdInterfaceClass = new EdInterfaceEnet();
                }
                else
                {
                    _ediabas.EdInterfaceClass = new EdInterfaceObd();
                }
                _ediabas.AbortJobFunc = AbortEdiabasJob;
                _ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(_sgbdFileName));
            }
            _jobList.Clear();
            UpdateDisplay();

            _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress);

            UpdateLogInfo();

            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.tool_read_sgbd));
            progress.Show();

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                List<string> messageList = new List<string>();
                try
                {
                    _ediabas.ResolveSgbdFile(_sgbdFileName);

                    _ediabas.ArgString = string.Empty;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("_JOBS");

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
                            if (resultDict.TryGetValue("JOBNAME", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    _jobList.Add(new JobInfo((string)resultData.OpData));
                                }
                            }
                            dictIndex++;
                        }
                    }

                    foreach (JobInfo job in _jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("_JOBCOMMENTS");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            for (int i = 0; ; i++)
                            {
                                EdiabasNet.ResultData resultData;
                                if (resultDict.TryGetValue("JOBCOMMENT" + i.ToString(Culture), out resultData))
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

                    foreach (JobInfo job in _jobList)
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
                                EdiabasNet.ResultData resultData;
                                string arg = string.Empty;
                                string argType = string.Empty;
                                List<string> argCommentList = new List<string>();
                                if (resultDict.TryGetValue("ARG", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        arg = (string)resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("ARGTYPE", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        argType = (string)resultData.OpData;
                                    }
                                }
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("ARGCOMMENT" + i.ToString(Culture), out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            argCommentList.Add((string)resultData.OpData);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                job.Arguments.Add(new ExtraInfo(arg, argType, argCommentList));
                                dictIndex++;
                            }
                        }
                    }

                    foreach (JobInfo job in _jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("_RESULTS");

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
                                EdiabasNet.ResultData resultData;
                                string result = string.Empty;
                                string resultType = string.Empty;
                                List<string> resultCommentList = new List<string>();
                                if (resultDict.TryGetValue("RESULT", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        result = (string)resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("RESULTTYPE", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        resultType = (string)resultData.OpData;
                                    }
                                }
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("RESULTCOMMENT" + i.ToString(Culture), out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            resultCommentList.Add((string)resultData.OpData);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                job.Results.Add(new ExtraInfo(result, resultType, resultCommentList));
                                dictIndex++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string exceptionText = String.Empty;
                    if (!AbortEdiabasJob())
                    {
                        exceptionText = EdiabasNet.GetExceptionText(ex);
                    }
                    messageList.Add(exceptionText);
                    if (ActivityCommon.IsCommunicationError(exceptionText))
                    {
                        _commErrorsOccured = true;
                    }
                }

                RunOnUiThread(() =>
                {
                    progress.Hide();
                    progress.Dispose();

                    if (IsJobRunning())
                    {
                        _jobThread.Join();
                    }
                    _infoListAdapter.Items.Clear();
                    foreach (string message in messageList)
                    {
                        _infoListAdapter.Items.Add(new TableResultItem(message, null));
                    }
                    _infoListAdapter.NotifyDataSetChanged();

                    _jobListAdapter.Items.Clear();
                    foreach (JobInfo job in _jobList.OrderBy(x => x.Name))
                    {
                        _jobListAdapter.Items.Add(job);
                    }
                    _jobListAdapter.NotifyDataSetChanged();

                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                });
            });
            _jobThread.Start();
        }

        private void ExecuteSelectedJob(bool continuous)
        {
            _infoListAdapter.Items.Clear();
            _infoListAdapter.NotifyDataSetChanged();
            if (_ediabas == null)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            if (jobInfo == null)
            {
                return;
            }
            if (IsJobRunning())
            {
                return;
            }

            string jobName = jobInfo.Name;
            string jobArgs = _editTextArgs.Text;
            StringBuilder stringBuilderResults = new StringBuilder();
            foreach (ExtraInfo info in jobInfo.Results)
            {
                if (info.Selected)
                {
                    if (stringBuilderResults.Length > 0)
                    {
                        stringBuilderResults.Append(";");
                    }
                    stringBuilderResults.Append(info.Name);
                }
            }
            string jobResults = stringBuilderResults.ToString();
            _runContinuous = continuous;
            if (_dataLogActive)
            {
                _activityCommon.SetCpuLock(true);
            }
            else
            {
                _activityCommon.SetScreenLock(true);
            }

            _jobThread = new Thread(() =>
            {
                for (; ; )
                {
                    List<string> messageList = new List<string>();
                    try
                    {
                        if (string.Compare(jobName, "FS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0 &&
                            string.IsNullOrEmpty(jobArgs))
                        {
                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob("FS_LESEN");

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;

                            bool jobOk = false;
                            if (resultSets != null && resultSets.Count > 1)
                            {
                                EdiabasNet.ResultData resultData;
                                if (resultSets[resultSets.Count - 1].TryGetValue("JOB_STATUS", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        // read details
                                        string jobStatus = (string) resultData.OpData;
                                        if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            jobOk = true;
                                        }
                                    }
                                }
                            }
                            // read error details
                            if (jobOk)
                            {
                                int dictIndex = 0;
                                foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                                {
                                    if (dictIndex == 0)
                                    {
                                        dictIndex++;
                                        continue;
                                    }

                                    EdiabasNet.ResultData resultData;
                                    if (resultDictLocal.TryGetValue("F_ORT_NR", out resultData))
                                    {
                                        if (resultData.OpData is Int64)
                                        {
                                            // read details
                                            _ediabas.ArgString = string.Format("0x{0:X02}", (Int64) resultData.OpData);
                                            _ediabas.ArgBinaryStd = null;
                                            _ediabas.ResultsRequests = jobResults;

                                            _ediabas.ExecuteJob("FS_LESEN_DETAIL");

                                            List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail =
                                                new List<Dictionary<string, EdiabasNet.ResultData>>(_ediabas.ResultSets);
                                            PrintResults(messageList, resultSetsDetail);
                                        }
                                    }
                                    dictIndex++;
                                }
                                if (messageList.Count == 0)
                                {
                                    messageList.Add(GetString(Resource.String.tool_no_errors));
                                }
                            }
                            else
                            {
                                messageList.Add(GetString(Resource.String.tool_read_errors_failure));
                            }
                        }
                        else
                        {
                            _ediabas.ArgString = jobArgs;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = jobResults;
                            _ediabas.ExecuteJob(jobName);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            PrintResults(messageList, resultSets);
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionText = String.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exceptionText = EdiabasNet.GetExceptionText(ex);
                        }
                        messageList.Add(exceptionText);
                        if (ActivityCommon.IsCommunicationError(exceptionText))
                        {
                            _commErrorsOccured = true;
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        _infoListAdapter.Items.Clear();
                        foreach (string message in messageList)
                        {
                            _infoListAdapter.Items.Add(new TableResultItem(message, null));
                        }
                        _infoListAdapter.NotifyDataSetChanged();
                        UpdateDisplay();
                    });

                    if (!_runContinuous)
                    {
                        break;
                    }
                }

                RunOnUiThread(() =>
                {
                    if (IsJobRunning())
                    {
                        _jobThread.Join();
                    }
                    _activityCommon.SetScreenLock(false);
                    _activityCommon.SetCpuLock(false);
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                });
            });
            _jobThread.Start();
            SupportInvalidateOptionsMenu();
            UpdateDisplay();
        }

        private void PrintResults(List<string> messageList, List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            int dataSet = 0;
            if (resultSets != null)
            {
                if (_ediabas != null && _dataLogActive && _swDataLog == null &&
                    !string.IsNullOrEmpty(_dataLogDir) && !string.IsNullOrEmpty(_ediabas.SgbdFileName))
                {
                    try
                    {
                        string fileName = Path.Combine(_dataLogDir, Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName)) + ".log";
                        FileMode fileMode = File.Exists(fileName) ? FileMode.Append : FileMode.Create;
                        _swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (_swDataLog != null)
                {
                    _swDataLog.Write("----------------------------------------\r\n");
                }
                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(string.Format(Culture, "DATASET: {0}", dataSet));
                    foreach (string key in resultDict.Keys.OrderBy(x => x))
                    {
                        EdiabasNet.ResultData resultData = resultDict[key];
                        string resultText = string.Empty;
                        if (resultData.OpData is string)
                        {
                            resultText = (string)resultData.OpData;
                        }
                        else if (resultData.OpData is double)
                        {
                            resultText = string.Format(Culture, "R: {0}", (Double)resultData.OpData);
                        }
                        else if (resultData.OpData is long)
                        {
                            Int64 value = (Int64)resultData.OpData;
                            switch (resultData.ResType)
                            {
                                case EdiabasNet.ResultType.TypeB:  // 8 bit
                                    resultText = string.Format(Culture, "B: {0} 0x{1:X02}", value, (Byte)value);
                                    break;

                                case EdiabasNet.ResultType.TypeC:  // 8 bit char
                                    resultText = string.Format(Culture, "C: {0} 0x{1:X02}", value, (Byte)value);
                                    break;

                                case EdiabasNet.ResultType.TypeW:  // 16 bit
                                    resultText = string.Format(Culture, "W: {0} 0x{1:X04}", value, (UInt16)value);
                                    break;

                                case EdiabasNet.ResultType.TypeI:  // 16 bit signed
                                    resultText = string.Format(Culture, "I: {0} 0x{1:X04}", value, (UInt16)value);
                                    break;

                                case EdiabasNet.ResultType.TypeD:  // 32 bit
                                    resultText = string.Format(Culture, "D: {0} 0x{1:X08}", value, (UInt32)value);
                                    break;

                                case EdiabasNet.ResultType.TypeL:  // 32 bit signed
                                    resultText = string.Format(Culture, "L: {0} 0x{1:X08}", value, (UInt32)value);
                                    break;

                                default:
                                    resultText = "?";
                                    break;
                            }
                        }
                        else if (resultData.OpData.GetType() == typeof(byte[]))
                        {
                            StringBuilder sb = new StringBuilder();
                            byte[] data = (byte[])resultData.OpData;
                            foreach (byte value in data)
                            {
                                sb.Append(string.Format(Culture, "{0:X02} ", value));
                            }
                            resultText = sb.ToString();
                        }
                        stringBuilder.Append("\r\n");
                        stringBuilder.Append(resultData.Name + ": " + resultText);
                    }
                    messageList.Add(stringBuilder.ToString());
                    if (_swDataLog != null)
                    {
                        _swDataLog.Write(stringBuilder.ToString());
                        _swDataLog.Write("\r\n");
                        _swDataLog.Write("\r\n");
                    }
                    dataSet++;
                }
            }
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
                return;
            }
            string action = intent.Action;
            switch (action)
            {
                case UsbManager.ActionUsbDeviceDetached:
                    EdiabasClose();
                    break;
            }
        }

        private class JobListAdapter : BaseAdapter<JobInfo>
        {
            private readonly List<JobInfo> _items;
            public List<JobInfo> Items
            {
                get
                {
                    return _items;
                }
            }
            private readonly Android.App.Activity _context;
            private readonly Android.Graphics.Color _backgroundColor;

            public JobListAdapter(Android.App.Activity context)
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

            public override JobInfo this[int position]
            {
                get { return _items[position]; }
            }

            public override int Count
            {
                get { return _items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.job_list, null);
                view.SetBackgroundColor(_backgroundColor);
                TextView textName = view.FindViewById<TextView>(Resource.Id.textJobName);
                TextView textDesc = view.FindViewById<TextView>(Resource.Id.textJobDesc);
                textName.Text = item.Name;
                if (item.Comments.Count > 0)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    foreach (string comment in item.Comments)
                    {
                        stringBuilderComments.Append(comment + " ");
                    }
                    textDesc.Text = stringBuilderComments.ToString();
                }
                else
                {
                    textDesc.Text = " ";
                }

                return view;
            }
        }

        private class ResultSelectListAdapter : BaseAdapter<ExtraInfo>
        {
            private readonly List<ExtraInfo> _items;
            public List<ExtraInfo> Items
            {
                get
                {
                    return _items;
                }
            }
            private readonly Android.App.Activity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public ResultSelectListAdapter(Android.App.Activity context)
            {
                _context = context;
                _items = new List<ExtraInfo>();
                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new[] { Android.Resource.Attribute.ColorBackground });
                _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ExtraInfo this[int position]
            {
                get { return _items[position]; }
            }

            public override int Count
            {
                get { return _items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ediabas_result_list, null);
                view.SetBackgroundColor(_backgroundColor);
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxResultSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textResultName = view.FindViewById<TextView>(Resource.Id.textResultName);
                TextView textResultDesc = view.FindViewById<TextView>(Resource.Id.textResultDesc);
                textResultName.Text = item.Name;
                if (item.CommentList.Count > 0)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    foreach (string comment in item.CommentList)
                    {
                        stringBuilderComments.Append(comment + " ");
                    }
                    textResultDesc.Text = stringBuilderComments.ToString();
                }
                else
                {
                    textResultDesc.Text = " ";
                }

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
                    }
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(ExtraInfo info)
                {
                    _info = info;
                }

                private readonly ExtraInfo _info;

                public ExtraInfo Info
                {
                    get
                    {
                        return _info;
                    }
                }
            }
        }
    }
}
