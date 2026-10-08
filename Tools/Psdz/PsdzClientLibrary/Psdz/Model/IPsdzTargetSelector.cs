namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzTargetSelector
    {
        string Baureihenverbund { get; }

        bool IsDirect { get; }

        string Project { get; }

        string VehicleInfo { get; }
    }
}
