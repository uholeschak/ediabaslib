using BMW.Rheingold.Psdz.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzKdsActionStatusResultCto
    {
        PsdzKdsActionStatusEto KdsActionStatus { get; }

        IPsdzKdsFailureResponseCto KdsFailureResponse { get; }

        IPsdzKdsIdCto KdsId { get; }
    }
}
