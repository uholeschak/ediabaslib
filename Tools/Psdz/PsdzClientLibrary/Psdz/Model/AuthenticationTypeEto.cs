using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum AuthenticationTypeEto
    {
        [EnumMember(Value = "SSL")]
        SSL,
        [EnumMember(Value = "BASIC")]
        BASIC,
        [EnumMember(Value = "BEARER")]
        BEARER,
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN
    }
}