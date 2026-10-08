using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SmacSwDeployOnMasterTaModel : TaModel
    {
        [JsonProperty("actualProtocol", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolModel? ActualProtocol { get; set; }

        [JsonProperty("preferredProtocol", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolModel? PreferredProtocol { get; set; }

        [JsonProperty("smacIds", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> SmacIds { get; set; }
    }
}