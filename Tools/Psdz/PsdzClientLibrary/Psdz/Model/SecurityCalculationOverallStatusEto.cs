using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum SecurityCalculationOverallStatusEto
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
        [EnumMember(Value = "INVALID_FAT_REQUEST")]
        INVALID_FAT_REQUEST,
        [EnumMember(Value = "REQUEST_NOT_ON_CERT_STORE")]
        REQUEST_NOT_ON_CERT_STORE,
        [EnumMember(Value = "INVALID_ECU_TYPE_CERTS")]
        INVALID_ECU_TYPE_CERTS,
        [EnumMember(Value = "INVALID_SIGNATURE")]
        INVALID_SIGNATURE,
        [EnumMember(Value = "OTHER_ERROR")]
        OTHER_ERROR
    }
}