// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceMessages.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   The downloader service messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExpansionDownloader.Service
{
    /// <summary>
    /// The downloader service messages.
    /// </summary>
    public enum ServiceMessages
    {
        /// <summary>
        /// A request to abort a download.
        /// </summary>
        RequestAbortDownload = 1, 

        /// <summary>
        /// A request to pause a download.
        /// </summary>
        RequestPauseDownload = 2, 

        /// <summary>
        /// Update the download flags.
        /// </summary>
        SetDownloadFlags = 3, 

        /// <summary>
        /// A request to continue a download.
        /// </summary>
        RequestContinueDownload = 4, 

        /// <summary>
        /// A request for the download state.
        /// </summary>
        RequestDownloadState = 5, 

        /// <summary>
        /// A request to update the client.
        /// </summary>
        RequestClientUpdate = 6
    }
}