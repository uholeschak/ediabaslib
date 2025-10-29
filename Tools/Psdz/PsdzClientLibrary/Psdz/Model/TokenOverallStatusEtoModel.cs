using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum TokenOverallStatusEtoModel
    {
        [EnumMember(Value = "OK")]
        OK,
        [EnumMember(Value = "ERROR")]
        ERROR,
        [EnumMember(Value = "MALFORMED")]
        MALFORMED,
        [EnumMember(Value = "UNKNOWN_VERSION")]
        UNKNOWN_VERSION,
        [EnumMember(Value = "EMPTY")]
        EMPTY,
        [EnumMember(Value = "CONFLICT")]
        CONFLICT,
        [EnumMember(Value = "DETAILED")]
        DETAILED,
        [EnumMember(Value = "VIN_MALFORMED")]
        VIN_MALFORMED,
        [EnumMember(Value = "WRONG_FORMAT")]
        WRONG_FORMAT,
        [EnumMember(Value = "DNS_NOT_AVAILABLE")]
        DNS_NOT_AVAILABLE,
        [EnumMember(Value = "UNKNOWN_HOSTNAME")]
        UNKNOWN_HOSTNAME,
        [EnumMember(Value = "INVALID_HOSTNAME")]
        INVALID_HOSTNAME,
        [EnumMember(Value = "HOSTNAME_NOT_CORRECT")]
        HOSTNAME_NOT_CORRECT,
        [EnumMember(Value = "AUTH_FAILED")]
        AUTH_FAILED,
        [EnumMember(Value = "NOT_IN_WHITELIST")]
        NOT_IN_WHITELIST,
        [EnumMember(Value = "TOKENPACKAGE_REBUILD_ERROR")]
        TOKENPACKAGE_REBUILD_ERROR,
        [EnumMember(Value = "FEATURE_SET_REBUILD_ERROR")]
        FEATURE_SET_REBUILD_ERROR,
        [EnumMember(Value = "NO_RIGHTS_ASSIGNED")]
        NO_RIGHTS_ASSIGNED,
        [EnumMember(Value = "LINK_TO_ID_UNKNOWN")]
        LINK_TO_ID_UNKNOWN,
        [EnumMember(Value = "OTHER_ERROR")]
        OTHER_ERROR,
        [EnumMember(Value = "UNDEFINED")]
        UNDEFINED,
        [EnumMember(Value = "NULL")]
        NULL
    }
}