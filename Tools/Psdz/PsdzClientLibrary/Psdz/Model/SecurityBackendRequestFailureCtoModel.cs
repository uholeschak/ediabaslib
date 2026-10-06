using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SecurityBackendRequestFailureCtoModel
    {
        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizableMessageToModel Cause { get; set; }

        [JsonProperty("retry", NullValueHandling = NullValueHandling.Ignore)]
        public int Retry { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}