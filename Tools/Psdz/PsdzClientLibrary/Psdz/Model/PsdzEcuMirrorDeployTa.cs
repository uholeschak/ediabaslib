using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Communications;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzEcuMirrorDeployTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long EstimatedExecutionTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long FlashFileSize { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzMirrorProtocolVersionCto ProtocolVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] ProgrammingToken { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool UseDeltaSwe { get; set; } = true;

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string SweFlashFile { get; set; }
    }
}