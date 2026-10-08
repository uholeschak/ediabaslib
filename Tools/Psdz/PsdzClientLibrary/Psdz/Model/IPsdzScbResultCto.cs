using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzScbResultCto
    {
        int ScbDurationOfLastRequest { get; }

        IList<IPsdzSecurityBackendRequestFailureCto> ScbFailures { get; }

        PsdzSecurityBackendRequestProgressStatusToEnum ScbProgressStatus { get; }

        IPsdzNcdCalculationRequestIdEto ScbRequestId { get; }

        IPsdzScbResultStatusCto ScbResultStatusCto { get; }
    }
}
