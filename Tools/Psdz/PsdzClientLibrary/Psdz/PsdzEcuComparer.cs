using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Programming;

namespace BMW.Rheingold.Psdz.Model.Comparer
{
    public sealed class PsdzEcuComparer : IEqualityComparer<IPsdzEcu>
    {
        public bool Equals(IPsdzEcu x, IPsdzEcu y)
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

            if (!string.Equals(x.BaseVariant, y.BaseVariant, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(x.EcuVariant, y.EcuVariant, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(x.BnTnName, y.BnTnName, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(x.SerialNumber, y.SerialNumber, StringComparison.Ordinal))
            {
                return false;
            }

            if (!x.DiagnosticBus.Equals(y.DiagnosticBus))
            {
                return false;
            }

            if (!EqualsBusConnections(x.BusConnections, y.BusConnections))
            {
                return false;
            }

            if (!EqualsMember(x.PrimaryKey, y.PrimaryKey))
            {
                return false;
            }

            if (!EqualsMember(x.GatewayDiagAddr, y.GatewayDiagAddr))
            {
                return false;
            }

            if (!EqualsMember(x.StandardSvk, y.StandardSvk))
            {
                return false;
            }

            if (!EqualsMember(x.EcuStatusInfo, y.EcuStatusInfo))
            {
                return false;
            }

            if (!EqualsMember(x.EcuDetailInfo, y.EcuDetailInfo))
            {
                return false;
            }

            if (!EqualsMember(x.PsdzEcuPdxInfo, y.PsdzEcuPdxInfo))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(IPsdzEcu obj)
        {
            return (((((((((((((((((((((obj.StandardSvk != null) ? obj.StandardSvk.GetHashCode() : 0) * 397) ^ ((obj.SerialNumber != null) ? obj.SerialNumber.GetHashCode() : 0)) * 397) ^ ((obj.PrimaryKey != null) ? obj.PrimaryKey.GetHashCode() : 0)) * 397) ^ ((obj.GatewayDiagAddr != null) ? obj.GatewayDiagAddr.GetHashCode() : 0)) * 397) ^ ((obj.EcuVariant != null) ? obj.EcuVariant.GetHashCode() : 0)) * 397) ^ ((obj.EcuStatusInfo != null) ? obj.EcuStatusInfo.GetHashCode() : 0)) * 397) ^ ((obj.EcuDetailInfo != null) ? obj.EcuDetailInfo.GetHashCode() : 0)) * 397) ^ ((obj.BusConnections != null) ? obj.BusConnections.OrderBy((PsdzBus x) => x).Aggregate(17, (int current, PsdzBus bus) => (current * 397) ^ bus.GetHashCode()) : 0)) * 397) ^ ((obj.BnTnName != null) ? obj.BnTnName.GetHashCode() : 0)) * 397) ^ ((obj.BaseVariant != null) ? obj.BaseVariant.GetHashCode() : 0)) * 397) ^ ((obj.PsdzEcuPdxInfo != null) ? obj.PsdzEcuPdxInfo.GetHashCode() : 0);
        }

        private static bool EqualsBusConnections(IEnumerable<PsdzBus> x, IEnumerable<PsdzBus> y)
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

            return x.OrderBy((PsdzBus bus) => bus).SequenceEqual(y.OrderBy((PsdzBus bus) => bus));
        }

        private static bool EqualsMember<T>(T x, T y)
        {
            if ((object)x == (object)y)
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

            object obj = y;
            return x.Equals(obj);
        }
    }
}