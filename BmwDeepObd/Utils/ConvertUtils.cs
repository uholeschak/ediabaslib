using Android.OS;

namespace BmwDeepObd
{
    public static class ConvertUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(ConvertUtils).FullName;
#endif

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
