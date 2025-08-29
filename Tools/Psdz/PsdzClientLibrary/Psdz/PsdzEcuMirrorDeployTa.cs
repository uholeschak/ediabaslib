using BMW.Rheingold.Psdz.Model.Tal;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzEcuMirrorDeployTa : PsdzTa
    {
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }

        [DataMember]
        public long EstimatedExecutionTime { get; set; }

        [DataMember]
        public long FlashFileSize { get; set; }

        [DataMember]
        public PsdzMirrorProtocolVersionCto ProtocolVersion { get; set; }

        [DataMember]
        public byte[] ProgrammingToken { get; set; }

        [DataMember]
        public bool UseDeltaSwe { get; set; } = true;

        [DataMember]
        public string SweFlashFile { get; set; }
    }
}