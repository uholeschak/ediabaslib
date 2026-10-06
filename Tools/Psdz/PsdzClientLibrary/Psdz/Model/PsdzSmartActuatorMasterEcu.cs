using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

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