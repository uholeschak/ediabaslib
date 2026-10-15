using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum StatusRequestFeatureTypeEto
    {
        [EnumMember(Value = "ALL_FEATURES")]
        ALL_FEATURES,
        [EnumMember(Value = "SYSTEM_FEATURES")]
        SYSTEM_FEATURES,
        [EnumMember(Value = "APPLICATION_FEATURES")]
        APPLICATION_FEATURES
    }
}