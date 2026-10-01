using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Swt
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
