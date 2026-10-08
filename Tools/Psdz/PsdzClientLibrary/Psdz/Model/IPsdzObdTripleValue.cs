namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Obd
{
    public interface IPsdzObdTripleValue
    {
        string CalId { get; }

        string ObdId { get; }

        string SubCVN { get; }
    }
}
