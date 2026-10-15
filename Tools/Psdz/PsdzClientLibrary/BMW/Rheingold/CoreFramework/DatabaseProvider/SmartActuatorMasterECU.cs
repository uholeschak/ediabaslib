using System.Collections.Generic;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class SmartActuatorMasterECU : ECU, ISmartActuatorMasterEcu, IEcuObj
    {
        [XmlIgnore]
        public IStandardSvk SmacMasterSVK { get; set; }

        [XmlIgnore]
        public IList<ISmartActuatorEcu> SmartActuators { get; set; }

        public SmartActuatorMasterECU(ECU ecu) : base(ecu)
        {
            SmartActuators = new List<ISmartActuatorEcu>();
        }

        public SmartActuatorMasterECU()
        {
        }
    }
}