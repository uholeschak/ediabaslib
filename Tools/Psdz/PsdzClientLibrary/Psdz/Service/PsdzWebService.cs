using BMW.Rheingold.Psdz.Client;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PsdzClient;

namespace BMW.Rheingold.Psdz
{
    public class PsdzWebService : IPsdzWebService
    {
        private const string JavaInstalationErrorMessage = "{0} Validation - {1}";
        private readonly Version _expectedJREVersion = new Version(17, 0, 10);
        private readonly string _psdzWebApiLogDir;
        private readonly int _waitTimeOutSeconds = 180;
        private readonly PsdzProgressListenerDispatcher progressListenerDispatcher = new PsdzProgressListenerDispatcher();
        private readonly ManualResetEvent _terminationSignal = new ManualResetEvent(initialState: false);
        private readonly Func<bool> _isPsdzInitialized;
        private IWebCallHandler webCallHandler;
        private PsdzWebApiLifeCycleController lifeCycleController;
        private Process psdzWebserviceProcess;
        public ICertificateManagementService CertificateManagementService { get; private set; }
        public IConfigurationService ConfigurationService { get; private set; }
        public IConnectionFactoryService ConnectionFactoryService { get; private set; }
        public IConnectionManagerService ConnectionManagerService { get; private set; }
        public IEcuService EcuService { get; private set; }
        public IEventManagerService EventManagerService { get; private set; }
        public IHttpConfigurationService HttpConfigurationService { get; private set; }
        public IHttpServerService HttpServerService { get; private set; }
        public IIndividualDataRestoreService IndividualDataRestoreService { get; private set; }
        public IKdsService KdsService { get; private set; }
        public ILogicService LogicService { get; private set; }
        public ILogService LogService { get; private set; }
        public IMacrosService MacrosService { get; private set; }
        public IObjectBuilderService ObjectBuilderService { get; private set; }
        public IProgrammingService ProgrammingService { get; private set; }
        public ISecureCodingService SecureCodingService { get; private set; }
        public ISecureDiagnosticsService SecureDiagnosticsService { get; private set; }
        public ISecureFeatureActivationService SecureFeatureActivationService { get; private set; }
        public ISecurityManagementService SecurityManagementService { get; private set; }
        public ITalExecutionService TalExecutionService { get; private set; }
        public IVcmService VcmService { get; private set; }
        public IProgrammingTokenService ProgrammingTokenService { get; private set; }

        [PreserveSource(Hint = "istaFolder added")]
        public PsdzWebService(string psdzWebAPILogDir, Func<bool> isPsdzInitialized, string istaFolder)
        {
            _psdzWebApiLogDir = psdzWebAPILogDir;
            _isPsdzInitialized = isPsdzInitialized;
            _istaFolder = istaFolder;
        }

