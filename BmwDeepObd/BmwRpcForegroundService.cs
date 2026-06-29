using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace BmwDeepObd
{
    [Android.App.Service(
        Label = "@string/app_name",
        DirectBootAware = true,
        Name = ActivityCommon.AppNameSpace + "." + nameof(BmwRpcForegroundService),
        ForegroundServiceType = Android.Content.PM.ForegroundService.TypeConnectedDevice
    )]
    public class BmwRpcForegroundService : Android.App.Service
    {
#if DEBUG
        private static readonly string Tag = typeof(BmwRpcForegroundService).FullName;
#endif
        public const int ServiceRunningNotificationId = 10000;
        public const string BroadcastMessageKey = "broadcast_message";
        public const string BroadcastStopComm = "stop_communication";
        public const string NotificationBroadcastAction = ActivityCommon.AppNameSpace + ".Notification.Action";
        public const string ActionBroadcastCommand = ActivityCommon.AppNameSpace + ".Action.Command";

        public const string ActionStartService = "BmwRpcForegroundService.action.START_SERVICE";
        public const string ActionStopService = "BmwRpcForegroundService.action.STOP_SERVICE";
        public const string ActionShowMainActivity = "BmwRpcForegroundService.action.SHOW_MAIN_ACTIVITY";
        public const string ExtraStartComm = "StartComm";
        private const int NotificationUpdateDelay = 2000;

        private bool _isStarted;
        private ActivityCommon _activityCommon;
        private Context _resourceContext;
        private Handler _stopHandler;
        private Handler _notificationHandler;
        private UpdateNotificationRunnable _notificationRunnable;
        private long _notificationUpdateTime;
        private static volatile bool _abortThread;
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

        static BmwRpcForegroundService()
        {
            _abortThread = false;
            _threadLockObject = new object();
        }

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Android.Util.Log.Info(Tag, "OnCreate: the service is initializing.");
#endif
            _stopHandler = new Handler(Looper.MainLooper);
            _notificationHandler = new Handler(Looper.MainLooper);
            _notificationRunnable = new UpdateNotificationRunnable(this);
            _activityCommon = new ActivityCommon(this, null, BroadcastReceived);
            _activityCommon?.SetLock(ActivityCommon.LockType.Cpu);
            _resourceContext = ActivityCommon.GetLocaleContext(this);
            _notificationUpdateTime = DateTime.MinValue.Ticks;

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
                    break;
                }

                case ActionStopService:
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "OnStartCommand: The service is stopping.");
#endif
                    StopService();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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
            string message = string.Empty;

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, ActivityCommon.NotificationChannelCommunication)
                .SetContentTitle(_resourceContext.GetString(Resource.String.app_name))
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.ic_stat_obd)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOnlyAlertOnce(true)
                .SetOngoing(true)
                .SetPriority(NotificationCompat.PriorityLow)
                .SetCategory(NotificationCompat.CategoryService);

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

            ActivityCommon.PostRunnable(_notificationHandler, _notificationRunnable, () =>
            {
                _notificationRunnable.DelayUpdate = delayUpdate;
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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

        private bool ShowMainActivity()
        {
            try
            {
                Intent intent = new Intent(this, typeof(ActivityMain));
                //intent.SetAction(Intent.ActionMain);
                //intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                intent.PutExtra(ActivityMain.ExtraShowTitle, true);
                intent.PutExtra(ActivityMain.ExtraNoAutoconnect, true);
                StartActivity(intent);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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

                lock (EdiabasThread.DataLock)
                {
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    if (ediabasThread != null)
                    {
                        if (ediabasThread.JobPageInfo != pageInfoSel)
                        {
                            ediabasThread.CommActive = true;
                            ediabasThread.JobPageInfo = pageInfoSel;
                        }
                    }
                }
            }
        }

        private void HandleCustomBroadcast(Context context, Intent intent)
        {
            try
            {
                JobReader.PageInfo pageInfo;
                lock (EdiabasThread.DataLock)
                {
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    // ReSharper disable once UseNullPropagation
                    if (ediabasThread == null)
                    {
                        return;
                    }

                    pageInfo = ediabasThread.JobPageInfo;
                }

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
            private BmwRpcForegroundService _foregroundService;

            public bool DelayUpdate { get; set; }

            public UpdateNotificationRunnable(BmwRpcForegroundService foregroundService)
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
