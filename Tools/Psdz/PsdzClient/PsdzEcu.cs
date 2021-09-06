using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	[KnownType(typeof(PsdzEcuStatusInfo))]
	[KnownType(typeof(PsdzEcuDetailInfo))]
	[KnownType(typeof(PsdzDiagAddress))]
	[KnownType(typeof(PsdzEcuIdentifier))]
	[KnownType(typeof(PsdzStandardSvk))]
	[KnownType(typeof(PsdzEcuPdxInfo))]
	[DataContract]
	public class PsdzEcu : IPsdzEcu
	{
		[DataMember]
		public string BaseVariant { get; set; }

		[DataMember]
		public string BnTnName { get; set; }

		[DataMember]
		public IEnumerable<PsdzBus> BusConnections { get; set; }

		[DataMember]
		public PsdzBus DiagnosticBus { get; set; }

		[DataMember]
		public IPsdzEcuDetailInfo EcuDetailInfo { get; set; }

		[DataMember]
		public IPsdzEcuStatusInfo EcuStatusInfo { get; set; }

		[DataMember]
		public string EcuVariant { get; set; }

		[DataMember]
		public IPsdzDiagAddress GatewayDiagAddr { get; set; }

		[DataMember]
		public IPsdzEcuIdentifier PrimaryKey { get; set; }

		[DataMember]
		public string SerialNumber { get; set; }

		[DataMember]
		public IPsdzStandardSvk StandardSvk { get; set; }

		[DataMember]
		public IPsdzEcuPdxInfo PsdzEcuPdxInfo { get; set; }

		public override bool Equals(object obj)
		{
			return PsdzEcu.PsdzEcuComparerInstance.Equals(this, obj as PsdzEcu);
		}

		public override int GetHashCode()
		{
			return PsdzEcu.PsdzEcuComparerInstance.GetHashCode(this);
		}

		private static readonly IEqualityComparer<PsdzEcu> PsdzEcuComparerInstance = new PsdzEcuComparer();
	}
}
