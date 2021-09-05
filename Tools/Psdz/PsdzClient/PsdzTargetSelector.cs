using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzTargetSelector : IPsdzTargetSelector
    {
        // Token: 0x1700008F RID: 143
        // (get) Token: 0x060001A4 RID: 420 RVA: 0x000026BC File Offset: 0x000008BC
        // (set) Token: 0x060001A5 RID: 421 RVA: 0x000026C4 File Offset: 0x000008C4
        [DataMember]
        public string Baureihenverbund { get; set; }

        // Token: 0x17000090 RID: 144
        // (get) Token: 0x060001A6 RID: 422 RVA: 0x000026CD File Offset: 0x000008CD
        // (set) Token: 0x060001A7 RID: 423 RVA: 0x000026D5 File Offset: 0x000008D5
        [DataMember]
        public bool IsDirect { get; set; }

        // Token: 0x17000091 RID: 145
        // (get) Token: 0x060001A8 RID: 424 RVA: 0x000026DE File Offset: 0x000008DE
        // (set) Token: 0x060001A9 RID: 425 RVA: 0x000026E6 File Offset: 0x000008E6
        [DataMember]
        public string Project { get; set; }

        // Token: 0x17000092 RID: 146
        // (get) Token: 0x060001AA RID: 426 RVA: 0x000026EF File Offset: 0x000008EF
        // (set) Token: 0x060001AB RID: 427 RVA: 0x000026F7 File Offset: 0x000008F7
        [DataMember]
        public string VehicleInfo { get; set; }

        // Token: 0x060001AC RID: 428 RVA: 0x00002700 File Offset: 0x00000900
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TargetSelector: Project={0}, VehicleInfo={1}", this.Project, this.VehicleInfo);
        }
    }
}
