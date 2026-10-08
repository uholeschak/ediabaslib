using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCalculationNcdResultCto
    {
        IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; }

        IPsdzScbResultCto ScbResultCto { get; }
    }
}
