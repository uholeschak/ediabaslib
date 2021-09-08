using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [DataContract]
    [KnownType(typeof(PsdzEcuLcsValueCto))]
    public class PsdzReadLcsResultCto : IPsdzReadLcsResultCto
    {
        [DataMember]
        public IEnumerable<IPsdzEcuLcsValueCto> EcuLcsValues { get; set; }

        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; set; }
    }
}
