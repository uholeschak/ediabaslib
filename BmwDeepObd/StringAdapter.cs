using System.Collections.Generic;
using Android.Content.Res;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    public class StringAdapter : BaseAdapter<string>
    {
        private readonly List<string> _items;

        public List<string> Items => _items;

        private readonly Android.App.Activity _context;
        private readonly Android.Graphics.Color _backgroundColor;

        public StringAdapter(Android.App.Activity context)
        {
            _context = context;
            _items = new List<string>();
            TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                new[] { Android.Resource.Attribute.ColorBackground });
            _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position] => _items[position];

        public override int Count => _items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.string_list, null);
            view.SetBackgroundColor(_backgroundColor);

            TextView textViewEntry = view.FindViewById<TextView>(Resource.Id.textStringEntry);
            textViewEntry.Text = item;

            TextView textViewDesc = view.FindViewById<TextView>(Resource.Id.textStringDesc);
            textViewDesc.Visibility = ViewStates.Gone;

            return view;
        }
    }
}
