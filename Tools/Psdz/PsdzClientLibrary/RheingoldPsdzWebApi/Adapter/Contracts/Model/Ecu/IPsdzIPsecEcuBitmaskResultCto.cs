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