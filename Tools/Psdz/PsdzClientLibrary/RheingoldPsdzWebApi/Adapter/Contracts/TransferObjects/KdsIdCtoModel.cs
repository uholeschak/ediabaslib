using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class KdsIdCtoModel
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty("idAsHex", NullValueHandling = NullValueHandling.Ignore)]
        public string IdAsHex { get; set; }
    }
}