using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzBlFlashTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
