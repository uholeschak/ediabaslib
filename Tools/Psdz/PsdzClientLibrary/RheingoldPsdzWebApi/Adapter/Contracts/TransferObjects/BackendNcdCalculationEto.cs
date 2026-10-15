using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum BackendNcdCalculationEto
    {
        [EnumMember(Value = "FORCE")]
        FORCE,
        [EnumMember(Value = "ALLOW")]
        ALLOW,
        [EnumMember(Value = "MUST_NOT")]
        MUST_NOT
    }
}