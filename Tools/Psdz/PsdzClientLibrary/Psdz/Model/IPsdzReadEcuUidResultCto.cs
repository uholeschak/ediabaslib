using System.Collections.Generic;
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
