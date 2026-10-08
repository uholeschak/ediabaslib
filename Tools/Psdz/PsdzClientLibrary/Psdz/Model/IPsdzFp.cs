namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzFp : IPsdzStandardFp
    {
        string Baureihenverbund { get; }

        string Entwicklungsbaureihe { get; }
    }
}
