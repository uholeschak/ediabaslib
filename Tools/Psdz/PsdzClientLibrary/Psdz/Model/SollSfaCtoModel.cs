using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SollSfaCtoModel
    {
        [JsonProperty("sollFeatures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFeatureTokenRelationCtoModel> SollFeatures { get; set; }
    }
}