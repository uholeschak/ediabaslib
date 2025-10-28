using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum CalculationStrategyEto
    {
        [EnumMember(Value = "BEFORE_CERTIFICATES")]
        BEFORE_CERTIFICATES,
        [EnumMember(Value = "AFTER_CERTIFICATES")]
        AFTER_CERTIFICATES,
        [EnumMember(Value = "END_OF_LINE")]
        END_OF_LINE,
        [EnumMember(Value = "UPDATE")]
        UPDATE,
        [EnumMember(Value = "UPDATE_WITHOUT_DELETE")]
        UPDATE_WITHOUT_DELETE
    }
}