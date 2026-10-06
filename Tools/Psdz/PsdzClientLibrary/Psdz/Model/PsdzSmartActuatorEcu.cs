using BMW.Rheingold.Psdz.Model.Ecu;
using System.Runtime.Serialization;
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
    public class PsdzSmartActuatorEcu : PsdzEcu
    {
        [DataMember]
        public string SmacID { get; set; }

        [DataMember]
        public IPsdzDiagAddress SmacMasterDiagAddress { get; set; }

        public PsdzSmartActuatorEcu(PsdzEcu ecu)
            : base(ecu)
        {
        }
    }
}