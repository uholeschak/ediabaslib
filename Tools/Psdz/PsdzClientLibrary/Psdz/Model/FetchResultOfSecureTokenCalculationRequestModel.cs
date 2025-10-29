using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FetchResultOfSecureTokenCalculationRequestModel
    {
        [JsonProperty("securityBackendRequestId", NullValueHandling = NullValueHandling.Ignore)]
        public SecurityBackendRequestIdEtoModel SecurityBackendRequestId { get; set; }
    }
}