using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface ISGBDBusLogisticsEntry
    {
        BusType Bus { get; }

        BusType[] SubBusList { get; }

        string Variant { get; }
    }
}
