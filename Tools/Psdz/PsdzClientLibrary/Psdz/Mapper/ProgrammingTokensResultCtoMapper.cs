using System.Linq;

namespace BMW.Rheingold.Psdz
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