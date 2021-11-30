using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace BmwDeepObd
{
    public class TraceInfoInputDialog : AlertDialog.Builder
    {
        private Android.App.Activity _activity;
        private AlertDialog _dialog;
        private InputMethodManager _imm;
        private View _view;
        private TextView _textViewMessage;
        private EditText _editTextEmailAddress;
        private EditText _editTextInfo;

        public string Message
        {
            get
            {
                if (_textViewMessage != null)
                {
                    return _textViewMessage.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_textViewMessage != null)
                {
                    _textViewMessage.Text = value;
                }
            }
        }

        public string EmailAddress
        {
            get
            {
                if (_editTextEmailAddress != null)
                {
                    return _editTextEmailAddress.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_editTextEmailAddress != null)
                {
                    _editTextEmailAddress.Text = value;
                }
            }
        }

        public string InfoText
        {
            get
            {
                if (_editTextInfo != null)
                {
                    return _editTextInfo.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_editTextInfo != null)
                {
                    _editTextInfo.Text = value;
                }
            }
        }

        public TraceInfoInputDialog(Context context) : base(context)
        {
            LoadView(context);
        }

        public TraceInfoInputDialog(Context context, int themeResId) : base(context, themeResId)
        {
            LoadView(context);
        }

        protected void LoadView(Context context)
        {
            SetCancelable(false);
            _activity = context as Android.App.Activity;

            if (_activity != null)
            {
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.trace_info, null);
                SetView(_view);

                _imm = (InputMethodManager)_activity.GetSystemService(Context.InputMethodService);

                _textViewMessage = _view.FindViewById<TextView>(Resource.Id.textViewMessage);

                _editTextEmailAddress = _view.FindViewById<EditText>(Resource.Id.editTextEmailAddress);
                _editTextEmailAddress.EditorAction += TextEditorAction;

                _editTextInfo = _view.FindViewById<EditText>(Resource.Id.editTextInfo);
                _editTextInfo.EditorAction += TextEditorAction;
            }
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_view.WindowToken, HideSoftInputFlags.None);
        }

        private void TextEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            switch (e.ActionId)
            {
                case ImeAction.Go:
                case ImeAction.Send:
                case ImeAction.Next:
                case ImeAction.Done:
                case ImeAction.Previous:
                    HideKeyboard();
                    break;
            }
        }

        public new void Show()
        {
            if (_dialog == null)
            {
                _dialog = base.Show();
            }
        }

        public new void SetMessage(string message)
        {
            Message = message;
        }

        public new void SetMessage(int messageId)
        {
            if (_activity != null)
            {
                Message = _activity.GetString(messageId);
            }
        }

        public void Dismiss()
        {
            _dialog?.Dismiss();
            _dialog = null;
        }
    }
}
