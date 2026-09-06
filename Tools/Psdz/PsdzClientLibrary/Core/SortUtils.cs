using PsdzClient.Programming;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework
{
    public static class SortUtils
    {
        public static int CompareDateTime(DateTime? x, DateTime? y)
        {
            if (!x.HasValue)
            {
                if (!y.HasValue)
                {
                    return 0;
                }
                return -1;
            }
            if (!y.HasValue)
            {
                return 1;
            }
            return x.Value.CompareTo(y.Value);
        }

        public static int CompareDate(DateTime? x, DateTime? y)
        {
            if (!x.HasValue)
            {
                if (!y.HasValue)
                {
                    return 0;
                }
                return -1;
            }
            if (!y.HasValue)
            {
                return 1;
            }
            return x.Value.Date.CompareTo(y.Value.Date);
        }

        public static int CompareNullable<T>(T? x, T? y) where T : struct, IComparable
        {
            if (!x.HasValue)
            {
                if (!y.HasValue)
                {
                    return 0;
                }
                return -1;
            }
            if (!y.HasValue)
            {
                return 1;
            }
            return x.Value.CompareTo(y.Value);
        }

        public static int CompareNullableDecimal(decimal? x, decimal? y)
        {
            if (!x.HasValue)
            {
                if (!y.HasValue)
                {
                    return 0;
                }
                return -1;
            }
            if (!y.HasValue)
            {
                return 1;
            }
            return Math.Sign(x.Value - y.Value);
        }

        public static int CompareNullableLong(long? x, long? y)
        {
            if (!x.HasValue)
            {
                if (!y.HasValue)
                {
                    return 0;
                }
                return -1;
            }
            if (!y.HasValue)
            {
                return 1;
            }
            if (x > y)
            {
                return 1;
            }
            if (x < y)
            {
                return -1;
            }
            return 0;
        }

        public static int CompareNullableDouble(double? x, double? y)
        {
            if (!x.HasValue)
            {
                if (!y.HasValue)
                {
                    return 0;
                }
                return -1;
            }
            if (!y.HasValue)
            {
                return 1;
            }
            if (x > y)
            {
                return 1;
            }
            if (x < y)
            {
                return -1;
            }
            return 0;
        }

        public static int CompareString(string x, string y)
        {
            return CompareString(x, y, ignoreLength: false);
        }

        public static int CompareString(string x, string y, bool ignoreLength)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (!ignoreLength)
            {
                int num = x.Length.CompareTo(y.Length);
                if (num != 0)
                {
                    return num;
                }
            }
            int num2 = string.Compare(x, y, StringComparison.Ordinal);
            if (num2 > 1)
            {
                num2 = 1;
            }
            else if (num2 < -1)
            {
                num2 = -1;
            }
            return num2;
        }

        public static int CompareStringLeftToRight(string x, string y)
        {
            return CompareString(x, y, ignoreLength: true);
        }

        public static int CompareInfoObjectState(typeDiagObjectState x, typeDiagObjectState y)
        {
            if (x.Equals(y))
            {
                return 0;
            }
            switch (x)
            {
                case typeDiagObjectState.Minimized:
                    return 1;
                case typeDiagObjectState.NotCalled:
                    if (typeDiagObjectState.Minimized == y)
                    {
                        return -1;
                    }
                    break;
                case typeDiagObjectState.Suspected:
                    if (typeDiagObjectState.Minimized == y || y == typeDiagObjectState.NotCalled)
                    {
                        return -1;
                    }
                    break;
                case typeDiagObjectState.Canceled:
                    if (typeDiagObjectState.Minimized == y || y == typeDiagObjectState.NotCalled || typeDiagObjectState.Suspected == y)
                    {
                        return -1;
                    }
                    break;
                case typeDiagObjectState.Performed:
                    if (typeDiagObjectState.Minimized == y || y == typeDiagObjectState.NotCalled || typeDiagObjectState.Suspected == y || typeDiagObjectState.Canceled == y)
                    {
                        return -1;
                    }
                    break;
                case typeDiagObjectState.Running:
                    if (typeDiagObjectState.Minimized == y || y == typeDiagObjectState.NotCalled || typeDiagObjectState.Suspected == y || typeDiagObjectState.Canceled == y || typeDiagObjectState.Performed == y)
                    {
                        return -1;
                    }
                    break;
            }
            return 1;
        }

        public static int CompareWarningLights(int x, int y)
        {
            List<int> obj = new List<int> { 4, 5, 0, 1, 2, 3 };
            int num = obj.IndexOf(x);
            int value = obj.IndexOf(y);
            return num.CompareTo(value);
        }
    }
}
