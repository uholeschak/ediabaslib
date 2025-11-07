using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz;

namespace BMW.Rheingold.Psdz
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