using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzReadSecureEcuModeResultCto
    {
        IDictionary<IPsdzEcuIdentifier, PsdzSecureEcuModeEtoEnum> SecureEcuModes { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; }
    }
}
