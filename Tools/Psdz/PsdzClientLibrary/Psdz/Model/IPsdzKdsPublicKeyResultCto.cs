namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzKdsPublicKeyResultCto
    {
        IPsdzKdsIdCto KdsId { get; }

        byte[] PublicKey { get; }
    }
}
