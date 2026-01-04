using BMW.Rheingold.Programming.Common;

namespace PsdzClient.Core
{
    public interface IVoltageThreshold
    {
        double MinError { get; }

        double MinWarning { get; }

        double MaxWarning { get; }

        double MaxError { get; }

        BatteryEnum BatteryType { get; }
    }
}