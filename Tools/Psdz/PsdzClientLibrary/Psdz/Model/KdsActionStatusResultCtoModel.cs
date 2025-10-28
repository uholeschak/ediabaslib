using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class KdsActionStatusResultCtoModel
    {
        [JsonProperty("kdsActionStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KdsActionStatusEto KdsActionStatus { get; set; }

        [JsonProperty("kdsFailureResponseCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsFailureResponseCtoModel KdsFailureResponseCto { get; set; }

        [JsonProperty("kdsId", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsId { get; set; }
    }
}