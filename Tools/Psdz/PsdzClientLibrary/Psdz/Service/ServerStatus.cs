using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum ServerStatus
    {
        [EnumMember(Value = "STOPPED")]
        STOPPED,
        [EnumMember(Value = "RUNNING")]
        RUNNING
    }
}