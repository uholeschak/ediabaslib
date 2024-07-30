using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace BmwDeepObd
{
    [Android.App.Service(
        Label = "@string/app_name",
        Name = ActivityCommon.AppNameSpace + "." + nameof(ForegroundService),
        ForegroundServiceType = Android.Content.PM.ForegroundService.TypeConnectedDevice
    )]
    public class ForegroundService : Android.App.Service
    {
#if DEBUG
        private static readonly string Tag = typeof(ForegroundService).FullName;
#endif
        public const int ServiceRunningNotificationId = 10000;
        public const string BroadcastMessageKey = "broadcast_message";
        public const string BroadcastStopComm = "stop_communication";
        public const string BroadcastFinishActivity = "finish_activity";
        public const string NotificationBroadcastAction = ActivityCommon.AppNameSpace + ".Notification.Action";
        public const string ActionBroadcastCommand = ActivityCommon.AppNameSpace + ".Action.Command";

        public const string ActionStartService = "ForegroundService.action.START_SERVICE";
        public const string ActionStopService = "ForegroundService.action.STOP_SERVICE";
        public const string ActionShowMainActivity = "ForegroundService.action.SHOW_MAIN_ACTIVITY";
        public const string ExtraStartComm = "StartComm";
        public const string ExtraAbortThread = "AbortThread";
        private const int UpdateInterval = 100;
        private const int NotificationUpdateDelay = 2000;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage
        };

        private enum StartState
        {
            None,
            WaitMedia,
            LoadSettings,
            CompileCode,
            InitReader,
            StartComm,
            Error,
            Terminate,
        }

        private bool _isStarted;
        private ActivityCommon _activityCommon;
        private ActivityCommon.InstanceDataCommon _instanceData;
        private Handler _stopHandler;
        private Java.Lang.Runnable _stopRunnable;
        private Handler _notificationHandler;
        private UpdateNotificationRunnable _notificationRunnable;
        private long _notificationUpdateTime;
        private static long _progressValue;
        private static volatile EdiabasThread.UpdateState _updateState;
        private static volatile StartState _startState;
        private static volatile bool _abortThread;
        private static volatile Thread _commThread;
        private static readonly object _threadLockObject;

        public ActivityCommon ActivityCommon => _activityCommon;

        public static bool AbortThread
        {
            get
            {
                lock (_threadLockObject)
                {
                    return _abortThread;
                }
            }
            set
            {
                lock (_threadLockObject)
                {
                    _abortThread = value;
                }
            }
        }

        static ForegroundService()
        {
            _startState = StartState.None;
            _abortThread = false;
            _commThread = null;
            _threadLockObject = new object();
        }

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Android.Util.Log.Info(Tag, "OnCreate: the service is initializing.");
#endif
            _stopHandler = new Handler(Looper.MainLooper);
            _stopRunnable = new Java.Lang.Runnable(StopEdiabasThread);
            _notificationHandler = new Handler(Looper.MainLooper);
            _notificationRunnable = new UpdateNotificationRunnable(this);
            _activityCommon = new ActivityCommon(this, null, BroadcastReceived);
            _activityCommon?.SetLock(ActivityCommon.LockType.Cpu);
            _instanceData = null;
            _notificationUpdateTime = DateTime.MinValue.Ticks;
            _progressValue = -1;
            _updateState = EdiabasThread.UpdateState.Init;

            lock (EdiabasThread.DataLock)
            {
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                if (ediabasThread != null)
                {
                    ediabasThread.ActiveContext = this;
                    ConnectEdiabasEvents();
                }
            }

            _activityCommon?.StartMtcService();
        }

        public override Android.App.StartCommandResult OnStartCommand(Intent intent, Android.App.StartCommandFlags flags, int startId)
        {
            if (intent?.Action == null)
            {
                return Android.App.StartCommandResult.RedeliverIntent;
            }

            switch (intent.Action)
            {
                case ActionStartService:
                {
                    bool startComm = intent.GetBooleanExtra(ExtraStartComm, false);
                    if (_isStarted)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "OnStartCommand: The service is already running.");
#endif
                    }
                    else
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "OnStartCommand: The service is starting.");
#endif
                        RegisterForegroundService();
                        _isStarted = true;
                    }

                    if (_isStarted)
                    {
                        if (startComm)
                        {
                            if (!BaseActivity.IsActivityListEmpty())
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, "OnStartCommand: Activities are active");
#endif
                                break;
                            }

                            if (!ActivityCommon.CommActive)
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, "OnStartCommand: Starting CommThread");
#endif
                                StartCommThread();
                            }
                        }

                        if (!IsCommThreadRunning())
                        {
                            _startState = StartState.None;
                            PostUpdateNotification();
                        }
                    }
                    break;
                }

                case ActionStopService:
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "OnStartCommand: The service is stopping.");
#endif
                    bool abortThread = intent.GetBooleanExtra(ExtraAbortThread, false);
                    if (IsCommThreadRunning())
                    {
                        switch (_startState)
                        {
                            case StartState.Error:
                            case StartState.Terminate:
                                break;

                            default:
                                if (abortThread)
                                {
#if DEBUG
                                    Android.Util.Log.Info(Tag, "OnStartCommand: Aborting thread");
#endif
                                    AbortThread = true;
                                }
                                break;
                        }
                        break;
                    }

                    SendStopCommBroadcast();
                    StopEdiabasThread(false);

                    if (!ActivityCommon.CommActive)
                    {
                        StopService();
                    }
                    break;
                }

                case ActionShowMainActivity:
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "OnStartCommand: Show main activity");
#endif
                    ShowMainActivity();
                    break;
                }
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return Android.App.StartCommandResult.RedeliverIntent;
        }

        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder.
            return null;
        }

        public override void OnDestroy()
        {
#if DEBUG
            Android.Util.Log.Info(Tag, "OnDestroy: Service is shutting down");
#endif
            // Remove the notification from the status bar.
            if (_notificationHandler != null)
            {
                try
                {
                    _notificationHandler.RemoveCallbacksAndMessages(null);
                }
                catch (Exception)
                {
                    // ignored
                }
                _notificationHandler = null;
            }

#if DEBUG
            Android.Util.Log.Info(Tag, "OnDestroy: Removing notifications");
#endif
            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Cancel(ServiceRunningNotificationId);

            DisconnectEdiabasEvents();
            lock (EdiabasThread.DataLock)
            {
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                if (ediabasThread != null)
                {
                    ediabasThread.ActiveContext = null;
                }
            }

            if (IsCommThreadRunning())
            {
                lock (_threadLockObject)
                {
                    _commThread?.Join();
                }
            }

            SendFinishActivityBroadcast();

            if (_activityCommon != null)
            {
                _activityCommon.StopMtcService();
                _activityCommon.SetLock(ActivityCommon.LockType.None);
                _activityCommon.Dispose();
                _activityCommon = null;
            }
            _isStarted = false;

            if (_stopHandler != null)
            {
                try
                {
                    _stopHandler.RemoveCallbacksAndMessages(null);
                }
                catch (Exception)
                {
                    // ignored
                }
                _stopHandler = null;
            }

            base.OnDestroy();
        }

        public static string GetStatusText(Context context)
        {
            if (context == null)
            {
                return string.Empty;
            }

            bool checkAbort = true;
            bool showProgress = false;
            string message = context.Resources.GetString(Resource.String.service_notification_comm_active);

            switch (_startState)
            {
                case StartState.None:
                    checkAbort = false;
                    if (!ActivityCommon.CommActive)
                    {
                        message = context.Resources.GetString(Resource.String.service_notification_idle);
                        break;
                    }

                    switch (_updateState)
                    {
                        case EdiabasThread.UpdateState.Init:
                            break;

                        case EdiabasThread.UpdateState.ReadErrors:
                        case EdiabasThread.UpdateState.Connected:
                            message = context.Resources.GetString(Resource.String.service_notification_comm_ok);
                            break;

                        default:
                            message = context.Resources.GetString(Resource.String.service_notification_comm_error);
                            break;
                    }
                    break;

                case StartState.WaitMedia:
                    message = context.Resources.GetString(Resource.String.service_notification_wait_media);
                    break;

                case StartState.LoadSettings:
                    message = context.Resources.GetString(Resource.String.service_notification_load_settings);
                    break;

                case StartState.CompileCode:
                    message = context.Resources.GetString(Resource.String.service_notification_compile_code);
                    showProgress = true;
                    break;

                case StartState.InitReader:
                    message = context.Resources.GetString(Resource.String.service_notification_init_reader);
                    showProgress = true;
                    break;

                case StartState.StartComm:
                    message = context.Resources.GetString(Resource.String.service_notification_connecting);
                    break;

                case StartState.Error:
                    checkAbort = false;
                    message = context.Resources.GetString(Resource.String.service_notification_error);
                    break;
            }

            if (checkAbort && AbortThread)
            {
                message = context.Resources.GetString(Resource.String.service_notification_abort);
            }

            if (showProgress)
            {
                if (_progressValue >= 0)
                {
                    message += " " + _progressValue + "%";
                }
            }

            return message;
        }

        private void StopService()
        {
            if (_isStarted)
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        StopForeground(Android.App.StopForegroundFlags.Remove);
                    }
                    else
                    {
#pragma warning disable CS0618
#pragma warning disable CA1422
                        StopForeground(true);
#pragma warning restore CA1422
#pragma warning restore CS0618
                    }

                    StopSelf();
                }
                catch (Exception)
                {
                    // ignored
                }

                _isStarted = false;
            }
        }

        private Android.App.Notification GetNotification()
        {
            string message = GetStatusText(this);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, ActivityCommon.NotificationChannelCommunication)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.ic_stat_obd)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOnlyAlertOnce(true)
                .SetOngoing(true)
                .SetPriority(NotificationCompat.PriorityLow)
                .SetCategory(NotificationCompat.CategoryService);

            NotificationCompat.Action action = BuildStopServiceAction();
            if (action != null)
            {
                builder.AddAction(action);
            }

            return builder.Build();
        }

        private void UpdateNotification(bool delayUpdate = false)
        {
            try
            {
                if (delayUpdate)
                {
                    if (Stopwatch.GetTimestamp() - _notificationUpdateTime < NotificationUpdateDelay * ActivityCommon.TickResolMs)
                    {
                        return;
                    }
                }

                Android.App.Notification notification = GetNotification();
                NotificationManagerCompat notificationManager = _activityCommon.NotificationManagerCompat;
                notificationManager?.Notify(ServiceRunningNotificationId, notification);

                _notificationUpdateTime = Stopwatch.GetTimestamp();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void PostUpdateNotification(bool delayUpdate = false)
        {
            if (_notificationHandler == null)
            {
                return;
            }

            if (!_notificationHandler.HasCallbacks(_notificationRunnable))
            {
                _notificationRunnable.DelayUpdate = delayUpdate;
                _notificationHandler.Post(_notificationRunnable);
            }
        }

        private void RegisterForegroundService()
        {
            try
            {
                Android.App.Notification notification = GetNotification();
                // Enlist this instance of the service as a foreground service
                ServiceCompat.StartForeground(this, ServiceRunningNotificationId, notification, (int) Android.Content.PM.ForegroundService.TypeConnectedDevice);
            }
#pragma warning disable CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            catch (Exception ex)
#pragma warning restore CS0168 // Variable ist deklariert, wird jedoch niemals verwendet
            {
                // ignored
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("RegisterForegroundService exception: {0}", ex.Message));
#endif
            }
        }

        private void SendStopCommBroadcast()
        {
            Intent broadcastIntent = new Intent(NotificationBroadcastAction);
            broadcastIntent.PutExtra(BroadcastMessageKey, BroadcastStopComm);
            InternalBroadcastManager.InternalBroadcastManager.GetInstance(this).SendBroadcast(broadcastIntent);
        }

        private void SendFinishActivityBroadcast()
        {
            Intent broadcastIntent = new Intent(NotificationBroadcastAction);
            broadcastIntent.PutExtra(BroadcastMessageKey, BroadcastFinishActivity);
            InternalBroadcastManager.InternalBroadcastManager.GetInstance(this).SendBroadcast(broadcastIntent);
            // if the activity has been destroyed, it will not be removed from the activity list
            bool isEmpty = BaseActivity.IsActivityListEmpty(new List<Type> { typeof(ActivityMain) });
            if (isEmpty)
            {
                BaseActivity.ClearActivityStack();
            }
        }

        private bool ShowMainActivity()
        {
            try
            {
                Intent intent = new Intent(this, typeof(ActivityMain));
                //intent.SetAction(Intent.ActionMain);
                //intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                intent.PutExtra(ActivityMain.ExtraShowTitle, true);
                StartActivity(intent);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ConnectEdiabasEvents()
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                ActivityCommon.EdiabasThread.DataUpdated += DataUpdated;
                ActivityCommon.EdiabasThread.ThreadTerminated += ThreadTerminated;
            }
        }

        private void DisconnectEdiabasEvents()
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                ActivityCommon.EdiabasThread.DataUpdated -= DataUpdated;
                ActivityCommon.EdiabasThread.ThreadTerminated -= ThreadTerminated;
            }
        }

        private void EdiabasEventHandler(bool connect)
        {
            // the GloblalLockObject is already locked
            if (connect)
            {
                ConnectEdiabasEvents();
            }
            else
            {
                DisconnectEdiabasEvents();
            }
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            EdiabasThread.UpdateState updateState;
            lock (EdiabasThread.DataLock)
            {
                updateState = ActivityCommon.EdiabasThread.UpdateProgressState;
            }

            bool changed = _updateState != updateState;
            _updateState = updateState;
            PostUpdateNotification(!changed);
        }

        private void ThreadTerminated(object sender, EventArgs e)
        {
            PostStopEdiabasThread();
        }

        private void PostStopEdiabasThread()
        {
            if (_stopHandler == null)
            {
                return;
            }

            if (!_stopHandler.HasCallbacks(_stopRunnable))
            {
                _stopHandler.Post(_stopRunnable);
            }
        }

        private void StopEdiabasThread()
        {
            if (_stopHandler == null)
            {
                return;
            }
            StopEdiabasThread(true);
        }

        private void StopEdiabasThread(bool wait)
        {
            lock (EdiabasThread.DataLock)
            {
                if (ActivityCommon.EdiabasThread != null)
                {
                    if (!ActivityCommon.EdiabasThread.ThreadStopping())
                    {
                        ActivityCommon.EdiabasThread.StopThread(wait);
                    }
                }
            }
            if (wait)
            {
                StopService();
                lock (EdiabasThread.DataLock)
                {
                    if (ActivityCommon.EdiabasThread != null)
                    {
                        ActivityCommon.EdiabasThread.Dispose();
                        ActivityCommon.EdiabasThread = null;
                    }
                }
            }
        }

        private bool StartCommThread()
        {
            if (IsCommThreadRunning())
            {
                return false;
            }

            if (ActivityCommon.CommActive)
            {
                return false;
            }

#if DEBUG
            Android.Util.Log.Info(Tag, "StartCommThread: Starting thread");
#endif

            lock (_threadLockObject)
            {
                _instanceData = null;
                _startState = StartState.WaitMedia;
                _progressValue = -1;
                _updateState = EdiabasThread.UpdateState.Init;
                _abortThread = false;   // already locked

                _commThread = new Thread(() =>
                {
                    try
                    {
                        for (; ; )
                        {
                            CommStateMachine();

                            if (_abortThread)
                            {
                                return;
                            }

                            switch (_startState)
                            {
                                case StartState.None:
                                case StartState.Error:
                                case StartState.Terminate:
                                    return;
                            }

                            Thread.Sleep(UpdateInterval);
                        }
                    }
                    finally
                    {
                        lock (_threadLockObject)
                        {
                            _commThread = null;

                            if (_abortThread)
                            {
                                _startState = StartState.Terminate;
                                _abortThread = false;
                            }
                        }

                        switch (_startState)
                        {
                            case StartState.Error:
                            case StartState.Terminate:
                                if (!ActivityCommon.CommActive)
                                {
                                    StopService();
                                }
                                break;

                            default:
                                PostUpdateNotification();
                                break;
                        }
                    }
                });
                _commThread.Start();
            }

            PostUpdateNotification();
            return true;
        }

        public static bool IsCommThreadRunning()
        {
            lock (_threadLockObject)
            {
                if (_commThread == null)
                {
                    return false;
                }

                if (_commThread.IsAlive)
                {
                    return true;
                }

                _commThread = null;
                _abortThread = false;   // already locked
            }
            return false;
        }

        private bool InitReader()
        {
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                if (!_activityCommon.IsInitEcuFunctionsRequired())
                {
                    return true;
                }

                if (!ActivityCommon.InitEcuFunctionReader(_instanceData.BmwPath, out string _, progress =>
                    {
                        if (progress == _progressValue)
                        {
                            return AbortThread;
                        }

                        _progressValue = progress;
                        PostUpdateNotification(true);
                        return AbortThread;
                    }))
                {
                    return false;
                }

                return true;
            }

            if (!_activityCommon.IsInitUdsReaderRequired())
            {
                return true;
            }

            if (!ActivityCommon.InitUdsReader(_instanceData.VagPath, out string _, progress =>
                {
                    if (progress == _progressValue)
                    {
                        return AbortThread;
                    }

                    _progressValue = progress;
                    PostUpdateNotification(true);
                    return AbortThread;
                }))
            {
                return false;
            }

            return true;
        }

        private bool LoadConfiguration()
        {
            try
            {
                if (!ActivityCommon.JobReader.ReadXml(_instanceData.ConfigFileName, out string _))
                {
                    return false;
                }

                if (ActivityCommon.JobReader.PageList.Count < 1)
                {
                    return false;
                }

                ActivityCommon.SelectedManufacturer = ActivityCommon.JobReader.Manufacturer;
                ActivityCommon.SelectedInterface = ActivityCommon.JobReader.Interface;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool RequestStoragePermissions(ActivityCommon.InstanceDataCommon instanceData)
        {
            if (_permissionsExternalStorage.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
            {
                return StoragePermissionGranted(instanceData);
            }

            if (!ActivityCommon.IsExtrenalStorageAccessRequired())
            {
                return StoragePermissionGranted(instanceData);
            }

            return false;
        }

        private bool StoragePermissionGranted(ActivityCommon.InstanceDataCommon instanceData)
        {
            ActivityCommon.SetStoragePath();
            ActivityCommon.RecentConfigListCleanup();
            return _activityCommon.UpdateDirectories(instanceData);
        }

        private bool CompileCode()
        {
            try
            {
                int compileCount = ActivityCommon.JobReader.PageList.Count(pageInfo => pageInfo.ClassCode != null);
                if (compileCount == 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "CompileCode: No compilation required");
#endif
                    return true;
                }

                List<Microsoft.CodeAnalysis.MetadataReference> referencesList = ActivityCommon.GetLoadedMetadataReferences(_instanceData.PackageAssembliesDir, out bool hasErrors);
                if (hasErrors)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "CompileCode: GetLoadedMetadataReferences failed");
#endif
                    return false;
                }

                int compileIndex = 0;
                foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                {
                    if (pageInfo.ClassCode == null)
                    {
                        continue;
                    }

                    if (AbortThread)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CompileCode: Aborted");
#endif
                        return false;
                    }

                    string result = _activityCommon.CompileCode(pageInfo, referencesList);
                    if (!string.IsNullOrEmpty(result))
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CompileCode: CompileCode failed");
#endif
                        return false;
                    }

                    compileIndex++;
                    _progressValue = compileIndex * 100 / compileCount;
                    PostUpdateNotification(true);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CommStateMachine()
        {
            if (ActivityCommon.CommActive)
            {
                _startState = StartState.None;
#if DEBUG
                Android.Util.Log.Info(Tag, "CommStateMachine: Communication active, stopping timer");
#endif
                return;
            }

            switch (_startState)
            {
                case StartState.WaitMedia:
                {
                    if (!_activityCommon.IsExStorageAvailable())
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: External storage not available");
#endif
                        Thread.Sleep(500);
                        break;
                    }

#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: External storage available");
#endif
                    _startState = StartState.LoadSettings;
                    PostUpdateNotification();
                    break;
                }

                case StartState.LoadSettings:
                {
                    ActivityCommon.InstanceDataCommon instanceData = new ActivityCommon.InstanceDataCommon();
                    if (!_activityCommon.GetSettings(instanceData, ActivityCommon.SettingsMode.All, true))
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: GetSettings failed");
#endif
                        _startState = StartState.Error;
                        return;
                    }

                    if (!RequestStoragePermissions(instanceData))
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: Request permissions failed");
#endif
                        _startState = StartState.Error;
                        return;
                    }

                    if (instanceData.LastSelectedJobIndex < 0)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: No page selected");
#endif
                        _startState = StartState.Terminate;
                        return;
                    }

                    _instanceData = instanceData;
