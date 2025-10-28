using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum ConditionTypeEto
    {
        [EnumMember(Value = "END_OF_CONDITIONS")]
        END_OF_CONDITIONS,
        [EnumMember(Value = "UNLIMITED")]
        UNLIMITED,
        [EnumMember(Value = "EXPIRATION_DATE")]
        EXPIRATION_DATE,
        [EnumMember(Value = "TIME_PERIOD")]
        TIME_PERIOD,
        [EnumMember(Value = "DAYS_AFTER_ACTIVATION")]
        DAYS_AFTER_ACTIVATION,
        [EnumMember(Value = "START_AND_END_ODOMETER_READING")]
        START_AND_END_ODOMETER_READING,
        [EnumMember(Value = "KM_AFTER_ACTIVATION")]
        KM_AFTER_ACTIVATION,
        [EnumMember(Value = "NUMBER_OF_EXECUTIONS")]
        NUMBER_OF_EXECUTIONS,
        [EnumMember(Value = "LOCAL_RELATIVE_TIME")]
        LOCAL_RELATIVE_TIME,
        [EnumMember(Value = "NUMBER_OF_DRIVING_CYCLES")]
        NUMBER_OF_DRIVING_CYCLES,
        [EnumMember(Value = "SPEED_TRESHOLD")]
        SPEED_TRESHOLD
    }
}