using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum NcdStatusEto
    {
        [EnumMember(Value = "SIGNED")]
        SIGNED,
        [EnumMember(Value = "UNSIGNED")]
        UNSIGNED,
        [EnumMember(Value = "NO_NCD")]
        NO_NCD,
        [EnumMember(Value = "CPS_INVALID")]
        CPS_INVALID
    }
}