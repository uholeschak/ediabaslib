using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzEcu
    {
        string BaseVariant { get; }

        string BnTnName { get; }

        IEnumerable<PsdzBus> BusConnections { get; }

        PsdzBus DiagnosticBus { get; }

        IPsdzEcuDetailInfo EcuDetailInfo { get; }

        IPsdzEcuStatusInfo EcuStatusInfo { get; }

        string EcuVariant { get; }

        IPsdzDiagAddress GatewayDiagAddr { get; }

        IPsdzEcuIdentifier PrimaryKey { get; }

        string SerialNumber { get; }

        IPsdzStandardSvk StandardSvk { get; }

        IPsdzEcuPdxInfo PsdzEcuPdxInfo { get; }

        bool IsSmartActuator { get; }
    }
}
