
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSecurityBackendRequestFailureCto
    {
        ILocalizableMessageTo Cause { get; }

        int Retry { get; }

        string Url { get; }
    }
}
