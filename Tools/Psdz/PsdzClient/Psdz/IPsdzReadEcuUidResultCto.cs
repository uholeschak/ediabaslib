using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz.Model.SecurityManagement
{
    public interface IPsdzReadEcuUidResultCto
    {
        IDictionary<IPsdzEcuIdentifier, IPsdzEcuUidCto> EcuUids { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; }
    }
}
