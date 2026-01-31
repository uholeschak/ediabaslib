namespace BMW.Rheingold.Psdz.Model.Certificate
{
    public enum PsdzEcuCertCheckingStatus
    {
        CheckStillRunning,
        Decryption_Error,
        Empty,
        Incomplete,
        IssuerCertError,
        Malformed,
        Ok,
        Other,
        Outdated,
        OwnCertNotPresent,
        SecurityError,
        Unchecked,
        Undefined,
        WrongEcuUid,
        WrongVin17,
        KeyError,
        NotUsed
    }
}