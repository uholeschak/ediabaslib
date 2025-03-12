using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
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

        public IConfigurationService ConfigurationService => psdzServiceClient.ConfigurationService;

        public IConnectionFactoryService ConnectionFactoryService => psdzServiceClient.ConnectionFactoryService;

        public IConnectionManagerService ConnectionManagerService => psdzServiceClient.ConnectionManagerService;

        public IEcuService EcuService => psdzServiceClient.EcuService;

        public IEventManagerService EventManagerService => psdzServiceClient.EventManagerService;

        public ISecureDiagnosticsService SecureDiagnosticsService => psdzServiceClient.SecureDiagnosticsService;

        public string ExpectedPsdzVersion { get; private set; }


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
            if (ClientContext.EnablePsdzMultiSession())
            {
                psdzServiceClient = new PsdzServiceClient(psdzConfig.ClientLogPath, Process.GetCurrentProcess().Id);
            }
            else
            {
                psdzServiceClient = new PsdzServiceClient(psdzConfig.ClientLogPath);
            }
            ObjectBuilder = new PsdzObjectBuilder(psdzServiceClient.ObjectBuilderService);
        }

        public bool IsPsdzInitialized
        {
            get
            {
                return PsdzServiceStarter.IsThisServerInstanceRunning();
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

        public IObjectBuilderService ObjectBuilderService => psdzServiceClient.ObjectBuilderService;

        public BMW.Rheingold.Psdz.IProgrammingService ProgrammingService => psdzServiceClient.ProgrammingService;

        public string PsdzDataPath => psdzServiceClient.ConfigurationService.GetRootDirectory();

        public string PsdzVersion { get; private set; }

        public ITalExecutionService TalExecutionService => psdzServiceClient.TalExecutionService;

        public IVcmService VcmService => psdzServiceClient.VcmService;

        public ICbbTlsConfiguratorService CbbTlsConfiguratorService => psdzServiceClient.CbbTlsConfiguratorService;

        public ICertificateManagementService CertificateManagementService => psdzServiceClient.CertificateManagementService;

        public IIndividualDataRestoreService IndividualDataRestoreService => psdzServiceClient.IndividualDataRestoreService;

        public ISecureFeatureActivationService SecureFeatureActivationService => psdzServiceClient.SecureFeatureActivationService;

        public ISecurityManagementService SecurityManagementService => psdzServiceClient.SecurityManagementService;

        public ISecureCodingService SecureCodingService => psdzServiceClient.SecureCodingService;

        public IKdsService KdsService => psdzServiceClient.KdsService;

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
                PsdzServiceStarter.PsdzServiceStartResult psdzServiceStartResult = !ClientContext.EnablePsdzMultiSession() ? psdzServiceStarter.StartIfNotRunning() : psdzServiceStarter.StartIfNotRunning(Process.GetCurrentProcess().Id);
                Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "Result: {0}", psdzServiceStartResult);
                switch (psdzServiceStartResult)
                {
                    case PsdzServiceStarter.PsdzServiceStartResult.PsdzStillRunning:
                    case PsdzServiceStarter.PsdzServiceStartResult.PsdzStartOk:
                        DoSettingsForInitializedPsdz();
                        Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "PSdZ initialized! Has valid version: {0}", IsValidPsdzVersion);
                        break;
                    default:
                        return false;
                    case PsdzServiceStarter.PsdzServiceStartResult.PsdzStartFailedMemError:
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
			if (this.IsPsdzInitialized && this.PsdzVersion == null && this.ExpectedPsdzVersion == null)
			{
				this.DoSettingsForInitializedPsdz();
			}
		}

		public bool Shutdown()
		{
			try
			{
				if (this.IsPsdzInitialized)
				{
					this.CloseConnectionsToPsdzHost();
					this.ConnectionManagerService.RequestShutdown();
				}
			}
			catch (Exception)
            {
                return false;
            }

            return true;
        }

		private void DoSettingsForInitializedPsdz()
        {
			if (this.psdzLoglevel != null && !this.psdzServiceArgs.IsTestRun)
			{
				this.psdzServiceClient.LogService.SetLogLevel(this.psdzLoglevel.Value);
			}

            if (this.prodiasLoglevel != null)
			{
				this.psdzServiceClient.ConnectionManagerService.SetProdiasLogLevel(this.prodiasLoglevel.Value);
			}
			this.ExpectedPsdzVersion = this.psdzServiceClient.ConfigurationService.GetExpectedPsdzVersion();
			this.PsdzVersion = this.psdzServiceClient.ConfigurationService.GetPsdzVersion();
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
