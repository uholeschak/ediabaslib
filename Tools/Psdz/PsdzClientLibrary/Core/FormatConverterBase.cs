using System.Globalization;
using System;

namespace PsdzClient.Core
{
    public class FormatConverterBase
    {
        public static DateTime C_DATE2DateTime(string constructionDate, string month, string year, ILogger logger)
        {
            try
            {
                if (!string.IsNullOrEmpty(constructionDate))
                {
                    if (DateTime.TryParseExact(constructionDate, "MMyy", null, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                    logger.Warning(logger.CurrentMethod(), "failed when TryParseExact() with the string {0}.", constructionDate);
                    if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
                    {
                        return new DateTime(int.Parse(year), int.Parse(month), 1);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warning(logger.CurrentMethod(), "failed with exception: {0}", ex.ToString());
            }
            return DateTime.Now;
        }

        public static string FillWithZeros(string text, int targetLength, ILogger logger)
        {
            try
            {
                if (targetLength < 0)
                {
                    return text;
                }
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Length == targetLength)
                    {
                        return text;
                    }
                    if (text.Length < targetLength)
                    {
                        int num = targetLength - text.Length;
                        return string.Format(CultureInfo.InvariantCulture, "{0:d" + num + "}{1}", 0, text);
                    }
                    if (text.Length > targetLength)
                    {
                        logger.Warning("FormatConverter.FillWithZeros()", "text was longer than target length; will be cut");
                        return text.Substring(0, targetLength);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warning("FormatConverter.FillWithZeros()", "failed with exception: {0}", ex.ToString());
            }
            return string.Format("{0:d" + targetLength + "}", 0);
        }

        public static string Dec2Hex(long? sgAdr)
        {
            if (!sgAdr.HasValue)
            {
                return "null";
            }
            return ((int)sgAdr.Value).ToString("X2");
        }
    }
}