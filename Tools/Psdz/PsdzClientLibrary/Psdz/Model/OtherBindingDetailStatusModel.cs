using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class OtherBindingDetailStatusModel
    {
        [JsonProperty("ecuName", NullValueHandling = NullValueHandling.Ignore)]
        public string EcuName { get; set; }

        [JsonProperty("otherBindingStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? OtherBindingStatus { get; set; }

        [JsonProperty("rollenName", NullValueHandling = NullValueHandling.Ignore)]
        public string RollenName { get; set; }
    }
}