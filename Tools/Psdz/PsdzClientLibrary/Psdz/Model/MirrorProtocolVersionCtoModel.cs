using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class MirrorProtocolVersionCtoModel
    {
        [JsonProperty("VERSION_BYTE_SIZE", NullValueHandling = NullValueHandling.Ignore)]
        public int VERSION_BYTE_SIZE { get; set; }

        [JsonProperty("MajorVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int MajorVersion { get; set; }

        [JsonProperty("MinorVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int MinorVersion { get; set; }

        [JsonProperty("DEFAULT_MAJOR_VERSION", NullValueHandling = NullValueHandling.Ignore)]
        public int DEFAULT_MAJOR_VERSION { get; set; }

        [JsonProperty("DEFAULT_MINOR_VERSION", NullValueHandling = NullValueHandling.Ignore)]
        public int DEFAULT_MINOR_VERSION { get; set; }
    }
}