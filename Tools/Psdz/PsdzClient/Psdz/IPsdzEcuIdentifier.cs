using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzEcuIdentifier : IComparable<IPsdzEcuIdentifier>
    {
        // Token: 0x170002DF RID: 735
        // (get) Token: 0x060005DC RID: 1500
        string BaseVariant { get; }

        // Token: 0x170002E0 RID: 736
        // (get) Token: 0x060005DD RID: 1501
        int DiagAddrAsInt { get; }

        // Token: 0x170002E1 RID: 737
        // (get) Token: 0x060005DE RID: 1502
        IPsdzDiagAddress DiagnosisAddress { get; }
    }
}
