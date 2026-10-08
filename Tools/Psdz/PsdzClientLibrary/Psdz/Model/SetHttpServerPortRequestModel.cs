using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetHttpServerPortRequestModel
    {
        [JsonProperty("serverPort", NullValueHandling = NullValueHandling.Ignore)]
        public int ServerPort { get; set; }
    }
}