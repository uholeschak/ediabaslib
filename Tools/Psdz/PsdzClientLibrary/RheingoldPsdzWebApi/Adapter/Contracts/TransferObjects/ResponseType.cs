using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum ResponseType
    {
        [EnumMember(Value = "UNDEFINED")]
        UNDEFINED,
        [EnumMember(Value = "ERROR")]
        ERROR,
        [EnumMember(Value = "NEGATIVE")]
        NEGATIVE,
        [EnumMember(Value = "TIMEOUT")]
        TIMEOUT,
        [EnumMember(Value = "POS_RESPONSE_WITH_NEG_CONTENT")]
        POS_RESPONSE_WITH_NEG_CONTENT,
        [EnumMember(Value = "POSITIVE")]
        POSITIVE,
        [EnumMember(Value = "TIMING")]
        TIMING
    }
}