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

        //private API api;

        private string _APP;

        private CommMode communicationMode = CommMode.Normal;

        private bool isProblemHandlingTraceRunning;

        //private List<string> apiJobNamesToBeCached = CachedApiJobConfigParser.Parse();

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
            return null;
        }

        public IEcuJob ApiJobWithRetries(string variant, string job, string param, string resultFilter, int retries)
        {
            return null;
        }

        public ECUKom()
            : this(null)
        {
        }

        public ECUKom(string app)
        {
            //api = new API();
            communicationMode = CommMode.Normal;
            jobList = new List<ECUJob>();
            APP = app;
            FromFastaConfig = false;
            CacheHitCounter = 0;
        }

        public void End()
        {
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
            return 0;
            //return apiJobNamesToBeCached.Count;
        }

        public void SetLogLevelToNormal()
        {
            //api.apiSetConfig("ApiTrace", 0.ToString(CultureInfo.InvariantCulture));
            isProblemHandlingTraceRunning = false;
        }

        public void SetLogLevelToMax()
        {
            //int configint = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Level.Trace.Ediabas", 5);
            //int configint2 = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Trace.Ediabas.Size", 32767);
            //api.apiSetConfig("ApiTrace", configint.ToString(CultureInfo.InvariantCulture));
            //api.apiSetConfig("TraceSize", configint2.ToString(CultureInfo.InvariantCulture));
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
            //return api.apiInitExt(ifh, unit, app, reserved);
            return true;
        }

        public void SetEcuPath(bool logging)
        {
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

        public static ECUKom DeSerialize(string filename)
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
            return null;
        }

        public bool InitVCI(IVciDevice device, bool logging)
        {
            return true;
        }

        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding)
        {
            return null;
        }

        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, int retries, bool fastaActive)
        {
            return null;
        }

        public IEcuJob ApiJob(string ecu, string job, string param, int retries, int millisecondsTimeout)
        {
            return null;
        }

        public ECUJob apiJob(string variant, string job, string param, string resultFilter, int retries, string sgbd = "")
        {
            return null;
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
            return null;
        }

        public IEcuJob ApiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter = "", int retries = 0)
        {
            return null;
        }

        public ECUJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter, int retries)
        {
            return null;
        }

        public ECUJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter)
        {
            return null;
        }

        public IEcuJob ExecuteJobOverEnet(string icomAddress, string ecu, string job, string param, string resultFilter = "", int retries = 0)
        {
            return null;
        }
    }
}
