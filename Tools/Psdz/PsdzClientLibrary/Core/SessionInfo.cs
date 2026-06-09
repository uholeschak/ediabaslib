using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [DataContract]
    public class SessionInfo : INotifyPropertyChanged
    {
        private SessionStart sessionStart;

        private Guid operationId;

        private string buNo;

        private CurrentSessionState sessionState = new CurrentSessionState();

        private bool isEcuIdentSuccessfull;

        private bool isClosingOperationActive;

        private bool isVehicleBusy;

        private bool isDoIP;

        private bool isCcmReadoutDone;

        private bool isProgrammingSessionStartable;

        private bool isVehicleTestDone;

        private bool isReadingFastaDataFinished;

        private bool vehicleIdentAlreadyDone;

        private bool isNewIdentActive;

        private bool isVehicleBreakdownAlreadyShown;

        private bool isPowerSafeModeActiveByNewEcus;

        private bool isPowerSafeModeActiveByOldEcus;

        private bool orderDataRequestFailed;

        private bool dOMRequestFailed;

        private bool ssl2RequestFailed;

        private bool tecCampaignsRequestFailed;

        private bool repHistoryRequestFailed;

        private bool vinNotReadbleFromCarAbort;

        private bool withLfpBattery;

        private bool withLfpNCarBattery;

        private bool kL15OverrideVoltageCheck;

        private bool kL15FaultILevelAlreadyAlerted;

        private bool simulatedParts;

        private bool zfsSuccessfull;

        private CentralErrorMemoryStatus centralErrorMemoryStatus;

        private bool isSendOBFCMDataForbidden;

        private bool isSendFastaDataForbidden;

        private bool noVehicleCommunicationRunning;

        private bool connectionLossRecognized;

        private bool isSecurityPopupAlreadyShown;

        private bool shouldSkipPinAuthentication;

        private string statusFunctionName;

        private string vin17FromInputFieldMR;

        private double statusFunctionProgress;

        private int? faultCodeSum;

        private int? nonSignalErrorFaultCodeSum;

        [DataMember]
        [IgnoreForReopenedOperations]
        public SessionStart SessionStart
        {
            get
            {
                return sessionStart;
            }
            set
            {
                if (sessionStart != value)
                {
                    sessionStart = value;
                    OnPropertyChanged("SessionStart");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        [XmlIgnore]
        public Guid OperationId
        {
            get
            {
                return operationId;
            }
            set
            {
                if (operationId != value)
                {
                    operationId = value;
                    OnPropertyChanged("OperationId");
                    OnPropertyChanged("IstaCaseId");
                }
            }
        }

        public string IstaCaseId => operationId.ToString();

        [DataMember]
        [IgnoreForReopenedOperations]
        public string BuNo
        {
            get
            {
                return buNo;
            }
            set
            {
                if (buNo != value)
                {
                    buNo = value;
                    OnPropertyChanged("BuNo");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public CurrentSessionState SessionState
        {
            get
            {
                return sessionState;
            }
            set
            {
                if (sessionState != value)
                {
                    sessionState = value;
                    OnPropertyChanged("SessionState");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsEcuIdentSuccessfull
        {
            get
            {
                return isEcuIdentSuccessfull;
            }
            set
            {
                if (isEcuIdentSuccessfull != value)
                {
                    isEcuIdentSuccessfull = value;
                    OnPropertyChanged("IsEcuIdentSuccessfull");
                }
            }
        }

        [DataMember]
        [XmlIgnore]
        public bool IsClosingOperationActive
        {
            get
            {
                return isClosingOperationActive;
            }
            set
            {
                if (isClosingOperationActive != value)
                {
                    isClosingOperationActive = value;
                    OnPropertyChanged("IsClosingOperationActive");
                }
            }
        }

        [DataMember]
        [XmlIgnore]
        public bool IsVehicleBusy
        {
            get
            {
                return isVehicleBusy;
            }
            set
            {
                if (isVehicleBusy != value)
                {
                    isVehicleBusy = value;
                    OnPropertyChanged("IsVehicleBusy");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsDoIP
        {
            get
            {
                return isDoIP;
            }
            set
            {
                if (isDoIP != value)
                {
                    isDoIP = value;
                    OnPropertyChanged("IsDoIP");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsCcmReadoutDone
        {
            get
            {
                return isCcmReadoutDone;
            }
            set
            {
                if (isCcmReadoutDone != value)
                {
                    isCcmReadoutDone = value;
                    OnPropertyChanged("IsCcmReadoutDone");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsProgrammingSessionStartable
        {
            get
            {
                return isProgrammingSessionStartable;
            }
            set
            {
                if (isProgrammingSessionStartable != value)
                {
                    isProgrammingSessionStartable = value;
                    OnPropertyChanged("IsProgrammingSessionStartable");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsVehicleTestDone
        {
            get
            {
                return isVehicleTestDone;
            }
            set
            {
                if (isVehicleTestDone != value)
                {
                    isVehicleTestDone = value;
                    OnPropertyChanged("IsVehicleTestDone");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsReadingFastaDataFinished
        {
            get
            {
                return isReadingFastaDataFinished;
            }
            set
            {
                if (isReadingFastaDataFinished != value)
                {
                    isReadingFastaDataFinished = value;
                    OnPropertyChanged("IsReadingFastaDataFinished");
                }
            }
        }

        [DataMember]
        public bool VehicleIdentAlreadyDone
        {
            get
            {
                return vehicleIdentAlreadyDone;
            }
            set
            {
                if (vehicleIdentAlreadyDone != value)
                {
                    vehicleIdentAlreadyDone = value;
                    OnPropertyChanged("VehicleIdentAlreadyDone");
                }
            }
        }

        [DataMember]
        public bool IsNewIdentActive
        {
            get
            {
                return isNewIdentActive;
            }
            set
            {
                isNewIdentActive = value;
                OnPropertyChanged("IsNewIdentActive");
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsVehicleBreakdownAlreadyShown
        {
            get
            {
                return isVehicleBreakdownAlreadyShown;
            }
            set
            {
                isVehicleBreakdownAlreadyShown = value;
                OnPropertyChanged("IsVehicleBreakdownAlreadyShown");
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsPowerSafeModeActiveByNewEcus
        {
            get
            {
                return isPowerSafeModeActiveByNewEcus;
            }
            set
            {
                isPowerSafeModeActiveByNewEcus = value;
                OnPropertyChanged("IsPowerSafeModeActiveByNewEcus");
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsPowerSafeModeActiveByOldEcus
        {
            get
            {
                return isPowerSafeModeActiveByOldEcus;
            }
            set
            {
                isPowerSafeModeActiveByOldEcus = value;
                OnPropertyChanged("IsPowerSafeModeActiveByOldEcus");
            }
        }

        [IgnoreDataMember]
        [XmlIgnore]
        public bool IsPowerSafeModeActive
        {
            get
            {
                if (!isPowerSafeModeActiveByOldEcus)
                {
                    return isPowerSafeModeActiveByNewEcus;
                }
                return true;
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
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

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool DOMRequestFailed
        {
            get
            {
                return dOMRequestFailed;
            }
            set
            {
                if (dOMRequestFailed != value)
                {
                    dOMRequestFailed = value;
                    OnPropertyChanged("DOMRequestFailed");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool Ssl2RequestFailed
        {
            get
            {
                return ssl2RequestFailed;
            }
            set
            {
                if (ssl2RequestFailed != value)
                {
                    ssl2RequestFailed = value;
                    OnPropertyChanged("Ssl2RequestFailed");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool TecCampaignsRequestFailed
        {
            get
            {
                return tecCampaignsRequestFailed;
            }
            set
            {
                if (tecCampaignsRequestFailed != value)
                {
                    tecCampaignsRequestFailed = value;
                    OnPropertyChanged("TecCampaignsRequestFailed");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool RepHistoryRequestFailed
        {
            get
            {
                return repHistoryRequestFailed;
            }
            set
            {
                if (repHistoryRequestFailed != value)
                {
                    repHistoryRequestFailed = value;
                    OnPropertyChanged("RepHistoryRequestFailed");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool VinNotReadbleFromCarAbort
        {
            get
            {
                return vinNotReadbleFromCarAbort;
            }
            set
            {
                vinNotReadbleFromCarAbort = value;
                OnPropertyChanged("VinNotReadbleFromCarAbort");
            }
        }

        [DataMember]
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

        [DataMember]
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

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool KL15OverrideVoltageCheck
        {
            get
            {
                return kL15OverrideVoltageCheck;
            }
            set
            {
                if (kL15OverrideVoltageCheck != value)
                {
                    kL15OverrideVoltageCheck = value;
                    OnPropertyChanged("KL15OverrideVoltageCheck");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool KL15FaultILevelAlreadyAlerted
        {
            get
            {
                return kL15FaultILevelAlreadyAlerted;
            }
            set
            {
                if (kL15FaultILevelAlreadyAlerted != value)
                {
                    kL15FaultILevelAlreadyAlerted = value;
                    OnPropertyChanged("KL15FaultILevelAlreadyAlerted");
                }
            }
        }

        [DataMember]
        public bool SimulatedParts
        {
            get
            {
                return simulatedParts;
            }
            set
            {
                if (simulatedParts != value)
                {
                    simulatedParts = value;
                    OnPropertyChanged("SimulatedParts");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool ZfsSuccessfull
        {
            get
            {
                return zfsSuccessfull;
            }
            set
            {
                if (zfsSuccessfull != value)
                {
                    zfsSuccessfull = value;
                    OnPropertyChanged("ZfsSuccessfull");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public CentralErrorMemoryStatus CentralErrorMemoryStatus
        {
            get
            {
                return centralErrorMemoryStatus;
            }
            set
            {
                if (centralErrorMemoryStatus != value)
                {
                    centralErrorMemoryStatus = value;
                }
            }
        }

        [DataMember]
        public bool IsSendOBFCMDataForbidden
        {
            get
            {
                return isSendOBFCMDataForbidden;
            }
            set
            {
                if (isSendOBFCMDataForbidden != value)
                {
                    isSendOBFCMDataForbidden = value;
                    OnPropertyChanged("IsSendOBFCMDataForbidden");
                }
            }
        }

        [DataMember]
        public bool IsSendFastaDataForbidden
        {
            get
            {
                return isSendFastaDataForbidden;
            }
            set
            {
                if (isSendFastaDataForbidden != value)
                {
                    isSendFastaDataForbidden = value;
                    OnPropertyChanged("IsSendFastaDataForbidden");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
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

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool ConnectionLossRecognized
        {
            get
            {
                return connectionLossRecognized;
            }
            set
            {
                if (connectionLossRecognized != value)
                {
                    connectionLossRecognized = value;
                    OnPropertyChanged("ConnectionLossRecognized");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public bool IsSecurityPopupAlreadyShown
        {
            get
            {
                return isSecurityPopupAlreadyShown;
            }
            set
            {
                if (isSecurityPopupAlreadyShown != value)
                {
                    isSecurityPopupAlreadyShown = value;
                    OnPropertyChanged("IsSecurityPopupAlreadyShown");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public string Status_FunctionName
        {
            get
            {
                return statusFunctionName;
            }
            set
            {
                if (statusFunctionName != value)
                {
                    statusFunctionName = value;
                    OnPropertyChanged("Status_FunctionName");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public string Vin17FromInputFieldMR
        {
            get
            {
                return vin17FromInputFieldMR;
            }
            set
            {
                if (vin17FromInputFieldMR != value)
                {
                    vin17FromInputFieldMR = value;
                    OnPropertyChanged("Vin17FromInputFieldMR");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
        public double Status_FunctionProgress
        {
            get
            {
                return statusFunctionProgress;
            }
            set
            {
                if (statusFunctionProgress != value)
                {
                    statusFunctionProgress = value;
                    OnPropertyChanged("Status_FunctionProgress");
                }
            }
        }

        [DataMember]
        [IgnoreForReopenedOperations]
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

        [DataMember]
        [IgnoreForReopenedOperations]
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

        [DataMember]
        [XmlIgnore]
        [IgnoreForReopenedOperations]
        public bool ShouldSkipPinAuthentication
        {
            get
            {
                return shouldSkipPinAuthentication;
            }
            set
            {
                if (shouldSkipPinAuthentication != value)
                {
                    shouldSkipPinAuthentication = value;
                    OnPropertyChanged("ShouldSkipPinAuthentication");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RewriteProperties(SessionInfo source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            PropertyInfo[] properties = typeof(SessionInfo).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.GetIndexParameters().Length == 0 && propertyInfo.CanRead && propertyInfo.CanWrite && !Attribute.IsDefined(propertyInfo, typeof(IgnoreForReopenedOperationsAttribute)) && !Attribute.IsDefined(propertyInfo, typeof(XmlIgnoreAttribute)))
                {
                    object value = propertyInfo.GetValue(source);
                    propertyInfo.SetValue(this, value);
                }
            }
        }

        public virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
