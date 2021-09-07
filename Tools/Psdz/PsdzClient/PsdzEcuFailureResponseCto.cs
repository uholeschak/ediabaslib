using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzLocalizableMessageTo))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzEcuFailureResponseCto : IPsdzEcuFailureResponseCto
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifierCto { get; set; }

        [DataMember]
        public ILocalizableMessageTo Cause { get; set; }
    }
}
