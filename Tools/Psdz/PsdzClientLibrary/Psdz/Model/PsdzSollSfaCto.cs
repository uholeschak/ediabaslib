using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFeatureTokenRelationCto))]
    public class PsdzSollSfaCto : IPsdzSollSfaCto
    {
        [DataMember]
        public IEnumerable<IPsdzEcuFeatureTokenRelationCto> SollFeatures { get; set; }
    }
}
