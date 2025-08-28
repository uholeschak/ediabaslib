using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    public interface IPsdzTransactionProgressEvent : IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        int Progress { get; }

        int TaProgress { get; }
    }
}
