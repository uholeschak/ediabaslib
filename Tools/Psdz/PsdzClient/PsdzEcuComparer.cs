using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	public class PsdzEcuComparer : IEqualityComparer<IPsdzEcu>
	{
		public bool Equals(IPsdzEcu x, IPsdzEcu y)
		{
			return x == y || (x != null && y != null && !(x.GetType() != y.GetType()) && string.Equals(x.BaseVariant, y.BaseVariant, StringComparison.Ordinal) && string.Equals(x.EcuVariant, y.EcuVariant, StringComparison.Ordinal) && string.Equals(x.BnTnName, y.BnTnName, StringComparison.Ordinal) && string.Equals(x.SerialNumber, y.SerialNumber, StringComparison.Ordinal) && x.DiagnosticBus == y.DiagnosticBus && PsdzEcuComparer.EqualsBusConnections(x.BusConnections, y.BusConnections) && PsdzEcuComparer.EqualsMember<IPsdzEcuIdentifier>(x.PrimaryKey, y.PrimaryKey) && PsdzEcuComparer.EqualsMember<IPsdzDiagAddress>(x.GatewayDiagAddr, y.GatewayDiagAddr) && PsdzEcuComparer.EqualsMember<IPsdzStandardSvk>(x.StandardSvk, y.StandardSvk) && PsdzEcuComparer.EqualsMember<IPsdzEcuStatusInfo>(x.EcuStatusInfo, y.EcuStatusInfo) && PsdzEcuComparer.EqualsMember<IPsdzEcuDetailInfo>(x.EcuDetailInfo, y.EcuDetailInfo) && PsdzEcuComparer.EqualsMember<IPsdzEcuPdxInfo>(x.PsdzEcuPdxInfo, y.PsdzEcuPdxInfo));
		}

		public int GetHashCode(IPsdzEcu obj)
		{
			int num = (((((((((obj.StandardSvk != null) ? obj.StandardSvk.GetHashCode() : 0) * 397 ^ ((obj.SerialNumber != null) ? obj.SerialNumber.GetHashCode() : 0)) * 397 ^ ((obj.PrimaryKey != null) ? obj.PrimaryKey.GetHashCode() : 0)) * 397 ^ ((obj.GatewayDiagAddr != null) ? obj.GatewayDiagAddr.GetHashCode() : 0)) * 397 ^ ((obj.EcuVariant != null) ? obj.EcuVariant.GetHashCode() : 0)) * 397 ^ ((obj.EcuStatusInfo != null) ? obj.EcuStatusInfo.GetHashCode() : 0)) * 397 ^ ((obj.EcuDetailInfo != null) ? obj.EcuDetailInfo.GetHashCode() : 0)) * 397 ^ (int)obj.DiagnosticBus) * 397;
			int num2;
			if (obj.BusConnections == null)
			{
				num2 = 0;
			}
			else
			{
				num2 = (from x in obj.BusConnections
						orderby x
						select x).Aggregate(17, (int current, PsdzBus bus) => current * 397 ^ (int)bus);
			}
			return (((num ^ num2) * 397 ^ ((obj.BnTnName != null) ? obj.BnTnName.GetHashCode() : 0)) * 397 ^ ((obj.BaseVariant != null) ? obj.BaseVariant.GetHashCode() : 0)) * 397 ^ ((obj.PsdzEcuPdxInfo != null) ? obj.PsdzEcuPdxInfo.GetHashCode() : 0);
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
			return (from bus in x
					orderby bus
					select bus).SequenceEqual(from bus in y
											  orderby bus
											  select bus);
		}

		private static bool EqualsMember<T>(T x, T y)
		{
			return (object) x == (object) y || (x != null && y != null && !(x.GetType() != y.GetType()) && x.Equals(y));
		}
	}
}
