using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [KnownType(typeof(PsdzFeatureRequestCto))]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzVin))]
    public class PsdzSecureTokenRequestCto : IPsdzSecureTokenRequestCto
    {
        [DataMember]
        public IPsdzVin VIN { get; set; }

        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, IEnumerable<IPsdzFeatureRequestCto>> EcuFeatureRequests { get; set; }
    }
}