#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: GetSettings Ok");
#endif
                    if (!LoadConfiguration())
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: LoadConfiguration failed");
#endif
                        _startState = StartState.Error;
                        return;
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: LoadConfiguration Ok");
#endif
                    _startState = StartState.CompileCode;
                    PostUpdateNotification();
                    break;
                }

                case StartState.CompileCode:
                {
                    if (_instanceData == null)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: CompileCode no instance");
#endif
                        _startState = StartState.Error;
                        return;
                    }

#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: CompileCode start");
#endif
                    if (!CompileCode())
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: CompileCode failed");
#endif
                        _startState = StartState.Error;
                        return;
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: CompileCode Ok");
#endif
                    _startState = StartState.InitReader;
                    _progressValue = -1;
                    PostUpdateNotification();
                    break;
                }

                case StartState.InitReader:
                {
                    if (_instanceData == null)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: InitReader no instance");
#endif
                        _startState = StartState.Error;
                        return;
                    }

#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: InitReader start");
#endif
                    if (!InitReader())
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: InitReader failed");
#endif
                        _startState = StartState.Error;
                        return;
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, "CommStateMachine: InitReader Ok");
#endif
                    _startState = StartState.StartComm;
                    _progressValue = -1;
                    PostUpdateNotification();
                    break;
                }

                case StartState.StartComm:
                {
                    if (_instanceData == null)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: StartComm no instance");
#endif
                        _startState = StartState.Error;
                        return;
                    }

                    if (!ActivityCommon.CommActive)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: StartEdiabasThread start");
