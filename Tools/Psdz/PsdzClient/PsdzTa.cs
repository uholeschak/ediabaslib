using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzTa : PsdzTalElement, IPsdzTa, IPsdzTalElement
    {
        // Token: 0x1700009F RID: 159
        // (get) Token: 0x060001C1 RID: 449 RVA: 0x00002758 File Offset: 0x00000958
        // (set) Token: 0x060001C2 RID: 450 RVA: 0x00002760 File Offset: 0x00000960
        [DataMember]
        public IPsdzSgbmId SgbmId { get; set; }
    }
}
