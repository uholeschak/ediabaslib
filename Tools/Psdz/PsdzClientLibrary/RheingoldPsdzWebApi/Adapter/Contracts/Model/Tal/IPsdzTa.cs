namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    public interface IPsdzTa : IPsdzTalElement
    {
        IPsdzSgbmId SgbmId { get; }
    }
}
