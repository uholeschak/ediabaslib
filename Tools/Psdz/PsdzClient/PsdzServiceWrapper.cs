using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	public class PsdzServiceWrapper : IPsdz, IPsdzService, IPsdzInfo, IDisposable
	{
		public PsdzServiceWrapper(PsdzConfig psdzConfig)
		{
			if (psdzConfig == null)
			{
				throw new ArgumentNullException("psdzConfig");
			}
			this.psdzHostPath = psdzConfig.HostPath;
			this.psdzServiceArgs = psdzConfig.PsdzServiceArgs;
			this.psdzServiceHostLogDir = psdzConfig.PsdzServiceHostLogDir;
			this.psdzServiceClient = new PsdzServiceClient(psdzConfig.ClientLogPath);
			this.ObjectBuilder = new PsdzObjectBuilder(this.psdzServiceClient.ObjectBuilderService);
		}

		public IConfigurationService ConfigurationService
		{
			get
			{
				return this.psdzServiceClient.ConfigurationService;
			}
		}

		public IConnectionFactoryService ConnectionFactoryService
		{
			get
			{
				return this.psdzServiceClient.ConnectionFactoryService;
			}
		}

		public IConnectionManagerService ConnectionManagerService
		{
			get
			{
				return this.psdzServiceClient.ConnectionManagerService;
			}
		}

		public IEcuService EcuService
		{
			get
			{
				return this.psdzServiceClient.EcuService;
			}
		}

		public IEventManagerService EventManagerService
		{
			get
			{
				return this.psdzServiceClient.EventManagerService;
			}
		}

		public string ExpectedPsdzVersion { get; private set; }

		public bool IsPsdzInitialized
		{
			get
			{
				return PsdzServiceStarter.IsServerInstanceRunning();
			}
		}

		public bool IsValidPsdzVersion
		{
			get
			{
				Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Check installed PSdZ-Version ...", Array.Empty<object>());
				if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.IgnorePsdzCheckAtOwnRisk", false))
				{
					Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "PSdZ Version check was diabled at own risk", Array.Empty<object>());
					Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Used PSdZ version:  {0}", new object[]
					{
						this.PsdzVersion
					});
					return true;
				}
				Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Expected: {0}", new object[]
				{
					this.ExpectedPsdzVersion
				});
				Log.Info("PsdzServiceWrapper.IsValidPsdzVersion()", "Current:  {0}", new object[]
				{
					this.PsdzVersion
				});
				if (!string.Equals(this.ExpectedPsdzVersion, this.PsdzVersion, StringComparison.Ordinal))
				{
					Log.Error("PsdzServiceWrapper.IsValidPsdzVersion()", "Installed PSdZ (ver. {0}) is invalid! You have to use PSdZ ver. {1}!", new object[]
					{
						this.PsdzVersion,
						this.ExpectedPsdzVersion
					});
					return false;
				}
				return true;
			}
		}

		public ILogService LogService
		{
			get
			{
				return this.psdzServiceClient.LogService;
			}
		}

		public ILogicService LogicService
		{
			get
			{
				return this.psdzServiceClient.LogicService;
			}
		}

		public IMacrosService MacrosService
		{
			get
			{
				return this.psdzServiceClient.MacrosService;
			}
		}

		public IPsdzObjectBuilder ObjectBuilder { get; private set; }

		public IObjectBuilderService ObjectBuilderService
		{
			get
			{
				return this.psdzServiceClient.ObjectBuilderService;
			}
		}

		public BMW.Rheingold.Psdz.IProgrammingService ProgrammingService
		{
			get
			{
				return this.psdzServiceClient.ProgrammingService;
			}
		}

		public string PsdzDataPath
		{
			get
			{
				return this.psdzServiceClient.ConfigurationService.GetRootDirectory();
			}
		}

		public string PsdzVersion { get; private set; }

		public ITalExecutionService TalExecutionService
		{
			get
			{
				return this.psdzServiceClient.TalExecutionService;
			}
		}

		public IVcmService VcmService
		{
			get
			{
				return this.psdzServiceClient.VcmService;
			}
		}

		public ICbbTlsConfiguratorService CbbTlsConfiguratorService
		{
			get
			{
				return this.psdzServiceClient.CbbTlsConfiguratorService;
			}
		}

		public ICertificateManagementService CertificateManagementService
		{
			get
			{
				return this.psdzServiceClient.CertificateManagementService;
			}
		}

		public IIndividualDataRestoreService IndividualDataRestoreService
		{
			get
			{
				return this.psdzServiceClient.IndividualDataRestoreService;
			}
		}

		public ISecureFeatureActivationService SecureFeatureActivationService
		{
			get
			{
				return this.psdzServiceClient.SecureFeatureActivationService;
			}
		}

		public ISecurityManagementService SecurityManagementService
		{
			get
			{
				return this.psdzServiceClient.SecurityManagementService;
			}
		}

		public ISecureCodingService SecureCodingService
		{
			get
			{
				return this.psdzServiceClient.SecureCodingService;
			}
		}

		public IKdsService KdsService
		{
			get
			{
				return this.psdzServiceClient.KdsService;
			}
		}

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
			this.psdzLoglevel = new PsdzLoglevel?(psdzLoglevel);
			this.prodiasLoglevel = new ProdiasLoglevel?(prodiasLoglevel);
			if (this.IsPsdzInitialized)
			{
				this.psdzServiceClient.LogService.SetLogLevel(psdzLoglevel);
				this.psdzServiceClient.ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel);
			}
		}

		public void StartHostIfNotRunning(IVehicle vehicle = null)
		{
			try
			{
				if (this.IsPsdzInitialized)
				{
					this.DoSettingsForInitializedPsdz();
				}
				else
				{
					if (this.psdzServiceArgs.IsTestRun)
					{
						this.psdzServiceArgs.TestRunParams = this.BuildTestRunParams(vehicle);
					}
					Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "Initialize PSdZ ...", Array.Empty<object>());
					PsdzServiceStartResult psdzServiceStartResult = new PsdzServiceStarter(this.psdzHostPath, this.psdzServiceHostLogDir, this.psdzServiceArgs).StartIfNotRunning();
					Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "Result: {0}", new object[]
					{
						psdzServiceStartResult
					});
					switch (psdzServiceStartResult)
					{
						case PsdzServiceStartResult.PsdzStillRunning:
						case PsdzServiceStartResult.PsdzStartOk:
							this.DoSettingsForInitializedPsdz();
							Log.Info("PsdzServiceWrapper.StartHostIfNotRunning()", "PSdZ initialized! Has valid version: {0}", new object[]
							{
							this.IsValidPsdzVersion
							});
							break;
						default:
							throw new AppException(new FormatedData("#PSdZNotStarted", Array.Empty<object>()));
						case PsdzServiceStartResult.PsdzStartFailedMemError:
							throw new AppException(new FormatedData("#PSdZNotStartedMemError", Array.Empty<object>()));
					}
				}
			}
			catch (AppException ex)
			{
				Log.Error("PsdzServiceWrapper.StartHostIfNotRunning()", "PSdZ could not be initialized: {0}", new object[]
				{
					ex
				});
				throw;
			}
			catch (Exception ex2)
			{
				Log.Error("PsdzServiceWrapper.StartHostIfNotRunning()", "PSdZ could not be initialized: {0}", new object[]
				{
					ex2
				});
				throw new AppException(new FormatedData("#PSdZNotStarted", Array.Empty<object>()));
			}
		}

		public void DoInitSettings()
		{
			if (this.IsPsdzInitialized && this.PsdzVersion == null && this.ExpectedPsdzVersion == null)
			{
				this.DoSettingsForInitializedPsdz();
			}
		}

		public void Shutdown()
		{
			try
			{
				if (this.IsPsdzInitialized)
				{
					this.CloseConnectionsToPsdzHost();
					Log.Info("PsdzServiceWrapper.Shutdown()", "PSdZ host: Closed connections.", Array.Empty<object>());
					this.ConnectionManagerService.RequestShutdown();
					Log.Info("PsdzServiceWrapper.Shutdown()", "PSdZ host: Shutdown requested.", Array.Empty<object>());
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("PsdzServiceWrapper.Shutdown()", exception);
			}
		}

		private void DoSettingsForInitializedPsdz()
		{
			int configint = ConfigSettings.getConfigint("DebugLevel", 0);
			if (configint > 0 && configint < 6)
			{
				this.psdzLoglevel = new PsdzLoglevel?((PsdzLoglevel)configint);
			}
			if (this.psdzLoglevel != null && !this.psdzServiceArgs.IsTestRun)
			{
				this.psdzServiceClient.LogService.SetLogLevel(this.psdzLoglevel.Value);
			}
			ProdiasLoglevel value;
			if (Enum.TryParse<ProdiasLoglevel>(ConfigSettings.getConfigString("BMW.Rheingold.Programming.Prodias.LogLevel", null), out value))
			{
				this.prodiasLoglevel = new ProdiasLoglevel?(value);
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
				StandardFa = this.ObjectBuilder.BuildFaActualFromVehicleContext(vehicle),
				SvtCurrent = this.ObjectBuilder.BuildStandardSvtActualFromVehicleContext(vehicle, null),
				IstufeCurrent = this.ObjectBuilder.BuildIstufe(vehicle.ILevel),
				DurationTalLineExecution = 1000,
				InitNoGeneratedTal = 0,
				IncNoGeneratedTal = 1
			};
		}

		private readonly PsdzServiceArgs psdzServiceArgs;

		private readonly PsdzServiceClient psdzServiceClient;

		private ProdiasLoglevel? prodiasLoglevel = new ProdiasLoglevel?(ProdiasLoglevel.ERROR);

		private PsdzLoglevel? psdzLoglevel = new PsdzLoglevel?(PsdzLoglevel.FINE);

		private readonly string psdzHostPath;

		private readonly string psdzServiceHostLogDir;
	}
}
