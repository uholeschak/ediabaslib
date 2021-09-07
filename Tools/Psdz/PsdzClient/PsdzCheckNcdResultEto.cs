using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzDetailedNcdInfoEto))]
    [DataContract]
    public class PsdzCheckNcdResultEto : IPsdzCheckNcdResultEto
    {
        [DataMember]
        public IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; set; }

        [DataMember]
        public bool isEachNcdSigned { get; set; }
    }
}
