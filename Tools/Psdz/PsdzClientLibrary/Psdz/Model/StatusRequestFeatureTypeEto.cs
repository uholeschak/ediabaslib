using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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