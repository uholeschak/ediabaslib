using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public interface IBusLogisticsEntry
    {
        BusType Bus { get; }

        int Column { get; }

        int MinRow { get; }

        int MaxRowLimit { get; }

        double XRelativeOffset { get; }

        double YRelativeOffset { get; }

        bool PaintToRoot { get; }

        bool ConnectOnlyToRightEcu { get; }

        bool DrawVerticalLines { get; }

        int[] RequiredEcuAddresses { get; }

        bool DrawHorizontalLines { get; }

        bool BikeBus { get; }
    }
}
