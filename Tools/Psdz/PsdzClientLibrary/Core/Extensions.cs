using PsdzClientLibrary.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
	public static class Extensions
	{
		public static string ToStringItems(this IEnumerable enumerable)
		{
			return enumerable.ToStringItems("{0}");
		}

		public static string ToStringItems(this IEnumerable enumerable, string format)
		{
			StringBuilder stringBuilder = new StringBuilder("[");
			foreach (object obj in enumerable)
			{
				string value;
				if (obj == null)
				{
					value = "null";
				}
				else
				{
					try
					{
						value = string.Format(CultureInfo.InvariantCulture, format, obj);
					}
					catch (Exception exception)
					{
						Log.WarningException("Extensions.ToStringItems()", exception);
						value = obj.ToString();
					}
				}
				stringBuilder.Append(value).Append(",");
			}
			stringBuilder.Replace(',', ']', stringBuilder.Length - 1, 1);
			if (stringBuilder[stringBuilder.Length - 1] != ']')
			{
				stringBuilder.Append("]");
			}
			return stringBuilder.ToString();
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (T obj in enumerable)
			{
				action(obj);
			}
		}

		public static bool AddIfNotContains<T>(this ICollection<T> collection, T item)
		{
			if (!collection.Contains(item))
			{
				collection.Add(item);
				return true;
			}
			return false;
		}

		public static bool AddIfNotContains<T>(this HashSet<T> collection, T item)
		{
			if (!collection.Contains(item))
			{
				collection.Add(item);
				return true;
			}
			return false;
		}

		public static void AddRangeIfNotContains<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				if (!collection.Contains(item))
				{
					collection.Add(item);
				}
			}
		}

		public static void AddRangeIfNotContains<T>(this BlockingCollection<T> collection, IEnumerable<T> items)
		{
			foreach (T t in items)
			{
				try
				{
					if (!collection.Contains(t))
					{
						collection.Add(t);
					}
				}
				catch (InvalidOperationException)
				{
					break;
				}
			}
		}

		public static void AddRangeIfNotContains<T>(this HashSet<T> collection, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				if (!collection.Contains(item))
				{
					collection.Add(item);
				}
			}
		}

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (items != null)
			{
				foreach (T item in items)
				{
					collection.Add(item);
				}
			}
		}

		public static void AddRangeOverride<T, K>(this IDictionary<T, K> dictionary, IDictionary<T, K> items)
		{
			if (items != null)
			{
				foreach (KeyValuePair<T, K> keyValuePair in items)
				{
					dictionary[keyValuePair.Key] = keyValuePair.Value;
				}
			}
		}

        public static void NotifyPropertyChanged<T>(this PropertyChangedEventHandler handler, object obj, Expression<Func<object>> property, ref T variable, T newValue)
		{
			if ((variable != null && !variable.Equals(newValue)) || (newValue != null && !newValue.Equals(variable)))
			{
				variable = newValue;
				handler.NotifyPropertyChanged(obj, property);
			}
		}

		public static void NotifyPropertyChanged(this PropertyChangedEventHandler handler, object obj, Expression<Func<object>> property)
		{
			if (handler != null)
			{
				handler(obj, new PropertyChangedEventArgs(Extensions.GetPropertyName(property)));
			}
		}

		private static string GetPropertyName(Expression<Func<object>> property)
		{
			MemberExpression memberExpression;
			if (property.Body is UnaryExpression)
			{
				memberExpression = ((property.Body as UnaryExpression).Operand as MemberExpression);
			}
			else
			{
				memberExpression = (property.Body as MemberExpression);
			}
			return (memberExpression.Member as PropertyInfo).Name;
		}

		public static object CreateInstance(this Type type, Type[] constructorParamType, object[] constructorParam)
		{
			object result;
			try
			{
				if (type == null)
				{
					Log.Error("Extensions.CreateInstance()", "type was null!!!!", Array.Empty<object>());
				}
				ConstructorInfo constructor = type.GetConstructor(constructorParamType);
				if (constructor == null)
				{
					throw new ArgumentException(string.Format("No constructor found for type {0} matching the appropriate types.", type.Name));
				}
				object obj = constructor.Invoke(constructorParam);
				if (obj == null)
				{
					throw new Exception("ConstructorInfo.Invoke(object[]) returns null.");
				}
				result = obj;
			}
			catch (Exception innerException)
			{
				StringBuilder stringBuilder = new StringBuilder("[");
				StringBuilder stringBuilder2 = new StringBuilder("[");
				if (constructorParamType == null)
				{
					stringBuilder.Append("null");
				}
				else
				{
					foreach (Type type2 in constructorParamType)
					{
						if (type2 == null)
						{
							stringBuilder.Append("null").Append(",");
						}
						else
						{
							stringBuilder.Append(type2.FullName).Append(",");
						}
					}
					if (','.Equals(stringBuilder[stringBuilder.Length - 1]))
					{
						stringBuilder.Length--;
					}
				}
				stringBuilder.Append("]");
				if (constructorParam == null)
				{
					stringBuilder2.Append("null");
				}
				else
				{
					foreach (object obj2 in constructorParam)
					{
						if (obj2 == null)
						{
							stringBuilder2.Append("null").Append(",");
						}
						else
						{
							stringBuilder2.Append(obj2.GetType().FullName).Append(",");
						}
					}
					if (','.Equals(stringBuilder2[stringBuilder2.Length - 1]))
					{
						stringBuilder2.Length--;
					}
				}
				stringBuilder2.Append("]");
				throw new Exception(string.Format("Failed to create instance of type {0}, with parameter types {1} and parameters {2}.", type.FullName, stringBuilder, stringBuilder2), innerException);
			}
			return result;
		}

		public static T ParseEnum<T>(this string value)
		{
			T result;
			try
			{
				result = (T)((object)Enum.Parse(typeof(T), value));
			}
			catch (Exception innerException)
			{
				string[] names = Enum.GetNames(typeof(T));
				throw new ArgumentException(string.Concat(new string[]
				{
					"Failed to convert \"",
					value,
					"\" into enumeration of type \"",
					typeof(T).Name,
					"\" with posible values ",
					names.ToStringItems(),
					"."
				}), "value", innerException);
			}
			return result;
		}

		public static T Convert<T>(this Enum value)
		{
			string text = value.ToString();
			T result;
			try
			{
				result = (T)((object)Enum.Parse(typeof(T), text));
			}
			catch (Exception innerException)
			{
				string[] names = Enum.GetNames(typeof(T));
				throw new Exception(string.Concat(new string[]
				{
					"Failed to convert \"",
					text,
					"\" into enumeration of type \"",
					typeof(T).Name,
					"\" with possible values ",
					names.ToStringItems(),
					"."
				}), innerException);
			}
			return result;
		}

		public static string ToString(this Enum enumValue, bool useXmlEnumAttribute)
		{
			if (!useXmlEnumAttribute)
			{
				return enumValue.ToString();
			}
			Type type = enumValue.GetType();
			string text = enumValue.ToString("G");
			FieldInfo field = type.GetField(text);
			if (field.IsDefined(typeof(XmlEnumAttribute), false))
			{
				object[] customAttributes = field.GetCustomAttributes(typeof(XmlEnumAttribute), false);
				if (customAttributes != null && customAttributes.Length != 0)
				{
					return ((XmlEnumAttribute)customAttributes[0]).Name;
				}
			}
			return text;
		}

		public static bool IsBitSet<T>(this T t, int pos) where T : struct, IConvertible
		{
			return (t.ToInt64(CultureInfo.CurrentCulture) & 1L << (pos & 31)) != 0L;
		}

		public static string ToFileSize(this long value)
		{
			string[] array = new string[]
			{
				"bytes",
				"KB",
				"MB",
				"GB",
				"TB",
				"PB",
				"EB",
				"ZB",
				"YB"
			};
			for (int i = 0; i < array.Length; i++)
			{
				if ((double)value <= Math.Pow(1024.0, (double)(i + 1)))
				{
					return Extensions.ThreeNonZeroDigits((double)value / Math.Pow(1024.0, (double)i)) + " " + array[i];
				}
			}
			return Extensions.ThreeNonZeroDigits((double)value / Math.Pow(1024.0, (double)(array.Length - 1))) + " " + array[array.Length - 1];
		}

		public static string[] TrimSplit(this string value, char separator)
		{
			if (value == null)
			{
				return null;
			}
			return (from x in value.Split(new char[]
			{
				separator
			})
					select x.Trim()).ToArray<string>();
		}

		private static string ThreeNonZeroDigits(double value)
		{
			if (value >= 100.0)
			{
				return value.ToString("0,0");
			}
			if (value >= 10.0)
			{
				return value.ToString("0.0");
			}
			return value.ToString("0.00");
		}

		public static string ToGeneralDateLongTimeWithMiliseconds(this DateTime dateTime)
		{
			string format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern + " " + Regex.Replace(DateTimeFormatInfo.CurrentInfo.LongTimePattern, "(:ss|:s)", "$1.fff");
			return dateTime.ToString(format);
		}

        public static string ToMileageDisplayFormat(this decimal? mileage, bool newFaultMemory)
        {
            if (!mileage.HasValue)
            {
                return null;
            }
            if (newFaultMemory && mileage.Value != -1m)
            {
                return mileage.Value.ToString("0.0");
            }
            return ((int)mileage.Value).ToString();
        }

        public static string ToMileageDisplayFormat(this double mileage, bool newFaultMemory)
        {
            return ToMileageDisplayFormat(System.Convert.ToDecimal(mileage), newFaultMemory);
        }

        public static string ToStringWithSeparator(this IEnumerable<string> enumerable, string separator)
        {
            return string.Join(separator, enumerable.ToArray());
        }
    }
}
