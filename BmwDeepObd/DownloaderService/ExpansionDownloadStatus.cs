// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpansionDownloadStatus.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   Lists the states that the download manager can set on a download to
//   notify applications of the download progress.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExpansionDownloader
{
    /// <summary>
    /// Lists the states that the download manager can set on a download to 
    /// notify applications of the download progress.
    /// </summary>
    /// <remarks>
    /// The codes follow the HTTP families:
    ///   1xx: informational
    ///   2xx: success
    ///   3xx: redirects (not used by the download manager)
    ///   4xx: client errors
    ///   5xx: server errors
    /// </remarks>
    public enum ExpansionDownloadStatus
    {
        /// <summary>
        /// Start of informational status codes
        /// </summary>
        InformationalMinimum = 100, 

        /// <summary>
        /// End of informational status codes
        /// </summary>
        InformationalMaximum = 199, 

        /// <summary>
        /// Start of succss status codes
        /// </summary>
        SuccessMinimum = 200, 

        /// <summary>
        /// End of succss status codes
        /// </summary>
        SuccessMaximum = 299, 

        /// <summary>
        /// Start of redirect status codes
        /// </summary>
        RedirectMinimum = 300, 

        /// <summary>
        /// End of redirect status codes
        /// </summary>
        RedirectMaximum = 399, 

        /// <summary>
        /// Start of client error status codes
        /// </summary>
        ClientErrorMinimum = 400, 

        /// <summary>
        /// Start of any error status codes
        /// </summary>
        AnyErrorMinimum = 400, 

        /// <summary>
        /// End of client error status codes
        /// </summary>
        ClientErrorMaximum = 499, 

        /// <summary>
        /// Start of server error status codes
        /// </summary>
        ServerErrorMinimum = 500, 

        /// <summary>
        /// End of server error status codes
        /// </summary>
        ServerErrorMaximum = 599, 

        /// <summary>
        /// End of any error status codes
        /// </summary>
        AnyErrorMaximum = 599, 

        /// <summary>
        /// This download hasn't stated yet.
        /// </summary>
        Pending = 190, 

        /// <summary>
        /// This download has started.
        /// </summary>
        Running = 192, 

        /// <summary>
        /// This download has been paused by the owning app.
        /// </summary>
        PausedByApp = 193, 

        /// <summary>
        /// This download encountered some network error and is waiting before 
        /// retrying the request.
        /// </summary>
        WaitingToRetry = 194, 

        /// <summary>
        /// This download is waiting for network connectivity to proceed.
        /// </summary>
        WaitingForNetwork = 195, 

        /// <summary>
        /// This download exceeded a size limit for mobile networks and is
        /// waiting for a Wi-Fi connection to proceed.
        /// </summary>
        QueuedForWifiOrCellularPermission = 196, 

        /// <summary>
        /// This download exceeded a size limit for mobile networks and is
        /// waiting for a Wi-Fi connection to proceed.
        /// </summary>
        QueuedForWifi = 197, 

        /// <summary>
        /// This download has successfully completed. Warning: there might be 
        /// other status values that indicate success in the future. 
        /// Use isSucccess() to capture the entire category.
        /// </summary>
        Success = 200, 

        /// <summary>
        /// The requested URL is no longer available.
        /// </summary>
        Forbidden = 403, 

        /// <summary>
        /// The file was delivered incorrectly.
        /// </summary>
        FileDeliveredIncorrectly = 487, 

        /// <summary>
        /// The requested destination file already exists.
        /// </summary>
        FileAlreadyExists = 488, 

        /// <summary>
        /// Some possibly transient error occurred, but we can't resume the 
        /// download.
        /// </summary>
        CannotResume = 489, 

        /// <summary>
        /// This download was canceled
        /// </summary>
        Canceled = 490, 

        /// <summary>
        /// This download has completed with an error. Warning: there will be 
        /// other status values that indicate errors in the future. 
        /// Use isStatusError() to capture the entire category.
        /// </summary>
        UnknownError = 491, 

        /// <summary>
        /// This download couldn't be completed because of a storage issue.
        /// Typically, that's because the filesystem is missing or full.
        /// Use the more specific {@link #InsufficientSpaceError} and
        /// {@link #DeviceNotFoundError} when appropriate.
        /// </summary>
        FileError = 492, 

        /// <summary>
        /// This download couldn't be completed because of an HTTP redirect 
        /// response that the download manager couldn't handle.
        /// </summary>
        UnhandledRedirect = 493, 

        /// <summary>
        /// This download couldn't be completed because of an unspecified 
        /// unhandled HTTP code.
        /// </summary>
        UnhandledHttpCode = 494, 

        /// <summary>
        /// This download couldn't be completed because of an error receiving 
        /// or processing data at the HTTP level.
        /// </summary>
        HttpDataError = 495, 

        /// <summary>
        /// This download couldn't be completed because of an HttpException 
        /// while setting up the request.
        /// </summary>
        HttpException = 496, 

        /// <summary>
        /// This download couldn't be completed because there were too many 
        /// redirects.
        /// </summary>
        TooManyRedirects = 497, 

        /// <summary>
        /// This download couldn't be completed due to insufficient storage 
        /// space. Typically, this is because the SD card is full.
        /// </summary>
        InsufficientSpaceError = 498, 

        /// <summary>
        /// This download couldn't be completed because no external storage 
        /// device was found. Typically, this is because the SD card is not 
        /// mounted.
        /// </summary>
        DeviceNotFoundError = 499, 

        /// <summary>
        /// This request couldn't be parsed. This is also used when processing 
        /// requests with unknown/unsupported URI schemes.
        /// </summary>
        BadRequest = 400, 

        /// <summary>
        /// This download can't be performed because the content type cannot 
        /// be handled.
        /// </summary>
        NotAcceptable = 406, 

        /// <summary>
        /// This download cannot be performed because the length cannot be
        /// determined accurately. 
        /// <br/>
        /// This is the code for the HTTP error "Length Required", which is 
        /// typically used when making requests that require a content length 
        /// but don't have one, and it is also used in the client when a 
        /// response is received whose length cannot be determined accurately 
        /// (thus making it impossible to know when a download completes).
        /// </summary>
        LengthRequired = 411, 

        /// <summary>
        /// This download was interrupted and cannot be resumed.
        /// <br/>
        /// This is the code for the HTTP error "Precondition Failed", and it 
        /// is also used in situations where the client doesn't have an ETag 
        /// at all.
        /// </summary>
        PreconditionFailed = 412, 

        /// <summary>
        /// The lowest-valued error status that is not an actual HTTP status 
        /// code.
        /// </summary>
        MinimumArtificialErrorStatus = 488, 

        /// <summary>
        /// The current status has not been set
        /// </summary>
        Unknown = -1, 

        /// <summary>
        /// The current download has finished correctly and is valid
        /// </summary>
        None = 0
    }
}