using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum SecurityMemoryObjectSourceEto
    {
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN,
        [EnumMember(Value = "VEHICLE")]
        VEHICLE,
        [EnumMember(Value = "CBB")]
        CBB
    }
}