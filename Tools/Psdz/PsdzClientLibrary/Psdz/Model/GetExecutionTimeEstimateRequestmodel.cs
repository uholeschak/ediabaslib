using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class GetExecutionTimeEstimateRequestmodel
    {
        [JsonProperty("parallel", NullValueHandling = NullValueHandling.Ignore)]
        public bool Parallel { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}