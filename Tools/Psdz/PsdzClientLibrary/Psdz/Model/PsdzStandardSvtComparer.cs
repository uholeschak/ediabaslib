using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    public sealed class PsdzStandardSvtComparer : IEqualityComparer<PsdzStandardSvt>
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
            if (!SequenceEqual(x.HoSignature, y.HoSignature, (byte item) => item))
            {
                return false;
            }
            if (!x.HoSignatureDate.Equals(y.HoSignatureDate))
            {
                return false;
            }
            if (!SequenceEqual(x.Ecus, y.Ecus, (IPsdzEcu ecu) => ecu.PrimaryKey))
            {
                return false;
            }
            return true;
        }

        public int GetHashCode(PsdzStandardSvt obj)
        {
            return (((((((obj.Ecus != null) ? obj.Ecus.OrderBy((IPsdzEcu ecu) => ecu.PrimaryKey).Aggregate(17, (int res, IPsdzEcu ecu) => (res * 397) ^ ecu.GetHashCode()) : 0) * 397) ^ ((obj.HoSignature != null) ? obj.HoSignature.Aggregate(17, (int res, byte val) => (res * 397) ^ val) : 0)) * 397) ^ obj.HoSignatureDate.GetHashCode()) * 397) ^ obj.Version;
        }

        private static bool SequenceEqual<TSource, TKey>(IEnumerable<TSource> x, IEnumerable<TSource> y, Func<TSource, TKey> keySelector)
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
            return x.OrderBy(keySelector).SequenceEqual(y.OrderBy(keySelector));
        }
    }
}
