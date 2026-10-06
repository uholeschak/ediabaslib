using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class TalFilterModel
    {
        [JsonProperty("asXml", NullValueHandling = NullValueHandling.Ignore)]
        public string AsXml { get; set; }
    }
}