using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzSecureCodingConfigCto
    {
        PsdzBackendNcdCalculationEtoEnum BackendNcdCalculationEtoEnum { get; }

        PsdzBackendSignatureEtoEnum BackendSignatureEtoEnum { get; }

        int ConnectionTimeout { get; }

        IList<string> Crls { get; }

        string NcdRootDirectory { get; }

        PsdzNcdRecalculationEtoEnum NcdRecalculationEtoEnum { get; }

        int Retries { get; }

        int ScbPollingTimeout { get; }

        IList<string> ScbUrls { get; }

        IList<string> SwlSecBackendUrls { get; }

        PsdzAuthenticationTypeEto PsdzAuthenticationTypeEto { get; }
    }
}
