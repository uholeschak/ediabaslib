using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class FeatureConditionCtoModel
    {
        [JsonProperty("conditionType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionTypeEto ConditionType { get; set; }

        [JsonProperty("currentValidityValue", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentValidityValue { get; set; }

        [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
        public int Length { get; set; }

        [JsonProperty("validityValue", NullValueHandling = NullValueHandling.Ignore)]
        public string ValidityValue { get; set; }
    }
}