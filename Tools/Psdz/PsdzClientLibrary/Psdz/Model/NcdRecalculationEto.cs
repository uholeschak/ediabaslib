using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum NcdRecalculationEto
    {
        [EnumMember(Value = "FORCE")]
        FORCE,
        [EnumMember(Value = "ALLOW")]
        ALLOW
    }
}