using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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