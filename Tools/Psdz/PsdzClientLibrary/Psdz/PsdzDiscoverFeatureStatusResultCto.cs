using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzFeatureStatusTo))]
    public class PsdzDiscoverFeatureStatusResultCto : IPsdzDiscoverFeatureStatusResultCto
    {
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public IList<IPsdzFeatureStatusTo> FeatureStatus { get; set; }
    }
}