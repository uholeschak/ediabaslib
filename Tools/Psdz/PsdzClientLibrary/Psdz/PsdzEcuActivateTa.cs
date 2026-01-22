using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuActivateTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int EstimatedTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzMirrorProtocolVersionCto ProtocolVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] ProgrammingToken { get; set; }
    }
}