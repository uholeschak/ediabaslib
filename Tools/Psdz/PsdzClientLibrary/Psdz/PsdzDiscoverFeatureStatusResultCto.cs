using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzFeatureStatusTo))]
    public class PsdzDiscoverFeatureStatusResultCto : IPsdzDiscoverFeatureStatusResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ErrorMessage { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzFeatureStatusTo> FeatureStatus { get; set; }
    }
}