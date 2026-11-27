using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.SmartMaintenance
{
    [DataContract]
    public enum LinkLocation
    {
        [EnumMember]
        EMBEDDED,
        [EnumMember]
        ATTACHED
    }
}
