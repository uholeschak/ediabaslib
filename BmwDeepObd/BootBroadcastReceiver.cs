using Android.Content;
using Android.Nfc;
using Android.Webkit;
using EdiabasLib;
using System.Collections.Generic;

namespace BmwDeepObd;

[BroadcastReceiver(
    Exported = true,
    Enabled = true,
    Name = ActivityCommon.AppNameSpace + "." + nameof(BootBroadcastReceiver)
)]
[Android.App.IntentFilter(new[] { Intent.ActionBootCompleted })]

public class BootBroadcastReceiver : BroadcastReceiver
{
#if DEBUG
    private static readonly string Tag = typeof(GlobalBroadcastReceiver).FullName;
#endif

    public override void OnReceive(Context context, Intent intent)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, "Boot event received");
#endif
    }
}
