using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum SecurityCalculationDetailedStatusEto
    {
        [EnumMember(Value = "OK")]
        OK,
        [EnumMember(Value = "ERROR")]
        ERROR,
        [EnumMember(Value = "MALFORMED")]
        MALFORMED,
        [EnumMember(Value = "ECU_UID_FORMAT")]
        ECU_UID_FORMAT,
        [EnumMember(Value = "ECU_UID_MISSING")]
        ECU_UID_MISSING,
        [EnumMember(Value = "ECU_UID_MISMATCH")]
        ECU_UID_MISMATCH,
        [EnumMember(Value = "ECU_UID_DENIED")]
        ECU_UID_DENIED,
        [EnumMember(Value = "CNAME_MISSING")]
        CNAME_MISSING,
        [EnumMember(Value = "CNAME_DENIED")]
        CNAME_DENIED,
        [EnumMember(Value = "ECU_NAME_MISSING")]
        ECU_NAME_MISSING,
        [EnumMember(Value = "ECU_NAME_DENIED")]
        ECU_NAME_DENIED,
        [EnumMember(Value = "CERT_DENIED")]
        CERT_DENIED,
        [EnumMember(Value = "CA_ERROR")]
        CA_ERROR,
        [EnumMember(Value = "ECU_DENIED")]
        ECU_DENIED,
        [EnumMember(Value = "KEYID_UNKNOWN")]
        KEYID_UNKNOWN,
        [EnumMember(Value = "KEYID_DENIED")]
        KEYID_DENIED
    }
}