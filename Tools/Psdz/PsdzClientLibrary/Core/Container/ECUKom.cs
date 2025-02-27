// BMW.Rheingold.VehicleCommunication.ECUKom
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using Ediabas;
using EdiabasLib;
using PsdzClient.Utility;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core.Container
{
    public enum CommMode
    {
        Normal,
        Simulation,
        CacheFirst
    }

    public class ECUKom : IEcuKom, IEcuKomApi
    {
        private const int STANDARD_EDIABAS_LOGLEVEL = 0;

        private const int DEFAULT_EDIABAS_TRACELEVEL = 5;

        private const int DEFAULT_EDIABAS_TRACE_SIZE = 32767;

        private const int _diagnosticPort = 50160;

        private const int _controlPort = 50161;

        private const int _portDoIP = 50162;

        private const int _sslPort = 50163;

        private const int _diagnosticPortW2V = 51560;

        private const int _controlPortW2V = 51561;

        private const int _portDoIPW2V = 51562;

        private ApiInternal api;

        private string _APP;

        private CommMode communicationMode;

        private bool isProblemHandlingTraceRunning;

        private List<string> apiJobNamesToBeCached = new List<string>();

        private DateTime lastJobExecution;

        private VCIDevice vci;

        private Dictionary<string, List<ECUJob>> ecuJobDictionary = new Dictionary<string, List<ECUJob>>();

        private bool m_FromFastaConfig;

        public List<ECUJob> jobList = new List<ECUJob>();

        public string APP
        {
            get
            {
                return _APP;
            }
            set
            {
                _APP = value;
            }
        }

        [XmlIgnore]
        public bool FromFastaConfig
        {
            get
            {
                return m_FromFastaConfig;
            }
            set
            {
                if (value != m_FromFastaConfig)
                {
                    Log.Info(Log.CurrentMethod(), "Stack: " + GetStack());
                    Log.Info(Log.CurrentMethod(), $"Setting new value to {value}");
                    m_FromFastaConfig = value;
                }
            }
        }

        [XmlIgnore]
        public int CacheHitCounter { get; set; }

        [XmlIgnore]
        public int CacheMissCounter { get; set; }

        [XmlIgnore]
        public CommMode CommunicationMode
        {
            get
            {
                return communicationMode;
            }
            set
            {
                if (value != communicationMode)
                {
                    communicationMode = value;
                }
            }
        }

        [XmlIgnore]
        public VCIDevice VCI
        {
            get
            {
                return vci;
            }
            internal set
            {
                if (vci != value)
                {
                    vci = value;
                }
            }
        }

        [XmlIgnore]
        public uint EdiabasHandle => 0;
        //public uint EdiabasHandle => api.Handle;

        [XmlIgnore]
        public bool IsInSimulationMode => CommunicationMode == CommMode.Simulation;

        public bool IsProblemHandlingTraceRunning => isProblemHandlingTraceRunning;

        public string VciIpAddress => VCI?.IPAddress;

        public VCIDeviceType VCIDeviceType => VCI.VCIType;

        public IEcuJob DefaultApiJob(string ecu, string job, string param, string resultFilter)
        {
            return apiJob(ecu, job, param, resultFilter);
        }

        public IEcuJob ApiJobWithRetries(string variant, string job, string param, string resultFilter, int retries)
        {
            return apiJob(variant, job, param, resultFilter, retries, null);
        }

        public ECUKom()
            : this(null)
        {
        }

        public ECUKom(string app, EdiabasNet ediabas = null)
        {
            api = new ApiInternal(ediabas);
            communicationMode = CommMode.Normal;
            jobList = new List<ECUJob>();
            APP = app;
            FromFastaConfig = false;
            CacheHitCounter = 0;
        }

        public void End()
        {
            try
            {
                api.apiSetConfig("EDIABASUnload", "1");
                api.apiEnd();
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.End()", exception);
            }
        }

        public string GetEdiabasIniFilePath(string iniFilename)
        {
            return EdiabasIniFilePath(iniFilename);
        }

        public bool InitVCI(IVciDevice vciDevice, bool isDoIP)
        {
            bool result = InitVCI(vciDevice, logging: true, isDoIP);
            if (isProblemHandlingTraceRunning)
            {
                SetLogLevelToMax();
            }
            return result;
        }

        public int GetCacheHitCounter()
        {
            return CacheHitCounter;
        }

        public int GetCacheMissCounter()
        {
            return CacheMissCounter;
        }

        public int GetCacheListNumberOfJobs()
        {
            return jobList.Count;
        }

        public int GetCacheListNumberOfJobsToBeRetrieved()
        {
            return apiJobNamesToBeCached.Count;
        }

        public void SetLogLevelToNormal()
        {
            api.apiSetConfig("ApiTrace", 0.ToString(CultureInfo.InvariantCulture));
            isProblemHandlingTraceRunning = false;
        }

        public void SetLogLevelToMax()
        {
            int configint = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Level.Trace.Ediabas", 5);
            int configint2 = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Trace.Ediabas.Size", 32767);
            api.apiSetConfig("ApiTrace", configint.ToString(CultureInfo.InvariantCulture));
            api.apiSetConfig("TraceSize", configint2.ToString(CultureInfo.InvariantCulture));
            isProblemHandlingTraceRunning = true;
        }

        public bool Refresh(bool isDoIP)
        {
            End();
            bool result = false;
            try
            {
                result = InitVCI(VCI, isDoIP);
            }
            catch (Exception exception)
            {
                Log.ErrorException("ECUKom.Refresh()", exception);
            }
            return result;
        }

        public bool ApiInitExt(string ifh, string unit, string app, string reserved)
        {
            return api.apiInitExt(ifh, unit, app, reserved);
        }

        public void SetEcuPath(bool logging)
        {
            try
            {
                string pathString = ConfigSettings.getPathString("BMW.Rheingold.VehicleCommunication.ECUKom.EcuPath", "..\\..\\..\\Ecu\\");
                if (!string.IsNullOrEmpty(pathString))
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    if (logging)
                    {
                        Log.Info("ECUKom.SetEcuPath()", "found EcuPath config setting: {0} AppDomain.BaseDirectory: {1}", pathString, baseDirectory);
                    }
                    if (Path.IsPathRooted(pathString))
                    {
                        api.apiSetConfig("EcuPath", Path.GetFullPath(pathString));
                        if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleCommunication.ECUKom.SetEcuPathSGCLIB", defaultValue: true))
                        {
                            Environment.SetEnvironmentVariable("SGCLIB_EDIABAS_ECU_PATH", Path.GetFullPath(pathString));
                        }
                    }
                    else
                    {
                        api.apiSetConfig("EcuPath", Path.GetFullPath(Path.Combine(baseDirectory, pathString)));
                        if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleCommunication.ECUKom.SetEcuPathSGCLIB", defaultValue: true))
                        {
                            Environment.SetEnvironmentVariable("SGCLIB_EDIABAS_ECU_PATH", Path.Combine(baseDirectory, pathString));
                        }
                    }
                    api.apiGetConfig("EcuPath", out var cfgValue);
                    if (logging)
                    {
                        Log.Info("ECUKom.SetEcuPath()", "Used EcuPath: {0}", cfgValue);
                    }
                }
                else if (logging)
                {
                    Log.Info("ECUKom.SetEcuPath()", "no config for specific ecu path used; using default values from ediabas config");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ECUKom.SetEcuPath()", exception);
            }
        }

        public static string APIFormatName(int resultFormat)
        {
            switch (resultFormat)
            {
                case 0:
                    return "Ediabas: API.APIFORMAT_CHAR <=> C#: char";
                case 1:
                    return "Ediabas: API.APIFORMAT_BYTE <=> C#: Byte";
                case 2:
                    return "Ediabas: API.APIFORMAT_INTEGER <=> C#: short";
                case 3:
                    return "Ediabas: API.APIFORMAT_WORD <=> C# ushort";
                case 4:
                    return "Ediabas: API.APIFORMAT_LONG <=> C#: int";
                case 5:
                    return "Ediabas: API.APIFORMAT_DWORD <=> C#: uint";
                case 6:
                    return "Ediabas: API.APIFORMAT_TEXT <=> C# String";
                case 8:
                    return "Ediabas: API.APIFORMAT_REAL <=> C#: double";
                case 7:
                    return "Ediabas: API.APIFORMAT_BINARY <=> C# Byte[]";
                default:
                    return "Ediabas: UNKNOWN !!!!";
            }
        }

        public static ECUKom DeSerialize(string filename, EdiabasNet ediabas = null)
        {
            Log.Info("ECUKom.DeSerialize()", "called");
            ECUKom eCUKom;
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(filename);
                eCUKom = (ECUKom)new XmlSerializer(typeof(ECUKom)).Deserialize(xmlTextReader);
                xmlTextReader.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.DeSerialize()", exception);
                eCUKom = new ECUKom("Rheingold");
            }
            VCIDevice vCIDevice = new VCIDevice(VCIDeviceType.SIM, "SIM", filename);
            vCIDevice.Serial = filename;
            vCIDevice.IPAddress = "127.0.0.1";
            eCUKom.VCI = vCIDevice;
            eCUKom.CommunicationMode = CommMode.Simulation;
            try
            {
                if (eCUKom.jobList != null)
                {
                    Log.Info("ECUKom.DeSerialize()", "got {0} jobs from simulation container", eCUKom.jobList.Count);
                    foreach (ECUJob job in eCUKom.jobList)
                    {
                        job.JobName = job.JobName.ToUpper(CultureInfo.InvariantCulture);
                        job.EcuName = job.EcuName.ToUpper(CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUKom.DeSerialize()", "failed to normalize EcuName and JobName tu uppercase with exception: {0}", ex.ToString());
            }
            Log.Info("ECUKom.DeSerialize()", "successfully done");
            return eCUKom;
        }

        public static string EdiabasBinPath()
        {
            try
            {
                string[] array = Environment.GetEnvironmentVariable("PATH").Split(';');
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api32.dll")))
                {
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
                string[] array2 = array;
                foreach (string text in array2)
                {
                    if (File.Exists(Path.Combine(text, "api32.dll")))
                    {
                        return text;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.EdiabasBinPath()", exception);
            }
            return null;
        }

        public static string EdiabasIniFilePath(string iniFilename)
        {
            try
            {
                string path = EdiabasBinPath();
                if (File.Exists(Path.Combine(path, iniFilename)))
                {
                    return Path.Combine(path, iniFilename);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.EdiabasIniFilePath()", exception);
            }
            return null;
        }

        public static bool Serialize(string filename, ECUKom ecuKom, Encoding encType)
        {
            Log.Info("ECUKom.Serialize()", "called");
            try
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(filename, encType);
                new XmlSerializer(typeof(ECUKom)).Serialize(xmlTextWriter, ecuKom);
                xmlTextWriter.Close();
                Log.Info("ECUKom.Serialize()", "successfully done");
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.Serialize()", exception);
            }
            Log.Warning("ECUKom.Serialize()", "failed when writing ECUKom data");
            return false;
        }

        public string GetLogPath()
        {
            string result = null;
            try
            {
                if (api.apiGetConfig("TracePath", out string value))
                {
                    result = value;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.getLogPath()", exception);
            }
            return result;
        }

        public bool InitVCI(IVciDevice device, bool logging, bool isDoIP)
        {
            bool flag = false;
            bool flag2 = false;
            if (device == null)
            {
                Log.Warning("ECUKom.InitVCI()", "failed because device was null");
                return false;
            }
            bool isDoIP2 = device.IsDoIP;
            try
            {
                string pathString = ConfigSettings.getPathString("BMW.Rheingold.Logging.Directory.Current", "..\\..\\..\\logs");
                switch (device.VCIType)
                {
                    case VCIDeviceType.ICOM:
                        if (isDoIP2 && flag2)
                        {
                            flag = InitEdiabasForDoIP(device);
                            break;
                        }
                        if (!string.IsNullOrEmpty(device.VIN) && !isDoIP)
                        {
                            flag = api.apiInitExt("RPLUS:ICOM_P:Remotehost=" + device.IPAddress + ";Port=6801", "", "", string.Empty);
                            break;
                        }
                        if (!isDoIP)
                        {
                            flag = api.apiInitExt("RPLUS:ICOM_P:Remotehost=" + device.IPAddress + ";Port=6801", "", "", string.Empty);
                        }
                        if (isDoIP && flag2)
                        {
                            flag = InitEdiabasForDoIP(device);
                        }
                        break;
                    case VCIDeviceType.ENET:
                        if (device.IsDoIP)
                        {
                            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service))
                            {
                                flag = api.apiInitExt("ENET", "_", "Rheingold",  "selectCertificate=" + service.CertificateFilePathWithoutEnding + ";remotehost=" + device.IPAddress );
                            }
                        }
                        else
                        {
                            flag = api.apiInitExt("ENET", "_", "Rheingold", "remotehost=" + device.IPAddress);
                        }
                        break;
                    case VCIDeviceType.TELESERVICE:
                        {
                            string text = string.Format(CultureInfo.InvariantCulture, "CompoundID={0};UsePdmResult={1}", device.DevId, device.UsePdmResult ? "true" : "false");
                            Log.Info("ECUKom.InitVCI()", "calling TELESERVICE api init with parameter: {0}", text);
                            flag = api.apiInitExt("TELE", "_", "Rheingold", text);
                            break;
                        }
                    case VCIDeviceType.PTT:
                        flag = false;
                        break;
                    case VCIDeviceType.SIM:
                        flag = true;
                        break;
                    default:
                        flag = api.apiInit();
                        break;
                }
                if (api.apiErrorCode() != 0)
                {
                    Log.Warning("ECUKom.InitVCI()", "failed when init IFH with : {0} / {1}", api.apiErrorCode(), api.apiErrorText());
                }
                api.apiSetConfig("TracePath", Path.GetFullPath(pathString));
                if (flag)
                {
                    vci = device as VCIDevice;
                    if (logging)
                    {
                        api.apiGetConfig("TracePath", out var cfgValue);
                        Log.Info("ECUKom.InitVCI()", "Ediabas TracePath is loaded: {0}", cfgValue);
                        api.apiGetConfig("EdiabasVersion", out var cfgValue2);
                        Log.Info("ECUKom.InitVCI()", "Ediabas version loaded: {0}", cfgValue2);
                        api.apiGetConfig("IfhVersion", out var cfgValue3);
                        Log.Info("ECUKom.InitVCI()", "IfhVersion version loaded: {0}", cfgValue3);
                        api.apiGetConfig("Session", out var cfgValue4);
                        Log.Info("ECUKom.InitVCI()", "Session name: {0}", cfgValue4);
                        api.apiGetConfig("Interface", out var cfgValue5);
                        Log.Info("ECUKom.InitVCI()", "Interface type loaded: {0}", cfgValue5);
                    }
                    SetEcuPath(logging);
                }
                return flag;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.InitVCI()", exception);
            }
            return true;
        }

        private bool CheckAuthentificationState(bool doip)
        {
            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var _) && doip)
            {
                IEcuJob ecuJob = ApiJob("IPB_APP2", "STATUS_LESEN", "ARG;SEC4DIAG_READ_AUTH_MODE");
                if (ecuJob.IsOkay())
                {
                    ecuJob.getuintResult("STAT_ROLL_MASK_WERT");
                    ecuJob.getStringResult("AuthenticationReturnParameter_TEXT");
                    return true;
                }
            }
            return false;
        }

        private void CreateEdiabasPublicKeyIfNotExist(IVciDevice device)
        {
            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service) && service.CheckIfEdiabasPublicKeyExists())
            {
                return;
            }
            string reserved = string.Empty;
            switch (device.VCIType)
            {
                case VCIDeviceType.ICOM:
                    reserved = $"RemoteHost={device.IPAddress};DiagnosticPort={50160};ControlPort={50161};PortDoIP={50162};";
                    break;
                case VCIDeviceType.ENET:
                    reserved = "RemoteHost=" + device.IPAddress;
                    break;
                case VCIDeviceType.PTT:
                    reserved = "RPLUS:ICOM_P:remotehost=127.0.0.1;Port=6408";
                    break;
            }
            if (ApiInitExt("ENET", "_", "Rheingold", reserved))
            {
                SetEcuPath(logging: false);
                ApiJob("IPB_APP2", "IDENT", string.Empty, string.Empty);
                while (api.apiState() == 0)
                {
                    SleepUtility.TaskDelay(200, "ECUKom.CreateEdiabasPubglickeyIfNotExist - IPB_APP2, IDENT").GetAwaiter().GetResult();
                }
                api.apiSetConfig("EDIABASUnload", "1");
                api.apiEnd();
            }
        }

        private bool HandleS29Authentication(IVciDevice device)
        {
            try
            {
                if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service) && ServiceLocator.Current.TryGetService<IBackendCallsWatchDog>(out var service2))
                {
                    service.EdiabasPublicKey = service.GetPublicKeyFromEdiabas();
                    string configString = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.Ca", string.Empty);
                    string configString2 = ConfigSettings.getConfigString("BMW.Rheingold.CoreFramework.Ediabas.Thumbprint.SubCa", string.Empty);
                    if (string.IsNullOrEmpty(configString) || string.IsNullOrEmpty(configString2))
                    {
                        return RequestCaAndSubCACertificates(device, service, service2);
                    }
                    X509Certificate2Collection subCaCertificate = new X509Certificate2Collection();
                    X509Certificate2Collection caCertificate = new X509Certificate2Collection();
                    if (!service.SearchForCertificatesInWindowsStore(configString, configString2, out subCaCertificate, out caCertificate) || subCaCertificate.Count == 0 || caCertificate.Count == 0)
                    {
                        return RequestCaAndSubCACertificates(device, service, service2);
                    }
                    if (subCaCertificate.Count == 1 || caCertificate.Count == 1)
                    {
                        service.CertificatesAreFoundAndValid(device, subCaCertificate, caCertificate);
                        return true;
                    }
                    return false;
                }
                Log.Error("HandleS29Authentication", "ISec4DiagHandler or IBackendCallsWatchDog not found");
                return false;
            }
            catch (Exception exception)
            {
                Log.ErrorException("HandleS29Authentication", exception);
                return false;
            }
        }

        private bool RequestCaAndSubCACertificates(IVciDevice device, ISec4DiagHandler sec4DiagHandler, IBackendCallsWatchDog backendCallsWatchDog)
        {
            if (ServiceLocator.Current.TryGetService<IDataContext>(out var service))
            {
                WebCallResponse<Sec4DiagResponseData> webCallResponse = Sec4DiagProcessorFactory.Create(backendCallsWatchDog).SendDataToBackend(sec4DiagHandler.BuildRequestModel(device.VIN), BackendServiceType.Sec4Diag, service.AccessToken);
                if (webCallResponse.IsSuccessful)
                {
                    sec4DiagHandler.CreateS29CertificateInstallCertificatesAndWriteToFile(device, webCallResponse.Response.Certificate, webCallResponse.Response.CertificateChain[0]);
                    return true;
                }
                return false;
            }
            throw new InvalidOperationException("IDataContext service not found.");
        }

        private bool InitEdiabasForDoIP(IVciDevice device)
        {
            if (ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service))
            {
                string reserved = $"RemoteHost={device.IPAddress};DiagnosticPort={50160};ControlPort={50161};PortDoIP={50162};selectCertificate={service.CertificateFilePathWithoutEnding};SSLPort={50163}";
                return ApiInitExt("ENET", "_", "Rheingold", reserved);
            }
            return false;
        }

        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding)
        {
            return apiJob(ecu, job, param, resultFilter, cacheAdding);
        }

        // [UH] added default values
        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter = "", int retries = 0, bool fastaActive = true)
        {
            return (retries == 0) ? apiJob(ecu, job, param, resultFilter) : apiJob(ecu, job, param, resultFilter, retries, 0);
        }

        public IEcuJob ApiJob(string ecu, string job, string param, int retries, int millisecondsTimeout)
        {
            return (retries == 0) ? apiJob(ecu, job, param, string.Empty) : apiJob(ecu, job, param, string.Empty, retries, millisecondsTimeout);
        }

        public ECUJob apiJob(string variant, string job, string param, string resultFilter, int retries, string sgbd = "")
        {
            if (FromFastaConfig && !string.IsNullOrEmpty(sgbd) && apiJobNamesToBeCached.Contains(job))
            {
                ECUJob jobFromCache = GetJobFromCache(sgbd, job, param, resultFilter);
                if (jobFromCache != null && (CommunicationMode == CommMode.Simulation || (jobFromCache.JobErrorCode != 0 && jobFromCache.JobResult != null && jobFromCache.JobResult.Count > 0)))
                {
                    return jobFromCache;
                }
            }
            return apiJob(variant, job, param, resultFilter, retries, 0);
        }

        public ECUJob apiJob(string ecu, string jobName, string param, string resultFilter, int retries, int millisecondsTimeout)
        {
            if (retries > 5)
            {
                Log.Warning("ECUKom.apiJob()", "Number of retries is set to {0}.", retries);
            }
            try
            {
                ECUJob eCUJob = apiJob(ecu, jobName, param, resultFilter);
                if (eCUJob.JobErrorCode == 98)
                {
                    return eCUJob;
                }
                ushort num = 1;
                while (num < retries && !eCUJob.IsDone())
                {
                    SleepUtility.ThreadSleep(millisecondsTimeout, "ECUKom.apiJob - " + ecu + ", " + jobName + ", " + param);
                    Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJob()", "(Sgbd: {0}, {1}) - is retrying {2} times", ecu, jobName, num);
                    eCUJob = apiJob(ecu, jobName, param, resultFilter);
                    num++;
                }
                return eCUJob;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.apiJob()", exception);
                ECUJob eCUJob = new ECUJob();
                eCUJob.EcuName = ecu;
                eCUJob.ExecutionStartTime = DateTime.Now;
                eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
                eCUJob.JobName = jobName;
                eCUJob.JobParam = param;
                eCUJob.JobResultFilter = resultFilter;
                eCUJob.JobErrorCode = 90;
                eCUJob.JobErrorText = "SYS-0000: INTERNAL ERROR";
                eCUJob.JobResult = new List<ECUResult>();
                AddJobInCache(eCUJob);
                return eCUJob;
            }
        }

        public ECUJob apiJob(string ecu, string job, string param, string resultFilter)
        {
            try
            {
                ECUJob eCUJob = null;
                if (string.Compare(ecu, "FA", StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(job, "FA_STREAM2STRUCT", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    int len;
                    byte[] param2 = FormatConverter.Ascii2ByteArray(param, out len);
                    eCUJob = apiJobData("FA", "FA_STREAM2STRUCT", param2, len, string.Empty);
                }
                else
                {
                    eCUJob = apiJob(ecu, job, param, resultFilter, cacheAdding: true);
                }
                return eCUJob;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.apiJob()", exception);
            }
            ECUJob eCUJob2 = new ECUJob();
            eCUJob2.EcuName = ecu;
            eCUJob2.JobName = job;
            eCUJob2.JobParam = param;
            eCUJob2.ExecutionStartTime = DateTime.Now;
            eCUJob2.ExecutionEndTime = eCUJob2.ExecutionStartTime;
            eCUJob2.JobErrorCode = 91;
            eCUJob2.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
            eCUJob2.JobResult = new List<ECUResult>();
            if (VehicleCommunication.DebugLevel > 2)
            {
                ECUJob.Dump(eCUJob2);
            }
            return eCUJob2;
        }

        public ECUJob apiJob(string ecu, string jobName, string param, string resultFilter, bool cacheAdding)
        {
            //TimeMetricsUtility.Instance.ApiJobStart(ecu, jobName, param, -1);
            try
            {
                if (string.IsNullOrEmpty(ecu))
                {
                    ECUJob obj = new ECUJob()
                    {
                        EcuName = string.Empty,
                        JobName = jobName,
                        JobParam = param,
                        ExecutionStartTime = DateTime.Now
                    };
                    obj.ExecutionEndTime = obj.ExecutionStartTime;
                    obj.JobErrorCode = 91;
                    obj.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
                    obj.JobResult = new List<ECUResult>();
                    return obj;
                }
                if (param == null)
                {
                    param = string.Empty;
                }
                if (resultFilter == null)
                {
                    resultFilter = string.Empty;
                }
                if (communicationMode == CommMode.Simulation)
                {
                    ECUJob eCUJob = ApiJobSim(ecu, jobName, param, resultFilter);
                    if (eCUJob != null)
                    {
                        return eCUJob;
                    }
                    ECUJob obj2 = new ECUJob()
                    {
                        EcuName = ecu,
                        JobName = jobName,
                        JobParam = param,
                        JobResultFilter = resultFilter,
                        ExecutionStartTime = DateTime.Now
                    };
                    obj2.ExecutionEndTime = obj2.ExecutionStartTime;
                    obj2.JobErrorCode = 19;
                    obj2.JobErrorText = "IFH-0009: NO RESPONSE FROM CONTROLUNIT";
                    obj2.JobResult = new List<ECUResult>();
                    return obj2;
                }
                if (communicationMode == CommMode.CacheFirst && cacheAdding)
                {
                    lastJobExecution = DateTime.MinValue;
                    ECUJob eCUJob2 = ApiJobSim(ecu, jobName, param, resultFilter);
                    if (eCUJob2 != null && eCUJob2.JobErrorCode == 0 && eCUJob2.JobResult != null && eCUJob2.JobResult.Count > 0)
                    {
                        return eCUJob2;
                    }
                }
                DateTimePrecise dateTimePrecise = new DateTimePrecise(10L);
                int num = 0;
                ECUJob eCUJob3 = new ECUJob();
                eCUJob3.EcuName = ecu;
                eCUJob3.ExecutionStartTime = dateTimePrecise.Now;
                eCUJob3.JobName = jobName;
                eCUJob3.JobParam = param;
                eCUJob3.JobResultFilter = resultFilter;
                eCUJob3.JobResult = new List<ECUResult>();
                try
                {
                    api.apiJob(ecu, jobName, param, resultFilter);
                    while (api.apiStateExt(1000) == 0)
                    {
                        SleepUtility.ThreadSleep(2, "ECUKom.apiJob - " + ecu + ", " + jobName + ", " + param);
                    }
                    num = (eCUJob3.JobErrorCode = api.apiErrorCode());
                    eCUJob3.JobErrorText = api.apiErrorText();
                    api.apiResultSets(out var rsets);
                    eCUJob3.JobResultSets = rsets;
                    if (rsets > 0)
                    {
                        Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJob()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - successfully called: {4}:{5} RSets: {6}", ecu, jobName, param, resultFilter, eCUJob3.JobErrorCode, eCUJob3.JobErrorText, rsets);
                        for (ushort num3 = 0; num3 <= rsets; num3++)
                        {
                            if (api.apiResultNumber(out var buffer, num3))
                            {
                                for (ushort num4 = 1; num4 <= buffer; num4++)
                                {
                                    ECUResult eCUResult = new ECUResult();
                                    string buffer2 = string.Empty;
                                    eCUResult.Set = num3;
                                    if (api.apiResultName(out buffer2, num4, num3))
                                    {
                                        eCUResult.Name = buffer2;
                                        if (api.apiResultFormat(out var buffer3, buffer2, num3))
                                        {
                                            eCUResult.Format = buffer3;
                                            switch (buffer3)
                                            {
                                                case 1:
                                                    {
                                                        api.apiResultByte(out var buffer10, buffer2, num3);
                                                        eCUResult.Value = buffer10;
                                                        break;
                                                    }
                                                case 0:
                                                    {
                                                        api.apiResultChar(out var buffer11, buffer2, num3);
                                                        eCUResult.Value = buffer11;
                                                        break;
                                                    }
                                                case 5:
                                                    {
                                                        api.apiResultDWord(out var buffer12, buffer2, num3);
                                                        eCUResult.Value = buffer12;
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        api.apiResultInt(out var buffer9, buffer2, num3);
                                                        eCUResult.Value = buffer9;
                                                        break;
                                                    }
                                                case 4:
                                                    {
                                                        api.apiResultLong(out var buffer6, buffer2, num3);
                                                        eCUResult.Value = buffer6;
                                                        break;
                                                    }
                                                case 8:
                                                    {
                                                        api.apiResultReal(out var buffer7, buffer2, num3);
                                                        eCUResult.Value = buffer7;
                                                        break;
                                                    }
                                                case 6:
                                                    {
                                                        api.apiResultText(out string buffer8, buffer2, num3, string.Empty);
                                                        eCUResult.Value = buffer8;
                                                        break;
                                                    }
                                                case 3:
                                                    {
                                                        api.apiResultWord(out var buffer5, buffer2, num3);
                                                        eCUResult.Value = buffer5;
                                                        break;
                                                    }
                                                case 7:
                                                    {
                                                        uint bufferLen2;
                                                        if (api.apiResultBinary(out var buffer4, out var bufferLen, buffer2, num3))
                                                        {
                                                            if (buffer4 != null)
                                                            {
                                                                Array.Resize(ref buffer4, bufferLen);
                                                            }
                                                            eCUResult.Value = buffer4;
                                                            eCUResult.Length = bufferLen;
                                                        }
                                                        else if (api.apiResultBinaryExt(out buffer4, out bufferLen2, 65536u, buffer2, num3))
                                                        {
                                                            if (buffer4 != null)
                                                            {
                                                                Array.Resize(ref buffer4, (int)bufferLen2);
                                                            }
                                                            eCUResult.Value = buffer4;
                                                            eCUResult.Length = bufferLen2;
                                                        }
                                                        else
                                                        {
                                                            eCUResult.Value = new byte[0];
                                                            eCUResult.Length = 0u;
                                                        }
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        api.apiResultVar(out var var);
                                                        eCUResult.Value = var;
                                                        break;
                                                    }
                                            }
                                        }
                                        eCUJob3.JobResult.Add(eCUResult);
                                    }
                                    else
                                    {
                                        buffer2 = string.Format(CultureInfo.InvariantCulture, "ResName unknown! Job was: {0} result index: {1} set index{2}", jobName, num4, num3);
                                    }
                                }
                            }
                        }
                    }
                    if (num != 0)
                    {
                        Log.Info("ECUKom.apiJob()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with apiError: {4}:{5}", ecu, jobName, param, resultFilter, eCUJob3.JobErrorCode, eCUJob3.JobErrorText);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Log.Warning("ECUKom.apiJob()", "buggy sgbd ({0}, {1}, {2}, {3}) apiError: {4} found; wrong result set length was set", ecu, jobName, param, resultFilter, eCUJob3.JobErrorText);
                }
                catch (Exception exception)
                {
                    Log.WarningException("ECUKom.apiJob()", exception);
                }
                eCUJob3.ExecutionEndTime = dateTimePrecise.Now;
                AddJobInCache(eCUJob3, cacheAdding);
                return eCUJob3;
            }
            finally
            {
                //TimeMetricsUtility.Instance.ApiJobEnd(ecu, jobName, param, -1);
            }
        }

        public IEcuJob ApiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter = "", int retries = 0)
        {
            if (retries == 0)
            {
                return apiJobData(ecu, job, param, paramlen, resultFilter);
            }
            return apiJobData(ecu, job, param, paramlen, resultFilter, retries);
        }

        public ECUJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter, int retries)
        {
            try
            {
                ECUJob eCUJob = apiJobData(ecu, job, param, paramlen, resultFilter);
                if (eCUJob.JobErrorCode == 98)
                {
                    return eCUJob;
                }
                ushort num2 = 0;
                while (num2 < retries && !eCUJob.IsDone())
                {
                    Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJobData()", "(Sgbd: {0}, {1}) - is retrying {2} times", ecu, job, num2);
                    eCUJob = apiJobData(ecu, job, param, paramlen, resultFilter);
                    num2 = (ushort)(num2 + 1);
                }
                return eCUJob;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.apiJobData()", exception);
                ECUJob eCUJob = new ECUJob();
                eCUJob.EcuName = ecu;
                eCUJob.JobName = job;
                eCUJob.JobParam = FormatConverter.ByteArray2String(param, (uint)paramlen);
                eCUJob.JobResultFilter = resultFilter;
                eCUJob.ExecutionStartTime = DateTime.Now;
                eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
                eCUJob.JobErrorCode = 90;
                eCUJob.JobErrorText = "SYS-0000: INTERNAL ERROR";
                eCUJob.JobResult = new List<ECUResult>();
                AddJobInCache(eCUJob);
                return eCUJob;
            }
        }

        public ECUJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter)
        {
            //TimeMetricsUtility.Instance.ApiJobStart(ecu, job, string.Empty, paramlen);
            try
            {
                if (string.IsNullOrEmpty(ecu))
                {
                    ECUJob obj = new ECUJob
                    {
                        JobName = string.Empty,
                        ExecutionStartTime = DateTime.Now
                    };
                    obj.ExecutionEndTime = obj.ExecutionStartTime;
                    obj.JobErrorCode = 91;
                    obj.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
                    obj.JobResult = new List<ECUResult>();
                    return obj;
                }
                if (!VehicleCommunication.validLicense)
                {
                    throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
                }
                if (param == null)
                {
                    param = new byte[0];
                }
                if (resultFilter == null)
                {
                    resultFilter = string.Empty;
                }
                if (paramlen == -1)
                {
                    paramlen = param.Length;
                }
                if (communicationMode == CommMode.Simulation)
                {
                    try
                    {
                        string param2 = FormatConverter.ByteArray2String(param, (uint)paramlen);
                        ECUJob eCUJob = ApiJobSim(ecu, job, param2, resultFilter);
                        if (eCUJob != null)
                        {
                            return eCUJob;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with exception: {4}", ecu, job, param, resultFilter, ex.ToString());
                    }
                    ECUJob obj2 = new ECUJob
                    {
                        EcuName = ecu,
                        JobName = job,
                        JobParam = FormatConverter.ByteArray2String(param, (uint)paramlen),
                        JobResultFilter = resultFilter,
                        ExecutionStartTime = DateTime.Now
                    };
                    obj2.ExecutionEndTime = obj2.ExecutionStartTime;
                    obj2.JobErrorCode = 19;
                    obj2.JobErrorText = "IFH-0009: NO RESPONSE FROM CONTROLUNIT";
                    obj2.JobResult = new List<ECUResult>();
                    return obj2;
                }
                DateTimePrecise dateTimePrecise = new DateTimePrecise(10L);
                int num = 0;
                ECUJob eCUJob2 = new ECUJob();
                eCUJob2.EcuName = ecu;
                eCUJob2.JobName = job;
                eCUJob2.ExecutionStartTime = dateTimePrecise.Now;
                eCUJob2.ExecutionEndTime = eCUJob2.ExecutionStartTime;
                eCUJob2.JobResultFilter = resultFilter;
                eCUJob2.JobResult = new List<ECUResult>();
                try
                {
                    eCUJob2.JobParam = FormatConverter.ByteArray2String(param, (uint)paramlen);
                    api.apiJobData(ecu, job, param, paramlen, resultFilter);
                    while (api.apiStateExt(1000) == 0)
                    {
                        SleepUtility.ThreadSleep(2, "ECUKom.apiJob - " + ecu + ", " + job + ", byte[]");
                    }
                    num = (eCUJob2.JobErrorCode = api.apiErrorCode());
                    eCUJob2.JobErrorText = api.apiErrorText();
                    if (num == 0)
                    {
                        Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - successfully called: {4}:{5}", ecu, job, param, resultFilter, eCUJob2.JobErrorCode, eCUJob2.JobErrorText);
                        if (api.apiResultSets(out var rsets))
                        {
                            eCUJob2.JobResultSets = rsets;
                            for (ushort num3 = 0; num3 <= rsets; num3++)
                            {
                                if (api.apiResultNumber(out var buffer, num3))
                                {
                                    for (ushort num4 = 1; num4 <= buffer; num4++)
                                    {
                                        ECUResult eCUResult = new ECUResult();
                                        eCUResult.Set = num3;
                                        if (api.apiResultName(out var buffer2, num4, num3))
                                        {
                                            eCUResult.Name = buffer2;
                                            if (api.apiResultFormat(out var buffer3, buffer2, num3))
                                            {
                                                eCUResult.Format = buffer3;
                                                switch (buffer3)
                                                {
                                                    case 1:
                                                        {
                                                            api.apiResultByte(out var buffer10, buffer2, num3);
                                                            eCUResult.Value = buffer10;
                                                            break;
                                                        }
                                                    case 0:
                                                        {
                                                            api.apiResultChar(out var buffer11, buffer2, num3);
                                                            eCUResult.Value = buffer11;
                                                            break;
                                                        }
                                                    case 5:
                                                        {
                                                            api.apiResultDWord(out var buffer12, buffer2, num3);
                                                            eCUResult.Value = buffer12;
                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            api.apiResultInt(out var buffer9, buffer2, num3);
                                                            eCUResult.Value = buffer9;
                                                            break;
                                                        }
                                                    case 4:
                                                        {
                                                            api.apiResultLong(out var buffer6, buffer2, num3);
                                                            eCUResult.Value = buffer6;
                                                            break;
                                                        }
                                                    case 8:
                                                        {
                                                            api.apiResultReal(out var buffer7, buffer2, num3);
                                                            eCUResult.Value = buffer7;
                                                            break;
                                                        }
                                                    case 6:
                                                        {
                                                            api.apiResultText(out string buffer8, buffer2, num3, string.Empty);
                                                            eCUResult.Value = buffer8;
                                                            break;
                                                        }
                                                    case 3:
                                                        {
                                                            api.apiResultWord(out var buffer5, buffer2, num3);
                                                            eCUResult.Value = buffer5;
                                                            break;
                                                        }
                                                    case 7:
                                                        {
                                                            uint bufferLen2;
                                                            if (api.apiResultBinary(out var buffer4, out var bufferLen, buffer2, num3))
                                                            {
                                                                if (buffer4 != null)
                                                                {
                                                                    Array.Resize(ref buffer4, bufferLen);
                                                                }
                                                                eCUResult.Value = buffer4;
                                                                eCUResult.Length = bufferLen;
                                                            }
                                                            else if (api.apiResultBinaryExt(out buffer4, out bufferLen2, 65536u, buffer2, num3))
                                                            {
                                                                if (buffer4 != null)
                                                                {
                                                                    Array.Resize(ref buffer4, (int)bufferLen2);
                                                                }
                                                                eCUResult.Value = buffer4;
                                                                eCUResult.Length = bufferLen2;
                                                            }
                                                            else
                                                            {
                                                                eCUResult.Value = new byte[0];
                                                                eCUResult.Length = 0u;
                                                            }
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            api.apiResultVar(out var var);
                                                            eCUResult.Value = var;
                                                            break;
                                                        }
                                                }
                                            }
                                            eCUJob2.JobResult.Add(eCUResult);
                                        }
                                        else
                                        {
                                            buffer2 = string.Format(CultureInfo.InvariantCulture, "ResName unknown! Job was: {0} result index: {1} set index{2}", job, num4, num3);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with apiError: {4}:{5}", ecu, job, param, resultFilter, eCUJob2.JobErrorCode, eCUJob2.JobErrorText);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Log.Warning("ECUKom.apiJobData()", "buggy sgbd ({0}, {1}, {2}, {3}) apiError: {4} found; wrong result set length was set", ecu, job, param, resultFilter, eCUJob2.JobErrorText);
                }
                catch (Exception ex3)
                {
                    Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with exception: {4}", ecu, job, param, resultFilter, ex3.ToString());
                }
                eCUJob2.ExecutionEndTime = dateTimePrecise.Now;
                AddJobInCache(eCUJob2);
                return eCUJob2;
            }
            finally
            {
                //TimeMetricsUtility.Instance.ApiJobEnd(ecu, job, string.Empty, paramlen);
            }
        }

        public int getErrorCode()
        {
            return api.apiErrorCode();
        }

        public string getErrorText()
        {
            return api.apiErrorText();
        }

        public int getState(int suspendTime)
        {
            return api.apiStateExt(suspendTime);
        }

        public bool setConfig(string cfgName, string cfgValue)
        {
            return api.apiSetConfig(cfgName, cfgValue);
        }

        public ECUJob GetJobFromCache(string ecuName, string jobName, string jobParam, string jobResultFilter)
        {
            Log.Info("ECUKom.GetJobFromCache()", "Try retrieve from Cache: EcuName:{0}, JobName:{1}, JobParam:{2})", ecuName, jobName, jobParam);
            if (!ecuJobDictionary.ContainsKey(ecuName + "-" + jobName))
            {
                ecuJobDictionary.Add(ecuName + "-" + jobName, new List<ECUJob>());
            }
            try
            {
                IEnumerable<ECUJob> enumerable = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobResultFilter, jobResultFilter, StringComparison.OrdinalIgnoreCase) && job.ExecutionStartTime > lastJobExecution);
                if (enumerable != null && enumerable.Count() > 0)
                {
                    return RetrieveEcuJob(enumerable, ecuName, jobName);
                }
                IEnumerable<ECUJob> enumerable2 = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase) && job.ExecutionStartTime > lastJobExecution);
                if (enumerable2 != null && enumerable2.Count() > 0)
                {
                    return RetrieveEcuJob(enumerable2, ecuName, jobName);
                }
                IEnumerable<ECUJob> enumerable3 = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobResultFilter, jobResultFilter, StringComparison.OrdinalIgnoreCase));
                if (enumerable3 != null && enumerable3.Count() > 0)
                {
                    return RetrieveEcuJobNoExecTime(enumerable3, ecuName, jobName);
                }
                IEnumerable<ECUJob> enumerable4 = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase));
                if (enumerable4 != null && enumerable4.Count() > 0)
                {
                    return RetrieveEcuJobNoExecTime(enumerable4, ecuName, jobName);
                }
                CacheMissCounter++;
            }
            catch (Exception ex)
            {
                Log.Warning("ECUKom.GetJobFromCache()", "job {0},{1}) - failed with exception {2}", ecuName, jobName, ex.ToString());
            }
            Log.Info("ECUKom.GetJobFromCache()", "No result! EcuName:{0}, JobName:{1}, JobParam:{2})", ecuName, jobName, jobParam);
            return null;
        }

        public IEcuJob ExecuteJobOverEnet(string icomAddress, string ecu, string job, string param, bool isDoIP, string resultFilter = "", int retries = 0)
        {
            string method = "ExecuteJobOverEnet";
            Log.Info(method, "Before End");
            End();
            Log.Info(method, "After End");
            IEcuJob result = ExecuteJobOverEnetWrapper(icomAddress, ecu, job, param, isDoIP, method, resultFilter, retries);
            RefreshEdiabasConnection(isDoIP);
            Log.Info(method, "After valid Refresh");
            return result;
        }

        public IEcuJob ExecuteJobOverEnetActivateDHCP(string icomAddress, string ecu, string job, string param, bool isDoIP, string resultFilter = "", int retries = 0)
        {
            string method = "ExecuteJobOverEnet";
            End();
            IEcuJob result = ExecuteJobOverEnetWrapper(icomAddress, ecu, job, param, isDoIP, method, resultFilter, retries);
            End();
            Log.Info(method, "After API End");
            return result;
        }

        private IEcuJob ExecuteJobOverEnetWrapper(string icomAddress, string ecu, string job, string param, bool isDoIP, string method, string resultFilter = "", int retries = 0)
        {
            string istaLogPath = GetIstaLogPath();
            if (string.IsNullOrEmpty(istaLogPath))
            {
                Log.Warning("EdiabasUtils.ExecuteJobOverEnet()", "Path to ista log cannot be found.");
                return null;
            }
            string cfgValue = (IsProblemHandlingTraceRunning ? "5" : "0");
            Log.Info(method, "Before ApiInitExt");
            string reserved = $"RemoteHost={icomAddress};DiagnosticPort={51560};ControlPort={51561};PortDoIP={51562};";
            bool num = ApiInitExt("ENET", "_", "Rheingold", reserved);
            api.apiSetConfig("ApiTrace", cfgValue);
            api.apiSetConfig("TracePath", Path.GetFullPath(istaLogPath));
            Log.Info(method, "After ApiInitExt");
            if (!num)
            {
                Log.Warning("EdiabasUtils.ExecuteJobOverEnet()", "Failed switching to ENET. The Job will not be executed. The EDIABAS connection will be refreshed.");
                Log.Info(method, "Before invalid refresh");
                RefreshEdiabasConnection(isDoIP);
                Log.Info(method, "After invalid refresh");
                return null;
            }
            SetEcuPath(logging: true);
            Log.Info(method, "Before ApiJob");
            IEcuJob ecuJob = ApiJob(ecu, job, param, resultFilter, retries);
            Log.Info(method, $"After ApiJob, ECode: {ecuJob.JobErrorCode}, EText: {ecuJob.JobErrorText}");
            Log.Info(method, "Before valid Refresh");
            return ecuJob;
        }

        private void RefreshEdiabasConnection(bool isDoIp)
        {
            if (Refresh(isDoIp))
            {
                Log.Info("EdiabasUtils.RefreshEdiabasConnection()", "Successfully connected to current VCI device.");
            }
            else
            {
                Log.Error("EdiabasUtils.RefreshEdiabasConnection()", "Failed to connect to current VCI device!");
            }
        }

        private static string GetIstaLogPath()
        {
            string result = string.Empty;
            try
            {
                result = Path.GetFullPath(ConfigSettings.getPathString("BMW.Rheingold.Logging.Directory.Current", "..\\..\\..\\logs"));
            }
            catch (Exception ex)
            {
                Log.Warning("EdiabasUtils.GetIstaLogPath()", "Exception occurred: ", ex);
            }
            return result;
        }

        private ECUJob RetrieveEcuJobNoExecTime(IEnumerable<ECUJob> query, string ecuName, string jobName)
        {
            foreach (ECUJob item in query)
            {
                if (!ecuJobDictionary[ecuName + "-" + jobName].Contains(item))
                {
                    if (item.ExecutionStartTime > lastJobExecution)
                    {
                        lastJobExecution = GetLastExecutionTime(item.ExecutionStartTime);
                    }
                    ecuJobDictionary[ecuName + "-" + jobName].Add(item);
                    Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUKom.GetJobFromCache()", "4th try: found job {0}/{1}/{2}/{3}/{4} at {5}", item.EcuName, item.JobName, item.JobParam, item.JobErrorCode, item.JobErrorText, item.ExecutionStartTime);
                    CacheHitCounter++;
                    return item;
                }
            }
            ecuJobDictionary[ecuName + "-" + jobName].Clear();
            ecuJobDictionary[ecuName + "-" + jobName].Add(query.First());
            if (query.First().ExecutionStartTime > lastJobExecution)
            {
                lastJobExecution = GetLastExecutionTime(query.First().ExecutionStartTime);
            }
            Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUKom.GetJobFromCache()", "1st try: found job {0}/{1}/{2}/{3}/{4} at {5}", query.First().EcuName, query.First().JobName, query.First().JobParam, query.First().JobErrorCode, query.First().JobErrorText, query.First().ExecutionStartTime);
            CacheHitCounter++;
            return query.First();
        }

        private ECUJob RetrieveEcuJob(IEnumerable<ECUJob> query, string ecuName, string jobName)
        {
            foreach (ECUJob item in query)
            {
                if (!ecuJobDictionary[ecuName + "-" + jobName].Contains(item))
                {
                    ecuJobDictionary[ecuName + "-" + jobName].Add(item);
                    lastJobExecution = GetLastExecutionTime(item.ExecutionStartTime);
                    Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUKom.GetJobFromCache()", "1st try: found job {0}/{1}/{2}/{3}/{4} at {5}", item.EcuName, item.JobName, item.JobParam, item.JobErrorCode, item.JobErrorText, item.ExecutionStartTime);
                    CacheHitCounter++;
                    return item;
                }
            }
            ecuJobDictionary[ecuName + "-" + jobName].Clear();
            ecuJobDictionary[ecuName + "-" + jobName].Add(query.First());
            lastJobExecution = GetLastExecutionTime(query.First().ExecutionStartTime);
            Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUKom.GetJobFromCache()", "1st try: found job {0}/{1}/{2}/{3}/{4} at {5}", query.First().EcuName, query.First().JobName, query.First().JobParam, query.First().JobErrorCode, query.First().JobErrorText, query.First().ExecutionStartTime);
            CacheHitCounter++;
            return query.First();
        }

        private ECUJob ApiJobSim(string ecu, string job, string param, string result)
        {
            ECUJob eCUJob = null;
            if (!FromFastaConfig)
            {
                Log.Info(Log.CurrentMethod(), "Retrieving ECU " + ecu + " job " + job + " from cache.");
                eCUJob = GetJobFromCache(ecu, job, param, result);
            }
            if (eCUJob != null)
            {
                eCUJob.FASTARelevant = false;
                foreach (ECUResult item in eCUJob.JobResult)
                {
                    item.FASTARelevant = false;
                }
            }
            return eCUJob;
        }

        private DateTime GetLastExecutionTime(DateTime executionStartTime)
        {
            IOrderedEnumerable<ECUJob> source = from job in jobList
                where job.ExecutionStartTime > lastJobExecution
                orderby job.ExecutionStartTime
                select job;
            if (source.FirstOrDefault().ExecutionStartTime < executionStartTime)
            {
                return source.FirstOrDefault().ExecutionStartTime;
            }
            return executionStartTime;
        }

        private void AddJobInCache(ECUJob job, bool cacheCondition = true)
        {
            if (jobList != null && cacheCondition)
            {
                string msg = "Store in Cache: EcuName:" + job.EcuName + ", JobName:" + job.JobName + ", JobParam:" + job.JobParam;
                Log.Info("ECUKom.AddJobInCache()", msg);
                jobList.Add(job);
            }
        }

        private static string GetStack()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                int num2 = 1;
                do
                {
                    MethodBase method = new StackFrame(num2).GetMethod();
                    string name = method.DeclaringType.Name;
                    stringBuilder.Append("-> " + name + "." + method.Name + " ");
                    num2++;
                }
                while (num2 < 10);
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
            return stringBuilder.ToString();
        }

        public ECUJob apiJobWaitWhenPending(string ecu, string job, string param, string resultFilter, double timeout)
        {
            ECUJob eCUJob = null;
            bool flag = false;
            DateTime dateTime = DateTime.Now.AddMilliseconds(timeout);
            try
            {
                do
                {
                    if (!flag && dateTime > DateTime.Now)
                    {
                        eCUJob = apiJob(ecu, job, param, resultFilter);
                        continue;
                    }
                    return eCUJob;
                }
                while (!eCUJob.IsDone() || eCUJob.IsJobState("ERROR_ECU_REQUEST_CORRECTLY_RECEIVED__RESPONSE_PENDING"));
                return eCUJob;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.apiJobWaitWhenPending()", exception);
                eCUJob = new ECUJob();
                eCUJob.EcuName = ecu;
                eCUJob.ExecutionStartTime = DateTime.Now;
                eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
                eCUJob.JobName = job;
                eCUJob.JobParam = param;
                eCUJob.JobResultFilter = resultFilter;
                eCUJob.JobErrorCode = 90;
                eCUJob.JobErrorText = "SYS-0000: INTERNAL ERROR";
                eCUJob.JobResult = new List<ECUResult>();
                AddJobInCache(eCUJob);
                return eCUJob;
            }
        }

        public bool getConfig(string cfgName, out string cfgValue)
        {
            return api.apiGetConfig(cfgName, out cfgValue);
        }

        public int getState()
        {
            return api.apiState();
        }

        public int waitJobDone(int suspendTime)
        {
            return getState(suspendTime);
        }
    }
}
