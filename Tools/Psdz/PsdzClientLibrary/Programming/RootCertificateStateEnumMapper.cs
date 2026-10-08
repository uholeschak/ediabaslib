using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

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
