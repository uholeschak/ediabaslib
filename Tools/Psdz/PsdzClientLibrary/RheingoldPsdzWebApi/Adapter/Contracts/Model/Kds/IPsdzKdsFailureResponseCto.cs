using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    public interface IPsdzKdsFailureResponseCto
    {
        ILocalizableMessageTo Cause { get; }

        IPsdzKdsIdCto KdsId { get; }
    }
}
