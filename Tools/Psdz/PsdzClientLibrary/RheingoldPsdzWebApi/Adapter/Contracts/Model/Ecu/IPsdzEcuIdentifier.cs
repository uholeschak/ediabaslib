using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzEcuIdentifier : IComparable<IPsdzEcuIdentifier>
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }

        IPsdzDiagAddress DiagnosisAddress { get; }
    }
}
