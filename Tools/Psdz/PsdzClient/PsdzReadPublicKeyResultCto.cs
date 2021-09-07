using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzKdsPublicKeyResultCto))]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    public class PsdzReadPublicKeyResultCto : IPsdzReadPublicKeyResultCto
    {
        [DataMember]
        public IPsdzKdsFailureResponseCto FailureResponse { get; set; }

        [DataMember]
        public IList<IPsdzKdsPublicKeyResultCto> KdsPublicKeys { get; set; }
    }
}
