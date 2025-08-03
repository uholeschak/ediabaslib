using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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