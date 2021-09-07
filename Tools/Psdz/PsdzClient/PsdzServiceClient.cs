using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	public class PsdzServiceClient : IDisposable, IPsdzService
	{
        private readonly ConfigurationServiceClient configurationService;

        private readonly ConnectionFactoryServiceClient connectionFactoryService;

        private readonly ConnectionManagerServiceClient connectionManagerService;

        private readonly EcuServiceClient ecuService;

        private readonly LogServiceClient logService;

        private readonly LogicServiceClient logicService;

        private readonly MacrosServiceClient macrosService;

        private readonly ObjectBuilderServiceClient objectBuilderServiceClient;

        private readonly Psdz.ProgrammingServiceClient programmingService;

        private readonly PsdzEventService psdzEventService;

        private readonly PsdzProgressListenerDispatcher psdzProgressListenerDispatcher = new PsdzProgressListenerDispatcher();

        private readonly TalExecutionServiceClient talExecutionService;

        private readonly VcmServiceClient vcmService;

        private readonly CbbTlsConfiguratorServiceClient cbbTlsConfiguratorService;

        private readonly ICertificateManagementService certificateManagementService;

        private readonly IndividualDataRestoreServiceClient individualDataRestoreService;

        private readonly SecureFeatureActivationServiceClient secureFeatureActivationService;

        private readonly SecurityManagementServiceClient securityManagementService;

        private readonly SecureCodingServiceClient secureCodingService;

        private readonly KdsServiceClient kdsService;

        public PsdzServiceClient(string clientLogDir)
		{
			NetNamedPipeBinding netNamedPipeBinding = new NetNamedPipeBinding
			{
				ReceiveTimeout = TimeSpan.MaxValue,
				SendTimeout = TimeSpan.MaxValue
			};
			netNamedPipeBinding.MaxReceivedMessageSize = 2147483647L;
			string clientId = Guid.NewGuid().ToString();
			this.objectBuilderServiceClient = new ObjectBuilderServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ObjectBuilderService", clientId, clientLogDir));
			this.connectionFactoryService = new ConnectionFactoryServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ConnectionFactoryService", clientId, clientLogDir));
			this.connectionManagerService = new ConnectionManagerServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ConnectionManagerService", clientId, clientLogDir));
			this.logicService = new LogicServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/LogicService", clientId, clientLogDir));
			this.configurationService = new ConfigurationServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ConfigurationService", clientId, clientLogDir));
			this.psdzEventService = new PsdzEventService(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/EventManagerService", clientId, clientLogDir));
			this.vcmService = new VcmServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/VcmService", clientId, clientLogDir));
			this.programmingService = new Psdz.ProgrammingServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ProgrammingService", clientId, clientLogDir));
			this.ecuService = new EcuServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/EcuService", clientId, clientLogDir));
			this.talExecutionService = new TalExecutionServiceClient(this.psdzProgressListenerDispatcher, netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/TalExecutionService", clientId, clientLogDir));
			this.logService = new LogServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/LogService", clientId, clientLogDir));
			this.macrosService = new MacrosServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/MacrosService", clientId, clientLogDir));
			this.cbbTlsConfiguratorService = new CbbTlsConfiguratorServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/CbbTlsConfiguratorService", clientId, clientLogDir));
			this.certificateManagementService = new CertificateManagementServiceClient(netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/CertificateManagementService", clientId, clientLogDir));
			this.secureFeatureActivationService = new SecureFeatureActivationServiceClient(this.psdzProgressListenerDispatcher, netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecureFeatureActivationService", clientId, clientLogDir));
			this.kdsService = new KdsServiceClient(this.psdzProgressListenerDispatcher, netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/KdsService", clientId, clientLogDir));
			this.securityManagementService = new SecurityManagementServiceClient(this.psdzProgressListenerDispatcher, netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecurityManagementService", clientId, clientLogDir));
			this.secureCodingService = new SecureCodingServiceClient(this.psdzProgressListenerDispatcher, netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecureCodingService", clientId, clientLogDir));
			this.individualDataRestoreService = new IndividualDataRestoreServiceClient(this.psdzProgressListenerDispatcher, netNamedPipeBinding, PsdzServiceClient.CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/IndividualDataRestoreService", clientId, clientLogDir));
		}

		private static EndpointAddress CreateEndpointAddress(string uri, string clientId, string clientLogDir)
		{
			return new EndpointAddress(new Uri(uri), new AddressHeader[]
			{
				AddressHeader.CreateAddressHeader("ClientIdentification", string.Empty, clientId),
				AddressHeader.CreateAddressHeader("ClientLogDir", string.Empty, clientLogDir)
			});
		}

		public IConfigurationService ConfigurationService
		{
			get
			{
				return this.configurationService;
			}
		}

		public IConnectionFactoryService ConnectionFactoryService
		{
			get
			{
				return this.connectionFactoryService;
			}
		}

		public IConnectionManagerService ConnectionManagerService
		{
			get
			{
				return this.connectionManagerService;
			}
		}

		public IEcuService EcuService
		{
			get
			{
				return this.ecuService;
			}
		}

		public IEventManagerService EventManagerService { get; private set; }

		public ILogService LogService
		{
			get
			{
				return this.logService;
			}
		}

		public ILogicService LogicService
		{
			get
			{
				return this.logicService;
			}
		}

		public IMacrosService MacrosService
		{
			get
			{
				return this.macrosService;
			}
		}

		public IObjectBuilderService ObjectBuilderService
		{
			get
			{
				return this.objectBuilderServiceClient;
			}
		}

		public Psdz.IProgrammingService ProgrammingService
		{
			get
			{
				return this.programmingService;
			}
		}

		public ITalExecutionService TalExecutionService
		{
			get
			{
				return this.talExecutionService;
			}
		}

		public IVcmService VcmService
		{
			get
			{
				return this.vcmService;
			}
		}

		public ICbbTlsConfiguratorService CbbTlsConfiguratorService
		{
			get
			{
				return this.cbbTlsConfiguratorService;
			}
		}

		public ICertificateManagementService CertificateManagementService
		{
			get
			{
				return this.certificateManagementService;
			}
		}

		public IIndividualDataRestoreService IndividualDataRestoreService
		{
			get
			{
				return this.individualDataRestoreService;
			}
		}

		public ISecureFeatureActivationService SecureFeatureActivationService
		{
			get
			{
				return this.secureFeatureActivationService;
			}
		}

		public IKdsService KdsService
		{
			get
			{
				return this.kdsService;
			}
		}

		public ISecurityManagementService SecurityManagementService
		{
			get
			{
				return this.securityManagementService;
			}
		}

		public ISecureCodingService SecureCodingService
		{
			get
			{
				return this.secureCodingService;
			}
		}

		public void AddPsdzEventListener(IPsdzEventListener psdzEventListener)
		{
			this.psdzEventService.AddEventListener(psdzEventListener);
		}

		public void AddPsdzProgressListener(IPsdzProgressListener psdzProgressListener)
		{
			this.psdzProgressListenerDispatcher.AddPsdzProgressListener(psdzProgressListener);
		}

		public void CloseAllConnections()
		{
			this.psdzEventService.RemoveAllEventListener();
			this.connectionFactoryService.CloseCachedChannels();
			this.configurationService.CloseCachedChannels();
			this.connectionManagerService.CloseCachedChannels();
			this.vcmService.CloseCachedChannels();
			this.logicService.CloseCachedChannels();
			this.programmingService.CloseCachedChannels();
			this.ecuService.CloseCachedChannels();
			this.objectBuilderServiceClient.CloseCachedChannels();
			this.talExecutionService.CloseCachedChannels();
			this.logService.CloseCachedChannels();
			this.macrosService.CloseCachedChannels();
			this.individualDataRestoreService.CloseCachedChannels();
			this.secureFeatureActivationService.CloseCachedChannels();
			this.psdzProgressListenerDispatcher.Clear();
		}

		public void Dispose()
		{
			this.psdzEventService.RemoveAllEventListener();
			this.CloseAllConnections();
			this.psdzProgressListenerDispatcher.Clear();
		}

		public void RemovePsdzEventListener(IPsdzEventListener psdzEventListener)
		{
			this.psdzEventService.RemoveEventListener(psdzEventListener);
		}

		public void RemovePsdzProgressListener(IPsdzProgressListener psdzProgressListener)
		{
			this.psdzProgressListenerDispatcher.RemovePsdzProgressListener(psdzProgressListener);
		}
    }
}
