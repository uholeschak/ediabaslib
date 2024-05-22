﻿using Android.Content;
using Android.Util;
using System.Globalization;

namespace BmwDeepObd;

[BroadcastReceiver(
    Exported = true,
    Enabled = true,
    Name = ActivityCommon.AppNameSpace + "." + nameof(ActionBroadcastReceiver)
)]
[Android.App.IntentFilter(new[]
    {
        Intent.ActionBootCompleted,
        Intent.ActionReboot,
        Intent.ActionShutdown,
        Intent.ActionMyPackageReplaced,
        Intent.ActionMyPackageUnsuspended,
        AndroidActionQuickBoot,
        HtcActionQuickBoot,
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
    public const string AndroidActionQuickBoot = "android.intent.action.QUICKBOOT_POWERON";
    public const string HtcActionQuickBoot = "com.htc.intent.action.QUICKBOOT_POWERON";
    public const string ActionStartService = ActivityCommon.AppNameSpace + ".ActionStartService";
    // testing broadcast, but Exported must be set to true in the manifest before it can be used
    // adb shell am broadcast -a android.intent.action.QUICKBOOT_POWERON -p de.holeschak.bmw_deep_obd

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
            case Intent.ActionMyPackageUnsuspended:
            case AndroidActionQuickBoot:
            case HtcActionQuickBoot:
            case ActionStartService:
            {
                ActivityCommon.AutoConnectType autoConnectType = ActivityCommon.GetAutoConnectSetting();
#if DEBUG
                Log.Info(Tag, string.Format(CultureInfo.InvariantCulture, "Auto start type: {0}", autoConnectType.ToString()));
#endif
                if (autoConnectType == ActivityCommon.AutoConnectType.StartBoot)
                {
#if DEBUG
                    Log.Info(Tag, "Starting service");
#endif
                    ActivityCommon.StartForegroundService(context, true);
                }
                break;
            }

            case Intent.ActionShutdown:
            {
#if DEBUG
                Log.Info(Tag, "Shutting down service");
#endif
                ActivityCommon.StopForegroundService(context);
                break;
            }
        }
    }
}
