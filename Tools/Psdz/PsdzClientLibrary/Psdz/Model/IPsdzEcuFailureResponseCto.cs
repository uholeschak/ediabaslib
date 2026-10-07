using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzEcuFailureResponseCto
    {
        IPsdzEcuIdentifier EcuIdentifierCto { get; }

        ILocalizableMessageTo Cause { get; }
    }
}
