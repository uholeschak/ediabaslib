using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzProgrammingTokensResultCto
    {
        IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; }

        IEnumerable<IPsdzProgrammingTokenCto> Tokens { get; }
    }
}