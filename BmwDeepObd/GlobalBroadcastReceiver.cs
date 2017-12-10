using System;
using Android.Content;
using Android.Support.V4.Content;
using Android.Util;

namespace BmwDeepObd
{
    [BroadcastReceiver(Enabled = true)]
    [Android.App.IntentFilter(new[] {
        MicrontekBtSmallon,
        MicrontekBtSmalloff,
        MicrontekBtReport
    }, Categories = new []{ Intent.CategoryDefault } )]
    public class GlobalBroadcastReceiver : BroadcastReceiver
    {
#if DEBUG
        static readonly string Tag = typeof(GlobalBroadcastReceiver).FullName;
#endif
        public const string MicrontekBtSmallon = @"com.microntek.bt.smallon";
        public const string MicrontekBtSmalloff = @"com.microntek.bt.smalloff";
        public const string MicrontekBtReport = @"com.microntek.bt.report";
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
                case MicrontekBtSmallon:
                case MicrontekBtSmalloff:
                    try
                    {
                        bool smallOn = intent.Action == MicrontekBtSmallon;
#if DEBUG
                        Log.Info(Tag, string.Format("BT small on: {0}", smallOn));
#endif
                        ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(ActivityCommon.AppNameSpace, FileCreationMode.Private);
                        ISharedPreferencesEditor prefsEdit = prefs.Edit();
                        prefsEdit.PutBoolean(StateBtSmallOn, smallOn);
                        prefsEdit.Commit();

                        Intent broadcastIntent = new Intent(NotificationBroadcastAction);
                        broadcastIntent.PutExtra(StateBtSmallOn, smallOn);
                        LocalBroadcastManager.GetInstance(context).SendBroadcast(broadcastIntent);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    break;

                case MicrontekBtReport:
                    if (intent.HasExtra("connect_state"))
                    {
                        try
                        {
                            int connectState = intent.GetIntExtra("connect_state", 0);
#if DEBUG
                            Log.Info(Tag, string.Format("BT connect_state: {0}", connectState));
#endif
                            bool connected = connectState != 0;
                            ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(ActivityCommon.AppNameSpace, FileCreationMode.Private);
                            ISharedPreferencesEditor prefsEdit = prefs.Edit();
                            prefsEdit.PutBoolean(StateBtConnected, connected);
                            prefsEdit.Commit();

                            Intent broadcastIntent = new Intent(NotificationBroadcastAction);
                            broadcastIntent.PutExtra(StateBtConnected, connected);
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
