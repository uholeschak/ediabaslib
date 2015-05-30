using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace CarControlAndroid
{
    public class ResultListAdapter : BaseAdapter<TableResultItem>
    {
        private List<TableResultItem> items;
        public List<TableResultItem> Items
        {
            get
            {
                return items;
            }
        }
        private Activity context;
        private float textWeight;

        public ResultListAdapter(Activity context, float textWeight)
            : base()
        {
            this.context = context;
            this.items = new List<TableResultItem> ();
            this.textWeight = textWeight;
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
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];

            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.result_list, null);
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
                if (textWeight >= 0)
                {
                    LinearLayout.LayoutParams layoutPar = (LinearLayout.LayoutParams)textView2.LayoutParameters;
                    layoutPar.Weight = textWeight;
                    textView2.LayoutParameters = layoutPar;
                }
            }

            return view;
        }
    }

    public class TableResultItem
    {
        private string text1;
        private string text2;

        public TableResultItem(string text1, string text2)
        {
            this.text1 = text1;
            this.text2 = text2;
        }

        public string Text1
        {
            get
            {
                return text1;
            }
        }

        public string Text2
        {
            get
            {
                return text2;
            }
        }
    }
}
