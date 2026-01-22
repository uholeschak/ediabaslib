using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(Hint = "Added OLD_PSDZ_FA")]
    [DataContract]
    public class PsdzFa : PsdzStandardFa, IPsdzFa, IPsdzStandardFa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AsXml { get; set; }

#if OLD_PSDZ_FA
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Vin { get; set; }
#endif
        public override bool Equals(object obj)
        {
            if (obj is PsdzFa psdzFa && base.Equals((object)psdzFa))
            {
//[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
//[+] return string.Equals(Vin, psdzFa.Vin);
return string.Equals(Vin, psdzFa.Vin);
//[+] #else
#else
                return string.Equals(base.Vin, psdzFa.Vin);
//[+] #endif
#endif
            }
            return false;
        }

        public override int GetHashCode()
        {
//[+] #if OLD_PSDZ_FA
#if OLD_PSDZ_FA
//[+] return (base.GetHashCode() * 397) ^ ((Vin != null) ? Vin.GetHashCode() : 0);
return (base.GetHashCode() * 397) ^ ((Vin != null) ? Vin.GetHashCode() : 0);
//[+] #else
#else
            return (base.GetHashCode() * 397) ^ ((base.Vin != null) ? base.Vin.GetHashCode() : 0);
//[+] #endif
#endif
        }
    }
}
