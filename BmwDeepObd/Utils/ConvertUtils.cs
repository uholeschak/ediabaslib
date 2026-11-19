using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;

namespace BmwDeepObd
{
    public static class ConvertUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(ConvertUtils).FullName;
#endif

        private class HandlerUrlSpan : URLSpan
        {
            private readonly System.Func<string, bool> _linkClickHandler;

            public HandlerUrlSpan(string url, System.Func<string, bool> linkClickHandler) : base(url)
            {
                _linkClickHandler = linkClickHandler;
            }

            public override void OnClick(View widget)
            {
                if (_linkClickHandler != null && _linkClickHandler(URL))
                {
                    return;
                }

                base.OnClick(widget);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static Java.Lang.ICharSequence FromHtml(string source)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return Android.Text.Html.FromHtml(source, Android.Text.FromHtmlOptions.ModeLegacy);
            }
#pragma warning disable 618
#pragma warning disable CA1422
            return Android.Text.Html.FromHtml(source);
#pragma warning restore CA1422
#pragma warning restore 618
        }

        // Link clicks call linkClickHandler, return true if handled (otherwise default ACTION_VIEW)
        public static Java.Lang.ICharSequence FromHtml(string source, System.Func<string, bool> linkClickHandler)
        {
            Java.Lang.ICharSequence text = FromHtml(source);
            if (linkClickHandler == null)
            {
                return text;
            }

            SpannableString spannable = new SpannableString(text);
            Java.Lang.Object[] urlSpans = spannable.GetSpans(0, spannable.Length(), Java.Lang.Class.FromType(typeof(URLSpan)));
            if (urlSpans != null)
            {
                foreach (Java.Lang.Object spanObject in urlSpans)
                {
                    if (spanObject is URLSpan urlSpan)
                    {
                        int start = spannable.GetSpanStart(urlSpan);
                        int end = spannable.GetSpanEnd(urlSpan);
                        SpanTypes flags = spannable.GetSpanFlags(urlSpan);
                        spannable.RemoveSpan(urlSpan);
                        spannable.SetSpan(new HandlerUrlSpan(urlSpan.URL, linkClickHandler), start, end, flags);
                    }
                }
            }

            return spannable;
        }

        public static int DecToBcd(int value)
        {
            return (((value / 10) << 4) | (value % 10));
        }

        public static int BcdToDec(int value)
        {
            return (((value >> 4) * 10) + (value & 0xF));
        }
    }
}
