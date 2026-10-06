using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FetchCalculationOfSecureTokensOfflineRequestModel
    {
        [JsonProperty("secureTokenFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string SecureTokenFilePath { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}