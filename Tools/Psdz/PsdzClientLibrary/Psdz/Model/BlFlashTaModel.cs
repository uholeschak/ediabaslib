using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class BlFlashTaModel : TaModel
    {
        [JsonProperty("actualProtocol", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolModel? ActualProtocol { get; set; }

        [JsonProperty("preferredProtocol", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolModel? PreferredProtocol { get; set; }
    }
}