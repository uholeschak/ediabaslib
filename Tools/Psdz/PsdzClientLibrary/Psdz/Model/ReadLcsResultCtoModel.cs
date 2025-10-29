using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadLcsResultCtoModel
    {
        [JsonProperty("ecuLcsValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuLcsValueCtoModel> EcuLcsValues { get; set; }

        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }
    }
}