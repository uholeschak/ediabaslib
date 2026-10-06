using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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