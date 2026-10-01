using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzDetailedNcdInfoEto))]
    public class PsdzCheckNcdResultEto : IPsdzCheckNcdResultEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool isEachNcdSigned { get; set; }
    }
}
