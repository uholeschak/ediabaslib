using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SecurityBackendRequestIdEtoModel
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public int Value { get; set; }
    }
}