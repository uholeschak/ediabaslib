using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
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
