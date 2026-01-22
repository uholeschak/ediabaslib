using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzRequestNcdEto : IPsdzRequestNcdEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Btld { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }
    }
}
