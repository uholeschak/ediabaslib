using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum ActionValues
    {
        [EnumMember(Value = "EMPTY")]
        EMPTY,
        [EnumMember(Value = "ALLOWED_TO_BE_TREATED")]
        ALLOWED_TO_BE_TREATED,
        [EnumMember(Value = "MUST_BE_TREATED")]
        MUST_BE_TREATED,
        [EnumMember(Value = "MUST_NOT_BE_TREATED")]
        MUST_NOT_BE_TREATED,
        [EnumMember(Value = "ONLY_TO_BE_TREATED_AND_BLOCK_CATEGORY_IN_ALL_ECU")]
        ONLY_TO_BE_TREATED_AND_BLOCK_CATEGORY_IN_ALL_ECU
    }
}