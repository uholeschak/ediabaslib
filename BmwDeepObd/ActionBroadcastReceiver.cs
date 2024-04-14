using Android.Content;
using Android.OS;
using Android.Util;
using System;

namespace BmwDeepObd;

[BroadcastReceiver(
    Exported = false,
    Enabled = true,
    Name = ActivityCommon.AppNameSpace + "." + nameof(ActionBroadcastReceiver)
)]
[Android.App.IntentFilter(new[]
    {
        Intent.ActionBootCompleted,
        Intent.ActionReboot,
        Intent.ActionMyPackageReplaced,
        Intent.ActionMyPackageSuspended,
        Intent.ActionMyPackageUnsuspended,
    },
    Categories = new[]
    {
        Intent.CategoryDefault
    }
    ,Priority = 100)
]

public class ActionBroadcastReceiver : BroadcastReceiver
{
#if DEBUG
    private static readonly string Tag = typeof(ActionBroadcastReceiver).FullName;
#endif
    public const string ActionStartTimer = ActivityCommon.AppNameSpace + ".ActionStartTimer";

    private Android.App.PendingIntent _alarmIntent;

    public override void OnReceive(Context context, Intent intent)
    {
        if (intent?.Action == null)
        {
#if DEBUG
            Log.Info(Tag, "Action missing");
#endif
            return;
        }

#if DEBUG
        Log.Info(Tag, string.Format("Action received: {0}", intent.Action));
#endif
        switch (intent.Action)
        {
            case Intent.ActionBootCompleted:
            case Intent.ActionReboot:
            case Intent.ActionMyPackageReplaced:
            case Intent.ActionMyPackageSuspended:
            case Intent.ActionMyPackageUnsuspended:
            case ActionStartTimer:
                ScheduleAlarm(context);
                break;
        }
    }

    public bool ScheduleAlarm(Context context)
    {
        try
        {
            if (_alarmIntent != null)
            {
                CancelAlarm(context);
            }

            Android.App.AlarmManager alarms = context?.GetSystemService(Context.AlarmService) as Android.App.AlarmManager;
            if (alarms == null)
            {
#if DEBUG
                Log.Info(Tag, "ScheduleAlarm: No alarm manager");
#endif
                return false;
            }

            Intent actionIntent = new Intent(context, typeof(BackgroundAlarmReceiver));
            actionIntent.SetAction(BackgroundAlarmReceiver.ActionTimeElapsed);

            long interval = 1000 * 5;   // 5 seconds
            Android.App.PendingIntentFlags intentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                intentFlags |= Android.App.PendingIntentFlags.Immutable;
            }

            _alarmIntent = Android.App.PendingIntent.GetBroadcast(context, 0, actionIntent, intentFlags);
            if (_alarmIntent == null)
            {
#if DEBUG
                Log.Info(Tag, "ScheduleAlarm: No alarm intent");
#endif
                return false;
            }
            alarms.SetInexactRepeating(Android.App.AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + interval, interval, _alarmIntent);
            return true;
        }
        catch (Exception ex)
        {
            Log.Debug(Tag, "ScheduleAlarm: alarm exception: {0}", ex.Message);
            return false;
        }
    }

    public bool CancelAlarm(Context context)
    {
        try
        {
            if (_alarmIntent == null)
            {
#if DEBUG
                Log.Info(Tag, "CancelAlarm: No alarm intent");
#endif
                return false;
            }

            Android.App.AlarmManager alarms = context?.GetSystemService(Context.AlarmService) as Android.App.AlarmManager;
            if (alarms == null)
            {
#if DEBUG
                Log.Info(Tag, "CancelAlarm: No alarm manager");
#endif
                return false;
            }

            alarms.Cancel(_alarmIntent);
            _alarmIntent = null;
            return true;
        }
        catch (Exception ex)
        {
            Log.Debug(Tag, "CancelAlarm: CancelAlarm alarm exception: {0}", ex.Message);
            return false;
        }
    }
}
