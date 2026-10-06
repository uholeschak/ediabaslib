using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    public class PsdzIPsecEcuBitmaskResultCto : IPsdzIPsecEcuBitmaskResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, byte[]> SuccessEcus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> FailedEcus { get; set; }
    }
}