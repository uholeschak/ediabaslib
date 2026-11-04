using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.Implementation.Sfa.Models.Request
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [DataContract]
    public class SfaValidityCondition
    {
        [DataMember]
        public SfaValidityConditionType CondType { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}