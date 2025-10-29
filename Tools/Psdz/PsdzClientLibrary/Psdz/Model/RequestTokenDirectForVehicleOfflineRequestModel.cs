using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestTokenDirectForVehicleOfflineRequestModel
    {
        [JsonProperty("requestFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestFilePath { get; set; }

        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public string Client { get; set; }

        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public string System { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }

        [JsonProperty("svtModel", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("secureTokenRequest", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenRequestCtoModel SecureTokenRequest { get; set; }
    }
}