using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public abstract class typeVehicle : INotifyPropertyChanged
	{
		public typeVehicle(ClientContext clientContext)
        {
            this._clientContext = clientContext;
			//this.perceivedSymptomsField = new ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX>();
			this.installedAdaptersField = new ObservableCollection<decimal>();
			//this.combinedFaultsField = new ObservableCollection<DTC>();
			//this.diagCodesField = new ObservableCollection<typeDiagCode>();
			//this.serviceHistoryField = new ObservableCollection<typeServiceHistoryEntry>();
			//this.technicalCampaignsField = new ObservableCollection<technicalCampaignType>();
			//this.mIBField = new VCIDevice();
			this.vCIField = new VCIDevice(clientContext);
			//this.testplanField = new TestPlanType();
			//this.historyInfoObjectsField = new ObservableCollection<InfoObject>();
			//this.faField = new FA();
			//this.fFMField = new ObservableCollection<FFMResult>();
			this.eMotorField = new EMotor();
			this.heatMotorsField = new List<HeatMotor>();
			this.genericMotorField = new GenericMotor();
			//this.cBSField = new ObservableCollection<typeCBSInfo>();
			this.selectedECUField = new ECU();
			//this.zFSField = new ObservableCollection<ZFSResult>();
			this.eCUField = new ObservableCollection<ECU>();
			this.zFS_SUCCESSFULLYField = false;
			this.prodartField = "P";
			this.bNTypeField = BNType.UNKNOWN;
			this.bNMixedField = BNMixed.UNKNOWN;
			this.chassisTypeField = ChassisType.UNKNOWN;
			this.gwszField = null;
			this.gwszUnitField = new GwszUnitType?(GwszUnitType.km);
			//this.simulatedPartsField = false;
			this.leistungsklasseField = "-";
			this.connectImageField = "grafik/gif/icon_offl_ACTIV.gif";
			this.connectIMIBImageField = "grafik/gif/icon_imib_INACTIV.gif";
			this.connectIPStateField = VisibilityType.Visible;
			this.connectIMIBIPStateField = VisibilityType.Visible;
			this.connectStateField = VisibilityType.Visible;
			this.connectIMIBStateField = VisibilityType.Visible;
			this.status_FunctionStateField = StateType.idle;
			this.pADVehicleField = false;
			this.pwfStateField = -1;
			this.applicationVersionField = "0.0.1";
			this.fASTAAlreadyDoneField = false;
			this.vehicleIdentLevelField = IdentificationLevel.None;
			this.vehicleIdentAlreadyDoneField = false;
			this.vehicleShortTestAsSessionEntryField = false;
			this.pannenfallField = false;
			this.selectedDiagBUSField = 0;
			this.dOMRequestFailedField = false;
			this.cVDRequestFailedField = false;
			this.cvsRequestFailedField = false;
			this.tecCampaignsRequestFailedField = false;
			this.repHistoryRequestFailedField = false;
			this.kL15OverrideVoltageCheckField = false;
			this.kL15FaultILevelAlreadyAlertedField = false;
			this.gWSZReadoutSuccessField = false;
			this.refSchemaField = "http://www.bmw.com/Rheingold/Vehicle.xsd";
			this.versionField = "3.42.20.10700";
			this.dealerSessionProperties = new List<DealerSessionProperty>();
		}

		public string VIN17
		{
			get
			{
				return this.vIN17Field;
			}
			set
			{
				if (this.vIN17Field != null)
				{
					if (!this.vIN17Field.Equals(value))
					{
						this.vIN17Field = value;
						this.OnPropertyChanged("VIN17");
						return;
					}
				}
				else
				{
					this.vIN17Field = value;
					this.OnPropertyChanged("VIN17");
				}
			}
		}

		public bool IsSendFastaDataForbidden
		{
			get
			{
				return this.isSendFastaDataForbiddenField;
			}
			set
			{
				if (!this.isSendFastaDataForbiddenField.Equals(value))
				{
					this.isSendFastaDataForbiddenField = value;
					this.OnPropertyChanged("IsSendFastaDataForbidden");
				}
			}
		}

		public bool IsSendFastaDataForbiddenBitsQueueFull
		{
			get
			{
				return this.isSendFastaDataForbiddenBitsQueueFullField;
			}
			set
			{
				if (!this.isSendFastaDataForbiddenBitsQueueFullField.Equals(value))
				{
					this.isSendFastaDataForbiddenBitsQueueFullField = value;
					this.OnPropertyChanged("IsSendFastaDataForbidden");
				}
			}
		}

		public string SerialBodyShell
		{
			get
			{
				return this.serialBodyShellField;
			}
			set
			{
				if (this.serialBodyShellField != null)
				{
					if (!this.serialBodyShellField.Equals(value))
					{
						this.serialBodyShellField = value;
						this.OnPropertyChanged("SerialBodyShell");
						return;
					}
				}
				else
				{
					this.serialBodyShellField = value;
					this.OnPropertyChanged("SerialBodyShell");
				}
			}
		}

		public string SerialGearBox
		{
			get
			{
				return this.serialGearBoxField;
			}
			set
			{
				if (this.serialGearBoxField != null)
				{
					if (!this.serialGearBoxField.Equals(value))
					{
						this.serialGearBoxField = value;
						this.OnPropertyChanged("SerialGearBox");
						return;
					}
				}
				else
				{
					this.serialGearBoxField = value;
					this.OnPropertyChanged("SerialGearBox");
				}
			}
		}

		public string SerialEngine
		{
			get
			{
				return this.serialEngineField;
			}
			set
			{
				if (this.serialEngineField != null)
				{
					if (!this.serialEngineField.Equals(value))
					{
						this.serialEngineField = value;
						this.OnPropertyChanged("SerialEngine");
						return;
					}
				}
				else
				{
					this.serialEngineField = value;
					this.OnPropertyChanged("SerialEngine");
				}
			}
		}

		public List<DealerSessionProperty> DealerSessionProperties
		{
			get
			{
				return this.dealerSessionProperties;
			}
			set
			{
				if (this.dealerSessionProperties != null)
				{
					if (!this.dealerSessionProperties.Equals(value))
					{
						this.dealerSessionProperties = value;
						return;
					}
				}
				else
				{
					this.dealerSessionProperties = value;
				}
			}
		}

		public BrandName? BrandName
		{
			get
			{
				return this.brandNameField;
			}
			set
			{
				if (this.brandNameField != null)
				{
					if (!this.brandNameField.Equals(value))
					{
						this.brandNameField = value;
						this.OnPropertyChanged("BrandName");
						return;
					}
				}
				else
				{
					this.brandNameField = value;
					this.OnPropertyChanged("BrandName");
				}
			}
		}

		public ObservableCollection<ECU> ECU
		{
			get
			{
				return this.eCUField;
			}
			set
			{
				if (this.eCUField != null)
				{
					if (!this.eCUField.Equals(value))
					{
						this.eCUField = value;
						this.OnPropertyChanged("ECU");
						return;
					}
				}
				else
				{
					this.eCUField = value;
					this.OnPropertyChanged("ECU");
				}
			}
		}
#if false
		public ObservableCollection<ZFSResult> ZFS
		{
			get
			{
				return this.zFSField;
			}
			set
			{
				if (this.zFSField != null)
				{
					if (!this.zFSField.Equals(value))
					{
						this.zFSField = value;
						this.OnPropertyChanged("ZFS");
						return;
					}
				}
				else
				{
					this.zFSField = value;
					this.OnPropertyChanged("ZFS");
				}
			}
		}
#endif
		public bool ZFS_SUCCESSFULLY
		{
			get
			{
				return this.zFS_SUCCESSFULLYField;
			}
			set
			{
				if (!this.zFS_SUCCESSFULLYField.Equals(value))
				{
					this.zFS_SUCCESSFULLYField = value;
					this.OnPropertyChanged("ZFS_SUCCESSFULLY");
				}
			}
		}

		public ECU SelectedECU
		{
			get
			{
				return this.selectedECUField;
			}
			set
			{
				if (this.selectedECUField != null)
				{
					if (!this.selectedECUField.Equals(value))
					{
						this.selectedECUField = value;
						this.OnPropertyChanged("SelectedECU");
						return;
					}
				}
				else
				{
					this.selectedECUField = value;
					this.OnPropertyChanged("SelectedECU");
				}
			}
		}
#if false
		public ObservableCollection<typeCBSInfo> CBS
		{
			get
			{
				return this.cBSField;
			}
			set
			{
				if (this.cBSField != null)
				{
					if (!this.cBSField.Equals(value))
					{
						this.cBSField = value;
						this.OnPropertyChanged("CBS");
						return;
					}
				}
				else
				{
					this.cBSField = value;
					this.OnPropertyChanged("CBS");
				}
			}
		}
#endif
		public string Typ
		{
			get
			{
				return this.typField;
			}
			set
			{
				if (this.typField != null)
				{
					if (!this.typField.Equals(value))
					{
						this.typField = value;
						this.OnPropertyChanged("Typ");
						return;
					}
				}
				else
				{
					this.typField = value;
					this.OnPropertyChanged("Typ");
				}
			}
		}

		public string BasicType
		{
			get
			{
				return this.basicTypeField;
			}
			set
			{
				if (this.basicTypeField != null)
				{
					if (!this.basicTypeField.Equals(value))
					{
						this.basicTypeField = value;
						this.OnPropertyChanged("BasicType");
						return;
					}
				}
				else
				{
					this.basicTypeField = value;
					this.OnPropertyChanged("BasicType");
				}
			}
		}

		public string DriveType
		{
			get
			{
				return this.driveTypeField;
			}
			set
			{
				if (this.driveTypeField != null)
				{
					if (!this.driveTypeField.Equals(value))
					{
						this.driveTypeField = value;
						this.OnPropertyChanged("DriveType");
						return;
					}
				}
				else
				{
					this.driveTypeField = value;
					this.OnPropertyChanged("DriveType");
				}
			}
		}

		public string WarrentyType
		{
			get
			{
				return this.warrentyTypeField;
			}
			set
			{
				if (this.warrentyTypeField != null)
				{
					if (!this.warrentyTypeField.Equals(value))
					{
						this.warrentyTypeField = value;
						this.OnPropertyChanged("WarrentyType");
						return;
					}
				}
				else
				{
					this.warrentyTypeField = value;
					this.OnPropertyChanged("WarrentyType");
				}
			}
		}

		public string Marke
		{
			get
			{
				return this.markeField;
			}
			set
			{
				if (this.markeField != null)
				{
					if (!this.markeField.Equals(value))
					{
						this.markeField = value;
						this.OnPropertyChanged("Marke");
						return;
					}
				}
				else
				{
					this.markeField = value;
					this.OnPropertyChanged("Marke");
				}
			}
		}

		public string Ueberarbeitung
		{
			get
			{
				return this.ueberarbeitungField;
			}
			set
			{
				if (this.ueberarbeitungField != null)
				{
					if (!this.ueberarbeitungField.Equals(value))
					{
						this.ueberarbeitungField = value;
						this.OnPropertyChanged("Ueberarbeitung");
						return;
					}
				}
				else
				{
					this.ueberarbeitungField = value;
					this.OnPropertyChanged("Ueberarbeitung");
				}
			}
		}

		[DefaultValue("P")]
		public string Prodart
		{
			get
			{
				return this.prodartField;
			}
			set
			{
				if (this.prodartField != null)
				{
					if (!this.prodartField.Equals(value))
					{
						this.prodartField = value;
						this.OnPropertyChanged("Prodart");
						return;
					}
				}
				else
				{
					this.prodartField = value;
					this.OnPropertyChanged("Prodart");
				}
			}
		}

		public string Ereihe
		{
			get
			{
				return this.ereiheField;
			}
			set
			{
				if (this.ereiheField != null)
				{
					if (!this.ereiheField.Equals(value))
					{
						this.ereiheField = value;
						this.OnPropertyChanged("Ereihe");
						return;
					}
				}
				else
				{
					this.ereiheField = value;
					this.OnPropertyChanged("Ereihe");
				}
			}
		}

		public string Gsgbd
		{
			get
			{
				return this.gsgbdField;
			}
			set
			{
				if (this.gsgbdField != null)
				{
					if (!this.gsgbdField.Equals(value))
					{
						this.gsgbdField = value;
						this.OnPropertyChanged("Gsgbd");
						return;
					}
				}
				else
				{
					this.gsgbdField = value;
					this.OnPropertyChanged("Gsgbd");
				}
			}
		}

		[DefaultValue(BNType.UNKNOWN)]
		public BNType BNType
		{
			get
			{
				return this.bNTypeField;
			}
			set
			{
				if (!this.bNTypeField.Equals(value))
				{
					this.bNTypeField = value;
					this.OnPropertyChanged("BNType");
				}
			}
		}

		public BNMixed BNMixed
		{
			get
			{
				return this.bNMixedField;
			}
			set
			{
				if (!this.bNMixedField.Equals(value))
				{
					this.bNMixedField = value;
					this.OnPropertyChanged("BNMixed");
				}
			}
		}

		public string Baureihe
		{
			get
			{
				return this.baureiheField;
			}
			set
			{
				if (this.baureiheField != null)
				{
					if (!this.baureiheField.Equals(value))
					{
						this.baureiheField = value;
						this.OnPropertyChanged("Baureihe");
						return;
					}
				}
				else
				{
					this.baureiheField = value;
					this.OnPropertyChanged("Baureihe");
				}
			}
		}

		public string VerkaufsBezeichnung
		{
			get
			{
				return this.verkaufsBezeichnungField;
			}
			set
			{
				if (this.verkaufsBezeichnungField != null)
				{
					if (!this.verkaufsBezeichnungField.Equals(value))
					{
						this.verkaufsBezeichnungField = value;
						this.OnPropertyChanged("VerkaufsBezeichnung");
						return;
					}
				}
				else
				{
					this.verkaufsBezeichnungField = value;
					this.OnPropertyChanged("VerkaufsBezeichnung");
				}
			}
		}

		public string RoadMap
		{
			get
			{
				return this.roadMapField;
			}
			set
			{
				if (this.roadMapField != null)
				{
					if (!this.roadMapField.Equals(value))
					{
						this.roadMapField = value;
						this.OnPropertyChanged("RoadMap");
						return;
					}
				}
				else
				{
					this.roadMapField = value;
					this.OnPropertyChanged("RoadMap");
				}
			}
		}

		public ChassisType ChassisType
		{
			get
			{
				return this.chassisTypeField;
			}
			set
			{
				if (!this.chassisTypeField.Equals(value))
				{
					this.chassisTypeField = value;
					this.OnPropertyChanged("ChassisType");
				}
			}
		}

		public string Karosserie
		{
			get
			{
				return this.karosserieField;
			}
			set
			{
				if (this.karosserieField != null)
				{
					if (!this.karosserieField.Equals(value))
					{
						this.karosserieField = value;
						this.OnPropertyChanged("Karosserie");
						return;
					}
				}
				else
				{
					this.karosserieField = value;
					this.OnPropertyChanged("Karosserie");
				}
			}
		}

		public EMotor EMotor
		{
			get
			{
				return this.eMotorField;
			}
			set
			{
				if (this.eMotorField != null)
				{
					if (!this.eMotorField.Equals(value))
					{
						this.eMotorField = value;
						this.OnPropertyChanged("EMotor");
						return;
					}
				}
				else
				{
					this.eMotorField = value;
					this.OnPropertyChanged("EMotor");
				}
			}
		}

		public List<HeatMotor> HeatMotors
		{
			get
			{
				return this.heatMotorsField;
			}
			set
			{
				if (this.heatMotorsField != null)
				{
					if (!this.heatMotorsField.Equals(value))
					{
						this.heatMotorsField = value;
						this.OnPropertyChanged("HeatMotors");
						return;
					}
				}
				else
				{
					this.heatMotorsField = value;
					this.OnPropertyChanged("HeatMotors");
				}
			}
		}

		public GenericMotor GenericMotor
		{
			get
			{
				return this.genericMotorField;
			}
			set
			{
				if (this.genericMotorField != null)
				{
					if (!this.genericMotorField.Equals(value))
					{
						this.genericMotorField = value;
						this.OnPropertyChanged("GenericMotor");
						return;
					}
				}
				else
				{
					this.genericMotorField = value;
					this.OnPropertyChanged("GenericMotor");
				}
			}
		}

		public string Motor
		{
			get
			{
				return this.motorField;
			}
			set
			{
				if (this.motorField != null)
				{
					if (!this.motorField.Equals(value))
					{
						this.motorField = value;
						this.GenericMotor.Engine1 = value;
						this.OnPropertyChanged("Motor");
						return;
					}
				}
				else
				{
					this.motorField = value;
					this.GenericMotor.Engine1 = value;
					this.OnPropertyChanged("Motor");
				}
			}
		}

		public string Hubraum
		{
			get
			{
				return this.hubraumField;
			}
			set
			{
				if (this.hubraumField != null)
				{
					if (!this.hubraumField.Equals(value))
					{
						this.hubraumField = value;
						this.OnPropertyChanged("Hubraum");
						return;
					}
				}
				else
				{
					this.hubraumField = value;
					this.OnPropertyChanged("Hubraum");
				}
			}
		}

		public string Land
		{
			get
			{
				return this.landField;
			}
			set
			{
				if (this.landField != null)
				{
					if (!this.landField.Equals(value))
					{
						this.landField = value;
						this.OnPropertyChanged("Land");
						return;
					}
				}
				else
				{
					this.landField = value;
					this.OnPropertyChanged("Land");
				}
			}
		}

		public string Lenkung
		{
			get
			{
				return this.lenkungField;
			}
			set
			{
				if (this.lenkungField != null)
				{
					if (!this.lenkungField.Equals(value))
					{
						this.lenkungField = value;
						this.OnPropertyChanged("Lenkung");
						return;
					}
				}
				else
				{
					this.lenkungField = value;
					this.OnPropertyChanged("Lenkung");
				}
			}
		}

		public string Getriebe
		{
			get
			{
				return this.getriebeField;
			}
			set
			{
				if (this.getriebeField != null)
				{
					if (!this.getriebeField.Equals(value))
					{
						this.getriebeField = value;
						this.OnPropertyChanged("Getriebe");
						return;
					}
				}
				else
				{
					this.getriebeField = value;
					this.OnPropertyChanged("Getriebe");
				}
			}
		}

		public string CountryOfAssembly
		{
			get
			{
				return this.countryOfAssemblyField;
			}
			set
			{
				if (this.countryOfAssemblyField != null)
				{
					if (!this.countryOfAssemblyField.Equals(value))
					{
						this.countryOfAssemblyField = value;
						this.OnPropertyChanged("CountryOfAssembly");
						return;
					}
				}
				else
				{
					this.countryOfAssemblyField = value;
					this.OnPropertyChanged("CountryOfAssembly");
				}
			}
		}

		public string BaseVersion
		{
			get
			{
				return this.baseVersionField;
			}
			set
			{
				if (this.baseVersionField != null)
				{
					if (!this.baseVersionField.Equals(value))
					{
						this.baseVersionField = value;
						this.OnPropertyChanged("BaseVersion");
						return;
					}
				}
				else
				{
					this.baseVersionField = value;
					this.OnPropertyChanged("BaseVersion");
				}
			}
		}

		public string Antrieb
		{
			get
			{
				return this.antriebField;
			}
			set
			{
				if (this.antriebField != null)
				{
					if (!this.antriebField.Equals(value))
					{
						this.antriebField = value;
						this.OnPropertyChanged("Antrieb");
						return;
					}
				}
				else
				{
					this.antriebField = value;
					this.OnPropertyChanged("Antrieb");
				}
			}
		}

		public string Abgas
		{
			get
			{
				return this.abgasField;
			}
			set
			{
				if (this.abgasField != null)
				{
					if (!this.abgasField.Equals(value))
					{
						this.abgasField = value;
						this.OnPropertyChanged("Abgas");
						return;
					}
				}
				else
				{
					this.abgasField = value;
					this.OnPropertyChanged("Abgas");
				}
			}
		}

		public DateTime ProductionDate
		{
			get
			{
				return this.productionDateField;
			}
			set
			{
				if (!this.productionDateField.Equals(value))
				{
					this.productionDateField = value;
					this.OnPropertyChanged("ProductionDate");
				}
			}
		}

		[XmlIgnore]
		public bool ProductionDateSpecified
		{
			get
			{
				return this.productionDateFieldSpecified;
			}
			set
			{
				if (!this.productionDateFieldSpecified.Equals(value))
				{
					this.productionDateFieldSpecified = value;
					this.OnPropertyChanged("ProductionDateSpecified");
				}
			}
		}

		public DateTime? FirstRegistration
		{
			get
			{
				return this.firstRegistrationField;
			}
			set
			{
				if (this.firstRegistrationField != null)
				{
					if (!this.firstRegistrationField.Equals(value))
					{
						this.firstRegistrationField = value;
						this.OnPropertyChanged("FirstRegistration");
						return;
					}
				}
				else
				{
					this.firstRegistrationField = value;
					this.OnPropertyChanged("FirstRegistration");
				}
			}
		}

		public string Modelljahr
		{
			get
			{
				return this.modelljahrField;
			}
			set
			{
				if (this.modelljahrField != null)
				{
					if (!this.modelljahrField.Equals(value))
					{
						this.modelljahrField = value;
						this.OnPropertyChanged("Modelljahr");
						return;
					}
				}
				else
				{
					this.modelljahrField = value;
					this.OnPropertyChanged("Modelljahr");
				}
			}
		}

		public string Modellmonat
		{
			get
			{
				return this.modellmonatField;
			}
			set
			{
				if (this.modellmonatField != null)
				{
					if (!this.modellmonatField.Equals(value))
					{
						this.modellmonatField = value;
						this.OnPropertyChanged("Modellmonat");
						return;
					}
				}
				else
				{
					this.modellmonatField = value;
					this.OnPropertyChanged("Modellmonat");
				}
			}
		}

		public string Modelltag
		{
			get
			{
				return this.modelltagField;
			}
			set
			{
				if (this.modelltagField != null)
				{
					if (!this.modelltagField.Equals(value))
					{
						this.modelltagField = value;
						this.OnPropertyChanged("Modelltag");
						return;
					}
				}
				else
				{
					this.modelltagField = value;
					this.OnPropertyChanged("Modelltag");
				}
			}
		}

		public string BaustandsJahr
		{
			get
			{
				return this.baustandsJahrField;
			}
			set
			{
				if (this.baustandsJahrField != null)
				{
					if (!this.baustandsJahrField.Equals(value))
					{
						this.baustandsJahrField = value;
						this.OnPropertyChanged("BaustandsJahr");
						return;
					}
				}
				else
				{
					this.baustandsJahrField = value;
					this.OnPropertyChanged("BaustandsJahr");
				}
			}
		}

		public string BaustandsMonat
		{
			get
			{
				return this.baustandsMonatField;
			}
			set
			{
				if (this.baustandsMonatField != null)
				{
					if (!this.baustandsMonatField.Equals(value))
					{
						this.baustandsMonatField = value;
						this.OnPropertyChanged("BaustandsMonat");
						return;
					}
				}
				else
				{
					this.baustandsMonatField = value;
					this.OnPropertyChanged("BaustandsMonat");
				}
			}
		}

		public string ILevel
		{
			get
			{
				return this.iLevelField;
			}
			set
			{
				if (this.iLevelField != null)
				{
					if (!this.iLevelField.Equals(value))
					{
						this.iLevelField = value;
						this.OnPropertyChanged("ILevel");
						return;
					}
				}
				else
				{
					this.iLevelField = value;
					this.OnPropertyChanged("ILevel");
				}
			}
		}

		public decimal? Gwsz
		{
			get
			{
				return this.gwszField;
			}
			set
			{
				if (this.gwszField != null)
				{
					if (!this.gwszField.Equals(value))
					{
						this.gwszField = value;
						this.OnPropertyChanged("Gwsz");
						return;
					}
				}
				else
				{
					this.gwszField = value;
					this.OnPropertyChanged("Gwsz");
				}
			}
		}

		public GwszUnitType? GwszUnit
		{
			get
			{
				return this.gwszUnitField;
			}
			set
			{
				if (this.gwszUnitField != null)
				{
					if (!this.gwszUnitField.Equals(value))
					{
						this.gwszUnitField = value;
						this.OnPropertyChanged("GwszUnit");
						return;
					}
				}
				else
				{
					this.gwszUnitField = value;
					this.OnPropertyChanged("GwszUnit");
				}
			}
		}

		public ObservableCollection<FFMResult> FFM
		{
			get
			{
				return this.fFMField;
			}
			set
			{
				if (this.fFMField != null)
				{
					if (!this.fFMField.Equals(value))
					{
						this.fFMField = value;
						this.OnPropertyChanged("FFM");
						return;
					}
				}
				else
				{
					this.fFMField = value;
					this.OnPropertyChanged("FFM");
				}
			}
		}

		public string ILevelWerk
		{
			get
			{
				return this.iLevelWerkField;
			}
			set
			{
				if (this.iLevelWerkField != null)
				{
					if (!this.iLevelWerkField.Equals(value))
					{
						this.iLevelWerkField = value;
						this.OnPropertyChanged("ILevelWerk");
						return;
					}
				}
				else
				{
					this.iLevelWerkField = value;
					this.OnPropertyChanged("ILevelWerk");
				}
			}
		}

		public string ILevelBackup
		{
			get
			{
				return this.iLevelBackupField;
			}
			set
			{
				if (this.iLevelBackupField != null)
				{
					if (!this.iLevelBackupField.Equals(value))
					{
						this.iLevelBackupField = value;
						this.OnPropertyChanged("ILevelBackup");
						return;
					}
				}
				else
				{
					this.iLevelBackupField = value;
					this.OnPropertyChanged("ILevelBackup");
				}
			}
		}

		public FA FA
		{
			get
			{
				return this.faField;
			}
			set
			{
				if (this.faField != null)
				{
					if (!this.faField.Equals(value))
					{
						this.faField = value;
						this.OnPropertyChanged("FA");
						return;
					}
				}
				else
				{
					this.faField = value;
					this.OnPropertyChanged("FA");
				}
			}
		}

		public string ZCS
		{
			get
			{
				return this.zCSField;
			}
			set
			{
				if (this.zCSField != null)
				{
					if (!this.zCSField.Equals(value))
					{
						this.zCSField = value;
						this.OnPropertyChanged("ZCS");
						return;
					}
				}
				else
				{
					this.zCSField = value;
					this.OnPropertyChanged("ZCS");
				}
			}
		}
#if false
		public ObservableCollection<InfoObject> HistoryInfoObjects
		{
			get
			{
				return this.historyInfoObjectsField;
			}
			set
			{
				if (this.historyInfoObjectsField != null)
				{
					if (!this.historyInfoObjectsField.Equals(value))
					{
						this.historyInfoObjectsField = value;
						this.OnPropertyChanged("HistoryInfoObjects");
						return;
					}
				}
				else
				{
					this.historyInfoObjectsField = value;
					this.OnPropertyChanged("HistoryInfoObjects");
				}
			}
		}

		public TestPlanType Testplan
		{
			get
			{
				return this.testplanField;
			}
			set
			{
				if (this.testplanField != null)
				{
					if (!this.testplanField.Equals(value))
					{
						this.testplanField = value;
						this.OnPropertyChanged("Testplan");
						return;
					}
				}
				else
				{
					this.testplanField = value;
					this.OnPropertyChanged("Testplan");
				}
			}
		}

		public bool SimulatedParts
		{
			get
			{
				return this.simulatedPartsField;
			}
			set
			{
				if (!this.simulatedPartsField.Equals(value))
				{
					this.simulatedPartsField = value;
					this.OnPropertyChanged("SimulatedParts");
				}
			}
		}
#endif
		public VCIDevice VCI
		{
			get
			{
				return this.vCIField;
			}
			set
			{
				if (this.vCIField != null)
				{
					if (!this.vCIField.Equals(value))
					{
						this.vCIField = value;
						this.OnPropertyChanged("VCI");
						return;
					}
				}
				else
				{
					this.vCIField = value;
					this.OnPropertyChanged("VCI");
				}
			}
		}
#if false
		public VCIDevice MIB
		{
			get
			{
				return this.mIBField;
			}
			set
			{
				if (this.mIBField != null)
				{
					if (!this.mIBField.Equals(value))
					{
						this.mIBField = value;
						this.OnPropertyChanged("MIB");
						return;
					}
				}
				else
				{
					this.mIBField = value;
					this.OnPropertyChanged("MIB");
				}
			}
		}

		public ObservableCollection<technicalCampaignType> TechnicalCampaigns
		{
			get
			{
				return this.technicalCampaignsField;
			}
			set
			{
				if (this.technicalCampaignsField != null)
				{
					if (!this.technicalCampaignsField.Equals(value))
					{
						this.technicalCampaignsField = value;
						this.OnPropertyChanged("TechnicalCampaigns");
						return;
					}
				}
				else
				{
					this.technicalCampaignsField = value;
					this.OnPropertyChanged("TechnicalCampaigns");
				}
			}
		}
#endif
		public string Leistung
		{
			get
			{
				return this.leistungField;
			}
			set
			{
				if (this.leistungField != null)
				{
					if (!this.leistungField.Equals(value))
					{
						this.leistungField = value;
						this.OnPropertyChanged("Leistung");
						return;
					}
				}
				else
				{
					this.leistungField = value;
					this.OnPropertyChanged("Leistung");
				}
			}
		}

		public string Leistungsklasse
		{
			get
			{
				return this.leistungsklasseField;
			}
			set
			{
				if (this.leistungsklasseField != null)
				{
					if (!this.leistungsklasseField.Equals(value))
					{
						this.leistungsklasseField = value;
						this.OnPropertyChanged("Leistungsklasse");
						return;
					}
				}
				else
				{
					this.leistungsklasseField = value;
					this.OnPropertyChanged("Leistungsklasse");
				}
			}
		}

		public string Kraftstoffart
		{
			get
			{
				return this.kraftstoffartField;
			}
			set
			{
				if (this.kraftstoffartField != null)
				{
					if (!this.kraftstoffartField.Equals(value))
					{
						this.kraftstoffartField = value;
						this.OnPropertyChanged("Kraftstoffart");
						return;
					}
				}
				else
				{
					this.kraftstoffartField = value;
					this.OnPropertyChanged("Kraftstoffart");
				}
			}
		}

		public string ECTypeApproval
		{
			get
			{
				return this.eCTypeApprovalField;
			}
			set
			{
				if (this.eCTypeApprovalField != null)
				{
					if (!this.eCTypeApprovalField.Equals(value))
					{
						this.eCTypeApprovalField = value;
						this.OnPropertyChanged("ECTypeApproval");
						return;
					}
				}
				else
				{
					this.eCTypeApprovalField = value;
					this.OnPropertyChanged("ECTypeApproval");
				}
			}
		}

		public DateTime LastSaveDate
		{
			get
			{
				return this.lastSaveDateField;
			}
			set
			{
				if (!this.lastSaveDateField.Equals(value))
				{
					this.lastSaveDateField = value;
					this.OnPropertyChanged("LastSaveDate");
				}
			}
		}

		public DateTime LastChangeDate
		{
			get
			{
				return this.lastChangeDateField;
			}
			set
			{
				if (!this.lastChangeDateField.Equals(value))
				{
					this.lastChangeDateField = value;
					this.OnPropertyChanged("LastChangeDate");
				}
			}
		}
#if false
		public ObservableCollection<typeServiceHistoryEntry> ServiceHistory
		{
			get
			{
				return this.serviceHistoryField;
			}
			set
			{
				if (this.serviceHistoryField != null)
				{
					if (!this.serviceHistoryField.Equals(value))
					{
						this.serviceHistoryField = value;
						this.OnPropertyChanged("ServiceHistory");
						return;
					}
				}
				else
				{
					this.serviceHistoryField = value;
					this.OnPropertyChanged("ServiceHistory");
				}
			}
		}

		public ObservableCollection<typeDiagCode> DiagCodes
		{
			get
			{
				return this.diagCodesField;
			}
			set
			{
				if (this.diagCodesField != null)
				{
					if (!this.diagCodesField.Equals(value))
					{
						this.diagCodesField = value;
						this.OnPropertyChanged("DiagCodes");
						return;
					}
				}
				else
				{
					this.diagCodesField = value;
					this.OnPropertyChanged("DiagCodes");
				}
			}
		}
#endif
		public string Motorarbeitsverfahren
		{
			get
			{
				return this.motorarbeitsverfahrenField;
			}
			set
			{
				if (this.motorarbeitsverfahrenField != null)
				{
					if (!this.motorarbeitsverfahrenField.Equals(value))
					{
						this.motorarbeitsverfahrenField = value;
						this.OnPropertyChanged("Motorarbeitsverfahren");
						return;
					}
				}
				else
				{
					this.motorarbeitsverfahrenField = value;
					this.OnPropertyChanged("Motorarbeitsverfahren");
				}
			}
		}

		public string Drehmoment
		{
			get
			{
				return this.drehmomentField;
			}
			set
			{
				if (this.drehmomentField != null)
				{
					if (!this.drehmomentField.Equals(value))
					{
						this.drehmomentField = value;
						this.OnPropertyChanged("Drehmoment");
						return;
					}
				}
				else
				{
					this.drehmomentField = value;
					this.OnPropertyChanged("Drehmoment");
				}
			}
		}

		public string Hybridkennzeichen
		{
			get
			{
				return this.hybridkennzeichenField ?? string.Empty;
			}
			set
			{
				if (this.hybridkennzeichenField != null)
				{
					if (!this.hybridkennzeichenField.Equals(value))
					{
						this.hybridkennzeichenField = value;
						this.OnPropertyChanged("Hybridkennzeichen");
						return;
					}
				}
				else
				{
					this.hybridkennzeichenField = value;
					this.OnPropertyChanged("Hybridkennzeichen");
				}
			}
		}
#if false
		public ObservableCollection<DTC> CombinedFaults
		{
			get
			{
				return this.combinedFaultsField;
			}
			set
			{
				if (this.combinedFaultsField != null)
				{
					if (!this.combinedFaultsField.Equals(value))
					{
						this.combinedFaultsField = value;
						this.OnPropertyChanged("CombinedFaults");
						return;
					}
				}
				else
				{
					this.combinedFaultsField = value;
					this.OnPropertyChanged("CombinedFaults");
				}
			}
		}
#endif
		public ObservableCollection<decimal> InstalledAdapters
		{
			get
			{
				return this.installedAdaptersField;
			}
			set
			{
				if (this.installedAdaptersField != null)
				{
					if (!this.installedAdaptersField.Equals(value))
					{
						this.installedAdaptersField = value;
						this.OnPropertyChanged("InstalledAdapters");
						return;
					}
				}
				else
				{
					this.installedAdaptersField = value;
					this.OnPropertyChanged("InstalledAdapters");
				}
			}
		}

		public string VIN17_OEM
		{
			get
			{
				return this.vIN17_OEMField;
			}
			set
			{
				if (this.vIN17_OEMField != null)
				{
					if (!this.vIN17_OEMField.Equals(value))
					{
						this.vIN17_OEMField = value;
						this.OnPropertyChanged("VIN17_OEM");
						return;
					}
				}
				else
				{
					this.vIN17_OEMField = value;
					this.OnPropertyChanged("VIN17_OEM");
				}
			}
		}

		public string MOTKraftstoffart
		{
			get
			{
				return this.mOTKraftstoffartField;
			}
			set
			{
				if (this.mOTKraftstoffartField != null)
				{
					if (!this.mOTKraftstoffartField.Equals(value))
					{
						this.mOTKraftstoffartField = value;
						this.OnPropertyChanged("MOTKraftstoffart");
						return;
					}
				}
				else
				{
					this.mOTKraftstoffartField = value;
					this.OnPropertyChanged("MOTKraftstoffart");
				}
			}
		}

		public string MOTEinbaulage
		{
			get
			{
				return this.mOTEinbaulageField;
			}
			set
			{
				if (this.mOTEinbaulageField != null)
				{
					if (!this.mOTEinbaulageField.Equals(value))
					{
						this.mOTEinbaulageField = value;
						this.OnPropertyChanged("MOTEinbaulage");
						return;
					}
				}
				else
				{
					this.mOTEinbaulageField = value;
					this.OnPropertyChanged("MOTEinbaulage");
				}
			}
		}

		public string MOTBezeichnung
		{
			get
			{
				return this.mOTBezeichnungField;
			}
			set
			{
				if (this.mOTBezeichnungField != null)
				{
					if (!this.mOTBezeichnungField.Equals(value))
					{
						this.mOTBezeichnungField = value;
						//this.GenericMotor.EngineLabel1 = value;
						this.OnPropertyChanged("MOTBezeichnung");
						return;
					}
				}
				else
				{
					this.mOTBezeichnungField = value;
					//this.GenericMotor.EngineLabel1 = value;
					this.OnPropertyChanged("MOTBezeichnung");
				}
			}
		}

		public string Baureihenverbund
		{
			get
			{
				return this.baureihenverbundField;
			}
			set
			{
				if (this.baureihenverbundField != null)
				{
					if (!this.baureihenverbundField.Equals(value))
					{
						this.baureihenverbundField = value;
						this.OnPropertyChanged("Baureihenverbund");
						return;
					}
				}
				else
				{
					this.baureihenverbundField = value;
					this.OnPropertyChanged("Baureihenverbund");
				}
			}
		}

		public string AEKurzbezeichnung
		{
			get
			{
				return this.aEKurzbezeichnungField;
			}
			set
			{
				if (this.aEKurzbezeichnungField != null)
				{
					if (!this.aEKurzbezeichnungField.Equals(value))
					{
						this.aEKurzbezeichnungField = value;
						this.OnPropertyChanged("AEKurzbezeichnung");
						return;
					}
				}
				else
				{
					this.aEKurzbezeichnungField = value;
					this.OnPropertyChanged("AEKurzbezeichnung");
				}
			}
		}

		public string AELeistungsklasse
		{
			get
			{
				return this.aELeistungsklasseField;
			}
			set
			{
				if (this.aELeistungsklasseField != null)
				{
					if (!this.aELeistungsklasseField.Equals(value))
					{
						this.aELeistungsklasseField = value;
						this.OnPropertyChanged("AELeistungsklasse");
						return;
					}
				}
				else
				{
					this.aELeistungsklasseField = value;
					this.OnPropertyChanged("AELeistungsklasse");
				}
			}
		}

		public string AEUeberarbeitung
		{
			get
			{
				return this.aEUeberarbeitungField;
			}
			set
			{
				if (this.aEUeberarbeitungField != null)
				{
					if (!this.aEUeberarbeitungField.Equals(value))
					{
						this.aEUeberarbeitungField = value;
						this.OnPropertyChanged("AEUeberarbeitung");
						return;
					}
				}
				else
				{
					this.aEUeberarbeitungField = value;
					this.OnPropertyChanged("AEUeberarbeitung");
				}
			}
		}
#if false
		public ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX> PerceivedSymptoms
		{
			get
			{
				return this.perceivedSymptomsField;
			}
			set
			{
				if (this.perceivedSymptomsField != null)
				{
					if (!this.perceivedSymptomsField.Equals(value))
					{
						this.perceivedSymptomsField = value;
						this.OnPropertyChanged("PerceivedSymptoms");
						return;
					}
				}
				else
				{
					this.perceivedSymptomsField = value;
					this.OnPropertyChanged("PerceivedSymptoms");
				}
			}
		}
#endif
		public string ProgmanVersion
		{
			get
			{
				return this.progmanVersionField;
			}
			set
			{
				if (this.progmanVersionField != null)
				{
					if (!this.progmanVersionField.Equals(value))
					{
						this.progmanVersionField = value;
						this.OnPropertyChanged("ProgmanVersion");
						return;
					}
				}
				else
				{
					this.progmanVersionField = value;
					this.OnPropertyChanged("ProgmanVersion");
				}
			}
		}

		[DefaultValue("grafik/gif/icon_offl_ACTIV.gif")]
		public string ConnectImage
		{
			get
			{
				return this.connectImageField;
			}
			set
			{
				if (this.connectImageField != null)
				{
					if (!this.connectImageField.Equals(value))
					{
						this.connectImageField = value;
						this.OnPropertyChanged("ConnectImage");
						return;
					}
				}
				else
				{
					this.connectImageField = value;
					this.OnPropertyChanged("ConnectImage");
				}
			}
		}

		[DefaultValue("grafik/gif/icon_imib_INACTIV.gif")]
		public string ConnectIMIBImage
		{
			get
			{
				return this.connectIMIBImageField;
			}
			set
			{
				if (this.connectIMIBImageField != null)
				{
					if (!this.connectIMIBImageField.Equals(value))
					{
						this.connectIMIBImageField = value;
						this.OnPropertyChanged("ConnectIMIBImage");
						return;
					}
				}
				else
				{
					this.connectIMIBImageField = value;
					this.OnPropertyChanged("ConnectIMIBImage");
				}
			}
		}

		[DefaultValue(VisibilityType.Visible)]
		public VisibilityType ConnectIPState
		{
			get
			{
				return this.connectIPStateField;
			}
			set
			{
				if (!this.connectIPStateField.Equals(value))
				{
					this.connectIPStateField = value;
					this.OnPropertyChanged("ConnectIPState");
				}
			}
		}

		[DefaultValue(VisibilityType.Visible)]
		public VisibilityType ConnectIMIBIPState
		{
			get
			{
				return this.connectIMIBIPStateField;
			}
			set
			{
				if (!this.connectIMIBIPStateField.Equals(value))
				{
					this.connectIMIBIPStateField = value;
					this.OnPropertyChanged("ConnectIMIBIPState");
				}
			}
		}

		[DefaultValue(VisibilityType.Visible)]
		public VisibilityType ConnectState
		{
			get
			{
				return this.connectStateField;
			}
			set
			{
				if (!this.connectStateField.Equals(value))
				{
					this.connectStateField = value;
					this.OnPropertyChanged("ConnectState");
				}
			}
		}

		[DefaultValue(VisibilityType.Visible)]
		public VisibilityType ConnectIMIBState
		{
			get
			{
				return this.connectIMIBStateField;
			}
			set
			{
				if (!this.connectIMIBStateField.Equals(value))
				{
					this.connectIMIBStateField = value;
					this.OnPropertyChanged("ConnectIMIBState");
				}
			}
		}

		public string Status_FunctionName
		{
			get
			{
				return this.status_FunctionNameField;
			}
			set
			{
				if (this.status_FunctionNameField != null)
				{
					if (!this.status_FunctionNameField.Equals(value))
					{
						this.status_FunctionNameField = value;
						this.OnPropertyChanged("Status_FunctionName");
						return;
					}
				}
				else
				{
					this.status_FunctionNameField = value;
					this.OnPropertyChanged("Status_FunctionName");
				}
			}
		}

		[DefaultValue(StateType.idle)]
		public StateType Status_FunctionState
		{
			get
			{
				return this.status_FunctionStateField;
			}
			set
			{
				if (!this.status_FunctionStateField.Equals(value))
				{
					this.status_FunctionStateField = value;
					this.OnPropertyChanged("Status_FunctionState");
				}
			}
		}

		public DateTime Status_FunctionStateLastChangeTime
		{
			get
			{
				return this.status_FunctionStateLastChangeTimeField;
			}
			set
			{
				if (!this.status_FunctionStateLastChangeTimeField.Equals(value))
				{
					this.status_FunctionStateLastChangeTimeField = value;
					this.OnPropertyChanged("Status_FunctionStateLastChangeTime");
				}
			}
		}

		[XmlIgnore]
		public bool Status_FunctionStateLastChangeTimeSpecified
		{
			get
			{
				return this.status_FunctionStateLastChangeTimeFieldSpecified;
			}
			set
			{
				if (!this.status_FunctionStateLastChangeTimeFieldSpecified.Equals(value))
				{
					this.status_FunctionStateLastChangeTimeFieldSpecified = value;
					this.OnPropertyChanged("Status_FunctionStateLastChangeTimeSpecified");
				}
			}
		}

		public double Status_FunctionProgress
		{
			get
			{
				return this.status_FunctionProgressField;
			}
			set
			{
				if (!this.status_FunctionProgressField.Equals(value))
				{
					this.status_FunctionProgressField = value;
					this.OnPropertyChanged("Status_FunctionProgress");
				}
			}
		}

		public string Kl15Voltage
		{
			get
			{
				return this.kl15VoltageField;
			}
			set
			{
				if (this.kl15VoltageField != null)
				{
					if (!this.kl15VoltageField.Equals(value))
					{
						this.kl15VoltageField = value;
						this.OnPropertyChanged("Kl15Voltage");
						return;
					}
				}
				else
				{
					this.kl15VoltageField = value;
					this.OnPropertyChanged("Kl15Voltage");
				}
			}
		}

		public string Kl30Voltage
		{
			get
			{
				return this.kl30VoltageField;
			}
			set
			{
				if (this.kl30VoltageField != null)
				{
					if (!this.kl30VoltageField.Equals(value))
					{
						this.kl30VoltageField = value;
						this.OnPropertyChanged("Kl30Voltage");
						return;
					}
				}
				else
				{
					this.kl30VoltageField = value;
					this.OnPropertyChanged("Kl30Voltage");
				}
			}
		}

		[DefaultValue(false)]
		public bool PADVehicle
		{
			get
			{
				return this.pADVehicleField;
			}
			set
			{
				if (!this.pADVehicleField.Equals(value))
				{
					this.pADVehicleField = value;
					this.OnPropertyChanged("PADVehicle");
				}
			}
		}

		[DefaultValue(-1)]
		public int PwfState
		{
			get
			{
				return this.pwfStateField;
			}
			set
			{
				if (!this.pwfStateField.Equals(value))
				{
					this.pwfStateField = value;
					this.OnPropertyChanged("PwfState");
				}
			}
		}

		public DateTime KlVoltageLastMessageTime
		{
			get
			{
				return this.klVoltageLastMessageTimeField;
			}
			set
			{
				if (!this.klVoltageLastMessageTimeField.Equals(value))
				{
					this.klVoltageLastMessageTimeField = value;
					this.OnPropertyChanged("KlVoltageLastMessageTime");
				}
			}
		}

		[XmlIgnore]
		public bool KlVoltageLastMessageTimeSpecified
		{
			get
			{
				return this.klVoltageLastMessageTimeFieldSpecified;
			}
			set
			{
				if (!this.klVoltageLastMessageTimeFieldSpecified.Equals(value))
				{
					this.klVoltageLastMessageTimeFieldSpecified = value;
					this.OnPropertyChanged("KlVoltageLastMessageTimeSpecified");
				}
			}
		}

		[DefaultValue("0.0.1")]
		public string ApplicationVersion
		{
			get
			{
				return this.applicationVersionField;
			}
			set
			{
				if (this.applicationVersionField != null)
				{
					if (!this.applicationVersionField.Equals(value))
					{
						this.applicationVersionField = value;
						this.OnPropertyChanged("ApplicationVersion");
						return;
					}
				}
				else
				{
					this.applicationVersionField = value;
					this.OnPropertyChanged("ApplicationVersion");
				}
			}
		}

		[DefaultValue(false)]
		public bool FASTAAlreadyDone
		{
			get
			{
				return this.fASTAAlreadyDoneField;
			}
			set
			{
				if (!this.fASTAAlreadyDoneField.Equals(value))
				{
					this.fASTAAlreadyDoneField = value;
					this.OnPropertyChanged("FASTAAlreadyDone");
				}
			}
		}

		[DefaultValue(IdentificationLevel.None)]
		public IdentificationLevel VehicleIdentLevel
		{
			get
			{
				return this.vehicleIdentLevelField;
			}
			set
			{
				if (!this.vehicleIdentLevelField.Equals(value))
				{
					this.vehicleIdentLevelField = value;
					this.OnPropertyChanged("VehicleIdentLevel");
				}
			}
		}

		[DefaultValue(false)]
		public bool VehicleIdentAlreadyDone
		{
			get
			{
				return this.vehicleIdentAlreadyDoneField;
			}
			set
			{
				if (this.vehicleIdentAlreadyDoneField != value)
				{
					this.vehicleIdentAlreadyDoneField = value;
					this.OnPropertyChanged("VehicleIdentAlreadyDone");
				}
			}
		}

		[DefaultValue(false)]
		public bool VehicleShortTestAsSessionEntry
		{
			get
			{
				return this.vehicleShortTestAsSessionEntryField;
			}
			set
			{
				if (!this.vehicleShortTestAsSessionEntryField.Equals(value))
				{
					this.vehicleShortTestAsSessionEntryField = value;
					this.OnPropertyChanged("VehicleShortTestAsSessionEntry");
				}
			}
		}

		[DefaultValue(false)]
		public bool Pannenfall
		{
			get
			{
				return this.pannenfallField;
			}
			set
			{
				if (!this.pannenfallField.Equals(value))
				{
					this.pannenfallField = value;
					this.OnPropertyChanged("Pannenfall");
				}
			}
		}

		[DefaultValue(0)]
		public int SelectedDiagBUS
		{
			get
			{
				return this.selectedDiagBUSField;
			}
			set
			{
				if (!this.selectedDiagBUSField.Equals(value))
				{
					this.selectedDiagBUSField = value;
					this.OnPropertyChanged("SelectedDiagBUS");
				}
			}
		}

		[DefaultValue(false)]
		public bool DOMRequestFailed
		{
			get
			{
				return this.dOMRequestFailedField;
			}
			set
			{
				if (!this.dOMRequestFailedField.Equals(value))
				{
					this.dOMRequestFailedField = value;
					this.OnPropertyChanged("DOMRequestFailed");
				}
			}
		}

		[DefaultValue(false)]
		public bool CVDRequestFailed
		{
			get
			{
				return this.cVDRequestFailedField;
			}
			set
			{
				if (!this.cVDRequestFailedField.Equals(value))
				{
					this.cVDRequestFailedField = value;
					this.OnPropertyChanged("CVDRequestFailed");
				}
			}
		}

		[DefaultValue(false)]
		public bool CvsRequestFailed
		{
			get
			{
				return this.cvsRequestFailedField;
			}
			set
			{
				if (!this.cvsRequestFailedField.Equals(value))
				{
					this.cvsRequestFailedField = value;
					this.OnPropertyChanged("CvsRequestFailed");
				}
			}
		}

		[DefaultValue(false)]
		public bool TecCampaignsRequestFailed
		{
			get
			{
				return this.tecCampaignsRequestFailedField;
			}
			set
			{
				if (!this.tecCampaignsRequestFailedField.Equals(value))
				{
					this.tecCampaignsRequestFailedField = value;
					this.OnPropertyChanged("TecCampaignsRequestFailed");
				}
			}
		}

		[DefaultValue(false)]
		public bool RepHistoryRequestFailed
		{
			get
			{
				return this.repHistoryRequestFailedField;
			}
			set
			{
				if (!this.repHistoryRequestFailedField.Equals(value))
				{
					this.repHistoryRequestFailedField = value;
					this.OnPropertyChanged("RepHistoryRequestFailed");
				}
			}
		}

		[DefaultValue(false)]
		public bool KL15OverrideVoltageCheck
		{
			get
			{
				return this.kL15OverrideVoltageCheckField;
			}
			set
			{
				if (!this.kL15OverrideVoltageCheckField.Equals(value))
				{
					this.kL15OverrideVoltageCheckField = value;
					this.OnPropertyChanged("KL15OverrideVoltageCheck");
				}
			}
		}

		[DefaultValue(false)]
		public bool KL15FaultILevelAlreadyAlerted
		{
			get
			{
				return this.kL15FaultILevelAlreadyAlertedField;
			}
			set
			{
				if (!this.kL15FaultILevelAlreadyAlertedField.Equals(value))
				{
					this.kL15FaultILevelAlreadyAlertedField = value;
					this.OnPropertyChanged("KL15FaultILevelAlreadyAlerted");
				}
			}
		}

		[DefaultValue(false)]
		public bool GWSZReadoutSuccess
		{
			get
			{
				return this.gWSZReadoutSuccessField;
			}
			set
			{
				if (!this.gWSZReadoutSuccessField.Equals(value))
				{
					this.gWSZReadoutSuccessField = value;
					this.OnPropertyChanged("GWSZReadoutSuccess");
				}
			}
		}

		public string refSchema
		{
			get
			{
				return this.refSchemaField;
			}
			set
			{
				if (this.refSchemaField != null)
				{
					if (!this.refSchemaField.Equals(value))
					{
						this.refSchemaField = value;
						this.OnPropertyChanged("refSchema");
						return;
					}
				}
				else
				{
					this.refSchemaField = value;
					this.OnPropertyChanged("refSchema");
				}
			}
		}

		public string version
		{
			get
			{
				return this.versionField;
			}
			set
			{
				if (this.versionField != null)
				{
					if (!this.versionField.Equals(value))
					{
						this.versionField = value;
						this.OnPropertyChanged("version");
						return;
					}
				}
				else
				{
					this.versionField = value;
					this.OnPropertyChanged("version");
				}
			}
		}

		public DateTime VehicleLifeStartDate
		{
			get
			{
				return this.vehicleLifeStartDate;
			}
			set
			{
				if (!this.vehicleLifeStartDate.Equals(value))
				{
					this.vehicleLifeStartDate = value;
					this.OnPropertyChanged("VehicleLifeStartDate");
				}
			}
		}

		public string ElektrischeReichweite
		{
			get
			{
				return this.elektrischeReichweiteField;
			}
			set
			{
				if (this.elektrischeReichweiteField != null)
				{
					if (!this.elektrischeReichweiteField.Equals(value))
					{
						this.elektrischeReichweiteField = value;
						this.OnPropertyChanged("ElektrischeReichweite");
						return;
					}
				}
				else
				{
					this.elektrischeReichweiteField = value;
					this.OnPropertyChanged("ElektrischeReichweite");
				}
			}
		}

		public string AEBezeichnung
		{
			get
			{
				return this.aeBezeichnungField;
			}
			set
			{
				if (this.aeBezeichnungField != null)
				{
					if (!this.aeBezeichnungField.Equals(value))
					{
						this.aeBezeichnungField = value;
						this.OnPropertyChanged("AEBezeichnung");
						return;
					}
				}
				else
				{
					this.aeBezeichnungField = value;
					this.OnPropertyChanged("AEBezeichnung");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

        public ClientContext ClientContext
        {
            get { return _clientContext; }
        }

        private string vIN17Field;

		private bool isSendFastaDataForbiddenField;

		private bool isSendFastaDataForbiddenBitsQueueFullField;

		private string serialBodyShellField;

		private string serialGearBoxField;

		private string serialEngineField;

		//private string batteryInfo;

		private BrandName? brandNameField;

		private ObservableCollection<ECU> eCUField;

		//private ObservableCollection<ZFSResult> zFSField;

		private bool zFS_SUCCESSFULLYField;

		private ECU selectedECUField;

		//private ObservableCollection<typeCBSInfo> cBSField;

		private string typField;

		private string basicTypeField;

		private string driveTypeField;

		private string warrentyTypeField;

		private string markeField;

		private string ueberarbeitungField;

		private string prodartField;

		private string ereiheField;

		private string gsgbdField;

		private BNType bNTypeField;

		private BNMixed bNMixedField;

		private string baureiheField;

		private string verkaufsBezeichnungField;

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

		private string abgasField;

		private DateTime productionDateField;

		private bool productionDateFieldSpecified;

		private DateTime? firstRegistrationField;

		private string modelljahrField;

		private string modellmonatField;

		private string modelltagField;

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

		//private ObservableCollection<InfoObject> historyInfoObjectsField;

		//private TestPlanType testplanField;

		//private bool simulatedPartsField;

		private VCIDevice vCIField;

		//private VCIDevice mIBField;

		//private ObservableCollection<technicalCampaignType> technicalCampaignsField;

		private string leistungField;

		private string leistungsklasseField;

		private string kraftstoffartField;

		private string eCTypeApprovalField;

		private DateTime lastSaveDateField;

		private DateTime lastChangeDateField;

		//private ObservableCollection<typeServiceHistoryEntry> serviceHistoryField;

		//private ObservableCollection<typeDiagCode> diagCodesField;

		private string motorarbeitsverfahrenField;

		private string drehmomentField;

		private string hybridkennzeichenField;

		//private ObservableCollection<DTC> combinedFaultsField;

		private ObservableCollection<decimal> installedAdaptersField;

		private string vIN17_OEMField;

		private string mOTKraftstoffartField;

		private string mOTEinbaulageField;

		private string mOTBezeichnungField;

		private string baureihenverbundField;

		private string aEKurzbezeichnungField;

		private string aELeistungsklasseField;

		private string aEUeberarbeitungField;

		//private ObservableCollection<XEP_PERCEIVEDSYMPTOMSEX> perceivedSymptomsField;

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

		private bool cVDRequestFailedField;

		private bool cvsRequestFailedField;

		private bool tecCampaignsRequestFailedField;

		private bool repHistoryRequestFailedField;

		private bool kL15OverrideVoltageCheckField;

		private bool kL15FaultILevelAlreadyAlertedField;

		private bool gWSZReadoutSuccessField;

		private string refSchemaField;

		private string versionField;

		private DateTime vehicleLifeStartDate;

		private List<DealerSessionProperty> dealerSessionProperties;

        private ClientContext _clientContext;
	}
}
