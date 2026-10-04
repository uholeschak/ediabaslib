using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface ISGBDBusLogisticsEntry
    {
        BusType Bus { get; }

        BusType[] SubBusList { get; }

        string Variant { get; }
    }
}
