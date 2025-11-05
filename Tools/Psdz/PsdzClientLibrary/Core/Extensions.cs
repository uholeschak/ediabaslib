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
using System.Threading;
using System.Windows.Threading;
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
            foreach (object item in enumerable)
            {
                string value;
                if (item == null)
                {
                    value = "null";
                }
                else
                {
                    try
                    {
                        value = string.Format(CultureInfo.InvariantCulture, format, item);
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException("Extensions.ToStringItems()", exception);
                        value = item.ToString();
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
            foreach (T item in enumerable)
            {
                action(item);
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
            foreach (T item in items)
            {
                try
                {
                    if (!collection.Contains(item))
                    {
                        collection.Add(item);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Log.Info("Extestions.AddRangeIfNotContains()", "Failed to add to blocking collection. {0}", ex);
                    break;
                }
            }
        }

        public static void AddIfNotContains<T>(this BlockingCollection<T> collection, T item)
        {
            try
            {
                if (!collection.Contains(item))
                {
                    collection.Add(item);
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.Info("Extestions.AddRangeIfNotContains()", "Failed to add to blocking collection. {0}", ex);
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
            if (items == null)
            {
                return;
            }

            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddRangeOverride<T, K>(this IDictionary<T, K> dictionary, IDictionary<T, K> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (KeyValuePair<T, K> item in items)
            {
                dictionary[item.Key] = item.Value;
            }
        }

        public static void InvokeIfNoAccess(this Dispatcher dispatcher, Action action)
        {
            try
            {
                if (dispatcher == null || dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    dispatcher.Invoke(action);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Extensions.InvokeIfNoAccess()", exception);
            }
        }

        public static void InvokeIfNoAccessException(this Dispatcher dispatcher, Action action)
        {
            try
            {
                if (dispatcher == null || dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    dispatcher.Invoke(action);
                }
            }
            catch (UserCanceledException ex)
            {
                Log.Info("Extensions.InvokeIfNoAccessException()", "User canceled action \"{0}\".", ex.Message);
                Log.Flush();
                throw;
            }
            catch (Exception exception)
            {
                Log.ErrorException("Extensions.InvokeIfNoAccessException()", exception);
            }
        }

        public static void InvokeIfNoAccess(this Dispatcher dispatcher, Action action, string actionName, long timeout)
        {
            try
            {
                if (dispatcher == null || dispatcher.CheckAccess())
                {
                    action();
                    return;
                }

                DispatcherOperation dispatcherOperation = dispatcher.BeginInvoke(action);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                do
                {
                    Thread.Sleep(300);
                }
                while (stopwatch.ElapsedMilliseconds > timeout || dispatcherOperation.Status != DispatcherOperationStatus.Completed);
                stopwatch.Stop();
                if (dispatcherOperation.Status != DispatcherOperationStatus.Completed)
                {
                    Log.Error("Extensions.InvokeIfNoAccess()", "Action \"{0}\" was in status \"{1}\", when timeout {2} reached.", actionName, dispatcherOperation.Status, timeout);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Extensions.InvokeIfNoAccess()", exception);
            }
        }

        public static void InvokeIfNoAccess(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            try
            {
                if (dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    dispatcher.Invoke(action, priority);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Extensions.InvokeIfNoAccess()", exception);
            }
        }

        public static void NotifyPropertyChanged<T>(this PropertyChangedEventHandler handler, object obj, Expression<Func<object>> property, ref T variable, T newValue)
        {
            if (variable != null)
            {
                object obj2 = newValue;
                if (!variable.Equals(obj2))
                {
                    goto IL_0045;
                }
            }

            if (newValue == null || newValue.Equals(variable))
            {
                return;
            }

            goto IL_0045;
            IL_0045:
                variable = newValue;
            handler.NotifyPropertyChanged(obj, property);
        }

        public static void NotifyPropertyChanged(this PropertyChangedEventHandler handler, object obj, Expression<Func<object>> property)
        {
            handler?.Invoke(obj, new PropertyChangedEventArgs(GetPropertyName(property)));
        }

        private static string GetPropertyName(Expression<Func<object>> property)
        {
            MemberExpression memberExpression = ((!(property.Body is UnaryExpression)) ? (property.Body as MemberExpression) : ((property.Body as UnaryExpression).Operand as MemberExpression));
            return (memberExpression.Member as PropertyInfo).Name;
        }

        public static object CreateInstance(this Type type, Type[] constructorParamType, object[] constructorParam)
        {
            try
            {
                if (type == null)
                {
                    Log.Error("Extensions.CreateInstance()", "type was null!!!!");
                }

                ConstructorInfo constructor = type.GetConstructor(constructorParamType);
                if (constructor == null)
                {
                    throw new ArgumentException($"No constructor found for type {type.Name} matching the appropriate types.");
                }

                return constructor.Invoke(constructorParam) ?? throw new Exception("ConstructorInfo.Invoke(object[]) returns null.");
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
                    foreach (object obj in constructorParam)
                    {
                        if (obj == null)
                        {
                            stringBuilder2.Append("null").Append(",");
                        }
                        else
                        {
                            stringBuilder2.Append(obj.GetType().FullName).Append(",");
                        }
                    }

                    if (','.Equals(stringBuilder2[stringBuilder2.Length - 1]))
                    {
                        stringBuilder2.Length--;
                    }
                }

                stringBuilder2.Append("]");
                throw new Exception($"Failed to create instance of type {type.FullName}, with parameter types {stringBuilder} and parameters {stringBuilder2}.", innerException);
            }
        }

        public static T ParseEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value);
            }
            catch (Exception innerException)
            {
                string[] names = Enum.GetNames(typeof(T));
                throw new ArgumentException("Failed to convert \"" + value + "\" into enumeration of type \"" + typeof(T).Name + "\" with posible values " + names.ToStringItems() + ".", "value", innerException);
            }
        }

        public static T Convert<T>(this Enum value)
        {
            string text = value.ToString();
            try
            {
                return (T)Enum.Parse(typeof(T), text);
            }
            catch (Exception innerException)
            {
                string[] names = Enum.GetNames(typeof(T));
                throw new Exception("Failed to convert \"" + text + "\" into enumeration of type \"" + typeof(T).Name + "\" with possible values " + names.ToStringItems() + ".", innerException);
            }
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
            if (field.IsDefined(typeof(XmlEnumAttribute), inherit: false))
            {
                object[] customAttributes = field.GetCustomAttributes(typeof(XmlEnumAttribute), inherit: false);
                if (customAttributes != null && customAttributes.Length != 0)
                {
                    return ((XmlEnumAttribute)customAttributes[0]).Name;
                }
            }

            return text;
        }

        public static bool IsBitSet<T>(this T t, int pos)
            where T : struct, IConvertible
        {
            return (t.ToInt64(CultureInfo.CurrentCulture) & (1 << pos)) != 0;
        }

        public static string ToFileSize(this long value)
        {
            string[] array = new string[9]
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
                if ((double)value <= Math.Pow(1024.0, i + 1))
                {
                    return ThreeNonZeroDigits((double)value / Math.Pow(1024.0, i)) + " " + array[i];
                }
            }

            return ThreeNonZeroDigits((double)value / Math.Pow(1024.0, array.Length - 1)) + " " + array[array.Length - 1];
        }

        public static string[] TrimSplit(this string value, char separator)
        {
            return (
                from x in value?.Split(separator)select x.Trim()).ToArray();
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
            string text = DateTimeFormatInfo.CurrentInfo.ShortDatePattern + " " + Regex.Replace(DateTimeFormatInfo.CurrentInfo.LongTimePattern, "(:ss|:s)", "$1.fff");
            return dateTime.ToString(text);
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

            return Math.Truncate(mileage.Value).ToString();
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