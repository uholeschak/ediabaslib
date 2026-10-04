using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
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
