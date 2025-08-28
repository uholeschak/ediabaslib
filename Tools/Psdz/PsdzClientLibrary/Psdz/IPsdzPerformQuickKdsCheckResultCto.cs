using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    public enum PsdzKdsActionStatusEto
    {
        ERROR,
        FORBIDDEN,
        IN_PROGRESS,
        PARTIAL,
        SUCCESS,
        TIMEOUT
    }

    public interface IPsdzPerformQuickKdsCheckResultCto
    {
        PsdzKdsActionStatusEto KdsActionStatus { get; }

        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IPsdzKdsIdCto KdsId { get; }

        IList<IPsdzKdsQuickCheckResultCto> KdsQuickCheckResult { get; }

        long ActionErrorCode { get; }
    }
}
