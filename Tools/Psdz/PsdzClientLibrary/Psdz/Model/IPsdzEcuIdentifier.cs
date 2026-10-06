using System;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzEcuIdentifier : IComparable<IPsdzEcuIdentifier>
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }

        IPsdzDiagAddress DiagnosisAddress { get; }
    }
}
