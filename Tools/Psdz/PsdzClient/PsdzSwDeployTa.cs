using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public enum PsdzProtocol
    {
        KWP2000,
        UDS,
        HTTP,
        MIRROR
    }

    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzSwDeployTa : PsdzTa
    {
        // Token: 0x170000C0 RID: 192
        // (get) Token: 0x0600020B RID: 523 RVA: 0x00002880 File Offset: 0x00000A80
        // (set) Token: 0x0600020C RID: 524 RVA: 0x00002888 File Offset: 0x00000A88
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        // Token: 0x170000C1 RID: 193
        // (get) Token: 0x0600020D RID: 525 RVA: 0x00002891 File Offset: 0x00000A91
        // (set) Token: 0x0600020E RID: 526 RVA: 0x00002899 File Offset: 0x00000A99
        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
