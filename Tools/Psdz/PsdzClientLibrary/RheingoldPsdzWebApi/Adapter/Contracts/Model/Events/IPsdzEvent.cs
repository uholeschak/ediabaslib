using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    public interface IPsdzEvent : ILocalizableMessage
    {
        IPsdzEcuIdentifier EcuId { get; }

        string EventId { get; }

        string Message { get; }

        long Timestamp { get; }
    }
}
