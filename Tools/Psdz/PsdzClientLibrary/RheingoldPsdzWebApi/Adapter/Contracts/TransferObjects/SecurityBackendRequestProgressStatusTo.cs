using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum SecurityBackendRequestProgressStatusTo
    {
        [EnumMember(Value = "UNKNOWN_REQUEST_ID")]
        UNKNOWN_REQUEST_ID,
        [EnumMember(Value = "RUNNING")]
        RUNNING,
        [EnumMember(Value = "SUCCESS")]
        SUCCESS,
        [EnumMember(Value = "ERROR")]
        ERROR
    }
}