using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    public class AndroidUtility
    {
        public static void SetListViewHeightBasedOnChildren(ListView listView)
        {
            if (listView.Adapter == null)
            {
                // pre-condition
                return;
            }

            int totalHeight = listView.PaddingTop + listView.PaddingBottom;
            int desiredWidth = View.MeasureSpec.MakeMeasureSpec(listView.Width, MeasureSpecMode.AtMost);
            for (int i = 0; i < listView.Count; i++)
            {
                View listItem = listView.Adapter.GetView(i, null, listView);
                if (listItem != null)
                {
                    if (listItem.GetType() == typeof(ViewGroup))
                    {
                        listItem.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    }
                    listItem.Measure(desiredWidth, View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                    totalHeight += listItem.MeasuredHeight;
                }
            }

            listView.LayoutParameters.Height = totalHeight + (listView.DividerHeight * (listView.Count - 1));

            if (!listView.IsInLayout)
            {
                listView.RequestLayout();
            }
        }
    }
}
