using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
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
