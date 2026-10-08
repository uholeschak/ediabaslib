namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzRequestNcdEto
    {
        IPsdzSgbmId Btld { get; }

        IPsdzSgbmId Cafd { get; }
    }
}
