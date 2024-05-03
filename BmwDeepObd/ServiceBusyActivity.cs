using System.Threading;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Annotations;

namespace BmwDeepObd;

[Android.App.Activity(Label = "@string/trans_api_key_title",
    Name = ActivityCommon.AppNameSpace + "." + nameof(ServiceBusyActivity),
    WindowSoftInputMode = SoftInput.StateAlwaysHidden,
    ConfigurationChanges = ActivityConfigChanges)]
public class ServiceBusyActivity : BaseActivity
{
    private Timer _statusCheckTimer;
    private TextView _statusText;
    private ProgressBar _progressBar;
    private Button _abortButton;
    private Button _closeButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        SetTheme(ActivityCommon.SelectedThemeId);
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.service_busy);

        SetResult(Android.App.Result.Canceled);

        _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
        _statusText = FindViewById<TextView>(Resource.Id.statusText);
        _abortButton = FindViewById<Button>(Resource.Id.abortButton);
        _abortButton.Click += (sender, e) =>
        {
            ForegroundService.AbortThread = true;
        };

        _closeButton = FindViewById<Button>(Resource.Id.closeButton);
        _closeButton.Click += (sender, e) =>
        {
            SetResult(Android.App.Result.Canceled);
            Finish();
        };
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (_statusCheckTimer == null)
        {
            _statusCheckTimer = new Timer(state =>
            {
                RunOnUiThread(() =>
                {
                    _statusText.Text = GetString(Resource.String.service_is_starting);
                });
            }, null, 1000, 1000);
        }
    }

    protected override void OnPause()
    {
        base.OnPause();
        DisposeTimer();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        DisposeTimer();
    }

    private void DisposeTimer()
    {
        if (_statusCheckTimer != null)
        {
            _statusCheckTimer.Dispose();
            _statusCheckTimer = null;
        }
    }
}
