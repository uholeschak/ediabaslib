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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

using Java.Lang;
using Google.Android.Vending.Expansion.Downloader;

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
        private static readonly int NotificationId = typeof(CustomDownloadNotification).GetHashCode();
        public const string NotificationChannelIdLow = "DownloaderNotificationChannelLow";

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
        private readonly NotificationManager notificationManager;

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
        private Notification currentNotification;

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
            this.notificationManager = this.context.GetSystemService(Context.NotificationService) as NotificationManager;
            RegisterNotificationChannels();
            this.customNotification = CustomNotificationFactory.CreateCustomNotification();
        }

        #endregion

        private bool RegisterNotificationChannels()
        {
            try
            {
                if (this.notificationManager == null)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationChannel notificationChannelDefault = new NotificationChannel(
                        NotificationChannelIdLow, this.context.Resources.GetString(Resource.String.app_name), NotificationImportance.Low);
                    this.notificationManager.CreateNotificationChannel(notificationChannelDefault);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool UnregisterNotificationChannels()
        {
            try
            {
                if (this.notificationManager == null)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    this.notificationManager.DeleteNotificationChannel(NotificationChannelIdLow);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
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
            PendingIntent PendingIntent { set; }

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
            Notification UpdateNotification(Context context);

            #endregion
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets PendingIntent.
        /// </summary>
        public PendingIntent ClientIntent { get; set; }

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

            if (this.customNotification != null)
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

                if (customNotification != null)
                {
                    this.customNotification.Icon = iconResource;
                    this.customNotification.PendingIntent = this.ClientIntent;
                    this.customNotification.Ticker = this.label + ": " + this.currentText;
                    this.customNotification.Title = this.currentTitle;
                    this.currentNotification = this.customNotification.UpdateNotification(this.context);
                    if (ongoingEvent)
                    {
                        this.currentNotification.Flags |= NotificationFlags.OngoingEvent;
                    }
                    else
                    {
                        this.currentNotification.Flags &= ~NotificationFlags.OngoingEvent;
                        this.currentNotification.Flags |= NotificationFlags.AutoCancel;
                    }

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

        #endregion
    }
}