using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzKdsClientsForRefurbishResultCto
    {
        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IList<IPsdzKdsIdCto> KdsIds { get; }
    }
}
