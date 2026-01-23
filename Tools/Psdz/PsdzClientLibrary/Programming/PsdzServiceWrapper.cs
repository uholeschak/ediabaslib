using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Diagnostics;
using PsdzClient;

namespace BMW.Rheingold.Programming
{
    [PreserveSource(Removed = true)]
    public class PsdzServiceWrapper : IPsdz, IPsdzService, IPsdzInfo, IDisposable
    {
        private readonly PsdzServiceArgs psdzServiceArgs;

        private readonly PsdzServiceClient psdzServiceClient;

        private ProdiasLoglevel? prodiasLoglevel = ProdiasLoglevel.ERROR;

        private PsdzLoglevel? psdzLoglevel = PsdzLoglevel.FINE;

        private readonly string psdzHostPath;

        private readonly string psdzServiceHostLogDir;

        private readonly string psdzServiceHostLogFilePath;

        private readonly string psdzLogFilePath;


        [PreserveSource(Hint = "Dummy")]
        public IBaureiheUtilityService BaureiheUtilityService => psdzServiceClient.BaureiheUtilityService;

        public IConfigurationService ConfigurationService => psdzServiceClient.ConfigurationService;

        public IConnectionFactoryService ConnectionFactoryService => psdzServiceClient.ConnectionFactoryService;

        public IConnectionManagerService ConnectionManagerService => psdzServiceClient.ConnectionManagerService;

        public IEcuService EcuService => psdzServiceClient.EcuService;

        public IEventManagerService EventManagerService => psdzServiceClient.EventManagerService;

        public ISecureDiagnosticsService SecureDiagnosticsService => psdzServiceClient.SecureDiagnosticsService;

        public string ExpectedPsdzVersion { get; private set; }

        [PreserveSource(Hint = "PsdzObjectBuilder modified")]
        public PsdzServiceWrapper(PsdzConfig psdzConfig)
        {
            if (psdzConfig == null)
            {
                throw new ArgumentNullException("psdzConfig");
            }
            psdzHostPath = psdzConfig.HostPath;
            psdzServiceArgs = psdzConfig.PsdzServiceArgs;
            psdzServiceHostLogDir = psdzConfig.PsdzServiceHostLogDir;
            psdzServiceHostLogFilePath = psdzConfig.PsdzServiceHostLogFilePath;
            psdzLogFilePath = psdzConfig.PsdzLogFilePath;
            if (ConfigSettings.GetActivateSdpOnlinePatch())
            {
                psdzServiceClient = new PsdzServiceClient(psdzConfig.ClientLogPath, Process.GetCurrentProcess().Id);
            }
            else
            {
                psdzServiceClient = new PsdzServiceClient(psdzConfig.ClientLogPath);
            }
            ObjectBuilder = new PsdzObjectBuilder(psdzServiceClient.ObjectBuilderService, this);
        }

        public bool IsPsdzInitialized
        {
            get
            {
                if (PsdzStarterGuard.Instance.CanCheckAvailability())
                {
                    if (!ConfigSettings.GetActivateSdpOnlinePatch())
                    {
                        return PsdzServiceStarter.IsServerInstanceRunning();
                    }
                    return PsdzServiceStarter.IsServerInstanceRunning(Process.GetCurrentProcess().Id);
                }
                return false;
            }
        }

        public bool IsValidPsdzVersion
        {
            get
            {
                Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Check installed PSdZ-Version ...");
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.IgnorePsdzCheckAtOwnRisk", defaultValue: false))
                {
                    Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "PSdZ Version check was diabled at own risk");
                    Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Used PSdZ version:  {0}", PsdzVersion);
                    return true;
                }
                Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Expected: {0}", ExpectedPsdzVersion);
                Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Current:  {0}", PsdzVersion);
                if (!string.Equals(ExpectedPsdzVersion, PsdzVersion, StringComparison.Ordinal))
                {
                    Log.Error("PsdzServiceWrapper.IsValidPsdzVersion()", "Installed PSdZ (ver. {0}) is invalid! You have to use PSdZ ver. {1}!", PsdzVersion, ExpectedPsdzVersion);
                    return false;
                }
                return true;
            }
        }

        public ILogService LogService => psdzServiceClient.LogService;

        public ILogicService LogicService => psdzServiceClient.LogicService;

        public IMacrosService MacrosService => psdzServiceClient.MacrosService;

        public IPsdzObjectBuilder ObjectBuilder { get; private set; }

        [PreserveSource(Hint = "For backward compatibility")]
        public IProgrammingTokenService ProgrammingTokenService { get; }

        public IObjectBuilderService ObjectBuilderService => psdzServiceClient.ObjectBuilderService;

        public BMW.Rheingold.Psdz.IProgrammingService ProgrammingService => psdzServiceClient.ProgrammingService;

        public string PsdzDataPath => psdzServiceClient.ConfigurationService.GetRootDirectory();

        public string PsdzVersion { get; private set; }

        public ITalExecutionService TalExecutionService => psdzServiceClient.TalExecutionService;

        public IVcmService VcmService => psdzServiceClient.VcmService;

        public ICertificateManagementService CertificateManagementService => psdzServiceClient.CertificateManagementService;

        public IIndividualDataRestoreService IndividualDataRestoreService => psdzServiceClient.IndividualDataRestoreService;

        public ISecureFeatureActivationService SecureFeatureActivationService => psdzServiceClient.SecureFeatureActivationService;

        public ISecurityManagementService SecurityManagementService => psdzServiceClient.SecurityManagementService;

