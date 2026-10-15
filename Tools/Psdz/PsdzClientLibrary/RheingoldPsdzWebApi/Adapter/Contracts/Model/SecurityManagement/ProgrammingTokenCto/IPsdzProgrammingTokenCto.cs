using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement.ProgrammingTokenCto
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
