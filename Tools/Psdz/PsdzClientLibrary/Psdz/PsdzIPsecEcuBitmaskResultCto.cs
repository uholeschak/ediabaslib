using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    public class PsdzIPsecEcuBitmaskResultCto : IPsdzIPsecEcuBitmaskResultCto
    {
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, byte[]> SuccessEcus { get; set; }

        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> FailedEcus { get; set; }
    }
}