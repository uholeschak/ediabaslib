using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class FscDeployTaModel : TaModel
    {
        [JsonProperty("action", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SwtActionTypeModel? Action { get; set; }

        [JsonProperty("applicationId", NullValueHandling = NullValueHandling.Ignore)]
        public SwtApplicationIdModel ApplicationId { get; set; }

        [JsonProperty("fsc", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Fsc { get; set; }

        [JsonProperty("fscCert", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] FscCert { get; set; }
    }
}