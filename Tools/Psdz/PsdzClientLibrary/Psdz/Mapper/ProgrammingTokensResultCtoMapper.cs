using System.Linq;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement.ProgrammingTokensResultCto;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ProgrammingTokensResultCtoMapper
    {
        internal static IPsdzProgrammingTokensResultCto Map(ProgrammingTokensResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzProgrammingTokensResultCto
            {
                Failures = model.Failures?.Select(EcuFailureResponseCtoMapper.MapCto).ToList(),
                Tokens = model.Tokens?.Select(ProgrammingTokenCtoMapper.Map).ToList()
            };
        }
    }
}