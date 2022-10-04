using Android.Content;
using Android.Content.PM;
using Android.Util;

namespace BmwDeepObd
{
    [BroadcastReceiver(
        Exported = false,
        Name = ActivityCommon.AppNameSpace + "." + nameof(ExpansionAlarmReceiver)
        )]
    public class ExpansionAlarmReceiver : BroadcastReceiver
    {
        private static readonly string Tag = typeof(ExpansionAlarmReceiver).FullName;

        /// <summary>
        /// This method is called when the BroadcastReceiver is receiving an Intent
        /// broadcast.
        /// </summary>
        /// <param name="context">
        /// The Context in which the receiver is running.
        /// </param>
        /// <param name="intent">
        /// The Intent being received.
        /// </param>
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                Log.Debug(Tag, "ExpansionAlarmReceiver.OnReceive");
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                ExpansionDownloaderService.StartDownloadServiceIfRequired(context, intent, typeof(ExpansionDownloaderService));
            }
            catch (PackageManager.NameNotFoundException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}
