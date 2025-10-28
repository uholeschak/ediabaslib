using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class KdsQuickCheckResultCtoModel
    {
        [JsonProperty("kdsId", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsId { get; set; }

        [JsonProperty("quickCheckResult", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KdsQuickCheckResultEto QuickCheckResult { get; set; }
    }
}