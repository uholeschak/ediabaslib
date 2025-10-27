using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class TransactionProgressEventModel : EventModel
    {
        [JsonProperty("baseTransactionEvent", NullValueHandling = NullValueHandling.Ignore)]
        public TransactionEventModel BaseTransactionEvent { get; set; }

        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public int Progress { get; set; }

        [JsonProperty("taProgress", NullValueHandling = NullValueHandling.Ignore)]
        public int TaProgress { get; set; }
    }
}