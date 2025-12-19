using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using PsdzClient.Programming;
using PsdzClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

#pragma warning disable CS0169
namespace PsdzClient.Core
{
    public class ECU : ICloneable, IEcu, INotifyPropertyChanged, IIdentEcu, IEcuObj
    {
        private string bntn;
        private int stillProgrammable;
        [PreserveSource(Hint = "private XEP_ECUCLIQUES", Placeholder = true)]
        private PlaceholderType xepEcuClique;
        private string eCUTitle;
        private string vARIANTEField;
        [PreserveSource(Hint = "private ObservableCollection<JOB>", Placeholder = true)]
        private PlaceholderType jOBSField;
        private BusType bUSField;
        private short? iD_BUS_INDEXField;
        private typeDiagProtocoll diagProtocollField;
        private int eCUTreeColumnField;
        private int eCUTreeRowField;
        private string tITLE_ECUTREEField;
        private string eCUTreeColorField;
        private string eCU_ADRField;
        private ObservableCollection<AIF> aIFField;
        private short? iD_LIEF_NRField;
        private string iD_LIEF_TEXTField;
        private short? iD_SW_NRField;
        private string iD_SW_NR_MCVField;
        private string iD_SW_NR_FSVField;
        private string iD_SW_NR_OSVField;
        private string iD_SW_NR_RESField;
        private short? iD_EWS_SSField;
        private string sERIENNUMMERField;
        private string iD_BMW_NRField;
        private string iD_HW_NRField;
        private short? iD_COD_INDEXField;
        private int? iD_DIAG_INDEXField;
        private int? iD_VAR_INDEXField;
        private int? iD_DATUM_JAHRField;
        private int? iD_DATUM_MONATField;
        private int? iD_DATUM_TAGField;
        private string iD_DATUMField;
        private short? iD_DATUM_KWField;
        private long? iD_SGBD_INDEXField;
        private long iD_SG_ADRField;
        private long? iD_LIN_SLAVE_ADRField;
        private int f_ANZField;
        [PreserveSource(Hint = "private ObservableCollection<DTC>", Placeholder = true)]
        private PlaceholderType fEHLERField;
        private int i_ANZField;
        [PreserveSource(Hint = "private ObservableCollection<DTC>", Placeholder = true)]
        private PlaceholderType iNFOField;
        private SVK sVKField;
        private string pHYSIKALISCHE_HW_NRField;
        private ObservableCollection<typeECU_Transaction> tALField;
        [PreserveSource(Hint = "private DTC", Placeholder = true)]
        private PlaceholderType selectedDTCField;
        [PreserveSource(Hint = "private DTC", Placeholder = true)]
        private PlaceholderType selectedINFOField;
        private string hARDWARE_REFERENZField;
        private int? hW_REF_STATUSField;
        private ObservableCollection<typeSWTStatus> sWTStatusField;
        private string dATEN_REFERENZField;
        private ObservableCollection<BusType> subBUSField;
        private string eCU_GROBNAMEField;
        private string eCU_NAMEField;
        private string eCU_SGBDField;
        private string eCU_GRUPPEField;
        private bool cOMMUNICATION_SUCCESSFULLYField;
        private bool iDENT_SUCCESSFULLYField;
        private bool aIF_SUCCESSFULLYField;
        private bool fS_SUCCESSFULLYField;
        private bool iS_SUCCESSFULLYField;
        private bool sERIAL_SUCCESSFULLYField;
        private bool sVK_SUCCESSFULLYField;
        private bool pHYSHW_SUCCESSFULLYField;
        private bool hWREF_SUCCESSFULLYField;
        private bool eCU_HAS_CONFIG_OVERRIDEField;
        private uint bUSIDField;
        private bool dATEN_REFERENZ_SUCCESSFULLYField;
        private int fLASH_STATEField;
        private bool eCU_ASSEMBLY_CONFIRMEDField;
        private IEcuStatusInfo statusInfo;
        public GenerationType Generation { get; set; }

