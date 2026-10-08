using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFeatureTokenRelationCto))]
    public class PsdzSollSfaCto : IPsdzSollSfaCto
    {
        [DataMember]
        public IEnumerable<IPsdzEcuFeatureTokenRelationCto> SollFeatures { get; set; }
    }
}
