using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;

namespace BMW.Rheingold.Psdz
{
    internal enum WebserviceSessionStatus
    {
        Created,
        ProcessStarted,
        Running
    }

    public interface IPsdzWebService
    {
        ICertificateManagementService CertificateManagementService { get; }

        IConfigurationService ConfigurationService { get; }

        IConnectionFactoryService ConnectionFactoryService { get; }

        IConnectionManagerService ConnectionManagerService { get; }

        IEcuService EcuService { get; }

        IEventManagerService EventManagerService { get; }

        IHttpConfigurationService HttpConfigurationService { get; }

        IHttpServerService HttpServerService { get; }

        IIndividualDataRestoreService IndividualDataRestoreService { get; }

        IKdsService KdsService { get; }

        ILogicService LogicService { get; }

        ILogService LogService { get; }

        IMacrosService MacrosService { get; }

        IProgrammingService ProgrammingService { get; }

        ISecureCodingService SecureCodingService { get; }

        ISecureDiagnosticsService SecureDiagnosticsService { get; }

        ISecureFeatureActivationService SecureFeatureActivationService { get; }

        ISecurityManagementService SecurityManagementService { get; }

        ITalExecutionService TalExecutionService { get; }

        IVcmService VcmService { get; }

        IObjectBuilderService ObjectBuilderService { get; }

        IProgrammingTokenService ProgrammingTokenService { get; }

        void StartIfNotRunning(string jrePath, string jvmOptions, string jarArguments);

        void Shutdown();

        void SetPsdzEventListener(IPsdzEventListener eventListener);

        void SetPsdzProgressListener(IPsdzProgressListener progressListener);

        void RemovePsdzEventListener(IPsdzEventListener eventListener);

        void RemovePsdzProgressListener(IPsdzProgressListener progressListener);

        bool IsReady();
    }
}