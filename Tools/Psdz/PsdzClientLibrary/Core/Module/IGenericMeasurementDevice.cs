using BMW.Rheingold.Measurement.Common;

namespace BMW.Rheingold.Measurement.Common.Contract
{
    public interface IGenericMeasurementDevice
    {
        MeasurementResult Command(string command);
    }
}
