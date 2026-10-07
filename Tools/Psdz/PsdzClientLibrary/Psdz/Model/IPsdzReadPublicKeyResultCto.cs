using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzReadPublicKeyResultCto
    {
        IPsdzKdsFailureResponseCto FailureResponse { get; }

        IList<IPsdzKdsPublicKeyResultCto> KdsPublicKeys { get; }
    }
}
