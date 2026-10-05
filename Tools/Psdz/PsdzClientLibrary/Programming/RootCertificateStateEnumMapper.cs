using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class RootCertificateStateEnumMapper : ProgrammingEnumMapperBase<PsdzRootCertificateState, RootCertificateState>
    {
        protected override IDictionary<PsdzRootCertificateState, RootCertificateState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
