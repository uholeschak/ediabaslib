using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzKdsClientsForRefurbishResultCto
    {
        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IList<IPsdzKdsIdCto> KdsIds { get; }
    }
}
