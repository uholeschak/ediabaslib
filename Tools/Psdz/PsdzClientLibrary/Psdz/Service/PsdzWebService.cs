using BMW.Rheingold.Psdz.Client;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Concurrent;
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
        private enum DataType
        {
            Output,
            Error
        }

        private readonly string _psdzWebApiLogDir;
        private readonly int _waitTimeOutSeconds = 180;
        private readonly PsdzProgressListenerDispatcher progressListenerDispatcher = new PsdzProgressListenerDispatcher();
        private readonly ManualResetEvent _terminationSignal = new ManualResetEvent(initialState: false);
        private readonly Func<bool> _isPsdzInitialized;
        private volatile PsdzWebserviceStartFailureReason _detectedStartFailureReason;
        private readonly ConcurrentQueue<string> _startupLogBuffer = new ConcurrentQueue<string>();
        private const int DEFAULT_HTTP_SERVER_PORT_MIN = 12300;
        private const int DEFAULT_HTTP_SERVER_PORT_MAX = 12400;
        private const int HTTP_SERVER_FALLBACK_PORT = 8888;
        private IWebCallHandler webCallHandler;
        private PsdzWebApiLifeCycleController lifeCycleController;
        private Process psdzWebserviceProcess;
        private readonly IPsdzWebservicePreflightChecker _preflightChecker;
        public IBaureiheUtilityService BaureiheUtilityService { get; private set; }
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
        public IFpService FpService { get; private set; }

        [PreserveSource(Hint = "istaFolder added", SignatureModified = true)]
        public PsdzWebService(string psdzWebAPILogDir, Func<bool> isPsdzInitialized, string istaFolder)
        {
            _psdzWebApiLogDir = psdzWebAPILogDir;
            _isPsdzInitialized = isPsdzInitialized;
            //[+] _istaFolder = istaFolder;
            _istaFolder = istaFolder;
            _preflightChecker = new PsdzWebservicePreflightChecker(_psdzWebApiLogDir, CreateBaseProcess, CreateMonitoredProcess);
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
                string jarPath = GetJarPath();
                string javaExePath = Path.Combine(jrePath, "bin", "java.exe");
                _preflightChecker.Execute(num, jarPath, javaExePath);
                switch (PsdzWebserviceRegistrar.GetWebserviceStatus())
                {
                    case WebserviceSessionStatus.Created:
                        TrySetupPsdzWebserviceProcess(javaExePath, jvmOptions, jarArguments, num);
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
            catch (PsdzWebserviceStartException ex)
            {
                AddServiceCodeToFastaProtocol(ex.ServiceCode, ex.Message);
                throw;
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
                PsdzWebserviceHelper.TryKillTree(psdzWebserviceProcess);
                Log.Info(Log.CurrentMethod(), "Shutdown is complete.");
            }
        }

        public bool IsReady()
        {
            IConfigurationService configurationService = ConfigurationService;
            if (configurationService == null || !configurationService.IsReady())
            {
                return false;
            }

            //[+] if (ClientContext.GetSwiVersionNum() < 40056)
            if (ClientContext.GetSwiVersionNum() < 40056)
            //[+] {
            {
                //[+] return true;
                return true;
            //[+] }
            }

            RootDirectorySetupResultModel rootDirectorySetupResult = ConfigurationService.GetRootDirectorySetupResult();
            if (rootDirectorySetupResult != null && rootDirectorySetupResult.Success)
            {
                return true;
            }

            Log.Error(Log.CurrentMethod(), "PSDZ WebService could not set root directory " + rootDirectorySetupResult?.Message);
            if (ServiceLocator.Current.TryGetService<IFasta2Service>(out var service))
            {
                service.AddServiceCode("PWS04_PsdzWebServiceSetRootDirectoryFailed_nu_LF", "Setting up root directory for PSDZ web service failed", LayoutGroup.P);
            }

            if (ServiceLocator.Current.TryGetService<IInteractionService>(out var service2))
            {
                service2.RegisterAsync(new InteractionMessageModel { Title = new FormatedData("#NotificationMessageTitle.Error").Localize(), MessageText = new FormatedData("#SetRootDirectoryFailed").Localize(), IsDetailButtonVisible = false, IsCloseButtonEnabled = false });
            }

            PsdzWebserviceHelper.TryKillTree(psdzWebserviceProcess);
            return false;
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

        private void TrySetupPsdzWebserviceProcess(string javaExePath, string jvmOptions, string jarArguments, int sessionWebservicePort)
        {
            Log.Info(Log.CurrentMethod(), "Creating a new PSdZ Webservice process...");
            try
            {
                BoolResultObject<Process> boolResultObject = ConfigureWebserviceProcess(jvmOptions, jarArguments, sessionWebservicePort, javaExePath);
                if (!boolResultObject.Result)
                {
                    throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.Unknown);
                }

                psdzWebserviceProcess = boolResultObject.ResultObject;
            }
            catch (PsdzWebserviceStartException)
            {
                throw;
            }
            catch (Exception ex2)
            {
                Log.ErrorException(Log.CurrentMethod(), ex2);
                AddServiceCodeToFastaProtocol("PWS05_WebserviceStartRuntimeError_nu_LF", ex2.Message + " - " + ex2.InnerException?.Message);
                throw;
            }
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
                process = CreateMonitoredProcess(javaExePath, arguments);
            }
            catch (Exception ex)
            {
                Log.ErrorException(Log.CurrentMethod(), ex);
                return BoolResultObject<Process>.Fail(ex.Message);
            }

            return BoolResultObject<Process>.Success(process);
        }

        internal Process CreateBaseProcess(string exePath, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = exePath;
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

        internal Process CreateMonitoredProcess(string javaExePath, string arguments)
        {
            Process process = CreateBaseProcess(javaExePath, arguments);
            process.OutputDataReceived += delegate (object s, DataReceivedEventArgs a)
            {
                HandleProcessStartupLine(a.Data, DataType.Output);
            };
            process.ErrorDataReceived += delegate (object s, DataReceivedEventArgs a)
            {
                HandleProcessStartupLine(a.Data, DataType.Error);
            };
            process.Exited += LogProcessOutputAfterExit;
            return process;
        }

        private void HandleProcessStartupLine(string line, DataType dataType)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            Log.Error(Log.CurrentMethod(), $"Received {dataType} data: {line}");
            _startupLogBuffer.Enqueue(line);
            string result;
            while (_startupLogBuffer.Count > 200 && _startupLogBuffer.TryDequeue(out result))
            {
            }

            if (_detectedStartFailureReason != PsdzWebserviceStartFailureReason.None)
            {
                return;
            }

            (Regex, PsdzWebserviceStartFailureReason)[] patterns = PsdzWebserviceErrorPatterns.Patterns;
            for (int i = 0; i < patterns.Length; i++)
            {
                var(regex, psdzWebserviceStartFailureReason) = patterns[i];
                if (regex.IsMatch(line))
                {
                    _detectedStartFailureReason = psdzWebserviceStartFailureReason;
                    Log.Error(Log.CurrentMethod(), $"Detected startup failure reason={psdzWebserviceStartFailureReason} via line: {line}");
                    break;
                }
            }
        }

        private void LogProcessOutputAfterExit(object sender, EventArgs e)
        {
            (sender as Process).Exited -= LogProcessOutputAfterExit;
            string text = psdzWebserviceProcess?.StandardOutput?.ReadToEnd();
            string text2 = psdzWebserviceProcess?.StandardError?.ReadToEnd();
            if (psdzWebserviceProcess.ExitCode == 0)
            {
                return;
            }

            string text3 = (text2 + "\n" + text).Trim();
            Log.Error(Log.CurrentMethod(), "Launch probe failed (exit={0}). Output:\n{1}", psdzWebserviceProcess.ExitCode, text3);
            (Regex, PsdzWebserviceStartFailureReason)[] patterns = PsdzWebserviceErrorPatterns.Patterns;
            for (int i = 0; i < patterns.Length; i++)
            {
                var(regex, psdzWebserviceStartFailureReason) = patterns[i];
                if (regex.IsMatch(text3))
                {
                    Log.Error(Log.CurrentMethod(), $"Launch probe failed. Reason={psdzWebserviceStartFailureReason}. Snippet={PsdzWebserviceHelper.Truncate(text3, 400)}");
                    throw PsdzWebserviceStartException.Create(psdzWebserviceStartFailureReason);
                }
            }

            Log.Error(Log.CurrentMethod(), $"Launch probe non-zero exit ({psdzWebserviceProcess.ExitCode}).");
            //[-]throw PsdzWebserviceStartException.Create(PsdzWebserviceStartFailureReason.JavaRuntimeFaulty);
        }

        [PreserveSource(Hint = "fullPath modified", SignatureModified = true)]
        private string GetJarPath()
        {
            //[-] string fullPath = Path.GetFullPath(ConfigSettings.getPathString("BMW.Rheingold.Programming.PsdzWebservice.Directory", "..\\..\\..\\PSdZ\\WebService"));
            //[+] string fullPath = Path.Combine(_istaFolder, "PSdZ\\WebService");
            string fullPath = Path.Combine(_istaFolder, "PSdZ\\WebService");
            if (!Directory.Exists(fullPath))
            {
                Log.Error(Log.CurrentMethod(), "Directory " + fullPath + " does not exists. You can check your BMW_RHEINGOLD_PROGRAMMING_PSDZWEBSERVICE_DIRECTORY registry key.");
            }

            string text = Directory.GetFiles(fullPath, "*.jar").FirstOrDefault((string s) => s.IndexOf("Psdz-Webservice", StringComparison.OrdinalIgnoreCase) >= 0);
            Log.Info(Log.CurrentMethod(), "returning " + text);
            return text;
        }

        private void StartPsdzWebserviceProcess()
        {
            try
            {
                if (PsdzWebserviceRegistrar.StartAndRegisterWebserviceProcess(psdzWebserviceProcess))
                {
                    try
                    {
                        WaitForWebserviceInitialization(IsReady);
                    }
                    catch (TimeoutException)
                    {
                        if (_detectedStartFailureReason == PsdzWebserviceStartFailureReason.None)
                        {
                            _detectedStartFailureReason = PsdzWebserviceStartFailureReason.Timeout;
                        }

                        Log.Error(Log.CurrentMethod(), $"PSdZ Webservice failed to become ready. Reason={_detectedStartFailureReason}");
                        throw PsdzWebserviceStartException.Create(_detectedStartFailureReason);
                    }

                    if (_detectedStartFailureReason != PsdzWebserviceStartFailureReason.None)
                    {
                        Log.Error(Log.CurrentMethod(), $"Detected startup failure reason={_detectedStartFailureReason}");
                        throw PsdzWebserviceStartException.Create(_detectedStartFailureReason);
                    }

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
            catch
            {
                PsdzWebserviceRegistrar.DeregisterCurrentProcess();
                if (!_startupLogBuffer.IsEmpty)
                {
                    Log.Error(Log.CurrentMethod(), "Startup log excerpt:\n" + string.Join("\n", _startupLogBuffer));
                }

                throw;
            }
        }

        private void InitServices()
        {
            string clientId = Guid.NewGuid().ToString();
            BaureiheUtilityService = new BaureiheUtilityService(webCallHandler);
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
            FpService = new FpService(webCallHandler);
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
            int num = DetermineHttpPort();
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

        private int DetermineHttpPort()
        {
            int? forcedPortFromRegKey = GetForcedPortFromRegKey("BMW.Rheingold.Programming.PsdzProg.HttpServerPort.Min");
            int? forcedPortFromRegKey2 = GetForcedPortFromRegKey("BMW.Rheingold.Programming.PsdzProg.HttpServerPort.Max");
            if (forcedPortFromRegKey.HasValue || forcedPortFromRegKey2.HasValue)
            {
                int minPort = forcedPortFromRegKey ?? 12300;
                int maxPort = forcedPortFromRegKey2 ?? 12400;
                Log.Info(Log.CurrentMethod(), "Determining Port via Registry Keys...");
                return NetUtils.GetFirstFreePort(minPort, maxPort, 8888);
            }

            if (ConfigSettings.GetFeatureEnabledStatus("UseReducedPortRangeForMirror").IsActive)
            {
                Log.Info(Log.CurrentMethod(), "Determining Port via Generic Feature Switch...");
                return NetUtils.GetFirstFreePort(12304, 12319, 8888);
            }

            Log.Info(Log.CurrentMethod(), "Determining Port via Default Values...");
            return NetUtils.GetFirstFreePort(12300, 12400, 8888);
        }

        private int? GetForcedPortFromRegKey(string regKey)
        {
            int configint = ConfigSettings.getConfigint(regKey, -1);
            int? result = null;
            if (configint > 0)
            {
                result = configint;
            }

            return result;
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

        [PreserveSource(Added = true)]
        private readonly string _istaFolder;
    }
}