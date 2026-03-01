using System.Runtime.Serialization;

namespace PsdzClient.Psdz.Model
{
    public enum ResetTypeEto
    {
        [EnumMember(Value = "ECU_RESET")]
        ECU_RESET,
        [EnumMember(Value = "ECU_AND_ETHERNET_SWITCH_RESET")]
        ECU_AND_ETHERNET_SWITCH_RESET,
        [EnumMember(Value = "ETHERNET_SWITCH_RESET")]
        ETHERNET_SWITCH_RESET
    }
}