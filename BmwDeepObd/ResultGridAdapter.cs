using System;
using System.Collections.Generic;
using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Pl.Pawelkleczkowski.CGauge;

namespace BmwDeepObd
{
    public class ResultGridAdapter : BaseAdapter<GridResultItem>
    {
        private const int GaugeBaseSize = 200;
        private readonly List<GridResultItem> _items;
        public List<GridResultItem> Items => _items;
        private readonly Activity _context;

        public ResultGridAdapter(Activity context)
        {
            _context = context;
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
            GridResultItem item = _items[position];

            View view = convertView;
            if (convertView == null || convertView.Id != item.ResourceId)
            {
                view = _context.LayoutInflater.Inflate(item.ResourceId, null);
                view.Id = item.ResourceId;
            }
            int gaugePadding = 20 * item.GaugeSize / GaugeBaseSize;
            int gaugeInnerSize = item.GaugeSize - (2 * gaugePadding);

            CustomGauge customGauge = view.FindViewById<CustomGauge>(Resource.Id.custom_gauge);
            if (customGauge != null)
            {
                try
                {
                    ViewGroup.LayoutParams layoutParams = customGauge.LayoutParameters;
                    layoutParams.Width = gaugeInnerSize;
                    layoutParams.Height = gaugeInnerSize;
                    customGauge.LayoutParameters = layoutParams;

                    customGauge.SetPadding(gaugePadding, gaugePadding, gaugePadding, gaugePadding);

                    int strokeWidth = string.Compare(customGauge.StrokeCap, "BUTT", StringComparison.OrdinalIgnoreCase) == 0 ? 20 : 10;
                    customGauge.StrokeWidth = (float)strokeWidth * gaugeInnerSize / GaugeBaseSize;
                    customGauge.Init();
                    Android.Graphics.Color gaugeColor = Android.Graphics.Color.White;
                    if (item.GaugeColor.HasValue)
                    {
                        gaugeColor = item.GaugeColor.Value;
                    }
                    customGauge.PointStartColor = gaugeColor;
                    customGauge.PointEndColor = gaugeColor;

                    int gaugeScale = customGauge.EndValue;
                    double range = item.MaxValue - item.MinValue;
                    int gaugeValue = 0;
                    if (Math.Abs(range) > 0.00001)
                    {
                        gaugeValue = (int)((item.Value - item.MinValue) / range * gaugeScale);
                        if (gaugeValue < 0)
                        {
                            gaugeValue = 0;
                        }
                        else if (gaugeValue > gaugeScale)
                        {
                            gaugeValue = gaugeScale;
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
                    RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams) textViewGaugeValue.LayoutParameters;
                    layoutParams.BottomMargin = 80 * gaugeInnerSize / GaugeBaseSize;
                    textViewGaugeValue.LayoutParameters = layoutParams;
                    textViewGaugeValue.SetTextSize(ComplexUnitType.Px, (float)30 * gaugeInnerSize / GaugeBaseSize);
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
                    RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)textViewGaugeName.LayoutParameters;
                    layoutParams.TopMargin = -30 * gaugeInnerSize / GaugeBaseSize;
                    textViewGaugeName.LayoutParameters = layoutParams;
                    textViewGaugeName.SetTextSize(ComplexUnitType.Px, (float)20 * gaugeInnerSize / GaugeBaseSize);
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
        public GridResultItem(int resourceId, string name, string valueText, double minValue, double maxValue, double value, int gaugeSize, Android.Graphics.Color? gaugeColor)
        {
            ResourceId = resourceId;
            Name = name;
            ValueText = valueText;
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;
            GaugeSize = gaugeSize;
            GaugeColor = gaugeColor;
        }

        public int ResourceId { get; }

        public string Name { get; }

        public string ValueText { get; }

        public double MinValue { get; }

        public double MaxValue { get; }

        public double Value { get; set; }

        public int GaugeSize { get; set; }

        public Android.Graphics.Color? GaugeColor { get; set; }
    }
}
