using Android.Content;
using Android.Nfc;
using Android.Util;

namespace BmwDeepObd;

[BroadcastReceiver(
    Exported = false,
    Enabled = true,
    Name = ActivityCommon.AppNameSpace + "." + nameof(BackgroundAlarmReceiver)
)]
public class BackgroundAlarmReceiver : BroadcastReceiver
{
#if DEBUG
    private static readonly string Tag = typeof(BackgroundAlarmReceiver).FullName;
#endif
    public const string ActionTimeElapsed = "ActionTimeElapsed";

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
            case ActionTimeElapsed:
#if DEBUG
                Log.Info(Tag, "Alarm time elapsed");
#endif
                break;
        }
    }
}
