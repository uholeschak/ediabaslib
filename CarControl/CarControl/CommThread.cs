using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using EdiabasLib;
using Ftdi;

namespace CarControl
{
    class CommThread : IDisposable
    {
        public delegate void DataUpdatedEventHandler(object sender, EventArgs e);
        public event DataUpdatedEventHandler DataUpdated;

        public enum OperationMode
        {
            OpModeStatus,
            OpModeUp,
            OpModeDown,
        }

        public enum SelectedDevice
        {
            DeviceAxis,
            DeviceMotor,
            DeviceMotorUnevenRunning,
            DeviceMotorRotIrregular,
            DeviceMotorPM,
            DeviceCccNav,
            DeviceIhk,
            DeviceErrors,
            Test,
        }

        public enum CccNavGpsPosType
        {
            CccNavGpsPosTypeUnknown,
            CccNavGpsPosTypeSearch,
            CccNavGpsPosTypeTrack,
            CccNavGpsPosType2D,
            CccNavGpsPosType3D,
        }

        public const int AxisModeNormal = 0x00;
        public const int AxisModeConveyor = 0x02;
        public const int AxisModeTransport = 0x04;
        public const int AxisModeGarage = 0x40;
        public const int AxisModeMask = 0x46;

        public OperationMode AxisOpMode
        {
            get;
            set;
        }

        public SelectedDevice Device
        {
            get;
            set;
        }

        public bool Connected
        {
            get;
            private set;
        }
        public int ErrorCounter
        {
            get;
            private set;
        }
        public Dictionary<string, EdiabasNet.ResultData> EdiabasResultDict
        {
            get;
            private set;
        }
        public List<EdiabasErrorReport> EdiabasErrorReportList
        {
            get;
            private set;
        }
        public string EdiabasErrorMessage
        {
            get;
            private set;
        }

        public string TestResult
        {
            get;
            private set;
        }

        private class EdiabasJob
        {
            private string jobName;
            private string jobArgs;
            private string resultRequests;

            public EdiabasJob(string jobName, string jobArgs, string resultRequests)
            {
                this.jobName = jobName;
                this.jobArgs = jobArgs;
                this.resultRequests = resultRequests;
            }

            public string JobName
            {
                get
                {
                    return jobName;
                }
            }

            public string JobArgs
            {
                get
                {
                    return jobArgs;
                }
            }

            public string ResultRequests
            {
                get
                {
                    return resultRequests;
                }
            }
        }

        private class EdiabasJobs
        {
            private string sgbdFile;
            private EdiabasJob[] jobArray;

            public EdiabasJobs(string sgbdFile, EdiabasJob[] jobArray)
            {
                this.sgbdFile = sgbdFile;
                this.jobArray = jobArray;
            }

            public string SgbdFile
            {
                get
                {
                    return sgbdFile;
                }
            }

            public EdiabasJob[] JobArray
            {
                get
                {
                    return jobArray;
                }
            }
        }

        private class EdiabasTestJob
        {
            private string sgbdFile;
            private string jobName;
            private string jobArgs;
            private string resultRequests;
            private string[] jobData;

            public EdiabasTestJob(string sgbdFile, string jobName, string jobArgs, string resultRequests, string[] jobData)
            {
                this.sgbdFile = sgbdFile;
                this.jobName = jobName;
                this.jobArgs = jobArgs;
                this.resultRequests = resultRequests;
                this.jobData = jobData;
            }

            public string SgbdFile
            {
                get
                {
                    return sgbdFile;
                }
            }

            public string JobName
            {
                get
                {
                    return jobName;
                }
            }

            public string JobArgs
            {
                get
                {
                    return jobArgs;
                }
            }

            public string ResultRequests
            {
                get
                {
                    return resultRequests;
                }
            }

            public string[] JobData
            {
                get
                {
                    return jobData;
                }
            }
        }

        private const int _readTimeoutMin = 500;    // min read timeout [ms]
        private const int _writeTimeout = 500;      // write timeout [ms]

        private class EdiabasErrorRequest
        {
            private string deviceName;
            private string sgbdFile;

            public EdiabasErrorRequest(string deviceName, string sgbdFile)
            {
                this.deviceName = deviceName;
                this.sgbdFile = sgbdFile;
            }

            public string DeviceName
            {
                get { return deviceName; }
            }
            public string SgbdFile
            {
                get { return sgbdFile; }
            }
        }

        public class EdiabasErrorReport
        {
            private string deviceName;
            private Dictionary<string, EdiabasNet.ResultData> errorDict;
            private List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet;
            private string execptionText;

            public EdiabasErrorReport(string deviceName, Dictionary<string, EdiabasNet.ResultData> errorDict, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet) :
                this(deviceName, errorDict, errorDetailSet, string.Empty)
            {
            }

            public EdiabasErrorReport(string deviceName, Dictionary<string, EdiabasNet.ResultData> errorDict, List<Dictionary<string, EdiabasNet.ResultData>> errorDetailSet, string execptionText)
            {
                this.deviceName = deviceName;
                this.errorDict = errorDict;
                this.errorDetailSet = errorDetailSet;
                this.execptionText = execptionText;
            }

            public string DeviceName
            {
                get
                {
                    return deviceName;
                }
            }

            public Dictionary<string, EdiabasNet.ResultData> ErrorDict
            {
                get
                {
                    return errorDict;
                }
            }

            public List<Dictionary<string, EdiabasNet.ResultData>> ErrorDetailSet
            {
                get
                {
                    return errorDetailSet;
                }
            }

            public string ExecptionText
            {
                get
                {
                    return execptionText;
                }
            }
        }

