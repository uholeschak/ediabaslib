using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal class ConditionTypeEtoMapper : MapperBase<PsdzConditionTypeEtoEnum, ConditionTypeEto>
    {
        protected override IDictionary<PsdzConditionTypeEtoEnum, ConditionTypeEto> CreateMap()
        {
            return new Dictionary<PsdzConditionTypeEtoEnum, ConditionTypeEto>
            {
                {
                    PsdzConditionTypeEtoEnum.DAYS_AFTER_ACTIVATION,
                    ConditionTypeEto.DAYS_AFTER_ACTIVATION
                },
                {
                    PsdzConditionTypeEtoEnum.END_OF_CONDITIONS,
                    ConditionTypeEto.END_OF_CONDITIONS
                },
                {
                    PsdzConditionTypeEtoEnum.EXPIRATION_DATE,
                    ConditionTypeEto.EXPIRATION_DATE
                },
                {
                    PsdzConditionTypeEtoEnum.KM_AFTER_ACTIVATION,
                    ConditionTypeEto.KM_AFTER_ACTIVATION
                },
                {
                    PsdzConditionTypeEtoEnum.LOCAL_RELATIVE_TIME,
                    ConditionTypeEto.LOCAL_RELATIVE_TIME
                },
                {
                    PsdzConditionTypeEtoEnum.NUMBER_OF_DRIVING_CYCLES,
                    ConditionTypeEto.NUMBER_OF_DRIVING_CYCLES
                },
                {
                    PsdzConditionTypeEtoEnum.NUMBER_OF_EXECUTIONS,
                    ConditionTypeEto.NUMBER_OF_EXECUTIONS
                },
                {
                    PsdzConditionTypeEtoEnum.SPEED_TRESHOLD,
                    ConditionTypeEto.SPEED_TRESHOLD
                },
                {
                    PsdzConditionTypeEtoEnum.START_AND_END_ODOMETER_READING,
                    ConditionTypeEto.START_AND_END_ODOMETER_READING
                },
                {
                    PsdzConditionTypeEtoEnum.TIME_PERIOD,
                    ConditionTypeEto.TIME_PERIOD
                },
                {
                    PsdzConditionTypeEtoEnum.UNLIMITED,
                    ConditionTypeEto.UNLIMITED
                }
            };
        }
    }
}