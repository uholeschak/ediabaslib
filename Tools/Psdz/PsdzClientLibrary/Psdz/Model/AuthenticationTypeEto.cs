using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum AuthenticationTypeEto
    {
        [EnumMember(Value = "SSL")]
        SSL,
        [EnumMember(Value = "BASIC")]
        BASIC,
        [EnumMember(Value = "BEARER")]
        BEARER,
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN
    }
}