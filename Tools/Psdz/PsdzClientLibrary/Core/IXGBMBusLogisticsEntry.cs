using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public interface IXGBMBusLogisticsEntry
    {
        BusType[] Bus { get; }

        string XgbmPrefix { get; }
    }
}
