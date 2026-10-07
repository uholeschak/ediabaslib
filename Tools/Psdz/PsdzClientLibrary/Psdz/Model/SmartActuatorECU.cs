using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public class SmartActuatorECU : ECU, ISmartActuatorEcu, IEcuObj
    {
        [XmlIgnore]
        public int? SmacMasterDiagAddressAsInt { get; set; }

        [XmlIgnore]
        public string SmacID { get; set; }

        public SmartActuatorECU(ECU ecu)
            : base(ecu)
        {
        }

        public SmartActuatorECU()
        {
        }
    }
}