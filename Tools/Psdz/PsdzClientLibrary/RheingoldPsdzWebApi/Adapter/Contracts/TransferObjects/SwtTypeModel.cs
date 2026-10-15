using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum SwtTypeModel
    {
        [EnumMember(Value = "Full")]
        Full,
        [EnumMember(Value = "Light")]
        Light,
        [EnumMember(Value = "PreEnabFull")]
        PreEnabFull,
        [EnumMember(Value = "PreEnabLight")]
        PreEnabLight,
        [EnumMember(Value = "Short")]
        Short,
        [EnumMember(Value = "Unknown")]
        Unknown
    }
}