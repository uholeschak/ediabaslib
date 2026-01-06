using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using PsdzClient;

namespace BMW.Rheingold.Programming
{
    public class PsdzWebServiceWrapper : IPsdz, IPsdzService, IPsdzInfo
    {
        private readonly PsdzWebServiceConfig _psdzConfig;
        private ProdiasLoglevel? _prodiasLoglevel = ProdiasLoglevel.ERROR;
        private PsdzLoglevel? _psdzLoglevel = PsdzLoglevel.FINE;
        private IPsdzObjectBuilder _objectBuilder;
        private IPsdzWebService _psdzWebService { get; }
        public IBaureiheUtilityService BaureiheUtilityService => _psdzWebService.BaureiheUtilityService;
        public ICertificateManagementService CertificateManagementService => _psdzWebService.CertificateManagementService;
        public IConfigurationService ConfigurationService => _psdzWebService.ConfigurationService;
        public IConnectionFactoryService ConnectionFactoryService => _psdzWebService.ConnectionFactoryService;
        public IConnectionManagerService ConnectionManagerService => _psdzWebService.ConnectionManagerService;
        public IEcuService EcuService => _psdzWebService.EcuService;
        public IEventManagerService EventManagerService => _psdzWebService.EventManagerService;
        public IHttpConfigurationService HttpConfigurationService => _psdzWebService.HttpConfigurationService;
        public IHttpServerService HttpServerService => _psdzWebService.HttpServerService;
        public IIndividualDataRestoreService IndividualDataRestoreService => _psdzWebService.IndividualDataRestoreService;
        public IKdsService KdsService => _psdzWebService.KdsService;
        public ILogicService LogicService => _psdzWebService.LogicService;
        public ILogService LogService => _psdzWebService.LogService;
        public IMacrosService MacrosService => _psdzWebService.MacrosService;
        public IObjectBuilderService ObjectBuilderService => _psdzWebService.ObjectBuilderService;
        public IProgrammingService ProgrammingService => _psdzWebService.ProgrammingService;
        public ISecureCodingService SecureCodingService => _psdzWebService.SecureCodingService;
        public ISecureFeatureActivationService SecureFeatureActivationService => _psdzWebService.SecureFeatureActivationService;
        public ISecurityManagementService SecurityManagementService => _psdzWebService.SecurityManagementService;
        public ITalExecutionService TalExecutionService => _psdzWebService.TalExecutionService;
        public IVcmService VcmService => _psdzWebService.VcmService;
        public ISecureDiagnosticsService SecureDiagnosticsService => _psdzWebService.SecureDiagnosticsService;
        public IProgrammingTokenService ProgrammingTokenService => _psdzWebService.ProgrammingTokenService;
        public bool IsValidPsdzVersion => true;
        public string PsdzDataPath => _psdzWebService.ConfigurationService.GetRootDirectory();
        public string PsdzVersion => _psdzWebService.ConfigurationService.GetPsdzVersion();

        [PreserveSource(Hint = "PsdzObjectBuilder modified", OriginalHash = "C48486360A1B592A23F907B3C084F52A")]
        public IPsdzObjectBuilder ObjectBuilder
        {
            get
            {
                if (_objectBuilder == null)
                {
                    _objectBuilder = new PsdzObjectBuilder(ObjectBuilderService, this);
                }

                return _objectBuilder;
            }
        }

        public bool IsPsdzInitialized
        {
            get
            {
                if (!PsdzStarterGuard.Instance.CanCheckAvailability())
                {
                    return false;
                }

                return _psdzWebService.IsReady();
            }
        }

        public string PsdzServiceLogFilePath => _psdzConfig.PsdzWebServiceLogFilePath;
        public string PsdzLogFilePath => _psdzConfig.PsdzLogFilePath;
        public string ProdiasDriverLogFilePath => _psdzConfig.ProdiasDriverLogFilePath;

        [PreserveSource(Hint = "istaFolder added", SignatureModified = true)]
        public PsdzWebServiceWrapper(PsdzWebServiceConfig psdzConfig, string istaFolder)
        {
            _psdzConfig = psdzConfig ?? throw new ArgumentNullException("psdzConfig");
            //[+] _istaFolder = istaFolder;
            _istaFolder = istaFolder;
            _psdzWebService = new PsdzWebService(_psdzConfig.PsdzWebApiLogDir, () => PsdzStarterGuard.Instance.CanCheckAvailability(), _istaFolder);
        }

        public void StartIfNotRunning()
        {
            try
            {
                //[-] _psdzWebService.StartIfNotRunning(Psdz64BitPathResolver.GetJrePath(), _psdzConfig.GetJvmOptionsAsOneString(), _psdzConfig.GetJarArgumentsAsOneString());
                //[+] _psdzWebService.StartIfNotRunning(Psdz64BitPathResolver.GetJrePath(_istaFolder, true), _psdzConfig.GetJvmOptionsAsOneString(), _psdzConfig.GetJarArgumentsAsOneString());
                _psdzWebService.StartIfNotRunning(Psdz64BitPathResolver.GetJrePath(_istaFolder, true), _psdzConfig.GetJvmOptionsAsOneString(), _psdzConfig.GetJarArgumentsAsOneString());
            }
            catch (JavaInstallationException exception)
            {
                Log.ErrorException(Log.CurrentMethod(), "PSdZ could not be initialized: {0}", exception);
            //[-] ServiceLocator.Current.TryGetService<IInteractionService>(out var service);
            //[-] service.RegisterMessage(new FormatedData("#Warning").Localize(), new FormatedData("#FaultJavaInstalation").Localize());
            }
            catch (Exception ex)
            {
                Log.Error(Log.CurrentMethod(), "PSdZ could not be initialized: {0}", ex);
                //[-] throw new AppException(new FormatedData("#PSdZNotStarted"));
                //[+] throw new Exception("PSdZNotStarted");
                throw new Exception("PSdZNotStarted");
            }
        }

        public void Shutdown()
        {
            if (IsPsdzInitialized)
            {
                Log.Info(Log.CurrentMethod(), "Shutting down Psdz Webservice...");
                _psdzWebService.Shutdown();
            }
        }

        public void AddPsdzEventListener(IPsdzEventListener psdzEventListener)
        {
            _psdzWebService.SetPsdzEventListener(psdzEventListener);
        }

        public void AddPsdzProgressListener(IPsdzProgressListener progressListener)
        {
            _psdzWebService.SetPsdzProgressListener(progressListener);
        }

        public void RemovePsdzEventListener(IPsdzEventListener psdzEventListener)
        {
            _psdzWebService.RemovePsdzEventListener(psdzEventListener);
        }

        public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
        {
            _psdzWebService.RemovePsdzProgressListener(progressListener);
        }

        public void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel)
        {
            _psdzLoglevel = psdzLoglevel;
            _prodiasLoglevel = prodiasLoglevel;
            if (IsPsdzInitialized)
            {
                LogService.SetLogLevel(psdzLoglevel);
                ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel);
            }
        }

        [PreserveSource(Hint = "Added")]
        private readonly string _istaFolder;
        [PreserveSource(Hint = "Added")]
        public string PsdzServiceLogDir => _psdzConfig.PsdzWebApiLogDir;

        [PreserveSource(Hint = "Added")]
        public string ExpectedPsdzVersion { get; private set; }
    }
}