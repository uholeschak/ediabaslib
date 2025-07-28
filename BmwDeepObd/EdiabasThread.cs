using Android.Content;
using Android.Util;
using BmwFileReader;
using EdiabasLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
// ReSharper disable StringLiteralTypo

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

namespace BmwDeepObd
{
    public class EdiabasThread : IDisposable
    {
        public enum UpdateState
        {
            Init,
            Error,
            DetectVehicle,
            ReadErrors,
            Connected
        }

        public class EdiabasErrorReport
        {
            public EdiabasErrorReport(string ecuName, string sgbd, string sgbdResolved, string vagDataFileName, string vagUdsFileName, bool readIs, bool isValid, bool isVisible, Dictionary<string, EdiabasNet.ResultData> errorDict = null, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet = null, string exceptionText = null)
            {
                EcuName = ecuName;
                Sgbd = sgbd;
                SgbdResolved = sgbdResolved;
                VagDataFileName = vagDataFileName;
                VagUdsFileName = vagUdsFileName;
                ReadIs = readIs;
                IsValid = isValid;
                IsVisible = isVisible;
                ErrorDict = errorDict;
                ErrorDetailSet = errorDetailSet;
                ExceptionText = exceptionText;
                EcuVariant = null;
                LifeStartDate = null;
            }

            public string EcuName { get; }

            public string Sgbd { get; }

            public string SgbdResolved { get; }

            public string VagDataFileName { get; }

            public string VagUdsFileName { get; }

            public bool ReadIs { get; }

            public bool IsValid { get; }

            public bool IsVisible { get; }

            public Dictionary<string, EdiabasNet.ResultData> ErrorDict { get; }

            public List<Dictionary<string, EdiabasNet.ResultData>> ErrorDetailSet { get; }

            public string ExceptionText { get; }

            public EcuFunctionStructs.EcuVariant EcuVariant { get; set; }

            public DateTime? LifeStartDate { get; set; }
        }

        public class EdiabasErrorShadowReport : EdiabasErrorReport
        {
            public EdiabasErrorShadowReport(string ecuName, string sgbd, string sgbdResolved, Dictionary<string, EdiabasNet.ResultData> errorDict) :
                base(ecuName, sgbd, sgbdResolved, null, null, false, true, true, errorDict)
            {
            }
        }

        public class EdiabasErrorReportReset : EdiabasErrorReport
        {
            public enum ErrorRestState
            {
                Undefined,
                Ok,
                Failed,
                Condition
            }

            public EdiabasErrorReportReset(string ecuName, string sgbd, string sgbdResolved, string vagDataFileName, string vagUdsFileName, bool resetIs, ErrorRestState resetState) :
                base(ecuName, sgbd, sgbdResolved, vagDataFileName, vagUdsFileName, resetIs, true, true)
            {
                ResetState = resetState;
            }

            public void Reset()
            {
                ResetState = ErrorRestState.Undefined;
            }

            public ErrorRestState ResetState { get; private set; }
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

        private class EnvCondResultInfo
        {
            public EnvCondResultInfo(string result, string unit = null, int? resourceId = null, int? minLength = null, double? minValue = null, bool? convertDate = false)
            {
                Result = result;
                Unit = unit;
                ResourceId = resourceId;
                MinLength = minLength;
                MinValue = minValue;
                ConvertDate = convertDate;
            }

            public string Result { get; }

            public string Unit { get; }

            public int? ResourceId { get; }

            public int? MinLength { get; }

            public double? MinValue { get; }

            public bool? ConvertDate { get; }
        }

        private class EnvCondDetailInfo
        {
            public EnvCondDetailInfo(string name, string value)
            {
                Name = name;
                ValueList = new List<string> { value };
                Index = null;
            }

            public string Name { get; }

            public List<string> ValueList { get; }

            public int? Index { get; set; }
        }

        private class LogInfo
        {
            public LogInfo(string info)
            {
                Info = info;
            }

            public LogInfo(string format, params object[] args)
            {
                Format = format;
                Args = args;
            }

            public string Info { get; }

            public string Format { get; }

            public object[] Args { get; }
        }

        public delegate void DataUpdatedEventHandler(object sender, EventArgs e);
        public event DataUpdatedEventHandler DataUpdated;
        public delegate void PageChangedEventHandler(object sender, EventArgs e);
        public event PageChangedEventHandler PageChanged;
        public delegate void ThreadTerminatedEventHandler(object sender, EventArgs e);
        public event ThreadTerminatedEventHandler ThreadTerminated;
        public delegate Tuple<string, string> TranslateDelegate(Tuple<string, string> tupleText, ref List<string> transList);

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
                if (value == null)
                {
                    return;
                }

