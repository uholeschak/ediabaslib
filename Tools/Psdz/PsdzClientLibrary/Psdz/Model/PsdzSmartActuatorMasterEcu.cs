using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    [DataContract]
    [KnownType(typeof(PsdzEcuDetailInfo))]
    [KnownType(typeof(PsdzEcuStatusInfo))]
    [KnownType(typeof(PsdzDiagAddress))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzStandardSvk))]
    [KnownType(typeof(PsdzEcuPdxInfo))]
    public class PsdzSmartActuatorMasterEcu : PsdzEcu
    {
        [DataMember]
        public IPsdzStandardSvk SmacMasterSVK { get; set; }

        [DataMember]
        public IEnumerable<IPsdzEcu> SmartActuatorEcus { get; set; }

        public PsdzSmartActuatorMasterEcu(PsdzEcu ecu)
            : base(ecu)
        {
            SmartActuatorEcus = new List<PsdzSmartActuatorEcu>();
        }
    }
}