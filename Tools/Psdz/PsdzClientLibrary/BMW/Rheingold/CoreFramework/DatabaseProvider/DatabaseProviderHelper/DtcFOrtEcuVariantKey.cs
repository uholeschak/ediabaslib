using System;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderHelper
{
    public class DtcFOrtEcuVariantKey : IEquatable<DtcFOrtEcuVariantKey>
    {
        public string DtcF_Ort { get; private set; }

        public string EcuVariant { get; private set; }

        public DtcFOrtEcuVariantKey(string dtcFOrt, string ecuVariant)
        {
            DtcF_Ort = dtcFOrt.ToLowerInvariant();
            EcuVariant = ecuVariant.ToLowerInvariant();
        }

        public bool Equals(DtcFOrtEcuVariantKey other)
        {
            if (other != null && string.Equals(DtcF_Ort, other.DtcF_Ort, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(EcuVariant, other.EcuVariant, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (17 * 23 + DtcF_Ort.GetHashCode()) * 23 + EcuVariant.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is DtcFOrtEcuVariantKey))
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            return Equals(obj as DtcFOrtEcuVariantKey);
        }
    }
}
