

using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface IBusNameEntry
    {
        BusType Bus { get; }

        string Name { get; }
    }
}
