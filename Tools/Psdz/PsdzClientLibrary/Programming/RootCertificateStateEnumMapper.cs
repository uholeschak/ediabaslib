using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    internal sealed class RootCertificateStateEnumMapper : ProgrammingEnumMapperBase<PsdzRootCertificateState, RootCertificateState>
    {
        protected override IDictionary<PsdzRootCertificateState, RootCertificateState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
