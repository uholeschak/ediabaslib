using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzFa : PsdzStandardFa, IPsdzFa, IPsdzStandardFa
    {
        [DataMember]
        public string AsXml { get; set; }

#if OLD_PSDZ_FA
        [DataMember]
        public string Vin { get; set; }
#endif
        public override bool Equals(object obj)
        {
            if (obj is PsdzFa psdzFa && base.Equals((object)psdzFa))
            {
                return string.Equals(base.Vin, psdzFa.Vin);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() * 397) ^ ((base.Vin != null) ? base.Vin.GetHashCode() : 0);
        }
    }
}
