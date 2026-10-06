using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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