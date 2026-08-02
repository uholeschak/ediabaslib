using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.26539")]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public enum MeasurementPointPositions
    {
        BatteryPlus,
        BatteryMinus,
        CarBodyMinus,
        MotorGround,
        ECUGround,
        AdapterPin,
        UniversalAdapterPin,
        BatteryMountingPointPlus,
        OwnText
    }
}
