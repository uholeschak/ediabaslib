using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuActivateTaModel : TaModel
    {
        [JsonProperty("protocolVersion", NullValueHandling = NullValueHandling.Ignore)]
        public MirrorProtocolVersionCtoModel ProtocolVersion { get; set; }

        [JsonProperty("programmingToken", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ProgrammingToken { get; set; }

        [JsonProperty("estimatedTime", NullValueHandling = NullValueHandling.Ignore)]
        public int EstimatedTime { get; set; }
    }
}