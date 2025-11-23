using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Utility;

namespace PsdzClient.Programming
{
	public class ProgrammingService : IProgrammingService, IDisposable
    {
        private readonly PsdzServiceGateway psdzServiceGateway;

        //private readonly IProgrammingWorker programmingWorker;

        private readonly PsdzConfig psdzConfig;

        public IPsdzProgressListener PsdzProgressListener { get; private set; }

        public IPsdzEventListener VehicleProgrammingEventHandler { get; private set; }

        internal ProgrammingEventManager EventManager { get; private set; }

        public EcuProgrammingInfos ProgrammingInfos { get; private set; }

        public PsdzDatabase PsdzDatabase { get; private set; }

        public string BackupDataPath { get; private set; }

        public IPsdz Psdz => psdzServiceGateway.Psdz;

        public ProgrammingService(string istaFolder, string dealerId)
        {
            this.psdzConfig = new PsdzConfig(istaFolder, dealerId);
            psdzServiceGateway = new PsdzServiceGateway(psdzConfig, istaFolder, dealerId);
            SetLogLevelToNormal();

            this.EventManager = new ProgrammingEventManager();
            this.PsdzDatabase = new PsdzDatabase(istaFolder);
            PreparePsdzBackupDataPath(istaFolder);
            //programmingWorker = CreateProgrammingWorker();

            // [UH] added: create services
            IFasta2Service fasta2Service = ServiceLocator.Current.GetService<IFasta2Service>();
            if (fasta2Service == null)
            {
                ServiceLocator.Current.TryAddService((IFasta2Service)new Fasta2Service());
            }

            IDiagnosticsBusinessData diagnosticsBusiness = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            if (diagnosticsBusiness == null)
            {
                ServiceLocator.Current.TryAddService((IDiagnosticsBusinessData)new DiagnosticsBusinessData());
            }
        }

        public bool CollectPsdzLog(string targetLogFilePath)
        {
            if (!Psdz.IsPsdzInitialized)
            {
                return false;
            }
            string text = Psdz.LogService.ClosePsdzLog();
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(targetLogFilePath))
            {
                bool flag = false;
                try
                {
                    File.Move(text, targetLogFilePath);
                    flag = true;
                }
                catch (Exception exception)
                {
                    Log.WarningException("ProgrammingService.CollectPsdzLog", exception);
                }
                if (!flag)
                {
                    File.Copy(text, targetLogFilePath, overwrite: true);
                }
                return true;
            }
            return false;
        }

        [PreserveSource(Hint = "IEnumerable<IProgrammingTask>", Placeholder = true)]
        public PlaceholderType RetrieveAvailableProgrammingTasks(IVehicle vehicle)
        {
            throw new NotImplementedException();
        }

        public void SetLogLevelToMax()
        {
            psdzServiceGateway.SetLogLevel(PsdzLoglevel.TRACE, ProdiasLoglevel.INFO);
        }

        public void SetLogLevelToNormal()
        {
            psdzServiceGateway.SetLogLevel(PsdzLoglevel.FINE, ProdiasLoglevel.ERROR);
        }

        [PreserveSource(Hint = "IProgrammingSessionExt", Placeholder = true)]
        public PlaceholderType Start(PlaceholderType programmingParam)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IProgrammingSessionExt", Placeholder = true)]
        public PlaceholderType Start(PlaceholderType programmingParam, bool avoidTlsConnection)
        {
            throw new NotImplementedException();
        }

#if false
        private void FillAdditionalDataForPretestConfig(ProgrammingSession session, PretestProgrammingPlanParams pretestParams)
        {
            try
            {
                Log.Info(Log.CurrentMethod(), "Gathering the minimum data for token requests...");
                IPsdzIstufe iStufe = Psdz.ObjectBuilder.BuildIstufe(session.Vehicle.ILevelWerk);
                session.PsdzContext.TargetSelectors = Psdz.ConnectionFactoryService.GetTargetSelectors();
                IPsdzConnection psdzConnection = ConnectToBn2020VehicleState.TryGetPsdzConnection(session);
                if (psdzConnection == null)
                {
                    Log.Warning(Log.CurrentMethod(), "Unable to get a PSdZ conneciton here => Retrieving ECU list from PSdZ without PSdZ Connection...");
                    session.PsdzContext.EcuListActual = Psdz.MacrosService.GetInstalledEcuList(pretestParams.Fa, iStufe);
                }
                else
                {
                    session.PsdzProg.ConnectionManager.SwitchFromEDIABASToPSdZIfConnectedViaPTTOrENET(session.PsdzContext);
                    session.PsdzContext.EcuListActual = Psdz.MacrosService.GetInstalledEcuListWithConnection(psdzConnection, pretestParams.Fa, iStufe);
                }
                session.PsdzContext.SetSvtActual(pretestParams.Svt);
                new RetrieveEcuUIDState().Handle(session);
                Log.Info(Log.CurrentMethod(), "Setup specific to pretest programming sessions is concluded (not necessarily to satisfaction).");
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), "Failed to completely set up pretest programming session! Expect things like token request without vehicle test not to work correctly.", exception);
            }
            finally
            {
                session.PsdzProg.ConnectionManager.SwitchFromPSdZToEDIABASIfConnectedViaPTTOrENET(session.PsdzContext);
                session.TryClosePSdZConnection();
            }
        }
