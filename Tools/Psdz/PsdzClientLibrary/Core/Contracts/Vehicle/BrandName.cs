using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    public enum BrandName
    {
        [EnumMember]
        [XmlEnum("BMW PKW")]
        BMWPKW,
        [EnumMember]
        [XmlEnum("MINI PKW")]
        MINIPKW,
        [EnumMember]
        [XmlEnum("ROLLS-ROYCE PKW")]
        ROLLSROYCEPKW,
        [EnumMember]
        [XmlEnum("BMW MOTORRAD")]
        BMWMOTORRAD,
        [EnumMember]
        [XmlEnum("BMW M GmbH PKW")]
        BMWMGmbHPKW,
        [EnumMember]
        [XmlEnum("BMW USA PKW")]
        BMWUSAPKW,
        [EnumMember]
        [XmlEnum("BMW i")]
        BMWi,
        [EnumMember]
        TOYOTA
    }
}