#endif
                        JobReader.PageInfo pageInfo = null;
                        if (ActivityCommon.JobReader.PageList.Count > 0)
                        {
                            int jobIndex = _instanceData.LastSelectedJobIndex;
                            if (jobIndex >= 0 && jobIndex < ActivityCommon.JobReader.PageList.Count)
                            {
                                pageInfo = ActivityCommon.JobReader.PageList[jobIndex];
                            }
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: StartEdiabasThread no page selected");
#endif
                        if (!_activityCommon.StartEdiabasThread(_instanceData, pageInfo, EdiabasEventHandler))
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, "CommStateMachine: StartEdiabasThread failed");
#endif
                            _startState = StartState.Error;
                            return;
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: StartEdiabasThread Ok");
#endif
                    }

                    if (ActivityCommon.CommActive)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "CommStateMachine: Communication active, stopping timer");
#endif
                        _startState = StartState.None;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        private Android.App.PendingIntent BuildIntentToShowMainActivity()
        {
            Intent showMainActivityIntent = new Intent(this, GetType());
            showMainActivityIntent.SetAction(ActionShowMainActivity);
            Android.App.PendingIntentFlags intentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                intentFlags |= Android.App.PendingIntentFlags.Mutable;
            }
            Android.App.PendingIntent pendingIntent = Android.App.PendingIntent.GetService(this, 0, showMainActivityIntent, intentFlags);
            return pendingIntent;
        }

        /// <summary>
        /// Builds the Notification.Action that will allow the user to stop the service via the
        /// notification in the status bar
        /// </summary>
        /// <returns>The stop service action.</returns>
        private NotificationCompat.Action BuildStopServiceAction()
        {
            string message;
            if (ActivityCommon.CommActive)
            {
                message = Resources.GetString(Resource.String.service_stop_comm);
            }
            else
            {
                switch (_startState)
                {
                    case StartState.Error:
                    case StartState.Terminate:
                        return null;

                    default:
                        message = Resources.GetString(Resource.String.service_abort_operation);
                        break;
                }
            }

            Intent stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(ActionStopService);
            stopServiceIntent.PutExtra(ExtraAbortThread, true);
            Android.App.PendingIntentFlags intentFlags = 0;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                intentFlags |= Android.App.PendingIntentFlags.Mutable;
            }
            Android.App.PendingIntent stopServicePendingIntent = Android.App.PendingIntent.GetService(this, 0, stopServiceIntent, intentFlags);

            NotificationCompat.Action.Builder builder = new NotificationCompat.Action.Builder(Resource.Drawable.ic_stat_cancel, message, stopServicePendingIntent);
            return builder.Build();
        }

        private void BroadcastReceived(Context context, Intent intent)
        {
            if (intent == null)
            {
                return;
            }
            string action = intent.Action;
            switch (action)
            {
                case ActionBroadcastCommand:
                {
                    HandleActionBroadcast(intent);
                    HandleCustomBroadcast(context, intent);
                    break;
                }
            }
        }

        private void HandleActionBroadcast(Intent intent)
        {
            string request = intent.GetStringExtra("action");
            if (string.IsNullOrEmpty(request))
            {
                return;
            }
            string[] requestList = request.Split(':');
            if (requestList.Length < 1)
            {
                return;
            }
            if (string.Compare(requestList[0], "new_page", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (requestList.Length < 2)
                {
                    return;
                }
                JobReader.PageInfo pageInfoSel = null;
                foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                {
                    if (string.Compare(pageInfo.Name, requestList[1], StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        pageInfoSel = pageInfo;
                        break;
                    }
                }
                if (pageInfoSel == null)
                {
                    return;
                }
                if (!ActivityCommon.CommActive)
                {
                    return;
                }
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                if (ediabasThread == null)
                {
                    return;
                }
                if (ediabasThread.JobPageInfo != pageInfoSel)
                {
                    ActivityCommon.EdiabasThread.CommActive = true;
                    ediabasThread.JobPageInfo = pageInfoSel;
                }
            }
        }

        private void HandleCustomBroadcast(Context context, Intent intent)
        {
            try
            {
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                // ReSharper disable once UseNullPropagation
                if (ediabasThread == null)
                {
                    return;
                }
                JobReader.PageInfo pageInfo = ediabasThread.JobPageInfo;
                if (pageInfo.ClassObject == null)
                {
                    return;
                }
                Type pageType = pageInfo.ClassObject.GetType();
                MethodInfo broadcastReceived = pageType.GetMethod("BroadcastReceived", new[] { typeof(JobReader.PageInfo), typeof(Context), typeof(Intent) });
                if (broadcastReceived == null)
                {
                    return;
                }
                object[] args = { pageInfo, context, intent };
                broadcastReceived.Invoke(pageInfo.ClassObject, args);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public class UpdateNotificationRunnable : Java.Lang.Object, Java.Lang.IRunnable
        {
            private ForegroundService _foregroundService;

            public bool DelayUpdate { get; set; }

            public UpdateNotificationRunnable(ForegroundService foregroundService)
            {
                _foregroundService = foregroundService;
                DelayUpdate = false;
            }

            public void Run()
            {
                try
                {
                    _foregroundService?.UpdateNotification(DelayUpdate);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
