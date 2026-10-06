using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz.Model.Swt;
using PsdzClient.Programming;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class FscCertificateStateEnumMapper : ProgrammingEnumMapperBase<PsdzFscCertificateState, FscCertificateState>
    {
        protected override IDictionary<PsdzFscCertificateState, FscCertificateState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
