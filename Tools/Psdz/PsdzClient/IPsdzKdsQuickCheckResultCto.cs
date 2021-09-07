using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public enum PsdzQuickCheckResultEto
    {
        MASTER_INVALID_CLIENT_INVALID,
        MASTER_INVALID_CLIENT_OK,
        MASTER_OK_CLIENT_INVALID
    }

    public interface IPsdzKdsQuickCheckResultCto
    {
        IPsdzKdsIdCto KdsId { get; }

        PsdzQuickCheckResultEto QuickCheckResult { get; }
    }
}
