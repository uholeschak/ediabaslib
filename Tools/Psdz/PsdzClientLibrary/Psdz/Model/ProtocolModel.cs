using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum ProtocolModel
    {
        [EnumMember(Value = "KWP2000")]
        KWP2000,
        [EnumMember(Value = "UDS")]
        UDS,
        [EnumMember(Value = "HTTP")]
        HTTP,
        [EnumMember(Value = "MIRROR")]
        MIRROR
    }
}