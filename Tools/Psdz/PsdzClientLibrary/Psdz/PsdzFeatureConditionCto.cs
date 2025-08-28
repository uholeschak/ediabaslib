using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    public class PsdzFeatureConditionCto : IPsdzFeatureConditionCto
    {
        [DataMember]
        public PsdzConditionTypeEtoEnum ConditionType { get; set; }

        [DataMember]
        public string CurrentValidityValue { get; set; }

        [DataMember]
        public int Length { get; set; }

        [DataMember]
        public string ValidityValue { get; set; }
    }
}
