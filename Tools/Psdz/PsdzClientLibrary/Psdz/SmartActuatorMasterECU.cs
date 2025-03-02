using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public class SmartActuatorMasterECU : ECU, ISmartActuatorMasterEcu, IEcuObj
    {
        [XmlIgnore]
        public IStandardSvk SmacMasterSVK { get; set; }

        public IList<ISmartActuatorEcu> SmartActuators { get; set; }

        public SmartActuatorMasterECU(ECU ecu)
            : base(ecu)
        {
            SmartActuators = new List<ISmartActuatorEcu>();
        }
    }
}