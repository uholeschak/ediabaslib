using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
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
