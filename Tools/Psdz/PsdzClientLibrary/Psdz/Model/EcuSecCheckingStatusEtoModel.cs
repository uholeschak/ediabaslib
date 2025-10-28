using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum EcuSecCheckingStatusEtoModel
    {
        [EnumMember(Value = "OK")]
        OK,
        [EnumMember(Value = "UNCHECKED")]
        UNCHECKED,
        [EnumMember(Value = "MALFORMED")]
        MALFORMED,
        [EnumMember(Value = "EMPTY")]
        EMPTY,
        [EnumMember(Value = "INCOMPLETE")]
        INCOMPLETE,
        [EnumMember(Value = "SECURITY_ERROR")]
        SECURITY_ERROR,
        [EnumMember(Value = "WRONG_VIN17")]
        WRONG_VIN17,
        [EnumMember(Value = "CHECK_STILL_RUNNING")]
        CHECK_STILL_RUNNING,
        [EnumMember(Value = "ISSUER_CERT_ERROR")]
        ISSUER_CERT_ERROR,
        [EnumMember(Value = "WRONG_ECU_UID")]
        WRONG_ECU_UID,
        [EnumMember(Value = "DECRYPTION_ERROR")]
        DECRYPTION_ERROR,
        [EnumMember(Value = "OWN_CERT_NOT_PRESENT")]
        OWN_CERT_NOT_PRESENT,
        [EnumMember(Value = "OUTDATED")]
        OUTDATED,
        [EnumMember(Value = "OTHER")]
        OTHER,
        [EnumMember(Value = "UNDEFINED")]
        UNDEFINED,
        [EnumMember(Value = "NOT_USED")]
        NOT_USED,
        [EnumMember(Value = "KEY_ERROR")]
        KEY_ERROR
    }
}