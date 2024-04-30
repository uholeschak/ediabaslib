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
        }

        private bool _isStarted;
        private ActivityCommon _activityCommon;
        private ActivityCommon.InstanceDataCommon _instanceData;
        private Handler _stopHandler;
        private Java.Lang.Runnable _stopRunnable;
        private long _progressValue;
        private volatile EdiabasThread.UpdateState _updateState;
        private long _notificationUpdateTime;
        private static volatile StartState _startState;
        private static volatile bool _abortThread;
        private static volatile Thread _commThread;
        private static readonly object _threadLockObject;

        public ActivityCommon ActivityCommon => _activityCommon;

        private static bool AbortThread
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
            _activityCommon = new ActivityCommon(this, null, BroadcastReceived);
            _activityCommon?.SetLock(ActivityCommon.LockType.Cpu);
            _instanceData = null;
            _progressValue = -1;
            _updateState = EdiabasThread.UpdateState.Init;
            _notificationUpdateTime = DateTime.MinValue.Ticks;

            lock (ActivityCommon.GlobalLockObject)
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
                return Android.App.StartCommandResult.Sticky;
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
                        if (startComm)
                        {
                            if (!ActivityCommon.CommActive)
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, "OnStartCommand: Starting CommThread");
#endif
                                StartCommThread();
                            }
                        }
                        _isStarted = true;
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
                        if (abortThread)
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, "OnStartCommand: Aborting thread");
#endif
                            AbortThread = true;
                        }

                        break;
                    }

                    SendStopCommBroadcast();
                    StopEdiabasThread(false);

                    if (!ActivityCommon.CommActive)
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
            return Android.App.StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder.
            return null;
        }

        public override void OnDestroy()
        {
            // We need to shut things down.
            //Log.Info(Tag, "OnDestroy: The started service is shutting down.");

            // Remove the notification from the status bar.
            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Cancel(ServiceRunningNotificationId);
            DisconnectEdiabasEvents();
            lock (ActivityCommon.GlobalLockObject)
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

        private Android.App.Notification GetNotification()
        {
            bool checkAbort = true;
            bool showProgress = false;
            string message = string.Empty;

            switch (_startState)
            {
                case StartState.None:
                    checkAbort = false;
                    if (!ActivityCommon.CommActive)
                    {
                        message = Resources.GetString(Resource.String.service_notification_idle);
                        break;
                    }

                    if (_updateState != EdiabasThread.UpdateState.Connected)
                    {
                        message = Resources.GetString(Resource.String.service_notification_comm_error);
                        break;
                    }
                    message = Resources.GetString(Resource.String.service_notification_comm_active);
                    break;

                case StartState.WaitMedia:
                    message = Resources.GetString(Resource.String.service_notification_wait_media);
                    break;

                case StartState.LoadSettings:
                    message = Resources.GetString(Resource.String.service_notification_load_settings);
                    break;

                case StartState.CompileCode:
                    message = Resources.GetString(Resource.String.service_notification_compile_code);
                    showProgress = true;
                    break;

                case StartState.InitReader:
                    message = Resources.GetString(Resource.String.service_notification_init_reader);
                    showProgress = true;
                    break;

                case StartState.StartComm:
                    message = Resources.GetString(Resource.String.service_notification_connecting);
                    break;

                case StartState.Error:
                    checkAbort = false;
                    message = Resources.GetString(Resource.String.service_notification_error);
                    break;
            }

            if (checkAbort && AbortThread)
            {
                message = Resources.GetString(Resource.String.service_notification_abort);
            }

            if (showProgress)
            {
                if (_progressValue >= 0)
                {
                    message += " " + _progressValue + "%";
                }
            }

            Android.App.Notification notification = new NotificationCompat.Builder(this, ActivityCommon.NotificationChannelCommunication)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.ic_stat_obd)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOnlyAlertOnce(true)
                .SetOngoing(true)
                .AddAction(BuildStopServiceAction())
                .SetPriority(NotificationCompat.PriorityLow)
                .SetCategory(NotificationCompat.CategoryService)
                .Build();

            return notification;
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

        private void ShowMainActivity()
        {
            try
            {
                Intent intent = new Intent(this, typeof(ActivityMain));
                //intent.SetAction(Intent.ActionMain);
                //intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                intent.PutExtra(ActivityMain.ExtraShowTitle, true);
                StartActivity(intent);
            }
            catch (Exception)
            {
                // ignored
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

            if (_updateState != updateState)
            {
                _updateState = updateState;
                UpdateNotification(true);
            }
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
            lock (ActivityCommon.GlobalLockObject)
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
                lock (ActivityCommon.GlobalLockObject)
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
                    for (; ; )
                    {
                        CommStateMachine();

                        switch (_startState)
                        {
                            case StartState.None:
                            case StartState.Error:
                                UpdateNotification();
                                return;

                            default:
                                if (AbortThread)
                                {
                                    _startState = StartState.Error;
                                    AbortThread = false;
                                    UpdateNotification();
                                    return;
                                }
                                break;
                        }

                        Thread.Sleep(UpdateInterval);
                    }
                });
                _commThread.Start();
            }

            UpdateNotification();
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
                        UpdateNotification(true);
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
                    UpdateNotification(true);
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

                List<Microsoft.CodeAnalysis.MetadataReference> referencesList = _activityCommon.GetLoadedMetadataReferences(_instanceData.PackageAssembliesDir, out bool hasErrors);
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
                    UpdateNotification(true);
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
                    UpdateNotification();
                    break;
                }

                case StartState.LoadSettings:
                {
                    string settingsFile = ActivityCommon.GetSettingsFileName();
                    if (!string.IsNullOrEmpty(settingsFile) && File.Exists(settingsFile))
                    {
                        ActivityCommon.InstanceDataCommon instanceData = new ActivityCommon.InstanceDataCommon();
                        if (!_activityCommon.GetSettings(instanceData, settingsFile, ActivityCommon.SettingsMode.All, true))
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
                        UpdateNotification();
                    }

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
                    UpdateNotification();
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
                    UpdateNotification();
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
                            pageInfo = ActivityCommon.JobReader.PageList[0];
                        }

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
            Intent stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(ActionStopService);
            stopServiceIntent.PutExtra(ExtraAbortThread, true);
            Android.App.PendingIntentFlags intentFlags = 0;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                intentFlags |= Android.App.PendingIntentFlags.Mutable;
            }
            Android.App.PendingIntent stopServicePendingIntent = Android.App.PendingIntent.GetService(this, 0, stopServiceIntent, intentFlags);

            string message;
            if (ActivityCommon.CommActive)
            {
                message = Resources.GetString(Resource.String.service_stop_comm);
            }
            else
            {
                message = Resources.GetString(Resource.String.service_abort_operation);
            }

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
    }
}
