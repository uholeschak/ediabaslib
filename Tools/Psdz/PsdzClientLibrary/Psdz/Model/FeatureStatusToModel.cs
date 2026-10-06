using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FeatureStatusToModel
    {
        [JsonProperty("diagAddress", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressCtoModel DiagAddress { get; set; }

        [JsonProperty("fatureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FatureId { get; set; }

        [JsonProperty("featureStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FeatureStatusEto FeatureStatus { get; set; }

        [JsonProperty("validationStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ValidationStatusEto ValidationStatus { get; set; }
    }
}