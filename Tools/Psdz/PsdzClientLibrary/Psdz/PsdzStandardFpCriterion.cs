using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
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
