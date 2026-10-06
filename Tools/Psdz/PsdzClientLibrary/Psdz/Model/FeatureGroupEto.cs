using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum FeatureGroupEto
    {
        [EnumMember(Value = "PLANT_SYSTEM_FEATURES")]
        PLANT_SYSTEM_FEATURES,
        [EnumMember(Value = "VEHICLE_SYSTEM_FEATURES")]
        VEHICLE_SYSTEM_FEATURES,
        [EnumMember(Value = "CUSTOMER_FEATURES")]
        CUSTOMER_FEATURES
    }
}