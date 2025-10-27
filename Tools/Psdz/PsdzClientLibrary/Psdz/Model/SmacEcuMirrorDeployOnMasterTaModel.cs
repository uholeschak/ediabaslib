using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SmacEcuMirrorDeployOnMasterTaModel : TaModel
    {
        [JsonProperty("actualProtocol", NullValueHandling = NullValueHandling.Ignore)]
        public ProtocolModel? ActualProtocol { get; set; }

        [JsonProperty("preferredProtocol", NullValueHandling = NullValueHandling.Ignore)]
        public ProtocolModel? PreferredProtocol { get; set; }

        [JsonProperty("estimatedExecutionTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EstimatedExecutionTime { get; set; }

        [JsonProperty("flashFileSize", NullValueHandling = NullValueHandling.Ignore)]
        public long FlashFileSize { get; set; }

        [JsonProperty("protocolVersion", NullValueHandling = NullValueHandling.Ignore)]
        public MirrorProtocolVersionCtoModel ProtocolVersion { get; set; }

        [JsonProperty("programmingToken", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ProgrammingToken { get; set; }

        [JsonProperty("useDeltaSwe", NullValueHandling = NullValueHandling.Ignore)]
        public bool UseDeltaSwe { get; set; }

        [JsonProperty("sweFlashFile", NullValueHandling = NullValueHandling.Ignore)]
        public string SweFlashFile { get; set; }

        [JsonProperty("smacIds", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> SmacIds { get; set; }
    }
}