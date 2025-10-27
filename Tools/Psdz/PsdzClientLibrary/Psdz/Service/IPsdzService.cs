using BMW.Rheingold.Psdz.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzService
    {
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
    }
}
