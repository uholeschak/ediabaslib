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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using BMW.Rheingold.Programming.Common;
using PsdzClient.Contracts;
using PsdzClient.Programming;

#pragma warning disable CS0169, CS0649, CS0618, CS0612
namespace PsdzClient.Core
{
    public class Vehicle : IVehicle, INotifyPropertyChanged, IVehicleRuleEvaluation, IVinValidatorVehicle, IIdentVehicle, IReactorVehicle, IEcuTreeVehicle
    {
        public const string BnProgramming = "BN2020,BN2020_MOTORBIKE";
        [PreserveSource(Hint = "ObservableCollectionEx<Fault>", Placeholder = true)]
        private readonly PlaceholderType pKodeList;
        private readonly ParameterContainer sessionDataStore;
        private string vinRangeType;
        private string vinRangeTypeLastResolvedType;
        private FA targetFA;
        private string productLine;
        private string doorNumber;
        private string securityRelevant;
        private DateTime? cDatetimeByModelYearMonth;
        private HashSet<int> validPWFStates;
        private double clamp15MinValue;
        private double clamp30MinValue;
        [PreserveSource(Hint = "Database modified", SuppressWarning = true)]
        private BatteryEnum batteryType;
        private string verkaufsBezeichnungField;
        private string targetILevel;
        private readonly ObservableCollection<string> diagCodesProgramming;
        [PreserveSource(Hint = "IList<Fault>", Placeholder = true)]
        private PlaceholderType faultList;
        [PreserveSource(Hint = "ObservableCollection<CheckControlMessage>", Placeholder = true)]
        private PlaceholderType checkControlMessages;
        private string salesDesignationBadgeUIText;
        private string eBezeichnungUIText;
        private const int indexOfFirsHDDAboUpdateInDecimal = 54;
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
        [PreserveSource(Hint = "Database modified", SuppressWarning = true)]
        private PsdzDatabase.BordnetsData bordnetsData;
        private VehicleClassification classification;
        private IVehicleProfileChecksum vpc;
        private CcmReadoutState ccmReadoutState;
        private string vIN17Field;
        private string serialBodyShellField;
        private string serialGearBoxField;
        private string serialEngineField;
        private BrandName? brandNameField;
        private ObservableCollection<ECU> eCUField;
        [PreserveSource(Hint = "ObservableCollection<ZFSResult>", Placeholder = true)]
        private PlaceholderType zFSField;
        private List<CEMResult> cemField;
        private ECU selectedECUField;
        [PreserveSource(Hint = "ObservableCollection<typeCBSInfo>", Placeholder = true)]
        private PlaceholderType cBSField;
        private string typField;
        private string basicTypeField;
        private string driveTypeField;
        private string warrentyTypeField;
        private string markeField;
        private string ueberarbeitungField;
        private string prodartField;
        private string ereiheField;
        private string mainSeriesSgbdField;
        private string mainSeriesSgbdAdditionalField;
        private BNType bNTypeField;
        private string baureiheField;
        private string roadMapField;
        private ChassisType chassisTypeField;
        private string karosserieField;
        private EMotor eMotorField;
        private List<HeatMotor> heatMotorsField;
        private GenericMotor genericMotorField;
        private string motorField;
        private string hubraumField;
        private string landField;
        private string lenkungField;
        private string getriebeField;
        private string countryOfAssemblyField;
        private string baseVersionField;
        private string antriebField;
        private DateTime? firstRegistrationField;
        private string baustandsJahrField;
        private string baustandsMonatField;
        private string elektrischeReichweiteField;
        private string aeBezeichnungField;
        private string iLevelField;
        private decimal? gwszField;
        private GwszUnitType? gwszUnitField;
        private ObservableCollection<FFMResult> fFMField;
        private string iLevelWerkField;
        private string iLevelBackupField;
        private FA faField;
        private string zCSField;
        [PreserveSource(Hint = "ObservableCollection<InfoObject>", Placeholder = true)]
        private PlaceholderType historyInfoObjectsField;
        [PreserveSource(Hint = "TestPlanType", Placeholder = true)]
        private PlaceholderType testplanField;
        [PreserveSource(Hint = "TestPlanCache", Placeholder = true)]
        private PlaceholderType testPlanCache;
        private VCIDevice vCIField;
        private VCIDevice mIBField;
        [PreserveSource(Hint = "ObservableCollection<technicalCampaignType>", Placeholder = true)]
        private PlaceholderType technicalCampaignsField;
        private string leistungsklasseField;
        private string kraftstoffartField;
        private string eCTypeApprovalField;
        private DateTime lastSaveDateField;
        private DateTime lastChangeDateField;
        [PreserveSource(Hint = "ObservableCollection<typeServiceHistoryEntry>", Placeholder = true)]
        private PlaceholderType serviceHistoryField;
        [PreserveSource(Hint = "ObservableCollection<typeDiagCode>", Placeholder = true)]
        private PlaceholderType diagCodesField;
        private string motorarbeitsverfahrenField;
        private string drehmomentField;
        private string hybridkennzeichenField;
        [PreserveSource(Hint = "ObservableCollection<DTC>", Placeholder = true)]
        private PlaceholderType combinedFaultsField;
        private ObservableCollection<decimal> installedAdaptersField;
        private string vIN17_OEMField;
        private string mOTKraftstoffartField;
        private string mOTEinbaulageField;
        private string mOTBezeichnungField;
        private string baureihenverbundField;
        private string aEKurzbezeichnungField;
        private string aELeistungsklasseField;
        private string aEUeberarbeitungField;
        [PreserveSource(Hint = "ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX>", Placeholder = true)]
        private PlaceholderType perceivedSymptomsField;
        private string progmanVersionField;
        private StateType status_FunctionStateField;
        private string kl15VoltageField;
        private string kl30VoltageField;
        private bool pADVehicleField;
        private int pwfStateField;
        private DateTime klVoltageLastMessageTimeField;
        private bool klVoltageLastMessageTimeFieldSpecified;
        private string applicationVersionField;
        private bool fASTAAlreadyDoneField;
        private IdentificationLevel vehicleIdentLevelField;
        private bool vehicleShortTestAsSessionEntryField;
        private bool pannenfallField;
        private int selectedDiagBUSField;
        private bool gWSZReadoutSuccessField;
        private DateTime vehicleLifeStartDate;
        private double vehicleSystemTime;
        private List<DealerSessionProperty> dealerSessionProperties;
        private DateTime productionDate;
        private string modelljahr;
        private string modellmonat;
        private string modelltag;
        private string chassisCode;
        [PreserveSource(Hint = "BackendsAvailabilityIndicator", Placeholder = true)]
        private PlaceholderType backendsAvailabilityIndicator;
        public string F2Date { get; set; }
        public string SoftwareId { get; set; }

        public VCIDevice VCI
        {
            get
            {
                return vCIField;
            }

            set
            {
                if (vCIField != null)
                {
                    if (!vCIField.Equals(value))
                    {
                        vCIField = value;
                        OnPropertyChanged("VCI");
                    }
                }
                else
                {
                    vCIField = value;
                    OnPropertyChanged("VCI");
                }
            }
        }

