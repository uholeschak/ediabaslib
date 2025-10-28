using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class RequestCertificatesBindingsAndKeypacksOfflineRequestModel
    {
        [JsonProperty("certificates", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityMemoryObjectEtoModel> Certificates { get; set; }

        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public string Client { get; set; }

        [JsonProperty("requestFile", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestFile { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public string System { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}