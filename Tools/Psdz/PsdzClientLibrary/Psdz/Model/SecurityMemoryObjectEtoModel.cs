using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class SecurityMemoryObjectEtoModel
    {
        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel Ecu { get; set; }

        [JsonProperty("serializedCertificate", NullValueHandling = NullValueHandling.Ignore)]
        public string SerializedCertificate { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityMemoryObjectSourceEto Source { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityMemoryObjectTypeEto Type { get; set; }
    }
}