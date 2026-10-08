using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzReadPublicKeyResultCto
    {
        IPsdzKdsFailureResponseCto FailureResponse { get; }

        IList<IPsdzKdsPublicKeyResultCto> KdsPublicKeys { get; }
    }
}
