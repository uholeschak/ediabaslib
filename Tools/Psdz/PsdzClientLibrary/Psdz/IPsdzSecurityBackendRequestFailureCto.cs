using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzSecurityBackendRequestFailureCto
    {
        ILocalizableMessageTo Cause { get; }

        int Retry { get; }

        string Url { get; }
    }
}
