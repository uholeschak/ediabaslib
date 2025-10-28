using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum ValidationStatusEto
    {
        [EnumMember(Value = "E_OK")]
        E_OK,
        [EnumMember(Value = "E_UNCHECKED")]
        E_UNCHECKED,
        [EnumMember(Value = "E_MALFORMED")]
        E_MALFORMED,
        [EnumMember(Value = "E_EMPTY")]
        E_EMPTY,
        [EnumMember(Value = "E_ERROR")]
        E_ERROR,
        [EnumMember(Value = "E_SECURITY_ERROR")]
        E_SECURITY_ERROR,
        [EnumMember(Value = "E_WRONG_LINKTOID")]
        E_WRONG_LINKTOID,
        [EnumMember(Value = "E_CHECK_RUNNING")]
        E_CHECK_RUNNING,
        [EnumMember(Value = "E_TIMESTAMP")]
        E_TIMESTAMP,
        [EnumMember(Value = "E_VERSION")]
        E_VERSION,
        [EnumMember(Value = "E_FEATUREID")]
        E_FEATUREID,
        [EnumMember(Value = "E_OTHER")]
        E_OTHER
    }
}