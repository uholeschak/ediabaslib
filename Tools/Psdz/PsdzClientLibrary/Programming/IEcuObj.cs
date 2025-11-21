using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using PsdzClient.Core;
using PsdzClient;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuObj
    {
        string EcuRep { get; }

        string EcuGroup { get; }

        string BaseVariant { get; }

        string BnTnName { get; }

        [Obsolete("use BusCons instead")]
        IList<Bus> BusConnections { get; }

        IList<IBusObject> BusCons { get; }

        IList<string> BusConnectionsAsString { get; }

        [Obsolete("use DiagBus instead")]
        Bus DiagnosticBus { get; }

        IBusObject DiagBus { get; }

        IEcuDetailInfo EcuDetailInfo { get; }

        IEcuIdentifier EcuIdentifier { get; }

        IEcuStatusInfo EcuStatusInfo { get; }

        string EcuVariant { get; }

        int? GatewayDiagAddrAsInt { get; }

        string SerialNumber { get; }

        IStandardSvk StandardSvk { get; }

        string OrderNumber { get; }

        IEcuPdxInfo EcuPdxInfo { get; }

        bool IsSmartActuator { get; }
    }
}