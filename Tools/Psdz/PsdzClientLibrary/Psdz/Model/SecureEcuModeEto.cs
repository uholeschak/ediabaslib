using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum SecureEcuModeEto
    {
        [EnumMember(Value = "PLANT")]
        PLANT,
        [EnumMember(Value = "FIELD")]
        FIELD,
        [EnumMember(Value = "ENGINEERING")]
        ENGINEERING
    }
}