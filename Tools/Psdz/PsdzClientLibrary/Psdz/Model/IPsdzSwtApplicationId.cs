namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt
{
    public interface IPsdzSwtApplicationId
    {
        int ApplicationNumber { get; }

        int UpgradeIndex { get; }
    }
}
