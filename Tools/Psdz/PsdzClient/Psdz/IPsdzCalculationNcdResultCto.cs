using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzCalculationNcdResultCto
    {
        string CalculatedNcdAsString { get; }

        IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; }

        IPsdzScbResultCto ScbResultCto { get; }
    }
}
