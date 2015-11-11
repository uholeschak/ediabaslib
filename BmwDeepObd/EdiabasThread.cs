using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EdiabasLib;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace BmwDeepObd
{
    public class EdiabasThread : IDisposable
    {
        public class EdiabasErrorReport
        {
            public EdiabasErrorReport(string ecuName, Dictionary<string, EdiabasNet.ResultData> errorDict, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet) :
                this(ecuName, errorDict, errorDetailSet, string.Empty)
            {
            }

            public EdiabasErrorReport(string ecuName, Dictionary<string, EdiabasNet.ResultData> errorDict, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet, string execptionText)
            {
                _ecuName = ecuName;
                _errorDict = errorDict;
                _errorDetailSet = errorDetailSet;
                _execptionText = execptionText;
            }

            private readonly string _ecuName;
            private readonly Dictionary<string, EdiabasNet.ResultData> _errorDict;
            private readonly List<Dictionary<string, EdiabasNet.ResultData>> _errorDetailSet;
            private readonly string _execptionText;

            public string EcuName
            {
                get
                {
                    return _ecuName;
                }
            }

            public Dictionary<string, EdiabasNet.ResultData> ErrorDict
            {
                get
                {
                    return _errorDict;
                }
            }

            public List<Dictionary<string, EdiabasNet.ResultData>> ErrorDetailSet
            {
                get
                {
                    return _errorDetailSet;
                }
            }

            public string ExecptionText
            {
                get
                {
                    return _execptionText;
                }
            }
        }

        public delegate void DataUpdatedEventHandler(object sender, EventArgs e);
        public event DataUpdatedEventHandler DataUpdated;
        public delegate void ThreadTerminatedEventHandler(object sender, EventArgs e);
        public event ThreadTerminatedEventHandler ThreadTerminated;

        public JobReader.PageInfo JobPageInfo
        {
            get;
            set;
        }

        public bool CommActive
        {
            get;
            set;
        }

        public bool Connected
        {
            get;
            private set;
        }

        public List<EdiabasErrorReport> EdiabasErrorReportList
        {
            get;
            private set;
        }

        public Dictionary<string, EdiabasNet.ResultData> EdiabasResultDict
        {
            get;
            private set;
        }

        public string EdiabasErrorMessage
        {
            get;
            private set;
        }

        public List<string> ErrorResetList { get; set; }

        public EdiabasNet Ediabas
        {
            get
            {
                return _ediabas;
            }
        }

        static public readonly Object DataLock = new Object();

        private bool _disposed;
        private volatile bool _stopThread;
        private bool _threadRunning;
        private Thread _workerThread;
        private EdiabasNet _ediabas;
        private bool _ediabasInitReq;
        private bool _ediabasJobAbort;

        public EdiabasThread(string ecuPath, ActivityCommon.InterfaceType interfaceType)
        {
            _stopThread = false;
            _threadRunning = false;
            _workerThread = null;
            _ediabas = new EdiabasNet();

            if (interfaceType == ActivityCommon.InterfaceType.Enet)
            {
                _ediabas.EdInterfaceClass = new EdInterfaceEnet();
            }
            else
            {
                _ediabas.EdInterfaceClass = new EdInterfaceObd();
            }
            _ediabas.AbortJobFunc = AbortEdiabasJob;
            _ediabas.SetConfigProperty("EcuPath", ecuPath);

            InitProperties();
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _ediabas.Dispose();
                    _ediabas = null;
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public bool StartThread(string comPort, object connectParameter, string traceDir, bool traceAppend, JobReader.PageInfo pageInfo, bool commActive)
        {
            if (_workerThread != null)
            {
                return false;
            }
            try
            {
                _stopThread = false;
                if (_ediabas.EdInterfaceClass is EdInterfaceObd)
                {
                    ((EdInterfaceObd)_ediabas.EdInterfaceClass).ComPort = comPort;
                }
                else if (_ediabas.EdInterfaceClass is EdInterfaceEnet)
                {
                    if (!string.IsNullOrEmpty(comPort))
                    {
                        ((EdInterfaceEnet)_ediabas.EdInterfaceClass).RemoteHost = comPort;
                    }
                }
                _ediabas.EdInterfaceClass.ConnectParameter = connectParameter;
                if (!string.IsNullOrEmpty(traceDir))
                {
                    _ediabas.SetConfigProperty("TracePath", traceDir);
                    _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                    _ediabas.SetConfigProperty("AppendTrace", traceAppend ? "1" : "0");
                }
                else
                {
                    _ediabas.SetConfigProperty("IfhTrace", "0");
                }
                InitProperties();
                CommActive = commActive;
                JobPageInfo = pageInfo;
                _workerThread = new Thread(ThreadFunc);
                _threadRunning = true;
                _workerThread.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void StopThread(bool wait)
        {
            if (_workerThread != null)
            {
                _stopThread = true;
                if (wait)
                {
                    _workerThread.Join();
                    _workerThread = null;
                }
            }
            _ediabas.SetConfigProperty("IfhTrace", "0");
        }

        public bool ThreadRunning()
        {
            if (_workerThread == null) return false;
            return _threadRunning;
        }

        public bool ThreadStopping()
        {
            if (_workerThread == null) return false;
            return _stopThread;
        }

        private void ThreadFunc()
        {
            DataUpdatedEvent();
            JobReader.PageInfo lastPageInfo = null;
            while (!_stopThread)
            {
                try
                {
                    if (!CommActive)
                    {
                        continue;
                    }
                    JobReader.PageInfo copyPageInfo = JobPageInfo;

                    if (lastPageInfo != copyPageInfo)
                    {
                        lastPageInfo = copyPageInfo;
                        InitProperties(true);
                    }

                    bool result = CommEdiabas(copyPageInfo);

                    if (result)
                    {
                        Connected = true;
                    }
                }
                catch (Exception)
                {
                    break;
                }
                DataUpdatedEvent();
            }
            _threadRunning = false;
            DataUpdatedEvent();
            ThreadTerminatedEvent();
        }

        private bool CommEdiabas(JobReader.PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                lock (DataLock)
                {
                    EdiabasErrorMessage = "No Page info";
                }
                Thread.Sleep(1000);
                return false;
            }

            if (pageInfo.ErrorsInfo != null)
            {   // read errors
                if (_ediabasInitReq)
                {
                    lock (DataLock)
                    {
                        EdiabasErrorReportList = null;
                    }
                    _ediabasJobAbort = false;
                    _ediabasInitReq = false;
                }
                List<string> errorResetList;
                lock (DataLock)
                {
                    errorResetList = ErrorResetList;
                    ErrorResetList = null;
                }

                List<EdiabasErrorReport> errorReportList = new List<EdiabasErrorReport>();

                foreach (JobReader.EcuInfo ecuInfo in pageInfo.ErrorsInfo.EcuList)
                {
                    if (_ediabasJobAbort)
                    {
                        break;
                    }
                    try
                    {
                        _ediabas.ResolveSgbdFile(ecuInfo.Sgbd);
                    }
                    catch (Exception ex)
                    {
                        string exText = EdiabasNet.GetExceptionText(ex);
                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, null, null, exText));
                        Thread.Sleep(10);
                        continue;
                    }

                    _ediabas.ArgString = string.Empty;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = "";

                    try
                    {
                        try
                        {
                            if (errorResetList != null && errorResetList.Any(ecu => ecu == ecuInfo.Name))
                            {   // error reset requested
                                _ediabas.ExecuteJob("FS_LOESCHEN");
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        _ediabas.ExecuteJob("FS_LESEN");

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(_ediabas.ResultSets);

                        bool jobOk = false;
                        if (resultSets.Count > 1)
                        {
                            EdiabasNet.ResultData resultData;
                            if (resultSets[resultSets.Count - 1].TryGetValue("JOB_STATUS", out resultData))
                            {
                                if (resultData.OpData is string)
                                {   // read details
                                    string jobStatus = (string)resultData.OpData;
                                    if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        jobOk = true;
                                    }
                                }
                            }
                        }

                        if (jobOk)
                        {
                            int dictIndex = 0;
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                            {
                                if (dictIndex == 0)
                                {
                                    dictIndex++;
                                    continue;
                                }

                                EdiabasNet.ResultData resultData;
                                if (resultDictLocal.TryGetValue("F_ORT_NR", out resultData))
                                {
                                    if (resultData.OpData is Int64)
                                    {   // read details
                                        _ediabas.ArgString = string.Format("0x{0:X02}", (Int64)resultData.OpData);
                                        _ediabas.ArgBinaryStd = null;
                                        _ediabas.ResultsRequests = ecuInfo.Results;

                                        _ediabas.ExecuteJob("FS_LESEN_DETAIL");

                                        List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail = new List<Dictionary<string, EdiabasNet.ResultData>>(_ediabas.ResultSets);
                                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, resultDictLocal,
                                            new List<Dictionary<string, EdiabasNet.ResultData>>(resultSetsDetail)));
                                    }
                                }
                                dictIndex++;
                            }
                        }
                        else
                        {
                            errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, null, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        string exText = EdiabasNet.GetExceptionText(ex);
                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, null, null, exText));
                        Thread.Sleep(10);
                        continue;
                    }
                    Thread.Sleep(10);
                }

                lock (DataLock)
                {
                    EdiabasErrorReportList = errorReportList;
                }
                return true;
            }
            // execute jobs

            bool firstRequestCall = false;
            if (_ediabasInitReq)
            {
                firstRequestCall = true;
                _ediabasJobAbort = false;

                if ((pageInfo.JobsInfo != null) && !string.IsNullOrEmpty(pageInfo.JobsInfo.Sgbd))
                {
                    try
                    {
                        _ediabas.ResolveSgbdFile(pageInfo.JobsInfo.Sgbd);
                    }
                    catch (Exception ex)
                    {
                        string exText = EdiabasNet.GetExceptionText(ex);
                        lock (DataLock)
                        {
                            EdiabasErrorMessage = exText;
                        }
                        Thread.Sleep(1000);
                        return false;
                    }
                }

                _ediabasInitReq = false;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = null;
            try
            {
                if ((pageInfo.JobsInfo != null) && (pageInfo.JobsInfo.JobList.Count > 0))
                {
                    foreach (JobReader.JobInfo jobInfo in pageInfo.JobsInfo.JobList)
                    {
                        if (!string.IsNullOrEmpty(jobInfo.Name))
                        {
                            if (firstRequestCall && !string.IsNullOrEmpty(jobInfo.ArgsFirst))
                            {
                                _ediabas.ArgString = jobInfo.ArgsFirst;
                            }
                            else
                            {
                                _ediabas.ArgString = jobInfo.Args;
                            }
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = jobInfo.Results;
                            _ediabas.ExecuteJob(jobInfo.Name);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                MergeResultDictionarys(ref resultDict, resultSets[1], jobInfo.Name + "#");
                            }
                        }
                    }
                }
                else
                {
                    if (pageInfo.ClassObject != null)
                    {
                        pageInfo.ClassObject.ExecuteJob(_ediabas, ref resultDict, firstRequestCall);
                    }
                }
            }
            catch (Exception ex)
            {
                _ediabasInitReq = true;
                string exText = EdiabasNet.GetExceptionText(ex);
                lock (DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorMessage = exText;
                }
                Thread.Sleep(1000);
                return false;
            }

            lock (DataLock)
            {
                EdiabasResultDict = resultDict;
                EdiabasErrorMessage = string.Empty;
            }
            Thread.Sleep(10);
            return true;
        }

        public static void MergeResultDictionarys(ref Dictionary<string, EdiabasNet.ResultData> resultDict, Dictionary<string, EdiabasNet.ResultData> mergeDict)
        {
            if (resultDict == null)
            {
                resultDict = mergeDict;
            }
            else
            {   // merge both dicts
                foreach (string key in mergeDict.Keys)
                {
                    if (!resultDict.ContainsKey(key))
                    {
                        resultDict.Add(key, mergeDict[key]);
                    }
                }
            }
        }

        public static void MergeResultDictionarys(ref Dictionary<string, EdiabasNet.ResultData> resultDict, Dictionary<string, EdiabasNet.ResultData> mergeDict, string prefix)
        {
            if (resultDict == null)
            {
                resultDict = new Dictionary<string,EdiabasNet.ResultData>();
            }

            foreach (string key in mergeDict.Keys)
            {
                string newKey = prefix + key;
                if (!resultDict.ContainsKey(newKey))
                {
                    resultDict.Add(newKey, mergeDict[key]);
                }
            }
        }

        private bool AbortEdiabasJob()
        {
            if (_ediabasJobAbort || _stopThread)
            {
                return true;
            }
            return false;
        }

        private void InitProperties(bool deviceChange = false)
        {
            if (!deviceChange)
            {
                Connected = false;
            }

            EdiabasResultDict = null;
            EdiabasErrorMessage = string.Empty;
            ErrorResetList = null;

            _ediabasInitReq = true;
            _ediabasJobAbort = deviceChange;
        }

        private void DataUpdatedEvent()
        {
            if (DataUpdated != null) DataUpdated(this, EventArgs.Empty);
        }

        private void ThreadTerminatedEvent()
        {
            if (ThreadTerminated != null) ThreadTerminated(this, EventArgs.Empty);
        }
    }
}
