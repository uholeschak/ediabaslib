using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class RequestDirectSecureTokensPackageWithoutCrlFilesWithReturnTokenRequestModel
    {
        [JsonProperty("backendUrlList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> BackendUrlList { get; set; }

        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public string Client { get; set; }

        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public string System { get; set; }

        [JsonProperty("retries", NullValueHandling = NullValueHandling.Ignore)]
        public int Retries { get; set; }

        [JsonProperty("timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int Timeout { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("secureTokenRequest", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenRequestCtoModel SecureTokenRequest { get; set; }
    }
}