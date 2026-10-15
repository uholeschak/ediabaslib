using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GetExecutionTimeEstimateRequestmodel
    {
        [JsonProperty("parallel", NullValueHandling = NullValueHandling.Ignore)]
        public bool Parallel { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}