        static private readonly EdiabasErrorRequest[] EdiabasErrorRequestList =
        {
            new EdiabasErrorRequest("errorNameCAS", "d_cas"),
            new EdiabasErrorRequest("errorNameDDE", "d_motor"),
            new EdiabasErrorRequest("errorNameEKPS", "d_ekp"),
            new EdiabasErrorRequest("errorNameEHC", "d_ehc"),
            new EdiabasErrorRequest("errorNameDSC", "d_dsc"),
            new EdiabasErrorRequest("errorNameACSM", "d_sim"),
            new EdiabasErrorRequest("errorNameAHM", "d_ahm"),
            new EdiabasErrorRequest("errorNameCCCBO", "d_mmi"),
            new EdiabasErrorRequest("errorNameCCCGW", "d_mostgw"),
            new EdiabasErrorRequest("errorNameCCCA", "d_ccc"),
            new EdiabasErrorRequest("errorNameCCCANT", "d_anttu"),
            new EdiabasErrorRequest("errorNameCCCASK", "d_ask"),
            new EdiabasErrorRequest("errorNameCDC", "d_cdc"),
            new EdiabasErrorRequest("errorNameCID", "d_cid"),
            new EdiabasErrorRequest("errorNameCON", "d_ec"),
            new EdiabasErrorRequest("errorNameIHK", "d_klima"),
            new EdiabasErrorRequest("errorNameKBM", "d_kbm"),
            new EdiabasErrorRequest("errorNameKGM", "d_zgm"),
            new EdiabasErrorRequest("errorNameKOMBI", "d_kombi"),
            new EdiabasErrorRequest("errorNameLM", "d_lm"),
            new EdiabasErrorRequest("errorNamePDC", "d_pdc"),
            new EdiabasErrorRequest("errorNameRLS", "rlss70" /*"d_rls"*/),
            new EdiabasErrorRequest("errorNameSZL", "d_szl"),
            new EdiabasErrorRequest("errorNameSZM", "d_bzm"),
            new EdiabasErrorRequest("errorNameTCU", "d_tel"),
        };

        static private readonly EdiabasJobs EhcJobs = new EdiabasJobs("d_ehc",
            new EdiabasJob[]
            {
                new EdiabasJob("LESEN_REGLERWERTE",
                    string.Empty,
                    "STATE_SPEED"),
                new EdiabasJob("LESEN_ANALOGWERTE",
                    string.Empty,
                    "ANALOG_U_KL30"),
                new EdiabasJob("LESEN_FILTERWERTE",
                    string.Empty,
                    "ORGFASTFILTER_RL;ORGFASTFILTER_RR;FASTFILTER_RL;FASTFILTER_RR"),
            });

