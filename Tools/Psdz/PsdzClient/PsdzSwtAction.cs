using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzSwtEcu))]
    public class PsdzSwtAction : IPsdzSwtAction
    {
        // Token: 0x170000F7 RID: 247
        // (get) Token: 0x06000267 RID: 615 RVA: 0x00002A7E File Offset: 0x00000C7E
        // (set) Token: 0x06000268 RID: 616 RVA: 0x00002A86 File Offset: 0x00000C86
        [DataMember]
        public IEnumerable<IPsdzSwtEcu> SwtEcus { get; set; }
    }
}
