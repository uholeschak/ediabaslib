using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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