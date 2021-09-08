using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzLocalizableMessageTo))]
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsFailureResponseCto : IPsdzKdsFailureResponseCto
    {
        [DataMember]
        public ILocalizableMessageTo Cause { get; set; }

        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }
    }
}
