using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

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
