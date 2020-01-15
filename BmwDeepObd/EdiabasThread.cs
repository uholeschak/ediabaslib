using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Android.Content;
using BmwFileReader;
using EdiabasLib;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace BmwDeepObd
{
    public class EdiabasThread : IDisposable
    {
        public class EdiabasErrorReport
        {
            public EdiabasErrorReport(string ecuName, string sgbd, string vagDataFileName, string vagUdsFileName, Dictionary<string, EdiabasNet.ResultData> errorDict, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet) :
                this(ecuName, sgbd, vagDataFileName, vagUdsFileName, errorDict, errorDetailSet, string.Empty)
            {
            }

            public EdiabasErrorReport(string ecuName, string sgbd, string vagDataFileName, string vagUdsFileName, Dictionary<string, EdiabasNet.ResultData> errorDict, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet, string execptionText)
            {
                EcuName = ecuName;
                Sgbd = sgbd;
                VagDataFileName = vagDataFileName;
                VagUdsFileName = vagUdsFileName;
                ErrorDict = errorDict;
                ErrorDetailSet = errorDetailSet;
                ExecptionText = execptionText;
            }

            public string EcuName { get; }

            public string Sgbd { get; }

            public string VagDataFileName { get; }

            public string VagUdsFileName { get; }

            public Dictionary<string, EdiabasNet.ResultData> ErrorDict { get; }

            public List<Dictionary<string, EdiabasNet.ResultData>> ErrorDetailSet { get; }

            public string ExecptionText { get; }
        }

        public class EdiabasErrorReportReset : EdiabasErrorReport
        {
            public EdiabasErrorReportReset(string ecuName, string sgbd, string vagDataFileName, string vagUdsFileName, Dictionary<string, EdiabasNet.ResultData> errorDict, bool errorResetOk) :
                base(ecuName, sgbd, vagDataFileName, vagUdsFileName, errorDict, null, string.Empty)
            {
                ErrorResetOk = errorResetOk;
            }

            public bool ErrorResetOk { get; }
        }

        public class EcuFunctionResult : EdiabasNet.ResultData
        {
            public EcuFunctionResult(EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct, EcuFunctionStructs.EcuJob ecuJob, EcuFunctionStructs.EcuJobResult ecuJobResult, EdiabasNet.ResultData resultData,
                string resultString, double? resultValue) :
                base(resultData.ResType, resultData.Name, resultData.OpData)
            {
                EcuFixedFuncStruct = ecuFixedFuncStruct;
                EcuJob = ecuJob;
                EcuJobResult = ecuJobResult;
                ResultString = resultString;
                ResultValue = resultValue;
            }

            public EcuFunctionStructs.EcuFixedFuncStruct EcuFixedFuncStruct { get; }

            public EcuFunctionStructs.EcuJob EcuJob { get; }

            public EcuFunctionStructs.EcuJobResult EcuJobResult { get; }

            public string ResultString { get; }

            public double? ResultValue { get; }
        }

        [DataContract]
        private class BroadcastItem
        {
            [DataMember]
            internal string Name;

            [DataMember]
            internal string Result;

            [DataMember]
            internal string Value;
        }

        [DataContract]
        private class BroadcastFrame
        {
            public BroadcastFrame()
            {
                ObdData = new List<BroadcastItem>();
            }

            [DataMember]
            internal string PageName;

            [DataMember]
            internal List<BroadcastItem> ObdData;
        }

        public delegate void DataUpdatedEventHandler(object sender, EventArgs e);
        public event DataUpdatedEventHandler DataUpdated;
        public delegate void PageChangedEventHandler(object sender, EventArgs e);
        public event PageChangedEventHandler PageChanged;
        public delegate void ThreadTerminatedEventHandler(object sender, EventArgs e);
        public event ThreadTerminatedEventHandler ThreadTerminated;

        public Context ActiveContext
        {
            get
            {
                lock (DataLock)
                {
                    return _context;
                }
            }
            set
            {
                lock (DataLock)
                {
                    _context = value;
                }
            }
        }

        private JobReader.PageInfo _jobPageInfo;
        public JobReader.PageInfo JobPageInfo
        {
            get => _jobPageInfo;
            set
            {
                bool changed = _jobPageInfo != value;
                _jobPageInfo = value;
                if (changed)
                {
                    PageChangedEvent();
                }
            }
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

        public MultiMap<string, EdiabasNet.ResultData> EdiabasResultDict
        {
            get;
            private set;
        }

        public string EdiabasErrorMessage
        {
            get;
            private set;
        }

        public int UpdateProgress
        {
            get;
            private set;
        }

        public JobReader.PageInfo ResultPageInfo
        {
            get;
            private set;
        }

        public List<string> ErrorResetList { get; set; }
        public string ErrorResetSgbdFunc { get; set; }
        public bool ErrorResetActive { get; private set; }
        public double? BatteryVoltage { get; private set; }
        public byte[] AdapterSerial { get; private set; }

        public EdiabasNet Ediabas { get; private set; }

        public static readonly Object DataLock = new Object();
        private const char DataLogSeparator = '\t';
        public const string NotificationBroadcastInfo = ActivityCommon.AppNameSpace + ".Notification.Info";
        private readonly string _resourceDatalogDate;

        private bool _disposed;
        private Context _context;
        private volatile bool _stopThread;
        private bool _threadRunning;
        private Thread _workerThread;
        private bool _ediabasInitReq;
        private bool _ediabasJobAbort;
        private JobReader.PageInfo _lastPageInfo;
        private long _lastUpdateTime;
        private long _lastBatteryUpdateTime;
        private string _vagPath;
        private string _logDir;
        private bool _appendLog;
        private StreamWriter _swDataLog;

        public EdiabasThread(string ecuPath, ActivityCommon activityCommon, Context context)
        {
            _resourceDatalogDate = context.GetString(Resource.String.datalog_date);
            _context = context;
            _stopThread = false;
            _threadRunning = false;
            _workerThread = null;
            Ediabas = new EdiabasNet
            {
                EdInterfaceClass = activityCommon.GetEdiabasInterfaceClass(),
                AbortJobFunc = AbortEdiabasJob
            };
            Ediabas.SetConfigProperty("EcuPath", ecuPath);

            InitProperties(null);
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
                    Ediabas.Dispose();
                    Ediabas = null;
                    CloseDataLog();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public bool StartThread(string comPort, object connectParameter, JobReader.PageInfo pageInfo, bool commActive, string vagPath, string traceDir, bool traceAppend, string logDir, bool appendLog)
        {
            if (_workerThread != null)
            {
                return false;
            }
            try
            {
                _stopThread = false;
                if (Ediabas.EdInterfaceClass is EdInterfaceObd)
                {
                    ((EdInterfaceObd)Ediabas.EdInterfaceClass).UdsDtcStatusOverride = ActivityCommon.UdsDtcStatusOverride;
                    ((EdInterfaceObd)Ediabas.EdInterfaceClass).ComPort = comPort;
                }
                else if (Ediabas.EdInterfaceClass is EdInterfaceEnet)
                {
                    if (!string.IsNullOrEmpty(comPort))
                    {
                        ((EdInterfaceEnet)Ediabas.EdInterfaceClass).RemoteHost = comPort;
                    }
                }
                Ediabas.EdInterfaceClass.ConnectParameter = connectParameter;
                if (!string.IsNullOrEmpty(traceDir))
                {
                    Ediabas.SetConfigProperty("TracePath", traceDir);
                    Ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                    Ediabas.SetConfigProperty("AppendTrace", traceAppend ? "1" : "0");
                    Ediabas.SetConfigProperty("CompressTrace", "1");
                }
                else
                {
                    Ediabas.SetConfigProperty("IfhTrace", "0");
                }
                CloseDataLog();
                CommActive = commActive;
                JobPageInfo = pageInfo;
                _lastPageInfo = null;
                _lastUpdateTime = Stopwatch.GetTimestamp();
                _lastBatteryUpdateTime = Stopwatch.GetTimestamp() - (100000 * ActivityCommon.TickResolMs);
                _vagPath = vagPath;
                _logDir = logDir;
                _appendLog = appendLog;
                InitProperties(null);
                _workerThread = new Thread(ThreadFunc);
                _threadRunning = true;
                _workerThread.Start();
                SendActionBroadcast("connect");
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
                    SendActionBroadcast("disconnect");
                    _workerThread.Join();
                    _workerThread = null;
                }
            }
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

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool OpenDataLog(JobReader.PageInfo pageInfo)
        {
            if (_swDataLog == null && pageInfo != null && !string.IsNullOrEmpty(_logDir) && !string.IsNullOrEmpty(pageInfo.LogFile))
            {
                try
                {
                    FileMode fileMode;
                    string logFileName = pageInfo.LogFile;
                    logFileName = logFileName.Replace("{D}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture));

                    string fileName = Path.Combine(_logDir, logFileName);
                    if (File.Exists(fileName))
                    {
                        fileMode = (_appendLog || ActivityCommon.JobReader.AppendLog) ? FileMode.Append : FileMode.Create;
                    }
                    else
                    {
                        fileMode = FileMode.Create;
                    }
                    _swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
                    if (fileMode == FileMode.Create)
                    {
                        // add header
                        StringBuilder sbLog = new StringBuilder();
                        sbLog.Append(_resourceDatalogDate);
                        foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                        {
                            if (!string.IsNullOrEmpty(displayInfo.LogTag))
                            {
                                sbLog.Append(DataLogSeparator);
                                sbLog.Append(displayInfo.LogTag.Replace(DataLogSeparator, ' '));
                            }
                        }
                        try
                        {
                            sbLog.Append("\r\n");
                            _swDataLog.Write(sbLog.ToString());
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        private void CloseDataLog()
        {
            if (_swDataLog != null)
            {
                _swDataLog.Dispose();
                _swDataLog = null;
            }
        }

        private string FormatResult(JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict)
        {
            string result = ActivityCommon.FormatResult(pageInfo, displayInfo, resultDict, out Android.Graphics.Color? _, out double? _);
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                if (ActivityCommon.VagUdsActive && !string.IsNullOrEmpty(pageInfo.JobsInfo.VagUdsFileName))
                {
                    string udsFileName = Path.Combine(_vagPath, pageInfo.JobsInfo.VagUdsFileName);
                    string resultUds = ActivityCommon.FormatResultVagUds(udsFileName, pageInfo, displayInfo, resultDict, out double? _);
                    if (!string.IsNullOrEmpty(resultUds))
                    {
                        result = resultUds;
                    }
                }
            }
            else
            {
                string resultEcuFunc = ActivityCommon.FormatResultEcuFunction(pageInfo, displayInfo, resultDict, out double? _);
                if (!string.IsNullOrEmpty(resultEcuFunc))
                {
                    result = resultEcuFunc;
                }
            }
            return result;
        }

        private void ProcessResults(JobReader.PageInfo pageInfo, MultiMap<string, EdiabasNet.ResultData> resultDict)
        {
            if (pageInfo.ClassObject != null)
            {
                Type pageType = pageInfo.ClassObject.GetType();
                MethodInfo processResult = pageType.GetMethod("ProcessResults", new[] { typeof(Context), typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>) });

                if (processResult != null)
                {
                    try
                    {
                        Context activeContext = ActiveContext;
                        if (activeContext != null)
                        {
                            object[] args = { activeContext, pageInfo, resultDict };
                            processResult.Invoke(pageInfo.ClassObject, args);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private void LogData(JobReader.PageInfo pageInfo, MultiMap<string, EdiabasNet.ResultData> resultDict)
        {
            if (_swDataLog == null)
            {
                return;
            }
            StringBuilder sbLog = new StringBuilder();
            string currDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            sbLog.Append(currDateTime);
            bool logDataPresent = false;
            foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
            {
                string result = FormatResult(pageInfo, displayInfo, resultDict);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(displayInfo.LogTag))
                    {
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            logDataPresent = true;
                        }
                        sbLog.Append(DataLogSeparator);
                        sbLog.Append(result.Replace(DataLogSeparator, ' '));
                    }
                }
            }
            if (logDataPresent)
            {
                try
                {
                    sbLog.Append("\r\n");
                    _swDataLog.Write(sbLog.ToString());
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void SendActionBroadcast(string action)
        {
            if (!ActivityCommon.SendDataBroadcast)
            {
                return;
            }
            //Android.Util.Log.Debug("Broadcast", action);
            Intent broadcastIntent = new Intent(NotificationBroadcastInfo);
            broadcastIntent.PutExtra("action", action);
            Context activeContext = ActiveContext;
            activeContext?.SendBroadcast(broadcastIntent);
        }

        private void SendInfoBroadcast(JobReader.PageInfo pageInfo, MultiMap<string, EdiabasNet.ResultData> resultDict)
        {
            if (!ActivityCommon.SendDataBroadcast)
            {
                return;
            }
            BroadcastFrame broadcastFrame = new BroadcastFrame
            {
                PageName = pageInfo.Name
            };
            foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
            {
                string result = FormatResult(pageInfo, displayInfo, resultDict);
                if (result != null)
                {
                    BroadcastItem broadcastItem = new BroadcastItem
                    {
                        Name = displayInfo.Name,
                        Result = displayInfo.Result,
                        Value = result
                    };
                    broadcastFrame.ObdData.Add(broadcastItem);
                }
            }
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BroadcastFrame));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, broadcastFrame);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    string jsonData = sr.ReadToEnd();
                    //Android.Util.Log.Debug("Broadcast", jsonData);
                    Intent broadcastIntent = new Intent(NotificationBroadcastInfo);
                    broadcastIntent.PutExtra("obd_data", jsonData);
                    Context activeContext = ActiveContext;
                    activeContext?.SendBroadcast(broadcastIntent);
                }
            }
        }

        private void ThreadFunc()
        {
            DataUpdatedEvent();
            _lastPageInfo = null;
            while (!_stopThread)
            {
                try
                {
                    if (!CommActive)
                    {
                        continue;
                    }
                    JobReader.PageInfo copyPageInfo = JobPageInfo;

                    if (_lastPageInfo != copyPageInfo)
                    {
                        _lastPageInfo = copyPageInfo;
                        InitProperties(copyPageInfo, true);
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
            CloseDataLog();
            ThreadTerminatedEvent();
        }

        private bool CommEdiabas(JobReader.PageInfo pageInfo)
        {
            if (Stopwatch.GetTimestamp() - _lastBatteryUpdateTime > 10000 * ActivityCommon.TickResolMs)
            {
                if (ActivityCommon.ReadBatteryVoltage(Ediabas, out double? batteryVoltage, out byte[] adapterSerial))
                {
                    _lastBatteryUpdateTime = Stopwatch.GetTimestamp();
                    lock (DataLock)
                    {
                        BatteryVoltage = batteryVoltage;
                        AdapterSerial = adapterSerial;
                    }
                }
            }

            if (pageInfo == null)
            {
                lock (DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorReportList = null;
                    EdiabasErrorMessage = "No Page info";
                    ResultPageInfo = null;
                    UpdateProgress = 0;
                }
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
                string errorResetSgbdFunc;
                lock (DataLock)
                {
                    errorResetList = ErrorResetList;
                    ErrorResetList = null;
                    errorResetSgbdFunc = ErrorResetSgbdFunc;
                    ErrorResetSgbdFunc = null;
                    ErrorResetActive = errorResetList != null || !string.IsNullOrEmpty(errorResetSgbdFunc);
                }

                List<EdiabasErrorReport> errorReportList = new List<EdiabasErrorReport>();

                try
                {
                    if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && !string.IsNullOrEmpty(errorResetSgbdFunc))
                    {
                        ActivityCommon.ResolveSgbdFile(Ediabas, errorResetSgbdFunc);

                        Ediabas.ArgString = string.Empty;
                        Ediabas.ArgBinaryStd = null;
                        Ediabas.ResultsRequests = string.Empty;
                        Ediabas.ExecuteJob("FS_LOESCHEN_FUNKTIONAL");

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                        if (resultSets.Count > 1)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDictOk = null;
                            int dictIndex = 0;
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                            {
                                if (dictIndex == 0)
                                {
                                    dictIndex++;
                                    continue;
                                }

                                if (IsJobStatusOk(resultDictLocal))
                                {
                                    resultDictOk = resultDictLocal;
                                    break;
                                }

                                dictIndex++;
                            }

                            if (resultDictOk != null)
                            {
                                errorReportList.Add(new EdiabasErrorReportReset(string.Empty, string.Empty, null, null, resultDictOk, true));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                int index = -1;
                foreach (JobReader.EcuInfo ecuInfo in pageInfo.ErrorsInfo.EcuList)
                {
                    index++;
                    if (_lastPageInfo != JobPageInfo)
                    {   // page change
                        break;
                    }
                    if (_ediabasJobAbort)
                    {
                        break;
                    }
                    try
                    {
                        ActivityCommon.ResolveSgbdFile(Ediabas, ecuInfo.Sgbd);
                    }
                    catch (Exception ex)
                    {
                        string exText = String.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exText = EdiabasNet.GetExceptionText(ex);
                        }
                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, null, null, exText));
                        continue;
                    }

                    try
                    {
                        try
                        {
                            if (errorResetList != null && errorResetList.Any(ecu => string.CompareOrdinal(ecu, ecuInfo.Name) == 0))
                            {   // error reset requested
                                Ediabas.ArgString = string.Empty;
                                Ediabas.ArgBinaryStd = null;
                                Ediabas.ResultsRequests = string.Empty;
                                Ediabas.ExecuteJob(ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw ? "FS_LOESCHEN" : "Fehlerspeicher_loeschen");

                                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                                if (resultSets.Count > 1)
                                {
                                    bool errorResetOk = false;
                                    string resultName;
                                    Dictionary<string, EdiabasNet.ResultData> resultDictCheck;
                                    if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                                    {
                                        resultName = "JOB_STATUS";
                                        resultDictCheck = resultSets[1];
                                    }
                                    else
                                    {
                                        resultName = "JOBSTATUS";
                                        resultDictCheck = resultSets[0];
                                    }
                                    if (resultDictCheck.TryGetValue(resultName, out EdiabasNet.ResultData resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            // read details
                                            string jobStatus = (string)resultData.OpData;
                                            if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                errorResetOk = true;
                                            }
                                        }
                                    }
                                    if (errorResetOk)
                                    {
                                        errorReportList.Add(new EdiabasErrorReportReset(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, resultDictCheck, errorResetOk));
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        Ediabas.ArgString = "ALL";
                        Ediabas.ArgBinaryStd = null;
                        Ediabas.ResultsRequests = string.Empty;
                        Ediabas.NoInitForVJobs = true;
                        Ediabas.ExecuteJob("_JOBS");    // force to load file

                        string errorJob = ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw ? "FS_LESEN" : "Fehlerspeicher_abfragen";
                        string argString = string.Empty;
                        if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && !string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                        {
                            argString = "MW_LESEN";
                        }
                        if (Ediabas.IsJobExisting(errorJob))
                        {
                            Ediabas.ArgString = argString;
                            Ediabas.ArgBinaryStd = null;
                            Ediabas.ResultsRequests = string.Empty;
                            Ediabas.ExecuteJob(errorJob);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);

                            bool jobOk = false;
                            bool saeMode = false;
                            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                            {
                                if (resultSets.Count > 0)
                                {
                                    if (resultSets[0].TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            // read details
                                            string jobStatus = (string) resultData.OpData;
                                            if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                jobOk = true;
                                            }
                                        }
                                    }
                                }
                                if (!jobOk)
                                {
                                    if (Ediabas.IsJobExisting("FehlerspeicherSAE_abfragen"))
                                    {
                                        Ediabas.ArgString = "MW_LESEN";
                                        Ediabas.ArgBinaryStd = null;
                                        Ediabas.ResultsRequests = string.Empty;
                                        Ediabas.ExecuteJob("FehlerspeicherSAE_abfragen");
                                        resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                                        if (resultSets.Count > 0)
                                        {
                                            if (resultSets[0].TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                                            {
                                                if (resultData.OpData is string)
                                                {
                                                    // read details
                                                    string jobStatus = (string) resultData.OpData;
                                                    if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                                    {
                                                        jobOk = true;
                                                        saeMode = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        jobOk = true;
                                    }
                                }
                            }
                            else
                            {
                                if (resultSets.Count > 1)
                                {
                                    if (IsJobStatusOk(resultSets[resultSets.Count - 1]))
                                    {
                                        jobOk = true;
                                    }
                                }
                            }

                            if (jobOk)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict0 = null;
                                int dictIndex = 0;
                                foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                                {
                                    EdiabasNet.ResultData resultData;
                                    if (dictIndex == 0)
                                    {
                                        resultDict0 = resultDictLocal;
                                        dictIndex++;
                                        continue;
                                    }

                                    if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                                    {
                                        if (resultDictLocal.TryGetValue("FNR_WERT", out resultData))
                                        {
                                            if (resultData.OpData is Int64)
                                            {
                                                Dictionary<string, EdiabasNet.ResultData> resultDictTemp = null;
                                                MergeResultDictionarys(ref resultDictTemp, resultDictLocal);
                                                MergeResultDictionarys(ref resultDictTemp, resultDict0);
                                                resultDictTemp.Add("SAE", new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeI, "SAE", (Int64) (saeMode ? 1 : 0)));
                                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, resultDictTemp, null));
                                            }
                                        }
                                        dictIndex++;
                                        continue;
                                    }
                                    // BMW only
                                    if (resultDictLocal.TryGetValue("F_ORT_NR", out resultData))
                                    {
                                        if (resultData.OpData is Int64)
                                        {
                                            // read details
                                            Ediabas.ArgString = string.Format("0x{0:X02}", (Int64) resultData.OpData);
                                            Ediabas.ArgBinaryStd = null;
                                            Ediabas.ResultsRequests = string.Empty;

                                            bool details;
                                            try
                                            {
                                                Ediabas.ExecuteJob("FS_LESEN_DETAIL");
                                                details = true;
                                            }
                                            catch (Exception)
                                            {
                                                // no details
                                                details = false;
                                            }

                                            if (details)
                                            {
                                                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, resultDictLocal, resultSetsDetail));
                                            }
                                            else
                                            {
                                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, resultDictLocal, null));
                                            }
                                        }
                                    }
                                    dictIndex++;
                                }
                            }
                            else
                            {
                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, null, null));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exText = String.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exText = EdiabasNet.GetExceptionText(ex);
                        }
                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, null, null, exText));
                        continue;
                    }
                    if (EdiabasErrorReportList == null)
                    {   // first update
                        lock (DataLock)
                        {
                            UpdateProgress = index * 100 / pageInfo.ErrorsInfo.EcuList.Count;
                        }
                        DataUpdatedEvent();
                    }
                }

                lock (DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorReportList = errorReportList;
                    EdiabasErrorMessage = string.Empty;
                    ResultPageInfo = pageInfo;
                    UpdateProgress = 100;
                }
                return true;
            }
            // execute jobs

            bool firstRequestCall = false;
            string currentSgbd = string.Empty;
            if (_ediabasInitReq)
            {
                firstRequestCall = true;
                _ediabasJobAbort = false;

                if (!string.IsNullOrEmpty(pageInfo.JobsInfo?.Sgbd))
                {
                    try
                    {
                        ActivityCommon.ResolveSgbdFile(Ediabas, pageInfo.JobsInfo?.Sgbd);
                    }
                    catch (Exception ex)
                    {
                        string exText = String.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exText = EdiabasNet.GetExceptionText(ex);
                        }
                        lock (DataLock)
                        {
                            EdiabasResultDict = null;
                            EdiabasErrorReportList = null;
                            EdiabasErrorMessage = exText;
                            ResultPageInfo = pageInfo;
                            UpdateProgress = 0;
                        }
                        return false;
                    }
                    currentSgbd = pageInfo.JobsInfo.Sgbd;
                }

                _ediabasInitReq = false;
            }

            MultiMap<string, EdiabasNet.ResultData> resultDict = null;
            try
            {
                if ((pageInfo.JobsInfo != null) && (pageInfo.JobsInfo.JobList.Count > 0))
                {
                    foreach (JobReader.JobInfo jobInfo in pageInfo.JobsInfo.JobList)
                    {
                        if (!string.IsNullOrEmpty(jobInfo.Name))
                        {
                            string sgbd = string.Empty;
                            if (!string.IsNullOrEmpty(jobInfo.Sgbd))
                            {
                                sgbd = jobInfo.Sgbd;
                            }
                            else if (!string.IsNullOrEmpty(pageInfo.JobsInfo.Sgbd))
                            {
                                sgbd = pageInfo.JobsInfo.Sgbd;
                            }

                            if (!string.IsNullOrEmpty(sgbd) &&
                                string.Compare(currentSgbd, sgbd, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                ActivityCommon.ResolveSgbdFile(Ediabas, sgbd);
                                currentSgbd = sgbd;
                            }

                            EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct = GetEcuFixedFuncStruct(currentSgbd, jobInfo);
                            if (ecuFixedFuncStruct != null)
                            {
                                if (ecuFixedFuncStruct.EcuJobList != null)
                                {
                                    foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                                    {
                                        if (!string.IsNullOrWhiteSpace(displayInfo.EcuJobId))
                                        {
                                            List<EcuFunctionResult> ecuFunctionResultList = ExecuteEcuJobs(Ediabas, ecuFixedFuncStruct, displayInfo.EcuJobId);
                                            if (ecuFunctionResultList != null)
                                            {
                                                Dictionary<string, EdiabasNet.ResultData> resultDictLocal = new Dictionary<string, EdiabasNet.ResultData>();
                                                foreach (EcuFunctionResult ecuFunctionResult in ecuFunctionResultList)
                                                {
                                                    string key = (ecuFunctionResult.EcuJob.Id + "#" + ecuFunctionResult.EcuJobResult.Id).ToUpperInvariant();
                                                    resultDictLocal.Add(key, ecuFunctionResult);
                                                }

                                                MergeResultDictionarys(ref resultDict, resultDictLocal, ecuFixedFuncStruct.Id + "#");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(jobInfo.FixedFuncStructId))
                                {
                                    if (firstRequestCall && !string.IsNullOrEmpty(jobInfo.ArgsFirst))
                                    {
                                        Ediabas.ArgString = jobInfo.ArgsFirst;
                                    }
                                    else
                                    {
                                        Ediabas.ArgString = jobInfo.Args;
                                    }
                                    Ediabas.ArgBinaryStd = null;
                                    Ediabas.ResultsRequests = jobInfo.Results;
                                    Ediabas.ExecuteJob(jobInfo.Name);

                                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = Ediabas.ResultSets;
                                    if (resultSets != null && resultSets.Count >= 2)
                                    {
                                        int dictIndex = 0;
                                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                                        {
                                            if (dictIndex == 0)
                                            {
                                                dictIndex++;
                                                continue;
                                            }
                                            if (string.IsNullOrEmpty(jobInfo.Id))
                                            {
                                                MergeResultDictionarys(ref resultDict, resultDictLocal, jobInfo.Name + "#");
                                            }
                                            else
                                            {
                                                MergeResultDictionarys(ref resultDict, resultDictLocal, jobInfo.Id + "#" + dictIndex + "#");
                                            }
                                            dictIndex++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (pageInfo.ClassObject != null)
                    {
                        bool executeJob = false;
                        bool executeJobMulti = false;
                        Type pageType = pageInfo.ClassObject.GetType();
                        MethodInfo methodInfoJob = pageType.GetMethod("ExecuteJob");
                        if (methodInfoJob != null)
                        {
                            ParameterInfo[] parInfo = methodInfoJob.GetParameters();
                            if (parInfo.Length == 3)
                            {
                                if (parInfo[0].ParameterType == typeof(EdiabasNet) && parInfo[2].ParameterType == typeof(bool))
                                {
                                    if (parInfo[1].ParameterType == typeof(Dictionary<string, EdiabasNet.ResultData>).MakeByRefType())
                                    {
                                        executeJob = true;
                                    }
                                    if (parInfo[1].ParameterType == typeof(MultiMap<string, EdiabasNet.ResultData>).MakeByRefType())
                                    {
                                        executeJobMulti = true;
                                    }
                                }
                            }
                            if (executeJobMulti)
                            {
                                object[] args = { Ediabas, null, firstRequestCall };
                                methodInfoJob.Invoke(pageInfo.ClassObject, args);
                                resultDict = args[1] as MultiMap<string, EdiabasNet.ResultData>;
                                //pageInfo.ClassObject.ExecuteJob(Ediabas, ref resultDict, firstRequestCall);
                            }
                            else if (executeJob)
                            {
                                object[] args = { Ediabas, null, firstRequestCall };
                                methodInfoJob.Invoke(pageInfo.ClassObject, args);
                                // ReSharper disable once UsePatternMatching
                                Dictionary<string, EdiabasNet.ResultData> resultDictLocal = args[1] as Dictionary<string, EdiabasNet.ResultData>;
                                //pageInfo.ClassObject.ExecuteJob(Ediabas, ref resultDictLocal, firstRequestCall);
                                if (resultDictLocal != null)
                                {
                                    MergeResultDictionarys(ref resultDict, resultDictLocal);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ediabasInitReq = true;
                string exText = String.Empty;
                if (!AbortEdiabasJob())
                {
                    exText = EdiabasNet.GetExceptionText(ex);
                }
                lock (DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorReportList = null;
                    EdiabasErrorMessage = exText;
                    ResultPageInfo = pageInfo;
                    UpdateProgress = 0;
                }
                return false;
            }

            ProcessResults(pageInfo, resultDict);
            LogData(pageInfo, resultDict);
            SendInfoBroadcast(pageInfo, resultDict);

            lock (DataLock)
            {
                EdiabasResultDict = resultDict;
                EdiabasErrorReportList = null;
                EdiabasErrorMessage = string.Empty;
                ResultPageInfo = pageInfo;
                UpdateProgress = 0;
            }
            return true;
        }

        public static bool IsJobStatusOk(Dictionary<string, EdiabasNet.ResultData> resultDict)
        {
            if (resultDict.TryGetValue("JOB_STATUS", out EdiabasNet.ResultData resultData))
            {
                if (resultData.OpData is string)
                {
                    // read details
                    string jobStatus = (string)resultData.OpData;
                    if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void MergeResultDictionarys(ref Dictionary<string, EdiabasNet.ResultData> resultDict, Dictionary<string, EdiabasNet.ResultData> mergeDict, string prefix = null)
        {
            if (resultDict == null)
            {
                resultDict = new Dictionary<string, EdiabasNet.ResultData>();
            }

            foreach (string key in mergeDict.Keys)
            {
                string newKey = key;
                if (prefix != null)
                {
                    newKey = (prefix + key).ToUpperInvariant();
                }
                if (!resultDict.ContainsKey(newKey))
                {
                    resultDict.Add(newKey, mergeDict[key]);
                }
            }
        }

        public static void MergeResultDictionarys(ref MultiMap<string, EdiabasNet.ResultData> resultDict, Dictionary<string, EdiabasNet.ResultData> mergeDict, string prefix = null)
        {
            if (resultDict == null)
            {
                resultDict = new MultiMap<string,EdiabasNet.ResultData>();
            }

            foreach (string key in mergeDict.Keys)
            {
                string newKey = key;
                if (prefix != null)
                {
                    newKey = (prefix + key).ToUpperInvariant();
                }
                resultDict.Add(newKey, mergeDict[key]);
            }
        }

        public static EcuFunctionStructs.EcuFixedFuncStruct GetEcuFixedFuncStruct(string sgbd, JobReader.JobInfo jobInfo)
        {
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(jobInfo.FixedFuncStructId))
            {
                return null;
            }

            EcuFunctionStructs.EcuVariant ecuVariant = null;
            if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
            {
                string ecuSgbdName = sgbd ?? string.Empty;
                ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(ecuSgbdName);
            }

            if (ecuVariant == null)
            {
                return null;
            }

            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList = ActivityCommon.EcuFunctionReader.GetFixedFuncStructList(ecuVariant);
            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in fixedFuncStructList)
            {
                if (string.Compare(ecuFixedFuncStruct.Id, jobInfo.FixedFuncStructId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return ecuFixedFuncStruct;
                }
            }

            return null;
        }

        public static List<EcuFunctionResult> ExecuteEcuJobs(EdiabasNet ediabas, EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct,
            string ecuJobId = null, EcuFunctionStructs.EcuJob.PhaseType phaseType = EcuFunctionStructs.EcuJob.PhaseType.Unknown)
        {
            List<EcuFunctionResult> ecuFunctionResultList = new List<EcuFunctionResult>();
            EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType nodeClassType = ecuFixedFuncStruct.GetNodeClassType();
            List<EcuFunctionStructs.EcuJob> ecuJobList = new List<EcuFunctionStructs.EcuJob>();
            if (ecuFixedFuncStruct.EcuJobList != null)
            {
                foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                {
                    if (string.IsNullOrEmpty(ecuJobId) ||
                        string.Compare(ecuJob.Id, ecuJobId, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        bool addJob = false;
                        if (nodeClassType == EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator &&
                            phaseType != EcuFunctionStructs.EcuJob.PhaseType.Unknown)
                        {
                            if (ecuJob.GetPhaseType() == phaseType)
                            {
                                addJob = true;
                            }
                        }
                        else
                        {
                            addJob = true;
                        }

                        if (addJob)
                        {
                            ecuJobList.Add(ecuJob);
                        }
                    }
                }
            }

            if (nodeClassType == EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator)
            {
                ecuJobList = ecuJobList.OrderBy(ecuJob => ecuJob.Rank.ConvertToInt()).ToList();
            }

            foreach (EcuFunctionStructs.EcuJob ecuJob in ecuJobList)
            {
                List<EcuFunctionResult> resultList = ExecuteEcuJob(ediabas, ecuJob, ecuFixedFuncStruct);
                if (resultList == null)
                {
                    return null;
                }

                if (resultList.Count > 0)
                {
                    ecuFunctionResultList.AddRange(resultList);
                }
            }

            return ecuFunctionResultList;
        }

        public static List<EcuFunctionResult> ExecuteEcuJob(EdiabasNet ediabas, EcuFunctionStructs.EcuJob ecuJob, EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct)
        {
            EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType nodeClassType = ecuFixedFuncStruct.GetNodeClassType();
            StringBuilder sbParameter = new StringBuilder();
            if (ecuJob.EcuJobParList != null)
            {
                foreach (EcuFunctionStructs.EcuJobParameter ecuJobParameter in ecuJob.EcuJobParList.OrderBy(x => x.Name))
                {
                    if (sbParameter.Length > 0)
                    {
                        sbParameter.Append(";");
                    }
                    sbParameter.Append(ecuJobParameter.Value);
                }
            }

            ediabas.ArgString = sbParameter.ToString();
            ediabas.ArgBinaryStd = null;
            ediabas.ResultsRequests = string.Empty;
            ediabas.ExecuteJob(ecuJob.Name);

            bool jobOk = false;
            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(ediabas.ResultSets);
            if (resultSets.Count > 1)
            {
                int dictIndex = 0;
                foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                {
                    if (dictIndex == 0)
                    {
                        dictIndex++;
                        continue;
                    }

                    if (IsJobStatusOk(resultDictLocal))
                    {
                        jobOk = true;
                        break;
                    }

                    dictIndex++;
                }
            }

            List<EcuFunctionResult> ecuFunctionResultList = null;
            if (jobOk)
            {
                ecuFunctionResultList = new List<EcuFunctionResult>();
                if (ecuJob.EcuJobResultList != null)
                {
                    foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJob.EcuJobResultList)
                    {
                        if (ecuJobResult.EcuFuncRelevant.ConvertToInt() <= 0)
                        {
                            continue;
                        }

                        int dictIndex = 0;
                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                        {
                            if (dictIndex == 0)
                            {
                                dictIndex++;
                                continue;
                            }

                            bool statusOk = IsJobStatusOk(resultDictLocal);
                            if (statusOk)
                            {
                                if (resultDictLocal.TryGetValue(ecuJobResult.Name.ToUpperInvariant(), out EdiabasNet.ResultData resultData))
                                {
                                    if (ecuJobResult.EcuFuncRelevant.ConvertToInt() > 0)
                                    {
                                        string resultString = null;
                                        double? resultValue = null;
                                        if (ecuJobResult.EcuResultStateValueList != null && ecuJobResult.EcuResultStateValueList.Count > 0)
                                        {
                                            EcuFunctionStructs.EcuResultStateValue ecuResultStateValue = MatchEcuResultStateValue(ecuJobResult.EcuResultStateValueList, resultData);
                                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                            if (ecuResultStateValue != null)
                                            {
                                                resultString = ecuResultStateValue.Title?.GetTitle(ActivityCommon.GetCurrentLanguage());
                                            }
                                        }

                                        if (resultString == null)
                                        {
                                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                            if (nodeClassType == EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.Identification)
                                            {
                                                resultString = ConvertEcuResultValueDefault(resultData, out resultValue);
                                            }
                                            else
                                            {
                                                resultString = ConvertEcuResultValueStatus(ecuJobResult, resultData, out resultValue);
                                            }
                                        }

                                        ecuFunctionResultList.Add(new EcuFunctionResult(ecuFixedFuncStruct, ecuJob, ecuJobResult, resultData, resultString, resultValue));
                                    }
                                }
                            }
                            dictIndex++;
                        }
                    }
                }
            }

            return ecuFunctionResultList;
        }

        public static string ConvertEcuResultValueDefault(EdiabasNet.ResultData resultData, out double? resultValue)
        {
            resultValue = null;
            try
            {
                string result = string.Empty;
                if (resultData.OpData is Int64)
                {
                    Int64 value = (Int64)resultData.OpData;
                    result = value.ToString(CultureInfo.InvariantCulture);
                    resultValue = value;
                }
                else if (resultData.OpData is Double)
                {
                    Double value = (Double)resultData.OpData;
                    result = value.ToString("0.00", CultureInfo.InvariantCulture);
                    resultValue = value;
                }
                else if (resultData.OpData is string)
                {
                    string value = (string)resultData.OpData;
                    result = value;
                }
                else if (resultData.OpData.GetType() == typeof(byte[]))
                {
                    StringBuilder sb = new StringBuilder();
                    byte[] data = (byte[])resultData.OpData;
                    foreach (byte value in data)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02} ", value));
                    }
                    result = sb.ToString();
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ConvertEcuEnvCondResultValue(EcuFunctionStructs.EcuEnvCondLabel envCondLabel, EdiabasNet.ResultData resultData, out double? resultValue)
        {
            resultValue = null;
            string resultString = null;
            if (envCondLabel.EcuResultStateValueList != null && envCondLabel.EcuResultStateValueList.Count > 0)
            {
                EcuFunctionStructs.EcuResultStateValue ecuResultStateValue = MatchEcuResultStateValue(envCondLabel.EcuResultStateValueList, resultData);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (ecuResultStateValue != null)
                {
                    resultString = ecuResultStateValue.Title?.GetTitle(ActivityCommon.GetCurrentLanguage());
                }
            }
            
            if (resultString == null)
            {
                resultString = ConvertEcuResultValueDefault(resultData, out resultValue);
            }

            return resultString;
        }

        public static string ConvertEcuResultValueStatus(EcuFunctionStructs.EcuJobResult ecuJobResult, EdiabasNet.ResultData resultData, out double? resultValue)
        {
            resultValue = null;
            try
            {
                string result = string.Empty;
                double? number = null;
                if (resultData.OpData is Int64)
                {
                    Int64 value = (Int64)resultData.OpData;
                    result = value.ToString(CultureInfo.InvariantCulture);
                    number = value;
                }
                else if (resultData.OpData is Double)
                {
                    Double value = (Double)resultData.OpData;
                    result = value.ToString(CultureInfo.InvariantCulture);
                    number = value;
                }
                else if (resultData.OpData is string)
                {
                    string value = (string)resultData.OpData;
                    result = value;
                    try
                    {
                        number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                else if (resultData.OpData.GetType() == typeof(byte[]))
                {
                    StringBuilder sb = new StringBuilder();
                    byte[] data = (byte[])resultData.OpData;
                    foreach (byte value in data)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02} ", value));
                    }
                    result = sb.ToString();
                }

                if (number != null)
                {
                    double mult = 1;
                    double offset = 0;

                    try
                    {
                        mult = Convert.ToDouble(ecuJobResult.Mult, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        offset = Convert.ToDouble(ecuJobResult.Offset, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    number = number.Value * mult + offset;
                    resultValue = number.Value;

                    result = UseEcuResultNumberFormat(number.Value, ecuJobResult);
                    if (string.IsNullOrEmpty(result))
                    {
                        result = TryEcuResultRounding(number.Value, ecuJobResult);
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        result = number.ToString();
                    }
                }

                if (!string.IsNullOrWhiteSpace(ecuJobResult.Unit))
                {
                    result += " " + ecuJobResult.Unit;
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string UseEcuResultNumberFormat(double value, EcuFunctionStructs.EcuJobResult ecuJobResult)
        {
            try
            {
                string numberFormat = ecuJobResult.NumberFormat;
                if (string.IsNullOrWhiteSpace(numberFormat))
                {
                    return null;
                }

                if (numberFormat.Contains(","))
                {
                    numberFormat = numberFormat.Replace(",", ".");
                }

                if (numberFormat.Contains("#"))
                {
                    int index = numberFormat.IndexOf(".", StringComparison.Ordinal);
                    if (-1 < index && index < numberFormat.Length - 1)
                    {
                        numberFormat = "{0:0." + numberFormat.Substring(numberFormat.IndexOf('.') + 1) + "}";
                    }
                    else
                    {
                        numberFormat = "{0:0}";
                    }
                }
                else
                {
                    numberFormat = "{0:" + numberFormat + "}";
                }

                string result = string.Format(CultureInfo.InvariantCulture, numberFormat, value);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string TryEcuResultRounding(double value, EcuFunctionStructs.EcuJobResult ecuJobResult)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ecuJobResult.NumberFormat))
                {
                    return null;
                }

                string result;
                if (!string.IsNullOrWhiteSpace(ecuJobResult.Round) && ecuJobResult.Round.ConvertToInt() > 0)
                {
                    string text = value.ToString(CultureInfo.InvariantCulture);
                    int num = text.Contains(".") ? text.LastIndexOf(".", StringComparison.Ordinal) : text.LastIndexOf(",", StringComparison.Ordinal);
                    int length = text.Substring(num + 1, text.Length - num - 1).Length;
                    double number = Math.Round(value, length - 1, MidpointRounding.ToEven);
                    result = number.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    result = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", value);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EcuFunctionStructs.EcuResultStateValue MatchEcuResultStateValue(List<EcuFunctionStructs.EcuResultStateValue> ecuResultStateValueList, EdiabasNet.ResultData resultData)
        {
            try
            {
                if (ecuResultStateValueList == null)
                {
                    return null;
                }

                Predicate<EcuFunctionStructs.EcuResultStateValue> predicateStateValue = null;
                EcuFunctionStructs.EcuResultStateValue ecuResultStateValue = null;
                if (resultData.OpData is Int64)
                {
                    Int64 value = (Int64)resultData.OpData;
                    predicateStateValue = stateValue => Convert.ToInt64(stateValue.StateValue, CultureInfo.InvariantCulture) == value;
                }
                else if (resultData.OpData is Double)
                {
                    Double value = (Double)resultData.OpData;
                    predicateStateValue = stateValue => Math.Abs(value - Convert.ToDouble(stateValue.StateValue, CultureInfo.InvariantCulture)) < 0.0001;
                }
                else if (resultData.OpData is string)
                {
                    string value = (string)resultData.OpData;
                    predicateStateValue = stateValue => stateValue.StateValue.Equals(value, StringComparison.OrdinalIgnoreCase);
                }

                if (predicateStateValue != null)
                {
                    ecuResultStateValue = ecuResultStateValueList.FirstOrDefault(stateValue => predicateStateValue(stateValue));
                }
                return ecuResultStateValue;
            }
            catch (Exception)
            {
                return null;
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

        private void InitProperties(JobReader.PageInfo pageInfo, bool deviceChange = false)
        {
            if (!deviceChange)
            {
                Connected = false;
            }
            CloseDataLog();
            EdiabasResultDict = null;
            EdiabasErrorMessage = string.Empty;
            EdiabasErrorReportList = null;
            ErrorResetList = null;
            ErrorResetSgbdFunc = null;
            ErrorResetActive = false;
            BatteryVoltage = null;
            AdapterSerial = null;
            ResultPageInfo = null;
            UpdateProgress = 0;

            _ediabasInitReq = true;
            _ediabasJobAbort = deviceChange;
            OpenDataLog(pageInfo);
            if (pageInfo != null)
            {
                SendActionBroadcast("page_change");
            }
        }

        private void DataUpdatedEvent()
        {
            while (Stopwatch.GetTimestamp() - _lastUpdateTime < 100 * ActivityCommon.TickResolMs)
            {
                if (AbortEdiabasJob())
                {
                    break;
                }
                Thread.Sleep(100);
            }
            _lastUpdateTime = Stopwatch.GetTimestamp();
            DataUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void PageChangedEvent()
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ThreadTerminatedEvent()
        {
            ThreadTerminated?.Invoke(this, EventArgs.Empty);
        }
    }
}
