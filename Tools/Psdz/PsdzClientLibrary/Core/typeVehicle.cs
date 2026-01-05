using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

#pragma warning disable CS0618, CS0169, CS0649
namespace PsdzClient.Core
{
    public abstract class typeVehicle : INotifyPropertyChanged
    {
        private string vIN17Field;
        private string serialBodyShellField;
        private string serialGearBoxField;
        private string serialEngineField;
        private BrandName? brandNameField;
        private ObservableCollection<ECU> eCUField;
        [PreserveSource(Hint = "ObservableCollection<ZFSResult>", Placeholder = true)]
        private PlaceholderType zFSField;
        private bool zFS_SUCCESSFULLYField;
        private ECU selectedECUField;
        [PreserveSource(Hint = "private ObservableCollection<typeCBSInfo>", Placeholder = true)]
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
        [PreserveSource(Hint = "private ObservableCollection<InfoObject>", Placeholder = true)]
        private PlaceholderType historyInfoObjectsField;
        [PreserveSource(Hint = "private TestPlanType", Placeholder = true)]
        private PlaceholderType testplanField;
        [PreserveSource(Hint = "private TestPlanCache", Placeholder = true)]
        private PlaceholderType testPlanCache;
        private bool simulatedPartsField;
        private VCIDevice vCIField;
        private VCIDevice mIBField;
        [PreserveSource(Hint = "private ObservableCollection<technicalCampaignType>", Placeholder = true)]
        private PlaceholderType technicalCampaignsField;
        private string leistungsklasseField;
        private string kraftstoffartField;
        private string eCTypeApprovalField;
        private DateTime lastSaveDateField;
        private DateTime lastChangeDateField;
        [PreserveSource(Hint = "private ObservableCollection<typeServiceHistoryEntry>", Placeholder = true)]
        private PlaceholderType serviceHistoryField;
        [PreserveSource(Hint = "private ObservableCollection<typeDiagCode>", Placeholder = true)]
        private PlaceholderType diagCodesField;
        private string motorarbeitsverfahrenField;
        private string drehmomentField;
        private string hybridkennzeichenField;
        [PreserveSource(Hint = "private ObservableCollection<DTC>", Placeholder = true)]
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
        [PreserveSource(Hint = "private ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX>", Placeholder = true)]
        private PlaceholderType perceivedSymptomsField;
        private string progmanVersionField;
        private string connectImageField;
        private string connectIMIBImageField;
        private VisibilityType connectIPStateField;
        private VisibilityType connectIMIBIPStateField;
        private VisibilityType connectStateField;
        private VisibilityType connectIMIBStateField;
        private string status_FunctionNameField;
        private StateType status_FunctionStateField;
        private DateTime status_FunctionStateLastChangeTimeField;
        private bool status_FunctionStateLastChangeTimeFieldSpecified;
        private double status_FunctionProgressField;
        private string kl15VoltageField;
        private string kl30VoltageField;
        private bool pADVehicleField;
        private int pwfStateField;
        private DateTime klVoltageLastMessageTimeField;
        private bool klVoltageLastMessageTimeFieldSpecified;
        private string applicationVersionField;
        private bool fASTAAlreadyDoneField;
        private IdentificationLevel vehicleIdentLevelField;
        private bool vehicleIdentAlreadyDoneField;
        private bool vehicleShortTestAsSessionEntryField;
        private bool pannenfallField;
        private int selectedDiagBUSField;
        private bool dOMRequestFailedField;
        private bool ssl2RequestFailedField;
        private bool tecCampaignsRequestFailedField;
        private bool repHistoryRequestFailedField;
        private bool kL15OverrideVoltageCheckField;
        private bool kL15FaultILevelAlreadyAlertedField;
        private bool gWSZReadoutSuccessField;
        private string refSchemaField;
        private string versionField;
        private DateTime vehicleLifeStartDate;
        private double vehicleSystemTime;
        private List<DealerSessionProperty> dealerSessionProperties;
        private DateTime productionDate;
        private string modelljahr;
        private string modellmonat;
        private string modelltag;
        private string chassisCode;
        private bool orderDataRequestFailed;
        [PreserveSource(Hint = "private BackendsAvailabilityIndicator", Placeholder = true)]
        private PlaceholderType backendsAvailabilityIndicator;
        private bool isSendFastaDataForbiddenField;
        private bool isSendOBFCMDataForbidden;
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

