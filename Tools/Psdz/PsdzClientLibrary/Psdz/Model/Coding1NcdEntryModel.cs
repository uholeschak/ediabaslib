using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class Coding1NcdEntryModel
    {
        [JsonProperty("blockAddress", NullValueHandling = NullValueHandling.Ignore)]
        public int BlockAddress { get; set; }

        [JsonProperty("userData", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] UserData { get; set; }

        [JsonProperty("writeable", NullValueHandling = NullValueHandling.Ignore)]
        public bool Writeable { get; set; }
    }
}