        public string VIN17
        {
            get
            {
                return vIN17Field;
            }

            set
            {
                if (vIN17Field != value)
                {
                    vIN17Field = value;
                    OnPropertyChanged("VIN17");
                }
            }
        }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsSendFastaDataForbidden")]
        public bool IsSendFastaDataForbidden { get; set; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsSendOBFCMDataForbidden")]
        public bool IsSendOBFCMDataForbidden { get; set; }

        public string ChassisCode
        {
            get
            {
                return chassisCode;
            }

            set
            {
                if (chassisCode != value)
                {
                    chassisCode = value;
                    OnPropertyChanged("ChassisCode");
                }
            }
        }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.OrderDataRequestFailed")]
        public bool OrderDataRequestFailed { get; set; }

        public string SerialBodyShell
        {
            get
            {
                return serialBodyShellField;
            }

            set
            {
                if (serialBodyShellField != value)
                {
                    serialBodyShellField = value;
                    OnPropertyChanged("SerialBodyShell");
                }
            }
        }

        public string SerialGearBox
        {
            get
            {
                return serialGearBoxField;
            }

            set
            {
                if (serialGearBoxField != value)
                {
                    serialGearBoxField = value;
                    OnPropertyChanged("SerialGearBox");
                    OnPropertyChanged("SerialGearBox7");
                }
            }
        }

        public string SerialEngine
        {
            get
            {
                return serialEngineField;
            }

            set
            {
                if (serialEngineField != value)
                {
                    serialEngineField = value;
                    OnPropertyChanged("SerialEngine");
                }
            }
        }

        public List<DealerSessionProperty> DealerSessionProperties
        {
            get
            {
                return dealerSessionProperties;
            }

            set
            {
                if (dealerSessionProperties != null)
                {
                    if (!dealerSessionProperties.Equals(value))
                    {
                        dealerSessionProperties = value;
                    }
                }
                else
                {
                    dealerSessionProperties = value;
                }
            }
        }

        public BrandName? BrandName
        {
            get
            {
                return brandNameField;
            }

            set
            {
                if (brandNameField != value)
                {
                    brandNameField = value;
                    OnPropertyChanged("BrandName");
                }
            }
        }

        public ObservableCollection<ECU> ECU
        {
            get
            {
                return eCUField;
            }

            set
            {
                if (eCUField != null)
                {
                    if (!eCUField.Equals(value))
                    {
                        eCUField = value;
                        OnPropertyChanged("ECU");
                    }
                }
                else
                {
                    eCUField = value;
                    OnPropertyChanged("ECU");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<ZFSResult>", Placeholder = true)]
        public PlaceholderType ZFS;
        public List<CEMResult> CEM
        {
            get
            {
                return cemField;
            }

            set
            {
                if (cemField != null)
                {
                    if (!cemField.Equals(value))
                    {
                        cemField = value;
                    }
                }
                else
                {
                    cemField = value;
                }
            }
        }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.ZfsSuccessfull")]
        public bool ZFS_SUCCESSFULLY { get; }

        public ECU SelectedECU
        {
            get
            {
                return selectedECUField;
            }

            set
            {
                if (selectedECUField != null)
                {
                    if (!selectedECUField.Equals(value))
                    {
                        selectedECUField = value;
                        OnPropertyChanged("SelectedECU");
                    }
                }
                else
                {
                    selectedECUField = value;
                    OnPropertyChanged("SelectedECU");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<typeCBSInfo>", Placeholder = true)]
        public PlaceholderType CBS;
        public string Typ
        {
            get
            {
                AddServiceCodeAndLogsForTypeKeys(typField, "Typ");
                return typField;
            }

            set
            {
                if (typField != value)
                {
                    typField = value;
                    OnPropertyChanged("Typ");
                }
            }
        }

        public string BasicType
        {
            get
            {
                return basicTypeField;
            }

            set
            {
                if (basicTypeField != value)
                {
                    basicTypeField = value;
                    OnPropertyChanged("BasicType");
                }
            }
        }

        public string DriveType
        {
            get
            {
                return driveTypeField;
            }

            set
            {
                if (driveTypeField != null)
                {
                    if (!driveTypeField.Equals(value))
                    {
                        driveTypeField = value;
                        OnPropertyChanged("DriveType");
                    }
                }
                else
                {
                    driveTypeField = value;
                    OnPropertyChanged("DriveType");
                }
            }
        }

        public string WarrentyType
        {
            get
            {
                return warrentyTypeField;
            }

            set
            {
                if (warrentyTypeField != null)
                {
                    if (!warrentyTypeField.Equals(value))
                    {
                        warrentyTypeField = value;
                        OnPropertyChanged("WarrentyType");
                    }
                }
                else
                {
                    warrentyTypeField = value;
                    OnPropertyChanged("WarrentyType");
                }
            }
        }

        public string Marke
        {
            get
            {
                return markeField;
            }

            set
            {
                if (markeField != value)
                {
                    markeField = value;
                    OnPropertyChanged("Marke");
                }
            }
        }

        public string Ueberarbeitung
        {
            get
            {
                return ueberarbeitungField;
            }

            set
            {
                if (ueberarbeitungField != value)
                {
                    ueberarbeitungField = value;
                    OnPropertyChanged("Ueberarbeitung");
                }
            }
        }

        [DefaultValue("P")]
        public string Prodart
        {
            get
            {
                return prodartField;
            }

            set
            {
                if (prodartField != value)
                {
                    prodartField = value;
                    OnPropertyChanged("Prodart");
                }
            }
        }

        public string Ereihe
        {
            get
            {
                return ereiheField;
            }

            set
            {
                if (ereiheField != value)
                {
                    ereiheField = value;
                    OnPropertyChanged("Ereihe");
                }
            }
        }

        public string MainSeriesSgbd
        {
            get
            {
                return mainSeriesSgbdField;
            }

            set
            {
                if (mainSeriesSgbdField != null)
                {
                    if (!mainSeriesSgbdField.Equals(value))
                    {
                        mainSeriesSgbdField = value;
                        OnPropertyChanged("MainSeriesSgbd");
                    }
                }
                else
                {
                    mainSeriesSgbdField = value;
                    OnPropertyChanged("MainSeriesSgbd");
                }
            }
        }

        public string MainSeriesSgbdAdditional
        {
            get
            {
                return mainSeriesSgbdAdditionalField;
            }

            set
            {
                if (mainSeriesSgbdAdditionalField != null)
                {
                    if (!mainSeriesSgbdAdditionalField.Equals(value))
                    {
                        mainSeriesSgbdAdditionalField = value;
                        OnPropertyChanged("MainSeriesSgbdAdditional");
                    }
                }
                else
                {
                    mainSeriesSgbdAdditionalField = value;
                    OnPropertyChanged("MainSeriesSgbdAdditional");
                }
            }
        }

        [DefaultValue(BNType.UNKNOWN)]
        public BNType BNType
        {
            get
            {
                return bNTypeField;
            }

            set
            {
                if (!bNTypeField.Equals(value))
                {
                    bNTypeField = value;
                    OnPropertyChanged("BNType");
                }
            }
        }

        public string Baureihe
        {
            get
            {
                return baureiheField;
            }

            set
            {
                if (baureiheField != value)
                {
                    baureiheField = value;
                    OnPropertyChanged("Baureihe");
                }
            }
        }

        public string RoadMap
        {
            get
            {
                return roadMapField;
            }

            set
            {
                if (roadMapField != null)
                {
                    if (!roadMapField.Equals(value))
                    {
                        roadMapField = value;
                        OnPropertyChanged("RoadMap");
                    }
                }
                else
                {
                    roadMapField = value;
                    OnPropertyChanged("RoadMap");
                }
            }
        }

        public ChassisType ChassisType
        {
            get
            {
                return chassisTypeField;
            }

            set
            {
                if (!chassisTypeField.Equals(value))
                {
                    chassisTypeField = value;
                    OnPropertyChanged("ChassisType");
                }
            }
        }

        public string Karosserie
        {
            get
            {
                return karosserieField;
            }

            set
            {
                if (karosserieField != value)
                {
                    karosserieField = value;
                    OnPropertyChanged("Karosserie");
                }
            }
        }

        public EMotor EMotor
        {
            get
            {
                return eMotorField;
            }

            set
            {
                if (eMotorField != value)
                {
                    eMotorField = value;
                    OnPropertyChanged("EMotor");
                }
            }
        }

        public List<HeatMotor> HeatMotors
        {
            get
            {
                return heatMotorsField;
            }

            set
            {
                if (heatMotorsField != value)
                {
                    heatMotorsField = value;
                    OnPropertyChanged("HeatMotors");
                }
            }
        }

        public GenericMotor GenericMotor
        {
            get
            {
                return genericMotorField;
            }

            set
            {
                if (genericMotorField != null)
                {
                    if (!genericMotorField.Equals(value))
                    {
                        genericMotorField = value;
                        OnPropertyChanged("GenericMotor");
                    }
                }
                else
                {
                    genericMotorField = value;
                    OnPropertyChanged("GenericMotor");
                }
            }
        }

        public string Motor
        {
            get
            {
                return motorField;
            }

            set
            {
                if (motorField != value)
                {
                    motorField = value;
                    GenericMotor.Engine1 = value;
                    OnPropertyChanged("Motor");
                }
            }
        }

        public string Hubraum
        {
            get
            {
                return hubraumField;
            }

            set
            {
                if (hubraumField != value)
                {
                    hubraumField = value;
                    OnPropertyChanged("Hubraum");
                }
            }
        }

        public string Land
        {
            get
            {
                return landField;
            }

            set
            {
                if (landField != value)
                {
                    landField = value;
                    OnPropertyChanged("Land");
                }
            }
        }

        public string Lenkung
        {
            get
            {
                return lenkungField;
            }

            set
            {
                if (lenkungField != value)
                {
                    lenkungField = value;
                    OnPropertyChanged("Lenkung");
                }
            }
        }

        public string Getriebe
        {
            get
            {
                return getriebeField;
            }

            set
            {
                if (getriebeField != value)
                {
                    getriebeField = value;
                    OnPropertyChanged("Getriebe");
                }
            }
        }

        public string CountryOfAssembly
        {
            get
            {
                return countryOfAssemblyField;
            }

            set
            {
                if (countryOfAssemblyField != value)
                {
                    countryOfAssemblyField = value;
                    OnPropertyChanged("CountryOfAssembly");
                }
            }
        }

        public string BaseVersion
        {
            get
            {
                return baseVersionField;
            }

            set
            {
                if (baseVersionField != value)
                {
                    baseVersionField = value;
                    OnPropertyChanged("BaseVersion");
                }
            }
        }

        public string Antrieb
        {
            get
            {
                return antriebField;
            }

            set
            {
                if (antriebField != value)
                {
                    antriebField = value;
                    OnPropertyChanged("Antrieb");
                }
            }
        }

        public DateTime ProductionDate
        {
            get
            {
                return productionDate;
            }

            set
            {
                if (productionDate != value)
                {
                    productionDate = value;
                    OnPropertyChanged("ProductionDate");
                }
            }
        }

        [XmlIgnore]
        public bool ProductionDateSpecified => productionDate != default(DateTime);

        public DateTime? FirstRegistration
        {
            get
            {
                return firstRegistrationField;
            }

            set
            {
                if (firstRegistrationField != value)
                {
                    firstRegistrationField = value;
                    OnPropertyChanged("FirstRegistration");
                }
            }
        }

        public string Modelljahr
        {
            get
            {
                return modelljahr;
            }

            set
            {
                if (modelljahr != value)
                {
                    modelljahr = value;
                    OnPropertyChanged("Modelljahr");
                }
            }
        }

        public string Modellmonat
        {
            get
            {
                return modellmonat;
            }

            set
            {
                if (modellmonat != value)
                {
                    modellmonat = value;
                    OnPropertyChanged("Modellmonat");
                }
            }
        }

        public string Modelltag
        {
            get
            {
                return modelltag;
            }

            set
            {
                if (modelltag != value)
                {
                    modelltag = value;
                    OnPropertyChanged("Modelltag");
                }
            }
        }

        public string BaustandsJahr
        {
            get
            {
                return baustandsJahrField;
            }

            set
            {
                if (baustandsJahrField != value)
                {
                    baustandsJahrField = value;
                    OnPropertyChanged("BaustandsJahr");
                }
            }
        }

        public string BaustandsMonat
        {
            get
            {
                return baustandsMonatField;
            }

            set
            {
                if (baustandsMonatField != value)
                {
                    baustandsMonatField = value;
                    OnPropertyChanged("BaustandsMonat");
                }
            }
        }

        public string ILevel
        {
            get
            {
                return iLevelField;
            }

            set
            {
                if (iLevelField != value)
                {
                    iLevelField = value;
                    OnPropertyChanged("ILevel");
                }
            }
        }

        public decimal? Gwsz
        {
            get
            {
                return gwszField;
            }

            set
            {
                if (!(gwszField == value))
                {
                    gwszField = value;
                    OnPropertyChanged("Gwsz");
                    OnPropertyChanged("DisplayGwsz");
                }
            }
        }

        public GwszUnitType? GwszUnit
        {
            get
            {
                return gwszUnitField;
            }

            set
            {
                if (gwszUnitField.HasValue)
                {
                    if (!gwszUnitField.Equals(value))
                    {
                        gwszUnitField = value;
                        OnPropertyChanged("GwszUnit");
                    }
                }
                else
                {
                    gwszUnitField = value;
                    OnPropertyChanged("GwszUnit");
                }
            }
        }

        public ObservableCollection<FFMResult> FFM
        {
            get
            {
                return fFMField;
            }

            set
            {
                if (fFMField != null)
                {
                    if (!fFMField.Equals(value))
                    {
                        fFMField = value;
                        OnPropertyChanged("FFM");
                    }
                }
                else
                {
                    fFMField = value;
                    OnPropertyChanged("FFM");
                }
            }
        }

        public string ILevelWerk
        {
            get
            {
                return iLevelWerkField;
            }

            set
            {
                if (iLevelWerkField != value)
                {
                    iLevelWerkField = value;
                    OnPropertyChanged("ILevelWerk");
                }
            }
        }

        public string ILevelBackup
        {
            get
            {
                return iLevelBackupField;
            }

            set
            {
                if (iLevelBackupField != null)
                {
                    if (!iLevelBackupField.Equals(value))
                    {
                        iLevelBackupField = value;
                        OnPropertyChanged("ILevelBackup");
                    }
                }
                else
                {
                    iLevelBackupField = value;
                    OnPropertyChanged("ILevelBackup");
                }
            }
        }

        public FA FA
        {
            get
            {
                return faField;
            }

            set
            {
                if (faField != value)
                {
                    faField = value;
                    OnPropertyChanged("FA");
                }
            }
        }

        public string ZCS
        {
            get
            {
                return zCSField;
            }

            set
            {
                if (zCSField != null)
                {
                    if (!zCSField.Equals(value))
                    {
                        zCSField = value;
                        OnPropertyChanged("ZCS");
                    }
                }
                else
                {
                    zCSField = value;
                    OnPropertyChanged("ZCS");
                }
            }
        }

        [PreserveSource(Hint = "public ObservableCollection<InfoObject>", Placeholder = true)]
        public PlaceholderType HistoryInfoObjects;

        [PreserveSource(Hint = "public TestPlanType", Placeholder = true)]
        public PlaceholderType Testplan;

        [PreserveSource(Hint = "public TestPlanCache", Placeholder = true)]
        [IgnoreDataMember]
        [XmlIgnore]
        public PlaceholderType TestPlanCache => testPlanCache;

        [Obsolete("Use SessionInfoAccessor.SessionInfo.SimulatedParts")]
        public bool SimulatedParts { get; }

        public VCIDevice MIB
        {
            get
            {
                return mIBField;
            }

            set
            {
                if (mIBField != null)
                {
                    if (!mIBField.Equals(value))
                    {
                        mIBField = value;
                        OnPropertyChanged("MIB");
                    }
                }
                else
                {
                    mIBField = value;
                    OnPropertyChanged("MIB");
                }
            }
        }

        [PreserveSource(Hint = "public ObservableCollection<technicalCampaignType>", Placeholder = true)]
        public PlaceholderType TechnicalCampaigns;
        public string Leistungsklasse
        {
            get
            {
                return leistungsklasseField;
            }

            set
            {
                if (leistungsklasseField != value)
                {
                    leistungsklasseField = value;
                    OnPropertyChanged("Leistungsklasse");
                }
            }
        }

        public string Kraftstoffart
        {
            get
            {
                return kraftstoffartField;
            }

            set
            {
                if (kraftstoffartField != value)
                {
                    kraftstoffartField = value;
                    OnPropertyChanged("Kraftstoffart");
                }
            }
        }

        public string ECTypeApproval
        {
            get
            {
                return eCTypeApprovalField;
            }

            set
            {
                if (eCTypeApprovalField != value)
                {
                    eCTypeApprovalField = value;
                    OnPropertyChanged("ECTypeApproval");
                }
            }
        }

        public DateTime LastSaveDate
        {
            get
            {
                return lastSaveDateField;
            }

            set
            {
                if (!lastSaveDateField.Equals(value))
                {
                    lastSaveDateField = value;
                    OnPropertyChanged("LastSaveDate");
                }
            }
        }

        public DateTime LastChangeDate
        {
            get
            {
                return lastChangeDateField;
            }

            set
            {
                if (!lastChangeDateField.Equals(value))
                {
                    lastChangeDateField = value;
                    OnPropertyChanged("LastChangeDate");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<typeServiceHistoryEntry>", Placeholder = true)]
        public PlaceholderType ServiceHistory;
        [PreserveSource(Hint = "ObservableCollection<typeDiagCode>", Placeholder = true)]
        public PlaceholderType DiagCodes;
        public string Motorarbeitsverfahren
        {
            get
            {
                return motorarbeitsverfahrenField;
            }

            set
            {
                if (motorarbeitsverfahrenField != value)
                {
                    motorarbeitsverfahrenField = value;
                    OnPropertyChanged("Motorarbeitsverfahren");
                }
            }
        }

        public string Drehmoment
        {
            get
            {
                return drehmomentField;
            }

            set
            {
                if (drehmomentField != value)
                {
                    drehmomentField = value;
                    OnPropertyChanged("Drehmoment");
                }
            }
        }

        public string Hybridkennzeichen
        {
            get
            {
                return hybridkennzeichenField ?? string.Empty;
            }

            set
            {
                if (hybridkennzeichenField != value)
                {
                    hybridkennzeichenField = value;
                    OnPropertyChanged("Hybridkennzeichen");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<DTC>", Placeholder = true)]
        public PlaceholderType CombinedFaults;
        public ObservableCollection<decimal> InstalledAdapters
        {
            get
            {
                return installedAdaptersField;
            }

            set
            {
                if (installedAdaptersField != null)
                {
                    if (!installedAdaptersField.Equals(value))
                    {
                        installedAdaptersField = value;
                        OnPropertyChanged("InstalledAdapters");
                    }
                }
                else
                {
                    installedAdaptersField = value;
                    OnPropertyChanged("InstalledAdapters");
                }
            }
        }

        public string VIN17_OEM
        {
            get
            {
                return vIN17_OEMField;
            }

            set
            {
                if (vIN17_OEMField != null)
                {
                    if (!vIN17_OEMField.Equals(value))
                    {
                        vIN17_OEMField = value;
                        OnPropertyChanged("VIN17_OEM");
                    }
                }
                else
                {
                    vIN17_OEMField = value;
                    OnPropertyChanged("VIN17_OEM");
                }
            }
        }

        public string MOTKraftstoffart
        {
            get
            {
                return mOTKraftstoffartField;
            }

            set
            {
                if (mOTKraftstoffartField != null)
                {
                    if (!mOTKraftstoffartField.Equals(value))
                    {
                        mOTKraftstoffartField = value;
                        OnPropertyChanged("MOTKraftstoffart");
                    }
                }
                else
                {
                    mOTKraftstoffartField = value;
                    OnPropertyChanged("MOTKraftstoffart");
                }
            }
        }

        public string MOTEinbaulage
        {
            get
            {
                return mOTEinbaulageField;
            }

            set
            {
                if (mOTEinbaulageField != value)
                {
                    mOTEinbaulageField = value;
                    OnPropertyChanged("MOTEinbaulage");
                }
            }
        }

        public string MOTBezeichnung
        {
            get
            {
                return mOTBezeichnungField;
            }

            set
            {
                if (mOTBezeichnungField != value)
                {
                    mOTBezeichnungField = value;
                    GenericMotor.EngineLabel1 = value;
                    OnPropertyChanged("MOTBezeichnung");
                }
            }
        }

        public string Baureihenverbund
        {
            get
            {
                return baureihenverbundField;
            }

            set
            {
                if (baureihenverbundField != value)
                {
                    baureihenverbundField = value;
                    OnPropertyChanged("Baureihenverbund");
                }
            }
        }

        public string AEKurzbezeichnung
        {
            get
            {
                return aEKurzbezeichnungField;
            }

            set
            {
                if (aEKurzbezeichnungField != value)
                {
                    aEKurzbezeichnungField = value;
                    OnPropertyChanged("AEKurzbezeichnung");
                }
            }
        }

        public string AELeistungsklasse
        {
            get
            {
                return aELeistungsklasseField;
            }

            set
            {
                if (aELeistungsklasseField != value)
                {
                    aELeistungsklasseField = value;
                    OnPropertyChanged("AELeistungsklasse");
                }
            }
        }

        public string AEUeberarbeitung
        {
            get
            {
                return aEUeberarbeitungField;
            }

            set
            {
                if (aEUeberarbeitungField != value)
                {
                    aEUeberarbeitungField = value;
                    OnPropertyChanged("AEUeberarbeitung");
                }
            }
        }

        [PreserveSource(Hint = "ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX>", Placeholder = true)]
        public PlaceholderType PerceivedSymptoms;
        public string ProgmanVersion
        {
            get
            {
                return progmanVersionField;
            }

            set
            {
                if (progmanVersionField != value)
                {
                    progmanVersionField = value;
                    OnPropertyChanged("ProgmanVersion");
                }
            }
        }

        [DefaultValue("grafik/gif/icon_offl_ACTIV.gif")]
        [Obsolete]
        public string ConnectImage { get; }

        [DefaultValue("grafik/gif/icon_imib_INACTIV.gif")]
        [Obsolete]
        public string ConnectIMIBImage { get; }

        [DefaultValue(VisibilityType.Visible)]
        [Obsolete]
        public VisibilityType ConnectIPState { get; }

        [Obsolete]
        [DefaultValue(VisibilityType.Visible)]
        public VisibilityType ConnectIMIBIPState { get; }

        [Obsolete]
        [DefaultValue(VisibilityType.Visible)]
        public VisibilityType ConnectState { get; }

        [DefaultValue(VisibilityType.Visible)]
        [Obsolete]
        public VisibilityType ConnectIMIBState { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.Status_FunctionName")]
        public string Status_FunctionName { get; }

        [DefaultValue(StateType.idle)]
        public StateType Status_FunctionState
        {
            get
            {
                return status_FunctionStateField;
            }

            set
            {
                if (!status_FunctionStateField.Equals(value))
                {
                    status_FunctionStateField = value;
                    OnPropertyChanged("Status_FunctionState");
                }
            }
        }

        [Obsolete]
        public DateTime Status_FunctionStateLastChangeTime { get; }

        [XmlIgnore]
        [Obsolete]
        public bool Status_FunctionStateLastChangeTimeSpecified { get; }

        [Obsolete("Use SessionInfoAccessor.SessionInfo.Status_FunctionProgress")]
        public double Status_FunctionProgress { get; }

        public string Kl15Voltage
        {
            get
            {
                return kl15VoltageField;
            }

            set
            {
                if (kl15VoltageField != null)
                {
                    if (!kl15VoltageField.Equals(value))
                    {
                        kl15VoltageField = value;
                        OnPropertyChanged("Kl15Voltage");
                    }
                }
                else
                {
                    kl15VoltageField = value;
                    OnPropertyChanged("Kl15Voltage");
                }
            }
        }

        public string Kl30Voltage
        {
            get
            {
                return kl30VoltageField;
            }

            set
            {
                if (kl30VoltageField != null)
                {
                    if (!kl30VoltageField.Equals(value))
                    {
                        kl30VoltageField = value;
                        OnPropertyChanged("Kl30Voltage");
                    }
                }
                else
                {
                    kl30VoltageField = value;
                    OnPropertyChanged("Kl30Voltage");
                }
            }
        }

        [DefaultValue(false)]
        public bool PADVehicle
        {
            get
            {
                return pADVehicleField;
            }

            set
            {
                if (!pADVehicleField.Equals(value))
                {
                    pADVehicleField = value;
                    OnPropertyChanged("PADVehicle");
                }
            }
        }

        [DefaultValue(-1)]
        public int PwfState
        {
            get
            {
                return pwfStateField;
            }

            set
            {
                if (!pwfStateField.Equals(value))
                {
                    pwfStateField = value;
                    OnPropertyChanged("PwfState");
                }
            }
        }

        public DateTime KlVoltageLastMessageTime
        {
            get
            {
                return klVoltageLastMessageTimeField;
            }

            set
            {
                if (!klVoltageLastMessageTimeField.Equals(value))
                {
                    klVoltageLastMessageTimeField = value;
                    OnPropertyChanged("KlVoltageLastMessageTime");
                }
            }
        }

        [XmlIgnore]
        public bool KlVoltageLastMessageTimeSpecified
        {
            get
            {
                return klVoltageLastMessageTimeFieldSpecified;
            }

            set
            {
                if (!klVoltageLastMessageTimeFieldSpecified.Equals(value))
                {
                    klVoltageLastMessageTimeFieldSpecified = value;
                    OnPropertyChanged("KlVoltageLastMessageTimeSpecified");
                }
            }
        }

        [DefaultValue("0.0.1")]
        public string ApplicationVersion
        {
            get
            {
                return applicationVersionField;
            }

            set
            {
                if (applicationVersionField != null)
                {
                    if (!applicationVersionField.Equals(value))
                    {
                        applicationVersionField = value;
                        OnPropertyChanged("ApplicationVersion");
                    }
                }
                else
                {
                    applicationVersionField = value;
                    OnPropertyChanged("ApplicationVersion");
                }
            }
        }

        [Obsolete]
        [DefaultValue(false)]
        public bool FASTAAlreadyDone
        {
            get
            {
                return fASTAAlreadyDoneField;
            }

            set
            {
                if (!fASTAAlreadyDoneField.Equals(value))
                {
                    fASTAAlreadyDoneField = value;
                    OnPropertyChanged("FASTAAlreadyDone");
                }
            }
        }

        [DefaultValue(IdentificationLevel.None)]
        public IdentificationLevel VehicleIdentLevel
        {
            get
            {
                return vehicleIdentLevelField;
            }

            set
            {
                if (!vehicleIdentLevelField.Equals(value))
                {
                    vehicleIdentLevelField = value;
                    OnPropertyChanged("VehicleIdentLevel");
                }
            }
        }

        [DefaultValue(false)]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.VehicleIdentAlreadyDone")]
        public bool VehicleIdentAlreadyDone { get; set; }

        [DefaultValue(false)]
        public bool VehicleShortTestAsSessionEntry
        {
            get
            {
                return vehicleShortTestAsSessionEntryField;
            }

            set
            {
                if (!vehicleShortTestAsSessionEntryField.Equals(value))
                {
                    vehicleShortTestAsSessionEntryField = value;
                    OnPropertyChanged("VehicleShortTestAsSessionEntry");
                }
            }
        }

        [DefaultValue(false)]
        public bool Pannenfall
        {
            get
            {
                return pannenfallField;
            }

            set
            {
                if (!pannenfallField.Equals(value))
                {
                    pannenfallField = value;
                    OnPropertyChanged("Pannenfall");
                }
            }
        }

        [DefaultValue(0)]
        public int SelectedDiagBUS
        {
            get
            {
                return selectedDiagBUSField;
            }

            set
            {
                if (!selectedDiagBUSField.Equals(value))
                {
                    selectedDiagBUSField = value;
                    OnPropertyChanged("SelectedDiagBUS");
                }
            }
        }

        [PreserveSource(Hint = "BackendsAvailabilityIndicator", Placeholder = true)]
        public PlaceholderType BackendsAvailabilityIndicator;
        [DefaultValue(false)]
        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.DOMRequestFailed")]
        public bool DOMRequestFailed { get; set; }

        [DefaultValue(false)]
        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.Ssl2RequestFailed")]
        public bool Ssl2RequestFailed { get; set; }

        [DefaultValue(false)]
        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.TecCampaignsRequestFailed")]
        public bool TecCampaignsRequestFailed { get; set; }

        [DefaultValue(false)]
        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.RepHistoryRequestFailed")]
        public bool RepHistoryRequestFailed { get; set; }

        [DefaultValue(false)]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.KL15OverrideVoltageCheck")]
        public bool KL15OverrideVoltageCheck { get; set; }

        [DefaultValue(false)]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.KL15FaultILevelAlreadyAlerted")]
        public bool KL15FaultILevelAlreadyAlerted { get; set; }

        [DefaultValue(false)]
        public bool GWSZReadoutSuccess
        {
            get
            {
                return gWSZReadoutSuccessField;
            }

            set
            {
                if (!gWSZReadoutSuccessField.Equals(value))
                {
                    gWSZReadoutSuccessField = value;
                    OnPropertyChanged("GWSZReadoutSuccess");
                }
            }
        }

        [Obsolete]
        public string RefSchema { get; }

        [Obsolete]
        public string Version { get; }

        public DateTime VehicleLifeStartDate
        {
            get
            {
                return vehicleLifeStartDate;
            }

            set
            {
                if (!vehicleLifeStartDate.Equals(value))
                {
                    vehicleLifeStartDate = value;
                    OnPropertyChanged("VehicleLifeStartDate");
                }
            }
        }

        public double VehicleSystemTime
        {
            get
            {
                return vehicleSystemTime;
            }

            set
            {
                if (!vehicleSystemTime.Equals(value))
                {
                    vehicleSystemTime = value;
                    OnPropertyChanged("VehicleSystemTime");
                }
            }
        }

        public string ElektrischeReichweite
        {
            get
            {
                return elektrischeReichweiteField;
            }

            set
            {
                if (elektrischeReichweiteField != value)
                {
                    elektrischeReichweiteField = value;
                    OnPropertyChanged("ElektrischeReichweite");
                }
            }
        }

        public string AEBezeichnung
        {
            get
            {
                return aeBezeichnungField;
            }

            set
            {
                if (aeBezeichnungField != value)
                {
                    aeBezeichnungField = value;
                    OnPropertyChanged("AEBezeichnung");
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
        [Obsolete]
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
                if (!string.IsNullOrEmpty(SerialGearBox) && SerialGearBox.Length >= 7)
                {
                    return SerialGearBox.Substring(0, 7);
                }

                return SerialGearBox;
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public string DisplayGwsz => Gwsz.ToMileageDisplayFormat(Classification.IsNewFaultMemoryActive);

        [XmlIgnore]
        public string VINRangeType
        {
            get
            {
                AddServiceCodeAndLogsForTypeKeys(vinRangeType, "VINRangeType");
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
        public ParameterContainer SessionDataStore => sessionDataStore;

        [XmlIgnore]
        public CcmReadoutState CcmReadoutState
        {
            get
            {
                return ccmReadoutState;
            }

            set
            {
                if (ccmReadoutState != value)
                {
                    ccmReadoutState = value;
                    OnPropertyChanged("CcmReadoutState");
                }
            }
        }

        public string VIN10Prefix
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(VIN17))
                    {
                        return null;
                    }

                    return VIN17.Substring(0, 10);
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
                if (!string.IsNullOrEmpty(MainSeriesSgbd) && MainSeriesSgbd.Length >= 3 && !MainSeriesSgbd.Equals("zcs_all"))
                {
                    return MainSeriesSgbd.Substring(0, 3);
                }

                return Ereihe;
            }
        }

        public string VIN7
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(VIN17))
                    {
                        return null;
                    }

                    return VIN17.Substring(10, 7);
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
                    if (FA != null && !string.IsNullOrEmpty(FA.TYPE) && FA.TYPE.Length == 4)
                    {
                        AddServiceCodeAndLogsForTypeKeys(FA.TYPE, "GMType.FA.TYPE");
                        return FA.TYPE;
                    }

                    if (string.IsNullOrEmpty(VIN17))
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(VINRangeType))
                    {
                        AddServiceCodeAndLogsForTypeKeys(VINRangeType, "GMType.VINRangeType");
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
                                AddServiceCodeAndLogsForTypeKeys(FA.TYPE, "GMType.VINType");
                                return VINType;
                        }

                        AddServiceCodeAndLogsForTypeKeys(FA.TYPE, "GMType.VINType");
                        return text;
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.get_GMType", exception);
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
                    if (string.IsNullOrEmpty(VIN17) || VIN17.Length < 17)
                    {
                        return null;
                    }

                    AddServiceCodeAndLogsForTypeKeys(VIN17.Substring(3, 4), "VINType");
                    return VIN17.Substring(3, 4);
                }
                catch (Exception exception)
                {
                    Log.WarningException("Vehicle.get_VINType", exception);
                }

                return null;
            }
        }

        public string EMotBaureihe => EMotor.EMOTBaureihe;

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
        [PreserveSource(Hint = "IList<Fault>", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType FaultList;
        [PreserveSource(Hint = "BlockingCollection<VirtualFaultInfo>", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType VirtualFaultInfoList;
        [XmlIgnore]
        public List<IEcu> SvtECU { get; set; } = new List<IEcu>();

        [XmlIgnore]
        [Obsolete]
        public bool IsDoIP { get; set; }

        [XmlIgnore]
        public DateTime? LastProgramDate { get; set; }

        [PreserveSource(Hint = "Database modified", SuppressWarning = true)]
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

        [PreserveSource(Hint = "ObservableCollectionEx<Fault>", Placeholder = true)]
        public PlaceholderType PKodeList => pKodeList;

        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsVehicleBreakdownAlreadyShown")]
        public bool IsVehicleBreakdownAlreadyShown { get; set; }

        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsPowerSafeModeActive")]
        public bool IsPowerSafeModeActive { get; }

        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsPowerSafeModeActiveByOldEcus")]
        public bool IsPowerSafeModeActiveByOldEcus { get; set; }

        [XmlIgnore]
        [Obsolete("Use SessionInfoAccessor.SessionInfo.IsPowerSafeModeActiveByNewEcus")]
        public bool IsPowerSafeModeActiveByNewEcus { get; set; }

        [XmlIgnore]
        public DateTime? C_DATETIME
        {
            get
            {
                try
                {
                    if (FA != null && FA.C_DATETIME.HasValue && FA.C_DATETIME > DateTime.MinValue)
                    {
                        return FA.C_DATETIME;
                    }

                    if (!string.IsNullOrEmpty(Modelljahr) && !string.IsNullOrEmpty(Modellmonat))
                    {
                        if (!cDatetimeByModelYearMonth.HasValue)
                        {
                            cDatetimeByModelYearMonth = DateTime.Parse(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-01", Modelljahr, Modellmonat), CultureInfo.InvariantCulture);
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
        PlaceholderType IVehicle.CBS => CBS;

        [PreserveSource(Hint = "IEnumerable<IDtc>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.CombinedFaults => CombinedFaults;

        [PreserveSource(Hint = "IEnumerable<IDiagCode>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.DiagCodes => DiagCodes;

        [XmlIgnore]
        IEnumerable<IEcu> IVehicle.ECU => ECU;

        [XmlIgnore]
        BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa IVehicle.FA => FA;

        [XmlIgnore]
        IEnumerable<IFfmResult> IVehicle.FFM => FFM;

        [XmlIgnore]
        IEnumerable<decimal> IVehicle.InstalledAdapters => InstalledAdapters;

        [XmlIgnore]
        IEcu IVehicle.SelectedECU => SelectedECU;

        [XmlIgnore]
        IVciDevice IVehicle.MIB => MIB;

        [PreserveSource(Hint = "IEnumerable<IServiceHistoryEntry>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.ServiceHistory => ServiceHistory;

        [PreserveSource(Hint = "IEnumerable<ITechnicalCampaign>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.TechnicalCampaigns => TechnicalCampaigns;

        [XmlIgnore]
        IVciDevice IVehicle.VCI => VCI;

        [PreserveSource(Hint = "IEnumerable<IZfsResult>", Placeholder = true)]
        [XmlIgnore]
        PlaceholderType IVehicle.ZFS => ZFS;

        [XmlIgnore]
        IEnumerable<ICemResult> IVehicle.CEM => CEM;

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

        [Obsolete("Is not used anymore in Testmodules. Will be removed in 4.48!")]
        [XmlIgnore]
        public BNMixed BNMixed { get; set; }

        [XmlIgnore]
        IReactorFa IReactorVehicle.FA
        {
            get
            {
                return FA;
            }

            set
            {
                if (FA != value)
                {
                    FA = (FA)value;
                }
            }
        }

        [XmlIgnore]
        public BordnetType BordnetType
        {
            get
            {
                return (BordnetType)BNType;
            }

            set
            {
                BNType = (BNType)value;
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

        [XmlIgnore]
        IVciDeviceRuleEvaluation IVehicleRuleEvaluation.VCI => VCI;

        [XmlIgnore]
        IList<IIdentEcu> IVehicleRuleEvaluation.ECU => GetEcusAsIIdentEcu();

        [XmlIgnore]
        IFARuleEvaluation IVehicleRuleEvaluation.FA => FA;

        [XmlIgnore]
        IFARuleEvaluation IVehicleRuleEvaluation.TargetFA => TargetFA;

        [XmlIgnore]
        IEnumerable<IEcuTreeEcu> IEcuTreeVehicle.ECU => ECU.Cast<IEcuTreeEcu>();

        [PreserveSource(Hint = "public SessionInfo", Placeholder = true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("SessionInfo")]
        public PlaceholderType SessionInfoForSerializationOnly { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        [PreserveSource(Hint = "Database modified", SignatureModified = true)]
        public string SetVINRangeTypeFromVINRanges()
        {
            //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
            //[-] if (instance != null && instance.DatabaseAccessType != DatabaseType.None && !"XXXXXXX".Equals(VIN7) && !string.IsNullOrEmpty(VIN7) && !VIN7.Equals(vinRangeTypeLastResolvedType, StringComparison.OrdinalIgnoreCase))
            //[+] PsdzDatabase database = ClientContext.GetDatabase(this);
            PsdzDatabase database = ClientContext.GetDatabase(this);
            //[+] if (database != null && !"XXXXXXX".Equals(VIN7) && !string.IsNullOrEmpty(VIN7) && !VIN7.Equals(vinRangeTypeLastResolvedType, StringComparison.OrdinalIgnoreCase))
            if (database != null && !"XXXXXXX".Equals(VIN7) && !string.IsNullOrEmpty(VIN7) && !VIN7.Equals(vinRangeTypeLastResolvedType, StringComparison.OrdinalIgnoreCase))
            {
                //[-] IVinRanges vinRangesByVin = instance.GetVinRangesByVin17(VINType, VIN7, returnFirstEntryWithoutCheck: false, IsVehicleWithOnlyVin7());
                //[+] PsdzDatabase.VinRanges vinRangesByVin = database.GetVinRangesByVin17(VINType, VIN7, returnFirstEntryWithoutCheck: false, IsVehicleWithOnlyVin7());
                PsdzDatabase.VinRanges vinRangesByVin = database.GetVinRangesByVin17(VINType, VIN7, returnFirstEntryWithoutCheck: false, IsVehicleWithOnlyVin7());
                if (vinRangesByVin != null)
                {
                    vinRangeTypeLastResolvedType = VIN7;
                    //[-] return vinRangesByVin.TYPSCHLUESSEL;
                    //[+] return vinRangesByVin.TypeKey;
                    return vinRangesByVin.TypeKey;
                }
            }

            return null;
        }

        [PreserveSource(Hint = "clientContext added", SignatureModified = true)]
        public Vehicle(ClientContext clientContext)
        {
            //[+] _clientContext = clientContext;
            _clientContext = clientContext;
            //[-] perceivedSymptomsField = new ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX>();
            //[-] installedAdaptersField = new ObservableCollection<decimal>();
            //[-] combinedFaultsField = new ObservableCollection<DTC>();
            //[-] diagCodesField = new ObservableCollection<typeDiagCode>();
            //[-] serviceHistoryField = new ObservableCollection<typeServiceHistoryEntry>();
            //[-] technicalCampaignsField = new ObservableCollection<technicalCampaignType>();
            //[-] mIBField = new VCIDevice();
            //[-] vCIField = new VCIDevice();
            //[+] mIBField = new VCIDevice(clientContext);
            mIBField = new VCIDevice(clientContext);
            //[+] vCIField = new VCIDevice(clientContext);
            vCIField = new VCIDevice(clientContext);
            //[-] testplanField = new TestPlanType();
            //[-] testPlanCache = new TestPlanCache();
            //[-] historyInfoObjectsField = new ObservableCollection<InfoObject>();
            faField = new FA();
            fFMField = new ObservableCollection<FFMResult>();
            eMotorField = new EMotor();
            heatMotorsField = new List<HeatMotor>();
            genericMotorField = new GenericMotor();
            //[-] cBSField = new ObservableCollection<typeCBSInfo>();
            selectedECUField = new ECU();
            //[-] zFSField = new ObservableCollection<ZFSResult>();
            eCUField = new ObservableCollection<ECU>();
            prodartField = "P";
            bNTypeField = BNType.UNKNOWN;
            chassisTypeField = ChassisType.UNKNOWN;
            gwszField = null;
            gwszUnitField = GwszUnitType.km;
            leistungsklasseField = "-";
            status_FunctionStateField = StateType.idle;
            pADVehicleField = false;
            pwfStateField = -1;
            applicationVersionField = "0.0.1";
            fASTAAlreadyDoneField = false;
            vehicleIdentLevelField = IdentificationLevel.None;
            vehicleShortTestAsSessionEntryField = false;
            pannenfallField = false;
            selectedDiagBUSField = 0;
            gWSZReadoutSuccessField = false;
            dealerSessionProperties = new List<DealerSessionProperty>();
            //[-] backendsAvailabilityIndicator = new BackendsAvailabilityIndicator();
            //[-]  pKodeList = new ObservableCollectionEx<Fault>();
            //[-] FaultList = new List<Fault>();
            //[-] VirtualFaultInfoList = new BlockingCollection<VirtualFaultInfo>();
            sessionDataStore = new ParameterContainer();
            //[-] Testplan = new TestPlanType(this);
            diagCodesProgramming = new ObservableCollection<string>();
            validPWFStates = new HashSet<int>(new int[17] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            clamp15MinValue = ConfigSettings.GetConfigDouble("BMW.Rheingold.ISTAGUI.Clamp15MinVoltage", 0.0);
            clamp30MinValue = new VoltageThreshold(BatteryEnum.Pb).MinError;
            //[-] RxSwin = new RxSwinData();
            //[-] checkControlMessages = new ObservableCollection<CheckControlMessage>();
            Classification = new VehicleClassification(this);
            //[+] Reactor = new Reactor(this, new NugetLogger(), new DataHolder());
            Reactor = new Reactor(this, new NugetLogger(), new DataHolder());
        //[-] SessionInfoForSerializationOnly = new SessionInfo();
        }

        protected void AddServiceCodeAndLogsForTypeKeys(string currentValue, string propertyName)
        {
            if (Environment.StackTrace.Contains("System.Runtime.Serialization") || Environment.StackTrace.Contains("BMW.Rheingold.ISTAGUI") || Environment.StackTrace.Contains("PropertyChangedEventHandler"))
            {
                return;
            }

            IFasta2Service service = ServiceLocator.Current.GetService<IFasta2Service>();
            if (service != null)
            {
            //[-] string text = "Used typeKey: " + propertyName + ",  value: " + currentValue + ". Values returned by VinValidator. TypeKey: " + string.Join(",", Validator.TypeKeys.Select((TypeKeys t) => t.TypeKey)) + ", TypeKeyBasic: " + string.Join(",", Validator.TypeKeys.Select((TypeKeys t) => t.TypeKeyBasic)) + ", TypeKeyLead: " + string.Join(",", Validator.TypeKeys.Select((TypeKeys t) => t.TypeKeyLead));
            //[-] service.AddServiceCode(ServiceCodes.IDE12_UsageOfAllTypeKeys_nu_LF, text, LayoutGroup.D);
            //[-] if (ConfigSettings.GetFeatureEnabledStatus("VinRangeUsagesLogging").IsActive)
            //[-] {
            //[-] Log.Info(Log.CurrentMethod(), text + Environment.NewLine + Environment.StackTrace);
            //[-] }
            }
        }

        [PreserveSource(Cleaned = true)]
        public List<string> PermanentSAEFehlercodesInFaultList()
        {
            List<string> list = new List<string>();
            return list;
        }

        [PreserveSource(Cleaned = true)]
        public PlaceholderType GetEnrichedFaultList(IFFMDynamicResolver ffmDynamicResolver)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
        public PlaceholderType ComputeResolveLabelsForAllFaultAsync(Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
        private PlaceholderType GetXepFaultModelLabelsByDtcFOrtEcuVariantAsync()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
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
            string ereihe = Ereihe;
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
            if (string.IsNullOrEmpty(Ereihe) || Ereihe == "UNBEK")
            {
                return false;
            }

            return true;
        }

        [PreserveSource(Cleaned = true)]
        public ECU GetECUbyDTC(decimal id)
        {
            return null;
        }

        [PreserveSource(Cleaned = true)]
        public PlaceholderType GetDTC(decimal id)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
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

            if (ECU != null)
            {
                foreach (ECU item in ECU)
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

            if (FA == null)
            {
                return false;
            }

            FA fA = ((targetFA != null) ? targetFA : FA);
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
            if (ECU != null)
            {
                foreach (ECU item in ECU)
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

            if (FFM != null)
            {
                foreach (FFMResult item in FFM)
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
            if (FFM == null || ffm == null)
            {
                return;
            }

            foreach (FFMResult item in FFM)
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

            FFM.Add(new FFMResult(ffm));
        }

        public ECU getECU(long? sgAdr)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }

            try
            {
                foreach (ECU item in ECU)
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
                foreach (ECU item in ECU)
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
            return ECU.AddIfNotContains(ecu as ECU);
        }

        public bool RemoveEcu(IEcu ecu)
        {
            return ECU.Remove(ecu as ECU);
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
                    foreach (ECU item in ECU)
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
                foreach (ECU item in ECU)
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

            if (ECU == null)
            {
                Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "ECU was null");
                return null;
            }

            try
            {
                foreach (ECU item in ECU)
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
                foreach (ECU item in ECU)
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

        [PreserveSource(Cleaned = true)]
        public PlaceholderType getCBSMeasurementValue(typeCBSMeaurementType mType)
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
        public bool addOrUpdateCBSMeasurementValue()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public bool addOrUpdateCBSMeasurementValues()
        {
            return false;
        }

        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public void AddEcu(ECU ecu)
        {
            ECU.Add(ecu);
        }

        [PreserveSource(Hint = "XEP_ECUCLIQUES removed", SignatureModified = true)]
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
            //[-] IXepEcuCliques xepEcuClique = ecu.XepEcuClique;
            //[-] if (xepEcuClique != null && xepEcuClique.IsValid)
            //[-] {
            //[-] if (ecu.XepEcuClique is XEP_ECUCLIQUES xepEcuClique2)
            //[-] {
            //[-] eCU.XepEcuClique = xepEcuClique2;
            //[-] }
            //[-] else
            //[-] {
            //[-] eCU.XepEcuClique = new XEP_ECUCLIQUES(ecu.XepEcuClique);
            //[-] }
            //[-] }
            //[-] else
            //[-] {
            //[-] eCU.XepEcuClique = new InvalidEcuClique();
            //[-] }
            //[-] if (ecu.XepEcuVariant != null)
            //[-] {
            //[-] if (ecu.XepEcuVariant is XEP_ECUVARIANTS xepEcuVariant)
            //[-] {
            //[-] eCU.XepEcuVariant = xepEcuVariant;
            //[-] }
            //[-] else
            //[-] {
            //[-] eCU.XepEcuVariant = new XEP_ECUVARIANTS(ecu.XepEcuVariant);
            //[-] }
            //[-] }
            ECU.AddIfNotContains(eCU);
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

                if (ECU == null)
                {
                    ECU = new ObservableCollection<ECU>();
                }

                foreach (ECU item in ECU)
                {
                    if (item.ID_SG_ADR == nECU.ID_SG_ADR)
                    {
                        int num = ECU.IndexOf(item);
                        if (num >= 0 && num < ECU.Count)
                        {
                            ECU[num] = nECU;
                            Log.Info("Vehicle.AddOrUpdateECU()", "updating ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                            return true;
                        }
                    }
                }

                ECU.Add(nECU);
                Log.Info("Vehicle.AddOrUpdateECU()", "adding ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.AddOrUpdateECU()", exception);
            }

            return false;
        }

        [PreserveSource(Hint = "Database modified", SignatureModified = true)]
        public bool getISTACharacteristics(decimal id, out string value, long datavalueId, ValidationRuleInternalResults internalResult)
        {
            //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
            //[-] IXepCharacteristicRoots characteristicRootsById = instance.GetCharacteristicRootsById(id);
            //[+] PsdzDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetDatabase(this)?.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
            PsdzDatabase.CharacteristicRoots characteristicRootsById = ClientContext.GetDatabase(this)?.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
            if (characteristicRootsById != null)
            {
                //[-] return new VehicleCharacteristicVehicleHelper(instance, this).GetISTACharacteristics(characteristicRootsById.Nodeclass, out value, id, this, datavalueId, internalResult);
                //[+] return new VehicleCharacteristicVehicleHelper(this).GetISTACharacteristics(characteristicRootsById.NodeClass, out value, id, this, datavalueId, internalResult);
                return new VehicleCharacteristicVehicleHelper(this).GetISTACharacteristics(characteristicRootsById.NodeClass, out value, id, this, datavalueId, internalResult);
            }

            Log.Warning("Vehicle.getISTACharactersitics()", "No entry found in CharacteristicRoots for id: {0}!", id);
            value = "???";
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public void UpdateStatus(string name, StateType type, double? progress)
        {
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

                if (string.IsNullOrEmpty(ILevel))
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
                    Log.Info("Vehicle.evalILevelExpression()", "expression:{0} vehicle iLEVEL:{1}", iLevelExpressions, ILevel);
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

                    Log.Info("Vehicle.evalILevelExpression()", "expression {0} {1}", ILevel, text);
                    if (string.Compare(ILevel, 0, array2[1], 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        switch (array2[0])
                        {
                            case ">":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "> was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "< was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case ">=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", ">= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "<= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "!=":
                            case "<>":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "!= was true");
                                }

                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))));
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
                foreach (ECU item in ECU)
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
                            obj = ILevel;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/Fahrzeugauftrag":
                            obj = FA.STANDARD_FA;
                            break;
                        case "/Result/DList":
                        case "/Result/GruppenListe":
                        {
                            string text4 = string.Empty;
                            foreach (ECU item in ECU)
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
                            foreach (string item2 in FA.SA)
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
                            foreach (string item3 in FA.E_WORT)
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
                            foreach (string item4 in FA.HO_WORT)
                            {
                                text = text + item4 + ",";
                            }

                            text = text.TrimEnd(',');
                            obj = text;
                            break;
                        }

                        case "/Result/Baustand":
                            obj = FA.C_DATE;
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

        [PreserveSource(Cleaned = true)]
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
            if (ECU != null && ECU.Count > 0)
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

        [PreserveSource(Cleaned = true)]
        private static PlaceholderType CalculateFaultList()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
        private static int? CalculateFaultCodeSum()
        {
            return null;
        }

        [PreserveSource(Cleaned = true)]
        public void AddCombinedDTC()
        {
        }

        public bool GetProgrammingEnabledForBn(string bn)
        {
            return GetBnTypes(bn).Contains(BNType);
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
            if (!string.IsNullOrWhiteSpace(VIN17))
            {
                num += VIN17.GetHashCode();
                num *= num2;
            }

            ObservableCollection<ECU> eCU = ECU;
            if (eCU != null && eCU.Any())
            {
                foreach (ECU item in ECU)
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

            if (!string.IsNullOrWhiteSpace(Ereihe))
            {
                num += Ereihe.GetHashCode();
                num *= num2;
            }

            if (!string.IsNullOrWhiteSpace(Baureihenverbund))
            {
                num += Baureihenverbund.GetHashCode();
                num *= num2;
            }

            if (C_DATETIME.HasValue)
            {
                num += C_DATETIME.GetHashCode();
                num *= num2;
            }

            return num;
        }

        [PreserveSource(Cleaned = true)]
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
            return ECU.Cast<IIdentEcu>().ToList();
        }

        IEcuTreeEcu IEcuTreeVehicle.getECU(long? sgAdr)
        {
            return getECU(sgAdr);
        }

        IEcuTreeEcu IEcuTreeVehicle.getECU(long? sgAdr, long? subAddress)
        {
            return getECU(sgAdr, subAddress);
        }

        bool IEcuTreeVehicle.AddEcu(IEcuTreeEcu ecu)
        {
            if (ecu == null)
            {
                return false;
            }

            if (ecu is ECU item)
            {
                return ECU.AddIfNotContains(item);
            }

            ECU item2 = new ECU(ecu);
            return ECU.AddIfNotContains(item2);
        }

        bool IEcuTreeVehicle.RemoveEcu(IEcuTreeEcu ecu)
        {
            if (ecu == null)
            {
                return false;
            }

            if (ecu is ECU item)
            {
                return ECU.Remove(item);
            }

            ECU item2 = new ECU(ecu);
            return ECU.Remove(item2);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [PreserveSource(Hint = "public TestPlanType", Placeholder = true)]
        public PlaceholderType TestPlanType;
        [PreserveSource(Hint = "SessionStart", Placeholder = true)]
        [XmlIgnore]
        public PlaceholderType SessionStart { get; set; }

        [PreserveSource(Added = true)]
        public Reactor Reactor { get; private set; }

        [PreserveSource(Added = true)]
        private ClientContext _clientContext;
        [PreserveSource(Added = true)]
        public ClientContext ClientContext
        {
            get
            {
                return _clientContext;
            }
        }
    }
}