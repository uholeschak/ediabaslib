using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestNcdEtoModel
    {
        [JsonProperty("btld", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel Btld { get; set; }

        [JsonProperty("cafd", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel Cafd { get; set; }
    }
}