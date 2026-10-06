using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzSecureTokenForTal : IPsdzSecureTokenForTal
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long FeatureId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string SerializedSecureToken { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string TokenId { get; set; }
    }
}
