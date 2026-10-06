using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public class FaModel : StandardFaModel
    {
        [JsonProperty("asXml", NullValueHandling = NullValueHandling.Ignore)]
        public string AsXml { get; set; }
    }
}