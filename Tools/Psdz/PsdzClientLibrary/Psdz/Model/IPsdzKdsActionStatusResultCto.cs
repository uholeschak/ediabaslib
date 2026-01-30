namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzKdsActionStatusResultCto
    {
        PsdzKdsActionStatusEto KdsActionStatus { get; }

        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IPsdzKdsIdCto KdsId { get; }
    }
}
