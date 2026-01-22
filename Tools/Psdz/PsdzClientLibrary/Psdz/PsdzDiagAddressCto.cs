using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzDiagAddressCto : IPsdzDiagAddressCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int INVALID_OFFSET { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int MAX_OFFSETT { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int MIN_OFFSET { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsValid { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string OffsetSetAsHex { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int OffsetSetAsInt { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string OffsetSetAsString { get; set; }
    }
}
