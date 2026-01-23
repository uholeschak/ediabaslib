using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzStandardFpCriterion))]
    public class PsdzStandardFp : IPsdzStandardFp
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public string AsString { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<int, IList<IPsdzStandardFpCriterion>> Category2Criteria { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<int, string> CategoryId2CategoryName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsValid { get; set; }
    }
}
