using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class BindingDetailStatusModel
    {
        [JsonProperty("bindingStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? BindingStatus { get; set; }

        [JsonProperty("certificateStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? CertificateStatus { get; set; }

        [JsonProperty("rollenName", NullValueHandling = NullValueHandling.Ignore)]
        public string RollenName { get; set; }
    }
}