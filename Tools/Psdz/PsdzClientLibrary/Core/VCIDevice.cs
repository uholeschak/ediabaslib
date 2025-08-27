using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class VCIDevice : IComparable, IVciDevice, INotifyPropertyChanged, IVciDeviceRuleEvaluation, ICloneable
    {
        public enum DeviceTypeDetails
        {
            ICOM = 1,
            ICOMNext = 2,
            Unspecified = 0
        }

        private readonly HashSet<DeviceState> connectableStates = new HashSet<DeviceState>
        {
            DeviceState.Init,
            DeviceState.Lost,
            DeviceState.Sleep,
            DeviceState.Free,
            DeviceState.Found,
            DeviceState.Unregistered,
            DeviceState.Unsupported
        };

        private string receivingIP;

        private bool isMarkedToDefault;

        private BasicFeaturesVci basicFeaturesVci;

        private string accuCapacityField;

        private string kl15VoltageField;

        private string kl30VoltageField;

        private string ownerField;

        private string descriptionField;

        private string stateField;

        private long leastSigBitsField;

        private bool leastSigBitsSpecified1Field;

        private long mostSigBitsField;

        private bool mostSigBitsSpecified1Field;

        private long reserveHandleField;

        private DateTime scanDateField;

        private string serviceField;

        private string kl15TriggerField;

        private string kl30TriggerField;

        private string uUIDField;

        private string imageVersionBootField;

        private string imageVersionApplicationField;

        private string imageVersionPackageField;

        private string counterField;

        private VCIDeviceType vCITypeField;

        private VCIReservationType vCIReservationField;

        private string signalStrengthField;

        private string gatewayField;

        private string vciChannelsField;

        private string netmaskField;

        private string networkTypeField;

        private string iPAddressField;

        private int? portField;

        private int? controlPortField;

        private string devIdField;

        private string devTypeField;

        private string devTypeExtField;

        private string macAddressField;

        private string wlanMacAddressField;

        private string description1Field;

        private string serialField;

        private string vINField;

        private string imagenameField;

        private string colorField;

        private string iFHParameterField;

        private string iFHReservedField;

        private bool forceReInitField;

        private bool usePdmResultField;

        private string pwfStateField;

        private bool connectionLossRecognizedField;

        private bool reconnectFailedField;

        private bool underVoltageRecognizedField;

        private DateTime underVoltageRecognizedLastTimeField;

        private bool underVoltageRecognizedLastTimeFieldSpecified;

        private bool communicationDisturbanceRecognizedField;

        // [UH] added
        private ClientContext _clientContext;

        public VCIDevice(VCIDeviceType vciType, string devid, string description)
		{
			this.ScanDate = DateTime.Now;
			this.DevId = devid;
			this.Description = description;
			this.Imagename = null;
			this.Color = "#73B2F5";
			this.VCIType = vciType;
		}

		[XmlIgnore]
		public DeviceTypeDetails DeviceTypeDetail { get; set; }

		[XmlIgnore]
		public bool IsDead
		{
			get
			{
				return DateTime.Now > this.ScanDate.AddSeconds(15.0);
			}
		}

        [XmlIgnore]
        public bool IsDoIP { get; set; }

        [XmlIgnore]
        public bool IsSimulation { get; set; }

        [XmlIgnore]
        public bool IsAlive => !IsDead;

        [XmlIgnore]
		public bool IsIdentified
		{
			get
			{
				return this.VCIType != VCIDeviceType.UNKNOWN && (!string.IsNullOrEmpty(this.IPAddress) || !string.IsNullOrEmpty(this.DevId));
			}
		}

		[XmlIgnore]
		public DeviceState DeviceState
		{
			get
			{
				DeviceState result = DeviceState.Unregistered;
				int num;
				if (int.TryParse(this.State, out num))
				{
					result = (DeviceState)num;
				}
				return result;
			}
			set
			{
				int num = (int)value;
				this.State = num.ToString(CultureInfo.InvariantCulture);
			}
		}

		[XmlIgnore]
		public BasicFeaturesVci BasicFeatures
		{
			get
			{
                if (!string.IsNullOrEmpty(VIN) && !VIN.Contains("XXXX") && VIN.Length == 17)
                {
                    string text = VIN.Substring(3, 3);
                    switch (VIN[6])
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
                        default:
                            text += VIN[6];
                            break;
                        case 'J':
                            text += "9";
                            break;
                    }
                    List<PsdzDatabase.Characteristics> vehicleIdentByTypeKey = _clientContext?.Database?.GetVehicleIdentByTypeKey(text, false);
                    if (vehicleIdentByTypeKey != null)
                    {
                        BasicFeaturesVci basicFeaturesVci = new BasicFeaturesVci();
                        VehicleCharacteristicVCIDeviceHelper vehicleCharacteristicVCIDeviceHelper = new VehicleCharacteristicVCIDeviceHelper(_clientContext);
                        foreach (PsdzDatabase.Characteristics xep_CHARACTERISTICS in vehicleIdentByTypeKey)
                        {
                            vehicleCharacteristicVCIDeviceHelper.AssignBasicFeaturesVciCharacteristic(xep_CHARACTERISTICS.RootNodeClass, basicFeaturesVci, xep_CHARACTERISTICS);
                        }
                        return basicFeaturesVci;
                    }
                }
				return null;
			}
		}

		public bool IsMarkedToDefault
		{
			get
			{
				return this.isMarkedToDefault;
			}
			set
			{
				this.isMarkedToDefault = value;
				this.OnPropertyChanged("IsMarkedToDefault");
			}
		}

		public bool IsImibR2
		{
			get
			{
				bool result;
				long num;
				if (!(result = (this.VCIType == VCIDeviceType.IMIB && "IMIB_R2".Equals(this.Description1))) && !"IMIB_R1".Equals(this.Description1) && long.TryParse(this.ImageVersionApplication, out num))
				{
					result = (this.VCIType == VCIDeviceType.IMIB && (num >= 20000L || num == 400L));
				}
				return result;
			}
		}

		public bool IsImibNext
		{
			get
			{
				return this.VCIType == VCIDeviceType.IMIB && "IMIB_NX".Equals(this.Description1);
			}
		}

		[XmlIgnore]
		public bool IsConnected { get; set; }

		public bool IsConnectable
		{
			get
			{
				bool result;
				try
				{
					int item;
					bool flag;
					if (int.TryParse(this.State, out item))
					{
						flag = this.connectableStates.Contains((DeviceState)item);
					}
					else
					{
						flag = false;
					}
					result = flag;
				}
				catch (Exception)
				{
					result = false;
				}
				return result;
			}
		}

		public bool IsSupportedImibOrICOM(string[] acceptedImibDevices)
		{
			return this.DevType != "IMIB" || acceptedImibDevices.Contains(this.Description1);
		}

		public bool CheckChannel(string channelId)
		{
			if (!string.IsNullOrEmpty(channelId) && !string.IsNullOrEmpty(this.VciChannels))
			{
				foreach (string text in this.VciChannels.Split(new char[]
				{
					';'
				}))
				{
					if (text.Contains(channelId))
					{
						return text.Contains("+") || text.Contains("*");
					}
				}
				return false;
			}
			return false;
		}

        public string getVCIDescription(VCIDeviceType devType)
        {
            switch (ConfigSettings.CurrentUICulture)
            {
                case "de-DE":
                    switch (devType)
                    {
                        case VCIDeviceType.ICOM:
                            return "Fahrzeug Diagnoseinterface";
                        case VCIDeviceType.IMIB:
                            return "Messtechnik Interface";
                        case VCIDeviceType.ENET:
                            return "Ethernet Direktverbindung";
                        case VCIDeviceType.EDIABAS:
                            return "Ediabas Direktverbindung";
                        case VCIDeviceType.SIM:
                            return "Simulation";
                        case VCIDeviceType.PTT:
                            return "PassThruDevice";
                        default:
                            return "Unbekannt";
                    }
                default:
                    switch (devType)
                    {
                        case VCIDeviceType.ICOM:
                            return "Vehicle diagnostic interface";
                        case VCIDeviceType.IMIB:
                            return "Measurement interface";
                        case VCIDeviceType.ENET:
                            return "Ethernet connection";
                        case VCIDeviceType.EDIABAS:
                            return "Ediabas connection";
                        case VCIDeviceType.SIM:
                            return "Simulation";
                        case VCIDeviceType.PTT:
                            return "PassThruDevice";
                        default:
                            return "Unknown";
                    }
            }
        }

        public string ToAttrList()
		{
			return this.ToAttrList(false);
		}

		public string ToAttrList(bool addLineFeed)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string arg = string.Empty;
			if (addLineFeed)
			{
				arg = "\n";
			}
			try
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(DevId={0}),{1}", this.DevId, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Service={0}),{1}", this.Service, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Serial={0}),{1}", this.Serial, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(MacAddress={0}),{1}", this.MacAddress, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(DevType={0}),{1}", (this.DeviceTypeDetail == DeviceTypeDetails.Unspecified) ? this.DevType : this.DeviceTypeDetail.ToString(), arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ImageVersionBoot={0}),{1}", this.ImageVersionBoot, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ImageVersionApplication={0}),{1}", this.ImageVersionApplication, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ImageVersionPackage={0}),{1}", this.ImageVersionPackage, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Color={0}),{1}", this.Color, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Counter={0}),{1}", this.Counter, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(State={0}),{1}", this.State, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Owner={0}),{1}", this.Owner, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Kl15Voltage={0}),{1}", this.Kl15Voltage, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Kl30Voltage={0}),{1}", this.Kl30Voltage, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(SignalStrength={0}),{1}", this.SignalStrength, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(VIN={0}),{1}", this.VIN, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Gateway={0}),{1}", this.Gateway, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(AccuCapacity={0}),{1}", this.AccuCapacity, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(PowerSupply={0}),{1}", this.PowerSupply, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(VciChannels={0}),{1}", this.VciChannels, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Netmask={0}),{1}", this.Netmask, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(NetworkType={0}),{1}", this.NetworkType, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(UUID={0}),{1}", this.UUID, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Port={0}),{1}", this.Port, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ControlPort={0}),{1}", this.ControlPort, arg);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(PwfState={0})", this.PwfState);
			}
			catch (Exception exception)
			{
				Log.WarningException("VCIDevice.ToAttrList()", exception);
			}
			return stringBuilder.ToString();
		}

		[XmlIgnore]
		public string ReceivingIP
		{
			get
			{
				return this.receivingIP;
			}
			set
			{
				if (value != this.receivingIP)
				{
					this.receivingIP = value;
					this.OnPropertyChanged("ReceivingIP");
				}
			}
		}

		[XmlIgnore]
		public string NetworkTypeLabel
		{
			get
			{
				if (string.IsNullOrEmpty(this.NetworkType))
				{
					return "-";
				}
				if ("1".Equals(this.NetworkType))
				{
					return "WLAN";
				}
				if ("0".Equals(this.NetworkType))
				{
					return "LAN";
				}
				return "UNKNOWN";
			}
		}

		public double? GetClamp30()
		{
			return this.GetVoltageForString(this.Kl30Voltage);
		}

		public double? GetClamp15()
		{
			return this.GetVoltageForString(this.Kl15Voltage);
		}

		private double? GetVoltageForString(string voltage)
		{
			bool flag = Regex.IsMatch(voltage, "\\d+([,.]\\d+)? *(mV||MV|mv|Mv)?");
			if (!string.IsNullOrEmpty(voltage) && flag)
			{
				double? result;
				try
				{
					result = new double?(Convert.ToDouble(new Regex("(mV|MV|mv|Mv)").Replace(voltage, string.Empty).Trim()));
				}
				catch (Exception)
				{
                    return null;
				}
				return result;
			}
			return null;
		}

		[XmlIgnore]
		IBasicFeatures IVciDevice.BasicFeatures
		{
			get
			{
				return this.BasicFeatures;
			}
		}

		[XmlIgnore]
		public ushort PowerSupply { get; set; }

		public NetworkType LocalAdapterNetworkType { get; set; }

		public bool IsVehicleProgrammingPossible
		{
			get
			{
				return this.LocalAdapterNetworkType == BMW.Rheingold.CoreFramework.Contracts.Vehicle.NetworkType.LAN && "0".Equals(this.NetworkType);
			}
		}

		public int CompareTo(object obj)
		{
			VCIDevice vcidevice = obj as VCIDevice;
			if (vcidevice == null)
			{
				return 1;
			}
			if (string.IsNullOrEmpty(vcidevice.DevId))
			{
				return 1;
			}
			int num = this.DevId.Length.CompareTo(vcidevice.DevId.Length);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.DevId, vcidevice.DevId, StringComparison.Ordinal);
		}

		public static string IntIPAddress2String(int ipInt)
		{
			try
			{
				string text = string.Format(CultureInfo.InvariantCulture, "{0}", ipInt.ToString("X8", CultureInfo.InvariantCulture));
				int num = Convert.ToInt32(text.Substring(0, 2), 16);
				int num2 = Convert.ToInt32(text.Substring(2, 2), 16);
				int num3 = Convert.ToInt32(text.Substring(4, 2), 16);
				int num4 = Convert.ToInt32(text.Substring(6, 2), 16);
				return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", new object[]
				{
					num,
					num2,
					num3,
					num4
				});
			}
			catch (Exception exception)
			{
				Log.WarningException("VCIDevice.IntIPAddress2String()", exception);
			}
			return null;
		}

		public static void UUIDString2UUID(string uuid, out long leastSigBits, out long mostSigBits)
		{
			try
			{
				if (!string.IsNullOrEmpty(uuid))
				{
					string text = uuid.Replace("-", string.Empty);
					string value = text.Substring(0, 16);
					string value2 = text.Substring(16, 16);
					leastSigBits = Convert.ToInt64(value2, 16);
					mostSigBits = Convert.ToInt64(value, 16);
					return;
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("VCIGuiDevice.UUIDString2UUID()", exception);
			}
			leastSigBits = 0L;
			mostSigBits = 0L;
		}

		public override int GetHashCode()
		{
			if (!string.IsNullOrEmpty(this.Serial))
			{
				return this.Serial.GetHashCode();
			}
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			VCIDevice vcidevice = obj as VCIDevice;
			if (!string.IsNullOrEmpty(this.Serial) && vcidevice != null)
			{
				return this.Serial.Equals(vcidevice.Serial);
			}
			return base.Equals(obj);
		}

		public void SetAlive()
		{
			this.ScanDate = DateTime.Now;
		}

		public override string ToString()
		{
			return "VCIDevice: " + this.ToAttrList(true);
		}

		public object Clone()
		{
			return new VCIDevice(_clientContext)
			{
				CommunicationDisturbanceRecognized = this.CommunicationDisturbanceRecognized,
				ConnectionLossRecognized = this.ConnectionLossRecognized,
				ControlPort = this.ControlPort,
				Counter = this.Counter,
				Description = this.Description,
				Description1 = this.Description1,
				ForceReInit = this.ForceReInit,
				IFHParameter = this.IFHParameter,
				IFHReserved = this.IFHReserved,
				DevId = this.DevId,
				Service = this.Service,
				Serial = this.Serial,
				MacAddress = this.MacAddress,
				IPAddress = this.IPAddress,
				DevType = this.DevType,
				ImageVersionBoot = this.ImageVersionBoot,
				ImageVersionApplication = this.ImageVersionApplication,
				ImageVersionPackage = this.ImageVersionPackage,
				Imagename = this.Imagename,
				IsConnected = this.IsConnected,
				Kl15Trigger = this.Kl15Trigger,
				//Kl15Voltage = this.Kl15Voltage,
				Kl30Trigger = this.Kl30Trigger,
				//Kl30Voltage = this.Kl30Voltage,
				//NetworkType = this.NetworkType,
				//VIN = this.VIN,
				mostSigBits = this.mostSigBits,
				mostSigBitsSpecified1 = this.mostSigBitsSpecified1,
				//PwfState = this.PwfState,
				ReceivingIP = this.ReceivingIP,
				ReconnectFailed = this.ReconnectFailed,
				ReserveHandle = this.ReserveHandle,
				//CommunicationDisturbanceRecognized = this.CommunicationDisturbanceRecognized,
				//ConnectionLossRecognized = this.ConnectionLossRecognized,
				//ScanDate = this.ScanDate,
				//Service = this.Service,
				UnderVoltageRecognized = this.UnderVoltageRecognized,
				UnderVoltageRecognizedLastTime = this.UnderVoltageRecognizedLastTime,
				UnderVoltageRecognizedLastTimeSpecified = this.UnderVoltageRecognizedLastTimeSpecified,
				VCIReservation = this.VCIReservation,
				Color = this.Color,
				//Counter = this.Counter,
				State = this.State,
				Owner = this.Owner,
				Kl15Voltage = this.Kl15Voltage,
				Kl30Voltage = this.Kl30Voltage,
				SignalStrength = this.SignalStrength,
				VIN = this.VIN,
				Gateway = this.Gateway,
				AccuCapacity = this.AccuCapacity,
				PowerSupply = this.PowerSupply,
				VciChannels = this.VciChannels,
				Netmask = this.Netmask,
				NetworkType = this.NetworkType,
				UUID = this.UUID,
				Port = this.Port,
				//ControlPort = this.ControlPort,
				PwfState = this.PwfState,
				VCIType = this.VCIType
			};
		}

		public VCIDevice(ClientContext clientContext)
        {
            this._clientContext = clientContext;
			this.vCITypeField = VCIDeviceType.UNKNOWN;
			this.vCIReservationField = VCIReservationType.NONE;
			this.portField = new int?(6801);
			this.colorField = "#73B2F5";
			this.forceReInitField = false;
			this.usePdmResultField = false;
			this.connectionLossRecognizedField = false;
			this.reconnectFailedField = false;
			this.underVoltageRecognizedField = false;
			this.communicationDisturbanceRecognizedField = false;
		}

		public string AccuCapacity
		{
			get
			{
				return this.accuCapacityField;
			}
			set
			{
				if (this.accuCapacityField != null)
				{
					if (!this.accuCapacityField.Equals(value))
					{
						this.accuCapacityField = value;
						this.OnPropertyChanged("AccuCapacity");
						return;
					}
				}
				else
				{
					this.accuCapacityField = value;
					this.OnPropertyChanged("AccuCapacity");
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

		public string Owner
		{
			get
			{
				return this.ownerField;
			}
			set
			{
				if (this.ownerField != null)
				{
					if (!this.ownerField.Equals(value))
					{
						this.ownerField = value;
						this.OnPropertyChanged("Owner");
						return;
					}
				}
				else
				{
					this.ownerField = value;
					this.OnPropertyChanged("Owner");
				}
			}
		}

		public string Description
		{
			get
			{
				return this.descriptionField;
			}
			set
			{
				if (this.descriptionField != null)
				{
					if (!this.descriptionField.Equals(value))
					{
						this.descriptionField = value;
						this.OnPropertyChanged("Description");
						return;
					}
				}
				else
				{
					this.descriptionField = value;
					this.OnPropertyChanged("Description");
				}
			}
		}

		public string State
		{
			get
			{
				return this.stateField;
			}
			set
			{
				if (this.stateField != null)
				{
					if (!this.stateField.Equals(value))
					{
						this.stateField = value;
						this.OnPropertyChanged("State");
						return;
					}
				}
				else
				{
					this.stateField = value;
					this.OnPropertyChanged("State");
				}
			}
		}

		public long leastSigBits
		{
			get
			{
				return this.leastSigBitsField;
			}
			set
			{
				if (!this.leastSigBitsField.Equals(value))
				{
					this.leastSigBitsField = value;
					this.OnPropertyChanged("leastSigBits");
				}
			}
		}

		public bool leastSigBitsSpecified1
		{
			get
			{
				return this.leastSigBitsSpecified1Field;
			}
			set
			{
				if (!this.leastSigBitsSpecified1Field.Equals(value))
				{
					this.leastSigBitsSpecified1Field = value;
					this.OnPropertyChanged("leastSigBitsSpecified1");
				}
			}
		}

		public long mostSigBits
		{
			get
			{
				return this.mostSigBitsField;
			}
			set
			{
				if (!this.mostSigBitsField.Equals(value))
				{
					this.mostSigBitsField = value;
					this.OnPropertyChanged("mostSigBits");
				}
			}
		}

		public bool mostSigBitsSpecified1
		{
			get
			{
				return this.mostSigBitsSpecified1Field;
			}
			set
			{
				if (!this.mostSigBitsSpecified1Field.Equals(value))
				{
					this.mostSigBitsSpecified1Field = value;
					this.OnPropertyChanged("mostSigBitsSpecified1");
				}
			}
		}

		public long ReserveHandle
		{
			get
			{
				return this.reserveHandleField;
			}
			set
			{
				if (!this.reserveHandleField.Equals(value))
				{
					this.reserveHandleField = value;
					this.OnPropertyChanged("ReserveHandle");
				}
			}
		}

		public DateTime ScanDate
		{
			get
			{
				return this.scanDateField;
			}
			set
			{
				if (!this.scanDateField.Equals(value))
				{
					this.scanDateField = value;
					this.OnPropertyChanged("ScanDate");
				}
			}
		}

		public string Service
		{
			get
			{
				return this.serviceField;
			}
			set
			{
				if (this.serviceField != null)
				{
					if (!this.serviceField.Equals(value))
					{
						this.serviceField = value;
						this.OnPropertyChanged("Service");
						return;
					}
				}
				else
				{
					this.serviceField = value;
					this.OnPropertyChanged("Service");
				}
			}
		}

		public string Kl15Trigger
		{
			get
			{
				return this.kl15TriggerField;
			}
			set
			{
				if (this.kl15TriggerField != null)
				{
					if (!this.kl15TriggerField.Equals(value))
					{
						this.kl15TriggerField = value;
						this.OnPropertyChanged("Kl15Trigger");
						return;
					}
				}
				else
				{
					this.kl15TriggerField = value;
					this.OnPropertyChanged("Kl15Trigger");
				}
			}
		}

		public string Kl30Trigger
		{
			get
			{
				return this.kl30TriggerField;
			}
			set
			{
				if (this.kl30TriggerField != null)
				{
					if (!this.kl30TriggerField.Equals(value))
					{
						this.kl30TriggerField = value;
						this.OnPropertyChanged("Kl30Trigger");
						return;
					}
				}
				else
				{
					this.kl30TriggerField = value;
					this.OnPropertyChanged("Kl30Trigger");
				}
			}
		}

		public string UUID
		{
			get
			{
				return this.uUIDField;
			}
			set
			{
				if (this.uUIDField != null)
				{
					if (!this.uUIDField.Equals(value))
					{
						this.uUIDField = value;
						this.OnPropertyChanged("UUID");
						return;
					}
				}
				else
				{
					this.uUIDField = value;
					this.OnPropertyChanged("UUID");
				}
			}
		}

		public string ImageVersionBoot
		{
			get
			{
				return this.imageVersionBootField;
			}
			set
			{
				if (this.imageVersionBootField != null)
				{
					if (!this.imageVersionBootField.Equals(value))
					{
						this.imageVersionBootField = value;
						this.OnPropertyChanged("ImageVersionBoot");
						return;
					}
				}
				else
				{
					this.imageVersionBootField = value;
					this.OnPropertyChanged("ImageVersionBoot");
				}
			}
		}

		public string ImageVersionApplication
		{
			get
			{
				return this.imageVersionApplicationField;
			}
			set
			{
				if (this.imageVersionApplicationField != null)
				{
					if (!this.imageVersionApplicationField.Equals(value))
					{
						this.imageVersionApplicationField = value;
						this.OnPropertyChanged("ImageVersionApplication");
						return;
					}
				}
				else
				{
					this.imageVersionApplicationField = value;
					this.OnPropertyChanged("ImageVersionApplication");
				}
			}
		}

		public string ImageVersionPackage
		{
			get
			{
				return this.imageVersionPackageField;
			}
			set
			{
				if (this.imageVersionPackageField != null)
				{
					if (!this.imageVersionPackageField.Equals(value))
					{
						this.imageVersionPackageField = value;
						this.OnPropertyChanged("ImageVersionPackage");
						return;
					}
				}
				else
				{
					this.imageVersionPackageField = value;
					this.OnPropertyChanged("ImageVersionPackage");
				}
			}
		}

		public string Counter
		{
			get
			{
				return this.counterField;
			}
			set
			{
				if (this.counterField != null)
				{
					if (!this.counterField.Equals(value))
					{
						this.counterField = value;
						this.OnPropertyChanged("Counter");
						return;
					}
				}
				else
				{
					this.counterField = value;
					this.OnPropertyChanged("Counter");
				}
			}
		}

		public VCIDeviceType VCIType
		{
			get
			{
				return this.vCITypeField;
			}
			set
			{
				if (!this.vCITypeField.Equals(value))
				{
					this.vCITypeField = value;
					this.OnPropertyChanged("VCIType");
				}
			}
		}

		public VCIReservationType VCIReservation
		{
			get
			{
				return this.vCIReservationField;
			}
			set
			{
				if (!this.vCIReservationField.Equals(value))
				{
					this.vCIReservationField = value;
					this.OnPropertyChanged("VCIReservation");
				}
			}
		}

		public string SignalStrength
		{
			get
			{
				return this.signalStrengthField;
			}
			set
			{
				if (this.signalStrengthField != null)
				{
					if (!this.signalStrengthField.Equals(value))
					{
						this.signalStrengthField = value;
						this.OnPropertyChanged("SignalStrength");
						return;
					}
				}
				else
				{
					this.signalStrengthField = value;
					this.OnPropertyChanged("SignalStrength");
				}
			}
		}

		public string Gateway
		{
			get
			{
				return this.gatewayField;
			}
			set
			{
				if (this.gatewayField != null)
				{
					if (!this.gatewayField.Equals(value))
					{
						this.gatewayField = value;
						this.OnPropertyChanged("Gateway");
						return;
					}
				}
				else
				{
					this.gatewayField = value;
					this.OnPropertyChanged("Gateway");
				}
			}
		}

		public string VciChannels
		{
			get
			{
				return this.vciChannelsField;
			}
			set
			{
				if (this.vciChannelsField != null)
				{
					if (!this.vciChannelsField.Equals(value))
					{
						this.vciChannelsField = value;
						this.OnPropertyChanged("VciChannels");
						return;
					}
				}
				else
				{
					this.vciChannelsField = value;
					this.OnPropertyChanged("VciChannels");
				}
			}
		}

		public string Netmask
		{
			get
			{
				return this.netmaskField;
			}
			set
			{
				if (this.netmaskField != null)
				{
					if (!this.netmaskField.Equals(value))
					{
						this.netmaskField = value;
						this.OnPropertyChanged("Netmask");
						return;
					}
				}
				else
				{
					this.netmaskField = value;
					this.OnPropertyChanged("Netmask");
				}
			}
		}

		public string NetworkType
		{
			get
			{
				return this.networkTypeField;
			}
			set
			{
				if (this.networkTypeField != null)
				{
					if (!this.networkTypeField.Equals(value))
					{
						this.networkTypeField = value;
						this.OnPropertyChanged("NetworkType");
						return;
					}
				}
				else
				{
					this.networkTypeField = value;
					this.OnPropertyChanged("NetworkType");
				}
			}
		}

		public string IPAddress
		{
			get
			{
				return this.iPAddressField;
			}
			set
			{
				if (this.iPAddressField != null)
				{
					if (!this.iPAddressField.Equals(value))
					{
						this.iPAddressField = value;
						this.OnPropertyChanged("IPAddress");
						return;
					}
				}
				else
				{
					this.iPAddressField = value;
					this.OnPropertyChanged("IPAddress");
				}
			}
		}

		public int? Port
		{
			get
			{
				return this.portField;
			}
			set
			{
				if (this.portField != null)
				{
					if (!this.portField.Equals(value))
					{
						this.portField = value;
						this.OnPropertyChanged("Port");
						return;
					}
				}
				else
				{
					this.portField = value;
					this.OnPropertyChanged("Port");
				}
			}
		}

		public int? ControlPort
		{
			get
			{
				return this.controlPortField;
			}
			set
			{
				if (this.controlPortField != null)
				{
					if (!this.controlPortField.Equals(value))
					{
						this.controlPortField = value;
						this.OnPropertyChanged("ControlPort");
						return;
					}
				}
				else
				{
					this.controlPortField = value;
					this.OnPropertyChanged("ControlPort");
				}
			}
		}

		public string DevId
		{
			get
			{
				return this.devIdField;
			}
			set
			{
				if (this.devIdField != null)
				{
					if (!this.devIdField.Equals(value))
					{
						this.devIdField = value;
						this.OnPropertyChanged("DevId");
						return;
					}
				}
				else
				{
					this.devIdField = value;
					this.OnPropertyChanged("DevId");
				}
			}
		}

		public string DevType
		{
			get
			{
				return this.devTypeField;
			}
			set
			{
				if (this.devTypeField != null)
				{
					if (!this.devTypeField.Equals(value))
					{
						this.devTypeField = value;
						this.OnPropertyChanged("DevType");
						return;
					}
				}
				else
				{
					this.devTypeField = value;
					this.OnPropertyChanged("DevType");
				}
			}
		}

		public string MacAddress
		{
			get
			{
				return this.macAddressField;
			}
			set
			{
				if (this.macAddressField != null)
				{
					if (!this.macAddressField.Equals(value))
					{
						this.macAddressField = value;
						this.OnPropertyChanged("MacAddress");
						return;
					}
				}
				else
				{
					this.macAddressField = value;
					this.OnPropertyChanged("MacAddress");
				}
			}
		}

		public string Description1
		{
			get
			{
				return this.description1Field;
			}
			set
			{
				if (this.description1Field != null)
				{
					if (!this.description1Field.Equals(value))
					{
						this.description1Field = value;
						this.OnPropertyChanged("Description1");
						return;
					}
				}
				else
				{
					this.description1Field = value;
					this.OnPropertyChanged("Description1");
				}
			}
		}

		public string Serial
		{
			get
			{
				return this.serialField;
			}
			set
			{
				if (this.serialField != null)
				{
					if (!this.serialField.Equals(value))
					{
						this.serialField = value;
						this.OnPropertyChanged("Serial");
						return;
					}
				}
				else
				{
					this.serialField = value;
					this.OnPropertyChanged("Serial");
				}
			}
		}

		public string VIN
		{
			get
			{
				return this.vINField;
			}
			set
			{
				if (this.vINField != null)
				{
					if (!this.vINField.Equals(value))
					{
						this.vINField = value;
						this.OnPropertyChanged("VIN");
						return;
					}
				}
				else
				{
					this.vINField = value;
					this.OnPropertyChanged("VIN");
				}
			}
		}

		public string Imagename
		{
			get
			{
				return this.imagenameField;
			}
			set
			{
				if (this.imagenameField != null)
				{
					if (!this.imagenameField.Equals(value))
					{
						this.imagenameField = value;
						this.OnPropertyChanged("Imagename");
						return;
					}
				}
				else
				{
					this.imagenameField = value;
					this.OnPropertyChanged("Imagename");
				}
			}
		}

		public string Color
		{
			get
			{
				return this.colorField;
			}
			set
			{
				if (this.colorField != null)
				{
					if (!this.colorField.Equals(value))
					{
						this.colorField = value;
						this.OnPropertyChanged("Color");
						return;
					}
				}
				else
				{
					this.colorField = value;
					this.OnPropertyChanged("Color");
				}
			}
		}

		public string IFHParameter
		{
			get
			{
				return this.iFHParameterField;
			}
			set
			{
				if (this.iFHParameterField != null)
				{
					if (!this.iFHParameterField.Equals(value))
					{
						this.iFHParameterField = value;
						this.OnPropertyChanged("IFHParameter");
						return;
					}
				}
				else
				{
					this.iFHParameterField = value;
					this.OnPropertyChanged("IFHParameter");
				}
			}
		}

		public string IFHReserved
		{
			get
			{
				return this.iFHReservedField;
			}
			set
			{
				if (this.iFHReservedField != null)
				{
					if (!this.iFHReservedField.Equals(value))
					{
						this.iFHReservedField = value;
						this.OnPropertyChanged("IFHReserved");
						return;
					}
				}
				else
				{
					this.iFHReservedField = value;
					this.OnPropertyChanged("IFHReserved");
				}
			}
		}

		public bool ForceReInit
		{
			get
			{
				return this.forceReInitField;
			}
			set
			{
				if (!this.forceReInitField.Equals(value))
				{
					this.forceReInitField = value;
					this.OnPropertyChanged("ForceReInit");
				}
			}
		}

		public bool UsePdmResult
		{
			get
			{
				return this.usePdmResultField;
			}
			set
			{
				if (!this.usePdmResultField.Equals(value))
				{
					this.usePdmResultField = value;
					this.OnPropertyChanged("UsePdmResult");
				}
			}
		}

		public string PwfState
		{
			get
			{
				return this.pwfStateField;
			}
			set
			{
				if (this.pwfStateField != null)
				{
					if (!this.pwfStateField.Equals(value))
					{
						this.pwfStateField = value;
						this.OnPropertyChanged("PwfState");
						return;
					}
				}
				else
				{
					this.pwfStateField = value;
					this.OnPropertyChanged("PwfState");
				}
			}
		}

		[DefaultValue(false)]
		public bool ConnectionLossRecognized
		{
			get
			{
				return this.connectionLossRecognizedField;
			}
			set
			{
				if (!this.connectionLossRecognizedField.Equals(value))
				{
					this.connectionLossRecognizedField = value;
					this.OnPropertyChanged("ConnectionLossRecognized");
				}
			}
		}

		[DefaultValue(false)]
		public bool ReconnectFailed
		{
			get
			{
				return this.reconnectFailedField;
			}
			set
			{
				if (!this.reconnectFailedField.Equals(value))
				{
					this.reconnectFailedField = value;
					this.OnPropertyChanged("ReconnectFailed");
				}
			}
		}

		[DefaultValue(false)]
		public bool UnderVoltageRecognized
		{
			get
			{
				return this.underVoltageRecognizedField;
			}
			set
			{
				if (!this.underVoltageRecognizedField.Equals(value))
				{
					this.underVoltageRecognizedField = value;
					this.OnPropertyChanged("UnderVoltageRecognized");
				}
			}
		}

		public DateTime UnderVoltageRecognizedLastTime
		{
			get
			{
				return this.underVoltageRecognizedLastTimeField;
			}
			set
			{
				if (!this.underVoltageRecognizedLastTimeField.Equals(value))
				{
					this.underVoltageRecognizedLastTimeField = value;
					this.OnPropertyChanged("UnderVoltageRecognizedLastTime");
				}
			}
		}

		[XmlIgnore]
		public bool UnderVoltageRecognizedLastTimeSpecified
		{
			get
			{
				return this.underVoltageRecognizedLastTimeFieldSpecified;
			}
			set
			{
				if (!this.underVoltageRecognizedLastTimeFieldSpecified.Equals(value))
				{
					this.underVoltageRecognizedLastTimeFieldSpecified = value;
					this.OnPropertyChanged("UnderVoltageRecognizedLastTimeSpecified");
				}
			}
		}

		[DefaultValue(false)]
		public bool CommunicationDisturbanceRecognized
		{
			get
			{
				return this.communicationDisturbanceRecognizedField;
			}
			set
			{
				if (!this.communicationDisturbanceRecognizedField.Equals(value))
				{
					this.communicationDisturbanceRecognizedField = value;
					this.OnPropertyChanged("CommunicationDisturbanceRecognized");
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
    }
}
