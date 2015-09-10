using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace BmwDiagnostics
{
    public class ResultListAdapter : BaseAdapter<TableResultItem>
    {
        private readonly List<TableResultItem> _items;
        public List<TableResultItem> Items
        {
            get
            {
                return _items;
            }
        }
        private readonly Activity _context;
        private readonly float _textWeight;

        public ResultListAdapter(Activity context, float textWeight)
        {
            _context = context;
            _items = new List<TableResultItem> ();
            _textWeight = textWeight;
        }

        public ResultListAdapter(Activity context) : this(context, -1)
        {
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TableResultItem this[int position]
        {
            get { return _items[position]; }
        }

        public override int Count
        {
            get { return _items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.result_list, null);
            TextView textView1 = view.FindViewById<TextView>(Resource.Id.ListText1);
            TextView textView2 = view.FindViewById<TextView>(Resource.Id.ListText2);
            textView1.Text = item.Text1;
            if (item.Text2 == null)
            {
                textView2.Visibility = ViewStates.Gone;
            }
            else
            {
                textView2.Visibility = ViewStates.Visible;
                textView2.Text = item.Text2;
                if (_textWeight >= 0)
                {
                    LinearLayout.LayoutParams layoutPar = (LinearLayout.LayoutParams)textView2.LayoutParameters;
                    layoutPar.Weight = _textWeight;
                    textView2.LayoutParameters = layoutPar;
                }
            }

            return view;
        }
    }

    public class TableResultItem
    {
        private readonly string _text1;
        private readonly string _text2;

        public TableResultItem(string text1, string text2)
        {
            _text1 = text1;
            _text2 = text2;
        }

        public string Text1
        {
            get
            {
                return _text1;
            }
        }

        public string Text2
        {
            get
            {
                return _text2;
            }
        }
    }
}
