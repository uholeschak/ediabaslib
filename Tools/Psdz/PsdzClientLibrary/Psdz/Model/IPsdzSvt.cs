namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzSvt : IPsdzStandardSvt
    {
        bool IsValid { get; }

        string Vin { get; }
    }
}
