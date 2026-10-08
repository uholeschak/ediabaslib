using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SecureTokenRequestCtoModel
    {
        [JsonProperty("ecuFeatureRequests", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFeatureRequests> EcuFeatureRequests { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}