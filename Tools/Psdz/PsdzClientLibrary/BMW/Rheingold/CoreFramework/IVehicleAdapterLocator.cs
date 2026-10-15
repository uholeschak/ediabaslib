namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI]
    public interface IVehicleAdapterLocator : ISPELocator
    {
        string Title { get; }
    }
}
