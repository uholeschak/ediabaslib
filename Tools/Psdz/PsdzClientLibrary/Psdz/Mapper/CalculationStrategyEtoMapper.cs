using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    internal class CalculationStrategyEtoMapper : MapperBase<PsdzCalculationStrategyEtoEnum, CalculationStrategyEto>
    {
        protected override IDictionary<PsdzCalculationStrategyEtoEnum, CalculationStrategyEto> CreateMap()
        {
            return new Dictionary<PsdzCalculationStrategyEtoEnum, CalculationStrategyEto>
            {
                {
                    PsdzCalculationStrategyEtoEnum.AFTER_CERTIFICATES,
                    CalculationStrategyEto.AFTER_CERTIFICATES
                },
                {
                    PsdzCalculationStrategyEtoEnum.BEFORE_CERTIFICATES,
                    CalculationStrategyEto.BEFORE_CERTIFICATES
                },
                {
                    PsdzCalculationStrategyEtoEnum.END_OF_LINE,
                    CalculationStrategyEto.END_OF_LINE
                },
                {
                    PsdzCalculationStrategyEtoEnum.UPDATE,
                    CalculationStrategyEto.UPDATE
                },
                {
                    PsdzCalculationStrategyEtoEnum.UPDATE_WITHOUT_DELETE,
                    CalculationStrategyEto.UPDATE_WITHOUT_DELETE
                }
            };
        }
    }
}