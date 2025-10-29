using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SecureTokenRequestCtoModel
    {
        [JsonProperty("ecuFeatureRequests", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFeatureRequests> EcuFeatureRequests { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}