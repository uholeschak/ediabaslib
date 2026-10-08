using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzKdsClientsForRefurbishResultCto
    {
        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IList<IPsdzKdsIdCto> KdsIds { get; }
    }
}
