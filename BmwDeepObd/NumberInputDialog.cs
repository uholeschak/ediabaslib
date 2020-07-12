using Android.Content;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    public class NumberInputDialog : AlertDialog.Builder
    {
        private Android.App.Activity _activity;
        private AlertDialog _dialog;
        private View _view;
        private TextView _textViewMessage;
        private EditText _editTextNumber;

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

        public string Number
        {
            get
            {
                if (_editTextNumber != null)
                {
                    return _editTextNumber.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_editTextNumber != null)
                {
                    _editTextNumber.Text = value;
                }
            }
        }

        public string Digits
        {
            set
            {
                if (_editTextNumber != null)
                {
                    _editTextNumber.InputType = InputTypes.ClassNumber;
                    if (!string.IsNullOrEmpty(value))
                    {
                        _editTextNumber.KeyListener = DigitsKeyListener.GetInstance(value);
                    }
                }
            }
        }

        public NumberInputDialog(Context context) : base(context)
        {
            LoadView(context);
        }

        public NumberInputDialog(Context context, int themeResId) : base(context, themeResId)
        {
            LoadView(context);
        }

        protected void LoadView(Context context)
        {
            SetCancelable(false);
            _activity = context as Android.App.Activity;
            if (_activity != null)
            {
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.number_input, null);
                SetView(_view);

                _textViewMessage = _view.FindViewById<TextView>(Resource.Id.textViewMessage);
                _editTextNumber = _view.FindViewById<EditText>(Resource.Id.editTextNumber);
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
