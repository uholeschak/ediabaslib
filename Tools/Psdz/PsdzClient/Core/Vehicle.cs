using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Utility;

namespace PsdzClient.Core
{
	public class Vehicle : typeVehicle, INotifyPropertyChanged, IVehicle
	{
		public Vehicle()
		{
			base.ConnectState = VisibilityType.Collapsed;
			//this.pKodeList = new ObservableCollectionEx<Fault>();
			//this.FaultList = new List<Fault>();
			//this.VirtualFaultInfoList = new BlockingCollection<VirtualFaultInfo>();
			//this.sessionDataStore = new ParameterContainer();
			//base.Testplan = new TestPlanType();
			this.diagCodesProgramming = new ObservableCollection<string>();
			this.IsClosingOperationActive = false;
			this.validPWFStates = new HashSet<int>(new int[]
			{
				0,
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9,
				10,
				11,
				12,
				13,
				14,
				15,
				16
			});
			this.clamp15MinValue = 0.0;
			this.clamp30MinValue = 9.95;
			//this.RxSwin = new RxSwinData();
		}
#if false
		public List<string> PermanentSAEFehlercodesInFaultList()
		{
			List<string> list = new List<string>();
			if (this.FaultList != null && this.FaultList.Count != 0)
			{
				foreach (Fault fault in this.FaultList)
				{
					if (fault.DTC.FortAsHexString == "S 0751")
					{
						list.Add("S 0751");
					}
					if (fault.DTC.FortAsHexString == "S 0756")
					{
						list.Add("S 0756");
					}
				}
				return list;
			}
			return new List<string>();
		}
#endif
		[XmlIgnore]
		public bool Sp2021Enabled
		{
			get
			{
				return this.sp2021Enabled;
			}
			set
			{
				this.sp2021Enabled = value;
			}
		}

		public string HmiVersion
		{
			get
			{
				return this.hmiVersion;
			}
			set
			{
				this.hmiVersion = value;
				this.OnPropertyChanged("HmiVersion");
			}
		}

		public string EBezeichnungUIText
		{
			get
			{
				return this.eBezeichnungUIText;
			}
			set
			{
				this.eBezeichnungUIText = value;
				this.OnPropertyChanged("EBezeichnungUIText");
			}
		}

		[XmlIgnore]
		public string SalesDesignationBadgeUIText
		{
			get
			{
				return this.salesDesignationBadgeUIText;
			}
			set
			{
				this.salesDesignationBadgeUIText = value;
				this.OnPropertyChanged("SalesDesignationBadgeUIText");
			}
		}

		public ObservableCollection<string> DiagCodesProgramming
		{
			get
			{
				return this.diagCodesProgramming;
			}
		}
#if false
		[XmlIgnore]
		public RxSwinData RxSwin { get; set; }

		[XmlIgnore]
		public List<IRxSwinObject> RxSwinObjectList { get; set; }
#endif
		[XmlIgnore]
		public FA TargetFA
		{
			get
			{
				return this.targetFA;
			}
			set
			{
				this.targetFA = value;
			}
		}

		[XmlIgnore]
		public string TargetILevel
		{
			get
			{
				return this.targetILevel;
			}
			set
			{
				this.targetILevel = value;
			}
		}

		[XmlIgnore]
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
#if false
		public bool SetVINRangeTypeFromVINRanges()
		{
			IDatabaseProvider instance = DatabaseProviderFactory.Instance;
			if (instance != null && instance.DatabaseAccessType != DatabaseType.None && !"XXXXXXX".Equals(this.VIN7) && !string.IsNullOrEmpty(this.VIN7) && !this.VIN7.Equals(this.vinRangeTypeLastResolvedType, StringComparison.OrdinalIgnoreCase))
			{
				VINRANGES vinRangesByVin = instance.GetVinRangesByVin17(this.VINType, this.VIN7, false);
				if (vinRangesByVin != null)
				{
					this.vinRangeType = vinRangesByVin.TYPSCHLUESSEL;
					this.vinRangeTypeLastResolvedType = this.VIN7;
					return true;
				}
			}
			return false;
		}
#endif
		[XmlIgnore]
		public string VINRangeType
		{
			get
			{
				return this.vinRangeType;
			}
			set
			{
				this.vinRangeType = value;
			}
		}

		[XmlIgnore]
		public bool IsClosingOperationActive
		{
			get
			{
				return this.isClosingOperationActive;
			}
			set
			{
				this.isClosingOperationActive = value;
			}
		}
#if false
		[XmlIgnore]
		public ParameterContainer SessionDataStore
		{
			get
			{
				return this.sessionDataStore;
			}
		}
#endif
		public string VIN10Prefix
		{
			get
			{
				string result;
				try
				{
					if (string.IsNullOrEmpty(base.VIN17))
					{
						result = null;
					}
					else
					{
						result = base.VIN17.Substring(0, 10);
					}
				}
				catch (Exception)
				{
					//Log.WarningException("Vehicle.VIN10Prefix", exception);
					result = null;
				}
				return result;
			}
		}

		public string BasisEReihe
		{
			get
			{
				if (!string.IsNullOrEmpty(base.Gsgbd) && base.Gsgbd.Length >= 3 && !base.Gsgbd.Equals("zcs_all"))
				{
					return base.Gsgbd.Substring(0, 3);
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
				catch (Exception)
				{
					//Log.WarningException("Vehicle.get_VIN7", exception);
				}
				return null;
			}
		}
#if false
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
					if (!string.IsNullOrEmpty(this.VINRangeType))
					{
						return this.VINRangeType;
					}
					string text = base.VIN17.Substring(3, 4);
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = text.Substring(0, 3);
						switch (text[3])
						{
							case 'A':
								text2 += "1";
								goto IL_150;
							case 'B':
								text2 += "2";
								goto IL_150;
							case 'C':
								text2 += "3";
								goto IL_150;
							case 'D':
								text2 += "4";
								goto IL_150;
							case 'E':
								text2 += "5";
								goto IL_150;
							case 'F':
								text2 += "6";
								goto IL_150;
							case 'G':
								text2 += "7";
								goto IL_150;
							case 'H':
								text2 += "8";
								goto IL_150;
							case 'J':
								text2 += "9";
								goto IL_150;
						}
						return text;
						IL_150:
						return text2;
					}
				}
				catch (Exception exception)
				{
					//Log.WarningException("Vehicle.get_VINType", exception);
				}
				return null;
			}
		}
#endif
		public string VINType
		{
			get
			{
				try
				{
					if (!string.IsNullOrEmpty(base.VIN17) && base.VIN17.Length >= 17)
					{
						return base.VIN17.Substring(3, 4);
					}
					return null;
				}
				catch (Exception)
				{
					//Log.WarningException("Vehicle.get_VINType", exception);
				}
				return null;
			}
		}

		public bool IsBusy
		{
			get
			{
				return this.isBusy;
			}
			set
			{
				this.isBusy = value;
				this.OnPropertyChanged("IsBusy");
			}
		}

		public string EMotBaureihe
		{
			get
			{
				return base.EMotor.EMOTBaureihe;
			}
		}
