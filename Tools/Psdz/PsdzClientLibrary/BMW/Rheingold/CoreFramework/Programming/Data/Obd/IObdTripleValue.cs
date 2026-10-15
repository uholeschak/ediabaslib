namespace BMW.Rheingold.CoreFramework.Programming.Data.Obd
{
    public interface IObdTripleValue
    {
        string CalId { get; }

        string ObdId { get; }

        string SubCVN { get; }
    }
}