                bool changed = _jobPageInfo != value;
                if (changed)
                {
                    CommActive = !value.JobActivate;
                    _jobPageInfo = value;
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

        public UpdateState UpdateProgressState
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

        public DetectVehicleBmw DetectVehicleBmw
        {
            get => _detectVehicleBmw;
        }

#if DEBUG
        private static readonly string Tag = typeof(EdiabasThread).FullName;
#endif
        public static readonly Object DataLock = new Object();
        public const int UdsFuncAddr = 0x7DF;
        private const char DataLogSeparator = '\t';
        public const int UpdateDisplayDelay = 200;
        public const string NotificationBroadcastInfo = ActivityCommon.AppNameSpace + ".Notification.Info";
        public static readonly Tuple<string, bool>[] ErrorFaultModeResultList =
        {
            new Tuple<string, bool>("F_VORHANDEN_NR", false),
            new Tuple<string, bool>("F_SYMPTOM_NR", false),
            new Tuple<string, bool>("F_FEHLERKLASSE_NR", true),
            new Tuple<string, bool>("F_WARNUNG_NR", true),
        };

        private static readonly EnvCondResultInfo[] ErrorEnvCondResultList =
        {
            new EnvCondResultInfo("F_HFK", null, Resource.String.error_env_frequency),
            new EnvCondResultInfo("F_PCODE_STRING", null, null, 4),
            new EnvCondResultInfo("F_HLZ"),
            new EnvCondResultInfo("F_LZ", null, Resource.String.error_env_log_count),
            new EnvCondResultInfo("F_SAE_CODE_STRING", null, null, 4),
            new EnvCondResultInfo("F_PCODE", null, Resource.String.error_env_pcode),
            new EnvCondResultInfo("F_CODE"),
            new EnvCondResultInfo("F_EREIGNIS_DTC"),
            new EnvCondResultInfo("F_UW_KM", "km", Resource.String.error_env_km, null, 1),
            new EnvCondResultInfo("F_UW_ZEIT", "s", Resource.String.error_env_time, null, 1, true),
        };

        public static readonly Tuple<string, string>[] SpecialInfoResetJobs =
        {
            new Tuple<string, string>("IS_LOESCHEN_TMS", "0x01"),
            new Tuple<string, string>("IS_LOESCHEN_TMS", "0x02"),
            new Tuple<string, string>("IS_LOESCHEN_TMS_L_LEAR", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_TMS_R_LEAR", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_SMC_L_LEAR", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_SMC_R_LEAR", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_SMC_L", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_SMC_R", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_SMC_L_LEAR", string.Empty),
            new Tuple<string, string>("IS_LOESCHEN_SMC_R_LEAR", string.Empty),
            new Tuple<string, string>("STEUERN_ZFS_LOESCHEN", string.Empty),
        };

        private readonly string _resourceDatalogDate;
        private bool _disposed;
        private Context _context;
        private volatile bool _stopThread;
        private bool _threadRunning;
        private Thread _workerThread;
        private DetectVehicleBmw _detectVehicleBmw;
        private RuleEvalBmw _ruleEvalBmw;
        private Queue<LogInfo> _logQueue = new Queue<LogInfo>();
        private object _logQueueLock = new object();
        private bool _ediabasInitReq;
        private bool _ediabasJobAbort;
        private JobReader.PageInfo _lastPageInfo;
        private long? _lastUpdateTime;
        private string _ecuPath;
        private string _vagPath;
        private string _bmwPath;
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
            _ruleEvalBmw = new RuleEvalBmw();
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
                    ProcessLogQueue();

                    _detectVehicleBmw = null;
                    // Dispose managed resources.
                    Ediabas.Dispose();
                    Ediabas = null;
                    CloseDataLog();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public bool StartThread(string portName, object connectParameter, JobReader.PageInfo pageInfo, bool commActive, ActivityCommon.InstanceDataCommon instanceData)
        {
            if (_workerThread != null)
            {
                return false;
            }

            if (instanceData == null)
            {
                return false;
            }

            try
            {
                _stopThread = false;
                string simulationPath = string.Empty;
                if (string.Compare(portName, EdInterfaceBase.PortIdSimulation, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    simulationPath = instanceData.SimulationPath;
                }

                if (Ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
                {
                    edInterfaceObd.UdsDtcStatusOverride = ActivityCommon.UdsDtcStatusOverride;
                    edInterfaceObd.ComPort = portName;
                }
                else if (Ediabas.EdInterfaceClass is EdInterfaceEnet edInterfaceEnet)
                {
                    if (!string.IsNullOrEmpty(portName))
                    {
                        edInterfaceEnet.RemoteHost = portName;
                    }

                    string appDataDir = instanceData.AppDataPath;
                    if (!string.IsNullOrEmpty(appDataDir))
                    {
                        string securityDir = Path.Combine(appDataDir, ActivityCommon.SecuritySubDir);
                        string caCertsDir = Path.Combine(securityDir, ActivityCommon.CaCertsSubDir);
                        string certsDir = Path.Combine(securityDir, ActivityCommon.CertsSubDir);
                        edInterfaceEnet.DoIpSslSecurityPath = caCertsDir;
                        edInterfaceEnet.DoIpS29Path = certsDir;
                        edInterfaceEnet.NetworkProtocol = EdInterfaceEnet.NetworkProtocolSsl;
                    }
                    else
                    {
                        edInterfaceEnet.NetworkProtocol = EdInterfaceEnet.NetworkProtocolTcp;
                    }
                }

                Ediabas.CloseSgbd();
                Ediabas.EdInterfaceClass.ConnectParameter = connectParameter;
                ActivityCommon.SetEdiabasConfigProperties(Ediabas, instanceData.TraceDir, simulationPath, instanceData.TraceAppend);
                CloseDataLog();

                CommActive = commActive;
                JobPageInfo = pageInfo;
                _lastPageInfo = null;
                _lastUpdateTime = null;
                _ecuPath = instanceData.EcuPath;
                _vagPath = instanceData.VagPath;
                _bmwPath = instanceData.BmwPath;
                _logDir = instanceData.DataLogDir;
                _appendLog = instanceData.DataLogAppend;
                _detectVehicleBmw = null;
                InitProperties(null);
                _ruleEvalBmw.SetEvalProperties(null, null);
                _workerThread = new Thread(ThreadFunc)
                {
                    Priority = ThreadPriority.Normal
                };
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

        public long GetResponseCount()
        {
            if (Ediabas?.EdInterfaceClass == null)
            {
                return 0;
            }

            return Ediabas.EdInterfaceClass.ResponseCount;
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
                        JobReader jobReader = ActivityCommon.JobReader;
                        fileMode = (_appendLog || jobReader.AppendLog) ? FileMode.Append : FileMode.Create;
                    }
                    else
                    {
                        fileMode = FileMode.Create;
                    }

                    bool createBom = fileMode == FileMode.Create;
                    _swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(createBom));
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

        public string FormatResult(JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict)
        {
            string result = ActivityCommon.FormatResult(Ediabas, pageInfo, displayInfo, resultDict, out Android.Graphics.Color? _, out double? _);
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

        public string GenerateErrorMessage(Context context, ActivityCommon activityCommon, JobReader.PageInfo pageInfo, EdiabasErrorReport errorReport, int errorIndex, MethodInfo formatErrorResult,
            ref List<string> translationList, TranslateDelegate translateFunc, ref List<ActivityCommon.VagDtcEntry> dtcList)
        {
            StringBuilder srMessage = new StringBuilder();
            string language = activityCommon.GetCurrentLanguage();
            bool shadow = errorReport is EdiabasErrorShadowReport;
            string ecuTitle = ActivityMain.GetPageString(pageInfo, errorReport.EcuName);
            EcuFunctionStructs.EcuVariant ecuVariant = errorReport.EcuVariant;
            if (ecuVariant != null)
            {
                string title = ecuVariant.Title?.GetTitle(language);
                if (!string.IsNullOrEmpty(title))
                {
                    if (!string.IsNullOrEmpty(ecuTitle))
                    {
                        ecuTitle += " (" + title + ")";
                    }
                    else
                    {
                        ecuTitle = title;
                    }
                }
            }

            srMessage.Append(ecuTitle);
            srMessage.Append(": ");

            if (errorReport.ErrorDict == null)
            {
                srMessage.Append(context.GetString(Resource.String.error_no_response));
            }
            else
            {
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    Int64 errorCode = 0;
                    if (errorReport.ErrorDict.TryGetValue("FNR_WERT", out EdiabasNet.ResultData resultData))
                    {
                        if (resultData.OpData is Int64)
                        {
                            errorCode = (Int64)resultData.OpData;
                        }
                    }

                    byte[] ecuResponse1 = null;
                    List<byte[]> ecuResponseList = new List<byte[]>();
                    foreach (string key in errorReport.ErrorDict.Keys)
                    {
                        if (key.StartsWith("ECU_RESPONSE", StringComparison.OrdinalIgnoreCase))
                        {
                            if (errorReport.ErrorDict.TryGetValue(key, out resultData))
                            {
                                if (resultData.OpData.GetType() == typeof(byte[]))
                                {
                                    if (string.Compare(key, "ECU_RESPONSE1", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        ecuResponse1 = (byte[])resultData.OpData;
                                    }
                                    else
                                    {
                                        ecuResponseList.Add((byte[])resultData.OpData);
                                    }
                                }
                            }
                        }
                    }

                    List<long> errorTypeList = new List<long>();
                    for (int i = 0; i < 1000; i++)
                    {
                        if (!errorReport.ErrorDict.TryGetValue(string.Format(CultureInfo.InvariantCulture, "FART{0}_WERT", i + 1), out resultData))
                        {
                            break;
                        }
                        if (!(resultData.OpData is Int64))
                        {
                            break;
                        }
                        Int64 errorType = (Int64)resultData.OpData;
                        errorTypeList.Add(errorType);
                    }
                    bool kwp1281 = false;
                    bool uds = false;
                    bool saeMode = false;
                    bool saeDetail = false;
                    if (errorReport.ErrorDict.TryGetValue("OBJECT", out resultData))
                    {
                        // ReSharper disable once UsePatternMatching
                        string objectName = resultData.OpData as string;
                        if (objectName != null)
                        {
                            if (XmlToolActivity.Is1281EcuName(objectName))
                            {
                                kwp1281 = true;
                            }
                            else if (XmlToolActivity.IsUdsEcuName(objectName))
                            {
                                uds = true;
                            }
                        }
                    }
                    if (errorReport.ErrorDict.TryGetValue("SAE", out resultData))
                    {
                        if (resultData.OpData is Int64)
                        {
                            if ((Int64)resultData.OpData != 0)
                            {
                                saeMode = true;
                                saeDetail = true;
                                errorCode <<= 8;
                            }
                        }
                    }

                    ActivityCommon.VagDtcEntry dtcEntry = null;
                    if (kwp1281)
                    {
                        dtcList = null;
                        byte dtcDetail = 0;
                        if (errorTypeList.Count >= 2)
                        {
                            dtcDetail = (byte)((errorTypeList[0] & 0x7F) | (errorTypeList[1] << 7));
                        }

                        dtcEntry = new ActivityCommon.VagDtcEntry((uint)errorCode, dtcDetail, UdsFileReader.DataReader.ErrorType.Iso9141);
                    }
                    else if (uds)
                    {
                        dtcList = null;
                        byte dtcDetail = 0;
                        if (errorTypeList.Count >= 8)
                        {
                            for (int idx = 0; idx < 8; idx++)
                            {
                                if (errorTypeList[idx] == 1)
                                {
                                    dtcDetail |= (byte)(1 << idx);
                                }
                            }
                        }

                        dtcEntry = new ActivityCommon.VagDtcEntry((uint)errorCode, dtcDetail, UdsFileReader.DataReader.ErrorType.Uds);
                        saeMode = true;
                        errorCode <<= 8;
                    }
                    else
                    {
                        if (ecuResponse1 != null)
                        {
                            errorIndex = 0;
                            dtcList = ActivityCommon.ParseEcuDtcResponse(ecuResponse1, saeMode);
                        }

                        if (dtcList != null && errorIndex < dtcList.Count)
                        {
                            dtcEntry = dtcList[errorIndex];
                        }
                    }

                    List<string> textList = null;
                    if (ActivityCommon.VagUdsActive && dtcEntry != null)
                    {
                        if (dtcEntry.ErrorType != UdsFileReader.DataReader.ErrorType.Uds)
                        {
                            string dataFileName = null;
                            if (!string.IsNullOrEmpty(errorReport.VagDataFileName))
                            {
                                dataFileName = Path.Combine(_vagPath, errorReport.VagDataFileName);
                            }
                            UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(dataFileName);
                            if (udsReader != null)
                            {
                                textList = udsReader.DataReader.ErrorCodeToString(
                                    dtcEntry.DtcCode, dtcEntry.DtcDetail, dtcEntry.ErrorType, udsReader);
                                if (saeDetail && ecuResponseList.Count > 0)
                                {
                                    byte[] detailData = ActivityCommon.ParseSaeDetailDtcResponse(ecuResponseList[0]);
                                    if (detailData != null)
                                    {
                                        List<string> saeDetailList = udsReader.DataReader.SaeErrorDetailHeadToString(detailData, udsReader);
                                        if (saeDetailList != null)
                                        {
                                            textList.AddRange(saeDetailList);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(errorReport.VagUdsFileName))
                            {
                                string udsFileName = null;
                                if (!string.IsNullOrEmpty(errorReport.VagUdsFileName))
                                {
                                    udsFileName = Path.Combine(_vagPath, errorReport.VagUdsFileName);
                                }
                                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(udsFileName);
                                if (udsReader != null)
                                {
                                    textList = udsReader.ErrorCodeToString(udsFileName, dtcEntry.DtcCode, dtcEntry.DtcDetail);
                                    if (ecuResponseList.Count > 0)
                                    {
                                        byte[] response = ActivityCommon.ExtractUdsEcuResponses(ecuResponseList[0]);
                                        if (response != null)
                                        {
                                            List<string> errorDetailList = udsReader.ErrorDetailBlockToString(udsFileName, response);
                                            if (errorDetailList != null)
                                            {
                                                textList.AddRange(errorDetailList);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        srMessage.Append("\r\n");
                        srMessage.Append(context.GetString(Resource.String.error_error_code));
                        srMessage.Append(": ");
                        srMessage.Append(string.Format("0x{0:X04} 0x{1:X02} {2}", dtcEntry.DtcCode, dtcEntry.DtcDetail, dtcEntry.ErrorType.ToString()));
                    }
                    if (textList == null)
                    {
                        textList = activityCommon.ConvertVagDtcCode(_ecuPath, errorCode, errorTypeList, kwp1281, saeMode);
                        srMessage.Append("\r\n");
                        srMessage.Append(context.GetString(Resource.String.error_error_code));
                        srMessage.Append(": ");
                        srMessage.Append(string.Format("0x{0:X}", errorCode));
                        foreach (long errorType in errorTypeList)
                        {
                            srMessage.Append(string.Format(";{0}", errorType));
                        }

                        if (saeMode)
                        {
                            srMessage.Append("\r\n");
                            srMessage.Append(string.Format("{0}-{1:X02}", ActivityCommon.SaeCode16ToString(errorCode >> 8), errorCode & 0xFF));
                        }
                    }
                    if (textList != null)
                    {
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (string text in textList)
                        {
                            srMessage.Append("\r\n");
                            srMessage.Append(text);
                        }
                    }
                }
                else
                {
                    // BMW
                    string text1 = string.Empty;
                    string text2 = string.Empty;
                    Int64 errorCode = ActivityMain.GetResultInt64(errorReport.ErrorDict, "F_ORT_NR", out bool errorCodeFound);
                    if (!errorCodeFound)
                    {
                        errorCode = 0x0000;
                    }

                    bool showUnknown = !(ActivityCommon.EcuFunctionsActive && ActivityCommon.ShowOnlyRelevantErrors);
                    List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelList = null;
                    if (ecuVariant != null)
                    {
#if false   // test code for result states
                        List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelResultList = ActivityCommon.EcuFunctionReader.GetEnvCondLabelListWithResultStates(ecuVariant, errorReport.ReadIs);
                        if (envCondLabelResultList != null && envCondLabelResultList.Count > 0)
                        {
                            string detailTestText = EdiabasThread.ConvertEnvCondErrorDetail(_context, errorReport, envCondLabelResultList);
                            if (!string.IsNullOrEmpty(detailTestText))
                            {
                                srMessage.Append("\r\n");
                                srMessage.Append(detailTestText);
                            }
                        }
#endif
                        if (errorCode != 0x0000)
                        {
                            envCondLabelList = ActivityCommon.EcuFunctionReader.GetEnvCondLabelList(errorCode, errorReport.ReadIs, ecuVariant);
                            List<string> faultResultList = ConvertFaultCodeError(context, errorCode, errorReport.ReadIs, errorReport, ecuVariant);

                            if (faultResultList != null && faultResultList.Count == 2)
                            {
                                text1 = faultResultList[0];
                                text2 = faultResultList[1];
                            }
                        }

                        bool isSuppressed = !(errorReport.IsValid && errorReport.IsVisible);
                        if (!showUnknown && isSuppressed)
                        {
                            errorCode = 0x0000;
                        }
                    }

                    if (showUnknown && errorCode != 0x0000 && string.IsNullOrEmpty(text1))
                    {
                        text1 = ActivityMain.FormatResultString(errorReport.ErrorDict, "F_ORT_TEXT", "{0}");
                        text2 = ActivityMain.FormatResultString(errorReport.ErrorDict, "F_VORHANDEN_TEXT", "{0}");
                        if (translateFunc != null)
                        {
                            Tuple<string, string> tupleTrans = translateFunc.Invoke(new Tuple<string, string>(text1, text2), ref translationList);
                            text1 = tupleTrans.Item1;
                            text2 = tupleTrans.Item2;
                        }
                    }

                    string textErrorCode = ActivityMain.FormatResultInt64(errorReport.ErrorDict, "F_ORT_NR", "0x{0:X06}");
                    if (errorCode == 0x0000 || (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(textErrorCode)))
                    {
                        srMessage.Clear();
                    }
                    else
                    {
                        srMessage.Append("\r\n");
                        if (!string.IsNullOrEmpty(textErrorCode))
                        {
                            if (errorReport.ReadIs)
                            {
                                srMessage.Append(context.GetString(Resource.String.error_info_code));
                            }
                            else
                            {
                                srMessage.Append(context.GetString(Resource.String.error_error_code));
                                if (shadow)
                                {
                                    srMessage.Append(" ");
                                    srMessage.Append(context.GetString(Resource.String.error_shadow));
                                }
                            }
                            srMessage.Append(": ");
                            srMessage.Append(textErrorCode);

                            if (!errorReport.IsValid)
                            {
                                srMessage.Append(" (");
                                srMessage.Append(context.GetString(Resource.String.error_unknown));
                                srMessage.Append(")");
                            }
                            else if (!errorReport.IsVisible)
                            {
                                srMessage.Append(" (");
                                srMessage.Append(context.GetString(Resource.String.error_hidden));
                                srMessage.Append(")");
                            }

                            srMessage.Append("\r\n");
                        }

                        srMessage.Append(text1);
                        if (!string.IsNullOrEmpty(text2))
                        {
                            srMessage.Append(", ");
                            srMessage.Append(text2);
                        }

                        string detailText = ConvertEnvCondErrorDetail(context, errorReport, envCondLabelList);
                        if (!string.IsNullOrEmpty(detailText))
                        {
                            srMessage.Append("\r\n");
                            srMessage.Append(detailText);
                        }
                    }
                }
            }

            string message = srMessage.ToString();
            if (formatErrorResult != null)
            {
                try
                {
                    object[] args = { pageInfo, errorReport, message };
                    message = formatErrorResult.Invoke(pageInfo.ClassObject, args) as string;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return message;
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
            _lastPageInfo = null;
            DataUpdatedEvent(true);

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
            DataUpdatedEvent(true);
            CloseDataLog();
            ThreadTerminatedEvent();
        }

        private bool CommEdiabas(JobReader.PageInfo pageInfo)
        {
            if (ActivityCommon.ReadBatteryVoltage(Ediabas, out double? batteryVoltage, out byte[] adapterSerial))
            {
                lock (DataLock)
                {
                    BatteryVoltage = batteryVoltage;
                    AdapterSerial = adapterSerial;
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
                    UpdateProgressState = UpdateState.Init;
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

                if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && !string.IsNullOrEmpty(errorResetSgbdFunc))
                {
                    try
                    {
                        ActivityCommon.ResolveSgbdFile(Ediabas, errorResetSgbdFunc);
                        ForceLoadSgbd();

                        EdiabasErrorReportReset.ErrorRestState resetState = ResetErrorFunctional(false);
                        bool resetIs = false;
                        EdiabasErrorReportReset.ErrorRestState resetStateIs = ResetErrorFunctional(true);
                        if (resetStateIs != EdiabasErrorReportReset.ErrorRestState.Undefined)
                        {
                            resetIs = resetStateIs == EdiabasErrorReportReset.ErrorRestState.Ok;
                            if (!resetIs)
                            {
                                resetState = resetStateIs;
                            }
                        }

                        if (resetState == EdiabasErrorReportReset.ErrorRestState.Ok)
                        {
                            errorReportList.Add(new EdiabasErrorReportReset(string.Empty, string.Empty, string.Empty, null, null, resetIs, resetState));
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (_detectVehicleBmw == null)
                {
                    try
                    {
                        JobReader jobReader = ActivityCommon.JobReader;
                        string xmlFileName = jobReader.XmlFileName;
                        string xmlDir = null;
                        if (!string.IsNullOrEmpty(xmlFileName))
                        {
                            xmlDir = Path.GetDirectoryName(xmlFileName);
                        }

                        if (!string.IsNullOrEmpty(xmlDir))
                        {
                            string vehicleDataFile = Path.Combine(xmlDir, Path.GetFileNameWithoutExtension(xmlFileName) + DetectVehicleBmw.DataFileExtension);
                            DateTime xmlFileTime = File.GetLastWriteTimeUtc(xmlFileName);
                            string xmlTimeStamp = xmlFileTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                            DetectVehicleBmw detectVehicleBmw = new DetectVehicleBmw(Ediabas, _bmwPath);
                            detectVehicleBmw.AbortFunc = () => _ediabasJobAbort;
                            detectVehicleBmw.ProgressFunc = percent =>
                            {
                                lock (DataLock)
                                {
                                    UpdateProgressState = UpdateState.DetectVehicle;
                                    UpdateProgress = percent;
                                }

                                DataUpdatedEvent();
                            };

                            lock (detectVehicleBmw.GlobalLockObject)
                            {
                                if (!detectVehicleBmw.LoadDataFromFile(vehicleDataFile, xmlTimeStamp))
                                {
                                    bool detectMotorbike = jobReader.IsMotorbike;
                                    if (!string.IsNullOrEmpty(pageInfo.ErrorsInfo.SgbdFunctional))
                                    {
                                        detectVehicleBmw.DetectVehicleBmwFast(detectMotorbike);
                                    }
                                    else
                                    {
                                        detectVehicleBmw.DetectVehicleDs2();
                                    }

                                    if (detectVehicleBmw.Valid)
                                    {
                                        detectVehicleBmw.SaveDataToFile(vehicleDataFile, xmlTimeStamp);
                                    }
                                }
                            }

                            _detectVehicleBmw = detectVehicleBmw;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                int index = -1;
                foreach (JobReader.EcuInfo ecuInfo in pageInfo.ErrorsInfo.EcuList)
                {
                    index++;
                    if (AbortEdiabasJob())
                    {
                        break;
                    }

                    try
                    {
                        ActivityCommon.ResolveSgbdFile(Ediabas, ecuInfo.Sgbd);
                    }
                    catch (Exception ex)
                    {
                        string exText = string.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exText = EdiabasNet.GetExceptionText(ex, false, false);
                        }
                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, string.Empty, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, false, true, true, null, null, exText));
                        lock (DataLock)
                        {
                            UpdateProgressState = UpdateState.Error;
                        }
                        DataUpdatedEvent();
                        continue;
                    }

                    try
                    {
                        string sgbdResolved = Ediabas.SgbdFileName ?? string.Empty;
                        if (!string.IsNullOrEmpty(sgbdResolved))
                        {
                            sgbdResolved = Path.GetFileNameWithoutExtension(sgbdResolved);
                        }

                        EcuFunctionStructs.EcuVariant ecuVariant = null;
                        DateTime? lifeStartDate = null;
                        if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                        {
                            if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                            {
                                ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(sgbdResolved);
                            }

                            if (_detectVehicleBmw != null)
                            {
                                lifeStartDate = _detectVehicleBmw.LifeStartDate;
                                _detectVehicleBmw.Ediabas = Ediabas;
                                _ruleEvalBmw.SetEvalProperties(_detectVehicleBmw, ecuVariant);
                            }
                        }

                        ForceLoadSgbd();
                        if (errorResetList != null && errorResetList.Any(ecu => string.CompareOrdinal(ecu, ecuInfo.Name) == 0))
                        {   // error reset requested
                            EdiabasErrorReportReset.ErrorRestState resetState = ResetError(false);

                            bool resetIs = false;
                            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                            {
                                try
                                {
                                    if (resetState != EdiabasErrorReportReset.ErrorRestState.Ok && XmlToolActivity.IsUdsEcuName(ecuInfo.Sgbd) &&
                                        Ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
                                    {
                                        int dataOffset = XmlToolActivity.VagUdsRawDataOffset;
                                        byte[] clearDtcRequest = { 0x04 };  // ISO 15031-5

                                        edInterfaceObd.UdsEcuCanIdOverride = UdsFuncAddr;
                                        bool funcAddress = edInterfaceObd.UdsEcuCanIdOverride == UdsFuncAddr;
                                        for (; ; )
                                        {
                                            bool dataReceived = false;
                                            try
                                            {
                                                if (Ediabas.EdInterfaceClass.TransmitData(clearDtcRequest, out byte[] clearDtcResponse) && clearDtcResponse?.Length >= dataOffset)
                                                {
                                                    dataReceived = true;
                                                    if (clearDtcResponse.Length >= dataOffset + 1)
                                                    {
                                                        if (clearDtcResponse[dataOffset + 0] == 0x44)
                                                        {
                                                            if (resetState == EdiabasErrorReportReset.ErrorRestState.Undefined)
                                                            {
                                                                resetState = EdiabasErrorReportReset.ErrorRestState.Ok;
                                                            }
                                                        }
                                                        else if (clearDtcResponse[dataOffset + 0] == 0x7F)
                                                        {
                                                            if (clearDtcResponse.Length >= dataOffset + 3 && clearDtcResponse[dataOffset + 1] == 0x04 && clearDtcResponse[dataOffset + 2] == 0x22)
                                                            {
                                                                resetState = EdiabasErrorReportReset.ErrorRestState.Condition;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }

                                            if (!funcAddress || !dataReceived)
                                            {
                                                break;
                                            }

                                            if (AbortEdiabasJob())
                                            {
                                                break;
                                            }

                                            clearDtcRequest = new []{ EdInterfaceObd.UdsNoSendData };   // no send data
                                        }

                                        // force reload
                                        Ediabas.CloseSgbd();
                                        ActivityCommon.ResolveSgbdFile(Ediabas, ecuInfo.Sgbd);
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                            else
                            {   // BMW
                                EdiabasErrorReportReset.ErrorRestState resetStateIs = ResetError(true);
                                if (resetStateIs != EdiabasErrorReportReset.ErrorRestState.Undefined)
                                {
                                    resetIs = resetStateIs == EdiabasErrorReportReset.ErrorRestState.Ok;
                                    if (!resetIs)
                                    {
                                        resetState = resetStateIs;
                                    }
                                }
                            }

                            if (resetState != EdiabasErrorReportReset.ErrorRestState.Undefined)
                            {
                                errorReportList.Add(new EdiabasErrorReportReset(ecuInfo.Name, ecuInfo.Sgbd, sgbdResolved, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, resetIs, resetState));
                            }
                        }

                        if (ReadErrors(ecuInfo, sgbdResolved, false, ecuVariant, lifeStartDate, errorReportList))
                        {
                            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                            {
                                ReadErrors(ecuInfo, sgbdResolved, true, ecuVariant, lifeStartDate, errorReportList);
                            }
                        }

                        if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw &&
                            ActivityCommon.EcuFunctionsActive && !ActivityCommon.ShowOnlyRelevantErrors)
                        {
                            try
                            {
                                string errorShadowJob = "FS_LESEN_SHADOW";
                                if (Ediabas.IsJobExisting(errorShadowJob))
                                {
                                    try
                                    {
                                        Ediabas.ArgString = string.Empty;
                                        Ediabas.ArgBinaryStd = null;
                                        Ediabas.ResultsRequests = string.Empty;
                                        Ediabas.ExecuteJob(errorShadowJob);
                                    }
                                    finally
                                    {
                                        ProcessLogQueue();
                                    }

                                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);

                                    bool jobOk = false;
                                    if (resultSets.Count > 1)
                                    {
                                        if (IsJobStatusOk(resultSets[^1]))
                                        {
                                            jobOk = true;
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

                                            errorReportList.Add(new EdiabasErrorShadowReport(ecuInfo.Name, ecuInfo.Sgbd, sgbdResolved, resultDictLocal));
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exText = string.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exText = EdiabasNet.GetExceptionText(ex, false, false);
                        }
                        errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, string.Empty, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, false, true, true, null, null, exText));
                        lock (DataLock)
                        {
                            UpdateProgressState = UpdateState.Error;
                        }
                        DataUpdatedEvent();
                        continue;
                    }
                    if (EdiabasErrorReportList == null)
                    {   // first update
                        lock (DataLock)
                        {
                            UpdateProgressState = UpdateState.ReadErrors;
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
                    UpdateProgressState = UpdateState.ReadErrors;
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
                        EdInterfaceBase edInterfaceClass = Ediabas.EdInterfaceClass;
                        if (edInterfaceClass != null)
                        {
                            edInterfaceClass.InterfaceDisconnect();
                            // clear transmit cache
                            edInterfaceClass.EnableTransmitCache = false;
                            edInterfaceClass.EnableTransmitCache = true;
                        }

                        string exText = string.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exText = EdiabasNet.GetExceptionText(ex, false, false);
                        }
                        lock (DataLock)
                        {
                            EdiabasResultDict = null;
                            EdiabasErrorReportList = null;
                            EdiabasErrorMessage = exText;
                            ResultPageInfo = pageInfo;
                            UpdateProgressState = UpdateState.Error;
                            UpdateProgress = 0;
                        }
                        return false;
                    }

                    if (pageInfo.JobsInfo != null)
                    {
                        currentSgbd = pageInfo.JobsInfo.Sgbd;
                    }
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
                        if (AbortEdiabasJob())
                        {
                            break;
                        }

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
                                            List<EcuFunctionResult> ecuFunctionResultList = ExecuteEcuJobs(Ediabas, ecuFixedFuncStruct, displayInfo.EcuJobId, pageInfo.UseCompatIds);
                                            if (ecuFunctionResultList != null)
                                            {
                                                Dictionary<string, EdiabasNet.ResultData> resultDictLocal = new Dictionary<string, EdiabasNet.ResultData>();
                                                Dictionary<string, EdiabasNet.ResultData> resultDictLocalCompat = new Dictionary<string, EdiabasNet.ResultData>();
                                                foreach (EcuFunctionResult ecuFunctionResult in ecuFunctionResultList)
                                                {
                                                    string key = (ecuFunctionResult.EcuJob.Id.Trim() + "#" + ecuFunctionResult.EcuJobResult.Id.Trim()).ToUpperInvariant();
                                                    resultDictLocal.Add(key, ecuFunctionResult);

                                                    if (ecuFunctionResult.EcuJob.CompatIdListList != null && ecuFunctionResult.EcuJobResult.CompatIdListList != null)
                                                    {
                                                        foreach (string jobId in ecuFunctionResult.EcuJob.CompatIdListList)
                                                        {
                                                            foreach (string resultId in ecuFunctionResult.EcuJobResult.CompatIdListList)
                                                            {
                                                                key = (jobId.Trim() + "#" + resultId.Trim()).ToUpperInvariant();
                                                                resultDictLocalCompat.Add(key, ecuFunctionResult);
                                                            }
                                                        }
                                                    }
                                                }

                                                MergeResultDictionarys(ref resultDict, resultDictLocal, ecuFixedFuncStruct.Id.Trim() + "#");

                                                if (ecuFixedFuncStruct.CompatIdListList != null)
                                                {
                                                    foreach (string structId in ecuFixedFuncStruct.CompatIdListList)
                                                    {
                                                        MergeResultDictionarys(ref resultDict, resultDictLocalCompat, structId.Trim() + "#");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(jobInfo.FixedFuncStructId))
                                {
                                    for (int retry = 0; retry < 2; retry++)
                                    {
                                        if (!ExecuteBmwStandardJob(jobInfo, firstRequestCall, ref resultDict))
                                        {
                                            break;
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
                string exText = string.Empty;
                if (!AbortEdiabasJob())
                {
                    exText = EdiabasNet.GetExceptionText(ex, false, false);
                }
                lock (DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorReportList = null;
                    EdiabasErrorMessage = exText;
                    ResultPageInfo = pageInfo;
                    UpdateProgressState = UpdateState.Error;
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
                UpdateProgressState = UpdateState.Connected;
                UpdateProgress = 0;
            }
            return true;
        }

        public EdiabasErrorReportReset.ErrorRestState ResetErrorFunctional(bool resetIs)
        {
            try
            {
                EdiabasErrorReportReset.ErrorRestState errorRestState = EdiabasErrorReportReset.ErrorRestState.Undefined;
                string resetJob = resetIs ? "IS_LOESCHEN_FUNKTIONAL" : "FS_LOESCHEN_FUNKTIONAL";
                if (!Ediabas.IsJobExisting(resetJob))
                {
                    return EdiabasErrorReportReset.ErrorRestState.Undefined;
                }

                try
                {
                    Ediabas.ArgString = string.Empty;
                    Ediabas.ArgBinaryStd = null;
                    Ediabas.ResultsRequests = string.Empty;
                    Ediabas.ExecuteJob(resetJob);
                }
                finally
                {
                    ProcessLogQueue();
                }

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
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
                            errorRestState = EdiabasErrorReportReset.ErrorRestState.Ok;
                            break;
                        }

                        dictIndex++;
                    }
                }

                return errorRestState;
            }
            catch (Exception)
            {
                return EdiabasErrorReportReset.ErrorRestState.Failed;
            }
        }

        public EdiabasErrorReportReset.ErrorRestState ResetError(bool resetIs)
        {
            try
            {
                EdiabasErrorReportReset.ErrorRestState resetState = EdiabasErrorReportReset.ErrorRestState.Undefined;
                string resetJob;
                if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
                {
                    resetJob = resetIs ? "IS_LOESCHEN" : "FS_LOESCHEN";
                }
                else
                {
                    if (resetIs)
                    {
                        return EdiabasErrorReportReset.ErrorRestState.Undefined;
                    }

                    resetJob = "Fehlerspeicher_loeschen";
                }

                if (!Ediabas.IsJobExisting(resetJob))
                {
                    return EdiabasErrorReportReset.ErrorRestState.Undefined;
                }

                try
                {
                    Ediabas.ArgString = string.Empty;
                    Ediabas.ArgBinaryStd = null;
                    Ediabas.ResultsRequests = string.Empty;
                    Ediabas.ExecuteJob(resetJob);
                }
                finally
                {
                    ProcessLogQueue();
                }

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                if (resultSets.Count > 1)
                {
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
                                resetState = EdiabasErrorReportReset.ErrorRestState.Ok;
                            }
                        }
                    }
                }

                if (resetIs)
                {
                    foreach (Tuple<string, string> infoResetJob in SpecialInfoResetJobs)
                    {
                        try
                        {
                            if (Ediabas.IsJobExisting(infoResetJob.Item1))
                            {
                                Ediabas.ArgString = infoResetJob.Item2;
                                Ediabas.ArgBinaryStd = null;
                                Ediabas.ResultsRequests = string.Empty;
                                Ediabas.ExecuteJob(infoResetJob.Item1);
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        finally
                        {
                            ProcessLogQueue();
                        }
                    }
                }

                return resetState;
            }
            catch (Exception)
            {
                return EdiabasErrorReportReset.ErrorRestState.Failed;
            }
        }

        public bool ReadErrors(JobReader.EcuInfo ecuInfo, string sgbdResolved, bool readIs, EcuFunctionStructs.EcuVariant ecuVariant, DateTime? lifeStartDate, List <EdiabasErrorReport> errorReportList)
        {
            string errorJob;
            string errorDetailJob = string.Empty;
            string argString = string.Empty;
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                errorJob = readIs ? "IS_LESEN" : "FS_LESEN";
                errorDetailJob = readIs ? "IS_LESEN_DETAIL" : "FS_LESEN_DETAIL";
            }
            else
            {
                if (readIs)
                {
                    return false;
                }

                errorJob = "Fehlerspeicher_abfragen";
                if (!string.IsNullOrEmpty(ecuInfo.VagUdsFileName))
                {
                    argString = "MW_LESEN";
                }
            }

            if (!Ediabas.IsJobExisting(errorJob))
            {
                return false;
            }

            try
            {
                Ediabas.ArgString = argString;
                Ediabas.ArgBinaryStd = null;
                Ediabas.ResultsRequests = string.Empty;
                Ediabas.ExecuteJob(errorJob);
            }
            finally
            {
                ProcessLogQueue();
            }

            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);

            bool jobOk = false;
            bool jobRejected = false;
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
                            string jobStatus = (string)resultData.OpData;
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
                        try
                        {
                            Ediabas.ArgString = "MW_LESEN";
                            Ediabas.ArgBinaryStd = null;
                            Ediabas.ResultsRequests = string.Empty;
                            Ediabas.ExecuteJob("FehlerspeicherSAE_abfragen");
                        }
                        finally
                        {
                            ProcessLogQueue();
                        }

                        resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                        if (resultSets.Count > 0)
                        {
                            if (resultSets[0].TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    // read details
                                    string jobStatus = (string)resultData.OpData;
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
                    else
                    {
                        jobRejected = true;
                    }
                }
            }

            if (jobOk)
            {
                bool showOnlyRelevant = ActivityCommon.EcuFunctionsActive && ActivityCommon.ShowOnlyRelevantErrors;
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
                                resultDictTemp.Add("SAE", new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeI, "SAE", (Int64)(saeMode ? 1 : 0)));
                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, sgbdResolved, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, false, true, true, resultDictTemp));
                            }
                        }
                        dictIndex++;
                        continue;
                    }
                    // BMW only
                    if (resultDictLocal.TryGetValue("F_ORT_NR", out resultData))
                    {
                        if (resultData.OpData is Int64 errorCode)
                        {
                            bool isValid = true;
                            bool isVisible = true;
                            if (ecuVariant != null)
                            {
                                isValid = ActivityCommon.EcuFunctionReader.IsValidFaultCode(errorCode, readIs, ecuVariant, _ruleEvalBmw, out isVisible);
                            }

                            bool readDetail = !showOnlyRelevant;
                            if (isValid && isVisible)
                            {
                                readDetail = true;
                            }

                            bool details = false;
                            if (readDetail && Ediabas.IsJobExisting(errorDetailJob))
                            {
                                // read details
                                Ediabas.ArgString = string.Format("0x{0:X02}", (Int64)resultData.OpData);
                                Ediabas.ArgBinaryStd = null;
                                Ediabas.ResultsRequests = string.Empty;

                                try
                                {
                                    Ediabas.ExecuteJob(errorDetailJob);
                                    details = true;
                                }
                                catch (Exception)
                                {
                                    // no details
                                    details = false;
                                }
                                finally
                                {
                                    ProcessLogQueue();
                                }
                            }

                            if (details)
                            {
                                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail = new List<Dictionary<string, EdiabasNet.ResultData>>(Ediabas.ResultSets);
                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, sgbdResolved, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, readIs, isValid, isVisible, resultDictLocal, resultSetsDetail)
                                {
                                    EcuVariant = ecuVariant,
                                    LifeStartDate = lifeStartDate
                                });
                            }
                            else
                            {
                                errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, sgbdResolved, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, readIs, isValid, isVisible, resultDictLocal)
                                {
                                    EcuVariant = ecuVariant,
                                    LifeStartDate = lifeStartDate
                                });
                            }
                        }
                    }
                    dictIndex++;
                }
            }
            else
            {
                if (!(readIs && jobRejected))
                {
                    errorReportList.Add(new EdiabasErrorReport(ecuInfo.Name, ecuInfo.Sgbd, sgbdResolved, ecuInfo.VagDataFileName, ecuInfo.VagUdsFileName, readIs, true, true)
                    {
                        EcuVariant = ecuVariant,
                        LifeStartDate = lifeStartDate
                    });
                }
            }

            return jobOk;
        }

        public bool ExecuteBmwStandardJob(JobReader.JobInfo jobInfo, bool firstRequestCall, ref MultiMap<string, EdiabasNet.ResultData> resultDict)
        {
            string argString;
            if (firstRequestCall && !string.IsNullOrEmpty(jobInfo.ArgsFirst))
            {
                argString = jobInfo.ArgsFirst;
            }
            else
            {
                argString = jobInfo.Args;
            }

            bool parameterChanged = false;
            List<string> edArgList = new List<string>();
            bool statMbBlock = string.Compare(jobInfo.Name, XmlToolActivity.JobReadStatMwBlock, StringComparison.OrdinalIgnoreCase) == 0;
            bool statBlock = string.Compare(jobInfo.Name, XmlToolActivity.JobReadStatBlock, StringComparison.OrdinalIgnoreCase) == 0;
            bool statRead = string.Compare(jobInfo.Name, XmlToolActivity.JobReadStat, StringComparison.OrdinalIgnoreCase) == 0;
            if ((statMbBlock || statBlock || statRead) && jobInfo.ArgLimit > 0)
            {
                List<string> argList = argString.Split(";").ToList();
                int? argStartBlock = null;
                StringBuilder sbArgStart = new StringBuilder();
                bool validArg = false;
                if (statMbBlock)
                {
                    if (argList.Count >= 1)
                    {
                        validArg = string.IsNullOrEmpty(jobInfo.ArgsFirst) && string.Compare(argList[0].Trim(), "JA", StringComparison.OrdinalIgnoreCase) == 0;
                        sbArgStart.Append(argList[0]);
                        argList.RemoveAt(0);
                    }
                }
                else if (statBlock)
                {
                    if (argList.Count >= 3)
                    {
                        Int64 blockNumber = EdiabasNet.StringToValue(argList[0], out bool valid);
                        if (valid)
                        {
                            argStartBlock = (int)blockNumber;
                        }
                        argList.RemoveAt(0);

                        validArg = argStartBlock.HasValue;
                        sbArgStart.Append(argList[0]);
                        argList.RemoveAt(0);

                        sbArgStart.Append(";");
                        sbArgStart.Append(argList[0]);
                        argList.RemoveAt(0);
                    }
                }
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                else if (statRead)
                {
                    if (argList.Count >= 1)
                    {
                        validArg = string.IsNullOrEmpty(jobInfo.ArgsFirst);
                        sbArgStart.Append(argList[0]);
                        argList.RemoveAt(0);
                    }
                }

                if (validArg && argList.Count >= 1)
                {
                    for (; ; )
                    {
                        StringBuilder sbArg = new StringBuilder();
                        if (argStartBlock.HasValue)
                        {
                            int blockNumber = argStartBlock.Value + edArgList.Count;
                            sbArg.Append(string.Format(CultureInfo.InvariantCulture, "{0}", blockNumber));
                            sbArg.Append(";");
                        }
                        sbArg.Append(sbArgStart);

                        int argCount = 0;
                        while (argList.Count > 0)
                        {
                            if (argCount >= jobInfo.ArgLimit)
                            {
                                break;
                            }

                            sbArg.Append(";");
                            sbArg.Append(argList[0]);
                            argList.RemoveAt(0);
                            argCount++;
                        }

                        if (argCount == 0)
                        {
                            break;
                        }

                        edArgList.Add(sbArg.ToString());
                    }
                }
            }

            if (edArgList.Count == 0)
            {
                edArgList.Add(argString);
            }

            foreach (string edArg in edArgList)
            {
                try
                {
                    Ediabas.ArgString = edArg;
                    Ediabas.ArgBinaryStd = null;
                    Ediabas.ResultsRequests = jobInfo.Results;
                    Ediabas.ExecuteJob(jobInfo.Name);
                }
                finally
                {
                    ProcessLogQueue();
                }

                if (!string.IsNullOrWhiteSpace(jobInfo.RawTelegrams))
                {
                    string[] rawTelegrams = jobInfo.RawTelegrams.Split(';');
                    for (int telIdx = 0; telIdx < rawTelegrams.Length; telIdx++)
                    {
                        string rawTelegram = rawTelegrams[telIdx].Replace(" ", string.Empty);
                        byte[] sendData = EdiabasNet.HexToByteArray(rawTelegram);
                        if (sendData.Length > 0)
                        {
                            bool funcAddress = false;
                            if (Ediabas.EdInterfaceClass.BmwFastProtocol)
                            {
                                funcAddress = (sendData[0] & 0xC0) == 0xC0;     // functional address
                            }

                            List<byte> responseList = new List<byte>();
                            for (; ; )
                            {
                                bool dataReceived = false;
                                try
                                {
                                    if (Ediabas.EdInterfaceClass.TransmitData(sendData, out byte[] receiveData) && receiveData?.Length > 0)
                                    {
                                        dataReceived = true;
                                        responseList.AddRange(receiveData);
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                if (!funcAddress || !dataReceived)
                                {
                                    break;
                                }

                                if (AbortEdiabasJob())
                                {
                                    break;
                                }

                                sendData = Array.Empty<byte>();
                            }

                            if (responseList.Count > 0)
                            {
                                string telName = string.Format(CultureInfo.InvariantCulture, "RAW_TELEGRAM_{0}", telIdx + 1);
                                Dictionary<string, EdiabasNet.ResultData> resultDictTel = new Dictionary<string, EdiabasNet.ResultData>
                                {
                                    { telName, new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeY, telName, responseList.ToArray()) }
                                };

                                if (string.IsNullOrEmpty(jobInfo.Id))
                                {
                                    MergeResultDictionarys(ref resultDict, resultDictTel, jobInfo.Name + "#");
                                }
                                else
                                {
                                    MergeResultDictionarys(ref resultDict, resultDictTel, jobInfo.Id + "#");
                                }
                            }

                            if (AbortEdiabasJob())
                            {
                                break;
                            }
                        }
                    }
                }

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

                        if (resultDictLocal.TryGetValue("JOB_MESSAGE", out EdiabasNet.ResultData resultDataMsg))
                        {
                            if (resultDataMsg.OpData is string messageText)
                            {
                                Regex maxArgRegex = new Regex(@"MAX_ARGUMENT_STATUS_LESEN\s*=\s*'([0-9]+)'", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                                MatchCollection argMatches = maxArgRegex.Matches(messageText);
                                if (argMatches.Count == 1 && argMatches[0].Groups.Count == 2)
                                {
                                    string maxArgs = argMatches[0].Groups[1].Value;
                                    if (int.TryParse(maxArgs, NumberStyles.Integer, CultureInfo.InvariantCulture, out int argLimit))
                                    {
                                        if (argLimit > 0 && jobInfo.ArgLimit != argLimit)
                                        {
                                            jobInfo.ArgLimit = argLimit;
                                            parameterChanged = true;
                                            break;
                                        }
                                    }
                                }
                            }
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

            return parameterChanged;
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

            List<Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>> fixedFuncStructList = ActivityCommon.EcuFunctionReader.GetFixedFuncStructList(ecuVariant);
            foreach (var ecuFixedFuncStructPair in fixedFuncStructList)
            {
                EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct = ecuFixedFuncStructPair.Item1;
                if (ecuFixedFuncStruct.IdPresent(jobInfo.FixedFuncStructId, jobInfo.UseCompatIds))
                {
                    return ecuFixedFuncStruct;
                }
            }

            return null;
        }

        public static List<EcuFunctionResult> ExecuteEcuJobs(EdiabasNet ediabas, EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct,
            string ecuJobId = null, bool useCompatIds = false, EcuFunctionStructs.EcuJob.PhaseType phaseType = EcuFunctionStructs.EcuJob.PhaseType.Unknown)
        {
            List<EcuFunctionResult> ecuFunctionResultList = new List<EcuFunctionResult>();
            EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType nodeClassType = ecuFixedFuncStruct.GetNodeClassType();
            List<EcuFunctionStructs.EcuJob> ecuJobList = new List<EcuFunctionStructs.EcuJob>();
            if (ecuFixedFuncStruct.EcuJobList != null)
            {
                foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                {
                    if (string.IsNullOrEmpty(ecuJobId) || ecuJob.IdPresent(ecuJobId, useCompatIds))
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
                // from: RheingoldCoreFramework.dll BMW.Rheingold.CoreFramework.DatabaseProvider.XEP_ECUJOBSEX.GetParameterString
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
                                                resultString = ecuResultStateValue.Title?.GetTitle(ActivityCommon.GetCurrentLanguageStatic());
                                            }
                                        }

                                        if (resultString == null)
                                        {
                                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                            if (nodeClassType == EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.Identification)
                                            {
                                                resultString = ConvertEcuResultValueIdent(resultData, out resultValue);
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

        // from: RheingoldCoreFramework.dll BMW.Rheingold.CoreFramework.FormatConverter.ConvertECUResultToString
        public static string ConvertEcuResultValueIdent(EdiabasNet.ResultData resultData, out double? resultValue)
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
                    try
                    {
                        resultValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
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

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        // from: RheingoldPresentationFramework.dll BMW.Rheingold.PresentationFramework.FUWFormatConverter.Convert
        public static string ConvertEcuResultValueEnv(EdiabasNet.ResultData resultData, out double? resultValue)
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
                    result = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", value);
                    resultValue = value;
                }
                else if (resultData.OpData is string)
                {
                    string value = (string)resultData.OpData;
                    result = value;
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
            if (resultData == null)
            {
                return null;
            }

            string resultString = null;
            if (envCondLabel != null && envCondLabel.EcuResultStateValueList != null && envCondLabel.EcuResultStateValueList.Count > 0)
            {
                EcuFunctionStructs.EcuResultStateValue ecuResultStateValue = MatchEcuResultStateValue(envCondLabel.EcuResultStateValueList, resultData);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (ecuResultStateValue != null)
                {
                    resultString = ecuResultStateValue.Title?.GetTitle(ActivityCommon.GetCurrentLanguageStatic());
                }
            }

            if (resultString == null)
            {
                resultString = ConvertEcuResultValueEnv(resultData, out resultValue);
            }

            return resultString;
        }

        // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.ConvertResultValue
        public static string ConvertEcuResultValueStatus(EcuFunctionStructs.EcuJobResult ecuJobResult, EdiabasNet.ResultData resultData, out double? resultValue)
        {
            resultValue = null;
            try
            {
                // corresponds to: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.FindMatchingValue
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
                    // corresponds to: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.TryConvertToDecimal
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

        // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.CheckAndUseNumberFormat
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

        // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.TryRounding
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
                    // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.RoundLastDigit
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

        public static List<string> ConvertFaultCodeError(Context context, Int64 errorCode, bool info, EdiabasErrorReport errorReport, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            List<string> resultList = new List<string>();
            string language = ActivityCommon.GetCurrentLanguageStatic(false, context);
            EcuFunctionStructs.EcuFaultCodeLabel ecuFaultCodeLabel = ActivityCommon.EcuFunctionReader.GetFaultCodeLabel(errorCode, info, ecuVariant);
            if (ecuFaultCodeLabel != null)
            {
                string label = ecuFaultCodeLabel.Title.GetTitle(language);
                if (!string.IsNullOrEmpty(label))
                {
                    resultList.Add(label);
                }
            }

            if (resultList.Count == 0)
            {
                return resultList;
            }

            List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = ActivityCommon.EcuFunctionReader.GetFaultModeLabelList(errorCode, info, ecuVariant);
            if (ecuFaultModeLabelList != null)
            {
                List<Tuple<string, bool>> faultModeResultList = ErrorFaultModeResultList.ToList();
                Int64 typeCount = ActivityMain.GetResultInt64(errorReport.ErrorDict, "F_ART_ANZ", out bool found);
                if (!found || typeCount == 0)
                {
                    typeCount = 1;
                }

                for (int index = 0; index < typeCount; index++)
                {
                    string typeName = string.Format(CultureInfo.InvariantCulture, "F_ART{0}_NR", index + 1);
                    faultModeResultList.Add(new Tuple<string, bool>(typeName, false));
                }

                StringBuilder sbDetail = new StringBuilder();
                foreach (Tuple<string, bool> faultModeResult in faultModeResultList)
                {
                    Int64 modeNumber = ActivityMain.GetResultInt64(errorReport.ErrorDict, faultModeResult.Item1, out found);
                    if (found)
                    {
                        if (!faultModeResult.Item2 || modeNumber != 0)
                        {
                            EcuFunctionStructs.EcuFaultModeLabel ecuFaultModeLabel =
                                ActivityCommon.EcuFunctionReader.GetFaultModeLabelMatchList(ecuFaultModeLabelList, modeNumber).LastOrDefault();
                            if (ecuFaultModeLabel != null)
                            {
                                string label = ecuFaultModeLabel.Title.GetTitle(language);
                                if (!string.IsNullOrEmpty(label))
                                {
                                    if (sbDetail.Length > 0)
                                    {
                                        sbDetail.Append(", ");
                                    }
                                    sbDetail.Append(label);
                                }
                            }
                        }
                    }
                }

                resultList.Add(sbDetail.ToString());
            }

            return resultList;
        }

        // from: RheingoldDiagnostics.dll BMW.Rheingold.Diagnostics.VehicleIdent.SetDTCDetailValues
        public static string ConvertEnvCondErrorDetail(Context context, EdiabasErrorReport errorReport, List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelList)
        {
            OrderedDictionary detailDict = new OrderedDictionary();
            if (errorReport.ErrorDetailSet == null)
            {
                ConvertEnvCondErrorDetailSingle(context, errorReport, ref detailDict, errorReport.ErrorDict, envCondLabelList);
            }
            else
            {
                int dictIndex = 0;
                foreach (Dictionary<string, EdiabasNet.ResultData> errorDetail in errorReport.ErrorDetailSet)
                {
                    if (dictIndex == 0)
                    {
                        dictIndex++;
                        continue;
                    }

                    Int64 envCount = ActivityMain.GetResultInt64(errorDetail, "F_UW_ANZ", out bool countFound);
                    if (!countFound || envCount < 1)
                    {
                        dictIndex++;
                        continue;
                    }

                    ConvertEnvCondErrorDetailSingle(context, errorReport, ref detailDict, errorDetail, envCondLabelList);
                    dictIndex++;
                }
            }

#if false   // test code for result states
            if (envCondLabelList != null)
            {
                int iTextIndex = 0;
                Dictionary<string, int> envCountTestDict = new Dictionary<string, int>();
                foreach (EcuFunctionStructs.EcuEnvCondLabel envCondLabel in envCondLabelList)
                {
                    string language = ActivityCommon.GetCurrentLanguageStatic(false, context);
                    if (envCondLabel.EcuResultStateValueList != null && envCondLabel.EcuResultStateValueList.Count > 0)
                    {
                        string envName = envCondLabel.Title?.GetTitle(language);
                        if (!string.IsNullOrWhiteSpace(envName))
                        {
                            envName = envName.Trim();
                            try
                            {
                                Int64 testVal = Convert.ToInt64(envCondLabel.EcuResultStateValueList[^1].StateValue, CultureInfo.InvariantCulture);
                                EdiabasNet.ResultData testDataVal = new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeI, "test", testVal);
                                string envVal = ConvertEcuEnvCondResultValue(envCondLabel, testDataVal, out double? _) ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(envVal))
                                {
                                    AddEnvCondErrorDetail(ref detailDict, envCountTestDict, envName, "@T" + iTextIndex.ToString(CultureInfo.InvariantCulture), envVal);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    iTextIndex++;
                }
            }
#endif

            if (detailDict.Count == 0)
            {
                return null;
            }

            StringBuilder sbResult = new StringBuilder();
            sbResult.Append(context.GetString(Resource.String.error_env_title));

            foreach (DictionaryEntry detailEntry in detailDict)
            {
                if (detailEntry.Value is EnvCondDetailInfo envCondDetailInfo && envCondDetailInfo.ValueList.Count > 0)
                {
                    sbResult.Append("\r\n- ");
                    if (envCondDetailInfo.Index.HasValue && envCondDetailInfo.Index.Value > 0)
                    {
                        sbResult.Append(string.Format(CultureInfo.InvariantCulture, "({0}.) ", envCondDetailInfo.Index + 1));
                    }

                    StringBuilder sbDetail = new StringBuilder();
                    sbDetail.Append(envCondDetailInfo.Name);
                    sbDetail.Append(": ");
                    int index = 0;
                    foreach (string value in envCondDetailInfo.ValueList)
                    {
                        if (index > 0)
                        {
                            sbDetail.Append(" | ");
                        }

                        sbDetail.Append(value);
                        index++;
                    }

                    sbResult.Append(sbDetail.ToString());
                }
            }

            return sbResult.ToString();
        }

        // from: RheingoldDiagnostics.dll BMW.Rheingold.Diagnostics.VehicleIdent.doECUReadFS, doECUReadFSDetails
        public static bool ConvertEnvCondErrorDetailSingle(Context context, EdiabasErrorReport errorReport, ref OrderedDictionary detailDict, Dictionary<string, EdiabasNet.ResultData> errorDetail, List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelList)
        {
            if (errorDetail == null)
            {
                return false;
            }

            string language = ActivityCommon.GetCurrentLanguageStatic(false, context);
            Int64 envCount = ActivityMain.GetResultInt64(errorDetail, "F_UW_ANZ", out bool countFound);
            if (!countFound || envCount < 1)
            {
                return false;
            }

            Dictionary<string, int> envCountDict = new Dictionary<string, int>();
            ConvertEnvCondErrorStd(ref envCountDict, ref detailDict, context, errorReport, errorDetail, envCondLabelList);

            if (envCondLabelList != null)
            {
                for (int index = 0; index < envCount; index++)
                {
                    string envNumName = string.Format(CultureInfo.InvariantCulture, "F_UW{0}_NR", index + 1);
                    Int64 envNum = ActivityMain.GetResultInt64(errorDetail, envNumName, out bool numFound);

                    string envValName = string.Format(CultureInfo.InvariantCulture, "F_UW{0}_WERT", index + 1);
                    bool valueFound = errorDetail.TryGetValue(envValName.ToUpperInvariant(), out EdiabasNet.ResultData resultDataVal);
                    string envUnitName = string.Format(CultureInfo.InvariantCulture, "F_UW{0}_EINH", index + 1);
                    string envUnit = ActivityMain.GetResultString(errorDetail, envUnitName, out bool unitFound);
                    if (!unitFound)
                    {
                        envUnit = string.Empty;
                    }

                    if (numFound && valueFound)
                    {
                        EcuFunctionStructs.EcuEnvCondLabel envCondLabel = ActivityCommon.EcuFunctionReader.GetEnvCondLabelMatchList(envCondLabelList, envNum).LastOrDefault();
                        if (envCondLabel != null)
                        {
                            string envName = envCondLabel.Title?.GetTitle(language);
                            if (!string.IsNullOrWhiteSpace(envName))
                            {
                                envName = envName.Trim();
                                string envVal = ConvertEcuEnvCondResultValue(envCondLabel, resultDataVal, out double? _) ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(envVal))
                                {
                                    envVal = envVal.Trim();
                                    if (!string.IsNullOrWhiteSpace(envCondLabel.Unit))
                                    {
                                        envUnit = envCondLabel.Unit;
                                    }

                                    StringBuilder sbValue = new StringBuilder();
                                    sbValue.Append(envVal);
                                    if (!string.IsNullOrWhiteSpace(envUnit))
                                    {
                                        sbValue.Append(" ");
                                        sbValue.Append(envUnit.Trim());
                                    }

                                    AddEnvCondErrorDetail(ref detailDict, ref envCountDict, envName, "@" + envNum.ToString(CultureInfo.InvariantCulture), sbValue.ToString());
                                }
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, int> envCountPair in envCountDict)
            {
                if (envCountPair.Value > 0)
                {
                    for (int index = 0; index <= envCountPair.Value; index++)
                    {
                        string detailKey = envCountPair.Key + "_" + index.ToString(CultureInfo.InvariantCulture);
                        if (detailDict[detailKey] is EnvCondDetailInfo envCondDetailInfo)
                        {
                            envCondDetailInfo.Index = index;
                        }
                    }
                }
            }

            return true;
        }

        public static bool ConvertEnvCondErrorStd(ref Dictionary<string, int> envCountDict, ref OrderedDictionary detailDict, Context context, EdiabasErrorReport errorReport, Dictionary<string, EdiabasNet.ResultData> errorDetail, List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelList)
        {
            DateTime? lifeStartDate = errorReport.LifeStartDate;
            string language = ActivityCommon.GetCurrentLanguageStatic(false, context);
            int envCondIndex = 0;
            foreach (EnvCondResultInfo envCondResult in ErrorEnvCondResultList)
            {
                string envCondName = envCondResult.Result;
                string envValText = null;
                bool valueFound = errorDetail.TryGetValue(envCondName.ToUpperInvariant(), out EdiabasNet.ResultData resultDataVal);
                if (!valueFound && string.Compare(envCondName, "F_UW_KM", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string kmText = GetEnvCondKmLast(errorDetail);
                    if (!string.IsNullOrEmpty(kmText))
                    {
                        envValText = kmText;
                    }
                }

                if (valueFound || !string.IsNullOrEmpty(envValText))
                {
                    string envName = null;
                    EcuFunctionStructs.EcuEnvCondLabel envCondLabel = null;
                    if (envCondLabelList != null)
                    {
                        envCondLabel = ActivityCommon.EcuFunctionReader.GetEnvCondLabelMatchList(envCondLabelList, envCondName).LastOrDefault();
                    }

                    if (envCondLabel != null)
                    {
                        envName = envCondLabel.Title?.GetTitle(language);
                    }

                    if (string.IsNullOrWhiteSpace(envName))
                    {
                        if (envCondResult.ResourceId.HasValue)
                        {
                            envName = context.GetString(envCondResult.ResourceId.Value);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(envName))
                    {
                        envName = envName.Trim();

                        string envVal;
                        if (!string.IsNullOrEmpty(envValText))
                        {
                            envVal = envValText;
                        }
                        else
                        {
                            envVal = ConvertEcuEnvCondResultValue(envCondLabel, resultDataVal, out double? _) ?? string.Empty;
                        }

                        double? resultValue = null;
                        if (resultDataVal != null)
                        {
                            if (resultDataVal.OpData is Int64)
                            {
                                resultValue = (Int64)resultDataVal.OpData;
                            }
                            else if (resultDataVal.OpData is Double)
                            {
                                resultValue = (Double)resultDataVal.OpData;
                            }
                        }

                        if (envCondResult.MinLength.HasValue)
                        {
                            if (envVal.Length < envCondResult.MinLength.Value)
                            {
                                envVal = string.Empty;
                            }
                        }

                        if (envCondResult.MinValue.HasValue)
                        {
                            if (resultValue.HasValue && resultValue < envCondResult.MinValue.Value)
                            {
                                envVal = string.Empty;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(envVal))
                        {
                            envVal = envVal.Trim();
                            string envUnit = envCondResult.Unit;
                            if (!string.IsNullOrWhiteSpace(envCondLabel?.Unit))
                            {
                                envUnit = envCondLabel.Unit;
                            }

                            if (envCondResult.ConvertDate.HasValue && envCondResult.ConvertDate.Value)
                            {
                                envVal = FormatTimeStampEntry(resultValue.Value, lifeStartDate);
                                envUnit = string.Empty;
                            }

                            StringBuilder sbValue = new StringBuilder();
                            sbValue.Append(envVal);
                            if (!string.IsNullOrWhiteSpace(envUnit))
                            {
                                sbValue.Append(" ");
                                sbValue.Append(envUnit.Trim());
                            }

                            AddEnvCondErrorDetail(ref detailDict, ref envCountDict, envName, "#" + (envCondIndex + 1).ToString(CultureInfo.InvariantCulture), sbValue.ToString());
                        }
                    }
                }

                envCondIndex++;
            }

            return true;
        }

        public static string GetEnvCondKmLast(Dictionary<string, EdiabasNet.ResultData> errorDetail)
        {
            string kmLastText = ActivityMain.FormatResultString(errorDetail, "F_KM_LAST", "{0}");
            if (!string.IsNullOrEmpty(kmLastText))
            {
                string[] kmArrary = kmLastText.Split(' ');
                if (kmArrary.Length > 0)
                {
                    return kmArrary[0];
                }
            }

            return string.Empty;
        }

        // from: BMW.Rheingold.CoreFramework.DatabaseProvider.FaultCode.GetTimeStampDisplayItem
        public static string FormatTimeStampEntry(double timeStamp, DateTime? lifeStartDate)
        {
            try
            {
                if (lifeStartDate.HasValue && lifeStartDate.Value != default(DateTime))
                {
                    DateTime timeStampDate = lifeStartDate.Value.AddSeconds(timeStamp);
                    return timeStampDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                if (timeStamp < TimeSpan.MinValue.TotalSeconds || timeStamp > TimeSpan.MaxValue.TotalSeconds)
                {
                    return string.Empty;
                }

                TimeSpan ts = TimeSpan.FromSeconds(timeStamp);
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1:D2}:{2:D2}:{3:D2}s", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        public static void AddEnvCondErrorDetail(ref OrderedDictionary detailDict, ref Dictionary<string, int> envCountDict, string name, string key, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
            {
                return;
            }

            if (envCountDict.TryGetValue(key, out int envIndex))
            {
                envIndex++;
            }
            else
            {
                envIndex = 0;
            }

            envCountDict[key] = envIndex;

            string detailKey = key + "_" + envIndex.ToString(CultureInfo.InvariantCulture);
            EnvCondDetailInfo envCondDetailInfo = detailDict[detailKey] as EnvCondDetailInfo;
            if (envCondDetailInfo == null)
            {
                envCondDetailInfo = new EnvCondDetailInfo(name, value);
                detailDict.Add(detailKey, envCondDetailInfo);
            }
            else
            {
                if (!envCondDetailInfo.ValueList.Contains(value))
                {
                    envCondDetailInfo.ValueList.Add(value);
                }
            }
        }

        // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionReadStatus.FindMatchingValue
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

            if (_lastPageInfo != null && _lastPageInfo != JobPageInfo)
            {
                return true;
            }

            return false;
        }

        private void ForceLoadSgbd()
        {
            Ediabas.ArgString = string.Empty;
            Ediabas.ArgBinaryStd = null;
            Ediabas.ResultsRequests = string.Empty;
            Ediabas.NoInitForVJobs = false;
            Ediabas.ExecuteJob("_JOBS");    // force to load file
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
            UpdateProgressState = UpdateState.Init;
            UpdateProgress = 0;

            _ediabasInitReq = true;
            _ediabasJobAbort = deviceChange;
            OpenDataLog(pageInfo);
            if (pageInfo != null)
            {
                SendActionBroadcast("page_change");
            }
        }

        public bool LogFormat(string format, params object[] args)
        {
            lock (_logQueueLock)
            {
                _logQueue.Enqueue(new LogInfo(format, args));
            }

            return true;
        }

        public bool LogString(string info)
        {
            lock (_logQueueLock)
            {
                _logQueue.Enqueue(new LogInfo(info));
            }

            return true;
        }

        private void ProcessLogQueue()
        {
            try
            {
                for (; ; )
                {
                    LogInfo logInfo = null;

                    lock (_logQueueLock)
                    {
                        if (_logQueue.Count > 0)
                        {
                            logInfo = _logQueue.Dequeue();
                        }
                    }

                    if (logInfo == null || Ediabas == null)
                    {
                        break;
                    }

                    if (!string.IsNullOrEmpty(logInfo.Info))
                    {
                        Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, logInfo.Info);
                    }

                    if (!string.IsNullOrEmpty(logInfo.Format))
                    {
                        Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, logInfo.Format, logInfo.Args);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public bool TriggerDisplayUpdate()
        {
            try
            {
                DataUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void DataUpdatedEvent(bool forceUpdate = false)
        {
            ProcessLogQueue();

            bool update = forceUpdate;
            if (_lastUpdateTime != null)
            {
                if (Stopwatch.GetTimestamp() - _lastUpdateTime >= UpdateDisplayDelay * ActivityCommon.TickResolMs)
                {
                    update = true;
                }
            }
            else
            {
                update = true;
            }

            if (update)
            {
                DataUpdated?.Invoke(this, EventArgs.Empty);
                _lastUpdateTime = Stopwatch.GetTimestamp();
            }
            else
            {
#if DEBUG
                Log.Info(Tag, "DataUpdatedEvent: ignored");
#endif
            }
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
