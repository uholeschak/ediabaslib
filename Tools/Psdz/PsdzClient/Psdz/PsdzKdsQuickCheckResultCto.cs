using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [KnownType(typeof(PsdzKdsIdCto))]
    [DataContract]
    public class PsdzKdsQuickCheckResultCto : IPsdzKdsQuickCheckResultCto
    {
        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }

        [DataMember]
        public PsdzQuickCheckResultEto QuickCheckResult { get; set; }
    }
}
