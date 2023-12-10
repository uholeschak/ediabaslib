using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using Com.Woxthebox.Draglistview;

namespace BmwDeepObd.Dialogs
{
    public class TextListReorderDialog : AlertDialog.Builder
    {
        private Android.App.Activity _activity;
        private AlertDialog _dialog;
        private InputMethodManager _imm;
        private View _view;
        private TextView _textViewMessage;
        private TextView _textViewMessageDetail;
        private DragListView _listViewItems;
        private List<StringObjType> _itemList;

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

        public string MessageDetail
        {
            get
            {
                if (_textViewMessageDetail != null)
                {
                    return _textViewMessageDetail.Text;
                }
                return string.Empty;
            }
            set
            {
                if (_textViewMessageDetail != null)
                {
                    _textViewMessageDetail.Text = value;
                    _textViewMessageDetail.Visibility = string.IsNullOrWhiteSpace(value) ? ViewStates.Gone : ViewStates.Visible;
                }
            }
        }

        public List<StringObjType> ItemList
        {
            get => _itemList;
            set
            {
                _itemList = value;
            }
        }

        public TextListReorderDialog(Context context) : base(context)
        {
            LoadView(context);
        }

        public TextListReorderDialog(Context context, int themeResId) : base(context, themeResId)
        {
            LoadView(context);
        }

        protected void LoadView(Context context)
        {
            SetCancelable(false);
            _activity = context as Android.App.Activity;

            if (_activity != null)
            {
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.list_reorder_dialog, null);
                SetView(_view);

                _imm = (InputMethodManager)_activity.GetSystemService(Context.InputMethodService);

                _textViewMessage = _view.FindViewById<TextView>(Resource.Id.textViewMessage);
                _textViewMessageDetail = _view.FindViewById<TextView>(Resource.Id.textViewMessageDetail);
                _listViewItems = _view.FindViewById<DragListView>(Resource.Id.listViewItems);
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
