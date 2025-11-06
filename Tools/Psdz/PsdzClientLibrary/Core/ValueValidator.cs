using System;
using System.Collections.Generic;
using System.Linq;

namespace PsdzClient.Core
{
    public class ValueValidator : IValueValidator
    {
        public bool IsValid<T>(string propertyName, object value)
        {
            if (value == null)
            {
                return false;
            }

            if (!(value is string value2))
            {
                if (!(value is DateTime value3))
                {
                    if (value is IEnumerable<string> value4 && !ValidateStringCollection(value4))
                    {
                        return false;
                    }
                }
                else if (!ValidateDateTime(value3))
                {
                    return false;
                }
            }
            else if (!ValidateString(value2))
            {
                return false;
            }

            return true;
        }

        private static bool ValidateString(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        private static bool ValidateDateTime(DateTime value)
        {
            if (value == DateTime.MinValue)
            {
                return false;
            }

            return true;
        }

        private static bool ValidateStringCollection(IEnumerable<string> value)
        {
            if (value == null || value.Count() == 0)
            {
                return false;
            }

            return true;
        }
    }
}