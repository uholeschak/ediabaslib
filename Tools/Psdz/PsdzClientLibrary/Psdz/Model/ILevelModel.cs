using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ILevelModel
    {
        [JsonProperty("valid", NullValueHandling = NullValueHandling.Ignore)]
        public bool Valid { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}