#endif

        [PreserveSource(Hint = "FcFnActivationResult", Placeholder = true)]
        public PlaceholderType StoreAndActivateFcFn(IVehicle vehicle, int appNo, int upgradeIdx, byte[] fsc)
        {
            throw new NotImplementedException();
        }

        public void CloseConnectionsToPsdz(bool force = false)
        {
            Log.Info(Log.CurrentMethod(), "Start.");
            psdzServiceGateway.CloseConnectionsToPsdz(force);
            Log.Info(Log.CurrentMethod(), "End.");
        }

        [PreserveSource(Hint = "FcFnActivationResult", Placeholder = true)]
        public PlaceholderType CreatePsdzProg(IVehicle vehicle, IEcuKom ecuKom, IProtocolBasic protocoller)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Added")]
        public string GetPsdzServiceHostLogDir()
        {
            return psdzServiceGateway.PsdzServiceLogDir;
        }

        public string GetPsdzWebServiceLogFilePath()
        {
            return psdzServiceGateway.PsdzWebServiceLogFilePath;
        }

        public string GetPsdzLogFilePath()
        {
            return psdzServiceGateway.PsdzLogFilePath;
        }

        public bool StartPsdzService(IVehicle vehicle = null)
        {
            if (PsdzStarterGuard.Instance.IsInitializationAlreadyAttempted())
            {
                Log.Debug(Log.CurrentMethod(), "There has already been an attempt to open PsdzService in the past. Returning...");
                return true;
            }
            TimeMetricsUtility.Instance.InitializePsdzStart();
            Log.Info(Log.CurrentMethod(), "Start.");
            try
            {
                if (!psdzServiceGateway.StartIfNotRunning(vehicle))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(Log.CurrentMethod(), ex);
                return false;
            }
            Log.Info(Log.CurrentMethod(), "End.");
            TimeMetricsUtility.Instance.InitializePsdzStop();
            return true;
        }

        [PreserveSource(Hint = "Added istaFolder")]
        private void PreparePsdzBackupDataPath(string istaFolder)
		{
            string pathString = PsdzContext.GetBackupBasePath(istaFolder);
            if (string.IsNullOrEmpty(pathString))
            {
                throw new InvalidOperationException("Key 'BMW.Rheingold.Programming.PsdzBackupDataPath' is not set properly but is required for programming!");
            }
			try
            {
				if (!Directory.Exists(pathString))
				{
					Directory.CreateDirectory(pathString);
				}
				string path = Path.Combine(pathString, Guid.NewGuid().ToString());
				File.WriteAllText(path, string.Empty);
				File.Delete(path);
                BackupDataPath = pathString;
            }
			catch (Exception)
			{
                Log.Error("ProgrammingService.PreparePsdzBackupDataPath()", "No write access to the folder \"{0}\".", pathString);
				throw;
			}
		}

        public bool IsPsdzServiceHostInitialized()
        {
            return this.Psdz.IsPsdzInitialized;
        }

        public void CreateEcuProgrammingInfos(IVehicle vehicle, IFFMDynamicResolver ffmResolver = null)
        {
            this.ProgrammingInfos = new EcuProgrammingInfos(vehicle, ffmResolver);
        }

		public void AddListener(PsdzContext psdzContext)
        {
            RemoveListener();
            this.PsdzProgressListener = new PsdzProgressListener(this.EventManager);
            this.Psdz.AddPsdzProgressListener(this.PsdzProgressListener);
            this.VehicleProgrammingEventHandler = new VehicleProgrammingEventHandler(ProgrammingInfos, psdzContext);
            this.Psdz.AddPsdzEventListener(this.VehicleProgrammingEventHandler);
        }

		public void RemoveListener()
        {
            if (PsdzProgressListener != null)
            {
                this.Psdz.RemovePsdzProgressListener(this.PsdzProgressListener);
                this.PsdzProgressListener = null;
            }
            if (VehicleProgrammingEventHandler != null)
            {
                this.Psdz.RemovePsdzEventListener(this.VehicleProgrammingEventHandler);
                this.VehicleProgrammingEventHandler = null;
            }
        }
		
        public void Dispose()
        {
            RemoveListener();
			this.psdzServiceGateway.Dispose();
            if (this.PsdzDatabase != null)
            {
                this.PsdzDatabase.Dispose();
                this.PsdzDatabase = null;
            }
		}
	}
}
