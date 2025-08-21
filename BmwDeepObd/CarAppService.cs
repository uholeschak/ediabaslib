using Android.Content;
using Android.OS;
using AndroidX.Car.App;
using AndroidX.Car.App.Constraints;
using AndroidX.Car.App.Model;
using AndroidX.Car.App.Validation;
using AndroidX.Lifecycle;
using EdiabasLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace BmwDeepObd
{
#if ANDROID_AUTO
    [Android.App.Service(
        Label = "@string/app_name",
        Icon = "@drawable/icon",
        Exported = true,
        Name = ActivityCommon.AppNameSpace + "." + nameof(CarService)
    )]
    [Android.App.IntentFilter(new[]
        {
            "androidx.car.app.CarAppService"
        },
        Categories = new[]
        {
            "androidx.car.app.category.IOT"
        })
    ]
#endif

    // Testing with Desktop Head Unit (DHU)
    // https://developer.android.com/training/cars/testing/dhu
    // Start Android Auto Emulation Server on the smartphone
    // Select once: Connect vehicle
    // Open Visual Studio adb console
    // cd /d C:\Users\Ulrich\AppData\Local\Android\android-sdk\extras\google\auto
    // adb forward tcp:5277 tcp:5277
    // desktop-head-unit.exe

    public class CarService : CarAppService
    {
        public class ErrorMessageEntry
        {
            public ErrorMessageEntry(EdiabasThread.EdiabasErrorReport errorReport, string message)
            {
                ErrorReport = errorReport;
                Message = message;
            }

            public EdiabasThread.EdiabasErrorReport ErrorReport { get; }
            public string Message { get; }
        }

        public delegate void ErrorMessageResultDelegate(List<ErrorMessageEntry> errorList);

#if DEBUG
        private static readonly string Tag = typeof(CarService).FullName;
#endif
        public const int UpdateInterval = 1000;
        public const int DefaultListItems = 6;
        private ActivityCommon _activityCommon;
        private Thread _errorEvalThread;

        public ActivityCommon ActivityCommon => _activityCommon;

        public override void OnCreate()
        {
            base.OnCreate();

            _activityCommon = new ActivityCommon(this);

            ActivityCommon.InstanceDataCommon instanceData = new ActivityCommon.InstanceDataCommon();
            if (!_activityCommon.GetSettings(instanceData, ActivityCommon.SettingsMode.All, true))
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "OnCreate: GetSettings failed");
#endif
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (IsErrorEvalJobRunning())
            {
                _errorEvalThread?.Join();
            }

            if (_activityCommon != null)
            {
                _activityCommon.Dispose();
                _activityCommon = null;
            }
        }

        public override HostValidator CreateHostValidator()
        {
            return HostValidator.AllowAllHostsValidator;
        }

        public override Session OnCreateSession()
        {
            return new CarSession(this);
        }

        public bool IsErrorEvalJobRunning()
        {
            if (_errorEvalThread == null)
            {
                return false;
            }
            if (_errorEvalThread.IsAlive)
            {
                return true;
            }
            _errorEvalThread = null;
            return false;
        }

        public bool EvaluateErrorMessages(Context resourceContext, JobReader.PageInfo pageInfo, List<EdiabasThread.EdiabasErrorReport> errorReportList, MethodInfo formatErrorResult, ErrorMessageResultDelegate resultHandler)
        {
            if (IsErrorEvalJobRunning())
            {
                CarSession.LogFormat("EvaluateErrorMessages: Thread still active");
                return false;
            }

            _errorEvalThread = new Thread(() =>
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "EvaluateErrorMessages: Thread started");
#endif
                List<ErrorMessageEntry> errorList = new List<ErrorMessageEntry>();
                List<ActivityCommon.VagDtcEntry> dtcList = null;
                int errorIndex = 0;
                foreach (EdiabasThread.EdiabasErrorReport errorReport in errorReportList)
                {
                    if (errorReport is EdiabasThread.EdiabasErrorReportReset)
                    {
                        continue;
                    }

                    string message = GenerateErrorMessage(resourceContext, pageInfo, errorReport, errorIndex, formatErrorResult, ref dtcList);
                    errorList.Add(new ErrorMessageEntry(errorReport, message));
                    errorIndex++;
                }

                if (resultHandler != null)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("EvaluateErrorMessages: Thread finished items: {0}", errorList.Count));
#endif
                    resultHandler.Invoke(errorList);
                }
            });

            _errorEvalThread.Start();

            return true;
        }

        private string GenerateErrorMessage(Context resourceContext, JobReader.PageInfo pageInfo, EdiabasThread.EdiabasErrorReport errorReport, int errorIndex, MethodInfo formatErrorResult, ref List<ActivityCommon.VagDtcEntry> dtcList)
        {
            List<string> translationList = new List<string>();
            string errorMessage = ActivityCommon.EdiabasThread?.GenerateErrorMessage(resourceContext, _activityCommon, pageInfo, errorReport, errorIndex, formatErrorResult, ref translationList,
                null, ref dtcList);

            if (errorMessage == null)
            {
                return string.Empty;
            }

            return errorMessage;
        }

        public sealed class CarSession : Session, ILifecycleEventObserver
        {
            public const string CarSessionBroadcastAction = ActivityCommon.AppNameSpace + ".CarSession.Action";
            public const string ExtraConnectStarted = "ConnectStarted";
            public const string ExtraActivityStopped = "ActivityStopped";

            private readonly CarService _carService;
            private readonly Receiver _bcReceiver;

            public MainScreen MainScreenInst { set; get; }

            public Context ResourceContext { set; get; }

            public bool IsConnecting { set; get; }

            public CarSession(CarService carService)
            {
                ResourceContext = CarContext;
                _carService = carService;
                _bcReceiver = new Receiver(this);
                Lifecycle.AddObserver(this);
            }

            public void OnStateChanged(ILifecycleOwner source, Lifecycle.Event e)
            {
                LogFormat("CarSession: OnStateChanged State={0}", e);

                if (e == Lifecycle.Event.OnCreate)
                {
                    ResourceContext = ActivityCommon.GetLocaleContext(CarContext);
                    InternalBroadcastManager.InternalBroadcastManager.GetInstance(CarContext).RegisterReceiver(_bcReceiver, new IntentFilter(CarSessionBroadcastAction));
                }
                else if (e == Lifecycle.Event.OnDestroy)
                {
                    InternalBroadcastManager.InternalBroadcastManager.GetInstance(CarContext).UnregisterReceiver(_bcReceiver);
                }
            }

            public override Screen OnCreateScreen(Intent intent)
            {
                LogFormat("CarSession: OnCreateScreen, Api={0}", CarContext.CarAppApiLevel);
                MainScreenInst = new MainScreen(CarContext, _carService, this);
                return MainScreenInst;
            }

            public static bool LogFormat(string format, params object[] args)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format(format, args));
