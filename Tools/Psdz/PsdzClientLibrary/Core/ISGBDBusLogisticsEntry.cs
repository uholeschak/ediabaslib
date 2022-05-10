using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public interface ISGBDBusLogisticsEntry
    {
        BusType Bus { get; }

        BusType[] SubBusList { get; }

        string Variant { get; }
    }
}
