namespace PsdzClient.Contracts
{
    public interface ITransmissionDataType
    {
        string TransmissionClassValue { get; }

        string TransmissionClassVersion { get; }

        bool IsDataFromVehicle { get; }

        void UpdateTransmissionDataValues(string classValue, string classVersion, bool isFromVehicle);

        bool IsAnyValueEmpty();
    }
}
