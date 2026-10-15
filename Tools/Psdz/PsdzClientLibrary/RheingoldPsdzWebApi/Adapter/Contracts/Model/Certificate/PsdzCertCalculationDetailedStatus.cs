namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
{
    public enum PsdzCertCalculationDetailedStatus
    {
        CertDenied,
        CnameDenied,
        CnameMissing,
        EcuNameDenied,
        EcuNameMissing,
        EcuUidDenied,
        EcuUidFormat,
        EcuUidMismatch,
        EcuUidMissing,
        Error,
        Malformed,
        Ok,
        CaError,
        EcuDenied,
        KeyIdUnknown,
        KeyIdDenied
    }
}