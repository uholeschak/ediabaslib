using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
	public class ProgrammingService : IDisposable
	{
        public ProgrammingService(string istaFolder, string dealerId, bool multiSession = false)
        {
            this.PsdzLoglevel = PsdzLoglevel.FINE;
            this.ProdiasLoglevel = ProdiasLoglevel.ERROR;
            this.psdzConfig = new PsdzConfig(istaFolder, dealerId);
            this.psdz = new PsdzServiceWrapper(this.psdzConfig, multiSession);
            this.psdz.SetLogLevel(PsdzLoglevel, ProdiasLoglevel);

            this.EventManager = new ProgrammingEventManager();
            this.PdszDatabase = new PdszDatabase(istaFolder);
            PreparePsdzBackupDataPath(istaFolder);
        }

        public bool CollectPsdzLog(string targetLogFilePath)
		{
			if (!this.psdz.IsPsdzInitialized)
			{
				return false;
			}
			string text = this.psdz.LogService.ClosePsdzLog();
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(targetLogFilePath))
			{
				bool flag = false;
				try
				{
					File.Move(text, targetLogFilePath);
					flag = true;
				}
				catch (Exception)
				{
					//Log.WarningException("ProgrammingService.CollectPsdzLog", exception);
				}
				if (!flag)
				{
					File.Copy(text, targetLogFilePath, true);
				}
				return true;
			}
			return false;
		}

		public void SetLogLevelToMax()
		{
			this.psdz.SetLogLevel(PsdzLoglevel.TRACE, ProdiasLoglevel.INFO);
		}

		public void SetLogLevelToNormal()
		{
			this.psdz.SetLogLevel(PsdzLoglevel.FINE, ProdiasLoglevel.ERROR);
		}
#if false
		public IProgrammingSessionExt Start(ProgrammingParam programmingParam)
		{
			this.StartPsdzServiceHost(programmingParam.Vehicle);
			ProgrammingSession programmingSession = new ProgrammingSession(this.psdz, programmingParam, this.programmingWorker);
			IFscValidationService fscValidationService = this.InitializeFscValidationService(programmingParam.FscValidationConfig);
			programmingSession.FscValidationService = fscValidationService;
			programmingSession.Start();
			return programmingSession;
		}

		public FcFnActivationResult StoreAndActivateFcFn(IVehicle vehicle, int appNo, int upgradeIdx, byte[] fsc, IEcuKom ecuKom)
		{
			FscService fscService = new FscService();
			this.StartPsdzServiceHost(vehicle);
			return fscService.StoreAndActivateFcFn(this.psdz, vehicle, appNo, upgradeIdx, fsc, ecuKom, this.programmingWorker);
		}
#endif
		public void CloseConnectionsToPsdzHost()
		{
			try
			{
				this.psdz.CloseConnectionsToPsdzHost();
			}
			catch (Exception)
			{
				//Log.WarningException("ProgrammingService.CloseConnectionsToPsdzHost()", exception);
			}
		}

        public string GetPsdzServiceHostLogDir()
        {
            return this.psdzConfig.PsdzServiceHostLogDir;
        }

		public string GetPsdzServiceHostLogFilePath()
		{
			return this.psdzConfig.PsdzServiceHostLogFilePath;
		}

		public string GetPsdzLogFilePath()
		{
			return this.psdzConfig.PsdzLogFilePath;
		}

		private void PreparePsdzBackupDataPath(string istaFolder)
		{
			string pathString = Path.Combine(istaFolder, "Temp");
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
				throw;
			}
		}

		public bool StartPsdzServiceHost(IVehicle vehicle = null)
		{
            this.psdz.StartHostIfNotRunning(vehicle);
            if (!this.WaitForPsdzServiceHostInitialization())
            {
                return false;
            }

            return true;
        }

        public bool WaitForPsdzServiceHostInitialization()
		{
			DateTime t = DateTime.Now.AddSeconds((double)40f);
			while (!this.psdz.IsPsdzInitialized)
			{
				if (DateTime.Now > t)
				{
					return false;
				}
				Thread.Sleep(500);
			}
			this.psdz.DoInitSettings();

            return true;
        }

        public bool IsPsdzPsdzServiceHostInitialized()
        {
            return this.psdz.IsPsdzInitialized;
        }

        public void CreateEcuProgrammingInfos(IVehicle vehicle, IFFMDynamicResolver ffmResolver = null)
        {
            this.ProgrammingInfos = new EcuProgrammingInfos(vehicle, ffmResolver);
        }

		public void AddListener(PsdzContext psdzContext)
        {
            RemoveListener();
            this.PsdzProgressListener = new PsdzProgressListener(this.EventManager);
            this.psdz.AddPsdzProgressListener(this.PsdzProgressListener);
            this.VehicleProgrammingEventHandler = new VehicleProgrammingEventHandler(ProgrammingInfos, psdzContext);
            this.psdz.AddPsdzEventListener(this.VehicleProgrammingEventHandler);
        }

		public void RemoveListener()
        {
            if (PsdzProgressListener != null)
            {
                this.psdz.RemovePsdzProgressListener(this.PsdzProgressListener);
                this.PsdzProgressListener = null;
            }
            if (VehicleProgrammingEventHandler != null)
            {
                this.psdz.RemovePsdzEventListener(this.VehicleProgrammingEventHandler);
                this.VehicleProgrammingEventHandler = null;
            }
        }
		
        public void Dispose()
        {
            RemoveListener();
			this.psdz.Dispose();
            if (this.PdszDatabase != null)
            {
                this.PdszDatabase.Dispose();
                this.PdszDatabase = null;
            }
		}

        public IPsdzProgressListener PsdzProgressListener { get; private set; }

		public IPsdzEventListener VehicleProgrammingEventHandler { get; private set; }
        
        public ProgrammingEventManager EventManager { get; private set; }

        public EcuProgrammingInfos ProgrammingInfos { get; private set; }

        public PdszDatabase PdszDatabase { get; private set; }

        public PsdzServiceWrapper Psdz => psdz;

        private readonly PsdzConfig psdzConfig;

		private readonly PsdzServiceWrapper psdz;

		public PsdzLoglevel PsdzLoglevel { get; set; }

        public ProdiasLoglevel ProdiasLoglevel { get; set; }

        public string BackupDataPath { get; private set; }

	}
}
