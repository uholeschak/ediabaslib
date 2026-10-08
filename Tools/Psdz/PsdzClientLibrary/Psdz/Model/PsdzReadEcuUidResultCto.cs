using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzEcuUidCto))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [DataContract]
    public class PsdzReadEcuUidResultCto : IPsdzReadEcuUidResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, IPsdzEcuUidCto> EcuUids { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; set; }
    }
}
