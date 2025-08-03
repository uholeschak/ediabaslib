using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzStandardFa : IPsdzStandardFa
    {
        [DataMember]
        public string AsString { get; set; }

        [DataMember]
        public IEnumerable<string> EWords { get; set; }

        [DataMember]
        public string Entwicklungsbaureihe { get; set; }

        [DataMember]
        public int FaVersion { get; set; }

        [DataMember]
        public IEnumerable<string> HOWords { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public string Lackcode { get; set; }

        [DataMember]
        public string Polstercode { get; set; }

        [DataMember]
        public IEnumerable<string> Salapas { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Zeitkriterium { get; set; }
#if !OLD_PSDZ_HOST
        [DataMember]
        public string Vin { get; set; }
#endif

        public override bool Equals(object obj)
        {
            if (!(obj is PsdzStandardFa psdzStandardFa))
            {
                return false;
            }
            if (FaVersion == psdzStandardFa.FaVersion && string.Equals(Entwicklungsbaureihe, psdzStandardFa.Entwicklungsbaureihe) && string.Equals(Type, psdzStandardFa.Type) && string.Equals(Zeitkriterium, psdzStandardFa.Zeitkriterium) && IsValid.Equals(psdzStandardFa.IsValid) && string.Equals(Lackcode, psdzStandardFa.Lackcode) && string.Equals(Polstercode, psdzStandardFa.Polstercode) && StringSequenceEqual(EWords, psdzStandardFa.EWords) && StringSequenceEqual(HOWords, psdzStandardFa.HOWords) && StringSequenceEqual(Salapas, psdzStandardFa.Salapas))
            {
#if OLD_PSDZ_HOST
                return true;
#else
                return string.Equals(Vin, psdzStandardFa.Vin);
#endif
            }
            return false;
        }

        public override int GetHashCode()
        {
            int num = ((((((((((((this.FaVersion.GetHashCode() * 397) ^ ((this.Entwicklungsbaureihe != null) ? this.Entwicklungsbaureihe.GetHashCode() : 0)) * 397) ^ ((this.Type != null) ? this.Type.GetHashCode() : 0)) * 397) ^ ((this.Zeitkriterium != null) ? this.Zeitkriterium.GetHashCode() : 0)) * 397) ^ this.IsValid.GetHashCode()) * 397) ^ ((this.Lackcode != null) ? this.Lackcode.GetHashCode() : 0)) * 397) ^ ((this.Polstercode != null) ? this.Polstercode.GetHashCode() : 0)) * 397;
            int num2;
            if (this.EWords == null)
            {
                num2 = 0;
            }
            else
            {
                num2 = this.EWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode());
            }
            int num3 = (num ^ num2) * 397;
            int num4;
            if (this.HOWords == null)
            {
                num4 = 0;
            }
            else
            {
                num4 = this.HOWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode());
            }
            int num5 = (num3 ^ num4) * 397;
            int num6;
            if (this.Salapas == null)
            {
                num6 = 0;
            }
            else
            {
                num6 = this.Salapas.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode());
            }
            int num7 = (num5 ^ num6) * 397;
#if OLD_PSDZ_HOST
            return num7;
#else
            return num7 ^ ((this.Vin != null) ? this.Vin.GetHashCode() : 0);
#endif
        }

        private static bool StringSequenceEqual(IEnumerable<string> first, IEnumerable<string> second)
        {
            if (first != null)
            {
                if (second != null)
                {
                    return first.OrderBy((string x) => x).SequenceEqual(second.OrderBy((string x) => x), StringComparer.OrdinalIgnoreCase);
                }
                return false;
            }
            return second == null;
        }
    }
}
