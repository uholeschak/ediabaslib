using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzReadLcsResultCto
    {
        IEnumerable<IPsdzEcuLcsValueCto> EcuLcsValues { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; }
    }
}
