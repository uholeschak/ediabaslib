using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core.Container;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using BMW.Rheingold.Programming.Common;
using PsdzClient.Programming;

#pragma warning disable CS0169, CS0649, CS0618
namespace PsdzClient.Core
{
    public class Vehicle : typeVehicle, IVehicle, INotifyPropertyChanged, IVehicleRuleEvaluation, IVinValidatorVehicle, IIdentVehicle, IReactorVehicle
    {
        public const string BnProgramming = "BN2020,BN2020_MOTORBIKE";
        [PreserveSource(Hint = "ObservableCollectionEx<Fault>", Placeholder = true)]
        private readonly PlaceholderType pKodeList;
        private readonly ParameterContainer sessionDataStore;
        private string vinRangeType;
        private string vinRangeTypeLastResolvedType;
        private FA targetFA;
        private bool isBusy;
        private string productLine;
        private string doorNumber;
        private string securityRelevant;
        private DateTime? cDatetimeByModelYearMonth;
        private HashSet<int> validPWFStates;
        private double clamp15MinValue;
        private double clamp30MinValue;
        private bool withLfpBattery;
        private bool withLfpNCarBattery;
        [PreserveSource(Hint = "Database modified")]
        private BatteryEnum batteryType;
        private bool isClosingOperationActive;
        private string verkaufsBezeichnungField;
        private bool powerSafeModeByOldEcus;
        private bool powerSafeModeByNewEcus;
        private bool vehicleTestDone;
        private bool isReadingFastaDataFinished = true;
        private bool vinNotReadbleFromCarAbort;
        private int? faultCodeSum;
        private int? nonSignalErrorFaultCodeSum;
        private string targetILevel;
        private readonly ObservableCollection<string> diagCodesProgramming;
        [PreserveSource(Hint = "IList<Fault>", Placeholder = true)]
        private PlaceholderType faultList;
        [PreserveSource(Hint = "ObservableCollection<CheckControlMessage>", Placeholder = true)]
        private PlaceholderType checkControlMessages;
        private bool noVehicleCommunicationRunning;
        private string salesDesignationBadgeUIText;
        private string eBezeichnungUIText;
        private const int indexOfFirsHDDAboUpdateInDecimal = 54;
        private bool isNewIdentActiveField;
        [PreserveSource(Hint = "BlockingCollection<VirtualFaultInfo>", Placeholder = true)]
        private PlaceholderType virtualFaultInfoList;
        private string hmiVersion;
        private string kraftstoffartEinbaulage;
        private string baustand;
        private string typeKey;
        private string typeKeyLead;
        private string typeKeyBasic;
        private string eSeriesLifeCycle;
        private string lifeCycle;
        private string sportausfuehrung;
        [PreserveSource(Hint = "Database modified")]
        private PsdzDatabase.BordnetsData bordnetsData;
        private VehicleClassification classification;
        private IVehicleProfileChecksum vpc;
        [XmlIgnore]
        public List<IEcu> SvtECU { get; set; } = new List<IEcu>();

        [XmlIgnore]
        public bool IsDoIP { get; set; }

        [XmlIgnore]
        public DateTime? LastProgramDate { get; set; }

        [PreserveSource(Hint = "Database modified")]
        [XmlIgnore]
        public PsdzDatabase.BordnetsData BordnetsData
        {
            get
            {
                return bordnetsData;
            }

            set
            {
                if (bordnetsData != value)
                {
                    bordnetsData = value;
                    OnPropertyChanged("BordnetsData");
                }
            }
        }

        public string VerkaufsBezeichnung
        {
            get
            {
                return verkaufsBezeichnungField;
            }

            set
            {
                if (verkaufsBezeichnungField != value)
                {
                    verkaufsBezeichnungField = value;
                    OnPropertyChanged("VerkaufsBezeichnung");
                    SalesDesignationBadgeUIText = value;
                }
            }
        }

        [XmlIgnore]
        public bool IsEcuIdentSuccessfull { get; set; }

        public string HmiVersion
        {
            get
            {
                return hmiVersion;
            }

            set
            {
                hmiVersion = value;
                OnPropertyChanged("HmiVersion");
            }
        }

        public string EBezeichnungUIText
        {
            get
            {
                return eBezeichnungUIText;
            }

            set
            {
                eBezeichnungUIText = value;
                OnPropertyChanged("EBezeichnungUIText");
            }
        }

        [XmlIgnore]
        public string SalesDesignationBadgeUIText
        {
            get
            {
                return salesDesignationBadgeUIText;
            }

            set
            {
                salesDesignationBadgeUIText = value;
                OnPropertyChanged("SalesDesignationBadgeUIText");
            }
        }

        public string KraftstoffartEinbaulage
        {
            get
            {
                return kraftstoffartEinbaulage;
            }

            set
            {
                if (kraftstoffartEinbaulage != value)
                {
                    kraftstoffartEinbaulage = value;
                    OnPropertyChanged("KraftstoffartEinbaulage");
                }
            }
        }

        public ObservableCollection<string> DiagCodesProgramming => diagCodesProgramming;

