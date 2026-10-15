using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzStandardFpCriterion : IPsdzStandardFpCriterion
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Name { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string NameEn { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Value { get; set; }
    }
}
