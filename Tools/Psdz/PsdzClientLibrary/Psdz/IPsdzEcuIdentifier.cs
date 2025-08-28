using System;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public interface IPsdzEcuIdentifier : IComparable<IPsdzEcuIdentifier>
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }

        IPsdzDiagAddress DiagnosisAddress { get; }
    }
}
