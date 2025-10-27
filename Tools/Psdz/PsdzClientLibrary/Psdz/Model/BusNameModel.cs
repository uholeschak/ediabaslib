using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class BusNameModel
    {
        [JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty("name", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("directAccess", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool DirectAccess { get; set; }

        [JsonProperty("isEthernet", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEthernet { get; set; }
    }
}