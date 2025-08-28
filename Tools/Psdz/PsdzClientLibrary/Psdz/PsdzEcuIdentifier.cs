using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [DataContract]
    [KnownType(typeof(PsdzDiagAddress))]
    public class PsdzEcuIdentifier : IPsdzEcuIdentifier, IComparable<IPsdzEcuIdentifier>
    {
        [DataMember(IsRequired = true)]
        public string BaseVariant { get; set; }

        public int DiagAddrAsInt => DiagnosisAddress?.Offset ?? (-1);

        [DataMember(IsRequired = true)]
        public IPsdzDiagAddress DiagnosisAddress { get; set; }

        public int CompareTo(IPsdzEcuIdentifier other)
        {
            if (other == null)
            {
                return 1;
            }
            int num = DiagAddrAsInt.CompareTo(other.DiagAddrAsInt);
            if (num != 0)
            {
                return num;
            }
            return string.CompareOrdinal(BaseVariant, other.BaseVariant);
        }

        public override bool Equals(object obj)
        {
            if (obj is PsdzEcuIdentifier psdzEcuIdentifier && EqualsDiagAddress(DiagnosisAddress, psdzEcuIdentifier.DiagnosisAddress))
            {
                return string.Equals(BaseVariant, psdzEcuIdentifier.BaseVariant, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}_0x{1:X2}", BaseVariant, (DiagnosisAddress == null) ? int.MaxValue : DiagnosisAddress.Offset);
        }

        private static bool EqualsDiagAddress(IPsdzDiagAddress thisDiagAddress, IPsdzDiagAddress otherDiagAddress)
        {
            return thisDiagAddress?.Equals(otherDiagAddress) ?? (otherDiagAddress == null);
        }
    }
}
