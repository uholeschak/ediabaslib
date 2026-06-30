using System;
using System.Diagnostics;
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
        public const int ServiceRunningNotificationId = 10001;
        public const string ActionBroadcastCommand = ActivityCommon.AppNameSpace + ".Action.Command";
        public const string ActionStartService = "BmwRpcForegroundService.action.START_SERVICE";
        public const string ActionStopService = "BmwRpcForegroundService.action.STOP_SERVICE";
        public const string ActionShowMainActivity = "BmwRpcForegroundService.action.SHOW_MAIN_ACTIVITY";
        private const int NotificationUpdateDelay = 2000;

        private bool _isStarted;
        private ActivityCommon _activityCommon;
        private Context _resourceContext;
        private Handler _stopHandler;
        private Handler _notificationHandler;
        private UpdateNotificationRunnable _notificationRunnable;
        private long _notificationUpdateTime;

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
#if DEBUG
                    Android.Util.Log.Info(Tag, "OnStartCommand: The service is starting.");
#endif
                    RegisterForegroundService();
                    _isStarted = true;
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
                    ShowRpcCodingActivity();
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

        private bool ShowRpcCodingActivity()
        {
            try
            {
                Intent intent = new Intent(this, typeof(BmwRpcCodingActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);
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
                    HandleMessageBroadcast(intent);
                    break;
                }
            }
        }

        private void HandleMessageBroadcast(Intent intent)
        {
            string request = intent.GetStringExtra("message");
            if (string.IsNullOrEmpty(request))
            {
                return;
            }

            PostUpdateNotification(true);
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
