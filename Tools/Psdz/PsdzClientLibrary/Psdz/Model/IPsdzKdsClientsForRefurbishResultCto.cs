using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzKdsClientsForRefurbishResultCto
    {
        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IList<IPsdzKdsIdCto> KdsIds { get; }
    }
}
