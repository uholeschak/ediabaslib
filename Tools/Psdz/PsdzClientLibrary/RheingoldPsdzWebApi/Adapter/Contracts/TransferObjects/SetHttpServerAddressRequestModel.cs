using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetHttpServerAddressRequestModel
    {
        [JsonProperty("serverAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ServerAddress { get; set; }
    }
}