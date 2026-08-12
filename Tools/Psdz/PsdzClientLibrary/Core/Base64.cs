using System;

namespace PsdzClient.Core
{
    [AuthorAPI]
    public static class Base64
    {
        public static byte[] FromBase64String(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                {
                    return Convert.FromBase64String(input);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Base64.FromBase64String()", exception);
            }
            return null;
        }

        public static string ToBase64(byte[] buf)
        {
            try
            {
                if (buf != null)
                {
                    return Convert.ToBase64String(buf);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Base64.ToBase64()", exception);
            }
            return null;
        }
    }
}
