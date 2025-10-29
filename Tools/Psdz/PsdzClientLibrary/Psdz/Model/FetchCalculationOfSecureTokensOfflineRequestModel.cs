using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FetchCalculationOfSecureTokensOfflineRequestModel
    {
        [JsonProperty("secureTokenFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string SecureTokenFilePath { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}