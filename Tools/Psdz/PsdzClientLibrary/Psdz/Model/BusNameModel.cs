using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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