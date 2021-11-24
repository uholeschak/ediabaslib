using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
	[DataContract]
	[KnownType(typeof(PsdzDiagAddress))]
	public class PsdzEcuIdentifier : IComparable<IPsdzEcuIdentifier>, IPsdzEcuIdentifier
	{
		[DataMember(IsRequired = true)]
		public string BaseVariant { get; set; }

		public int DiagAddrAsInt
		{
			get
			{
				IPsdzDiagAddress diagnosisAddress = this.DiagnosisAddress;
				if (diagnosisAddress == null)
				{
					return -1;
				}
				return diagnosisAddress.Offset;
			}
		}

		[DataMember(IsRequired = true)]
		public IPsdzDiagAddress DiagnosisAddress { get; set; }

		public int CompareTo(IPsdzEcuIdentifier other)
		{
			if (other == null)
			{
				return 1;
			}
			int num = this.DiagAddrAsInt.CompareTo(other.DiagAddrAsInt);
			if (num != 0)
			{
				return num;
			}
			return string.CompareOrdinal(this.BaseVariant, other.BaseVariant);
		}

		public override bool Equals(object obj)
		{
			PsdzEcuIdentifier psdzEcuIdentifier = obj as PsdzEcuIdentifier;
			return psdzEcuIdentifier != null && PsdzEcuIdentifier.EqualsDiagAddress(this.DiagnosisAddress, psdzEcuIdentifier.DiagnosisAddress) && string.Equals(this.BaseVariant, psdzEcuIdentifier.BaseVariant, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}_0x{1:X2}", this.BaseVariant, (this.DiagnosisAddress == null) ? int.MaxValue : this.DiagnosisAddress.Offset);
		}

		private static bool EqualsDiagAddress(IPsdzDiagAddress thisDiagAddress, IPsdzDiagAddress otherDiagAddress)
		{
			if (thisDiagAddress == null)
			{
				return otherDiagAddress == null;
			}
			return thisDiagAddress.Equals(otherDiagAddress);
		}
	}
}
