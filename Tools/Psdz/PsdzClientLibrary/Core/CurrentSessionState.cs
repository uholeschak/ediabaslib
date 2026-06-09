using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    [DataContract]
    public class CurrentSessionState : INotifyPropertyChanged
    {
        private ObservableCollection<BackendServiceType> backendsFailedToResponse = new ObservableCollection<BackendServiceType>();

        private ObservableCollection<BackendServiceType> requestedBackends = new ObservableCollection<BackendServiceType>();

        private bool internetAccessable;

        private bool lanAvailable;

        private int wiFiStrength;

        private bool switchConfigured;

        private bool iCOMConfigured;

        private bool iCOMUpdateAvailable;

        private bool isCarInfosession;

        private bool isMotorcycleInfoSession;

        private bool isPTTConfigured;

        private bool isIMIBMAConfigured;

        private bool isICOMOverLan;

        private bool isIMIBOverLan;

        private bool isDirectConnection;

        private bool isOfflineMode;

        private bool isConnectedToSwitchOverLan;

        private bool eOSTest;

        private bool isCarSession;

        private bool isSimulation;

        [DataMember]
        public ObservableCollection<BackendServiceType> BackendsFailedToResponse
        {
            get
            {
                return backendsFailedToResponse;
            }
            set
            {
                backendsFailedToResponse = value;
                OnPropertyChanged("BackendsFailedToResponse");
            }
        }

        [DataMember]
        public ObservableCollection<BackendServiceType> RequestedBackends
        {
            get
            {
                return requestedBackends;
            }
            set
            {
                requestedBackends = value;
                OnPropertyChanged("RequestedBackends");
            }
        }

        [DataMember]
        public bool InternetAccessable
        {
            get
            {
                return internetAccessable;
            }
            set
            {
                internetAccessable = value;
                OnPropertyChanged("InternetAccessable");
            }
        }

        [DataMember]
        public bool LanAvailable
        {
            get
            {
                return lanAvailable;
            }
            set
            {
                lanAvailable = value;
                OnPropertyChanged("LanAvailable");
            }
        }

        [DataMember]
        public int WiFiStrength
        {
            get
            {
                return wiFiStrength;
            }
            set
            {
                wiFiStrength = value;
                OnPropertyChanged("WiFiStrength");
            }
        }

        [DataMember]
        public bool SwitchConfigured
        {
            get
            {
                return switchConfigured;
            }
            set
            {
                switchConfigured = value;
                OnPropertyChanged("SwitchConfigured");
            }
        }

        [DataMember]
        public bool ICOMConfigured
        {
            get
            {
                return iCOMConfigured;
            }
            set
            {
                iCOMConfigured = value;
                OnPropertyChanged("ICOMConfigured");
            }
        }

        [DataMember]
        public bool ICOMUpdateAvailable
        {
            get
            {
                return iCOMUpdateAvailable;
            }
            set
            {
                iCOMUpdateAvailable = value;
                OnPropertyChanged("ICOMUpdateAvailable");
            }
        }

        [DataMember]
        public bool IsCarInfosession
        {
            get
            {
                return isCarInfosession;
            }
            set
            {
                isCarInfosession = value;
                OnPropertyChanged("IsCarInfosession");
            }
        }

        [DataMember]
        public bool IsMotorcycleInfoSession
        {
            get
            {
                return isMotorcycleInfoSession;
            }
            set
            {
                isMotorcycleInfoSession = value;
                OnPropertyChanged("IsMotorcycleInfoSession");
            }
        }

        [DataMember]
        public bool IsPTTConfigured
        {
            get
            {
                return isPTTConfigured;
            }
            set
            {
                isPTTConfigured = value;
                OnPropertyChanged("IsPTTConfigured");
            }
        }

        [DataMember]
        public bool IsIMIBMAConfigured
        {
            get
            {
                return isIMIBMAConfigured;
            }
            set
            {
                isIMIBMAConfigured = value;
                OnPropertyChanged("IsIMIBMAConfigured");
            }
        }

        [DataMember]
        public bool IsICOMOverLan
        {
            get
            {
                return isICOMOverLan;
            }
            set
            {
                isICOMOverLan = value;
                OnPropertyChanged("IsICOMOverLan");
            }
        }

        [DataMember]
        public bool IsIMIBOverLan
        {
            get
            {
                return isIMIBOverLan;
            }
            set
            {
                isIMIBOverLan = value;
                OnPropertyChanged("IsIMIBOverLan");
            }
        }

        [DataMember]
        public bool IsDirectConnection
        {
            get
            {
                return isDirectConnection;
            }
            set
            {
                isDirectConnection = value;
                OnPropertyChanged("IsDirectConnection");
            }
        }

        [DataMember]
        public bool IsOfflineMode
        {
            get
            {
                return isOfflineMode;
            }
            set
            {
                isOfflineMode = value;
                OnPropertyChanged("IsOfflineMode");
            }
        }

        [DataMember]
        public bool IsConnectedToSwitchOverLan
        {
            get
            {
                return isConnectedToSwitchOverLan;
            }
            set
            {
                isConnectedToSwitchOverLan = value;
                OnPropertyChanged("IsConnectedToSwitchOverLan");
            }
        }

        [DataMember]
        public bool EOSTest
        {
            get
            {
                return eOSTest;
            }
            set
            {
                eOSTest = value;
                OnPropertyChanged("EOSTest");
            }
        }

        [DataMember]
        public bool IsCarSession
        {
            get
            {
                return isCarSession;
            }
            set
            {
                isCarSession = value;
                OnPropertyChanged("IsCarSession");
            }
        }

        [DataMember]
        public bool IsSimulation
        {
            get
            {
                return isSimulation;
            }
            set
            {
                isSimulation = value;
                OnPropertyChanged("IsSimulation");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
