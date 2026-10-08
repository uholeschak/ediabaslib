using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
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
