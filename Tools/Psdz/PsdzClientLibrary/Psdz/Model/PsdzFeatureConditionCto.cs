using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
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
