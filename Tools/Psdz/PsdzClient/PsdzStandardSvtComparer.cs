using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public class PsdzStandardSvtComparer : IEqualityComparer<PsdzStandardSvt>
    {
        public bool Equals(PsdzStandardSvt x, PsdzStandardSvt y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null)
            {
                return false;
            }
            if (y == null)
            {
                return false;
            }
            if (x.GetType() != y.GetType())
            {
                return false;
            }
            if (x.Version != y.Version)
            {
                return false;
            }
            if (!PsdzStandardSvtComparer.SequenceEqual<byte, byte>(x.HoSignature, y.HoSignature, (byte item) => item))
            {
                return false;
            }
            if (!x.HoSignatureDate.Equals(y.HoSignatureDate))
            {
                return false;
            }
            return PsdzStandardSvtComparer.SequenceEqual<IPsdzEcu, IPsdzEcuIdentifier>(x.Ecus, y.Ecus, (IPsdzEcu ecu) => ecu.PrimaryKey);
        }

        public int GetHashCode(PsdzStandardSvt obj)
        {
            int num;
            if (obj.Ecus == null)
            {
                num = 0;
            }
            else
            {
                num = (from ecu in obj.Ecus
                    orderby ecu.PrimaryKey
                    select ecu).Aggregate(17, (int res, IPsdzEcu ecu) => res * 397 ^ ecu.GetHashCode());
            }
            int num2 = num * 397;
            int num3;
            if (obj.HoSignature == null)
            {
                num3 = 0;
            }
            else
            {
                num3 = obj.HoSignature.Aggregate(17, (int res, byte val) => res * 397 ^ (int)val);
            }
            return ((num2 ^ num3) * 397 ^ obj.HoSignatureDate.GetHashCode()) * 397 ^ obj.Version;
        }

        private static bool SequenceEqual<TSource, TKey>(IEnumerable<TSource> x, IEnumerable<TSource> y, Func<TSource, TKey> keySelector)
        {
            return x == y || (x != null && y != null && !(x.GetType() != y.GetType()) && x.OrderBy(keySelector).SequenceEqual(y.OrderBy(keySelector)));
        }
    }
}
