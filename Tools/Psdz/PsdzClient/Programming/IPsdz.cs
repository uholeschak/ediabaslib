using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Psdz;

namespace PsdzClient.Programming
{
    public interface IPsdz : IPsdzService, IPsdzInfo
    {
        IPsdzObjectBuilder ObjectBuilder { get; }

        void AddPsdzEventListener(IPsdzEventListener psdzEventListener);

        void AddPsdzProgressListener(IPsdzProgressListener progressListener);

        void RemovePsdzEventListener(IPsdzEventListener psdzEventListener);

        void RemovePsdzProgressListener(IPsdzProgressListener progressListener);
    }
}