        public bool IsSendFastaDataForbidden
        {
            get
            {
                return isSendFastaDataForbiddenField;
            }

            set
            {
                if (!isSendFastaDataForbiddenField.Equals(value))
                {
                    isSendFastaDataForbiddenField = value;
                    OnPropertyChanged("IsSendFastaDataForbidden");
                }
            }
        }

        public bool IsSendOBFCMDataForbidden
        {
            get
            {
                return isSendOBFCMDataForbidden;
            }

            set
            {
                if (!isSendOBFCMDataForbidden.Equals(value))
                {
                    isSendOBFCMDataForbidden = value;
                    OnPropertyChanged("IsSendOBFCMDataForbidden");
                }
            }
        }

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

        public bool OrderDataRequestFailed
        {
            get
            {
                return orderDataRequestFailed;
            }

            set
            {
                if (orderDataRequestFailed != value)
                {
                    orderDataRequestFailed = value;
                    OnPropertyChanged("OrderDataRequestFailed");
                }
            }
        }

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

        [PreserveSource(Hint = "public ObservableCollection<ZFSResult>", Placeholder = true)]
        public PlaceholderType ZFS;
        public bool ZFS_SUCCESSFULLY
        {
            get
            {
                return zFS_SUCCESSFULLYField;
            }

            set
            {
                if (!zFS_SUCCESSFULLYField.Equals(value))
                {
                    zFS_SUCCESSFULLYField = value;
                    OnPropertyChanged("ZFS_SUCCESSFULLY");
                }
            }
        }

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

