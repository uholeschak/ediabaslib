using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzReadEcuUidResultCto
    {
        IDictionary<IPsdzEcuIdentifier, IPsdzEcuUidCto> EcuUids { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; }
    }
}
