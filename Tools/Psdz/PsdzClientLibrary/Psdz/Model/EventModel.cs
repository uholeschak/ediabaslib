using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("MCDDiagServiceEventModel", typeof(MCDDiagServiceEventModel))]
    [JsonInheritance("ProgressEventModel", typeof(ProgressEventModel))]
    [JsonInheritance("TransactionEventModel", typeof(TransactionEventModel))]
    [JsonInheritance("TransactionProgressEventModel", typeof(TransactionProgressEventModel))]
    public class EventModel
    {
        [JsonProperty("ecuId", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuId { get; set; }

        [JsonProperty("eventId", NullValueHandling = NullValueHandling.Ignore)]
        public string EventId { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("messageId", NullValueHandling = NullValueHandling.Ignore)]
        public int MessageId { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long Timestamp { get; set; }
    }
}