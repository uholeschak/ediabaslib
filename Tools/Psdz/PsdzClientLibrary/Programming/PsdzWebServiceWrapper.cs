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
        private IPsdzObjectBuilder _objectBuilder;
        private IPsdzWebService _psdzWebservice;
        public IPsdzWebService PsdzWebservice
        {
            get
            {
                if (_psdzWebservice == null)
                {
                    return ServiceLocator.Current.GetService<IPsdzWebService>();
                }

                return _psdzWebservice;
            }

            set
            {
                _psdzWebservice = value;
            }
        }

        public IBaureiheUtilityService BaureiheUtilityService => PsdzWebservice.BaureiheUtilityService;
        public ICertificateManagementService CertificateManagementService => PsdzWebservice.CertificateManagementService;
        public IConfigurationService ConfigurationService => PsdzWebservice.ConfigurationService;
        public IConnectionFactoryService ConnectionFactoryService => PsdzWebservice.ConnectionFactoryService;
        public IConnectionManagerService ConnectionManagerService => PsdzWebservice.ConnectionManagerService;
        public IEcuService EcuService => PsdzWebservice.EcuService;
        public IEventManagerService EventManagerService => PsdzWebservice.EventManagerService;
        public IHttpConfigurationService HttpConfigurationService => PsdzWebservice.HttpConfigurationService;
        public IHttpServerService HttpServerService => PsdzWebservice.HttpServerService;
        public IIndividualDataRestoreService IndividualDataRestoreService => PsdzWebservice.IndividualDataRestoreService;
        public IKdsService KdsService => PsdzWebservice.KdsService;
        public ILogicService LogicService => PsdzWebservice.LogicService;
        public ILogService LogService => PsdzWebservice.LogService;
        public IMacrosService MacrosService => PsdzWebservice.MacrosService;
        public IObjectBuilderService ObjectBuilderService => PsdzWebservice.ObjectBuilderService;
        public IProgrammingService ProgrammingService => PsdzWebservice.ProgrammingService;
        public ISecureCodingService SecureCodingService => PsdzWebservice.SecureCodingService;
        public ISecureFeatureActivationService SecureFeatureActivationService => PsdzWebservice.SecureFeatureActivationService;
        public ISecurityManagementService SecurityManagementService => PsdzWebservice.SecurityManagementService;
        public ITalExecutionService TalExecutionService => PsdzWebservice.TalExecutionService;
        public IVcmService VcmService => PsdzWebservice.VcmService;
        public ISecureDiagnosticsService SecureDiagnosticsService => PsdzWebservice.SecureDiagnosticsService;
        public IProgrammingTokenService ProgrammingTokenService => PsdzWebservice.ProgrammingTokenService;
        public IFpService FpService => PsdzWebservice.FpService;
        public bool IsValidPsdzVersion => true;
        public string PsdzDataPath => PsdzWebservice.ConfigurationService.GetRootDirectory();
        public string PsdzVersion => PsdzWebservice.ConfigurationService.GetPsdzVersion();

        public IPsdzObjectBuilder ObjectBuilder
        {
            get
            {
                if (_objectBuilder == null)
                {
                    //[-] _objectBuilder = new PsdzObjectBuilder(ObjectBuilderService);
                    //[+] _objectBuilder = new PsdzObjectBuilder(ObjectBuilderService, this);
                    _objectBuilder = new PsdzObjectBuilder(ObjectBuilderService, this);
                }

                return _objectBuilder;
            }
        }

        public bool IsPsdzInitialized
        {
            get
            {
                if (PsdzWebservice == null)
                {
                    return false;
                }

                if (!PsdzStarterGuard.Instance.CanCheckAvailability())
                {
                    return false;
                }

                return PsdzWebservice.IsReady();
            }
        }

        public string PsdzServiceLogFilePath => _psdzConfig.PsdzWebServiceLogFilePath;
        public string PsdzLogFilePath => _psdzConfig.PsdzLogFilePath;
        public string ProdiasDriverLogFilePath => _psdzConfig.ProdiasDriverLogFilePath;

        [PreserveSource(Hint = "istaFolder, dealerId added", SignatureModified = true)]
        public PsdzWebServiceWrapper(PsdzWebServiceConfig psdzConfig, string istaFolder, string dealerId)
        {
            //[-]_psdzConfig = psdzConfig ?? new PsdzWebServiceConfig();
            //[-]PsdzWebservice = psdzWebService;
            //[+]_psdzConfig = psdzConfig ?? new PsdzWebServiceConfig(istaFolder);
            _psdzConfig = psdzConfig ?? new PsdzWebServiceConfig(istaFolder, dealerId);
            //[+] _istaFolder = istaFolder;
            _istaFolder = istaFolder;
            //[+]PsdzWebservice = new PsdzWebService(_psdzConfig.PsdzWebApiLogDir, () => PsdzStarterGuard.Instance.CanCheckAvailability(), _istaFolder);
            PsdzWebservice = new PsdzWebService(_psdzConfig.PsdzWebApiLogDir, () => PsdzStarterGuard.Instance.CanCheckAvailability(), _istaFolder);
        }

        public void StartIfNotRunning()
        {
            try
            {
                PsdzWebservice.StartIfNotRunning(_psdzConfig.GetJrePath(), _psdzConfig.GetJvmOptionsAsOneString(), _psdzConfig.GetJarArgumentsAsOneString());
            }
            catch (PsdzWebserviceStartException exception)
            {
                Log.ErrorException(Log.CurrentMethod(), "PSdZ could not be initialized: {0}", exception);
                DisplayJavaFaultyInstallationPopUp();
            }
            catch (Exception ex)
            {
                Log.Error(Log.CurrentMethod(), "PSdZ could not be initialized: {0}", ex);
                //[-] throw new AppException(new FormatedData("#PSdZNotStarted"));
                //[+] throw new Exception("PSdZNotStarted");
                throw new Exception("PSdZNotStarted");
            }
        }

        private static void DisplayJavaFaultyInstallationPopUp()
        {
            ServiceLocator.Current.TryGetService<IInteractionService>(out var service);
            service.RegisterMessage(new FormatedData("#Warning").Localize(), new FormatedData("#FaultJavaInstalation").Localize());
        }

        public void Shutdown()
        {
            if (IsPsdzInitialized)
            {
                Log.Info(Log.CurrentMethod(), "Shutting down Psdz Webservice...");
                PsdzWebservice.Shutdown();
            }
        }

        public void AddPsdzEventListener(IPsdzEventListener psdzEventListener)
        {
            PsdzWebservice.SetPsdzEventListener(psdzEventListener);
        }

        public void AddPsdzProgressListener(IPsdzProgressListener progressListener)
        {
            PsdzWebservice.SetPsdzProgressListener(progressListener);
        }

        public void RemovePsdzEventListener(IPsdzEventListener psdzEventListener)
        {
            PsdzWebservice.RemovePsdzEventListener(psdzEventListener);
        }

        public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
        {
            PsdzWebservice.RemovePsdzProgressListener(progressListener);
        }

        public void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel)
        {
            if (IsPsdzInitialized)
            {
                LogService.SetLogLevel(psdzLoglevel);
                ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel);
            }
        }

        [PreserveSource(Added = true)]
        private readonly string _istaFolder;
        [PreserveSource(Added = true)]
        public string PsdzServiceLogDir => _psdzConfig.PsdzWebApiLogDir;

        [PreserveSource(Added = true)]
        public string ExpectedPsdzVersion { get; private set; }
    }
}