using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public enum PsdzBackendNcdCalculationEtoEnum
    {
        ALLOW,
        FORCE,
        MUST_NOT
    }

    public enum PsdzBackendSignatureEtoEnum
    {
        ALLOW,
        FORCE,
        MUST_NOT
    }

    public enum PsdzNcdRecalculationEtoEnum
    {
        ALLOW,
        FORCE
    }

    public enum PsdzAuthenticationTypeEto
    {
        SSL,
        BASIC,
        BEARER,
        UNKNOWN
    }

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
