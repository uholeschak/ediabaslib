using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    [KnownType(typeof(PsdzStandardFpCriterion))]
    public class PsdzStandardFp : IPsdzStandardFp
    {
        [DataMember(IsRequired = true)]
        public string AsString { get; set; }

        [DataMember]
        public IDictionary<int, IList<IPsdzStandardFpCriterion>> Category2Criteria { get; set; }

        [DataMember]
        public IDictionary<int, string> CategoryId2CategoryName { get; set; }

        [DataMember]
        public bool IsValid { get; set; }
    }
}
