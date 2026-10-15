using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestEcuSecCheckingResultModel
    {
        [JsonProperty("ecuSecCheckingMaxWaitingTimes", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuCheckingMaxWaitingTimeResultModel> EcuSecCheckingMaxWaitingTimes { get; set; }

        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }
    }
}