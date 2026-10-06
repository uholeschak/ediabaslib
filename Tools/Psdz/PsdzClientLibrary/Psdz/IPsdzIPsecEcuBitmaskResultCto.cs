using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzIPsecEcuBitmaskResultCto
    {
        IDictionary<IPsdzEcuIdentifier, byte[]> SuccessEcus { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailedEcus { get; }
    }
}