using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class MCDDiagServiceEventModel : EventModel
    {
        [JsonProperty("errorId", NullValueHandling = NullValueHandling.Ignore)]
        public int ErrorId { get; set; }

        [JsonProperty("errorName", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorName { get; set; }

        [JsonProperty("isTimingEvent", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsTimingEvent { get; set; }

        [JsonProperty("jobName", NullValueHandling = NullValueHandling.Ignore)]
        public string JobName { get; set; }

        [JsonProperty("linkName", NullValueHandling = NullValueHandling.Ignore)]
        public string LinkName { get; set; }

        [JsonProperty("responseType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseType ResponseType { get; set; }

        [JsonProperty("serviceName", NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceName { get; set; }
    }
}