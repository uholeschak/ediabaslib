using EdiabasLib;
using System;
using System.Collections.Generic;
using System.Threading;
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace CarControlAndroid
{
    public class EdiabasThread : IDisposable
    {
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

        public bool StartThread(string comPort, string logDir, JobReader.PageInfo pageInfo, bool commActive)
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
                if (!string.IsNullOrEmpty(logDir))
                {
                    _ediabas.SetConfigProperty("TracePath", logDir);
                    _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
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
            bool firstRequestCall = false;
            if (_ediabasInitReq)
            {
                firstRequestCall = true;
                _ediabasJobAbort = false;

                if (!string.IsNullOrEmpty(pageInfo.JobInfo.Sgbd))
                {
                    try
                    {
                        _ediabas.ResolveSgbdFile(pageInfo.JobInfo.Sgbd);
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
                if (!string.IsNullOrEmpty(pageInfo.JobInfo.Name))
                {
                    _ediabas.ArgString = pageInfo.JobInfo.Args;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = pageInfo.JobInfo.Results;
                    _ediabas.ExecuteJob(pageInfo.JobInfo.Name);

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        MergeResultDictionarys(ref resultDict, resultSets[1]);
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
