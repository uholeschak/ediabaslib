using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using Pl.Pawelkleczkowski.CGauge;

namespace BmwDeepObd
{
    public class ResultGridAdapter : BaseAdapter<GridResultItem>
    {
        private readonly List<GridResultItem> _items;
        public List<GridResultItem> Items => _items;
        private readonly Activity _context;
        private readonly int _resourceId;
        private const int GaugeScale = 100;

        public ResultGridAdapter(Activity context, int resourceId)
        {
            _context = context;
            _resourceId = resourceId;
            _items = new List<GridResultItem>();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override GridResultItem this[int position] => _items[position];

        public override int Count => _items.Count;

        public override bool IsEnabled(int position)
        {
            return false;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            View view = convertView ?? _context.LayoutInflater.Inflate(_resourceId, null);

            CustomGauge customGauge = view.FindViewById<CustomGauge>(Resource.Id.custom_gauge);
            if (customGauge != null)
            {
                try
                {
                    customGauge.StartValue = 0;
                    customGauge.EndValue = GaugeScale;
                    double range = item.MaxValue - item.MinValue;
                    int gaugeValue = 0;
                    if (Math.Abs(range) > 0.00001)
                    {
                        gaugeValue = (int)((item.Value - item.MinValue) / range * GaugeScale);
                        if (gaugeValue < 0)
                        {
                            gaugeValue = 0;
                        }
                        else if (gaugeValue > GaugeScale)
                        {
                            gaugeValue = GaugeScale;
                        }
                    }
                    customGauge.Value = gaugeValue;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            TextView textViewGaugeValue = view.FindViewById<TextView>(Resource.Id.gauge_value);
            if (textViewGaugeValue != null)
            {
                try
                {
                    textViewGaugeValue.Text = item.ValueText;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            TextView textViewGaugeName = view.FindViewById<TextView>(Resource.Id.gauge_name);
            if (textViewGaugeName != null)
            {
                try
                {
                    textViewGaugeName.Text = item.Name;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return view;
        }
    }

    public class GridResultItem
    {
        public GridResultItem(string name, string valueText, double minValue, double maxValue, double value)
        {
            Name = name;
            ValueText = valueText;
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;
        }

        public string Name { get; }

        public string ValueText { get; }

        public double MinValue { get; }

        public double MaxValue { get; }

        public double Value { get; set; }
    }
}
