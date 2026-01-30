namespace BMW.Rheingold.Psdz.Model.Certificate
{
    public enum PsdzSecurityCalculationOverallStatus
    {
        Conflict,
        Detailed,
        Empty,
        Error,
        Malformed,
        Ok,
        UnknownVersion,
        VinMalformed,
        WrongFormat,
        InvalidFatRequest,
        RequestNotOnCertStore,
        InvalidEcuTypeCerts,
        InvalidSignature,
        OTHER_ERROR
    }
}