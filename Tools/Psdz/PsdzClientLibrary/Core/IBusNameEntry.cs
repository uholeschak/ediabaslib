using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface IBusNameEntry
    {
        BusType Bus { get; }

        string Name { get; }
    }
}
