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

            TextView textViewCaption = view.FindViewById<TextView>(Resource.Id.textStringCaption);
            textViewCaption.Text = item.Caption;
            textViewCaption.Visibility = string.IsNullOrWhiteSpace(item.Description) ? ViewStates.Gone : ViewStates.Visible;

            TextView textViewContent = view.FindViewById<TextView>(Resource.Id.textStringContent);
            textViewContent.Text = item.Text;

            TextView textViewDesc = view.FindViewById<TextView>(Resource.Id.textStringDesc);
            textViewDesc.Text = item.Description;
            textViewDesc.Visibility = string.IsNullOrWhiteSpace(item.Description) ? ViewStates.Gone : ViewStates.Visible;

            return view;
        }
    }

    public class StringObjType
    {
        public StringObjType(string text, object data, Android.Graphics.Color? backgroundColor = null) :
            this(text, string.Empty, string.Empty, data, backgroundColor)
        {
        }

        public StringObjType(string text, string caption, string description, object data, Android.Graphics.Color? backgroundColor = null)
        {
            Text = text;
            Caption = caption;
            Description = description;
            Data = data;
            BackgroundColor = backgroundColor;
        }
        public string Text { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public object Data { get; set; }
        public Android.Graphics.Color? BackgroundColor { get; set; }
    }
}
