using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzMirrorProtocolVersionCto : IPsdzMirrorProtocolVersionCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int VERSION_BYTE_SIZE { get; set; } = 2;

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int MajorVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int MinorVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int DEFAULT_MAJOR_VERSION { get; set; } = 1;

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int DEFAULT_MINOR_VERSION { get; set; } = 31;
    }
}