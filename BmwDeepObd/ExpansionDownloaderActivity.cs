using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.Content.PM;
using Google.Android.Vending.Expansion.Downloader;
using Xamarin.Google.Android.Play.Core.AssetPacks;

[assembly: Android.App.UsesPermission("com.android.vending.CHECK_LICENSE")]

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name",
        Name = ActivityCommon.AppNameSpace + "." + nameof(ExpansionDownloaderActivity),
        MainLauncher = true,
        Exported = true,
        AlwaysRetainTaskState = true,
        Icon = "@drawable/icon",
        ConfigurationChanges = ActivityConfigChanges)]
    [Android.App.IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { Intent.CategoryLauncher })]
    public class ExpansionDownloaderActivity : BaseActivity, IDownloaderClient
    {
        private static readonly string Tag = typeof(ExpansionDownloaderActivity).FullName;

        private enum ActivityRequest
        {
            RequestAppStorePermissions,
        }

#if true
        private const int ObbFileSize = 414847056;
        private static readonly byte[] ObbMd5 = { 0x04, 0xF5, 0xB6, 0x0A, 0x7D, 0x4C, 0xEB, 0x86, 0x33, 0xC6, 0xA4, 0x86, 0xA6, 0x68, 0xB8, 0xB9 };
#else
        private const int ObbFileSize = 404603600;
        private static readonly byte[] ObbMd5 = { 0x68, 0xAF, 0xA4, 0x5D, 0x3C, 0xAC, 0x9E, 0xF0, 0x6C, 0x57, 0xF7, 0x7C, 0x8A, 0x1C, 0x92, 0x15 };
#endif
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage
        };

        private readonly string[] _permissionsPostNotifications =
        {
            Android.Manifest.Permission.PostNotifications
        };

        private static string _assetFileName;
        private bool _storageAccessRequested;
        private bool _storageAccessGranted;
        private bool _notificationGranted;
        private bool _googlePlayErrorShown;
        private bool? _expansionFileDelivered;
        private bool _downloadStarted;
        private bool _activityActive;

        /// <summary>
        /// The downloader service.
        /// </summary>
        private IDownloaderService _downloaderService;

        /// <summary>
        /// The downloader service connection.
        /// </summary>
        private IDownloaderServiceConnection _downloaderServiceConnection;

        /// <summary>
        /// The downloader state.
        /// </summary>
        private DownloaderClientState _downloaderState;

        /// <summary>
        /// The is paused.
        /// </summary>
        private bool _isPaused;

        /// <summary>
        /// The average speed text view.
        /// </summary>
        private TextView _averageSpeedTextView;

        /// <summary>
        /// The dashboard view.
        /// </summary>
        private View _dashboardView;

        /// <summary>
        /// The open wifi settings button.
        /// </summary>
        private Button _openWiFiSettingsButton;

        /// <summary>
        /// The pause button.
        /// </summary>
        private Button _pauseButton;

        /// <summary>
        /// The progress bar.
        /// </summary>
        private ProgressBar _progressBar;

        /// <summary>
        /// The progress fraction text view.
        /// </summary>
        private TextView _progressFractionTextView;

        /// <summary>
        /// The progress percent text view.
        /// </summary>
        private TextView _progressPercentTextView;

        /// <summary>
        /// The resume on cell data button.
        /// </summary>
        private Button _resumeOnCellDataButton;

        /// <summary>
        /// The status text view.
        /// </summary>
        private TextView _statusTextView;

        /// <summary>
        /// The time remaining text view.
        /// </summary>
        private TextView _timeRemainingTextView;

        /// <summary>
        /// The use cell data view.
        /// </summary>
        private View _useCellDataView;

        /// <summary>
        /// Sets the state of the various controls based on the progressinfo 
        /// object sent from the downloader service.
        /// </summary>
        /// <param name="progress">
        /// The progressinfo object sent from the downloader service.
        /// </param>
        public void OnDownloadProgress(DownloadProgressInfo progress)
        {
            Android.Util.Log.Info(Tag, string.Format("Download progress: {0} of {1} bytes", progress.OverallProgress, progress.OverallTotal));

            _averageSpeedTextView.Text = string.Format("{0} Kb/s", Helpers.GetSpeedString(progress.CurrentSpeed));
            _timeRemainingTextView.Text = string.Format(Resources.GetString(Resource.String.exp_down_time_remaining), Helpers.GetTimeRemaining(progress.TimeRemaining));
            _progressBar.Max = (int)(progress.OverallTotal >> 8);
            _progressBar.Progress = (int)(progress.OverallProgress >> 8);
            _progressPercentTextView.Text = string.Format("{0}%", progress.OverallProgress * 100 / progress.OverallTotal);
            _progressFractionTextView.Text = Helpers.GetDownloadProgressString(progress.OverallProgress, progress.OverallTotal);
        }

        /// <summary>
        /// The download state should trigger changes in the UI.
        /// It may be useful to show the state as being indeterminate at times.  
        /// </summary>
        /// <param name="newState">
        /// The new state.
        /// </param>
        public void OnDownloadStateChanged(DownloaderClientState newState)
        {
            Android.Util.Log.Info(Tag, string.Format("Download state: {0}", newState));

            if (_downloaderState != newState)
            {
                _downloaderState = newState;
                _statusTextView.Text = Helpers.GetDownloaderStringFromState(this, newState);
            }

            bool showDashboard = true;
            bool showCellMessage = false;
            bool paused = false;
            bool indeterminate = true;
            switch (newState)
            {
                case DownloaderClientState.Idle:
                case DownloaderClientState.Connecting:
                case DownloaderClientState.FetchingUrl:
                    break;

                case DownloaderClientState.Downloading:
                    indeterminate = false;
                    break;

                case DownloaderClientState.Failed:
                case DownloaderClientState.FailedCanceled:
                case DownloaderClientState.FailedFetchingUrl:
                case DownloaderClientState.FailedUnlicensed:
                    paused = true;
                    indeterminate = false;
                    break;

                case DownloaderClientState.PausedNeedCellularPermission:
                case DownloaderClientState.PausedWifiDisabledNeedCellularPermission:
                    showDashboard = false;
                    paused = true;
                    indeterminate = false;
                    showCellMessage = true;
                    break;

                case DownloaderClientState.PausedByRequest:
                    paused = true;
                    indeterminate = false;
                    break;

                case DownloaderClientState.PausedRoaming:
                case DownloaderClientState.PausedSdCardUnavailable:
                    paused = true;
                    indeterminate = false;
                    break;

                case DownloaderClientState.PausedNetworkUnavailable:
                    showDashboard = false;
                    paused = true;
                    break;

                default:
                    paused = true;
                    break;
            }

            if (newState != DownloaderClientState.Completed)
            {
                _dashboardView.Visibility = showDashboard ? ViewStates.Visible : ViewStates.Gone;
                _useCellDataView.Visibility = showCellMessage ? ViewStates.Visible : ViewStates.Gone;
                _progressBar.Indeterminate = indeterminate;
                UpdatePauseButton(paused);

                switch (newState)
                {
                    case DownloaderClientState.FailedUnlicensed:
                        CheckGooglePlay();
                        break;
                }
            }
            else
            {
                ValidateExpansionFiles();
            }
        }

        /// <summary>
        /// Create the remote service and marshaler.
        /// </summary>
        /// <remarks>
        /// This is how we pass the client information back to the service so 
        /// the client can be properly notified of changes. 
        /// Do this every time we reconnect to the service.
        /// </remarks>
        /// <param name="m">
        /// The messenger to use.
        /// </param>
        public void OnServiceConnected(Messenger m)
        {
            _downloaderService = DownloaderServiceMarshaller.CreateProxy(m);
            _downloaderService.OnClientUpdated(_downloaderServiceConnection.GetMessenger());
        }

        /// <summary>
        /// Called when the activity is first created; we wouldn't create a 
        /// layout in the case where we have the file and are moving to another
        /// activity without downloading.
        /// </summary>
        /// <param name="savedInstanceState">
        /// The saved instance state.
        /// </param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _downloadStarted = false;
            _storageAccessRequested = false;
            _storageAccessGranted = false;
            _notificationGranted = false;
            _googlePlayErrorShown = false;
            _expansionFileDelivered = null;

            CustomDownloadNotification.RegisterNotificationChannels(this);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _storageAccessRequested = false;
            _googlePlayErrorShown = false;
            _expansionFileDelivered = null;
            if (IsAssetPresent(this))
            {
                try
                {
                    Intent intent = new Intent(this, typeof(ActivityMain));
                    intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    StartActivity(intent);
                    Finish();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Re-connect the stub to our service on resume.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            _activityActive = true;
            try
            {
                _downloaderServiceConnection?.Connect(this);
            }
            catch (Exception)
            {
                // ignored
            }

            if (!_storageAccessRequested)
            {
                _storageAccessRequested = true;
                RequestStoragePermissions();
            }

            StartDownload();
        }

        /// <summary>
        /// Disconnect the stub from our service on stop.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            try
            {
                _downloaderServiceConnection?.Disconnect(this);
            }
            catch (Exception)
            {
                // ignored
            }

            _activityActive = false;
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestAppStorePermissions:
                    RequestStoragePermissions(true);
                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (_actvityDestroyed)
            {
                return;
            }
            switch (requestCode)
            {
                case ActivityCommon.RequestPermissionExternalStorage:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        StoragePermissionGranted();
                        break;
                    }

                    if (!ActivityCommon.IsExtrenalStorageAccessRequired())
                    {
                        StoragePermissionGranted();
                        break;
                    }

                    bool finish = true;
                    AlertDialog alertDialog = new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            if (ActivityCommon.OpenAppSettingDetails(this, (int)ActivityRequest.RequestAppStorePermissions))
                            {
                                finish = false;
                            }
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.access_denied_ext_storage)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();

                    if (alertDialog != null)
                    {
                        alertDialog.DismissEvent += (sender, args) =>
                        {
                            if (_actvityDestroyed)
                            {
                                return;
                            }

                            if (finish)
                            {
                                Finish();
                            }
                        };
                    }
                    break;

                case ActivityCommon.RequestPermissionNotifications:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        NotificationsPermissionGranted();
                        break;
                    }

                    NotificationsPermissionGranted(false);
                    break;
            }
        }

        private void InitializeControls()
        {
            SetContentView(Resource.Layout.downloader);

            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            _statusTextView = FindViewById<TextView>(Resource.Id.statusText);
            _progressFractionTextView = FindViewById<TextView>(Resource.Id.progressAsFraction);
            _progressPercentTextView = FindViewById<TextView>(Resource.Id.progressAsPercentage);
            _averageSpeedTextView = FindViewById<TextView>(Resource.Id.progressAverageSpeed);
            _timeRemainingTextView = FindViewById<TextView>(Resource.Id.progressTimeRemaining);
            _dashboardView = FindViewById(Resource.Id.downloaderDashboard);
            _useCellDataView = FindViewById(Resource.Id.approveCellular);
            _pauseButton = FindViewById<Button>(Resource.Id.pauseButton);
            _openWiFiSettingsButton = FindViewById<Button>(Resource.Id.wifiSettingsButton);
            _resumeOnCellDataButton = FindViewById<Button>(Resource.Id.resumeOverCellular);

            _pauseButton.Click += OnButtonOnClick;
            _openWiFiSettingsButton.Click += OnOpenWiFiSettingsButtonOnClick;
            _resumeOnCellDataButton.Click += OnEventHandler;
        }

        /// <summary>
        /// Is package downloaded form google play
        /// </summary>
        public static bool IsFromGooglePlay(Context context)
        {
            try
            {
                String installer = context?.PackageManager?.GetInstallerPackageName(context.PackageName ?? string.Empty);
                if (string.IsNullOrEmpty(installer))
                {
                    return false;
                }
                return string.Compare(installer, "com.android.vending", StringComparison.OrdinalIgnoreCase) == 0;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public static bool IsAssetPresent(Context context)
        {
            List<string> assetList = GetAssetPathList(context);
            if (assetList?.Count > 0)
            {
                return true;
            }

            return !string.IsNullOrEmpty(GetAssetFilename());
        }

        /// <summary>
        /// Go through each of the Expansion APK files defined in the project 
        /// and determine if the files are present and match the required size. 
        /// </summary>
        /// <remarks>
        /// Free applications should definitely consider doing this, as this 
        /// allows the application to be launched for the first time without
        /// having a network connection present.
        /// Paid applications that use LVL should probably do at least one LVL 
        /// check that requires the network to be present, so this is not as
        /// necessary.
        /// </remarks>
        /// <returns>
        /// True if they are present, otherwise False;
        /// </returns>
        // ReSharper disable once UnusedParameter.Local
        private bool AreExpansionFilesDelivered(bool checkMd5 = false)
        {
            string obbFile = GetObbFilename(this);
            if (obbFile == null)
            {
                return false;
            }

#if !DEBUG
            if (!checkMd5)
            {
                return true;
            }
#endif

            byte[] md5 = CalculateMd5(obbFile);
            if (md5 != null)
            {
#if DEBUG
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("MD5: {");
                int index = 0;
                foreach (byte value in md5)
                {
                    if (index > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(string.Format("0x{0:X02}", value));
                    index++;
                }
                sb.Append("};");
                Android.Util.Log.Debug(Tag, sb.ToString());
#endif
                if (md5.SequenceEqual(ObbMd5))
                {
                    return true;
                }
            }

            try
            {
                File.Delete(obbFile);
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public static string GetObbFilename(Context context)
        {
            Regex regex = new Regex(@"^main\.([0-9]+)\." + context.PackageName + @"\.obb$", RegexOptions.IgnoreCase);
            PackageInfo packageInfo = context.PackageManager?.GetPackageInfo(context.PackageName ?? string.Empty, 0);
            long packageVersion = -1;
            if (packageInfo != null)
            {
                packageVersion = PackageInfoCompat.GetLongVersionCode(packageInfo);
            }
            string obbFile = null;
            Java.IO.File[] obbDirs;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                obbDirs = context.GetObbDirs();
            }
            else
            {
                obbDirs = new [] { context.ObbDir };
            }
            foreach (Java.IO.File dir in obbDirs)
            {
                try
                {
                    if (dir != null && Directory.Exists(dir.AbsolutePath))
                    {
                        string[] files = Directory.GetFiles(dir.AbsolutePath);
                        foreach (string file in files)
                        {
                            MatchCollection matchesFile = regex.Matches(Path.GetFileName(file));
                            if ((matchesFile.Count == 1) && (matchesFile[0].Groups.Count == 2))
                            {
                                string fileVersion = matchesFile[0].Groups[1].Value;
                                if ((packageVersion < 0) || (Int64.TryParse(fileVersion, out long version) && version <= packageVersion))
                                {
                                    FileInfo fileInfo = new FileInfo(file);
                                    if (fileInfo.Exists && fileInfo.Length == ObbFileSize)
                                    {
                                        obbFile = file;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return obbFile;
        }

        public static List<string> GetAssetPathList(Context context)
        {
            try
            {
                List<string> assetList = new List<string>();
                using (IAssetPackManager assetPackManager = AssetPackManagerFactory.GetInstance(context))
                {
                    IDictionary<string, AssetPackLocation> assetPackLocations = assetPackManager.PackLocations;
                    if (assetPackLocations != null)
                    {
                        foreach (KeyValuePair<string, AssetPackLocation> location in assetPackLocations)
                        {
                            assetList.Add(location.Value.AssetsPath());
                        }
                    }
                }
                return assetList;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static string GetAssetFilename()
        {
            if (!string.IsNullOrEmpty(_assetFileName))
            {
                return _assetFileName;
            }

            try
            {
                Regex regex = new Regex(@"^Ecu.*\.bin$", RegexOptions.IgnoreCase);
                AssetManager assets = ActivityCommon.GetPackageContext()?.Assets;
                if (assets != null)
                {
                    string[] assetFiles = assets.List(string.Empty);
                    if (assetFiles != null)
                    {
                        foreach (string fileName in assetFiles)
                        {
                            if (regex.IsMatch(fileName))
                            {
                                _assetFileName = fileName;
                                return fileName;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _assetFileName = string.Empty;
            return null;
        }

        /// <summary>
        /// Calculate MD5 of file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] CalculateMd5(string fileName)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        return md5.ComputeHash(stream);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// The do validate zip files.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private void DoValidateZipFiles(object state)
        {
            bool result = AreExpansionFilesDelivered(true);

            RunOnUiThread(() => 
            {
                if (!_activityActive)
                {
                    return;
                }
                _progressBar.Indeterminate = false;
                OnDownloadProgress(new DownloadProgressInfo(ObbFileSize, ObbFileSize, 0, 0));
                _pauseButton.Visibility = ViewStates.Visible;
                _pauseButton.Click -= OnButtonOnClick;
                _pauseButton.Click += (sender, args) => 
                {
                    if (!_activityActive)
                    {
                        return;
                    }

                    if (result)
                    {
                        try
                        {
                            Intent intent = new Intent(this, typeof(ActivityMain));
                            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                            StartActivity(intent);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    Finish();
                };

                _dashboardView.Visibility = ViewStates.Visible;
                _useCellDataView.Visibility = ViewStates.Gone;

                if (result)
                {
                    _statusTextView.SetText(Resource.String.exp_down_validation_complete);
                    _pauseButton.SetText(Resource.String.button_ok);
                }
                else
                {
                    _statusTextView.SetText(Resource.String.exp_down_validation_failed);
                    _pauseButton.SetText(Resource.String.button_abort);
                    CheckGooglePlay();
                }
            });
        }

        /// <summary>
        /// The get expansion files.
        /// </summary>
        /// <returns>
        /// The get expansion files.
        /// </returns>
        private bool GetExpansionFiles()
        {
            bool result = false;

            try
            {
                // Build the intent that launches this activity.
                Intent launchIntent = Intent;
                var intent = new Intent(this, typeof(ExpansionDownloaderActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                intent.SetAction(launchIntent.Action);

                if (launchIntent.Categories != null)
                {
                    foreach (string category in launchIntent.Categories)
                    {
                        intent.AddCategory(category);
                    }
                }

                // Build PendingIntent used to open this activity when user 
                // taps the notification.
                Android.App.PendingIntentFlags intentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    intentFlags |= Android.App.PendingIntentFlags.Mutable;
                }
                Android.App.PendingIntent pendingIntent = Android.App.PendingIntent.GetActivity(this, 0, intent, intentFlags);

                // Always force download of LVL
                DownloadsDB db = DownloadsDB.GetDB(this);
                if (db != null)
                {
                    db.UpdateMetadata(0, 0);
                    db.UpdateFlags(DownloaderServiceFlags.None);
                }

                // Request to start the download
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                DownloaderServiceRequirement startResult = ExpansionDownloaderService.StartDownloadServiceIfRequired(
                    this, pendingIntent, typeof(ExpansionDownloaderService));

                // The DownloaderService has started downloading the files, 
                // show progress otherwise, the download is not needed so  we 
                // fall through to starting the actual app.
                if (startResult != DownloaderServiceRequirement.NoDownloadRequired)
                {
                    InitializeDownloadUi();
                    result = true;
                }
            }
            catch (PackageManager.NameNotFoundException e)
            {
                Android.Util.Log.Error(Tag, "Cannot find own package!");
                e.PrintStackTrace();
            }
            catch (Exception ex)
            {
                // ignored
                Android.Util.Log.Error(Tag, string.Format("GetExpansionFiles Exception: {0}", ex.Message));
            }

            return result;
        }

        private void CheckGooglePlay()
        {
            if (_googlePlayErrorShown)
            {
                return;
            }

            if (IsFromGooglePlay(this))
            {
                return;
            }

            PackageInfo packageInfo = ActivityCommon.GetPackageInfo(PackageManager, PackageName);
            long packageVersion = 0;
            if (packageInfo != null)
            {
                packageVersion = PackageInfoCompat.GetLongVersionCode(packageInfo);
            }
            string obbFileName = string.Format(CultureInfo.InvariantCulture, "main.{0}.{1}.obb", packageVersion, PackageName);

            Java.IO.File[] obbDirs;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                obbDirs = GetObbDirs();
            }
            else
            {
                obbDirs = new[] { ObbDir };
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (obbDirs != null)
            {
                foreach (Java.IO.File dir in obbDirs)
                {
                    if (dir != null && !string.IsNullOrEmpty(dir.AbsolutePath))
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine();
                        }
                        sb.Append(@"'");
                        sb.Append(dir.AbsolutePath);
                        sb.Append(@"'");
                    }
                }
            }

            string obbDirsName = sb.ToString();
            if (string.IsNullOrEmpty(obbDirsName))
            {
                obbDirsName = "-";
            }

            _googlePlayErrorShown = true;
            string message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.exp_down_obb_missing), obbFileName, obbDirsName);
            new AlertDialog.Builder(this)
                .SetMessage(message)
                .SetTitle(Resource.String.alert_title_error)
                .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                .Show();
        }

        /// <summary>
        /// If the download isn't present, we initialize the download UI. This ties
        /// all of the controls into the remote service calls.
        /// </summary>
        private void InitializeDownloadUi()
        {
            if (_downloaderServiceConnection != null)
            {
                return;
            }

            try
            {
                NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
                notificationManager.Cancel(CustomDownloadNotification.NotificationId);
            }
            catch (Exception)
            {
                // ignored
            }

            InitializeControls();
            _downloaderServiceConnection = DownloaderClientMarshaller.CreateStub(this, typeof(ExpansionDownloaderService));
        }

        /// <summary>
        /// The on button on click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnButtonOnClick(object sender, EventArgs e)
        {
            if (_isPaused)
            {
                _downloaderService?.RequestContinueDownload();
            }
            else
            {
                _downloaderService?.RequestPauseDownload();
            }

            UpdatePauseButton(!_isPaused);
        }

        /// <summary>
        /// The on event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private void OnEventHandler(object sender, EventArgs args)
        {
            _downloaderService?.SetDownloadFlags(DownloaderServiceFlags.DownloadOverCellular);
            _downloaderService?.RequestContinueDownload();
            _useCellDataView.Visibility = ViewStates.Gone;
        }

        /// <summary>
        /// The on open wi fi settings button on click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnOpenWiFiSettingsButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Update the pause button.
        /// </summary>
        /// <param name="paused">
        /// Is the download paused.
        /// </param>
        private void UpdatePauseButton(bool paused)
        {
            _isPaused = paused;
            int stringResourceId = paused ? Resource.String.exp_down_button_resume : Resource.String.exp_down_button_pause;
            _pauseButton.SetText(stringResourceId);
        }

        /// <summary>
        /// Perfom a check to see if the expansion files are vanid zip files.
        /// </summary>
        private void ValidateExpansionFiles()
        {
            // Pre execute
            _dashboardView.Visibility = ViewStates.Visible;
            _useCellDataView.Visibility = ViewStates.Gone;
            _statusTextView.SetText(Resource.String.exp_down_verifying_download);
            _progressBar.Indeterminate = true;
            _pauseButton.SetText(Resource.String.exp_down_button_cancel_verify);
            _pauseButton.Visibility = ViewStates.Invisible;

            ThreadPool.QueueUserWorkItem(DoValidateZipFiles);
        }

        private void RequestStoragePermissions(bool finish = false)
        {
            if (_actvityDestroyed)
            {
                return;
            }

            if (ActivityCommon.IsNotificationsAccessRequired())
            {
                _expansionFileDelivered = AreExpansionFilesDelivered();
                if (!_expansionFileDelivered.Value)
                {
                    RequestNotificationsPermissions();
                    return;
                }
            }

            if (_permissionsExternalStorage.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
            {
                StoragePermissionGranted();
                return;
            }

            if (!ActivityCommon.IsExtrenalStorageAccessRequired())
            {
                StoragePermissionGranted();
                return;
            }

            if (finish)
            {
                Finish();
                return;
            }

            ActivityCompat.RequestPermissions(this, _permissionsExternalStorage, ActivityCommon.RequestPermissionExternalStorage);
        }

        private void StoragePermissionGranted()
        {
            _storageAccessGranted = true;
            if (_activityActive)
            {
                StartDownload();
            }
        }

        private void RequestNotificationsPermissions()
        {
            if (_actvityDestroyed)
            {
                return;
            }

            if (_permissionsPostNotifications.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
            {
                NotificationsPermissionGranted();
                return;
            }

            ActivityCompat.RequestPermissions(this, _permissionsPostNotifications, ActivityCommon.RequestPermissionNotifications);
        }

        private void NotificationsPermissionGranted(bool granted = true)
        {
            _notificationGranted = granted;
            StoragePermissionGranted();
        }

        private void StartDownload()
        {
            if (!_storageAccessGranted)
            {
                return;
            }

            if (!_downloadStarted)
            {
                _downloadStarted = true;
                // Before we do anything, are the files we expect already here and 
                // delivered (presumably by Market) 
                // For free titles, this is probably worth doing. (so no Market 
                // request is necessary)
                bool delivered = _expansionFileDelivered.HasValue ? _expansionFileDelivered.Value : AreExpansionFilesDelivered();
                if (delivered)
                {
                    try
                    {
                        Intent intent = new Intent(this, typeof(ActivityMain));
                        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                        StartActivity(intent);
                        Finish();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    return;
                }

                InitializeDownloadUi();
                if (_activityActive)
                {
                    try
                    {
                        _downloaderServiceConnection?.Connect(this);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                GetExpansionFiles();
            }
        }
    }
}
