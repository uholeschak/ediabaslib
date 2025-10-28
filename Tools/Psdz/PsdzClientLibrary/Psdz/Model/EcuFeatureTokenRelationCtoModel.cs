using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class EcuFeatureTokenRelationCtoModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel EcuIdentifier { get; set; }

        [JsonProperty("featureGroup", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FeatureGroupEto FeatureGroup { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureId { get; set; }

        [JsonProperty("tokenId", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenId { get; set; }
    }
}