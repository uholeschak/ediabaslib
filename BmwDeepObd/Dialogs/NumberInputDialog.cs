using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace BmwDeepObd.Dialogs
{
    public class NumberInputDialog : AlertDialog.Builder
    {
        private Android.App.Activity _activity;
        private AlertDialog _dialog;
        private View _view;
        private TextView _textViewInfo;
        private bool _infoVisible;
        private TextView _textViewMessage1;
        private EditText _editTextNumber1;
        private bool _visible1;
        private TextView _textViewMessage2;
        private EditText _editTextNumber2;
        private bool _visible2;

        public string Info
        {
            get
            {
                if (_textViewInfo != null)
                {
                    return _textViewInfo.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_textViewInfo != null)
                {
                    _textViewInfo.Text = value;
                }
            }
        }

        public string Message1
        {
            get
            {
                if (_textViewMessage1 != null)
                {
                    return _textViewMessage1.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_textViewMessage1 != null)
                {
                    _textViewMessage1.Text = value;
                }
            }
        }

        public string Message2
        {
            get
            {
                if (_textViewMessage2 != null)
                {
                    return _textViewMessage2.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_textViewMessage2 != null)
                {
                    _textViewMessage2.Text = value;
                }
            }
        }

        public string Number1
        {
            get
            {
                if (_editTextNumber1 != null)
                {
                    return _editTextNumber1.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_editTextNumber1 != null)
                {
                    _editTextNumber1.Text = value;
                }
            }
        }

        public string Number2
        {
            get
            {
                if (_editTextNumber2 != null)
                {
                    return _editTextNumber2.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_editTextNumber2 != null)
                {
                    _editTextNumber2.Text = value;
                }
            }
        }

        public string Digits1
        {
            set
            {
                if (_editTextNumber1 != null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _editTextNumber1.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
                        _editTextNumber1.KeyListener = DigitsKeyListener.GetInstance(value);
                    }
                    else
                    {
                        _editTextNumber1.InputType = InputTypes.ClassNumber;
                    }
                }
            }
        }

        public string Digits2
        {
            set
            {
                if (_editTextNumber2 != null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _editTextNumber2.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;
                        _editTextNumber2.KeyListener = DigitsKeyListener.GetInstance(value);
                    }
                    else
                    {
                        _editTextNumber2.InputType = InputTypes.ClassNumber;
                    }
                }
            }
        }

        public bool InfoVisible
        {
            get
            {
                return _infoVisible;
            }
            set
            {
                _infoVisible = value;
                ViewStates visibility = _infoVisible ? ViewStates.Visible : ViewStates.Gone;
                if (_textViewInfo != null)
                {
                    _textViewInfo.Visibility = visibility;
                }

                if (!_view.IsInLayout)
                {
                    _view.RequestLayout();
                }
            }
        }

        public bool Visible1
        {
            get
            {
                return _visible1;
            }
            set
            {
                _visible1 = value;
                ViewStates visibility = _visible1 ? ViewStates.Visible : ViewStates.Gone;
                if (_textViewMessage1 != null)
                {
                    _textViewMessage1.Visibility = visibility;
                }
                if (_editTextNumber1 != null)
                {
                    _editTextNumber1.Visibility = visibility;
                }

                if (!_view.IsInLayout)
                {
                    _view.RequestLayout();
                }
            }
        }

        public bool Visible2
        {
            get
            {
                return _visible2;
            }
            set
            {
                _visible2 = value;
                ViewStates visibility = _visible2 ? ViewStates.Visible : ViewStates.Gone;
                if (_textViewMessage2 != null)
                {
                    _textViewMessage2.Visibility = visibility;
                }
                if (_editTextNumber2 != null)
                {
                    _editTextNumber2.Visibility = visibility;
                }
                _view.RequestLayout();
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
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.number_input_dialog, null);
                SetView(_view);

                _textViewInfo = _view.FindViewById<TextView>(Resource.Id.textViewInfo);
                _textViewMessage1 = _view.FindViewById<TextView>(Resource.Id.textViewMessage1);
                _editTextNumber1 = _view.FindViewById<EditText>(Resource.Id.editTextNumber1);
                _textViewMessage2 = _view.FindViewById<TextView>(Resource.Id.textViewMessage2);
                _editTextNumber2 = _view.FindViewById<EditText>(Resource.Id.editTextNumber2);
            }

            InfoVisible = false;
            Visible1 = true;
            Visible2 = false;
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
            Message1 = message;
        }

        public new void SetMessage(int messageId)
        {
            if (_activity != null)
            {
                Message1 = _activity.GetString(messageId);
            }
        }

        public void Dismiss()
        {
            _dialog?.Dismiss();
            _dialog = null;
        }
    }
}
