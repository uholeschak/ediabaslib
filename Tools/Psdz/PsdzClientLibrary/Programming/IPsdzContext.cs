using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IPsdzContext
    {
        string IstufeCurrent { get; }

        string IstufeLast { get; }

        string IstufeShipment { get; }

        string LatestPossibleIstufeTarget { get; }

        string ProjectName { get; }

        string VehicleInfo { get; }

        IVehicleProfileChecksum VpcFromVcm { get; }

        IPsdzTal Tal { get; set; }

        bool? IsSoftwareUpToDate(int diagnosticAddress);

        string GetBaseVariant(int diagnosticAddress);

        IEnumerable<ISgbmIdChange> GetDifferentSgbmIds(int diagnosticAddress);
    }
}
