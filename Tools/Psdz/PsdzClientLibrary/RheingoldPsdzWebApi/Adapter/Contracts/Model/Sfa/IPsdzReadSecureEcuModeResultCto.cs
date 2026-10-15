using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzReadSecureEcuModeResultCto
    {
        IDictionary<IPsdzEcuIdentifier, PsdzSecureEcuModeEtoEnum> SecureEcuModes { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; }
    }
}
