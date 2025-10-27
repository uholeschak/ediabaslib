using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class DiagAddressCtoModel
    {
        [JsonProperty("invalidOffset", NullValueHandling = NullValueHandling.Ignore)]
        public int InvalidOffset { get; set; }

        [JsonProperty("maxOffset", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxOffset { get; set; }

        [JsonProperty("minOffset", NullValueHandling = NullValueHandling.Ignore)]
        public int MinOffset { get; set; }

        [JsonProperty("offsetAsHex", NullValueHandling = NullValueHandling.Ignore)]
        public string OffsetAsHex { get; set; }

        [JsonProperty("offsetAsInt", NullValueHandling = NullValueHandling.Ignore)]
        public int OffsetAsInt { get; set; }

        [JsonProperty("offsetAsString", NullValueHandling = NullValueHandling.Ignore)]
        public string OffsetAsString { get; set; }

        [JsonProperty("valid", NullValueHandling = NullValueHandling.Ignore)]
        public bool Valid { get; set; }
    }
}