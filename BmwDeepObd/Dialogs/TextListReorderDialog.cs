using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Woxthebox.Draglistview;
using Skydoves.BalloonLib;

namespace BmwDeepObd.Dialogs
{
    public class TextListReorderDialog : AlertDialog.Builder
    {
        private Android.App.Activity _activity;
        private AlertDialog _dialog;
        private View _view;
        private TextView _textViewMessage;
        private TextView _textViewMessageDetail;
        private Button _buttonAbort;
        private Button _buttonExtra;
        private Button _buttonOk;
        private DragListView _listViewItems;
        private DragListAdapter _dragListAdapter;
        private List<StringObjInfo> _itemList;

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
                    _textViewMessage.Visibility = string.IsNullOrWhiteSpace(value) ? ViewStates.Gone : ViewStates.Visible;

                    if (!_view.IsInLayout)
                    {
                        _view.RequestLayout();
                    }
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

                    if (!_view.IsInLayout)
                    {
                        _view.RequestLayout();
                    }
                }
            }
        }

        public List<StringObjInfo> ItemList => _itemList;
        public Button ButtonAbort => _buttonAbort;
        public Button ButtonExtra => _buttonExtra;
        public Button ButtonOk => _buttonOk;

        public TextListReorderDialog(Context context, List<StringObjInfo> itemList) : base(context)
        {
            _itemList = itemList;
            LoadView(context);
        }

        public TextListReorderDialog(Context context, List<StringObjInfo> itemList, int themeResId) : base(context, themeResId)
        {
            _itemList = itemList;
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

                _textViewMessage = _view.FindViewById<TextView>(Resource.Id.textViewMessage);
                _textViewMessage.Text = string.Empty;

                _textViewMessageDetail = _view.FindViewById<TextView>(Resource.Id.textViewMessageDetail);
                _textViewMessageDetail.Text = string.Empty;

                _buttonAbort = _view.FindViewById<Button>(Resource.Id.buttonAbort);

                _buttonExtra = _view.FindViewById<Button>(Resource.Id.buttonExtra);
                _buttonExtra.Text = string.Empty;
                _buttonExtra.Visibility = ViewStates.Gone;

                _buttonOk = _view.FindViewById<Button>(Resource.Id.buttonOk);

                _listViewItems = _view.FindViewById<DragListView>(Resource.Id.listViewItems);
                _dragListAdapter = new DragListAdapter(_activity, _itemList, Resource.Layout.reorder_select_list_item, Resource.Id.item_layout, true);
                _listViewItems.SetLayoutManager(new LinearLayoutManager(_activity, LinearLayoutManager.Vertical, false));
                _listViewItems.SetAdapter(_dragListAdapter, false);
                _listViewItems.SetCanDragHorizontally(false);
                _listViewItems.SetCanDragVertically(true);
                _listViewItems.SetCustomDragItem(new CustomDragItem(_activity, Resource.Layout.reorder_select_list_item));
                _listViewItems.SetDragListListener(new CustomDragListener(this));
                _listViewItems.SetDragListCallback(new CustomDragListCallback(this));
                _listViewItems.DragEnabled = true;
            }
        }

        public override AlertDialog Create()
        {
            AlertDialog alertDialog = base.Create();
            alertDialog.ShowEvent += (sender, args) =>
            {
                alertDialog.Window?.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                int itemsCount = _dragListAdapter.ItemsCount;
                if (itemsCount > 0)
                {
                    View itemViewBalloon = null;
                    for (int item = 0; item < itemsCount; item++)
                    {
                        DragListAdapter.CustomViewHolder viewHolder = _dragListAdapter.GetItemViewHolder(item);
                        View itemView = viewHolder?.ItemView;
                        if (itemView != null && itemView.IsShown)
                        {
                            itemViewBalloon = itemView;
                            break;
                        }
                    }

                    if (itemViewBalloon != null)
                    {
                        Balloon.Builder balloonBuilder = ActivityCommon.GetBalloonBuilder(_activity);
                        balloonBuilder.SetText(_activity.GetString(Resource.String.display_order_edit_hint));
                        Balloon balloon = balloonBuilder.Build();
                        balloon.ShowAtCenter(itemViewBalloon);
                    }
                }
            };

            return alertDialog;
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

        public void UpdateItemList(List<StringObjInfo> itemList)
        {
            _dragListAdapter.ClearItems();
            foreach (StringObjInfo itemInfo in itemList)
            {
                _dragListAdapter.AppendItem(itemInfo);
            }

            _itemList = itemList;
            _dragListAdapter.NotifyDataSetChanged();
        }

        public void Dismiss()
        {
            _dialog?.Dismiss();
            _dialog = null;
        }

        public class StringObjInfo
        {
            public StringObjInfo(string title, string description, object data, long itemId) : this(title, description, null, data, itemId)
            {
            }

            public StringObjInfo(string title, string description, string detail, object data, long itemId)
            {
                Title = title;
                Description = description;
                Detail = detail;
                Data = data;
                ItemId = itemId;
            }

            public string Title { get; set; }
            public string Description { get; set; }
            public string Detail { get; set; }
            public object Data { get; set; }
            public long ItemId { get; set; }
        }

        public class DragListAdapter : DragItemAdapter
        {
            public int ItemsCount => ItemList.Count;

            private readonly Context _context;
            private readonly int _layoutId;
            private readonly int _dragHandleId;
            private readonly bool _dragOnLongPress;

            public DragListAdapter(Context context, List<StringObjInfo> itemList, int layoutId, int dragHandleId, bool dragOnLongPress)
            {
                _context = context;
                _layoutId = layoutId;
                _dragHandleId = dragHandleId;
                _dragOnLongPress = dragOnLongPress;

                List<InfoWrapper> infoList = new List<InfoWrapper>();
                foreach (StringObjInfo info in itemList)
                {
                    infoList.Add(new InfoWrapper(this, info));
                }

                ItemList = infoList;
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

                infoWrapper.BoundViewHolder = customHolder;
                View grabView = customHolder.MGrabView;
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

            public CustomViewHolder GetItemViewHolder(int itemIndex)
            {
                if ((itemIndex < 0) || (itemIndex >= ItemsCount))
                {
                    return null;
                }

                InfoWrapper infoWrapper = ItemList[itemIndex] as InfoWrapper;
                if (infoWrapper != null)
                {
                    return infoWrapper.BoundViewHolder;
                }

                return null;
            }

            public class CustomViewHolder : ViewHolder
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
                    ItemId = info.ItemId;
                    BoundViewHolder = null;
                }

                public StringObjInfo Info { get; }

                public long ItemId { get; }

                public CustomViewHolder BoundViewHolder { get; set; }
            }
        }

        private class CustomDragItem : DragItem
        {
            private readonly Context _context;
            private readonly Android.Graphics.Color _backgroundColor;

            public CustomDragItem(Context context, int layoutId) : base(context, layoutId)
            {
                _context = context;
                _backgroundColor = ActivityCommon.GetStyleColor(context, Resource.Attribute.dragBackgroundColor);
            }

            public override void OnBindDragView(View clickedView, View dragView)
            {
                TextView textItemTitleClick = clickedView.FindViewById<TextView>(Resource.Id.textItemTitle);
                TextView textItemDescClick = clickedView.FindViewById<TextView>(Resource.Id.textItemDesc);
                TextView textItemDetailClick = clickedView.FindViewById<TextView>(Resource.Id.textItemDetail);

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

        private class CustomDragListener : Java.Lang.Object, DragListView.IDragListListener
        {
            private readonly TextListReorderDialog _dialog;

            public CustomDragListener(TextListReorderDialog dialog)
            {
                _dialog = dialog;
            }

            public void OnItemDragStarted(int p0)
            {
            }

            public void OnItemDragEnded(int p0, int p1)
            {
                if (p0 != p1)
                {
                    if (p0 >= 0 && p0 < _dialog.ItemList.Count && p1 >= 0 && p1 < _dialog.ItemList.Count)
                    {
                        int oldIndex = p0;
                        int newIndex = p1;

                        StringObjInfo ecuInfo = _dialog._itemList[oldIndex];
                        _dialog._itemList.RemoveAt(oldIndex);
                        _dialog._itemList.Insert(newIndex, ecuInfo);
                    }
                }
            }

            public void OnItemDragging(int p0, float p1, float p2)
            {
            }
        }

        private class CustomDragListCallback : Java.Lang.Object, DragListView.IDragListCallback
        {
            private readonly TextListReorderDialog _dialog;

            public CustomDragListCallback(TextListReorderDialog dialog)
            {
                _dialog = dialog;
            }

            public bool CanDragItemAtPosition(int p0)
            {
                if (p0 < 0 || p0 >= _dialog._itemList.Count)
                {
                    return false;
                }

                return true;
            }

            public bool CanDropItemAtPosition(int p0)
            {
                return CanDragItemAtPosition(p0);
            }
        }
    }
}
