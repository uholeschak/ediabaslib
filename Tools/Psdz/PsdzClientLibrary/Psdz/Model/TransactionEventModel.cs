using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class TransactionEventModel : EventModel
    {
        [JsonProperty("transactionInfo", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionInfoModel TransactionInfo { get; set; }

        [JsonProperty("transactionType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TACategories TransactionType { get; set; }
    }
}