using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzRequestNcdEto : IPsdzRequestNcdEto
    {
        [DataMember]
        public IPsdzSgbmId Btld { get; set; }

        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }
    }
}
