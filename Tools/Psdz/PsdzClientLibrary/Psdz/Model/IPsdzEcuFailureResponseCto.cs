using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzEcuFailureResponseCto
    {
        IPsdzEcuIdentifier EcuIdentifierCto { get; }

        ILocalizableMessageTo Cause { get; }
    }
}
