using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SetLcsRequestModel
    {
        [JsonProperty("ecuLcsValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuLcsValueCtoModel> EcuLcsValues { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}