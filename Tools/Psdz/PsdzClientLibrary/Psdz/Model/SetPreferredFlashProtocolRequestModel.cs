using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetPreferredFlashProtocolRequestModel
    {
        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel Ecu { get; set; }

        [JsonProperty("protocol", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolModel? Protocol { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}