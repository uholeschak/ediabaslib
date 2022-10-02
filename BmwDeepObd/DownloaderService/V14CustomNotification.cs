using AndroidX.Core.App;
using Android.Content;
using Google.Android.Vending.Expansion.Downloader;

namespace BmwDeepObd
{
    /// <summary>
    /// The custom notification for API levels 14+.
    /// </summary>
    public class V14CustomNotification : CustomDownloadNotification.ICustomNotification
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="V14CustomNotification"/> class.
        /// </summary>
        public V14CustomNotification()
        {
            this.CurrentBytes = -1;
            this.TotalBytes = -1;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets CurrentBytes.
        /// </summary>
        public long CurrentBytes { private get; set; }

        /// <summary>
        /// Gets or sets Icon.
        /// </summary>
        public int Icon { private get; set; }

        /// <summary>
        /// Gets or sets PendingIntent.
        /// </summary>
        public Android.App.PendingIntent PendingIntent { private get; set; }

        /// <summary>
        /// Gets or sets Ticker.
        /// </summary>
        public string Ticker { private get; set; }

        /// <summary>
        /// Gets or sets TimeRemaining.
        /// </summary>
        public long TimeRemaining { private get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { private get; set; }

        /// <summary>
        /// Gets or sets TotalBytes.
        /// </summary>
        public long TotalBytes { private get; set; }

        /// <summary>
        /// Gets or sets  ongoing.
        /// </summary>
        public bool Ongoing { private get; set; }

        /// <summary>
        /// Gets or sets completed
        /// </summary>
        public bool Completed { private get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Update the notification.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The updated notification.
        /// </returns>
        public Android.App.Notification UpdateNotification(Context context)
        {
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CustomDownloadNotification.NotificationChannelIdLow);

            string contentText = Helpers.GetDownloadProgressString(this.CurrentBytes, this.TotalBytes);
            string contentInfo = context.GetString(Resource.String.time_remaining_notification, Helpers.GetTimeRemaining(this.TimeRemaining));
            builder.SetContentTitle(this.Title);
            if (this.TotalBytes > 0 && this.CurrentBytes >= 0)
            {
                if (Completed)
                {
                    builder.SetProgress((int)(this.TotalBytes >> 8), (int)(this.TotalBytes >> 8), false);
                }
                else
                {
                    builder.SetProgress((int)(this.TotalBytes >> 8), (int)(this.CurrentBytes >> 8), false);
                }
            }
            else
            {
                builder.SetProgress(0, 0, true);
            }

            builder.SetContentText(contentText);
            builder.SetContentInfo(contentInfo);

            builder.SetSmallIcon(this.Icon != 0 ? this.Icon : Android.Resource.Drawable.StatSysDownload);
            builder.SetOngoing(Ongoing);
            if (!Ongoing)
            {
                builder.SetAutoCancel(true);
            }
            builder.SetTicker(this.Ticker);
            builder.SetStyle(new NotificationCompat.BigTextStyle().
                BigText(contentInfo + "\r\n" + Ticker).
                SetBigContentTitle(contentText));

            builder.SetContentIntent(this.PendingIntent);
            builder.SetOnlyAlertOnce(true);
            builder.SetPriority(NotificationCompat.PriorityLow);
            builder.SetCategory(NotificationCompat.CategoryProgress);

            return builder.Build();
        }

        #endregion
    }
}