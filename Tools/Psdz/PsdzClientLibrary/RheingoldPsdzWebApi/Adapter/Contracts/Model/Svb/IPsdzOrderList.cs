namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzOrderList
    {
        int NumberOfUnits { get; }

        IPsdzEcuVariantInstance[] BntnVariantInstances { get; }
    }
}
