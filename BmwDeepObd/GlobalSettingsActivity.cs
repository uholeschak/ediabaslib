using System;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.DocumentFile.Provider;
using BmwDeepObd.FilePicker;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/settings_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(GlobalSettingsActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class GlobalSettingsActivity : BaseActivity
    {
        public class InstanceData
        {
            public string CopyToAppSrcUri { get; set; }
            public string CopyToAppDstPath { get; set; }
            public string CopyFromAppSrcPath { get; set; }
            public string CopyFromAppDstUri { get; set; }
        }

        // Intent extra
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraCopyFileName = "copy_file_name";
        public const string ExtraSelection = "selection";
        public const string SelectionStorageLocation = "storage_location";
        public const string SelectionCopyFromApp = "copy_from_app";
        public const string ExtraExportFile = "export_file";
        public const string ExtraImportFile = "import_file";
        public const string ExtraSettingsMode = "settigs_mode";

        public const string SettingsFileName = "DeepObdSetting.xml";

        private enum ActivityRequest
        {
            RequestDevelopmentSettings,
            RequestNotificationSettings,
            RequestOpenDocumentTreeToApp,
            RequestOpenDocumentTreeFromApp,
            RequestSelDirToApp,
            RequestSelDirFromApp,
            RequestSelDirDelApp,
        }

        private InstanceData _instanceData = new InstanceData();
        private string _appDataDir;
        private string _copyFileName;
        private string _selection;
        private ActivityCommon _activityCommon;
        private string _exportFileName;

        private RadioButton _radioButtonLocaleDefault;
        private RadioButton _radioButtonLocaleEn;
        private RadioButton _radioButtonLocaleDe;
        private RadioButton _radioButtonLocaleRu;
        private RadioButton _radioButtonThemeDark;
        private RadioButton _radioButtonThemeLight;
        private TextView _textViewCaptionTranslator;
        private RadioButton _radioButtonTranslatorYandex;
        private RadioButton _radioButtonTranslatorIbm;
        private TextView _textViewCaptionTranslatorLogin;
        private CheckBox _checkBoxTranslatorLogin;
        private CheckBox _checkBoxAutoHideTitleBar;
        private CheckBox _checkBoxSuppressTitleBar;
        private CheckBox _checkBoxFullScreenMode;
        private TextView _textViewCaptionMultiWindow;
        private CheckBox _checkBoxSwapMultiWindowOrientation;
        private TextView _textViewCaptionInternet;
        private RadioGroup _radioGroupInternet;
        private RadioButton _radioButtonInternetCellular;
        private RadioButton _radioButtonInternetWifi;
        private RadioButton _radioButtonInternetEthernet;
        private RadioButton _radioButtonAskForBtEnable;
        private RadioButton _radioButtonAlwaysEnableBt;
        private RadioButton _radioButtonNoBtHandling;
        private CheckBox _checkBoxDisableBtAtExit;
        private RadioButton _radioButtonCommLockNone;
        private RadioButton _radioButtonCommLockCpu;
        private RadioButton _radioButtonCommLockDim;
        private RadioButton _radioButtonCommLockBright;
        private RadioButton _radioButtonLogLockNone;
        private RadioButton _radioButtonLogLockCpu;
        private RadioButton _radioButtonLogLockDim;
        private RadioButton _radioButtonLogLockBright;
        private CheckBox _checkBoxStoreDataLogSettings;
        private RadioButton _radioButtonStartOffline;
        private RadioButton _radioButtonStartConnect;
        private RadioButton _radioButtonStartConnectClose;
        private CheckBox _checkBoxDoubleClickForAppExit;
        private CheckBox _checkBoxSendDataBroadcast;
        private RadioButton _radioButtonUpdateOff;
        private RadioButton _radioButtonUpdate1Day;
        private RadioButton _radioButtonUpdate1Week;
        private TextView _textViewCaptionCpuUsage;
        private CheckBox _checkBoxCheckCpuUsage;
        private CheckBox _checkBoxCheckEcuFiles;
        private CheckBox _checkBoxShowBatteryVoltageWarning;
        private CheckBox _checkBoxOldVagMode;
        private CheckBox _checkBoxUseBmwDatabase;
        private CheckBox _checkBoxShowOnlyRelevantErrors;
        private CheckBox _checkBoxScanAllEcus;
        private LinearLayout _layoutFileManage;
        private Button _buttonStorageCopyTreeToApp;
        private Button _buttonStorageCopyTreeFromApp;
        private Button _buttonStorageDelTreeFromApp;
        private Button _buttonStorageLocation;
        private TextView _textViewCaptionNotifications;
        private Button _buttonManageNotifications;
        private CheckBox _checkBoxCollectDebugInfo;
        private CheckBox _checkBoxHciSnoopLog;
        private Button _buttonHciSnoopLog;
        private Button _buttonDefaultSettings;
        private Button _buttonExportSettings;
        private Button _buttonImportSettings;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);
            _allowFullScreenMode = false;

            _instanceData.CopyToAppSrcUri = ActivityCommon.CopyToAppSrc;
            _instanceData.CopyToAppDstPath = ActivityCommon.CopyToAppDst;
            _instanceData.CopyFromAppSrcPath = ActivityCommon.CopyFromAppSrc;
            _instanceData.CopyFromAppDstUri = ActivityCommon.CopyFromAppDst;
            if (savedInstanceState != null)
            {
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.settings);

            SetResult(Android.App.Result.Canceled);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _copyFileName = Intent.GetStringExtra(ExtraCopyFileName);
            _selection = Intent.GetStringExtra(ExtraSelection);

            _activityCommon = new ActivityCommon(this);

            bool allowExport = false;
            bool allowImport = false;
            try
            {
                if (!string.IsNullOrEmpty(_appDataDir))
                {
                    _exportFileName = Path.Combine(_appDataDir, SettingsFileName);
                    allowExport = true;
                    allowImport = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _radioButtonLocaleDefault = FindViewById<RadioButton>(Resource.Id.radioButtonLocaleDefault);
            _radioButtonLocaleEn = FindViewById<RadioButton>(Resource.Id.radioButtonLocaleEn);
            _radioButtonLocaleDe = FindViewById<RadioButton>(Resource.Id.radioButtonLocaleDe);
            _radioButtonLocaleRu = FindViewById<RadioButton>(Resource.Id.radioButtonLocaleRu);

            _radioButtonThemeDark = FindViewById<RadioButton>(Resource.Id.radioButtonThemeDark);
            _radioButtonThemeLight = FindViewById<RadioButton>(Resource.Id.radioButtonThemeLight);

            ViewStates viewStateTranslation = ActivityCommon.IsTranslationRequired() ? ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionTranslator = FindViewById<TextView>(Resource.Id.textViewCaptionTranslator);
            _textViewCaptionTranslator.Visibility = viewStateTranslation;
            _radioButtonTranslatorYandex = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorYandex);
            _radioButtonTranslatorYandex.Visibility = viewStateTranslation;
            _radioButtonTranslatorIbm = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorIbm);
            _radioButtonTranslatorIbm.Visibility = viewStateTranslation;

            ViewStates viewStateTransLogin =
                ActivityCommon.IsTranslationRequired() ||
                (ActivityCommon.SelectedTranslator == ActivityCommon.TranslatorType.IbmWatson && ActivityCommon.IsTranslationAvailable()) ?
                ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionTranslatorLogin = FindViewById<TextView>(Resource.Id.textViewCaptionTranslatorLogin);
            _textViewCaptionTranslatorLogin.Visibility = viewStateTransLogin;
            _checkBoxTranslatorLogin = FindViewById<CheckBox>(Resource.Id.checkBoxTranslatorLogin);
            _checkBoxTranslatorLogin.Visibility = viewStateTransLogin;

            _checkBoxAutoHideTitleBar = FindViewById<CheckBox>(Resource.Id.checkBoxAutoHideTitleBar);
            _checkBoxSuppressTitleBar = FindViewById<CheckBox>(Resource.Id.checkBoxSuppressTitleBar);
            _checkBoxFullScreenMode = FindViewById<CheckBox>(Resource.Id.checkBoxFullScreenMode);

            ViewStates viewStateMultiWindow = Build.VERSION.SdkInt >= BuildVersionCodes.N ? ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionMultiWindow = FindViewById<TextView>(Resource.Id.textViewCaptionMultiWindow);
            _checkBoxSwapMultiWindowOrientation = FindViewById<CheckBox>(Resource.Id.checkBoxSwapMultiWindowOrientation);
            _textViewCaptionMultiWindow.Visibility = viewStateMultiWindow;
            _checkBoxSwapMultiWindowOrientation.Visibility = viewStateMultiWindow;

            ViewStates viewStateInternet = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionInternet = FindViewById<TextView>(Resource.Id.textViewCaptionInternet);
            _radioGroupInternet = FindViewById<RadioGroup>(Resource.Id.radioGroupInternet);
            _textViewCaptionInternet.Visibility = viewStateInternet;
            _radioGroupInternet.Visibility = viewStateInternet;

            _radioButtonInternetCellular = FindViewById<RadioButton>(Resource.Id.radioButtonInternetCellular);
            _radioButtonInternetWifi = FindViewById<RadioButton>(Resource.Id.radioButtonInternetWifi);
            _radioButtonInternetEthernet = FindViewById<RadioButton>(Resource.Id.radioButtonInternetEthernet);

            _radioButtonAskForBtEnable = FindViewById<RadioButton>(Resource.Id.radioButtonAskForBtEnable);
            _radioButtonAlwaysEnableBt = FindViewById<RadioButton>(Resource.Id.radioButtonAlwaysEnableBt);
            _radioButtonNoBtHandling = FindViewById<RadioButton>(Resource.Id.radioButtonNoBtHandling);

            _checkBoxDisableBtAtExit = FindViewById<CheckBox>(Resource.Id.checkBoxDisableBtAtExit);

            _radioButtonCommLockNone = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockNone);
            _radioButtonCommLockCpu = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockCpu);
            _radioButtonCommLockDim = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockDim);
            _radioButtonCommLockBright = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockBright);

            _radioButtonLogLockNone = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockNone);
            _radioButtonLogLockCpu = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockCpu);
            _radioButtonLogLockDim = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockDim);
            _radioButtonLogLockBright = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockBright);

            _checkBoxStoreDataLogSettings = FindViewById<CheckBox>(Resource.Id.checkBoxStoreDataLogSettings);

            _radioButtonStartOffline = FindViewById<RadioButton>(Resource.Id.radioButtonStartOffline);
            _radioButtonStartConnect = FindViewById<RadioButton>(Resource.Id.radioButtonStartConnect);
            _radioButtonStartConnectClose = FindViewById<RadioButton>(Resource.Id.radioButtonStartConnectClose);

            _checkBoxDoubleClickForAppExit = FindViewById<CheckBox>(Resource.Id.checkBoxDoubleClickForAppExit);
            _checkBoxSendDataBroadcast = FindViewById<CheckBox>(Resource.Id.checkBoxSendDataBroadcast);

            _radioButtonUpdateOff = FindViewById<RadioButton>(Resource.Id.radioButtonUpdateOff);
            _radioButtonUpdate1Day = FindViewById<RadioButton>(Resource.Id.radioButtonUpdate1Day);
            _radioButtonUpdate1Week = FindViewById<RadioButton>(Resource.Id.radioButtonUpdate1Week);

            _textViewCaptionCpuUsage = FindViewById<TextView>(Resource.Id.textViewCaptionCpuUsage);
            _checkBoxCheckCpuUsage = FindViewById<CheckBox>(Resource.Id.checkBoxCheckCpuUsage);
            ViewStates viewStateCpuUsage = ActivityCommon.IsCpuStatisticsSupported() ? ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionCpuUsage.Visibility = viewStateCpuUsage;
            _checkBoxCheckCpuUsage.Visibility = viewStateCpuUsage;

            _checkBoxCheckEcuFiles = FindViewById<CheckBox>(Resource.Id.checkBoxCheckEcuFiles);
            _checkBoxShowBatteryVoltageWarning = FindViewById<CheckBox>(Resource.Id.checkBoxShowBatteryVoltageWarning);
            _checkBoxOldVagMode = FindViewById<CheckBox>(Resource.Id.checkBoxOldVagMode);
            _checkBoxUseBmwDatabase = FindViewById<CheckBox>(Resource.Id.checkBoxUseBmwDatabase);
            _checkBoxUseBmwDatabase.Click += (sender, args) =>
            {
                UpdateDisplay();
            };

            _checkBoxShowOnlyRelevantErrors = FindViewById<CheckBox>(Resource.Id.checkBoxShowOnlyRelevantErrors);

            _checkBoxScanAllEcus = FindViewById<CheckBox>(Resource.Id.checkBoxScanAllEcus);

            _layoutFileManage = FindViewById<LinearLayout>(Resource.Id.layoutFileManage);
            _layoutFileManage.Visibility = ActivityCommon.IsDocumentTreeSupported() ? ViewStates.Visible : ViewStates.Gone;

            _buttonStorageCopyTreeToApp = FindViewById<Button>(Resource.Id.buttonStorageCopyTreeToApp);
            _buttonStorageCopyTreeToApp.Click += (sender, args) =>
            {
                SelectCopyDocumentTree(false);
            };

            _buttonStorageCopyTreeFromApp = FindViewById<Button>(Resource.Id.buttonStorageCopyTreeFromApp);
            _buttonStorageCopyTreeFromApp.Click += (sender, args) =>
            {
                SelectCopyAppDir(true);
            };

            _buttonStorageDelTreeFromApp = FindViewById<Button>(Resource.Id.buttonStorageDelTreeFromApp);
            _buttonStorageDelTreeFromApp.Click += (sender, args) =>
            {
                SelectDeleteAppDir();
            };

            _buttonStorageLocation = FindViewById<Button>(Resource.Id.buttonStorageLocation);
            _buttonStorageLocation.Click += (sender, args) =>
            {
                SelectMedia();
            };

            ViewStates viewStateNotifications = Build.VERSION.SdkInt >= BuildVersionCodes.O ? ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionNotifications = FindViewById<TextView>(Resource.Id.textViewCaptionNotifications);
            _buttonManageNotifications = FindViewById<Button>(Resource.Id.buttonManageNotifications);
            _buttonManageNotifications.Click += (sender, args) =>
            {
                _activityCommon.ShowNotificationSettings((int) ActivityRequest.RequestNotificationSettings);
            };
            _textViewCaptionNotifications.Visibility = viewStateNotifications;
            _buttonManageNotifications.Visibility = viewStateNotifications;

            _checkBoxCollectDebugInfo = FindViewById<CheckBox>(Resource.Id.checkBoxCollectDebugInfo);

            ViewStates viewStateSnoopLog = _activityCommon.GetConfigHciSnoopLog(out bool _) ? ViewStates.Visible : ViewStates.Gone;
            _checkBoxHciSnoopLog = FindViewById<CheckBox>(Resource.Id.checkBoxHciSnoopLog);
            _checkBoxHciSnoopLog.Visibility = viewStateSnoopLog;
            _checkBoxHciSnoopLog.Enabled = false;

            _buttonHciSnoopLog = FindViewById<Button>(Resource.Id.buttonHciSnoopLog);
            _buttonHciSnoopLog.Visibility = viewStateSnoopLog;
            _buttonHciSnoopLog.Click += (sender, args) =>
            {
                ShowDevelopmentSettings();
            };

            _buttonDefaultSettings = FindViewById<Button>(Resource.Id.buttonDefaultSettings);
            _buttonDefaultSettings.Click += (sender, args) =>
            {
                DefaultSettings();
            };

            _buttonExportSettings = FindViewById<Button>(Resource.Id.buttonExportSettings);
            _buttonExportSettings.Enabled = allowExport;
            _buttonExportSettings.Click += (sender, args) =>
            {
                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (o, eventArgs) =>
                    {
                        ExportSettings(ActivityMain.SettingsMode.Private);
                    })
                    .SetNegativeButton(Resource.String.button_no, (o, eventArgs) =>
                    {
                        ExportSettings(ActivityMain.SettingsMode.Public);
                    })
                    .SetNeutralButton(Resource.String.button_abort, (o, eventArgs) => { })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.settings_export_private)
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
            };

            _buttonImportSettings = FindViewById<Button>(Resource.Id.buttonImportSettings);
            _buttonImportSettings.Enabled = allowImport;
            _buttonImportSettings.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                if (!File.Exists(_exportFileName))
                {
                    string message = GetString(Resource.String.settings_import_no_file) + "\r\n" + _exportFileName;
                    _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                    return;
                }

                ImportSettings();
            };

            ReadSettings();
            CheckSelection(_selection);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressedEvent()
        {
            StoreSettings();
            base.OnBackPressedEvent();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    StoreSettings();
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest) requestCode)
            {
                case ActivityRequest.RequestDevelopmentSettings:
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestNotificationSettings:
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestOpenDocumentTreeToApp:
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        Android.Net.Uri treeUri = data.Data;
                        if (treeUri != null)
                        {
                            _instanceData.CopyToAppSrcUri = treeUri.ToString();
                            if (!string.IsNullOrEmpty(_instanceData.CopyToAppSrcUri))
                            {
                                SelectCopyAppDir(false);
                            }
                        }
                    }
                    break;

                case ActivityRequest.RequestOpenDocumentTreeFromApp:
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        Android.Net.Uri treeUri = data.Data;
                        if (treeUri != null)
                        {
                            _instanceData.CopyFromAppDstUri = treeUri.ToString();
                            if (!string.IsNullOrEmpty(_instanceData.CopyFromAppSrcPath) && !string.IsNullOrEmpty(_instanceData.CopyFromAppDstUri))
                            {
                                try
                                {
                                    DocumentFile srcDir = DocumentFile.FromFile(new Java.IO.File(_instanceData.CopyFromAppSrcPath));
                                    DocumentFile dstDir = DocumentFile.FromTreeUri(this, Android.Net.Uri.Parse(_instanceData.CopyFromAppDstUri));
                                    if (_activityCommon.RequestCopyDocumentsThread(srcDir, dstDir, (result, aborted) =>
                                        {
                                            if (HasSelection())
                                            {
                                                Finish();
                                            }
                                        }))
                                    {
                                        if (HasSelection())
                                        {
                                            break;
                                        }

                                        ActivityCommon.CopyFromAppSrc = _instanceData.CopyFromAppSrcPath;
                                        ActivityCommon.CopyFromAppDst = _instanceData.CopyFromAppDstUri;
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    }

                    if (HasSelection())
                    {
                        Finish();
                    }
                    break;

                case ActivityRequest.RequestSelDirToApp:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        _instanceData.CopyToAppDstPath = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        if (!string.IsNullOrEmpty(_instanceData.CopyToAppSrcUri) && !string.IsNullOrEmpty(_instanceData.CopyToAppDstPath))
                        {
                            try
                            {
                                DocumentFile srcDir = DocumentFile.FromTreeUri(this, Android.Net.Uri.Parse(_instanceData.CopyToAppSrcUri));
                                DocumentFile dstDir = DocumentFile.FromFile(new Java.IO.File(_instanceData.CopyToAppDstPath));
                                if (_activityCommon.RequestCopyDocumentsThread(srcDir, dstDir, (result, aborted) => { }))
                                {
                                    ActivityCommon.CopyToAppDst = _instanceData.CopyToAppDstPath;
                                    ActivityCommon.CopyFromAppDst = _instanceData.CopyFromAppDstUri;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                    break;

                case ActivityRequest.RequestSelDirFromApp:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        _instanceData.CopyFromAppSrcPath = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        if (!string.IsNullOrEmpty(_instanceData.CopyFromAppSrcPath))
                        {
                            SelectCopyDocumentTree(true);
                        }
                    }
                    break;

                case ActivityRequest.RequestSelDirDelApp:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        string delPath = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        if (!string.IsNullOrEmpty(delPath))
                        {
                            try
                            {
                                DocumentFile delDir = DocumentFile.FromFile(new Java.IO.File(delPath));
                                _activityCommon.RequestDeleteDocumentsThread(delDir, (result, aborted) => { });
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                    break;
            }
        }

        private void ReadSettings()
        {
            string locale = ActivityCommon.SelectedLocale ?? string.Empty;
            switch (locale.ToLowerInvariant())
            {
                case "en":
                    _radioButtonLocaleEn.Checked = true;
                    break;

                case "de":
                    _radioButtonLocaleDe.Checked = true;
                    break;

                case "ru":
                    _radioButtonLocaleRu.Checked = true;
                    break;

                default:
                    _radioButtonLocaleDefault.Checked = true;
                    break;
            }

            switch (ActivityCommon.SelectedTheme ?? ActivityCommon.ThemeDefault)
            {
                case ActivityCommon.ThemeType.Light:
                    _radioButtonThemeLight.Checked = true;
                    break;

                default:
                    _radioButtonThemeDark.Checked = true;
                    break;
            }

            switch (ActivityCommon.SelectedTranslator)
            {
                case ActivityCommon.TranslatorType.IbmWatson:
                    _radioButtonTranslatorIbm.Checked = true;
                    break;

                default:
                    _radioButtonTranslatorYandex.Checked = true;
                    break;
            }

            _checkBoxTranslatorLogin.Checked = ActivityCommon.EnableTranslateLogin;

            _checkBoxAutoHideTitleBar.Checked = ActivityCommon.AutoHideTitleBar;
            _checkBoxSuppressTitleBar.Checked = ActivityCommon.SuppressTitleBar;
            _checkBoxFullScreenMode.Checked = ActivityCommon.FullScreenMode;

            _checkBoxSwapMultiWindowOrientation.Checked = ActivityCommon.SwapMultiWindowOrientation;

            switch (ActivityCommon.SelectedInternetConnection)
            {
                case ActivityCommon.InternetConnectionType.Wifi:
                    _radioButtonInternetWifi.Checked = true;
                    break;

                case ActivityCommon.InternetConnectionType.Ethernet:
                    _radioButtonInternetEthernet.Checked = true;
                    break;

                default:
                    _radioButtonInternetCellular.Checked = true;
                    break;
            }

            switch (ActivityCommon.BtEnbaleHandling)
            {
                case ActivityCommon.BtEnableType.Ask:
                    _radioButtonAskForBtEnable.Checked = true;
                    break;

                case ActivityCommon.BtEnableType.Always:
                    _radioButtonAlwaysEnableBt.Checked = true;
                    break;

                default:
                    _radioButtonNoBtHandling.Checked = true;
                    break;
            }

            _checkBoxDisableBtAtExit.Enabled = Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu;
            _checkBoxDisableBtAtExit.Checked = ActivityCommon.BtDisableHandling == ActivityCommon.BtDisableType.DisableIfByApp;

            switch (ActivityCommon.LockTypeCommunication)
            {
                case ActivityCommon.LockType.None:
                    _radioButtonCommLockNone.Checked = true;
                    break;

                case ActivityCommon.LockType.Cpu:
                    _radioButtonCommLockCpu.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenDim:
                    _radioButtonCommLockDim.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenBright:
                    _radioButtonCommLockBright.Checked = true;
                    break;
            }

            switch (ActivityCommon.LockTypeLogging)
            {
                case ActivityCommon.LockType.None:
                    _radioButtonLogLockNone.Checked = true;
                    break;

                case ActivityCommon.LockType.Cpu:
                    _radioButtonLogLockCpu.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenDim:
                    _radioButtonLogLockDim.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenBright:
                    _radioButtonLogLockBright.Checked = true;
                    break;
            }

            _checkBoxStoreDataLogSettings.Checked = ActivityCommon.StoreDataLogSettings;

            switch (ActivityCommon.AutoConnectHandling)
            {
                case ActivityCommon.AutoConnectType.Offline:
                    _radioButtonStartOffline.Checked = true;
                    break;

                case ActivityCommon.AutoConnectType.Connect:
                    _radioButtonStartConnect.Checked = true;
                    break;

                case ActivityCommon.AutoConnectType.ConnectClose:
                    _radioButtonStartConnectClose.Checked = true;
                    break;
            }

            _checkBoxDoubleClickForAppExit.Checked = ActivityCommon.DoubleClickForAppExit;

            if (ActivityCommon.UpdateCheckDelay == TimeSpan.TicksPerDay * 7)
            {
                _radioButtonUpdate1Week.Checked = true;
            }
            else if (ActivityCommon.UpdateCheckDelay < 0)
            {
                _radioButtonUpdateOff.Checked = true;
            }
            else
            {
                _radioButtonUpdate1Day.Checked = true;
            }

            _checkBoxSendDataBroadcast.Checked = ActivityCommon.SendDataBroadcast;
            _checkBoxCheckCpuUsage.Checked = ActivityCommon.CheckCpuUsage;
            _checkBoxCheckEcuFiles.Checked = ActivityCommon.CheckEcuFiles;
            _checkBoxShowBatteryVoltageWarning.Checked = ActivityCommon.ShowBatteryVoltageWarning;
            _checkBoxOldVagMode.Checked = ActivityCommon.OldVagMode;
            _checkBoxUseBmwDatabase.Checked = ActivityCommon.UseBmwDatabase;
            _checkBoxShowOnlyRelevantErrors.Checked = ActivityCommon.ShowOnlyRelevantErrors;
            _checkBoxScanAllEcus.Checked = ActivityCommon.ScanAllEcus;
            _checkBoxCollectDebugInfo.Checked = ActivityCommon.CollectDebugInfo;
            UpdateDisplay();
        }

        private void StoreSettings()
        {
            string locale = ActivityCommon.SelectedLocale ?? string.Empty;
            if (_radioButtonLocaleEn.Checked)
            {
                locale = "en";
            }
            else if (_radioButtonLocaleDe.Checked)
            {
                locale = "de";
            }
            else if (_radioButtonLocaleRu.Checked)
            {
                locale = "ru";
            }
            else if (_radioButtonLocaleDefault.Checked)
            {
                locale = string.Empty;
            }
            ActivityCommon.SelectedLocale = locale;

            ActivityCommon.ThemeType themeType = ActivityCommon.SelectedTheme ?? ActivityCommon.ThemeDefault;
            if (_radioButtonThemeLight.Checked)
            {
                themeType = ActivityCommon.ThemeType.Light;
            }
            else if (_radioButtonThemeDark.Checked)
            {
                themeType = ActivityCommon.ThemeType.Dark;
            }
            ActivityCommon.SelectedTheme = themeType;

            ActivityCommon.TranslatorType translatorType = ActivityCommon.SelectedTranslator;
            if (_radioButtonTranslatorYandex.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.YandexTranslate;
            }
            else if (_radioButtonTranslatorIbm.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.IbmWatson;
            }
            _activityCommon.Translator = translatorType;

            ActivityCommon.EnableTranslateLogin = _checkBoxTranslatorLogin.Checked;

            ActivityCommon.AutoHideTitleBar = _checkBoxAutoHideTitleBar.Checked;
            ActivityCommon.SuppressTitleBar = _checkBoxSuppressTitleBar.Checked;
            ActivityCommon.FullScreenMode = _checkBoxFullScreenMode.Checked;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_checkBoxSwapMultiWindowOrientation.Visibility == ViewStates.Visible)
            {
                ActivityCommon.SwapMultiWindowOrientation = _checkBoxSwapMultiWindowOrientation.Checked;
            }
            else
            {
                ActivityCommon.SwapMultiWindowOrientation = false;
            }

            ActivityCommon.InternetConnectionType internetConnectionType = ActivityCommon.SelectedInternetConnection;
            if (_radioGroupInternet.Visibility == ViewStates.Visible)
            {
                if (_radioButtonInternetCellular.Checked)
                {
                    internetConnectionType = ActivityCommon.InternetConnectionType.Cellular;
                }
                else if (_radioButtonInternetWifi.Checked)
                {
                    internetConnectionType = ActivityCommon.InternetConnectionType.Wifi;
                }
                else if (_radioButtonInternetEthernet.Checked)
                {
                    internetConnectionType = ActivityCommon.InternetConnectionType.Ethernet;
                }
            }
            else
            {
                internetConnectionType = ActivityCommon.InternetConnectionType.Cellular;
            }
            ActivityCommon.SelectedInternetConnection = internetConnectionType;

            ActivityCommon.BtEnableType enableType = ActivityCommon.BtEnbaleHandling;
            if (_radioButtonAskForBtEnable.Checked)
            {
                enableType = ActivityCommon.BtEnableType.Ask;
            }
            else if (_radioButtonAlwaysEnableBt.Checked)
            {
                enableType = ActivityCommon.BtEnableType.Always;
            }
            else if (_radioButtonNoBtHandling.Checked)
            {
                enableType = ActivityCommon.BtEnableType.Nothing;
            }
            ActivityCommon.BtEnbaleHandling = enableType;

            ActivityCommon.BtDisableHandling = _checkBoxDisableBtAtExit.Checked ? ActivityCommon.BtDisableType.DisableIfByApp : ActivityCommon.BtDisableType.Nothing;

            ActivityCommon.LockType lockType = ActivityCommon.LockTypeCommunication;
            if (_radioButtonCommLockNone.Checked)
            {
                lockType = ActivityCommon.LockType.None;
            }
            else if(_radioButtonCommLockCpu.Checked)
            {
                lockType = ActivityCommon.LockType.Cpu;
            }
            else if (_radioButtonCommLockDim.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenDim;
            }
            else if (_radioButtonCommLockBright.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenBright;
            }
            ActivityCommon.LockTypeCommunication = lockType;

            lockType = ActivityCommon.LockTypeLogging;
            if (_radioButtonLogLockNone.Checked)
            {
                lockType = ActivityCommon.LockType.None;
            }
            else if (_radioButtonLogLockCpu.Checked)
            {
                lockType = ActivityCommon.LockType.Cpu;
            }
            else if (_radioButtonLogLockDim.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenDim;
            }
            else if (_radioButtonLogLockBright.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenBright;
            }
            ActivityCommon.LockTypeLogging = lockType;

            ActivityCommon.StoreDataLogSettings = _checkBoxStoreDataLogSettings.Checked;

            ActivityCommon.AutoConnectType autoConnectType = ActivityCommon.AutoConnectType.Offline;
            if (_radioButtonStartOffline.Checked)
            {
                autoConnectType = ActivityCommon.AutoConnectType.Offline;
            }
            else if (_radioButtonStartConnect.Checked)
            {
                autoConnectType = ActivityCommon.AutoConnectType.Connect;
            }
            else if (_radioButtonStartConnectClose.Checked)
            {
                autoConnectType = ActivityCommon.AutoConnectType.ConnectClose;
            }
            ActivityCommon.AutoConnectHandling = autoConnectType;

            ActivityCommon.DoubleClickForAppExit = _checkBoxDoubleClickForAppExit.Checked;

            long updateCheckDelay = ActivityCommon.UpdateCheckDelayDefault;
            if (_radioButtonUpdateOff.Checked)
            {
                updateCheckDelay = -1;
            }
            else if (_radioButtonUpdate1Day.Checked)
            {
                updateCheckDelay = TimeSpan.TicksPerDay;
            }
            else if (_radioButtonUpdate1Week.Checked)
            {
                updateCheckDelay = TimeSpan.TicksPerDay * 7;
            }
            ActivityCommon.UpdateCheckDelay = updateCheckDelay;

            ActivityCommon.SendDataBroadcast = _checkBoxSendDataBroadcast.Checked;
            ActivityCommon.CheckCpuUsage = _checkBoxCheckCpuUsage.Checked;
            ActivityCommon.CheckEcuFiles = _checkBoxCheckEcuFiles.Checked;
            ActivityCommon.ShowBatteryVoltageWarning = _checkBoxShowBatteryVoltageWarning.Checked;
            ActivityCommon.OldVagMode = _checkBoxOldVagMode.Checked;
            ActivityCommon.UseBmwDatabase = _checkBoxUseBmwDatabase.Checked;
            ActivityCommon.ShowOnlyRelevantErrors = _checkBoxShowOnlyRelevantErrors.Checked;
            ActivityCommon.ScanAllEcus = _checkBoxScanAllEcus.Checked;
            ActivityCommon.CollectDebugInfo = _checkBoxCollectDebugInfo.Checked;
        }

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }

            string displayName = GetString(Resource.String.default_media);
            if (!string.IsNullOrEmpty(ActivityCommon.CustomStorageMedia))
            {
                string shortName = ActivityCommon.GetTruncatedPathName(ActivityCommon.CustomStorageMedia);
                if (!string.IsNullOrEmpty(shortName))
                {
                    displayName = shortName;
                }
            }
            _buttonStorageLocation.Text = displayName;

            bool snoopLogEnabled = false;
            if (_activityCommon.GetConfigHciSnoopLog(out bool enabledConfig))
            {
                snoopLogEnabled = enabledConfig;
            }
            if (ActivityCommon.ReadHciSnoopLogSettings(out bool enabledSettings, out string logFileName))
            {
                if (!enabledSettings)
                {
                    snoopLogEnabled = false;
                }
            }
            _checkBoxHciSnoopLog.Checked = snoopLogEnabled;
            _checkBoxHciSnoopLog.Text = string.Format(GetString(Resource.String.settings_hci_snoop_log), logFileName ?? "-");

            _checkBoxShowOnlyRelevantErrors.Visibility = _checkBoxUseBmwDatabase.Checked ? ViewStates.Visible : ViewStates.Gone;
        }

        private void SelectMedia()
        {
            // ReSharper disable once UseNullPropagation
            if (_activityCommon == null)
            {
                return;
            }
            _activityCommon.SelectMedia((s, a) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                UpdateDisplay();
            });
        }

        private void CheckSelection(string selection)
        {
            if (selection == null)
            {
                return;
            }
            switch (selection)
            {
                case SelectionStorageLocation:
                    SelectMedia();
                    break;

                case SelectionCopyFromApp:
                    _instanceData.CopyFromAppSrcPath = _copyFileName;
                    if (!SelectCopyDocumentTree(true))
                    {
                        Finish();
                    }
                    break;
            }
        }

        private bool HasSelection()
        {
            return !string.IsNullOrEmpty(_selection);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
            private bool ShowDevelopmentSettings()
        {
            try
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionApplicationDevelopmentSettings);
                StartActivityForResult(intent, (int)ActivityRequest.RequestDevelopmentSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool SelectCopyDocumentTree(bool fromApp)
        {
            if (!ActivityCommon.IsDocumentTreeSupported())
            {
                return false;
            }

            try
            {
                ActivityRequest request;
                Intent intent = new Intent(Intent.ActionOpenDocumentTree);
                intent.PutExtra(Intent.ExtraTitle, GetString(Resource.String.settings_storage_sel_public_dir));
                if (fromApp)
                {
                    request = ActivityRequest.RequestOpenDocumentTreeFromApp;
                    if (!string.IsNullOrEmpty(_instanceData.CopyFromAppDstUri))
                    {
                        intent.PutExtra(Android.Provider.DocumentsContract.ExtraInitialUri, _instanceData.CopyFromAppDstUri);
                    }
                }
                else
                {
                    request = ActivityRequest.RequestOpenDocumentTreeToApp;
                    if (!string.IsNullOrEmpty(_instanceData.CopyToAppSrcUri))
                    {
                        intent.PutExtra(Android.Provider.DocumentsContract.ExtraInitialUri, _instanceData.CopyToAppSrcUri);
                    }
                }

                StartActivityForResult(intent, (int)request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SelectCopyAppDir(bool fromApp)
        {
            ActivityRequest request;
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = string.Empty;
            if (fromApp)
            {
                request = ActivityRequest.RequestSelDirFromApp;
                if (!string.IsNullOrEmpty(_instanceData.CopyFromAppSrcPath))
                {
                    initDir = _instanceData.CopyFromAppSrcPath;
                }
            }
            else
            {
                request = ActivityRequest.RequestSelDirToApp;
                if (!string.IsNullOrEmpty(_instanceData.CopyToAppDstPath))
                {
                    initDir = _instanceData.CopyToAppDstPath;
                }
            }

            if (!string.IsNullOrEmpty(initDir))
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(initDir);
                    if (!attr.HasFlag(FileAttributes.Directory))
                    {
                        initDir = Path.GetDirectoryName(initDir);
                    }
                }
                catch (Exception)
                {
                    initDir = string.Empty;
                }
            }

            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.settings_storage_sel_app_dir));
            serverIntent.PutExtra(FilePickerActivity.ExtraDirSelect, true);
            serverIntent.PutExtra(FilePickerActivity.ExtraShowCurrentDir, true);
            serverIntent.PutExtra(FilePickerActivity.ExtraShowFiles, fromApp);
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            StartActivityForResult(serverIntent, (int)request);
        }

        private void SelectDeleteAppDir()
        {
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = string.Empty;
            if (!string.IsNullOrEmpty(_instanceData.CopyFromAppSrcPath) && File.Exists(_instanceData.CopyFromAppSrcPath))
            {
                initDir = _instanceData.CopyFromAppSrcPath;
            }

            if (!string.IsNullOrEmpty(initDir))
            {
                try
                {
                    FileAttributes attr = File.GetAttributes(initDir);
                    if (!attr.HasFlag(FileAttributes.Directory))
                    {
                        initDir = Path.GetDirectoryName(initDir);
                    }
                }
                catch (Exception)
                {
                    initDir = string.Empty;
                }
            }

            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.settings_storage_sel_app_dir));
            serverIntent.PutExtra(FilePickerActivity.ExtraDirSelect, true);
            serverIntent.PutExtra(FilePickerActivity.ExtraShowCurrentDir, true);
            serverIntent.PutExtra(FilePickerActivity.ExtraShowFiles, true);
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelDirDelApp);
        }

        private void DefaultSettings()
        {
            _activityCommon.SetDefaultSettings(true, true);
            ReadSettings();
        }

        private void ExportSettings(ActivityMain.SettingsMode settingsMode)
        {
            Intent intent = new Intent();
            intent.PutExtra(ExtraExportFile, _exportFileName);
            intent.PutExtra(ExtraSettingsMode, (int)settingsMode);
            SetResult(Android.App.Result.Ok, intent);
            Finish();
        }

        private void ImportSettings()
        {
            Intent intent = new Intent();
            intent.PutExtra(ExtraImportFile, _exportFileName);
            SetResult(Android.App.Result.Ok, intent);
            Finish();
        }
    }
}
