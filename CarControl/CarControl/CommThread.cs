using EdiabasLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CarControl
{
#if !WindowsCE
    public
#endif
    class CommThread : IDisposable
    {
        public delegate void DataUpdatedEventHandler(object sender, EventArgs e);
        public event DataUpdatedEventHandler DataUpdated;
        public delegate void ThreadTerminatedEventHandler(object sender, EventArgs e);
        public event ThreadTerminatedEventHandler ThreadTerminated;

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
            AdapterConfig,
            Test,
            Dynamic,
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

#if !WindowsCE
        public JobReader.PageInfo JobPageInfo
        {
            get;
            set;
        }
#endif

        public bool CommActive
        {
            get;
            set;
        }

        private int adapterConfigValue;
        public int AdapterConfigValue
        {
            get
            {
                return adapterConfigValue;
            }
            set
            {
                adapterConfigValue = value;
                EdiabasResultDict = null;
            }
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

        public EdiabasNet Ediabas
        {
            get
            {
                return ediabas;
            }
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
            new []
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
            new []
            {
                new EdiabasJob("STATUS_MESSWERTBLOCK_LESEN",
                    "IUBAT;ITKUM;CTSCD_tClntLin;ITKRS;ILMKG;ILMMG;SLMMG;ITUMG;ITKRS;IPLAD;SPLAD;ITLAL;IPUMG;IPRDR;SPRDR;ITAVO;ITAVP1;IPDIP;IDSLRE;IREAN;EGT_st;ISRBF;ISOED",
                    string.Empty),
            });

        static private readonly EdiabasJobs MotorUnevenJobs = new EdiabasJobs("d_motor",
            new []
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
            new []
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
            new []
            {
                new EdiabasJob("STATUS_SYSTEMCHECK_PM_INFO_2",
                    string.Empty,
                    "STAT_BATTERIE_KAPAZITAET_WERT;STAT_KALIBRIER_EVENT_CNT_WERT;STAT_LADUNGSZUSTAND_AKTUELL_WERT;STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT;STAT_Q_SOC_AKTUELL_WERT;STAT_Q_SOC_VOR_1_TAG_WERT;STAT_SOC_FIT_WERT;STAT_SOH_WERT;STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT;STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT;STAT_TEMP_SAISON_WERT"
                    ),
            });

        static private readonly EdiabasJobs CccNavJobs = new EdiabasJobs("d_ccc",
            new []
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
            new []
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
                new [] {
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
                new [] {
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
                new [] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_motor", "FS_LESEN_DETAIL", "0x4232", "F_UW_KM",
                new [] {
                    "F_UW_KM",
                }),
            new EdiabasTestJob("d_ccc", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new [] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_ehc", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new [] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_ehc", "FS_LESEN_DETAIL", "0x5FB4", "F_UW_KM",
                new [] {
                    "F_UW_KM",
                }),
            new EdiabasTestJob("d_klima", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new [] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            };

        static public readonly Object DataLock = new Object();
        protected static readonly long tickResolMs = Stopwatch.Frequency / 1000;

        private bool disposed = false;
        private volatile bool _stopThread;
        private bool _threadRunning;
        private Thread _workerThread;
        private Stopwatch commStopWatch;
        private EdiabasNet ediabas;
        private bool ediabasInitReq;
        private bool ediabasJobAbort;
        private string ediabasSgbdFile;
        private int ediabasUpdateCount;
        private int ediabasInternalStep;
        private Dictionary<string, EdiabasNet.ResultData> ediabasTempDict;
        private Dictionary<string, EdiabasNet.ResultData> ediabasDynDict;

        public CommThread(string ecuPath)
        {
            _stopThread = false;
            _threadRunning = false;
            _workerThread = null;
            commStopWatch = new Stopwatch();
            ediabas = new EdiabasNet();

#if true
            ediabas.EdInterfaceClass = new EdInterfaceObd();
#else
            ediabas.EdInterfaceClass = new EdInterfaceEnet();
#endif
            ediabas.AbortJobFunc = AbortEdiabasJob;
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
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    ediabas.Dispose();
                    ediabas = null;
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

#if WindowsCE
        public bool StartThread(string comPort, string logFile, SelectedDevice selectedDevice, bool commActive)
#else
        public bool StartThread(string comPort, string logFile, SelectedDevice selectedDevice, JobReader.PageInfo pageInfo, bool commActive)
#endif
        {
            if (_workerThread != null)
            {
                return false;
            }
            try
            {
                _stopThread = false;
                if (ediabas.EdInterfaceClass is EdInterfaceObd)
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = comPort;
                }
                if (ediabas.EdInterfaceClass is EdInterfaceEnet)
                {
                    //((EdInterfaceEnet)ediabas.EdInterfaceClass).RemoteHost = "192.168.10.244";
                }
                if (logFile != null)
                {
                    ediabas.SetConfigProperty("TracePath", Path.GetDirectoryName(logFile));
                    ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.ED_LOG_LEVEL.IFH));
                }
                else
                {
                    ediabas.SetConfigProperty("IfhTrace", "0");
                }
                InitProperties();
                CommActive = commActive;
                Device = selectedDevice;
#if !WindowsCE
                JobPageInfo = pageInfo;
#endif
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
            ediabas.SetConfigProperty("IfhTrace", "0");
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
            SelectedDevice lastDevice = (SelectedDevice)(-1);
#if !WindowsCE
            JobReader.PageInfo lastPageInfo = null;
#endif
            while (!_stopThread)
            {
                try
                {
                    if (!CommActive)
                    {
                        continue;
                    }
                    bool result = true;
                    SelectedDevice copyDevice = Device;
#if !WindowsCE
                    JobReader.PageInfo copyPageInfo = JobPageInfo;
#endif

                    if ((lastDevice != copyDevice)
#if !WindowsCE
                        || (lastPageInfo != copyPageInfo)
#endif
                        )
                    {
                        lastDevice = copyDevice;
#if !WindowsCE
                        lastPageInfo = copyPageInfo;
#endif
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

                        case SelectedDevice.AdapterConfig:
                            result = CommAdapterConfig(copyDevice);
                            break;

                        case SelectedDevice.Test:
                            result = CommTest();
                            break;

#if !WindowsCE
                        case SelectedDevice.Dynamic:
                            result = CommDynamic(copyDevice, copyPageInfo);
                            break;
#endif
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
            _threadRunning = false;
            DataUpdatedEvent();
            ThreadTerminatedEvent();
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

            Dictionary<string, EdiabasNet.ResultData> resultDict;

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
                        if (resultData.opData is Int64)
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
                        // longer timeout for bluetooth
                        ediabas.ArgString = string.Format("0x{0:X02};0x01;0x0C", 0x11 + channel);
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
                            if (resultData.opData is string)
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
                                if (resultData.opData is Int64)
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

        private bool CommAdapterConfig(SelectedDevice device)
        {
            if (AdapterConfigValue < 0)
            {
                Thread.Sleep (500);
                return true;
            }
            byte adapterConfig = (byte)AdapterConfigValue;
            AdapterConfigValue = -1;
            if (ediabasInitReq)
            {
                ediabasJobAbort = false;

                try
                {
                    ediabas.ResolveSgbdFile("adapter_prg");
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

            // send command
            ediabas.ArgString = string.Format("0x{0:X02}", adapterConfig);
            ediabas.ArgBinaryStd = null;
            ediabas.ResultsRequests = string.Empty;

            try
            {
                ediabas.ExecuteJob("ADAPTER_PRG");

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
                    EdiabasResultDict = resultDict;
                    EdiabasErrorMessage = exText;
                }
                Thread.Sleep(1000);
                return true;
            }

            lock (CommThread.DataLock)
            {
                EdiabasResultDict = resultDict;
                EdiabasErrorMessage = string.Empty;
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
                                    if (resultData.opData is string)
                                    {
                                        valueText = (string)resultData.opData;
                                    }
                                    else if (resultData.opData is Double)
                                    {
                                        valueText = ((Double)resultData.opData).ToString ();
                                    }
                                    else if (resultData.opData is Int64)
                                    {
                                        valueText = ((Int64)resultData.opData).ToString ();
                                    }
                                    else if (resultData.opData is byte[])
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

#if !WindowsCE
        private bool CommDynamic(SelectedDevice device, JobReader.PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                lock (CommThread.DataLock)
                {
                    EdiabasErrorMessage = "No Page info";
                }
                Thread.Sleep(1000);
                return false;
            }
            if (pageInfo.ClassObject == null)
            {
                lock (CommThread.DataLock)
                {
                    EdiabasErrorMessage = "No Class object";
                }
                Thread.Sleep(1000);
                return false;
            }
#pragma warning disable 219
            bool firstRequestCall = false;
#pragma warning restore 219
            if (ediabasInitReq)
            {
                firstRequestCall = true;
                ediabasJobAbort = false;

                try
                {
                    ediabas.ResolveSgbdFile(pageInfo.ClassObject.GetSgbdFileName());
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

            try
            {
                pageInfo.ClassObject.ExecuteJob(ediabas, ref resultDict, firstRequestCall);
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

            lock (CommThread.DataLock)
            {
                EdiabasResultDict = resultDict;
                EdiabasErrorMessage = string.Empty;
            }
            Thread.Sleep(10);
            return true;
        }
#endif

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
            if (ediabasJobAbort || _stopThread)
            {
                return true;
            }
            return false;
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
                AdapterConfigValue = -1;
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