#if false
		public string Produktlinie
		{
			get
			{
				if (!string.IsNullOrEmpty(this.productLine))
				{
					return this.productLine;
				}
				return VehicleESeriesPropertiesGetter.GetProductLineBasedOnESeries(base.Ereihe);
			}
			set
			{
				if (this.productLine != value)
				{
					this.productLine = value;
					this.OnPropertyChanged("Produktlinie");
				}
			}
		}

		public string Sicherheitsrelevant
		{
			get
			{
				if (!string.IsNullOrEmpty(this.securityRelevant))
				{
					return this.securityRelevant;
				}
				return VehicleESeriesPropertiesGetter.GetSecurityRelevanceBasedOnESeries(base.Ereihe, this.hasSA("106"), this.hasSA("109"), this.hasSA("8SU"));
			}
			set
			{
				if (this.securityRelevant != value)
				{
					this.securityRelevant = value;
					this.OnPropertyChanged("Sicherheitsrelevant");
				}
			}
		}

		public string Tueren
		{
			get
			{
				if (!string.IsNullOrEmpty(this.doorNumber))
				{
					return this.doorNumber;
				}
				return VehicleESeriesPropertiesGetter.GetDoorNumberBasedOnESeries(base.Ereihe, base.Prodart);
			}
			set
			{
				if (value != this.doorNumber)
				{
					this.doorNumber = value;
					this.OnPropertyChanged("Tueren");
				}
			}
		}

		[XmlIgnore]
		public IList<Fault> FaultList
		{
			get
			{
				return this.faultList;
			}
			set
			{
				if (value != null)
				{
					this.faultList = value;
					this.OnPropertyChanged("FaultList");
				}
			}
		}

		[XmlIgnore]
		public BlockingCollection<VirtualFaultInfo> VirtualFaultInfoList
		{
			get
			{
				return this.virtualFaultInfoList;
			}
			set
			{
				this.virtualFaultInfoList = value;
			}
		}

		public IEnumerable<Fault> GetEnrichedFaultList(IFFMDynamicResolver ffmDynamicResolver)
		{
			List<Fault> list = new List<Fault>();
			foreach (Fault fault in this.FaultList)
			{
				fault.ResolveLabels(this, ffmDynamicResolver);
				list.Add(fault);
			}
			return list;
		}

		public ObservableCollectionEx<Fault> PKodeList
		{
			get
			{
				return this.pKodeList;
			}
		}
#endif
		[XmlIgnore]
		public bool IsFastaReadDone { get; set; }

		[XmlIgnore]
		public bool IsProgrammingSessionStartable { get; set; }

		[XmlIgnore]
		public bool IsVehicleTestDone
		{
			get
			{
				return this.vehicleTestDone;
			}
			set
			{
				if (this.vehicleTestDone != value)
				{
					this.vehicleTestDone = value;
					this.OnPropertyChanged("IsVehicleTestDone");
				}
			}
		}

		public bool IsReadingFastaDataFinished
		{
			get
			{
				return this.isReadingFastaDataFinished;
			}
			set
			{
				this.isReadingFastaDataFinished = value;
				this.OnPropertyChanged("IsReadingFastaDataFinished");
			}
		}

		public bool IsNewFaultMemoryActive
		{
			get
			{
				return this.isNewFaultMemoryActiveField;
			}
			set
			{
				this.isNewFaultMemoryActiveField = value;
				this.OnPropertyChanged("IsNewFaultMemoryActive");
			}
		}

		public bool IsNewFaultMemoryExpertModeActive
		{
			get
			{
				return this.isNewFaultMemoryExpertModeActiveField;
			}
			set
			{
				this.isNewFaultMemoryExpertModeActiveField = value;
				this.OnPropertyChanged("IsNewFaultMemoryExpertModeActive");
			}
		}

		[XmlIgnore]
		public bool IsVehicleBreakdownAlreadyShown { get; set; }

		[XmlIgnore]
		public bool IsPowerSafeModeActive
		{
			get
			{
				return this.powerSafeModeByOldEcus || this.powerSafeModeByNewEcus;
			}
		}

		[XmlIgnore]
		public bool IsPowerSafeModeActiveByOldEcus
		{
			get
			{
				return this.powerSafeModeByOldEcus;
			}
			set
			{
				this.powerSafeModeByOldEcus = value;
			}
		}

		[XmlIgnore]
		public bool VinNotReadbleFromCarAbort
		{
			get
			{
				return this.vinNotReadbleFromCarAbort;
			}
			set
			{
				this.vinNotReadbleFromCarAbort = value;
			}
		}

		[XmlIgnore]
		public bool IsPowerSafeModeActiveByNewEcus
		{
			get
			{
				return this.powerSafeModeByNewEcus;
			}
			set
			{
				this.powerSafeModeByNewEcus = value;
			}
		}

		[XmlIgnore]
		public int? FaultCodeSum
		{
			get
			{
				return this.faultCodeSum;
			}
			set
			{
				this.faultCodeSum = value;
				this.OnPropertyChanged("FaultCodeSum");
			}
		}

		[XmlIgnore]
		public string WarrantyType
		{
			get
			{
				if (!string.IsNullOrEmpty(base.Typ) && base.Typ.Length == 4)
				{
					string str = base.Typ.Substring(0, 2);
					switch (base.Typ[2])
					{
						case '0':
							str += "9";
							goto IL_D2;
						case '2':
							str += "1";
							goto IL_D2;
						case '4':
							str += "3";
							goto IL_D2;
						case '6':
							str += "5";
							goto IL_D2;
						case '8':
							str += "7";
							goto IL_D2;
					}
					str += base.Typ[2].ToString();
					IL_D2:
					return str + base.Typ[3].ToString();
				}
				return string.Empty;
			}
		}

		[XmlIgnore]
		public DateTime? C_DATETIME
		{
			get
			{
				try
				{
					if (base.FA != null)
					{
						DateTime? c_DATETIME = base.FA.C_DATETIME;
						if (c_DATETIME != null)
						{
							c_DATETIME = base.FA.C_DATETIME;
							DateTime minValue = DateTime.MinValue;
							if (c_DATETIME > minValue)
							{
								return base.FA.C_DATETIME;
							}
						}
					}
					if (!string.IsNullOrEmpty(base.Modelljahr) && !string.IsNullOrEmpty(base.Modellmonat))
					{
						if (this.cDatetimeByModelYearMonth == null)
						{
							this.cDatetimeByModelYearMonth = new DateTime?(DateTime.Parse(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-01", base.Modelljahr, base.Modellmonat), CultureInfo.InvariantCulture));
						}
						return this.cDatetimeByModelYearMonth;
					}
				}
				catch (Exception)
				{
					//Log.WarningException("Vehicle.get_C_DATETIME()", exception);
				}
				return null;
			}
		}
#if false
		[XmlIgnore]
		IEnumerable<ICbsInfo> IVehicle.CBS
		{
			get
			{
				return base.CBS;
			}
		}

		[XmlIgnore]
		IEnumerable<IDtc> IVehicle.CombinedFaults
		{
			get
			{
				return base.CombinedFaults;
			}
		}

		[XmlIgnore]
		IEnumerable<IDiagCode> IVehicle.DiagCodes
		{
			get
			{
				return base.DiagCodes;
			}
		}
#endif
		[XmlIgnore]
		IEnumerable<IEcu> IVehicle.ECU
		{
			get
			{
				return base.ECU;
			}
		}

		[XmlIgnore]
		IFa IVehicle.FA
		{
			get
			{
				return base.FA;
			}
		}

		[XmlIgnore]
		IEnumerable<IFfmResult> IVehicle.FFM
		{
			get
			{
				return base.FFM;
			}
		}

		[XmlIgnore]
		IEnumerable<decimal> IVehicle.InstalledAdapters
		{
			get
			{
				return base.InstalledAdapters;
			}
		}

		[XmlIgnore]
		IEcu IVehicle.SelectedECU
		{
			get
			{
				return base.SelectedECU;
			}
		}
#if false
		[XmlIgnore]
		IVciDevice IVehicle.MIB
		{
			get
			{
				return base.MIB;
			}
		}

		[XmlIgnore]
		IEnumerable<IServiceHistoryEntry> IVehicle.ServiceHistory
		{
			get
			{
				return base.ServiceHistory;
			}
		}

		[XmlIgnore]
		IEnumerable<ITechnicalCampaign> IVehicle.TechnicalCampaigns
		{
			get
			{
				return base.TechnicalCampaigns;
			}
		}
#endif
		[XmlIgnore]
		IVciDevice IVehicle.VCI
		{
			get
			{
				return base.VCI;
			}
		}
#if false
		[XmlIgnore]
		IEnumerable<IZfsResult> IVehicle.ZFS
		{
			get
			{
				return base.ZFS;
			}
		}
#endif
		[XmlIgnore]
		public double Clamp15MinValue
		{
			get
			{
				return this.clamp15MinValue;
			}
			set
			{
				if (this.clamp15MinValue != value)
				{
					this.clamp15MinValue = value;
					this.OnPropertyChanged("Clamp15MinValue");
				}
			}
		}

		public bool WithLfpBattery
		{
			get
			{
				return this.withLfpBattery;
			}
			set
			{
				if (this.withLfpBattery != value)
				{
					this.withLfpBattery = value;
					this.OnPropertyChanged("WithLfpBattery");
				}
			}
		}

		[XmlIgnore]
		public double Clamp30MinValue
		{
			get
			{
				return this.clamp30MinValue;
			}
			set
			{
				if (this.clamp30MinValue != value)
				{
					this.clamp30MinValue = value;
					this.OnPropertyChanged("Clamp30MinValue");
				}
			}
		}

		[XmlIgnore]
		public HashSet<int> ValidPWFStates
		{
			get
			{
				return this.validPWFStates;
			}
			set
			{
				if (this.validPWFStates != value)
				{
					this.validPWFStates = value;
					this.OnPropertyChanged("ValidPWFStates");
				}
			}
		}

		public override void OnPropertyChanged(string propertyName)
		{
			base.OnPropertyChanged(propertyName);
			if ("SerialGearBox".Equals(propertyName))
			{
				base.OnPropertyChanged("SerialGearBox7");
			}
		}

		public string GetFSCfromUpdateIndex(string updateIndex, string huVariante)
		{
			string[] source = new string[]
			{
				"HU_MGU",
				"ENAVEVO"
			};
			string result;
			try
			{
				int num = Convert.ToInt32(updateIndex, 16);
				if (source.Any((string x) => huVariante.Equals(x)))
				{
					string str = updateIndex.Substring(0, 2);
					result = updateIndex.Substring(2, 2) + "-" + str;
				}
				else if (num > 45)
				{
					int months = num - 54;
					DateTime dateTime = new DateTime(2018, 7, 1);
					DateTime dateTime2 = dateTime.AddMonths(months);
					new DateTime(2017, 10, 1);
					result = dateTime2.Month + "-" + dateTime2.Year;
				}
				else if (num > 33)
				{
					int num2 = 46 - num;
					int months2 = -1 * (num2 * 3 - 3);
					DateTime dateTime3 = new DateTime(2017, 10, 1);
					DateTime dateTime4 = dateTime3.AddMonths(months2);
					result = dateTime4.Month + "-" + dateTime4.Year;
				}
				else
				{
					result = "-";
				}
			}
			catch
			{
				result = "-";
			}
			return result;
		}

		public static Vehicle Deserialize(string filename)
		{
			try
			{
				if (File.Exists(filename))
				{
					using (FileStream fileStream = File.OpenRead(filename))
					{
						using (XmlTextReader xmlTextReader = new XmlTextReader(fileStream))
						{
							Vehicle vehicle = (Vehicle)new XmlSerializer(typeof(Vehicle)).Deserialize(xmlTextReader);
							vehicle.CalculateFaultProperties(null);
							return vehicle;
						}
					}
				}
				return null;
			}
			catch (Exception)
			{
				//Log.WarningException(Log.CurrentMethod() + "()", exception);
			}
			return null;
		}

		public object Clone()
		{
			return base.MemberwiseClone();
		}

		public Vehicle DeepClone()
		{
			Vehicle result;
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Vehicle));
				using (MemoryStream memoryStream = new MemoryStream())
				{
					xmlSerializer.Serialize(memoryStream, this);
					memoryStream.Seek(0L, SeekOrigin.Begin);
					Vehicle vehicle = (Vehicle)xmlSerializer.Deserialize(memoryStream);
					vehicle.CalculateFaultProperties(null);
					result = vehicle;
				}
			}
			catch (Exception)
			{
				//Log.WarningException(Log.CurrentMethod() + "()", exception);
				throw;
			}
			return result;
		}
