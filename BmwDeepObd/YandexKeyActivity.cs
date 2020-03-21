using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/yandex_api_key_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class YandexKeyActivity : BaseActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public string OldYandexApiKey { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private InputMethodManager _imm;
        private Timer _clipboardCheckTimer;
        private ActivityCommon _activityCommon;
        private View _contentView;
        private LinearLayout _layoutYandexKey;
        private Button _buttonYandexApiKeyCreate;
        private Button _buttonYandexApiKeyGet;
        private Button _buttonYandexApiKeyPaste;
        private EditText _editTextYandexApiKey;
        private Button _buttonYandexApiKeyTest;
        private TextView _textViewYandexApiKeyTestResult;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);
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
                _instanceData.OldYandexApiKey = ActivityCommon.YandexApiKey ?? string.Empty;
            }

            _activityCommon = new ActivityCommon(this);

            _layoutYandexKey = FindViewById<LinearLayout>(Resource.Id.layoutYandexKey);
            _layoutYandexKey.SetOnTouchListener(this);

            _editTextYandexApiKey = FindViewById<EditText>(Resource.Id.editTextYandexApiKey);
            _editTextYandexApiKey.Text = _instanceData.OldYandexApiKey;

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
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://tech.yandex.com/keys/get/?service=trnsl")));
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
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://tech.yandex.com/keys/")));
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

            _textViewYandexApiKeyTestResult = FindViewById<TextView>(Resource.Id.textViewYandexKeyTestResult);

            _buttonYandexApiKeyTest = FindViewById<Button>(Resource.Id.buttonYandexKeyTest);
            _buttonYandexApiKeyTest.SetOnTouchListener(this);
            _buttonYandexApiKeyTest.Click += (sender, args) =>
            {
                _textViewYandexApiKeyTestResult.Text = string.Empty;
                ActivityCommon.YandexApiKey = _editTextYandexApiKey.Text.Trim();
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
            if (_clipboardCheckTimer != null)
            {
                _clipboardCheckTimer.Dispose();
                _clipboardCheckTimer = null;
            }
        }

        public override void OnBackPressed()
        {
            if (StoreYandexKey((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                base.OnBackPressed();
            }))
            {
                base.OnBackPressed();
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

        private void UpdateDisplay()
        {
            bool pasteEnable = false;

            string clipText = _activityCommon.GetClipboardText();
            if (!string.IsNullOrWhiteSpace(clipText))
            {
                pasteEnable = true;
            }
            _buttonYandexApiKeyPaste.Enabled = pasteEnable;
            _buttonYandexApiKeyTest.Enabled = !string.IsNullOrWhiteSpace(_editTextYandexApiKey.Text);
        }

        private bool StoreYandexKey(EventHandler handler)
        {
            string newYandexApiKey = _editTextYandexApiKey.Text.Trim();
            if (string.Compare(_instanceData.OldYandexApiKey, newYandexApiKey, StringComparison.Ordinal) == 0)
            {
                return true;
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    ActivityCommon.YandexApiKey = newYandexApiKey;
                    SetResult(Android.App.Result.Ok);
                    handler?.Invoke(sender, args);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    ActivityCommon.YandexApiKey = _instanceData.OldYandexApiKey;
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
