using System;
using Android.Content;
using Android.Support.V4.Content;
using Android.Util;

namespace BmwDeepObd
{
    [BroadcastReceiver(Enabled = true, Exported = true, Name = ActivityCommon.AppNameSpace + ".GlobalBroadcastReceiver")]
    [Android.App.IntentFilter(new[] {
        MtcBtSmallon,
        MtcBtSmalloff,
        MicBtReport
    }, Categories = new []{ Intent.CategoryDefault } )]
    public class GlobalBroadcastReceiver : BroadcastReceiver
    {
#if DEBUG
        static readonly string Tag = typeof(GlobalBroadcastReceiver).FullName;
#endif
        public const string MtcBtSmallon = @"com.microntek.bt.smallon";
        public const string MtcBtSmalloff = @"com.microntek.bt.smalloff";
        public const string MicBtReport = @"com.microntek.bt.report";
        public const string StateBtSmallOn = @"MicrontectBtSmallOn";
        public const string StateBtConnected = @"MicrontectBtConnected";
        public const string NotificationBroadcastAction = ActivityCommon.AppNameSpace + ".Notification.Action";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Action == null)
            {
                return;
            }
            switch (intent.Action)
            {
                case MtcBtSmallon:
                case MtcBtSmalloff:
                    try
                    {
                        bool smallOn = intent.Action == MtcBtSmallon;
#if DEBUG
                        Log.Info(Tag, string.Format("BT small on: {0}", smallOn));
#endif
                        Intent broadcastIntent = new Intent(NotificationBroadcastAction);
                        broadcastIntent.PutExtra(StateBtSmallOn, smallOn);
                        LocalBroadcastManager.GetInstance(context).SendBroadcast(broadcastIntent);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    break;

                case MicBtReport:
                    if (intent.HasExtra("connect_state"))
                    {
                        try
                        {
                            int connectState = intent.GetIntExtra("connect_state", 0);
#if DEBUG
                            Log.Info(Tag, string.Format("BT connect_state: {0}", connectState));
#endif
                            ActivityCommon.MtcBtConnectState = connectState != 0;

                            Intent broadcastIntent = new Intent(NotificationBroadcastAction);
                            broadcastIntent.PutExtra(StateBtConnected, ActivityCommon.MtcBtConnectState);
                            LocalBroadcastManager.GetInstance(context).SendBroadcast(broadcastIntent);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;
            }
        }
    }
}
