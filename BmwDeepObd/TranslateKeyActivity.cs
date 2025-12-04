using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/trans_api_key_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(TranslateKeyActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class TranslateKeyActivity : BaseActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public InstanceData()
            {
                OldTranslator = string.Empty;
                OldYandexApiKey = string.Empty;
                OldIbmTranslatorApiKey = string.Empty;
                OldIbmTranslatorUrl = string.Empty;
                OldDeeplApiKey = string.Empty;
                OldYandexCloudApiKey = string.Empty;
                OldYandexCloudFolderId = string.Empty;
                OldGoogleApisUrl = string.Empty;
            }

            public string OldTranslator { get; set; }
            public string OldYandexApiKey { get; set; }
            public string OldIbmTranslatorApiKey { get; set; }
            public string OldIbmTranslatorUrl { get; set; }
            public string OldDeeplApiKey { get; set; }
            public string OldYandexCloudApiKey { get; set; }
            public string OldYandexCloudFolderId { get; set; }
            public string OldGoogleApisUrl { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private InputMethodManager _imm;
        private Timer _clipboardCheckTimer;
        private ActivityCommon _activityCommon;
        private View _contentView;
        private TextView _textViewCaptionTranslator;
        private RadioButton _radioButtonTranslatorYandexCloud;
        private RadioButton _radioButtonTranslatorYandexTranslate;
        private RadioButton _radioButtonTranslatorIbm;
        private RadioButton _radioButtonTranslatorDeepl;
        private RadioButton _radioButtonTranslatorGoogleApis;
        private TextView _textViewTransKeyDesc;
        private LinearLayout _layoutYandexKey;
        private TextView _textViewTransKeyCopyTitle;
        private Button _buttonYandexApiKeyCreate;
        private Button _buttonYandexApiKeyGet;
        private TextView _textViewYandexKeyPasteTitle;
        private Button _buttonYandexApiKeyPaste;
        private EditText _editTextYandexApiKey;
        private TextView _textViewFolderIdPasteTitle;
        private Button _buttonFolderIdPaste;
        private EditText _editTextFolderId;
        private TextView _textViewApiUrlPasteTitle;
        private Button _buttonApiUrlCopy;
        private Button _buttonApiUrlPaste;
        private EditText _editTextApiUrl;
        private Button _buttonYandexApiKeyTest;
        private TextView _textViewYandexApiKeyTestResult;
        private bool _ignoreChange;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme();
            base.OnCreate(savedInstanceState);
            _allowTitleHiding = false;

            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.translate_key_select);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            if (!_activityRecreated)
            {
                _instanceData.OldTranslator = ActivityCommon.SelectedTranslator.ToString();
                _instanceData.OldYandexApiKey = ActivityCommon.YandexApiKey ?? string.Empty;
                _instanceData.OldIbmTranslatorApiKey = ActivityCommon.IbmTranslatorApiKey ?? string.Empty;
                _instanceData.OldIbmTranslatorUrl = ActivityCommon.IbmTranslatorUrl ?? string.Empty;
                _instanceData.OldDeeplApiKey = ActivityCommon.DeeplApiKey ?? string.Empty;
                _instanceData.OldYandexCloudApiKey = ActivityCommon.YandexCloudApiKey ?? string.Empty;
                _instanceData.OldYandexCloudFolderId = ActivityCommon.YandexCloudFolderId ?? string.Empty;
                _instanceData.OldGoogleApisUrl = ActivityCommon.GoogleApisUrl ?? string.Empty;
            }

            _activityCommon = new ActivityCommon(this);

            _textViewCaptionTranslator = FindViewById<TextView>(Resource.Id.textViewCaptionTranslator);

            _radioButtonTranslatorYandexCloud = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorYandexCloud);
            _radioButtonTranslatorYandexCloud.CheckedChange += TranslatorChanged;

            _radioButtonTranslatorYandexTranslate = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorYandexTranslate);
            _radioButtonTranslatorYandexTranslate.CheckedChange += TranslatorChanged;

            _radioButtonTranslatorIbm = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorIbm);
            _radioButtonTranslatorIbm.CheckedChange += TranslatorChanged;
            _radioButtonTranslatorIbm.Visibility =
                ActivityCommon.SelectedTranslator == ActivityCommon.TranslatorType.IbmWatson ? ViewStates.Visible : ViewStates.Gone;

            _radioButtonTranslatorDeepl = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorDeepl);
            _radioButtonTranslatorDeepl.CheckedChange += TranslatorChanged;

            _radioButtonTranslatorGoogleApis = FindViewById<RadioButton>(Resource.Id.radioButtonTranslatorGoogleApis);
            _radioButtonTranslatorGoogleApis.CheckedChange += TranslatorChanged;

            _textViewTransKeyDesc = FindViewById<TextView>(Resource.Id.textViewTransKeyDesc);

            _layoutYandexKey = FindViewById<LinearLayout>(Resource.Id.layoutYandexKey);
            _layoutYandexKey.SetOnTouchListener(this);

            _editTextYandexApiKey = FindViewById<EditText>(Resource.Id.editTextYandexApiKey);

            _textViewFolderIdPasteTitle = FindViewById<TextView>(Resource.Id.textViewFolderIdPasteTitle);
            _editTextFolderId = FindViewById<EditText>(Resource.Id.editTextFolderId);

            _textViewApiUrlPasteTitle = FindViewById<TextView>(Resource.Id.textViewApiUrlPasteTitle);
            _editTextApiUrl = FindViewById<EditText>(Resource.Id.editTextApiUrl);

            _textViewTransKeyCopyTitle = FindViewById<TextView>(Resource.Id.textViewTransKeyCopyTitle);

            _buttonYandexApiKeyCreate = FindViewById<Button>(Resource.Id.buttonYandexKeyCreate);
            _buttonYandexApiKeyCreate.SetOnTouchListener(this);
            _buttonYandexApiKeyCreate.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _activityCommon.ShowWifiConnectedWarning(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.YandexTranslate:
                            _activityCommon.OpenWebUrl("https://translate.yandex.com/developers/keys");
                            break;

                        case ActivityCommon.TranslatorType.IbmWatson:
                            _activityCommon.OpenWebUrl("https://cloud.ibm.com/catalog/services/language-translator");
                            break;

                        case ActivityCommon.TranslatorType.Deepl:
                            _activityCommon.OpenWebUrl("https://www.deepl.com/account");
                            break;

                        case ActivityCommon.TranslatorType.YandexCloud:
                            _activityCommon.OpenWebUrl("https://cloud.yandex.com/en/docs/translate/");
                            break;
                    }
                });
            };

            _buttonYandexApiKeyGet = FindViewById<Button>(Resource.Id.buttonYandexKeyGet);
            _buttonYandexApiKeyGet.SetOnTouchListener(this);
            _buttonYandexApiKeyGet.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _activityCommon.ShowWifiConnectedWarning(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.YandexTranslate:
                            _activityCommon.OpenWebUrl("https://translate.yandex.com/developers/keys");
                            break;

                        case ActivityCommon.TranslatorType.IbmWatson:
                            _activityCommon.OpenWebUrl("https://cloud.ibm.com/resources");
                            break;

                        case ActivityCommon.TranslatorType.Deepl:
                            _activityCommon.OpenWebUrl("https://www.deepl.com/de/account/summary");
                            break;

                        case ActivityCommon.TranslatorType.YandexCloud:
                            _activityCommon.OpenWebUrl("https://cloud.yandex.com/en/docs/iam/operations/iam-token/create");
                            break;
                    }
                });
            };

            _textViewYandexKeyPasteTitle = FindViewById<TextView>(Resource.Id.textViewYandexKeyPasteTitle);

            _buttonYandexApiKeyPaste = FindViewById<Button>(Resource.Id.buttonYandexKeyPaste);
            _buttonYandexApiKeyPaste.SetOnTouchListener(this);
            _buttonYandexApiKeyPaste.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                string clipText = _activityCommon.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(clipText))
                {
                    try
                    {
                        _ignoreChange = true;
                        _editTextYandexApiKey.Text = clipText.Trim();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    finally
                    {
                        _ignoreChange = false;
                    }

                    UpdateButtonState();
                }
            };
            _buttonYandexApiKeyPaste.TextChanged += (sender, args) =>
            {
                if (_ignoreChange)
                {
                    return;
                }

                UpdateButtonState();
            };

            _buttonFolderIdPaste = FindViewById<Button>(Resource.Id.buttonFolderIdPaste);
            _buttonFolderIdPaste.SetOnTouchListener(this);
            _buttonFolderIdPaste.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                string clipText = _activityCommon.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(clipText))
                {
                    try
                    {
                        _ignoreChange = true;
                        _editTextFolderId.Text = clipText.Trim();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    finally
                    {
                        _ignoreChange = false;
                    }

                    UpdateButtonState();
                }
            };
            _buttonFolderIdPaste.TextChanged += (sender, args) =>
            {
                if (_ignoreChange)
                {
                    return;
                }

                UpdateButtonState();
            };

            _buttonApiUrlCopy = FindViewById<Button>(Resource.Id.buttonApiUrlCopy);
            _buttonApiUrlCopy.SetOnTouchListener(this);
            _buttonApiUrlCopy.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _activityCommon.ShowWifiConnectedWarning(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.GoogleApis:
                            _activityCommon.OpenWebUrl("https://github.com/uholeschak/ediabaslib/blob/master/docs/Deep_OBD_for_BMW_and_VAG.md#google-translation-apis");
                            break;
                    }
                });
            };

            _buttonApiUrlPaste = FindViewById<Button>(Resource.Id.buttonApiUrlPaste);
            _buttonApiUrlPaste.SetOnTouchListener(this);
            _buttonApiUrlPaste.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                string clipText = _activityCommon.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(clipText))
                {
                    try
                    {
                        _ignoreChange = true;
                        _editTextApiUrl.Text = clipText.Trim();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    finally
                    {
                        _ignoreChange = false;
                    }

                    UpdateButtonState();
                }
            };
            _buttonApiUrlPaste.TextChanged += (sender, args) =>
            {
                if (_ignoreChange)
                {
                    return;
                }

                UpdateButtonState();
            };

            _textViewYandexApiKeyTestResult = FindViewById<TextView>(Resource.Id.textViewYandexKeyTestResult);

            _buttonYandexApiKeyTest = FindViewById<Button>(Resource.Id.buttonYandexKeyTest);
            _buttonYandexApiKeyTest.SetOnTouchListener(this);
            _buttonYandexApiKeyTest.Click += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                UpdateSettings();
                _textViewYandexApiKeyTestResult.Text = string.Empty;

                if (!_activityCommon.TranslateStrings(new List<string>
                    {
                        "Dieser Text wurde erfolgreich \x00fcbersetzt.",
                        "Es werden auch mehrere Zeilen gleichzeitig \x00fcbersetzt.",
                        "Hier folgt eine weitere Zeile. Dies besteht aus zwei Teilen.",
                    }, list =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (list != null && list.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (string s in list)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append("\r\n");
                            }
                            sb.Append(s);
                        }
                        _textViewYandexApiKeyTestResult.Text = sb.ToString();
                    }
                    else
                    {
                        _textViewYandexApiKeyTestResult.Text = GetString(Resource.String.button_api_key_test_failed);
                    }
                }, true))
                {
                    _textViewYandexApiKeyTestResult.Text = GetString(Resource.String.button_api_key_test_failed);
                }
            };

            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DisposeTimer();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        protected override void OnResume()
        {
            base.OnResume();
            UpdateButtonState();
            if (_clipboardCheckTimer == null)
            {
                _clipboardCheckTimer = new Timer(state =>
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        UpdateButtonState();
                    });
                }, null, 1000, 1000);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            DisposeTimer();
        }

        public override void OnBackPressedEvent()
        {
            if (StoreYandexKey((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                base.OnBackPressedEvent();
            }))
            {
                base.OnBackPressedEvent();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (StoreYandexKey((sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        Finish();
                    }))
                    {
                        Finish();
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool IsFinishAllowed()
        {
            if (_activityCommon == null)
            {
                return true;
            }

            if (_activityCommon.TranslateActive)
            {
                return false;
            }

            return true;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    HideKeyboard();
                    break;
            }
            return false;
        }

        private void DisposeTimer()
        {
            if (_clipboardCheckTimer != null)
            {
                _clipboardCheckTimer.Dispose();
                _clipboardCheckTimer = null;
            }
        }

        private void TranslatorChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (_ignoreChange)
            {
                return;
            }

            UpdateTranslatorType();
            UpdateDisplay();
        }

        private void UpdateButtonState()
        {
            if (_activityCommon == null)
            {
                return;
            }

            try
            {
                _ignoreChange = true;
                StringBuilder sbDescription = new StringBuilder();
                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.GoogleApis:
                        sbDescription.Append(string.Format(GetString(Resource.String.trans_public_desc), _activityCommon.TranslatorName()));
                        break;

                    default:
                        sbDescription.Append(string.Format(GetString(Resource.String.trans_key_desc), _activityCommon.TranslatorName()));
                        break;
                }

                sbDescription.Append("\r\n");
                sbDescription.Append(GetString(Resource.String.trans_copy_hint));

                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.YandexCloud:
                        sbDescription.Append("\r\n");
                        sbDescription.Append(GetString(Resource.String.trans_key_yandex_cloud));
                        break;
                }

                _textViewTransKeyDesc.Text = sbDescription.ToString();

                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.IbmWatson:
                        _radioButtonTranslatorIbm.Checked = true;
                        break;

                    case ActivityCommon.TranslatorType.Deepl:
                        _radioButtonTranslatorDeepl.Checked = true;
                        break;

                    case ActivityCommon.TranslatorType.YandexCloud:
                        _radioButtonTranslatorYandexCloud.Checked = true;
                        break;

                    case ActivityCommon.TranslatorType.GoogleApis:
                        _radioButtonTranslatorGoogleApis.Checked = true;
                        break;

                    default:
                        _radioButtonTranslatorYandexTranslate.Checked = true;
                        break;
                }

                bool folderIdVisible = false;
                if (ActivityCommon.SelectedTranslator == ActivityCommon.TranslatorType.YandexCloud)
                {
                    folderIdVisible = ActivityCommon.IsYandexCloudOauthToken(_editTextYandexApiKey.Text);
                }

                _textViewFolderIdPasteTitle.Visibility = folderIdVisible ? ViewStates.Visible : ViewStates.Gone;
                _editTextFolderId.Visibility = folderIdVisible ? ViewStates.Visible : ViewStates.Gone;
                _buttonFolderIdPaste.Visibility = folderIdVisible ? ViewStates.Visible : ViewStates.Gone;

                bool apiUrlVisible = false;
                bool apiUrlCopyVisible = false;
                bool apiKeyVisible = true;

                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.IbmWatson:
                        apiUrlVisible = true;
                        break;

                    case ActivityCommon.TranslatorType.GoogleApis:
                        apiUrlVisible = true;
                        apiUrlCopyVisible = true;
                        apiKeyVisible = false;
                        break;
                }

                _textViewApiUrlPasteTitle.Visibility = apiUrlVisible ? ViewStates.Visible : ViewStates.Gone;
                _editTextApiUrl.Visibility = apiUrlVisible ? ViewStates.Visible : ViewStates.Gone;
                _buttonApiUrlCopy.Visibility = apiUrlVisible ? ViewStates.Visible : ViewStates.Gone;
                _buttonApiUrlPaste.Visibility = apiUrlCopyVisible ? ViewStates.Visible : ViewStates.Gone;

                _textViewTransKeyCopyTitle.Visibility = apiKeyVisible ? ViewStates.Visible : ViewStates.Gone;
                _buttonYandexApiKeyCreate.Visibility = apiKeyVisible ? ViewStates.Visible : ViewStates.Gone;
                _buttonYandexApiKeyGet.Visibility = apiKeyVisible ? ViewStates.Visible : ViewStates.Gone;
                _textViewYandexKeyPasteTitle.Visibility = apiKeyVisible ? ViewStates.Visible : ViewStates.Gone;
                _buttonYandexApiKeyPaste.Visibility = apiKeyVisible ? ViewStates.Visible : ViewStates.Gone;
                _editTextYandexApiKey.Visibility = apiKeyVisible ? ViewStates.Visible : ViewStates.Gone;

                bool pasteEnable = false;
                string clipText = _activityCommon.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(clipText))
                {
                    pasteEnable = true;
                }

                _buttonYandexApiKeyPaste.Enabled = pasteEnable;

                bool testEnabled = true;
                if (apiKeyVisible)
                {
                    testEnabled = !string.IsNullOrWhiteSpace(_editTextYandexApiKey.Text);
                }

                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.IbmWatson:
                        if (string.IsNullOrWhiteSpace(_editTextApiUrl.Text))
                        {
                            testEnabled = false;
                        }
                        break;

                    case ActivityCommon.TranslatorType.YandexCloud:
                        if (folderIdVisible)
                        {
                            if (string.IsNullOrWhiteSpace(_editTextFolderId.Text))
                            {
                                testEnabled = false;
                            }
                        }
                        break;

                    case ActivityCommon.TranslatorType.GoogleApis:
                        if (string.IsNullOrWhiteSpace(_editTextApiUrl.Text))
                        {
                            testEnabled = false;
                        }
                        break;
                }

                _buttonYandexApiKeyTest.Enabled = testEnabled;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _ignoreChange = false;
            }
        }

        private void UpdateApiText()
        {
            if (_activityCommon == null)
            {
                return;
            }

            try
            {
                _ignoreChange = true;
                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.IbmWatson:
                        _editTextYandexApiKey.Text = ActivityCommon.IbmTranslatorApiKey;
                        _editTextApiUrl.Text = ActivityCommon.IbmTranslatorUrl;
                        break;

                    case ActivityCommon.TranslatorType.Deepl:
                        _editTextYandexApiKey.Text = ActivityCommon.DeeplApiKey;
                        _editTextApiUrl.Text = string.Empty;
                        break;

                    case ActivityCommon.TranslatorType.YandexCloud:
                        _editTextYandexApiKey.Text = ActivityCommon.YandexCloudApiKey;
                        _editTextFolderId.Text = ActivityCommon.YandexCloudFolderId;
                        break;

                    case ActivityCommon.TranslatorType.GoogleApis:
                        _editTextApiUrl.Text = ActivityCommon.GoogleApisUrl;
                        break;

                    default:
                        _editTextYandexApiKey.Text = ActivityCommon.YandexApiKey;
                        _editTextApiUrl.Text = string.Empty;
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _ignoreChange = false;
            }
        }

        private void UpdateDisplay()
        {
            UpdateApiText();
            UpdateButtonState();
        }

        private void UpdateSettings()
        {
            UpdateTranslatorKeys();
            UpdateTranslatorType();
        }

        private void UpdateTranslatorKeys()
        {
            if (_activityCommon == null)
            {
                return;
            }

            switch (ActivityCommon.SelectedTranslator)
            {
                case ActivityCommon.TranslatorType.YandexTranslate:
                    ActivityCommon.YandexApiKey = _editTextYandexApiKey.Text.Trim();
                    break;

                case ActivityCommon.TranslatorType.IbmWatson:
                    ActivityCommon.IbmTranslatorApiKey = _editTextYandexApiKey.Text.Trim();
                    ActivityCommon.IbmTranslatorUrl = _editTextApiUrl.Text.Trim();
                    break;

                case ActivityCommon.TranslatorType.Deepl:
                    ActivityCommon.DeeplApiKey = _editTextYandexApiKey.Text.Trim();
                    break;

                case ActivityCommon.TranslatorType.YandexCloud:
                    ActivityCommon.YandexCloudApiKey = _editTextYandexApiKey.Text.Trim();
                    ActivityCommon.YandexCloudFolderId = _editTextFolderId.Text.Trim();
                    break;

                case ActivityCommon.TranslatorType.GoogleApis:
                    ActivityCommon.GoogleApisUrl = _editTextApiUrl.Text.Trim();
                    break;
            }
        }

        private void UpdateTranslatorType()
        {
            if (_activityCommon == null)
            {
                return;
            }

            ActivityCommon.TranslatorType translatorType = ActivityCommon.SelectedTranslator;
            if (_radioButtonTranslatorYandexTranslate.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.YandexTranslate;
            }
            else if (_radioButtonTranslatorIbm.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.IbmWatson;
            }
            else if (_radioButtonTranslatorDeepl.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.Deepl;
            }
            else if (_radioButtonTranslatorYandexCloud.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.YandexCloud;
            }
            else if (_radioButtonTranslatorGoogleApis.Checked)
            {
                translatorType = ActivityCommon.TranslatorType.GoogleApis;
            }

            _activityCommon.Translator = translatorType;
        }

        private void RestoreSetting()
        {
            if (Enum.TryParse(_instanceData.OldTranslator, out ActivityCommon.TranslatorType translator))
            {
                _activityCommon.Translator = translator;
            }

            ActivityCommon.YandexApiKey = _instanceData.OldYandexApiKey ?? string.Empty;
            ActivityCommon.IbmTranslatorApiKey = _instanceData.OldIbmTranslatorApiKey ?? string.Empty;
            ActivityCommon.IbmTranslatorUrl = _instanceData.OldIbmTranslatorUrl ?? string.Empty;
            ActivityCommon.DeeplApiKey = _instanceData.OldDeeplApiKey ?? string.Empty;
            ActivityCommon.YandexCloudApiKey = _instanceData.OldYandexCloudApiKey ?? string.Empty;
            ActivityCommon.YandexCloudFolderId = _instanceData.OldYandexCloudFolderId ?? string.Empty;
            ActivityCommon.GoogleApisUrl = _instanceData.OldGoogleApisUrl ?? string.Empty;
        }

        private bool SettingsChanged()
        {
            try
            {
                if (Enum.TryParse(_instanceData.OldTranslator, out ActivityCommon.TranslatorType translator))
                {
                    if (_activityCommon.Translator != translator)
                    {
                        return true;
                    }
                }

                if (string.Compare(ActivityCommon.YandexApiKey ?? string.Empty, _instanceData.OldYandexApiKey ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                if (string.Compare(ActivityCommon.IbmTranslatorApiKey ?? string.Empty, _instanceData.OldIbmTranslatorApiKey ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                if (string.Compare(ActivityCommon.IbmTranslatorUrl ?? string.Empty, _instanceData.OldIbmTranslatorUrl ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                if (string.Compare(ActivityCommon.DeeplApiKey ?? string.Empty, _instanceData.OldDeeplApiKey ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                if (string.Compare(ActivityCommon.YandexCloudApiKey ?? string.Empty, _instanceData.OldYandexCloudApiKey ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                if (string.Compare(ActivityCommon.YandexCloudFolderId ?? string.Empty, _instanceData.OldYandexCloudFolderId ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                if (string.Compare(ActivityCommon.GoogleApisUrl ?? string.Empty, _instanceData.OldGoogleApisUrl ?? string.Empty, StringComparison.Ordinal) != 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool StoreYandexKey(EventHandler handler)
        {
            UpdateSettings();
            if (!SettingsChanged())
            {
                return true;
            }

            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    SetResult(Android.App.Result.Ok);
                    handler?.Invoke(sender, args);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    RestoreSetting();
                    handler?.Invoke(sender, args);
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.translate_store_settings)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
            return false;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }
    }
}
