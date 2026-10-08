namespace BMW.Rheingold.Measurement.Common.Contract
{
    public interface IGenericMeasurementDevice
    {
        MeasurementResult Command(string command);
    }
}
