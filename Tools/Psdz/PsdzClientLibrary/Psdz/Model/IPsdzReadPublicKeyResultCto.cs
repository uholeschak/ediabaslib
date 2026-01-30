using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzReadPublicKeyResultCto
    {
        IPsdzKdsFailureResponseCto FailureResponse { get; }

        IList<IPsdzKdsPublicKeyResultCto> KdsPublicKeys { get; }
    }
}
