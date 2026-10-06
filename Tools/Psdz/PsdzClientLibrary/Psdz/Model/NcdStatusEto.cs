using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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