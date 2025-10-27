using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class TalElementModel
    {
        [JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime EndTime { get; set; }

        [JsonProperty("executionStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TaExecutionStateModel ExecutionStatus { get; set; }

        [JsonProperty("failureCauses", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<FailureCauseModel> FailureCauses { get; set; }

        [JsonProperty("hasFailureCauses", NullValueHandling = NullValueHandling.Ignore)]
        public bool HasFailureCauses { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime StartTime { get; set; }
    }
}