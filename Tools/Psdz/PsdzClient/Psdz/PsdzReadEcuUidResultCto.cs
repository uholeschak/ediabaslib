using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz.Model.SecurityManagement
{
    [KnownType(typeof(PsdzEcuUidCto))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [DataContract]
    public class PsdzReadEcuUidResultCto : IPsdzReadEcuUidResultCto
    {
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, IPsdzEcuUidCto> EcuUids { get; set; }

        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; set; }
    }
}
