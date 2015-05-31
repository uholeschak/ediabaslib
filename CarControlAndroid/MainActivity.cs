using Android.Bluetooth;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using com.xamarin.recipes.filepicker;
using EdiabasLib;
using Java.Interop;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CarControlAndroid
{
    [Android.App.Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat", MainLauncher = true,
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                Android.Content.PM.ConfigChanges.Orientation |
                Android.Content.PM.ConfigChanges.ScreenSize)]
    public class ActivityMain : AppCompatActivity, ActionBar.ITabListener
    {
        private enum activityRequest
        {
            REQUEST_SELECT_DEVICE,
            REQUEST_SELECT_CONFIG,
            REQUEST_EDIABAS_TOOL,
        }

        public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private string deviceName = string.Empty;
        private string deviceAddress = string.Empty;
        private string configFileName = string.Empty;
        private bool tracingActive = false;
        private bool dataLogActive = false;
        private bool activityStarted = false;
        private bool createTabsPending = false;
        private const string sharedAppName = "CarControl";
        private string externalPath;
        private string externalWritePath;
        private bool autoStart = false;
        private ActivityCommon activityCommon;
        private JobReader jobReader;
        private Handler updateHandler;
        private EdiabasThread ediabasThread;
        private StreamWriter swDataLog;
        private string dataLogDir;
        private List<Fragment> fragmentList;
        private Fragment lastFragment;
        private ToggleButton buttonConnect;
        private ImageView imageBackground;
        private View barConnectView;
        private Receiver receiver;

        public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //Log.Debug(Tag, "The tab {0} was re-selected.", tab.Text);
        }

        public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //Log.Debug(Tag, "The tab {0} has been selected.", tab.Text);
            Fragment frag = fragmentList[tab.Position];
            ft.Replace(Resource.Id.tabFrameLayout, frag);
            lastFragment = frag;
            UpdateSelectedPage();
        }

        public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            // perform any extra work associated with saving fragment state here.
            //Log.Debug(Tag, "The tab {0} as been unselected.", tab.Text);
            if (lastFragment != null)
            {
                ft.Remove(lastFragment);
                lastFragment = null;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.NavigationMode = Android.Support.V7.App.ActionBar.NavigationModeStandard;
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SupportActionBar.SetDisplayUseLogoEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetIcon(Android.Resource.Color.Transparent);   // hide icon
            SetContentView(Resource.Layout.main);

            activityCommon = new ActivityCommon(this);
            updateHandler = new Handler();
            jobReader = new JobReader();
            SetStoragePath();
            GetSettings();

            barConnectView = LayoutInflater.Inflate(Resource.Layout.bar_connect, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(barConnectView, barLayoutParams);

            buttonConnect = barConnectView.FindViewById<ToggleButton>(Resource.Id.buttonConnect);
            imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);
            fragmentList = new List<Fragment>();
            buttonConnect.Click += ButtonConnectClick;

            ReadConfigFile();

            receiver = new Receiver(this);
            RegisterReceiver(receiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));
            RegisterReceiver(receiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        void AddTabToActionBar(string label)
        {
            ActionBar.Tab tab = SupportActionBar.NewTab()
                .SetText(label)
                .SetTabListener(this);
            SupportActionBar.AddTab(tab);
        }

        void CreateActionBarTabs()
        {
            if (!activityStarted)
            {
                createTabsPending = true;
                return;
            }
            createTabsPending = false;
            SupportActionBar.RemoveAllTabs();
            fragmentList.Clear();
            SupportActionBar.NavigationMode = Android.Support.V7.App.ActionBar.NavigationModeStandard;
            foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
            {
                int resourceId = Resource.Layout.tab_list;
                if (pageInfo.JobInfo.Activate) resourceId = Resource.Layout.tab_activate;

                Fragment fragmentPage = new TabContentFragment(this, resourceId, pageInfo);
                fragmentList.Add(fragmentPage);
                pageInfo.InfoObject = fragmentPage;
                AddTabToActionBar(GetPageString(pageInfo, pageInfo.Name));
            }
            SupportActionBar.NavigationMode = (jobReader.PageList.Count > 0) ? Android.Support.V7.App.ActionBar.NavigationModeTabs : Android.Support.V7.App.ActionBar.NavigationModeStandard;
            UpdateDisplay();
        }

        protected override void OnStart()
        {
            base.OnStart();

            activityStarted = true;
            if (createTabsPending)
            {
                CreateActionBarTabs();
            }
            activityCommon.RequestInterfaceEnable((sender, args) =>
                {
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                });
        }

        protected override void OnStop()
        {
            base.OnStop();

            activityStarted = false;
            if (swDataLog == null)
            {
                StopEdiabasThread(false);
            }
            StoreSettings();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(receiver);
            StopEdiabasThread(true);
            StoreSettings();
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch((activityRequest)requestCode)
            {
                case activityRequest.REQUEST_SELECT_DEVICE:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        deviceName = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_NAME);
                        deviceAddress = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                        SupportInvalidateOptionsMenu();
                        if (autoStart)
                        {
                            ButtonConnectClick(buttonConnect, new EventArgs());
                        }
                    }
                    autoStart = false;
                    break;

                case activityRequest.REQUEST_SELECT_CONFIG:
                    // When FilePickerActivity returns with a file
                    if (resultCode == Android.App.Result.Ok)
                    {
                        configFileName = data.Extras.GetString(FilePickerActivity.EXTRA_FILE_NAME);
                        ReadConfigFile();
                        SupportInvalidateOptionsMenu();
                    }
                    break;

                case activityRequest.REQUEST_EDIABAS_TOOL:
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            bool commActive = ediabasThread != null && ediabasThread.ThreadRunning();
            bool interfaceAvailable = activityCommon.IsInterfaceAvailable();
            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(culture, "{0}: {1}", GetString(Resource.String.menu_device), deviceName));
                scanMenu.SetEnabled(interfaceAvailable && !commActive);
                scanMenu.SetVisible(activityCommon.SelectedInterface == ActivityCommon.InterfaceType.BLUETOOTH);
            }

            IMenuItem selCfgMenu = menu.FindItem(Resource.Id.menu_sel_cfg);
            if (selCfgMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(configFileName))
                {
                    fileName = Path.GetFileNameWithoutExtension(configFileName);
                }
                selCfgMenu.SetTitle(string.Format(culture, "{0}: {1}", GetString(Resource.String.menu_sel_cfg), fileName));
                selCfgMenu.SetEnabled(!commActive);
            }

            IMenuItem ediabasToolMenu = menu.FindItem(Resource.Id.menu_ediabas_tool);
            if (ediabasToolMenu != null)
            {
                ediabasToolMenu.SetEnabled(!commActive);
            }

            IMenuItem traceMenu = menu.FindItem(Resource.Id.menu_enable_trace);
            if (traceMenu != null)
            {
                traceMenu.SetEnabled(interfaceAvailable && !commActive);
                traceMenu.SetChecked(tracingActive);
            }

            IMenuItem dataLogMenu = menu.FindItem(Resource.Id.menu_enable_datalog);
            if (dataLogMenu != null)
            {
                dataLogMenu.SetEnabled(interfaceAvailable);
                dataLogMenu.SetChecked(dataLogActive);
            }

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_scan:
                    SelectBluetoothDevice();
                    break;

                case Resource.Id.menu_sel_cfg:
                    SelectConfigFile();
                    return true;

                case Resource.Id.menu_ediabas_tool:
                    StartEdiabasTool();
                    return true;

                case Resource.Id.menu_enable_trace:
                    tracingActive = !tracingActive;
                    SupportInvalidateOptionsMenu();
                    return true;

                case Resource.Id.menu_enable_datalog:
                    dataLogActive = !dataLogActive;
                    if (!dataLogActive)
                    {
                        CloseDataLog();
                    }
                    SupportInvalidateOptionsMenu();
                    return true;

                case Resource.Id.menu_exit:
                    OnDestroy();
                    System.Environment.Exit(0);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected void ButtonConnectClick(object sender, EventArgs e)
        {
            autoStart = false;
            if (!RequestBluetoothDeviceSelect())
            {
                return;
            }
            if (ediabasThread != null && ediabasThread.ThreadRunning())
            {
                StopEdiabasThread(false);
            }
            else
            {
                StartEdiabasThread();
                UpdateSelectedPage();
            }
            UpdateDisplay();
        }

        [Export("onActiveClick")]
        public void OnActiveClick(View v)
        {
            if (ediabasThread == null)
            {
                return;
            }
            ToggleButton button = v.FindViewById<ToggleButton>(Resource.Id.button_active);
            ediabasThread.CommActive = button.Checked;
        }

        private bool StartEdiabasThread()
        {
            autoStart = false;
            try
            {
                if (ediabasThread == null)
                {
                    ediabasThread = new EdiabasThread(jobReader.EcuPath, activityCommon.SelectedInterface);
                    ediabasThread.DataUpdated += DataUpdated;
                    ediabasThread.ThreadTerminated += ThreadTerminated;
                }
                string logDir;
                if (string.IsNullOrEmpty(externalWritePath))
                {
                    logDir = Path.GetDirectoryName(configFileName);
                }
                else
                {
                    logDir = externalWritePath;
                }

                if (!string.IsNullOrEmpty(jobReader.LogPath))
                {
                    if (Path.IsPathRooted(jobReader.LogPath))
                    {
                        logDir = jobReader.LogPath;
                    }
                    else
                    {
                        logDir = Path.Combine(logDir, jobReader.LogPath);
                    }
                }
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch (Exception)
                {
                    logDir = string.Empty;
                }
                dataLogDir = logDir;

                string traceDir = null;
                if (tracingActive && !string.IsNullOrEmpty(configFileName))
                {
                    traceDir = logDir;
                }
                JobReader.PageInfo pageInfo = GetSelectedDevice();
                if (pageInfo != null)
                {
                    string portName = string.Empty;
                    switch (activityCommon.SelectedInterface)
                    {
                        case ActivityCommon.InterfaceType.BLUETOOTH:
                            portName = "BLUETOOTH:" + deviceAddress;
                            break;

                        case ActivityCommon.InterfaceType.ENET:
                            if (activityCommon.Emulator)
                            {   // broadcast is not working with emulator
                                portName = ActivityCommon.EMULATOR_ENET_IP;
                            }
                            break;
                    }
                    ediabasThread.StartThread(portName, traceDir, pageInfo, true);
                }
            }
            catch (Exception)
            {
                return false;
            }
            SupportInvalidateOptionsMenu();
            return true;
        }

        private bool StopEdiabasThread(bool wait)
        {
            if (ediabasThread != null)
            {
                try
                {
                    ediabasThread.StopThread(wait);
                    if (wait)
                    {
                        ediabasThread.DataUpdated -= DataUpdated;
                        ediabasThread.ThreadTerminated -= ThreadTerminated;
                        ediabasThread.Dispose();
                        ediabasThread = null;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            CloseDataLog();
            SupportInvalidateOptionsMenu();
            return true;
        }

        private void CloseDataLog()
        {
            if (swDataLog != null)
            {
                swDataLog.Dispose();
                swDataLog = null;
            }
        }

        private bool GetSettings()
        {
            try
            {
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                deviceName = prefs.GetString("DeviceName", string.Empty);
                deviceAddress = prefs.GetString("DeviceAddress", string.Empty);
                configFileName = prefs.GetString("ConfigFile", string.Empty);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StoreSettings()
        {
            try
            {
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.PutString("DeviceName", deviceName);
                prefsEdit.PutString("DeviceAddress", deviceAddress);
                prefsEdit.PutString("ConfigFile", configFileName);
                prefsEdit.Commit();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            RunOnUiThread(DataUpdatedMethode);
        }

        private void ThreadTerminated(object sender, EventArgs e)
        {
            RunOnUiThread(ThreadTerminatedMethode);
        }

        private void ThreadTerminatedMethode()
        {
            StopEdiabasThread(true);
            UpdateDisplay();
        }

        private JobReader.PageInfo GetSelectedDevice()
        {
            JobReader.PageInfo pageInfo = null;
            if (SupportActionBar.SelectedTab != null)
            {
                int index = SupportActionBar.SelectedTab.Position;
                if (index >= 0 && index < (jobReader.PageList.Count))
                {
                    pageInfo = jobReader.PageList[index];
                }
            }
            return pageInfo;
        }

        private void UpdateSelectedPage()
        {
            if ((ediabasThread == null) || !ediabasThread.ThreadRunning())
            {
                return;
            }

            JobReader.PageInfo newPageInfo = GetSelectedDevice();
            if (newPageInfo == null)
            {
                return;
            }
            bool newCommActive = true;
            if (newPageInfo.JobInfo.Activate)
            {
                newCommActive = false;
            }
            if (ediabasThread.JobPageInfo != newPageInfo)
            {
                ediabasThread.CommActive = newCommActive;
                ediabasThread.JobPageInfo = newPageInfo;
                CloseDataLog();
            }
        }

        private void UpdateDisplay()
        {
            DataUpdatedMethode();
        }

        private void DataUpdatedMethode()
        {
            bool dynamicValid = false;
            bool buttonConnectEnable = true;
            bool threadRunning = false;

            if (ediabasThread != null && ediabasThread.ThreadRunning())
            {
                if (ediabasThread.ThreadStopping())
                {
                    buttonConnectEnable = false;
                }
                else
                {
                    threadRunning = true;
                }
                if (ediabasThread.CommActive)
                {
                    dynamicValid = true;
                }
                buttonConnect.Checked = true;
            }
            else
            {
                if (!activityCommon.IsInterfaceAvailable())
                {
                    buttonConnectEnable = false;
                }
                buttonConnect.Checked = false;
            }
            buttonConnect.Enabled = buttonConnectEnable;
            imageBackground.Visibility = dynamicValid ? ViewStates.Invisible : ViewStates.Visible;

            Fragment dynamicFragment = null;
            JobReader.PageInfo pageInfo = GetSelectedDevice();
            if (pageInfo != null)
            {
                dynamicFragment = (Fragment)pageInfo.InfoObject;
            }

            if (dynamicFragment != null && dynamicFragment.View != null)
            {
                ListView listViewResult = dynamicFragment.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this, pageInfo.Weight);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                ToggleButton buttonActive = null;
                if (pageInfo.JobInfo.Activate)
                {
                    buttonActive = dynamicFragment.View.FindViewById<ToggleButton>(Resource.Id.button_active);
                }

                if (dynamicValid)
                {
                    if (dataLogActive && threadRunning && swDataLog == null && !string.IsNullOrEmpty(pageInfo.LogFile))
                    {
                        try
                        {
                            FileMode fileMode;
                            string fileName = Path.Combine(dataLogDir, pageInfo.LogFile);
                            if (File.Exists(fileName))
                            {
                                fileMode = jobReader.AppendLog ? FileMode.Append : FileMode.Create;
                            }
                            else
                            {
                                fileMode = FileMode.Create;
                            }
                            swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (EdiabasThread.DataLock)
                    {
                        resultDict = ediabasThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear();

                    bool formatResult = false;
                    bool updateResult = false;
                    if (pageInfo.ClassObject != null)
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        formatResult = pageType.GetMethod("FormatResult") != null;
                        updateResult = pageType.GetMethod("UpdateResultList") != null;
                    }
                    string currDateTime = string.Empty;
                    if (dataLogActive)
                    {
                        currDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", culture);
                    }
                    foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                    {
                        string result = string.Empty;
                        if (displayInfo.Format == null)
                        {
                            if (resultDict != null)
                            {
                                try
                                {
                                    if (formatResult)
                                    {
                                        result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict, displayInfo.Result);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        else
                        {
                            result = FormatResultEdiabas(resultDict, displayInfo.Result, displayInfo.Format);
                        }
                        if (result != null)
                        {
                            resultListAdapter.Items.Add(new TableResultItem(GetPageString(pageInfo, displayInfo.Name), result));
                            if (!string.IsNullOrEmpty(displayInfo.LogTag) && dataLogActive && swDataLog != null)
                            {
                                try
                                {
                                    swDataLog.Write(string.Format("{0}\t{1}\t{2}\r\n", displayInfo.LogTag, currDateTime, result));
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }

                    if (updateResult)
                    {
                        pageInfo.ClassObject.UpdateResultList(pageInfo, resultDict, resultListAdapter);
                    }

                    resultListAdapter.NotifyDataSetChanged();
                }
                else
                {
                    resultListAdapter.Items.Clear();
                    resultListAdapter.NotifyDataSetChanged();
                }

                if (pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("UpdateLayout") != null)
                        {
                            pageInfo.ClassObject.UpdateLayout(pageInfo, dynamicValid, ediabasThread != null);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (buttonActive != null)
                {
                    if (ediabasThread != null && ediabasThread.ThreadRunning())
                    {
                        buttonActive.Enabled = true;
                        buttonActive.Checked = ediabasThread.CommActive;
                    }
                    else
                    {
                        buttonActive.Enabled = false;
                        buttonActive.Checked = false;
                    }
                }
            }
        }

        public static String FormatResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            double value = GetResultDouble(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            Int64 value = GetResultInt64(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            string value = GetResultString(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultEdiabas(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            string result = string.Empty;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                result = EdiabasNet.FormatResult(resultData, format);
                if (result == null) result = "?";
            }
            return result;
        }

        public static Int64 GetResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData is Int64)
                {
                    found = true;
                    return (Int64)resultData.opData;
                }
            }
            return 0;
        }

        public static Double GetResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData is Double)
                {
                    found = true;
                    return (Double)resultData.opData;
                }
            }
            return 0;
        }

        public static String GetResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData is String)
                {
                    found = true;
                    return (String)resultData.opData;
                }
            }
            return string.Empty;
        }

        public static String GetPageString(JobReader.PageInfo pageInfo, string name)
        {
            string lang = CultureInfo.CurrentUICulture.Name;
            string langIso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            JobReader.StringInfo stringInfoDefault = null;
            JobReader.StringInfo stringInfoSel = null;
            foreach (JobReader.StringInfo stringInfo in pageInfo.StringList)
            {
                if (string.IsNullOrEmpty(stringInfo.Lang))
                {
                    stringInfoDefault = stringInfo;
                }
                else if ((string.Compare(stringInfo.Lang, lang, StringComparison.OrdinalIgnoreCase) == 0) ||
                        (string.Compare(stringInfo.Lang, langIso, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    stringInfoSel = stringInfo;
                }
            }
            if (stringInfoSel == null) stringInfoSel = stringInfoDefault;
            string result = String.Empty;
            if (stringInfoSel != null)
            {
                if (!stringInfoSel.StringDict.TryGetValue(name, out result))
                {
                    result = String.Empty;
                }
            }
            return result;
        }

        private void ReadConfigFile()
        {
            jobReader.ReadXml(configFileName);
            if (jobReader.PageList.Count > 0)
            {
                activityCommon.SelectedInterface = jobReader.Interface;
            }
            else
            {
                activityCommon.SelectedInterface = ActivityCommon.InterfaceType.NONE;
            }
            RequestConfigSelect();
            CompileCode();
        }

        private void CompileCode()
        {
            if (jobReader.PageList.Count == 0)
            {
                updateHandler.Post(() =>
                {
                    CreateActionBarTabs();
                });
                return;
            }
            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.compile_start));
            progress.Show();

            Task.Factory.StartNew(() =>
            {
                List<Task<string>> taskList = new List<Task<string>>();
                foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
                {
                    if (pageInfo.JobInfo.ClassCode == null) continue;
                    Task<string> compileTask = Task<string>.Factory.StartNew(() =>
                    {
                        string result = string.Empty;
                        StringWriter reportWriter = new StringWriter();
                        try
                        {
                            Evaluator evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter(reportWriter)));
                            evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
                            evaluator.ReferenceAssembly(typeof(EdiabasNet).Assembly);
                            evaluator.ReferenceAssembly(typeof(View).Assembly);
                            string classCode = @"
                                using Android.Views;
                                using Android.Widget;
                                using EdiabasLib;
                                using CarControlAndroid;
                                using System;
                                using System.Collections.Generic;
                                using System.Diagnostics;
                                using System.Threading;"
                                + pageInfo.JobInfo.ClassCode;
                            evaluator.Compile(classCode);
                            pageInfo.ClassObject = evaluator.Evaluate("new PageClass()");
                            if (string.IsNullOrEmpty(pageInfo.JobInfo.Name))
                            {
                                Type pageType = pageInfo.ClassObject.GetType();
                                if (pageType.GetMethod("ExecuteJob") == null)
                                {
                                    throw new Exception("No ExecuteJob method");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result = reportWriter.ToString();
                            if (string.IsNullOrEmpty(result))
                            {
                                result = EdiabasNet.GetExceptionText(ex);
                            }
                            result = GetPageString(pageInfo, pageInfo.Name) + ":\r\n" + result;
                        }
                        if (pageInfo.JobInfo.ShowWarnings && string.IsNullOrEmpty(result))
                        {
                            result = reportWriter.ToString();
                        }

                        return result;
                    });
                    taskList.Add(compileTask);
                }
                Task.WaitAll(taskList.ToArray());

                foreach (Task<string> task in taskList)
                {
                    string result = task.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        RunOnUiThread(() => activityCommon.ShowAlert(result));
                    }
                }

                RunOnUiThread(() =>
                {
                    CreateActionBarTabs();
                    progress.Hide();
                });
            });
        }

        private void RequestConfigSelect()
        {
            if (jobReader.PageList.Count > 0)
            {
                return;
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    SelectConfigFile();
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(false)
                .SetMessage(Resource.String.config_select)
                .SetTitle(Resource.String.config_select_title)
                .Show();
        }

        private void SelectConfigFile()
        {
            // Launch the FilePickerActivity to select a configuration
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = externalPath;
            try
            {
                if (!string.IsNullOrEmpty(configFileName))
                {
                    initDir = Path.GetDirectoryName(configFileName);
                }
            }
            catch (Exception)
            {
            }
            serverIntent.PutExtra(FilePickerActivity.EXTRA_INIT_DIR, initDir);
            serverIntent.PutExtra(FilePickerActivity.EXTRA_FILE_EXTENSIONS, ".cccfg");
            StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_SELECT_CONFIG);
        }

        private bool RequestBluetoothDeviceSelect()
        {
            if (!activityCommon.IsInterfaceAvailable())
            {
                return true;
            }
            if (activityCommon.SelectedInterface != ActivityCommon.InterfaceType.BLUETOOTH)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(deviceAddress))
            {
                return true;
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    if (SelectBluetoothDevice())
                    {
                        autoStart = true;
                    }
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(false)
                .SetMessage(Resource.String.bt_device_select)
                .SetTitle(Resource.String.bt_device_select_title)
                .Show();
            return false;
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

        private bool StartEdiabasTool()
        {
            Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
            string initDir = externalPath;
            try
            {
                if (!string.IsNullOrEmpty(configFileName))
                {
                    initDir = Path.GetDirectoryName(configFileName);
                }
            }
            catch (Exception)
            {
            }
            serverIntent.PutExtra(EdiabasToolActivity.EXTRA_INIT_DIR, initDir);
            StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_EDIABAS_TOOL);
            return true;
        }

        private void SetStoragePath()
        {
            externalPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            externalWritePath = string.Empty;
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {   // writing to external disk is only allowed in special directories.
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                if (externalFilesDirs.Length > 0)
                {
                    // index 0 is the internal disk
                    externalWritePath = externalFilesDirs.Length > 1 ? externalFilesDirs[1].AbsolutePath : externalFilesDirs[0].AbsolutePath;
                }
            }
        }

        public class Receiver : BroadcastReceiver
        {
            ActivityMain activity;

            public Receiver(ActivityMain activity)
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
                    activity.UpdateDisplay();
                }
            }
        }

        public class TabContentFragment : Fragment
        {
            private ActivityMain activity;
            private int resourceId;
            private JobReader.PageInfo pageInfo;

            public TabContentFragment(ActivityMain activity, int resourceId, JobReader.PageInfo pageInfo)
            {
                this.activity = activity;
                this.resourceId = resourceId;
                this.pageInfo = pageInfo;
            }

            public TabContentFragment(ActivityMain activity, int resourceId)
                : this(activity, resourceId, null)
            {
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                View view = inflater.Inflate(resourceId, null);
                if (pageInfo != null && pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("CreateLayout") != null)
                        {
                            LinearLayout pageLayout = view.FindViewById<LinearLayout>(Resource.Id.listLayout);
                            pageInfo.ClassObject.CreateLayout(activity, pageInfo, pageLayout);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                activity.updateHandler.Post(() =>
                {
                    activity.UpdateDisplay();
                });
                return view;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();

                if (pageInfo != null && pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("DestroyLayout") != null)
                        {
                            pageInfo.ClassObject.DestroyLayout(pageInfo);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
