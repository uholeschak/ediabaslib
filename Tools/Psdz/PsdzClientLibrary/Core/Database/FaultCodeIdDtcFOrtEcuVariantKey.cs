using BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderHelper;
using System;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public sealed class FaultCodeIdDtcFOrtEcuVariantKey : DtcFOrtEcuVariantKey, IEquatable<FaultCodeIdDtcFOrtEcuVariantKey>
    {
        public decimal FaultId { get; private set; }

        public FaultCodeIdDtcFOrtEcuVariantKey(decimal faultId, string dtcFOrt, string ecuVariant)
            : base(dtcFOrt, ecuVariant)
        {
            FaultId = faultId;
        }

        public DtcFOrtEcuVariantKey GetDtcFOrtEcuVariantKey()
        {
            return new DtcFOrtEcuVariantKey(base.DtcF_Ort, base.EcuVariant);
        }

        public bool Equals(FaultCodeIdDtcFOrtEcuVariantKey other)
        {
            if (other != null && FaultId == other.FaultId && string.Equals(base.DtcF_Ort, other.DtcF_Ort, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(base.EcuVariant, other.EcuVariant, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ((17 * 23 + FaultId.GetHashCode()) * 23 + base.DtcF_Ort.GetHashCode()) * 23 + base.EcuVariant.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FaultCodeIdDtcFOrtEcuVariantKey))
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            return Equals(obj as FaultCodeIdDtcFOrtEcuVariantKey);
        }
    }
}
