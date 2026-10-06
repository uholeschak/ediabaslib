using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.Interaction.Responses;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.Localization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionConnectionManagerModel : InteractionRequestModel<InteractionConnectionManagerResponse>
    {
        private readonly ILogic logic;
        [DataMember]
        private readonly bool shouldLogin;
        [DataMember]
        private bool doImibReservation;
        [DataMember]
        private string reservationIcomType;
        [DataMember]
        private IVciDevice connectedVci;
        [DataMember]
        private IVciDevice connectedImib;
        [DataMember]
        private ConnectionTargetTypes vciTypesToShow;
        [DataMember]
        private ObservableCollection<IVciDevice> devices;
        private const string RegPathImibIsvmApplication = "HKEY_LOCAL_MACHINE\\SOFTWARE\\DiTest\\Dix\\IMIBNextApplication";
        private const string RegPathImibIsvmApplication32bit = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\DiTest\\Dix\\IMIBNextApplication";
        public bool ShouldLogin => shouldLogin;

        public string ReservationIcomType
        {
            get
            {
                return reservationIcomType;
            }

            set
            {
                reservationIcomType = value;
                OnPropertyChanged("ReservationIcomType");
            }
        }

        public bool DoImibReservation
        {
            get
            {
                return doImibReservation;
            }

            set
            {
                doImibReservation = value;
                OnPropertyChanged("DoImibReservation");
            }
        }

        public ObservableCollection<IVciDevice> Devices
        {
            get
            {
                return devices;
            }

            set
            {
                devices = value;
            }
        }

        public IVciDevice ConnectedVci
        {
            get
            {
                return connectedVci;
            }

            set
            {
                connectedVci = value;
                OnPropertyChanged("ConnectedVci");
            }
        }

        public IVciDevice ConnectedImib
        {
            get
            {
                return connectedImib;
            }

            set
            {
                connectedImib = value;
                OnPropertyChanged("ConnectedImib");
            }
        }

        public ConnectionTargetTypes VciTypesToShow
        {
            get
            {
                return vciTypesToShow;
            }

            set
            {
                vciTypesToShow = value;
                OnPropertyChanged("VciTypesToShow");
            }
        }

        public InteractionConnectionManagerModel(ILogic logic, IVciDevice connectedVci, IVciDevice connectedImib, ConnectionTargetTypes vciTypesToShow, bool shouldLogin)
        {
            this.logic = logic;
            this.shouldLogin = shouldLogin;
            devices = new ObservableCollection<IVciDevice>();
            ConnectedVci = connectedVci;
            ConnectedImib = connectedImib;
            VciTypesToShow = vciTypesToShow;
            DialogSize = 2;
            Title = FormatedData.Localize("#ConnectionManager");
            IsCloseButtonEnabled = true;
            doImibReservation = ConfigSettings.getConfigStringAsBoolean("TesterGUI.IMIBDeviceReservation", defaultValue: true);
            reservationIcomType = ConfigSettings.getConfigString("xVM_VCI_RESERVATION_TYPE", "ivm");
        }

        public override void OnResponseReceived(InteractionConnectionManagerResponse response)
        {
            Log.Info("InteractionConnectionManagerModel.OnResponseRecived()", "InteractionConnectionManagerResponse was set to the model. Parameters:  Button = '{0}'", response?.Action);
            NotifyAboutResponseReceived();
            Dispose();
        }

        public bool CanDisconnect(IVciDevice device)
        {
            if (device == null)
            {
                return false;
            }

            if (string.Equals(connectedImib?.Serial, device.Serial) && int.Parse(device.State) == 4)
            {
                connectedImib = null;
            }

            bool num = device.VCIType == VCIDeviceType.IMIB && string.Equals(connectedImib?.Serial, device.Serial) && (ReadConfigStringValue("STATESWITCH") != "0" || int.Parse(device.State) != 4);
            bool flag = (device.VCIType == VCIDeviceType.ICOM || device.VCIType == VCIDeviceType.SIM || device.VCIType == VCIDeviceType.ENET) && string.Equals(connectedVci?.Serial, device.Serial);
            bool flag2 = (device.VCIType == VCIDeviceType.EDIABAS || device.VCIType == VCIDeviceType.PTT) && connectedVci != null;
            return num | flag | flag2;
        }

        public bool CanConnect(IVciDevice device)
        {
            if (device == null)
            {
                return false;
            }

            if (logic?.VecInfo != null && ConfigSettings.IsLightModeActive)
            {
                return false;
            }

            if (VCIDeviceType.IMIB.Equals(device.VCIType) && ReadConfigStringValue("STATESWITCH") == "0" && device.IsConnectable && connectedImib != null)
            {
                ConnectedImib = null;
            }

            bool num = VCIDeviceType.IMIB.Equals(device.VCIType) && (connectedImib == null || connectedImib.VCIType == VCIDeviceType.UNKNOWN) && (!doImibReservation || !device.IsConnected);
            bool flag = VCIDeviceType.ICOM.Equals(device.VCIType) && device.VCIReservation == VCIReservationType.NONE && (connectedVci == null || connectedVci.VCIType == VCIDeviceType.UNKNOWN || connectedVci.VCIType == VCIDeviceType.INFOSESSION) && (string.Compare(ReservationIcomType, "none", StringComparison.OrdinalIgnoreCase) == 0 || device.IsConnectable);
            bool flag2 = VCIDeviceType.ENET.Equals(device.VCIType) && device.VCIReservation == VCIReservationType.NONE && (connectedVci == null || connectedVci.VCIType == VCIDeviceType.UNKNOWN || connectedVci == null || connectedVci.VCIType == VCIDeviceType.INFOSESSION);
            bool flag3 = VCIDeviceType.SIM.Equals(device.VCIType) && (connectedVci == null || connectedVci.VCIType == VCIDeviceType.UNKNOWN || connectedVci.VCIType == VCIDeviceType.INFOSESSION);
            bool flag4 = VCIDeviceType.EDIABAS.Equals(device.VCIType) && (connectedVci == null || connectedVci.VCIType == VCIDeviceType.UNKNOWN || connectedVci.VCIType == VCIDeviceType.INFOSESSION);
            bool flag5 = VCIDeviceType.PTT.Equals(device.VCIType) && (connectedVci == null || connectedVci.VCIType == VCIDeviceType.UNKNOWN || connectedVci.VCIType == VCIDeviceType.INFOSESSION);
            return num | flag | flag2 | flag3 | flag4 | flag5;
        }

        private string ReadConfigStringValue(string regkey)
        {
            string text = ConfigSettings.getConfigString("HKEY_LOCAL_MACHINE\\SOFTWARE\\DiTest\\Dix\\IMIBNextApplication", regkey, null);
            string configString = ConfigSettings.getConfigString("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\DiTest\\Dix\\IMIBNextApplication", regkey, null);
            if (text == null)
            {
                text = configString ?? "0";
            }

            return text;
        }

        public bool CanConfigInterface(IVciDevice device)
        {
            bool result = false;
            if (device != null && device.VCIType == VCIDeviceType.ICOM && device is VCIDevice && (CanConnect(device) || device.State == "14") && ConfigIAPHelper.IsDecentralVciConfigActive())
            {
                result = true;
            }

            return result;
        }

        public ILogic GetLogic()
        {
            return logic;
        }

        public override void LogMessage()
        {
        }

        public override void LogResponseMessage(object response)
        {
        }
    }
}