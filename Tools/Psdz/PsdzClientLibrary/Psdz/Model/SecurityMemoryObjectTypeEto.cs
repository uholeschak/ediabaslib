using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum SecurityMemoryObjectTypeEto
    {
        [EnumMember(Value = "CERTIFICATE")]
        CERTIFICATE,
        [EnumMember(Value = "BINDING")]
        BINDING,
        [EnumMember(Value = "OTHER_BINDING")]
        OTHER_BINDING,
        [EnumMember(Value = "ONLINE_CERTIFICATES_ECU")]
        ONLINE_CERTIFICATES_ECU,
        [EnumMember(Value = "ONLINE_BINDINGS_ECU")]
        ONLINE_BINDINGS_ECU,
        [EnumMember(Value = "KEYLIST")]
        KEYLIST
    }
}