using PsdzClient.Contracts;
using System;

namespace PsdzClient.Core
{
    public class TransmissionDataType : ICloneable, IEquatable<TransmissionDataType>, ITransmissionDataType
    {
        public string TransmissionClassValue { get; private set; }

        public string TransmissionClassVersion { get; private set; }

        public bool IsDataFromVehicle { get; private set; }

        public TransmissionDataType()
        {
        }

        public TransmissionDataType(string classValue, string classVersion, bool isDataFromVehicle)
        {
            TransmissionClassValue = classValue;
            TransmissionClassVersion = classVersion;
            IsDataFromVehicle = isDataFromVehicle;
        }

        public void UpdateTransmissionDataValues(string classValue, string classVersion, bool isFromVehicle)
        {
            TransmissionClassValue = classValue;
            TransmissionClassVersion = classVersion;
            IsDataFromVehicle = isFromVehicle;
        }

        public bool IsAnyValueEmpty()
        {
            if (!string.IsNullOrEmpty(TransmissionClassValue))
            {
                return string.IsNullOrEmpty(TransmissionClassVersion);
            }
            return true;
        }

        public bool Equals(TransmissionDataType other)
        {
            if (other != null && string.Equals(TransmissionClassValue, other.TransmissionClassValue, StringComparison.InvariantCultureIgnoreCase) && string.Equals(TransmissionClassVersion, other.TransmissionClassVersion, StringComparison.InvariantCultureIgnoreCase))
            {
                return IsDataFromVehicle == other.IsDataFromVehicle;
            }
            return false;
        }

        public object Clone()
        {
            return new TransmissionDataType(TransmissionClassValue, TransmissionClassVersion, IsDataFromVehicle);
        }
    }
}
