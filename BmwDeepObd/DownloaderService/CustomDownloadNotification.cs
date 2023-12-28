// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadNotification.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   The download notification.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Android.Content;
using Android.OS;

using Java.Lang;
using Google.Android.Vending.Expansion.Downloader;
using AndroidX.Core.App;

namespace BmwDeepObd
{

    /// <summary>
    /// The download notification.
    /// </summary>
    public class CustomDownloadNotification : Object, IDownloaderClient
    {
        #region Static Fields

        /// <summary>
        /// The notification id.
        /// </summary>
        public static readonly int NotificationId = 10001;
        public const string NotificationChannelDownload = "DownloaderNotificationChannel";

        #endregion

        #region Fields

        /// <summary>
        /// The m context.
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// The m label.
        /// </summary>
        private readonly string label;

        /// <summary>
        /// The m custom notification.
        /// </summary>
        private readonly ICustomNotification customNotification;

        /// <summary>
        /// The m notification manager.
        /// </summary>
        private readonly NotificationManagerCompat notificationManager;

        /// <summary>
        /// The m client proxy.
        /// </summary>
        private IDownloaderClient clientProxy;

        /// <summary>
        /// The m state.
        /// </summary>
        private DownloaderClientState clientState;

        /// <summary>
        /// The m current notification.
        /// </summary>
        private Android.App.Notification currentNotification;

        /// <summary>
        /// The m current text.
        /// </summary>
        private string currentText;

        /// <summary>
        /// The m current title.
        /// </summary>
        private string currentTitle;

        /// <summary>
        /// The m progress info.
        /// </summary>
        private DownloadProgressInfo progressInfo;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDownloadNotification"/> class.
        /// </summary>
        /// <param name="ctx">
        /// The ctx.
        /// </param>
        /// <param name="applicationLabel">
        /// The application label.
        /// </param>
        internal CustomDownloadNotification(Context ctx, string applicationLabel)
        {
            this.clientState = (DownloaderClientState)(-1);
            this.context = ctx;
            this.label = applicationLabel;
            this.notificationManager = NotificationManagerCompat.From(this.context);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                this.customNotification = new V14CustomNotification();
            }
        }

        #endregion

