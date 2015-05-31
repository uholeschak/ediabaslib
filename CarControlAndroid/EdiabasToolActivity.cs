using Android.Bluetooth;
using Android.Content;
using Android.Content.Res;
using Android.Net;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using com.xamarin.recipes.filepicker;
using EdiabasLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CarControlAndroid
{
    [Android.App.Activity(Label = "@string/tool_title",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                        Android.Content.PM.ConfigChanges.Orientation |
                        Android.Content.PM.ConfigChanges.ScreenSize)]
    public class EdiabasToolActivity : AppCompatActivity
    {
        private enum activityRequest
        {
            REQUEST_SELECT_SGBD,
            REQUEST_SELECT_DEVICE,
        }

        private class ExtraInfo
        {
            public ExtraInfo(string name, string type, List<string> commentList)
            {
                this.name = name;
                this.type = type;
                this.commentList = commentList;
                this.selected = false;
            }

            private string name;
            private string type;
            private List<string> commentList;
            private bool selected;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public string Type
            {
                get
                {
                    return type;
                }
            }

            public List<string> CommentList
            {
                get
                {
                    return commentList;
                }
            }

            public bool Selected
            {
                get
                {
                    return selected;
                }
                set
                {
                   selected = value;
                }
            }
        }

        private class JobInfo
        {
            public JobInfo(string name)
            {
                this.name = name;
                this.comments = new List<string>();
                this.arguments = new List<ExtraInfo>();
                this.results = new List<ExtraInfo>();
            }

            private string name;
            private List<string> comments;
            private List<ExtraInfo> arguments;
            private List<ExtraInfo> results;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public List<string> Comments
            {
                get
                {
                    return comments;
                }
            }

            public List<ExtraInfo> Arguments
            {
                get
                {
                    return arguments;
                }
            }

            public List<ExtraInfo> Results
            {
                get
                {
                    return results;
                }
            }
        }

        // Intent extra
        public const string EXTRA_INIT_DIR = "init_dir";
        public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");

        private Spinner spinnerJobs;
        private JobListAdapter jobListAdapter;
        private EditText editTextArgs;
        private Spinner spinnerResults;
        private ResultSelectListAdapter resultSelectListAdapter;
        private ListView listViewInfo;
        private ResultListAdapter infoListAdapter;
        private string initDirStart;
        private ActivityCommon activityCommon;
        private EdiabasNet ediabas;
        private bool ediabasJobAbort = false;
        private int ignoreResultSelectLayoutChange = 0;
        private string sgbdFileName = string.Empty;
        private string deviceName = string.Empty;
        private string deviceAddress = string.Empty;
        private Receiver receiver;
        private List<JobInfo> jobList = new List<JobInfo>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.ediabas_tool);

            SetResult(Android.App.Result.Canceled);

            initDirStart = Intent.GetStringExtra(EXTRA_INIT_DIR);

            spinnerJobs = FindViewById<Spinner>(Resource.Id.spinnerJobs);
            jobListAdapter = new JobListAdapter(this);
            spinnerJobs.Adapter = jobListAdapter;
            spinnerJobs.ItemSelected += (sender, args) =>
                {
                    NewJobSelected();
                    DisplayJobComments();
                };
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                spinnerJobs.LayoutChange += (sender, args) =>
                {
                    DisplayJobComments();
                };
            }

            editTextArgs = FindViewById<EditText>(Resource.Id.editTextArgs);
            editTextArgs.Enabled = false;
            editTextArgs.Click += (sender, args) =>
                {
                    DisplayJobArguments();
                };

            spinnerResults = FindViewById<Spinner>(Resource.Id.spinnerResults);
            resultSelectListAdapter = new ResultSelectListAdapter(this);
            spinnerResults.Adapter = resultSelectListAdapter;
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                spinnerResults.LayoutChange += (sender, args) =>
                    {
                        if (ignoreResultSelectLayoutChange > 0)
                        {
                            ignoreResultSelectLayoutChange--;
                        }
                        else
                        {
                            DisplayJobResult();
                        }
                    };
            }
            else
            {
                spinnerResults.ItemSelected += (sender, args) =>
                {
                    DisplayJobResult();
                };
            }

            listViewInfo = FindViewById<ListView>(Resource.Id.infoList);
            infoListAdapter = new ResultListAdapter(this);
            listViewInfo.Adapter = infoListAdapter;

            activityCommon = new ActivityCommon(this);
            activityCommon.SelectedInterface = ActivityCommon.InterfaceType.NONE;

            EdiabasClose();

            receiver = new Receiver(this);
            RegisterReceiver(receiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));
            RegisterReceiver(receiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (activityCommon.SelectedInterface == ActivityCommon.InterfaceType.NONE)
            {
                SelectInterface();
            }
            SelectInterfaceEnable();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(receiver);
            EdiabasClose();
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((activityRequest)requestCode)
            {
                case activityRequest.REQUEST_SELECT_SGBD:
                    // When FilePickerActivity returns with a file
                    if (resultCode == Android.App.Result.Ok)
                    {
                        sgbdFileName = data.Extras.GetString(FilePickerActivity.EXTRA_FILE_NAME);
                        SupportInvalidateOptionsMenu();
                        ReadSgbd();
                    }
                    break;

                case activityRequest.REQUEST_SELECT_DEVICE:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        deviceName = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_NAME);
                        deviceAddress = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                        SupportInvalidateOptionsMenu();
                    }
                    break;

            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.tool_menu, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            bool commActive = false;
            bool interfaceAvailable = activityCommon.IsInterfaceAvailable();

            IMenuItem selInterfaceMenu = menu.FindItem(Resource.Id.menu_tool_sel_interface);
            if (selInterfaceMenu != null)
            {
                string interfaceName = string.Empty;
                switch (activityCommon.SelectedInterface)
                {
                    case ActivityCommon.InterfaceType.BLUETOOTH:
                        interfaceName = GetString(Resource.String.select_interface_bt);
                        break;

                    case ActivityCommon.InterfaceType.ENET:
                        interfaceName = GetString(Resource.String.select_interface_enet);
                        break;
                }
                selInterfaceMenu.SetTitle(string.Format(culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_interface), interfaceName));
                selInterfaceMenu.SetEnabled(!commActive);
            }

            IMenuItem selCfgMenu = menu.FindItem(Resource.Id.menu_tool_sel_sgbd);
            if (selCfgMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(sgbdFileName))
                {
                    fileName = Path.GetFileNameWithoutExtension(sgbdFileName);
                }
                selCfgMenu.SetTitle(string.Format(culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_sgbd), fileName));
                selCfgMenu.SetEnabled(!commActive && interfaceAvailable);
            }

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(culture, "{0}: {1}", GetString(Resource.String.menu_device), deviceName));
                scanMenu.SetEnabled(!commActive && interfaceAvailable);
                scanMenu.SetVisible(activityCommon.SelectedInterface == ActivityCommon.InterfaceType.BLUETOOTH);
            }

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.menu_tool_sel_interface:
                    SelectInterface();
                    return true;

                case Resource.Id.menu_tool_sel_sgbd:
                    SelectSgbdFile();
                    return true;

                case Resource.Id.menu_scan:
                    SelectBluetoothDevice();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void EdiabasClose()
        {
            jobListAdapter.Items.Clear();
            jobListAdapter.NotifyDataSetChanged();
            resultSelectListAdapter.Items.Clear();
            resultSelectListAdapter.NotifyDataSetChanged();
            infoListAdapter.Items.Clear();
            infoListAdapter.NotifyDataSetChanged();
            editTextArgs.Enabled = false;
            jobList.Clear();

            if (ediabas != null)
            {
                ediabas.Dispose();
                ediabas = null;
            }
            SupportInvalidateOptionsMenu();
        }

        private void SelectSgbdFile()
        {
            // Launch the FilePickerActivity to select a sgbd file
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = initDirStart;
            try
            {
                if (!string.IsNullOrEmpty(sgbdFileName))
                {
                    initDir = Path.GetDirectoryName(sgbdFileName);
                }
            }
            catch (Exception)
            {
            }
            serverIntent.PutExtra(FilePickerActivity.EXTRA_TITLE, GetString(Resource.String.tool_select_sgbd));
            serverIntent.PutExtra(FilePickerActivity.EXTRA_INIT_DIR, initDir);
            serverIntent.PutExtra(FilePickerActivity.EXTRA_FILE_EXTENSIONS, ".grp;.prg");
            StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_SELECT_SGBD);
        }

        private void SelectInterface()
        {
            activityCommon.SelectInterface((sender, args) =>
            {
                EdiabasClose();
                sgbdFileName = string.Empty;
                SupportInvalidateOptionsMenu();
                SelectInterfaceEnable();
            });
        }

        private void SelectInterfaceEnable()
        {
            activityCommon.RequestInterfaceEnable((sender, args) =>
            {
                SupportInvalidateOptionsMenu();
            });
        }

        private bool SelectBluetoothDevice()
        {
            if (!activityCommon.IsInterfaceAvailable())
            {
                return false;
            }
            if (activityCommon.SelectedInterface != ActivityCommon.InterfaceType.BLUETOOTH)
            {
                return false;
            }

            Intent serverIntent = new Intent(this, typeof(DeviceListActivity));
            StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_SELECT_DEVICE);
            return true;
        }

        private JobInfo GetSelectedJob()
        {
            int pos = spinnerJobs.SelectedItemPosition;
            if (pos < 0)
            {
                return null;
            }
            return jobListAdapter.Items[pos];
        }

        private void NewJobSelected()
        {
            if (jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            ignoreResultSelectLayoutChange = 1;
            resultSelectListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                foreach (ExtraInfo result in jobInfo.Results)
                {
                    resultSelectListAdapter.Items.Add(result);
                }
            }
            resultSelectListAdapter.NotifyDataSetChanged();
            editTextArgs.Text = string.Empty;
        }

        private void DisplayJobComments()
        {
            if (jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_job), null));
                StringBuilder stringBuilderComments = new StringBuilder();
                stringBuilderComments.Append(jobInfo.Name);
                stringBuilderComments.Append(":");
                foreach (string comment in jobInfo.Comments)
                {
                    stringBuilderComments.Append("\r\n");
                    stringBuilderComments.Append(comment);
                }
                infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
            }
            infoListAdapter.NotifyDataSetChanged();
        }

        private void DisplayJobArguments()
        {
            if (jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_arguments), null));
                foreach (ExtraInfo info in jobInfo.Arguments)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                    infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            infoListAdapter.NotifyDataSetChanged();
        }

        private void DisplayJobResult()
        {
            if (jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_result), null));
                if (spinnerResults.SelectedItemPosition >= 0)
                {
                    ExtraInfo info = jobInfo.Results[spinnerResults.SelectedItemPosition];
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                    infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            infoListAdapter.NotifyDataSetChanged();
        }

        private void ReadSgbd()
        {
            if (string.IsNullOrEmpty(sgbdFileName))
            {
                return;
            }
            EdiabasClose();
            ediabas = new EdiabasNet();
            if (activityCommon.SelectedInterface == ActivityCommon.InterfaceType.ENET)
            {
                ediabas.EdInterfaceClass = new EdInterfaceEnet();
            }
            else
            {
                ediabas.EdInterfaceClass = new EdInterfaceObd();
            }
            ediabas.AbortJobFunc = AbortEdiabasJob;
            ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(sgbdFileName));

            if (ediabas.EdInterfaceClass is EdInterfaceObd)
            {
                ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "BLUETOOTH:" + deviceAddress;
            }
            if (ediabas.EdInterfaceClass is EdInterfaceEnet)
            {
                string remoteHost = "auto";
                if (activityCommon.Emulator)
                {   // broadcast is not working with emulator
                    remoteHost = ActivityCommon.EMULATOR_ENET_IP;
                }
                ((EdInterfaceEnet)ediabas.EdInterfaceClass).RemoteHost = remoteHost;
            }

            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.tool_read_sgbd));
            progress.Show();

            ediabasJobAbort = false;
            Task.Factory.StartNew(() =>
            {
                List<string> messageList = new List<string>();
                try
                {
                    ediabas.ResolveSgbdFile(sgbdFileName);

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                    ediabas.ArgString = string.Empty;
                    ediabas.ArgBinaryStd = null;
                    ediabas.ResultsRequests = string.Empty;
                    ediabas.ExecuteJob("_JOBS");

                    resultSets = ediabas.ResultSets;
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
                                if (resultData.opData is string)
                                {
                                    jobList.Add(new JobInfo((string)resultData.opData));
                                }
                            }
                            dictIndex++;
                        }
                    }

                    foreach (JobInfo job in jobList)
                    {
                        ediabas.ArgString = job.Name;
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = string.Empty;
                        ediabas.ExecuteJob("_JOBCOMMENTS");

                        resultSets = ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            for (int i = 0; ; i++)
                            {
                                EdiabasNet.ResultData resultData;
                                if (resultDict.TryGetValue("JOBCOMMENT" + i.ToString(culture), out resultData))
                                {
                                    if (resultData.opData is string)
                                    {
                                        job.Comments.Add((string)resultData.opData);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    foreach (JobInfo job in jobList)
                    {
                        ediabas.ArgString = job.Name;
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = string.Empty;
                        ediabas.ExecuteJob("_ARGUMENTS");

                        resultSets = ediabas.ResultSets;
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
                                    if (resultData.opData is string)
                                    {
                                        arg = (string)resultData.opData;
                                    }
                                }
                                if (resultDict.TryGetValue("ARGTYPE", out resultData))
                                {
                                    if (resultData.opData is string)
                                    {
                                        argType = (string)resultData.opData;
                                    }
                                }
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("ARGCOMMENT" + i.ToString(culture), out resultData))
                                    {
                                        if (resultData.opData is string)
                                        {
                                            argCommentList.Add((string)resultData.opData);
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

                    foreach (JobInfo job in jobList)
                    {
                        ediabas.ArgString = job.Name;
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = string.Empty;
                        ediabas.ExecuteJob("_RESULTS");

                        resultSets = ediabas.ResultSets;
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
                                    if (resultData.opData is string)
                                    {
                                        result = (string)resultData.opData;
                                    }
                                }
                                if (resultDict.TryGetValue("RESULTTYPE", out resultData))
                                {
                                    if (resultData.opData is string)
                                    {
                                        resultType = (string)resultData.opData;
                                    }
                                }
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("RESULTCOMMENT" + i.ToString(culture), out resultData))
                                    {
                                        if (resultData.opData is string)
                                        {
                                            resultCommentList.Add((string)resultData.opData);
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
                    messageList.Add(EdiabasNet.GetExceptionText(ex));
                }
                RunOnUiThread(() =>
                {
                    progress.Hide();

                    foreach (string message in messageList)
                    {
                        infoListAdapter.Items.Add(new TableResultItem(message, null));
                    }
                    infoListAdapter.NotifyDataSetChanged();

                    foreach (JobInfo job in jobList)
                    {
                        jobListAdapter.Items.Add(job);
                    }
                    jobListAdapter.NotifyDataSetChanged();

                    editTextArgs.Enabled = jobList.Count > 0;
                });
            });
        }

        private bool AbortEdiabasJob()
        {
            if (ediabasJobAbort)
            {
                return true;
            }
            return false;
        }

        public class Receiver : BroadcastReceiver
        {
            EdiabasToolActivity activity;

            public Receiver(EdiabasToolActivity activity)
            {
                this.activity = activity;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;

                if ((action == BluetoothAdapter.ActionStateChanged) ||
                    (action == ConnectivityManager.ConnectivityAction))
                {
                    activity.SupportInvalidateOptionsMenu();
                }
            }
        }

        private class JobListAdapter : BaseAdapter<JobInfo>
        {
            private List<JobInfo> items;
            public List<JobInfo> Items
            {
                get
                {
                    return items;
                }
            }
            private Android.App.Activity context;
            private Android.Graphics.Color backgroundColor;
            private Android.Graphics.Color textColor;

            public JobListAdapter(Android.App.Activity context)
                : base()
            {
                this.context = context;
                this.items = new List<JobInfo>();

                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new int[] { Android.Resource.Attribute.ColorBackground, Android.Resource.Attribute.TextColorPrimary });
                backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
                textColor = typedArray.GetColor(1, 0x000000);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override JobInfo this[int position]
            {
                get { return items[position]; }
            }

            public override int Count
            {
                get { return items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = items[position];

                View view = convertView;
                if (view == null) // no view to re-use, create new
                    view = context.LayoutInflater.Inflate(Resource.Layout.job_list, null);
                view.SetBackgroundColor(backgroundColor);
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
            private List<ExtraInfo> items;
            public List<ExtraInfo> Items
            {
                get
                {
                    return items;
                }
            }
            private Android.App.Activity context;
            private Android.Graphics.Color backgroundColor;
            private Android.Graphics.Color textColor;
            private bool ignoreCheckEvent = false;

            public ResultSelectListAdapter(Android.App.Activity context)
                : base()
            {
                this.context = context;
                this.items = new List<ExtraInfo>();
                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new int[] { Android.Resource.Attribute.ColorBackground, Android.Resource.Attribute.TextColorPrimary });
                backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
                textColor = typedArray.GetColor(1, 0x000000);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ExtraInfo this[int position]
            {
                get { return items[position]; }
            }

            public override int Count
            {
                get { return items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = items[position];

                View view = convertView;
                if (view == null) // no view to re-use, create new
                    view = context.LayoutInflater.Inflate(Resource.Layout.result_select_list, null);
                view.SetBackgroundColor(backgroundColor);
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxResultSelect);
                ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                ignoreCheckEvent = false;

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
                if (!ignoreCheckEvent)
                {
                    CheckBox checkBox = (CheckBox)sender;
                    TagInfo tagInfo = (TagInfo)checkBox.Tag;
                    if (tagInfo.Info.Selected != args.IsChecked)
                    {
                        NotifyDataSetChanged();
                    }
                    tagInfo.Info.Selected = args.IsChecked;
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(ExtraInfo info)
                {
                    this.info = info;
                }

                private ExtraInfo info;

                public ExtraInfo Info
                {
                    get
                    {
                        return info;
                    }
                }
            }
        }
    }
}
