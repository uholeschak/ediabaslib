using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzTargetSelector
    {
        // Token: 0x17000077 RID: 119
        // (get) Token: 0x0600016A RID: 362
        string Baureihenverbund { get; }

        // Token: 0x17000078 RID: 120
        // (get) Token: 0x0600016B RID: 363
        bool IsDirect { get; }

        // Token: 0x17000079 RID: 121
        // (get) Token: 0x0600016C RID: 364
        string Project { get; }

        // Token: 0x1700007A RID: 122
        // (get) Token: 0x0600016D RID: 365
        string VehicleInfo { get; }
    }
}
