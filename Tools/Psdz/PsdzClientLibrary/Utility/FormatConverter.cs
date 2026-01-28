using PsdzClient.Core;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PsdzClient.Utility
{
    public class FormatConverter
    {
        [PreserveSource(Cleaned = true)]
        public static string GetLocalizedREPS(string ecuVariant, string ecuGroup, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public static string GetLocalizedClique(string ecuVariant, string ecuGroup)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public static string GetLocalizedLongName(string ecuVariant, string ecuGroup, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            throw new NotImplementedException();
        }

        public static byte[] Ascii2ByteArray(string asciiText, out int len)
        {
            try
            {
                if (string.IsNullOrEmpty(asciiText))
                {
                    len = 0;
                    return new byte[0];
                }

                asciiText = asciiText.ToString(CultureInfo.InvariantCulture);
                char[] array = asciiText.ToCharArray();
                byte[] array2 = new byte[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array2[i] = (byte)array[i];
                }

                len = array.Length;
                return array2;
            }
            catch (Exception ex)
            {
                Log.Warning("FormatConverter.Ascii2Byte()", "failed with exception: {0}", ex.ToString());
                len = 0;
                return new byte[0];
            }
        }

        public static byte[] HexArray2ByteArray(string hexInBuf)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < hexInBuf.Length; i += 2)
            {
                int num = Convert.ToInt16(hexInBuf.Substring(i, 2), 16);
                list.Add((byte)num);
            }

            return list.ToArray();
        }

        public static string Ascii2UTF8(object textObj)
        {
            try
            {
                if (textObj == null)
                {
                    return null;
                }

                string text = ((!(textObj is string)) ? textObj.ToString() : ((string)textObj));
                if (string.IsNullOrEmpty(text))
                {
                    return string.Empty;
                }

                text = text.ToString(CultureInfo.InvariantCulture);
                char[] array = text.ToCharArray();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] > '\u001f' && array[i] != '\u007f')
                    {
                        switch (array[i])
                        {
                            case 'ﾺ':
                                array[i] = '°';
                                continue;
                            case 'ﾰ':
                                array[i] = '°';
                                continue;
                            case 'ￖ':
                                array[i] = 'Ö';
                                continue;
                            case 'ￔ':
                                array[i] = 'Ä';
                                continue;
                            case 'ￜ':
                                array[i] = 'Ü';
                                continue;
                            case '\ufff6':
                                array[i] = 'ö';
                                continue;
                            case '￤':
                                array[i] = 'ä';
                                continue;
                            case '￼':
                                array[i] = 'ü';
                                continue;
                            case '\uffdf':
                                array[i] = 'ß';
                                continue;
                            case '\uffff':
                                array[i] = ' ';
                                continue;
                            case 'ﾧ':
                                array[i] = '§';
                                continue;
                            case '￭':
                                array[i] = 'í';
                                continue;
                        }

                        if (array[i] >= '\uff00')
                        {
                            byte[] bytes = new UnicodeEncoding().GetBytes(array, i, 1);
                            if (CoreFramework.DebugLevel > 1)
                            {
                                Log.Warning("FormatConverter.Ascii2UTF8()", "for char(hex): {0} in text: {1}", ByteArray2String(bytes, 1u), text);
                            }

                            byte b = (byte)array[i];
                            array[i] = (char)b;
                        }
                    }
                    else
                    {
                        array[i] = '*';
                    }
                }

                return new string (array);
            }
            catch (Exception ex)
            {
                Log.Warning("FormatConverter.Ascii2UTF8()", "failed with exception: {0}", ex.ToString());
                return null;
            }
        }

        public static string ByteArray2String(byte[] param, uint paramlen)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < paramlen; i++)
                {
                    stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", param[i]));
                }

                return stringBuilder.ToString();
            }
            catch (Exception exception)
            {
                Log.WarningException("FormatConverter.ByteArray2String()", exception);
            }

            return string.Empty;
        }

        public static string ByteArray2StringFASTA(byte[] param, uint paramlen)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (uint num = 0u; num < paramlen; num++)
                {
                    stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2} ", param[num]));
                }

                return stringBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                Log.Warning("FormatConverter.ByteArray2StringFASTA()", "failed with exception: {0}", ex.ToString());
            }

            return string.Empty;
        }

        public static string ByteArray2StringJavascript(byte[] param, uint paramlen)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (uint num = 0u; num < paramlen; num++)
                {
                    stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "\\{0}", param[num]));
                }

                return stringBuilder.ToString().Trim();
            }
            catch (Exception exception)
            {
                Log.WarningException("FormatConverter.ByteArray2StringJavascript()", exception);
            }

            return null;
        }

        public static long Hex2Dec(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue))
            {
                return 0L;
            }

            return long.Parse(hexValue, NumberStyles.HexNumber);
        }

        public static string ISTAXmlShaper(string inString)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(inString);
                byte[] array = new byte[bytes.Length];
                bool flag = false;
                int newSize = 0;
                byte[] array2 = bytes;
                foreach (byte b in array2)
                {
                    if (b == 60)
                    {
                        flag = true;
                    }

                    if (flag)
                    {
                        array[newSize++] = b;
                    }
                }

                Array.Resize(ref array, newSize);
                return Encoding.UTF8.GetString(array);
            }
            catch (Exception exception)
            {
                Log.WarningException("FormatConverter.ISTAXmlShaper()", exception);
            }

            return null;
        }

        public static string ConvertECUResultToString(object resultValue)
        {
            if (resultValue is int num)
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }

            if (resultValue is double num2)
            {
                return num2.ToString("0.00", new CultureInfo(ConfigSettings.CurrentUICulture));
            }

            if (resultValue is float num3)
            {
                return num3.ToString("0.00", new CultureInfo(ConfigSettings.CurrentUICulture));
            }

            return resultValue.ToString();
        }

        public static string ConvertECUParamToString(object resultValue)
        {
            if (resultValue is int num)
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }

            if (resultValue is double num2)
            {
                return num2.ToString(CultureInfo.InvariantCulture);
            }

            if (resultValue is float num3)
            {
                return num3.ToString(CultureInfo.InvariantCulture);
            }

            if (resultValue is byte[])
            {
                byte[] obj = (byte[])resultValue;
                return ByteArray2String(obj, (uint)obj.Length);
            }

            return resultValue.ToString();
        }

        public static string CompareChar(char readValue, string stateValue)
        {
            try
            {
                int num = Convert.ToInt16(stateValue, CultureInfo.InvariantCulture);
                if (readValue == num)
                {
                    return stateValue;
                }
            }
            catch (FormatException)
            {
                Log.Error("FormatConverter.CompareChar()", "Couln't convert {0} to Int16!", stateValue);
            }
            catch (OverflowException)
            {
                Log.Error("FormatConverter.CompareChar()", "Couln't convert {0} to Int16! The value is greater than Int16.MaxValue.", stateValue);
            }

            return null;
        }

        public static string CompareString(string readValue, string stateValue)
        {
            if (string.Compare(readValue, stateValue, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return stateValue;
            }

            return null;
        }

        public static int CompareILevels(string ilevel1, string ilevel2)
        {
            int? num = ExtractNumericalILevel(ilevel1);
            int? num2 = ExtractNumericalILevel(ilevel2);
            if (num.HasValue && num2.HasValue)
            {
                if (num == num2)
                {
                    return 0;
                }

                if (num > num2)
                {
                    return 1;
                }

                if (num2 > num)
                {
                    return 2;
                }
            }

            return -1;
        }

        public static string CompareDouble(double readValue, string stateValue)
        {
            try
            {
                double num = Convert.ToDouble(stateValue, CultureInfo.InvariantCulture);
                if (Math.Abs(readValue - num) < 0.0001)
                {
                    return stateValue;
                }
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            return null;
        }

        public static string ASCII2PlainText(string inText)
        {
            if (!string.IsNullOrEmpty(inText))
            {
                return inText.Replace("\r", "_").Replace("\n", "_");
            }

            return inText;
        }

        public static int? ExtractNumericalILevel(string iLevel)
        {
            if (string.IsNullOrEmpty(iLevel) || iLevel.Length != 14)
            {
                Log.Warning("FormatConverter.ExtractNumericalILevel()", "iLevel format was not correct: '{0}'", iLevel);
                return null;
            }

            try
            {
                return Convert.ToInt32(iLevel.Replace("-", string.Empty).Substring(4), CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                Log.WarningException("FormatConverter.ExtractNumericalILevel()", exception);
            }

            return null;
        }

        public static char DecodeFAChar(char inChar)
        {
            byte b = (byte)inChar;
            int num = b >> 4;
            byte b2 = 0;
            switch (num)
            {
                case 1:
                    b2 = 48;
                    break;
                case 2:
                    b2 = 64;
                    break;
                case 3:
                    b2 = 80;
                    break;
                default:
                    Log.Warning("FormatConverter.DecodeFAChar()", "unknown encoding found for character: 0x{0:X} '{1}'", (int)inChar, inChar);
                    break;
            }

            return (char)((b & 0xF) | b2);
        }

        public static string ConvertToBn2020ConformModelSeries(string modelSeries)
        {
            if (!string.IsNullOrWhiteSpace(modelSeries))
            {
                if (modelSeries.StartsWith("NA"))
                {
                    return modelSeries;
                }

                Match match = Regex.Match(modelSeries.Trim(), "^(?<letterpart>[A-Z]+)(?<numberpart>[0-9]+)");
                if (match.Success)
                {
                    string value = match.Groups["letterpart"].Value;
                    string text = match.Groups["numberpart"].Value.TrimStart('0');
                    if (value.Length + text.Length > 4)
                    {
                        Log.Warning("FormatConverter.ConvertToBn2020ConformModelSeries()", "Model series '{0}' exceeds expected length of 4!", modelSeries);
                        return modelSeries;
                    }

                    return value + text.PadLeft(4 - value.Length, '0');
                }

                Log.Warning("FormatConverter.ConvertToBn2020ConformModelSeries()", "Model series '{0}' could not be converted (Invalid pattern)!", modelSeries);
                return modelSeries;
            }

            return null;
        }

        public static string Convert6BitNibblesTo4DigitString(byte[] inBuf, uint offset)
        {
            char c = DecodeFAChar((char)(inBuf[offset] >> 2));
            char c2 = DecodeFAChar((char)(((inBuf[offset] & 3) << 4) | (inBuf[offset + 1] >> 4)));
            char c3 = DecodeFAChar((char)(((inBuf[offset + 1] & 0xF) << 2) | ((inBuf[offset + 2] & 0xC0) >> 6)));
            char c4 = DecodeFAChar((char)(inBuf[offset + 2] & 0x3F));
            return c.ToString(CultureInfo.InvariantCulture) + c2.ToString(CultureInfo.InvariantCulture) + c3.ToString(CultureInfo.InvariantCulture) + c4.ToString(CultureInfo.InvariantCulture);
        }
    }
}