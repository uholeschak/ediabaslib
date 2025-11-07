using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class EbcdicVIN7Comparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }

                return 1;
            }

            if (y == null)
            {
                throw new ArgumentNullException("x");
            }

            if (x.Length != 7 || y.Length != 7)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Vin must be of length 7: {0}, {1}", x, y));
            }

            int num = 0;
            for (int i = 0; i < 7; i++)
            {
                if (char.IsDigit(x[i]))
                {
                    if (!char.IsDigit(y[i]))
                    {
                        return 1;
                    }

                    num = x[i].CompareTo(y[i]);
                    if (num != 0)
                    {
                        return num;
                    }
                }
                else
                {
                    if (char.IsDigit(y[i]))
                    {
                        return -1;
                    }

                    num = x[i].CompareTo(y[i]);
                    if (num != 0)
                    {
                        return num;
                    }
                }
            }

            return num;
        }
    }
}