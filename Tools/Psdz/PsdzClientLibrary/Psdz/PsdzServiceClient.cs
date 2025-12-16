using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using PsdzClient;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
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

        private readonly ProgrammingServiceClient programmingService;

        private readonly PsdzEventService psdzEventService;

        private readonly PsdzProgressListenerDispatcher psdzProgressListenerDispatcher = new PsdzProgressListenerDispatcher();

        private readonly TalExecutionServiceClient talExecutionService;

        private readonly VcmServiceClient vcmService;

        private readonly ICertificateManagementService certificateManagementService;

        private readonly IndividualDataRestoreServiceClient individualDataRestoreService;

        private readonly HttpConfigurationServiceClient httpConfigurationService;

        private readonly SecureDiagnosticsServiceClient secureDiagnosticsService;

        private readonly SecureFeatureActivationServiceClient secureFeatureActivationService;

        private readonly SecurityManagementServiceClient securityManagementService;

        private readonly SecureCodingServiceClient secureCodingService;

        private readonly KdsServiceClient kdsService;

        [PreserveSource(Hint = "Dummy")]
        public IBaureiheUtilityService BaureiheUtilityService { get; private set; }

        public IConfigurationService ConfigurationService => configurationService;

        public IConnectionFactoryService ConnectionFactoryService => connectionFactoryService;

        public IConnectionManagerService ConnectionManagerService => connectionManagerService;

        public IEcuService EcuService => ecuService;

        public IEventManagerService EventManagerService { get; private set; }

        public ILogService LogService => logService;

        public ILogicService LogicService => logicService;

        public IMacrosService MacrosService => macrosService;

        public IObjectBuilderService ObjectBuilderService => objectBuilderServiceClient;

        public IProgrammingService ProgrammingService => programmingService;

        public ITalExecutionService TalExecutionService => talExecutionService;

        public IVcmService VcmService => vcmService;

        public ICertificateManagementService CertificateManagementService => certificateManagementService;

        public IIndividualDataRestoreService IndividualDataRestoreService => individualDataRestoreService;

        public ISecureFeatureActivationService SecureFeatureActivationService => secureFeatureActivationService;

        public IKdsService KdsService => kdsService;

        public ISecurityManagementService SecurityManagementService => securityManagementService;

        public IHttpConfigurationService HttpConfigurationService => httpConfigurationService;

        public ISecureDiagnosticsService SecureDiagnosticsService => secureDiagnosticsService;

        public ISecureCodingService SecureCodingService => secureCodingService;

        public PsdzServiceClient(string clientLogDir, int istaProcessId = 0)
        {
            NetNamedPipeBinding netNamedPipeBinding = new NetNamedPipeBinding
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = TimeSpan.MaxValue
            };
            netNamedPipeBinding.MaxReceivedMessageSize = 2147483647L;
            string clientId = Guid.NewGuid().ToString();
            if (istaProcessId == 0)
            {
                objectBuilderServiceClient = new ObjectBuilderServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ObjectBuilderService", clientId, clientLogDir));
                connectionFactoryService = new ConnectionFactoryServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ConnectionFactoryService", clientId, clientLogDir));
                connectionManagerService = new ConnectionManagerServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ConnectionManagerService", clientId, clientLogDir));
                logicService = new LogicServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/LogicService", clientId, clientLogDir));
                configurationService = new ConfigurationServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ConfigurationService", clientId, clientLogDir));
                psdzEventService = new PsdzEventService(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/EventManagerService", clientId, clientLogDir));
                vcmService = new VcmServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/VcmService", clientId, clientLogDir));
                programmingService = new ProgrammingServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/ProgrammingService", clientId, clientLogDir));
                ecuService = new EcuServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/EcuService", clientId, clientLogDir));
                talExecutionService = new TalExecutionServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/TalExecutionService", clientId, clientLogDir));
                logService = new LogServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/LogService", clientId, clientLogDir));
                macrosService = new MacrosServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/MacrosService", clientId, clientLogDir));
                certificateManagementService = new CertificateManagementServiceClient(netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/CertificateManagementService", clientId, clientLogDir));
                secureFeatureActivationService = new SecureFeatureActivationServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecureFeatureActivationService", clientId, clientLogDir));
                kdsService = new KdsServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/KdsService", clientId, clientLogDir));
                securityManagementService = new SecurityManagementServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecurityManagementService", clientId, clientLogDir));
                secureCodingService = new SecureCodingServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecureCodingService", clientId, clientLogDir));
                httpConfigurationService = new HttpConfigurationServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/HttpConfigurationService", clientId, clientLogDir));
                secureDiagnosticsService = new SecureDiagnosticsServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/SecureDiagnosticsService", clientId, clientLogDir));
                individualDataRestoreService = new IndividualDataRestoreServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress("net.pipe://localhost/PsdzServiceHost/IndividualDataRestoreService", clientId, clientLogDir));
            }
            else
            {
                objectBuilderServiceClient = new ObjectBuilderServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/ObjectBuilderService", clientId, clientLogDir));
                connectionFactoryService = new ConnectionFactoryServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/ConnectionFactoryService", clientId, clientLogDir));
                connectionManagerService = new ConnectionManagerServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/ConnectionManagerService", clientId, clientLogDir));
                logicService = new LogicServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/LogicService", clientId, clientLogDir));
                configurationService = new ConfigurationServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/ConfigurationService", clientId, clientLogDir));
                psdzEventService = new PsdzEventService(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/EventManagerService", clientId, clientLogDir));
                vcmService = new VcmServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/VcmService", clientId, clientLogDir));
                programmingService = new ProgrammingServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/ProgrammingService", clientId, clientLogDir));
                ecuService = new EcuServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/EcuService", clientId, clientLogDir));
                talExecutionService = new TalExecutionServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/TalExecutionService", clientId, clientLogDir));
                logService = new LogServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/LogService", clientId, clientLogDir));
                macrosService = new MacrosServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/MacrosService", clientId, clientLogDir));
                certificateManagementService = new CertificateManagementServiceClient(netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/CertificateManagementService", clientId, clientLogDir));
                secureFeatureActivationService = new SecureFeatureActivationServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/SecureFeatureActivationService", clientId, clientLogDir));
                kdsService = new KdsServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/KdsService", clientId, clientLogDir));
                securityManagementService = new SecurityManagementServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/SecurityManagementService", clientId, clientLogDir));
                secureCodingService = new SecureCodingServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/SecureCodingService", clientId, clientLogDir));
                httpConfigurationService = new HttpConfigurationServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/HttpConfigurationService", clientId, clientLogDir));
                secureDiagnosticsService = new SecureDiagnosticsServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/SecureDiagnosticsService", clientId, clientLogDir));
                individualDataRestoreService = new IndividualDataRestoreServiceClient(psdzProgressListenerDispatcher, netNamedPipeBinding, CreateEndpointAddress($"net.pipe://localhost/PsdzServiceHost{istaProcessId}/IndividualDataRestoreService", clientId, clientLogDir));
            }
        }

        private static EndpointAddress CreateEndpointAddress(string uri, string clientId, string clientLogDir)
        {
            return new EndpointAddress(new Uri(uri), AddressHeader.CreateAddressHeader("ClientIdentification", string.Empty, clientId), AddressHeader.CreateAddressHeader("ClientLogDir", string.Empty, clientLogDir));
        }

        public void AddPsdzEventListener(IPsdzEventListener psdzEventListener)
        {
            psdzEventService.AddEventListener(psdzEventListener);
        }

        public void AddPsdzProgressListener(IPsdzProgressListener psdzProgressListener)
        {
            psdzProgressListenerDispatcher.AddPsdzProgressListener(psdzProgressListener);
        }

        public void CloseAllConnections()
        {
            psdzEventService.RemoveAllEventListener();
            connectionFactoryService.CloseCachedChannels();
            configurationService.CloseCachedChannels();
            connectionManagerService.CloseCachedChannels();
            vcmService.CloseCachedChannels();
            logicService.CloseCachedChannels();
            programmingService.CloseCachedChannels();
            ecuService.CloseCachedChannels();
            objectBuilderServiceClient.CloseCachedChannels();
            talExecutionService.CloseCachedChannels();
            logService.CloseCachedChannels();
            macrosService.CloseCachedChannels();
            individualDataRestoreService.CloseCachedChannels();
            secureFeatureActivationService.CloseCachedChannels();
            psdzProgressListenerDispatcher.Clear();
        }

        public void Dispose()
        {
            psdzEventService.RemoveAllEventListener();
            CloseAllConnections();
            psdzProgressListenerDispatcher.Clear();
        }

        public void RemovePsdzEventListener(IPsdzEventListener psdzEventListener)
        {
            psdzEventService.RemoveEventListener(psdzEventListener);
        }

        public void RemovePsdzProgressListener(IPsdzProgressListener psdzProgressListener)
        {
            psdzProgressListenerDispatcher.RemovePsdzProgressListener(psdzProgressListener);
        }
    }
}
