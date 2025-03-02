using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzIPsecEcuBitmaskResultCto
    {
        IDictionary<IPsdzEcuIdentifier, byte[]> SuccessEcus { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailedEcus { get; }
    }
}