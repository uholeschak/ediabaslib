namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzTargetSelector
    {
        string Baureihenverbund { get; }

        bool IsDirect { get; }

        string Project { get; }

        string VehicleInfo { get; }
    }
}
