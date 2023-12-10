using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
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
        private DragListAdapter _dragListAdapter;
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
                _dragListAdapter = new DragListAdapter(_activity, Resource.Layout.reorder_select_list_item, Resource.Id.item_layout, true);
                _listViewItems.SetAdapter(_dragListAdapter, false);
                _listViewItems.SetCanDragHorizontally(false);
                _listViewItems.SetCanDragVertically(true);
                _listViewItems.SetCustomDragItem(new CustomDragItem(_activity, Resource.Layout.reorder_select_list_item));
                //_listViewItems.SetDragListListener(new CustomDragListener(this));
                //_listViewItems.SetDragListCallback(new CustomDragListCallback(this));
                _listViewItems.DragEnabled = true;
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

        private class DragListAdapter : DragItemAdapter
        {
            public int ItemsCount => ItemList.Count;

            private readonly Context _context;
            private readonly int _layoutId;
            private readonly int _dragHandleId;
            private readonly bool _dragOnLongPress;
            private long _itemIdCurrent;
            private readonly int? _backgroundResource;

            public DragListAdapter(Context context, int layoutId, int dragHandleId, bool dragOnLongPress)
            {
                _context = context;
                _layoutId = layoutId;
                _dragHandleId = dragHandleId;
                _dragOnLongPress = dragOnLongPress;
                _itemIdCurrent = 0;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    TypedArray typedArray = context.Theme.ObtainStyledAttributes(new[] { Android.Resource.Attribute.SelectableItemBackground });
                    _backgroundResource = typedArray.GetResourceId(0, 0);
                }

                ItemList = new List<InfoWrapper>();
            }

            public override long GetUniqueItemId(int position)
            {
                InfoWrapper infoWrapper = ItemList[position] as InfoWrapper;
                if (infoWrapper != null)
                {
                    return infoWrapper.ItemId;
                }

                return -1;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent.Context)?.Inflate(_layoutId, parent, false);
                return new CustomViewHolder(this, view, _dragHandleId, _dragOnLongPress);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                base.OnBindViewHolder(holder, position);

                InfoWrapper infoWrapper = ItemList[position] as InfoWrapper;
                StringObjInfo item = infoWrapper?.Info;
                if (item == null)
                {
                    return;
                }

                CustomViewHolder customHolder = holder as CustomViewHolder;
                if (customHolder == null)
                {
                    return;
                }

                View grabView = customHolder.MGrabView;
                if (_backgroundResource != null)
                {
                    grabView.SetBackgroundResource(_backgroundResource.Value);
                }

                grabView.Tag = infoWrapper;

                View view = customHolder.ItemView;
                view.Tag = infoWrapper;

                View itemDividerTop = view.FindViewById<View>(Resource.Id.item_divider_top);
                View itemDividerBottom = view.FindViewById<View>(Resource.Id.item_divider_bottom);
                itemDividerTop.Visibility = ViewStates.Invisible;
                itemDividerBottom.Visibility = ViewStates.Visible;

                TextView textItemTitle = view.FindViewById<TextView>(Resource.Id.textItemTitle);
                TextView textItemDesc = view.FindViewById<TextView>(Resource.Id.textItemDesc);
                TextView textItemDetail = view.FindViewById<TextView>(Resource.Id.textItemDetail);

                if (!string.IsNullOrEmpty(item.Title))
                {
                    textItemTitle.Text = item.Title;
                    textItemTitle.Visibility = ViewStates.Visible;
                }
                else
                {
                    textItemTitle.Visibility = ViewStates.Gone;
                }

                if (!string.IsNullOrEmpty(item.Description))
                {
                    textItemDesc.Text = item.Description;
                    textItemDesc.Visibility = ViewStates.Visible;
                }
                else
                {
                    textItemDesc.Visibility = ViewStates.Gone;
                }

                if (!string.IsNullOrEmpty(item.Detail))
                {
                    textItemDetail.Text = item.Detail;
                    textItemDetail.Visibility = ViewStates.Visible;
                }
                else
                {
                    textItemDetail.Visibility = ViewStates.Gone;
                }
            }

            public void ClearItems()
            {
                while (ItemList.Count > 0)
                {
                    RemoveItem(0);
                }
            }

            public void AppendItem(StringObjInfo itemInfo)
            {
                AddItem(ItemList.Count, new InfoWrapper(this, itemInfo));
            }

            public int GetItemIndex(StringObjInfo info)
            {
                for (int i = 0; i < ItemsCount; i++)
                {
                    InfoWrapper infoWrapper = ItemList[i] as InfoWrapper;
                    if (infoWrapper != null)
                    {
                        if (infoWrapper.Info == info)
                        {
                            return i;
                        }
                    }
                }

                return -1;
            }

            public class StringObjInfo
            {
                public StringObjInfo(string title, string description, object data) : this(title, description, null, data)
                {
                }

                public StringObjInfo(string title, string description, string detail, object data)
                {
                    Title = title;
                    Description = description;
                    Detail = detail;
                    Data = data;
                }

                public string Title { get; set; }
                public string Description { get; set; }
                public string Detail { get; set; }
                public object Data { get; set; }
                public long? ItemId { get; set; }
            }

            private class CustomViewHolder : ViewHolder
            {
                private readonly DragListAdapter _adapter;

                public CustomViewHolder(DragListAdapter adapter, View itemView, int handleResId, bool dragOnLongPress) : base(itemView, handleResId, dragOnLongPress)
                {
                    _adapter = adapter;
                }
            }

            private class InfoWrapper : Java.Lang.Object
            {
                public InfoWrapper(DragListAdapter adapter, StringObjInfo info)
                {
                    Info = info;
                    if (info.ItemId == null)
                    {
                        info.ItemId = adapter._itemIdCurrent++;
                    }
                    ItemId = info.ItemId.Value;
                }

                public StringObjInfo Info { get; }

                public long ItemId { get; }
            }
        }

        private class CustomDragItem : DragItem
        {
            private readonly Context _context;
            private readonly Android.Graphics.Color _backgroundColor;

            public CustomDragItem(Context context, int layoutId) : base(context, layoutId)
            {
                _context = context;

                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new[] { Resource.Attribute.dragBackgroundColor });
                _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
            }

            public override void OnBindDragView(View clickedView, View dragView)
            {
                TextView textItemTitleClick = dragView.FindViewById<TextView>(Resource.Id.textItemTitle);
                TextView textItemDescClick = dragView.FindViewById<TextView>(Resource.Id.textItemDesc);
                TextView textItemDetailClick = dragView.FindViewById<TextView>(Resource.Id.textItemDetail);

                TextView textItemTitleDrag = dragView.FindViewById<TextView>(Resource.Id.textItemTitle);
                TextView textItemDescDrag = dragView.FindViewById<TextView>(Resource.Id.textItemDesc);
                TextView textItemDetailDrag = dragView.FindViewById<TextView>(Resource.Id.textItemDetail);

                View itemDividerTopDrag = dragView.FindViewById<View>(Resource.Id.item_divider_top);
                View itemDividerBottomDrag = dragView.FindViewById<View>(Resource.Id.item_divider_bottom);

                textItemTitleDrag.Text = textItemTitleClick.Text;
                textItemTitleDrag.Visibility = textItemTitleClick.Visibility;

                textItemDescDrag.Text = textItemDescClick.Text;
                textItemDescDrag.Visibility = textItemDescClick.Visibility;

                textItemDetailDrag.Text = textItemDetailClick.Text;
                textItemDetailDrag.Visibility = textItemDetailClick.Visibility;

                itemDividerTopDrag.Visibility = ViewStates.Visible;
                itemDividerBottomDrag.Visibility = ViewStates.Visible;

                dragView.SetBackgroundColor(_backgroundColor);
                dragView.JumpDrawablesToCurrentState();
            }
        }

    }
}
