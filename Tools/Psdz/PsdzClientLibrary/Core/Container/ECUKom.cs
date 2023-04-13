// BMW.Rheingold.VehicleCommunication.ECUKom
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using Ediabas;
using EdiabasLib;
using PsdzClient.Utility;

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

        private ApiInternal api;

        private string _APP;

        private CommMode communicationMode = CommMode.Normal;

        private bool isProblemHandlingTraceRunning;

        private List<string> apiJobNamesToBeCached = new List<string>();

        //private DateTime lastJobExecution;

        private VCIDevice vci;

        private Dictionary<string, List<ECUJob>> ecuJobDictionary = new Dictionary<string, List<ECUJob>>();

        private bool m_FromFastaConfig = false;

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
                    //Log.Info(Log.CurrentMethod(), "Stack: " + GetStack());
                    //Log.Info(Log.CurrentMethod(), $"Setting new value to {value}");
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
                api.apiEnd();
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.End()", exception);
            }
        }

        public string GetEdiabasIniFilePath(string iniFilename)
        {
            return EdiabasIniFilePath(iniFilename);
        }

        public bool InitVCI(IVciDevice vciDevice)
        {
            bool result = InitVCI(vciDevice, logging: true);
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
            int configint = 5;
            int configint2 = 32767;
            api.apiSetConfig("ApiTrace", configint.ToString(CultureInfo.InvariantCulture));
            api.apiSetConfig("TraceSize", configint2.ToString(CultureInfo.InvariantCulture));
            isProblemHandlingTraceRunning = true;
        }

        public bool Refresh()
        {
            End();
            bool result = false;
            try
            {
                result = InitVCI(VCI);
                return result;
            }
            catch (Exception)
            {
                //Log.ErrorException("ECUKom.Refresh()", exception);
                return result;
            }
        }

        public bool ApiInitExt(string ifh, string unit, string app, string reserved)
        {
            return api.apiInitExt(ifh, unit, app, reserved);
        }

        public void SetEcuPath(bool logging)
        {
            try
            {
                string pathString = "..\\..\\..\\Ecu\\";
                if (!string.IsNullOrEmpty(pathString))
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    if (logging)
                    {
                        //Log.Info("ECUKom.SetEcuPath()", "found EcuPath config setting: {0} AppDomain.BaseDirectory: {1}", pathString, baseDirectory);
                    }
                    if (Path.IsPathRooted(pathString))
                    {
                        api.apiSetConfig("EcuPath", Path.GetFullPath(pathString));
                    }
                    else
                    {
                        api.apiSetConfig("EcuPath", Path.GetFullPath(Path.Combine(baseDirectory, pathString)));
                    }
                    api.apiGetConfig("EcuPath", out var cfgValue);
                    if (logging)
                    {
                        //Log.Info("ECUKom.SetEcuPath()", "Used EcuPath: {0}", cfgValue);
                    }
                }
                else if (logging)
                {
                    //Log.Info("ECUKom.SetEcuPath()", "no config for specific ecu path used; using default values from ediabas config");
                }
            }
            catch (Exception)
            {
                //Log.ErrorException("ECUKom.SetEcuPath()", exception);
            }
        }

        public static string APIFormatName(int resultFormat)
        {
            switch (resultFormat)
            {
                default:
                    return "Ediabas: UNKNOWN !!!!";
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
                case 7:
                    return "Ediabas: API.APIFORMAT_BINARY <=> C# Byte[]";
                case 8:
                    return "Ediabas: API.APIFORMAT_REAL <=> C#: double";
            }
        }

        public static ECUKom DeSerialize(string filename, EdiabasNet ediabas = null)
        {
            ECUKom eCUKom;
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(filename);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ECUKom));
                eCUKom = (ECUKom)xmlSerializer.Deserialize(xmlTextReader);
                xmlTextReader.Close();
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.DeSerialize()", exception);
                eCUKom = new ECUKom("Rheingold", ediabas);
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
                    //Log.Info("ECUKom.DeSerialize()", "got {0} jobs from simulation container", eCUKom.jobList.Count);
                    foreach (ECUJob job in eCUKom.jobList)
                    {
                        job.JobName = job.JobName.ToUpper(CultureInfo.InvariantCulture);
                        job.EcuName = job.EcuName.ToUpper(CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception)
            {
                //Log.Warning("ECUKom.DeSerialize()", "failed to normalize EcuName and JobName tu uppercase with exception: {0}", ex.ToString());
            }
            return eCUKom;
        }

        public static string EdiabasBinPath()
        {
            try
            {
                string environmentVariable = Environment.GetEnvironmentVariable("PATH");
                string[] array = environmentVariable.Split(';');
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
            catch (Exception)
            {
                //Log.WarningException("ECUKom.EdiabasBinPath()", exception);
            }
            return null;
        }

        public static string EdiabasIniFilePath()
        {
            try
            {
                string path = EdiabasBinPath();
                if (File.Exists(Path.Combine(path, "ediabas.ini")))
                {
                    return Path.Combine(path, "ediabas.ini");
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.EdiabasIniFilePath()", exception);
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
            catch (Exception)
            {
                //Log.WarningException("ECUKom.EdiabasIniFilePath()", exception);
            }
            return null;
        }

        public static bool Serialize(string filename, ECUKom ecuKom, Encoding encType)
        {
            try
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(filename, encType);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ECUKom));
                xmlSerializer.Serialize(xmlTextWriter, ecuKom);
                xmlTextWriter.Close();
                //Log.Info("ECUKom.Serialize()", "successfully done");
                return true;
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.Serialize()", exception);
            }
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
            catch (Exception)
            {
                //Log.WarningException("ECUKom.getLogPath()", exception);
            }
            return result;
        }

        public bool InitVCI(IVciDevice device, bool logging)
        {
            if (device == null)
            {
                //Log.Warning("ECUKom.InitVCI()", "failed because device was null");
                return false;
            }
            try
            {
                bool flag;
                switch (device.VCIType)
                {
                    case VCIDeviceType.ENET:
                        flag = api.apiInitExt("ENET", string.Empty, string.Empty, "remotehost=" + device.IPAddress);
                        break;
                    case VCIDeviceType.ICOM:
                        {
                            int configint3 = 6801;
                            if (!string.IsNullOrEmpty(device.VIN) && device.VIN.Length == 17 && !device.VIN.Contains("XXXX"))
                            {
                                flag = api.apiInitExt("ENET", "_", "Rheingold", "RemoteHost=" + device.IPAddress + ";DiagnosticPort=50160;ControlPort=50161");
                                break;
                            }
                            flag = api.apiInitExt("REMOTE::remotehost=" + device.IPAddress + ";Port=" + configint3, "_", "Rheingold", string.Empty);
                            break;
                        }
                    case VCIDeviceType.SIM:
                        flag = true;
                        break;
                    case VCIDeviceType.OMITEC:
                        flag = api.apiInitExt("STD:OMITEC", "_", "Rheingold", string.Empty);
                        break;
                    default:
                        flag = api.apiInit();
                        break;
                    case VCIDeviceType.TELESERVICE:
                        {
                            flag = false;
                            break;
                        }
                    case VCIDeviceType.IRAM:
                        flag = false;
                        break;
                    case VCIDeviceType.PTT:
                        flag = false;
                        break;
                }
                if (api.apiErrorCode() != 0)
                {
                    //Log.Warning("ECUKom.InitVCI()", "failed when init IFH with : {0} / {1}", api.apiErrorCode(), api.apiErrorText());
                }
                string pathString = "..\\..\\..\\logs";
                api.apiSetConfig("TracePath", Path.GetFullPath(pathString));
                if (flag)
                {
                    vci = device as VCIDevice;
                    if (logging)
                    {
                        api.apiGetConfig("TracePath", out var cfgValue);
                        //Log.Info("ECUKom.InitVCI()", "Ediabas TracePath is loaded: {0}", cfgValue);
                        api.apiGetConfig("EdiabasVersion", out var cfgValue2);
                        //Log.Info("ECUKom.InitVCI()", "Ediabas version loaded: {0}", cfgValue2);
                        api.apiGetConfig("IfhVersion", out var cfgValue3);
                        //Log.Info("ECUKom.InitVCI()", "IfhVersion version loaded: {0}", cfgValue3);
                        api.apiGetConfig("Session", out var cfgValue4);
                        //Log.Info("ECUKom.InitVCI()", "Session name: {0}", cfgValue4);
                        api.apiGetConfig("Interface", out var cfgValue5);
                        //Log.Info("ECUKom.InitVCI()", "Interface type loaded: {0}", cfgValue5);
                        if (device.VCIType == VCIDeviceType.ICOM || (!string.IsNullOrEmpty(cfgValue5) && cfgValue5.StartsWith("remote", StringComparison.OrdinalIgnoreCase)))
                        {
                            string cfgValue6 = null;
                            api.apiGetConfig("DisconnectOnApiEnd", out var cfgValue7);
                            //Log.Info("ECUKom.InitVCI()", "DisconnectOnApiEnd configured: {0}", cfgValue7);
                            api.apiGetConfig("TimeoutReceive", out var cfgValue8);
                            //Log.Info("ECUKom.InitVCI()", "TimeoutConnect configured: {0}", cfgValue6);
                            api.apiGetConfig("TimeoutConnect", out cfgValue6);
                            //Log.Info("ECUKom.InitVCI()", "TimeoutReceive configured: {0}", cfgValue8);
                            api.apiGetConfig("TimeoutFunction", out var cfgValue9);
                            //Log.Info("ECUKom.InitVCI()", "TimeoutFunction configured: {0}", cfgValue9);
                            api.apiGetConfig("TimeResponsePending", out var cfgValue10);
                            //Log.Info("ECUKom.InitVCI()", "TimeResponsePending configured: {0}", cfgValue10);
                        }
                    }
                    SetEcuPath(logging);
                }
                return flag;
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.InitVCI()", exception);
            }
            return true;
        }

        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding)
        {
            return apiJob(ecu, job, param, resultFilter, cacheAdding);
        }

        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, int retries, bool fastaActive)
        {
            return (retries == 0) ? apiJob(ecu, job, param, resultFilter) : apiJob(ecu, job, param, resultFilter, retries, 0);
        }

        public IEcuJob ApiJob(string ecu, string job, string param, int retries, int millisecondsTimeout)
        {
            return (retries == 0) ? apiJob(ecu, job, param, string.Empty) : apiJob(ecu, job, param, string.Empty, retries, millisecondsTimeout);
        }

        public ECUJob apiJob(string variant, string job, string param, string resultFilter, int retries, string sgbd = "")
        {
            return apiJob(variant, job, param, resultFilter, retries, 0);
        }

        public ECUJob apiJob(string ecu, string jobName, string param, string resultFilter, int retries, int millisecondsTimeout)
        {
            try
            {
                ECUJob eCUJob = apiJob(ecu, jobName, param, resultFilter);
                if (eCUJob.JobErrorCode == 98)
                {
                    return eCUJob;
                }
                ushort num2 = 1;
                while (num2 < retries && !eCUJob.IsDone())
                {
                    Thread.Sleep(millisecondsTimeout);
                    //Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJob()", "(Sgbd: {0}, {1}) - is retrying {2} times", ecu, jobName, num2);
                    eCUJob = apiJob(ecu, jobName, param, resultFilter);
                    num2 = (ushort)(num2 + 1);
                }
                return eCUJob;
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.apiJob()", exception);
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
            catch (Exception)
            {
                //Log.WarningException("ECUKom.apiJob()", exception);
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
            return eCUJob2;
        }

        public ECUJob apiJob(string ecu, string jobName, string param, string resultFilter, bool cacheAdding)
        {
            if (string.IsNullOrEmpty(ecu))
            {
                ECUJob eCUJob = new ECUJob();
                eCUJob.EcuName = string.Empty;
                eCUJob.JobName = jobName;
                eCUJob.JobParam = param;
                eCUJob.ExecutionStartTime = DateTime.Now;
                eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
                eCUJob.JobErrorCode = 91;
                eCUJob.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
                eCUJob.JobResult = new List<ECUResult>();
                return eCUJob;
            }
            if (param == null)
            {
                param = string.Empty;
            }
            if (resultFilter == null)
            {
                resultFilter = string.Empty;
            }

            int num2 = 0;
            ECUJob eCUJob5 = new ECUJob();
            eCUJob5.EcuName = ecu;
            eCUJob5.ExecutionStartTime = DateTime.Now;
            eCUJob5.JobName = jobName;
            eCUJob5.JobParam = param;
            eCUJob5.JobResultFilter = resultFilter;
            eCUJob5.JobResult = new List<ECUResult>();
            try
            {
                if (vci != null && vci.ForceReInit)
                {
                    api.apiEnd();
                    if (!InitVCI(vci, logging: false))
                    {
                        //Log.Warning("ECUKom.apiJob()", "failed to switch/reinit ediabas API");
                    }
                }
                api.apiJob(ecu, jobName, param, resultFilter);
                while (api.apiStateExt(1000) == 0)
                {
                    Thread.Sleep(2);
                }
                num2 = (eCUJob5.JobErrorCode = api.apiErrorCode());
                eCUJob5.JobErrorText = api.apiErrorText();
                api.apiResultSets(out var rsets);
                eCUJob5.JobResultSets = rsets;
                if (rsets > 0)
                {
                    //Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJob()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - successfully called: {4}:{5} RSets: {6}", ecu, jobName, param, resultFilter, eCUJob5.JobErrorCode, eCUJob5.JobErrorText, rsets);
                    for (ushort num4 = 0; num4 <= rsets; num4 = (ushort)(num4 + 1))
                    {
                        if (api.apiResultNumber(out var buffer, num4))
                        {
                            for (ushort num5 = 1; num5 <= buffer; num5 = (ushort)(num5 + 1))
                            {
                                ECUResult eCUResult = new ECUResult();
                                string buffer2 = string.Empty;
                                eCUResult.Set = num4;
                                if (api.apiResultName(out buffer2, num5, num4))
                                {
                                    eCUResult.Name = buffer2;
                                    if (api.apiResultFormat(out var buffer3, buffer2, num4))
                                    {
                                        eCUResult.Format = buffer3;
                                        switch (buffer3)
                                        {
                                            default:
                                                {
                                                    api.apiResultVar(out var var);
                                                    eCUResult.Value = var;
                                                    break;
                                                }
                                            case 0:
                                                {
                                                    api.apiResultChar(out var buffer11, buffer2, num4);
                                                    eCUResult.Value = buffer11;
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    api.apiResultByte(out var buffer12, buffer2, num4);
                                                    eCUResult.Value = buffer12;
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    api.apiResultInt(out var buffer10, buffer2, num4);
                                                    eCUResult.Value = buffer10;
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    api.apiResultWord(out var buffer7, buffer2, num4);
                                                    eCUResult.Value = buffer7;
                                                    break;
                                                }
                                            case 4:
                                                {
                                                    api.apiResultLong(out var buffer8, buffer2, num4);
                                                    eCUResult.Value = buffer8;
                                                    break;
                                                }
                                            case 5:
                                                {
                                                    api.apiResultDWord(out var buffer9, buffer2, num4);
                                                    eCUResult.Value = buffer9;
                                                    break;
                                                }
                                            case 6:
                                                {
                                                    api.apiResultText(out string buffer6, buffer2, num4, string.Empty);
                                                    eCUResult.Value = buffer6;
                                                    break;
                                                }
                                            case 7:
                                                {
                                                    uint bufferLen2;
                                                    if (api.apiResultBinary(out var buffer5, out var bufferLen, buffer2, num4))
                                                    {
                                                        if (buffer5 != null)
                                                        {
                                                            Array.Resize(ref buffer5, bufferLen);
                                                        }
                                                        eCUResult.Value = buffer5;
                                                        eCUResult.Length = bufferLen;
                                                    }
                                                    else if (api.apiResultBinaryExt(out buffer5, out bufferLen2, 65536u, buffer2, num4))
                                                    {
                                                        if (buffer5 != null)
                                                        {
                                                            Array.Resize(ref buffer5, (int)bufferLen2);
                                                        }
                                                        eCUResult.Value = buffer5;
                                                        eCUResult.Length = bufferLen2;
                                                    }
                                                    else
                                                    {
                                                        eCUResult.Value = new byte[0];
                                                        eCUResult.Length = 0u;
                                                    }
                                                    break;
                                                }
                                            case 8:
                                                {
                                                    api.apiResultReal(out var buffer4, buffer2, num4);
                                                    eCUResult.Value = buffer4;
                                                    break;
                                                }
                                        }
                                    }
                                    eCUJob5.JobResult.Add(eCUResult);
                                }
                                else
                                {
                                    buffer2 = string.Format(CultureInfo.InvariantCulture, "ResName unknown! Job was: {0} result index: {1} set index{2}", jobName, num5, num4);
                                }
                            }
                        }
                    }
                }
                if (num2 != 0)
                {
                    //Log.Info("ECUKom.apiJob()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with apiError: {4}:{5}", ecu, jobName, param, resultFilter, eCUJob5.JobErrorCode, eCUJob5.JobErrorText);
                }
            }
            catch (IndexOutOfRangeException)
            {
                //Log.Warning("ECUKom.apiJob()", "buggy sgbd ({0}, {1}, {2}, {3}) apiError: {4} found; wrong result set length was set", ecu, jobName, param, resultFilter, eCUJob5.JobErrorText);
            }
            catch (Exception)
            {
                //Log.WarningException("ECUKom.apiJob()", exception);
            }
            eCUJob5.ExecutionEndTime = DateTime.Now;
            AddJobInCache(eCUJob5, cacheAdding);
            return eCUJob5;
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
                    //Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJobData()", "(Sgbd: {0}, {1}) - is retrying {2} times", ecu, job, num2);
                    eCUJob = apiJobData(ecu, job, param, paramlen, resultFilter);
                    num2 = (ushort)(num2 + 1);
                }
                return eCUJob;
            }
            catch (Exception exception)
            {
                //Log.WarningException("ECUKom.apiJobData()", exception);
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
            if (string.IsNullOrEmpty(ecu))
            {
                ECUJob eCUJob = new ECUJob();
                eCUJob.JobName = string.Empty;
                eCUJob.ExecutionStartTime = DateTime.Now;
                eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
                eCUJob.JobErrorCode = 91;
                eCUJob.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
                eCUJob.JobResult = new List<ECUResult>();
                return eCUJob;
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

            int num2 = 0;
            ECUJob eCUJob4 = new ECUJob();
            eCUJob4.EcuName = ecu;
            eCUJob4.JobName = job;
            eCUJob4.ExecutionStartTime = DateTime.Now;
            eCUJob4.ExecutionEndTime = eCUJob4.ExecutionStartTime;
            eCUJob4.JobResultFilter = resultFilter;
            eCUJob4.JobResult = new List<ECUResult>();
            try
            {
                eCUJob4.JobParam = FormatConverter.ByteArray2String(param, (uint)paramlen);
                if (vci != null && vci.ForceReInit)
                {
                    api.apiEnd();
                    if (!InitVCI(vci, logging: false))
                    {
                        //Log.Warning("ECUKom.apiJob()", "failed to switch/reinit ediabas API");
                    }
                }
                api.apiJobData(ecu, job, param, paramlen, resultFilter);
                while (api.apiStateExt(1000) == 0)
                {
                    Thread.Sleep(2);
                }
                num2 = (eCUJob4.JobErrorCode = api.apiErrorCode());
                eCUJob4.JobErrorText = api.apiErrorText();
                if (num2 == 0)
                {
                    //Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - successfully called: {4}:{5}", ecu, job, param, resultFilter, eCUJob4.JobErrorCode, eCUJob4.JobErrorText);
                    if (api.apiResultSets(out var rsets))
                    {
                        eCUJob4.JobResultSets = rsets;
                        for (ushort num4 = 0; num4 <= rsets; num4 = (ushort)(num4 + 1))
                        {
                            if (api.apiResultNumber(out var buffer, num4))
                            {
                                for (ushort num5 = 1; num5 <= buffer; num5 = (ushort)(num5 + 1))
                                {
                                    ECUResult eCUResult = new ECUResult();
                                    eCUResult.Set = num4;
                                    if (api.apiResultName(out var buffer2, num5, num4))
                                    {
                                        eCUResult.Name = buffer2;
                                        if (api.apiResultFormat(out var buffer3, buffer2, num4))
                                        {
                                            eCUResult.Format = buffer3;
                                            switch (buffer3)
                                            {
                                                default:
                                                    {
                                                        api.apiResultVar(out var var);
                                                        eCUResult.Value = var;
                                                        break;
                                                    }
                                                case 0:
                                                    {
                                                        api.apiResultChar(out var buffer11, buffer2, num4);
                                                        eCUResult.Value = buffer11;
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        api.apiResultByte(out var buffer12, buffer2, num4);
                                                        eCUResult.Value = buffer12;
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        api.apiResultInt(out var buffer10, buffer2, num4);
                                                        eCUResult.Value = buffer10;
                                                        break;
                                                    }
                                                case 3:
                                                    {
                                                        api.apiResultWord(out var buffer7, buffer2, num4);
                                                        eCUResult.Value = buffer7;
                                                        break;
                                                    }
                                                case 4:
                                                    {
                                                        api.apiResultLong(out var buffer8, buffer2, num4);
                                                        eCUResult.Value = buffer8;
                                                        break;
                                                    }
                                                case 5:
                                                    {
                                                        api.apiResultDWord(out var buffer9, buffer2, num4);
                                                        eCUResult.Value = buffer9;
                                                        break;
                                                    }
                                                case 6:
                                                    {
                                                        api.apiResultText(out string buffer6, buffer2, num4, string.Empty);
                                                        eCUResult.Value = buffer6;
                                                        break;
                                                    }
                                                case 7:
                                                    {
                                                        uint bufferLen2;
                                                        if (api.apiResultBinary(out var buffer5, out var bufferLen, buffer2, num4))
                                                        {
                                                            if (buffer5 != null)
                                                            {
                                                                Array.Resize(ref buffer5, bufferLen);
                                                            }
                                                            eCUResult.Value = buffer5;
                                                            eCUResult.Length = bufferLen;
                                                        }
                                                        else if (api.apiResultBinaryExt(out buffer5, out bufferLen2, 65536u, buffer2, num4))
                                                        {
                                                            if (buffer5 != null)
                                                            {
                                                                Array.Resize(ref buffer5, (int)bufferLen2);
                                                            }
                                                            eCUResult.Value = buffer5;
                                                            eCUResult.Length = bufferLen2;
                                                        }
                                                        else
                                                        {
                                                            eCUResult.Value = new byte[0];
                                                            eCUResult.Length = 0u;
                                                        }
                                                        break;
                                                    }
                                                case 8:
                                                    {
                                                        api.apiResultReal(out var buffer4, buffer2, num4);
                                                        eCUResult.Value = buffer4;
                                                        break;
                                                    }
                                            }
                                        }
                                        eCUJob4.JobResult.Add(eCUResult);
                                    }
                                    else
                                    {
                                        buffer2 = string.Format(CultureInfo.InvariantCulture, "ResName unknown! Job was: {0} result index: {1} set index{2}", job, num5, num4);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with apiError: {4}:{5}", ecu, job, param, resultFilter, eCUJob4.JobErrorCode, eCUJob4.JobErrorText);
                }
            }
            catch (IndexOutOfRangeException)
            {
                //Log.Warning("ECUKom.apiJobData()", "buggy sgbd ({0}, {1}, {2}, {3}) apiError: {4} found; wrong result set length was set", ecu, job, param, resultFilter, eCUJob4.JobErrorText);
            }
            catch (Exception ex3)
            {
                //Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with exception: {4}", ecu, job, param, resultFilter, ex3.ToString());
            }
            eCUJob4.ExecutionEndTime = DateTime.Now;
            AddJobInCache(eCUJob4);
            return eCUJob4;
        }

        public IEcuJob ExecuteJobOverEnet(string icomAddress, string ecu, string job, string param, string resultFilter = "", int retries = 0)
        {
            End();
            string istaLogPath = GetIstaLogPath();
            if (string.IsNullOrEmpty(istaLogPath))
            {
                //Log.Warning("EdiabasUtils.ExecuteJobOverEnet()", "Path to ista log cannot be found.");
                return null;
            }
            string text = (IsProblemHandlingTraceRunning ? "5" : "0");
            if (!ApiInitExt("ENET", "_", "Rheingold", "RemoteHost=" + icomAddress + ";DiagnosticPort=51560;ControlPort=51561;TracePath=" + istaLogPath + ";ApiTrace=" + text))
            {
                //Log.Warning("EdiabasUtils.ExecuteJobOverEnet()", "Failed switching to ENET. The Job will not be executed. The EDIABAS connection will be refreshed.");
                RefreshEdiabasConnection();
                return null;
            }
            SetEcuPath(logging: true);
            IEcuJob result = ApiJob(ecu, job, param, resultFilter, retries, true);
            RefreshEdiabasConnection();
            return result;
        }

        private void RefreshEdiabasConnection()
        {
            if (Refresh())
            {
                //Log.Info("EdiabasUtils.RefreshEdiabasConnection()", "Successfully connected to current VCI device.");
            }
            else
            {
                //Log.Error("EdiabasUtils.RefreshEdiabasConnection()", "Failed to connect to current VCI device!");
            }
        }

        private static string GetIstaLogPath()
        {
            string result = string.Empty;
            try
            {
                string pathString = "..\\..\\..\\logs";
                result = Path.GetFullPath(pathString);
            }
            catch (Exception)
            {
                //Log.Warning("EdiabasUtils.GetIstaLogPath()", "Exception occurred: ", ex);
            }
            return result;
        }

        private void AddJobInCache(ECUJob job, bool cacheCondition = true)
        {
            if (jobList != null && cacheCondition)
            {
                string msg = "Store in Cache: EcuName:" + job.EcuName + ", JobName:" + job.JobName + ", JobParam:" + job.JobParam;
                //Log.Info("ECUKom.AddJobInCache()", msg);
                jobList.Add(job);
            }
        }

    }
}
