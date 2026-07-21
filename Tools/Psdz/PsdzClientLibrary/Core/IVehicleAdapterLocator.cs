namespace PsdzClient.Core;

[AuthorAPI]
public interface IVehicleAdapterLocator : ISPELocator
{
    string Title { get; }
}
