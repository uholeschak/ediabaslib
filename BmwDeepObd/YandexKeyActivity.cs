using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/yandex_api_key_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(YandexKeyActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class YandexKeyActivity : BaseActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public InstanceData()
            {
                OldApiKey = string.Empty;
                OldApiUrl = string.Empty;
            }

            public string OldApiKey { get; set; }
            public string OldApiUrl { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private InputMethodManager _imm;
        private Timer _clipboardCheckTimer;
        private ActivityCommon _activityCommon;
        private View _contentView;
        private TextView _textViewYandexKeyDesc;
        private LinearLayout _layoutYandexKey;
        private Button _buttonYandexApiKeyCreate;
        private Button _buttonYandexApiKeyGet;
        private Button _buttonYandexApiKeyPaste;
        private EditText _editTextYandexApiKey;
        private TextView _textViewApiUrlPasteTitle;
        private Button _buttonApiUrlPaste;
        private EditText _editTextApiUrl;
        private Button _buttonYandexApiKeyTest;
        private TextView _textViewYandexApiKeyTestResult;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);
            _allowFullScreenMode = false;
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.yandex_key_select);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            if (!_activityRecreated)
            {
                switch (ActivityCommon.SelectedTranslator)
                {
                    case ActivityCommon.TranslatorType.YandexTranslate:
                        _instanceData.OldApiKey = ActivityCommon.YandexApiKey ?? string.Empty;
                        break;

                    case ActivityCommon.TranslatorType.IbmWatson:
                        _instanceData.OldApiKey = ActivityCommon.IbmTranslatorApiKey ?? string.Empty;
                        _instanceData.OldApiUrl = ActivityCommon.IbmTranslatorUrl ?? string.Empty;
                        break;

                    case ActivityCommon.TranslatorType.Deepl:
                        _instanceData.OldApiKey = ActivityCommon.DeeplApiKey ?? string.Empty;
                        break;
                }
            }

            _activityCommon = new ActivityCommon(this);

            _textViewYandexKeyDesc = FindViewById<TextView>(Resource.Id.textViewYandexKeyDesc);
            _textViewYandexKeyDesc.Text = string.Format(GetString(Resource.String.yandex_key_desc), _activityCommon.TranslatorName());

            _layoutYandexKey = FindViewById<LinearLayout>(Resource.Id.layoutYandexKey);
            _layoutYandexKey.SetOnTouchListener(this);

            bool apiUrlVisible = ActivityCommon.SelectedTranslator == ActivityCommon.TranslatorType.IbmWatson;

            _editTextYandexApiKey = FindViewById<EditText>(Resource.Id.editTextYandexApiKey);
            _editTextYandexApiKey.Text = _instanceData.OldApiKey;

            _textViewApiUrlPasteTitle = FindViewById<TextView>(Resource.Id.textViewApiUrlPasteTitle);
            _textViewApiUrlPasteTitle.Visibility = apiUrlVisible ? ViewStates.Visible : ViewStates.Gone;

            _editTextApiUrl = FindViewById<EditText>(Resource.Id.editTextApiUrl);
            _editTextApiUrl.Text = _instanceData.OldApiUrl;
            _editTextApiUrl.Visibility = apiUrlVisible ? ViewStates.Visible : ViewStates.Gone;

            _buttonYandexApiKeyCreate = FindViewById<Button>(Resource.Id.buttonYandexKeyCreate);
            _buttonYandexApiKeyCreate.SetOnTouchListener(this);
            _buttonYandexApiKeyCreate.Click += (sender, args) =>
            {
                _activityCommon.ShowWifiConnectedWarning(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.YandexTranslate:
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://translate.yandex.com/developers/keys")));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;

                        case ActivityCommon.TranslatorType.IbmWatson:
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://cloud.ibm.com/catalog/services/language-translator")));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;

                        case ActivityCommon.TranslatorType.Deepl:
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://www.deepl.com/account")));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;
                    }
                });
            };

            _buttonYandexApiKeyGet = FindViewById<Button>(Resource.Id.buttonYandexKeyGet);
            _buttonYandexApiKeyGet.SetOnTouchListener(this);
            _buttonYandexApiKeyGet.Click += (sender, args) =>
            {
                _activityCommon.ShowWifiConnectedWarning(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.YandexTranslate:
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://translate.yandex.com/developers/keys")));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;

                        case ActivityCommon.TranslatorType.IbmWatson:
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://cloud.ibm.com/resources")));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;

                        case ActivityCommon.TranslatorType.Deepl:
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://www.deepl.com/de/account/summary")));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            break;
                    }
                });
            };

            _buttonYandexApiKeyPaste = FindViewById<Button>(Resource.Id.buttonYandexKeyPaste);
            _buttonYandexApiKeyPaste.SetOnTouchListener(this);
            _buttonYandexApiKeyPaste.Click += (sender, args) =>
            {
                string clipText = _activityCommon.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(clipText))
                {
                    _editTextYandexApiKey.Text = clipText.Trim();
                    UpdateDisplay();
                }
            };
            _buttonYandexApiKeyPaste.TextChanged += (sender, args) =>
            {
                UpdateDisplay();
            };

            _buttonApiUrlPaste = FindViewById<Button>(Resource.Id.buttonApiUrlPaste);
            _buttonApiUrlPaste.Visibility = apiUrlVisible ? ViewStates.Visible : ViewStates.Gone;
            _buttonApiUrlPaste.SetOnTouchListener(this);
            _buttonApiUrlPaste.Click += (sender, args) =>
            {
                string clipText = _activityCommon.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(clipText))
                {
                    _editTextApiUrl.Text = clipText.Trim();
                    UpdateDisplay();
                }
            };
            _buttonApiUrlPaste.TextChanged += (sender, args) =>
            {
                UpdateDisplay();
            };

            _textViewYandexApiKeyTestResult = FindViewById<TextView>(Resource.Id.textViewYandexKeyTestResult);

            _buttonYandexApiKeyTest = FindViewById<Button>(Resource.Id.buttonYandexKeyTest);
            _buttonYandexApiKeyTest.SetOnTouchListener(this);
            _buttonYandexApiKeyTest.Click += (sender, args) =>
            {
                _textViewYandexApiKeyTestResult.Text = string.Empty;
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
                }

                if (!_activityCommon.TranslateStrings(new List<string> {"Dieser Text wurde erfolgreich \x00fcbersetzt"}, list =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (list != null && list.Count > 0)
                    {
                        _textViewYandexApiKeyTestResult.Text = list[0];
                    }
                    else
                    {
                        _textViewYandexApiKeyTestResult.Text = GetString(Resource.String.button_yandex_key_test_failed);
                    }
                }, true))
                {
                    _textViewYandexApiKeyTestResult.Text = GetString(Resource.String.button_yandex_key_test_failed);
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
            UpdateDisplay();
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
                        UpdateDisplay();
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

        private void UpdateDisplay()
        {
            bool pasteEnable = false;

            string clipText = _activityCommon.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(clipText))
            {
                pasteEnable = true;
            }
            _buttonYandexApiKeyPaste.Enabled = pasteEnable;

            bool testEnabled = !string.IsNullOrWhiteSpace(_editTextYandexApiKey.Text);
            switch (ActivityCommon.SelectedTranslator)
            {
                case ActivityCommon.TranslatorType.IbmWatson:
                case ActivityCommon.TranslatorType.Deepl:
                    if (string.IsNullOrWhiteSpace(_editTextApiUrl.Text))
                    {
                        testEnabled = false;
                    }
                    break;
            }

            _buttonYandexApiKeyTest.Enabled = testEnabled;
        }

        private bool StoreYandexKey(EventHandler handler)
        {
            string newApiKey = _editTextYandexApiKey.Text.Trim();
            string newApiUrl = _editTextApiUrl.Text.Trim();
            if (string.Compare(_instanceData.OldApiKey, newApiKey, StringComparison.Ordinal) == 0 &&
                string.Compare(_instanceData.OldApiUrl, newApiUrl, StringComparison.Ordinal) == 0)
            {
                return true;
            }

            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.YandexTranslate:
                            ActivityCommon.YandexApiKey = newApiKey;
                            break;

                        case ActivityCommon.TranslatorType.IbmWatson:
                            ActivityCommon.IbmTranslatorApiKey = newApiKey;
                            ActivityCommon.IbmTranslatorUrl = newApiUrl;
                            break;

                        case ActivityCommon.TranslatorType.Deepl:
                            ActivityCommon.DeeplApiKey = newApiKey;
                            break;
                    }

                    SetResult(Android.App.Result.Ok);
                    handler?.Invoke(sender, args);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    switch (ActivityCommon.SelectedTranslator)
                    {
                        case ActivityCommon.TranslatorType.YandexTranslate:
                            ActivityCommon.YandexApiKey = _instanceData.OldApiKey;
                            break;

                        case ActivityCommon.TranslatorType.IbmWatson:
                            ActivityCommon.IbmTranslatorApiKey = _instanceData.OldApiKey;
                            ActivityCommon.IbmTranslatorUrl = _instanceData.OldApiUrl;
                            break;

                        case ActivityCommon.TranslatorType.Deepl:
                            ActivityCommon.DeeplApiKey = _instanceData.OldApiKey;
                            break;
                    }

                    handler?.Invoke(sender, args);
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.translate_store_key)
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
