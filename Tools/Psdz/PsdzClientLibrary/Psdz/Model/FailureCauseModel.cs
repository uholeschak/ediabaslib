using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FailureCauseModel
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("idReference", NullValueHandling = NullValueHandling.Ignore)]
        public string IdReference { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("messageId", NullValueHandling = NullValueHandling.Ignore)]
        public int MessageId { get; set; }

        [JsonProperty("talElement", NullValueHandling = NullValueHandling.Ignore)]
        public TalElementModel TalElement { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long Timestamp { get; set; }
    }
}