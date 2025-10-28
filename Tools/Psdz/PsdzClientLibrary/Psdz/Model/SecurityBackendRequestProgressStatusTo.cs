using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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