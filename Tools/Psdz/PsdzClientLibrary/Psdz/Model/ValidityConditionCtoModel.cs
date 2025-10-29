using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class ValidityConditionCtoModel
    {
        [JsonProperty("conditionType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionTypeEto ConditionType { get; set; }

        [JsonProperty("validityValue", NullValueHandling = NullValueHandling.Ignore)]
        public string ValidityValue { get; set; }
    }
}