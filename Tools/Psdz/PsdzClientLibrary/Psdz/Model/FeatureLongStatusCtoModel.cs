using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class FeatureLongStatusCtoModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel EcuIdentifier { get; set; }

        [JsonProperty("featureConditions", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureConditionCtoModel> FeatureConditions { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public FeatureIdCtoModel FeatureId { get; set; }

        [JsonProperty("featureStatusEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FeatureStatusEto FeatureStatusEto { get; set; }

        [JsonProperty("mileageOfActivation", NullValueHandling = NullValueHandling.Ignore)]
        public int MileageOfActivation { get; set; }

        [JsonProperty("tokenId", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenId { get; set; }

        [JsonProperty("validationStatusEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ValidationStatusEto ValidationStatusEto { get; set; }
    }
}