using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestDirectSecureTokensPackageOfflineRequestModel
    {
        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public string Client { get; set; }

        [JsonProperty("filepath", NullValueHandling = NullValueHandling.Ignore)]
        public string Filepath { get; set; }

        [JsonProperty("secureTokenRequest", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenRequestCtoModel SecureTokenRequest { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public string System { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}