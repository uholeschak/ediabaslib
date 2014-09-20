using System;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Ftdi;
using EdiabasLib;

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

        public struct ErrorEntry
        {
            public int errorCode;
            public int errorType;
            public int errorCount;
            public int errorDistance1;
            public int errorDistance2;
        }

        public struct ErrorList
        {
            public int errorCount;
            public ErrorEntry[] errorList;
        }

        public const int MaxErrorCount = 10;

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
        public Dictionary<string, Ediabas.ResultData> EdiabasResultDict
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

        public bool ErrorsValid
        {
            get;
            private set;
        }
        public ErrorList[] ErrorDetails
        {
            get;
            private set;
        }
        public string TestResult
        {
            get;
            private set;
        }

        private class OBDData
        {
            public byte address;
            public byte length;
            public byte[] data;

            public OBDData()
            {
                address = 0;
                length = 0;
                data = new byte[256];
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
            private string sgdbFile;
            private EdiabasJob[] jobArray;

            public EdiabasJobs(string sgdbFile, EdiabasJob[] jobArray)
            {
                this.sgdbFile = sgdbFile;
                this.jobArray = jobArray;
            }

            public string SgdbFile
            {
                get
                {
                    return sgdbFile;
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
            private string sgdbFile;
            private string jobName;
            private string jobArgs;
            private string resultRequests;
            private string[] jobData;

            public EdiabasTestJob(string sgdbFile, string jobName, string jobArgs, string resultRequests, string[] jobData)
            {
                this.sgdbFile = sgdbFile;
                this.jobName = jobName;
                this.jobArgs = jobArgs;
                this.resultRequests = resultRequests;
                this.jobData = jobData;
            }

            public string SgdbFile
            {
                get
                {
                    return sgdbFile;
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

        private const byte _localAddr = 0xF1;       // local device address
        private const int _timeoutStd = 1200;       // default read timeout [ms]
        private const int _timeoutNR = 5000;        // neg response read timeout [ms]
        private const int _retryNR = 2;             // number of neg response retries
        private const int _writeTimeout = 500;      // write timeout [ms]

        private class EdiabasErrorRequest
        {
            private string deviceName;
            private string sgdbFile;

            public EdiabasErrorRequest(string deviceName, string sgdbFile)
            {
                this.deviceName = deviceName;
                this.sgdbFile = sgdbFile;
            }

            public string DeviceName
            {
                get { return deviceName; }
            }
            public string SgdbFile
            {
                get { return sgdbFile; }
            }
        }

        public class EdiabasErrorReport
        {
            private string deviceName;
            private Dictionary<string, Ediabas.ResultData> errorDict;
            private List<Dictionary<string, Ediabas.ResultData>> errorDetailSet;
            private string execptionText;

            public EdiabasErrorReport(string deviceName, Dictionary<string, Ediabas.ResultData> errorDict, List<Dictionary<string, Ediabas.ResultData>> errorDetailSet) :
                this(deviceName, errorDict, errorDetailSet, string.Empty)
            {
            }

            public EdiabasErrorReport(string deviceName, Dictionary<string, Ediabas.ResultData> errorDict, List<Dictionary<string, Ediabas.ResultData>> errorDetailSet, string execptionText)
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

            public Dictionary<string, Ediabas.ResultData> ErrorDict
            {
                get
                {
                    return errorDict;
                }
            }

            public List<Dictionary<string, Ediabas.ResultData>> ErrorDetailSet
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

        public struct DeviceEntry
        {
            private byte deviceAddress;
            private string deviceName;
            private string errorXml;

            public DeviceEntry(byte address, string name, string xml)
            {
                deviceAddress = address;
                deviceName = name;
                errorXml = xml;
            }

            public byte Address
            {
                get { return deviceAddress; }
            }
            public string Name
            {
                get { return deviceName; }
            }
            public string Xml
            {
                get { return errorXml; }
            }
        }

        static private readonly EdiabasErrorRequest[] EdiabasErrorRequestList =
        {
            new EdiabasErrorRequest("errorNameCAS", "d_cas.grp"),
            new EdiabasErrorRequest("errorNameDDE", "d_motor.grp"),
            new EdiabasErrorRequest("errorNameEKPS", "d_ekp.grp"),
            new EdiabasErrorRequest("errorNameEHC", "d_ehc.grp"),
            new EdiabasErrorRequest("errorNameDSC", "d_dsc.grp"),
            new EdiabasErrorRequest("errorNameACSM", "d_sim.grp"),
            new EdiabasErrorRequest("errorNameAHM", "d_ahm.grp"),
            new EdiabasErrorRequest("errorNameCCCBO", "d_mmi.grp"),
            new EdiabasErrorRequest("errorNameCCCGW", "d_mostgw.grp"),
            new EdiabasErrorRequest("errorNameCCCA", "d_ccc.grp"),
            new EdiabasErrorRequest("errorNameCCCANT", "d_anttu.grp"),
            new EdiabasErrorRequest("errorNameCCCASK", "d_ask.grp"),
            new EdiabasErrorRequest("errorNameCDC", "d_cdc.grp"),
            new EdiabasErrorRequest("errorNameCID", "d_cid.grp"),
            new EdiabasErrorRequest("errorNameCON", "d_ec.grp"),
            new EdiabasErrorRequest("errorNameIHK", "d_klima.grp"),
            new EdiabasErrorRequest("errorNameKBM", "d_kbm.grp"),
            new EdiabasErrorRequest("errorNameKGM", "d_zgm.grp"),
            new EdiabasErrorRequest("errorNameKOMBI", "d_kombi.grp"),
            new EdiabasErrorRequest("errorNameLM", "d_lm.grp"),
            new EdiabasErrorRequest("errorNamePDC", "d_pdc.grp"),
            new EdiabasErrorRequest("errorNameRLS", "rlss70.prg" /*"d_rls.grp"*/),
            new EdiabasErrorRequest("errorNameSZL", "d_szl.grp"),
            new EdiabasErrorRequest("errorNameSZM", "d_bzm.grp"),
            new EdiabasErrorRequest("errorNameTCU", "d_tel.grp"),
        };

        static public readonly DeviceEntry[] ErrorDeviceList = {
            new DeviceEntry(0x40, "errorNameCAS", "CAS.TXT"),
            new DeviceEntry(0x12, "errorNameDDE", "D60M47A0.TXT"),
            new DeviceEntry(0x17, "errorNameEKPS", "EKPM60_3.TXT"),
            new DeviceEntry(0x38, "errorNameEHC", "EHC_E65.TXT"),
            new DeviceEntry(0x29, "errorNameDSC", "DXC8_P.TXT"),
            new DeviceEntry(0x01, "errorNameACSM", "ACSM60.TXT"),
            new DeviceEntry(0x71, "errorNameAHM", "AHM_E65.TXT"),
            new DeviceEntry(0x63, "errorNameCCCBO", "CCC_60.TXT"),
            new DeviceEntry(0x62, "errorNameCCCGW", "CCCG60.TXT"),
            new DeviceEntry(0xA0, "errorNameCCCA", "CCCA60.TXT"),
            new DeviceEntry(0x47, "errorNameCCCANT", "ANT_60.TXT"),
            new DeviceEntry(0x3F, "errorNameCCCASK", "ASK_60.TXT"),
            new DeviceEntry(0x3C, "errorNameCDC", "CDC_E65.TXT"),
            new DeviceEntry(0x73, "errorNameCID", "CID_90.TXT"),
            new DeviceEntry(0x67, "errorNameCON", "ECL60.TXT"),
            new DeviceEntry(0x78, "errorNameIHK", "IHKA60_2.TXT"),
            new DeviceEntry(0x72, "errorNameKBM", "KBM_60.TXT"),
            new DeviceEntry(0x00, "errorNameKGM", "KGM_60.TXT"),
            new DeviceEntry(0x60, "errorNameKOMBI", "KOMB60.TXT"),
            new DeviceEntry(0x70, "errorNameLM", "LM_AHL_2.TXT"),
            new DeviceEntry(0x64, "errorNamePDC", "PDC_65_2.TXT"),
            new DeviceEntry(0x45, "errorNameRLS", "RLSS70.TXT"),
            new DeviceEntry(0x02, "errorNameSZL", "SCL_60.TXT"),
            new DeviceEntry(0x65, "errorNameSZM", "SZM_60.TXT"),
            new DeviceEntry(0x36, "errorNameTCU", "TELE60_3.TXT"),
            };

        static private readonly EdiabasJobs EhcJobs = new EdiabasJobs("d_ehc.grp",
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

        static private readonly EdiabasJobs MotorJobs = new EdiabasJobs("d_motor.grp",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_MESSWERTBLOCK_LESEN",
                    "IUBAT;ITKUM;CTSCD_tClntLin;ITKRS;ILMKG;ILMMG;SLMMG;ITUMG;ITKRS;IPLAD;SPLAD;ITLAL;IPUMG;IPRDR;SPRDR;ITAVO;ITAVP1;IPDIP;IDSLRE;IREAN;EGT_st;ISRBF;ISOED",
                    string.Empty),
            });

        static private readonly EdiabasJobs MotorUnevenJobs = new EdiabasJobs("d_motor.grp",
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

        static private readonly EdiabasJobs MotorRotIrregularJobs = new EdiabasJobs("d_motor.grp",
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

        static private readonly EdiabasJobs MotorPmJobs = new EdiabasJobs("d_motor.grp",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_SYSTEMCHECK_PM_INFO_2",
                    string.Empty,
                    "STAT_BATTERIE_KAPAZITAET_WERT;STAT_KALIBRIER_EVENT_CNT_WERT;STAT_LADUNGSZUSTAND_AKTUELL_WERT;STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT;STAT_Q_SOC_AKTUELL_WERT;STAT_Q_SOC_VOR_1_TAG_WERT;STAT_SOC_FIT_WERT;STAT_SOH_WERT;STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT;STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT;STAT_TEMP_SAISON_WERT"
                    ),
            });

        static private readonly EdiabasJobs CccNavJobs = new EdiabasJobs("d_ccc.grp",
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

        static private readonly EdiabasJobs IhkJobs = new EdiabasJobs("d_klima.grp",
            new EdiabasJob[]
            {
                new EdiabasJob("STATUS_REGLERGROESSEN",
                    string.Empty,
                    "STAT_TINNEN_WERT;STAT_TINNEN_VERZOEGERT_WERT;STAT_TAUSSEN_WERT;STAT_SOLL_LI_KORRIGIERT_WERT;STAT_WT_RE_WERT;STAT_WTSOLL_RE_WERT"
                    ),
            });

        // for tests
        static private readonly EdiabasTestJob[] MotorJobList = {
            new EdiabasTestJob("d_motor.grp", "STATUS_MESSWERTBLOCK_LESEN",
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
            new EdiabasTestJob("d_motor.grp", "STATUS_SYSTEMCHECK_PM_INFO_2",
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
            new EdiabasTestJob("d_motor.grp", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_motor.grp", "FS_LESEN_DETAIL", "0x4232", "F_UW_KM",
                new string[] {
                    "F_UW_KM",
                }),
            new EdiabasTestJob("d_ccc.grp", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_ehc.grp", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
                new string[] {
                    "F_ORT_TEXT",
                    "F_READY_TEXT",
                    "F_SYMPTOM_TEXT",
                    "F_VORHANDEN_TEXT",
                    "F_WARNUNG_TEXT",
                }),
            new EdiabasTestJob("d_ehc.grp", "FS_LESEN_DETAIL", "0x5FB4", "F_UW_KM",
                new string[] {
                    "F_UW_KM",
                }),
            new EdiabasTestJob("d_klima.grp", "FS_LESEN", string.Empty, "F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT",
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
        private byte[] _sendData;
        private byte[] _receiveData;
        private OBDData _sendObdData;
        private OBDData _receiveObdData;
        private StreamWriter _swLog;
        private Stopwatch _logTimeWatch;
        private Ediabas ediabas;
        private bool ediabasInitReq;
        private bool ediabasJobAbort;
        private string ediabasSgdbFile;
        private int ediabasUpdateCount;
        private int ediabasInternalStep;
        private Dictionary<string, Ediabas.ResultData> ediabasTempDict;
        private Dictionary<string, Ediabas.ResultData> ediabasDynDict;

        public CommThread()
        {
            _stopThread = false;
            _threadRunning = false;
            _workerThread = null;
            _serialPort = new SerialPort();
            _handleFtdi = (IntPtr) 0;
            _sendData = new byte[261];
            _receiveData = new byte[261];
            _sendObdData = new OBDData();
            _receiveObdData = new OBDData();
            _swLog = null;
            _logTimeWatch = new Stopwatch();
            ediabas = new Ediabas();

            EdCommBmwFast edCommBwmFast = new EdCommBmwFast(ediabas);
            ediabas.EdCommClass = edCommBwmFast;
            edCommBwmFast.InterfaceConnectFunc = InterfaceConnect;
            edCommBwmFast.InterfaceDisconnectFunc = InterfaceDisconnect;
            edCommBwmFast.TransmitDataFunc = OBDTrans;

            ediabas.ConfigDict.Add("SIMULATION", "0");
            ediabas.AbortJobFunc = AbortEdiabasJob;
            ediabas.FileSearchDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "Ecu");
            if (ediabas.FileSearchDir.StartsWith("\\Windows\\"))
            {
                ediabas.FileSearchDir = "\\UM\\Program Files\\CarControl\\Ecu";
            }

            // public properties
            ErrorDetails = new ErrorList[ErrorDeviceList.Length];
            for (int i = 0; i < ErrorDetails.Length; i++)
            {
                ErrorDetails[i] = new ErrorList();
                ErrorDetails[i].errorCount = 0;
                ErrorDetails[i].errorList = new ErrorEntry[MaxErrorCount];
            }
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
                    _swLog.Close();
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
                                //result = CommErrors();
                                result = CommErrorsEdiabas(copyDevice);
                                break;

                            case SelectedDevice.Test:
                                result = CommTest();
                                break;
                        }

                        if (!result)
                        {
#if false
                            {
                                // for debugging of connection errors
                                int i = 0;
                                Thread.Sleep(500);
                                _sendData[i++] = 0x84;
                                _sendData[i++] = 0x12;
                                _sendData[i++] = 0x34;
                                _sendData[i++] = 0x56;
                                _sendData[i++] = 0x78;
                                _sendData[i++] = 0x9A;
                                _sendData[i++] = 0xBC;
                                _sendData[i++] = 0xDE;
                                SendData(_sendData, i);
                                Thread.Sleep(1000);
                            }
#endif
                        }
                        else
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

        private bool CommErrors()
        {
            int i;
            bool result = true;

            for (int device = 0; device < ErrorDeviceList.Length; device++)
            {
                if (!_stopThread /*&& result*/)
                {
                    i = 0;
                    _sendObdData.address = ErrorDeviceList[device].Address;
                    _sendObdData.data[i++] = 0x18;
                    _sendObdData.data[i++] = 0x02;
                    _sendObdData.data[i++] = 0xFF;
                    _sendObdData.data[i++] = 0xFF;
                    _sendObdData.length = (byte)i;
                    if (!OBDTrans(_sendObdData, ref _receiveObdData))
                    {
                        ErrorDetails[device].errorCount = -1;
                        result = false;
                    }
                    else
                    {
                        if (_receiveObdData.length >= 2 &&
                            _receiveObdData.data[0] == 0x58)
                        {
                            lock (DataLock)
                            {
                                int errorCount = _receiveObdData.data[1];
                                if (errorCount > MaxErrorCount) errorCount = MaxErrorCount;
                                for (int j = 0; j < errorCount; j++)
                                {
                                    int errorCode = ((int)_receiveObdData.data[2 + (j * 3)] << 8) + _receiveObdData.data[3 + (j * 3)];
                                    ErrorDetails[device].errorList[j].errorCode = errorCode;
                                    ErrorDetails[device].errorList[j].errorType = _receiveObdData.data[4 + (j * 3)];
                                    ErrorDetails[device].errorList[j].errorCount = -1;
                                    ErrorDetails[device].errorList[j].errorDistance1 = -1;
                                    ErrorDetails[device].errorList[j].errorDistance2 = -1;
                                }
                                for (int j = 0; j < errorCount; j++)
                                {
                                    i = 0;
                                    _sendObdData.address = ErrorDeviceList[device].Address;
                                    _sendObdData.data[i++] = 0x17;
                                    _sendObdData.data[i++] = (byte)(ErrorDetails[device].errorList[j].errorCode >> 8);
                                    _sendObdData.data[i++] = (byte)ErrorDetails[device].errorList[j].errorCode;
                                    _sendObdData.length = (byte)i;
                                    if (!OBDTrans(_sendObdData, ref _receiveObdData))
                                    {
                                        result = false;
                                    }
                                    else
                                    {
                                        switch (ErrorDeviceList[device].Address)
                                        {
                                            case 0x12:   // DDE
                                                if (_receiveObdData.length < 34) break;
                                                ErrorDetails[device].errorList[j].errorCount = _receiveObdData.data[8];
                                                if ((_receiveObdData.data[5] & 0x02) != 0)
                                                {
                                                    ErrorDetails[device].errorList[j].errorDistance1 =
                                                        (((int)_receiveObdData.data[10] << 8) + _receiveObdData.data[11]) << 3;
                                                }
                                                if ((_receiveObdData.data[5] & 0x04) != 0)
                                                {
                                                    ErrorDetails[device].errorList[j].errorDistance2 =
                                                    (((int)_receiveObdData.data[22] << 8) + _receiveObdData.data[23]) << 3;
                                                }
                                                break;

                                            case 0x38:   // EHC
                                                if (_receiveObdData.length < 15) break;
                                                ErrorDetails[device].errorList[j].errorCount = _receiveObdData.data[5];
                                                ErrorDetails[device].errorList[j].errorDistance1 =
                                                    (((int)_receiveObdData.data[7] << 8) + _receiveObdData.data[8]) << 3;
                                                break;

                                            case 0x64:   // PDC
                                                if (_receiveObdData.length < 12) break;
                                                ErrorDetails[device].errorList[j].errorCount = _receiveObdData.data[5];
                                                ErrorDetails[device].errorList[j].errorDistance1 =
                                                    (((int)_receiveObdData.data[6] << 8) + _receiveObdData.data[7]) << 3;
                                                if (ErrorDetails[device].errorList[j].errorCount > 1)
                                                {
                                                    ErrorDetails[device].errorList[j].errorDistance2 =
                                                    (((int)_receiveObdData.data[9] << 8) + _receiveObdData.data[10]) << 3;
                                                }
                                                break;

                                            case 0x65:   // SZM
                                                if (_receiveObdData.length < 9) break;
                                                ErrorDetails[device].errorList[j].errorDistance1 =
                                                    (((int)_receiveObdData.data[5] << 8) + _receiveObdData.data[6]) << 3;
                                                ErrorDetails[device].errorList[j].errorDistance2 =
                                                    (((int)_receiveObdData.data[7] << 8) + _receiveObdData.data[8]) << 3;
                                                break;
                                        }
                                    }
                                }
                                ErrorDetails[device].errorCount = errorCount;
                            }
                        }
                        else
                        {
                            ErrorDetails[device].errorCount = -1;
                            result = false;
                        }
                    }
                }
            }
            ErrorsValid = true;
            return result;
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
                    ediabas.ResolveSgdbFile(ediabasJobs.SgdbFile);
                }
                catch (Exception ex)
                {
                    string exText = Ediabas.GetExceptionText(ex);
                    lock (CommThread.DataLock)
                    {
                        EdiabasErrorMessage = exText;
                    }
                    Thread.Sleep(1000);
                    return false;
                }

                ediabasInitReq = false;
            }

            Dictionary<string, Ediabas.ResultData> resultDict = null;

            if (firstRequestCall)
            {
                ediabasTempDict = null;
                ediabasDynDict = null;
                ediabasInternalStep = 0;
            }

            EdiabasJob job = ediabasJobs.JobArray[ediabasInternalStep];
            ediabas.ArgString = job.JobArgs;
            ediabas.ResultsRequests = job.ResultRequests;

            ediabas.TimeMeas = 0;
            try
            {
                ediabas.ExecuteJob(job.JobName);

                List<Dictionary<string, Ediabas.ResultData>> resultSets = ediabas.ResultSets;
                if (resultSets.Count >= 1)
                {
                    MergeResultDictionarys(ref ediabasTempDict, resultSets[0]);
                }
            }
            catch (Exception ex)
            {
                ediabasInitReq = true;
                string exText = Ediabas.GetExceptionText(ex);
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
            ediabas.ResultsRequests = "WERT";

            try
            {
                ediabas.ExecuteJob("MODE_CTRL_LESEN");

                List<Dictionary<string, Ediabas.ResultData>> resultSets = ediabas.ResultSets;
                if (resultSets.Count >= 1)
                {
                    Ediabas.ResultData resultData;
                    if (resultSets[0].TryGetValue("WERT", out resultData))
                    {
                        if (resultData.opData.GetType() == typeof(Int64))
                        {
                            axisMode = (int)((Int64)resultData.opData);
                        }
                    }
                    MergeResultDictionarys(ref resultDict, resultSets[0], "MODE_CTRL_LESEN_");
                }
            }
            catch (Exception ex)
            {
                ediabasInitReq = true;
                string exText = Ediabas.GetExceptionText(ex);
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
                    ediabas.ResultsRequests = "JOB_STATUS";

                    try
                    {
                        ediabas.ExecuteJob("ENERGIESPARMODE");
                    }
                    catch (Exception ex)
                    {
                        ediabasInitReq = true;
                        string exText = Ediabas.GetExceptionText(ex);
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
                    ediabas.ResultsRequests = "JOB_STATUS";

                    try
                    {
                        ediabas.ExecuteJob("ENERGIESPARMODE");
                    }
                    catch (Exception ex)
                    {
                        ediabasInitReq = true;
                        string exText = Ediabas.GetExceptionText(ex);
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
                        ediabas.ResultsRequests = "WERT";

                        try
                        {
                            ediabas.ExecuteJob("STATUS_SIGNALE_NUMERISCH");
                            List<Dictionary<string, Ediabas.ResultData>> resultSets = ediabas.ResultSets;
                            if (resultSets.Count >= 1)
                            {
                                MergeResultDictionarys(ref resultDict, resultSets[0], string.Format("STATUS_SIGNALE_NUMERISCH{0}_", channel));
                            }
                        }
                        catch (Exception ex)
                        {
                            ediabasInitReq = true;
                            string exText = Ediabas.GetExceptionText(ex);
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
                        ediabas.ResultsRequests = "JOB_STATUS";

                        try
                        {
                            ediabas.ExecuteJob("STEUERN_DIGITALSIGNALE");
                        }
                        catch (Exception ex)
                        {
                            ediabasInitReq = true;
                            string exText = Ediabas.GetExceptionText(ex);
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
                    ediabas.ResolveSgdbFile(errorRequest.SgdbFile);
                }
                catch (Exception ex)
                {
                    string exText = Ediabas.GetExceptionText(ex);
                    errorReportList.Add(new EdiabasErrorReport(errorRequest.DeviceName, null, null, exText));
                    continue;
                }

                ediabas.ArgString = string.Empty;
                ediabas.ResultsRequests = "JOB_STATUS;F_ORT_NR;F_ORT_TEXT;F_READY_TEXT;F_READY_NR;F_SYMPTOM_NR;F_SYMPTOM_TEXT;F_VORHANDEN_NR;F_VORHANDEN_TEXT;F_WARNUNG_NR;F_WARNUNG_TEXT";

                ediabas.TimeMeas = 0;
                try
                {
                    ediabas.ExecuteJob("FS_LESEN");

                    List<Dictionary<string, Ediabas.ResultData>> resultSets = new List<Dictionary<string, Ediabas.ResultData>>(ediabas.ResultSets);

                    bool jobOk = false;
                    if (resultSets.Count > 0)
                    {
                        Ediabas.ResultData resultData;
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
                        foreach (Dictionary<string, Ediabas.ResultData> resultDict in resultSets)
                        {
                            Ediabas.ResultData resultData;
                            if (resultDict.TryGetValue("F_ORT_NR", out resultData))
                            {
                                if (resultData.opData.GetType() == typeof(Int64))
                                {   // read details
                                    ediabas.ArgString = string.Format("0x{0:X02}", (Int64)resultData.opData);
                                    ediabas.ResultsRequests = "F_UW_KM";

                                    ediabas.ExecuteJob("FS_LESEN_DETAIL");

                                    errorReportList.Add(new EdiabasErrorReport(errorRequest.DeviceName, resultDict,
                                        new List<Dictionary<string, Ediabas.ResultData>>(ediabas.ResultSets)));
                                }
                            }
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
                    string exText = Ediabas.GetExceptionText(ex);
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
                    ediabas.ResolveSgdbFile(ediabasJobs.SgdbFile);
                }
                catch (Exception ex)
                {
                    string exText = Ediabas.GetExceptionText(ex);
                    lock (CommThread.DataLock)
                    {
                        EdiabasErrorMessage = exText;
                    }
                    Thread.Sleep(1000);
                    return false;
                }

                ediabasInitReq = false;
            }

            Dictionary<string, Ediabas.ResultData> resultDict = null;

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
                ediabas.ResultsRequests = job.ResultRequests;

                ediabas.TimeMeas = 0;
                try
                {
                    ediabas.ExecuteJob(job.JobName);

                    List<Dictionary<string, Ediabas.ResultData>> resultSets = ediabas.ResultSets;
                    if (resultSets.Count >= 1)
                    {
                        MergeResultDictionarys(ref resultDict, resultSets[0]);
                    }
                }
                catch (Exception ex)
                {
                    ediabasInitReq = true;
                    string exText = Ediabas.GetExceptionText(ex);
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
                ediabasSgdbFile = string.Empty;

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

                if (string.Compare(ediabasSgdbFile, ediabasJob.SgdbFile, StringComparison.Ordinal) != 0)
                {
                    try
                    {
                        ediabas.ResolveSgdbFile(ediabasJob.SgdbFile);
                    }
                    catch (Exception ex)
                    {
                        ediabasInitReq = true;
                        string exText = Ediabas.GetExceptionText(ex);
                        lock (CommThread.DataLock)
                        {
                            TestResult = exText;
                        }
                        Thread.Sleep(1000);
                        return false;
                    }
                    ediabasSgdbFile = ediabasJob.SgdbFile;
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
                ediabas.ResultsRequests = ediabasJob.ResultRequests;

                try
                {
                    long startTime = Stopwatch.GetTimestamp();
                    ediabas.ExecuteJob(ediabasJob.JobName);
                    timeDiff += Stopwatch.GetTimestamp() - startTime;

                    string lineText = string.Empty;
                    List<Dictionary<string, Ediabas.ResultData>> resultSets = ediabas.ResultSets;
                    if (resultSets.Count < 1)
                    {
                        lineText += "-\r\n";
                    }
                    else
                    {
                        foreach (Dictionary<string, Ediabas.ResultData> resultDict in resultSets)
                        {
                            string newLineText = string.Empty;
                            foreach (string dataName in ediabasJob.JobData)
                            {
                                Ediabas.ResultData resultData;
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
                        }
                    }
                    resultText += lineText;
                }
                catch (Exception ex)
                {
                    ediabasInitReq = true;
                    string exText = Ediabas.GetExceptionText(ex);
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

                    ftStatus = Ftd2xx.FT_SetDataCharacteristics(_handleFtdi, Ftd2xx.FT_BITS_8, Ftd2xx.FT_STOP_BITS_1, Ftd2xx.FT_PARITY_NONE);
                    if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
                    {
                        return false;
                    }

                    ftStatus = Ftd2xx.FT_SetTimeouts(_handleFtdi, _timeoutStd, _writeTimeout);
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

        private static void MergeResultDictionarys(ref Dictionary<string, Ediabas.ResultData> resultDict, Dictionary<string, Ediabas.ResultData> mergeDict)
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

        private static void MergeResultDictionarys(ref Dictionary<string, Ediabas.ResultData> resultDict, Dictionary<string, Ediabas.ResultData> mergeDict, string prefix)
        {
            if (resultDict == null)
            {
                resultDict = new Dictionary<string,Ediabas.ResultData>();
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
            ErrorsValid = false;
            for (int j = 0; j < ErrorDetails.Length; j++)
            {
                ErrorDetails[j].errorCount = 0;
            }
            TestResult = string.Empty;

            ediabasInitReq = true;
            ediabasJobAbort = deviceChange;
            ediabasSgdbFile = string.Empty;
            ediabasUpdateCount = 0;
            ediabasInternalStep = 0;
            ediabasTempDict = null;
            ediabasDynDict = null;
        }

        private bool SendData(byte[] sendData, int length)
        {
            if (_handleFtdi == (IntPtr)0)
            {   // com port
                _serialPort.DiscardInBuffer();
                _serialPort.Write(sendData, 0, length);
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

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, bool logResponse)
        {
            if (_handleFtdi == (IntPtr)0)
            {   // com port
                try
                {
                    int recLen = 0;
                    for (int i = 0; i < timeout/10; i++)
                    {
                        if (_serialPort.BytesToRead >= length)
                        {
                            recLen = _serialPort.Read(receiveData, offset + recLen, length - recLen);
                        }
                        if (recLen >= length)
                        {
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    if (logResponse)
                    {
                        LogData(receiveData, recLen, "Rec ");
                    }
                    if (recLen < length)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {   // ftdi
                Ftd2xx.FT_STATUS ftStatus;
                uint bytesRead = 0;

                ftStatus = Ftd2xx.FT_SetTimeouts(_handleFtdi, (uint) timeout, _writeTimeout);
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
                    LogData(receiveData, (int) bytesRead, "Rec ");
                }
                if (bytesRead < length)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout)
        {
            return ReceiveData(receiveData, offset, length, timeout, false);
        }

        private bool OBDTrans(OBDData sendData, ref OBDData receiveData)
        {
            return OBDTrans(sendData, ref receiveData, _timeoutStd, _timeoutNR, 2);
        }

        private bool OBDTrans(OBDData sendData, ref OBDData receiveData, int timeoutStd, int timeoutNR, int retryNR)
        {
            int i = 0;

            if (sendData.length > 0x3F)
            {
                _sendData[i++] = 0x80;
            }
            else
            {
                _sendData[i++] = (byte)(sendData.length | 0x80);
            }
            _sendData[i++] = sendData.address;
            _sendData[i++] = _localAddr;
            if (sendData.length > 0x3F)
            {
                _sendData[i++] = sendData.length;
            }
            Array.Copy(sendData.data, 0, _sendData, i, sendData.length);

            if (!OBDTrans(_sendData, ref _receiveData, timeoutStd, timeoutNR, retryNR))
            {
                return false;
            }

            if ((_receiveData[0] & ~0x3F) != 0x80)
            {
                return false;
            }

            int recLength = _receiveData[0] & 0x3F;
            if (recLength == 0)
            {   // with length byte
                receiveData.length = _receiveData[3];
                i = 4;
            }
            else
            {
                receiveData.length = (byte)recLength;
                i = 3;
            }
            receiveData.address = _receiveData[2];
            Array.Copy(_receiveData, i, receiveData.data, 0, receiveData.length);

            return true;
        }

        private bool OBDTrans(byte[] sendData, ref byte[] receiveData, int timeoutStd, int timeoutNR, int retryNR)
        {
            int sendLength = sendData[0] & 0x3F;
            if (sendLength == 0)
            {   // with length byte
                sendLength = sendData[3] + 4;
            }
            else
            {
                sendLength += 3;
            }
            sendData[sendLength] = CalcChecksum(sendData, sendLength);
            sendLength++;
            LogData(sendData, sendLength, "Send");
            if (!SendData(sendData, sendLength))
            {
                LogString("*** Sending failed");
                return false;
            }
            // remove remote echo
            if (!ReceiveData(receiveData, 0, sendLength, timeoutStd))
            {
                LogString("*** No echo received");
#if DEBUG
                Trace.WriteLine("No Echo");
#endif
                ReceiveData(receiveData, 0, receiveData.Length, timeoutStd, true);
                return false;
            }
            LogData(receiveData, sendLength, "Echo");
            for (int i = 0; i < sendLength; i++)
            {
                if (receiveData[i] != sendData[i])
                {
                    LogString("*** Echo incorrect");
#if DEBUG
                    Trace.WriteLine("Echo incorrect");
#endif
                    ReceiveData(receiveData, 0, receiveData.Length, timeoutStd, true);
                    return false;
                }
            }

            int timeout = timeoutStd;
            for (int retry = 0; retry <= retryNR; retry++)
            {
                // header byte
                if (!ReceiveData(receiveData, 0, 4, timeout))
                {
                    LogString("*** No header received");
#if DEBUG
                    Trace.WriteLine("No Head");
#endif
                    return false;
                }
                if ((receiveData[0] & 0xC0) != 0x80)
                {
                    LogData(receiveData, 4, "Head");
                    LogString("*** Invalid header");
#if DEBUG
                    Trace.WriteLine("Bad Head");
#endif
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, true);
                    return false;
                }
                int recLength = receiveData[0] & 0x3F;
                if (recLength == 0)
                {   // with length byte
                    recLength = receiveData[3] + 4;
                }
                else
                {
                    recLength += 3;
                }
                if (!ReceiveData(receiveData, 4, recLength - 3, timeout))
                {
                    LogString("*** No tail received");
#if DEBUG
                    Trace.WriteLine("No Tail");
#endif
                    return false;
                }
                LogData(receiveData, recLength + 1, "Resp");
                if (CalcChecksum(receiveData, recLength) != receiveData[recLength])
                {
                    LogString("*** Checksum incorrect");
#if DEBUG
                    Trace.WriteLine("Bad Checksum");
#endif
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, true);
                    return false;
                }
                if ((receiveData[1] != sendData[2]) ||
                    (receiveData[2] != sendData[1]))
                {
                    LogString("*** Address incorrect");
#if DEBUG
                    Trace.WriteLine("Bad Address");
#endif
                    ReceiveData(receiveData, 0, receiveData.Length, timeout, true);
                    return false;
                }

                int dataLen = receiveData[0] & 0x3F;
                int dataStart = 3;
                if (dataLen == 0)
                {   // with length byte
                    dataLen = receiveData[3];
                    dataStart++;
                }
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    LogString("*** NR 0x78");
#if DEBUG
                    Trace.WriteLine("Neg response");
#endif
                    timeout = timeoutNR;
                }
                else
                {
                    break;
                }
            }
            return true;
        }

        private byte CalcChecksum(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
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

        private void LogData(byte[] data, int length, string info)
        {
            if (_swLog == null) return;
            string logString = "";

            for (int i = 0; i < length; i++)
            {
                logString += string.Format("{0:X02} ", data[i]);
            }
            LogTimeStamp();
            try
            {
                _swLog.Write(" (" + info + "): ");
                _swLog.WriteLine(logString);
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
