using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzEcuFailureResponseCto
    {
        IPsdzEcuIdentifier EcuIdentifierCto { get; }

        ILocalizableMessageTo Cause { get; }
    }
}
