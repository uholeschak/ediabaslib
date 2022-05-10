using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public interface IBusInterConnectionEntry
    {
        BusType Bus { get; }

        double XStart { get; }

        double YStart { get; }

        double XEnd { get; }

        double YEnd { get; }

        int[] RequiredEcuAddresses { get; }
    }
}
