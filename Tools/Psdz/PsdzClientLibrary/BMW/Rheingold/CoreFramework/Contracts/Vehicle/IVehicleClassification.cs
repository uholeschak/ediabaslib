namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    public interface IVehicleClassification
    {
        bool IsSp2021 { get; set; }

        bool IsSp2025 { get; set; }

        bool IsNewFaultMemoryActive { get; set; }

        bool IsNewFaultMemoryExpertModeActive { get; set; }

        bool IsNCar { get; }

        bool IsPreE65Vehicle();

        bool IsPreDS2Vehicle();

        bool IsMotorcycle();
    }
}