using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    public interface IPsdzMcdDiagServiceEvent : IPsdzEvent, ILocalizableMessage
    {
        int ErrorId { get; }

        string ErrorName { get; }

        string JobName { get; }

        string LinkName { get; }

        string ServiceName { get; }

        string ResponseType { get; }

        bool IsTimingEvent { get; }
    }
}