        public void StartIfNotRunning(string jrePath, string jvmOptions, string jarArguments)
        {
            try
            {
                if (webCallHandler != null)
                {
                    Log.Info(Log.CurrentMethod(), "Psdz Webservice is already running.");
                    return;
                }

                Log.Debug(Log.CurrentMethod(), "Psdz Webservice will be started.");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int num = PsdzWebserviceRegistrar.RegisterCurrentProcess();
                webCallHandler = new WebCallHandler($"https://localhost:{num}/", Prepare, _isPsdzInitialized);
                InitServices();
                switch (PsdzWebserviceRegistrar.GetWebserviceStatus())
                {
                    case WebserviceSessionStatus.Created:
                        TrySetupPsdzWebserviceProcess(jrePath, jvmOptions, jarArguments, num);
                        StartPsdzWebserviceProcess();
                        break;
                    case WebserviceSessionStatus.ProcessStarted:
                        HandleOngoingInitializationByAnotherThread();
                        break;
                    case WebserviceSessionStatus.Running:
                        Log.Debug(Log.CurrentMethod(), "Psdz Webservice is already running, we can go ahead and use it right away.");
                        InitLifeCycleController();
                        break;
                    default:
                        throw new NotImplementedException("It looks like an extra value was added to WebserviceSessionStatus but not correctly handled in " + Log.CurrentMethod() + ".");
                }

                Log.Info(Log.CurrentMethod(), "PSdZ Webservice is now ready and configured.");
                stopwatch.Stop();
                AddServiceCodeToFastaProtocol("PWS01_DurationWebserviceStart_nu_LF", $"Duration={stopwatch.Elapsed.TotalSeconds} seconds");
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void Shutdown()
        {
            Log.Info(Log.CurrentMethod(), "called");
            if (lifeCycleController != null)
            {
                lifeCycleController.RequestShutdown();
                Log.Info(Log.CurrentMethod(), "Shutdown has been requested. Waiting for the shutdown.");
                _terminationSignal.WaitOne();
            }
        }

        [PreserveSource(Hint = "GetSwiVersionNum added, ServiceLocator removed")]
        public bool IsReady()
        {
            IConfigurationService configurationService = ConfigurationService;
            if (configurationService == null || !configurationService.IsReady())
            {
                return false;
            }

            if (ClientContext.GetSwiVersionNum() < 40056)
            {
                return true;
            }

            RootDirectorySetupResultModel rootDirectorySetupResult = ConfigurationService.GetRootDirectorySetupResult();
            if (rootDirectorySetupResult != null && rootDirectorySetupResult.Success)
            {
                return true;
            }

            Log.Error(Log.CurrentMethod(), "PSDZ WebService could not set root directory " + rootDirectorySetupResult?.Message);
            TryKillTree(psdzWebserviceProcess);
            return false;
        }

        private static bool TryKillTree(Process proc, int timeoutMs = 10000)
        {
            if (proc == null)
            {
                return true;
            }

            try
            {
                proc.Refresh();
                if (proc.HasExited)
                {
                    return true;
                }

                using (Process process = Process.Start(new ProcessStartInfo { FileName = "taskkill", Arguments = $"/PID {proc.Id} /T /F", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true }))
                {
                    process?.WaitForExit(timeoutMs);
                }

                proc.Refresh();
                if (proc.HasExited)
                {
                    return true;
                }

                proc.Kill();
                return proc.WaitForExit(Math.Max(3000, timeoutMs / 3));
            }
            catch
            {
                try
                {
                    proc.Refresh();
                    return proc.HasExited;
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                try
                {
                    proc.Close();
                }
                catch
                {
                }
            }
        }

        public void SetPsdzEventListener(IPsdzEventListener eventListener)
        {
            EventManagerService.AddEventListener(eventListener);
        }

        public void SetPsdzProgressListener(IPsdzProgressListener progressListener)
        {
            progressListenerDispatcher.AddPsdzProgressListener(progressListener);
        }

        public void RemovePsdzEventListener(IPsdzEventListener eventListener)
        {
            EventManagerService.RemoveEventListener(eventListener);
        }

        public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
        {
            progressListenerDispatcher.RemovePsdzProgressListener(progressListener);
        }

        private void TrySetupPsdzWebserviceProcess(string jrePath, string jvmOptions, string jarArguments, int sessionWebservicePort)
        {
            Log.Info(Log.CurrentMethod(), "Creating a new PSdZ Webservice process...");
            try
            {
                string javaExePath = Path.Combine(jrePath, "bin", "java.exe");
                BoolResultObject boolResultObject = EnsureJavaIsCorrectlyInstalled(javaExePath);
                if (!boolResultObject.Result)
                {
                    throw new JavaInstallationException(boolResultObject.ErrorMessage);
                }

                BoolResultObject<Process> boolResultObject2 = ConfigureWebserviceProcess(jvmOptions, jarArguments, sessionWebservicePort, javaExePath);
                if (!boolResultObject2.Result)
                {
                    throw new InvalidOperationException(boolResultObject2.ErrorMessage);
                }

                psdzWebserviceProcess = boolResultObject2.ResultObject;
            }
            catch (Exception ex)
            {
                Log.ErrorException(Log.CurrentMethod(), ex);
                AddServiceCodeToFastaProtocol("PWS05_WebserviceStartRuntimeError_nu_LF", string.Format(ex.Message + " - " + ex.InnerException?.Message));
                throw;
            }
        }

        private BoolResultObject EnsureJavaIsCorrectlyInstalled(string javaExePath)
        {
            if (!TryValidateJavaExecutable(javaExePath, out var error))
            {
                return Fail("PWS02_JavaInstalationValidationError_nu_LF", error);
            }

            BoolResultObject<string> boolResultObject = TryValidateJavaInitialization(javaExePath, out error);
            if (!boolResultObject.Result)
            {
                return Fail("PWS03_JavaInitializationValidationError_nu_LF", error);
            }

            if (!TryValidateJavaVersion(boolResultObject.ResultObject, out error))
            {
                return Fail("PWS02_JavaInstalationValidationError_nu_LF", error);
            }

            if (!TryValidateJar(GetJarPath(), out error))
            {
                return Fail("PWS02_JavaInstalationValidationError_nu_LF", error);
            }

            return BoolResultObject.SuccessResult;
        }

        private bool TryValidateJar(string jarPath, out string error)
        {
            if (jarPath == null || !File.Exists(jarPath))
            {
                error = string.Format("{0} Validation - {1}", "Jar Path", "Path " + jarPath + " for the .jar file not found!");
                Log.Error(Log.CurrentMethod(), error);
                return false;
            }

            error = null;
            return true;
        }

        private BoolResultObject<string> TryValidateJavaInitialization(string javaExePath, out string error)
        {
            string text;
            try
            {
                string arguments = "-version";
                Process process = CreateBaseProcess(javaExePath, arguments);
                process.Start();
                text = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Log.Info(Log.CurrentMethod(), "JDK Version Info: \n{0}", text);
            }
            catch (Exception ex)
            {
                error = string.Format(ex.Message);
                Log.ErrorException(Log.CurrentMethod(), error, ex);
                return BoolResultObject<string>.Fail(error);
            }

            error = null;
            return BoolResultObject<string>.Success(text);
        }

        private bool TryValidateJavaVersion(string checkVersionProcessOutput, out string error)
        {
            Match match = Regex.Match(checkVersionProcessOutput, "\\d+\\.\\d+\\.\\d+");
            try
            {
                if (match.Success && Version.Parse(match.Value) < _expectedJREVersion)
                {
                    error = string.Format("{0} Validation - {1}", "Java Version", $"Installed Java version is too low: {match.Value} - Expected {_expectedJREVersion}.");
                    Log.Error(Log.CurrentMethod(), error);
                    return false;
                }

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = string.Format("{0} Validation - {1}", "Java Version", "Could not parse Java version from string '" + match.Value + "'. Exception: " + ex.Message);
                Log.ErrorException(Log.CurrentMethod(), error, ex);
                return false;
            }
        }

        private bool TryValidateJavaExecutable(string javaExePath, out string error)
        {
            Log.Info(Log.CurrentMethod(), $"JDK {_expectedJREVersion} Java.exe path: {javaExePath}");
            if (!File.Exists(javaExePath))
            {
                error = string.Format("{0} Validation - {1}", "Java Executable", $"java.exe not found at {javaExePath}. JDK {_expectedJREVersion} is required for the PSdZ Webservice.");
                Log.Error(Log.CurrentMethod(), error);
                return false;
            }

            error = null;
            return true;
        }

        private BoolResultObject Fail(string serviceCode, string message)
        {
            AddServiceCodeToFastaProtocol(serviceCode, message);
            return BoolResultObject.FailResult(message);
        }

        private static void AddServiceCodeToFastaProtocol(string serviceCode, string message)
        {
            if (ServiceLocator.Current.TryGetService<IFasta2Service>(out var service))
            {
                service.AddServiceCode(serviceCode, message, LayoutGroup.P, allowMultipleEntries: false, bufferIfSessionNotStarted: true);
            }
        }

        private BoolResultObject<Process> ConfigureWebserviceProcess(string jvmOptions, string jarArguments, int sessionWebservicePort, string javaExePath)
        {
            Process process = null;
            try
            {
                string arguments = $"{jvmOptions} -jar \"{GetJarPath()}\" {jarArguments} --server.port={sessionWebservicePort}";
                process = CreateBaseProcess(javaExePath, arguments);
                process.Exited += LogProcessOutputAfterExit;
            }
            catch (Exception ex)
            {
                Log.ErrorException(Log.CurrentMethod(), ex);
                return BoolResultObject<Process>.Fail(ex.Message);
            }

            return BoolResultObject<Process>.Success(process);
        }

        private Process CreateBaseProcess(string javaExePath, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = javaExePath;
            process.StartInfo.Arguments = arguments;
            Log.Info(Log.CurrentMethod(), "Process Arguments: " + process.StartInfo.Arguments);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            return process;
        }

        [PreserveSource(Hint = "fullPath modified")]
        private string GetJarPath()
        {
            string fullPath = Path.Combine(_istaFolder, "PSdZ\\WebService");
            if (!Directory.Exists(fullPath))
            {
                Log.Error(Log.CurrentMethod(), "Directory " + fullPath + " does not exists. You can check your BMW_RHEINGOLD_PROGRAMMING_PSDZWEBSERVICE_DIRECTORY registry key.");
            }

            string text = Directory.GetFiles(fullPath, "*.jar").FirstOrDefault((string s) => s.IndexOf("Psdz-Webservice", StringComparison.OrdinalIgnoreCase) >= 0);
            Log.Info(Log.CurrentMethod(), "returning " + text);
            return text;
        }

        private void LogProcessOutputAfterExit(object sender, EventArgs e)
        {
            string text = psdzWebserviceProcess.StandardOutput.ReadToEnd();
            string text2 = psdzWebserviceProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(text2))
            {
                Log.Error(Log.CurrentMethod(), "Output from PSdZ Webservice Process (StandardError): \n" + text2);
            }
            else
            {
                Log.Info(Log.CurrentMethod(), "Output from PSdZ Webservice Process (StandardOutput): \n" + text);
            }
        }

        private void StartPsdzWebserviceProcess()
        {
            try
            {
                if (PsdzWebserviceRegistrar.StartAndRegisterWebserviceProcess(psdzWebserviceProcess))
                {
                    WaitForWebserviceInitialization(IsReady);
                    Log.Debug(Log.CurrentMethod(), "Psdz Webservice started and is responsive.");
                    InitLifeCycleController();
                    DoInitSettings();
                    PsdzWebserviceRegistrar.SignalWebserviceInitializationCompleted();
                }
                else
                {
                    HandleOngoingInitializationByAnotherThread();
                }
            }
            catch (Exception)
            {
                PsdzWebserviceRegistrar.DeregisterCurrentProcess();
                throw;
            }
        }

        private void InitServices()
        {
            string clientId = Guid.NewGuid().ToString();
            HttpServerService = new HttpServerService(webCallHandler);
            HttpConfigurationService = new HttpConfigurationService(webCallHandler);
            ConnectionFactoryService = new ConnectionFactoryService(webCallHandler);
            ConfigurationService = new ConfigurationService(webCallHandler, HttpServerService);
            ConnectionManagerService = new ConnectionManagerService(webCallHandler, ConnectionFactoryService, HttpConfigurationService);
            EcuService = new EcuService(webCallHandler);
            EventManagerService = new EventManagerService(webCallHandler, clientId);
            MacrosService = new MacrosService(webCallHandler);
            ProgrammingService = new ProgrammingService(webCallHandler);
            IndividualDataRestoreService = new IndividualDataRestoreService(webCallHandler, progressListenerDispatcher, MacrosService, ProgrammingService);
            CertificateManagementService = new CertificateManagementService(webCallHandler);
            KdsService = new KdsService(webCallHandler);
            LogicService = new LogicService(webCallHandler);
            LogService = new LogService(webCallHandler, clientId, _psdzWebApiLogDir);
            ObjectBuilderService = new ObjectBuilderService(webCallHandler);
            SecureCodingService = new SecureCodingService(webCallHandler);
            SecureDiagnosticsService = new SecureDiagnosticsService(webCallHandler);
            SecureFeatureActivationService = new SecureFeatureActivationService(webCallHandler);
            SecurityManagementService = new SecurityManagementService(webCallHandler);
            ProgrammingTokenService = new ProgrammingTokenService(webCallHandler);
            TalExecutionService = new TalExecutionService(webCallHandler, ProgrammingService, ObjectBuilderService, EventManagerService, progressListenerDispatcher);
            VcmService = new VcmService(webCallHandler);
        }

        private void InitLifeCycleController()
        {
            if (lifeCycleController != null)
            {
                Log.Warning(Log.CurrentMethod(), "Attempted to initialize life cycle controller more than once! This is unintended behavior, and likely indicates a problem.");
                return;
            }

            lifeCycleController = new PsdzWebApiLifeCycleController(new ILifeCycleDependencyProvider[2] { webCallHandler, TalExecutionService });
            lifeCycleController.Shutdown += OnPsdzWebServiceShutdown;
        }

        private void Prepare()
        {
            try
            {
                webCallHandler.IgnorePrepareExecuteRequest = true;
                LogService?.PrepareLoggingForCurrentThread();
                EventManagerService?.PrepareListening();
            }
            finally
            {
                webCallHandler.IgnorePrepareExecuteRequest = false;
            }
        }

        private void OnPsdzWebServiceShutdown(object sender, EventArgs e)
        {
            try
            {
                PsdzWebserviceRegistrar.DeregisterCurrentProcess();
                EventManagerService?.RemoveAllEventListeners();
                lifeCycleController.Shutdown -= OnPsdzWebServiceShutdown;
                lifeCycleController.Dispose();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
            finally
            {
                _terminationSignal.Set();
            }
        }

        private void WaitForWebserviceInitialization(Func<bool> readinessTest)
        {
            int lBPTimeoutForStartPsdzWebservice = GetLBPTimeoutForStartPsdzWebservice();
            DateTime dateTime = DateTime.Now.AddSeconds(lBPTimeoutForStartPsdzWebservice);
            Log.Debug(Log.CurrentMethod(), $"Waiting up to {lBPTimeoutForStartPsdzWebservice} seconds for Psdz Webservice initialization...");
            while (!readinessTest())
            {
                if (DateTime.Now > dateTime)
                {
                    Log.Error(Log.CurrentMethod(), $"Failed to start in {lBPTimeoutForStartPsdzWebservice} seconds. -> Throwing a Timeout Exception.");
                    throw new TimeoutException($"PSdZ Webservice failed to start in {lBPTimeoutForStartPsdzWebservice} seconds.");
                }

                Log.Debug(Log.CurrentMethod(), "Still waiting...");
                Task.Delay(500).Wait();
            }

            Log.Info(Log.CurrentMethod(), "PSdZ Webservice is now ready.");
        }

        public int GetLBPTimeoutForStartPsdzWebservice()
        {
            IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient();
            try
            {
                if (istaIcsServiceClient.IsAvailable())
                {
                    int? timeoutLengthWebserviceStart = istaIcsServiceClient.GetTimeoutLengthWebserviceStart();
                    if (timeoutLengthWebserviceStart.HasValue && timeoutLengthWebserviceStart.Value > 0)
                    {
                        return (int)TimeSpan.FromMinutes(timeoutLengthWebserviceStart.Value).TotalSeconds;
                    }
                }
            }
            finally
            {
                ((IDisposable)istaIcsServiceClient)?.Dispose();
            }

            return _waitTimeOutSeconds;
        }

        private void HandleOngoingInitializationByAnotherThread()
        {
            Log.Info(Log.CurrentMethod(), "Psdz Webservice is being started by a different thread, waiting for initialization to be completed.");
            WaitForWebserviceInitialization(() => PsdzWebserviceRegistrar.GetWebserviceStatus() == WebserviceSessionStatus.Running);
            InitLifeCycleController();
        }

        private void DoInitSettings()
        {
            SetPortForPsdzHttpServer();
            SetPsdzLogLevel();
            SetProdiasLoglevel();
        }

        private void SetPortForPsdzHttpServer()
        {
            int num = ((!ConfigSettings.GetFeatureEnabledStatus("UseReducedPortRangeForMirror").IsActive) ? NetUtils.GetFirstFreePort(12300, 12400) : NetUtils.GetFirstFreePort(12304, 12319));
            if (num != -1)
            {
                Log.Info(Log.CurrentMethod(), $"Set the HTTP server port to {num}");
                HttpConfigurationService.SetHttpServerPort(num);
            }
            else
            {
                Log.Info(Log.CurrentMethod(), "The HTTP server port is not set");
            }
        }

        private void SetPsdzLogLevel()
        {
            PsdzLoglevel? psdzLoglevel = null;
            int configint = ConfigSettings.getConfigint("DebugLevel", 0);
            if (configint > 0 && configint < 6)
            {
                psdzLoglevel = (PsdzLoglevel)configint;
            }

            if (psdzLoglevel.HasValue)
            {
                Log.Info(Log.CurrentMethod(), $"Setting PSdZ Log Level to: {psdzLoglevel}");
                LogService.SetLogLevel(psdzLoglevel.Value);
            }
        }

        private void SetProdiasLoglevel()
        {
            ProdiasLoglevel? prodiasLoglevel = null;
            if (Enum.TryParse<ProdiasLoglevel>(ConfigSettings.getConfigString("BMW.Rheingold.Programming.Prodias.LogLevel", null), out var result))
            {
                prodiasLoglevel = result;
            }

            if (prodiasLoglevel.HasValue)
            {
                Log.Info(Log.CurrentMethod(), $"Setting Prodias Log Level to: {prodiasLoglevel}");
                ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel.Value);
            }
        }

        [PreserveSource(Hint = "Added")]
        private readonly string _istaFolder;
    }
}