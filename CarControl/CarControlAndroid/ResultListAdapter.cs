using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CarControlAndroid
{
    class ResultListAdapter : BaseAdapter<TableResultItem>
    {
        private List<TableResultItem> items;
        public List<TableResultItem> Items
        {
            get
            {
                return items;
            }
        }
        Activity context;
        public ResultListAdapter(Activity context)
            : base()
        {
            this.context = context;
            this.items = new List<TableResultItem> ();
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
            view.FindViewById<TextView>(Resource.Id.ListText1).Text = item.Text1;
            view.FindViewById<TextView>(Resource.Id.ListText2).Text = item.Text2;

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