        public ISecureCodingService SecureCodingService => psdzServiceClient.SecureCodingService;

        public IKdsService KdsService => psdzServiceClient.KdsService;

        public IHttpConfigurationService HttpConfigurationService => psdzServiceClient.HttpConfigurationService;

        [PreserveSource(Hint = "Added")]
        public string PsdzServiceLogDir => psdzServiceHostLogDir;

        public string PsdzServiceLogFilePath => psdzServiceHostLogFilePath;

        public string PsdzLogFilePath => psdzLogFilePath;


        public void AddPsdzEventListener(IPsdzEventListener psdzEventListener)
		{
			this.psdzServiceClient.AddPsdzEventListener(psdzEventListener);
		}

		public void AddPsdzProgressListener(IPsdzProgressListener progressListener)
		{
			this.psdzServiceClient.AddPsdzProgressListener(progressListener);
		}

		public void CloseConnectionsToPsdzHost()
		{
			if (this.IsPsdzInitialized)
			{
				this.psdzServiceClient.CloseAllConnections();
			}
		}

		public void Dispose()
		{
			if (this.IsPsdzInitialized)
			{
				this.psdzServiceClient.Dispose();
			}
		}

		public void RemovePsdzEventListener(IPsdzEventListener psdzEventListener)
		{
			this.psdzServiceClient.RemovePsdzEventListener(psdzEventListener);
		}

		public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
		{
			this.psdzServiceClient.RemovePsdzProgressListener(progressListener);
		}

        public void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel)
        {
            this.psdzLoglevel = psdzLoglevel;
            this.prodiasLoglevel = prodiasLoglevel;
            if (IsPsdzInitialized)
            {
                psdzServiceClient.LogService.SetLogLevel(psdzLoglevel);
                psdzServiceClient.ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel);
            }
        }

        [PreserveSource(Hint = "Modified")]
        public bool StartHostIfNotRunning(IVehicle vehicle = null)
        {
            try
            {
                if (IsPsdzInitialized)
                {
                    DoSettingsForInitializedPsdz();
                    return true;
                }
                if (psdzServiceArgs.IsTestRun)
                {
                    psdzServiceArgs.TestRunParams = BuildTestRunParams(vehicle);
                }
                Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "Initialize PSdZ ...");
                PsdzServiceStarter psdzServiceStarter = new PsdzServiceStarter(psdzHostPath, psdzServiceHostLogDir, psdzServiceArgs);
                PsdzServiceStartResult psdzServiceStartResult = (ConfigSettings.GetActivateSdpOnlinePatch() ? psdzServiceStarter.StartIfNotRunning(Process.GetCurrentProcess().Id) : psdzServiceStarter.StartIfNotRunning());
                Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "Result: {0}", psdzServiceStartResult);
                switch (psdzServiceStartResult)
                {
                    case PsdzServiceStartResult.PsdzStillRunning:
                    case PsdzServiceStartResult.PsdzStartOk:
                        DoSettingsForInitializedPsdz();
                        Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "PSdZ initialized! Has valid version: {0}", IsValidPsdzVersion);
                        break;
                    default:
                        return false;
                    case PsdzServiceStartResult.PsdzStartFailedMemError:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void DoInitSettings()
		{
            if (IsPsdzInitialized && PsdzVersion == null && ExpectedPsdzVersion == null)
            {
                DoSettingsForInitializedPsdz();
            }
		}

        public bool Shutdown()
		{
            try
            {
                if (IsPsdzInitialized)
                {
                    CloseConnectionsToPsdzHost();
                    Log.Info("PsdzServiceWrapper.Shutdown()", "PSdZ host: Closed connections.");
                    ConnectionManagerService.RequestShutdown();
                    Log.Info("PsdzServiceWrapper.Shutdown()", "PSdZ host: Shutdown requested.");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("PsdzServiceWrapper.Shutdown()", exception);
                return false;
            }

            return true;
        }

        private void DoSettingsForInitializedPsdz()
        {
            int configint = ConfigSettings.getConfigint("DebugLevel", 0);
            if (configint > 0 && configint < 6)
            {
                //[-] psdzLoglevel = (PsdzLoglevel)configint;
            }

            if (psdzLoglevel.HasValue && !psdzServiceArgs.IsTestRun)
            {
                psdzServiceClient.LogService.SetLogLevel(psdzLoglevel.Value);
            }

            if (Enum.TryParse<ProdiasLoglevel>(ConfigSettings.getConfigString("BMW.Rheingold.Programming.Prodias.LogLevel", null), out var result))
            {
                //[-] prodiasLoglevel = result;
            }

            if (prodiasLoglevel.HasValue)
            {
                psdzServiceClient.ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel.Value);
            }
            ExpectedPsdzVersion = psdzServiceClient.ConfigurationService.GetExpectedPsdzVersion();
            PsdzVersion = psdzServiceClient.ConfigurationService.GetPsdzVersion();
        }

        private TestRunParams BuildTestRunParams(IVehicle vehicle)
        {
            return new TestRunParams
            {
                StandardFa = ObjectBuilder.BuildFaActualFromVehicleContext(vehicle),
                SvtCurrent = ObjectBuilder.BuildStandardSvtActualFromVehicleContext(vehicle),
                IstufeCurrent = ObjectBuilder.BuildIstufe(vehicle.ILevel),
                DurationTalLineExecution = 1000,
                InitNoGeneratedTal = 0,
                IncNoGeneratedTal = 1
            };
        }
    }
}
