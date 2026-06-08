using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
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