        private bool NotificationsEnabled()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    if (this.notificationManager == null)
                    {
                        return false;
                    }
                    return this.notificationManager.AreNotificationsEnabled();
                }

                return true;
            }
            catch (System.Exception)
            {
                return true;
            }
        }

        #region Interfaces

        /// <summary>
        /// The custom notification.
        /// </summary>
        public interface ICustomNotification
        {
            #region Public Properties

            /// <summary>
            /// Sets CurrentBytes.
            /// </summary>
            long CurrentBytes { set; }

            /// <summary>
            /// Sets Icon.
            /// </summary>
            int Icon { set; }

            /// <summary>
            /// Sets PendingIntent.
            /// </summary>
            Android.App.PendingIntent PendingIntent { set; }

            /// <summary>
            /// Sets Ticker.
            /// </summary>
            string Ticker { set; }

            /// <summary>
            /// Sets TimeRemaining.
            /// </summary>
            long TimeRemaining { set; }

            /// <summary>
            /// Sets Title.
            /// </summary>
            string Title { set; }

            /// <summary>
            /// Sets TotalBytes.
            /// </summary>
            long TotalBytes { set; }

            /// <summary>
            /// Gets or sets  ongoing.
            /// </summary>
            bool Ongoing { set; }

            /// <summary>
            /// Gets or sets completed
            /// </summary>
            bool Completed { set; }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="context">
            /// The context to use to obtain access to the Notification Service
            /// </param>
            /// <returns>
            /// The Android.App.Notification.
            /// </returns>
            Android.App.Notification UpdateNotification(Context context);

            #endregion
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets PendingIntent.
        /// </summary>
        public Android.App.PendingIntent ClientIntent { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on download progress.
        /// </summary>
        /// <param name="progress">
        /// The progress.
        /// </param>
        public void OnDownloadProgress(DownloadProgressInfo progress)
        {
            this.progressInfo = progress;
            if (null != this.clientProxy)
            {
                this.clientProxy.OnDownloadProgress(progress);
            }

            if (this.customNotification != null && NotificationsEnabled())
            {
                this.customNotification.CurrentBytes = progress.OverallProgress;
                this.customNotification.TotalBytes = progress.OverallTotal;
                this.customNotification.Icon = Android.Resource.Drawable.StatSysDownload;
                this.customNotification.PendingIntent = this.ClientIntent;
                if (progress.OverallTotal <= 0)
                {
                    this.customNotification.Ticker = this.currentTitle;
                }
                else
                {
                    this.customNotification.Ticker = string.Format("{0}: {1}", this.label, this.currentText);
                }
                this.customNotification.Title = this.label;
                this.customNotification.TimeRemaining = progress.TimeRemaining;
                this.customNotification.Ongoing = true;
                this.customNotification.Completed = false;
                this.currentNotification = this.customNotification.UpdateNotification(this.context);
                this.notificationManager.Notify(NotificationId, this.currentNotification);
            }
        }

        /// <summary>
        /// The on download state changed.
        /// </summary>
        /// <param name="newState">
        /// The new state.
        /// </param>
        public void OnDownloadStateChanged(DownloaderClientState newState)
        {
            if (null != this.clientProxy)
            {
                this.clientProxy.OnDownloadStateChanged(newState);
            }

            if (newState != this.clientState)
            {
                this.clientState = newState;
                if (newState == DownloaderClientState.Idle || null == this.ClientIntent)
                {
                    return;
                }

                int stringDownload;
                int iconResource;
                bool ongoingEvent;

                // get the new title string and paused text
                switch (newState)
                {
                    case DownloaderClientState.Downloading:
                        iconResource = Android.Resource.Drawable.StatSysDownload;
                        stringDownload = Helpers.GetDownloaderStringResourceIdFromState(newState);
                        ongoingEvent = true;
                        break;

                    case DownloaderClientState.FetchingUrl:
                    case DownloaderClientState.Connecting:
                        iconResource = Android.Resource.Drawable.StatSysDownloadDone;
                        stringDownload = Helpers.GetDownloaderStringResourceIdFromState(newState);
                        ongoingEvent = true;
                        break;

                    case DownloaderClientState.Completed:
                    case DownloaderClientState.PausedByRequest:
                        iconResource = Android.Resource.Drawable.StatSysDownloadDone;
                        stringDownload = Helpers.GetDownloaderStringResourceIdFromState(newState);
                        ongoingEvent = false;
                        break;

                    case DownloaderClientState.Failed:
                    case DownloaderClientState.FailedCanceled:
                    case DownloaderClientState.FailedFetchingUrl:
                    case DownloaderClientState.FailedSdCardFull:
                    case DownloaderClientState.FailedUnlicensed:
                        iconResource = Android.Resource.Drawable.StatSysWarning;
                        stringDownload = Helpers.GetDownloaderStringResourceIdFromState(newState);
                        ongoingEvent = false;
                        break;

                    default:
                        iconResource = Android.Resource.Drawable.StatSysWarning;
                        stringDownload = Helpers.GetDownloaderStringResourceIdFromState(newState);
                        ongoingEvent = true;
                        break;
                }

                this.currentText = context.GetString(stringDownload);
                this.currentTitle = this.label;

                if (customNotification != null && NotificationsEnabled())
                {
                    this.customNotification.Icon = iconResource;
                    this.customNotification.PendingIntent = this.ClientIntent;
                    this.customNotification.Ticker = this.label + ": " + this.currentText;
                    this.customNotification.Title = this.currentTitle;
                    this.customNotification.Ongoing = ongoingEvent;
                    this.customNotification.Completed = newState == DownloaderClientState.Completed;
                    this.currentNotification = this.customNotification.UpdateNotification(this.context);
                    this.notificationManager.Notify(NotificationId, this.currentNotification);
                }
            }
        }

        /// <summary>
        /// The on service connected.
        /// </summary>
        /// <param name="m">
        /// The m.
        /// </param>
        public void OnServiceConnected(Messenger m)
        {
        }

        /// <summary>
        /// The resend state.
        /// </summary>
        public void ResendState()
        {
            if (null != this.clientProxy)
            {
                this.clientProxy.OnDownloadStateChanged(this.clientState);
            }
        }

        /// <summary>
        /// Called in response to OnClientUpdated. Creates a new proxy and 
        /// notifies it of the current state.
        /// </summary>
        /// <param name="msg">
        /// the client Messenger to notify
        /// </param>
        public void SetMessenger(Messenger msg)
        {
            this.clientProxy = DownloaderClientMarshaller.CreateProxy(msg);
            if (null != this.progressInfo)
            {
                this.clientProxy.OnDownloadProgress(this.progressInfo);
            }

            if (this.clientState != (DownloaderClientState) (-1))
            {
                this.clientProxy.OnDownloadStateChanged(this.clientState);
            }
        }

        public static bool RegisterNotificationChannels(Context context)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
                    UnregisterNotificationChannels(context);

                    Android.App.NotificationChannel notificationChannelDownload =
                        new Android.App.NotificationChannel(NotificationChannelDownload, context.Resources.GetString(Resource.String.notification_download), Android.App.NotificationImportance.Low);
                    notificationManager.CreateNotificationChannel(notificationChannelDownload);
                }

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool UnregisterNotificationChannels(Context context, bool unregisterAll = false)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
                    if (unregisterAll)
                    {
                        notificationManager.DeleteNotificationChannel(CustomDownloadNotification.NotificationChannelDownload);
                    }

                    notificationManager.DeleteNotificationChannel("DownloaderNotificationChannelLow");
                    notificationManager.DeleteNotificationChannel("DownloaderNotificationChannelDefault");
                }

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        #endregion
    }
}