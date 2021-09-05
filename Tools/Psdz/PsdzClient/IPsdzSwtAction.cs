using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzSwtAction
    {
        // Token: 0x170000E5 RID: 229
        // (get) Token: 0x06000255 RID: 597
        IEnumerable<IPsdzSwtEcu> SwtEcus { get; }
    }
}
