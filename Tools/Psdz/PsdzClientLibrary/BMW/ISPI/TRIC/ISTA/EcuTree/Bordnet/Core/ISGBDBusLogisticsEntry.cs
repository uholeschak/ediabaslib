
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface ISGBDBusLogisticsEntry
    {
        BusType Bus { get; }

        BusType[] SubBusList { get; }

        string Variant { get; }
    }
}
