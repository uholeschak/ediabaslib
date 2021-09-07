using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzEcuLcsValueCto
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        int LcsNumber { get; }

        int LcsValue { get; }
    }
}
