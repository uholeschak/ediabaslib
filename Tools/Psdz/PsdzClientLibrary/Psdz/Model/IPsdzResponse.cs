namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzResponse
    {
        string Cause { get; set; }

        object Result { get; set; }

        bool IsSuccessful { get; set; }
    }
}
