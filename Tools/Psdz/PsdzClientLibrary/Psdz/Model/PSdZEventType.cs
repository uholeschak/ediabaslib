using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum PSdZEventType
    {
        [EnumMember(Value = "PROGRESS")]
        PROGRESS,
        [EnumMember(Value = "TRANSACTION")]
        TRANSACTION,
        [EnumMember(Value = "DIAGSERVICE")]
        DIAGSERVICE,
        [EnumMember(Value = "TAL_EXECUTION_FINISHED")]
        TAL_EXECUTION_FINISHED,
        [EnumMember(Value = "AEP")]
        AEP,
        [EnumMember(Value = "PDXIMPORT")]
        PDXIMPORT,
        [EnumMember(Value = "WARN")]
        WARN,
        [EnumMember(Value = "TAL_GENERATION_WARN")]
        TAL_GENERATION_WARN,
        [EnumMember(Value = "THROWABLE")]
        THROWABLE
    }
}