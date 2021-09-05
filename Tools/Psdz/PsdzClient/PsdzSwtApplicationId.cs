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
    public class PsdzSwtApplicationId : IPsdzSwtApplicationId
    {
        // Token: 0x170000F8 RID: 248
        // (get) Token: 0x0600026A RID: 618 RVA: 0x00002A8F File Offset: 0x00000C8F
        // (set) Token: 0x0600026B RID: 619 RVA: 0x00002A97 File Offset: 0x00000C97
        [DataMember]
        public int ApplicationNumber { get; set; }

        // Token: 0x170000F9 RID: 249
        // (get) Token: 0x0600026C RID: 620 RVA: 0x00002AA0 File Offset: 0x00000CA0
        // (set) Token: 0x0600026D RID: 621 RVA: 0x00002AA8 File Offset: 0x00000CA8
        [DataMember]
        public int UpgradeIndex { get; set; }

        // Token: 0x0600026E RID: 622 RVA: 0x000047B8 File Offset: 0x000029B8
        public override bool Equals(object obj)
        {
            PsdzSwtApplicationId psdzSwtApplicationId = obj as PsdzSwtApplicationId;
            return psdzSwtApplicationId != null && psdzSwtApplicationId.ApplicationNumber == this.ApplicationNumber && psdzSwtApplicationId.UpgradeIndex == this.UpgradeIndex;
        }

        // Token: 0x0600026F RID: 623 RVA: 0x00002AB1 File Offset: 0x00000CB1
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        // Token: 0x06000270 RID: 624 RVA: 0x00002ABE File Offset: 0x00000CBE
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X4}{1:X4}", this.ApplicationNumber, this.UpgradeIndex);
        }
    }
}