        [PreserveSource(Hint = "RxSwinData", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType RxSwin { get; set; }

        [PreserveSource(Hint = "List<IRxSwinObject>", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType RxSwinObjectList { get; set; }

        [XmlIgnore]
        public FA TargetFA
        {
            get
            {
                return targetFA;
            }

            set
            {
                targetFA = value;
            }
        }

        [XmlIgnore]
        public string TargetILevel
        {
            get
            {
                return targetILevel;
            }

            set
            {
                targetILevel = value;
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public string SerialGearBox7
        {
            get
            {
                if (!string.IsNullOrEmpty(base.SerialGearBox) && base.SerialGearBox.Length >= 7)
                {
                    return base.SerialGearBox.Substring(0, 7);
                }

                return base.SerialGearBox;
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public string DisplayGwsz => base.Gwsz.ToMileageDisplayFormat(Classification.IsNewFaultMemoryActive);

        [XmlIgnore]
        public string VINRangeType
        {
            get
            {
                return vinRangeType;
            }

            set
            {
                if (vinRangeType != value)
                {
                    vinRangeType = value;
                    OnPropertyChanged("VINRangeType");
                }
            }
        }

        [XmlIgnore]
        public bool IsClosingOperationActive
        {
            get
            {
                return isClosingOperationActive;
            }

            set
            {
                isClosingOperationActive = value;
            }
        }

        [XmlIgnore]
        public ParameterContainer SessionDataStore => sessionDataStore;

        public string VIN10Prefix
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(base.VIN17))
                    {
                        return null;
                    }

                    return base.VIN17.Substring(0, 10);
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.VIN10Prefix", exception);
                    return null;
                }
            }
        }

        public string BasisEReihe
        {
            get
            {
                if (!string.IsNullOrEmpty(base.MainSeriesSgbd) && base.MainSeriesSgbd.Length >= 3 && !base.MainSeriesSgbd.Equals("zcs_all"))
                {
                    return base.MainSeriesSgbd.Substring(0, 3);
                }

                return base.Ereihe;
            }
        }

        public string VIN7
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(base.VIN17))
                    {
                        return null;
                    }

                    return base.VIN17.Substring(10, 7);
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.get_VIN7", exception);
                }

                return null;
            }
        }

        public string GMType
        {
            get
            {
                try
                {
                    if (base.FA != null && !string.IsNullOrEmpty(base.FA.TYPE) && base.FA.TYPE.Length == 4)
                    {
                        return base.FA.TYPE;
                    }

                    if (string.IsNullOrEmpty(base.VIN17))
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(VINRangeType))
                    {
                        return VINRangeType;
                    }

                    if (!string.IsNullOrEmpty(VINType))
                    {
                        string text = VINType.Substring(0, 3);
                        switch (VINType[3])
                        {
                            case 'A':
                                text += "1";
                                break;
                            case 'B':
                                text += "2";
                                break;
                            case 'C':
                                text += "3";
                                break;
                            case 'D':
                                text += "4";
                                break;
                            case 'E':
                                text += "5";
                                break;
                            case 'F':
                                text += "6";
                                break;
                            case 'G':
                                text += "7";
                                break;
                            case 'H':
                                text += "8";
                                break;
                            case 'J':
                                text += "9";
                                break;
                            default:
                                return VINType;
                        }

                        return text;
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.get_VINType", exception);
                }

                return null;
            }
        }

        public string VINType
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(base.VIN17) || base.VIN17.Length < 17)
                    {
                        return null;
                    }

