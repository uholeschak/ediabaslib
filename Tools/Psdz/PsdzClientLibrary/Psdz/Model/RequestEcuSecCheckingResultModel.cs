using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class RequestEcuSecCheckingResultModel
    {
        [JsonProperty("ecuSecCheckingMaxWaitingTimes", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuCheckingMaxWaitingTimeResultModel> EcuSecCheckingMaxWaitingTimes { get; set; }

        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }
    }
}