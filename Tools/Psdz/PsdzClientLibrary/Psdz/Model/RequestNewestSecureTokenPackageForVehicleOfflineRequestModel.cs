using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestNewestSecureTokenPackageForVehicleOfflineRequestModel
    {
        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public string Client { get; set; }

        [JsonProperty("rebuildTokens", NullValueHandling = NullValueHandling.Ignore)]
        public bool RebuildTokens { get; set; }

        [JsonProperty("requestFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestFilePath { get; set; }

        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public string System { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}