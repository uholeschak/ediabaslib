using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzPerformQuickKdsCheckResultCto
    {
        PsdzKdsActionStatusEto KdsActionStatus { get; }

        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IPsdzKdsIdCto KdsId { get; }

        IList<IPsdzKdsQuickCheckResultCto> KdsQuickCheckResult { get; }

        long ActionErrorCode { get; }
    }
}
