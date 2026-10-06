using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

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
