using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo
{
    public interface ILocalizableMessageTo : ILocalizableMessage
    {
        string Description { get; }
    }
}
