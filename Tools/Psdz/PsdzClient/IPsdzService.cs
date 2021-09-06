using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	public interface IPsdzService
	{
		IConfigurationService ConfigurationService { get; }

        IConnectionFactoryService ConnectionFactoryService { get; }

		IConnectionManagerService ConnectionManagerService { get; }

        IEcuService EcuService { get; }

		IEventManagerService EventManagerService { get; }
#if false
		IIndividualDataRestoreService IndividualDataRestoreService { get; }
#endif
		ILogService LogService { get; }

        ILogicService LogicService { get; }

		IMacrosService MacrosService { get; }

		IObjectBuilderService ObjectBuilderService { get; }

		IProgrammingService ProgrammingService { get; }

		ITalExecutionService TalExecutionService { get; }

		IVcmService VcmService { get; }

		ICbbTlsConfiguratorService CbbTlsConfiguratorService { get; }

		ICertificateManagementService CertificateManagementService { get; }
#if false
        ISecureFeatureActivationService SecureFeatureActivationService { get; }

		ISecurityManagementService SecurityManagementService { get; }

		ISecureCodingService SecureCodingService { get; }

		IKdsService KdsService { get; }
#endif
	}
}
