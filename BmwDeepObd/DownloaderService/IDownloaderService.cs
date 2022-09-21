// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDownloaderService.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   Represents a service that will perform downloads.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Google.Android.Vending.Expansion.Downloader;

namespace ExpansionDownloader.Service
{
    using Android.OS;

    /// <summary>
    /// Represents a service that will perform downloads.
    /// </summary>
    public interface IDownloaderService
    {
        #region Public Methods and Operators

        /// <summary>
        /// Call this when you get <see cref="IDownloaderClient.OnServiceConnected"/> 
        /// from the downloader client to register it with the service. 
        /// It will automatically send the current status to the client.
        /// </summary>
        /// <param name="clientMessenger">
        /// The client Messenger.
        /// </param>
        void OnClientUpdated(Messenger clientMessenger);

        /// <summary>
        /// Request that the service abort the current download. The service 
        /// should respond by changing the state to 
        /// <see cref="DownloaderState.FailedCanceled"/>.
        /// </summary>
        void RequestAbortDownload();

        /// <summary>
        /// Request that the service continue a paused download, when in any
        /// paused or failed state, including
        /// <see cref="DownloaderState.PausedByRequest"/>.
        /// </summary>
        void RequestContinueDownload();

        /// <summary>
        /// Requests that the download status be sent to the client.
        /// </summary>
        void RequestDownloadStatus();

        /// <summary>
        /// Request that the service pause the current download. The service
        /// should respond by changing the state to 
        /// <see cref="DownloaderState.PausedByRequest"/>.
        /// </summary>
        void RequestPauseDownload();

        /// <summary>
        /// Set the flags for this download (e.g. 
        /// <see cref="ServiceFlags.FlagsDownloadOverCellular"/>).
        /// </summary>
        /// <param name="flags">
        /// The new flags to use.
        /// </param>
        void SetDownloadFlags(DownloaderServiceFlags flags);

        #endregion
    }
}