#if false
		public bool IsVINLessEReihe()
		{
			string ereihe = base.Ereihe;
			if (ereihe != null)
			{
				uint num = < PrivateImplementationDetails >.ComputeStringHash(ereihe);
				if (num <= 1523257365U)
				{
					if (num <= 870152785U)
					{
						if (num <= 752709452U)
						{
							if (num != 714685471U)
							{
								if (num != 752709452U)
								{
									return false;
								}
								if (!(ereihe == "247"))
								{
									return false;
								}
							}
							else if (!(ereihe == "K599"))
							{
								return false;
							}
						}
						else if (num != 786117595U)
						{
							if (num != 870152785U)
							{
								return false;
							}
							if (!(ereihe == "248"))
							{
								return false;
							}
						}
						else if (!(ereihe == "259"))
						{
							return false;
						}
					}
					else if (num <= 1276256427U)
					{
						if (num != 1259478808U)
						{
							if (num != 1276256427U)
							{
								return false;
							}
							if (!(ereihe == "259R"))
							{
								return false;
							}
						}
						else if (!(ereihe == "259S"))
						{
							return false;
						}
					}
					else if (num != 1472924508U)
					{
						if (num != 1523257365U)
						{
							return false;
						}
						if (!(ereihe == "R22"))
						{
							return false;
						}
					}
					else if (!(ereihe == "R21"))
					{
						return false;
					}
				}
				else if (num <= 2442436689U)
				{
					if (num <= 1623923079U)
					{
						if (num != 1527920712U)
						{
							if (num != 1623923079U)
							{
								return false;
							}
							if (!(ereihe == "R28"))
							{
								return false;
							}
						}
						else if (!(ereihe == "259C"))
						{
							return false;
						}
					}
					else if (num != 2426497713U)
					{
						if (num != 2442436689U)
						{
							return false;
						}
						if (!(ereihe == "K30"))
						{
							return false;
						}
					}
					else if (!(ereihe == "K41"))
					{
						return false;
					}
				}
				else if (num <= 2845166379U)
				{
					if (num != 2729132584U)
					{
						if (num != 2845166379U)
						{
							return false;
						}
						if (!(ereihe == "247E"))
						{
							return false;
						}
					}
					else if (!(ereihe == "K569"))
					{
						return false;
					}
				}
				else if (num != 2929478274U)
				{
					if (num != 3233663528U)
					{
						if (num != 3434009218U)
						{
							return false;
						}
						if (!(ereihe == "E169"))
						{
							return false;
						}
					}
					else if (!(ereihe == "E189"))
					{
						return false;
					}
				}
				else if (!(ereihe == "K589"))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public bool IsEreiheValid()
		{
			return !string.IsNullOrEmpty(base.Ereihe) && !(base.Ereihe == "UNBEK");
		}

		public ECU GetECUbyDTC(decimal id)
		{
			if (base.ECU != null)
			{
				using (IEnumerator<ECU> enumerator = base.ECU.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ECU ecu = enumerator.Current;
						if (ecu.FEHLER != null)
						{
							foreach (DTC dtc in ecu.FEHLER)
							{
								if (id.Equals(dtc.Id))
								{
									return ecu;
								}
							}
						}
						if (ecu.INFO != null)
						{
							foreach (DTC dtc2 in ecu.INFO)
							{
								if (id.Equals(dtc2.Id))
								{
									return ecu;
								}
							}
						}
					}
					goto IL_D6;
				}
				ECU result;
				return result;
			}
			IL_D6:
			return null;
		}

		public DTC GetDTC(decimal id)
		{
			if (base.ECU != null)
			{
				using (IEnumerator<ECU> enumerator = base.ECU.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ECU ecu = enumerator.Current;
						if (ecu.FEHLER != null)
						{
							foreach (DTC dtc in ecu.FEHLER)
							{
								if (id.Equals(dtc.Id))
								{
									return dtc;
								}
							}
						}
						if (ecu.INFO != null)
						{
							foreach (DTC dtc2 in ecu.INFO)
							{
								if (id.Equals(dtc2.Id))
								{
									return dtc2;
								}
							}
						}
					}
					goto IL_EF;
				}
				DTC result;
				return result;
			}
			IL_EF:
			if (base.CombinedFaults != null)
			{
				return base.CombinedFaults.FirstOrDefault(delegate (DTC item)
				{
					decimal? id2 = item.Id;
					decimal id3 = id;
					return id2.GetValueOrDefault() == id3 & id2 != null;
				});
			}
			return null;
		}

		public DTC GetFaultCode(FaultCode faultCode)
		{
			if (faultCode == null)
			{
				//Log.Warning("Vehicle.GetFaultCode()", "faultCode was null", Array.Empty<object>());
				return null;
			}
			try
			{
				if (base.ECU != null)
				{
					foreach (ECU ecu in base.ECU)
					{
						if (ecu.FEHLER != null)
						{
							foreach (DTC dtc in ecu.FEHLER)
							{
								if (dtc.Id != null)
								{
									decimal? id = dtc.Id;
									decimal id2 = faultCode.ID;
									if (id.GetValueOrDefault() == id2 & id != null)
									{
										return dtc;
									}
								}
								if (dtc.Id == null)
								{
									long? f_ORT = dtc.F_ORT;
									long? f_ORT2 = faultCode.F_ORT;
									if ((f_ORT.GetValueOrDefault() == f_ORT2.GetValueOrDefault() & f_ORT != null == (f_ORT2 != null)) && !dtc.IsVirtual && !dtc.IsCombined)
									{
										return dtc;
									}
								}
							}
						}
					}
				}
				if (base.CombinedFaults != null)
				{
					return base.CombinedFaults.FirstOrDefault(delegate (DTC item)
					{
						decimal? id3 = item.Id;
						decimal signedId = faultCode.SignedId;
						return id3.GetValueOrDefault() == signedId & id3 != null;
					});
				}
			}
			catch (Exception exception)
			{
				//Log.WarningException("Vehicle.GetFaultCode()", exception);
			}
			return null;
		}
