using PsdzClient;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    [PreserveSource(Hint = "Added OLD_PSDZ_FA", SuppressWarning = true)]
    public interface IPsdzFa : IPsdzStandardFa
    {
        string AsXml { get; }

#if OLD_PSDZ_FA
#warning OLD_PSDZ_FA activated. Do not use for release builds.
        string Vin { get; }
#endif
    }
}
