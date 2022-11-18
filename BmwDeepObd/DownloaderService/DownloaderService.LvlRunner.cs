// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloaderService.LvlRunner.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   The downloader service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Android.App;
using Android.Content.PM;
using Android.Provider;
using Android.Util;
using AndroidX.Core.Content.PM;
using Google.Android.Vending.Expansion.Downloader;
using Google.Android.Vending.Licensing;

namespace BmwDeepObd
{
    /// <summary>
    /// The downloader service.
    /// </summary>
    public abstract partial class CustomDownloaderService
    {
        /// <summary>
        /// The lvl runnable.
        /// </summary>
        private class LvlRunnable : Java.Lang.Object, Java.Lang.IRunnable
        {
            #region Fields

            /// <summary>
            /// The context.
            /// </summary>
            private readonly Android.Content.Context context;

            /// <summary>
            /// The context.
            /// </summary>
            private readonly CustomDownloaderService downloaderService;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="LvlRunnable"/> class.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            /// <param name="intent">
            /// The intent.
            /// </param>
            internal LvlRunnable(Android.Content.Context context, CustomDownloaderService downloader, PendingIntent intent)
            {
                Log.Info(Tag, "DownloaderService.LvlRunnable.ctor");
                this.context = context;
                this.downloaderService = downloader;
                this.downloaderService.pPendingIntent = intent;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The run.
            /// </summary>
            public void Run()
            {
                IsServiceRunning = true;
                this.downloaderService.downloadNotification.OnDownloadStateChanged(DownloaderClientState.FetchingUrl);
                string deviceId = Settings.Secure.GetString(this.context.ContentResolver, Settings.Secure.AndroidId);

                APKExpansionPolicy aep = new APKExpansionPolicy(this.context, new AESObfuscator(this.downloaderService.GetSalt(), this.context.PackageName, deviceId));

                // reset our policy back to the start of the world to force a re-check
                aep.ResetPolicy();

                // let's try and get the OBB file from LVL first
                // Construct the LicenseChecker with a IPolicy.
                LicenseChecker checker = new LicenseChecker(this.context, aep, this.downloaderService.PublicKey);
                checker.CheckAccess(new ApkLicenseCheckerCallback(this, aep));
            }

            #endregion

            /// <summary>
            /// The apk license checker callback.
            /// </summary>
            private class ApkLicenseCheckerCallback : Java.Lang.Object, ILicenseCheckerCallback
            {
                #region Fields

                /// <summary>
                /// The lvl runnable.
                /// </summary>
                private readonly LvlRunnable lvlRunnable;

                /// <summary>
                /// The policy.
                /// </summary>
                private readonly APKExpansionPolicy policy;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initializes a new instance of the <see cref="ApkLicenseCheckerCallback"/> class.
                /// </summary>
                /// <param name="lvlRunnable">
                /// The lvl runnable.
                /// </param>
                /// <param name="policy">
                /// The policy.
                /// </param>
                public ApkLicenseCheckerCallback(LvlRunnable lvlRunnable, APKExpansionPolicy policy)
                {
                    this.lvlRunnable = lvlRunnable;
                    this.policy = policy;
                }

                #endregion

                #region Properties

                /// <summary>
                /// Gets Downlaoder service.
                /// </summary>
                private CustomDownloaderService Downloader
                {
                    get
                    {
                        return this.lvlRunnable.downloaderService;
                    }
                }

                #endregion

                #region Public Methods and Operators

                /// <summary>
                /// The allow.
                /// </summary>
                /// <param name="reason">
                /// The reason.
                /// </param>
                /// <exception cref="Java.Lang.RuntimeException">
                /// Error with LVL checking and database integrity
                /// </exception>
                /// <exception cref="Java.Lang.RuntimeException">
                /// Error with getting information from package name
                /// </exception>
                public void Allow(PolicyResponse reason)
                {
                    try
                    {
                        int count = this.policy.ExpansionURLCount;
                        DownloadsDB db = DownloadsDB.GetDB(Downloader);
                        if (count == 0)
                        {
                            Log.Info(Tag, "No expansion packs.");
                        }

                        DownloaderServiceStatus status = 0;
                        for (int index = 0; index < count; index++)
                        {
                            string currentFileName = this.policy.GetExpansionFileName(index);
                            if (currentFileName != null)
                            {
                                DownloadInfo di = new DownloadInfo(index, currentFileName, Downloader.PackageName);
                                long fileSize = this.policy.GetExpansionFileSize(index);
                                string expansionUrl = this.policy.GetExpansionURL(index) ?? string.Empty;
                                if (this.Downloader.HandleFileUpdated(db, index, currentFileName, fileSize))
                                {
                                    Log.Info(Tag, string.Format("File: {0} new URL: {1}", di.FileName, expansionUrl));
                                    status = (DownloaderServiceStatus)(-1);
                                    di.ResetDownload();
                                    di.Uri = expansionUrl;
                                    di.TotalBytes = fileSize;
                                    di.Status = (DownloadStatus) status;
                                    db.UpdateDownload(di);
                                }
                                else
                                {
                                    // we need to read the download information from the database
                                    DownloadInfo dbdi = GetDownloadInfoByFileName(db, di.FileName);
                                    if (dbdi == null)
                                    {
                                        // the file exists already and is the correct size
                                        // was delivered by Market or through another mechanism
                                        Log.Info(Tag, string.Format("file {0} found. Not downloading.", di.FileName));
                                        di.Status = (DownloadStatus) DownloaderServiceStatus.Success;
                                        di.TotalBytes = fileSize;
                                        di.CurrentBytes = fileSize;
                                        di.Uri = expansionUrl;
                                        db.UpdateDownload(di);
                                    }
                                    else if ((DownloaderServiceStatus) dbdi.Status != DownloaderServiceStatus.Success ||
                                             string.Compare(dbdi.Uri ?? string.Empty, expansionUrl, System.StringComparison.Ordinal) != 0)
                                    {
                                        // we just update the URL
                                        Log.Info(Tag, string.Format("File: {0} update URL: {1}", di.FileName, expansionUrl));
                                        dbdi.Uri = expansionUrl;
                                        db.UpdateDownload(dbdi);
                                        status = (DownloaderServiceStatus) (-1);
                                    }
                                }
                            }
                        }

                        // first: do we need to do an LVL update?
                        // we begin by getting our APK version from the package manager
                        try
                        {
                            PackageInfo pi = ActivityCommon.GetPackageInfo(this.Downloader.PackageManager, this.Downloader.PackageName);
                            if (pi != null)
                            {
                                db.UpdateMetadata((int)PackageInfoCompat.GetLongVersionCode(pi), status);
                            }
                            DownloaderServiceRequirement required = StartDownloadServiceIfRequired(this.Downloader, this.Downloader.pPendingIntent, this.Downloader.GetType());
                            switch (required)
                            {
                                case DownloaderServiceRequirement.NoDownloadRequired:
                                    this.Downloader.downloadNotification.OnDownloadStateChanged(DownloaderClientState.Completed);
                                    break;

                                case DownloaderServiceRequirement.LvlCheckRequired: // DANGER WILL ROBINSON!
                                    Log.Error(Tag, "In LVL checking loop!");
                                    this.Downloader.downloadNotification.OnDownloadStateChanged(DownloaderClientState.FailedUnlicensed);
                                    throw new Java.Lang.RuntimeException("Error with LVL checking and database integrity");

                                case DownloaderServiceRequirement.DownloadRequired:
                                    // do nothing: the download will notify the application when things are done
                                    break;
                            }
                        }
                        catch (PackageManager.NameNotFoundException e1)
                        {
                            e1.PrintStackTrace();
                            throw new Java.Lang.RuntimeException("Error with getting information from package name");
                        }
                        catch (Java.Lang.Exception ex)
                        {
                            Log.Error(Tag, string.Format("LVL Update Exception: {0}", ex.Message));
                            throw;
                        }
                    }
                    catch (Java.Lang.Exception ex)
                    {
                        Log.Error(Tag, string.Format("Allow Exception: {0}", ex.Message));
                        throw;
                    }
                    finally
                    {
                        IsServiceRunning = false;
                    }
                }

                /// <summary>
                /// The application error.
                /// </summary>
                /// <param name="errorCode">
                /// The error code.
                /// </param>
                public void ApplicationError(LicenseCheckerErrorCode errorCode)
                {
                    try
                    {
                        this.Downloader.downloadNotification.OnDownloadStateChanged(DownloaderClientState.FailedFetchingUrl);
                    }
                    finally
                    {
                        IsServiceRunning = false;
                    }
                }

                /// <summary>
                /// The dont allow.
                /// </summary>
                /// <param name="reason">
                /// The reason.
                /// </param>
                public void DontAllow(PolicyResponse reason)
                {
                    try
                    {
                        switch (reason)
                        {
                            case PolicyResponse.NotLicensed:
                                this.Downloader.downloadNotification.OnDownloadStateChanged(DownloaderClientState.FailedUnlicensed);
                                break;
                            case PolicyResponse.Retry:
                                this.Downloader.downloadNotification.OnDownloadStateChanged(DownloaderClientState.FailedFetchingUrl);
                                break;
                        }
                    }
                    finally
                    {
                        IsServiceRunning = false;
                    }
                }

                #endregion
            }
        }
    }
}