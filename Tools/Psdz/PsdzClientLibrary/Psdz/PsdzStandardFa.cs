using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(Hint = "Added OLD_PSDZ_FA", SuppressWarning = true)]
    [DataContract]
    public class PsdzStandardFa : IPsdzStandardFa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AsString { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<string> EWords { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Entwicklungsbaureihe { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int FaVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<string> HOWords { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsValid { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Lackcode { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Polstercode { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<string> Salapas { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Type { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Zeitkriterium { get; set; }
#if !OLD_PSDZ_FA
        [PreserveSource(KeepAttribute = true)]
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
//[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
//[+] return true;
return true;
//[+] #else
#else
                return string.Equals(Vin, psdzStandardFa.Vin);
//[+] #endif
#endif
            }
            return false;
        }

        public override int GetHashCode()
        {
            //[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
//[+] return (((((((((((((((((((FaVersion.GetHashCode() * 397) ^ ((Entwicklungsbaureihe != null) ? Entwicklungsbaureihe.GetHashCode() : 0)) * 397) ^ ((Type != null) ? Type.GetHashCode() : 0)) * 397) ^ ((Zeitkriterium != null) ? Zeitkriterium.GetHashCode() : 0)) * 397) ^ IsValid.GetHashCode()) * 397) ^ ((Lackcode != null) ? Lackcode.GetHashCode() : 0)) * 397) ^ ((Polstercode != null) ? Polstercode.GetHashCode() : 0)) * 397) ^ ((EWords != null) ? EWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((HOWords != null) ? HOWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((Salapas != null) ? Salapas.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397);
return (((((((((((((((((((FaVersion.GetHashCode() * 397) ^ ((Entwicklungsbaureihe != null) ? Entwicklungsbaureihe.GetHashCode() : 0)) * 397) ^ ((Type != null) ? Type.GetHashCode() : 0)) * 397) ^ ((Zeitkriterium != null) ? Zeitkriterium.GetHashCode() : 0)) * 397) ^ IsValid.GetHashCode()) * 397) ^ ((Lackcode != null) ? Lackcode.GetHashCode() : 0)) * 397) ^ ((Polstercode != null) ? Polstercode.GetHashCode() : 0)) * 397) ^ ((EWords != null) ? EWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((HOWords != null) ? HOWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((Salapas != null) ? Salapas.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397);
//[+] #else
#else
            return (((((((((((((((((((FaVersion.GetHashCode() * 397) ^ ((Entwicklungsbaureihe != null) ? Entwicklungsbaureihe.GetHashCode() : 0)) * 397) ^ ((Type != null) ? Type.GetHashCode() : 0)) * 397) ^ ((Zeitkriterium != null) ? Zeitkriterium.GetHashCode() : 0)) * 397) ^ IsValid.GetHashCode()) * 397) ^ ((Lackcode != null) ? Lackcode.GetHashCode() : 0)) * 397) ^ ((Polstercode != null) ? Polstercode.GetHashCode() : 0)) * 397) ^ ((EWords != null) ? EWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((HOWords != null) ? HOWords.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((Salapas != null) ? Salapas.OrderBy((string x) => x).Aggregate(17, (int current, string sgbmId) => (current * 397) ^ sgbmId.ToLowerInvariant().GetHashCode()) : 0)) * 397) ^ ((Vin != null) ? Vin.GetHashCode() : 0);
//[+] #endif
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
