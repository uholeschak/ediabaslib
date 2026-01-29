using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Utility;
using System;
using System.IO;

#pragma warning disable CS0169
namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "ProgrammingService renamed", InheritanceModified = true)]
    public class ProgrammingService2 : IProgrammingService2, IDisposable
    {
        [PreserveSource(Added = true)]
        private readonly PsdzServiceGateway psdzServiceGateway;
        [PreserveSource(Hint = "IProgrammingWorker", Placeholder = true)]
        private readonly PlaceholderType programmingWorker;
        private readonly IOperationServices services;
        public IPsdz Psdz => psdzServiceGateway.Psdz;

        [PreserveSource(Hint = "Modified, create services", SignatureModified = true)]
        public ProgrammingService2(string istaFolder, string dealerId)
        {
            //[-] this.services = services;
            //[-] psdzServiceGateway = new PsdzServiceGateway();
            //[-] PreparePsdzBackupDataPath();
            //[-] programmingWorker = CreateProgrammingWorker();
            //[+] psdzConfig = new PsdzConfig(istaFolder, dealerId);
            psdzConfig = new PsdzConfig(istaFolder, dealerId);
            //[+] psdzServiceGateway = new PsdzServiceGateway(psdzConfig, istaFolder, dealerId);
            psdzServiceGateway = new PsdzServiceGateway(psdzConfig, istaFolder, dealerId);
            //[+] SetLogLevelToNormal();
            SetLogLevelToNormal();
            //[+] EventManager = new ProgrammingEventManager();
            EventManager = new ProgrammingEventManager();
            //[+] PsdzDatabase = new PsdzDatabase(istaFolder);
            PsdzDatabase = new PsdzDatabase(istaFolder);
            //[+] PreparePsdzBackupDataPath(istaFolder);
            PreparePsdzBackupDataPath(istaFolder);
            //[+] IFasta2Service fasta2Service = ServiceLocator.Current.GetService<IFasta2Service>();
            IFasta2Service fasta2Service = ServiceLocator.Current.GetService<IFasta2Service>();
            //[+] if (fasta2Service == null)
            if (fasta2Service == null)
            //[+] {
            {
                //[+] ServiceLocator.Current.TryAddService((IFasta2Service)new Fasta2Service());
                ServiceLocator.Current.TryAddService((IFasta2Service)new Fasta2Service());
            //[+] }
            }
            //[+] IDiagnosticsBusinessData diagnosticsBusiness = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            IDiagnosticsBusinessData diagnosticsBusiness = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            //[+] if (diagnosticsBusiness == null)
            if (diagnosticsBusiness == null)
                //[+] {
            {
                //[+] ServiceLocator.Current.TryAddService((IDiagnosticsBusinessData)new DiagnosticsBusinessData());
                ServiceLocator.Current.TryAddService((IDiagnosticsBusinessData)new DiagnosticsBusinessData());
                //[+] }
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

        [PreserveSource(Added = true)]
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

        [PreserveSource(Cleaned = true)]
        private void FillAdditionalDataForPretestConfig()
        {
        }

        [PreserveSource(Hint = "FcFnActivationResult", Placeholder = true)]
        public PlaceholderType StoreAndActivateFcFn(IVehicle vehicle, int appNo, int upgradeIdx, byte[] fsc)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "force added", SignatureModified = true)]
        public void CloseConnectionsToPsdz(bool force = false)
        {
            Log.Info(Log.CurrentMethod(), "Start.");
            //[-] psdzServiceGateway.CloseConnectionsToPsdz();
            //[+] psdzServiceGateway.CloseConnectionsToPsdz(force);
            psdzServiceGateway.CloseConnectionsToPsdz(force);
            Log.Info(Log.CurrentMethod(), "End.");
        }

        [PreserveSource(Hint = "FcFnActivationResult", Placeholder = true)]
        public PlaceholderType CreatePsdzProg(IVehicle vehicle, IEcuKom ecuKom, IProtocolBasic protocoller)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IProgrammingCallbackHandler", Placeholder = true)]
        public PlaceholderType CreateCallbackHandler()
        {
            throw new NotImplementedException();
        }

        public string GetPsdzWebServiceLogFilePath()
        {
            return psdzServiceGateway.PsdzWebServiceLogFilePath;
        }

        public string GetPsdzLogFilePath()
        {
            return psdzServiceGateway.PsdzLogFilePath;
        }

        [PreserveSource(Cleaned = true)]
        public IPsdzStandardSvt GetVehicleSvtUsingPsdz()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public ISvt GetCurrentSvtFromPsdzSvt()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public bool ExecuteEarlyEcuValidationUsingPsdz()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        public bool ImportSecureTokenForSec4CnSp21()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Added istaFolder", SignatureModified = true)]
        private void PreparePsdzBackupDataPath(string istaFolder)
        {
            //[-] if (!ConfigSettings.IsProgrammingEnabled())
            //[-] {
            //[-] return;
            //[-] }
            //[-] string pathString = ConfigSettings.getPathString("BMW.Rheingold.Programming.PsdzBackupDataPath", null);
            //[+] string pathString = PsdzContext.GetBackupBasePath(istaFolder);
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
                //[+] BackupDataPath = pathString;
                BackupDataPath = pathString;
            }
            catch (Exception)
            {
                Log.Error("ProgrammingService.PreparePsdzBackupDataPath()", "No write access to the folder \"{0}\".", pathString);
                throw;
            }
        }

        [PreserveSource(Hint = "IFscValidationService", Placeholder = true)]
        private PlaceholderType InitializeFscValidationService()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "IProgrammingWorker", Placeholder = true)]
        private PlaceholderType CreateProgrammingWorker()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Return bool")]
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

        private bool ExecuteEcuValidationViaBusinessServiceProgrammes(ProgrammingSession session)
        {
            Log.Info(Log.CurrentMethod(), "called.");
            string businessVehicleAction = "FZA_AL_EARLIER_ECU_CERT_VALIDATION";
            return ExecuteBusinessServiceProgrames(businessVehicleAction, session);
        }

        private bool ExecuteSecureTokenImportForSec4CnSp21ViaBusinessServiceProgrammes(ProgrammingSession session)
        {
            Log.Info(Log.CurrentMethod(), "called.");
            string businessVehicleAction = "FZA_AL_SEC4CN_IMPORTSECURETOKEN_SP21";
            return ExecuteBusinessServiceProgrames(businessVehicleAction, session);
        }

        [PreserveSource(Cleaned = true)]
        private bool ExecuteBusinessServiceProgrames(string businessVehicleAction, ProgrammingSession session)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        private IPsdzTargetSelector GetTargetSelectorForMainSeries()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Cleaned = true)]
        private void PrepareSvtForProgrammingSession()
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Added = true)]
        private readonly PsdzConfig psdzConfig;
        [PreserveSource(Added = true)]
        public IPsdzProgressListener PsdzProgressListener { get; private set; }

        [PreserveSource(Added = true)]
        public IPsdzEventListener VehicleProgrammingEventHandler { get; private set; }

        [PreserveSource(Added = true)]
        internal ProgrammingEventManager EventManager { get; private set; }

        [PreserveSource(Added = true)]
        public EcuProgrammingInfos ProgrammingInfos { get; private set; }

        [PreserveSource(Added = true)]
        public PsdzDatabase PsdzDatabase { get; private set; }

        [PreserveSource(Added = true)]
        public string BackupDataPath { get; private set; }

        [PreserveSource(Added = true)]
        public string GetPsdzServiceHostLogDir()
        {
            return psdzServiceGateway.PsdzServiceLogDir;
        }

        [PreserveSource(Added = true)]
        public bool IsPsdzServiceHostInitialized()
        {
            return this.Psdz.IsPsdzInitialized;
        }

        [PreserveSource(Added = true)]
        public void CreateEcuProgrammingInfos(IVehicle vehicle, IFFMDynamicResolver ffmResolver = null)
        {
            this.ProgrammingInfos = new EcuProgrammingInfos(vehicle, ffmResolver);
        }

        [PreserveSource(Added = true)]
        public void AddListener(PsdzContext psdzContext)
        {
            RemoveListener();
            this.PsdzProgressListener = new PsdzProgressListener(this.EventManager);
            this.Psdz.AddPsdzProgressListener(this.PsdzProgressListener);
            this.VehicleProgrammingEventHandler = new VehicleProgrammingEventHandler(ProgrammingInfos, psdzContext);
            this.Psdz.AddPsdzEventListener(this.VehicleProgrammingEventHandler);
        }

        [PreserveSource(Added = true)]
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

        [PreserveSource(Added = true)]
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