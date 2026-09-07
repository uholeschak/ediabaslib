using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class EcuObj : IEcuObj
    {
        public XEP_ECUCLIQUES XepEcuClique { get; internal set; }

        public XEP_ECUVARIANTS XepEcuVariant { get; internal set; }
        public string EcuGroup { get; internal set; }
        public string EcuRep { get; internal set; }
        public string BaseVariant { get; internal set; }
        public string BnTnName { get; internal set; }
        public IList<Bus> BusConnections => GetBusConnections();
        public IList<IBusObject> BusCons { get; internal set; }
        public IList<string> BusConnectionsAsString => GetBusConnectionsAsString();
        public Bus DiagnosticBus => GetDiagnosticBus();
        public IBusObject DiagBus { get; internal set; }
        public IEcuDetailInfo EcuDetailInfo { get; internal set; }
        public IEcuIdentifier EcuIdentifier { get; internal set; }
        public IEcuStatusInfo EcuStatusInfo { get; internal set; }
        public string EcuVariant { get; internal set; }
        public int? GatewayDiagAddrAsInt { get; internal set; }
        public string SerialNumber { get; internal set; }
        public IStandardSvk StandardSvk { get; internal set; }
        public string OrderNumber { get; set; }
        public IEcuPdxInfo EcuPdxInfo { get; internal set; }
        public bool IsSmartActuator { get; internal set; }

        private IList<Bus> GetBusConnections()
        {
            return BusCons?.Select((IBusObject x) => x.ConvertToBus()).ToList();
        }

        private IList<string> GetBusConnectionsAsString()
        {
            return BusCons?.Select((IBusObject x) => x.ToString()).ToList();
        }

        private Bus GetDiagnosticBus()
        {
            if (DiagBus != null)
            {
                return DiagBus.ConvertToBus();
            }

            return Bus.Unknown;
        }
    }
}