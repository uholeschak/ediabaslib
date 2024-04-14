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
