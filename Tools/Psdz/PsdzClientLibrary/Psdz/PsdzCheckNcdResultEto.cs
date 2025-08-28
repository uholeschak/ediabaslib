using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [DataContract]
    [KnownType(typeof(PsdzDetailedNcdInfoEto))]
    public class PsdzCheckNcdResultEto : IPsdzCheckNcdResultEto
    {
        [DataMember]
        public IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; set; }

        [DataMember]
        public bool isEachNcdSigned { get; set; }
    }
}
