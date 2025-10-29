using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum SfaLinkTypeEto
    {
        [EnumMember(Value = "VIN")]
        VIN,
        [EnumMember(Value = "ECU_UID")]
        ECU_UID,
        [EnumMember(Value = "VIN_ECU_UID")]
        VIN_ECU_UID
    }
}