using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum KdsActionIdEto
    {
        [EnumMember(Value = "TRIGGER_FREE_PAIRING")]
        TRIGGER_FREE_PAIRING,
        [EnumMember(Value = "TRIGGER_INDIVIDUALIZATION")]
        TRIGGER_INDIVIDUALIZATION,
        [EnumMember(Value = "TRIGGER_VERIFICATION")]
        TRIGGER_VERIFICATION,
        [EnumMember(Value = "REPAIR_OR_CLEAR_DATA")]
        REPAIR_OR_CLEAR_DATA,
        [EnumMember(Value = "LOCK_ECU")]
        LOCK_ECU,
        [EnumMember(Value = "SET_OPERATION_MODE")]
        SET_OPERATION_MODE,
        [EnumMember(Value = "CHECK_PARING_CONSISTENCY")]
        CHECK_PARING_CONSISTENCY,
        [EnumMember(Value = "TEST_SIGNATURE")]
        TEST_SIGNATURE,
        [EnumMember(Value = "SHOW_REACTION")]
        SHOW_REACTION,
        [EnumMember(Value = "CUT_COMMUNICATION")]
        CUT_COMMUNICATION
    }
}