using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container;

public abstract class ECUKomBase : IEcuKom, IEcuKomApi
{
    private string _APP;

    public CommMode communicationMode;

    public bool isProblemHandlingTraceRunning;

    private List<string> apiJobNamesToBeCached = CachedApiJobConfigParser.Parse();

    private DateTime lastJobExecution;

    public VCIDevice vci;

    private Dictionary<string, List<IEcuJob>> ecuJobDictionary = new Dictionary<string, List<IEcuJob>>();

    private bool m_FromFastaConfig;

    [XmlIgnore]
    public IList<string> lang = new List<string>();

    public uint EdiabasHandle { get; }

    [XmlIgnore]
    public IReadOnlyList<IEcuJob> JobList => jobList;

    public List<ECUJob> jobList { get; set; }

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
        set
        {
            if (vci != value)
            {
                vci = value;
            }
        }
    }

    [XmlIgnore]
    public bool IsInSimulationMode => CommunicationMode == CommMode.Simulation;

    public bool IsProblemHandlingTraceRunning
    {
        get
        {
            return isProblemHandlingTraceRunning;
        }
        set
        {
            if (isProblemHandlingTraceRunning != value)
            {
                isProblemHandlingTraceRunning = value;
            }
        }
    }

    public string VciIpAddress => VCI?.IPAddress;

    public VCIDeviceType VCIDeviceType
    {
        get
        {
            if (VCI == null)
            {
                return VCIDeviceType.UNKNOWN;
            }
            return VCI.VCIType;
        }
    }

    public IEcuJob DefaultApiJob(string ecu, string job, string param, string resultFilter)
    {
        return apiJob(ecu, job, param, resultFilter);
    }

    public IEcuJob ApiJobWithRetries(string variant, string job, string param, string resultFilter, int retries)
    {
        return apiJob(variant, job, param, resultFilter, retries, null, null, "ApiJobWithRetries");
    }

    public abstract void End();

    public abstract string GetEdiabasIniFilePath(string iniFilename);

    public abstract BoolResultObject InitVCI(IVciDevice vciDevice, bool isDoIP);

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

    public void SetLogLevelToMax()
    {
        isProblemHandlingTraceRunning = true;
    }

    public abstract bool Refresh(bool isDoIP);

    public abstract bool ApiInitExt(string ifh, string unit, string app, string reserved);

    public abstract void SetEcuPath(bool logging);

    public abstract string GetLogPath();

    public abstract BoolResultObject InitVCI(IVciDevice device, bool logging, bool isDoIP);

    public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding)
    {
        return apiJob(ecu, job, param, resultFilter, cacheAdding, isRetry: false);
    }

    public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, int retries, bool fastaActive)
    {
        return ApiJob(ecu, job, param, resultFilter, retries, null, fastaActive);
    }

    public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter = "", int retries = 0, IProtocolBasic fastaprotocoller = null, bool fastaActive = true)
    {
        if (retries != 0)
        {
            return apiJob(ecu, job, param, resultFilter, retries, 0, fastaprotocoller);
        }
        return apiJob(ecu, job, param, resultFilter, fastaprotocoller);
    }

    public IEcuJob ApiJob(string ecu, string job, string param, int retries, int millisecondsTimeout)
    {
        if (retries != 0)
        {
            return apiJob(ecu, job, param, string.Empty, retries, millisecondsTimeout);
        }
        return apiJob(ecu, job, param, string.Empty);
    }

    public IEcuJob apiJob(string variant, string job, string param, string resultFilter, int retries, string sgbd = "", IProtocolBasic fastaprotocoller = null, [CallerMemberName] string callerMember = "")
    {
        if (FromFastaConfig && !string.IsNullOrEmpty(sgbd) && apiJobNamesToBeCached.Contains(job))
        {
            IEcuJob jobFromCache = GetJobFromCache(sgbd, job, param, resultFilter);
            if (jobFromCache != null && (CommunicationMode == CommMode.Simulation || (jobFromCache.JobErrorCode != 0 && jobFromCache.JobResult != null && jobFromCache.JobResult.Count > 0)))
            {
                return jobFromCache;
            }
        }
        return apiJob(variant, job, param, resultFilter, retries, 0, fastaprotocoller, callerMember);
    }

    public IEcuJob apiJob(string ecu, string jobName, string param, string resultFilter, int retries, int millisecondsTimeout, IProtocolBasic fastaprotocoller = null, string callerMember = "")
    {
        if (!VehicleCommunication.validLicense)
        {
            throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
        }
        if (retries > 5)
        {
            Log.Warning("ECUKom.apiJob()", "Number of retries is set to {0}.", retries);
        }
        try
        {
            IEcuJob ecuJob = apiJob(ecu, jobName, param, resultFilter, fastaprotocoller, callerMember);
            if (ecuJob.JobErrorCode == 98)
            {
                return ecuJob;
            }
            ushort num = 1;
            while (num < retries && !ecuJob.IsDone())
            {
                SleepUtility.ThreadSleep(millisecondsTimeout, "ECUKom.apiJob - " + ecu + ", " + jobName + ", " + param);
                Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJob()", "(Sgbd: {0}, {1}) - is retrying {2} times", ecu, jobName, num);
                ecuJob = apiJob(ecu, jobName, param, resultFilter, fastaprotocoller, callerMember);
                num++;
            }
            return ecuJob;
        }
        catch (Exception exception)
        {
            Log.WarningException("ECUKom.apiJob()", exception);
            IEcuJob ecuJob = new ECUJob(fastaprotocoller);
            ecuJob.EcuName = ecu;
            ecuJob.ExecutionStartTime = DateTime.Now;
            ecuJob.ExecutionEndTime = ecuJob.ExecutionStartTime;
            ecuJob.JobName = jobName;
            ecuJob.JobParam = param;
            ecuJob.JobResultFilter = resultFilter;
            ecuJob.JobErrorCode = 90;
            ecuJob.JobErrorText = "SYS-0000: INTERNAL ERROR";
            ecuJob.JobResult = new List<IEcuResult>();
            AddJobInCache(ecuJob);
            return ecuJob;
        }
    }

    public IEcuJob apiJob(string ecu, string job, string param, string resultFilter, IProtocolBasic fastaprotocoller = null, string callerMember = "")
    {
        try
        {
            IEcuJob ecuJob = null;
            if (string.Compare(ecu, "FA", StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(job, "FA_STREAM2STRUCT", StringComparison.OrdinalIgnoreCase) == 0)
            {
                int len;
                byte[] param2 = FormatConverter.Ascii2ByteArray(param, out len);
                ecuJob = apiJobData("FA", "FA_STREAM2STRUCT", param2, len, string.Empty, callerMember);
            }
            else
            {
                ecuJob = apiJob(ecu, job, param, resultFilter, cacheAdding: true, isRetry: false, fastaprotocoller, callerMember);
            }
            if (ecuJob != null && VehicleCommunication.DebugLevel > 2)
            {
                ECUJob.Dump(ecuJob);
            }
            return ecuJob;
        }
        catch (Exception exception)
        {
            Log.WarningException("ECUKom.apiJob()", exception);
        }
        ECUJob eCUJob = new ECUJob(fastaprotocoller);
        eCUJob.EcuName = ecu;
        eCUJob.JobName = job;
        eCUJob.JobParam = param;
        eCUJob.ExecutionStartTime = DateTime.Now;
        eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
        eCUJob.JobErrorCode = 91;
        eCUJob.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
        eCUJob.JobResult = new List<IEcuResult>();
        if (VehicleCommunication.DebugLevel > 2)
        {
            ECUJob.Dump(eCUJob);
        }
        return eCUJob;
    }

    public IEcuJob apiJob(string ecu, string jobName, string param, string resultFilter, bool cacheAdding, bool isRetry, IProtocolBasic fastaprotocoller = null, string callerMember = "")
    {
        StartTimeMetric(ecu, jobName, param);
        try
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }
            if (string.IsNullOrEmpty(ecu))
            {
                ECUJob obj = new ECUJob(fastaprotocoller)
                {
                    EcuName = string.Empty,
                    JobName = jobName,
                    JobParam = param,
                    ExecutionStartTime = DateTime.Now
                };
                obj.ExecutionEndTime = obj.ExecutionStartTime;
                obj.JobErrorCode = 91;
                obj.JobErrorText = "SYS-0001: ILLEGAL FUNCTION";
                obj.JobResult = new List<IEcuResult>();
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
                IEcuJob ecuJob = ApiJobSim(ecu, jobName, param, resultFilter);
                if (ecuJob != null)
                {
                    return ecuJob;
                }
                ECUJob obj2 = new ECUJob(fastaprotocoller)
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
                obj2.JobResult = new List<IEcuResult>();
                return obj2;
            }
            if (communicationMode == CommMode.CacheFirst && cacheAdding)
            {
                lastJobExecution = DateTime.MinValue;
                IEcuJob ecuJob2 = ApiJobSim(ecu, jobName, param, resultFilter);
                if (ecuJob2 != null && ecuJob2.JobErrorCode == 0 && ecuJob2.JobResult != null && ecuJob2.JobResult.Count > 0)
                {
                    return ecuJob2;
                }
            }
            DateTimePrecise dateTimePrecise = new DateTimePrecise(10L);
            ECUJob eCUJob = new ECUJob(fastaprotocoller);
            eCUJob.EcuName = ecu;
            eCUJob.ExecutionStartTime = dateTimePrecise.Now;
            eCUJob.JobName = jobName;
            eCUJob.JobParam = param;
            eCUJob.JobResultFilter = resultFilter;
            eCUJob.JobResult = new List<IEcuResult>();
            try
            {
                ExecuteEdiabasJobAndGetResults(eCUJob, callerMember);
            }
            catch (IndexOutOfRangeException)
            {
                Log.Warning("ECUKom.apiJob()", "buggy sgbd ({0}, {1}, {2}, {3}) apiError: {4} found; wrong result set length was set", ecu, jobName, param, resultFilter, eCUJob.JobErrorText);
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUKom.apiJob()", exception);
            }
            eCUJob.ExecutionEndTime = dateTimePrecise.Now;
            AddJobInCache(eCUJob, cacheAdding);
            string stringResult = eCUJob.getStringResult(1, "JOB_STATUS");
            (bool, ECUJob) tuple = HandleEcuAuthorizationRejection(ecu, jobName, param, resultFilter, (stringResult == null) ? string.Empty : stringResult, isRetry);
            if (tuple.Item1)
            {
                eCUJob = tuple.Item2;
            }
            return eCUJob;
        }
        finally
        {
            EndTimeMetric(ecu, jobName, param);
        }
    }

    public abstract void EndTimeMetric(string ecu, string jobName, string param, int argsLength = -1);

    public abstract void StartTimeMetric(string ecu, string jobName, string param, int argsLength = -1);

    public abstract (bool, ECUJob) HandleEcuAuthorizationRejection(string ecu, string jobName, string param, string resultFilter, string jobStatus, bool isRetry);

    public abstract void RemoveTraceLevel(string callerMember);

    public abstract void SetTraceLevelToMax(string callerMember);

    public abstract SpecialSecurityCases DetectedSpecialSecurityCase();

    public IEcuJob ApiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter = "", int retries = 0)
    {
        if (retries == 0)
        {
            return apiJobData(ecu, job, param, paramlen, resultFilter, string.Empty);
        }
        return apiJobData(ecu, job, param, paramlen, resultFilter, retries);
    }

    public IEcuJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter, int retries)
    {
        if (!VehicleCommunication.validLicense)
        {
            throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
        }
        try
        {
            IEcuJob ecuJob = apiJobData(ecu, job, param, paramlen, resultFilter, string.Empty);
            if (ecuJob.JobErrorCode == 98)
            {
                return ecuJob;
            }
            ushort num = 0;
            while (num < retries && !ecuJob.IsDone())
            {
                Log.Debug(VehicleCommunication.DebugLevel, "ECUKom.apiJobData()", "(Sgbd: {0}, {1}) - is retrying {2} times", ecu, job, num);
                ecuJob = apiJobData(ecu, job, param, paramlen, resultFilter, string.Empty);
                num++;
            }
            return ecuJob;
        }
        catch (Exception exception)
        {
            Log.WarningException("ECUKom.apiJobData()", exception);
            IEcuJob ecuJob = new ECUJob();
            ecuJob.EcuName = ecu;
            ecuJob.JobName = job;
            ecuJob.JobParam = FormatConverter.ByteArray2String(param, (uint)paramlen);
            ecuJob.JobResultFilter = resultFilter;
            ecuJob.ExecutionStartTime = DateTime.Now;
            ecuJob.ExecutionEndTime = ecuJob.ExecutionStartTime;
            ecuJob.JobErrorCode = 90;
            ecuJob.JobErrorText = "SYS-0000: INTERNAL ERROR";
            ecuJob.JobResult = new List<IEcuResult>();
            AddJobInCache(ecuJob);
            return ecuJob;
        }
    }

    public IEcuJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter, string callerMember)
    {
        StartTimeMetric(ecu, job, string.Empty, paramlen);
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
                obj.JobResult = new List<IEcuResult>();
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
                    IEcuJob ecuJob = ApiJobSim(ecu, job, param2, resultFilter);
                    if (ecuJob != null)
                    {
                        return ecuJob;
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
                obj2.JobResult = new List<IEcuResult>();
                return obj2;
            }
            DateTimePrecise dateTimePrecise = new DateTimePrecise(10L);
            ECUJob eCUJob = new ECUJob();
            eCUJob.EcuName = ecu;
            eCUJob.JobName = job;
            eCUJob.ExecutionStartTime = dateTimePrecise.Now;
            eCUJob.ExecutionEndTime = eCUJob.ExecutionStartTime;
            eCUJob.JobResultFilter = resultFilter;
            eCUJob.JobResult = new List<IEcuResult>();
            try
            {
                ExecuteEdiabasJobAndGetResultsWithByteParams(eCUJob, param, paramlen, callerMember);
            }
            catch (IndexOutOfRangeException)
            {
                Log.Warning("ECUKom.apiJobData()", "buggy sgbd ({0}, {1}, {2}, {3}) apiError: {4} found; wrong result set length was set", ecu, job, param, resultFilter, eCUJob.JobErrorText);
            }
            catch (Exception ex3)
            {
                Log.Warning("ECUKom.apiJobData()", "(ecu: {0}, job: {1}, param: {2}, resultFilter {3}) - failed with exception: {4}", ecu, job, param, resultFilter, ex3.ToString());
            }
            eCUJob.ExecutionEndTime = dateTimePrecise.Now;
            AddJobInCache(eCUJob);
            return eCUJob;
        }
        finally
        {
            EndTimeMetric(ecu, job, string.Empty, paramlen);
        }
    }

    public abstract void ExecuteEdiabasJobAndGetResults(ECUJob ecuJob, string callerMember);

    public abstract void ExecuteEdiabasJobAndGetResultsWithByteParams(ECUJob ecuJob, byte[] param, int paramlen, string callerMember);

    public abstract int getErrorCode();

    public abstract string getErrorText();

    public abstract bool setConfig(string cfgName, string cfgValue);

    public IEcuJob GetJobFromCache(string ecuName, string jobName, string jobParam, string jobResultFilter)
    {
        Log.Info("ECUKom.GetJobFromCache()", "Try retrieve from Cache: EcuName:{0}, JobName:{1}, JobParam:{2})", ecuName, jobName, jobParam);
        if (!VehicleCommunication.validLicense)
        {
            throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
        }
        if (!ecuJobDictionary.ContainsKey(ecuName + "-" + jobName))
        {
            ecuJobDictionary.Add(ecuName + "-" + jobName, new List<IEcuJob>());
        }
        try
        {
            IEnumerable<ECUJob> enumerable = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobResultFilter, jobResultFilter, StringComparison.OrdinalIgnoreCase) && job.ExecutionStartTime > lastJobExecution);
            if (enumerable != null && ((IEnumerable<IEcuJob>)enumerable).Count() > 0)
            {
                return RetrieveEcuJob(enumerable, ecuName, jobName);
            }
            IEnumerable<ECUJob> enumerable2 = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase) && job.ExecutionStartTime > lastJobExecution);
            if (enumerable2 != null && ((IEnumerable<IEcuJob>)enumerable2).Count() > 0)
            {
                return RetrieveEcuJob(enumerable2, ecuName, jobName);
            }
            IEnumerable<ECUJob> enumerable3 = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobResultFilter, jobResultFilter, StringComparison.OrdinalIgnoreCase));
            if (enumerable3 != null && ((IEnumerable<IEcuJob>)enumerable3).Count() > 0)
            {
                return RetrieveEcuJobNoExecTime(enumerable3, ecuName, jobName);
            }
            IEnumerable<ECUJob> enumerable4 = jobList.Where((ECUJob job) => string.Equals(job.EcuName, ecuName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase) && string.Equals(job.JobParam, jobParam, StringComparison.OrdinalIgnoreCase));
            if (enumerable4 != null && ((IEnumerable<IEcuJob>)enumerable4).Count() > 0)
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

    public abstract IEcuJob ExecuteJobOverEnetWrapper(string icomAddress, string ecu, string job, string param, bool isDoIP, string method, string resultFilter = "", int retries = 0);

    public void RefreshEdiabasConnection(bool isDoIp)
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

    public virtual IEcuJob RetrieveEcuJobNoExecTime(IEnumerable<IEcuJob> query, string ecuName, string jobName)
    {
        foreach (IEcuJob item in query)
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

    public virtual IEcuJob RetrieveEcuJob(IEnumerable<IEcuJob> query, string ecuName, string jobName)
    {
        foreach (IEcuJob item in query)
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

    public virtual IEcuJob ApiJobSim(string ecu, string job, string param, string result)
    {
        if (!VehicleCommunication.validLicense)
        {
            throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
        }
        IEcuJob ecuJob = null;
        if (!FromFastaConfig)
        {
            Log.Info(Log.CurrentMethod(), "Retrieving ECU " + ecu + " job " + job + " from cache.");
            ecuJob = GetJobFromCache(ecu, job, param, result);
        }
        if (ecuJob != null)
        {
            ecuJob.FASTARelevant = false;
            foreach (IEcuResult item in ecuJob.JobResult)
            {
                item.FASTARelevant = false;
            }
        }
        return ecuJob;
    }

    public virtual DateTime GetLastExecutionTime(DateTime executionStartTime)
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

    public virtual void AddJobInCache(IEcuJob job, bool cacheCondition = true)
    {
        if (!ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.JobResultsCachingDisabled", defaultValue: false) && jobList != null && cacheCondition)
        {
            string msg = "Store in Cache: EcuName:" + job.EcuName + ", JobName:" + job.JobName + ", JobParam:" + job.JobParam;
            Log.Info("ECUKom.AddJobInCache()", msg);
            jobList.Add(job as ECUJob);
        }
    }

    private static string GetStack()
    {
        StringBuilder stringBuilder = new StringBuilder();
        try
        {
            int num = 1;
            do
            {
                MethodBase method = new StackFrame(num).GetMethod();
                string name = method.DeclaringType.Name;
                stringBuilder.Append("-> " + name + "." + method.Name + " ");
                num++;
            }
            while (num < 10);
        }
        catch (Exception exception)
        {
            Log.WarningException(Log.CurrentMethod(), exception);
        }
        return stringBuilder.ToString();
    }
}
