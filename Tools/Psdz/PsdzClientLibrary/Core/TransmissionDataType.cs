using PsdzClient.Contracts;
using System;

namespace PsdzClient.Core
{
    public class TransmissionDataType : IEquatable<TransmissionDataType>, ICloneable
    {
        public string TransmissionClassValue { get; set; }

        public string TransmissionClassVersion { get; set; }

        public bool IsDataFromVehicle { get; set; }

        public string VtgSerialNumber { get; set; }

        public int VtgCalculatedCharacteristicCurve { get; set; }

        public TransmissionDataType()
        {
        }

        public TransmissionDataType(string classValue, string classVersion, string vtgSerialNumber, int vtgCalculatedCharacteristicsCurve, bool isDataFromVehicle)
        {
            TransmissionClassValue = classValue;
            TransmissionClassVersion = classVersion;
            VtgCalculatedCharacteristicCurve = vtgCalculatedCharacteristicsCurve;
            VtgSerialNumber = vtgSerialNumber;
            IsDataFromVehicle = isDataFromVehicle;
        }

        public void UpdateTransmissionDataValues(string classValue, string classVersion, string vtgSerialNumber, int vtgCalculatedCharacteristicsCurve, bool isFromVehicle)
        {
            TransmissionClassValue = classValue;
            TransmissionClassVersion = classVersion;
            VtgCalculatedCharacteristicCurve = vtgCalculatedCharacteristicsCurve;
            VtgSerialNumber = vtgSerialNumber;
            IsDataFromVehicle = isFromVehicle;
        }

        public bool IsAnyValueEmpty()
        {
            return string.IsNullOrEmpty(TransmissionClassValue) || string.IsNullOrEmpty(TransmissionClassVersion);
        }

        public bool Equals(TransmissionDataType other)
        {
            return other != null && string.Equals(TransmissionClassValue, other.TransmissionClassValue, StringComparison.InvariantCultureIgnoreCase) && string.Equals(TransmissionClassVersion, other.TransmissionClassVersion, StringComparison.InvariantCultureIgnoreCase) && VtgCalculatedCharacteristicCurve == other.VtgCalculatedCharacteristicCurve && string.Equals(VtgSerialNumber, other.VtgSerialNumber, StringComparison.InvariantCultureIgnoreCase) && IsDataFromVehicle == other.IsDataFromVehicle;
        }

        public object Clone()
        {
            return new TransmissionDataType(TransmissionClassValue, TransmissionClassVersion, VtgSerialNumber, VtgCalculatedCharacteristicCurve, IsDataFromVehicle);
        }
    }
}
