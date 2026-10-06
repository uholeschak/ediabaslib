using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCalculationNcdResultCto
    {
        IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; }

        IPsdzScbResultCto ScbResultCto { get; }
    }
}
