using Android.Content;
using Android.Nfc;
using Android.Webkit;
using EdiabasLib;
using System.Collections.Generic;

namespace BmwDeepObd;

[BroadcastReceiver(
    Exported = true,
    Enabled = true,
    Name = ActivityCommon.AppNameSpace + "." + nameof(ActionBroadcastReceiver)
)]
[Android.App.IntentFilter(new[]
    {
        Intent.ActionBootCompleted,
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

    public override void OnReceive(Context context, Intent intent)
    {
        if (intent?.Action == null)
        {
            return;
        }
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("Action received: {0}", intent.Action));
#endif
    }
}
