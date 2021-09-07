using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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
    }
}
