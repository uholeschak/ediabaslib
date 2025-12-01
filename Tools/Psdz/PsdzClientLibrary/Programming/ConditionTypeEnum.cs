using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ConditionTypeEnum
    {
        DAYS_AFTER_ACTIVATION,
        END_OF_CONDITIONS,
        EXPIRATION_DATE,
        KM_AFTER_ACTIVATION,
        LOCAL_RELATIVE_TIME,
        NUMBER_OF_DRIVING_CYCLES,
        NUMBER_OF_EXECUTIONS,
        SPEED_TRESHOLD,
        START_AND_END_ODOMETER_READING,
        TIME_PERIOD,
        UNLIMITED
    }
}