using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzSvt : PsdzStandardSvt, IPsdzSvt, IPsdzStandardSvt
    {
        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public string Vin { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PsdzSvt psdzSvt && base.Equals((object)psdzSvt) && IsValid.Equals(psdzSvt.IsValid))
            {
                return string.Equals(Vin, psdzSvt.Vin);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (((base.GetHashCode() * 397) ^ IsValid.GetHashCode()) * 397) ^ ((Vin != null) ? Vin.GetHashCode() : 0);
        }
    }
}
