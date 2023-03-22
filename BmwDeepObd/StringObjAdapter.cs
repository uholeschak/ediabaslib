using System.Collections.Generic;
using Android.Content.Res;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    public class StringObjAdapter : BaseAdapter<StringObjType>
    {
        private readonly List<StringObjType> _items;

        public List<StringObjType> Items => _items;

        private readonly Android.App.Activity _context;
        private readonly Android.Graphics.Color _backgroundColor;

        public StringObjAdapter(Android.App.Activity context)
        {
            _context = context;
            _items = new List<StringObjType>();
            TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                new[] { Android.Resource.Attribute.ColorBackground });
            _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override StringObjType this[int position] => _items[position];

        public override int Count => _items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.string_list, null);
            Android.Graphics.Color backgroundColor = item.BackgroundColor.HasValue ? item.BackgroundColor.Value : _backgroundColor;
            view.SetBackgroundColor(backgroundColor);

            TextView textViewEntry = view.FindViewById<TextView>(Resource.Id.textStringEntry);
            textViewEntry.Text = item.Text1;

            TextView textViewDesc = view.FindViewById<TextView>(Resource.Id.textStringDesc);
            textViewDesc.Text = item.Text2;
            textViewDesc.Visibility = string.IsNullOrEmpty(item.Text2) ? ViewStates.Gone : ViewStates.Visible;

            return view;
        }
    }

    public class StringObjType
    {
        public StringObjType(string text1, object data, Android.Graphics.Color? backgroundColor = null) :
            this(text1, string.Empty, data, backgroundColor)
        {
        }

        public StringObjType(string text1, string text2, object data, Android.Graphics.Color? backgroundColor = null)
        {
            Text1 = text1;
            Text2 = text2;
            Data = data;
            BackgroundColor = backgroundColor;
        }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public object Data { get; set; }
        public Android.Graphics.Color? BackgroundColor { get; set; }
    }
}
