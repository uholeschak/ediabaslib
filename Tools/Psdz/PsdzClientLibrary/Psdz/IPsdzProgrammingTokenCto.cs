using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzProgrammingTokenCto
    {
        int TokenVersion { get; }

        IPsdzVin Vin { get; }

        IPsdzEcuIdentifier EcuIdentifier { get; }

        IPsdzEcuUidCto EcuUidCto { get; }

        IEnumerable<IPsdzSgbmId> ActiveSGBMIDs { get; }

        IEnumerable<IPsdzSgbmId> NewSGBMIDs { get; }

        byte[] ActiveSGBMIDsHash { get; }

        byte[] ValidityStartTime { get; }

        byte[] ValidityEndTime { get; }

        bool IsSigned { get; }

        byte[] ProgrammingTokenAsBytes { get; }
    }
}
