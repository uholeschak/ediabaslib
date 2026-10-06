using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class DeleteSecureTokenRequestModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifier { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureId { get; set; }
    }
}