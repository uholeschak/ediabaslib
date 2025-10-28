using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class PerformRefurbishProcessRequestModel
    {
        [JsonProperty("kdsActionId", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KdsActionIdEto KdsActionId { get; set; }

        [JsonProperty("kdsId", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsId { get; set; }

        [JsonProperty("retries", NullValueHandling = NullValueHandling.Ignore)]
        public int Retries { get; set; }

        [JsonProperty("secureToken", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenEtoModel SecureToken { get; set; }

        [JsonProperty("timeBetweenRetries", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeBetweenRetries { get; set; }
    }
}