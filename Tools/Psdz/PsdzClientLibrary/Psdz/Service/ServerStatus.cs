using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum ServerStatus
    {
        [EnumMember(Value = "STOPPED")]
        STOPPED,
        [EnumMember(Value = "RUNNING")]
        RUNNING
    }
}