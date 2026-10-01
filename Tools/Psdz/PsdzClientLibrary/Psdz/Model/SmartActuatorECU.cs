using PsdzClient.Core;
using PsdzClient.Programming;
using System.Xml.Serialization;

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