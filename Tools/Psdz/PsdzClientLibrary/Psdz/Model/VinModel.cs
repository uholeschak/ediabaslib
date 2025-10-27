using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class VinModel
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}