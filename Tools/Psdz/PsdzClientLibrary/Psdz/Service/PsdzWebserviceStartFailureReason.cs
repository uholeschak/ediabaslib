namespace BMW.Rheingold.Psdz
{
    public enum PsdzWebserviceStartFailureReason
    {
        None,
        PortInUse,
        JarMissingOrCorrupt,
        JavaRuntimeFaulty,
        SpringBootStartupError,
        Timeout,
        AccessDenied,
        MissingRights,
        SdpFaulty,
        JavaRuntimeInstallationFaulty,
        JavaVersionError,
        Unknown
    }
}