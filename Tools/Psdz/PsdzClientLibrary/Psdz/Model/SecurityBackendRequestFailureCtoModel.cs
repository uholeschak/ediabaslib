using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SecurityBackendRequestFailureCtoModel
    {
        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizableMessageToModel Cause { get; set; }

        [JsonProperty("retry", NullValueHandling = NullValueHandling.Ignore)]
        public int Retry { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}