        static private readonly EdiabasJobs MotorJobs = new EdiabasJobs("d_motor",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_MESSWERTBLOCK_LESEN",
                    "IUBAT;ITKUM;CTSCD_tClntLin;ITKRS;ILMKG;ILMMG;SLMMG;ITUMG;ITKRS;IPLAD;SPLAD;ITLAL;IPUMG;IPRDR;SPRDR;ITAVO;ITAVP1;IPDIP;IDSLRE;IREAN;EGT_st;ISRBF;ISOED",
                    string.Empty),
            });

        static private readonly EdiabasJobs MotorUnevenJobs = new EdiabasJobs("d_motor",
            new EdiabasJob[]
            {
                new EdiabasJob("START_SYSTEMCHECK_ZYL",
                    "LLR_EIN",
                    "JOB_STATUS"
                    ),
                new EdiabasJob("STATUS_LAUFUNRUHE_LLR_MENGE",
                    string.Empty,
                    "STAT_LAUFUNRUHE_LLR_MENGE_ZYL1_WERT;STAT_LAUFUNRUHE_LLR_MENGE_ZYL2_WERT;STAT_LAUFUNRUHE_LLR_MENGE_ZYL3_WERT;STAT_LAUFUNRUHE_LLR_MENGE_ZYL4_WERT"
                    ),
            });

        static private readonly EdiabasJobs MotorRotIrregularJobs = new EdiabasJobs("d_motor",
            new EdiabasJob[]
            {
                new EdiabasJob("START_SYSTEMCHECK_ZYL",
                    "LLR_AUS",
                    "JOB_STATUS"
                    ),
                new EdiabasJob("STATUS_LAUFUNRUHE_DREHZAHL",
                    string.Empty,
                    "STAT_LAUFUNRUHE_DREHZAHL_ZYL1_WERT;STAT_LAUFUNRUHE_DREHZAHL_ZYL2_WERT;STAT_LAUFUNRUHE_DREHZAHL_ZYL3_WERT;STAT_LAUFUNRUHE_DREHZAHL_ZYL4_WERT"
                    ),
            });

        static private readonly EdiabasJobs MotorPmJobs = new EdiabasJobs("d_motor",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_SYSTEMCHECK_PM_INFO_2",
                    string.Empty,
                    "STAT_BATTERIE_KAPAZITAET_WERT;STAT_KALIBRIER_EVENT_CNT_WERT;STAT_LADUNGSZUSTAND_AKTUELL_WERT;STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT;STAT_Q_SOC_AKTUELL_WERT;STAT_Q_SOC_VOR_1_TAG_WERT;STAT_SOC_FIT_WERT;STAT_SOH_WERT;STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT;STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT;STAT_TEMP_SAISON_WERT"
                    ),
            });

        static private readonly EdiabasJobs CccNavJobs = new EdiabasJobs("d_ccc",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_GPS_TRACKING",
                    string.Empty,
                    "STAT_HIP_DRIVER;STAT_GPS;STAT_GPS_TEXT;STAT_ALMANACH"
                    ),
                new EdiabasJob("STATUS_DR_POSITION",
                    string.Empty,
                    "STAT_GPS_POSITION_BREITE;STAT_GPS_POSITION_HOEHE;STAT_GPS_POSITION_LAENGE;STAT_SPEED_VAL"
                    ),
                new EdiabasJob("STATUS_GPS_TIME",
                    string.Empty,
                    "STAT_TIME_DATE_VAL"
                    ),
                new EdiabasJob("STATUS_GPS_DOP",
                    string.Empty,
                    "STAT_HORIZONTALE_AUFLOES;STAT_VERTICALE_AUFLOES;STAT_POSITION_AUFLOES"
                    ),
            });

        static private readonly EdiabasJobs IhkJobs = new EdiabasJobs("d_klima",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_REGLERGROESSEN",
                    string.Empty,
                    "STAT_TINNEN_WERT;STAT_TINNEN_VERZOEGERT_WERT;STAT_TAUSSEN_WERT;STAT_SOLL_LI_KORRIGIERT_WERT;STAT_WT_RE_WERT;STAT_WTSOLL_RE_WERT"
                    ),
            });

        // for tests
        static private readonly EdiabasTestJob[] MotorJobList = {
            new EdiabasTestJob("d_motor", "STATUS_MESSWERTBLOCK_LESEN",
                "IUBAT;CTSCD_tClntLin;ITKRS;ILMKG;ILMMG;SLMMG;ITUMG;ITKRS;IPLAD;SPLAD;ITLAL;IPUMG;IPRDR;SPRDR;ITAVO;ITAVP1;IPDIP;IDSLRE;IREAN;EGT_st;ISRBF;ISOED",
                string.Empty,
                new string[] {
                    "STAT_UBATT_WERT",
                    "STAT_CTSCD_tClntLin_WERT",
                    "STAT_KRAFTSTOFFTEMPERATURK_WERT",
                    "STAT_LUFTMASSE_WERT",
                    "STAT_LUFTMASSE_PRO_HUB_WERT",
                    "STAT_LUFTMASSE_SOLL_WERT",
                    "STAT_UMGEBUNGSTEMPERATUR_WERT",
                    "STAT_KRAFTSTOFFTEMPERATURK_WERT",
                    "STAT_LADEDRUCK_WERT",
                    "STAT_LADEDRUCK_SOLL_WERT",
                    "STAT_LADELUFTTEMPERATUR_WERT",
                    "STAT_UMGEBUNGSDRUCK_WERT",
                    "STAT_RAILDRUCK_WERT",
                    "STAT_RAILDRUCK_SOLL_WERT",
                    "STAT_ABGASTEMPERATUR_VOR_KATALYSATOR_WERT",
                    "STAT_ABGASTEMPERATUR_VOR_PARTIKELFILTER_1_WERT",
                    "STAT_DIFFERENZDRUCK_UEBER_PARTIKELFILTER_WERT",
                    "STAT_STRECKE_SEIT_ERFOLGREICHER_REGENERATION_WERT",
                    "STAT_REGENERATIONSANFORDERUNG_WERT",
                    "STAT_EGT_st_WERT",
                    "STAT_REGENERATION_BLOCKIERUNG_UND_FREIGABE_WERT",
                    "STAT_OELDRUCKSCHALTER_EIN_WERT",
                }),
            };

        static private readonly EdiabasTestJob[] MotorPmJobList = {
            new EdiabasTestJob("d_motor", "STATUS_SYSTEMCHECK_PM_INFO_2",
                "STAT_BATTERIE_KAPAZITAET_WERT;STAT_KALIBRIER_EVENT_CNT_WERT;STAT_LADUNGSZUSTAND_AKTUELL_WERT;STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT;STAT_Q_SOC_AKTUELL_WERT;STAT_Q_SOC_VOR_1_TAG_WERT;STAT_SOC_FIT_WERT;STAT_SOH_WERT;STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT;STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT;STAT_TEMP_SAISON_WERT",
                "STAT_BATTERIE_KAPAZITAET_WERT;STAT_KALIBRIER_EVENT_CNT_WERT;STAT_LADUNGSZUSTAND_AKTUELL_WERT;STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT;STAT_Q_SOC_AKTUELL_WERT;STAT_Q_SOC_VOR_1_TAG_WERT;STAT_SOC_FIT_WERT;STAT_SOH_WERT;STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT;STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT;STAT_TEMP_SAISON_WERT",
                new string[] {
                    "STAT_BATTERIE_KAPAZITAET_WERT",
                    "STAT_KALIBRIER_EVENT_CNT_WERT",
                    "STAT_LADUNGSZUSTAND_AKTUELL_WERT",
                    "STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT",
                    "STAT_Q_SOC_AKTUELL_WERT",
                    "STAT_Q_SOC_VOR_1_TAG_WERT",
                    "STAT_SOC_FIT_WERT",
                    "STAT_SOH_WERT",
                    "STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT",
                    "STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT",
                    "STAT_TEMP_SAISON_WERT",
                }),
            };

        static private readonly EdiabasTestJob[] ErrorJobList = {
            new EdiabasTestJob("d_motor", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_motor", "FS_LESEN_DETAIL", "0x4232", "F_UW_KM",
                new string[] {
                    "F_UW_KM",
                }),
            new EdiabasTestJob("d_ccc", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_ehc", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_ehc", "FS_LESEN_DETAIL", "0x5FB4", "F_UW_KM",
                new string[] {
                    "F_UW_KM",
                }),
            new EdiabasTestJob("d_klima", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            };

        static public readonly Object DataLock = new Object();

        private bool disposed = false;
        private volatile bool _stopThread;
        private bool _threadRunning;
        private Thread _workerThread;
        private string _comPort;
        private SerialPort _serialPort;
        private IntPtr _handleFtdi;
        private int _baudRateFtdi;
        private Parity _parityFtdi;
        private StreamWriter _swLog;
        private Stopwatch _logTimeWatch;
        private Stopwatch commStopWatch;
        private EdiabasNet ediabas;
        private bool ediabasInitReq;
        private bool ediabasJobAbort;
        private string ediabasSgbdFile;
        private int ediabasUpdateCount;
        private int ediabasInternalStep;
        private Dictionary<string, EdiabasNet.ResultData> ediabasTempDict;
        private Dictionary<string, EdiabasNet.ResultData> ediabasDynDict;

        public CommThread()
        {
            _stopThread = false;
            _threadRunning = false;
            _workerThread = null;
            _serialPort = new SerialPort();
            _handleFtdi = (IntPtr) 0;
            _baudRateFtdi = 0;
            _parityFtdi = Parity.None;
            _swLog = null;
            _logTimeWatch = new Stopwatch();
            commStopWatch = new Stopwatch();
            ediabas = new EdiabasNet();

            EdInterfaceObd edInterfaceBwmFast = new EdInterfaceObd(ediabas);
            ediabas.EdInterfaceClass = edInterfaceBwmFast;
            edInterfaceBwmFast.InterfaceConnectFunc = InterfaceConnect;
            edInterfaceBwmFast.InterfaceDisconnectFunc = InterfaceDisconnect;
            edInterfaceBwmFast.InterfaceSetConfigFunc = InterfaceSetConfig;
            edInterfaceBwmFast.SendDataFunc = SendData;
            edInterfaceBwmFast.ReceiveDataFunc = ReceiveData;

            ediabas.AbortJobFunc = AbortEdiabasJob;

            string ecuPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "Ecu");
            if (ecuPath.StartsWith("\\Windows\\"))
            {
                ecuPath = "\\UM\\Program Files\\CarControl\\Ecu";
            }
            ediabas.SetConfigProperty("EcuPath", ecuPath);

            // public properties
            TestResult = string.Empty;

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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_swLog != null)
                    {
                        _swLog.Dispose();
                        _swLog = null;
                    }
                    ediabas.Dispose();
                    ediabas = null;
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        public bool StartThread(string comPort, string logFile)
        {
            try
            {
                StopThread();
                _stopThread = false;
                _comPort = comPort;
                _swLog = null;
                if (logFile.Length > 0)
                {
                    try
                    {
                        _swLog = new StreamWriter(logFile);
                    }
                    catch (Exception)
                    {
                        _swLog = null;
                    }
                }
                _logTimeWatch.Reset();
                InitProperties();
                _workerThread = new Thread(ThreadFunc);
                _threadRunning = true;
                _workerThread.Priority = ThreadPriority.Highest;
                _workerThread.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void StopThread()
        {
            if (_swLog != null)
            {
                try
                {
                    _swLog.Dispose();
                }
                catch (Exception)
                {
                }
                _swLog = null;
            }
            _logTimeWatch.Reset();
            if (_workerThread != null)
            {
                _stopThread = true;
                _workerThread.Join();
                _workerThread = null;
            }
        }

        public bool ThreadRunning()
        {
            if (_workerThread == null) return false;
            return _threadRunning;
        }

        private void ThreadFunc()
        {
            if (Connect())
            {
                DataUpdatedEvent();
                SelectedDevice lastDevice = (SelectedDevice) (-1);
                while (!_stopThread)
                {
                    try
                    {
                        bool result = true;
                        SelectedDevice copyDevice = Device;

                        if (lastDevice != copyDevice)
                        {
                            lastDevice = copyDevice;
                            InitProperties(true);
                        }

                        switch (copyDevice)
                        {
                            case SelectedDevice.DeviceAxis:
                                result = CommEhc(copyDevice, EhcJobs);
                                break;

                            case SelectedDevice.DeviceMotor:
                                result = CommEdiabas(copyDevice, MotorJobs);
                                break;

                            case SelectedDevice.DeviceMotorUnevenRunning:
                                result = CommEdiabas(copyDevice, MotorUnevenJobs);
                                break;

                            case SelectedDevice.DeviceMotorRotIrregular:
                                result = CommEdiabas(copyDevice, MotorRotIrregularJobs);
                                break;

                            case SelectedDevice.DeviceMotorPM:
                                result = CommEdiabas(copyDevice, MotorPmJobs);
                                break;

                            case SelectedDevice.DeviceCccNav:
                                result = CommEdiabas(copyDevice, CccNavJobs);
                                break;

                            case SelectedDevice.DeviceIhk:
                                result = CommEdiabas(copyDevice, IhkJobs);
                                break;

                            case SelectedDevice.DeviceErrors:
                                result = CommErrorsEdiabas(copyDevice);
                                break;

                            case SelectedDevice.Test:
                                result = CommTest();
                                break;
                        }

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
                Disconnect();
            }
            _threadRunning = false;
            DataUpdatedEvent();
        }

        private bool CommEhc(SelectedDevice device, EdiabasJobs ediabasJobs)
        {
            OperationMode copyOpMode = AxisOpMode;
            bool firstRequestCall = false;
            if (ediabasInitReq)
            {
                firstRequestCall = true;
                ediabasJobAbort = false;

                try
                {
                    ediabas.ResolveSgbdFile(ediabasJobs.SgbdFile);
                }
                catch (Exception ex)
                {
                    string exText = EdiabasNet.GetExceptionText(ex);
                    lock (CommThread.DataLock)
                    {
                        EdiabasErrorMessage = exText;
                    }
                    Thread.Sleep(1000);
                    return false;
                }

                ediabasInitReq = false;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = null;

            if (firstRequestCall)
            {
                ediabasTempDict = null;
                ediabasDynDict = null;
                ediabasInternalStep = 0;
            }

            EdiabasJob job = ediabasJobs.JobArray[ediabasInternalStep];
            ediabas.ArgString = job.JobArgs;
            ediabas.ArgBinaryStd = null;
            ediabas.ResultsRequests = job.ResultRequests;

            ediabas.TimeMeas = 0;
            try
            {
                ediabas.ExecuteJob(job.JobName);

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                if (resultSets != null && resultSets.Count >= 2)
                {
                    MergeResultDictionarys(ref ediabasTempDict, resultSets[1]);
                }
            }
            catch (Exception ex)
            {
                ediabasInitReq = true;
                string exText = EdiabasNet.GetExceptionText(ex);
                lock (CommThread.DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorMessage = exText;
                }
                Thread.Sleep(1000);
                return true;
            }
            ediabasInternalStep++;
            if (ediabasInternalStep >= ediabasJobs.JobArray.Length)
            {
                ediabasInternalStep = 0;
                ediabasDynDict = ediabasTempDict;
                ediabasTempDict = null;
            }

            resultDict = ediabasDynDict;
            // get mode
            int axisMode = -1;
            ediabas.ArgString = string.Empty;
            ediabas.ArgBinaryStd = null;
            ediabas.ResultsRequests = "WERT";

            try
            {
                ediabas.ExecuteJob("MODE_CTRL_LESEN");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                if (resultSets != null && resultSets.Count >= 2)
                {
                    EdiabasNet.ResultData resultData;
                    if (resultSets[1].TryGetValue("WERT", out resultData))
                    {
                        if (resultData.opData.GetType() == typeof(Int64))
                        {
                            axisMode = (int)((Int64)resultData.opData);
                        }
                    }
                    MergeResultDictionarys(ref resultDict, resultSets[1], "MODE_CTRL_LESEN_");
                }
            }
            catch (Exception ex)
            {
                ediabasInitReq = true;
                string exText = EdiabasNet.GetExceptionText(ex);
                lock (CommThread.DataLock)
                {
                    EdiabasResultDict = null;
                    EdiabasErrorMessage = exText;
                }
                Thread.Sleep(1000);
                return true;
            }

            if (axisMode >= 0)
            {
                if (!_stopThread && (copyOpMode == OperationMode.OpModeUp) &&
                    (axisMode & AxisModeMask) != 0x00)
                {   // set normal mode
                    ediabas.ArgString = "aus;aus;aus";
                    ediabas.ArgBinaryStd = null;
                    ediabas.ResultsRequests = "JOB_STATUS";

                    try
                    {
                        ediabas.ExecuteJob("ENERGIESPARMODE");
                    }
                    catch (Exception ex)
                    {
                        ediabasInitReq = true;
                        string exText = EdiabasNet.GetExceptionText(ex);
                        lock (CommThread.DataLock)
                        {
                            EdiabasResultDict = null;
                            EdiabasErrorMessage = exText;
                        }
                        Thread.Sleep(1000);
                        return true;
                    }
                }

                if (!_stopThread && (copyOpMode == OperationMode.OpModeDown) &&
                    (axisMode & AxisModeConveyor) == 0x00)
                {   // set conveyor mode
                    ediabas.ArgString = "ein;aus;aus";
                    ediabas.ArgBinaryStd = null;
                    ediabas.ResultsRequests = "JOB_STATUS";

                    try
                    {
                        ediabas.ExecuteJob("ENERGIESPARMODE");
                    }
                    catch (Exception ex)
                    {
                        ediabasInitReq = true;
                        string exText = EdiabasNet.GetExceptionText(ex);
                        lock (CommThread.DataLock)
                        {
                            EdiabasResultDict = null;
                            EdiabasErrorMessage = exText;
                        }
                        Thread.Sleep(1000);
                        return true;
                    }
                }

                if (!_stopThread && (copyOpMode == OperationMode.OpModeDown) && (axisMode & AxisModeConveyor) != 0x00)
                {   // manual down

                    // read channel states
                    for (int channel = 0; channel < 4; channel++)
                    {
                        if (_stopThread)
                        {
                            break;
                        }
                        ediabas.ArgString = string.Format("0x{0:X02}", 0x11 + channel);
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = "WERT";

                        try
                        {
                            ediabas.ExecuteJob("STATUS_SIGNALE_NUMERISCH");
                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                MergeResultDictionarys(ref resultDict, resultSets[1], string.Format("STATUS_SIGNALE_NUMERISCH{0}_", channel));
                            }
                        }
                        catch (Exception ex)
                        {
                            ediabasInitReq = true;
                            string exText = EdiabasNet.GetExceptionText(ex);
                            lock (CommThread.DataLock)
                            {
                                EdiabasResultDict = null;
                                EdiabasErrorMessage = exText;
                            }
                            Thread.Sleep(1000);
                            return true;
                        }
                    }

                    // set channel states
                    for (int channel = 0; channel < 3; channel++)
                    {
                        if (_stopThread)
                        {
                            break;
                        }
                        ediabas.ArgString = string.Format("0x{0:X02};0x01;0x06", 0x11 + channel);
                        ediabas.ArgBinaryStd = null;
                        ediabas.ResultsRequests = "JOB_STATUS";

                        try
                        {
                            ediabas.ExecuteJob("STEUERN_DIGITALSIGNALE");
                        }
                        catch (Exception ex)
                        {
                            ediabasInitReq = true;
                            string exText = EdiabasNet.GetExceptionText(ex);
                            lock (CommThread.DataLock)
                            {
                                EdiabasResultDict = null;
                                EdiabasErrorMessage = exText;
                            }
                            Thread.Sleep(1000);
                            return true;
                        }
                    }
                }
            }

            lock (CommThread.DataLock)
            {
                EdiabasResultDict = resultDict;
                EdiabasErrorMessage = string.Empty;
            }
            Thread.Sleep(10);
            return true;
        }

        private bool CommErrorsEdiabas(SelectedDevice device)
        {
#pragma warning disable 219
            bool firstRequestCall = false;
#pragma warning restore 219
            if (ediabasInitReq)
            {
                firstRequestCall = true;
                ediabasJobAbort = false;

                ediabasInitReq = false;
            }

            List<EdiabasErrorReport> errorReportList = new List<EdiabasErrorReport>();

            foreach (EdiabasErrorRequest errorRequest in EdiabasErrorRequestList)
            {
                if (_stopThread || ediabasJobAbort)
                {
                    break;
                }

                try
                {
                    ediabas.ResolveSgbdFile(errorRequest.SgbdFile);
                }
                catch (Exception ex)
                {
                    string exText = EdiabasNet.GetExceptionText(ex);
                    errorReportList.Add(new EdiabasErrorReport(errorRequest.DeviceName, null, null, exText));
                    continue;
                }

                ediabas.ArgString = string.Empty;
                ediabas.ArgBinaryStd = null;
                ediabas.ResultsRequests = "JOB_STATUS;F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT";

                ediabas.TimeMeas = 0;
                try
                {
                    ediabas.ExecuteJob("FS_LESEN");

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = new List<Dictionary<string, EdiabasNet.ResultData>>(ediabas.ResultSets);

                    bool jobOk = false;
                    if (resultSets != null && resultSets.Count > 1)
                    {
                        EdiabasNet.ResultData resultData;
                        if (resultSets[resultSets.Count - 1].TryGetValue("JOB_STATUS", out resultData))
                        {
                            if (resultData.opData.GetType() == typeof(string))
                            {   // read details
                                string jobStatus = (string)resultData.opData;
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
                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                        {
                            if (dictIndex == 0)
                            {
                                dictIndex++;
                                continue;
                            }

                            EdiabasNet.ResultData resultData;
                            if (resultDict.TryGetValue("F_ORT_NR", out resultData))
                            {
                                if (resultData.opData.GetType() == typeof(Int64))
                                {   // read details
                                    ediabas.ArgString = string.Format("0x{0:X02}", (Int64)resultData.opData);
                                    ediabas.ArgBinaryStd = null;
                                    ediabas.ResultsRequests = "F_UW_KM";

                                    ediabas.ExecuteJob("FS_LESEN_DETAIL");

                                    List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail = new List<Dictionary<string, EdiabasNet.ResultData>>(ediabas.ResultSets);
                                    if (resultSetsDetail != null)
                                    {
                                        errorReportList.Add(new EdiabasErrorReport(errorRequest.DeviceName, resultDict,
                                            new List<Dictionary<string, EdiabasNet.ResultData>>(resultSetsDetail)));
                                    }
                                }
                            }
                            dictIndex++;
                        }
                    }
                    else
                    {
                        errorReportList.Add(new EdiabasErrorReport(errorRequest.DeviceName, null, null));
                    }
                }
                catch (Exception ex)
                {
                    ediabasInitReq = true;
                    string exText = EdiabasNet.GetExceptionText(ex);
                    errorReportList.Add(new EdiabasErrorReport(errorRequest.DeviceName, null, null, exText));
                    continue;
                }
                Thread.Sleep(10);
            }

            lock (CommThread.DataLock)
            {
                EdiabasErrorReportList = errorReportList;
            }
            Thread.Sleep(10);
            return true;
        }

        private bool CommEdiabas(SelectedDevice device, EdiabasJobs ediabasJobs)
        {
            bool firstRequestCall = false;
            if (ediabasInitReq)
            {
                firstRequestCall = true;
                ediabasJobAbort = false;

                try
                {
                    ediabas.ResolveSgbdFile(ediabasJobs.SgbdFile);
                }
                catch (Exception ex)
                {
                    string exText = EdiabasNet.GetExceptionText(ex);
                    lock (CommThread.DataLock)
                    {
                        EdiabasErrorMessage = exText;
                    }
                    Thread.Sleep(1000);
                    return false;
                }

                ediabasInitReq = false;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = null;

            foreach (EdiabasJob job in ediabasJobs.JobArray)
            {
                if (_stopThread)
                {
                    break;
                }
                string argString = job.JobArgs;
                switch (device)
                {
                    case SelectedDevice.DeviceMotor:
                        if (firstRequestCall)
                        {
                            argString = "JA;" + argString;
                        }
                        else
                        {
                            argString = "NEIN;" + argString;
                        }
                        break;
                }
                ediabas.ArgString = argString;
                ediabas.ArgBinaryStd = null;
                ediabas.ResultsRequests = job.ResultRequests;

                ediabas.TimeMeas = 0;
                try
                {
                    ediabas.ExecuteJob(job.JobName);

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        MergeResultDictionarys(ref resultDict, resultSets[1]);
                    }
                }
                catch (Exception ex)
                {
                    ediabasInitReq = true;
                    string exText = EdiabasNet.GetExceptionText(ex);
                    lock (CommThread.DataLock)
                    {
                        EdiabasResultDict = null;
                        EdiabasErrorMessage = exText;
                    }
                    Thread.Sleep(1000);
                    return true;
                }
            }

            lock (CommThread.DataLock)
            {
                EdiabasResultDict = resultDict;
                EdiabasErrorMessage = string.Empty;
            }
            Thread.Sleep(10);
            return true;
        }

        private bool CommTest()
        {
#pragma warning disable 219
            bool firstRequestCall = false;
#pragma warning restore 219
            if (ediabasInitReq)
            {
                firstRequestCall = true;
                ediabasJobAbort = false;
                ediabasSgbdFile = string.Empty;

                ediabasInitReq = false;
            }

            string resultText = string.Format("Update: {0}\r\n", ediabasUpdateCount++);
            ediabas.TimeMeas = 0;
            long timeDiff = 0;
            foreach (EdiabasTestJob ediabasJob in ErrorJobList)
            {
                if (_stopThread)
                {
                    break;
                }

                if (string.Compare(ediabasSgbdFile, ediabasJob.SgbdFile, StringComparison.Ordinal) != 0)
                {
                    try
                    {
                        ediabas.ResolveSgbdFile(ediabasJob.SgbdFile);
                    }
                    catch (Exception ex)
                    {
                        ediabasInitReq = true;
                        string exText = EdiabasNet.GetExceptionText(ex);
                        lock (CommThread.DataLock)
                        {
                            TestResult = exText;
                        }
                        Thread.Sleep(1000);
                        return false;
                    }
                    ediabasSgbdFile = ediabasJob.SgbdFile;
                }

                string argString = ediabasJob.JobArgs;

                if (string.Compare(ediabasJob.JobName, "STATUS_MESSWERTBLOCK_LESEN", StringComparison.Ordinal) == 0)
                {
                    if (firstRequestCall)
                    {
                        argString = "JA;" + argString;
                    }
                    else
                    {
                        argString = "NEIN;" + argString;
                    }
                }

                ediabas.ArgString = argString;
                ediabas.ArgBinaryStd = null;
                ediabas.ResultsRequests = ediabasJob.ResultRequests;

                try
                {
                    long startTime = Stopwatch.GetTimestamp();
                    ediabas.ExecuteJob(ediabasJob.JobName);
                    timeDiff += Stopwatch.GetTimestamp() - startTime;

                    string lineText = string.Empty;
                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = ediabas.ResultSets;
                    if ((resultSets == null) || (resultSets.Count < 2))
                    {
                        lineText += "-\r\n";
                    }
                    else
                    {
                        int dictIndex = 0;
                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                        {
                            if (dictIndex == 0)
                            {
                                dictIndex++;
                                continue;
                            }

                            string newLineText = string.Empty;
                            foreach (string dataName in ediabasJob.JobData)
                            {
                                EdiabasNet.ResultData resultData;
                                if (resultDict.TryGetValue(dataName, out resultData))
                                {
                                    string valueText = string.Empty;
                                    if (resultData.opData.GetType() == typeof(string))
                                    {
                                        valueText = (string)resultData.opData;
                                    }
                                    else if (resultData.opData.GetType() == typeof(Double))
                                    {
                                        valueText = ((Double)resultData.opData).ToString();
                                    }
                                    else if (resultData.opData.GetType() == typeof(Int64))
                                    {
                                        valueText = ((Int64)resultData.opData).ToString();
                                    }
                                    else if (resultData.opData.GetType() == typeof(byte[]))
                                    {
                                        byte[] dataArray = (byte[])resultData.opData;
                                        foreach (byte value in dataArray)
                                        {
                                            valueText += string.Format("{0:X02} ", value);
                                        }
                                    }
                                    newLineText += dataName + ": " + valueText + " ";
                                }
                            }
                            if (newLineText.Length > 0)
                            {
                                newLineText += "\r\n";
                            }
                            lineText += newLineText;
                            dictIndex++;
                        }
                    }
                    resultText += lineText;
                }
                catch (Exception ex)
                {
                    ediabasInitReq = true;
                    string exText = EdiabasNet.GetExceptionText(ex);
                    lock (CommThread.DataLock)
                    {
                        TestResult = exText;
                    }
                    Thread.Sleep(1000);
                    return true;
                }
            }
            resultText += string.Format("Zeit: {0}ms, Intern: {1}ms", timeDiff / (Stopwatch.Frequency / 1000), ediabas.TimeMeas / (Stopwatch.Frequency / 1000));

            lock (CommThread.DataLock)
            {
                TestResult = resultText;
            }
            Thread.Sleep(20);
            return true;
        }

        private bool Connect()
        {
            if (_comPort.StartsWith("COM"))
            {
                if (_serialPort.IsOpen) return true;
                try
                {
                    _serialPort.PortName = _comPort;
                    _serialPort.BaudRate = 115200;
                    _serialPort.DataBits = 8;
                    _serialPort.Parity = Parity.None;
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.Handshake = Handshake.None;
                    _serialPort.ReadTimeout = 0;
                    _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(ErrorReceived);
                    _serialPort.Open();
                }
                catch (Exception)
                {
                    Disconnect();
                    return false;
                }
            }
            else
            {
                if (_handleFtdi != (IntPtr) 0) return true;
                try
                {
                    string usbIndexStr = _comPort.Remove(0, 3);
                    uint usbIndex = Convert.ToUInt32(usbIndexStr);
                    Ftd2xx.FT_STATUS ftStatus;

                    ftStatus = Ftd2xx.FT_Open(usbIndex, out _handleFtdi);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }

                    ftStatus = Ftd2xx.FT_SetLatencyTimer(_handleFtdi, 2);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }

                    ftStatus = Ftd2xx.FT_SetBaudRate(_handleFtdi, Ftd2xx.FT_BAUD_115200);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    _baudRateFtdi = (int)Ftd2xx.FT_BAUD_115200;

                    ftStatus = Ftd2xx.FT_SetDataCharacteristics(_handleFtdi, Ftd2xx.FT_BITS_8, Ftd2xx.FT_STOP_BITS_1, Ftd2xx.FT_PARITY_NONE);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    _parityFtdi = Parity.None;

                    ftStatus = Ftd2xx.FT_SetTimeouts(_handleFtdi, 0, _writeTimeout);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }

                    ftStatus = Ftd2xx.FT_SetFlowControl(_handleFtdi, Ftd2xx.FT_FLOW_NONE, 0, 0);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }

                    ftStatus = Ftd2xx.FT_Purge(_handleFtdi, Ftd2xx.FT_PURGE_TX | Ftd2xx.FT_PURGE_RX);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    Disconnect();
                    return false;
                }
            }
            return true;
        }

        private static void MergeResultDictionarys(ref Dictionary<string, EdiabasNet.ResultData> resultDict, Dictionary<string, EdiabasNet.ResultData> mergeDict)
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

        private static void MergeResultDictionarys(ref Dictionary<string, EdiabasNet.ResultData> resultDict, Dictionary<string, EdiabasNet.ResultData> mergeDict, string prefix)
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

        private bool Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Close();
                }
                catch (Exception)
                {
                }
                _serialPort.ErrorReceived -= new SerialErrorReceivedEventHandler(ErrorReceived);
            }
            if (_handleFtdi != (IntPtr) 0)
            {
                Ftd2xx.FT_Close(_handleFtdi);
                _handleFtdi = (IntPtr) 0;
            }
            _baudRateFtdi = 0;
            _parityFtdi = Parity.None;
            return true;
        }

        private bool AbortEdiabasJob()
        {
            if (ediabasJobAbort || _stopThread)
            {
                return true;
            }
            return false;
        }

        private bool InterfaceConnect()
        {
            return true;
        }

        private bool InterfaceDisconnect()
        {
            return true;
        }

        private bool InterfaceSetConfig(int baudRate, Parity parity)
        {
            if (_handleFtdi == (IntPtr)0)
            {   // com port
                if (_serialPort.BaudRate != baudRate)
                {
                    _serialPort.BaudRate = baudRate;
                }
                if (_serialPort.Parity != parity)
                {
                    _serialPort.Parity = parity;
                }
            }
            else
            {
                if (_baudRateFtdi != baudRate)
                {
                    Ftd2xx.FT_STATUS ftStatus = Ftd2xx.FT_SetBaudRate(_handleFtdi, (uint)baudRate);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    _baudRateFtdi = baudRate;
                }

                if (_parityFtdi != parity)
                {
                    byte parityLocal;

                    switch (parity)
                    {
                        case Parity.None:
                            parityLocal = Ftd2xx.FT_PARITY_NONE;
                            break;

                        case Parity.Even:
                            parityLocal = Ftd2xx.FT_PARITY_EVEN;
                            break;

                        case Parity.Odd:
                            parityLocal = Ftd2xx.FT_PARITY_ODD;
                            break;

                        case Parity.Mark:
                            parityLocal = Ftd2xx.FT_PARITY_MARK;
                            break;

                        case Parity.Space:
                            parityLocal = Ftd2xx.FT_PARITY_SPACE;
                            break;

                        default:
                            return false;
                    }

                    Ftd2xx.FT_STATUS ftStatus = Ftd2xx.FT_SetDataCharacteristics(_handleFtdi, Ftd2xx.FT_BITS_8, Ftd2xx.FT_STOP_BITS_1, parityLocal);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }
                    _parityFtdi = parity;
                }

            }
            return true;
        }

        private void InitProperties()
        {
            InitProperties(false);
        }

        private void InitProperties(bool deviceChange)
        {
            AxisOpMode = OperationMode.OpModeStatus;
            if (!deviceChange)
            {
                Device = SelectedDevice.DeviceAxis;
                Connected = false;
                ErrorCounter = 0;
            }

            EdiabasResultDict = null;
            EdiabasErrorReportList = null;
            EdiabasErrorMessage = string.Empty;
            TestResult = string.Empty;

            ediabasInitReq = true;
            ediabasJobAbort = deviceChange;
            ediabasSgbdFile = string.Empty;
            ediabasUpdateCount = 0;
            ediabasInternalStep = 0;
            ediabasTempDict = null;
            ediabasDynDict = null;
        }

        private bool SendData(byte[] sendData, int length)
        {
            LogData(sendData, 0, length, "Send");
            if (_handleFtdi == (IntPtr)0)
            {   // com port
                _serialPort.DiscardInBuffer();
                _serialPort.Write(sendData, 0, length);
                while (_serialPort.BytesToWrite > 0)
                {
                    Thread.Sleep(10);
                }
            }
            else
            {   // ftdi
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesWritten = 0;

                ftStatus = Ftd2xx.FT_Purge(_handleFtdi, Ftd2xx.FT_PURGE_RX);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    throw new IOException();
                }
#if false
                ftStatus = Ftd2xx.FT_WriteWrapper(_handleFtdi, sendData, length, 0, out bytesWritten);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    throw new IOException();
                }
