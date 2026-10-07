using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzKdsQuickCheckResultCto
    {
        IPsdzKdsIdCto KdsId { get; }

        PsdzQuickCheckResultEto QuickCheckResult { get; }
    }
}
