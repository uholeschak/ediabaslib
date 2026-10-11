using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface IXGBMBusLogisticsEntry
    {
        BusType[] Bus { get; }

        string XgbmPrefix { get; }
    }
}
