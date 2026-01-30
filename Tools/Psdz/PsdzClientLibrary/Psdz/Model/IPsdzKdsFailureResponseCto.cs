using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzKdsFailureResponseCto
    {
        ILocalizableMessageTo Cause { get; }

        IPsdzKdsIdCto KdsId { get; }
    }
}
