using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [DataContract]
    public class VehicleClassification : IVehicleClassification, INotifyPropertyChanged
    {
        private readonly IVehicle vehicle;

        private readonly IDiagnosticsBusinessData diagnosticsBusinessData;

        private bool isSp2021;

        private bool isSp2025;

        private bool isNewFaultMemoryActive;

        private bool isNewFaultMemoryExpertModeActive;

        [DataMember]
        public bool IsSp2021
        {
            get
            {
                return isSp2021;
            }
            set
            {
                if (isSp2021 != value)
                {
                    isSp2021 = value;
                    OnPropertyChanged("IsSp2021");
                }
            }
        }

        [DataMember]
        public bool IsSp2025
        {
            get
            {
                return isSp2025;
            }
            set
            {
                if (isSp2025 != value)
                {
                    isSp2025 = value;
                    OnPropertyChanged("IsSp2025");
                }
            }
        }

        [DataMember]
        public bool IsNewFaultMemoryActive
        {
            get
            {
                return isNewFaultMemoryActive;
            }
            set
            {
                if (isNewFaultMemoryActive != value)
                {
                    isNewFaultMemoryActive = value;
                    OnPropertyChanged("IsNewFaultMemoryActive");
                }
            }
        }

        [DataMember]
        public bool IsNewFaultMemoryExpertModeActive
        {
            get
            {
                return isNewFaultMemoryExpertModeActive;
            }
            set
            {
                if (isNewFaultMemoryExpertModeActive != value)
                {
                    isNewFaultMemoryExpertModeActive = value;
                    OnPropertyChanged("IsNewFaultMemoryExpertModeActive");
                }
            }
        }

        public bool IsNCar => diagnosticsBusinessData.IsEES25Vehicle(vehicle);

        public event PropertyChangedEventHandler PropertyChanged;

        public VehicleClassification(IVehicle vec)
        {
            vehicle = vec;
            if (!ServiceLocator.Current.TryGetService<IDiagnosticsBusinessData>(out diagnosticsBusinessData))
            {
                string msg = "Cannot resolve DiagnosticBusinessData.";
                Log.Error(Log.CurrentMethod(), msg);
            }
        }

        public bool IsPreE65Vehicle()
        {
            return diagnosticsBusinessData.IsPreE65Vehicle(vehicle.Ereihe);
        }

        public bool IsPreDS2Vehicle()
        {
            return diagnosticsBusinessData.IsPreDS2Vehicle(vehicle.Ereihe, vehicle.C_DATETIME);
        }

        public bool IsMotorcycle()
        {
            if (vehicle.BNType != BNType.BN2000_MOTORBIKE && vehicle.BNType != BNType.BN2020_MOTORBIKE)
            {
                return vehicle.BNType == BNType.BNK01X_MOTORBIKE;
            }
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}