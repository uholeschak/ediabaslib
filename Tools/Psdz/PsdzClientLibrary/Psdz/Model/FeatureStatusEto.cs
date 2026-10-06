using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public enum FeatureStatusEto
    {
        [EnumMember(Value = "INITIAL_DISABLED")]
        INITIAL_DISABLED,
        [EnumMember(Value = "ENABLED")]
        ENABLED,
        [EnumMember(Value = "DISABLED")]
        DISABLED,
        [EnumMember(Value = "EXPIRED")]
        EXPIRED,
        [EnumMember(Value = "INVALID")]
        INVALID
    }
}