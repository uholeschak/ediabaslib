using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReadLcsResultCtoModel
    {
        [JsonProperty("ecuLcsValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuLcsValueCtoModel> EcuLcsValues { get; set; }

        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }
    }
}