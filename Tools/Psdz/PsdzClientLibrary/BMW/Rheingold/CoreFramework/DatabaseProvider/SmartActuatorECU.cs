using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
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