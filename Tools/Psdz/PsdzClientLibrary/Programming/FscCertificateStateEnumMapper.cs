using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    internal sealed class FscCertificateStateEnumMapper : ProgrammingEnumMapperBase<PsdzFscCertificateState, FscCertificateState>
    {
        protected override IDictionary<PsdzFscCertificateState, FscCertificateState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
