namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    public interface IPsdzService
    {
        IBaureiheUtilityService BaureiheUtilityService { get; }

        IConfigurationService ConfigurationService { get; }

        IConnectionFactoryService ConnectionFactoryService { get; }

        IConnectionManagerService ConnectionManagerService { get; }

        IEcuService EcuService { get; }

        IEventManagerService EventManagerService { get; }

        IIndividualDataRestoreService IndividualDataRestoreService { get; }

        ILogService LogService { get; }

        ILogicService LogicService { get; }

        IMacrosService MacrosService { get; }

        IObjectBuilderService ObjectBuilderService { get; }

        IProgrammingService ProgrammingService { get; }

        ITalExecutionService TalExecutionService { get; }

        IVcmService VcmService { get; }

        ICertificateManagementService CertificateManagementService { get; }

        ISecureFeatureActivationService SecureFeatureActivationService { get; }

        ISecurityManagementService SecurityManagementService { get; }

        IHttpConfigurationService HttpConfigurationService { get; }

        ISecureDiagnosticsService SecureDiagnosticsService { get; }

        ISecureCodingService SecureCodingService { get; }

        IKdsService KdsService { get; }

        IFpService FpService { get; }
    }
}