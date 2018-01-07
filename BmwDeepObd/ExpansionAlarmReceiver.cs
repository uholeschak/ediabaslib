using Android.Content;
using Android.Content.PM;
using Google.Android.Vending.Expansion.Downloader;

namespace BmwDeepObd
{
    [BroadcastReceiver(Exported = false)]
    public class ExpansionAlarmReceiver : BroadcastReceiver
    {
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
                DownloaderClientMarshaller.StartDownloadServiceIfRequired(
                    context, intent, typeof(ExpansionDownloaderService));
            }
            catch (PackageManager.NameNotFoundException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}
