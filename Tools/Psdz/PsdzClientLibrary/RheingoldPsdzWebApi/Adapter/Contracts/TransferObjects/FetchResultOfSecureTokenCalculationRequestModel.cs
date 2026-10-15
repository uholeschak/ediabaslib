using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FetchResultOfSecureTokenCalculationRequestModel
    {
        [JsonProperty("securityBackendRequestId", NullValueHandling = NullValueHandling.Ignore)]
        public SecurityBackendRequestIdEtoModel SecurityBackendRequestId { get; set; }
    }
}