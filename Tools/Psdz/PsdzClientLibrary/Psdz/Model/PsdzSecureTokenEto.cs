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
    public class PsdzSecureTokenEto : IPsdzSecureTokenEto
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureIdCto { get; set; }

        [DataMember]
        public string SerializedSecureToken { get; set; }

        [DataMember]
        public string TokenId { get; set; }
    }
}
