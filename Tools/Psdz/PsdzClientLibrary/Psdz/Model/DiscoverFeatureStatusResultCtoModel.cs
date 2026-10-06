using Newtonsoft.Json;
using System.Collections.Generic;
using BMW.Rheingold.Psdz;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class DiscoverFeatureStatusResultCtoModel
    {
        [JsonProperty("errorMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [JsonProperty("featureStatusList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureStatusToModel> FeatureStatusList { get; set; }
    }
}