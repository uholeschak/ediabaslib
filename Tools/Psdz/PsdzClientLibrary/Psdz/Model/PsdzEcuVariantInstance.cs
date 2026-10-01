using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [DataContract]
    [KnownType(typeof(PsdzOrderPart))]
    [KnownType(typeof(PsdzEcu))]
    [KnownType(typeof(PsdzSmartActuatorMasterEcu))]
    [KnownType(typeof(PsdzSmartActuatorEcu))]
    [KnownType(typeof(PsdzEcuVariantInstance))]
    public class PsdzEcuVariantInstance : PsdzLogisticPart, IPsdzEcuVariantInstance, IPsdzLogisticPart
    {
        [DataMember]
        public IPsdzOrderPart OrderablePart { get; set; }

        [DataMember]
        public IPsdzEcuVariantInstance[] CombinedWith { get; set; }

        [DataMember]
        public IPsdzEcu Ecu { get; set; }
    }
}