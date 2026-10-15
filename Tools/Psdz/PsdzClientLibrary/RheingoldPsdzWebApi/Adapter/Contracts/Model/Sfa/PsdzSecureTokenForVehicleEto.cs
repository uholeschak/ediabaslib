using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzFeatureIdCto))]
    public class PsdzSecureTokenForVehicleEto : IPsdzSecureTokenForVehicleEto
    {
        [DataMember]
        public IPsdzFeatureIdCto FeatureIdCto { get; set; }

        [DataMember]
        public string TokenId { get; set; }

        [DataMember]
        public string SerializedSecureToken { get; set; }
    }
}