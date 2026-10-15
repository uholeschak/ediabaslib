using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.Services;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IPsdz : IPsdzService, IPsdzInfo
    {
        IPsdzObjectBuilder ObjectBuilder { get; }

        IProgrammingTokenService ProgrammingTokenService { get; }

        void AddPsdzEventListener(IPsdzEventListener psdzEventListener);
        void AddPsdzProgressListener(IPsdzProgressListener progressListener);
        void RemovePsdzEventListener(IPsdzEventListener psdzEventListener);
        void RemovePsdzProgressListener(IPsdzProgressListener progressListener);
    }
}