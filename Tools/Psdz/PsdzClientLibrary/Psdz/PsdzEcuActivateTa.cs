using BMW.Rheingold.Psdz.Model.Tal;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzEcuActivateTa : PsdzTa
    {
        [DataMember]
        public int EstimatedTime { get; set; }

        [DataMember]
        public PsdzMirrorProtocolVersionCto ProtocolVersion { get; set; }

        [DataMember]
        public byte[] ProgrammingToken { get; set; }
    }
}