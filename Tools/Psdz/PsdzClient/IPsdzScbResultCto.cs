using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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
