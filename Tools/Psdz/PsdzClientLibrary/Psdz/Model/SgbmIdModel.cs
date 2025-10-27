using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SgbmIdModel
    {
        [JsonProperty("hexString", NullValueHandling = NullValueHandling.Ignore)]
        public string HexString { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("idAsLong", NullValueHandling = NullValueHandling.Ignore)]
        public long IdAsLong { get; set; }

        [JsonProperty("mainVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int MainVersion { get; set; }

        [JsonProperty("patchVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int PatchVersion { get; set; }

        [JsonProperty("processClass", NullValueHandling = NullValueHandling.Ignore)]
        public string ProcessClass { get; set; }

        [JsonProperty("subVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int SubVersion { get; set; }
    }
}