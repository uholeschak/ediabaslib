using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class FetchEcuSecCheckingResultModel
    {
        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }

        [JsonProperty("results", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuSecCheckingResponseEtoModel> Results { get; set; }
    }
}