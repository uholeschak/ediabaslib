using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzKdsActionStatusResultCto
    {
        PsdzKdsActionStatusEto KdsActionStatus { get; }

        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IPsdzKdsIdCto KdsId { get; }
    }
}
