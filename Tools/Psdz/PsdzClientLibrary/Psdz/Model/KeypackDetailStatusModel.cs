using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class KeypackDetailStatusModel
    {
        [JsonProperty("keyId", NullValueHandling = NullValueHandling.Ignore)]
        public string KeyId { get; set; }

        [JsonProperty("keypackStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? KeypackStatus { get; set; }
    }
}