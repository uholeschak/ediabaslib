using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FetchResultOfSecureTokenCalculationRequestModel
    {
        [JsonProperty("securityBackendRequestId", NullValueHandling = NullValueHandling.Ignore)]
        public SecurityBackendRequestIdEtoModel SecurityBackendRequestId { get; set; }
    }
}