using System;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzEcuContextInfo
    {
        IPsdzEcuIdentifier EcuId { get; }

        DateTime? LastProgrammingDate { get; }

        DateTime ManufacturingDate { get; }

        int PerformedFlashCycles { get; }

        int ProgramCounter { get; }

        int RemainingFlashCycles { get; }
    }
}
