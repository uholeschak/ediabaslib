using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using PsdzClientLibrary;

namespace PsdzClient.Programming
{
    internal class EcuObj : IEcuObj
    {
        [PreserveSource(Hint = "public XEP_ECUCLIQUES", Removed = true)]
        public PlaceholderType XepEcuClique { get; internal set; }

        [PreserveSource(Hint = "public XEP_ECUVARIANTS", Removed = true)]
        public PlaceholderType XepEcuVariant { get; internal set; }

        public string EcuGroup { get; internal set; }

        public string EcuRep { get; internal set; }

        public string BaseVariant { get; internal set; }

        public string BnTnName { get; internal set; }

        public IList<Bus> BusConnections => BusCons?.Select((IBusObject x) => x.ConvertToBus()).ToList();

        public IList<IBusObject> BusCons { get; internal set; }

        public IList<string> BusConnectionsAsString => BusCons?.Select((IBusObject x) => x.ToString()).ToList();

        public Bus DiagnosticBus
        {
            get
            {
                if (DiagBus != null)
                {
                    return DiagBus.ConvertToBus();
                }
                return Bus.Unknown;
            }
        }

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
    }
}
