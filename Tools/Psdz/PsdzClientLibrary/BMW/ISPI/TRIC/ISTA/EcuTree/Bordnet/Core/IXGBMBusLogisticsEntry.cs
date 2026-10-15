
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface IXGBMBusLogisticsEntry
    {
        BusType[] Bus { get; }

        string XgbmPrefix { get; }
    }
}
