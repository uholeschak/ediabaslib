namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzKdsPublicKeyResultCto
    {
        IPsdzKdsIdCto KdsId { get; }

        byte[] PublicKey { get; }
    }
}
