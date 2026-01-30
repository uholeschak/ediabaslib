using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    [KnownType(typeof(PsdzNcd))]
    public class PsdzCalculatedNcdsEto : IPsdzCalculatedNcdsEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Btld { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId CafdId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzNcd Ncd { get; set; }
    }
}
