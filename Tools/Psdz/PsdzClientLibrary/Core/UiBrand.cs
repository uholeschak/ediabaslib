using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    [DataContract]
    public enum UiBrand
    {
        [DataMember]
        BMWBMWiMINI,
        [DataMember]
        BMWBMWi,
        [DataMember]
        BMWiMINI,
        [DataMember]
        BMWMINI,
        [DataMember]
        BMWPKW,
        [DataMember]
        Mini,
        [DataMember]
        RollsRoyce,
        [DataMember]
        BMWMotorrad,
        [DataMember]
        BMWi,
        [DataMember]
        TOYOTA,
        [DataMember]
        Unknown
    }
}