namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzIstufenTriple
    {
        string Current { get; }

        string Last { get; }

        string Shipment { get; }
    }
}
