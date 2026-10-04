using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface IBusNameEntry
    {
        BusType Bus { get; }

        string Name { get; }
    }
}
