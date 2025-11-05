using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace BMW.Rheingold.Psdz
{
    internal static class SecureCodingConfigCtoMapper
    {
        private static AuthenticationTypeEtoMapper _authenticationTypeEtoMapper = new AuthenticationTypeEtoMapper();
        private static BackendNcdCalculationEtoMapper _backendNcdCalculationEtoMapper = new BackendNcdCalculationEtoMapper();
        private static BackendSignatureEtoMapper _backendSignatureEtoMapper = new BackendSignatureEtoMapper();
        private static NcdRecalculationEtoMapper _ncdRecalculationEtoMapper = new NcdRecalculationEtoMapper();
        public static SecureCodingConfigCtoModel Map(IPsdzSecureCodingConfigCto psdzSecureCodingConfigCto)
        {
            if (psdzSecureCodingConfigCto == null)
            {
                return null;
            }

            return new SecureCodingConfigCtoModel
            {
                AuthenticationTypeEto = _authenticationTypeEtoMapper.GetValue(psdzSecureCodingConfigCto.PsdzAuthenticationTypeEto),
                BackendNcdCalculationEto = _backendNcdCalculationEtoMapper.GetValue(psdzSecureCodingConfigCto.BackendNcdCalculationEtoEnum),
                BackendSignatureEto = _backendSignatureEtoMapper.GetValue(psdzSecureCodingConfigCto.BackendSignatureEtoEnum),
                NcdRecalculationEto = _ncdRecalculationEtoMapper.GetValue(psdzSecureCodingConfigCto.NcdRecalculationEtoEnum),
                NcdRootDirectory = psdzSecureCodingConfigCto.NcdRootDirectory,
                SwlSecBackendUrls = psdzSecureCodingConfigCto.SwlSecBackendUrls,
                ScbUrls = psdzSecureCodingConfigCto.ScbUrls,
                ScbPollingTimeout = psdzSecureCodingConfigCto.ScbPollingTimeout,
                Retries = psdzSecureCodingConfigCto.Retries,
                ConnectionTimeout = psdzSecureCodingConfigCto.ConnectionTimeout,
                Crls = psdzSecureCodingConfigCto.Crls
            };
        }
    }
}