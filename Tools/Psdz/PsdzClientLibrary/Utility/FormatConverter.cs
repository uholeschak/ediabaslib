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
		public static byte[] Ascii2ByteArray(string asciiText, out int len)
		{
			byte[] result;
			try
			{
				if (string.IsNullOrEmpty(asciiText))
				{
					len = 0;
					result = new byte[0];
				}
				else
				{
					asciiText = asciiText.ToString(CultureInfo.InvariantCulture);
					char[] array = asciiText.ToCharArray();
					byte[] array2 = new byte[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = (byte)array[i];
					}
					len = array.Length;
					result = array2;
				}
			}
			catch (Exception)
			{
				len = 0;
				result = new byte[0];
			}
			return result;
		}

		public static byte[] HexArray2ByteArray(string hexInBuf)
		{
			List<byte> list = new List<byte>();
			for (int i = 0; i < hexInBuf.Length; i += 2)
			{
				int num = (int)Convert.ToInt16(hexInBuf.Substring(i, 2), 16);
				list.Add((byte)num);
			}
			return list.ToArray();
		}

		public static string Ascii2UTF8(object textObj)
		{
			string result;
			try
			{
				if (textObj == null)
				{
					result = null;
				}
				else
				{
					string text;
					if (textObj is string)
					{
						text = (string)textObj;
					}
					else
					{
						text = textObj.ToString();
					}
					if (string.IsNullOrEmpty(text))
					{
						result = string.Empty;
					}
					else
					{
						text = text.ToString(CultureInfo.InvariantCulture);
						char[] array = text.ToCharArray();
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] > '\u001f' && array[i] != '\u007f')
							{
								char c = array[i];
								if (c <= 'ￜ')
								{
									if (c <= 'ﾺ')
									{
										if (c == 'ﾧ')
										{
											array[i] = '§';
											goto IL_1F2;
										}
										if (c == 'ﾰ')
										{
											array[i] = '°';
											goto IL_1F2;
										}
										if (c == 'ﾺ')
										{
											array[i] = '°';
											goto IL_1F2;
										}
									}
									else
									{
										if (c == 'ￔ')
										{
											array[i] = 'Ä';
											goto IL_1F2;
										}
										if (c == 'ￖ')
										{
											array[i] = 'Ö';
											goto IL_1F2;
										}
										if (c == 'ￜ')
										{
											array[i] = 'Ü';
											goto IL_1F2;
										}
									}
								}
								else if (c <= '￭')
								{
									if (c == '￟')
									{
										array[i] = 'ß';
										goto IL_1F2;
									}
									if (c == '￤')
									{
										array[i] = 'ä';
										goto IL_1F2;
									}
									if (c == '￭')
									{
										array[i] = 'í';
										goto IL_1F2;
									}
								}
								else
								{
									if (c == '￶')
									{
										array[i] = 'ö';
										goto IL_1F2;
									}
									if (c == '￼')
									{
										array[i] = 'ü';
										goto IL_1F2;
									}
									if (c == '￿')
									{
										array[i] = ' ';
										goto IL_1F2;
									}
								}
								if (array[i] >= '＀')
								{
									byte[] bytes = new UnicodeEncoding().GetBytes(array, i, 1);
									byte b = (byte)array[i];
									array[i] = (char)b;
								}
							}
							else
							{
								array[i] = '*';
							}
							IL_1F2:;
						}
						result = new string(array);
					}
				}
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		public static string ByteArray2String(byte[] param, uint paramlen)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num = 0;
				while ((long)num < (long)((ulong)paramlen))
				{
					stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", param[num]));
					num++;
				}
				return stringBuilder.ToString();
			}
			catch (Exception)
			{
			}
			return string.Empty;
		}

		public static string ByteArray2StringFASTA(byte[] param, uint paramlen)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (uint num = 0U; num < paramlen; num += 1U)
				{
					stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2} ", param[(int)num]));
				}
				return stringBuilder.ToString().Trim();
			}
			catch (Exception)
			{
			}
			return string.Empty;
		}

		public static string ByteArray2StringJavascript(byte[] param, uint paramlen)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (uint num = 0U; num < paramlen; num += 1U)
				{
					stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "\\{0}", param[(int)num]));
				}
				return stringBuilder.ToString().Trim();
			}
			catch (Exception)
			{
			}
			return null;
		}

		public static DateTime C_DATE2DateTime(string constructionDate, string month, string year)
		{
			try
			{
				if (!string.IsNullOrEmpty(constructionDate))
				{
					DateTime result;
					if (DateTime.TryParseExact(constructionDate, "MMyy", null, DateTimeStyles.None, out result))
					{
						return result;
					}
					if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
					{
						return new DateTime(int.Parse(year), int.Parse(month), 1);
					}
				}
			}
			catch (Exception)
			{
			}
			return DateTime.Now;
		}

		public static string Dec2Hex(long? sgAdr)
		{
			if (sgAdr == null)
			{
				return "null";
			}
			return ((int)sgAdr.Value).ToString("X2");
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
				foreach (byte b in bytes)
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
				Array.Resize<byte>(ref array, newSize);
				return Encoding.UTF8.GetString(array);
			}
			catch (Exception)
			{
			}
			return null;
		}

		public static string FillWithZeros(string text, int targetLength)
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
						return text.Substring(0, targetLength);
					}
				}
			}
			catch (Exception)
			{
			}
			return string.Format("{0:d" + targetLength + "}", 0);
		}

		public static string ConvertECUResultToString(object resultValue)
		{
			string result;
			if (resultValue is char)
			{
				result = ((int)((char)resultValue)).ToString(CultureInfo.InvariantCulture);
			}
			else if (resultValue is double)
			{
				result = ((double)resultValue).ToString("0.00", CultureInfo.InvariantCulture);
			}
			else if (resultValue is float)
			{
				result = ((float)resultValue).ToString("0.00", CultureInfo.InvariantCulture);
			}
			else
			{
				result = resultValue.ToString();
			}
			return result;
		}

		public static string ConvertECUParamToString(object resultValue)
		{
			string result;
			if (resultValue is char)
			{
				result = ((int)((char)resultValue)).ToString(CultureInfo.InvariantCulture);
			}
			else if (resultValue is double)
			{
				result = ((double)resultValue).ToString(CultureInfo.InvariantCulture);
			}
			else if (resultValue is float)
			{
				result = ((float)resultValue).ToString(CultureInfo.InvariantCulture);
			}
			else if (resultValue is byte[])
			{
				byte[] array = (byte[])resultValue;
				result = FormatConverter.ByteArray2String(array, (uint)array.Length);
			}
			else
			{
				result = resultValue.ToString();
			}
			return result;
		}

		public static string CompareChar(char readValue, string stateValue)
		{
			try
			{
				int num = (int)Convert.ToInt16(stateValue, CultureInfo.InvariantCulture);
				if ((int)readValue == num)
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

		public static string CompareString(string readValue, string stateValue)
		{
			if (string.Compare(readValue, stateValue, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return stateValue;
			}
			return null;
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
			if (!string.IsNullOrEmpty(iLevel))
			{
				if (iLevel.Length == 14)
				{
					try
					{
						return new int?(Convert.ToInt32(iLevel.Replace("-", string.Empty).Substring(4), CultureInfo.InvariantCulture));
					}
					catch (Exception)
					{
					}
					return null;
				}
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
				default:
					break;
				case 1:
					b2 = 48;
					break;
				case 2:
					b2 = 64;
					break;
				case 3:
					b2 = 80;
					break;
			}
			return (char)((b & 15) | b2);
		}

		public static string ConvertToBn2020ConformModelSeries(string modelSeries)
		{
			if (string.IsNullOrWhiteSpace(modelSeries))
			{
				return null;
			}
			Match match = Regex.Match(modelSeries.Trim(), "^(?<letterpart>[A-Z]+)(?<numberpart>[0-9]+)");
			if (!match.Success)
			{
				return modelSeries;
			}
			string value = match.Groups["letterpart"].Value;
			string text = match.Groups["numberpart"].Value.TrimStart(new char[]
			{
				'0'
			});
			if (value.Length + text.Length > 4)
			{
				return modelSeries;
			}
			return value + text.PadLeft(4 - value.Length, '0');
		}

		public static string Convert6BitNibblesTo4DigitString(byte[] inBuf, uint offset)
		{
			char c = FormatConverter.DecodeFAChar((char)(inBuf[(int)offset] >> 2));
			char c2 = FormatConverter.DecodeFAChar((char)((int)(inBuf[(int)offset] & 3) << 4 | inBuf[(int)(offset + 1U)] >> 4));
			char c3 = FormatConverter.DecodeFAChar((char)((int)(inBuf[(int)(offset + 1U)] & 15) << 2 | (inBuf[(int)(offset + 2U)] & 192) >> 6));
			char c4 = FormatConverter.DecodeFAChar((char)(inBuf[(int)(offset + 2U)] & 63));
			return c.ToString(CultureInfo.InvariantCulture) + c2.ToString(CultureInfo.InvariantCulture) + c3.ToString(CultureInfo.InvariantCulture) + c4.ToString(CultureInfo.InvariantCulture);
		}

	}
}
