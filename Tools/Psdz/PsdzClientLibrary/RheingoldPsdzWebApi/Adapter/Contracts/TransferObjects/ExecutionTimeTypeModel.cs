using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ExecutionTimeTypeModel
    {
        [JsonProperty("actualEndTime", NullValueHandling = NullValueHandling.Ignore)]
        public long ActualEndTime { get; set; }

        [JsonProperty("actualStartTime", NullValueHandling = NullValueHandling.Ignore)]
        public long ActualStartTime { get; set; }

        [JsonProperty("plannedEndTime", NullValueHandling = NullValueHandling.Ignore)]
        public long PlannedEndTime { get; set; }

        [JsonProperty("plannedStartTime", NullValueHandling = NullValueHandling.Ignore)]
        public long PlannedStartTime { get; set; }
    }
}