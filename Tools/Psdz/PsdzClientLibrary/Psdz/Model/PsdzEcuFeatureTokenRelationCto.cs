using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzFeatureIdCto))]
    public class PsdzEcuFeatureTokenRelationCto : IPsdzEcuFeatureTokenRelationCto
    {
        [DataMember]
        public IPsdzEcuIdentifier ECUIdentifier { get; set; }

        [DataMember]
        public PsdzFeatureGroupEtoEnum FeatureGroup { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [DataMember]
        public string TokenId { get; set; }
    }
}
