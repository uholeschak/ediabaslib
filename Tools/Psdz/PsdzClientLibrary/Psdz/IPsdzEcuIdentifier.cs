using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public interface IPsdzEcuIdentifier : IComparable<IPsdzEcuIdentifier>
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }

        IPsdzDiagAddress DiagnosisAddress { get; }
    }
}
