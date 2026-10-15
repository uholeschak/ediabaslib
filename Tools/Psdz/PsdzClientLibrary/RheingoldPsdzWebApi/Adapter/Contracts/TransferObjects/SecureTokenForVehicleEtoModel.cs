using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SecureTokenForVehicleEtoModel
    {
        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureId { get; set; }

        [JsonProperty("tokenId", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenId { get; set; }

        [JsonProperty("serializedSecureToken", NullValueHandling = NullValueHandling.Ignore)]
        public string SerializedSecureToken { get; set; }
    }
}