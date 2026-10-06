using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzFeatureRequestCto))]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzVin))]
    public class PsdzSecureTokenRequestCto : IPsdzSecureTokenRequestCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzVin VIN { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, IEnumerable<IPsdzFeatureRequestCto>> EcuFeatureRequests { get; set; }
    }
}
