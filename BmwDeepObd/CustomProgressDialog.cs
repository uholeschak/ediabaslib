using Android.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    public class CustomProgressDialog : AlertDialog.Builder
    {
        private Android.App.Activity _activity;
        private AlertDialog _dialog;
        private View _view;
        private ProgressBar _progressBar;
        private LinearLayout _layoutProgressText;
        private TextView _textViewProgressMessage;
        private TextView _textViewProgressLeft;
        private TextView _textViewProgressRight;
        private Button _buttonAbort;

        public string Message
        {
            get
            {
                if (_textViewProgressMessage != null)
                {
                    return _textViewProgressMessage.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_textViewProgressMessage != null)
                {
                    _textViewProgressMessage.Text = value;
                }
            }
        }

        public int Max
        {
            get
            {
                if (_progressBar != null)
                {
                    return _progressBar.Max;
                }
                return 0;
            }
            set
            {
                if (_progressBar != null)
                {
                    _progressBar.Max = value;
                    UpdateText();
                }
            }
        }

        public int Progress
        {
            get
            {
                if (_progressBar != null)
                {
                    return _progressBar.Progress;
                }
                return 0;
            }
            set
            {
                if (_progressBar != null)
                {
                    _progressBar.Progress = value;
                    UpdateText();
                }
            }
        }

        public bool Indeterminate
        {
            get
            {
                if (_progressBar != null)
                {
                    return _progressBar.Indeterminate;
                }
                return false;
            }
            set
            {
                if (_progressBar != null)
                {
                    _progressBar.Indeterminate = value;
                    UpdateText();
                }
            }
        }

        public Button ButtonAbort => _buttonAbort;

        public CustomProgressDialog(Context context) : base(context)
        {
            LoadView(context);
        }

        public CustomProgressDialog(Context context, int themeResId) : base(context, themeResId)
        {
            LoadView(context);
        }

        protected void LoadView(Context context)
        {
            _activity = context as Android.App.Activity;
            if (_activity != null)
            {
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.custom_progress, null);
                SetView(_view);
                _progressBar = _view.FindViewById<ProgressBar>(Resource.Id.progressBar);
                _progressBar.Indeterminate = false;
                _progressBar.Max = 100;
                _progressBar.Progress = 0;

                _layoutProgressText = _view.FindViewById<LinearLayout>(Resource.Id.layoutProgressText);
                _textViewProgressMessage = _view.FindViewById<TextView>(Resource.Id.textViewProgressMessage);
                _textViewProgressMessage.Text = string.Empty;
                _textViewProgressLeft = _view.FindViewById<TextView>(Resource.Id.textViewProgressLeft);
                _textViewProgressRight = _view.FindViewById<TextView>(Resource.Id.textViewProgressRight);

                _buttonAbort = _view.FindViewById<Button>(Resource.Id.buttonAbort);
                UpdateText();
            }
        }

        protected void UpdateText()
        {
            if (_progressBar == null || _textViewProgressLeft == null || _textViewProgressRight == null)
            {
                return;
            }
            _layoutProgressText.Visibility = _progressBar.Indeterminate ? ViewStates.Gone : ViewStates.Visible;
            _textViewProgressLeft.Text = string.Format("{0}%", _progressBar.Progress);
            _textViewProgressRight.Text = string.Format("{0}/{1}", _progressBar.Progress, _progressBar.Max);
        }

        public new void Show()
        {
            _dialog = base.Show();
        }

        public void Hide()
        {
            _dialog?.Hide();
        }

        public void Dismiss()
        {
            _dialog?.Dismiss();
        }
    }
}
