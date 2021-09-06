using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzRequestEcuSecCheckingResult
    {
        [DataMember]
        public IEnumerable<PsdzEcuFailureResponse> FailedEcus { get; set; }

        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, int> EcuSecCheckingMaxWaitingTimes { get; set; }
    }
}
