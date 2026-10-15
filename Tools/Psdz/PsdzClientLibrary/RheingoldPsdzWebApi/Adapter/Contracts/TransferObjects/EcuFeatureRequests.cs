using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuFeatureRequests
    {
        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel Ecu { get; set; }

        [JsonProperty("featureRequests", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureRequestCtoModel> FeatureRequests { get; set; }
    }
}