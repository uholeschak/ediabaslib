using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement.ProgrammingTokenCto;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement.ProgrammingTokensResultCto
{
    public interface IPsdzProgrammingTokensResultCto
    {
        IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; }

        IEnumerable<IPsdzProgrammingTokenCto> Tokens { get; }
    }
}