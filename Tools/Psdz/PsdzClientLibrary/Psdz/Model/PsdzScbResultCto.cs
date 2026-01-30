using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
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
