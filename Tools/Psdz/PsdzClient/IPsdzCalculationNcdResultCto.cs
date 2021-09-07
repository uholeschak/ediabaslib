using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzCalculationNcdResultCto
    {
        string CalculatedNcdAsString { get; }

        IList<IPsdzCalculatedNcdsEto> CalculatedNcds { get; }

        IPsdzScbResultCto ScbResultCto { get; }
    }
}
