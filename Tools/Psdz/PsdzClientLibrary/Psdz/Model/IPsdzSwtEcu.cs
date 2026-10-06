using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt
{
    public interface IPsdzSwtEcu
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        PsdzRootCertificateState RootCertState { get; }

        PsdzSoftwareSigState SoftwareSigState { get; }

        IEnumerable<IPsdzSwtApplication> SwtApplications { get; }

        string Vin { get; }
    }
}
