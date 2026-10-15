using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum KdsActionStatusEto
    {
        [EnumMember(Value = "SUCCESS")]
        SUCCESS,
        [EnumMember(Value = "IN_PROGRESS")]
        IN_PROGRESS,
        [EnumMember(Value = "PARTIAL")]
        PARTIAL,
        [EnumMember(Value = "ERROR")]
        ERROR,
        [EnumMember(Value = "FORBIDDEN")]
        FORBIDDEN,
        [EnumMember(Value = "TIMEOUT")]
        TIMEOUT
    }
}