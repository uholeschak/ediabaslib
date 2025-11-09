using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model.Comparer;
using PsdzClient.Programming;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [DataContract]
    [KnownType(typeof(PsdzEcuDetailInfo))]
    [KnownType(typeof(PsdzEcuStatusInfo))]
    [KnownType(typeof(PsdzDiagAddress))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzStandardSvk))]
    [KnownType(typeof(PsdzEcuPdxInfo))]
    public class PsdzEcu : IPsdzEcu
    {
        private static readonly IEqualityComparer<PsdzEcu> PsdzEcuComparerInstance = new PsdzEcuComparer();

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string BaseVariant { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string BnTnName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<PsdzBus> BusConnections { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBus DiagnosticBus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuDetailInfo EcuDetailInfo { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuStatusInfo EcuStatusInfo { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string EcuVariant { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzDiagAddress GatewayDiagAddr { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier PrimaryKey { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string SerialNumber { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzStandardSvk StandardSvk { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuPdxInfo PsdzEcuPdxInfo { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSmartActuator { get; set; }

        public PsdzEcu()
        {
        }

        public PsdzEcu(IPsdzEcu ecu)
        {
            BaseVariant = ecu.BaseVariant;
            BnTnName = ecu.BnTnName;
            BusConnections = ecu.BusConnections;
            DiagnosticBus = ecu.DiagnosticBus;
            EcuDetailInfo = ecu.EcuDetailInfo;
            EcuStatusInfo = ecu.EcuStatusInfo;
            EcuVariant = ecu.EcuVariant;
            GatewayDiagAddr = ecu.GatewayDiagAddr;
            PrimaryKey = ecu.PrimaryKey;
            SerialNumber = ecu.SerialNumber;
            StandardSvk = ecu.StandardSvk;
            PsdzEcuPdxInfo = ecu.PsdzEcuPdxInfo;
            IsSmartActuator = ecu.IsSmartActuator;
        }

        public override bool Equals(object obj)
        {
            return PsdzEcuComparerInstance.Equals(this, obj as PsdzEcu);
        }

        public override int GetHashCode()
        {
            return PsdzEcuComparerInstance.GetHashCode(this);
        }
    }
}
