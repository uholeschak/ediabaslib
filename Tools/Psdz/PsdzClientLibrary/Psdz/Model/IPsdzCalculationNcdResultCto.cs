using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzCalculationNcdResultCto
    {
        IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; }

        IPsdzScbResultCto ScbResultCto { get; }
    }
}
