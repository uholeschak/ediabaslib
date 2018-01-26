using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Google.Android.Vending.Expansion.Downloader;

[assembly: Android.App.UsesPermission("com.android.vending.CHECK_LICENSE")]

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.KeyboardHidden |
                               ConfigChanges.Orientation |
                               ConfigChanges.ScreenSize)]
    public class ExpansionDownloaderActivity : AppCompatActivity, IDownloaderClient
    {
#if DEBUG
        static readonly string Tag = typeof(ExpansionDownloaderActivity).FullName;
#endif
        private const int ObbFileSize = 176457424;
        private static readonly byte[] ObbMd5 = { 0xB2, 0xE2, 0x59, 0x20, 0xBD, 0x2B, 0x11, 0x83, 0xE7, 0x94, 0xD7, 0xCE, 0x52, 0xD3, 0x8E, 0x25 };
        private const int RequestPermissionExternalStorage = 0;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage
        };

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
#if DEBUG
            Android.Util.Log.Debug(Tag, "Download state: " + newState);
#endif

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
                    showDashboard = false;
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
        }

        /// <summary>
        /// Re-connect the stub to our service on resume.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            _activityActive = true;
            _downloaderServiceConnection?.Connect(this);
            RequestStoragePermissions();
        }

        /// <summary>
        /// Disconnect the stub from our service on stop.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            _downloaderServiceConnection?.Disconnect(this);
            _activityActive = false;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (!_activityActive)
            {
                return;
            }
            switch (requestCode)
            {
                case RequestPermissionExternalStorage:
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StoragePermissonGranted();
                        break;
                    }
                    Toast.MakeText(this, GetString(Resource.String.access_denied_ext_storage), ToastLength.Long).Show();
                    Finish();
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
        public bool IsFromGooglePlay()
        {
            try
            {
                String installer = PackageManager.GetInstallerPackageName(PackageName);
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
        private bool AreExpansionFilesDelivered(bool checkMd5 = false)
        {
            string obbFile = GetObbFilename(this);
            if (obbFile == null)
            {
                return false;
            }

            if (!checkMd5)
            {
                return true;
            }

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
            Regex regex = new Regex(@"^main\.([0-9]+)\." + ActivityCommon.AppNameSpace + @"\.obb$", RegexOptions.IgnoreCase);
            PackageInfo packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            int packageVersion = -1;
            if (packageInfo != null)
            {
                packageVersion = packageInfo.VersionCode;
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
                                if ((packageVersion < 0) || (Int32.TryParse(fileVersion, out int version) && version <= packageVersion))
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
                _progressBar.Indeterminate = false;
                OnDownloadProgress(new DownloadProgressInfo(ObbFileSize, ObbFileSize, 0, 0));
                _pauseButton.Visibility = ViewStates.Visible;
                _pauseButton.Click -= OnButtonOnClick;
                _pauseButton.Click += (sender, args) => 
                {
                    Finish();
                    if (result)
                    {
                        StartActivity(typeof(ActivityMain));
                    }
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
                Android.App.PendingIntent pendingIntent = Android.App.PendingIntent.GetActivity(
                    this, 0, intent, Android.App.PendingIntentFlags.UpdateCurrent);

                // Always force download of LVL
                DownloadsDB.GetDB(this).UpdateMetadata(0, 0);
                // Request to start the download
                DownloaderServiceRequirement startResult = DownloaderService.StartDownloadServiceIfRequired(
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
#if DEBUG
                Android.Util.Log.Debug(Tag, "Cannot find own package!");
#endif
                e.PrintStackTrace();
            }

            return result;
        }

        /// <summary>
        /// If the download isn't present, we initialize the download UI. This ties
        /// all of the controls into the remote service calls.
        /// </summary>
        private void InitializeDownloadUi()
        {
            InitializeControls();
            _downloaderServiceConnection = DownloaderClientMarshaller.CreateStub(
                this, typeof(ExpansionDownloaderService));
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
            StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
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

        private void RequestStoragePermissions()
        {
            if (_permissionsExternalStorage.All(permisson => ContextCompat.CheckSelfPermission(this, permisson) == Permission.Granted))
            {
                StoragePermissonGranted();
                return;
            }
            ActivityCompat.RequestPermissions(this, _permissionsExternalStorage, RequestPermissionExternalStorage);
        }

        private void StoragePermissonGranted()
        {
            if (!_downloadStarted)
            {
                _downloadStarted = true;
                // Before we do anything, are the files we expect already here and 
                // delivered (presumably by Market) 
                // For free titles, this is probably worth doing. (so no Market 
                // request is necessary)
                bool delivered = AreExpansionFilesDelivered();

                if (delivered)
                {
                    StartActivity(typeof(ActivityMain));
                    Finish();
                    return;
                }

                if (!IsFromGooglePlay())
                {
                    AlertDialog alertDialog = new AlertDialog.Builder(this)
                        .SetMessage(Resource.String.exp_down_obb_missing)
                        .SetTitle(Resource.String.alert_title_error)
                        .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                        .Show();
                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        Finish();
                    };
                    return;
                }

                if (!GetExpansionFiles())
                {
                    InitializeDownloadUi();
                }

                if (_activityActive)
                {
                    _downloaderServiceConnection?.Connect(this);
                }
            }
        }
    }
}