        [PreserveSource(Hint = "public ObservableCollection<typeCBSInfo>", Placeholder = true)]
        public PlaceholderType CBS;
        public string Typ
        {
            get
            {
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
        [PreserveSource(Hint = "public TestPlanCache TestPlanCache", Placeholder = true)]
        [IgnoreDataMember]
        [XmlIgnore]
        public PlaceholderType TestPlanCache => testPlanCache;

        public bool SimulatedParts
        {
            get
            {
                return simulatedPartsField;
            }

            set
            {
                if (!simulatedPartsField.Equals(value))
                {
                    simulatedPartsField = value;
                    OnPropertyChanged("SimulatedParts");
                }
            }
        }

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
        [PreserveSource(Hint = "public ObservableCollection<typeDiagCode>", Placeholder = true)]
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

        [PreserveSource(Hint = "public ObservableCollection<DTC>", Placeholder = true)]
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
        public string ConnectImage
        {
            get
            {
                return connectImageField;
            }

            set
            {
                if (connectImageField != null)
                {
                    if (!connectImageField.Equals(value))
                    {
                        connectImageField = value;
                        OnPropertyChanged("ConnectImage");
                    }
                }
                else
                {
                    connectImageField = value;
                    OnPropertyChanged("ConnectImage");
                }
            }
        }

        [DefaultValue("grafik/gif/icon_imib_INACTIV.gif")]
        public string ConnectIMIBImage
        {
            get
            {
                return connectIMIBImageField;
            }

            set
            {
                if (connectIMIBImageField != null)
                {
                    if (!connectIMIBImageField.Equals(value))
                    {
                        connectIMIBImageField = value;
                        OnPropertyChanged("ConnectIMIBImage");
                    }
                }
                else
                {
                    connectIMIBImageField = value;
                    OnPropertyChanged("ConnectIMIBImage");
                }
            }
        }

        [DefaultValue(VisibilityType.Visible)]
        public VisibilityType ConnectIPState
        {
            get
            {
                return connectIPStateField;
            }

            set
            {
                if (!connectIPStateField.Equals(value))
                {
                    connectIPStateField = value;
                    OnPropertyChanged("ConnectIPState");
                }
            }
        }

        [DefaultValue(VisibilityType.Visible)]
        public VisibilityType ConnectIMIBIPState
        {
            get
            {
                return connectIMIBIPStateField;
            }

            set
            {
                if (!connectIMIBIPStateField.Equals(value))
                {
                    connectIMIBIPStateField = value;
                    OnPropertyChanged("ConnectIMIBIPState");
                }
            }
        }

        [DefaultValue(VisibilityType.Visible)]
        public VisibilityType ConnectState
        {
            get
            {
                return connectStateField;
            }

            set
            {
                if (!connectStateField.Equals(value))
                {
                    connectStateField = value;
                    OnPropertyChanged("ConnectState");
                }
            }
        }

        [DefaultValue(VisibilityType.Visible)]
        public VisibilityType ConnectIMIBState
        {
            get
            {
                return connectIMIBStateField;
            }

            set
            {
                if (!connectIMIBStateField.Equals(value))
                {
                    connectIMIBStateField = value;
                    OnPropertyChanged("ConnectIMIBState");
                }
            }
        }

        public string Status_FunctionName
        {
            get
            {
                return status_FunctionNameField;
            }

            set
            {
                if (status_FunctionNameField != null)
                {
                    if (!status_FunctionNameField.Equals(value))
                    {
                        status_FunctionNameField = value;
                        OnPropertyChanged("Status_FunctionName");
                    }
                }
                else
                {
                    status_FunctionNameField = value;
                    OnPropertyChanged("Status_FunctionName");
                }
            }
        }

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

        public DateTime Status_FunctionStateLastChangeTime
        {
            get
            {
                return status_FunctionStateLastChangeTimeField;
            }

            set
            {
                if (!status_FunctionStateLastChangeTimeField.Equals(value))
                {
                    status_FunctionStateLastChangeTimeField = value;
                    OnPropertyChanged("Status_FunctionStateLastChangeTime");
                }
            }
        }

        [XmlIgnore]
        public bool Status_FunctionStateLastChangeTimeSpecified
        {
            get
            {
                return status_FunctionStateLastChangeTimeFieldSpecified;
            }

            set
            {
                if (!status_FunctionStateLastChangeTimeFieldSpecified.Equals(value))
                {
                    status_FunctionStateLastChangeTimeFieldSpecified = value;
                    OnPropertyChanged("Status_FunctionStateLastChangeTimeSpecified");
                }
            }
        }

        public double Status_FunctionProgress
        {
            get
            {
                return status_FunctionProgressField;
            }

            set
            {
                if (!status_FunctionProgressField.Equals(value))
                {
                    status_FunctionProgressField = value;
                    OnPropertyChanged("Status_FunctionProgress");
                }
            }
        }

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
        public bool VehicleIdentAlreadyDone
        {
            get
            {
                return vehicleIdentAlreadyDoneField;
            }

            set
            {
                if (vehicleIdentAlreadyDoneField != value)
                {
                    vehicleIdentAlreadyDoneField = value;
                    OnPropertyChanged("VehicleIdentAlreadyDone");
                    Log.Info(Log.CurrentMethod(), "VehicleIdentAlreadyDone '{0}'", vehicleIdentAlreadyDoneField);
                }
            }
        }

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

        [PreserveSource(Hint = "public BackendsAvailabilityIndicator", Placeholder = true)]
        public PlaceholderType BackendsAvailabilityIndicator;
        [DefaultValue(false)]
        [XmlIgnore]
        public bool DOMRequestFailed
        {
            get
            {
                return dOMRequestFailedField;
            }

            set
            {
                if (!dOMRequestFailedField.Equals(value))
                {
                    dOMRequestFailedField = value;
                    OnPropertyChanged("DOMRequestFailed");
                }
            }
        }

        [DefaultValue(false)]
        [XmlIgnore]
        public bool Ssl2RequestFailed
        {
            get
            {
                return ssl2RequestFailedField;
            }

            set
            {
                if (!ssl2RequestFailedField.Equals(value))
                {
                    ssl2RequestFailedField = value;
                    OnPropertyChanged("Ssl2RequestFailed");
                }
            }
        }

        [DefaultValue(false)]
        [XmlIgnore]
        public bool TecCampaignsRequestFailed
        {
            get
            {
                return tecCampaignsRequestFailedField;
            }

            set
            {
                if (!tecCampaignsRequestFailedField.Equals(value))
                {
                    tecCampaignsRequestFailedField = value;
                    OnPropertyChanged("TecCampaignsRequestFailed");
                }
            }
        }

        [DefaultValue(false)]
        [XmlIgnore]
        public bool RepHistoryRequestFailed
        {
            get
            {
                return repHistoryRequestFailedField;
            }

            set
            {
                if (!repHistoryRequestFailedField.Equals(value))
                {
                    repHistoryRequestFailedField = value;
                    OnPropertyChanged("RepHistoryRequestFailed");
                }
            }
        }

        [DefaultValue(false)]
        public bool KL15OverrideVoltageCheck
        {
            get
            {
                return kL15OverrideVoltageCheckField;
            }

            set
            {
                if (!kL15OverrideVoltageCheckField.Equals(value))
                {
                    kL15OverrideVoltageCheckField = value;
                    OnPropertyChanged("KL15OverrideVoltageCheck");
                }
            }
        }

        [DefaultValue(false)]
        public bool KL15FaultILevelAlreadyAlerted
        {
            get
            {
                return kL15FaultILevelAlreadyAlertedField;
            }

            set
            {
                if (!kL15FaultILevelAlreadyAlertedField.Equals(value))
                {
                    kL15FaultILevelAlreadyAlertedField = value;
                    OnPropertyChanged("KL15FaultILevelAlreadyAlerted");
                }
            }
        }

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

        public string RefSchema
        {
            get
            {
                return refSchemaField;
            }

            set
            {
                if (refSchemaField != null)
                {
                    if (!refSchemaField.Equals(value))
                    {
                        refSchemaField = value;
                        OnPropertyChanged("refSchema");
                    }
                }
                else
                {
                    refSchemaField = value;
                    OnPropertyChanged("refSchema");
                }
            }
        }

        public string Version
        {
            get
            {
                return versionField;
            }

            set
            {
                if (versionField != null)
                {
                    if (!versionField.Equals(value))
                    {
                        versionField = value;
                        OnPropertyChanged("version");
                    }
                }
                else
                {
                    versionField = value;
                    OnPropertyChanged("version");
                }
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        [PreserveSource(Hint = "added clientContext", SignatureModified = true)]
        public typeVehicle(ClientContext clientContext)
        {
            //[+] _clientContext = clientContext;
            _clientContext = clientContext;
            //[-] perceivedSymptomsField = new ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX>();
            installedAdaptersField = new ObservableCollection<decimal>();
            //[-] combinedFaultsField = new ObservableCollection<DTC>();
            //[-] diagCodesField = new ObservableCollection<typeDiagCode>();
            //[-] serviceHistoryField = new ObservableCollection<typeServiceHistoryEntry>();
            //[-] technicalCampaignsField = new ObservableCollection<technicalCampaignType>();
            //[-] mIBField = new VCIDevice();
            //[+] mIBField = new VCIDevice(clientContext);
            mIBField = new VCIDevice(clientContext);
            //[-] vCIField = new VCIDevice();
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
            zFS_SUCCESSFULLYField = false;
            prodartField = "P";
            bNTypeField = BNType.UNKNOWN;
            chassisTypeField = ChassisType.UNKNOWN;
            gwszField = null;
            gwszUnitField = GwszUnitType.km;
            simulatedPartsField = false;
            leistungsklasseField = "-";
            connectImageField = "grafik/gif/icon_offl_ACTIV.gif";
            connectIMIBImageField = "grafik/gif/icon_imib_INACTIV.gif";
            connectIPStateField = VisibilityType.Visible;
            connectIMIBIPStateField = VisibilityType.Visible;
            connectStateField = VisibilityType.Visible;
            connectIMIBStateField = VisibilityType.Visible;
            status_FunctionStateField = StateType.idle;
            pADVehicleField = false;
            pwfStateField = -1;
            applicationVersionField = "0.0.1";
            fASTAAlreadyDoneField = false;
            vehicleIdentLevelField = IdentificationLevel.None;
            vehicleIdentAlreadyDoneField = false;
            vehicleShortTestAsSessionEntryField = false;
            pannenfallField = false;
            selectedDiagBUSField = 0;
            dOMRequestFailedField = false;
            ssl2RequestFailedField = false;
            tecCampaignsRequestFailedField = false;
            repHistoryRequestFailedField = false;
            kL15OverrideVoltageCheckField = false;
            kL15FaultILevelAlreadyAlertedField = false;
            gWSZReadoutSuccessField = false;
            refSchemaField = "http://www.bmw.com/Rheingold/Vehicle.xsd";
            versionField = "3.42.20.10700";
            dealerSessionProperties = new List<DealerSessionProperty>();
            //[-] backendsAvailabilityIndicator = new BackendsAvailabilityIndicator();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [PreserveSource(Hint = "Added")]
        private ClientContext _clientContext;
        [PreserveSource(Hint = "Added")]
        public ClientContext ClientContext
        {
            get
            {
                return _clientContext;
            }
        }
    }
}