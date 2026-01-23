using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public enum PsdzProtocol
    {
        KWP2000,
        UDS,
        HTTP,
        MIRROR
    }

    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzSwDeployTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
