using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadStatusResultCtoModel
    {
        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }

        [JsonProperty("featureStatusSet", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FeatureLongStatusCtoModel> FeatureStatusSet { get; set; }
    }
}