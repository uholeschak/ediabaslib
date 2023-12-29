// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadThread.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   The download thread.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;

using Android.Content;
using Android.Util;
using Google.Android.Vending.Expansion.Downloader;
using Google.Android.Vending.Licensing;

namespace BmwDeepObd
{
    /// <summary>
    /// The download thread.
    /// </summary>
    public class CustomDownloadThread : Java.Lang.Object, Java.Lang.IRunnable
    {
        private static readonly string Tag = typeof(CustomDownloadThread).FullName;

        #region Fields

        /// <summary>
        /// The context.
        /// </summary>
        private readonly CustomDownloaderService context;

        /// <summary>
        /// The download info.
        /// </summary>
        private readonly DownloadInfo downloadInfo;

        /// <summary>
        /// The download notification.
        /// </summary>
        private readonly CustomDownloadNotification downloadNotification;

        /// <summary>
        /// The downloader service.
        /// </summary>
        private readonly CustomDownloaderService downloaderService;

        /// <summary>
        /// The downloader service.
        /// </summary>
        private readonly DownloadsDB db;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadThread"/> class.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="notification">
        /// The notification.
        /// </param>
        public CustomDownloadThread(DownloadInfo info, CustomDownloaderService service, CustomDownloadNotification notification)
        {
            this.context = service;
            this.downloadInfo = info;
            this.downloaderService = service;
            this.downloadNotification = notification;
            this.db = DownloadsDB.GetDB(service);
            this.UserAgent = string.Format("APKXDL (Linux; U; Android {0};{1}; {2}/{3}){4}",
                                           Android.OS.Build.VERSION.Release,
                                           System.Threading.Thread.CurrentThread.CurrentCulture.Name,
                                           Android.OS.Build.Device,
                                           Android.OS.Build.Id,
                                           this.downloaderService.PackageName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the download in a separate thread
        /// </summary>
        public void Run()
        {
            Android.OS.Process.SetThreadPriority(Android.OS.ThreadPriority.Background);

            State state = new State(this.downloadInfo, this.downloaderService);
            Android.OS.PowerManager.WakeLock wakeLock = null;
            DownloaderServiceStatus finalStatus = DownloaderServiceStatus.UnknownError;

            try
            {
                Android.OS.PowerManager pm = this.context.GetSystemService(Context.PowerService) as Android.OS.PowerManager;
                wakeLock = pm?.NewWakeLock(Android.OS.WakeLockFlags.Partial, this.GetType().Name);
                wakeLock?.Acquire();

                bool finished = false;
                do
                {
                    Log.Debug(Tag, string.Format("DownloadThread: initiating download for: {0} at: {1}", this.downloadInfo.FileName, this.downloadInfo.Uri));
                    Uri requestUri = new Uri(state.RequestUri);
                    int minute = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

#pragma warning disable SYSLIB0014
                    HttpWebRequest request = WebRequest.CreateHttp(requestUri);
#pragma warning restore SYSLIB0014
                    request.Proxy = WebRequest.DefaultWebProxy;
                    request.UserAgent = this.UserAgent;
                    request.Timeout = minute;
                    request.ReadWriteTimeout = minute;
                    request.AllowAutoRedirect = false;

                    try
                    {
                        this.ExecuteDownload(state, request);
                        finished = true;
                    }
                    catch (RetryDownloadException)
                    {
                        // fall through
                    }
                    catch (Exception ex)
                    {
                        Log.Error(Tag, string.Format("DownloadThread: Exception: {0}", ex.Message));
                        throw;
                    }
                    finally
                    {
                        request.Abort();
                    }
                    Log.Debug(Tag, string.Format("DownloadThread: download for: {0} finished: {1}", this.downloadInfo.FileName, finished));
                }
                while (!finished);

                Log.Debug(Tag, string.Format("DownloadThread : download completed for: {0} at: {1}", this.downloadInfo.FileName, this.downloadInfo.Uri));

                this.FinalizeDestinationFile(state);
                finalStatus = DownloaderServiceStatus.Success;
            }
            catch (StopRequestException error)
            {
                // remove the cause before printing, in case it contains PII

                Log.Error(Tag, string.Format("DownloadThread: Aborting request for download {0}: {1}", this.downloadInfo.FileName, error.Message));
                finalStatus = error.FinalStatus;
            }
            catch (Exception ex)
            {
                // sometimes the socket code throws unchecked exceptions
                Log.Error(Tag, string.Format("DownloadThread: Exception for {0}: {1}", this.downloadInfo.FileName, ex.Message));
                finalStatus = DownloaderServiceStatus.UnknownError;
                if (ex is WebException webException)
                {
                    if (webException.Status == WebExceptionStatus.ProtocolError)
                    {   // force LVL update
                        finalStatus = DownloaderServiceStatus.Forbidden;
                    }
                }
            }
            finally
            {
                if (wakeLock != null)
                {
                    wakeLock.Release();
                }

                Log.Info(Tag, string.Format("DownloadThread: Finished for {0}", this.downloadInfo.FileName));
                CleanupDestination(state, finalStatus);
                this.NotifyDownloadCompleted(finalStatus, state.CountRetry, state.RetryAfter, state.RedirectCount, state.GotData);
            }
        }

        /// <summary>
        /// Add headers for this download to the HTTP request to allow for resume.
        /// </summary>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        private static void AddRequestHeaders(InnerState innerState, HttpWebRequest request)
        {
            if (innerState.ContinuingDownload)
            {
                if (innerState.HeaderETag != null)
                {
                    request.Headers.Add("If-Match", innerState.HeaderETag);
                }

                request.AddRange(innerState.BytesSoFar);
            }

            // request.SendChunked = true;
        }

        /// <summary>
        /// The cannot resume.
        /// </summary>
        /// <param name="innerState">
        /// The inner state.
        /// </param>
        /// <returns>
        /// True if the download cannot resume, otherwise false
        /// </returns>
        private static bool CannotResume(InnerState innerState)
        {
            return innerState.BytesSoFar > 0 && innerState.HeaderETag == null;
        }

        /// <summary>
        /// Called just before the thread finishes, regardless of status, to take any
        /// necessary action on the downloaded file.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="finalStatus">
        /// The final Status.
        /// </param>
        private static void CleanupDestination(State state, DownloaderServiceStatus finalStatus)
        {
            CloseDestination(state);
            if (state.Filename != null && finalStatus.IsError() && File.Exists(state.Filename))
            {
                File.Delete(state.Filename);
                state.Filename = null;
            }
        }

        /// <summary>
        /// Close the destination output stream.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private static void CloseDestination(State state)
        {
            try
            {
                // close the file
                if (state.Stream != null)
                {
                    state.Stream.Close();
                    state.Stream = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(Tag, string.Format("DownloadThread : exception when closing the file after download: {0}", ex.Message));
                // nothing can really be done if the file can't be closed
            }
        }

        /// <summary>
        /// Handle a status that we don't know how to deal with properly.
        /// </summary>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        /// <param name="statusCode">
        /// The status Code.
        /// </param>
        private static void HandleOtherStatus(InnerState innerState, HttpStatusCode statusCode)
        {
            DownloaderServiceStatus finalStatus;
            DownloaderServiceStatus downloadStatus = (DownloaderServiceStatus)statusCode;

            if (downloadStatus.IsError())
            {
                finalStatus = downloadStatus;
            }
            else if (downloadStatus.IsRedirect())
            {
                finalStatus = DownloaderServiceStatus.UnhandledRedirect;
            }
            else if (innerState.ContinuingDownload && downloadStatus == DownloaderServiceStatus.Success)
            {
                finalStatus = DownloaderServiceStatus.CannotResume;
            }
            else
            {
                finalStatus = DownloaderServiceStatus.UnhandledHttpCode;
            }

            throw new StopRequestException(finalStatus, "http error " + statusCode);
        }

        /// <summary>
        /// Handle a 503 Service Unavailable status by processing the Retry-After header.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private static void HandleServiceUnavailable(State state, HttpWebResponse response)
        {
            state.CountRetry = true;
            string header = response.GetResponseHeader("Retry-After");

            if (!string.IsNullOrEmpty(header))
            {
                Log.Info(Tag, string.Format("DownloadThread : got HTTP response code 503 (retry after {0})", header));

                if (!int.TryParse(header, out int retryAfter))
                {
                    retryAfter = 0;
                }

                state.RetryAfter = Math.Max(retryAfter, 0);
                if (state.RetryAfter < CustomDownloaderService.MinimumRetryAfter)
                {
                    state.RetryAfter = CustomDownloaderService.MinimumRetryAfter;
                }
                else if (state.RetryAfter > CustomDownloaderService.MaxRetryAfter)
                {
                    state.RetryAfter = CustomDownloaderService.MaxRetryAfter;
                }

                state.RetryAfter += Helpers.Random.NextInt(CustomDownloaderService.MinimumRetryAfter + 1);
                state.RetryAfter *= 1000;
            }

            throw new StopRequestException(DownloaderServiceStatus.WaitingToRetry, "got 503 Service Unavailable, will retry later");
        }

        /// <summary>
        /// Sync the destination file to storage.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private static void SyncDestination(State state)
        {
            try
            {
                using (var downloadedFileStream = new FileStream(state.Filename, FileMode.Append))
                {
                    downloadedFileStream.Flush();
                    downloadedFileStream.Close();
                }
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(Tag, string.Format("LVLDL file {0} not found: {1}", state.Filename, ex.Message));
            }
            catch (IOException ex)
            {
                Log.Error(Tag, string.Format("LVLDL IOException trying to sync {0}: {1}", state.Filename, ex.Message));
            }
            catch (Exception ex)
            {
                Log.Error(Tag, string.Format("LVLDL exception while syncing file: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Returns the default user agent
        /// </summary>
        /// <returns>
        /// The user agent.
        /// </returns>
        private string UserAgent { get; set; }

        /// <summary>
        /// Write a data buffer to the destination file.
        /// </summary>
        /// <param name="state">
        /// </param>
        /// <param name="data">
        /// data buffer containing the data to write
        /// </param>
        /// <param name="bytesRead">
        /// bytesRead how many bytes to write from the buffer
        /// </param>
        private static void WriteDataToDestination(State state, byte[] data, int bytesRead)
        {
            bool finished = false;

            while (!finished)
            {
                try
                {
                    if (state.Stream == null)
                    {
                        state.Stream = new FileStream(state.Filename, FileMode.Append);
                    }

                    state.Stream.Write(data, 0, bytesRead);

                    // we close after every write - todo: this may be too inefficient
                    CloseDestination(state);

                    finished = true;
                }
                catch (IOException ex)
                {
                    if (!Helpers.IsExternalMediaMounted)
                    {
                        throw new StopRequestException(DownloaderServiceStatus.DeviceNotFound, "external media not mounted while writing destination file");
                    }

                    long availableBytes = Helpers.GetAvailableBytes(Helpers.GetFilesystemRoot(state.Filename));

                    if (availableBytes < bytesRead)
                    {
                        throw new StopRequestException(DownloaderServiceStatus.InsufficientSpace, "insufficient space while writing destination file", ex);
                    }

                    throw new StopRequestException(DownloaderServiceStatus.FileError, "while writing destination file: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Check if current connectivity is valid for this request.
        /// </summary>
        private void CheckConnectivity()
        {
            DownloaderServiceNetworkAvailability availabilityState = this.downloaderService.GetNetworkAvailabilityState(db);

            switch (availabilityState)
            {
                case DownloaderServiceNetworkAvailability.Ok:
                    return;
                case DownloaderServiceNetworkAvailability.NoConnection:
                    throw new StopRequestException(DownloaderServiceStatus.WaitingForNetwork, "waiting for network to return");
                case DownloaderServiceNetworkAvailability.TypeDisallowedByRequestor:
                    throw new StopRequestException(DownloaderServiceStatus.QueuedForWifiOrCellularPermission, "waiting for wifi or for download over cellular to be authorized");
                case DownloaderServiceNetworkAvailability.CannotUseRoaming:
                    throw new StopRequestException(DownloaderServiceStatus.WaitingForNetwork, "roaming is not allowed");
                case DownloaderServiceNetworkAvailability.UnusableDueToSize:
                    throw new StopRequestException(DownloaderServiceStatus.QueuedForWifi, "waiting for wifi");
            }
        }

        /// <summary>
        /// Check if the download has been paused or canceled, stopping the 
        /// request appropriately if it has been.
        /// </summary>
        private void CheckPausedOrCanceled()
        {
            if (this.downloaderService.Control == DownloaderServiceControlAction.Paused)
            {
                if (this.downloaderService.Status == DownloaderServiceStatus.PausedByApp)
                {
                    throw new StopRequestException(this.downloaderService.Status, "download paused");
                }
            }
        }

        /// <summary>
        /// Fully execute a single download request - setup and send the request,
        /// handle the response, and transfer the data to the destination file.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        private void ExecuteDownload(State state, HttpWebRequest request)
        {
            InnerState innerState = new InnerState();
            byte[] data = new byte[CustomDownloaderService.BufferSize];

            this.CheckPausedOrCanceled();

            this.SetupDestinationFile(state, innerState);
            AddRequestHeaders(innerState, request);

            // check just before sending the request to avoid using an invalid connection at all
            this.CheckConnectivity();

            this.downloadNotification?.OnDownloadStateChanged(DownloaderClientState.Connecting);
            HttpWebResponse response = this.SendRequest(state, request);
            this.HandleExceptionalStatus(state, innerState, response);

            Log.Info(Tag, string.Format("DownloadThread: received response for {0}", this.downloadInfo.Uri));

            this.ProcessResponseHeaders(state, innerState, response);
            Stream entityStream = this.OpenResponseEntity(state, response);
            this.downloadNotification?.OnDownloadStateChanged(DownloaderClientState.Downloading);
            this.TransferData(state, innerState, data, entityStream);
        }

        /// <summary>
        /// Called after a successful completion to take any necessary action on the downloaded file.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private void FinalizeDestinationFile(State state)
        {
            SyncDestination(state);
            string tempFilename = state.Filename;
            string finalFilename = Helpers.GenerateSaveFileName(this.downloaderService, this.downloadInfo.FileName);
            if (state.Filename != finalFilename)
            {
                var startFile = new FileInfo(tempFilename);
                if (this.downloadInfo.TotalBytes != -1 && this.downloadInfo.CurrentBytes == this.downloadInfo.TotalBytes)
                {
                    try
                    {
                        startFile.MoveTo(finalFilename);
                    }
                    catch (Exception)
                    {
                        throw new StopRequestException(DownloaderServiceStatus.FileError, "unable to finalize destination file");
                    }
                }
                else
                {
                    throw new StopRequestException(
                        DownloaderServiceStatus.FileDeliveredIncorrectly, 
                        "file delivered with incorrect size. probably due to network not browser configured");
                }
            }
        }

        /// <summary>
        /// The get final status for http error.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <returns>
        /// The ExpansionDownloader.DownloaderServiceStatus.
        /// </returns>
        private DownloaderServiceStatus GetFinalStatusForHttpError(State state)
        {
            if (this.downloaderService.GetNetworkAvailabilityState(db) != DownloaderServiceNetworkAvailability.Ok)
            {
                return DownloaderServiceStatus.WaitingForNetwork;
            }

            if (this.downloadInfo.FailedCount < CustomDownloaderService.MaximumRetries)
            {
                state.CountRetry = true;
                return DownloaderServiceStatus.WaitingToRetry;
            }

            Log.Error(Tag, string.Format("DownloadThread: reached max retries for {0}", this.downloadInfo.FailedCount));

            return DownloaderServiceStatus.HttpDataError;
        }

        /// <summary>
        /// Called when we've reached the end of the HTTP response stream, to update the database and
        /// check for consistency.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        private void HandleEndOfStream(State state, InnerState innerState)
        {
            Log.Info(Tag, string.Format("DownloadThread: HandleEndOfStream State={0}, InnerState={1}", state, innerState));

            this.downloadInfo.CurrentBytes = innerState.BytesSoFar;

            //// this should always be set from the market
            // if (innerState.HeaderContentLength == null)
            // {
            // downloadInfo.TotalBytes = innerState.BytesSoFar;
            // }
            db.UpdateDownload(this.downloadInfo);

            bool lengthMismatched = innerState.HeaderContentLength != null
                                    && innerState.BytesSoFar != int.Parse(innerState.HeaderContentLength);
            if (lengthMismatched)
            {
                string message;
                DownloaderServiceStatus finalStatus;
                if (CannotResume(innerState))
                {
                    finalStatus = DownloaderServiceStatus.CannotResume;
                    message = "mismatched content length";
                }
                else
                {
                    finalStatus = this.GetFinalStatusForHttpError(state);
                    message = "closed socket before end of file";
                }

                throw new StopRequestException(finalStatus, message);
            }
        }

        /// <summary>
        /// Check the HTTP response status and handle anything unusual (e.g. not 200/206).
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private void HandleExceptionalStatus(State state, InnerState innerState, HttpWebResponse response)
        {
            HttpStatusCode statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.ServiceUnavailable
                && this.downloadInfo.FailedCount < CustomDownloaderService.MaximumRetries)
            {
                HandleServiceUnavailable(state, response);
            }

            switch (statusCode)
            {
                case HttpStatusCode.TemporaryRedirect:
                case HttpStatusCode.SeeOther:
                case HttpStatusCode.Found:
                case HttpStatusCode.Moved:
                    this.HandleRedirect(state, response, statusCode);
                    break;
            }

            HttpStatusCode expectedStatus = innerState.ContinuingDownload
                                                ? HttpStatusCode.PartialContent
                                                : HttpStatusCode.OK;
            if (statusCode != expectedStatus)
            {
                HandleOtherStatus(innerState, statusCode);
            }
            else
            {
                // no longer redirected
                state.RedirectCount = 0;
            }
        }

        /// <summary>
        /// Handle a 3xx redirect status.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="statusCode">
        /// The status Code.
        /// </param>
        private void HandleRedirect(State state, HttpWebResponse response, HttpStatusCode statusCode)
        {
            Log.Info(Tag, string.Format("DownloadThread: got HTTP redirect {0}", statusCode));

            if (state.RedirectCount >= CustomDownloaderService.MaxRedirects)
            {
                throw new StopRequestException(DownloaderServiceStatus.TooManyRedirects, "too many redirects");
            }

            string header = response.GetResponseHeader("Location");
            if (string.IsNullOrEmpty(header))
            {
                return;
            }

            Log.Info(Tag, string.Format("DownloadThread: Redirecting to: {0}", header));

            string newUri;
            try
            {
                newUri = new Java.Net.URI(this.downloadInfo.Uri).Resolve(new Java.Net.URI(header))?.ToString();
            }
            catch (Java.Net.URISyntaxException)
            {
                Log.Error(Tag, string.Format("DownloadThread: Couldn't resolve redirect URI {0} for {1}", header, this.downloadInfo.Uri));
                throw new StopRequestException(DownloaderServiceStatus.HttpDataError, "Couldn't resolve redirect URI");
            }

            if (string.IsNullOrEmpty(newUri))
            {
                Log.Error(Tag, string.Format("DownloadThread: Empty redirect URI {0} for {1}", header, this.downloadInfo.Uri));
                throw new StopRequestException(DownloaderServiceStatus.HttpDataError, "Couldn't resolve redirect URI");
            }

            ++state.RedirectCount;
            state.RequestUri = newUri;
            if ((int)statusCode == 301 || (int)statusCode == 303)
            {
                // use the new URI for all future requests (should a retry/resume be necessary)
                state.NewUri = newUri;
            }

            throw new RetryDownloadException();
        }

        /// <summary>
        /// The log network state.
        /// </summary>
        private void LogNetworkState()
        {
            string network = this.downloaderService.GetNetworkAvailabilityState(db) == DownloaderServiceNetworkAvailability.Ok ? "Up" : "Down";
            Log.Info(Tag, string.Format("DownloadThread: Network is {0}.", network));
        }

        /// <summary>
        /// Stores information about the completed download, and notifies the initiating application.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="countRetry">
        /// The count Retry.
        /// </param>
        /// <param name="retryAfter">
        /// The retry After.
        /// </param>
        /// <param name="redirectCount">
        /// The redirect Count.
        /// </param>
        /// <param name="gotData">
        /// The got Data.
        /// </param>
        private void NotifyDownloadCompleted(DownloaderServiceStatus status, bool countRetry, int retryAfter, int redirectCount, bool gotData)
        {
            Log.Info(Tag, string.Format("DownloadThread: NotifyDownloadCompleted Status={0}", status));
            this.UpdateDownloadDatabase(status, countRetry, retryAfter, redirectCount, gotData);
            if (status.IsCompleted())
            {
                // TBD: send status update?
            }
        }

        /// <summary>
        /// Open a stream for the HTTP response entity, handling I/O errors.
        /// </summary>
        /// <param name="state">
        /// </param>
        /// <param name="response">
        /// </param>
        /// <returns>
        /// an InputStream to read the response entity
        /// </returns>
        private Stream OpenResponseEntity(State state, HttpWebResponse response)
        {
            try
            {
                return response.GetResponseStream();
            }
            catch (Exception ex)
            {
                this.LogNetworkState();
                throw new StopRequestException(this.GetFinalStatusForHttpError(state), string.Format("while getting entity: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Read HTTP response headers and take appropriate action, including setting up the destination
        /// file and updating the database.
        /// </summary>
        /// <param name="state">
        /// </param>
        /// <param name="innerState">
        /// </param>
        /// <param name="response">
        /// </param>
        private void ProcessResponseHeaders(State state, InnerState innerState, HttpWebResponse response)
        {
            if (!innerState.ContinuingDownload)
            {
                this.ReadResponseHeaders(innerState, response);

                try
                {
                    state.Filename = this.downloaderService.GenerateSaveFile(this.downloadInfo.FileName, this.downloadInfo.TotalBytes);
                }
                catch (DownloaderService.GenerateSaveFileError exc)
                {
                    throw new StopRequestException((DownloaderServiceStatus) CustomDownloaderService.GetSafeFileErrorStatus(exc),
                        CustomDownloaderService.GetSafeFileErrorMessage(exc));
                }

                try
                {
                    if (!File.Exists(state.Filename))
                    {
                        // make sure the directory exists
                        string path = Helpers.GetSaveFilePath(this.downloaderService);
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        state.Stream = new FileStream(state.Filename, FileMode.Create);
                    }
                    else
                    {
                        state.Stream = new FileStream(state.Filename, FileMode.Open);
                    }
                }
                catch (Exception ex)
                {
                    throw new StopRequestException(DownloaderServiceStatus.FileError, string.Format("while opening destination file: {0}", ex.Message), ex);
                }

                Log.Info(Tag, string.Format("DownloadThread: writing {0} to {1}", this.downloadInfo.Uri, state.Filename));

                this.UpdateDatabaseFromHeaders(innerState);

                // check connectivity again now that we know the total size
                this.CheckConnectivity();
            }
        }

        /// <summary>
        /// Read some data from the HTTP response stream, handling I/O errors.
        /// </summary>
        /// <param name="state">
        /// </param>
        /// <param name="innerState">
        /// </param>
        /// <param name="data">
        /// data buffer to use to read data
        /// </param>
        /// <param name="entityStream">
        /// entityStream stream for reading the HTTP response entity
        /// </param>
        /// <returns>
        /// the number of bytes actually read or 0 if the end of the stream has been reached
        /// </returns>
        private int ReadFromResponse(State state, InnerState innerState, byte[] data, Stream entityStream)
        {
            try
            {
                return entityStream.Read(data, 0, data.Length);
            }
            catch (IOException ex)
            {
                this.LogNetworkState();
                this.downloadInfo.CurrentBytes = innerState.BytesSoFar;
                db.UpdateDownload(this.downloadInfo);

                string message;
                DownloaderServiceStatus finalStatus;
                if (CannotResume(innerState))
                {
                    finalStatus = DownloaderServiceStatus.CannotResume;
                    message = string.Format("while reading response: {0}, can't resume interrupted download with no ETag", ex.Message);
                }
                else
                {
                    finalStatus = this.GetFinalStatusForHttpError(state);
                    message = string.Format("while reading response: {0}", ex.Message);
                }

                throw new StopRequestException(finalStatus, message, ex);
            }
        }

        /// <summary>
        /// Read headers from the HTTP response and store them into local state.
        /// </summary>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private void ReadResponseHeaders(InnerState innerState, HttpWebResponse response)
        {
            string header = response.GetResponseHeader("Content-Disposition");
            if (!string.IsNullOrEmpty(header))
            {
                innerState.HeaderContentDisposition = header;
            }

            header = response.GetResponseHeader("Content-Location");
            if (!string.IsNullOrEmpty(header))
            {
                innerState.HeaderContentLocation = header;
            }

            header = response.GetResponseHeader("ETag");
            if (!string.IsNullOrEmpty(header))
            {
                innerState.HeaderETag = header;
            }

            header = response.GetResponseHeader("Content-Type");
            if (!string.IsNullOrEmpty(header) && header != "application/vnd.android.obb")
            {
                throw new StopRequestException(DownloaderServiceStatus.FileDeliveredIncorrectly, "file delivered with incorrect Mime type");
            }

            string headerTransferEncoding = response.GetResponseHeader("Transfer-Encoding");

            // todo - there seems to be no similar thing in .NET
            // if (!string.IsNullOrEmpty(headerTransferEncoding))
            // {
            header = response.GetResponseHeader("Content-Length");
            if (!string.IsNullOrEmpty(header))
            {
                innerState.HeaderContentLength = header;
                // this is always set from Market
                if (!long.TryParse(innerState.HeaderContentLength, out long contentLength))
                {
                    contentLength = -1;
                }

                if (contentLength != -1 && contentLength != this.downloadInfo.TotalBytes)
                {
                    // we're most likely on a bad wifi connection -- we should probably
                    // also look at the mime type --- but the size mismatch is enough
                    // to tell us that something is wrong here
                    Log.Error(Tag, string.Format("DownloadThread: Incorrect file size delivered: Length={0} Total={1}", contentLength, this.downloadInfo.TotalBytes));
                }
            }

            // }
            // else
            // {
            // // Ignore content-length with transfer-encoding - 2616 4.4 3
            // System.Diagnostics.System.Diagnostics.Debug.WriteLine("DownloadThread : ignoring content-length because of xfer-encoding");
            // }
            Log.Info(Tag, string.Format("DownloadThread : Content-Length: {0}", innerState.HeaderContentLength));
            Log.Info(Tag, string.Format("DownloadThread : Content-Location: {0}", innerState.HeaderContentLocation));
            Log.Info(Tag, string.Format("DownloadThread: ETag: {0}", innerState.HeaderETag));
            Log.Info(Tag, string.Format("DownloadThread : Transfer-Encoding: {0}", headerTransferEncoding));

            bool noSizeInfo = string.IsNullOrEmpty(innerState.HeaderContentLength) &&
                              (string.IsNullOrEmpty(headerTransferEncoding) || !headerTransferEncoding.Equals("chunked", StringComparison.OrdinalIgnoreCase));
            if (noSizeInfo)
            {
                throw new StopRequestException(DownloaderServiceStatus.HttpDataError, "can't know size of download, giving up");
            }
        }

        /// <summary>
        /// Report download progress through the database if necessary.
        /// </summary>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        private void ReportProgress(InnerState innerState)
        {
            long now = PolicyExtensions.GetCurrentMilliseconds();
            if (innerState.BytesSoFar - innerState.BytesNotified > CustomDownloaderService.MinimumProgressStep
                && now - innerState.TimeLastNotification > CustomDownloaderService.MinimumProgressTime)
            {
                // we store progress updates to the database here
                this.downloadInfo.CurrentBytes = innerState.BytesSoFar;
                db.UpdateDownloadCurrentBytes(this.downloadInfo);

                innerState.BytesNotified = innerState.BytesSoFar;
                innerState.TimeLastNotification = now;

                long totalBytesSoFar = innerState.BytesThisSession + this.downloaderService.BytesSoFar;

                Log.Info(Tag, string.Format("DownloadThread: downloaded {0} out of {1}", this.downloadInfo.CurrentBytes, this.downloadInfo.TotalBytes));
                Log.Info(Tag, string.Format("DownloadThread:      total {0} out of {1}", totalBytesSoFar, this.downloaderService.TotalLength));

                this.downloaderService.NotifyUpdateBytes(totalBytesSoFar);
            }
        }

        /// <summary>
        /// Send the request to the server, handling any I/O exceptions.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The System.Net.HttpWebResponse.
        /// </returns>
        private HttpWebResponse SendRequest(State state, HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (Java.Lang.IllegalArgumentException ex)
            {
                throw new StopRequestException(
                    DownloaderServiceStatus.HttpDataError, "while trying to execute request: " + ex.Message, ex);
            }
            catch (IOException ex)
            {
                this.LogNetworkState();
                throw new StopRequestException(
                    this.GetFinalStatusForHttpError(state), "while trying to execute request: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Prepare the destination file to receive data. 
        /// If the file already exists, we'll set up appropriately for resumption.
        /// </summary>
        /// <param name="state">
        /// </param>
        /// <param name="innerState">
        /// </param>
        private void SetupDestinationFile(State state, InnerState innerState)
        {
            if (state.Filename != null)
            {
                // only true if we've already run a thread for this download
                if (!Helpers.IsFilenameValid(state.Filename))
                {
                    // this should never happen
                    throw new StopRequestException(DownloaderServiceStatus.FileError, "found invalid internal destination filename");
                }

                // We're resuming a download that got interrupted
                if (File.Exists(state.Filename))
                {
                    long fileLength = new FileInfo(state.Filename).Length;
                    if (fileLength == 0)
                    {
                        // The download hadn't actually started, we can restart from scratch
                        File.Delete(state.Filename);
                        state.Filename = null;
                    }
                    else if (this.downloadInfo.ETag == null)
                    {
                        // This should've been caught upon failure
                        File.Delete(state.Filename);
                        throw new StopRequestException(DownloaderServiceStatus.CannotResume, "Trying to resume a download that can't be resumed");
                    }
                    else
                    {
                        // All right, we'll be able to resume this download
                        try
                        {
                            state.Stream = new FileStream(state.Filename, FileMode.Append);
                        }
                        catch (FileNotFoundException exc)
                        {
                            throw new StopRequestException(DownloaderServiceStatus.FileError, "while opening destination for resuming: " + exc, exc);
                        }

                        innerState.BytesSoFar = (int)fileLength;
                        if (this.downloadInfo.TotalBytes != -1)
                        {
                            innerState.HeaderContentLength = this.downloadInfo.TotalBytes.ToString();
                        }

                        innerState.HeaderETag = this.downloadInfo.ETag;
                        innerState.ContinuingDownload = true;
                    }
                }
            }

            if (state.Stream != null)
            {
                CloseDestination(state);
            }
        }

        /// <summary>
        /// Transfer as much data as possible from the HTTP response to the destination file.
        /// </summary>
        /// <param name="state">
        /// </param>
        /// <param name="innerState">
        /// </param>
        /// <param name="data">
        /// buffer to use to read data
        /// </param>
        /// <param name="entityStream">
        /// stream for reading the HTTP response entity
        /// </param>
        private void TransferData(State state, InnerState innerState, byte[] data, Stream entityStream)
        {
            int bytesRead;

            do
            {
                bytesRead = this.ReadFromResponse(state, innerState, data, entityStream);

                if (bytesRead != 0)
                {
                    state.GotData = true;
                    WriteDataToDestination(state, data, bytesRead);
                    innerState.BytesSoFar += bytesRead;
                    innerState.BytesThisSession += bytesRead;
                    this.ReportProgress(innerState);

                    this.CheckPausedOrCanceled();
                }
            }
            while (bytesRead != 0);

            // success, end of stream already reached
            this.HandleEndOfStream(state, innerState);
        }

        /// <summary>
        /// Update necessary database fields based on values of HTTP response headers that have been read.
        /// </summary>
        /// <param name="innerState">
        /// The inner State.
        /// </param>
        private void UpdateDatabaseFromHeaders(InnerState innerState)
        {
            this.downloadInfo.ETag = innerState.HeaderETag;
            db.UpdateDownload(this.downloadInfo);
        }

        /// <summary>
        /// The update download database.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="countRetry">
        /// The count retry.
        /// </param>
        /// <param name="retryAfter">
        /// The retry after.
        /// </param>
        /// <param name="redirectCount">
        /// The redirect count.
        /// </param>
        /// <param name="gotData">
        /// The got data.
        /// </param>
        private void UpdateDownloadDatabase(
            DownloaderServiceStatus status, bool countRetry, int retryAfter, int redirectCount, bool gotData)
        {
            this.downloadInfo.Status = (Android.App.DownloadStatus) status;
            this.downloadInfo.RetryAfter = retryAfter;
            this.downloadInfo.RedirectCount = redirectCount;
            this.downloadInfo.LastModified = PolicyExtensions.GetCurrentMilliseconds();
            if (!countRetry)
            {
                this.downloadInfo.FailedCount = 0;
            }
            else if (gotData)
            {
                this.downloadInfo.FailedCount = 1;
            }
            else
            {
                this.downloadInfo.FailedCount++;
            }

            db.UpdateDownload(this.downloadInfo);
        }

        #endregion

        /// <summary>
        /// State within ExecuteDownload()
        /// </summary>
        private class InnerState
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the bytes notified.
            /// </summary>
            public int BytesNotified { get; set; }

            /// <summary>
            /// Gets or sets the bytes so far.
            /// </summary>
            public int BytesSoFar { get; set; }

            /// <summary>
            /// Gets or sets the bytes this session.
            /// </summary>
            public int BytesThisSession { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the download is
            /// continuing.
            /// </summary>
            public bool ContinuingDownload { get; set; }

            /// <summary>
            /// Gets or sets the header content disposition.
            /// </summary>
            public string HeaderContentDisposition { get; set; }

            /// <summary>
            /// Gets or sets the header content length.
            /// </summary>
            public string HeaderContentLength { get; set; }

            /// <summary>
            /// Gets or sets the header content location.
            /// </summary>
            public string HeaderContentLocation { get; set; }

            /// <summary>
            /// Gets or sets the header e-tag.
            /// </summary>
            public string HeaderETag { get; set; }

            /// <summary>
            /// Gets or sets the time of the last notification.
            /// </summary>
            public long TimeLastNotification { get; set; }

            #endregion
        }

        /// <summary>
        /// The retry download exception.
        /// </summary>
        private sealed class RetryDownloadException : Exception
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="RetryDownloadException"/> class.
            /// </summary>
            public RetryDownloadException() : base("Retrying download...")
            {
                Log.Info(Tag, string.Format("DownloadThread: RetryDownloadException: {0}", this.Message));
            }

            #endregion
        }

        /// <summary>
        /// State for the entire Run() method.
        /// </summary>
        private class State
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="info">
            /// The info.
            /// </param>
            /// <param name="service">
            /// The service.
            /// </param>
            public State(DownloadInfo info, CustomDownloaderService service)
            {
                this.RedirectCount = info.RedirectCount;
                this.RequestUri = info.Uri;
                this.Filename = service.GenerateTempSaveFileName(info.FileName);
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets or sets a value indicating whether to count the count the
            /// retry.
            /// </summary>
            public bool CountRetry { get; set; }

            /// <summary>
            /// Gets or sets the filename.
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether there is data.
            /// </summary>
            public bool GotData { get; set; }

            /// <summary>
            /// Gets or sets the new uri.
            /// </summary>
            public string NewUri { get; set; }

            /// <summary>
            /// Gets or sets the number of redirects.
            /// </summary>
            public int RedirectCount { get; set; }

            /// <summary>
            /// Gets or sets the uri of the request
            /// </summary>
            public string RequestUri { get; set; }

            /// <summary>
            /// Gets or sets the time before attempting a retry.
            /// </summary>
            public int RetryAfter { get; set; }

            /// <summary>
            /// Gets or sets the file stream of the destination.
            /// </summary>
            public FileStream Stream { get; set; }

            #endregion
        }

        /// <summary>
        /// The stop request exception.
        /// </summary>
        private class StopRequestException : Exception
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="StopRequestException"/> class.
            /// </summary>
            /// <param name="finalStatus">
            /// The final status.
            /// </param>
            /// <param name="message">
            /// The message.
            /// </param>
            /// <param name="throwable">
            /// The throwable.
            /// </param>
            public StopRequestException(DownloaderServiceStatus finalStatus, string message, Exception throwable = null)
                : base(message, throwable)
            {
                Log.Error(Tag, string.Format("DownloadThread: StopRequestException {0}", message ?? string.Empty));
                this.FinalStatus = finalStatus;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the final status.
            /// </summary>
            public DownloaderServiceStatus FinalStatus { get; private set; }

            #endregion
        }
    }
}