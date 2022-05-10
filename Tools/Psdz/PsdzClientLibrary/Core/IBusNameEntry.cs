using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public interface IBusNameEntry
    {
        BusType Bus { get; }

        string Name { get; }
    }
}
