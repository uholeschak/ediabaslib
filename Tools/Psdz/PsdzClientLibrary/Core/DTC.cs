using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class DTC : ICloneable, IDtc, INotifyPropertyChanged, IDtcVehicle
    {
        private ObservableCollection<int> relevanceFaultCode;
        private readonly ObservableCollectionEx<F_UW_Display> f_UW_Display = new ObservableCollectionEx<F_UW_Display>();
        private ObservableCollection<typeSAEContext> SAEContextField;
        private int StateWarningLightField;
        private long? f_VERSIONField;
        private long? f_ARTField;
        private ObservableCollection<typeFArtExt> f_ART_EXTField;
        private long? f_ORTField;
        private string f_ORT_TEXTField;
        private int? f_EREIGNIS_DTCField;
        private int? f_FEHLERKLASSE_NRField;
        private string f_FEHLERKLASSE_TEXTField;
        private ObservableCollection<typeDTCContext> dTCContextField;
        private int? f_SYMPTOM_NRField;
        private string f_SYMPTOM_TEXTField;
        private int? f_READY_NRField;
        private string f_READY_TEXTField;
        private int? f_VORHANDEN_NRField;
        private string f_VORHANDEN_TEXTField;
        private int? f_WARNUNG_NRField;
        private string f_WARNUNG_TEXTField;
        private long? f_HFKField;
        private long? f_HLZField;
        private long? f_UEBERLAUFField;
        private byte[] f_HEX_CODEField;
        private bool? relevanceField;
        private ushort? f_PCODEField;
        private string f_PCODE_STRINGField;
        private string f_PCODE_TEXTField;
        private uint? f_SAE_CODEField;
        private string f_SAE_CODE_STRINGField;
        private string f_SAE_CODE_TEXTField;
        private long? f_LZField;
        private string f_CODEField;
        private decimal? idField;
        private bool fS_LESEN_SUCCESSFULLYField;
        private bool fS_LESEN_DETAIL_SUCCESSFULLYField;
        private bool isVirtualField;
        private bool isCombinedField;
        private string ecuDTCTypeField;
        private int faultClass;
        private int faultGroup;
        private ObservableCollection<DTC> combinedFaultsRelatedDtcs;
        [XmlIgnore]
        private static Random rnd = new Random();
        [XmlIgnore]
        private static bool showWarningLights = ConfigSettings.getConfigStringAsBoolean("TesterGUI.ShowTestWarningLights", defaultValue: false);
        [XmlIgnore]
        private decimal fcId;
        [XmlIgnore]
        public bool IsFSLesenExpert { get; set; }

        [XmlIgnore]
        public bool IsDisabledByUser { get; set; }

        [XmlIgnore]
        public ObservableCollection<int> RelevanceFaultCode
        {
            get
            {
                return relevanceFaultCode;
            }

            set
            {
                relevanceFaultCode = value;
            }
        }

        [XmlIgnore]
        public typeDTCContext Current
        {
            get
            {
                typeDTCContext result = null;
                if (DTCContext != null && DTCContext.Count > 0)
                {
                    result = DTCContext[DTCContext.Count - 1];
                }

                return result;
            }
        }

        [XmlIgnore]
        public double Mileage => Current?.Mileage ?? (-1.0);

        [XmlIgnore]
        public typeDTCContext First
        {
            get
            {
                typeDTCContext result = null;
                if (DTCContext != null && DTCContext.Count > 0)
                {
                    result = DTCContext[0];
                }

                return result;
            }
        }

        [XmlIgnore]
        public typeDTCContext Second
        {
            get
            {
                typeDTCContext result = null;
                if (DTCContext != null && DTCContext.Count > 1)
                {
                    result = DTCContext[1];
                }

                return result;
            }
        }

        [XmlIgnore]
        public ObservableCollectionEx<F_UW_Display> F_UW_Display => f_UW_Display;

        [XmlIgnore]
        public long? F_UW_KM
        {
            get
            {
                if (Current != null && Current.F_UW_KM is long)
                {
                    return Current.F_UW_KM.Value;
                }

                return null;
            }
        }

        [XmlIgnore]
        public double? F_UW_KM_SUPREME
        {
            get
            {
                if (Current != null && Current.F_UW_KM_SUPREME is double)
                {
                    return Current.F_UW_KM_SUPREME;
                }

                return null;
            }
        }

        [XmlIgnore]
        public long F_UW_KM_Min
        {
            get
            {
                long num = -1L;
                if (DTCContext != null && DTCContext.Count > 0)
                {
                    if (DTCContext[0].F_UW_KM_SUPREME.HasValue)
                    {
                        num = (long)DTCContext[0].F_UW_KM_SUPREME.Value;
                        foreach (typeDTCContext item in DTCContext)
                        {
                            if (item.F_UW_KM_SUPREME.HasValue && item.F_UW_KM_SUPREME < (double)num)
                            {
                                num = (long)item.F_UW_KM_SUPREME.Value;
                            }
                        }
                    }
                    else if (DTCContext[0].F_UW_KM.HasValue)
                    {
                        num = DTCContext[0].F_UW_KM.Value;
                        foreach (typeDTCContext item2 in DTCContext)
                        {
                            if (item2.F_UW_KM.HasValue && item2.F_UW_KM < num)
                            {
                                num = item2.F_UW_KM.Value;
                            }
                        }
                    }
                }

                return num;
            }
        }

        [XmlIgnore]
        public long F_UW_ZEIT
        {
            get
            {
                long result = -1L;
                if (Current != null && Current.F_UW_ZEIT is long)
                {
                    result = Current.F_UW_ZEIT.Value;
                }

                return result;
            }
        }

        [XmlIgnore]
        public double? F_UW_ZEIT_SUPREME
        {
            get
            {
                double value = -1.0;
                if (Current != null && Current.F_UW_ZEIT_SUPREME is double)
                {
                    value = Current.F_UW_ZEIT_SUPREME.Value;
                }

                return value;
            }
        }

        [XmlIgnore]
        [PreserveSource(Hint = "FaultCodeConverters.FormatFort(this)", Placeholder = true)]
        public string FortAsHexString => string.Empty;

        [XmlIgnore]
        IEnumerable<IFArtExt> IDtc.F_ART_EXT => F_ART_EXT;

        [XmlIgnore]
        IDtcContext IDtc.Current => Current;

        [XmlIgnore]
        IEnumerable<IDtcContext> IDtc.DTCContext => DTCContext;

        [XmlIgnore]
        IEnumerable<IDtcUmweltDisplay> IDtc.F_UW_Display => F_UW_Display;

        [XmlIgnore]
        IDtcContext IDtc.First => First;

        [XmlIgnore]
        IDtcContext IDtc.Second => Second;
        public string ecuAddress { get; set; }

        public string dtcId
        {
            get
            {
                return Id.ToString();
            }

            set
            {
                if (value != null && decimal.TryParse(value, out var result))
                {
                    Id = result;
                }
            }
        }

        bool? IDtcVehicle.Relevance
        {
            get
            {
                return Relevance;
            }

            set
            {
                Relevance = value;
            }
        }

        public decimal? fOrt
        {
            get
            {
                return F_ORT;
            }

            set
            {
                F_ORT = (long)value.Value;
            }
        }

        [XmlIgnore]
        public Guid UniqueId { get; set; }

        public int FaultClass
        {
            get
            {
                return faultClass;
            }

            set
            {
                if (faultClass != value)
                {
                    faultClass = value;
                    OnPropertyChanged("FaultClass");
                    MapFaultClassToFaultGroup();
                }
            }
        }

        public int FaultGroup
        {
            get
            {
                return faultGroup;
            }

            set
            {
                if (faultGroup != value)
                {
                    faultGroup = value;
                    OnPropertyChanged("FaultGroup");
                }
            }
        }

        public ObservableCollection<typeSAEContext> SAEContext
        {
            get
            {
                return SAEContextField;
            }

            set
            {
                if (SAEContextField != null)
                {
                    if (!SAEContextField.Equals(value))
                    {
                        SAEContextField = value;
                        OnPropertyChanged("SAEContext");
                    }
                }
                else
                {
                    SAEContextField = value;
                    OnPropertyChanged("SAEContext");
                }
            }
        }

        public long? F_VERSION
        {
            get
            {
                return f_VERSIONField;
            }

            set
            {
                if (f_VERSIONField.HasValue)
                {
                    if (!f_VERSIONField.Equals(value))
                    {
                        f_VERSIONField = value;
                        OnPropertyChanged("F_VERSION");
                    }
                }
                else
                {
                    f_VERSIONField = value;
                    OnPropertyChanged("F_VERSION");
                }
            }
        }

        public long? F_ART
        {
            get
            {
                return f_ARTField;
            }

            set
            {
                if (f_ARTField.HasValue)
                {
                    if (!f_ARTField.Equals(value))
                    {
                        f_ARTField = value;
                        OnPropertyChanged("F_ART");
                    }
                }
                else
                {
                    f_ARTField = value;
                    OnPropertyChanged("F_ART");
                }
            }
        }

        public ObservableCollection<typeFArtExt> F_ART_EXT
        {
            get
            {
                return f_ART_EXTField;
            }

            set
            {
                if (f_ART_EXTField != null)
                {
                    if (!f_ART_EXTField.Equals(value))
                    {
                        f_ART_EXTField = value;
                        OnPropertyChanged("F_ART_EXT");
                    }
                }
                else
                {
                    f_ART_EXTField = value;
                    OnPropertyChanged("F_ART_EXT");
                }
            }
        }

        public long? F_ORT
        {
            get
            {
                return f_ORTField;
            }

            set
            {
                if (f_ORTField.HasValue)
                {
                    if (!f_ORTField.Equals(value))
                    {
                        f_ORTField = value;
                        OnPropertyChanged("F_ORT");
                    }
                }
                else
                {
                    f_ORTField = value;
                    OnPropertyChanged("F_ORT");
                }
            }
        }

        public string F_ORT_TEXT
        {
            get
            {
                return f_ORT_TEXTField;
            }

            set
            {
                if (f_ORT_TEXTField != null)
                {
                    if (!f_ORT_TEXTField.Equals(value))
                    {
                        f_ORT_TEXTField = value;
                        OnPropertyChanged("F_ORT_TEXT");
                    }
                }
                else
                {
                    f_ORT_TEXTField = value;
                    OnPropertyChanged("F_ORT_TEXT");
                }
            }
        }

        public int? F_EREIGNIS_DTC
        {
            get
            {
                return f_EREIGNIS_DTCField;
            }

            set
            {
                if (f_EREIGNIS_DTCField.HasValue)
                {
                    if (!f_EREIGNIS_DTCField.Equals(value))
                    {
                        f_EREIGNIS_DTCField = value;
                        OnPropertyChanged("F_EREIGNIS_DTC");
                    }
                }
                else
                {
                    f_EREIGNIS_DTCField = value;
                    OnPropertyChanged("F_EREIGNIS_DTC");
                }
            }
        }

        public int? F_FEHLERKLASSE_NR
        {
            get
            {
                return f_FEHLERKLASSE_NRField;
            }

            set
            {
                if (f_FEHLERKLASSE_NRField != value)
                {
                    f_FEHLERKLASSE_NRField = value;
                    OnPropertyChanged("F_FEHLERKLASSE_NR");
                    AddFaultGroupClass();
                }
            }
        }

        public string F_FEHLERKLASSE_TEXT
        {
            get
            {
                return f_FEHLERKLASSE_TEXTField;
            }

            set
            {
                if (f_FEHLERKLASSE_TEXTField != null)
                {
                    if (!f_FEHLERKLASSE_TEXTField.Equals(value))
                    {
                        f_FEHLERKLASSE_TEXTField = value;
                        OnPropertyChanged("F_FEHLERKLASSE_TEXT");
                    }
                }
                else
                {
                    f_FEHLERKLASSE_TEXTField = value;
                    OnPropertyChanged("F_FEHLERKLASSE_TEXT");
                }
            }
        }

        public ObservableCollection<typeDTCContext> DTCContext
        {
            get
            {
                return dTCContextField;
            }

            set
            {
                if (dTCContextField != null)
                {
                    if (!dTCContextField.Equals(value))
                    {
                        dTCContextField = value;
                        OnPropertyChanged("DTCContext");
                        dTCContextField.CollectionChanged += DTCContextField_CollectionChanged;
                    }
                }
                else
                {
                    dTCContextField = value;
                    OnPropertyChanged("DTCContext");
                    dTCContextField.CollectionChanged += DTCContextField_CollectionChanged;
                }
            }
        }

        public int? F_SYMPTOM_NR
        {
            get
            {
                return f_SYMPTOM_NRField;
            }

            set
            {
                if (f_SYMPTOM_NRField.HasValue)
                {
                    if (!f_SYMPTOM_NRField.Equals(value))
                    {
                        f_SYMPTOM_NRField = value;
                        OnPropertyChanged("F_SYMPTOM_NR");
                    }
                }
                else
                {
                    f_SYMPTOM_NRField = value;
                    OnPropertyChanged("F_SYMPTOM_NR");
                }
            }
        }

        public int StateWarningLight
        {
            get
            {
                return StateWarningLightField;
            }

            set
            {
                _ = StateWarningLightField;
                if (!StateWarningLightField.Equals(value))
                {
                    StateWarningLightField = value;
                    OnPropertyChanged("StateWarningLight");
                }
            }
        }

        public string F_SYMPTOM_TEXT
        {
            get
            {
                return f_SYMPTOM_TEXTField;
            }

            set
            {
                if (f_SYMPTOM_TEXTField != null)
                {
                    if (!f_SYMPTOM_TEXTField.Equals(value))
                    {
                        f_SYMPTOM_TEXTField = value;
                        OnPropertyChanged("F_SYMPTOM_TEXT");
                    }
                }
                else
                {
                    f_SYMPTOM_TEXTField = value;
                    OnPropertyChanged("F_SYMPTOM_TEXT");
                }
            }
        }

        public int? F_READY_NR
        {
            get
            {
                return f_READY_NRField;
            }

            set
            {
                if (f_READY_NRField.HasValue)
                {
                    if (!f_READY_NRField.Equals(value))
                    {
                        f_READY_NRField = value;
                        OnPropertyChanged("F_READY_NR");
                    }
                }
                else
                {
                    f_READY_NRField = value;
                    OnPropertyChanged("F_READY_NR");
                }
            }
        }

        public string F_READY_TEXT
        {
            get
            {
                return f_READY_TEXTField;
            }

            set
            {
                if (f_READY_TEXTField != null)
                {
                    if (!f_READY_TEXTField.Equals(value))
                    {
                        f_READY_TEXTField = value;
                        OnPropertyChanged("F_READY_TEXT");
                    }
                }
                else
                {
                    f_READY_TEXTField = value;
                    OnPropertyChanged("F_READY_TEXT");
                }
            }
        }

        public int? F_VORHANDEN_NR
        {
            get
            {
                return f_VORHANDEN_NRField;
            }

            set
            {
                if (f_VORHANDEN_NRField.HasValue)
                {
                    if (!f_VORHANDEN_NRField.Equals(value))
                    {
                        f_VORHANDEN_NRField = value;
                        OnPropertyChanged("F_VORHANDEN_NR");
                    }
                }
                else
                {
                    f_VORHANDEN_NRField = value;
                    OnPropertyChanged("F_VORHANDEN_NR");
                }
            }
        }

        public string F_VORHANDEN_TEXT
        {
            get
            {
                return f_VORHANDEN_TEXTField;
            }

            set
            {
                if (f_VORHANDEN_TEXTField != null)
                {
                    if (!f_VORHANDEN_TEXTField.Equals(value))
                    {
                        f_VORHANDEN_TEXTField = value;
                        OnPropertyChanged("F_VORHANDEN_TEXT");
                    }
                }
                else
                {
                    f_VORHANDEN_TEXTField = value;
                    OnPropertyChanged("F_VORHANDEN_TEXT");
                }
            }
        }

        public int? F_WARNUNG_NR
        {
            get
            {
                return f_WARNUNG_NRField;
            }

            set
            {
                if (f_WARNUNG_NRField.HasValue)
                {
                    if (!f_WARNUNG_NRField.Equals(value))
                    {
                        f_WARNUNG_NRField = value;
                        OnPropertyChanged("F_WARNUNG_NR");
                    }
                }
                else
                {
                    f_WARNUNG_NRField = value;
                    OnPropertyChanged("F_WARNUNG_NR");
                }
            }
        }

        public string F_WARNUNG_TEXT
        {
            get
            {
                return f_WARNUNG_TEXTField;
            }

            set
            {
                if (f_WARNUNG_TEXTField != null)
                {
                    if (!f_WARNUNG_TEXTField.Equals(value))
                    {
                        f_WARNUNG_TEXTField = value;
                        OnPropertyChanged("F_WARNUNG_TEXT");
                    }
                }
                else
                {
                    f_WARNUNG_TEXTField = value;
                    OnPropertyChanged("F_WARNUNG_TEXT");
                }
            }
        }

        public long? F_HFK
        {
            get
            {
                return f_HFKField;
            }

            set
            {
                if (f_HFKField.HasValue)
                {
                    if (!f_HFKField.Equals(value))
                    {
                        f_HFKField = value;
                        OnPropertyChanged("F_HFK");
                    }
                }
                else
                {
                    f_HFKField = value;
                    OnPropertyChanged("F_HFK");
                }
            }
        }

        public long? F_HLZ
        {
            get
            {
                return f_HLZField;
            }

            set
            {
                if (f_HLZField.HasValue)
                {
                    if (!f_HLZField.Equals(value))
                    {
                        f_HLZField = value;
                        OnPropertyChanged("F_HLZ");
                    }
                }
                else
                {
                    f_HLZField = value;
                    OnPropertyChanged("F_HLZ");
                }
            }
        }

        public long? F_UEBERLAUF
        {
            get
            {
                return f_UEBERLAUFField;
            }

            set
            {
                if (f_UEBERLAUFField.HasValue)
                {
                    if (!f_UEBERLAUFField.Equals(value))
                    {
                        f_UEBERLAUFField = value;
                        OnPropertyChanged("F_UEBERLAUF");
                    }
                }
                else
                {
                    f_UEBERLAUFField = value;
                    OnPropertyChanged("F_UEBERLAUF");
                }
            }
        }

        public byte[] F_HEX_CODE
        {
            get
            {
                return f_HEX_CODEField;
            }

            set
            {
                if (f_HEX_CODEField != null)
                {
                    if (!f_HEX_CODEField.Equals(value))
                    {
                        f_HEX_CODEField = value;
                        OnPropertyChanged("F_HEX_CODE");
                    }
                }
                else
                {
                    f_HEX_CODEField = value;
                    OnPropertyChanged("F_HEX_CODE");
                }
            }
        }

        public bool? Relevance
        {
            get
            {
                return relevanceField;
            }

            set
            {
                if (relevanceField.HasValue)
                {
                    if (!relevanceField.Equals(value))
                    {
                        relevanceField = value;
                        OnPropertyChanged("Relevance");
                    }
                }
                else
                {
                    relevanceField = value;
                    OnPropertyChanged("Relevance");
                }
            }
        }

        public ushort? F_PCODE
        {
            get
            {
                return f_PCODEField;
            }

            set
            {
                if (f_PCODEField.HasValue)
                {
                    if (!f_PCODEField.Equals(value))
                    {
                        f_PCODEField = value;
                        OnPropertyChanged("F_PCODE");
                    }
                }
                else
                {
                    f_PCODEField = value;
                    OnPropertyChanged("F_PCODE");
                }
            }
        }

        public string F_PCODE_STRING
        {
            get
            {
                return f_PCODE_STRINGField;
            }

            set
            {
                if (f_PCODE_STRINGField != null)
                {
                    if (!f_PCODE_STRINGField.Equals(value))
                    {
                        f_PCODE_STRINGField = value;
                        OnPropertyChanged("F_PCODE_STRING");
                    }
                }
                else
                {
                    f_PCODE_STRINGField = value;
                    OnPropertyChanged("F_PCODE_STRING");
                }
            }
        }

        public string F_PCODE_TEXT
        {
            get
            {
                return f_PCODE_TEXTField;
            }

            set
            {
                if (f_PCODE_TEXTField != null)
                {
                    if (!f_PCODE_TEXTField.Equals(value))
                    {
                        f_PCODE_TEXTField = value;
                        OnPropertyChanged("F_PCODE_TEXT");
                    }
                }
                else
                {
                    f_PCODE_TEXTField = value;
                    OnPropertyChanged("F_PCODE_TEXT");
                }
            }
        }

        public uint? F_SAE_CODE
        {
            get
            {
                return f_SAE_CODEField;
            }

            set
            {
                if (f_SAE_CODEField.HasValue)
                {
                    if (!f_SAE_CODEField.Equals(value))
                    {
                        f_SAE_CODEField = value;
                        OnPropertyChanged("F_SAE_CODE");
                    }
                }
                else
                {
                    f_SAE_CODEField = value;
                    OnPropertyChanged("F_SAE_CODE");
                }
            }
        }

        public string F_SAE_CODE_STRING
        {
            get
            {
                return f_SAE_CODE_STRINGField;
            }

            set
            {
                if (f_SAE_CODE_STRINGField != null)
                {
                    if (!f_SAE_CODE_STRINGField.Equals(value))
                    {
                        f_SAE_CODE_STRINGField = value;
                        OnPropertyChanged("F_SAE_CODE_STRING");
                    }
                }
                else
                {
                    f_SAE_CODE_STRINGField = value;
                    OnPropertyChanged("F_SAE_CODE_STRING");
                }
            }
        }

        public string F_SAE_CODE_TEXT
        {
            get
            {
                return f_SAE_CODE_TEXTField;
            }

            set
            {
                if (f_SAE_CODE_TEXTField != null)
                {
                    if (!f_SAE_CODE_TEXTField.Equals(value))
                    {
                        f_SAE_CODE_TEXTField = value;
                        OnPropertyChanged("F_SAE_CODE_TEXT");
                    }
                }
                else
                {
                    f_SAE_CODE_TEXTField = value;
                    OnPropertyChanged("F_SAE_CODE_TEXT");
                }
            }
        }

        public long? F_LZ
        {
            get
            {
                return f_LZField;
            }

            set
            {
                if (f_LZField.HasValue)
                {
                    if (!f_LZField.Equals(value))
                    {
                        f_LZField = value;
                        OnPropertyChanged("F_LZ");
                    }
                }
                else
                {
                    f_LZField = value;
                    OnPropertyChanged("F_LZ");
                }
            }
        }

        public string F_CODE
        {
            get
            {
                return f_CODEField;
            }

            set
            {
                if (f_CODEField != null)
                {
                    if (!f_CODEField.Equals(value))
                    {
                        f_CODEField = value;
                        OnPropertyChanged("F_CODE");
                    }
                }
                else
                {
                    f_CODEField = value;
                    OnPropertyChanged("F_CODE");
                }
            }
        }

        public decimal? Id
        {
            get
            {
                return idField;
            }

            set
            {
                if (idField.HasValue)
                {
                    if (!idField.Equals(value))
                    {
                        idField = value;
                        OnPropertyChanged("Id");
                    }
                }
                else
                {
                    idField = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        [DefaultValue(false)]
        public bool FS_LESEN_SUCCESSFULLY
        {
            get
            {
                return fS_LESEN_SUCCESSFULLYField;
            }

            set
            {
                if (!fS_LESEN_SUCCESSFULLYField.Equals(value))
                {
                    fS_LESEN_SUCCESSFULLYField = value;
                    OnPropertyChanged("FS_LESEN_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool FS_LESEN_DETAIL_SUCCESSFULLY
        {
            get
            {
                return fS_LESEN_DETAIL_SUCCESSFULLYField;
            }

            set
            {
                if (!fS_LESEN_DETAIL_SUCCESSFULLYField.Equals(value))
                {
                    fS_LESEN_DETAIL_SUCCESSFULLYField = value;
                    OnPropertyChanged("FS_LESEN_DETAIL_SUCCESSFULLY");
                }
            }
        }

        [DefaultValue(false)]
        public bool IsVirtual
        {
            get
            {
                return isVirtualField;
            }

            set
            {
                if (!isVirtualField.Equals(value))
                {
                    isVirtualField = value;
                    OnPropertyChanged("IsVirtual");
                }
            }
        }

        [DefaultValue(false)]
        public bool IsCombined
        {
            get
            {
                return isCombinedField;
            }

            set
            {
                if (!isCombinedField.Equals(value))
                {
                    isCombinedField = value;
                    OnPropertyChanged("IsCombined");
                }
            }
        }

        [DefaultValue("F")]
        public string EcuDTCType
        {
            get
            {
                return ecuDTCTypeField;
            }

            set
            {
                if (ecuDTCTypeField != null)
                {
                    if (!ecuDTCTypeField.Equals(value))
                    {
                        ecuDTCTypeField = value;
                        OnPropertyChanged("EcuDTCType");
                    }
                }
                else
                {
                    ecuDTCTypeField = value;
                    OnPropertyChanged("EcuDTCType");
                }
            }
        }

        public ObservableCollection<DTC> CombinedFaultRelatedDtcs
        {
            get
            {
                return combinedFaultsRelatedDtcs;
            }

            set
            {
                if (combinedFaultsRelatedDtcs != value)
                {
                    combinedFaultsRelatedDtcs = value;
                    OnPropertyChanged("CombinedFaultRelatedDtcs");
                }
            }
        }

        public decimal FcId
        {
            get
            {
                return fcId;
            }

            set
            {
                _ = fcId;
                if (!fcId.Equals(value))
                {
                    fcId = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public object Clone()
        {
            return MemberwiseClone();
        }

        [PreserveSource(Hint = "XEP_VIRTUALFAULTCODES", SignatureModified = true)]
        public static DTC GetDTCByFaultCode(PlaceholderType vFaultCode)
        {
            //[-] if (vFaultCode == null)
            //[-] {
            //[-] Log.Warning("DTC.GetDTCByFaultCode(XEP_VIRTUALFAULTCODES)", "vFaultCode was null");
            //[-] return null;
            //[-] }
            DTC dTC = new DTC();
            dTC.IsVirtual = true;
            //[-] dTC.Id = vFaultCode.ID;
            //[-] dTC.SetVirtualDtcFaultClassAndGroup(vFaultCode.ECUNOANSWER);
            try
            {
            //[-] string value = vFaultCode.CODE.TrimStart('S', ' ');
            //[-] dTC.F_ORT = Convert.ToInt64(value, 16);
            }
            catch (Exception exception)
            {
                Log.WarningException("DTC.GetDTCByFaultCode(XEP_VIRTUALFAULTCODES)", exception);
                return null;
            }

            dTC.FS_LESEN_SUCCESSFULLY = true;
            dTC.F_HEX_CODE = new byte[0];
            return dTC;
        }

        public override int GetHashCode()
        {
            if (Id.HasValue)
            {
                return Id.GetHashCode();
            }

            if (F_ORT.HasValue)
            {
                return F_ORT.GetHashCode();
            }

            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is DTC)
            {
                DTC dTC = (DTC)obj;
                if (Id.HasValue)
                {
                    if (Id == dTC.Id)
                    {
                        return true;
                    }

                    return false;
                }

                if (F_ORT.HasValue)
                {
                    if (F_ORT == dTC.F_ORT)
                    {
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public void SetCombinedDtcFaultClass(bool newFaultMemory)
        {
            if (newFaultMemory && CombinedFaultRelatedDtcs.All((DTC d) => d.FaultClass == 1))
            {
                FaultClass = 1;
            }
            else
            {
                FaultClass = 7;
            }
        }

        private void SetVirtualDtcFaultClassAndGroup(decimal? ecuNoAnswer)
        {
            if (!ecuNoAnswer.HasValue)
            {
                return;
            }

            decimal value = ecuNoAnswer.Value;
            if (!(value == 0m))
            {
                if (!(value == 1m))
                {
                    if (value == 2m)
                    {
                        FaultClass = 4;
                    }
                }
                else
                {
                    FaultClass = 9;
                }
            }
            else
            {
                FaultClass = 7;
            }
        }

        private void AddFaultGroupClass()
        {
            if (F_FEHLERKLASSE_NR.HasValue && F_FEHLERKLASSE_NR != 0 && !IsVirtual && !IsCombined)
            {
                int? num = ConvertToFaultClass(F_FEHLERKLASSE_NR);
                if (num.HasValue)
                {
                    FaultClass = num.Value;
                }
            }
        }

        private int? ConvertToFaultClass(int? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            int? result = null;
            string text = value.Value.ToString("X8");
            string text2 = text.Substring(2, 2);
            string text3 = text.Substring(4, 2);
            string text4 = text.Substring(6, 1);
            if (text2 != "00")
            {
                switch (text2)
                {
                    case "08":
                        result = 14;
                        break;
                    case "04":
                        result = 13;
                        break;
                    case "02":
                        result = 12;
                        break;
                    case "01":
                        result = 16;
                        break;
                }
            }

            if (!result.HasValue && text3 != "00")
            {
                switch (text3)
                {
                    case "80":
                        result = 15;
                        break;
                    case "40":
                        result = 11;
                        break;
                    case "20":
                        result = 10;
                        break;
                    case "10":
                        result = 9;
                        break;
                    case "08":
                        result = 1;
                        break;
                    case "04":
                        result = 6;
                        break;
                    case "02":
                        result = 5;
                        break;
                    case "01":
                        result = 8;
                        break;
                }
            }

            if (!result.HasValue && text4 != "0")
            {
                switch (text4)
                {
                    case "8":
                        result = 7;
                        break;
                    case "4":
                        result = 4;
                        break;
                    case "2":
                        result = 3;
                        break;
                    case "1":
                        result = 2;
                        break;
                }
            }

            return result;
        }

        private void MapFaultClassToFaultGroup()
        {
            switch (FaultClass)
            {
                case 1:
                    FaultGroup = 1;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    FaultGroup = 2;
                    break;
                case 7:
                case 8:
                    FaultGroup = 3;
                    break;
                case 9:
                case 10:
                    FaultGroup = 4;
                    break;
                case 12:
                case 13:
                case 14:
                    FaultGroup = 5;
                    break;
                case 11:
                case 15:
                case 16:
                    FaultGroup = 6;
                    break;
                default:
                    FaultGroup = 0;
                    break;
            }
        }

        public DTC()
        {
            UniqueId = Guid.NewGuid();
            dTCContextField = new ObservableCollection<typeDTCContext>();
            f_ART_EXTField = new ObservableCollection<typeFArtExt>();
            SAEContextField = new ObservableCollection<typeSAEContext>();
            combinedFaultsRelatedDtcs = new ObservableCollection<DTC>();
            relevanceField = true;
            fS_LESEN_SUCCESSFULLYField = false;
            fS_LESEN_DETAIL_SUCCESSFULLYField = false;
            isVirtualField = false;
            isCombinedField = false;
            ecuDTCTypeField = "F";
            if (showWarningLights)
            {
                StateWarningLight = rnd.Next(0, 6);
            }
        }

        private void DTCContextField_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            foreach (typeDTCContext newItem in e.NewItems)
            {
                typeDTCContext typeDTCContext2 = dTCContextField.LastOrDefault((typeDTCContext cf) => cf.UniqueId == newItem.UniqueId && cf != newItem);
                if (typeDTCContext2 != null)
                {
                    dTCContextField.Remove(typeDTCContext2);
                }
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}