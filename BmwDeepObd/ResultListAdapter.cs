using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    public class ResultListAdapter : BaseAdapter<TableResultItem>
    {
        private readonly List<TableResultItem> _items;
        public List<TableResultItem> Items => _items;
        private readonly Activity _context;
        private readonly float _textWeight;
        private readonly int _textResId;
        private readonly bool _showCheckBox;
        private bool _ignoreCheckEvent;
        private readonly Android.Content.Res.ColorStateList _defaultTextColors;

        public ResultListAdapter(Activity context, float textWeight, int textResId, bool showCheckBox)
        {
            _context = context;
            _items = new List<TableResultItem> ();
            _textWeight = textWeight;
            _textResId = textResId;
            _showCheckBox = showCheckBox;

            TextView dummy = new TextView(context);
            _defaultTextColors = dummy.TextColors;
        }

        public ResultListAdapter(Activity context, float textWeight, int textResId)
            : this(context, textWeight, textResId, false)
        {
        }

        public ResultListAdapter(Activity context, float textWeight)
            : this(context, textWeight, 0, false)
        {
        }

        public ResultListAdapter(Activity context)
            : this(context, -1, 0, false)
        {
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TableResultItem this[int position] => _items[position];

        public override int Count => _items.Count;

        public override bool IsEnabled(int position)
        {
            return false;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            TableResultItem item = _items[position];

            View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.result_list, null);
            CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxResultSelect);

            if (_showCheckBox)
            {
                checkBoxSelect.Visibility = item.CheckVisible ? ViewStates.Visible : ViewStates.Invisible;
            }
            else
            {
                checkBoxSelect.Visibility = ViewStates.Gone;
            }
            _ignoreCheckEvent = true;
            checkBoxSelect.Checked = item.Selected;
            _ignoreCheckEvent = false;

            checkBoxSelect.Tag = new TagInfo(item);
            checkBoxSelect.CheckedChange -= OnCheckChanged;
            checkBoxSelect.CheckedChange += OnCheckChanged;

            TextView textView1 = view.FindViewById<TextView>(Resource.Id.ListText1);
            TextView textView2 = view.FindViewById<TextView>(Resource.Id.ListText2);

            if (textView1 != null)
            {
                try
                {
                    textView1.Text = item.Text1;
                    SetTextApperance(textView1, _textResId);
                    if (item.TextColor != null)
                    {
                        textView1.SetTextColor(item.TextColor.Value);
                    }
                    else
                    {
                        textView1.SetTextColor(_defaultTextColors);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (item.Text2 == null)
            {
                textView2.Visibility = ViewStates.Gone;
            }
            else
            {
                if (textView2 != null)
                {
                    try
                    {
                        textView2.Visibility = ViewStates.Visible;
                        textView2.Text = item.Text2;
                        SetTextApperance(textView2, _textResId);
                        if (item.TextColor != null)
                        {
                            textView2.SetTextColor(item.TextColor.Value);
                        }
                        else
                        {
                            textView2.SetTextColor(_defaultTextColors);
                        }
                        if (_textWeight >= 0)
                        {
                            LinearLayout.LayoutParams layoutPar = (LinearLayout.LayoutParams)textView2.LayoutParameters;
                            layoutPar.Weight = _textWeight;
                            textView2.LayoutParameters = layoutPar;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return view;
        }

        private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
        {
            if (!_ignoreCheckEvent)
            {
                CheckBox checkBox = (CheckBox)sender;
                TagInfo tagInfo = (TagInfo)checkBox.Tag;
                if (tagInfo.Info.Selected != args.IsChecked)
                {
                    tagInfo.Info.Selected = args.IsChecked;
                    NotifyDataSetChanged();
                }
            }
        }

        private void SetTextApperance(TextView textView, int resId)
        {
            if (resId == 0)
            {
                return;
            }
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable 618
                textView.SetTextAppearance(_context, resId);
#pragma warning restore 618
            }
            else
            {
                textView.SetTextAppearance(resId);
            }
        }

        private class TagInfo : Java.Lang.Object
        {
            public TagInfo(TableResultItem info)
            {
                Info = info;
            }

            public TableResultItem Info { get; }
        }
    }

    public class TableResultItem
    {
        private bool _selected;
        public delegate void CheckChangeEventHandler(TableResultItem item);
        public event CheckChangeEventHandler CheckChangeEvent;

        public TableResultItem(string text1, string text2, object tag, bool checkVisible, bool selected, Android.Graphics.Color? textColor)
        {
            Text1 = text1;
            Text2 = text2;
            Tag = tag;
            CheckVisible = checkVisible;
            _selected = selected;
            TextColor = textColor;
        }

        public TableResultItem(string text1, string text2, object tag, bool checkVisible, bool selected)
            : this(text1, text2, tag, checkVisible, selected, null)
        {
        }

        public TableResultItem(string text1, string text2, object tag)
            : this(text1, text2, tag, false, false, null)
        {
        }

        public TableResultItem(string text1, string text2)
            : this(text1, text2, null, false, false, null)
        {
        }

        public string Text1 { get; }

        public string Text2 { get; }

        public object Tag { get; }

        public bool CheckVisible { get; }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                bool changed = _selected != value && CheckChangeEvent != null;
                _selected = value;
                if (changed)
                {
                    CheckChangeEvent(this);
                }
            }
        }

        public Android.Graphics.Color? TextColor { get; }
    }
}
