using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Localization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

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
