using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SecureTokenEtoModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel EcuIdentifier { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureId { get; set; }

        [JsonProperty("serializedSecureToken", NullValueHandling = NullValueHandling.Ignore)]
        public string SerializedSecureToken { get; set; }

        [JsonProperty("tokenId", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenId { get; set; }
    }
}