using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SfaPerEcuOptionsModel
    {
        [JsonProperty("categoryAction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionValues CategoryAction { get; set; }

        [JsonProperty("sfaWriteAction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionValues SfaWriteAction { get; set; }

        [JsonProperty("sfaDeleteAction", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionValues SfaDeleteAction { get; set; }
    }
}
