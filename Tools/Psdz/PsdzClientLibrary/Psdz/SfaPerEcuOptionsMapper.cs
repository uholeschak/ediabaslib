using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Mapper;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter;

namespace BMW.Rheingold.Programming.Common
{
    public class SfaPerEcuOptionsMapper
    {
        public static IPsdzSfaPerEcuOptions Map(ISfaPerEcuOptions input)
        {
            if (input != null)
            {
                return new PsdzSfaPerEcuOptions
                {
                    CategoryAction = TalFilterOptionMapper.Map(input.CategoryAction),
                    SfaWriteAction = TalFilterOptionMapper.Map(input.SfaWriteAction),
                    SfaDeleteAction = TalFilterOptionMapper.Map(input.SfaDeleteAction)
                };
            }
            return null;
        }
    }
}
