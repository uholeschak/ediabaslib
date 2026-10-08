using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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