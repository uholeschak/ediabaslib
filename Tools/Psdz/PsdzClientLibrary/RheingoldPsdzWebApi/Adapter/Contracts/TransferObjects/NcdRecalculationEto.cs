using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum NcdRecalculationEto
    {
        [EnumMember(Value = "FORCE")]
        FORCE,
        [EnumMember(Value = "ALLOW")]
        ALLOW
    }
}