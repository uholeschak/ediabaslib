using System.Runtime.Serialization;

namespace BMW.Authoring.API.Implementation.Sfa.Models.Request
{
    [DataContract]
    public class SfaValidityCondition
    {
        [DataMember]
        public SfaValidityConditionType CondType { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
