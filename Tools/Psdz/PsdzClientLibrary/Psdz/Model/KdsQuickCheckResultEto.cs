using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum KdsQuickCheckResultEto
    {
        [EnumMember(Value = "MASTER_OK_CLIENT_INVALID")]
        MASTER_OK_CLIENT_INVALID,
        [EnumMember(Value = "MASTER_INVALID_CLIENT_OK")]
        MASTER_INVALID_CLIENT_OK,
        [EnumMember(Value = "MASTER_INVALID_CLIENT_INVALID")]
        MASTER_INVALID_CLIENT_INVALID,
        [EnumMember(Value = "MASTER_UNKNOWN_CLIENT_NOT_PAIRED")]
        MASTER_UNKNOWN_CLIENT_NOT_PAIRED,
        [EnumMember(Value = "MASTER_NOT_PAIRED_CLIENT_UNKNOWN")]
        MASTER_NOT_PAIRED_CLIENT_UNKNOWN,
        [EnumMember(Value = "MASTER_UNKNOWN_CLIENT_TIMEOUT")]
        MASTER_UNKNOWN_CLIENT_TIMEOUT,
        [EnumMember(Value = "MASTER_ERROR_CLIENT_ERROR")]
        MASTER_ERROR_CLIENT_ERROR
    }
}