                    return base.VIN17.Substring(3, 4);
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.get_VINType", exception);
                }

                return null;
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }

            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public string EMotBaureihe => base.EMotor.EMOTBaureihe;

        public string Produktlinie
        {
            get
            {
                return productLine;
            }

            set
            {
                if (productLine != value)
                {
                    productLine = value;
                    OnPropertyChanged("Produktlinie");
                }
            }
        }

        public string Sicherheitsrelevant
        {
            get
            {
                return securityRelevant;
            }

            set
            {
                if (securityRelevant != value)
                {
                    securityRelevant = value;
                    OnPropertyChanged("Sicherheitsrelevant");
                }
            }
        }

        public string Tueren
        {
            get
            {
                return doorNumber;
            }

            set
            {
                if (doorNumber != value)
                {
                    doorNumber = value;
                    OnPropertyChanged("Tueren");
                }
            }
        }

        [XmlIgnore]
        public List<string> SxCodes { get; set; } = new List<string>();

        [XmlIgnore]
        public string TypeKey
        {
            get
            {
                return typeKey;
            }

            set
            {
                if (typeKey != value)
                {
                    typeKey = value;
                    OnPropertyChanged("TypeKey");
                }
            }
        }

        [XmlIgnore]
        public string TypeKeyLead
        {
            get
            {
                return typeKeyLead;
            }

            set
            {
                if (typeKeyLead != value)
                {
                    typeKeyLead = value;
                    OnPropertyChanged("TypeKeyLead");
                }
            }
        }

        public string TypeKeyBasic
        {
            get
            {
                return typeKeyBasic;
            }

            set
            {
                if (typeKeyBasic != value)
                {
                    typeKeyBasic = value;
                    OnPropertyChanged("TypeKeyBasic");
                }
            }
        }

        public string ESeriesLifeCycle
        {
            get
            {
                return eSeriesLifeCycle;
            }

            set
            {
                if (eSeriesLifeCycle != value)
                {
                    eSeriesLifeCycle = value;
                    OnPropertyChanged("ESeriesLifeCycle");
                }
            }
        }

        public string LifeCycle
        {
            get
            {
                return lifeCycle;
            }

            set
            {
                if (lifeCycle != value)
                {
                    lifeCycle = value;
                    OnPropertyChanged("LifeCycle");
                }
            }
        }

        public string Sportausfuehrung
        {
            get
            {
                return sportausfuehrung;
            }

            set
            {
                if (sportausfuehrung != value)
                {
                    sportausfuehrung = value;
                    OnPropertyChanged("Sportausfuehrung");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<CheckControlMessage>", Placeholder = true)]
        public PlaceholderType CheckControlMessages;
        [XmlIgnore]
        public bool IsCcmReadoutDone { get; set; }

        [PreserveSource(Hint = "IList<Fault>", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType FaultList;
        [PreserveSource(Hint = "BlockingCollection<VirtualFaultInfo>", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType VirtualFaultInfoList;
        [PreserveSource(Hint = "ObservableCollectionEx<Fault>", Placeholder = true)]
        public PlaceholderType PKodeList => pKodeList;

        [XmlIgnore]
        public bool IsFastaReadDone { get; set; }

        [XmlIgnore]
        public bool IsProgrammingSessionStartable { get; set; }

        [XmlIgnore]
        public bool IsVehicleTestDone
        {
            get
            {
                return vehicleTestDone;
            }

            set
            {
                if (vehicleTestDone != value)
                {
                    vehicleTestDone = value;
                    OnPropertyChanged("IsVehicleTestDone");
                }
            }
        }

        public bool IsReadingFastaDataFinished
        {
            get
            {
                return isReadingFastaDataFinished;
            }

            set
            {
                isReadingFastaDataFinished = value;
                OnPropertyChanged("IsReadingFastaDataFinished");
            }
        }

        public bool IsNewIdentActive
        {
            get
            {
                return isNewIdentActiveField;
            }

            set
            {
                isNewIdentActiveField = value;
                OnPropertyChanged("IsNewIdentActive");
            }
        }

        [XmlIgnore]
        public bool IsVehicleBreakdownAlreadyShown { get; set; }

        [XmlIgnore]
        public bool IsPowerSafeModeActive
        {
            get
            {
                if (!powerSafeModeByOldEcus)
                {
                    return powerSafeModeByNewEcus;
                }

                return true;
            }
        }

        [XmlIgnore]
        public bool IsPowerSafeModeActiveByOldEcus
        {
            get
            {
                return powerSafeModeByOldEcus;
            }

            set
            {
                Log.Info("Vehicle.IsPowerSafeModeActiveByOldEcus_set", "Setting vehicle power safe modus from \"{0}\" to \"{1}\".", powerSafeModeByOldEcus, value);
                powerSafeModeByOldEcus = value;
            }
        }

        [XmlIgnore]
        public bool VinNotReadbleFromCarAbort
        {
            get
            {
                return vinNotReadbleFromCarAbort;
            }

            set
            {
                vinNotReadbleFromCarAbort = value;
            }
        }

        [XmlIgnore]
        public bool IsPowerSafeModeActiveByNewEcus
        {
            get
            {
                return powerSafeModeByNewEcus;
            }

            set
            {
                Log.Info("Vehicle.IsPowerSafeModeActiveByNewEcus_set", "Setting vehicle power safe modus from \"{0}\" to \"{1}\".", powerSafeModeByNewEcus, value);
                powerSafeModeByNewEcus = value;
            }
        }

        [XmlIgnore]
        public int? FaultCodeSum
        {
            get
            {
                return faultCodeSum;
            }

            set
            {
                faultCodeSum = value;
                OnPropertyChanged("FaultCodeSum");
            }
        }

        [XmlIgnore]
        public int? NonSignalErrorFaultCodeSum
        {
            get
            {
                return nonSignalErrorFaultCodeSum;
            }

            set
            {
                nonSignalErrorFaultCodeSum = value;
                OnPropertyChanged("NonSignalErrorFaultCodeSum");
            }
        }

        [XmlIgnore]
        public DateTime? C_DATETIME
        {
            get
            {
                try
                {
                    if (base.FA != null && base.FA.C_DATETIME.HasValue && base.FA.C_DATETIME > DateTime.MinValue)
                    {
                        return base.FA.C_DATETIME;
                    }

                    if (!string.IsNullOrEmpty(base.Modelljahr) && !string.IsNullOrEmpty(base.Modellmonat))
                    {
                        if (!cDatetimeByModelYearMonth.HasValue)
                        {
                            cDatetimeByModelYearMonth = DateTime.Parse(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-01", base.Modelljahr, base.Modellmonat), CultureInfo.InvariantCulture);
                        }

                        return cDatetimeByModelYearMonth;
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.get_C_DATETIME()", exception);
                }

                return null;
            }
        }

        [PreserveSource(Hint = "IEnumerable<ICbsInfo>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.CBS => base.CBS;

        [PreserveSource(Hint = "IEnumerable<IDtc>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.CombinedFaults => base.CombinedFaults;

        [PreserveSource(Hint = "IEnumerable<IDiagCode>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.DiagCodes => base.DiagCodes;

        [XmlIgnore]
        IEnumerable<IEcu> IVehicle.ECU => base.ECU;

        [XmlIgnore]
        BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa IVehicle.FA => base.FA;

        [XmlIgnore]
        IEnumerable<IFfmResult> IVehicle.FFM => base.FFM;

        [XmlIgnore]
        IEnumerable<decimal> IVehicle.InstalledAdapters => base.InstalledAdapters;

        [XmlIgnore]
        IEcu IVehicle.SelectedECU => base.SelectedECU;

        [XmlIgnore]
        IVciDevice IVehicle.MIB => base.MIB;

        [PreserveSource(Hint = "IEnumerable<IServiceHistoryEntry>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.ServiceHistory => base.ServiceHistory;

        [PreserveSource(Hint = "IEnumerable<ITechnicalCampaign>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.TechnicalCampaigns => base.TechnicalCampaigns;

        [XmlIgnore]
        IVciDevice IVehicle.VCI => base.VCI;

        [PreserveSource(Hint = "IEnumerable<IZfsResult>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.ZFS => base.ZFS;

        [XmlIgnore]
        public double Clamp15MinValue
        {
            get
            {
                return clamp15MinValue;
            }

            set
            {
                if (clamp15MinValue != value)
                {
                    clamp15MinValue = value;
                    OnPropertyChanged("Clamp15MinValue");
                }
            }
        }

        public bool WithLfpBattery
        {
            get
            {
                return withLfpBattery;
            }

            set
            {
                if (withLfpBattery != value)
                {
                    withLfpBattery = value;
                    OnPropertyChanged("WithLfpBattery");
                }
            }
        }

        public bool WithLfpNCarBattery
        {
            get
            {
                return withLfpNCarBattery;
            }

            set
            {
                if (withLfpNCarBattery != value)
                {
                    withLfpNCarBattery = value;
                    OnPropertyChanged("WithLfpNCarBattery");
                }
            }
        }

        [XmlIgnore]
        public TransmissionDataType TransmissionDataType { get; private set; } = new TransmissionDataType();

        [XmlIgnore]
        public BatteryEnum BatteryType
        {
            get
            {
                return batteryType;
            }

            set
            {
                if (batteryType != value)
                {
                    batteryType = value;
                    OnPropertyChanged("BatteryType");
                }
            }
        }

        [XmlIgnore]
        public double Clamp30MinValue
        {
            get
            {
                return clamp30MinValue;
            }

            set
            {
                if (clamp30MinValue != value)
                {
                    clamp30MinValue = value;
                    OnPropertyChanged("Clamp30MinValue");
                }
            }
        }

        [XmlIgnore]
        public HashSet<int> ValidPWFStates
        {
            get
            {
                return validPWFStates;
            }

            set
            {
                if (validPWFStates != value)
                {
                    validPWFStates = value;
                    OnPropertyChanged("ValidPWFStates");
                }
            }
        }

        [PreserveSource(Hint = "IList<EslDocumentIsta>", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType ESLDocuments { get; set; }

        [XmlIgnore]
        public string Baustand
        {
            get
            {
                return baustand;
            }

            set
            {
                if (baustand != value)
                {
                    baustand = value;
                    OnPropertyChanged("Baustand");
                }
            }
        }

        [XmlIgnore]
        public bool IsNoVehicleCommunicationRunning
        {
            get
            {
                return noVehicleCommunicationRunning;
            }

            set
            {
                noVehicleCommunicationRunning = value;
                OnPropertyChanged("IsNoVehicleCommunicationRunning");
            }
        }

        [XmlIgnore]
        IVciDeviceRuleEvaluation IVehicleRuleEvaluation.VCI => base.VCI;

        [XmlIgnore]
        IList<IIdentEcu> IVehicleRuleEvaluation.ECU => GetEcusAsIIdentEcu();

        [XmlIgnore]
        IFARuleEvaluation IVehicleRuleEvaluation.FA => base.FA;

        [XmlIgnore]
        IFARuleEvaluation IVehicleRuleEvaluation.TargetFA => TargetFA;

        [Obsolete("Is not used anymore in Testmodules. Will be removed in 4.48!")]
        [XmlIgnore]
        public BNMixed BNMixed { get; set; }

        [XmlIgnore]
        IReactorFa IReactorVehicle.FA
        {
            get
            {
                return base.FA;
            }

            set
            {
                if (base.FA != value)
                {
                    base.FA = (FA)value;
                }
            }
        }

        [XmlIgnore]
        public BordnetType BordnetType
        {
            get
            {
                return (BordnetType)base.BNType;
            }

            set
            {
                base.BNType = (BNType)value;
            }
        }

        [XmlIgnore]
        public VehicleClassification Classification
        {
            get
            {
                return classification;
            }

            set
            {
                if (classification != value)
                {
                    classification = value;
                    OnPropertyChanged("Classification");
                }
            }
        }

        [XmlIgnore]
        IVehicleClassification IVehicle.Classification
        {
            get
            {
                return Classification;
            }

            set
            {
                Classification = (VehicleClassification)value;
            }
        }

        [XmlIgnore]
        public IVehicleProfileChecksum VPC
        {
            get
            {
                return vpc;
            }

            set
            {
                vpc = value;
            }
        }

        [XmlIgnore]
        public string VehicleModelRecognition { get; set; }
        public string TempTypeKeyLeadFromDb { get; set; }
        public string TempTypeKeyBasicFromFbm { get; set; }

        [PreserveSource(Hint = "clientContext added", OriginalHash = "8EBE51BFBEB5058EAFC36EF3F3322DCF")]
        public Vehicle(ClientContext clientContext) : base(clientContext)
        {
            base.ConnectState = VisibilityType.Collapsed;
            //[-]  pKodeList = new ObservableCollectionEx<Fault>();
            //[-] FaultList = new List<Fault>();
            //[-] VirtualFaultInfoList = new BlockingCollection<VirtualFaultInfo>();
            sessionDataStore = new ParameterContainer();
            //[-] base.Testplan = new TestPlanType(this);
            diagCodesProgramming = new ObservableCollection<string>();
            IsClosingOperationActive = false;
            validPWFStates = new HashSet<int>(new int[17] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            clamp15MinValue = ConfigSettings.GetConfigDouble("BMW.Rheingold.ISTAGUI.Clamp15MinVoltage", 0.0);
            clamp30MinValue = new VoltageThreshold(BatteryEnum.Pb).MinError;
            //[-] RxSwin = new RxSwinData();
            //[-] checkControlMessages = new ObservableCollection<CheckControlMessage>();
            Classification = new VehicleClassification(this);
            Reactor = new Reactor(this, new NugetLogger(), new DataHolder());   // [UH] [IGNORE] Reactor init
        }

        [PreserveSource(Hint = "Cleaned")]
        public List<string> PermanentSAEFehlercodesInFaultList()
        {
            List<string> list = new List<string>();
            return list;
        }

        [PreserveSource(Hint = "Database modified", OriginalHash = "991CED817BF1E4F638AF985457212396")]
        public string SetVINRangeTypeFromVINRanges()
        {
            PsdzDatabase database = ClientContext.GetDatabase(this);
            if (database != null && !"XXXXXXX".Equals(VIN7) && !string.IsNullOrEmpty(VIN7) && !VIN7.Equals(vinRangeTypeLastResolvedType, StringComparison.OrdinalIgnoreCase))
            {
                PsdzDatabase.VinRanges vinRangesByVin = database.GetVinRangesByVin17(VINType, VIN7, returnFirstEntryWithoutCheck: false, IsVehicleWithOnlyVin7());
                if (vinRangesByVin != null)
                {
                    vinRangeTypeLastResolvedType = VIN7;
                    return vinRangesByVin.TypeKey;
                }
            }

            return null;
        }

        [PreserveSource(Hint = "Cleaned")]
        public PlaceholderType GetEnrichedFaultList(IFFMDynamicResolver ffmDynamicResolver)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        public PlaceholderType ComputeResolveLabelsForAllFaultAsync(Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        private PlaceholderType GetXepFaultModelLabelsByDtcFOrtEcuVariantAsync()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        private PlaceholderType GetXepFaultLabelsByDtcFOrtEcuVariantAsync()
        {
            return PlaceholderType.Value;
        }

        public string GetFSCfromUpdateIndex(string updateIndex, string huVariante)
        {
            string[] source = new string[2]
            {
                "HU_MGU",
                "ENAVEVO"
            };
            try
            {
                int num = Convert.ToInt32(updateIndex, 16);
                if (source.Any((string x) => huVariante.Equals(x)))
                {
                    string text = updateIndex.Substring(0, 2);
                    return updateIndex.Substring(2, 2) + "-" + text;
                }

                if (num > 45)
                {
                    int months = num - 54;
                    DateTime dateTime = new DateTime(2018, 7, 1).AddMonths(months);
                    new DateTime(2017, 10, 1);
                    return dateTime.Month + "-" + dateTime.Year;
                }

                if (num > 33)
                {
                    int num2 = 46 - num;
                    int months2 = -1 * (num2 * 3 - 3);
                    DateTime dateTime2 = new DateTime(2017, 10, 1).AddMonths(months2);
                    return dateTime2.Month + "-" + dateTime2.Year;
                }

                return "-";
            }
            catch
            {
                Log.Warning("Vehicle.ValidateFSC", "Exception Occurred validating HDDUpdateIndex {0}", updateIndex);
                return "-";
            }
        }

        public static Vehicle Deserialize(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    Log.Warning(Log.CurrentMethod() + "()", "file doesn't exist: {0}", filename);
                    return null;
                }

                using (FileStream input = File.OpenRead(filename))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(input))
                    {
                        Vehicle obj = (Vehicle)new XmlSerializer(typeof(Vehicle)).Deserialize(xmlReader);
                        obj.CalculateFaultProperties();
                        return obj;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod() + "()", exception);
            }

            return null;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Vehicle DeepClone()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Vehicle));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memoryStream, this);
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    Vehicle obj = (Vehicle)xmlSerializer.Deserialize(memoryStream);
                    obj.CalculateFaultProperties();
                    return obj;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
                Log.Info(Log.CurrentMethod(), "Trying reflection based fallback.");
                try
                {
                    return DeepCloneUtility.DeepClone(this);
                }
                catch (Exception exception2)
                {
                    Log.WarningException(Log.CurrentMethod(), exception2);
                    throw;
                }
            }
        }

        public bool IsVINLessEReihe()
        {
            string ereihe = base.Ereihe;
            if (ereihe != null)
            {
                int length = ereihe.Length;
                if (length != 3)
                {
                    if (length == 4)
                    {
                        char c = ereihe[3];
                        if ((uint)c <= 67u)
                        {
                            if (c != '9')
                            {
                                if (c != 'C' || !(ereihe == "259C"))
                                {
                                    goto IL_01b5;
                                }
                            }
                            else
                            {
                                switch (ereihe)
                                {
                                    case "K569":
                                    case "K589":
                                    case "K599":
                                    case "E169":
                                    case "E189":
                                        break;
                                    default:
                                        goto IL_01b5;
                                }
                            }

                            goto IL_01b3;
                        }

                        if (c != 'E')
                        {
                            if (c != 'R')
                            {
                                if (c == 'S' && ereihe == "259S")
                                {
                                    goto IL_01b3;
                                }
                            }
                            else if (ereihe == "259R")
                            {
                                goto IL_01b3;
                            }
                        }
                        else if (ereihe == "247E")
                        {
                            goto IL_01b3;
                        }
                    }
                }
                else
                {
                    switch (ereihe[2])
                    {
                        case '1':
                            break;
                        case '2':
                            goto IL_00c3;
                        case '8':
                            goto IL_00d8;
                        case '7':
                            goto IL_00fd;
                        case '9':
                            goto IL_0112;
                        case '0':
                            goto IL_0127;
                        default:
                            goto IL_01b5;
                    }

                    if (ereihe == "K41" || ereihe == "R21")
                    {
                        goto IL_01b3;
                    }
                }
            }

            goto IL_01b5;
            IL_00fd:
                if (ereihe == "247")
                {
                    goto IL_01b3;
                }

            goto IL_01b5;
            IL_0127:
                if (ereihe == "K30")
                {
                    goto IL_01b3;
                }

            goto IL_01b5;
            IL_0112:
                if (ereihe == "259")
                {
                    goto IL_01b3;
                }

            goto IL_01b5;
            IL_01b3:
                return true;
            IL_00c3:
                if (ereihe == "R22")
                {
                    goto IL_01b3;
                }

            goto IL_01b5;
            IL_00d8:
                if (ereihe == "R28" || ereihe == "248")
                {
                    goto IL_01b3;
                }

            goto IL_01b5;
            IL_01b5:
                return false;
        }

        public bool IsEreiheValid()
        {
            if (string.IsNullOrEmpty(base.Ereihe) || base.Ereihe == "UNBEK")
            {
                return false;
            }

            return true;
        }

        [PreserveSource(Hint = "Cleaned")]
        public ECU GetECUbyDTC(decimal id)
        {
            return null;
        }

        [PreserveSource(Hint = "Cleaned")]
        public PlaceholderType GetDTC(decimal id)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        public void CalculateFaultProperties(IFFMDynamicResolver ffmResolver = null)
        {
        }

        public typeECU_Transaction getECUTransaction(ECU transECU, string transId)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            if (transECU == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(transId))
            {
                return null;
            }

            try
            {
                if (transECU.TAL != null)
                {
                    foreach (typeECU_Transaction item in transECU.TAL)
                    {
                        if (string.Compare(item.transactionId, transId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECUTransaction()", exception);
            }

            return null;
        }

        public bool hasBusType(BusType bus)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            if (base.ECU != null)
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.BUS == bus)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasSA(string checkSA)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(checkSA))
            {
                Log.Warning("CoreFramework.hasSA()", "checkSA was null or empty");
                return false;
            }

            if (base.FA == null)
            {
                return false;
            }

            FA fA = ((targetFA != null) ? targetFA : base.FA);
            if (fA.SA != null)
            {
                foreach (string item in fA.SA)
                {
                    if (string.Compare(item, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }

                    if (item.Length == 4 && string.Compare(item.Substring(1), checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            if (fA.E_WORT != null)
            {
                foreach (string item2 in fA.E_WORT)
                {
                    if (string.Compare(item2, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            if (fA.HO_WORT != null)
            {
                foreach (string item3 in fA.HO_WORT)
                {
                    if (string.Compare(item3, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasUnidentifiedECU()
        {
            bool flag = false;
            if (base.ECU != null)
            {
                foreach (ECU item in base.ECU)
                {
                    if (string.IsNullOrEmpty(item.VARIANTE) || !item.COMMUNICATION_SUCCESSFULLY)
                    {
                        flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
                    }
                }

                return flag;
            }

            return true;
        }

        public bool? hasFFM(string checkFFM)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(checkFFM))
            {
                Log.Warning("CoreFramework.hasFFM()", "checkFFM was null or empty");
                return true;
            }

            if (base.FFM != null)
            {
                foreach (FFMResult item in base.FFM)
                {
                    if (string.Compare(item.Name, checkFFM, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return item.Result;
                    }
                }
            }

            return null;
        }

        public void AddOrUpdateFFM(IFfmResultRuleEvaluation ffm)
        {
            if (base.FFM == null || ffm == null)
            {
                return;
            }

            foreach (FFMResult item in base.FFM)
            {
                if (string.Compare(item.Name, ffm.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    item.ID = ffm.ID;
                    item.Evaluation = ffm.Evaluation;
                    item.ReEvaluationNeeded = ffm.ReEvaluationNeeded;
                    item.Result = ffm.Result;
                    return;
                }
            }

            base.FFM.Add(new FFMResult(ffm));
        }

        public ECU getECU(long? sgAdr)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR == sgAdr)
                    {
                        return item;
                    }

                    if (!string.IsNullOrEmpty(item.ECU_ADR))
                    {
                        string text = string.Empty;
                        if (item.ECU_ADR.Length >= 4 && item.ECU_ADR.Substring(0, 2).ToLower() == "0x")
                        {
                            text = item.ECU_ADR.ToUpper().Substring(2);
                        }

                        if (item.ECU_ADR.Length == 2)
                        {
                            text = item.ECU_ADR.ToUpper();
                        }

                        if (text == string.Format(CultureInfo.InvariantCulture, "{0:X2}", sgAdr))
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECU()", exception);
            }

            return null;
        }

        public ECU getECU(long? sgAdr, long? subAddress)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR == sgAdr && item.ID_LIN_SLAVE_ADR == subAddress)
                    {
                        return item;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehcile.getECU()", exception);
            }

            return null;
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public bool AddEcu(IEcu ecu)
        {
            return base.ECU.AddIfNotContains(ecu as ECU);
        }

        public bool RemoveEcu(IEcu ecu)
        {
            return base.ECU.Remove(ecu as ECU);
        }

        public IEcu getECUbyECU_SGBD(string ECU_SGBD)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed!!!");
            }

            if (string.IsNullOrEmpty(ECU_SGBD))
            {
                return null;
            }

            try
            {
                string[] array = ECU_SGBD.Split('|');
                foreach (string b in array)
                {
                    foreach (ECU item in base.ECU)
                    {
                        if (string.Equals(item.ECU_SGBD, b, StringComparison.OrdinalIgnoreCase) || string.Equals(item.VARIANTE, b, StringComparison.OrdinalIgnoreCase))
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECUbyECU_SGBD()", exception);
            }

            return null;
        }

        public IEcu getECUbyTITLE_ECUTREE(string grobName)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(grobName))
            {
                return null;
            }

            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (string.Compare(item.TITLE_ECUTREE, grobName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return item;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECUbyTITLE_ECUTREE()", exception);
            }

            return null;
        }

        public ECU getECUbyECU_GRUPPE(string ECU_GRUPPE)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(ECU_GRUPPE))
            {
                Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "parameter was null or empty");
                return null;
            }

            if (base.ECU == null)
            {
                Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "ECU was null");
                return null;
            }

            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (string.IsNullOrEmpty(item.ECU_GRUPPE))
                    {
                        continue;
                    }

                    string[] array = ECU_GRUPPE.Split('|');
                    string[] array2 = item.ECU_GRUPPE.Split('|');
                    foreach (string a in array2)
                    {
                        string[] array3 = array;
                        foreach (string b in array3)
                        {
                            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECUbyECU_GRUPPE()", exception);
            }

            return null;
        }

        public uint getDiagProtECUCount(typeDiagProtocoll ecuDiag)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            uint num = 0u;
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.DiagProtocoll == ecuDiag)
                    {
                        num++;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehcile.getECU()", exception);
            }

            return num;
        }

        [PreserveSource(Hint = "Cleaned")]
        public PlaceholderType getCBSMeasurementValue(typeCBSMeaurementType mType)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool addOrUpdateCBSMeasurementValue()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool addOrUpdateCBSMeasurementValues()
        {
            return false;
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public void AddEcu(ECU ecu)
        {
            base.ECU.Add(ecu);
        }

        [PreserveSource(Hint = "XEP_ECUCLIQUES removed", OriginalHash = "FFF58050A256B72A929205601AC5D0C2")]
        public void AddEcu(IIdentEcu ecu)
        {
            ECU eCU = new ECU
            {
                ProgrammingVariantName = ecu.ProgrammingVariantName,
                SERIENNUMMER = ecu.SERIENNUMMER,
                ID_SG_ADR = ecu.ID_SG_ADR,
                ECU_SGBD = ecu.ECU_SGBD,
                VARIANTE = ecu.VARIANTE,
                ECU_ADR = ecu.ECU_ADR,
                ECU_GRUPPE = ecu.ECU_GRUPPE,
                TITLE_ECUTREE = ecu.TITLE_ECUTREE,
                ECUTreeColor = ecu.ECUTreeColor,
                ECUTitle = ecu.ECUTitle
            };
            base.ECU.AddIfNotContains(eCU);
        }

        public bool AddOrUpdateECU(ECU nECU)
        {
            try
            {
                if (nECU == null)
                {
                    Log.Warning("Vehicle.AddOrUpdateECU()", "ecu was null");
                    return false;
                }

                if (base.ECU == null)
                {
                    base.ECU = new ObservableCollection<ECU>();
                }

                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR == nECU.ID_SG_ADR)
                    {
                        int num = base.ECU.IndexOf(item);
                        if (num >= 0 && num < base.ECU.Count)
                        {
                            base.ECU[num] = nECU;
                            Log.Info("Vehicle.AddOrUpdateECU()", "updating ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                            return true;
                        }
                    }
                }

                base.ECU.Add(nECU);
                Log.Info("Vehicle.AddOrUpdateECU()", "adding ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.AddOrUpdateECU()", exception);
            }

            return false;
        }

        [PreserveSource(Hint = "Database modified", OriginalHash = "B2BD63A6FC2FF0ED2D61C5AA6CDB4090")]
        public bool getISTACharacteristics(decimal id, out string value, long datavalueId, ValidationRuleInternalResults internalResult)
        {
            PsdzDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetDatabase(this)?.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
            if (characteristicRootsById != null)
            {
                return new VehicleCharacteristicVehicleHelper(this).GetISTACharacteristics(characteristicRootsById.NodeClass, out value, id, this, datavalueId, internalResult);
            }

            Log.Warning("Vehicle.getISTACharactersitics()", "No entry found in CharacteristicRoots for id: {0}!", id);
            value = "???";
            return false;
        }

        public void UpdateStatus(string name, StateType type, double? progress)
        {
            try
            {
                string status_FunctionName = base.Status_FunctionName;
                StateType status_FunctionState = base.Status_FunctionState;
                Log.Info("Vehicle.UpdateStatus()", "Change state from '{0}/{1}' to '{2}/{3}'", status_FunctionName, status_FunctionState, name, type);
                base.Status_FunctionName = name;
                base.Status_FunctionState = type;
                base.Status_FunctionStateLastChangeTime = DateTime.Now;
                if (progress.HasValue)
                {
                    base.Status_FunctionProgress = progress.Value;
                }

                IsNoVehicleCommunicationRunning = base.Status_FunctionState != StateType.running;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.UpdateStatus()", exception);
            }
        }

        public bool IsVehicleWithOnlyVin7()
        {
            return VIN10Prefix.Equals("FILLER17II", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool evalILevelExpression(string iLevelExpressions)
        {
            bool flag = false;
            bool flag2 = true;
            try
            {
                if (string.IsNullOrEmpty(iLevelExpressions))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(base.ILevel))
                {
                    Log.Info("Vehicle.evaILevelExpression()", "ILevel unknown; result will be true; expression was: {0}", iLevelExpressions);
                    return true;
                }

                if (iLevelExpressions.Contains("&"))
                {
                    flag2 = false;
                    flag = true;
                }

                if (CoreFramework.DebugLevel > 0)
                {
                    Log.Info("Vehicle.evalILevelExpression()", "expression:{0} vehicle iLEVEL:{1}", iLevelExpressions, base.ILevel);
                }

                string[] separator = new string[2]
                {
                    "&",
                    "|"
                };
                string[] array = iLevelExpressions.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string text in array)
                {
                    string[] separator2 = new string[1]
                    {
                        ","
                    };
                    string[] array2 = text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
                    if (array2.Length != 2)
                    {
                        continue;
                    }

                    Log.Info("Vehicle.evalILevelExpression()", "expression {0} {1}", base.ILevel, text);
                    if (string.Compare(base.ILevel, 0, array2[1], 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        switch (array2[0])
                        {
                            case ">":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "> was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "< was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case ">=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", ">= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "<= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "!=":
                            case "<>":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(base.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "!= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(base.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(base.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                        }
                    }
                    else
                    {
                        Log.Warning("Vehicle.evalILevelExpression()", "iLevel main type does not match");
                    }
                }

                return flag;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.evalILevelExpression()", exception);
                return true;
            }
        }

        public bool isECUAlreadyScanned(ECU checkSG)
        {
            try
            {
                foreach (ECU item in base.ECU)
                {
                    if (item.ID_SG_ADR == checkSG.ID_SG_ADR)
                    {
                        return true;
                    }

                    if (!string.IsNullOrEmpty(item.ECU_ADR) && !string.IsNullOrEmpty(checkSG.ECU_ADR) && string.Compare(item.ECU_ADR, checkSG.ECU_ADR, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.isECUAlreadyScanned()", exception);
            }

            return false;
        }

        public T getResultAs<T>(string resultName)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                if (!string.IsNullOrEmpty(resultName))
                {
                    object obj = null;
                    switch (resultName)
                    {
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/BaureihenVerbund":
                            obj = BasisEReihe;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/IStufe":
                            obj = base.ILevel;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/Fahrzeugauftrag":
                            obj = base.FA.STANDARD_FA;
                            break;
                        case "/Result/DList":
                        case "/Result/GruppenListe":
                        {
                            string text4 = string.Empty;
                            foreach (ECU item in base.ECU)
                            {
                                text4 = text4 + item.ECU_GRUPPE + ",";
                            }

                            text4 = text4.TrimEnd(',');
                            obj = text4;
                            break;
                        }

                        case "/Result/SonderAusstattungsListe":
                        {
                            string text3 = string.Empty;
                            foreach (string item2 in base.FA.SA)
                            {
                                text3 = text3 + item2 + ",";
                            }

                            text3 = text3.TrimEnd(',');
                            obj = text3;
                            break;
                        }

                        case "/Result/EWortListe":
                        {
                            string text2 = string.Empty;
                            foreach (string item3 in base.FA.E_WORT)
                            {
                                text2 = text2 + item3 + ",";
                            }

                            text2 = text2.TrimEnd(',');
                            obj = text2;
                            break;
                        }

                        case "/Result/HOWortListe":
                        {
                            string text = string.Empty;
                            foreach (string item4 in base.FA.HO_WORT)
                            {
                                text = text + item4 + ",";
                            }

                            text = text.TrimEnd(',');
                            obj = text;
                            break;
                        }

                        case "/Result/Baustand":
                            obj = base.FA.C_DATE;
                            break;
                        default:
                            Log.Error("VehicleHelper.getResultAs<T>", "Unknown resultName '{0}' found!", resultName);
                            break;
                    }

                    if (obj != null)
                    {
                        if (obj.GetType() != typeFromHandle)
                        {
                            return (T)Convert.ChangeType(obj, typeFromHandle);
                        }

                        return (T)obj;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getISTAResultAs(string resultName)", exception);
            }

            return default(T);
        }

        [PreserveSource(Hint = "Cleaned")]
        public void AddDiagCode(string diagCodeString, string diagCodeSuffixString, string originatingAblauf, IList<string> reparaturPaketList, bool teileClearingFlag)
        {
        }

        IEcu IVehicle.getECU(long? sgAdr)
        {
            return getECU(sgAdr);
        }

        IEcu IVehicle.getECU(long? sgAdr, long? subAddress)
        {
            return getECU(sgAdr, subAddress);
        }

        IEcu IVehicle.getECUbyECU_GRUPPE(string ECU_GRUPPE)
        {
            return getECUbyECU_GRUPPE(ECU_GRUPPE);
        }

        public bool? IsABSVehicle()
        {
            if (base.ECU != null && base.ECU.Count > 0)
            {
                string[] array = new string[16]
                {
                    "ASCMK20",
                    "absmk4",
                    "absmk4g",
                    "abs5",
                    "abs_uc",
                    "asc4gus",
                    "asc5",
                    "asc57",
                    "asc57r75",
                    "asc5d",
                    "ascmk20",
                    "ascmk4.prg",
                    "ascmk4g",
                    "ascmk4g1",
                    "asc_l22",
                    "asc_t"
                };
                ECU eCU = getECU(86L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                eCU = getECU(41L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                eCU = getECU(54L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            return null;
        }

        [PreserveSource(Hint = "Cleaned")]
        private static PlaceholderType CalculateFaultList()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        private static int? CalculateFaultCodeSum()
        {
            return null;
        }

        [PreserveSource(Hint = "Cleaned")]
        public void AddCombinedDTC()
        {
        }

        public bool GetProgrammingEnabledForBn(string bn)
        {
            return GetBnTypes(bn).Contains(base.BNType);
        }

        public bool IsProgrammingSupported(bool considerLogisticBase)
        {
            if ((ConfigSettings.IsProgrammingEnabled() || (considerLogisticBase && ConfigSettings.IsLogisticBaseEnabled())) && GetProgrammingEnabledForBn(ConfigSettings.getConfigString("BMW.Rheingold.Programming.BN", "BN2020,BN2020_MOTORBIKE")))
            {
                return !ConfigSettings.IsISTAModeRITA;
            }

            return false;
        }

        private static ISet<BNType> GetBnTypes(string bnTypes)
        {
            ISet<BNType> set = new HashSet<BNType>();
            if (string.IsNullOrEmpty(bnTypes))
            {
                return set;
            }

            string[] array = bnTypes.Split(',');
            foreach (string text in array)
            {
                if (Enum.TryParse<BNType>(text, ignoreCase: false, out var result))
                {
                    set.Add(result);
                    continue;
                }

                Log.Error("Vehicle.GetBnTypes()", "Ignore BN \"{0}\", because of missconfiguration.", text);
            }

            return set;
        }

        public int GetCustomHashCode()
        {
            int num = 37;
            int num2 = 327;
            num *= GetHashCode();
            if (!string.IsNullOrWhiteSpace(base.VIN17))
            {
                num += base.VIN17.GetHashCode();
                num *= num2;
            }

            ObservableCollection<ECU> eCU = base.ECU;
            if (eCU != null && eCU.Any())
            {
                foreach (ECU item in base.ECU)
                {
                    num += item.GetHashCode();
                    num *= num2;
                    if (!string.IsNullOrEmpty(item.VARIANTE))
                    {
                        num += item.VARIANTE.GetHashCode();
                        num *= num2;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(base.Ereihe))
            {
                num += base.Ereihe.GetHashCode();
                num *= num2;
            }

            if (!string.IsNullOrWhiteSpace(base.Baureihenverbund))
            {
                num += base.Baureihenverbund.GetHashCode();
                num *= num2;
            }

            if (C_DATETIME.HasValue)
            {
                num += C_DATETIME.GetHashCode();
                num *= num2;
            }

            return num;
        }

        [PreserveSource(Hint = "Cleaned")]
        public int GetFaultListHashCode()
        {
            return 0;
        }

        public IReactorFa GetFaInstance()
        {
            return new FA();
        }

        bool IIdentVehicle.IsPreE65Vehicle()
        {
            return Classification.IsPreE65Vehicle();
        }

        IIdentEcu IIdentVehicle.getECU(long? sgAdr)
        {
            return getECU(sgAdr);
        }

        IIdentEcu IIdentVehicle.getECUbyECU_GRUPPE(string ECU_GRUPPE)
        {
            return getECUbyECU_GRUPPE(ECU_GRUPPE);
        }

        private List<IIdentEcu> GetEcusAsIIdentEcu()
        {
            return base.ECU.Cast<IIdentEcu>().ToList();
        }

        [PreserveSource(Hint = "SessionStart", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType SessionStart { get; set; }

        [PreserveSource(Hint = "Local reactor added")]
        public Reactor Reactor { get; private set; }
    }
}