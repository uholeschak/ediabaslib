using Android.Content;
using Android.OS;
using Android.Util;
using System;
using System.Globalization;

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
    public const string ActionStartService = ActivityCommon.AppNameSpace + ".ActionStartService";

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
        Log.Info(Tag, string.Format(CultureInfo.InvariantCulture, "Action received: {0}", intent.Action));
#endif
        switch (intent.Action)
        {
            case Intent.ActionBootCompleted:
            case Intent.ActionReboot:
            case Intent.ActionMyPackageReplaced:
            case Intent.ActionMyPackageSuspended:
            case ActionStartService:
                ActivityCommon.StartForegroundService(context);
                break;
        }
    }
}