#endif
		public void CalculateFaultProperties(IFFMDynamicResolver ffmResolver = null)
		{
#if false
			IEnumerable<Fault> collection = Vehicle.CalculateFaultList(this, base.ECU, base.CombinedFaults, base.ZFS, ffmResolver);
			this.FaultCodeSum = Vehicle.CalculateFaultCodeSum(base.ECU, base.CombinedFaults);
			string method = "Vehicle.CalculateFaultProperties()";
			string msg = "FaultCodeSum changed from \"{0}\" to \"{1}\".";
			object[] array = new object[2];
			int num = 0;
			IList<Fault> list = this.FaultList;
			array[num] = ((list != null) ? new int?(list.Count) : null);
			array[1] = this.FaultCodeSum;
			//Log.Info(method, msg, array);
			this.FaultList = new List<Fault>(collection);
#endif
		}

		public typeECU_Transaction getECUTransaction(ECU transECU, string transId)
		{
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
					foreach (typeECU_Transaction typeECU_Transaction in transECU.TAL)
					{
						if (string.Compare(typeECU_Transaction.transactionId, transId, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return typeECU_Transaction;
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.getECUTransaction()", exception);
			}
			return null;
		}

		public bool hasBusType(BusType bus)
		{
			if (base.ECU != null)
			{
				using (IEnumerator<ECU> enumerator = base.ECU.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.BUS == bus)
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		public bool hasSA(string checkSA)
		{
			if (string.IsNullOrEmpty(checkSA))
			{
				//Log.Warning("CoreFramework.hasSA()", "checkSA was null or empty", Array.Empty<object>());
				return false;
			}
			if (base.FA == null)
			{
				return false;
			}
			FA fa = (this.targetFA != null) ? this.targetFA : base.FA;
			if (fa.SA != null)
			{
				using (IEnumerator<string> enumerator = fa.SA.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (string.Compare(enumerator.Current, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return true;
						}
					}
				}
			}
			if (fa.E_WORT != null)
			{
				using (IEnumerator<string> enumerator = fa.E_WORT.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (string.Compare(enumerator.Current, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return true;
						}
					}
				}
			}
			if (fa.HO_WORT != null)
			{
				using (IEnumerator<string> enumerator = fa.HO_WORT.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (string.Compare(enumerator.Current, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return true;
						}
					}
                    return fa.DealerInstalledSA != null && fa.DealerInstalledSA.Any((string item) => string.Equals(item, checkSA, StringComparison.OrdinalIgnoreCase));
				}
			}

            return false;
        }

		public bool HasUnidentifiedECU()
		{
			bool flag = false;
			if (base.ECU != null)
			{
				foreach (ECU ecu in base.ECU)
				{
					if (string.IsNullOrEmpty(ecu.VARIANTE) || !ecu.COMMUNICATION_SUCCESSFULLY)
					{
						flag |= true;
					}
				}
				return flag;
			}
			return true;
		}

		public bool? hasFFM(string checkFFM)
		{
			if (string.IsNullOrEmpty(checkFFM))
			{
				//Log.Warning("CoreFramework.hasFFM()", "checkFFM was null or empty", Array.Empty<object>());
				return new bool?(true);
			}
			if (base.FFM != null)
			{
				using (IEnumerator<FFMResult> enumerator = base.FFM.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						FFMResult ffmresult = enumerator.Current;
						if (string.Compare(ffmresult.Name, checkFFM, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return ffmresult.Result;
						}
					}
                    return null;
				}
			}
			return null;
		}

		public void AddOrUpdateFFM(FFMResult ffm)
		{
			if (base.FFM != null && ffm != null)
			{
				foreach (FFMResult ffmresult in base.FFM)
				{
					if (string.Compare(ffmresult.Name, ffm.Name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						ffmresult.ID = ffm.ID;
						ffmresult.Evaluation = ffm.Evaluation;
						ffmresult.ReEvaluationNeeded = ffm.ReEvaluationNeeded;
						ffmresult.Result = ffm.Result;
						return;
					}
				}
				base.FFM.Add(ffm);
			}
		}

		public ECU getECU(long? sgAdr)
		{
			try
			{
				foreach (ECU ecu in base.ECU)
				{
					long id_SG_ADR = ecu.ID_SG_ADR;
					long? num = sgAdr;
					if (id_SG_ADR == num.GetValueOrDefault() & num != null)
					{
						return ecu;
					}
					if (!string.IsNullOrEmpty(ecu.ECU_ADR))
					{
						string a = string.Empty;
						if (ecu.ECU_ADR.Length >= 4 && ecu.ECU_ADR.Substring(0, 2).ToLower() == "0x")
						{
							a = ecu.ECU_ADR.ToUpper().Substring(2);
						}
						if (ecu.ECU_ADR.Length == 2)
						{
							a = ecu.ECU_ADR.ToUpper();
						}
						if (a == string.Format(CultureInfo.InvariantCulture, "{0:X2}", sgAdr))
						{
							return ecu;
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.getECU()", exception);
			}
			return null;
		}

		public ECU getECU(long? sgAdr, long? subAddress)
		{
			try
			{
				foreach (ECU ecu in base.ECU)
				{
					long id_SG_ADR = ecu.ID_SG_ADR;
					long? num = sgAdr;
					if (id_SG_ADR == num.GetValueOrDefault() & num != null)
					{
						num = ecu.ID_LIN_SLAVE_ADR;
						long? num2 = subAddress;
						if (num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null))
						{
							return ecu;
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehcile.getECU()", exception);
			}
			return null;
		}

		public ECU getECUbyECU_SGBD(string ECU_SGBD)
		{
			if (string.IsNullOrEmpty(ECU_SGBD))
			{
				return null;
			}
			try
			{
				foreach (string b in ECU_SGBD.Split(new char[]
				{
					'|'
				}))
				{
					foreach (ECU ecu in base.ECU)
					{
						if (string.Equals(ecu.ECU_SGBD, b, StringComparison.OrdinalIgnoreCase) || string.Equals(ecu.VARIANTE, b, StringComparison.OrdinalIgnoreCase))
						{
							return ecu;
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.getECUbyECU_SGBD()", exception);
                return null;
			}
			return null;
		}

		public ECU getECUbyTITLE_ECUTREE(string grobName)
		{
			if (string.IsNullOrEmpty(grobName))
			{
				return null;
			}
			try
			{
				foreach (ECU ecu in base.ECU)
				{
					if (string.Compare(ecu.TITLE_ECUTREE, grobName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return ecu;
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.getECUbyTITLE_ECUTREE()", exception);
			}
			return null;
		}

		public ECU getECUbyECU_GRUPPE(string ECU_GRUPPE)
		{
			if (string.IsNullOrEmpty(ECU_GRUPPE))
			{
				//Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "parameter was null or empty", Array.Empty<object>());
				return null;
			}
			if (base.ECU == null)
			{
				//Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "ECU was null", Array.Empty<object>());
				return null;
			}
			try
			{
				foreach (ECU ecu in base.ECU)
				{
					if (!string.IsNullOrEmpty(ecu.ECU_GRUPPE))
					{
						string[] array = ECU_GRUPPE.Split(new char[]
						{
							'|'
						});
						foreach (string a in ecu.ECU_GRUPPE.Split(new char[]
						{
							'|'
						}))
						{
							foreach (string b in array)
							{
								if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
								{
									return ecu;
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.getECUbyECU_GRUPPE()", exception);
			}
			return null;
		}

		public uint getDiagProtECUCount(typeDiagProtocoll ecuDiag)
		{
			uint num = 0U;
			try
			{
				using (IEnumerator<ECU> enumerator = base.ECU.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.DiagProtocoll == ecuDiag)
						{
							num += 1U;
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.WarningException("Vehcile.getECU()", exception);
			}
			return num;
		}
#if false
		public typeCBSInfo getCBSMeasurementValue(typeCBSMeaurementType mType)
		{
			try
			{
				if (base.CBS == null)
				{
					return null;
				}
				foreach (typeCBSInfo typeCBSInfo in base.CBS)
				{
					if (typeCBSInfo.Type == mType)
					{
						return typeCBSInfo;
					}
				}
				return null;
			}
			catch (Exception exception)
			{
				Log.WarningException("Vehicle.getCBSMeasurementValue()", exception);
			}
			return null;
		}

		public bool addOrUpdateCBSMeasurementValue(typeCBSInfo cbsNew)
		{
			try
			{
				if (cbsNew == null)
				{
					return false;
				}
				if (base.CBS == null)
				{
					base.CBS = new ObservableCollection<typeCBSInfo>();
				}
				foreach (typeCBSInfo typeCBSInfo in base.CBS)
				{
					if (typeCBSInfo.Type == cbsNew.Type)
					{
						base.CBS.Remove(typeCBSInfo);
						base.CBS.Add(cbsNew);
						return true;
					}
				}
				base.CBS.Add(cbsNew);
				return true;
			}
			catch (Exception exception)
			{
				Log.WarningException("Vehicle.addOrUpdateCBSMeasurementValue()", exception);
			}
			return false;
		}

		public bool addOrUpdateCBSMeasurementValues(IList<typeCBSInfo> cbsNewList)
		{
			try
			{
				if (cbsNewList == null)
				{
					return false;
				}
				if (base.CBS == null)
				{
					base.CBS = new ObservableCollection<typeCBSInfo>();
				}
				foreach (typeCBSInfo typeCBSInfo in cbsNewList)
				{
					bool flag = false;
					foreach (typeCBSInfo typeCBSInfo2 in base.CBS)
					{
						if (typeCBSInfo2.Type == typeCBSInfo.Type)
						{
							int num = base.CBS.IndexOf(typeCBSInfo2);
							if (num >= 0 && num < base.CBS.Count)
							{
								base.CBS[num] = typeCBSInfo;
							}
							flag = true;
						}
					}
					if (!flag)
					{
						base.CBS.Add(typeCBSInfo);
					}
				}
				return true;
			}
			catch (Exception exception)
			{
				Log.WarningException("Vehicle.addOrUpdateCBSMeasurementValue()", exception);
			}
			return false;
		}
#endif
		public void AddEcu(ECU ecu)
		{
			base.ECU.Add(ecu);
		}

		public bool AddOrUpdateECU(ECU nECU)
		{
			try
			{
				if (nECU == null)
				{
					return false;
				}
				if (base.ECU == null)
				{
					base.ECU = new ObservableCollection<ECU>();
				}
				foreach (ECU ecu in base.ECU)
				{
					if (ecu.ID_SG_ADR == nECU.ID_SG_ADR)
					{
						int num = base.ECU.IndexOf(ecu);
						if (num >= 0 && num < base.ECU.Count)
						{
							base.ECU[num] = nECU;
							return true;
						}
					}
				}
				base.ECU.Add(nECU);
				return true;
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.AddOrUpdateECU()", exception);
			}
			return false;
		}

		public bool getISTACharacteristics(decimal id, out string value, long datavalueId, ValidationRuleInternalResults internalResult)
		{
            PdszDatabase.CharacteristicRoots characteristicRootsById = ClientContext.Database?.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
			if (characteristicRootsById != null)
			{
				return new VehicleCharacteristicVehicleHelper().GetISTACharacteristics(characteristicRootsById, out value, id, this, datavalueId, internalResult);
			}
			value = "???";
			return false;
		}

		public void UpdateStatus(string name, StateType type, double? progress)
		{
			try
			{
				string status_FunctionName = base.Status_FunctionName;
				StateType status_FunctionState = base.Status_FunctionState;
				base.Status_FunctionName = name;
				base.Status_FunctionState = type;
				base.Status_FunctionStateLastChangeTime = DateTime.Now;
				if (progress != null)
				{
					base.Status_FunctionProgress = progress.Value;
				}
				this.IsNoVehicleCommunicationRunning = (base.Status_FunctionState != StateType.running);
			}
			catch (Exception)
			{
				//Log.WarningException("Vehicle.UpdateStatus()", exception);
			}
		}

		[XmlIgnore]
		public bool IsNoVehicleCommunicationRunning
		{
			get
			{
				return this.noVehicleCommunicationRunning;
			}
			set
			{
				this.noVehicleCommunicationRunning = value;
				this.OnPropertyChanged("IsNoVehicleCommunicationRunning");
			}
		}
#if false
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
					return true;
				}
				if (iLevelExpressions.Contains("&"))
				{
					flag2 = false;
					flag = true;
				}
				string[] separator = new string[]
				{
					"&",
					"|"
				};
				foreach (string text in iLevelExpressions.Split(separator, StringSplitOptions.RemoveEmptyEntries))
				{
					string[] separator2 = new string[]
					{
						","
					};
					string[] array2 = text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
					if (array2.Length == 2)
					{
						if (string.Compare(base.ILevel, 0, array2[1], 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
						{
							string text2 = array2[0];
							if (text2 != null)
							{
								uint num = < PrivateImplementationDetails >.ComputeStringHash(text2);
								if (num <= 957132539U)
								{
									if (num != 284975636U)
									{
										if (num != 940354920U)
										{
											if (num == 957132539U)
											{
												if (text2 == "<")
												{
													if (flag2)
													{
														bool flag3 = flag;
														int? num3 = FormatConverter.ExtractNumericalILevel(base.ILevel);
														int? num2 = FormatConverter.ExtractNumericalILevel(array2[1]);
														flag = (flag3 | (num3.GetValueOrDefault() < num2.GetValueOrDefault() & (num3 != null & num2 != null)));
													}
													else
													{
														bool flag4 = flag;
														int? num2 = FormatConverter.ExtractNumericalILevel(base.ILevel);
														int? num3 = FormatConverter.ExtractNumericalILevel(array2[1]);
														flag = (flag4 & (num2.GetValueOrDefault() < num3.GetValueOrDefault() & (num2 != null & num3 != null)));
													}
												}
											}
										}
										else if (text2 == "=")
										{
											if (flag2)
											{
												bool flag5 = flag;
												int? num2 = FormatConverter.ExtractNumericalILevel(base.ILevel);
												int? num3 = FormatConverter.ExtractNumericalILevel(array2[1]);
												flag = (flag5 | (num2.GetValueOrDefault() == num3.GetValueOrDefault() & num2 != null == (num3 != null)));
											}
											else
											{
												bool flag6 = flag;
												int? num3 = FormatConverter.ExtractNumericalILevel(base.ILevel);
												int? num2 = FormatConverter.ExtractNumericalILevel(array2[1]);
												flag = (flag6 & (num3.GetValueOrDefault() == num2.GetValueOrDefault() & num3 != null == (num2 != null)));
											}
										}
									}
									else if (text2 == ">=")
									{
										if (flag2)
										{
											bool flag7 = flag;
											int? num3 = FormatConverter.ExtractNumericalILevel(base.ILevel);
											int? num2 = FormatConverter.ExtractNumericalILevel(array2[1]);
											flag = (flag7 | (num3.GetValueOrDefault() >= num2.GetValueOrDefault() & (num3 != null & num2 != null)));
										}
										else
										{
											bool flag8 = flag;
											int? num2 = FormatConverter.ExtractNumericalILevel(base.ILevel);
											int? num3 = FormatConverter.ExtractNumericalILevel(array2[1]);
											flag = (flag8 & (num2.GetValueOrDefault() >= num3.GetValueOrDefault() & (num2 != null & num3 != null)));
										}
									}
								}
								else
								{
									if (num <= 2428715011U)
									{
										if (num != 990687777U)
										{
											if (num != 2428715011U)
											{
												goto IL_757;
											}
											if (!(text2 == "!="))
											{
												goto IL_757;
											}
										}
										else
										{
											if (!(text2 == ">"))
											{
												goto IL_757;
											}
											int? num2;
											int? num3;
											if (flag2)
											{
												bool flag9 = flag;
												num2 = FormatConverter.ExtractNumericalILevel(base.ILevel);
												num3 = FormatConverter.ExtractNumericalILevel(array2[1]);
												flag = (flag9 | (num2.GetValueOrDefault() > num3.GetValueOrDefault() & (num2 != null & num3 != null)));
												goto IL_757;
											}
											bool flag10 = flag;
											num3 = FormatConverter.ExtractNumericalILevel(base.ILevel);
											num2 = FormatConverter.ExtractNumericalILevel(array2[1]);
											flag = (flag10 & (num3.GetValueOrDefault() > num2.GetValueOrDefault() & (num3 != null & num2 != null)));
											goto IL_757;
										}
									}
									else if (num != 2482446367U)
									{
										if (num != 2499223986U)
										{
											goto IL_757;
										}
										if (!(text2 == "<="))
										{
											goto IL_757;
										}
										int? num2;
										int? num3;
										if (flag2)
										{
											bool flag11 = flag;
											num2 = FormatConverter.ExtractNumericalILevel(base.ILevel);
											num3 = FormatConverter.ExtractNumericalILevel(array2[1]);
											flag = (flag11 | (num2.GetValueOrDefault() <= num3.GetValueOrDefault() & (num2 != null & num3 != null)));
											goto IL_757;
										}
										bool flag12 = flag;
										num3 = FormatConverter.ExtractNumericalILevel(base.ILevel);
										num2 = FormatConverter.ExtractNumericalILevel(array2[1]);
										flag = (flag12 & (num3.GetValueOrDefault() <= num2.GetValueOrDefault() & (num3 != null & num2 != null)));
										goto IL_757;
									}
									else if (!(text2 == "<>"))
									{
										goto IL_757;
									}
									if (flag2)
									{
										bool flag13 = flag;
										int? num3 = FormatConverter.ExtractNumericalILevel(base.ILevel);
										int? num2 = FormatConverter.ExtractNumericalILevel(array2[1]);
										flag = (flag13 | !(num3.GetValueOrDefault() == num2.GetValueOrDefault() & num3 != null == (num2 != null)));
									}
									else
									{
										bool flag14 = flag;
										int? num2 = FormatConverter.ExtractNumericalILevel(base.ILevel);
										int? num3 = FormatConverter.ExtractNumericalILevel(array2[1]);
										flag = (flag14 & !(num2.GetValueOrDefault() == num3.GetValueOrDefault() & num2 != null == (num3 != null)));
									}
								}
							}
						}
						else
						{
							//Log.Warning("Vehicle.evalILevelExpression()", "iLevel main type does not match", Array.Empty<object>());
						}
					}
					IL_757:;
				}
			}
			catch (Exception exception)
			{
				//Log.WarningException("Vehicle.evalILevelExpression()", exception);
				return true;
			}
			return flag;
		}

		public bool? HasMSAButton()
		{
			string text = this.Produktlinie.ToUpper();
			if (text != null)
			{
				uint num = < PrivateImplementationDetails >.ComputeStringHash(text);
				if (num <= 217051978U)
				{
					if (num <= 183496740U)
					{
						if (num != 149941502U)
						{
							if (num != 166719121U)
							{
								if (num != 183496740U)
								{
									goto IL_212;
								}
								if (!(text == "PL5"))
								{
									goto IL_212;
								}
							}
							else if (!(text == "PL2"))
							{
								goto IL_212;
							}
						}
						else if (!(text == "PL3"))
						{
							goto IL_212;
						}
					}
					else if (num != 200274359U)
					{
						if (num != 204446827U)
						{
							if (num != 217051978U)
							{
								goto IL_212;
							}
							if (!(text == "PL7"))
							{
								goto IL_212;
							}
						}
						else
						{
							if (!(text == "PL6-ALT"))
							{
								goto IL_212;
							}
							if (base.FA != null && base.FA.C_DATETIME != null && base.FA.C_DATETIME > Vehicle.LciDateE60)
							{
								return new bool?(true);
							}
							return new bool?(false);
						}
					}
					else if (!(text == "PL4"))
					{
						goto IL_212;
					}
				}
				else
				{
					if (num > 2184974868U)
					{
						if (num <= 3046534054U)
						{
							if (num != 2826802770U)
							{
								if (num != 3046534054U)
								{
									goto IL_212;
								}
								if (!(text == "PL5-ALT"))
								{
									goto IL_212;
								}
							}
							else
							{
								if (!(text == "PLLI"))
								{
									goto IL_212;
								}
								goto IL_20B;
							}
						}
						else if (num != 3162355150U)
						{
							if (num != 3604331608U)
							{
								goto IL_212;
							}
							if (!(text == "PL3-ALT"))
							{
								goto IL_212;
							}
						}
						else
						{
							if (text == "PLLU")
							{
								goto IL_20B;
							}
							goto IL_212;
						}
						return new bool?(false);
					}
					if (num != 233829597U)
					{
						if (num != 671913016U)
						{
							if (num != 2184974868U)
							{
								goto IL_212;
							}
							if (!(text == "35LG"))
							{
								goto IL_212;
							}
						}
						else
						{
							if (!(text == "-"))
							{
								goto IL_212;
							}
							goto IL_212;
						}
					}
					else if (!(text == "PL6"))
					{
						goto IL_212;
					}
				}
				IL_20B:
				return new bool?(true);
			}
			IL_212:
			return null;
		}

		public bool isECUAlreadyScanned(ECU checkSG)
		{
			try
			{
				foreach (ECU ecu in base.ECU)
				{
					if (ecu.ID_SG_ADR == checkSG.ID_SG_ADR)
					{
						return true;
					}
					if (!string.IsNullOrEmpty(ecu.ECU_ADR) && !string.IsNullOrEmpty(checkSG.ECU_ADR) && string.Compare(ecu.ECU_ADR, checkSG.ECU_ADR, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
				}
			}
			catch (Exception exception)
			{
				//Log.WarningException("Vehicle.isECUAlreadyScanned()", exception);
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
					if (resultName != null)
					{
						uint num = < PrivateImplementationDetails >.ComputeStringHash(resultName);
						if (num > 1444285944U)
						{
							if (num <= 1946743292U)
							{
								if (num != 1857409602U)
								{
									if (num != 1946743292U)
									{
										goto IL_31E;
									}
									if (resultName == "/Result/HOWortListe")
									{
										string text = string.Empty;
										foreach (string str in base.FA.HO_WORT)
										{
											text = text + str + ",";
										}
										text = text.TrimEnd(new char[]
										{
											','
										});
										obj = text;
										goto IL_337;
									}
									goto IL_31E;
								}
								else if (!(resultName == "/Result/GruppenListe"))
								{
									goto IL_31E;
								}
							}
							else if (num != 2030676002U)
							{
								if (num != 3210309580U)
								{
									if (num != 3286336872U)
									{
										goto IL_31E;
									}
									if (resultName == "/Result/SonderAusstattungsListe")
									{
										string text2 = string.Empty;
										foreach (string str2 in base.FA.SA)
										{
											text2 = text2 + str2 + ",";
										}
										text2 = text2.TrimEnd(new char[]
										{
											','
										});
										obj = text2;
										goto IL_337;
									}
									goto IL_31E;
								}
								else if (!(resultName == "/Result/DList"))
								{
									goto IL_31E;
								}
							}
							else
							{
								if (resultName == "/Result/EWortListe")
								{
									string text3 = string.Empty;
									foreach (string str3 in base.FA.E_WORT)
									{
										text3 = text3 + str3 + ",";
									}
									text3 = text3.TrimEnd(new char[]
									{
										','
									});
									obj = text3;
									goto IL_337;
								}
								goto IL_31E;
							}
							string text4 = string.Empty;
							foreach (ECU ecu in base.ECU)
							{
								text4 = text4 + ecu.ECU_GRUPPE + ",";
							}
							text4 = text4.TrimEnd(new char[]
							{
								','
							});
							obj = text4;
							goto IL_337;
						}
						if (num <= 1032288925U)
						{
							if (num != 680186856U)
							{
								if (num == 1032288925U)
								{
									if (resultName == "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/Fahrzeugauftrag")
									{
										obj = base.FA.STANDARD_FA;
										goto IL_337;
									}
								}
							}
							else if (resultName == "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/BaureihenVerbund")
							{
								obj = this.BasisEReihe;
								goto IL_337;
							}
						}
						else if (num != 1270135361U)
						{
							if (num == 1444285944U)
							{
								if (resultName == "/Result/Baustand")
								{
									obj = base.FA.C_DATE;
									goto IL_337;
								}
							}
						}
						else if (resultName == "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/IStufe")
						{
							obj = base.ILevel;
							goto IL_337;
						}
					}
					IL_31E:
					Log.Error("VehicleHelper.getResultAs<T>", "Unknown resultName '{0}' found!", new object[]
					{
						resultName
					});
					IL_337:
					if (obj != null)
					{
						if (obj.GetType() != typeFromHandle)
						{
							return (T)((object)Convert.ChangeType(obj, typeFromHandle));
						}
						return (T)((object)obj);
					}
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("Vehicle.getISTAResultAs(string resultName)", exception);
			}
			return default(T);
		}

		public void AddDiagCode(string diagCodeString, string diagCodeSuffixString, string originatingAblauf, IList<string> reparaturPaketList, bool teileClearingFlag)
		{
			if (!string.IsNullOrEmpty(diagCodeString))
			{
				if (base.DiagCodes == null)
				{
					base.DiagCodes = new ObservableCollection<typeDiagCode>();
				}
				typeDiagCode typeDiagCode = new typeDiagCode();
				typeDiagCode.DiagnoseCode = diagCodeString;
				typeDiagCode.DiagnoseCodeSuffix = diagCodeSuffixString;
				typeDiagCode.Origin = ((originatingAblauf == null) ? string.Empty : originatingAblauf);
				if (reparaturPaketList != null)
				{
					typeDiagCode.ReparaturPaket = new ObservableCollection<string>(reparaturPaketList);
				}
				else
				{
					typeDiagCode.ReparaturPaket = new ObservableCollection<string>();
				}
				typeDiagCode.TeileClearing = teileClearingFlag;
				base.DiagCodes.Add(typeDiagCode);
				if (!string.IsNullOrEmpty(diagCodeString) && !this.diagCodesProgramming.Contains(diagCodeString))
				{
					this.diagCodesProgramming.Add(diagCodeString);
				}
			}
		}
#endif
		public bool IsPreE65Vehicle()
		{
			return !string.IsNullOrEmpty(base.Ereihe) && (Regex.Match(base.Ereihe, "^E[0-5][0-9]$").Success || Regex.Match(base.Ereihe, "^E6[0-4]$").Success);
		}

		public bool IsPreDS2Vehicle()
		{
			if (!string.IsNullOrEmpty(base.Ereihe))
			{
				if (Regex.Match(base.Ereihe, "^E[0-3][0-5]$").Success)
				{
					return true;
				}
				if ("E36".Equals(base.Ereihe))
				{
					return this.C_DATETIME < Vehicle.LciDateE36;
				}
			}
			return false;
		}

		public bool IsMotorcycle()
		{
			return base.BNType == BNType.BN2000_MOTORBIKE || base.BNType == BNType.BN2020_MOTORBIKE || base.BNType == BNType.BNK01X_MOTORBIKE || base.BNType == BNType.BN2000_GIBBS || base.BNType == BNType.BN2020_CAMPAGNA;
		}

		public bool IsRRSeries2()
		{
			return ("RR1".Equals(base.Ereihe) || "RR2".Equals(base.Ereihe) || "RR3".Equals(base.Ereihe)) && ("RR1_2020".Equals(base.Gsgbd) || (this.C_DATETIME != null && this.C_DATETIME > Vehicle.lciRRS2));
		}

		public bool IsPowertrainSystemCustomerVehicle()
		{
			return base.BNType == BNType.BN2000_GIBBS || base.BNType == BNType.BN2000_RODING || base.BNType == BNType.BN2000_WIESMANN || base.BNType == BNType.BN2000_PGO;
		}

		IEcu IVehicle.getECU(long? sgAdr)
		{
			return this.getECU(sgAdr);
		}

		IEcu IVehicle.getECU(long? sgAdr, long? subAddress)
		{
			return this.getECU(sgAdr, subAddress);
		}

		IEcu IVehicle.getECUbyECU_GRUPPE(string ECU_GRUPPE)
		{
			return this.getECUbyECU_GRUPPE(ECU_GRUPPE);
		}

		public bool IsVehicleLockedDown()
		{
			return false;
		}

		public bool? IsABSVehicle()
		{
			if (base.ECU != null && base.ECU.Count > 0)
			{
				string[] array = new string[]
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
				ECU ecu = this.getECU(new long?(86L), null);
				if (ecu != null && ecu.IDENT_SUCCESSFULLY)
				{
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].Equals(ecu.VARIANTE, StringComparison.OrdinalIgnoreCase))
						{
							return new bool?(true);
						}
					}
					return new bool?(false);
				}
				ecu = this.getECU(new long?(41L), null);
				if (ecu != null && ecu.IDENT_SUCCESSFULLY)
				{
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].Equals(ecu.VARIANTE, StringComparison.OrdinalIgnoreCase))
						{
							return new bool?(true);
						}
					}
					return new bool?(false);
				}
				ecu = this.getECU(new long?(54L), null);
				if (ecu != null && ecu.IDENT_SUCCESSFULLY)
				{
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].Equals(ecu.VARIANTE, StringComparison.OrdinalIgnoreCase))
						{
							return new bool?(true);
						}
					}
					return new bool?(false);
				}
			}
			return null;
		}
#if false
		private static ObservableCollection<Fault> CalculateFaultList(Vehicle vehicle, IEnumerable<ECU> ecus, IEnumerable<DTC> combinedFaults, ObservableCollection<ZFSResult> zfs, IFFMDynamicResolver ffmFesolver = null)
		{
			bool flag = true;
			bool flag2 = true;
			if (ConfigSettings.OperationalMode != OperationalMode.ISTA)
			{
				flag = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideBogusFaults", true);
				flag2 = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideUnknownFaults", false);
			}
			ObservableCollection<Fault> observableCollection = new ObservableCollection<Fault>();
			try
			{
				if (ecus != null)
				{
					foreach (ECU ecu in from item in ecus
										where item.FEHLER != null
										select item)
					{
						foreach (DTC dtc in ecu.FEHLER)
						{
							if (dtc.IsVirtual)
							{
								Vehicle.UpdateVirtualDtcTime(dtc, vehicle, zfs);
							}
							Fault fault = new Fault(ecu, dtc, zfs, vehicle.IsNewFaultMemoryActive);
							bool? relevance = dtc.Relevance;
							if (relevance.GetValueOrDefault() & relevance != null)
							{
								if (ffmFesolver != null && ConfigSettings.getConfigStringAsBoolean("EnableRelevanceFaultCode", true))
								{
									fault.ResolveRelevanceFaultCode(vehicle, ffmFesolver);
									relevance = fault.DTC.Relevance;
									if (relevance.GetValueOrDefault() & relevance != null)
									{
										observableCollection.AddIfNotContains(fault);
									}
								}
								else
								{
									observableCollection.AddIfNotContains(fault);
								}
							}
							else
							{
								relevance = dtc.Relevance;
								if ((!relevance.GetValueOrDefault() & relevance != null) && !flag)
								{
									observableCollection.AddIfNotContains(new Fault(ecu, dtc, zfs, vehicle.IsNewFaultMemoryActive));
								}
								else if (dtc.Relevance == null && !flag2)
								{
									observableCollection.AddIfNotContains(new Fault(ecu, dtc, zfs, vehicle.IsNewFaultMemoryActive));
								}
							}
						}
					}
				}
				if (combinedFaults == null)
				{
					return observableCollection;
				}
				foreach (DTC dtc2 in combinedFaults)
				{
					Fault fault2 = new Fault(null, dtc2, null, vehicle.IsNewFaultMemoryActive);
					fault2.ResolveLabels(vehicle, null);
					observableCollection.AddIfNotContains(fault2);
				}
			}
			catch (Exception exception)
			{
				//Log.ErrorException("Vehicle.CalculateFaultList()", exception);
			}
			return observableCollection;
		}

		private static void UpdateVirtualDtcTime(DTC dtc, Vehicle vehicle, ObservableCollection<ZFSResult> zfs)
		{
			if (vehicle.IsNewFaultMemoryActive)
			{
				ulong? num = (from x in zfs.Where(delegate (ZFSResult x)
				{
					long? stat_DM_MELDUNG_NR = x.STAT_DM_MELDUNG_NR;
					long? f_ORT = dtc.F_ORT;
					return stat_DM_MELDUNG_NR.GetValueOrDefault() == f_ORT.GetValueOrDefault() & stat_DM_MELDUNG_NR != null == (f_ORT != null);
				}).ToList<ZFSResult>()
							  select x.STAT_DM_ZEITSTEMPEL).Max<ulong?>();
				if (num != null)
				{
					ulong? num2 = num;
					if (num2.GetValueOrDefault() > 0UL & num2 != null)
					{
						DateTime vehicleLifeStartDate = vehicle.VehicleLifeStartDate;
						if (vehicleLifeStartDate != default(DateTime))
						{
							vehicleLifeStartDate.AddSeconds(num.Value);
							dtc.Current.F_UW_ZEIT = new long?((long)TimeSpan.FromTicks(vehicleLifeStartDate.Ticks).TotalSeconds + 1L);
						}
						else
						{
							dtc.Current.F_UW_ZEIT = new long?((long)(num.Value + 1UL));
						}
					}
				}
				long? f_UW_ZEIT = dtc.Current.F_UW_ZEIT;
				if (f_UW_ZEIT.GetValueOrDefault() <= 0L & f_UW_ZEIT != null)
				{
					TimeSpan timeSpan = TimeSpan.FromTicks(vehicle.Status_FunctionStateLastChangeTime.Ticks);
					dtc.Current.F_UW_ZEIT = new long?((long)timeSpan.TotalSeconds);
				}
			}
		}

		private static int? CalculateFaultCodeSum(IEnumerable<IEcu> ecus, IEnumerable<DTC> combinedFaults)
		{
			int num = 0;
			bool flag = true;
			bool flag2 = true;
			if (ConfigSettings.OperationalMode != OperationalMode.ISTA)
			{
				flag = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideBogusFaults", true);
				flag2 = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideUnknownFaults", false);
			}
			int? result;
			try
			{
				if (ecus != null)
				{
					foreach (IEcu ecu in ecus)
					{
						if (ecu.FEHLER != null)
						{
							foreach (IDtc dtc in ecu.FEHLER)
							{
								bool? relevance = dtc.Relevance;
								if (relevance != null)
								{
									if (relevance.GetValueOrDefault())
									{
										num++;
									}
									else if (!flag)
									{
										num++;
									}
								}
								else if (!flag2)
								{
									num++;
								}
							}
						}
					}
				}
				if (combinedFaults != null && combinedFaults.Any<DTC>())
				{
					num += combinedFaults.Count<DTC>();
				}
				if (num == 0)
				{
					if (ecus != null && ecus.Any<IEcu>())
					{
						if (!ecus.Any((IEcu item) => !item.FS_SUCCESSFULLY && !item.BUS.ToString().Contains("VIRTUAL")))
						{
							goto IL_11B;
						}
					}
					result = null;
					return result;
				}
				IL_11B:
				result = new int?(num);
			}
			catch (Exception exception)
			{
				//Log.WarningException("Vehicle.CalculateFaultCodeSum()", exception);
				result = null;
			}
			return result;
		}

		public void AddCombinedDTC(DTC dtc)
		{
			if (dtc == null)
			{
				Log.Warning("Vehicle.AddCombinedDTC()", "dtc was null", Array.Empty<object>());
				return;
			}
			if (dtc.IsVirtual && dtc.IsCombined && base.CombinedFaults != null)
			{
				base.CombinedFaults.AddIfNotContains(dtc);
			}
		}
#endif
		public bool GetProgrammingEnabledForBn(string bn)
		{
			return Vehicle.GetBnTypes(bn).Contains(base.BNType);
		}

		public bool IsProgrammingSupported(bool considerLogisticBase)
        {
            return true;
            //return (ConfigSettings.IsProgrammingEnabled() || (considerLogisticBase && ConfigSettings.IsLogisticBaseEnabled())) && this.GetProgrammingEnabledForBn(ConfigSettings.getConfigString("BMW.Rheingold.Programming.BN", "BN2020,BN2020_MOTORBIKE")) && ConfigSettings.OperationalMode != OperationalMode.TELESERVICE;
        }

		private static ISet<BNType> GetBnTypes(string bnTypes)
		{
			ISet<BNType> set = new HashSet<BNType>();
			if (string.IsNullOrEmpty(bnTypes))
			{
				return set;
			}
			foreach (string text in bnTypes.Split(new char[]
			{
				','
			}))
			{
				BNType item;
				if (Enum.TryParse<BNType>(text, false, out item))
				{
					set.Add(item);
				}
			}
			return set;
		}

		public int GetCustomHashCode()
		{
			int num = 37;
			int num2 = 327;
			num = 37 * this.GetHashCode();
			if (!string.IsNullOrWhiteSpace(base.VIN17))
			{
				num += base.VIN17.GetHashCode();
				num *= num2;
			}
			ObservableCollection<ECU> ecu = base.ECU;
			if (ecu != null && ecu.Any<ECU>())
			{
				foreach (ECU ecu2 in base.ECU)
				{
					num += ecu2.GetHashCode();
					num *= num2;
				}
			}
			if (!string.IsNullOrWhiteSpace(base.Ereihe))
			{
				num += base.Ereihe.GetHashCode();
				num *= num2;
			}
			if (this.C_DATETIME != null)
			{
				num += this.C_DATETIME.GetHashCode();
				num *= num2;
			}
			return num;
		}

		public const string BnProgramming = "BN2020,BN2020_MOTORBIKE";

		private static readonly DateTime LciDateE36 = DateTime.Parse("1998-03-01", CultureInfo.InvariantCulture);

		private static readonly DateTime LciDateE60 = DateTime.Parse("2005-09-01", CultureInfo.InvariantCulture);

		//private readonly ObservableCollectionEx<Fault> pKodeList;

		//private readonly ParameterContainer sessionDataStore;

		private string vinRangeType;

		//private string vinRangeTypeLastResolvedType;

		private FA targetFA;

		private bool isBusy;

		//private string productLine;

		//private string doorNumber;

		//private string securityRelevant;

		private DateTime? cDatetimeByModelYearMonth;

		private HashSet<int> validPWFStates;

		private double clamp15MinValue;

		private double clamp30MinValue;

		private bool withLfpBattery;

		private bool isClosingOperationActive;

		private bool powerSafeModeByOldEcus;

		private bool powerSafeModeByNewEcus;

		private bool vehicleTestDone;

		private bool isReadingFastaDataFinished = true;

		private bool vinNotReadbleFromCarAbort;

		private int? faultCodeSum;

		private string targetILevel;

		private readonly ObservableCollection<string> diagCodesProgramming;

		//private IList<Fault> faultList;

		private bool noVehicleCommunicationRunning;

		private string salesDesignationBadgeUIText;

		private string eBezeichnungUIText;

		private const int indexOfFirsHDDAboUpdateInDecimal = 54;

		private bool isNewFaultMemoryActiveField;

		private bool isNewFaultMemoryExpertModeActiveField;

		//private BlockingCollection<VirtualFaultInfo> virtualFaultInfoList;

		private string hmiVersion;

		private bool sp2021Enabled;

		private static readonly DateTime lciRRS2 = DateTime.Parse("2012-05-31", CultureInfo.InvariantCulture);
	}
}