        [PreserveSource(Hint = "IXepEcuVariants", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IIdentEcu.XepEcuVariant
        {
            get
            {
                return XepEcuVariant;
            }

            set
            {
            }
        }

        [PreserveSource(Hint = "IXepEcuCliques", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IIdentEcu.XepEcuClique
        {
            get
            {
                return XepEcuClique;
            }

            set
            {
            }
        }

        [XmlIgnore]
        IEnumerable<IAif> IEcu.AIF => AIF;

        [XmlIgnore]
        public string EcuUid { get; set; }

        [PreserveSource(Hint = "IEnumerable<IDtc>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IEcu.FEHLER => FEHLER;

        [PreserveSource(Hint = "IEnumerable<IDtc>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IEcu.INFO => INFO;

        [PreserveSource(Hint = "IEnumerable<IJob>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IEcu.JOBS => JOBS;

        [XmlIgnore]
        public string ProgrammingVariantName
        {
            get
            {
                return bntn;
            }

            set
            {
                if ((value == null && bntn != null) || (value != null && !value.Equals(bntn)))
                {
                    bntn = value;
                    OnPropertyChanged("ProgrammingVariantName");
                }
            }
        }

        [XmlIgnore]
        IEcuStatusInfo IEcu.StatusInfo
        {
            get
            {
                return StatusInfo;
            }

            set
            {
                StatusInfo = value;
            }
        }

        [XmlIgnore]
        public int StillProgrammable
        {
            get
            {
                return stillProgrammable;
            }

            set
            {
                this.PropertyChanged.NotifyPropertyChanged(this, () => StillProgrammable, ref stillProgrammable, value);
            }
        }

        [XmlIgnore]
        IEnumerable<BusType> IEcu.SubBUS => SubBUS;

        [XmlIgnore]
        ISvk IEcu.SVK => SVK;

        [XmlIgnore]
        IEnumerable<ISwtStatus> IEcu.SWTStatus => SWTStatus;

        [XmlIgnore]
        IEnumerable<IEcuTransaction> IEcu.TAL => TAL;

        [PreserveSource(Hint = "XEP_ECUCLIQUES", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType XepEcuClique { get; set; }

        public string ECUTitle
        {
            get
            {
                return eCUTitle;
            }

            set
            {
                if (eCUTitle != value)
                {
                    eCUTitle = value;
                    OnPropertyChanged("ECUTitle");
                }
            }
        }

        [PreserveSource(Hint = "XEP_ECUVARIANTS", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType XepEcuVariant { get; set; }

        [PreserveSource(Hint = "ILcSwitchList", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType LCSwitchList { get; set; }

        [XmlIgnore]
        public string EcuRep
        {
            get
            {
                return TITLE_ECUTREE;
            }

            set
            {
                TITLE_ECUTREE = value;
            }
        }

        [XmlIgnore]
        public string EcuGroup
        {
            get
            {
                return ECU_GRUPPE;
            }

            set
            {
                ECU_GRUPPE = value;
            }
        }

        [XmlIgnore]
        public string BaseVariant { get; set; }

        [XmlIgnore]
        public string BnTnName
        {
            get
            {
                return ProgrammingVariantName;
            }

            set
            {
                ProgrammingVariantName = value;
            }
        }

        [XmlIgnore]
        public IList<Bus> BusConnections
        {
            get
            {
                return GetBusConnections();
            }

            private set
            {
                throw new NotImplementedException();
            }
        }

        [XmlIgnore]
        public IList<IBusObject> BusCons { get; set; }

        [XmlIgnore]
        public IList<string> BusConnectionsAsString
        {
            get
            {
                return GetBusConnectionsAsString();
            }

            private set
            {
                throw new NotImplementedException();
            }
        }

        [XmlIgnore]
        public Bus DiagnosticBus
        {
            get
            {
                if (DiagBus != null)
                {
                    return DiagBus.ConvertToBus();
                }

                return Bus.Unknown;
            }

            private set
            {
                throw new NotImplementedException();
            }
        }

        [XmlIgnore]
        public IBusObject DiagBus { get; set; }

        [XmlIgnore]
        public IEcuDetailInfo EcuDetailInfo { get; set; }

        [XmlIgnore]
        public IEcuIdentifier EcuIdentifier { get; set; }

        [XmlIgnore]
        public IEcuStatusInfo EcuStatusInfo
        {
            get
            {
                return StatusInfo;
            }

            set
            {
                StatusInfo = value;
            }
        }

        [XmlIgnore]
        public string EcuVariant
        {
            get
            {
                return VARIANTE;
            }

            set
            {
                VARIANTE = value;
            }
        }

        [XmlIgnore]
        public int? GatewayDiagAddrAsInt { get; set; }

        [XmlIgnore]
        public string SerialNumber
        {
            get
            {
                return SERIENNUMMER;
            }

            set
            {
                SERIENNUMMER = value;
            }
        }

        [XmlIgnore]
        public IStandardSvk StandardSvk { get; set; }

        [XmlIgnore]
        public string OrderNumber { get; set; }

        [XmlIgnore]
        public IEcuPdxInfo EcuPdxInfo { get; set; }

        [XmlIgnore]
        public string EcuFullName { get; set; }

        [XmlIgnore]
        public bool IsSmartActuator { get; set; }

        [XmlIgnore]
        public IEcuStatusInfo StatusInfo
        {
            get
            {
                return statusInfo;
            }

            set
            {
                if (statusInfo != value)
                {
                    statusInfo = value;
                    OnPropertyChanged("StatusInfo");
                }
            }
        }

        public string VARIANTE
        {
            get
            {
                return vARIANTEField;
            }

            set
            {
                if (vARIANTEField != null)
                {
                    if (!vARIANTEField.Equals(value))
                    {
                        vARIANTEField = value;
                        OnPropertyChanged("VARIANTE");
                    }
                }
                else
                {
                    vARIANTEField = value;
                    OnPropertyChanged("VARIANTE");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<JOB>", Placeholder = true)]
        public PlaceholderType JOBS;
        public BusType BUS
        {
            get
            {
                return bUSField;
            }

            set
            {
                if (!bUSField.Equals(value))
                {
                    bUSField = value;
                    OnPropertyChanged("BUS");
                }
            }
        }

        public short? ID_BUS_INDEX
        {
            get
            {
                return iD_BUS_INDEXField;
            }

            set
            {
                if (iD_BUS_INDEXField.HasValue)
                {
                    if (!iD_BUS_INDEXField.Equals(value))
                    {
                        iD_BUS_INDEXField = value;
                        OnPropertyChanged("ID_BUS_INDEX");
                    }
                }
                else
                {
                    iD_BUS_INDEXField = value;
                    OnPropertyChanged("ID_BUS_INDEX");
                }
            }
        }

        public typeDiagProtocoll DiagProtocoll
        {
            get
            {
                return diagProtocollField;
            }

            set
            {
                if (!diagProtocollField.Equals(value))
                {
                    diagProtocollField = value;
                    OnPropertyChanged("DiagProtocoll");
                }
            }
        }

        public int ECUTreeColumn
        {
            get
            {
                return eCUTreeColumnField;
            }

            set
            {
                if (!eCUTreeColumnField.Equals(value))
                {
                    eCUTreeColumnField = value;
                    OnPropertyChanged("ECUTreeColumn");
                }
            }
        }

        public int ECUTreeRow
        {
            get
            {
                return eCUTreeRowField;
            }

            set
            {
                if (!eCUTreeRowField.Equals(value))
                {
                    eCUTreeRowField = value;
                    OnPropertyChanged("ECUTreeRow");
                }
            }
        }

        public string TITLE_ECUTREE
        {
            get
            {
                return tITLE_ECUTREEField;
            }

            set
            {
                if (tITLE_ECUTREEField != null)
                {
                    if (!tITLE_ECUTREEField.Equals(value))
                    {
                        tITLE_ECUTREEField = value;
                        OnPropertyChanged("TITLE_ECUTREE");
                    }
                }
                else
                {
                    tITLE_ECUTREEField = value;
                    OnPropertyChanged("TITLE_ECUTREE");
                }
            }
        }

        public string ECUTreeColor
        {
            get
            {
                return eCUTreeColorField;
            }

            set
            {
                if (eCUTreeColorField != null)
                {
                    if (!eCUTreeColorField.Equals(value))
                    {
                        eCUTreeColorField = value;
                        OnPropertyChanged("ECUTreeColor");
                    }
                }
                else
                {
                    eCUTreeColorField = value;
                    OnPropertyChanged("ECUTreeColor");
                }
            }
        }

        public string ECU_ADR
        {
            get
            {
                return eCU_ADRField;
            }

            set
            {
                if (eCU_ADRField != null)
                {
                    if (!eCU_ADRField.Equals(value))
                    {
                        eCU_ADRField = value;
                        OnPropertyChanged("ECU_ADR");
                    }
                }
                else
                {
                    eCU_ADRField = value;
                    OnPropertyChanged("ECU_ADR");
                }
            }
        }

        public ObservableCollection<AIF> AIF
        {
            get
            {
                return aIFField;
            }

            set
            {
                if (aIFField != null)
                {
                    if (!aIFField.Equals(value))
                    {
                        aIFField = value;
                        OnPropertyChanged("AIF");
                    }
                }
                else
                {
                    aIFField = value;
                    OnPropertyChanged("AIF");
                }
            }
        }

        public short? ID_LIEF_NR
        {
            get
            {
                return iD_LIEF_NRField;
            }

            set
            {
                if (iD_LIEF_NRField.HasValue)
                {
                    if (!iD_LIEF_NRField.Equals(value))
                    {
                        iD_LIEF_NRField = value;
                        OnPropertyChanged("ID_LIEF_NR");
                    }
                }
                else
                {
                    iD_LIEF_NRField = value;
                    OnPropertyChanged("ID_LIEF_NR");
                }
            }
        }

        public string ID_LIEF_TEXT
        {
            get
            {
                return iD_LIEF_TEXTField;
            }

            set
            {
                if (iD_LIEF_TEXTField != null)
                {
                    if (!iD_LIEF_TEXTField.Equals(value))
                    {
                        iD_LIEF_TEXTField = value;
                        OnPropertyChanged("ID_LIEF_TEXT");
                    }
                }
                else
                {
                    iD_LIEF_TEXTField = value;
                    OnPropertyChanged("ID_LIEF_TEXT");
                }
            }
        }

        public short? ID_SW_NR
        {
            get
            {
                return iD_SW_NRField;
            }

            set
            {
                if (iD_SW_NRField.HasValue)
                {
                    if (!iD_SW_NRField.Equals(value))
                    {
                        iD_SW_NRField = value;
                        OnPropertyChanged("ID_SW_NR");
                    }
                }
                else
                {
                    iD_SW_NRField = value;
                    OnPropertyChanged("ID_SW_NR");
                }
            }
        }

        public string ID_SW_NR_MCV
        {
            get
            {
                return iD_SW_NR_MCVField;
            }

            set
            {
                if (iD_SW_NR_MCVField != null)
                {
                    if (!iD_SW_NR_MCVField.Equals(value))
                    {
                        iD_SW_NR_MCVField = value;
                        OnPropertyChanged("ID_SW_NR_MCV");
                    }
                }
                else
                {
                    iD_SW_NR_MCVField = value;
                    OnPropertyChanged("ID_SW_NR_MCV");
                }
            }
        }

        public string ID_SW_NR_FSV
        {
            get
            {
                return iD_SW_NR_FSVField;
            }

            set
            {
                if (iD_SW_NR_FSVField != null)
                {
                    if (!iD_SW_NR_FSVField.Equals(value))
                    {
                        iD_SW_NR_FSVField = value;
                        OnPropertyChanged("ID_SW_NR_FSV");
                    }
                }
                else
                {
                    iD_SW_NR_FSVField = value;
                    OnPropertyChanged("ID_SW_NR_FSV");
                }
            }
        }

        public string ID_SW_NR_OSV
        {
            get
            {
                return iD_SW_NR_OSVField;
            }

            set
            {
                if (iD_SW_NR_OSVField != null)
                {
                    if (!iD_SW_NR_OSVField.Equals(value))
                    {
                        iD_SW_NR_OSVField = value;
                        OnPropertyChanged("ID_SW_NR_OSV");
                    }
                }
                else
                {
                    iD_SW_NR_OSVField = value;
                    OnPropertyChanged("ID_SW_NR_OSV");
                }
            }
        }

        public string ID_SW_NR_RES
        {
            get
            {
                return iD_SW_NR_RESField;
            }

            set
            {
                if (iD_SW_NR_RESField != null)
                {
                    if (!iD_SW_NR_RESField.Equals(value))
                    {
                        iD_SW_NR_RESField = value;
                        OnPropertyChanged("ID_SW_NR_RES");
                    }
                }
                else
                {
                    iD_SW_NR_RESField = value;
                    OnPropertyChanged("ID_SW_NR_RES");
                }
            }
        }

        public short? ID_EWS_SS
        {
            get
            {
                return iD_EWS_SSField;
            }

            set
            {
                if (iD_EWS_SSField.HasValue)
                {
                    if (!iD_EWS_SSField.Equals(value))
                    {
                        iD_EWS_SSField = value;
                        OnPropertyChanged("ID_EWS_SS");
                    }
                }
                else
                {
                    iD_EWS_SSField = value;
                    OnPropertyChanged("ID_EWS_SS");
                }
            }
        }

        public string SERIENNUMMER
        {
            get
            {
                return sERIENNUMMERField;
            }

            set
            {
                if (sERIENNUMMERField != null)
                {
                    if (!sERIENNUMMERField.Equals(value))
                    {
                        sERIENNUMMERField = value;
                        OnPropertyChanged("SERIENNUMMER");
                    }
                }
                else
                {
                    sERIENNUMMERField = value;
                    OnPropertyChanged("SERIENNUMMER");
                }
            }
        }

        public string ID_BMW_NR
        {
            get
            {
                return iD_BMW_NRField;
            }

            set
            {
                if (iD_BMW_NRField != null)
                {
                    if (!iD_BMW_NRField.Equals(value))
                    {
                        iD_BMW_NRField = value;
                        OnPropertyChanged("ID_BMW_NR");
                    }
                }
                else
                {
                    iD_BMW_NRField = value;
                    OnPropertyChanged("ID_BMW_NR");
                }
            }
        }

        public string ID_HW_NR
        {
            get
            {
                return iD_HW_NRField;
            }

            set
            {
                if (iD_HW_NRField != null)
                {
                    if (!iD_HW_NRField.Equals(value))
                    {
                        iD_HW_NRField = value;
                        OnPropertyChanged("ID_HW_NR");
                    }
                }
                else
                {
                    iD_HW_NRField = value;
                    OnPropertyChanged("ID_HW_NR");
                }
            }
        }

        public short? ID_COD_INDEX
        {
            get
            {
                return iD_COD_INDEXField;
            }

            set
            {
                if (iD_COD_INDEXField.HasValue)
                {
                    if (!iD_COD_INDEXField.Equals(value))
                    {
                        iD_COD_INDEXField = value;
                        OnPropertyChanged("ID_COD_INDEX");
                    }
                }
                else
                {
                    iD_COD_INDEXField = value;
                    OnPropertyChanged("ID_COD_INDEX");
                }
            }
        }

        public int? ID_DIAG_INDEX
        {
            get
            {
                return iD_DIAG_INDEXField;
            }

            set
            {
                if (iD_DIAG_INDEXField.HasValue)
                {
                    if (!iD_DIAG_INDEXField.Equals(value))
                    {
                        iD_DIAG_INDEXField = value;
                        OnPropertyChanged("ID_DIAG_INDEX");
                    }
                }
                else
                {
                    iD_DIAG_INDEXField = value;
                    OnPropertyChanged("ID_DIAG_INDEX");
                }
            }
        }

        public int? ID_VAR_INDEX
        {
            get
            {
                return iD_VAR_INDEXField;
            }

            set
            {
                if (iD_VAR_INDEXField.HasValue)
                {
                    if (!iD_VAR_INDEXField.Equals(value))
                    {
                        iD_VAR_INDEXField = value;
                        OnPropertyChanged("ID_VAR_INDEX");
                    }
                }
                else
                {
                    iD_VAR_INDEXField = value;
                    OnPropertyChanged("ID_VAR_INDEX");
                }
            }
        }

        public int? ID_DATUM_JAHR
        {
            get
            {
                return iD_DATUM_JAHRField;
            }

            set
            {
                if (iD_DATUM_JAHRField.HasValue)
                {
                    if (!iD_DATUM_JAHRField.Equals(value))
                    {
                        iD_DATUM_JAHRField = value;
                        OnPropertyChanged("ID_DATUM_JAHR");
                    }
                }
                else
                {
                    iD_DATUM_JAHRField = value;
                    OnPropertyChanged("ID_DATUM_JAHR");
                }
            }
        }

        public int? ID_DATUM_MONAT
        {
            get
            {
                return iD_DATUM_MONATField;
            }

            set
            {
                if (iD_DATUM_MONATField.HasValue)
                {
                    if (!iD_DATUM_MONATField.Equals(value))
                    {
                        iD_DATUM_MONATField = value;
                        OnPropertyChanged("ID_DATUM_MONAT");
                    }
                }
                else
                {
                    iD_DATUM_MONATField = value;
                    OnPropertyChanged("ID_DATUM_MONAT");
                }
            }
        }

        public int? ID_DATUM_TAG
        {
            get
            {
                return iD_DATUM_TAGField;
            }

            set
            {
                if (iD_DATUM_TAGField.HasValue)
                {
                    if (!iD_DATUM_TAGField.Equals(value))
                    {
                        iD_DATUM_TAGField = value;
                        OnPropertyChanged("ID_DATUM_TAG");
                    }
                }
                else
                {
                    iD_DATUM_TAGField = value;
                    OnPropertyChanged("ID_DATUM_TAG");
                }
            }
        }

        public string ID_DATUM
        {
            get
            {
                return iD_DATUMField;
            }

            set
            {
                if (iD_DATUMField != null)
                {
                    if (!iD_DATUMField.Equals(value))
                    {
                        iD_DATUMField = value;
                        OnPropertyChanged("ID_DATUM");
                    }
                }
                else
                {
                    iD_DATUMField = value;
                    OnPropertyChanged("ID_DATUM");
                }
            }
        }

        public short? ID_DATUM_KW
        {
            get
            {
                return iD_DATUM_KWField;
            }

            set
            {
                if (iD_DATUM_KWField.HasValue)
                {
                    if (!iD_DATUM_KWField.Equals(value))
                    {
                        iD_DATUM_KWField = value;
                        OnPropertyChanged("ID_DATUM_KW");
                    }
                }
                else
                {
                    iD_DATUM_KWField = value;
                    OnPropertyChanged("ID_DATUM_KW");
                }
            }
        }

        public long? ID_SGBD_INDEX
        {
            get
            {
                return iD_SGBD_INDEXField;
            }

            set
            {
                if (iD_SGBD_INDEXField.HasValue)
                {
                    if (!iD_SGBD_INDEXField.Equals(value))
                    {
                        iD_SGBD_INDEXField = value;
                        OnPropertyChanged("ID_SGBD_INDEX");
                    }
                }
                else
                {
                    iD_SGBD_INDEXField = value;
                    OnPropertyChanged("ID_SGBD_INDEX");
                }
            }
        }

        public long ID_SG_ADR
        {
            get
            {
                return iD_SG_ADRField;
            }

            set
            {
                if (!iD_SG_ADRField.Equals(value))
                {
                    iD_SG_ADRField = value;
                    OnPropertyChanged("ID_SG_ADR");
                }
            }
        }

        public long? ID_LIN_SLAVE_ADR
        {
            get
            {
                return iD_LIN_SLAVE_ADRField;
            }

            set
            {
                if (iD_LIN_SLAVE_ADRField.HasValue)
                {
                    if (!iD_LIN_SLAVE_ADRField.Equals(value))
                    {
                        iD_LIN_SLAVE_ADRField = value;
                        OnPropertyChanged("ID_LIN_SLAVE_ADR");
                    }
                }
                else
                {
                    iD_LIN_SLAVE_ADRField = value;
                    OnPropertyChanged("ID_LIN_SLAVE_ADR");
                }
            }
        }

        public int F_ANZ
        {
            get
            {
                return f_ANZField;
            }

            set
            {
                if (!f_ANZField.Equals(value))
                {
                    f_ANZField = value;
                    OnPropertyChanged("F_ANZ");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<DTC>", Placeholder = true)]
        public PlaceholderType FEHLER;
        public int I_ANZ
        {
            get
            {
                return i_ANZField;
            }

            set
            {
                if (!i_ANZField.Equals(value))
                {
                    i_ANZField = value;
                    OnPropertyChanged("I_ANZ");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<DTC>", Placeholder = true)]
        public PlaceholderType INFO;
        public SVK SVK
        {
            get
            {
                return sVKField;
            }

            set
            {
                if (sVKField != null)
                {
                    if (!sVKField.Equals(value))
                    {
                        sVKField = value;
                        OnPropertyChanged("SVK");
                    }
                }
                else
                {
                    sVKField = value;
                    OnPropertyChanged("SVK");
                }
            }
        }

        public string PHYSIKALISCHE_HW_NR
        {
            get
            {
                return pHYSIKALISCHE_HW_NRField;
            }

            set
            {
                if (pHYSIKALISCHE_HW_NRField != null)
                {
                    if (!pHYSIKALISCHE_HW_NRField.Equals(value))
                    {
                        pHYSIKALISCHE_HW_NRField = value;
                        OnPropertyChanged("PHYSIKALISCHE_HW_NR");
                    }
                }
                else
                {
                    pHYSIKALISCHE_HW_NRField = value;
                    OnPropertyChanged("PHYSIKALISCHE_HW_NR");
                }
            }
        }

        public ObservableCollection<typeECU_Transaction> TAL
        {
            get
            {
                return tALField;
            }

            set
            {
                if (tALField != null)
                {
                    if (!tALField.Equals(value))
                    {
                        tALField = value;
                        OnPropertyChanged("TAL");
                    }
                }
                else
                {
                    tALField = value;
                    OnPropertyChanged("TAL");
                }
            }
        }

        public string HARDWARE_REFERENZ
        {
            get
            {
                return hARDWARE_REFERENZField;
            }

            set
            {
                if (hARDWARE_REFERENZField != null)
                {
                    if (!hARDWARE_REFERENZField.Equals(value))
                    {
                        hARDWARE_REFERENZField = value;
                        OnPropertyChanged("HARDWARE_REFERENZ");
                    }
                }
                else
                {
                    hARDWARE_REFERENZField = value;
                    OnPropertyChanged("HARDWARE_REFERENZ");
                }
            }
        }

        public int? HW_REF_STATUS
        {
            get
            {
                return hW_REF_STATUSField;
            }

            set
            {
                if (hW_REF_STATUSField.HasValue)
                {
                    if (!hW_REF_STATUSField.Equals(value))
                    {
                        hW_REF_STATUSField = value;
                        OnPropertyChanged("HW_REF_STATUS");
                    }
                }
                else
                {
                    hW_REF_STATUSField = value;
                    OnPropertyChanged("HW_REF_STATUS");
                }
            }
        }

        public ObservableCollection<typeSWTStatus> SWTStatus
        {
            get
            {
                return sWTStatusField;
            }

            set
            {
                if (sWTStatusField != null)
                {
                    if (!sWTStatusField.Equals(value))
                    {
                        sWTStatusField = value;
                        OnPropertyChanged("SWTStatus");
                    }
                }
                else
                {
                    sWTStatusField = value;
                    OnPropertyChanged("SWTStatus");
                }
            }
        }

        public string DATEN_REFERENZ
        {
            get
            {
                return dATEN_REFERENZField;
            }

            set
            {
                if (dATEN_REFERENZField != null)
                {
                    if (!dATEN_REFERENZField.Equals(value))
                    {
                        dATEN_REFERENZField = value;
                        OnPropertyChanged("DATEN_REFERENZ");
                    }
                }
                else
                {
                    dATEN_REFERENZField = value;
                    OnPropertyChanged("DATEN_REFERENZ");
                }
            }
        }

        public ObservableCollection<BusType> SubBUS
        {
            get
            {
                return subBUSField;
            }

            set
            {
                if (subBUSField != null)
                {
                    if (!subBUSField.Equals(value))
                    {
                        subBUSField = value;
                        OnPropertyChanged("SubBUS");
                    }
                }
                else
                {
                    subBUSField = value;
                    OnPropertyChanged("SubBUS");
                }
            }
        }

        public string ECU_GROBNAME
        {
            get
            {
                return eCU_GROBNAMEField;
            }

            set
            {
                if (eCU_GROBNAMEField != null)
                {
                    if (!eCU_GROBNAMEField.Equals(value))
                    {
                        eCU_GROBNAMEField = value;
                        OnPropertyChanged("ECU_GROBNAME");
                    }
                }
                else
                {
                    eCU_GROBNAMEField = value;
                    OnPropertyChanged("ECU_GROBNAME");
                }
            }
        }

        public string ECU_NAME
        {
            get
            {
                return eCU_NAMEField;
            }

            set
            {
                if (eCU_NAMEField != null)
                {
                    if (!eCU_NAMEField.Equals(value))
                    {
                        eCU_NAMEField = value;
                        OnPropertyChanged("ECU_NAME");
                    }
                }
                else
                {
                    eCU_NAMEField = value;
                    OnPropertyChanged("ECU_NAME");
                }
            }
        }

        public string ECU_SGBD
        {
            get
            {
                return eCU_SGBDField;
            }

            set
            {
                if (eCU_SGBDField != null)
                {
                    if (!eCU_SGBDField.Equals(value))
                    {
                        eCU_SGBDField = value;
                        OnPropertyChanged("ECU_SGBD");
                    }
                }
                else
                {
                    eCU_SGBDField = value;
                    OnPropertyChanged("ECU_SGBD");
                }
            }
        }

        public string ECU_GRUPPE
        {
            get
            {
                return eCU_GRUPPEField;
            }

            set
            {
                if (eCU_GRUPPEField != null)
                {
                    if (!eCU_GRUPPEField.Equals(value))
                    {
                        eCU_GRUPPEField = value;
                        OnPropertyChanged("ECU_GRUPPE");
                    }
                }
                else
                {
                    eCU_GRUPPEField = value;
                    OnPropertyChanged("ECU_GRUPPE");
                }
            }
        }

        [DefaultValue(false)]
        public bool COMMUNICATION_SUCCESSFULLY
        {
            get
            {
                return cOMMUNICATION_SUCCESSFULLYField;
            }

            set
            {
                if (!cOMMUNICATION_SUCCESSFULLYField.Equals(value))
                {
                    cOMMUNICATION_SUCCESSFULLYField = value;
                    OnPropertyChanged("COMMUNICATION_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool IDENT_SUCCESSFULLY
        {
            get
            {
                return iDENT_SUCCESSFULLYField;
            }

            set
            {
                if (!iDENT_SUCCESSFULLYField.Equals(value))
                {
                    iDENT_SUCCESSFULLYField = value;
                    OnPropertyChanged("IDENT_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool AIF_SUCCESSFULLY
        {
            get
            {
                return aIF_SUCCESSFULLYField;
            }

            set
            {
                if (!aIF_SUCCESSFULLYField.Equals(value))
                {
                    aIF_SUCCESSFULLYField = value;
                    OnPropertyChanged("AIF_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool FS_SUCCESSFULLY
        {
            get
            {
                return fS_SUCCESSFULLYField;
            }

            set
            {
                if (!fS_SUCCESSFULLYField.Equals(value))
                {
                    fS_SUCCESSFULLYField = value;
                    OnPropertyChanged("FS_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool IS_SUCCESSFULLY
        {
            get
            {
                return iS_SUCCESSFULLYField;
            }

            set
            {
                if (!iS_SUCCESSFULLYField.Equals(value))
                {
                    iS_SUCCESSFULLYField = value;
                    OnPropertyChanged("IS_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool SERIAL_SUCCESSFULLY
        {
            get
            {
                return sERIAL_SUCCESSFULLYField;
            }

            set
            {
                if (!sERIAL_SUCCESSFULLYField.Equals(value))
                {
                    sERIAL_SUCCESSFULLYField = value;
                    OnPropertyChanged("SERIAL_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool SVK_SUCCESSFULLY
        {
            get
            {
                return sVK_SUCCESSFULLYField;
            }

            set
            {
                if (!sVK_SUCCESSFULLYField.Equals(value))
                {
                    sVK_SUCCESSFULLYField = value;
                    OnPropertyChanged("SVK_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool PHYSHW_SUCCESSFULLY
        {
            get
            {
                return pHYSHW_SUCCESSFULLYField;
            }

            set
            {
                if (!pHYSHW_SUCCESSFULLYField.Equals(value))
                {
                    pHYSHW_SUCCESSFULLYField = value;
                    OnPropertyChanged("PHYSHW_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool HWREF_SUCCESSFULLY
        {
            get
            {
                return hWREF_SUCCESSFULLYField;
            }

            set
            {
                if (!hWREF_SUCCESSFULLYField.Equals(value))
                {
                    hWREF_SUCCESSFULLYField = value;
                    OnPropertyChanged("HWREF_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool ECU_HAS_CONFIG_OVERRIDE
        {
            get
            {
                return eCU_HAS_CONFIG_OVERRIDEField;
            }

            set
            {
                if (!eCU_HAS_CONFIG_OVERRIDEField.Equals(value))
                {
                    eCU_HAS_CONFIG_OVERRIDEField = value;
                    OnPropertyChanged("ECU_HAS_CONFIG_OVERRIDE");
                }
            }
        }

        [DefaultValue(typeof(uint), "0")]
        public uint BUSID
        {
            get
            {
                return bUSIDField;
            }

            set
            {
                _ = bUSIDField;
                if (!bUSIDField.Equals(value))
                {
                    bUSIDField = value;
                    OnPropertyChanged("BUSID");
                }
            }
        }

        [DefaultValue(false)]
        public bool DATEN_REFERENZ_SUCCESSFULLY
        {
            get
            {
                return dATEN_REFERENZ_SUCCESSFULLYField;
            }

            set
            {
                if (!dATEN_REFERENZ_SUCCESSFULLYField.Equals(value))
                {
                    dATEN_REFERENZ_SUCCESSFULLYField = value;
                    OnPropertyChanged("DATEN_REFERENZ_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(-1)]
        public int FLASH_STATE
        {
            get
            {
                return fLASH_STATEField;
            }

            set
            {
                if (!fLASH_STATEField.Equals(value))
                {
                    fLASH_STATEField = value;
                    OnPropertyChanged("FLASH_STATE");
                }
            }
        }

        [DefaultValue(true)]
        public bool ECU_ASSEMBLY_CONFIRMED
        {
            get
            {
                return eCU_ASSEMBLY_CONFIRMEDField;
            }

            set
            {
                if (!eCU_ASSEMBLY_CONFIRMEDField.Equals(value))
                {
                    eCU_ASSEMBLY_CONFIRMEDField = value;
                    OnPropertyChanged("ECU_ASSEMBLY_CONFIRMED");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [PreserveSource(Hint = "Unchanged", OriginalHash = "879276A46FA5BDD31930A0163729BCCE", SignatureModified = true)]
        protected ECU(ECU ecu) : this()
        {
            BaseVariant = ecu.BaseVariant;
            EcuVariant = ecu.EcuVariant;
            BnTnName = ecu.BnTnName;
            GatewayDiagAddrAsInt = ecu.GatewayDiagAddrAsInt;
            DiagBus = ecu.DiagBus;
            SerialNumber = ecu.SerialNumber;
            EcuIdentifier = ecu.EcuIdentifier;
            StandardSvk = ecu.StandardSvk;
            BusCons = ecu.BusCons;
            EcuDetailInfo = ecu.EcuDetailInfo;
            EcuStatusInfo = ecu.EcuStatusInfo;
            EcuPdxInfo = ecu.EcuPdxInfo;
            ID_SG_ADR = ecu.ID_SG_ADR;
            XepEcuVariant = ecu.XepEcuVariant;
            EcuVariant = ecu.EcuVariant;
            XepEcuClique = ecu.XepEcuClique;
            EcuGroup = ecu.EcuGroup;
            EcuRep = ecu.EcuRep;
            IsSmartActuator = ecu.IsSmartActuator;
        }

        private IList<Bus> GetBusConnections()
        {
            return BusCons?.Select((IBusObject x) => x.ConvertToBus()).ToList();
        }

        private IList<string> GetBusConnectionsAsString()
        {
            return BusCons?.Select((IBusObject x) => x.ToString()).ToList();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ECU eCU))
            {
                return false;
            }

            if (ID_SG_ADR == eCU.ID_SG_ADR && ID_LIN_SLAVE_ADR == eCU.ID_LIN_SLAVE_ADR)
            {
                return true;
            }

            return false;
        }

        public void FillEcuTitleTree(string ecuShortName)
        {
            if (string.IsNullOrEmpty(TITLE_ECUTREE) || !IsTitleEcuTreeFilled())
            {
                string eCU_GROBNAME = ECU_GROBNAME;
                TITLE_ECUTREE = (string.IsNullOrEmpty(ecuShortName) ? eCU_GROBNAME : ecuShortName);
            }
        }

        public void FillEcuTitleTree(ISet<string> ecuShortName)
        {
            TITLE_ECUTREE = ((ecuShortName.Count == 0) ? ECU_GROBNAME : ecuShortName.First());
        }

        [PreserveSource(Hint = "public DTC", Placeholder = true)]
        public PlaceholderType getDTCbyF_ORT(int F_ORT)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Modified")]
        IDtc IEcu.getDTCbyF_ORT(int F_ORT)
        {
            return null;
        }

        [PreserveSource(Hint = "public DTC", Placeholder = true)]
        public PlaceholderType GetDTCById(decimal id)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "IDtc", Placeholder = true)]
        PlaceholderType IEcu.GetDTCById(decimal id)
        {
            return GetDTCById(id);
        }

        public override int GetHashCode()
        {
            if (ID_LIN_SLAVE_ADR.HasValue)
            {
                return (ID_SG_ADR + ID_LIN_SLAVE_ADR.Value).GetHashCode();
            }

            return ID_SG_ADR.GetHashCode();
        }

        public string GetNewestZusbauNoFromAif()
        {
            string result = null;
            if (AIF != null)
            {
                int? num = null;
                foreach (AIF item in AIF)
                {
                    if (!num.HasValue || item.AIF_ANZAHL_PROG > num)
                    {
                        num = item.AIF_ANZAHL_PROG;
                        result = item.AIF_ZB_NR;
                    }
                }
            }

            return result;
        }

        public bool IsRoot()
        {
            BusType bUS = BUS;
            if (bUS == BusType.ROOT || bUS == BusType.VIRTUALROOT)
            {
                return true;
            }

            return false;
        }

        [PreserveSource(Hint = "Modified")]
        public bool IsSet(long fOrt)
        {
            return false;
        }

        public bool IsTitleEcuTreeFilled()
        {
            if (!string.IsNullOrEmpty(TITLE_ECUTREE))
            {
                return !TITLE_ECUTREE.EndsWith($"0x{ID_SG_ADR:X2}", StringComparison.Ordinal);
            }

            return false;
        }

        public bool IsVirtual()
        {
            BusType bUS = BUS;
            if (bUS == BusType.VIRTUAL || bUS == BusType.VIRTUALROOT)
            {
                return true;
            }

            return false;
        }

        public bool IsVirtualOrVirtualBusCheck()
        {
            BusType bUS = BUS;
            if ((uint)(bUS - 26) <= 2u)
            {
                return true;
            }

            return false;
        }

        public bool IsVirtualRootOrVirtualBusCheck()
        {
            BusType bUS = BUS;
            if ((uint)(bUS - 27) <= 1u)
            {
                return true;
            }

            return false;
        }

        public string LogEcu()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("TITLE_ECUTREE: \"").Append(TITLE_ECUTREE).Append("\"");
            stringBuilder.Append("; ID_SG_ADR: \"").Append(string.Format(CultureInfo.InvariantCulture, "0x{0:X2}", ID_SG_ADR)).Append("\"");
            stringBuilder.Append("; ECU_GROBNAME: \"").Append(ECU_GROBNAME).Append("\"");
            stringBuilder.Append("; ECU_GRUPPE: \"").Append(ECU_GRUPPE).Append("\"");
            stringBuilder.Append("; ECU_SGBD: \"").Append(ECU_SGBD).Append("\"");
            stringBuilder.Append("; VARIANTE: \"").Append(VARIANTE).Append("\"");
            stringBuilder.Append("; ProgrammingVariantName: \"").Append(ProgrammingVariantName).Append("\"");
            stringBuilder.Append("; SERIENNUMMER: \"").Append(SERIENNUMMER).Append("\"");
            stringBuilder.Append("; IDENT_SUCCESSFULLY: \"").Append(IDENT_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; IS_SUCCESSFULLY: \"").Append(IS_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; FS_SUCCESSFULLY: \"").Append(FS_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; SERIAL_SUCCESSFULLY: \"").Append(SERIAL_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; AIF_SUCCESSFULLY: \"").Append(AIF_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; COMMUNICATION_SUCCESSFULLY: \"").Append(COMMUNICATION_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; DATEN_REFERENZ_SUCCESSFULLY: \"").Append(DATEN_REFERENZ_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; HWREF_SUCCESSFULLY: \"").Append(HWREF_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; PHYSHW_SUCCESSFULLY: \"").Append(PHYSHW_SUCCESSFULLY).Append("\"");
            stringBuilder.Append("; SVK_SUCCESSFULLY: \"").Append(SVK_SUCCESSFULLY).Append("\"");
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.AppendFormat("ID_SG_ADR: 0x{0:X2} \n", ID_SG_ADR);
                stringBuilder.AppendFormat("ECU_HAS_CONFIG_OVERRIDE: {0} \n", ECU_HAS_CONFIG_OVERRIDE);
                stringBuilder.AppendFormat("VARIANTE: {0} \n", VARIANTE);
                stringBuilder.AppendFormat("ECU_GRUPPE: {0} \n", ECU_GRUPPE);
                stringBuilder.AppendFormat("ECU_SGBD: {0} \n", ECU_SGBD);
                stringBuilder.AppendFormat("DiagProtocoll: {0} \n", DiagProtocoll);
                stringBuilder.AppendFormat("BUS: {0}\n", BUS);
                stringBuilder.AppendFormat("COMMUNICATION_SUCCESSFULLY: {0}\n", COMMUNICATION_SUCCESSFULLY);
                stringBuilder.AppendFormat("IDENT_SUCCESSFULLY: {0} \n", IDENT_SUCCESSFULLY);
                stringBuilder.AppendFormat("FS_SUCCESSFULLY: {0} \n", FS_SUCCESSFULLY);
                stringBuilder.AppendFormat("SVK_SUCCESSFULLY: {0} \n", SVK_SUCCESSFULLY);
                stringBuilder.AppendFormat("SERIAL_SUCCESSFULLY: {0} \n", SERIAL_SUCCESSFULLY);
                stringBuilder.AppendFormat("PHYSHW_SUCCESSFULLY: {0} \n", PHYSHW_SUCCESSFULLY);
            }
            catch (Exception exception)
            {
                Log.WarningException("ECU.ToString()", exception);
            }

            return stringBuilder.ToString();
        }

        [PreserveSource(Hint = "Modified", OriginalHash = "F486A9952B9B93F952D0B5663177646B")]
        public ECU()
        {
            subBUSField = new ObservableCollection<BusType>();
            sWTStatusField = new ObservableCollection<typeSWTStatus>();
            // [IGNORE] selectedINFOField = new DTC();
            // [IGNORE] selectedDTCField = new DTC();
            tALField = new ObservableCollection<typeECU_Transaction>();
            sVKField = new SVK();
            // [IGNORE] iNFOField = new ObservableCollection<DTC>();
            // [IGNORE] FEHLER = new ObservableCollection<DTC>();
            // [IGNORE] aIFField = new ObservableCollection<AIF>();
            // [IGNORE] jOBSField = new ObservableCollection<JOB>();
            bUSField = BusType.UNKNOWN;
            diagProtocollField = typeDiagProtocoll.UNKNOWN;
            eCUTreeColumnField = -1;
            eCUTreeRowField = -1;
            eCUTreeColorField = "#323232";
            iD_SG_ADRField = -1L;
            f_ANZField = 0;
            i_ANZField = 0;
            cOMMUNICATION_SUCCESSFULLYField = false;
            iDENT_SUCCESSFULLYField = false;
            aIF_SUCCESSFULLYField = false;
            fS_SUCCESSFULLYField = false;
            iS_SUCCESSFULLYField = false;
            sERIAL_SUCCESSFULLYField = false;
            sVK_SUCCESSFULLYField = false;
            pHYSHW_SUCCESSFULLYField = false;
            hWREF_SUCCESSFULLYField = false;
            eCU_HAS_CONFIG_OVERRIDEField = false;
            bUSIDField = 0u;
            dATEN_REFERENZ_SUCCESSFULLYField = false;
            fLASH_STATEField = -1;
            eCU_ASSEMBLY_CONFIRMEDField = true;
            DiagBus = BusObject.ACan;
            eCU_NAMEField = string.Empty;
            eCU_SGBDField = string.Empty;
        }

        [PreserveSource(Hint = "Cleaned", OriginalHash = "798C0814C72C55C6E9E0D1F8AC2D1F11")]
        private void FEHLER_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}