#else
                const int sendBlockSize = 4;
                for (int i = 0; i < length; i += sendBlockSize)
                {
                    int sendLength = length - i;
                    if (sendLength > sendBlockSize) sendLength = sendBlockSize;
                    ftStatus = Ftd2xx.FT_WriteWrapper(_handleFtdi, sendData, sendLength, i, out bytesWritten);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        throw new IOException();
                    }
                }
#endif
            }

            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, bool logResponse)
        {
            if (timeout < _readTimeoutMin)
            {
                timeout = _readTimeoutMin;
            }
            if (timeoutTelEnd < _readTimeoutMin)
            {
                timeoutTelEnd = _readTimeoutMin;
            }
            if (_handleFtdi == (IntPtr)0)
            {   // com port
                try
                {
                    // wait for first byte
                    int lastBytesToRead = 0;
                    commStopWatch.Reset();
                    commStopWatch.Start();
                    for (; ; )
                    {
                        lastBytesToRead = _serialPort.BytesToRead;
                        if (lastBytesToRead > 0)
                        {
                            break;
                        }
                        if (commStopWatch.ElapsedMilliseconds > timeout)
                        {
                            commStopWatch.Stop();
                            LogString(string.Format("T*** {0}ms", timeout));
                            return false;
                        }
                        Thread.Sleep(10);
                    }

                    int recLen = 0;
                    commStopWatch.Reset();
                    commStopWatch.Start();
                    for (; ; )
                    {
                        int bytesToRead = _serialPort.BytesToRead;
                        if (bytesToRead >= length)
                        {
                            recLen += _serialPort.Read(receiveData, offset + recLen, length - recLen);
                        }
                        if (recLen >= length)
                        {
                            break;
                        }
                        if (lastBytesToRead != bytesToRead)
                        {   // bytes received
                            commStopWatch.Reset();
                            commStopWatch.Start();
                            lastBytesToRead = bytesToRead;
                        }
                        else
                        {
                            if (commStopWatch.ElapsedMilliseconds > timeoutTelEnd)
                            {
                                LogString(string.Format("Len {0} < {1} ({2}ms)", bytesToRead, length, timeoutTelEnd));
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }
                    commStopWatch.Stop();
                    if (logResponse)
                    {
                        ediabas.LogData(receiveData, offset, recLen, "Rec ");
                    }
                    LogData(receiveData, offset, recLen, "Rec ");
                    if (recLen < length)
                    {
                        LogString("L***");
                        return false;
                    }
                }
                catch (Exception)
                {
                    LogString("E***");
                    return false;
                }
            }
            else
            {   // ftdi
#if true
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesRead = 0;

                ftStatus = Ftd2xx.FT_SetTimeouts(_handleFtdi, (uint)timeout, _writeTimeout);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    throw new IOException();
                }
                ftStatus = Ftd2xx.FT_ReadWrapper(_handleFtdi, receiveData, length, offset, out bytesRead);
                if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                {
                    throw new IOException();
                }
                if (logResponse)
                {
                    ediabas.LogData(receiveData, offset, (int)bytesRead, "Rec ");
                }
                LogData(receiveData, offset, (int)bytesRead, "Rec ");
                if (bytesRead < length)
                {
                    LogString("L***");
                    return false;
                }
#else
                try
                {
                    Ftd2xx.FT_STATUS ftStatus;

                    // wait for first byte
                    uint lastBytesToRead = 0;
                    commStopWatch.Reset();
                    commStopWatch.Start();
                    for (; ; )
                    {
                        ftStatus = Ftd2xx.FT_GetQueueStatus(_handleFtdi, out lastBytesToRead);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            throw new IOException();
                        }
                        if (lastBytesToRead > 0)
                        {
                            break;
                        }
                        if (commStopWatch.ElapsedMilliseconds > timeout)
                        {
                            commStopWatch.Stop();
                            LogString(string.Format("T*** {0}ms {1}", timeout));
                            return false;
                        }
                        Thread.Sleep(10);
                    }

                    int recLen = 0;
                    commStopWatch.Reset();
                    commStopWatch.Start();
                    for (; ; )
                    {
                        uint bytesToRead;
                        ftStatus = Ftd2xx.FT_GetQueueStatus(_handleFtdi, out bytesToRead);
                        if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                        {
                            throw new IOException();
                        }
                        if (bytesToRead >= length)
                        {
                            uint bytesRead;
                            ftStatus = Ftd2xx.FT_ReadWrapper(_handleFtdi, receiveData, length - recLen, offset + recLen, out bytesRead);
                            if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                            {
                                throw new IOException();
                            }
                            recLen += (int)bytesRead;
                        }
                        if (recLen >= length)
                        {
                            break;
                        }
                        if (lastBytesToRead != bytesToRead)
                        {   // bytes received
                            commStopWatch.Reset();
                            commStopWatch.Start();
                            lastBytesToRead = bytesToRead;
                        }
                        else
                        {
                            if (commStopWatch.ElapsedMilliseconds > timeoutTelEnd)
                            {
                                LogString(string.Format("Len {0} < {1} ({2}ms)", bytesToRead, length, timeoutTelEnd));
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }
                    commStopWatch.Stop();
                    if (logResponse)
                    {
                        ediabas.LogData(receiveData, offset, recLen, "Rec ");
                    }
                    LogData(receiveData, offset, recLen, "Rec ");
                    if (recLen < length)
                    {
                        LogString("L***");
                        return false;
                    }
                }
                catch (Exception)
                {
                    LogString("E***");
                    return false;
                }
#endif
            }
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd)
        {
            return ReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, false);
        }

        private void DataUpdatedEvent()
        {
            if (DataUpdated != null) DataUpdated(this, EventArgs.Empty);
        }

        private void ErrorReceived(Object sender, SerialErrorReceivedEventArgs e)
        {
#if DEBUG
            Trace.WriteLine("Error received");
#endif
        }

        private void LogString(string info)
        {
            if (_swLog == null) return;
            try
            {
                LogTimeStamp();
                _swLog.WriteLine(" " + info);
            }
            catch (Exception)
            {
            }
        }

        private void LogTimeStamp()
        {
            if (_swLog == null) return;

            if (!_logTimeWatch.IsRunning)
            {
                _logTimeWatch.Reset();
                _logTimeWatch.Start();
            }
            long elapsed = _logTimeWatch.ElapsedMilliseconds;
            _logTimeWatch.Reset();
            _logTimeWatch.Start();
            try
            {
                _swLog.Write(string.Format("{0:D04}", elapsed));
            }
            catch (Exception)
            {
            }
        }

        private void LogData(byte[] data, int offset, int length, string info)
        {
            if (_swLog == null) return;
            string logString = "";

            for (int i = 0; i < length; i++)
            {
                logString += string.Format("{0:X02} ", data[offset + i]);
            }
            LogTimeStamp();
            try
            {
                _swLog.WriteLine(" (" + info + "): " + logString);
            }
            catch (Exception)
            {
            }
        }

        private int BcdToInt(byte bcdValue)
        {
            int result = 0;
            int lowPart = bcdValue & 0x0F;
            if (lowPart > 9) lowPart = 9;
            int highPart = (bcdValue >> 4) & 0x0F;
            if (highPart > 9) highPart = 9;
            result = lowPart + 10 * highPart;
            return result;
        }
    }
}
