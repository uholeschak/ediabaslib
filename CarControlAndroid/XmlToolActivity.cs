using System;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Android.Bluetooth;
using Android.Content;
using Android.Net;
using Android.Widget;
using CarControlAndroid.FilePicker;
using EdiabasLib;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace CarControlAndroid
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
            RequestSelectJobs,
        }

        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string description, string sgbd, string grp)
            {
                _name = name;
                _address = address;
                _description = description;
                _sgbd = sgbd;
                _grp = grp;
                Selected = false;
                PageName = name;
                JobList = null;
            }

            private readonly string _name;
            private readonly Int64 _address;
            private readonly string _description;
            private readonly string _sgbd;
            private readonly string _grp;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public Int64 Address
            {
                get
                {
                    return _address;
                }
            }

            public string Description
            {
                get
                {
                    return _description;
                }
            }

            public string Sgbd
            {
                get
                {
                    return _sgbd;
                }
            }

            public string Grp
            {
                get
                {
                    return _grp;
                }
            }

            public string Vin { get; set; }

            public bool Selected { get; set; }

            public string PageName { get; set; }

            public List<XmlToolEcuActivity.JobInfo> JobList { get; set; }
        }

        private const string XmlDocumentFrame =
            @"<?xml version=""1.0"" encoding=""utf-8"" ?>
            <{0} xmlns=""http://www.holeschak.de/CarControl""
            xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
            xsi:schemaLocation=""http://www.holeschak.de/CarControl ../CarControl.xsd"">
            </{0}>";

        private const string PageExtension = ".ccpage";
        private const string PagesFileName = "Pages.ccpages";
        private const string ConfigFileNameBt = "Bluetooth.cccfg";
        private const string ConfigFileNameEnet = "Enet.cccfg";
        private const string DisplayNamePage = "!PAGE_NAME";
        private const string DisplayNamePrefix = "!JOB#";

        // Intent extra
        public const string ExtraInitDir = "init_dir";
        public const string ExtraXmlDir = "xml_dir";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraFileName = "file_name";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");

        private View _barView;
        private Button _buttonRead;
        private Button _buttonSafe;
        private EcuListAdapter _ecuListAdapter;
        private TextView _textViewCarInfo;
        private string _initDirStart;
        private string _xmlBaseDir;
        private string _sgbdFileName = string.Empty;
        private string _deviceName = string.Empty;
        private string _deviceAddress = string.Empty;
        private bool _traceActive;
        private bool _traceAppend;
        private volatile bool _ediabasJobAbort;
        private bool _autoStart;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private Task _jobTask;
        private Receiver _receiver;
        private string _statusText = string.Empty;
        private string _vin = string.Empty;
        private readonly List<EcuInfo> _ecuList = new List<EcuInfo>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

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

            _buttonRead = _barView.FindViewById<Button>(Resource.Id.buttonXmlRead);
            _buttonRead.Click += (sender, args) =>
            {
                PerformAnalyze();
            };
            _buttonSafe = _barView.FindViewById<Button>(Resource.Id.buttonXmlSafe);
            _buttonSafe.Click += (sender, args) =>
            {
                if (IsJobRunning())
                {
                    return;
                }
                string xmlFileName = SaveAllXml();
                if (xmlFileName == null)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.xml_tool_save_xml_failed));
                }
                else
                {
                    Intent intent = new Intent();
                    intent.PutExtra(ExtraFileName, xmlFileName);

                    // Set result and finish this Activity
                    SetResult(Android.App.Result.Ok, intent);
                    Finish();
                }
            };

            SetResult(Android.App.Result.Canceled);

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

            _activityCommon = new ActivityCommon(this)
            {
                SelectedInterface = (ActivityCommon.InterfaceType)
                    Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None)
            };

            _initDirStart = Intent.GetStringExtra(ExtraInitDir);
            _xmlBaseDir = Intent.GetStringExtra(ExtraXmlDir);
            _deviceName = Intent.GetStringExtra(ExtraDeviceName);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);

            EdiabasClose();
            UpdateDisplay();

            _receiver = new Receiver(this);
            RegisterReceiver(_receiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));
            RegisterReceiver(_receiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.None)
            {
                SelectInterface();
            }
            SelectInterfaceEnable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(_receiver);
            _ediabasJobAbort = true;
            if (IsJobRunning())
            {
                _jobTask.Wait();
            }
            EdiabasClose();
        }

        public override void OnBackPressed()
        {
            if (!IsJobRunning())
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
                    if (resultCode == Android.App.Result.Ok)
                    {
                        _sgbdFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        SupportInvalidateOptionsMenu();
                        ExecuteAnalyzeJob();
                    }
                    break;

                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _deviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _deviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        EdiabasClose();
                        SupportInvalidateOptionsMenu();
                        if (_autoStart)
                        {
                            SelectSgbdFile();
                        }
                    }
                    _autoStart = false;
                    break;

                case ActivityRequest.RequestSelectJobs:
                    if (XmlToolEcuActivity.IntentEcuInfo.JobList != null)
                    {
                        int selectCount = XmlToolEcuActivity.IntentEcuInfo.JobList.Count(job => job.Selected);
                        XmlToolEcuActivity.IntentEcuInfo.Selected = selectCount > 0;
                        _ecuListAdapter.NotifyDataSetChanged();
                    }
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.xml_tool_menu, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            bool commActive = IsJobRunning();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable();

            IMenuItem selInterfaceMenu = menu.FindItem(Resource.Id.menu_tool_sel_interface);
            if (selInterfaceMenu != null)
            {
                string interfaceName = string.Empty;
                switch (_activityCommon.SelectedInterface)
                {
                    case ActivityCommon.InterfaceType.Bluetooth:
                        interfaceName = GetString(Resource.String.select_interface_bt);
                        break;

                    case ActivityCommon.InterfaceType.Enet:
                        interfaceName = GetString(Resource.String.select_interface_enet);
                        break;
                }
                selInterfaceMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_interface), interfaceName));
                selInterfaceMenu.SetEnabled(!commActive);
            }

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_device), _deviceName));
                scanMenu.SetEnabled(!commActive && interfaceAvailable);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem enableTraceMenu = menu.FindItem(Resource.Id.menu_enable_trace);
            if (enableTraceMenu != null)
            {
                enableTraceMenu.SetEnabled(interfaceAvailable && !commActive);
                enableTraceMenu.SetChecked(_traceActive);
            }

            IMenuItem appendTraceMenu = menu.FindItem(Resource.Id.menu_append_trace);
            if (appendTraceMenu != null)
            {
                appendTraceMenu.SetEnabled(interfaceAvailable && !commActive);
                appendTraceMenu.SetChecked(_traceAppend);
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
                    Finish();
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
                    _autoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice);
                    return true;

                case Resource.Id.menu_enable_trace:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    _traceActive = !_traceActive;
                    UpdateLogInfo();
                    SupportInvalidateOptionsMenu();
                    return true;

                case Resource.Id.menu_append_trace:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    _traceAppend = !_traceAppend;
                    UpdateLogInfo();
                    SupportInvalidateOptionsMenu();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
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
            UpdateDisplay();
            return true;
        }

        private bool IsJobRunning()
        {
            if (_jobTask == null)
            {
                return false;
            }
            if (!_jobTask.IsCompleted)
            {
                return true;
            }
            _jobTask.Dispose();
            _jobTask = null;
            return false;
        }

        private void UpdateDisplay()
        {
            _ecuListAdapter.Items.Clear();
            if ((_ediabas == null) || (_ecuList.Count == 0))
            {
                if (_ediabas == null) _statusText = string.Empty;
                _vin = string.Empty;
                _ecuList.Clear();
            }
            else
            {
                foreach (EcuInfo ecu in _ecuList)
                {
                    _ecuListAdapter.Items.Add(ecu);
                }
            }

            _buttonRead.Enabled = _activityCommon.IsInterfaceAvailable();
            _buttonSafe.Enabled = _ecuList.Count > 0;
            _ecuListAdapter.NotifyDataSetChanged();

            _textViewCarInfo.Text = _statusText;
        }

        private void UpdateLogInfo()
        {
            if (_ediabas == null)
            {
                return;
            }
            string logDir = string.IsNullOrEmpty(_activityCommon.ExternalWritePath) ? Path.GetDirectoryName(_sgbdFileName) : _activityCommon.ExternalWritePath;

            if (!string.IsNullOrEmpty(logDir))
            {
                logDir = Path.Combine(logDir, "LogXmlTool");
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch (Exception)
                {
                    logDir = string.Empty;
                }
            }
            else
            {
                logDir = string.Empty;
            }

            string traceDir = null;
            if (_traceActive && !string.IsNullOrEmpty(_sgbdFileName))
            {
                traceDir = logDir;
            }

            if (!string.IsNullOrEmpty(traceDir))
            {
                _ediabas.SetConfigProperty("TracePath", traceDir);
                _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                _ediabas.SetConfigProperty("AppendTrace", _traceAppend ? "1" : "0");
            }
            else
            {
                _ediabas.SetConfigProperty("IfhTrace", "0");
            }
        }

        private void SelectSgbdFile()
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
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".prg");
            serverIntent.PutExtra(FilePickerActivity.ExtraFileRegex, @"^([efmr]|rr)[0-9]{2}[^_].");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectSgbd);
        }

        private void SelectJobs(EcuInfo ecuInfo)
        {
            if (ecuInfo.JobList == null)
            {
                return;
            }
            XmlToolEcuActivity.IntentEcuInfo = ecuInfo;
            Intent serverIntent = new Intent(this, typeof(XmlToolEcuActivity));
            serverIntent.PutExtra(XmlToolEcuActivity.ExtraEcuName, ecuInfo.Name);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectJobs);
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

        private void PerformAnalyze()
        {
            if (IsJobRunning())
            {
                return;
            }
            _autoStart = false;
            if (string.IsNullOrEmpty(_deviceAddress))
            {
                if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, (sender, args) =>
                {
                    _autoStart = true;
                }))
                {
                    return;
                }
            }
            SelectSgbdFile();
        }

        private void ExecuteAnalyzeJob()
        {
            if (string.IsNullOrEmpty(_sgbdFileName))
            {
                return;
            }
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
            _statusText = string.Empty;
            _vin = string.Empty;
            _ecuList.Clear();
            UpdateDisplay();

            _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress);

            UpdateLogInfo();

            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            progress.Show();

            _ediabasJobAbort = false;
            _jobTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    _ediabas.ResolveSgbdFile(_sgbdFileName);

                    _ediabas.ArgString = string.Empty;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("IDENT_FUNKTIONAL");

                    List<EcuInfo> ecuList = new List<EcuInfo>();
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
                            string ecuName = string.Empty;
                            Int64 ecuAdr = -1;
                            string ecuDesc = string.Empty;
                            string ecuSgbd = string.Empty;
                            string ecuGroup = string.Empty;
                            EdiabasNet.ResultData resultData;
                            if (resultDict.TryGetValue("ECU_GROBNAME", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    ecuName = (string) resultData.OpData;
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
                                if (resultData.OpData is string)
                                {
                                    ecuSgbd = (string)resultData.OpData;
                                }
                            }
                            if (resultDict.TryGetValue("ECU_GRUPPE", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    ecuGroup = (string)resultData.OpData;
                                }
                            }
                            if (!string.IsNullOrEmpty(ecuName) && ecuAdr >= 0 && !string.IsNullOrEmpty(ecuSgbd))
                            {
                                ecuList.Add(new EcuInfo(ecuName, ecuAdr, ecuDesc, ecuSgbd, ecuGroup));
                            }
                            dictIndex++;
                        }
                        _ecuList.AddRange(ecuList.OrderBy(x => x.Name));
                    }

                    try
                    {
                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("C_FG_LESEN_FUNKTIONAL");

                        Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
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
                                Int64 ecuAdr = -1;
                                string ecuVin = string.Empty;
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
                                if (!string.IsNullOrEmpty(ecuVin) && regex.IsMatch(ecuVin))
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
                    _vin = _ecuList.GroupBy(x => x.Vin).Where(x => !string.IsNullOrEmpty(x.Key)).OrderByDescending(x => x.Count()).First().Key;
                    _statusText = GetString(Resource.String.xml_tool_info_vin) + ": " + _vin;
                    ReadAllXml();
                }
                catch (Exception)
                {
                    _statusText = GetString(Resource.String.xml_tool_analyze_failed);
                }

                RunOnUiThread(() =>
                {
                    progress.Hide();
                    progress.Dispose();

                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                });
            });
        }

        private void ExecuteJobsRead(EcuInfo ecuInfo)
        {
            if (_ediabas == null)
            {
                return;
            }
            if (ecuInfo.JobList != null)
            {
                SelectJobs(ecuInfo);
                return;
            }
            _statusText = GetString(Resource.String.xml_tool_info_vin) + ": " + _vin;

            UpdateDisplay();

            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.xml_tool_analyze));
            progress.Show();

            bool readFailed = false;
            _ediabasJobAbort = false;
            _jobTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    _ediabas.ResolveSgbdFile(ecuInfo.Sgbd);

                    _ediabas.ArgString = string.Empty;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
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
                            EdiabasNet.ResultData resultData;
                            if (resultDict.TryGetValue("JOBNAME", out resultData))
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
                                EdiabasNet.ResultData resultData;
                                uint argCount = 0;
                                if (resultDict.TryGetValue("ARG", out resultData))
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

                    foreach (XmlToolEcuActivity.JobInfo job in jobList)
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
                                job.Results.Add(new XmlToolEcuActivity.ResultInfo(result, resultType, resultCommentList));
                                dictIndex++;
                            }
                        }
                    }

                    ecuInfo.JobList = jobList;

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
                catch (Exception)
                {
                    readFailed = true;
                    _statusText = GetString(Resource.String.xml_tool_read_jobs_failed);
                }

                RunOnUiThread(() =>
                {
                    progress.Hide();
                    progress.Dispose();

                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                    if (!readFailed && ecuInfo.JobList.Count > 0)
                    {
                        SelectJobs(ecuInfo);
                    }
                });
            });
        }

        private void EcuCheckChanged(EcuInfo ecuInfo)
        {
            if (ecuInfo.Selected)
            {
                ExecuteJobsRead(ecuInfo);
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
                        XElement displayNode = GetDisplayNode(result, ns, jobNode);
                        if (displayNode != null)
                        {
                            result.Selected = true;
                            XAttribute formatAttr = displayNode.Attribute("format");
                            if (formatAttr != null)
                            {
                                result.Format = formatAttr.Value;
                            }
                        }
                        if (stringsNode != null)
                        {
                            string displayTag = DisplayNamePrefix + job.Name + "#" + result.Name;
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

        private XDocument GeneratePageXml(EcuInfo ecuInfo, XDocument documentOld)
        {
            try
            {
                if (ecuInfo.JobList == null)
                {
                    return null;
                }
                XDocument document = documentOld;
                if ((document == null) || (document.Root == null))
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
                {   // remove all generated entries
                    List<XElement> removeList =
                        (from node in stringsNode.Elements(ns + "string")
                         let nameAttr = node.Attribute("name")
                         where nameAttr != null &&
                         (nameAttr.Value.StartsWith(DisplayNamePrefix, StringComparison.Ordinal) || string.Compare(nameAttr.Value, DisplayNamePage, StringComparison.Ordinal) == 0)
                         select node).ToList();
                    foreach (XElement node in removeList)
                    {
                        node.Remove();
                    }
                }

                XElement stringNodePage = new XElement(ns + "string", ecuInfo.PageName);
                stringNodePage.Add(new XAttribute("name", DisplayNamePage));
                stringsNode.Add(stringNodePage);

                XElement jobsNode = pageNode.Element(ns + "jobs");
                if (jobsNode == null)
                {
                    jobsNode = new XElement(ns + "jobs");
                    pageNode.Add(jobsNode);
                }
                else
                {
                    XAttribute attr = jobsNode.Attribute("sgbd");
                    if (attr != null) attr.Remove();
                }

                jobsNode.Add(new XAttribute("sgbd", ecuInfo.Sgbd));

                foreach (XmlToolEcuActivity.JobInfo job in ecuInfo.JobList)
                {
                    XElement jobNode = GetJobNode(job, ns, jobsNode);
                    if (!job.Selected)
                    {
                        if (jobNode != null)
                        {
                            jobNode.Remove();
                        }
                        continue;
                    }
                    if (jobNode == null)
                    {
                        jobNode = new XElement(ns + "job");
                        jobsNode.Add(jobNode);
                    }
                    else
                    {
                        XAttribute attr = jobNode.Attribute("name");
                        if (attr != null) attr.Remove();
                    }
                    jobNode.Add(new XAttribute("name", job.Name));

                    foreach (XmlToolEcuActivity.ResultInfo result in job.Results)
                    {
                        XElement displayNode = GetDisplayNode(result, ns, jobNode);
                        if (!result.Selected)
                        {
                            if (displayNode != null)
                            {
                                displayNode.Remove();
                            }
                            continue;
                        }
                        if (displayNode == null)
                        {
                            displayNode = new XElement(ns + "display");
                            jobNode.Add(displayNode);
                        }
                        else
                        {
                            XAttribute attr = displayNode.Attribute("name");
                            if (attr != null)
                            {
                                if (attr.Value.StartsWith(DisplayNamePrefix, StringComparison.Ordinal))
                                {
                                    attr.Remove();
                                }
                            }
                            attr = displayNode.Attribute("result");
                            if (attr != null) attr.Remove();
                            attr = displayNode.Attribute("format");
                            if (attr != null) attr.Remove();
                        }
                        string displayTag = DisplayNamePrefix + job.Name + "#" + result.Name;
                        if (displayNode.Attribute("name") == null)
                        {
                            displayNode.Add(new XAttribute("name", displayTag));
                        }
                        displayNode.Add(new XAttribute("result", result.Name));
                        displayNode.Add(new XAttribute("format", result.Format));

                        XElement stringNode = new XElement(ns + "string", result.DisplayText);
                        stringNode.Add(new XAttribute("name", displayTag));
                        stringsNode.Add(stringNode);
                    }
                }

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

            foreach (EcuInfo ecuInfo in _ecuList)
            {
                string fileName = ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension);
                XElement fileNode = GetFileNode(fileName, ns, pagesNode);
                if (fileNode != null && File.Exists(Path.Combine(xmlFileDir, fileName)))
                {
                    ecuInfo.Selected = true;
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
                if ((document == null) || (document.Root == null))
                {
                    document = XDocument.Parse(string.Format(XmlDocumentFrame, "fragment"));
                }
                if (document.Root == null)
                {
                    return null;
                }
                XNamespace ns = document.Root.GetDefaultNamespace();
                XElement pagesNode = document.Root.Element(ns + "pages");
                if (pagesNode == null)
                {
                    pagesNode = new XElement(ns + "pages");
                    document.Root.Add(pagesNode);
                }

                foreach (EcuInfo ecuInfo in _ecuList)
                {
                    string fileName = ActivityCommon.CreateValidFileName(ecuInfo.Name + PageExtension);
                    XElement fileNode = GetFileNode(fileName, ns, pagesNode);
                    if (!ecuInfo.Selected || !File.Exists(Path.Combine(xmlFileDir, fileName)))
                    {
                        if (fileNode != null)
                        {
                            fileNode.Remove();
                        }
                        continue;
                    }
                    if (fileNode == null)
                    {
                        fileNode = new XElement(ns + "include");
                        pagesNode.Add(fileNode);
                    }
                    else
                    {
                        XAttribute attr = fileNode.Attribute("filename");
                        if (attr != null) attr.Remove();
                    }

                    fileNode.Add(new XAttribute("filename", fileName));
                }

                return document;
            }
            catch (Exception)
            {
                return null;
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
                if ((document == null) || (document.Root == null))
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
                    if (attr != null) attr.Remove();
                    attr = globalNode.Attribute("interface");
                    if (attr != null) attr.Remove();
                }

                string ecuDir = Path.GetDirectoryName(_sgbdFileName) ?? string.Empty;
                globalNode.Add(new XAttribute("ecu_path", ActivityCommon.MakeRelativePath(xmlFileDir + Path.DirectorySeparatorChar, ecuDir + Path.DirectorySeparatorChar)));

                XAttribute logPathAttr = globalNode.Attribute("log_path");
                if (logPathAttr == null)
                {
                    globalNode.Add(new XAttribute("log_path", "Log"));
                }

                globalNode.Add(new XAttribute("interface",
                    (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet) ? "ENET" : "BLUETOOTH"));

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

        private void ReadAllXml()
        {
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
                string xmlConfigFile = Path.Combine(xmlFileDir,
                    (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet) ? ConfigFileNameEnet : ConfigFileNameBt);
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
                return xmlConfigFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private XElement GetJobNode(XmlToolEcuActivity.JobInfo job, XNamespace ns, XElement jobsNode)
        {
            return (from node in jobsNode.Elements(ns + "job")
                    let nameAttrib = node.Attribute("name")
                    where nameAttrib != null
                    where string.Compare(nameAttrib.Value, job.Name, StringComparison.OrdinalIgnoreCase) == 0 select node).FirstOrDefault();
        }

        private XElement GetDisplayNode(XmlToolEcuActivity.ResultInfo result, XNamespace ns, XElement jobNode)
        {
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

        private string XmlFileDir()
        {
            if (string.IsNullOrEmpty(_xmlBaseDir))
            {
                return null;
            }
            if (string.IsNullOrEmpty(_vin))
            {
                return null;
            }
            try
            {
                return Path.Combine(_xmlBaseDir, _vin);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class Receiver : BroadcastReceiver
        {
            readonly XmlToolActivity _activity;

            public Receiver(XmlToolActivity activity)
            {
                _activity = activity;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;

                if ((action == BluetoothAdapter.ActionStateChanged) ||
                    (action == ConnectivityManager.ConnectivityAction))
                {
                    _activity.SupportInvalidateOptionsMenu();
                    _activity.UpdateDisplay();
                }
            }
        }

        private class EcuListAdapter : BaseAdapter<EcuInfo>
        {
            private readonly List<EcuInfo> _items;
            public List<EcuInfo> Items
            {
                get
                {
                    return _items;
                }
            }
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

            public override EcuInfo this[int position]
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
                textEcuName.Text = item.Name + ": " + item.Description;

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
                    _info = info;
                }

                private readonly EcuInfo _info;

                public EcuInfo Info
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
