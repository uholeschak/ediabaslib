using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzMirrorProtocolVersionCto : IPsdzMirrorProtocolVersionCto
    {
        [DataMember]
        public int VERSION_BYTE_SIZE { get; set; } = 2;

        [DataMember]
        public int MajorVersion { get; set; }

        [DataMember]
        public int MinorVersion { get; set; }

        [DataMember]
        public int DEFAULT_MAJOR_VERSION { get; set; } = 1;

        [DataMember]
        public int DEFAULT_MINOR_VERSION { get; set; } = 31;
    }
}