#endif
                try
                {
                    string formatPrefix = GetLogPrefix() + format;
                    lock (EdiabasThread.DataLock)
                    {
                        ActivityCommon.EdiabasThread?.LogFormat(formatPrefix, args);
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            public static bool LogString(string info)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, info);
#endif
                try
                {
                    string infoPrefix = GetLogPrefix() + info;
                    lock (EdiabasThread.DataLock)
                    {
                        ActivityCommon.EdiabasThread?.LogString(infoPrefix);
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            private static string GetLogPrefix()
            {
                return string.Format(CultureInfo.InvariantCulture, "CarService [{0}]: ", DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            }

            public class Receiver(CarSession carSession) : BroadcastReceiver
            {
                public override void OnReceive(Context context, Intent intent)
                {
                    if (intent == null)
                    {
                        return;
                    }

                    string action = intent.Action;
                    switch (action)
                    {
                        case CarSessionBroadcastAction:
                        {
                            bool connectStarted = intent.GetBooleanExtra(ExtraConnectStarted, false);
                            bool activityStopped = intent.GetBooleanExtra(ExtraActivityStopped, false);

                            bool isConnecting = connectStarted;
                            if (activityStopped)
                            {
                                isConnecting = false;
                            }

                            bool connectingChanged = isConnecting != carSession.IsConnecting;
                            carSession.IsConnecting = isConnecting;
                            if (connectingChanged)
                            {
                                carSession.MainScreenInst?.RequestUpdate(false, 0);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public class BaseScreen : Screen, ILifecycleEventObserver
        {
            private readonly CarService _carServiceInst;
            private readonly CarSession _carSessionInst;
            private readonly Context _resourceContext;
            private readonly Handler _updateHandler;
            private readonly UpdateScreenRunnable _updateScreenRunnable;
            private readonly int _carAppApiLevel;
            private readonly int _listLimit;

            public CarService CarServiceInst => _carServiceInst;
            public CarSession CarSessionInst => _carSessionInst;
            public Context ResourceContext => _resourceContext;
            public int CarAppApiLevel => _carAppApiLevel;
            public int ListLimit => _listLimit;


            public BaseScreen(CarContext carContext, CarService carService, CarSession carSession) : base(carContext)
            {
                CarSession.LogFormat("BaseScreen: Class={0}", GetType().FullName);

                _carServiceInst = carService;
                _carSessionInst = carSession;
                _resourceContext = carSession.ResourceContext;
                _updateHandler = new Handler(Looper.MainLooper);
                _updateScreenRunnable = new UpdateScreenRunnable(this);
                _carAppApiLevel = carContext.CarAppApiLevel;
                _listLimit = GetContentLimit(ConstraintManager.ContentLimitTypeList, DefaultListItems);
#if DEBUG
                //_carAppApiLevel = 1;    // test with lower api level
#endif
                Lifecycle.AddObserver(this);
            }

            public void OnStateChanged(ILifecycleOwner source, Lifecycle.Event e)
            {
                CarSession.LogFormat("BaseScreen: OnStateChanged Class={0}, State={1}", GetType().FullName, e);

                if (e == Lifecycle.Event.OnStart)
                {
                    RequestUpdate();
                }
                else if (e == Lifecycle.Event.OnStop)
                {
                    RequestUpdate(true);
                }
            }

            public override ITemplate OnGetTemplate()
            {
                return null!;
            }

            public virtual void RequestUpdate(bool stop = false, int updateInterval = UpdateInterval)
            {
                Lifecycle.State currentState = Lifecycle.CurrentState;
                bool isValid = currentState == Lifecycle.State.Started || currentState == Lifecycle.State.Resumed;
                CarSession.LogFormat("RequestUpdate: State={0}, Valid={1}, Stop={2}, Class={3}", currentState, isValid, stop, GetType().FullName);

                _updateHandler.RemoveCallbacks(_updateScreenRunnable);
                if (isValid && !stop)
                {
                    CarSession.LogFormat("RequestUpdate: PostDelayed Class={0}", GetType().FullName);
                    _updateHandler.PostDelayed(_updateScreenRunnable, updateInterval);
                }
                else
                {
                    CarSession.LogFormat("RequestUpdate: Stopped Class={0}", GetType().FullName);
                }
            }

            public virtual bool ContentChanged()
            {
                return false;
            }

            public virtual bool GetConnected()
            {
                JobReader.PageInfo pageInfoActive = ActivityCommon.EdiabasThread?.JobPageInfo;
                return ActivityCommon.CommActive && pageInfoActive != null;
            }

            public virtual bool GetFgServiceActive()
            {
                return ActivityCommon.LockTypeLogging != ActivityCommon.LockType.None &&
                       ActivityCommon.LockTypeCommunication != ActivityCommon.LockType.None;
            }

            public virtual bool GetIsConnecting()
            {
                return CarSessionInst.IsConnecting;
            }

            public virtual bool GetForegroundServiceStarting()
            {
                return ForegroundService.IsCommThreadRunning();
            }

            public virtual string GetForegroundServiceStatus()
            {
                return ForegroundService.GetStatusText(ResourceContext);
            }

            public int GetContentLimit(int contentLimitType, int defaultValue)
            {
                try
                {
                    if (CarAppApiLevel >= 2)
                    {
                        if (CarContext.GetCarService(Java.Lang.Class.FromType(typeof(ConstraintManager))) is ConstraintManager constraintManager)
                        {
                            return constraintManager.GetContentLimit(ConstraintManager.ContentLimitTypeList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CarSession.LogFormat("GetContentLimit: Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                }

                return defaultValue;
            }
        }

        public class MainScreen(CarContext carContext, CarService carService, CarSession carSession) : BaseScreen(carContext, carService, carSession)
        {
            private Tuple<string, string> _lastContent = null;
            private readonly object _lockObject = new object();
            private bool _connected = false;
            private bool _isConnecting = false;
            private bool _fgServiceStarting = false;
            private bool _useService = false;
            private bool _configFileValid = false;
            private ActivityCommon.LockType _lockTypeComm = ActivityCommon.LockType.None;
            private ActivityCommon.LockType _lockTypeLogging = ActivityCommon.LockType.None;

            public override ITemplate OnGetTemplate()
            {
                CarSession.LogString("MainScreen: OnGetTemplate");

                _lastContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                bool loading = lastStructureContent == null || lastValueContent == null;

                bool connectedCopy;
                bool isConnectingCopy;
                bool fgServiceStartingCopy;
                bool useServiceCopy;
                bool configFileValidCopy;
                ActivityCommon.LockType lockTypeCommCopy;
                ActivityCommon.LockType lockTypeLoggingCopy;

                lock (_lockObject)
                {
                    connectedCopy = _connected;
                    isConnectingCopy = _isConnecting;
                    fgServiceStartingCopy = _fgServiceStarting;
                    useServiceCopy = _useService;
                    configFileValidCopy = _configFileValid;
                    lockTypeCommCopy = _lockTypeComm;
                    lockTypeLoggingCopy = _lockTypeLogging;
                }

                ItemList.Builder itemBuilderPagesBuilder = new ItemList.Builder();
                Row.Builder rowPageListBuilder = new Row.Builder()
                    .SetTitle(ResourceContext.GetString(Resource.String.car_service_page_list));

                if (!(useServiceCopy && connectedCopy))
                {
                    if (fgServiceStartingCopy)
                    {
                        rowPageListBuilder.AddText(ResourceContext.GetString(Resource.String.service_status_caption));
                        rowPageListBuilder.AddText(GetForegroundServiceStatus());
                        if (CarAppApiLevel >= 5)
                        {
                            rowPageListBuilder.SetEnabled(false);
                        }
                    }
                    else if (isConnectingCopy)
                    {
                        rowPageListBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_app_processing));
                    }
                    else
                    {
                        if (!useServiceCopy)
                        {
                            rowPageListBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_fg_service_disabled));
                        }
                        else
                        {
                            if (configFileValidCopy)
                            {
                                rowPageListBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_disconnected));
                            }
                            else
                            {
                                rowPageListBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_no_config));
                            }
                        }
                    }

                    rowPageListBuilder.SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                    {
                        if (fgServiceStartingCopy)
                        {
                            return;
                        }

                        if (ShowMainActivity())
                        {
                            CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_app_displayed),
                                CarToast.LengthLong).Show();
                        }
                    })));
                }
                else
                {
                    rowPageListBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_page_list_show));
                    rowPageListBuilder.SetBrowsable(true);
                    rowPageListBuilder.SetOnClickListener(new ActionListener((page) =>
                    {
                        try
                        {
                            ScreenManager.Push(new PageListScreen(CarContext, CarServiceInst, CarSessionInst));
                        }
                        catch (Exception ex)
                        {
                            CarSession.LogFormat("MainScreen: Push Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                        }
                    }));
                }
                itemBuilderPagesBuilder.AddItem(rowPageListBuilder.Build());

                AndroidX.Car.App.Model.Action actionButton = null;
                if (!isConnectingCopy && !fgServiceStartingCopy && configFileValidCopy)
                {
                    if (connectedCopy)
                    {
                        AndroidX.Car.App.Model.Action.Builder actionButtonBuilder = new AndroidX.Car.App.Model.Action.Builder()
                            .SetTitle(ResourceContext.GetString(Resource.String.car_service_button_disconnect))
                            .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                            {
                                if (ShowMainActivity(ActivityMain.CommOptionDisconnect))
                                {
                                    CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_app_displayed),
                                        CarToast.LengthLong).Show();
                                }
                            })));

                        actionButton = actionButtonBuilder.Build();
                    }
                    else
                    {
                        AndroidX.Car.App.Model.Action.Builder actionButtonBuilder = new AndroidX.Car.App.Model.Action.Builder()
                            .SetTitle(ResourceContext.GetString(Resource.String.car_service_button_connect))
                            .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                            {
                                if (ShowMainActivity(ActivityMain.CommOptionConnect))
                                {
                                    CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_app_displayed),
                                        CarToast.LengthLong).Show();
                                }
                            })));

                        actionButton = actionButtonBuilder.Build();
                    }
                }

                ItemList.Builder itemBuilderCommLockBuilder = new ItemList.Builder();
                bool disableLock = connectedCopy || isConnectingCopy;

                foreach (ActivityCommon.LockType lockType in Enum.GetValues(typeof(ActivityCommon.LockType)))
                {
                    string itemTitle = GetLockTypeTitle(lockType);
                    string itemText = GetLockTypeText(lockType, out bool invalidType);

                    if (string.IsNullOrEmpty(itemTitle))
                    {
                        continue;
                    }

                    Toggle.Builder toggleBuilder = new Toggle.Builder(new CheckListener(isChecked =>
                        {
                            if (disableLock || !isChecked)
                            {
                                Invalidate();
                                return;
                            }

                            SetLockType(lockType);
                        }))
                        .SetChecked(lockTypeCommCopy == lockType);
                    if (CarAppApiLevel >= 5)
                    {
                        if (disableLock || lockTypeCommCopy == lockType || invalidType)
                        {
                            toggleBuilder.SetEnabled(false);
                        }
                    }

                    Row.Builder itemRowBuilder = new Row.Builder()
                        .SetTitle(itemTitle)
                        .SetToggle(toggleBuilder.Build());

                    if (!string.IsNullOrEmpty(itemText))
                    {
                        itemRowBuilder.AddText(itemText);
                    }

                    itemBuilderCommLockBuilder.AddItem(itemRowBuilder.Build());
                }

                ItemList.Builder itemBuilderLoggingLockBuilder = new ItemList.Builder();

                foreach (ActivityCommon.LockType lockType in Enum.GetValues(typeof(ActivityCommon.LockType)))
                {
                    string itemTitle = GetLockTypeTitle(lockType);
                    string itemText = GetLockTypeText(lockType, out bool invalidType);

                    if (string.IsNullOrEmpty(itemTitle))
                    {
                        continue;
                    }

                    Toggle.Builder toggleBuilder = new Toggle.Builder(new CheckListener(isChecked =>
                        {
                            if (disableLock || !isChecked)
                            {
                                Invalidate();
                                return;
                            }

                            SetLockType(lockType, true);
                        }))
                        .SetChecked(lockTypeLoggingCopy == lockType);
                    if (CarAppApiLevel >= 5)
                    {
                        if (disableLock || lockTypeLoggingCopy == lockType || invalidType)
                        {
                            toggleBuilder.SetEnabled(false);
                        }
                    }

                    Row.Builder itemRowBuilder = new Row.Builder()
                        .SetTitle(itemTitle)
                        .SetToggle(toggleBuilder.Build());

                    if (!string.IsNullOrEmpty(itemText))
                    {
                        itemRowBuilder.AddText(itemText);
                    }

                    itemBuilderLoggingLockBuilder.AddItem(itemRowBuilder.Build());
                }

                ListTemplate.Builder listTemplateBuilder = new ListTemplate.Builder();
                Header.Builder headerBuilder = null;

                if (CarAppApiLevel >= 7)
                {
                    headerBuilder = new Header.Builder()
                        .SetStartHeaderAction(AndroidX.Car.App.Model.Action.AppIcon)
                        .SetTitle(ResourceContext.GetString(Resource.String.app_name));
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    listTemplateBuilder.SetHeaderAction(AndroidX.Car.App.Model.Action.AppIcon);
                    listTemplateBuilder.SetTitle(ResourceContext.GetString(Resource.String.app_name));
#pragma warning restore CS0618 // Type or member is obsolete
                }

                if (loading)
                {
                    listTemplateBuilder.SetLoading(true);
                }
                else
                {
                    listTemplateBuilder.AddSectionedList(SectionedItemList.Create(itemBuilderPagesBuilder.Build(),
                        ResourceContext.GetString(Resource.String.car_service_section_pages)));

                    listTemplateBuilder.AddSectionedList(SectionedItemList.Create(itemBuilderCommLockBuilder.Build(),
                        ResourceContext.GetString(Resource.String.settings_caption_lock_communication)));

                    listTemplateBuilder.AddSectionedList(SectionedItemList.Create(itemBuilderLoggingLockBuilder.Build(),
                        ResourceContext.GetString(Resource.String.settings_caption_lock_logging)));

                    if (actionButton != null)
                    {
                        if (headerBuilder != null)
                        {
                            headerBuilder.AddEndHeaderAction(actionButton);
                        }
                        else
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            ActionStrip.Builder actionStripBuilder = new ActionStrip.Builder()
                                .AddAction(actionButton);
                            listTemplateBuilder.SetActionStrip(actionStripBuilder.Build());
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                    }
                }

                if (headerBuilder != null)
                {
                    listTemplateBuilder.SetHeader(headerBuilder.Build());
                }

                RequestUpdate();

                return listTemplateBuilder.Build();
            }

            public override bool ContentChanged()
            {
                Tuple<string, string> newContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                string newStructureContent = newContent?.Item1;
                string newValueContent = newContent?.Item2;

                if (newStructureContent == null || newValueContent == null)
                {   // loading
                    CarSession.LogString("MainScreen: ContentChanged loading");
                    return true;
                }

                if (_lastContent != null && string.Compare(lastStructureContent ?? string.Empty, newStructureContent, StringComparison.Ordinal) != 0)
                {
                    CarSession.LogString("MainScreen: ContentChanged structure has changed");
                    try
                    {
                        CarContext.FinishCarApp();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    return false;
                }

                if (string.Compare(lastValueContent ?? string.Empty, newValueContent, StringComparison.Ordinal) != 0)
                {
                    CarSession.LogString("MainScreen: ContentChanged value has changed");
                    return true;
                }

                return false;
            }

            private Tuple<string, string> GetContentString()
            {
                try
                {
                    StringBuilder sbStructureContent = new StringBuilder();
                    StringBuilder sbValueContent = new StringBuilder();
                    bool connected = GetConnected();
                    bool isConnecting = GetIsConnecting();
                    bool fgServiceStarting = GetForegroundServiceStarting();
                    bool useService = GetFgServiceActive();
                    JobReader jobReader = ActivityCommon.JobReader;
                    string configFileName = jobReader.XmlFileName;

                    bool configFileValid = true;
                    if (jobReader.Configured)
                    {
                        configFileValid = !string.IsNullOrEmpty(configFileName);
                    }
                    ActivityCommon.LockType lockTypeComm = ActivityCommon.LockTypeCommunication;
                    ActivityCommon.LockType lockTypeLogging = ActivityCommon.LockTypeLogging;

                    sbStructureContent.AppendLine(ActivityCommon.GetLocaleSetting(CarContext));
                    sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_page_list));

                    sbValueContent.AppendLine();
                    if (!(connected && useService))
                    {
                        if (fgServiceStarting)
                        {
                            sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.service_status_caption));
                            sbValueContent.AppendLine(GetForegroundServiceStatus());
                        }
                        else if (isConnecting)
                        {
                            sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_app_processing));
                        }
                        else
                        {
                            if (!useService)
                            {
                                sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_fg_service_disabled));
                            }
                            else
                            {
                                if (configFileValid)
                                {
                                    sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_disconnected));
                                }
                                else
                                {
                                    sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_no_config));
                                }
                            }
                        }
                    }
                    else
                    {
                        sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_page_list_show));
                    }

                    if (!isConnecting && !fgServiceStarting && configFileValid)
                    {
                        if (connected)
                        {
                            sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_button_disconnect));
                        }
                        else
                        {
                            sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_button_connect));
                        }
                    }

                    sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.settings_caption_lock_communication));
                    sbValueContent.AppendLine(lockTypeComm.ToString());

                    sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.settings_caption_lock_logging));
                    sbValueContent.AppendLine(lockTypeLogging.ToString());

                    lock (_lockObject)
                    {
                        _connected = connected;
                        _isConnecting = isConnecting;
                        _fgServiceStarting = fgServiceStarting;
                        _useService = useService;
                        _configFileValid = configFileValid;
                        _lockTypeComm = lockTypeComm;
                        _lockTypeLogging = lockTypeLogging;
                    }

                    return new Tuple<string, string>(sbStructureContent.ToString(), sbValueContent.ToString());
                }
                catch (Exception ex)
                {
                    CarSession.LogFormat("MainScreen: GetContentString Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    return null;
                }
            }

            private string GetLockTypeTitle(ActivityCommon.LockType lockType)
            {
                switch (lockType)
                {
                    case ActivityCommon.LockType.None:
                        return ResourceContext.GetString(Resource.String.car_service_settings_lock_none);

                    case ActivityCommon.LockType.Cpu:
                        return ResourceContext.GetString(Resource.String.settings_lock_cpu);

                    case ActivityCommon.LockType.ScreenDim:
                        return ResourceContext.GetString(Resource.String.settings_lock_dim);

                    case ActivityCommon.LockType.ScreenBright:
                        return ResourceContext.GetString(Resource.String.settings_lock_bright);
                }

                return string.Empty;
            }

            private string GetLockTypeText(ActivityCommon.LockType lockType, out bool invalidType)
            {
                invalidType = false;
                switch (lockType)
                {
                    case ActivityCommon.LockType.None:
                        invalidType = true;
                        return ResourceContext.GetString(Resource.String.car_service_settings_invalid_mode);
                }

                return string.Empty;
            }

            private void SetLockType(ActivityCommon.LockType lockType, bool typeLogging = false)
            {
                bool changed = false;
                if (typeLogging)
                {
                    if (ActivityCommon.LockTypeLogging != lockType)
                    {
                        changed = true;
                        ActivityCommon.LockTypeLogging = lockType;
                    }
                }
                else
                {
                    if (ActivityCommon.LockTypeCommunication != lockType)
                    {
                        changed = true;
                        ActivityCommon.LockTypeCommunication = lockType;
                    }
                }

                if (changed)
                {
                    RequestUpdate(false, 0);

                    if (ShowMainActivity(null, ActivityMain.StoreOptionSettings))
                    {
                        CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_app_store_settings),
                            CarToast.LengthLong).Show();
                    }
                }
            }

            private bool ShowMainActivity(string commOption = null, string storeOption = null)
            {
                try
                {
                    Intent intent = new Intent(CarContext, typeof(ActivityMain));
                    //intent.SetAction(Intent.ActionMain);
                    //intent.AddCategory(Intent.CategoryLauncher);
                    intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    intent.PutExtra(ActivityMain.ExtraShowTitle, true);
                    intent.PutExtra(ActivityMain.ExtraNoAutoconnect, true);
                    if (!string.IsNullOrEmpty(commOption))
                    {
                        intent.PutExtra(ActivityMain.ExtraCommOption, commOption);
                    }

                    if (!string.IsNullOrEmpty(storeOption))
                    {
                        intent.PutExtra(ActivityMain.ExtraStoreOption, storeOption);
                    }

                    CarContext.StartActivity(intent);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public class PageListScreen(CarContext carContext, CarService carService, CarSession carSession) : BaseScreen(carContext, carService, carSession)
        {
            private Tuple<string, string> _lastContent = null;
            private readonly object _lockObject = new object();
            private bool _connected = false;
            private List<PageInfoEntry> _pageList;

            private class PageInfoEntry
            {
                public PageInfoEntry(string name, bool activePage, bool errorResetActive = false)
                {
                    Name = name;
                    ActivePage = activePage;
                    ErrorResetActive = errorResetActive;
                }

                public string Name { get; }

                public bool ActivePage { get; }

                public bool ErrorResetActive { get; }
            }

            public override ITemplate OnGetTemplate()
            {
                CarSession.LogString("PageListScreen: OnGetTemplate");

                _lastContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                bool loading = lastStructureContent == null || lastValueContent == null;

                bool connectedCopy;
                List<PageInfoEntry> pageListCopy;

                lock (_lockObject)
                {
                    connectedCopy = _connected;
                    pageListCopy = _pageList;
                }

                ItemList.Builder itemBuilderBuilder = new ItemList.Builder();

                if (!connectedCopy)
                {
                    itemBuilderBuilder.AddItem(new Row.Builder()
                        .SetTitle(ResourceContext.GetString(Resource.String.car_service_page_list))
                        .AddText(ResourceContext.GetString(Resource.String.car_service_disconnected))
                        .Build());
                }
                else
                {
                    int pageIndex = 0;
                    if (pageListCopy != null)
                    {
                        JobReader jobReader = ActivityCommon.JobReader;
                        bool errorResetActive = pageListCopy.Any(pageInfo => pageInfo.ErrorResetActive);

                        foreach (PageInfoEntry pageInfo in pageListCopy)
                        {
                            if (pageIndex >= ListLimit)
                            {
                                break;
                            }

                            string pageName = pageInfo.Name;
                            bool activePage = pageInfo.ActivePage;

                            Row.Builder rowBuilder = new Row.Builder()
                                .SetTitle(pageName)
                                .SetBrowsable(true)
                                .SetOnClickListener(new ActionListener((page) =>
                                {
                                    if (!(page is int index))
                                    {
                                        return;
                                    }

                                    if (index < 0 || index >= jobReader.PageList.Count)
                                    {
                                        return;
                                    }

                                    if (errorResetActive)
                                    {
                                        CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_error_reset_active), CarToast.LengthLong).Show();
                                        return;
                                    }

                                    JobReader.PageInfo newPageInfo = jobReader.PageList[index];
                                    lock (EdiabasThread.DataLock)
                                    {
                                        EdiabasThread ediabasThreadLocal = ActivityCommon.EdiabasThread;
                                        if (ediabasThreadLocal != null)
                                        {
                                            ediabasThreadLocal.JobPageInfo = newPageInfo;
                                        }
                                    }

                                    try
                                    {
                                        ScreenManager.Push(new PageScreen(CarContext, CarServiceInst, CarSessionInst));
                                    }
                                    catch (Exception ex)
                                    {
                                        CarSession.LogFormat("PageListScreen: Push Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                                    }
                                }, pageIndex));

                            if (errorResetActive)
                            {
                                if (pageInfo.ErrorResetActive)
                                {
                                    rowBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_error_reset_active));
                                }

                                if (CarAppApiLevel >= 5)
                                {
                                    rowBuilder.SetEnabled(false);
                                }
                            }
                            else if (activePage)
                            {
                                rowBuilder.AddText(ResourceContext.GetString(Resource.String.car_service_active_page));
                            }

                            itemBuilderBuilder.AddItem(rowBuilder.Build());
                            pageIndex++;
                        }

                        if (pageIndex == 0)
                        {
                            itemBuilderBuilder.AddItem(new Row.Builder()
                                .SetTitle(ResourceContext.GetString(Resource.String.car_service_no_pages))
                                .Build());
                        }
                    }
                }

                ListTemplate.Builder listTemplateBuilder = new ListTemplate.Builder();
                if (CarAppApiLevel >= 7)
                {
                    Header.Builder headerBuilder = new Header.Builder()
                        .SetStartHeaderAction(AndroidX.Car.App.Model.Action.Back)
                        .SetTitle(ResourceContext.GetString(Resource.String.car_service_page_list));
                    listTemplateBuilder.SetHeader(headerBuilder.Build());
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    listTemplateBuilder.SetHeaderAction(AndroidX.Car.App.Model.Action.Back);
                    listTemplateBuilder.SetTitle(ResourceContext.GetString(Resource.String.car_service_page_list));
#pragma warning restore CS0618 // Type or member is obsolete
                }

                if (loading)
                {
                    listTemplateBuilder.SetLoading(true);
                }
                else
                {
                    listTemplateBuilder.SetSingleList(itemBuilderBuilder.Build());
                }

                RequestUpdate();

                return listTemplateBuilder.Build();
            }

            public override bool ContentChanged()
            {
                Tuple<string, string> newContent = GetContentString();

                bool connectedCopy;
                lock (_lockObject)
                {
                    connectedCopy = _connected;
                }

                if (!connectedCopy)
                {
                    CarSession.LogString("PageListScreen: ContentChanged disconnected");

                    try
                    {
                        ScreenManager.PopToRoot();
                    }
                    catch (Exception ex)
                    {
                        CarSession.LogFormat("PageListScreen: Push Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    }

                    return false;
                }

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                string newStructureContent = newContent?.Item1;
                string newValueContent = newContent?.Item2;

                if (newStructureContent == null || newValueContent == null)
                {   // loading
                    CarSession.LogString("PageListScreen: ContentChanged loading");

                    return true;
                }

                if (_lastContent != null && string.Compare(lastStructureContent ?? string.Empty, newStructureContent, StringComparison.Ordinal) != 0)
                {
                    CarSession.LogString("PageListScreen: ContentChanged structure has changed");

                    try
                    {
                        ScreenManager.Pop();
                    }
                    catch (Exception ex)
                    {
                        CarSession.LogFormat("PageListScreen: Pop Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    }

                    return false;
                }

                if (string.Compare(lastValueContent ?? string.Empty, newValueContent, StringComparison.Ordinal) != 0)
                {
                    CarSession.LogString("PageListScreen: ContentChanged value has changed");
                    return true;
                }

                return false;
            }

            private Tuple<string, string> GetContentString()
            {
                try
                {
                    StringBuilder sbStructureContent = new StringBuilder();
                    StringBuilder sbValueContent = new StringBuilder();
                    JobReader.PageInfo pageInfoActive = ActivityCommon.EdiabasThread?.JobPageInfo;

                    bool connected = GetConnected();
                    List<PageInfoEntry> pageList = null;

                    if (!connected || pageInfoActive == null)
                    {
                        sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_page_list));

                        sbValueContent.AppendLine();
                        sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_disconnected));
                    }
                    else
                    {
                        JobReader jobReader = ActivityCommon.JobReader;
                        pageList = new List<PageInfoEntry>();
                        int pageIndex = 0;
                        foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
                        {
                            string pageName = ActivityMain.GetPageString(pageInfo, pageInfo.Name);
                            sbStructureContent.AppendLine(pageName);
                            bool activePage = pageInfo == pageInfoActive;

                            sbValueContent.AppendLine();

                            bool errorResetActive = false;
                            if (pageInfo.ErrorsInfo != null)
                            {
                                if (ActivityCommon.ErrorResetActive)
                                {
                                    errorResetActive = true;
                                }
                            }

                            if (errorResetActive)
                            {
                                sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_error_reset_active));
                            }
                            else if (activePage)
                            {
                                sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_active_page));
                            }

                            pageList.Add(new PageInfoEntry(pageName, activePage, errorResetActive));
                            pageIndex++;
                        }

                        if (pageIndex == 0)
                        {
                            sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_no_pages));
                        }
                    }

                    lock (_lockObject)
                    {
                        _connected = connected;
                        _pageList = pageList;
                    }

                    return new Tuple<string, string>(sbStructureContent.ToString(), sbValueContent.ToString());
                }
                catch (Exception ex)
                {
                    CarSession.LogFormat("PageListScreen: GetContentString Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    return null;
                }
            }
        }

        public class PageScreen(CarContext carContext, CarService carService, CarSession carSession) : BaseScreen(carContext, carService, carSession), IOnScreenResultListener
        {
            private Tuple<string, string, JobReader.PageInfo> _lastContent = null;
            private readonly object _lockObject = new object();
            private bool _connected = false;
            private string _pageTitle = string.Empty;
            private bool _errorPage = false;
            private List<ErrorMessageEntry> _errorList;
            private List<DataInfoEntry> _dataList;

            private class DataInfoEntry
            {
                public DataInfoEntry(string title, string result)
                {
                    Title = title;
                    Result = result;
                }

                public string Title { get; }
                public string Result { get; }
            }

            public override ITemplate OnGetTemplate()
            {
                CarSession.LogString("PageScreen: OnGetTemplate");

                _lastContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                bool loading = lastStructureContent == null || lastValueContent == null;

                ItemList.Builder itemBuilder = new ItemList.Builder();
                string pageTitle = ResourceContext.GetString(Resource.String.app_name);

                bool connectedCopy;
                string pageTitleCopy;
                bool errorPageCopy;
                List<ErrorMessageEntry> errorListCopy;
                List<DataInfoEntry> dataListCopy;

                lock (_lockObject)
                {
                    connectedCopy = _connected;
                    pageTitleCopy = _pageTitle;
                    errorPageCopy = _errorPage;
                    errorListCopy = _errorList;
                    dataListCopy = _dataList;
                }

                if (!string.IsNullOrEmpty(pageTitleCopy))
                {
                    pageTitle = pageTitleCopy;
                }

                if (!connectedCopy)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(ResourceContext.GetString(Resource.String.car_service_page_list))
                        .AddText(ResourceContext.GetString(Resource.String.car_service_disconnected))
                        .Build());
                }
                else
                {
                    if (errorPageCopy)
                    {
                        int lineIndex = 0;
                        if (errorListCopy != null)
                        {
                            foreach (ErrorMessageEntry errorEntry in errorListCopy)
                            {
                                if (lineIndex >= ListLimit)
                                {
                                    break;
                                }

                                string message = errorEntry.Message;
                                string ecuName = errorEntry.ErrorReport.EcuName;
                                bool validResponse = !string.IsNullOrEmpty(ecuName) && errorEntry.ErrorReport.ErrorDict != null;
                                bool shadow = errorEntry.ErrorReport is EdiabasThread.EdiabasErrorShadowReport;
                                if (!string.IsNullOrEmpty(message))
                                {
                                    string[] messageLines = message.Split(new[] { '\r', '\n' });

                                    string rowTitle = string.Empty;
                                    StringBuilder sbText = new StringBuilder();
                                    int lineCount = 0;

                                    foreach (string messageLine in messageLines)
                                    {
                                        if (string.IsNullOrEmpty(messageLine))
                                        {
                                            continue;
                                        }

                                        if (lineCount == 0)
                                        {
                                            rowTitle = messageLine;
                                        }
                                        else
                                        {
                                            if (sbText.Length > 0)
                                            {
                                                sbText.AppendLine();
                                            }

                                            sbText.Append(messageLine);
                                        }
                                        lineCount++;
                                    }

                                    if (!string.IsNullOrEmpty(rowTitle) && sbText.Length > 0)
                                    {
                                        Row.Builder rowBuilder = new Row.Builder()
                                            .SetTitle(rowTitle)
                                            .AddText(sbText.ToString())
                                            .SetOnClickListener(new ActionListener((page) =>
                                            {
                                                try
                                                {
                                                    string actionText = null;
                                                    Java.Lang.String actionResult = null;

                                                    if (validResponse && !shadow)
                                                    {
                                                        actionText = ResourceContext.GetString(Resource.String.car_service_error_reset);
                                                        actionResult = new Java.Lang.String(ecuName);
                                                    }

                                                    ScreenManager.PushForResult(new PageDetailScreen(CarContext, CarServiceInst, CarSessionInst, rowTitle, sbText.ToString(),
                                                        actionText, actionResult), this);
                                                }
                                                catch (Exception ex)
                                                {
                                                    CarSession.LogFormat("PageScreen: PushForResult Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                                                }
                                            }));

                                        itemBuilder.AddItem(rowBuilder.Build());
                                        lineIndex++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            loading = true;
                        }

                        if (lineIndex == 0 && !loading)
                        {
                            Row.Builder rowBuilder = new Row.Builder()
                                .SetTitle(ResourceContext.GetString(Resource.String.error_no_error));

                            itemBuilder.AddItem(rowBuilder.Build());
                        }
                    }
                    else if (dataListCopy != null)
                    {
                        int lineIndex = 0;
                        foreach (DataInfoEntry dataEntry in dataListCopy)
                        {
                            if (lineIndex >= ListLimit)
                            {
                                break;
                            }

                            string rowTitle = dataEntry.Title;
                            string result = dataEntry.Result;

                            Row.Builder rowBuilder = new Row.Builder()
                                .SetTitle(rowTitle);

                            if (!string.IsNullOrEmpty(result))
                            {
                                rowBuilder.AddText(result);
                            }

                            rowBuilder.SetOnClickListener(new ActionListener((page) =>
                            {
                                try
                                {
                                    ScreenManager.Push(new PageDetailScreen(CarContext, CarServiceInst, CarSessionInst, rowTitle, result));
                                }
                                catch (Exception ex)
                                {
                                    CarSession.LogFormat("PageScreen: Push Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                                }
                            }));

                            itemBuilder.AddItem(rowBuilder.Build());
                            lineIndex++;
                        }

                        if (lineIndex == 0)
                        {
                            itemBuilder.AddItem(new Row.Builder()
                                .SetTitle(ResourceContext.GetString(Resource.String.car_service_no_data))
                                .Build());
                        }
                    }
                }

                ListTemplate.Builder listTemplateBuilder = new ListTemplate.Builder();
                if (CarAppApiLevel >= 7)
                {
                    Header.Builder headerBuilder = new Header.Builder()
                        .SetStartHeaderAction(AndroidX.Car.App.Model.Action.Back)
                        .SetTitle(pageTitle);
                    listTemplateBuilder.SetHeader(headerBuilder.Build());
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    listTemplateBuilder.SetHeaderAction(AndroidX.Car.App.Model.Action.Back);
                    listTemplateBuilder.SetTitle(pageTitle);
#pragma warning restore CS0618 // Type or member is obsolete
                }

                if (loading)
                {
                    listTemplateBuilder.SetLoading(true);
                }
                else
                {
                    listTemplateBuilder.SetSingleList(itemBuilder.Build());
                }

                RequestUpdate();

                return listTemplateBuilder.Build();
            }

            public void OnScreenResult(Java.Lang.Object p0)
            {
                Java.Lang.String ecuNameString = p0 as Java.Lang.String;
                if (ecuNameString == null)
                {
                    return;
                }

                if (ActivityCommon.ErrorResetActive)
                {
                    CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_error_reset_active), CarToast.LengthLong).Show();
                    return;
                }

                string ecuName = ecuNameString.ToString();
                CarSession.LogFormat("PageScreen: OnScreenResult Ecu={0}", ecuName);

                List<string> errorResetList = new List<string>() { ecuName };
                lock (EdiabasThread.DataLock)
                {
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    if (ediabasThread != null)
                    {
                        ediabasThread.ErrorResetList = errorResetList;
                    }
                }

                // force app screen update
                ActivityCommon.EdiabasThread?.TriggerDisplayUpdate();
                CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_error_reset_started), CarToast.LengthLong).Show();
                try
                {
                    ScreenManager.Pop();
                }
                catch (Exception ex)
                {
                    CarSession.LogFormat("PageScreen: Pop Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                }
            }

            public override bool ContentChanged()
            {
                Tuple<string, string, JobReader.PageInfo> newContent = GetContentString();

                bool connectedCopy;
                bool errorPageCopy;
                lock (_lockObject)
                {
                    connectedCopy = _connected;
                    errorPageCopy = _errorPage;
                }

                if (!connectedCopy)
                {
                    CarSession.LogString("PageScreen: ContentChanged disconnected");

                    try
                    {
                        ScreenManager.PopToRoot();
                    }
                    catch (Exception ex)
                    {
                        CarSession.LogFormat("PageScreen: PopToRoot Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    }

                    return false;
                }

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                JobReader.PageInfo lastPageInfo = _lastContent?.Item3;
                string newStructureContent = newContent?.Item1;
                string newValueContent = newContent?.Item2;
                JobReader.PageInfo newPageInfo = newContent?.Item3;

                if (lastPageInfo != null && lastPageInfo != newPageInfo)
                {
                    CarSession.LogString("PageScreen: ContentChanged page has changed");

                    try
                    {
                        CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_active_page_change),
                            CarToast.LengthLong).Show();
                        ScreenManager.Pop();
                    }
                    catch (Exception ex)
                    {
                        CarSession.LogFormat("PageScreen: Pop Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    }

                    return false;
                }

                if (newStructureContent == null || newValueContent == null)
                {   // loading
                    CarSession.LogString("PageScreen: ContentChanged loading");
                    return true;
                }

                if (errorPageCopy)
                {
                    if (lastStructureContent == null || lastValueContent == null)
                    {   // loading
                        return true;
                    }
                }

                if (_lastContent != null && string.Compare(lastStructureContent ?? string.Empty, newStructureContent, StringComparison.Ordinal) != 0)
                {
                    CarSession.LogString("PageScreen: ContentChanged structure has changed");

                    try
                    {
                        CarToast.MakeText(CarContext, ResourceContext.GetString(Resource.String.car_service_active_data_amount_change),
                            CarToast.LengthLong).Show();
                        ScreenManager.Pop();
                    }
                    catch (Exception ex)
                    {
                        CarSession.LogFormat("PageScreen: Pop Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    }

                    return false;
                }

                if (string.Compare(lastValueContent ?? string.Empty, newValueContent, StringComparison.Ordinal) != 0)
                {
                    CarSession.LogString("PageScreen: ContentChanged value has changed");
                    return true;
                }

                return false;
            }

            private Tuple<string, string, JobReader.PageInfo> GetContentString()
            {
                try
                {
                    StringBuilder sbStructureContent = new StringBuilder();
                    StringBuilder sbValueContent = new StringBuilder();
                    JobReader.PageInfo pageInfoActive = ActivityCommon.EdiabasThread?.JobPageInfo;

                    string pageTitle = ResourceContext.GetString(Resource.String.app_name);
                    bool connected = GetConnected();
                    bool errorPage = false;
                    bool loading = false;

                    if (!connected || pageInfoActive == null)
                    {
                        sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_page_list));
                        sbValueContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_disconnected));
                        lock (_lockObject)
                        {
                            _errorList = null;
                            _dataList = null;
                        }
                    }
                    else
                    {
                        pageTitle = ActivityMain.GetPageString(pageInfoActive, pageInfoActive.Name);

                        if (pageInfoActive.ErrorsInfo != null)
                        {
                            errorPage = true;
                            List<EdiabasThread.EdiabasErrorReport> errorReportList = null;
                            lock (EdiabasThread.DataLock)
                            {
                                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                                if (ediabasThread != null)
                                {
                                    if (ediabasThread.ResultPageInfo == pageInfoActive)
                                    {
                                        errorReportList = ediabasThread.EdiabasErrorReportList;
                                    }
                                }
                            }

                            if (errorReportList == null)
                            {
                                lock (_lockObject)
                                {
                                    _errorList = null;
                                    _dataList = null;
                                }
                            }
                            else
                            {
                                CarServiceInst.EvaluateErrorMessages(ResourceContext, pageInfoActive, errorReportList, null,
                                    list =>
                                    {
                                        lock (_lockObject)
                                        {
                                            _errorList = list;
                                            _dataList = null;
                                        }
                                    });
                            }

                            List<ErrorMessageEntry> errorListCopy;
                            lock (_lockObject)
                            {
                                errorListCopy = _errorList;
                            }

                            int lineIndex = 0;
                            if (errorListCopy != null)
                            {
                                foreach (ErrorMessageEntry errorEntry in errorListCopy)
                                {
                                    string message = errorEntry.Message;
                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        sbStructureContent.AppendLine(message);

                                        sbValueContent.AppendLine();
                                        sbValueContent.AppendLine(message);
                                        lineIndex++;
                                    }
                                }
                            }
                            else
                            {
                                loading = true;
                            }

                            if (lineIndex == 0 && !loading)
                            {
                                sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.error_no_error));
                            }
                        }
                        else
                        {
                            MultiMap<string, EdiabasNet.ResultData> resultDict = null;
                            lock (EdiabasThread.DataLock)
                            {
                                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                                if (ediabasThread != null)
                                {
                                    if (ediabasThread.ResultPageInfo == pageInfoActive)
                                    {
                                        resultDict = ActivityCommon.EdiabasThread.EdiabasResultDict;
                                    }
                                }
                            }

                            int lineIndex = 0;
                            List<DataInfoEntry> dataList = new List<DataInfoEntry>();
                            foreach (JobReader.DisplayInfo displayInfo in pageInfoActive.DisplayList)
                            {
                                string rowTitle = ActivityMain.GetPageString(pageInfoActive, displayInfo.Name);
                                string result = string.Empty;

                                if (resultDict != null)
                                {
                                    result = ActivityCommon.EdiabasThread?.FormatResult(pageInfoActive, displayInfo, resultDict);
                                }

                                sbStructureContent.AppendLine(rowTitle);
                                sbValueContent.AppendLine();
                                if (!string.IsNullOrEmpty(result))
                                {
                                    sbValueContent.AppendLine(result);
                                }

                                dataList.Add(new DataInfoEntry(rowTitle, result));
                                lineIndex++;
                            }

                            if (lineIndex == 0)
                            {
                                sbStructureContent.AppendLine(ResourceContext.GetString(Resource.String.car_service_no_data));
                            }

                            lock (_lockObject)
                            {
                                _errorList = null;
                                _dataList = dataList;
                            }
                        }
                    }

                    lock (_lockObject)
                    {
                        _connected = connected;
                        _pageTitle = pageTitle;
                        _errorPage = errorPage;
                    }

                    sbStructureContent.AppendLine(pageTitle);
                    if (loading)
                    {
                        return new Tuple<string, string, JobReader.PageInfo>(null, null, pageInfoActive);
                    }

                    return new Tuple<string, string, JobReader.PageInfo>(sbStructureContent.ToString(), sbValueContent.ToString(), pageInfoActive);
                }
                catch (Exception ex)
                {
                    CarSession.LogFormat("PageScreen: GetContentString Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                    return null;
                }
            }
        }

        public class PageDetailScreen(CarContext carContext, CarService carService, CarSession carSession, string title, string message,
            string actionText = null, Java.Lang.Object actionResult = null) : BaseScreen(carContext, carService, carSession)
        {
            public override ITemplate OnGetTemplate()
            {
                string itemMessage = message;
                if (string.IsNullOrEmpty(itemMessage))
                {
                    itemMessage = ResourceContext.GetString(Resource.String.car_service_no_data);
                }

                CarSession.LogFormat("PageDetailScreen: OnGetTemplate, Title='{0}', Message='{1}', Action='{2}'",
                    title ?? string.Empty, itemMessage, actionText ?? string.Empty);

                if (CarAppApiLevel >= 2)
                {
                    AndroidX.Car.App.Model.Action actionButton = null;
                    if (!string.IsNullOrEmpty(actionText) && actionResult != null)
                    {
                        AndroidX.Car.App.Model.Action.Builder actionButtonBuilder = new AndroidX.Car.App.Model.Action.Builder()
                            .SetTitle(actionText)
                            .SetOnClickListener(new ActionListener((page) =>
                            {
                                SetResult(actionResult);

                                try
                                {
                                    ScreenManager.Pop();
                                }
                                catch (Exception ex)
                                {
                                    CarSession.LogFormat("PageDetailScreen: Pop Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                                }
                            }));

                        actionButton = actionButtonBuilder.Build();
                    }

                    LongMessageTemplate.Builder longMessageTemplateBuilder = new LongMessageTemplate.Builder(itemMessage)
                        .SetHeaderAction(AndroidX.Car.App.Model.Action.Back);
                    if (!string.IsNullOrEmpty(title))
                    {
                        longMessageTemplateBuilder.SetTitle(title);
                    }

                    if (actionButton != null)
                    {
                        ActionStrip.Builder actionStripBuilder = new ActionStrip.Builder();
                        actionStripBuilder.AddAction(actionButton);
                        longMessageTemplateBuilder.SetActionStrip(actionStripBuilder.Build());
                    }

                    return longMessageTemplateBuilder.Build();
                }

                // CarAppApiLevel < 2
                MessageTemplate.Builder messageTemplateBuilder = new MessageTemplate.Builder(itemMessage);
#pragma warning disable CS0618 // Type or member is obsolete
                messageTemplateBuilder.SetHeaderAction(AndroidX.Car.App.Model.Action.Back);
                if (!string.IsNullOrEmpty(title))
                {
                    messageTemplateBuilder.SetTitle(title);
                }
#pragma warning restore CS0618 // Type or member is obsolete

                return messageTemplateBuilder.Build();
            }
        }

        public class ActionListener(ActionListener.ClickDelegate handler, object parameter = null) : Java.Lang.Object, IOnClickListener
        {
            public delegate void ClickDelegate(object parameter);

            public void OnClick()
            {
                handler?.Invoke(parameter);
            }
        }

        public class CheckListener(CheckListener.CheckDelegate handler) : Java.Lang.Object, Toggle.IOnCheckedChangeListener
        {
            public delegate void CheckDelegate(bool isChecked);

            public void OnCheckedChange(bool p0)
            {
                handler?.Invoke(p0);
            }
        }

        public class UpdateScreenRunnable(BaseScreen screen) : Java.Lang.Object, Java.Lang.IRunnable
        {
            public void Run()
            {
                try
                {
                    if (screen != null)
                    {
                        bool invalidate = screen.ContentChanged();
                        CarSession.LogFormat("UpdateScreenRunnable: Invalidate={0}, Class={1}", invalidate, screen.GetType().FullName);

                        if (invalidate)
                        {
                            screen.Invalidate();
                        }

                        screen.RequestUpdate();
                    }
                }
                catch (Exception ex)
                {
                    CarSession.LogFormat("UpdateScreenRunnable: Exception '{0}'", EdiabasNet.GetExceptionText(ex));
                }
            }
        }
    }
}
