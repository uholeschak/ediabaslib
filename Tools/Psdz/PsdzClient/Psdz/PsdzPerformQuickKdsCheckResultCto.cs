using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [DataContract]
    [KnownType(typeof(PsdzKdsQuickCheckResultCto))]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzPerformQuickKdsCheckResultCto : IPsdzPerformQuickKdsCheckResultCto
    {
        [DataMember]
        public PsdzKdsActionStatusEto KdsActionStatus { get; set; }

        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }

        [DataMember]
        public IList<IPsdzKdsQuickCheckResultCto> KdsQuickCheckResult { get; set; }
    }
}
