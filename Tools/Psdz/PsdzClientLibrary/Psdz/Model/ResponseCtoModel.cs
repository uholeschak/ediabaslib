using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ResponseCtoModel
    {
        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public string Cause { get; set; }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public object Result { get; set; }

        [JsonProperty("successful", NullValueHandling = NullValueHandling.Ignore)]
        public bool Successful { get; set; }
    }
}