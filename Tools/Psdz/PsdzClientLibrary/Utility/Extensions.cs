using System;

namespace PsdzClient.Utility
{
    [PreserveSource(Hint = "Do not update, only used for logging")]
    public static class Extensions
    {
        public static string ToText<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (!value.ToString().StartsWith("0A", StringComparison.Ordinal))
            {
                return "0A_" + value.ToString();
            }
            return value.ToString();
        }
    }
}