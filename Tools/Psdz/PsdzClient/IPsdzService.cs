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
#if false

		IEventManagerService EventManagerService { get; }

		IIndividualDataRestoreService IndividualDataRestoreService { get; }
#endif
        ILogService LogService { get; }
#if false
		ILogicService LogicService { get; }

		IMacrosService MacrosService { get; }
#endif
		IObjectBuilderService ObjectBuilderService { get; }
#if false
		IProgrammingService ProgrammingService { get; }

		ITalExecutionService TalExecutionService { get; }

		IVcmService VcmService { get; }

		ICbbTlsConfiguratorService CbbTlsConfiguratorService { get; }

		ICertificateManagementService CertificateManagementService { get; }

		ISecureFeatureActivationService SecureFeatureActivationService { get; }

		ISecurityManagementService SecurityManagementService { get; }

		ISecureCodingService SecureCodingService { get; }

		IKdsService KdsService { get; }
#endif
	}
}
