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
    })
]

public class ActionBroadcastReceiver : BroadcastReceiver
{
#if DEBUG
    private static readonly string Tag = typeof(ActionBroadcastReceiver).FullName;
#endif
    private const string ActionTimeElapsed = "ActionTimeElapsed";
    public const string ActionStartTimer = "ActionStartTimer";

    public override void OnReceive(Context context, Intent intent)
    {
        if (intent?.Action == null)
        {
            return;
        }
#if DEBUG
        Log.Info(Tag, string.Format("Action received: {0}", intent.Action));
#endif
        switch (intent.Action)
        {
            case Intent.ActionBootCompleted:
            case Intent.ActionReboot:
            case ActionStartTimer:
                Android.App.AlarmManager alarms = context?.GetSystemService(Context.AlarmService) as Android.App.AlarmManager;
                if (alarms == null)
                {
#if DEBUG
                    Log.Info(Tag, "No alarm manager");
#endif
                    return;
                }

                try
                {
                    Intent actionIntent = new Intent(context, typeof(ActionBroadcastReceiver));
                    actionIntent.SetAction(ActionTimeElapsed);

                    long interval = 1000 * 5;   // 5 seconds
                    Android.App.PendingIntentFlags intentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        intentFlags |= Android.App.PendingIntentFlags.Immutable;
                    }

                    Android.App.PendingIntent alarmIntent = Android.App.PendingIntent.GetBroadcast(context, 0, actionIntent, intentFlags);
                    if (alarmIntent == null)
                    {
#if DEBUG
                        Log.Info(Tag, "Boot: No alarm intent");
#endif
                        break;
                    }
                    alarms.SetInexactRepeating(Android.App.AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + interval, interval, alarmIntent);
                }
                catch (Exception ex)
                {
                    Log.Debug(Tag, "Boot: ScheduleAlarm alarm exception: {0}", ex.Message);
                }
                break;

            case ActionTimeElapsed:
#if DEBUG
                Log.Info(Tag, "Alarm time elapsed");
#endif
                break;
        }
    }
}
