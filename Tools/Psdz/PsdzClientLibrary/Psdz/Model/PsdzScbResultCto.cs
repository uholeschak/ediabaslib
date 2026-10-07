using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzScbResultStatusCto))]
    [DataContract]
    [KnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
    [KnownType(typeof(PsdzNcdCalculationRequestIdEto))]
    public class PsdzScbResultCto : IPsdzScbResultCto
    {
        public int ScbDurationOfLastRequest { get; set; }

        public IList<IPsdzSecurityBackendRequestFailureCto> ScbFailures { get; set; }

        public PsdzSecurityBackendRequestProgressStatusToEnum ScbProgressStatus { get; set; }

        public IPsdzNcdCalculationRequestIdEto ScbRequestId { get; set; }

        public IPsdzScbResultStatusCto ScbResultStatusCto { get; set; }
    }
}
