using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    public class VCIDevice : IComparable, IVciDevice, INotifyPropertyChanged, IVciDeviceRuleEvaluation, ICloneable
    {
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
        [XmlIgnore]
        public DeviceTypeDetails DeviceTypeDetail { get; set; }

        [XmlIgnore]
        public bool IsDead => DateTime.Now > ScanDate.AddSeconds(15.0);

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
                if (VCIType != VCIDeviceType.UNKNOWN)
                {
                    if (string.IsNullOrEmpty(IPAddress))
                    {
                        return !string.IsNullOrEmpty(DevId);
                    }

                    return true;
                }

                return false;
            }
        }

        [XmlIgnore]
        public DeviceState DeviceState
        {
            get
            {
                DeviceState result = DeviceState.Unregistered;
                if (int.TryParse(State, out var result2))
                {
                    result = (DeviceState)result2;
                }

                return result;
            }

            set
            {
                int num = (int)value;
                State = num.ToString(CultureInfo.InvariantCulture);
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public BasicFeaturesVci BasicFeatures
        {
            get
            {
                if (!string.IsNullOrEmpty(VIN))
                {
                    LoadCharacteristicsFromDatabase();
                    return basicFeaturesVci;
                }

                return null;
            }
        }

        public bool IsMarkedToDefault
        {
            get
            {
                return isMarkedToDefault;
            }

            set
            {
                isMarkedToDefault = value;
                OnPropertyChanged("IsMarkedToDefault");
            }
        }

        public bool IsImibR2
        {
            get
            {
                bool flag = VCIType == VCIDeviceType.IMIB && "IMIB_R2".Equals(Description1);
                if (!flag && !"IMIB_R1".Equals(Description1) && long.TryParse(ImageVersionApplication, out var result))
                {
                    flag = VCIType == VCIDeviceType.IMIB && (result >= 20000 || result == 400);
                }

                return flag;
            }
        }

        public bool IsImibNext
        {
            get
            {
                if (VCIType == VCIDeviceType.IMIB)
                {
                    return "IMIB_NX".Equals(Description1);
                }

                return false;
            }
        }

        [XmlIgnore]
        public bool IsConnected { get; set; }

        public bool IsConnectable
        {
            get
            {
                try
                {
                    bool result2;
                    if (int.TryParse(State, out var result))
                    {
                        result2 = connectableStates.Contains((DeviceState)result);
                    }
                    else
                    {
                        Log.Error("IsConnectable", "State '{0}' is not to int parsable.", State);
                        result2 = false;
                    }

                    return result2;
                }
                catch (Exception)
                {
                    Log.Error("IsConnectable", "State '{0}' is not to int parsable.", State);
                    return false;
                }
            }
        }

        [XmlIgnore]
        public string ReceivingIP
        {
            get
            {
                return receivingIP;
            }

            set
            {
                if (value != receivingIP)
                {
                    receivingIP = value;
                    OnPropertyChanged("ReceivingIP");
                }
            }
        }

        [XmlIgnore]
        public string NetworkTypeLabel
        {
            get
            {
                if (string.IsNullOrEmpty(NetworkType))
                {
                    return "-";
                }

                if ("1".Equals(NetworkType))
                {
                    return "WLAN";
                }

                if ("0".Equals(NetworkType))
                {
                    return "LAN";
                }

                if ("2".Equals(NetworkType))
                {
                    LocalAdapterNetworkType = BMW.Rheingold.CoreFramework.Contracts.Vehicle.NetworkType.directLAN;
                    return "directLAN";
                }

                return "UNKNOWN";
            }
        }

        [XmlIgnore]
        IBasicFeatures IVciDevice.BasicFeatures => BasicFeatures;

        [XmlIgnore]
        public ushort PowerSupply { get; set; }
        public NetworkType LocalAdapterNetworkType { get; set; }

        public bool IsVehicleProgrammingPossible
        {
            get
            {
                if (NetworkType == "0" || NetworkType == "2")
                {
                    if (LocalAdapterNetworkType != BMW.Rheingold.CoreFramework.Contracts.Vehicle.NetworkType.LAN && LocalAdapterNetworkType != BMW.Rheingold.CoreFramework.Contracts.Vehicle.NetworkType.directLAN)
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        public string AccuCapacity
        {
            get
            {
                return accuCapacityField;
            }

            set
            {
                if (accuCapacityField != null)
                {
                    if (!accuCapacityField.Equals(value))
                    {
                        accuCapacityField = value;
                        OnPropertyChanged("AccuCapacity");
                    }
                }
                else
                {
                    accuCapacityField = value;
                    OnPropertyChanged("AccuCapacity");
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

        public string Owner
        {
            get
            {
                return ownerField;
            }

            set
            {
                if (ownerField != null)
                {
                    if (!ownerField.Equals(value))
                    {
                        ownerField = value;
                        OnPropertyChanged("Owner");
                    }
                }
                else
                {
                    ownerField = value;
                    OnPropertyChanged("Owner");
                }
            }
        }

        public string Description
        {
            get
            {
                return descriptionField;
            }

            set
            {
                if (descriptionField != null)
                {
                    if (!descriptionField.Equals(value))
                    {
                        descriptionField = value;
                        OnPropertyChanged("Description");
                    }
                }
                else
                {
                    descriptionField = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public string State
        {
            get
            {
                return stateField;
            }

            set
            {
                if (stateField != null)
                {
                    if (!stateField.Equals(value))
                    {
                        stateField = value;
                        OnPropertyChanged("State");
                    }
                }
                else
                {
                    stateField = value;
                    OnPropertyChanged("State");
                }
            }
        }

        public long leastSigBits
        {
            get
            {
                return leastSigBitsField;
            }

            set
            {
                if (!leastSigBitsField.Equals(value))
                {
                    leastSigBitsField = value;
                    OnPropertyChanged("leastSigBits");
                }
            }
        }

        public bool leastSigBitsSpecified1
        {
            get
            {
                return leastSigBitsSpecified1Field;
            }

            set
            {
                if (!leastSigBitsSpecified1Field.Equals(value))
                {
                    leastSigBitsSpecified1Field = value;
                    OnPropertyChanged("leastSigBitsSpecified1");
                }
            }
        }

        public long mostSigBits
        {
            get
            {
                return mostSigBitsField;
            }

            set
            {
                if (!mostSigBitsField.Equals(value))
                {
                    mostSigBitsField = value;
                    OnPropertyChanged("mostSigBits");
                }
            }
        }

        public bool mostSigBitsSpecified1
        {
            get
            {
                return mostSigBitsSpecified1Field;
            }

            set
            {
                if (!mostSigBitsSpecified1Field.Equals(value))
                {
                    mostSigBitsSpecified1Field = value;
                    OnPropertyChanged("mostSigBitsSpecified1");
                }
            }
        }

        public long ReserveHandle
        {
            get
            {
                return reserveHandleField;
            }

            set
            {
                if (!reserveHandleField.Equals(value))
                {
                    reserveHandleField = value;
                    OnPropertyChanged("ReserveHandle");
                }
            }
        }

        public DateTime ScanDate
        {
            get
            {
                return scanDateField;
            }

            set
            {
                if (!scanDateField.Equals(value))
                {
                    scanDateField = value;
                    OnPropertyChanged("ScanDate");
                }
            }
        }

        public string Service
        {
            get
            {
                return serviceField;
            }

            set
            {
                if (serviceField != null)
                {
                    if (!serviceField.Equals(value))
                    {
                        serviceField = value;
                        OnPropertyChanged("Service");
                    }
                }
                else
                {
                    serviceField = value;
                    OnPropertyChanged("Service");
                }
            }
        }

        public string Kl15Trigger
        {
            get
            {
                return kl15TriggerField;
            }

            set
            {
                if (kl15TriggerField != null)
                {
                    if (!kl15TriggerField.Equals(value))
                    {
                        kl15TriggerField = value;
                        OnPropertyChanged("Kl15Trigger");
                    }
                }
                else
                {
                    kl15TriggerField = value;
                    OnPropertyChanged("Kl15Trigger");
                }
            }
        }

        public string Kl30Trigger
        {
            get
            {
                return kl30TriggerField;
            }

            set
            {
                if (kl30TriggerField != null)
                {
                    if (!kl30TriggerField.Equals(value))
                    {
                        kl30TriggerField = value;
                        OnPropertyChanged("Kl30Trigger");
                    }
                }
                else
                {
                    kl30TriggerField = value;
                    OnPropertyChanged("Kl30Trigger");
                }
            }
        }

        public string UUID
        {
            get
            {
                return uUIDField;
            }

            set
            {
                if (uUIDField != null)
                {
                    if (!uUIDField.Equals(value))
                    {
                        uUIDField = value;
                        OnPropertyChanged("UUID");
                    }
                }
                else
                {
                    uUIDField = value;
                    OnPropertyChanged("UUID");
                }
            }
        }

        public string ImageVersionBoot
        {
            get
            {
                return imageVersionBootField;
            }

            set
            {
                if (imageVersionBootField != null)
                {
                    if (!imageVersionBootField.Equals(value))
                    {
                        imageVersionBootField = value;
                        OnPropertyChanged("ImageVersionBoot");
                    }
                }
                else
                {
                    imageVersionBootField = value;
                    OnPropertyChanged("ImageVersionBoot");
                }
            }
        }

        public string ImageVersionApplication
        {
            get
            {
                return imageVersionApplicationField;
            }

            set
            {
                if (imageVersionApplicationField != null)
                {
                    if (!imageVersionApplicationField.Equals(value))
                    {
                        imageVersionApplicationField = value;
                        OnPropertyChanged("ImageVersionApplication");
                    }
                }
                else
                {
                    imageVersionApplicationField = value;
                    OnPropertyChanged("ImageVersionApplication");
                }
            }
        }

        public string ImageVersionPackage
        {
            get
            {
                return imageVersionPackageField;
            }

            set
            {
                if (imageVersionPackageField != null)
                {
                    if (!imageVersionPackageField.Equals(value))
                    {
                        imageVersionPackageField = value;
                        OnPropertyChanged("ImageVersionPackage");
                    }
                }
                else
                {
                    imageVersionPackageField = value;
                    OnPropertyChanged("ImageVersionPackage");
                }
            }
        }

        public string Counter
        {
            get
            {
                return counterField;
            }

            set
            {
                if (counterField != null)
                {
                    if (!counterField.Equals(value))
                    {
                        counterField = value;
                        OnPropertyChanged("Counter");
                    }
                }
                else
                {
                    counterField = value;
                    OnPropertyChanged("Counter");
                }
            }
        }

        public VCIDeviceType VCIType
        {
            get
            {
                return vCITypeField;
            }

            set
            {
                if (!vCITypeField.Equals(value))
                {
                    vCITypeField = value;
                    OnPropertyChanged("VCIType");
                }
            }
        }

        public VCIReservationType VCIReservation
        {
            get
            {
                return vCIReservationField;
            }

            set
            {
                if (!vCIReservationField.Equals(value))
                {
                    vCIReservationField = value;
                    OnPropertyChanged("VCIReservation");
                }
            }
        }

        public string SignalStrength
        {
            get
            {
                return signalStrengthField;
            }

            set
            {
                if (signalStrengthField != null)
                {
                    if (!signalStrengthField.Equals(value))
                    {
                        signalStrengthField = value;
                        OnPropertyChanged("SignalStrength");
                    }
                }
                else
                {
                    signalStrengthField = value;
                    OnPropertyChanged("SignalStrength");
                }
            }
        }

        public string Gateway
        {
            get
            {
                return gatewayField;
            }

            set
            {
                if (gatewayField != null)
                {
                    if (!gatewayField.Equals(value))
                    {
                        gatewayField = value;
                        OnPropertyChanged("Gateway");
                    }
                }
                else
                {
                    gatewayField = value;
                    OnPropertyChanged("Gateway");
                }
            }
        }

        public string VciChannels
        {
            get
            {
                return vciChannelsField;
            }

            set
            {
                if (vciChannelsField != null)
                {
                    if (!vciChannelsField.Equals(value))
                    {
                        vciChannelsField = value;
                        OnPropertyChanged("VciChannels");
                    }
                }
                else
                {
                    vciChannelsField = value;
                    OnPropertyChanged("VciChannels");
                }
            }
        }

        public string Netmask
        {
            get
            {
                return netmaskField;
            }

            set
            {
                if (netmaskField != null)
                {
                    if (!netmaskField.Equals(value))
                    {
                        netmaskField = value;
                        OnPropertyChanged("Netmask");
                    }
                }
                else
                {
                    netmaskField = value;
                    OnPropertyChanged("Netmask");
                }
            }
        }

        public string NetworkType
        {
            get
            {
                return networkTypeField;
            }

            set
            {
                if (networkTypeField != null)
                {
                    if (!networkTypeField.Equals(value))
                    {
                        networkTypeField = value;
                        OnPropertyChanged("NetworkType");
                    }
                }
                else
                {
                    networkTypeField = value;
                    OnPropertyChanged("NetworkType");
                }
            }
        }

        public string IPAddress
        {
            get
            {
                return iPAddressField;
            }

            set
            {
                if (iPAddressField != null)
                {
                    if (!iPAddressField.Equals(value))
                    {
                        iPAddressField = value;
                        OnPropertyChanged("IPAddress");
                    }
                }
                else
                {
                    iPAddressField = value;
                    OnPropertyChanged("IPAddress");
                }
            }
        }

        public int? Port
        {
            get
            {
                return portField;
            }

            set
            {
                if (portField.HasValue)
                {
                    if (!portField.Equals(value))
                    {
                        portField = value;
                        OnPropertyChanged("Port");
                    }
                }
                else
                {
                    portField = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        public int? ControlPort
        {
            get
            {
                return controlPortField;
            }

            set
            {
                if (controlPortField.HasValue)
                {
                    if (!controlPortField.Equals(value))
                    {
                        controlPortField = value;
                        OnPropertyChanged("ControlPort");
                    }
                }
                else
                {
                    controlPortField = value;
                    OnPropertyChanged("ControlPort");
                }
            }
        }

        public string DevId
        {
            get
            {
                return devIdField;
            }

            set
            {
                if (devIdField != null)
                {
                    if (!devIdField.Equals(value))
                    {
                        devIdField = value;
                        OnPropertyChanged("DevId");
                    }
                }
                else
                {
                    devIdField = value;
                    OnPropertyChanged("DevId");
                }
            }
        }

        public string DevType
        {
            get
            {
                return devTypeField;
            }

            set
            {
                if (devTypeField != null)
                {
                    if (!devTypeField.Equals(value))
                    {
                        devTypeField = value;
                        OnPropertyChanged("DevType");
                    }
                }
                else
                {
                    devTypeField = value;
                    OnPropertyChanged("DevType");
                }
            }
        }

        public string DevTypeExt
        {
            get
            {
                return devTypeExtField;
            }

            set
            {
                if (devTypeExtField != value)
                {
                    devTypeExtField = value;
                    OnPropertyChanged("DevTypeExt");
                }
            }
        }

        public string MacAddress
        {
            get
            {
                return macAddressField;
            }

            set
            {
                if (macAddressField != null)
                {
                    if (!macAddressField.Equals(value))
                    {
                        macAddressField = value;
                        OnPropertyChanged("MacAddress");
                    }
                }
                else
                {
                    macAddressField = value;
                    OnPropertyChanged("MacAddress");
                }
            }
        }

        public string WLANMacAddress
        {
            get
            {
                return wlanMacAddressField;
            }

            set
            {
                if (wlanMacAddressField != value)
                {
                    wlanMacAddressField = value;
                    OnPropertyChanged("WLANMacAddress");
                }
            }
        }

        public string Description1
        {
            get
            {
                return description1Field;
            }

            set
            {
                if (description1Field != null)
                {
                    if (!description1Field.Equals(value))
                    {
                        description1Field = value;
                        OnPropertyChanged("Description1");
                    }
                }
                else
                {
                    description1Field = value;
                    OnPropertyChanged("Description1");
                }
            }
        }

        public string Serial
        {
            get
            {
                return serialField;
            }

            set
            {
                if (serialField != null)
                {
                    if (!serialField.Equals(value))
                    {
                        serialField = value;
                        OnPropertyChanged("Serial");
                    }
                }
                else
                {
                    serialField = value;
                    OnPropertyChanged("Serial");
                }
            }
        }

        public string VIN
        {
            get
            {
                return vINField;
            }

            set
            {
                if (vINField != null)
                {
                    if (!vINField.Equals(value))
                    {
                        vINField = value;
                        OnPropertyChanged("VIN");
                    }
                }
                else
                {
                    vINField = value;
                    OnPropertyChanged("VIN");
                }
            }
        }

        public string Imagename
        {
            get
            {
                return imagenameField;
            }

            set
            {
                if (imagenameField != null)
                {
                    if (!imagenameField.Equals(value))
                    {
                        imagenameField = value;
                        OnPropertyChanged("Imagename");
                    }
                }
                else
                {
                    imagenameField = value;
                    OnPropertyChanged("Imagename");
                }
            }
        }

        public string Color
        {
            get
            {
                return colorField;
            }

            set
            {
                if (colorField != null)
                {
                    if (!colorField.Equals(value))
                    {
                        colorField = value;
                        OnPropertyChanged("Color");
                    }
                }
                else
                {
                    colorField = value;
                    OnPropertyChanged("Color");
                }
            }
        }

        public string IFHParameter
        {
            get
            {
                return iFHParameterField;
            }

            set
            {
                if (iFHParameterField != null)
                {
                    if (!iFHParameterField.Equals(value))
                    {
                        iFHParameterField = value;
                        OnPropertyChanged("IFHParameter");
                    }
                }
                else
                {
                    iFHParameterField = value;
                    OnPropertyChanged("IFHParameter");
                }
            }
        }

        public string IFHReserved
        {
            get
            {
                return iFHReservedField;
            }

            set
            {
                if (iFHReservedField != null)
                {
                    if (!iFHReservedField.Equals(value))
                    {
                        iFHReservedField = value;
                        OnPropertyChanged("IFHReserved");
                    }
                }
                else
                {
                    iFHReservedField = value;
                    OnPropertyChanged("IFHReserved");
                }
            }
        }

        public bool ForceReInit
        {
            get
            {
                return forceReInitField;
            }

            set
            {
                if (!forceReInitField.Equals(value))
                {
                    forceReInitField = value;
                    OnPropertyChanged("ForceReInit");
                }
            }
        }

        public bool UsePdmResult
        {
            get
            {
                return usePdmResultField;
            }

            set
            {
                if (!usePdmResultField.Equals(value))
                {
                    usePdmResultField = value;
                    OnPropertyChanged("UsePdmResult");
                }
            }
        }

        public string PwfState
        {
            get
            {
                return pwfStateField;
            }

            set
            {
                if (pwfStateField != null)
                {
                    if (!pwfStateField.Equals(value))
                    {
                        pwfStateField = value;
                        OnPropertyChanged("PwfState");
                    }
                }
                else
                {
                    pwfStateField = value;
                    OnPropertyChanged("PwfState");
                }
            }
        }

        [DefaultValue(false)]
        public bool ConnectionLossRecognized
        {
            get
            {
                return connectionLossRecognizedField;
            }

            set
            {
                if (!connectionLossRecognizedField.Equals(value))
                {
                    connectionLossRecognizedField = value;
                    OnPropertyChanged("ConnectionLossRecognized");
                }
            }
        }

        [DefaultValue(false)]
        public bool ReconnectFailed
        {
            get
            {
                return reconnectFailedField;
            }

            set
            {
                if (!reconnectFailedField.Equals(value))
                {
                    reconnectFailedField = value;
                    OnPropertyChanged("ReconnectFailed");
                }
            }
        }

        [DefaultValue(false)]
        public bool UnderVoltageRecognized
        {
            get
            {
                return underVoltageRecognizedField;
            }

            set
            {
                if (!underVoltageRecognizedField.Equals(value))
                {
                    underVoltageRecognizedField = value;
                    OnPropertyChanged("UnderVoltageRecognized");
                }
            }
        }

        public DateTime UnderVoltageRecognizedLastTime
        {
            get
            {
                return underVoltageRecognizedLastTimeField;
            }

            set
            {
                if (!underVoltageRecognizedLastTimeField.Equals(value))
                {
                    underVoltageRecognizedLastTimeField = value;
                    OnPropertyChanged("UnderVoltageRecognizedLastTime");
                }
            }
        }

        [XmlIgnore]
        public bool UnderVoltageRecognizedLastTimeSpecified
        {
            get
            {
                return underVoltageRecognizedLastTimeFieldSpecified;
            }

            set
            {
                if (!underVoltageRecognizedLastTimeFieldSpecified.Equals(value))
                {
                    underVoltageRecognizedLastTimeFieldSpecified = value;
                    OnPropertyChanged("UnderVoltageRecognizedLastTimeSpecified");
                }
            }
        }

        [DefaultValue(false)]
        public bool CommunicationDisturbanceRecognized
        {
            get
            {
                return communicationDisturbanceRecognizedField;
            }

            set
            {
                if (!communicationDisturbanceRecognizedField.Equals(value))
                {
                    communicationDisturbanceRecognizedField = value;
                    OnPropertyChanged("CommunicationDisturbanceRecognized");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [PreserveSource(Hint = "Unchanged", SignatureModified = true)]
        public VCIDevice(VCIDeviceType vciType, string devid, string description)
        {
            ScanDate = DateTime.Now;
            DevId = devid;
            Description = description;
            Imagename = null;
            Color = "#73B2F5";
            VCIType = vciType;
        }

        private void LoadCharacteristicsFromDatabase()
        {
            if (basicFeaturesVci != null)
            {
                return;
            }

            //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
            //[-] if (VIN.Contains("XXXX") || VIN.Length != 17 || instance == null)
            //[+] if (VIN.Contains("XXXX") || VIN.Length != 17 || _clientContext == null)
            if (VIN.Contains("XXXX") || VIN.Length != 17 || _clientContext == null)
            {
                return;
            }

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
                case 'J':
                    text += "9";
                    break;
                default:
                    text += VIN[6];
                    break;
            }

            //[-] IList<IXepCharacteristics> vehicleIdentByTypeKey = instance.GetVehicleIdentByTypeKey(text);
            //[+] List<PsdzDatabase.Characteristics> vehicleIdentByTypeKey = _clientContext?.Database?.GetVehicleIdentByTypeKey(text, false);
            List<PsdzDatabase.Characteristics> vehicleIdentByTypeKey = _clientContext?.Database?.GetVehicleIdentByTypeKey(text, false);
            if (vehicleIdentByTypeKey == null)
            {
                return;
            }

            BasicFeaturesVci vehicle = new BasicFeaturesVci();
            //[-] VehicleCharacteristicVCIDeviceHelper vehicleCharacteristicVCIDeviceHelper = new VehicleCharacteristicVCIDeviceHelper();
            //[-] foreach (IXepCharacteristics item in vehicleIdentByTypeKey)
            //[+] VehicleCharacteristicVCIDeviceHelper vehicleCharacteristicVCIDeviceHelper = new VehicleCharacteristicVCIDeviceHelper(_clientContext);
            VehicleCharacteristicVCIDeviceHelper vehicleCharacteristicVCIDeviceHelper = new VehicleCharacteristicVCIDeviceHelper(_clientContext);
            //[+] foreach (PsdzDatabase.Characteristics item in vehicleIdentByTypeKey)
            foreach (PsdzDatabase.Characteristics item in vehicleIdentByTypeKey)
            {
                vehicleCharacteristicVCIDeviceHelper.AssignBasicFeaturesVciCharacteristic(item.RootNodeClass.ToString(), vehicle, item);
            }

            basicFeaturesVci = vehicle;
        }

        public bool IsSupportedImibOrICOM(string[] acceptedImibDevices)
        {
            if (!(DevType != "IMIB"))
            {
                return acceptedImibDevices.Contains(Description1);
            }

            return true;
        }

        public bool CheckChannel(string channelId)
        {
            if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(VciChannels))
            {
                return false;
            }

            string[] array = VciChannels.Split(';');
            foreach (string text in array)
            {
                if (text.Contains(channelId))
                {
                    if (!text.Contains("+"))
                    {
                        return text.Contains("*");
                    }

                    return true;
                }
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
            return ToAttrList(addLineFeed: false);
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
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(DevId={0}),{1}", DevId, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Service={0}),{1}", Service, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Serial={0}),{1}", Serial, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(MacAddress={0}),{1}", MacAddress, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(WLANMacAddress={0}),{1}", WLANMacAddress, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(DevType={0}),{1}", (DeviceTypeDetail == DeviceTypeDetails.Unspecified) ? DevType : DeviceTypeDetail.ToString(), arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ImageVersionBoot={0}),{1}", ImageVersionBoot, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ImageVersionApplication={0}),{1}", ImageVersionApplication, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ImageVersionPackage={0}),{1}", ImageVersionPackage, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Color={0}),{1}", Color, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Counter={0}),{1}", Counter, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(State={0}),{1}", State, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Owner={0}),{1}", Owner, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Kl15Voltage={0}),{1}", Kl15Voltage, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Kl30Voltage={0}),{1}", Kl30Voltage, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(SignalStrength={0}),{1}", SignalStrength, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(VIN={0}),{1}", VIN, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Gateway={0}),{1}", Gateway, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(AccuCapacity={0}),{1}", AccuCapacity, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(PowerSupply={0}),{1}", PowerSupply, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(VciChannels={0}),{1}", VciChannels, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Netmask={0}),{1}", Netmask, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(NetworkType={0}),{1}", NetworkType, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(UUID={0}),{1}", UUID, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(Port={0}),{1}", Port, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ControlPort={0}),{1}", ControlPort, arg);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(PwfState={0})", PwfState);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(DoIP={0})", IsDoIP);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(DevTypeExt={0})", DevTypeExt);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(IsSimulation={0})", IsSimulation);
            }
            catch (Exception exception)
            {
                Log.WarningException("VCIDevice.ToAttrList()", exception);
            }

            return stringBuilder.ToString();
        }

        public double? GetClamp30()
        {
            return GetVoltageForString(Kl30Voltage);
        }

        public double? GetClamp15()
        {
            return GetVoltageForString(Kl15Voltage);
        }

        private double? GetVoltageForString(string voltage)
        {
            bool flag = Regex.IsMatch(voltage, "\\d+([,.]\\d+)? *(mV||MV|mv|Mv)?");
            if (!string.IsNullOrEmpty(voltage) && flag)
            {
                try
                {
                    return Convert.ToDouble(new Regex("(mV|MV|mv|Mv)").Replace(voltage, string.Empty).Trim());
                }
                catch (Exception exception)
                {
                    Log.WarningException("VCIDevice.GetVoltageForString()", exception);
                    Log.Warning("VCIDevice.GetVoltageForString()", "The voltage: {0} is not valid!", voltage);
                }
            }

            return null;
        }

        public int CompareTo(object obj)
        {
            if (obj is VCIDevice vCIDevice)
            {
                if (string.IsNullOrEmpty(vCIDevice.DevId))
                {
                    return 1;
                }

                int num = DevId.Length.CompareTo(vCIDevice.DevId.Length);
                if (num != 0)
                {
                    return num;
                }

                return string.Compare(DevId, vCIDevice.DevId, StringComparison.Ordinal);
            }

            return 1;
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
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", num, num2, num3, num4);
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
            if (!string.IsNullOrEmpty(Serial))
            {
                return Serial.GetHashCode();
            }

            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            VCIDevice vCIDevice = obj as VCIDevice;
            if (!string.IsNullOrEmpty(Serial) && vCIDevice != null)
            {
                return Serial.Equals(vCIDevice.Serial);
            }

            return base.Equals(obj);
        }

        public void SetAlive()
        {
            ScanDate = DateTime.Now;
        }

        public override string ToString()
        {
            return "VCIDevice: " + ToAttrList(addLineFeed: true);
        }

        public object Clone()
        {
            //[-] VCIDevice vCIDevice = new VCIDevice();
            //[+] VCIDevice vCIDevice = new VCIDevice(_clientContext);
            VCIDevice vCIDevice = new VCIDevice(_clientContext);
            vCIDevice.CommunicationDisturbanceRecognized = CommunicationDisturbanceRecognized;
            vCIDevice.ConnectionLossRecognized = ConnectionLossRecognized;
            vCIDevice.ControlPort = ControlPort;
            vCIDevice.Counter = Counter;
            vCIDevice.Description = Description;
            vCIDevice.Description1 = Description1;
            vCIDevice.ForceReInit = ForceReInit;
            vCIDevice.IFHParameter = IFHParameter;
            vCIDevice.IFHReserved = IFHReserved;
            vCIDevice.DevId = DevId;
            vCIDevice.Service = Service;
            vCIDevice.Serial = Serial;
            vCIDevice.MacAddress = MacAddress;
            vCIDevice.WLANMacAddress = WLANMacAddress;
            vCIDevice.IPAddress = IPAddress;
            vCIDevice.DevType = DevType;
            vCIDevice.DevTypeExt = DevTypeExt;
            vCIDevice.ImageVersionBoot = ImageVersionBoot;
            vCIDevice.ImageVersionApplication = ImageVersionApplication;
            vCIDevice.ImageVersionPackage = ImageVersionPackage;
            vCIDevice.Imagename = Imagename;
            vCIDevice.IsConnected = IsConnected;
            vCIDevice.Kl15Trigger = Kl15Trigger;
            vCIDevice.Kl15Voltage = Kl15Voltage;
            vCIDevice.Kl30Trigger = Kl30Trigger;
            vCIDevice.Kl30Voltage = Kl30Voltage;
            vCIDevice.NetworkType = NetworkType;
            vCIDevice.VIN = VIN;
            vCIDevice.mostSigBits = mostSigBits;
            vCIDevice.mostSigBitsSpecified1 = mostSigBitsSpecified1;
            vCIDevice.PwfState = PwfState;
            vCIDevice.ReceivingIP = ReceivingIP;
            vCIDevice.ReconnectFailed = ReconnectFailed;
            vCIDevice.ReserveHandle = ReserveHandle;
            vCIDevice.CommunicationDisturbanceRecognized = CommunicationDisturbanceRecognized;
            vCIDevice.ConnectionLossRecognized = ConnectionLossRecognized;
            vCIDevice.ScanDate = ScanDate;
            vCIDevice.Service = Service;
            vCIDevice.UnderVoltageRecognized = UnderVoltageRecognized;
            vCIDevice.UnderVoltageRecognizedLastTime = UnderVoltageRecognizedLastTime;
            vCIDevice.UnderVoltageRecognizedLastTimeSpecified = UnderVoltageRecognizedLastTimeSpecified;
            vCIDevice.VCIReservation = VCIReservation;
            vCIDevice.Color = Color;
            vCIDevice.Counter = Counter;
            vCIDevice.State = State;
            vCIDevice.Owner = Owner;
            vCIDevice.Kl15Voltage = Kl15Voltage;
            vCIDevice.Kl30Voltage = Kl30Voltage;
            vCIDevice.SignalStrength = SignalStrength;
            vCIDevice.VIN = VIN;
            vCIDevice.Gateway = Gateway;
            vCIDevice.AccuCapacity = AccuCapacity;
            vCIDevice.PowerSupply = PowerSupply;
            vCIDevice.VciChannels = VciChannels;
            vCIDevice.Netmask = Netmask;
            vCIDevice.NetworkType = NetworkType;
            vCIDevice.UUID = UUID;
            vCIDevice.Port = Port;
            vCIDevice.ControlPort = ControlPort;
            vCIDevice.PwfState = PwfState;
            vCIDevice.VCIType = VCIType;
            vCIDevice.IsDoIP = IsDoIP;
            vCIDevice.IsSimulation = IsSimulation;
            return vCIDevice;
        }

        [PreserveSource(Hint = "clientContext added", SignatureModified = true)]
        public VCIDevice(ClientContext clientContext)
        {
            //[+] _clientContext = clientContext;
            _clientContext = clientContext;
            vCITypeField = VCIDeviceType.UNKNOWN;
            vCIReservationField = VCIReservationType.NONE;
            portField = 6801;
            colorField = "#73B2F5";
            forceReInitField = false;
            usePdmResultField = false;
            connectionLossRecognizedField = false;
            reconnectFailedField = false;
            underVoltageRecognizedField = false;
            communicationDisturbanceRecognizedField = false;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [PreserveSource(Added = true)]
        private ClientContext _clientContext;
    }
}